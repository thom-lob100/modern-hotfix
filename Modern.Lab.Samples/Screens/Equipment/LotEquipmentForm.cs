using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Controls.Wpf.Input;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.ResponseContracts;
using Modern.Lab.Samples.Contracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Display;
using Modern.Lab.WinForms.Controls.Selection;

using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class LotEquipmentForm : ModernFormBase
    {
        private static readonly ResponseContractSet Contracts = EquipmentLotContracts.Build();

        private DataTable equipmentData;
        private DataTable portData;
        private DataTable childData;
        private DataTable lotData;

        private DataTable requestData;
        private DataTable sampleData;
        private DataTable durableData;
        private RequestSnapshot requestSnapshot;

        private TableResponse equipmentCurrent;
        private TableResponse portCurrent;
        private TableResponse childCurrent;
        private TableResponse lotCurrent;
        private TableResponse requestCurrent;
        private TableResponse sampleCurrent;
        private TableResponse durableCurrent;

        private string decisionPortEqpId = string.Empty;

        private string durableListEqpId = string.Empty;
        private string durableListGoalPortNm = string.Empty;

        private bool cyclePortsReflected;
        private bool cycleLotsReflected;
        private bool cycleRefresh;

        private const string channelGroups = "groups";
        private const string channelEquipments = "equipments";
        private const string channelPorts = "ports";
        private const string channelChildren = "children";
        private const int ChildChipHeight = 28;
        private const int ChildChipGap = 6;
        private const int ChildChipMinWidth = 80;
        private const string channelLots = "lots";
        private const string channelRequests = "requests";

        private const string channelDurables = "durables";

        private readonly JobDecision decision = new JobDecision();
        private bool lotFinderBinding;
        private bool groupSetup;
        private string loadedEqpGrpId = string.Empty;
        private readonly System.Windows.Forms.Timer refreshTimer = new System.Windows.Forms.Timer();
        private const string channelJobState = "jobState";
        private const int JobStateTrackIntervalMs = 3000;
        private const int JobStateTrackLimitMs = 30000;
        private readonly System.Windows.Forms.Timer jobStateTimer = new System.Windows.Forms.Timer();
        private string trackedLotId = string.Empty;
        private DateTime trackedUntil;

        private readonly System.Windows.Forms.Timer targetFlashTimer = new System.Windows.Forms.Timer();

        private System.Drawing.Color targetRestColor = System.Drawing.Color.Empty;

        private int refreshSerial;
        private int dependentSerial;

        private string portEqpId = string.Empty;
        private string childEqpId = string.Empty;
        private string requestLotId = string.Empty;
        private string reqSerialNo = string.Empty;

        private bool gridBinding;
        private bool silentRefresh;

        private bool userRefresh;

        private int decisionHeight = -1;
        private int decisionSplitHeightSeen = -1;
        private int decisionSplitDistanceSeen = -1;
        private bool fittingColumns;
        private bool fittingListPanels;
        private string requestFieldShape = string.Empty;
        private int requestHeaderHeight;
        private bool durableComboBinding;
        private int decisionPending;
        private int requestLoadSerial;
        private string durableListLotId = string.Empty;
        private bool lotEquipmentsPending;
        private bool durablesPending;
        private bool lotRetarget;
        private bool retargetPorts;

        private int intervalSeconds;
        private int secondsLeft;

        private readonly DependentCover portCover;
        private readonly DependentCover durableCover;

        public LotEquipmentForm()
        {
            this.InitializeComponent();

            this.portCover = new DependentCover(this.portBusy);
            this.childChips.SizeChanged += this.OnChildChipsSizeChanged;
            this.durableCover = new DependentCover(this.durableBusy);

            this.InitializeModernForm(this.midPanel);

            this.DeferredResize = true;

            this.RegisterFindShortcut(this.gridLotPriorities, this.gridEquipmentPriorities);

            this.menuEqp.Renderer = new Modern.Lab.WinForms.Rendering.ModernMenuRenderer();
            this.menuLot.Renderer = new Modern.Lab.WinForms.Rendering.ModernMenuRenderer();
            this.menuLot.ShowItemToolTips = true;

            this.targetFlashTimer.Interval = 1500;
            this.targetFlashTimer.Tick += this.OnTargetFlashElapsed;
            this.refreshTimer.Interval = 1000;
            this.refreshTimer.Tick += this.OnRefreshTick;
            this.jobStateTimer.Interval = JobStateTrackIntervalMs;
            this.jobStateTimer.Tick += this.OnJobStateTick;
            this.Disposed += this.OnFormDisposed;

            this.splitRight.SizeChanged += this.OnBottomLayoutSizeChanged;
            this.splitDurableDecision.SizeChanged += this.OnBottomLayoutSizeChanged;
            this.midPanel.SizeChanged += this.OnMidPanelSizeChanged;
            this.splitRequest.SizeChanged += this.OnRequestSplitSizeChanged;
            this.durablePanel.Resize += this.OnDurablePanelResize;
            this.actionCard.SizeChanged += this.OnActionColumnSizeChanged;
            this.lblRequestRemarkCaption.ForeColor = Modern.Lab.Theming.ModernTheme.TextSecondary;
            this.lblRequestRemark.Font = Modern.Lab.WinForms.Rendering.ModernFonts.BodyStrong;
            this.SyncActionColumns();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (this.decisionHeight < 0)
            {
                this.decisionHeight = DecisionHeight * this.DeviceDpi / 96;
                this.decisionSplitHeightSeen = this.splitDurableDecision.Height;
                this.decisionSplitDistanceSeen = this.splitDurableDecision.SplitterDistance;
            }

            this.FitDecisionPanel();
            this.FitListPanels();
            this.SyncActionColumns();

            this.FitLowerColumns();
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            this.cboGroup.UseEntity(Schemas.EquipmentGroup);
            this.cboLot.UseEntity(Schemas.LotPriority);

            this.cboInterval.DisplayMember = RefreshInterval.LabelColumn;
            this.cboInterval.ValueMember = RefreshInterval.SecondsColumn;
            this.cboInterval.DataSource = RefreshInterval.Options(EquipmentLotPresenter.IntervalChoices);
            this.cboInterval.SelectedValue = EquipmentLotPresenter.DefaultIntervalSeconds;
            this.ApplyInterval();

            this.gridEquipmentPriorities.RowKeyMember = Schemas.EquipmentPriority.RowKey;
            this.gridPortPriorities.RowKeyMember = Schemas.PortPriority.RowKey;

            this.gridLotPriorities.RowKeyMember = Schemas.LotPriority.RowKey;
            this.gridLotPriorities.CellLinkClick += this.OnLotRequestLinkClick;
            this.gridLotPriorities.CellLinkClick += this.OnDurableIdLinkClick;

            this.cboDurable.UseEntity(Schemas.DurablePriority);
            this.cboDurable.SelectedIndexChanged += this.OnDurableComboChanged;
            this.gridSamples.RowKeyMember = Schemas.Sample.RowKey;
            this.fieldRequest.FieldLinkClick += this.OnRequestFieldLinkClick;
            string[] tones = Modern.Lab.Theming.Palette.GetColors(4);
            this.kpiEquipment.Tone = tones[0];
            this.kpiPorts.Tone = tones[1];
            this.kpiLot.Tone = tones[2];
            this.kpiDurable.Tone = tones[3];

            this.badgeJob.SpinValues = EquipmentLotPresenter.JobStateStart;

            this.PopulateMenu(this.menuEqp, "Mode", EquipmentPortManagementPresenter.EquipmentActions, this.OnEquipmentMenuItemClick);
            this.PopulateJobMenu();

            this.RefreshDecisionCaption();
            this.RefreshDecisionPanel();
            this.RefreshActionStates();

            this.LoadGroups();
            this.ExecuteSearch();
        }

        private void OnFormDisposed(object sender, EventArgs e)
        {
            this.refreshTimer.Stop();
            this.refreshTimer.Dispose();
            this.jobStateTimer.Stop();
            this.jobStateTimer.Dispose();
        }

        private async void LoadGroups()
        {
            LoadOutcome<DataTable> outcome = await this.FetchAsync(
                    channelGroups, () => this.GetEquipmentGroups());

            if (!outcome.IsCurrent || outcome.Failure != null)
            {
                return;
            }

            this.groupSetup = true;

            try
            {
                this.cboGroup.DataSource = outcome.Value;
                this.cboGroup.SelectedIndex = -1;
            }
            finally
            {
                this.groupSetup = false;
            }
        }

        private void OnGroupChanged(object sender, EventArgs e)
        {
            if (this.groupSetup)
            {
                return;
            }

            this.StopJobStateTracking();
            this.ExecuteSearch();
        }

        private void AfterJobAction(string lotId)
        {
            this.jobStateTimer.Stop();
            this.trackedLotId = (lotId ?? string.Empty).Trim();
            this.trackedUntil = DateTime.Now.AddMilliseconds(JobStateTrackLimitMs);
            this.ExecuteSearch(true);
        }

        private void ReviewJobStateTracking()
        {
            if (this.trackedLotId.Length == 0)
            {
                return;
            }

            DataRow lot = EquipmentLotPresenter.FindById(this.lotData, Schemas.LotPriority.RowKey, this.trackedLotId);

            if (lot == null
                    || EquipmentLotPresenter.JobState(lot) != EquipmentLotPresenter.JobStatePrep
                    || DateTime.Now >= this.trackedUntil)
            {
                this.StopJobStateTracking();
                return;
            }

            this.jobStateTimer.Start();
        }

        private void StopJobStateTracking()
        {
            this.jobStateTimer.Stop();
            this.trackedLotId = string.Empty;
        }

        private async void OnJobStateTick(object sender, EventArgs e)
        {
            this.jobStateTimer.Stop();
            string lotId = this.trackedLotId;
            string eqpGrpId = this.SelectedEqpGrpId();

            if (lotId.Length == 0 || eqpGrpId.Length == 0)
            {
                this.StopJobStateTracking();
                return;
            }

            LoadOutcome<DataTable> outcome = await this.FetchAsync(
                    channelJobState, () => this.GetLotPriorities(eqpGrpId, string.Empty, lotId), true);

            if (this.IsDisposed || this.Disposing || lotId != this.trackedLotId)
            {
                return;
            }

            if (outcome.IsCurrent && outcome.Failure == null)
            {
                this.ApplyJobState(lotId, outcome.Value);
            }

            this.ReviewJobStateTracking();
        }

        private void ApplyJobState(string lotId, DataTable response)
        {
            DataRow fresh = EquipmentLotPresenter.FindById(response, Schemas.LotPriority.RowKey, lotId);
            DataRow lot = EquipmentLotPresenter.FindById(this.lotData, Schemas.LotPriority.RowKey, lotId);

            if (fresh == null || lot == null)
            {
                return;
            }

            EquipmentLotPresenter.OverwriteJobLot(lot, fresh);

            if (Judged(this.lotCurrent))
            {
                EquipmentLotPresenter.MarkJobColors(this.lotData);
            }

            this.dependentSerial++;
            this.RefreshDecisionPanel();
            this.RefreshActionStates();
            this.SyncDecisionDurables(false);
        }

        private string SelectedEqpGrpId()
        {
            return (Convert.ToString(this.cboGroup.SelectedValue) ?? string.Empty).Trim();
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            this.secondsLeft = this.intervalSeconds;
            this.UpdateCountdown();
            this.ExecuteSearch();
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

            if (this.QueryInProgress || this.ActionInProgress)
            {
                this.secondsLeft = 1;
                this.UpdateCountdown();
                return;
            }

            this.secondsLeft = this.intervalSeconds;
            this.UpdateCountdown();
            this.ExecuteSearch(true);
        }

        private void OnIntervalChanged(object sender, EventArgs e)
        {
            this.ApplyInterval();
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

        private void OnDecisionSplitterMoved(object sender, SplitterEventArgs e)
        {
            Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer lower = this.splitDurableDecision;
            bool dragged = Control.MouseButtons == MouseButtons.Left
                    && lower.Height == this.decisionSplitHeightSeen && lower.SplitterDistance != this.decisionSplitDistanceSeen;

            if (this.decisionHeight >= 0 && !this.fittingColumns && dragged)
            {
                this.decisionHeight = lower.Panel2.Height;
                this.FitDecisionPanel();
            }

            this.decisionSplitHeightSeen = lower.Height;
            this.decisionSplitDistanceSeen = lower.SplitterDistance;

            this.FitLowerPanels();
        }

        private void FitLowerPanels()
        {
            int room = this.splitLeft.Height - this.splitLeft.SplitterWidth;

            if (room < this.splitLeft.Panel1MinSize + this.splitLeft.Panel2MinSize)
            {
                return;
            }

            int distance = Math.Max(
                    this.splitLeft.Panel1MinSize,
                    Math.Min(this.splitDurableDecision.SplitterDistance, room - this.splitLeft.Panel2MinSize));
            this.fittingListPanels = true;

            try
            {
                if (this.splitLeft.SplitterDistance != distance)
                {
                    this.splitLeft.SplitterDistance = distance;
                }
            }
            catch (InvalidOperationException)
            {
            }
            finally
            {
                this.fittingListPanels = false;
            }
        }

        private void FitLowerColumns()
        {
            int width = this.splitMain.Width - this.splitMain.SplitterWidth;
            int equipmentWidth = width - PortColumnsWidth * this.DeviceDpi / 96;
            int room = this.splitLotRequest.Height - this.splitLotRequest.SplitterWidth;

            try
            {
                if (equipmentWidth >= this.splitMain.Panel1MinSize && width - equipmentWidth >= this.splitMain.Panel2MinSize)
                {
                    this.splitMain.SplitterDistance = equipmentWidth;
                }

                int portHeight = PortListHeight * this.DeviceDpi / 96;

                if (portHeight >= this.splitLotRequest.Panel1MinSize && room - portHeight >= this.splitLotRequest.Panel2MinSize)
                {
                    this.splitLotRequest.SplitterDistance = portHeight;
                }
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void OnMidPanelSizeChanged(object sender, EventArgs e)
        {
            this.FitListPanels();
        }

        private void FitListPanels()
        {
            this.FitLowerPanels();
        }

        private void OnBottomLayoutSizeChanged(object sender, EventArgs e)
        {
            this.FitDecisionPanel();
            this.SyncActionColumns();
        }

        private void OnActionColumnSizeChanged(object sender, EventArgs e)
        {
            this.SyncActionColumns();
        }

        private void SyncActionColumns()
        {
            this.lblTarget.Width = Math.Max(0, this.actionCard.ClientSize.Width - this.lblTarget.Left - this.actionCard.Padding.Right);
        }

        private void FitDecisionPanel()
        {
            if (this.decisionHeight < 0 || this.fittingColumns)
            {
                return;
            }

            Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer lower = this.splitDurableDecision;
            int room = lower.Height - lower.SplitterWidth;

            if (room < lower.Panel1MinSize + lower.Panel2MinSize)
            {
                return;
            }

            int decision = Math.Max(lower.Panel2MinSize, Math.Min(this.decisionHeight, room - lower.Panel1MinSize));
            int upper = room - decision;

            this.fittingColumns = true;

            try
            {
                if (lower.SplitterDistance != upper)
                {
                    lower.SplitterDistance = upper;
                }
            }
            catch (InvalidOperationException)
            {
            }
            finally
            {
                this.fittingColumns = false;
            }

            this.FitLowerPanels();
        }

        private void RefreshDecisionCaption()
        {
            DataRow topLot = EquipmentLotPresenter.TopPriority(this.lotData);
            bool topPriority = this.decision.Lot == null
                    || (topLot != null && TableHelper.CellText(topLot, Schemas.LotPriority.LotId.Name).Trim() == this.decision.LotId);
            this.decisionCard.TitleRightText = this.JobRunMode()
                    ? "Job in progress"
                    : (topPriority ? "Top priority" : "Selected");
        }

        private void NotifyJobRunMode()
        {
            this.ShowToast(
                    this.decision.LotId + " has a job in progress. Equipment, port and durable follow that job.",
                    ToastKind.Info);
        }

        private string SelectedEquipmentId()
        {
            DataRowView equipment = this.gridEquipmentPriorities.SelectedItem as DataRowView;
            return equipment == null ? string.Empty : TableHelper.CellText(equipment.Row, Schemas.EquipmentPriority.EqpId.Name).Trim();
        }

        private string SelectedLotId()
        {
            DataRow lot = LiveRow(this.gridLotPriorities.SelectedItem);
            return lot == null ? string.Empty : TableHelper.CellText(lot, Schemas.LotPriority.LotId.Name).Trim();
        }

        private string SelectedRequestSerialNo()
        {
            DataRow lot = LiveRow(this.gridLotPriorities.SelectedItem);
            return lot == null ? string.Empty : TableHelper.CellText(lot, Schemas.LotPriority.ReqSerialNo.Name).Trim();
        }

        private async void ExecuteSearch(bool silent = false)
        {
            string eqpGrpId = this.SelectedEqpGrpId();
            bool inPlace = !silent && this.lotData != null && eqpGrpId == this.loadedEqpGrpId;
            this.userRefresh = inPlace;

            IDisposable busy = silent ? null : this.Busy("Loading lots...", eqpGrpId);

            if (!silent && !inPlace)
            {
                this.InvalidateChannel(channelEquipments);
                this.BindLotPriorities(null, false);
                this.BindEquipmentPriorityTable(null, string.Empty, false);
            }

            LoadOutcome<DataTable> outcome;

            try
            {
                outcome = await this.FetchAsync(channelLots, () => this.GetLotPriorities(eqpGrpId, string.Empty, string.Empty), silent);
            }
            finally
            {
                if (busy != null)
                {
                    busy.Dispose();
                }
            }

            if (!outcome.IsCurrent)
            {
                return;
            }

            this.loadedEqpGrpId = eqpGrpId;
            this.BindLotPriorities(outcome.Failure != null ? null : outcome.Value, silent || inPlace);
        }

        private void BindEquipmentPriorityTable(DataTable equipments, string keepEqpId, bool silent)
        {
            this.BindEquipmentPriorityTable(equipments, keepEqpId, silent, silent);
        }

        private void BindEquipmentPriorityTable(DataTable equipments, string keepEqpId, bool silent, bool merge)
        {
            this.BindEquipmentPriorityTable(equipments, keepEqpId, silent, merge, false);
        }

        private void BindEquipmentPriorityTable(
                DataTable equipments, string keepEqpId, bool silent, bool merge, bool retarget)
        {
            bool merged;
            this.equipmentData = this.BindJudged(
                    EquipmentLotContracts.EquipmentTable, this.equipmentData, ref this.equipmentCurrent,
                    equipments, Schemas.EquipmentPriority.RowKey, silent, merge, out merged);

            this.decision.EquipmentList = this.equipmentData;

            if (Judged(this.equipmentCurrent))
            {
                EquipmentLotPresenter.MarkAutoCan(this.equipmentData);
            }

            ConfigureGrid(this.gridEquipmentPriorities, this.equipmentData, Judged(this.equipmentCurrent), EquipmentPriorityColumns);

            this.decision.DropDetached();
            DataTable table = this.equipmentData;
            DataRow equipment = retarget
                    ? null
                    : EquipmentLotPresenter.FindById(table, Schemas.EquipmentPriority.RowKey, keepEqpId);

            if (equipment == null)
            {
                equipment = EquipmentLotPresenter.TopPriority(table);
            }
            string decidedEqpId = TableHelper.CellText(equipment, Schemas.EquipmentPriority.EqpId.Name).Trim();

            this.gridBinding = true;

            try
            {
                if (!merged)
                {
                    this.gridEquipmentPriorities.DataSource = table;
                    this.SelectRow(this.gridEquipmentPriorities, table, Schemas.EquipmentPriority.RowKey, decidedEqpId);
                }
                else if (this.gridEquipmentPriorities.SelectedItem == null
                        || (decidedEqpId.Length > 0 && decidedEqpId != this.decision.EqpId))
                {
                    this.SelectRow(this.gridEquipmentPriorities, table, Schemas.EquipmentPriority.RowKey, decidedEqpId);
                }
            }
            finally
            {
                this.gridBinding = false;
            }

            this.RefreshDecisionTitles();

            string previousEqpId = this.decision.EqpId;
            this.decision.Equipment = equipment;
            this.refreshSerial++;

            if (this.decision.EqpId != previousEqpId)
            {
                this.decision.Port = null;
                this.decision.GoalPort = null;
                this.decision.ChildEquipment = null;
                this.decision.ChildEquipmentList = null;
            }

            this.silentRefresh = merged && this.decision.EqpId == previousEqpId && this.decision.EqpId.Length > 0;

            this.LoadPortPriorities(this.SelectedEquipmentId(), this.silentRefresh);
            this.ApplyEquipmentDecision();

            this.SyncDecisionDurables(false);
        }

        private static void FollowServerOrder(DataTable table, DataTable source, string keyColumn)
        {
            if (table == null)
            {
                return;
            }

            EquipmentLotPresenter.MarkServerOrder(table, source, keyColumn);
            string sort = EquipmentLotPresenter.ServerOrderColumn + " ASC";

            if (table.DefaultView.Sort != sort)
            {
                table.DefaultView.Sort = sort;
            }
        }

        private void SelectRow(Modern.Lab.WinForms.Controls.Data.ModernDataGrid grid, DataTable table, string column, string id)
        {
            if (table == null || id.Length == 0)
            {
                return;
            }

            DataView view = table.DefaultView;

            for (int index = 0; index < view.Count; index++)
            {
                if (TableHelper.CellText(view[index].Row, column).Trim() == id)
                {
                    if (grid.SelectedIndex != index)
                    {
                        grid.SelectedIndex = index;
                    }

                    return;
                }
            }
        }

        private void ApplyEquipmentDecision()
        {
            string eqpId = this.decision.EqpId;

            this.ResolveDecisionPorts();
            this.RefreshDecisionPanel();
            this.RefreshActionStates();

            this.cycleRefresh = this.silentRefresh;
            this.cyclePortsReflected = this.portEqpId == eqpId;
            this.cycleLotsReflected = true;
        }

        private void SyncDecisionDurables(bool refresh)
        {
            bool runMode = EquipmentLotPresenter.HasActiveJob(this.decision.Lot);
            string eqpId = runMode
                    ? TableHelper.CellText(this.decision.Lot, Schemas.LotPriority.EqpId.Name).Trim()
                    : this.decision.EqpId;
            bool portsKnown = !runMode && eqpId.Length > 0 && this.decisionPortEqpId == eqpId;
            string goalPortNm = EquipmentLotPresenter.JobGoalPortNm(this.decision.Lot);

            if (goalPortNm.Length == 0 && portsKnown)
            {
                goalPortNm = this.decision.GoalPortNm;
            }

            if (!runMode && goalPortNm.Length == 0 && !portsKnown && eqpId == this.durableListEqpId)
            {
                return;
            }

            if (eqpId == this.durableListEqpId && goalPortNm == this.durableListGoalPortNm)
            {
                if (runMode)
                {
                    this.FollowJobDurable();
                }

                if (refresh && goalPortNm.Length > 0)
                {
                    string refreshLotId = this.decision.LotId;
                    bool lotChanged = refreshLotId != this.durableListLotId;
                    this.durableListLotId = refreshLotId;
                    DataRow refreshLot = ActionRows.Copy(this.decision.Lot);

                    if (lotChanged && !runMode)
                    {
                        this.durablesPending = true;
                        this.UpdateMaterialsPending();
                        this.RefreshActionStates();
                    }

                    this.LoadDependent(channelDurables, () => this.GetDurablePriorities(eqpId, goalPortNm, refreshLot), table => this.BindDurablePriorities(table, true, true, lotChanged), false, true, null);
                }

                return;
            }

            bool keepShown = this.durableData != null && eqpId.Length > 0 && goalPortNm.Length > 0;
            this.InvalidateChannel(channelDurables);
            this.durableListEqpId = eqpId;
            this.durableListGoalPortNm = goalPortNm;

            if (keepShown)
            {
                this.durableListLotId = this.decision.LotId;
                DataRow keptLot = ActionRows.Copy(this.decision.Lot);
                this.durablesPending = !runMode;
                this.UpdateMaterialsPending();
                this.LoadDependent(channelDurables, () => this.GetDurablePriorities(eqpId, goalPortNm, keptLot), table => this.BindDurablePriorities(table, false, true, true), false, false, null);
                this.dependentSerial++;
                this.RefreshDecisionPanel();
                this.RefreshActionStates();
                return;
            }

            this.durableData = null;
            this.lblDurable.Text = "-";
            this.decision.GoalDurable = null;
            this.durablesPending = false;
            this.UpdateMaterialsPending();

            if (eqpId.Length == 0 || goalPortNm.Length == 0)
            {
                this.durableCover.Reset();
                this.BindDurableCombo(null);
                this.dependentSerial++;
                this.RefreshDecisionPanel();
                this.RefreshActionStates();
                return;
            }

            this.durableListLotId = this.decision.LotId;
            DataRow durableLot = ActionRows.Copy(this.decision.Lot);
            this.LoadDependent(channelDurables, () => this.GetDurablePriorities(eqpId, goalPortNm, durableLot), table => this.BindDurablePriorities(table, false, false, true), !runMode, false, this.durableCover);
            this.dependentSerial++;
            this.RefreshDecisionPanel();
            this.RefreshActionStates();
        }

        private void UpdateMaterialsPending()
        {
            this.decision.MaterialsPending = this.lotEquipmentsPending || this.durablesPending || this.retargetPorts;
        }

        private void FollowJobDurable()
        {
            string goalDurableId = EquipmentLotPresenter.JobGoalDurableId(this.decision.Lot);

            if (this.decision.GoalDurableId != goalDurableId)
            {
                this.decision.GoalDurable = goalDurableId.Length == 0
                        ? null
                        : EquipmentLotPresenter.FindById(this.durableData, Schemas.DurablePriority.RowKey, goalDurableId);
            }

            this.dependentSerial++;
            this.RefreshDecisionPanel();
            this.RefreshActionStates();
        }

        private void SyncDecisionDurablesIfReady()
        {
            if (this.cyclePortsReflected && this.cycleLotsReflected)
            {
                this.SyncDecisionDurables(this.cycleRefresh);
                this.cycleRefresh = false;
            }
        }

        private string GoalDurableTyp()
        {
            DataRow port = this.portEqpId == this.durableListEqpId
                    ? EquipmentLotPresenter.FindById(this.portData, Schemas.PortPriority.PortNm.Name, this.durableListGoalPortNm)
                    : null;
            string durableTyp = EquipmentLotPresenter.PortDurableTyp(port);

            if (durableTyp.Length == 0 && this.durableData != null && this.durableData.Rows.Count > 0)
            {
                durableTyp = TableHelper.CellText(this.durableData.Rows[0], Schemas.DurablePriority.DurableTyp.Name).Trim();
            }

            return durableTyp;
        }

        private async void LoadDependent(
                string channel, Func<DataTable> request, Action<DataTable> bind, bool busy, bool silent, DependentCover cover)
        {
            if (busy)
            {
                this.decisionPending++;
                this.decisionBusy.Busy = true;
            }

            if (cover != null)
            {
                cover.Begin();
            }

            LoadOutcome<DataTable> outcome;

            try
            {
                outcome = await this.FetchAsync(channel, request, silent && !this.userRefresh);
            }
            finally
            {
                if (cover != null)
                {
                    cover.End();
                }

                if (busy)
                {
                    this.decisionPending--;

                    if (this.decisionPending == 0 && !this.decisionBusy.IsDisposed)
                    {
                        this.decisionBusy.Busy = false;
                    }
                }
            }

            if (!outcome.IsCurrent)
            {
                return;
            }

            bind(outcome.Failure != null ? null : outcome.Value);
        }

        private DataTable BindJudged(
                string tableId, DataTable bound, ref TableResponse current, DataTable incoming,
                string keyColumn, bool silent, bool allowMerge, out bool merged)
        {
            merged = false;

            if (incoming == null)
            {
                current = null;
                return null;
            }

            TableReception reception = TableJudgment.Receive(
                    tableId, incoming, Contracts, EquipmentLotPresenter.ScreenColumns);

            if (reception.IsMissingRequired)
            {
                current = null;
                this.ShowMissingColumns(reception.Response, silent && !this.userRefresh);
                return null;
            }

            if (reception.HasReservedColumns)
            {
                current = null;
                return reception.DisplayCopy;
            }

            DataTable normalized = reception.Normalized;
            merged = allowMerge && bound != null && current != null
                    && TableJudgment.SameSchema(current.Table, normalized);
            DataTable table = merged ? bound : normalized;

            if (merged)
            {
                bool bindingBefore = this.gridBinding;
                this.gridBinding = true;

                try
                {
                    TableMerge.Apply(table, normalized, keyColumn, EquipmentLotPresenter.ScreenColumns);
                }
                finally
                {
                    this.gridBinding = bindingBefore;
                }

                this.decision.DropDetached();
            }

            FollowServerOrder(table, merged ? normalized : null, keyColumn);

            current = TableJudgment.Judge(reception, table, Contracts);
            EquipmentLotPresenter.MarkPriorityColors(table);
            return table;
        }

        private static bool Judged(TableResponse current)
        {
            return current != null;
        }

        private DataRow ValidEquipment(DataRow row)
        {
            return Judged(this.equipmentCurrent) && row != null
                    && TableHelper.CellText(row, Schemas.EquipmentPriority.EqpId.Name).Trim().Length > 0 ? row : null;
        }

        private static void ConfigureGrid(
                Modern.Lab.WinForms.Controls.Data.ModernDataGrid grid, DataTable table, bool judged,
                Func<DataTable, GridColumns> configure)
        {
            if (table == null)
            {
                return;
            }

            if (!judged)
            {
                GridColumns.Of(table).Apply(grid);
                return;
            }

            HideScreenColumns(configure(table)).Apply(grid);
        }

        private static GridColumns AutoVocabulary(GridColumns columns)
        {
            foreach (ModernDataGridColumn column in columns.ToArray())
            {
                if (column.DataPropertyName == Schemas.EquipmentPriority.AutoYn.Name)
                {
                    column.CheckTrueValue = Schemas.EquipmentPriority.AutoYn.Y.Value;
                    column.CheckFalseValue = Schemas.EquipmentPriority.AutoYn.N.Value;
                }
            }

            return columns;
        }

        private void OnEqpSelectionChanged(object sender, EventArgs e)
        {
            if (this.gridBinding)
            {
                return;
            }

            this.LoadPortPriorities(this.SelectedEquipmentId(), false);

            if (!this.JobRunMode())
            {
                this.DecideSelectedEquipment();
            }

            this.RefreshActionStates();
        }

        private void LoadPortPriorities(string eqpId, bool silent)
        {
            bool same = silent && eqpId == this.portEqpId && this.portData != null;

            if (!same)
            {
                this.InvalidateChannel(channelPorts);
                this.portData = null;
                this.portCurrent = null;
            }

            this.portEqpId = eqpId;
            this.portCard.Text = eqpId.Length > 0 ? "Port List — " + eqpId : "Port List";
            this.LoadChildEquipments(eqpId, silent);

            if (eqpId.Length > 0)
            {
                DataRow portLot = ActionRows.Copy(this.decision.Lot);
                this.LoadDependent(channelPorts, () => this.GetPortPriorities(eqpId, portLot), table => this.BindPortPriorities(table, same), !same && eqpId == this.decision.EqpId, same, same ? null : this.portCover);
            }
            else
            {
                this.retargetPorts = false;
                this.UpdateMaterialsPending();
                this.portCover.Reset();
                this.gridPortPriorities.DataSource = null;
            }
        }

        private void LoadChildEquipments(string eqpId, bool silent)
        {
            bool same = silent && eqpId == this.childEqpId && this.childData != null;

            if (!same)
            {
                this.InvalidateChannel(channelChildren);
                this.childData = null;
                this.childCurrent = null;
            }

            this.childEqpId = eqpId;

            if (eqpId.Length > 0)
            {
                DataRow childLot = ActionRows.Copy(this.decision.Lot);
                this.LoadDependent(channelChildren, () => this.GetChildEquipmentPriorities(eqpId, childLot), table => this.BindChildEquipments(table, same), false, same, null);
                return;
            }

            this.childData = null;
            this.BuildChildChips();
        }

        private void BindChildEquipments(DataTable children, bool silent)
        {
            bool merged;
            this.childData = this.BindJudged(
                    EquipmentLotContracts.ChildEquipmentTable, this.childData, ref this.childCurrent, children, Schemas.ChildEquipment.EqpId.Name, silent, silent, out merged);
            this.decision.DropDetached();

            if (this.childEqpId == this.decision.EqpId)
            {
                this.decision.ChildEquipmentList = this.childData;
                this.ResolveDecisionChild();
            }

            this.BuildChildChips();
            this.dependentSerial++;
            this.RefreshDecisionPanel();
            this.RefreshActionStates();
        }

        private void BuildChildChips()
        {
            bool bindingBefore = this.gridBinding;
            this.gridBinding = true;

            try
            {
                this.childChips.SuspendLayout();

                while (this.childChips.Controls.Count > 0)
                {
                    Control chip = this.childChips.Controls[0];
                    this.childChips.Controls.RemoveAt(0);
                    chip.Dispose();
                }

                if (this.childData != null)
                {
                    foreach (DataRow child in this.childData.Rows)
                    {
                        this.childChips.Controls.Add(this.ChildChip(child));
                    }
                }

                this.childChips.Visible = this.childChips.Controls.Count > 0;
                this.childChips.ResumeLayout();
                this.SyncChildChips();
            }
            finally
            {
                this.gridBinding = bindingBefore;
            }
        }

        private Modern.Lab.WinForms.Controls.Input.ModernChoiceChip ChildChip(DataRow child)
        {
            string status = TableHelper.CellText(child, Schemas.ChildEquipment.MesEqpStatCd.Name).Trim();
            string label = EquipmentLotPresenter.ChildLabel(
                    this.childEqpId,
                    TableHelper.CellText(child, Schemas.ChildEquipment.EqpId.Name),
                    TableHelper.CellText(child, Schemas.ChildEquipment.EqpNm.Name));
            Modern.Lab.WinForms.Controls.Input.ModernChoiceChip chip = new Modern.Lab.WinForms.Controls.Input.ModernChoiceChip();
            chip.Tag = child;
            chip.Text = status.Length == 0 ? label : label + " · " + status;
            chip.Spinning = status == Schemas.ChildEquipment.MesEqpStatCd.Run.Value;
            chip.Margin = new Padding(0, 0, ChildChipGap, ChildChipGap);
            chip.Height = ChildChipHeight;
            chip.Width = this.ChildChipWidth();
            chip.CheckedChanged += this.OnChildChipCheckedChanged;
            return chip;
        }

        private int ChildChipWidth()
        {
            int width = (this.childChips.ClientSize.Width - this.childChips.Padding.Horizontal - (ChildChipGap * 2)) / 2;
            return Math.Max(ChildChipMinWidth, width);
        }

        private void OnChildChipsSizeChanged(object sender, EventArgs e)
        {
            int width = this.ChildChipWidth();

            foreach (Control chip in this.childChips.Controls)
            {
                if (chip.Width != width)
                {
                    chip.Width = width;
                }
            }
        }

        private void SyncChildChips()
        {
            DataRow summary = EquipmentLotPresenter.JobSummary(this.decision);
            string eqpId = TableHelper.CellText(summary, Schemas.EquipmentPriority.EqpId.Name).Trim();
            string childId = eqpId == this.childEqpId ? TableHelper.CellText(summary, Schemas.LotPriority.ChildEqpId.Name).Trim() : string.Empty;
            bool runMode = this.JobRunMode();
            bool bindingBefore = this.gridBinding;
            this.gridBinding = true;

            try
            {
                foreach (Control control in this.childChips.Controls)
                {
                    Modern.Lab.WinForms.Controls.Input.ModernChoiceChip chip = control as Modern.Lab.WinForms.Controls.Input.ModernChoiceChip;

                    if (chip == null)
                    {
                        continue;
                    }

                    DataRow child = chip.Tag as DataRow;
                    string id = TableHelper.CellText(child, Schemas.ChildEquipment.EqpId.Name).Trim();
                    chip.Checked = childId.Length > 0 && id == childId;
                    chip.Enabled = !runMode && EquipmentLotPresenter.IsCandidate(child);
                }
            }
            finally
            {
                this.gridBinding = bindingBefore;
            }
        }

        private void OnChildChipCheckedChanged(object sender, EventArgs e)
        {
            if (this.gridBinding)
            {
                return;
            }

            Modern.Lab.WinForms.Controls.Input.ModernChoiceChip chip = (Modern.Lab.WinForms.Controls.Input.ModernChoiceChip)sender;
            DataRow child = CandidateRow(chip.Tag as DataRow);

            if (this.JobRunMode() || this.childEqpId != this.decision.EqpId || child == null || !chip.Checked)
            {
                this.SyncChildChips();
                return;
            }

            this.decision.ChildEquipment = child;
            this.SyncChildChips();
            this.RefreshDecisionPanel();
            this.RefreshActionStates();
        }

        private void ResolveDecisionChild()
        {
            if (EquipmentLotPresenter.HasActiveJob(this.decision.Lot))
            {
                this.decision.ChildEquipment = EquipmentLotPresenter.FindById(
                        this.childData, Schemas.ChildEquipment.EqpId.Name,
                        TableHelper.CellText(this.decision.Lot, Schemas.LotPriority.ChildEqpId.Name).Trim());
                return;
            }

            DataRow kept = EquipmentLotPresenter.FindById(this.childData, Schemas.ChildEquipment.EqpId.Name, this.decision.ChildEqpId);
            this.decision.ChildEquipment = kept ?? EquipmentLotPresenter.TopPriority(this.childData);
        }

        private void BindPortPriorities(DataTable ports, bool silent)
        {
            bool merged;
            this.portData = this.BindJudged(
                    EquipmentLotContracts.PortTable, this.portData, ref this.portCurrent, ports, Schemas.PortPriority.PortNm.Name, silent, silent, out merged);

            ConfigureGrid(
                    this.gridPortPriorities, this.portData, Judged(this.portCurrent),
                    PortPriorityColumns);

            if (!merged)
            {
                this.gridBinding = true;

                try
                {
                    this.gridPortPriorities.DataSource = this.portData;
                }
                finally
                {
                    this.gridBinding = false;
                }
            }

            this.decision.DropDetached();
            bool decisionPortsLost = this.portData == null && this.portEqpId.Length > 0 && this.portEqpId == this.decision.EqpId;

            if (decisionPortsLost)
            {
                this.decision.Port = null;
                this.decision.GoalPort = null;
                this.decisionPortEqpId = this.portEqpId;
            }

            this.ResolveDecisionPorts();
            this.SelectDecisionPortRow();

            if (this.portEqpId == this.decision.EqpId)
            {
                this.cyclePortsReflected = true;
                this.retargetPorts = false;
                this.UpdateMaterialsPending();

                if (decisionPortsLost)
                {
                    this.SyncDecisionDurables(false);
                }
                else if (this.durableListEqpId != this.decision.EqpId || this.durableListGoalPortNm.Length == 0)
                {
                    this.SyncDecisionDurablesIfReady();
                }
                else if (!EquipmentLotPresenter.HasActiveJob(this.decision.Lot)
                        && this.decision.GoalPortNm != this.durableListGoalPortNm)
                {
                    this.SyncDecisionDurables(false);
                }
            }

            this.dependentSerial++;
            this.RefreshDecisionPanel();
            this.RefreshActionStates();
        }

        private void SelectDecisionPortRow()
        {
            DataRow summary = EquipmentLotPresenter.JobSummary(this.decision);
            string eqpId = TableHelper.CellText(summary, Schemas.EquipmentPriority.EqpId.Name).Trim();
            string portNm = TableHelper.CellText(summary, Schemas.LotPriority.PortNm.Name).Trim();

            if (eqpId.Length == 0 || eqpId != this.portEqpId)
            {
                return;
            }

            this.gridBinding = true;

            try
            {
                this.SelectRow(this.gridPortPriorities, this.portData, Schemas.PortPriority.PortNm.Name, portNm);
            }
            finally
            {
                this.gridBinding = false;
            }
        }

        private void ResolveDecisionPortsIfEmpty()
        {
            if (this.decision.Port == null && this.decision.GoalPort == null)
            {
                this.ResolveDecisionPorts();
            }
        }

        private void ResolveDecisionPorts()
        {
            if (this.decision.Equipment == null)
            {
                this.decision.Port = null;
                this.decision.GoalPort = null;
                this.decision.ChildEquipment = null;
                this.decision.ChildEquipmentList = null;
            }

            if (EquipmentLotPresenter.HasActiveJob(this.decision.Lot))
            {
                return;
            }

            if (this.portData == null || this.portEqpId.Length == 0 || this.portEqpId != this.decision.EqpId)
            {
                return;
            }

            this.decisionPortEqpId = this.portEqpId;

            if (!this.retargetPorts && (this.decision.PortNm.Length > 0 || this.decision.GoalPortNm.Length > 0))
            {
                DataRow keptPort = EquipmentLotPresenter.FindById(this.portData, Schemas.PortPriority.PortNm.Name, this.decision.PortNm);
                DataRow keptGoalPort = EquipmentLotPresenter.FindById(this.portData, Schemas.PortPriority.PortNm.Name, this.decision.GoalPortNm);
                this.decision.Port = keptPort ?? EquipmentLotPresenter.TopPort(this.portData, Schemas.PortPriority.PortTyp.Input.Value);
                this.decision.GoalPort = keptGoalPort ?? EquipmentLotPresenter.TopPort(this.portData, Schemas.PortPriority.PortTyp.Output.Value);

                if (this.decision.GoalPort == null && this.decision.Port != null
                        && EquipmentLotPresenter.PortTyp(this.decision.Port) == Schemas.PortPriority.PortTyp.InputOutput.Value)
                {
                    this.decision.GoalPort = this.decision.Port;
                }

                return;
            }

            string recommendedPortNm = TableHelper.CellText(this.decision.Equipment, Schemas.EquipmentPriority.PortNm.Name).Trim();
            string recommendedGoalPortNm = TableHelper.CellText(this.decision.Equipment, Schemas.EquipmentPriority.GoalPortNm.Name).Trim();

            DataRow port = recommendedPortNm.Length > 0
                    ? CandidateRow(EquipmentLotPresenter.FindById(this.portData, Schemas.PortPriority.PortNm.Name, recommendedPortNm))
                    : null;
            DataRow goalPort = recommendedGoalPortNm.Length > 0
                    ? CandidateRow(EquipmentLotPresenter.FindById(this.portData, Schemas.PortPriority.PortNm.Name, recommendedGoalPortNm))
                    : null;

            if (port == null)
            {
                port = EquipmentLotPresenter.TopPort(this.portData, Schemas.PortPriority.PortTyp.Input.Value);
            }

            if (goalPort == null)
            {
                goalPort = EquipmentLotPresenter.TopPort(this.portData, Schemas.PortPriority.PortTyp.Output.Value);
            }

            if (port != null && EquipmentLotPresenter.PortTyp(port) == Schemas.PortPriority.PortTyp.InputOutput.Value)
            {
                goalPort = port;
            }

            this.decision.Port = port;
            this.decision.GoalPort = goalPort;
        }

        private void BindLotPriorities(DataTable lots, bool silent)
        {
            string previousLotId = this.decision.LotId;
            string colorsBefore = EquipmentLotPresenter.JobColorSignature(this.lotData);
            bool merged;
            this.lotData = this.BindJudged(
                    EquipmentLotContracts.LotTable, this.lotData, ref this.lotCurrent, lots, Schemas.LotPriority.RowKey, silent, silent, out merged);
            this.decision.DropDetached();

            if (Judged(this.lotCurrent))
            {
                EquipmentLotPresenter.MarkJobColors(this.lotData);
            }

            ConfigureGrid(this.gridLotPriorities, this.lotData, Judged(this.lotCurrent), LotPriorityColumns);

            if (!merged || (this.lotData != null && colorsBefore != EquipmentLotPresenter.JobColorSignature(this.lotData)))
            {
                bool bindingBefore = this.gridBinding;
                this.gridBinding = true;

                try
                {
                    this.gridLotPriorities.DataSource = this.lotData;
                }
                finally
                {
                    this.gridBinding = bindingBefore;
                }
            }

            this.RefreshDecisionTitles();

            this.decision.Lot = EquipmentLotPresenter.FindById(this.lotData, Schemas.LotPriority.RowKey, previousLotId)
                    ?? EquipmentLotPresenter.TopPriority(this.lotData);

            this.ResolveDecisionPortsIfEmpty();
            this.cycleLotsReflected = true;
            this.SyncDecisionDurablesIfReady();

            this.gridBinding = true;

            try
            {
                if (!silent || this.gridLotPriorities.SelectedItem == null)
                {
                    this.SelectRow(this.gridLotPriorities, this.lotData, Schemas.LotPriority.RowKey, this.decision.LotId);
                }
            }
            finally
            {
                this.gridBinding = false;
            }

            this.dependentSerial++;
            this.LoadRequestSamples(this.SelectedLotId(), this.SelectedRequestSerialNo(), silent);
            this.ReviewJobStateTracking();
            this.RefreshDecisionPanel();
            this.RefreshActionStates();

            this.FillLotFinder();
            if (lots != null)
            {
                this.LoadLotEquipments(this.decision.LotId);
            }
        }

        private void FillLotFinder()
        {
            string keepLotId = this.SelectedLotId();
            DataTable finder = new DataTable("LOT_FINDER");
            finder.Columns.Add(Schemas.LotPriority.LotId.Name);
            finder.Columns.Add(Schemas.LotPriority.ReqSerialNo.Name);

            if (this.lotData != null)
            {
                DataView view = this.lotData.DefaultView;

                for (int index = 0; index < view.Count; index++)
                {
                    string lotId = TableHelper.CellText(view[index].Row, Schemas.LotPriority.LotId.Name).Trim();

                    if (lotId.Length > 0)
                    {
                        finder.Rows.Add(lotId, TableHelper.CellText(view[index].Row, Schemas.LotPriority.ReqSerialNo.Name).Trim());
                    }
                }
            }

            this.lotFinderBinding = true;

            try
            {
                this.cboLot.DataSource = finder;
                this.cboLot.SelectedValue = keepLotId.Length > 0 ? keepLotId : null;
            }
            finally
            {
                this.lotFinderBinding = false;
            }
        }

        private void SyncLotFinder(string lotId)
        {
            if (this.lotFinderBinding || LotFinderValue(this.cboLot) == lotId)
            {
                return;
            }

            this.lotFinderBinding = true;

            try
            {
                this.cboLot.SelectedValue = lotId.Length > 0 ? lotId : null;
            }
            finally
            {
                this.lotFinderBinding = false;
            }
        }

        private static string LotFinderValue(Modern.Lab.WinForms.Controls.Selection.ModernComboBox finder)
        {
            return (Convert.ToString(finder.SelectedValue) ?? string.Empty).Trim();
        }

        private void OnLotFinderChanged(object sender, EventArgs e)
        {
            if (this.lotFinderBinding || this.gridBinding)
            {
                return;
            }

            string lotId = LotFinderValue(this.cboLot);

            if (lotId.Length > 0)
            {
                this.SelectRow(this.gridLotPriorities, this.lotData, Schemas.LotPriority.RowKey, lotId);
            }
        }

        private void BindDurablePriorities(DataTable durables, bool silent)
        {
            this.BindDurablePriorities(durables, silent, silent, false);
        }

        private void BindDurablePriorities(DataTable durables, bool silent, bool merge, bool retarget)
        {
            string keepDurable = this.decision.GoalDurableId;
            bool merged;
            this.durableData = this.BindJudged(
                    EquipmentLotContracts.DurableTable, this.durableData, ref this.durableCurrent, durables, Schemas.DurablePriority.RowKey, silent, merge, out merged);

            this.BindDurableCombo(this.durableData);
            this.decision.DropDetached();
            this.lblDurable.Text = ValueOrDash(this.GoalDurableTyp());

            if (this.JobRunMode())
            {
                string jobDurableId = EquipmentLotPresenter.JobGoalDurableId(this.decision.Lot);
                this.decision.GoalDurable = jobDurableId.Length == 0
                        ? null
                        : EquipmentLotPresenter.FindById(this.durableData, Schemas.DurablePriority.RowKey, jobDurableId);
            }
            else
            {
                DataRow keptDurable = retarget
                        ? null
                        : EquipmentLotPresenter.FindById(this.durableData, Schemas.DurablePriority.RowKey, keepDurable);
                this.decision.GoalDurable = keptDurable ?? EquipmentLotPresenter.TopPriority(this.durableData);
            }

            this.durablesPending = false;
            this.UpdateMaterialsPending();
            this.dependentSerial++;
            this.RefreshDecisionPanel();
            this.RefreshActionStates();
        }

        private void FollowSelectedLot()
        {
            DataRow lot = LiveRow(this.gridLotPriorities.SelectedItem);

            if (lot == null || object.ReferenceEquals(lot, this.decision.Lot))
            {
                return;
            }

            bool sameLot = TableHelper.CellText(lot, Schemas.LotPriority.LotId.Name).Trim() == this.decision.LotId;
            this.decision.Lot = lot;

            if (!sameLot)
            {
                this.lotRetarget = true;
                this.lotEquipmentsPending = true;
                this.UpdateMaterialsPending();
            }

            this.ResolveDecisionPortsIfEmpty();
            this.RefreshDecisionPanel();
            this.RefreshActionStates();

            if (!sameLot)
            {
                this.LoadLotEquipments(this.decision.LotId);
            }
        }

        private async void LoadLotEquipments(string lotId)
        {
            if (lotId.Length == 0)
            {
                this.lotRetarget = false;
                this.lotEquipmentsPending = false;
                this.UpdateMaterialsPending();
                this.BindEquipmentPriorityTable(null, string.Empty, true, false);
                return;
            }

            bool retarget = this.lotRetarget;
            string keepEqpId = this.decision.EqpId;
            DataRow equipmentLot = this.decision.Lot;
            LoadOutcome<DataTable> outcome = await this.FetchAsync(
                    channelEquipments, () => this.GetEquipmentPriorities(equipmentLot), !this.userRefresh);

            if (!outcome.IsCurrent || lotId != this.decision.LotId)
            {
                return;
            }

            this.lotEquipmentsPending = false;
            this.UpdateMaterialsPending();

            if (outcome.Failure != null)
            {
                this.BindEquipmentPriorityTable(null, string.Empty, false, false);
                return;
            }

            this.lotRetarget = false;

            if (retarget)
            {
                this.retargetPorts = true;
                this.UpdateMaterialsPending();
            }

            bool durablesListed = this.durableListGoalPortNm.Length > 0 && this.durableListLotId != lotId;
            this.BindEquipmentPriorityTable(outcome.Value, keepEqpId, true, true, retarget);

            if (durablesListed)
            {
                this.SyncDecisionDurables(true);
            }
        }

        private void OnLotSelectionChanged(object sender, EventArgs e)
        {
            if (this.gridBinding)
            {
                return;
            }

            this.FollowSelectedLot();
            this.SyncLotFinder(this.SelectedLotId());
            string key = this.SelectedRequestSerialNo();

            if (key.Length > 0 && key == this.reqSerialNo && this.requestData != null)
            {
                this.requestLotId = this.SelectedLotId();
                return;
            }

            this.LoadRequestSamples(this.SelectedLotId(), key, false);
        }

        private void OnPortSelectionChanged(object sender, EventArgs e)
        {
            if (this.gridBinding)
            {
                return;
            }

            if (!this.JobRunMode())
            {
                this.DecideSelectedPort();
            }

            this.RefreshActionStates();
        }

        private async void LoadRequestSamples(string lotId, string reqSerialNo, bool silent)
        {
            string key = (reqSerialNo ?? string.Empty).Trim();
            bool sameRequest = silent && key.Length > 0 && key == this.reqSerialNo && this.requestData != null;
            bool coverInstead = !sameRequest && !silent && key.Length > 0 && this.requestData != null;

            if (!sameRequest && !coverInstead)
            {
                this.ClearRequestSamples();
            }

            int requestSerial = ++this.requestLoadSerial;

            if (coverInstead)
            {
                this.InvalidateChannel(channelRequests);
                this.requestSnapshot = null;
                this.requestBusy.Busy = true;
            }

            this.requestLotId = lotId;
            this.reqSerialNo = key;
            this.UpdateRequestTitle();

            this.lblRequestEmpty.Text = lotId.Length == 0 ? "Select a lot" : "No request on this lot";

            if (key.Length == 0)
            {
                if (!this.requestBusy.IsDisposed)
                {
                    this.requestBusy.Busy = false;
                }

                return;
            }

            LoadOutcome<RequestSnapshot> outcome;

            try
            {
                outcome = await this.FetchAsync(
                        channelRequests, () => this.GetRequestSnapshot(key), silent && !this.userRefresh);
            }
            finally
            {
                if (requestSerial == this.requestLoadSerial && !this.requestBusy.IsDisposed)
                {
                    this.requestBusy.Busy = false;
                }
            }

            if (!outcome.IsCurrent || key != this.reqSerialNo)
            {
                return;
            }

            if (outcome.Failure != null)
            {
                if (coverInstead)
                {
                    this.ClearRequestSamples();
                    this.requestLotId = lotId;
                    this.reqSerialNo = key;
                    this.UpdateRequestTitle();
                }

                this.BindRequestSamples(null, false);
                return;
            }

            this.BindRequestSamples(outcome.Value, silent);
        }

        private RequestSnapshot GetRequestSnapshot(string reqSerialNo)
        {
            System.Threading.Tasks.Task<DataTable> masterTask = System.Threading.Tasks.Task.Run(
                    () => this.GetRequests(reqSerialNo));
            System.Threading.Tasks.Task<DataTable> detailTask = System.Threading.Tasks.Task.Run(
                    () => this.GetSamples(reqSerialNo));
            System.Threading.Tasks.Task.WaitAll(masterTask, detailTask);
            return RequestSnapshot.Create(reqSerialNo, masterTask.Result, detailTask.Result);
        }

        private void BindRequestSamples(RequestSnapshot snapshot, bool silent)
        {
            DataTable master = snapshot == null ? null : snapshot.Master.Copy();
            DataTable details = snapshot == null ? null : snapshot.Details.Copy();

            bool merged;
            this.requestData = this.BindJudged(
                    EquipmentLotContracts.RequestTable, this.requestData, ref this.requestCurrent,
                    master, Schemas.Request.RowKey, silent, silent, out merged);

            this.BindRequestFields(this.requestData, snapshot == null ? string.Empty : snapshot.Remarks);

            bool samplesMerged;
            this.sampleData = this.BindJudged(
                    EquipmentLotContracts.SampleTable, this.sampleData, ref this.sampleCurrent,
                    details, null, silent, silent && merged, out samplesMerged);

            ConfigureGrid(this.gridSamples, this.sampleData, Judged(this.sampleCurrent), SampleColumns);

            if (!samplesMerged)
            {
                this.gridSamples.DataSource = this.sampleData;
            }

            this.requestSnapshot = snapshot;
        }

        private void BindRequestFields(DataTable overview, string remarks)
        {
            GridColumns columns = Judged(this.requestCurrent)
                    ? RequestOverviewColumns(overview)
                    : GridColumns.Of(overview);
            ModernDataGridColumn[] members = columns.ToArray();
            DataRow row = overview == null || overview.Rows.Count == 0 ? null : overview.Rows[0];
            List<ModernFieldDefinition> fields = new List<ModernFieldDefinition>();

            for (int index = 0; index < members.Length; index++)
            {
                string member = members[index].DataPropertyName;
                fields.Add(new ModernFieldDefinition(member)
                {
                    IsLink = Judged(this.requestCurrent)
                            && string.Equals(member, ReqSerialNoColumn, StringComparison.OrdinalIgnoreCase)
                });
            }

            int fieldColumns = RequestFieldColumns(fields.Count);

            if (this.fieldRequest.Columns != fieldColumns)
            {
                this.fieldRequest.Columns = fieldColumns;
                this.requestFieldShape = string.Empty;
            }

            string shape = RequestFieldShape(fields);

            if (!string.Equals(shape, this.requestFieldShape, StringComparison.Ordinal))
            {
                this.fieldRequest.DefineFields(fields.ToArray());
                this.requestFieldShape = shape;
            }

            this.fieldRequest.SetRow(row);
            int fieldRows = (fields.Count + fieldColumns - 1) / fieldColumns;
            int fieldHeight = Math.Max(1, fieldRows) * RequestFieldRowHeight * this.DeviceDpi / 96;
            int remarkHeight = row == null ? 0 : RequestRemarkHeight * this.DeviceDpi / 96;
            int headerHeight = fieldHeight + remarkHeight;

            if (row != null)
            {
                if (this.tableRequestMaster.Height != headerHeight)
                {
                    this.tableRequestMaster.RowStyles[0].Height = fieldHeight;
                    this.tableRequestMaster.RowStyles[1].Height = remarkHeight;
                    this.tableRequestMaster.Height = headerHeight;
                }

                this.requestHeaderHeight = headerHeight;
                this.FitRequestHeader(headerHeight);
            }
            this.lblRequestRemarkCaption.Text = "Purpose";
            this.ApplyRemarkLine(row == null ? string.Empty : remarks);
            this.panelRequestRemark.Visible = row != null;
            this.tableRequestMaster.Visible = row != null;
            this.lblRequestEmpty.Visible = row == null;
        }

        private void OnRequestRemarkMoreClick(object sender, EventArgs e)
        {
            string remarks = this.btnRequestRemarkMore.Tag as string;

            if (!string.IsNullOrEmpty(remarks))
            {
                Modern.Lab.WinForms.Controls.Dialogs.ModernMessageDialog.ShowInformation(this, "Purpose", remarks);
            }
        }

        private void ApplyRemarkLine(string remarks)
        {
            string full = (remarks ?? string.Empty).Trim();
            this.lblRequestRemark.Text = full.Length == 0 ? "-" : full.Replace("\r\n", " ").Replace('\n', ' ');
            this.btnRequestRemarkMore.Tag = full;
            this.btnRequestRemarkMore.Enabled = full.Length > 0;
            this.btnRequestRemarkMore.Location = new System.Drawing.Point(
                    this.panelRequestRemark.ClientSize.Width - this.btnRequestRemarkMore.Width, this.lblRequestRemark.Top);
            this.lblRequestRemark.Dock = System.Windows.Forms.DockStyle.None;
            this.lblRequestRemark.Padding = System.Windows.Forms.Padding.Empty;
            this.lblRequestRemark.SetBounds(
                    0, this.lblRequestRemarkCaption.Bottom,
                    System.Math.Max(0, this.btnRequestRemarkMore.Left - 4), this.btnRequestRemarkMore.Height);
            this.lblRequestRemark.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.btnRequestRemarkMore.BringToFront();
        }
        private const int PortListHeight = 228;

        private const int PortColumnsWidth = 340;

        private const int DecisionHeight = 288;

        private const int RequestFieldBaseColumns = 3;

        private const int RequestFieldRowHeight = 32;

        private const int RequestFieldMaxColumns = 4;

        private const int RequestRemarkHeight = 34;

        private const int RequestDetailMinHeight = 80;

        private static int RequestFieldColumns(int count)
        {
            int columns = RequestFieldBaseColumns;

            while (columns < RequestFieldMaxColumns && count > columns && count % columns == 1)
            {
                columns++;
            }

            return columns;
        }

        private static string RequestFieldShape(List<ModernFieldDefinition> fields)
        {
            StringBuilder shape = new StringBuilder();

            for (int index = 0; index < fields.Count; index++)
            {
                shape.Append(fields[index].Member).Append(fields[index].IsLink ? "*" : string.Empty).Append('|');
            }

            return shape.ToString();
        }

        private void OnDurablePanelResize(object sender, EventArgs e)
        {
            int width = this.durablePanel.ClientSize.Width - this.cboDurable.Left - 6;

            if (width > 0 && this.cboDurable.Width != width)
            {
                this.cboDurable.Width = width;
            }
        }

        private void OnRequestSplitSizeChanged(object sender, EventArgs e)
        {
            if (this.requestHeaderHeight > 0)
            {
                this.FitRequestHeader(this.requestHeaderHeight);
            }
        }

        private void FitRequestHeader(int desiredHeight)
        {
            int room = this.splitRequest.Height - this.splitRequest.SplitterWidth;
            int detailMinimum = Math.Max(
                    this.splitRequest.Panel2MinSize, RequestDetailMinHeight * this.DeviceDpi / 96);

            if (room < this.splitRequest.Panel1MinSize + detailMinimum)
            {
                return;
            }

            int maximum = room - detailMinimum;
            int height = Math.Max(this.splitRequest.Panel1MinSize, Math.Min(desiredHeight, maximum));

            if (this.splitRequest.SplitterDistance == height)
            {
                return;
            }

            try
            {
                this.splitRequest.SplitterDistance = height;
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void OnRequestFieldLinkClick(object sender, ModernFieldLinkClickEventArgs e)
        {
            if (string.Equals(e.Member, ReqSerialNoColumn, StringComparison.OrdinalIgnoreCase))
            {
                this.ShowRequestSnapshot(e.Value);
            }
        }

        private void OnDurableIdLinkClick(object sender, GridButtonClickEventArgs e)
        {
            string durableId = DurableInfoDialogForm.LinkedDurableId(
                    e,
                    Schemas.LotPriority.DurableId.Name,
                    Schemas.LotPriority.GoalDurableId.Name,
                    Schemas.DurablePriority.DurableId.Name);

            if (durableId.Length == 0)
            {
                return;
            }

            this.ShowDurableInfo(durableId);
        }

        protected virtual void ShowDurableInfo(string durableId)
        {
            DurableInfoDialogForm.ShowFor(this, durableId);
        }

        private void OnLotRequestLinkClick(object sender, GridButtonClickEventArgs e)
        {
            DataRowView row = e.Item as DataRowView;

            if (row == null || !string.Equals(e.DataPropertyName, ReqSerialNoColumn, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            this.ShowRequestSnapshot(TableHelper.CellText(row.Row, e.DataPropertyName));
        }

        private void ShowRequestSnapshot(string reqSerialNo)
        {
            string key = (reqSerialNo ?? string.Empty).Trim();
            RequestSnapshot snapshot = this.requestSnapshot;

            if (snapshot == null || key.Length == 0 || key != this.reqSerialNo || key != snapshot.RequestSerialNo)
            {
                return;
            }

            using (RequestInfoDialogForm dialog = new RequestInfoDialogForm())
            {
                dialog.SetRequest(
                        snapshot.RequestSerialNo, snapshot.Master.Copy(), snapshot.Details.Copy(), snapshot.Remarks);
                dialog.ShowDialog(this);
            }
        }

        private void UpdateRequestTitle()
        {
            this.requestCard.Text = this.reqSerialNo.Length > 0 ? "Request List — " + this.reqSerialNo : "Request List";
        }

        private static DataRow Candidate(object selectedItem)
        {
            DataRowView view = selectedItem as DataRowView;
            return view == null ? null : CandidateRow(view.Row);
        }

        private static DataRow LiveRow(object selectedItem)
        {
            DataRowView view = selectedItem as DataRowView;
            return view == null || !JobDecision.IsLive(view.Row) ? null : view.Row;
        }

        private static DataRow CandidateRow(DataRow row)
        {
            if (!JobDecision.IsLive(row) || !EquipmentLotPresenter.IsCandidate(row))
            {
                return null;
            }

            return row;
        }

        private bool JobRunMode()
        {
            return EquipmentLotPresenter.HasActiveJob(this.decision.Lot);
        }

        private void DecideSelectedEquipment()
        {
            DataRow equipment = Candidate(this.gridEquipmentPriorities.SelectedItem);

            if (equipment == null
                    || TableHelper.CellText(equipment, Schemas.EquipmentPriority.EqpId.Name).Trim() == this.decision.EqpId)
            {
                return;
            }

            this.decision.Equipment = equipment;
            this.decision.Port = null;
            this.decision.GoalPort = null;
            this.decision.ChildEquipment = null;
            this.decision.ChildEquipmentList = null;
            this.silentRefresh = false;
            this.ApplyEquipmentDecision();

            if (this.cyclePortsReflected)
            {
                this.SyncDecisionDurables(false);
            }
        }

        private void DecideSelectedPort()
        {
            DataRow port = Candidate(this.gridPortPriorities.SelectedItem);

            if (port == null)
            {
                return;
            }

            if (this.portEqpId != this.decision.EqpId)
            {
                DataRow equipment = CandidateRow(
                        EquipmentLotPresenter.FindById(this.equipmentData, Schemas.EquipmentPriority.RowKey, this.portEqpId));

                if (equipment == null)
                {
                    return;
                }

                this.decision.Equipment = equipment;
                this.decision.Port = null;
                this.decision.GoalPort = null;
                this.decision.ChildEquipment = null;
                this.decision.ChildEquipmentList = null;
                this.ApplyPortChoice(port);
                this.silentRefresh = false;
                this.RefreshDecisionPanel();
                this.RefreshActionStates();
                this.cycleRefresh = false;
                this.cyclePortsReflected = true;
                this.cycleLotsReflected = true;
                this.SyncDecisionDurables(false);
                return;
            }

            string goalPortBefore = this.decision.GoalPortNm;
            this.ApplyPortChoice(port);
            this.RefreshDecisionPanel();
            this.RefreshActionStates();

            if (this.decision.GoalPortNm != goalPortBefore)
            {
                this.SyncDecisionDurables(false);
            }
        }

        private void ApplyPortChoice(DataRow port)
        {
            this.decisionPortEqpId = this.portEqpId;
            string portTyp = EquipmentLotPresenter.PortTyp(port);

            if (portTyp == Schemas.PortPriority.PortTyp.Input.Value || portTyp == Schemas.PortPriority.PortTyp.InputOutput.Value)
            {
                this.decision.Port = port;
            }

            if (portTyp == Schemas.PortPriority.PortTyp.Output.Value || portTyp == Schemas.PortPriority.PortTyp.InputOutput.Value)
            {
                this.decision.GoalPort = port;
            }
        }

        private void DecideLot(DataRow lot)
        {
            bool jobBefore = EquipmentLotPresenter.HasActiveJob(this.decision.Lot);
            this.decision.Lot = lot;
            this.ResolveDecisionPortsIfEmpty();
            this.RefreshDecisionPanel();
            this.RefreshActionStates();

            if (jobBefore || EquipmentLotPresenter.HasActiveJob(lot))
            {
                this.SyncDecisionDurables(false);
            }
        }

        private void OnDurableComboChanged(object sender, EventArgs e)
        {
            if (this.durableComboBinding)
            {
                return;
            }

            if (this.JobRunMode())
            {
                this.NotifyJobRunMode();
                return;
            }

            DataRow durable = CandidateRow(EquipmentLotPresenter.FindById(
                    this.durableData, Schemas.DurablePriority.RowKey, Convert.ToString(this.cboDurable.SelectedValue)));

            if (durable == null)
            {
                this.SyncDurableCombo();
                return;
            }

            this.decision.GoalDurable = durable;
            this.RefreshDecisionPanel();
            this.RefreshActionStates();
        }

        private void BindDurableCombo(DataTable source)
        {
            this.durableComboBinding = true;

            try
            {
                DataTable choices = null;

                if (source != null)
                {
                    choices = source.Clone();

                    foreach (DataRow row in source.Rows)
                    {
                        if (CandidateRow(row) != null)
                        {
                            choices.ImportRow(row);
                        }
                    }
                }

                this.cboDurable.DataSource = choices;
                this.cboDurable.Enabled = choices != null && choices.Rows.Count > 0;
            }
            finally
            {
                this.durableComboBinding = false;
            }
        }

        private void SyncDurableCombo()
        {
            string goalDurableId = this.decision.GoalDurableId;

            if (Convert.ToString(this.cboDurable.SelectedValue) == goalDurableId)
            {
                return;
            }

            this.durableComboBinding = true;

            try
            {
                this.cboDurable.SelectedValue = goalDurableId.Length == 0 ? null : goalDurableId;
            }
            finally
            {
                this.durableComboBinding = false;
            }
        }

        private void RefreshDecisionTitles()
        {
            DataRow summary = EquipmentLotPresenter.JobSummary(this.decision);
            string word = this.JobRunMode() ? "Job " : "Decided ";
            this.eqpCard.TitleRightText = DecidedTitle(
                    word, TableHelper.CellText(summary, Schemas.EquipmentPriority.EqpId.Name), this.equipmentData, "equipment");
            this.lotCard.TitleRightText = DecidedTitle("Decided ", this.decision.LotId, this.lotData, "lots");
            this.portCard.TitleRightText = string.Empty;
            this.RefreshDecisionCaption();
        }

        private static string DecidedTitle(string word, string decided, DataTable table, string noun)
        {
            string count = table == null ? string.Empty : table.Rows.Count.ToString("N0") + " " + noun;

            if (decided == null || decided.Length == 0)
            {
                return count;
            }

            return count.Length == 0 ? word + decided : word + decided + "  ·  " + count;
        }

        private void RefreshDecisionPanel()
        {
            this.SyncDurableCombo();
            DataRow lot = this.decision.Lot;
            DataRow summary = EquipmentLotPresenter.JobSummary(this.decision);

            string portNm = TableHelper.CellText(summary, Schemas.LotPriority.PortNm.Name);
            string goalPortNm = TableHelper.CellText(summary, Schemas.LotPriority.GoalPortNm.Name);
            string goalDurableTyp = TableHelper.CellText(summary, Schemas.DurablePriority.DurableTyp.Name);
            string mode = TableHelper.CellText(summary, Schemas.EquipmentPriority.CommStatTyp.Name);
            string jobState = TableHelper.CellText(summary, Schemas.LotPriority.LastEventCd.Name);

            this.kpiEquipment.Value = ValueOrDash(TableHelper.CellText(summary, Schemas.EquipmentPriority.EqpId.Name));
            string childEqpId = TableHelper.CellText(summary, Schemas.LotPriority.ChildEqpId.Name).Trim();
            DataRow childRow = this.decision.ChildEqpId == childEqpId ? this.decision.ChildEquipment : null;
            this.kpiEquipment.BottomLabel = EquipmentLotPresenter.ChildLabel(
                    TableHelper.CellText(summary, Schemas.EquipmentPriority.EqpId.Name),
                    childEqpId,
                    JobDecision.IsLive(childRow) ? TableHelper.CellText(childRow, Schemas.ChildEquipment.EqpNm.Name) : string.Empty);
            this.kpiPorts.Value = portNm.Length == 0 && goalPortNm.Length == 0
                    ? "-"
                    : ValueOrDash(portNm) + " → " + ValueOrDash(goalPortNm);
            string lotValue;
            string lotBottomLabel;
            EquipmentLotPresenter.LotIdCardLines(this.decision.LotId, out lotValue, out lotBottomLabel);
            this.kpiLot.BottomLabel = lotBottomLabel;
            this.kpiLot.Value = ValueOrDash(lotValue);
            string lotFlowOper = EquipmentLotPresenter.LotFlowOper(lot);
            this.kpiLot.Title = lotFlowOper.Length == 0 ? "Lot" : "Lot  " + lotFlowOper;
            string durableId = TableHelper.CellText(summary, Schemas.LotPriority.DurableId.Name);
            string goalDurableId = TableHelper.CellText(summary, Schemas.LotPriority.GoalDurableId.Name);
            this.kpiDurable.Title = (goalDurableTyp.Length == 0 ? "Durable" : "Durable · " + goalDurableTyp)
                    + (durableId.Length == 0 ? string.Empty : "  from " + durableId);
            this.kpiDurable.Value = ValueOrDash(goalDurableId);

            string procState = JobDecision.IsLive(lot)
                    ? TableHelper.CellText(lot, Schemas.LotPriority.MesProcStatCd.Name).Trim()
                    : string.Empty;

            this.badgeMode.Text = ValueOrDash(mode);
            this.badgeMode.ColorValue = mode;
            this.badgeProc.Text = ValueOrDash(procState);
            this.badgeProc.ColorValue = procState;
            this.badgeJob.Text = jobState.Length == 0 ? "No job" : jobState;
            this.badgeJob.ColorValue = jobState;
            this.badgeJob.SpinValues = Schemas.LotPriority.LastEventCd.JobStart.Value;
            this.badgeProc.Left = this.badgeMode.Right + 8;
            this.badgeJob.Left = this.badgeProc.Right + 8;
        }

        private static string ValueOrDash(string value)
        {
            return string.IsNullOrEmpty(value) ? "-" : value;
        }

        private void RefreshActionStates()
        {
            this.ApplyActionReason(
                    this.btnJobPrep, JobActionReason(EquipmentLotPresenter.ActionJobPrep, this.decision));
            this.ApplyActionReason(
                    this.btnJobStart, JobActionReason(EquipmentLotPresenter.ActionJobStart, this.decision));
            this.ApplyActionReason(
                    this.btnJobEnd, JobActionReason(EquipmentLotPresenter.ActionJobEnd, this.decision));
            string targetText = EquipmentLotPresenter.StatusLineText(this.decision);

            if (!string.Equals(this.lblTarget.Text, targetText, StringComparison.Ordinal))
            {
                this.lblTarget.Text = targetText;
                this.FlashTarget();
            }

            this.RefreshDecisionTitles();
        }

        private void FlashTarget()
        {
            if (this.targetRestColor.IsEmpty)
            {
                this.targetRestColor = this.lblTarget.ForeColor;
            }

            this.lblTarget.ForeColor = Modern.Lab.Theming.ModernTokenColors.Get(
                    "Brush.Accent", Modern.Lab.Theming.ModernTheme.Accent);
            this.targetFlashTimer.Stop();
            this.targetFlashTimer.Start();
        }

        private void OnTargetFlashElapsed(object sender, EventArgs e)
        {
            this.targetFlashTimer.Stop();

            if (!this.targetRestColor.IsEmpty)
            {
                this.lblTarget.ForeColor = this.targetRestColor;
            }
        }

        private bool Allows(string key)
        {
            return JobActionReason(key, this.decision) == null;
        }

        private void OnJobPrepClick(object sender, EventArgs e)
        {
            this.ExecuteJob(EquipmentLotPresenter.ActionJobPrep);
        }

        private void OnJobStartClick(object sender, EventArgs e)
        {
            this.ExecuteJob(EquipmentLotPresenter.ActionJobStart);
        }

        private void OnJobEndClick(object sender, EventArgs e)
        {
            this.ExecuteJob(EquipmentLotPresenter.ActionJobEnd);
        }

        private void ExecuteJob(string key)
        {
            if (!this.Allows(key))
            {
                return;
            }

            EquipmentJobDialogOptions options = new EquipmentJobDialogOptions();
            options.Title = EquipmentLotPresenter.JobLabel(key) + " — " + this.decision.LotId;
            options.SourceCaption = options.Title;
            options.OkText = EquipmentLotPresenter.JobLabel(key);
            if (key == EquipmentLotPresenter.ActionJobPrepCancel || key == EquipmentLotPresenter.ActionJobStartCancel)
            {
                options.OkTopLabel = EquipmentLotPresenter.JobLabel(key == EquipmentLotPresenter.ActionJobPrepCancel
                    ? EquipmentLotPresenter.ActionJobPrep : EquipmentLotPresenter.ActionJobStart);
                options.OkText = EquipmentLotPresenter.CancelWord;
            }
            options.Target = new JobDecision
            {
                Lot = ActionRows.Copy(this.decision.Lot),
                Equipment = ActionRows.Copy(EquipmentLotPresenter.JobEquipment(this.decision)),
                Port = ActionRows.Copy(this.decision.Port),
                GoalPort = ActionRows.Copy(this.decision.GoalPort),
                ChildEquipment = ActionRows.Copy(this.decision.ChildEquipment),
                ChildEquipmentList = this.decision.ChildEquipmentList,
                GoalDurable = ActionRows.Copy(this.decision.GoalDurable)
            };
            options.ExecutionTarget = new JobExecutionTarget(options.Target, string.Empty);
            options.Source = EquipmentLotPresenter.JobTargetSummary(options.ExecutionTarget);
            options.SourceFields = JobSummaryFields();
            options.DefaultDescription = DefaultDescriptions.ForJob(key);

            if (key == EquipmentLotPresenter.ActionCancelOper)
            {
                JobSkipDialogResult skip = this.OpenSkipDialog(options.Target.Lot);

                if (skip != null)
                {
                    this.ProcessSkip(options, skip);
                }

                return;
            }

            if (key == EquipmentLotPresenter.ActionBackToPrevOper)
            {
                this.OpenBackToPrevOperDialog(key, options);
                return;
            }

            JudgeKind judge = key == EquipmentLotPresenter.ActionJobEnd
                    ? JudgeKindOf(this.JobGroupId(options.Source))
                    : JudgeKind.None;

            if (judge != JudgeKind.None)
            {
                JudgeDialogOptions judgeOptions = new JudgeDialogOptions();
                judgeOptions.Title = options.Title;
                judgeOptions.OkText = options.OkText;
                judgeOptions.Kind = judge;
                judgeOptions.GoalDurableId = TableHelper.CellText(options.Source, Schemas.LotPriority.GoalDurableId.Name).Trim();
                judgeOptions.ReqSerialNo = this.decision.Lot == null
                        ? string.Empty
                        : TableHelper.CellText(this.decision.Lot, Schemas.LotPriority.ReqSerialNo.Name).Trim();
                judgeOptions.Source = options.Source;
                judgeOptions.SourceFields = options.SourceFields;
                judgeOptions.SourceCaption = options.SourceCaption;
                judgeOptions.ShowDescription = true;
                judgeOptions.DefaultDescription = DefaultDescriptions.ForJob(key);

                JudgeDialogResult result = this.OpenJudgeDialog(judgeOptions);

                if (result != null)
                {
                    this.ProcessJudgedJob(key, options, result.Description, result.Rows);
                }

                return;
            }

            this.OpenJobDialog(key, options);
        }

        private string JobGroupId(DataRow job)
        {
            string eqpGrpId = TableHelper.CellText(job, Schemas.EquipmentPriority.EqpGrpId.Name).Trim();
            return eqpGrpId;
        }

        protected virtual JudgeDialogResult OpenJudgeDialog(JudgeDialogOptions options)
        {
            using (JudgeDialogForm dialog = new JudgeDialogForm(options))
            {
                return dialog.ShowDialog(this) == DialogResult.OK ? dialog.Result : null;
            }
        }

        protected virtual void OpenBackToPrevOperDialog(string key, EquipmentJobDialogOptions options)
        {
            if (!JobDecision.IsLive(options.Target.Lot))
            {
                return;
            }

            using (BackToPrevOperDialogForm dialog = new BackToPrevOperDialogForm(options.Target.Lot))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    options.ProdId = dialog.ProdId;
                    options.FlowId = dialog.FlowId;
                    options.OperId = dialog.OperId;
                    this.ProcessJob(key, options, dialog.Description);
                }
            }
        }

        protected virtual void OpenJobDialog(string key, EquipmentJobDialogOptions options)
        {
            using (EquipmentJobDialogForm dialog = new EquipmentJobDialogForm(options))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    this.ProcessJob(key, options, dialog.Description);
                }
            }
        }

        protected virtual JobSkipDialogResult OpenSkipDialog(DataRow lot)
        {
            if (!JobDecision.IsLive(lot))
            {
                return null;
            }

            using (JobSkipDialogForm dialog = new JobSkipDialogForm(lot, EquipmentLotPresenter.JobSummary(this.decision), JobSummaryFields()))
            {
                return dialog.ShowDialog(this) == DialogResult.OK ? dialog.Result : null;
            }
        }

        private void ProcessSkip(EquipmentJobDialogOptions options, JobSkipDialogResult skip)
        {
            DataRow job = options.Source;
            string key = EquipmentLotPresenter.ActionCancelOper;
            string lotId = TableHelper.CellText(job, Schemas.LotPriority.LotId.Name).Trim();
            string reason = skip != null && skip.LotId == lotId
                            && SameJobTarget(job, EquipmentLotPresenter.JobSummary(this.decision))
                    ? JobActionReason(key, this.decision)
                    : JobTargetChanged;

            if (reason != null)
            {
                this.ShowWarningMessage(reason);
                return;
            }

            if (skip.ReasonCode.Length == 0)
            {
                return;
            }

            JobExecutionTarget target = options.ExecutionTarget;
            string skipCode = skip.ReasonCode;
            string description = skip.Description;

            this.Process(
                    () => this.JobSkip(target, skipCode, description),
                    "Cancelling the operation for " + lotId + "…",
                    () => this.AfterJobAction(lotId));
        }

        private void ProcessJob(string key, EquipmentJobDialogOptions options, string description)
        {
            this.ProcessJudgedJob(key, options, description, null);
        }

        private void ProcessJudgedJob(string key, EquipmentJobDialogOptions options, string description, DataTable judgeResults)
        {
            DataRow job = options.Source;
            string reason = SameJobTarget(job, EquipmentLotPresenter.JobSummary(this.decision))
                    ? JobActionReason(key, this.decision)
                    : JobTargetChanged;

            if (reason != null)
            {
                this.ShowWarningMessage(reason);
                return;
            }

            JobExecutionTarget target = options.ExecutionTarget;
            string lotId = TableHelper.CellText(job, Schemas.LotPriority.LotId.Name).Trim();

            switch (key)
            {
                case EquipmentLotPresenter.ActionJobPrep:
                    this.Process(
                            () => this.JobPrep(target, description),
                            "Preparing job for " + lotId + "…",
                            () => this.AfterJobAction(lotId));
                    break;

                case EquipmentLotPresenter.ActionJobStart:
                    this.Process(
                            () => this.JobStart(target, description),
                            "Starting job " + lotId + "…",
                            () => this.AfterJobAction(lotId));
                    break;

                case EquipmentLotPresenter.ActionJobEnd:
                    if (JudgeKindOf(this.JobGroupId(job)) != JudgeKind.None && judgeResults == null)
                    {
                        this.ShowWarningMessage("Enter the judge results to end this job.");
                        return;
                    }

                    this.Process(
                            () => this.JobEnd(target, description, judgeResults),
                            "Ending job " + lotId + "…",
                            () => this.AfterJobAction(lotId));
                    break;

                case EquipmentLotPresenter.ActionJobPrepCancel:
                    this.Process(
                            () => this.JobPrepCancel(target, description),
                            "Cancelling job preparation for " + lotId + "…",
                            () => this.AfterJobAction(lotId));
                    break;

                case EquipmentLotPresenter.ActionJobStartCancel:
                    this.Process(
                            () => this.JobStartCancel(target, description),
                            "Cancelling job start for " + lotId + "…",
                            () => this.AfterJobAction(lotId));
                    break;

                case EquipmentLotPresenter.ActionBackToPrevOper:
                    this.Process(
                            () => this.JobUndo(target, options.ProdId, options.FlowId, options.OperId, description),
                            "Returning " + lotId + " to the previous operation…",
                            () => this.AfterJobAction(lotId));
                    break;
            }
        }

        private const string JobTargetChanged = "The job target changed while the dialog was open. Check it and try again.";

        private static bool SameJobTarget(DataRow confirmed, DataRow current)
        {
            if (confirmed == null || current == null)
            {
                return false;
            }

            foreach (DataColumn column in confirmed.Table.Columns)
            {
                if (!current.Table.Columns.Contains(column.ColumnName)
                        || TableHelper.CellText(confirmed, column.ColumnName).Trim()
                                != TableHelper.CellText(current, column.ColumnName).Trim())
                {
                    return false;
                }
            }

            return true;
        }

        private bool EquipmentStillAllows(string key, string eqpId)
        {
            DataRow equipment = this.ValidEquipment(
                    EquipmentLotPresenter.FindById(this.equipmentData, Schemas.EquipmentPriority.RowKey, eqpId));
            string reason = equipment == null
                    ? EquipmentPortManagementForm.NoEquipmentSelected
                    : EquipmentPortManagementForm.EquipmentActionReason(key, equipment);

            if (reason == null)
            {
                return true;
            }

            this.ShowWarningMessage(reason);
            return false;
        }

        private void ExecuteEquipmentAction(string key)
        {
            DataRowView selected = this.gridEquipmentPriorities.SelectedItem as DataRowView;
            DataRow equipment = this.ValidEquipment(selected == null ? null : selected.Row);

            if (equipment == null
                    || EquipmentPortManagementForm.EquipmentActionReason(key, equipment) != null)
            {
                return;
            }

            string eqpId = TableHelper.CellText(equipment, Schemas.EquipmentPriority.EqpId.Name).Trim();

            if (key == EquipmentPortManagementPresenter.ActionAuto)
            {
                this.ExecuteAutoToggle(equipment, !EquipmentPortManagementForm.IsAuto(equipment));
                return;
            }

            if (!this.Confirm(
                    "Change " + eqpId + " from " + EquipmentPortManagementPresenter.Mode(equipment) + " to "
                            + Modern.Lab.WinForms.Controls.Dialogs.ModernMessageDialog.Emphasis(key) + "?",
                    "Confirm"))
            {
                return;
            }

            if (!this.EquipmentStillAllows(key, eqpId))
            {
                return;
            }

            DataRow actionEquipment = ActionRows.Copy(equipment);
            this.Process(
                    () => this.SetMode(actionEquipment, key),
                    "Changing mode to " + key + "…",
                    () => this.ExecuteSearch(true));
        }

        private void ExecuteAutoToggle(DataRow equipment, bool turnOn)
        {
            if (this.ValidEquipment(equipment) == null
                    || EquipmentPortManagementForm.EquipmentActionReason(
                            EquipmentPortManagementPresenter.ActionAuto, equipment) != null)
            {
                return;
            }

            string eqpId = TableHelper.CellText(equipment, Schemas.EquipmentPriority.EqpId.Name).Trim();

            if (!this.Confirm(
                    "Turn Auto " + Modern.Lab.WinForms.Controls.Dialogs.ModernMessageDialog.Emphasis(turnOn ? "on" : "off")
                            + " for " + eqpId + "?",
                    "Confirm"))
            {
                return;
            }

            if (!this.EquipmentStillAllows(EquipmentPortManagementPresenter.ActionAuto, eqpId))
            {
                return;
            }

            DataRow actionEquipment = ActionRows.Copy(equipment);
            this.Process(
                    () => this.SetAuto(actionEquipment, turnOn),
                    turnOn ? "Turning Auto on…" : "Turning Auto off…",
                    () => this.ExecuteSearch(true));
        }

        private void OnEqpCellCheckChanged(object sender, GridCheckChangedEventArgs e)
        {
            DataRowView equipment = e.Item as DataRowView;

            if (equipment == null || e.DataPropertyName != Schemas.EquipmentPriority.AutoYn.Name
                    || equipment.Row.RowState == DataRowState.Detached || equipment.Row.RowState == DataRowState.Deleted)
            {
                return;
            }

            equipment.Row[Schemas.EquipmentPriority.AutoYn.Name] = e.IsChecked ? Schemas.EquipmentPriority.AutoYn.N.Value : Schemas.EquipmentPriority.AutoYn.Y.Value;
            this.ExecuteAutoToggle(equipment.Row, e.IsChecked);
        }

        private void Process(Func<DataActionResult> call, string busyText, Action refresh)
        {
            this.RunAction(
                    call,
                    delegate(DataActionResult reply)
                    {
                        this.ShowToast(
                                reply.Message.Length > 0 ? reply.Message : "Done.",
                                ToastKind.Success);
                        refresh();
                    },
                    busyText);
        }

        private void PopulateMenu(
                ContextMenuStrip menu, string headerText,
                IList<EquipmentPortAction> actions, EventHandler onClick)
        {
            menu.Items.Clear();

            if (!string.IsNullOrEmpty(headerText))
            {
                menu.Items.Add(new ToolStripLabel(headerText));
            }

            foreach (EquipmentPortAction action in actions)
            {
                if (action.SeparatorBefore)
                {
                    menu.Items.Add(new ToolStripSeparator());
                }

                ToolStripMenuItem item = MenuGlyphs.CreateItem(action.Label);
                item.Tag = action.Key;
                item.Click += onClick;
                menu.Items.Add(item);
            }
        }

        private void PopulateJobMenu()
        {
            this.menuLot.Items.Clear();
            this.menuLot.Items.Add(new ToolStripLabel("Execute"));

            foreach (LotAction action in EquipmentLotPresenter.JobActions)
            {
                if (action.Key == EquipmentLotPresenter.JobMenuSeparatorBefore)
                {
                    this.menuLot.Items.Add(new ToolStripSeparator());
                }

                Modern.Lab.WinForms.Rendering.ModernMenuItem item = MenuGlyphs.CreateItem(action.Label);
                item.Tag = action.Key;

                if (action.Label.EndsWith(EquipmentLotPresenter.CancelWord, StringComparison.Ordinal))
                {
                    item.AccentText = EquipmentLotPresenter.CancelWord;
                }

                item.Click += this.OnJobMenuItemClick;
                this.menuLot.Items.Add(item);
            }
        }

        private void OnMenuEqpOpening(object sender, CancelEventArgs e)
        {
            DataRowView selected = this.gridEquipmentPriorities.SelectedItem as DataRowView;
            DataRow equipment = this.ValidEquipment(selected == null ? null : selected.Row);

            e.Cancel = !ApplyMenuStates(
                    this.menuEqp, EquipmentPortManagementPresenter.EquipmentActions, equipment, equipment != null);
        }

        private void OnMenuLotOpening(object sender, CancelEventArgs e)
        {
            DataRow lot = LiveRow(this.gridLotPriorities.SelectedItem);

            if (lot != null && !object.ReferenceEquals(lot, this.decision.Lot))
            {
                this.DecideLot(lot);
            }

            foreach (object entry in this.menuLot.Items)
            {
                ToolStripMenuItem item = entry as ToolStripMenuItem;

                if (item != null && EquipmentLotPresenter.IsJobKey(item.Tag as string))
                {
                    ApplyActionReason(item, JobActionReason(item.Tag as string, this.decision));
                }
            }

            e.Cancel = this.decision.Lot == null;
        }

        private static bool ApplyMenuStates(
                ContextMenuStrip menu, IList<EquipmentPortAction> actions,
                DataRow equipment, bool hasTarget)
        {
            if (!hasTarget)
            {
                return false;
            }

            foreach (object entry in menu.Items)
            {
                ToolStripMenuItem item = entry as ToolStripMenuItem;

                if (item == null)
                {
                    continue;
                }

                string key = item.Tag as string;

                foreach (EquipmentPortAction action in actions)
                {
                    if (action.Key == key)
                    {
                        ApplyActionReason(
                                item, EquipmentPortManagementForm.EquipmentActionReason(key, equipment));
                        item.Checked = EquipmentPortManagementPresenter.IsChecked(key, equipment);
                        item.Text = EquipmentPortManagementPresenter.LabelOf(action, equipment, false);
                        break;
                    }
                }
            }

            return true;
        }

        private void OnEquipmentMenuItemClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;

            if (item != null)
            {
                this.ExecuteEquipmentAction(item.Tag as string);
            }
        }

        private void OnJobMenuItemClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;

            if (item != null)
            {
                this.ExecuteJob(item.Tag as string);
            }
        }

        private sealed class RequestSnapshot
        {
            private RequestSnapshot(string reqSerialNo, DataTable master, DataTable details, string remarks)
            {
                this.RequestSerialNo = reqSerialNo;
                this.Master = master;
                this.Details = details;
                this.Remarks = remarks;
            }

            public string RequestSerialNo { get; private set; }

            public DataTable Master { get; private set; }

            public DataTable Details { get; private set; }

            public string Remarks { get; private set; }

            public static RequestSnapshot Create(string reqSerialNo, DataTable master, DataTable details)
            {
                DataTable masterCopy = master == null ? new DataTable("REQUEST") : master.Copy();
                DataTable detailCopy = details == null ? new DataTable("REQUEST_DETAIL") : details.Copy();
                string remarks = string.Empty;

                if (masterCopy.Columns.Contains(Schemas.Request.Purpose.Name))
                {
                    if (masterCopy.Rows.Count > 0)
                    {
                        remarks = TableHelper.CellText(masterCopy.Rows[0], Schemas.Request.Purpose.Name);
                    }

                    masterCopy.Columns.Remove(Schemas.Request.Purpose.Name);
                }

                return new RequestSnapshot(reqSerialNo, masterCopy, detailCopy, remarks);
            }
        }
        private void ClearRequestSamples()
        {
            this.InvalidateChannel(channelRequests);
            this.requestLotId = string.Empty;
            this.reqSerialNo = string.Empty;
            this.requestSnapshot = null;
            this.requestData = null;
            this.requestCurrent = null;
            this.sampleData = null;
            this.sampleCurrent = null;
            this.BindRequestFields(null, string.Empty);
            this.gridSamples.DataSource = null;
            this.UpdateRequestTitle();
        }
        protected override DataRow RuleGuideRow(DataRow selectedRow)
        {
            return EquipmentLotPresenter.RuleRow(this.decision);
        }
    }
}
