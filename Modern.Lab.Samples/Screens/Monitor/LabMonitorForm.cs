using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Controls.Wpf.Input;
using Modern.Lab.Data;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Display;
using Modern.Lab.WinForms.Controls.Input;

namespace Modern.Lab.Samples
{
    public partial class LabMonitorForm : ModernFormBase
    {
        private const string channelMonitor = "monitor";
        private const string focusRework = "REWORK";
        private const string focusScrap = "SCRAP";
        private const string focusReturn = "RETURN";
        private const string focusSwap = "SWAP";
        private const string boardFlow = "flow";
        private const string boardLine = "line";
        private const string boardValueColumn = "VALUE";
        private const string boardLabelColumn = "LABEL";

        private readonly Timer refreshTimer = new Timer();
        private readonly HashSet<string> expandedFoups = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, ModernButton> railButtons = new Dictionary<string, ModernButton>(StringComparer.Ordinal);
        private readonly List<string> railOrder = new List<string>();

        private DataTable lotsTable;
        private DataTable equipmentsTable;
        private DataTable portsTable;
        private DataTable durablesTable;
        private DateTime lastSuccess;
        private int intervalSeconds;
        private int secondsLeft;
        private string focusedStage;
        private string currentBoard = boardFlow;
        private bool ready;

        public LabMonitorForm()
        {
            this.InitializeComponent();
            this.InitializeBoards();
            this.InitializeModernForm();
            this.RegisterRefreshShortcut(this.btnRefresh);
            this.RegisterFindShortcut(this.gridFocus);
            this.DeferredResize = true;
            this.refreshTimer.Interval = 1000;
            this.refreshTimer.Tick += this.OnRefreshTick;
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            if (IsDesignTime() || this.DesignMode)
            {
                return;
            }

            MonitorStageField stage = Schemas.MonitorLot.MonStage;
            this.flowColumns[stage.Exchange.Value].Queue.RowClick += this.OnExchangeRowClick;
            this.listIncoming.RowClick += this.OnExchangeRowClick;
            this.ConfigureHandler(this.flowColumns[stage.Fib.Value].Handler);
            this.ConfigureHandler(this.lineColumns[stage.Fib.Value].Handler);

            this.gridFocus.RowKeyMember = LabMonitorPresenter.RowKeyColumn;
            this.gridFocus.RowDoubleClick += this.OnFocusRowDoubleClick;

            foreach (string stageKey in this.StageKeys())
            {
                this.RegisterRail(stageKey);
            }

            this.RegisterRail(focusRework);
            this.RegisterRail(focusScrap);
            this.RegisterRail(focusReturn);
            this.RegisterRail(focusSwap);

            DataTable boards = new DataTable();
            boards.Locale = CultureInfo.InvariantCulture;
            boards.Columns.Add(boardValueColumn, typeof(string));
            boards.Columns.Add(boardLabelColumn, typeof(string));
            boards.Rows.Add(boardFlow, "Flow");
            boards.Rows.Add(boardLine, "Line");
            this.toggleBoard.DisplayMember = boardLabelColumn;
            this.toggleBoard.ValueMember = boardValueColumn;
            this.toggleBoard.DataSource = boards;
            this.toggleBoard.SelectedValue = boardFlow;

            this.cboInterval.DisplayMember = RefreshInterval.LabelColumn;
            this.cboInterval.ValueMember = RefreshInterval.SecondsColumn;
            this.cboInterval.DataSource = RefreshInterval.Options(LabMonitorPresenter.IntervalChoices);
            this.cboInterval.SelectedValue = LabMonitorPresenter.DefaultIntervalSeconds;

            this.ready = true;
            this.ApplyInterval();
            this.ExecuteRefresh(false);
        }

        private void ConfigureList(ModernQueueList list)
        {
            list.KeyMember = LabMonitorPresenter.RowKeyColumn;
            list.TitleMember = LabMonitorPresenter.TitleColumn;
            list.SubtitleMember = LabMonitorPresenter.SubtitleColumn;
            list.QtyMember = LabMonitorPresenter.QtyTextColumn;
            list.BadgeTextMember = LabMonitorPresenter.BadgeTextColumn;
            list.BadgeKindMember = LabMonitorPresenter.BadgeKindColumn;
            list.EdgeKindMember = LabMonitorPresenter.EdgeKindColumn;
            list.LockMember = LabMonitorPresenter.LockColumn;
            list.TrailingMember = LabMonitorPresenter.TrailingColumn;
            list.ToolTipMember = LabMonitorPresenter.ToolTipColumn;
            list.IndentMember = LabMonitorPresenter.IndentColumn;
            list.RowDoubleClick += this.OnListRowDoubleClick;
        }

        private void ConfigureHandler(ModernHandlerLayout handler)
        {
            if (handler == null)
            {
                return;
            }

            handler.PortLabelMember = LabMonitorPresenter.PortLabelColumn;
            handler.PortDirectionMember = LabMonitorPresenter.PortDirectionColumn;
            handler.PortStateMember = LabMonitorPresenter.PortStateColumn;
            handler.PortLotMember = Schemas.MonitorPort.LotId.Name;
            handler.SiteIdMember = LabMonitorPresenter.SiteIdColumn;
            handler.SiteStateMember = LabMonitorPresenter.SiteStateColumn;
            handler.SiteLotMember = Schemas.MonitorEquipment.LotId.Name;
            handler.SiteReservedLotMember = Schemas.MonitorEquipment.ReservedLotId.Name;
        }

        private void RegisterRail(string stageKey)
        {
            ModernButton rail = new ModernButton();
            rail.Kind = ButtonKind.Subtle;
            rail.Text = this.StageLabel(stageKey);
            rail.Tag = stageKey;
            rail.Size = new System.Drawing.Size(140, 32);
            rail.Margin = new Padding(0, 0, 0, 6);
            rail.Click += this.OnRailClick;
            this.railPanel.Controls.Add(rail);
            this.railButtons[stageKey] = rail;
            this.railOrder.Add(stageKey);
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            this.secondsLeft = this.intervalSeconds;
            this.UpdateCountdown();
            this.ExecuteRefresh(false);
        }

        private void OnRefreshTick(object sender, EventArgs e)
        {
            if (this.intervalSeconds <= 0)
            {
                return;
            }

            this.secondsLeft--;

            if (this.secondsLeft > 0)
            {
                this.UpdateCountdown();
                return;
            }

            if (this.QueryInProgress)
            {
                this.secondsLeft = 1;
                this.UpdateCountdown();
                return;
            }

            this.secondsLeft = this.intervalSeconds;
            this.UpdateCountdown();
            this.ExecuteRefresh(true);
        }

        private void OnIntervalChanged(object sender, EventArgs e)
        {
            if (this.ready)
            {
                this.ApplyInterval();
            }
        }

        private void OnBoardToggleChanged(object sender, EventArgs e)
        {
            if (!this.ready)
            {
                return;
            }

            string board = Convert.ToString(this.toggleBoard.SelectedValue, CultureInfo.InvariantCulture);

            if (board != boardFlow && board != boardLine)
            {
                return;
            }

            this.currentBoard = board;
            this.ShowCurrentBoard();
        }

        private void ShowCurrentBoard()
        {
            bool flow = this.currentBoard == boardFlow;
            this.flowPanel.Visible = flow;
            this.linePanel.Visible = !flow;
        }

        private void ApplyInterval()
        {
            this.intervalSeconds = RefreshInterval.Seconds(this.cboInterval.SelectedValue);
            this.secondsLeft = this.intervalSeconds;
            this.refreshTimer.Enabled = this.intervalSeconds > 0;
            this.UpdateCountdown();
        }

        private void UpdateCountdown()
        {
            string text = RefreshInterval.CountdownText(this.secondsLeft, this.intervalSeconds);
            this.badgeCountdown.Text = text.Length == 0 ? "-" : text;
            this.badgeCountdown.Visible = text.Length > 0;
        }

        private async void ExecuteRefresh(bool silent)
        {
            IDisposable busy = silent ? null : this.Busy("Loading monitor...", string.Empty);
            LoadOutcome<DataTable[]> outcome;

            try
            {
                outcome = await this.FetchAsync(
                        channelMonitor,
                        () => new DataTable[]
                        {
                            this.GetMonitorLots(),
                            this.GetMonitorEquipments(),
                            this.GetMonitorPorts(),
                            this.GetMonitorDurables()
                        },
                        silent);
            }
            finally
            {
                if (busy != null)
                {
                    busy.Dispose();
                }
            }

            if (!outcome.IsCurrent || outcome.Failure != null || outcome.Value == null)
            {
                return;
            }

            this.Apply(outcome.Value);
        }

        private void Apply(DataTable[] tables)
        {
            DateTime now = DateTime.Now;
            this.lotsTable = LabMonitorPresenter.DecorateLots(tables[0], now);
            this.equipmentsTable = tables[1];
            this.portsTable = tables[2];
            this.durablesTable = tables[3];
            this.lastSuccess = now;
            this.lblUpdated.Text = "Updated " + now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

            this.BindBoards(now);

            if (this.focusedStage != null)
            {
                this.BindFocus();
            }
        }

        private void BindBoards(DateTime now)
        {
            MonitorStageField stage = Schemas.MonitorLot.MonStage;

            foreach (LabMonitorPresenter.StageTileInfo info in LabMonitorPresenter.StageTiles(this.lotsTable, this.equipmentsTable, this.durablesTable, now))
            {
                this.BindTile(this.flowTiles[info.Key], info);
                this.BindTile(this.lineTiles[info.Key], info);
            }

            foreach (string stageKey in this.StageKeys())
            {
                StageColumn column = this.flowColumns[stageKey];
                bool equipment = column.RunTitle != null;

                if (stageKey == stage.Exchange.Value)
                {
                    column.Queue.DataSource = LabMonitorPresenter.ExchangeRows(this.lotsTable, this.expandedFoups, now);
                }
                else if (stageKey == stage.Done.Value)
                {
                    column.Queue.DataSource = this.LotView(LabMonitorPresenter.DoneFilter());
                }
                else if (equipment)
                {
                    column.Queue.DataSource = this.LotView(LabMonitorPresenter.QueueFilter(this.StageValue(stageKey)));
                    this.BindRunning(column, this.StageValue(stageKey), now);
                }
                else
                {
                    column.Queue.DataSource = this.LotView(LabMonitorPresenter.StageFilter(this.StageValue(stageKey)));
                }
            }

            this.BindRunning(this.lineColumns[stage.Prep.Value], stage.Prep, now);
            this.BindRunning(this.lineColumns[stage.Fib.Value], stage.Fib, now);
            this.BindRunning(this.lineColumns[stage.Tem.Value], stage.Tem, now);
            this.lineColumns[stage.Prep.Value].Queue.DataSource = this.LotView(LabMonitorPresenter.QueueFilter(stage.Prep));
            this.lineColumns[stage.Fib.Value].Queue.DataSource = this.LotView(LabMonitorPresenter.QueueFilter(stage.Fib));
            this.lineColumns[stage.Tem.Value].Queue.DataSource = this.LotView(LabMonitorPresenter.QueueFilter(stage.Tem));
            this.listIncoming.DataSource = LabMonitorPresenter.IncomingRows(this.lotsTable, this.expandedFoups, now);

            DataTable returns = LabMonitorPresenter.ReturnRows(this.durablesTable, now);
            DataTable swaps = LabMonitorPresenter.SwapRows(this.durablesTable, now);
            this.branchLists[focusReturn].DataSource = returns;
            this.branchLists[focusSwap].DataSource = swaps;
            this.branchLists[focusRework].DataSource = this.LotView(LabMonitorPresenter.ReworkFilter());
            this.branchLists[focusScrap].DataSource = this.LotView(LabMonitorPresenter.ScrapDueFilter());

            DataTable attention = LabMonitorPresenter.AttentionRows(this.lotsTable, this.equipmentsTable, this.durablesTable, now);
            this.listFlowAttention.DataSource = attention;
            this.listLineAttention.DataSource = attention.Copy();

            LabMonitorPresenter.StorageCounts counts = LabMonitorPresenter.CountStorage(this.durablesTable);
            this.lblFlowStorageFoups.Text = "STB FOUP " + Count(counts.StbFoups);
            this.lblFlowStorageStored.Text = "Stored FOUP " + Count(counts.StoredFoups) + " · " + Count(counts.MixedFoups) + " mixed";
            this.lblFlowStorageTrays.Text = "Trays " + Count(counts.StockerTrays) + " · " + Count(counts.FreeTrays) + " free";
            this.lblLineStorage.Text = "STB FOUP " + Count(counts.StbFoups) + " · Stored " + Count(counts.StoredFoups) + " (" + Count(counts.MixedFoups) + " mixed) · Trays "
                    + Count(counts.StockerTrays) + " · " + Count(counts.FreeTrays) + " free";

            foreach (KeyValuePair<string, ModernButton> entry in this.branchButtons)
            {
                entry.Value.Text = this.StageLabel(entry.Key) + " · " + Count(this.StageCount(entry.Key));
            }

            this.UpdateRail();
        }

        private void BindTile(ModernStageTile tile, LabMonitorPresenter.StageTileInfo info)
        {
            tile.Count = info.Count;
            tile.Summary = info.Summary;
            tile.SummaryKind = info.SummaryKind;
            tile.LampState = info.LampState;
        }

        private void BindRunning(StageColumn column, Modern.Lab.WinForms.Controls.Data.FieldValue stage, DateTime now)
        {
            DataRow equipment = LabMonitorPresenter.FindEquipment(this.equipmentsTable, stage);
            string eqpId = equipment == null ? string.Empty : Schemas.MonitorEquipment.EqpId.Text(equipment);
            string status = equipment == null ? string.Empty : Schemas.MonitorEquipment.MesEqpStatCd.Text(equipment);
            bool run = equipment != null && Schemas.MonitorEquipment.MesEqpStatCd.Is(equipment, Schemas.MonitorEquipment.MesEqpStatCd.Run);
            bool down = equipment != null && Schemas.MonitorEquipment.MesEqpStatCd.Is(equipment, Schemas.MonitorEquipment.MesEqpStatCd.Down);

            if (column.Lamp != null)
            {
                column.Lamp.Text = status.Length == 0 ? "-" : status;
                column.Lamp.Semantic = run ? Modern.Lab.Theming.SemanticKind.Success
                        : down ? Modern.Lab.Theming.SemanticKind.Error
                        : Modern.Lab.Theming.SemanticKind.Neutral;
                column.Lamp.Spinning = run;
            }

            string title = LabMonitorPresenter.RunningTitle(this.lotsTable, stage);

            if (column.Handler != null)
            {
                column.Handler.SetLayoutSource(
                        LabMonitorPresenter.HandlerPortRows(this.portsTable, eqpId),
                        LabMonitorPresenter.HandlerSiteRows(this.equipmentsTable, eqpId));
                string summary = LabMonitorPresenter.HandlerSummary(this.equipmentsTable, eqpId);
                column.RunTitle.Text = eqpId.Length == 0 ? "No equipment" : eqpId;
                column.RunSub.Text = summary.Length == 0 ? LabMonitorPresenter.RunningSubtitle(this.lotsTable, equipment, stage, now) : summary;
            }
            else
            {
                column.RunTitle.Text = title.Length == 0 ? (status.Length == 0 ? "No equipment" : status) : title;
                column.RunSub.Text = LabMonitorPresenter.RunningSubtitle(this.lotsTable, equipment, stage, now);
            }

            if (column.QueueCount != null)
            {
                column.QueueCount.Text = "queue " + Count(LabMonitorPresenter.CountRows(this.lotsTable, LabMonitorPresenter.QueueFilter(stage)));
            }
        }

        private DataView LotView(string filter)
        {
            DataTable source = this.lotsTable ?? LabMonitorPresenter.NewDisplayTable();
            return new DataView(source, filter, LabMonitorPresenter.PrioritySortColumn + ", " + LabMonitorPresenter.WaitSecondsColumn + " DESC", DataViewRowState.CurrentRows);
        }

        private Modern.Lab.WinForms.Controls.Data.FieldValue StageValue(string stageKey)
        {
            foreach (Modern.Lab.WinForms.Controls.Data.FieldValue value in Schemas.MonitorLot.MonStage.Flow)
            {
                if (value.Value == stageKey)
                {
                    return value;
                }
            }

            return Schemas.MonitorLot.MonStage.Request;
        }

        private void UpdateRail()
        {
            foreach (string stageKey in this.railOrder)
            {
                ModernButton rail = this.railButtons[stageKey];
                rail.Text = this.StageLabel(stageKey) + " · " + this.CountText(stageKey);
                rail.Kind = string.Equals(stageKey, this.focusedStage, StringComparison.Ordinal) ? ButtonKind.Primary : ButtonKind.Subtle;
            }

            foreach (KeyValuePair<string, ModernStageTile> entry in this.flowTiles)
            {
                entry.Value.Selected = string.Equals(entry.Key, this.focusedStage, StringComparison.Ordinal);
            }

            foreach (KeyValuePair<string, ModernStageTile> entry in this.lineTiles)
            {
                entry.Value.Selected = string.Equals(entry.Key, this.focusedStage, StringComparison.Ordinal);
            }
        }

        private string CountText(string stageKey)
        {
            if (stageKey == Schemas.MonitorLot.MonStage.Exchange.Value)
            {
                return Count(LabMonitorPresenter.FoupCount(this.lotsTable)) + " FOUP";
            }

            return Count(this.StageCount(stageKey));
        }

        private int StageCount(string stageKey)
        {
            if (stageKey == focusReturn)
            {
                return this.branchLists[focusReturn].RowCount;
            }

            if (stageKey == focusSwap)
            {
                return this.branchLists[focusSwap].RowCount;
            }

            if (stageKey == Schemas.MonitorLot.MonStage.Exchange.Value)
            {
                return LabMonitorPresenter.FoupCount(this.lotsTable);
            }

            return LabMonitorPresenter.CountRows(this.lotsTable, this.LotFilter(stageKey));
        }

        private string LotFilter(string stageKey)
        {
            if (stageKey == focusRework)
            {
                return LabMonitorPresenter.ReworkFilter();
            }

            if (stageKey == focusScrap)
            {
                return LabMonitorPresenter.ScrapDueFilter();
            }

            if (stageKey == Schemas.MonitorLot.MonStage.Done.Value)
            {
                return LabMonitorPresenter.DoneFilter();
            }

            return LabMonitorPresenter.RequestFilter(stageKey);
        }

        private string StageLabel(string stageKey)
        {
            if (stageKey == focusRework)
            {
                return "Rework";
            }

            if (stageKey == focusScrap)
            {
                return "Scrap";
            }

            if (stageKey == focusReturn)
            {
                return "Return";
            }

            if (stageKey == focusSwap)
            {
                return "Swap";
            }

            return LabMonitorPresenter.StageLabel(stageKey);
        }

        private void OnRailClick(object sender, EventArgs e)
        {
            ModernButton button = sender as ModernButton;
            string stageKey = button == null ? null : button.Tag as string;

            if (!string.IsNullOrEmpty(stageKey))
            {
                this.ShowFocus(stageKey);
            }
        }

        private void OnBackClick(object sender, EventArgs e)
        {
            this.ShowBoard();
        }

        private void ShowFocus(string stageKey)
        {
            this.focusedStage = stageKey;
            this.flowPanel.Visible = false;
            this.linePanel.Visible = false;
            this.toggleBoard.Visible = false;
            this.focusPanel.Visible = true;
            this.UpdateRail();
            this.BindFocus();
        }

        private void ShowBoard()
        {
            this.focusedStage = null;
            this.focusPanel.Visible = false;
            this.toggleBoard.Visible = true;
            this.ShowCurrentBoard();
            this.UpdateRail();
        }

        private void BindFocus()
        {
            string stageKey = this.focusedStage;

            if (stageKey == null)
            {
                return;
            }

            DataView view;

            if (stageKey == focusReturn)
            {
                view = new DataView(LabMonitorPresenter.ReturnRows(this.durablesTable, this.lastSuccess), string.Empty, LabMonitorPresenter.PrioritySortColumn, DataViewRowState.CurrentRows);
            }
            else if (stageKey == focusSwap)
            {
                view = new DataView(LabMonitorPresenter.SwapRows(this.durablesTable, this.lastSuccess), string.Empty, LabMonitorPresenter.PrioritySortColumn, DataViewRowState.CurrentRows);
            }
            else
            {
                view = this.LotView(this.LotFilter(stageKey));
            }

            this.gridFocus.DataSource = view;
            this.gridFocus.StatusText = this.StageLabel(stageKey);
            string suffix = stageKey == focusReturn || stageKey == focusSwap ? " FOUP" : (view.Count == 1 ? " lot" : " lots");
            this.lblFocusTitle.Text = this.StageLabel(stageKey) + " · " + Count(view.Count) + suffix;
        }

        private void OnExchangeRowClick(object sender, QueueRowEventArgs e)
        {
            DataRowView row = e.Item as DataRowView;

            if (row == null || TableHelper.CellText(row.Row, LabMonitorPresenter.RowKindColumn) != LabMonitorPresenter.KindFoup)
            {
                return;
            }

            string foup = Schemas.MonitorLot.DurableId.Text(row.Row);

            if (!this.expandedFoups.Remove(foup))
            {
                this.expandedFoups.Add(foup);
            }

            this.flowColumns[Schemas.MonitorLot.MonStage.Exchange.Value].Queue.DataSource = LabMonitorPresenter.ExchangeRows(this.lotsTable, this.expandedFoups, this.lastSuccess);
            this.listIncoming.DataSource = LabMonitorPresenter.IncomingRows(this.lotsTable, this.expandedFoups, this.lastSuccess);
        }

        private void OnListRowDoubleClick(object sender, QueueRowEventArgs e)
        {
            DataRowView row = e.Item as DataRowView;

            if (row == null || TableHelper.CellText(row.Row, LabMonitorPresenter.RowKindColumn) != LabMonitorPresenter.KindLot)
            {
                return;
            }

            this.OpenDetail(row.Row);
        }

        private void OnFocusRowDoubleClick(object sender, EventArgs e)
        {
            DataRowView row = this.gridFocus.SelectedItem as DataRowView;

            if (row == null || !row.Row.Table.Columns.Contains(Schemas.MonitorLot.LotId.Name))
            {
                return;
            }

            this.OpenDetail(row.Row);
        }

        private void OpenDetail(DataRow row)
        {
            MonitorDetailDialogForm.ShowFor(this, Schemas.MonitorLot.ReqSerialNo.Text(row), Schemas.MonitorLot.LotId.Text(row));
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape && this.focusedStage != null)
            {
                this.ShowBoard();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            this.refreshTimer.Stop();
            base.OnFormClosing(e);
        }

        private static string Count(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
