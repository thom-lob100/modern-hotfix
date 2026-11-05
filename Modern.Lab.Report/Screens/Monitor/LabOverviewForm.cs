using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Hosting;
using Modern.Lab.Report.Services;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Display;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.Report
{
    public partial class LabOverviewForm : ModernFormBase
    {
        private const string channelOverview = "overview";
        private const string emptyAttention = "Nothing needs attention";
        private const string pendingAttention = "Not loaded";
        private const string selectedHint = "Click a stage on the map or an attention row";
        private const int attentionMinWidth = 220;
        private const double attentionShare = 0.23d;

        private readonly Timer refreshTimer = new Timer();

        private DataTable nodeLayout;
        private DataTable linkLayout;
        private LabOverviewPresenter.Snapshot snapshot;
        private DateTime lastSuccess = DateTime.MinValue;
        private int intervalSeconds;
        private int secondsLeft;
        private string selectedTarget = string.Empty;
        private string selectedKey = string.Empty;
        private string selectedShape = string.Empty;
        private bool suppressListEvent;
        private bool ready;

        public LabOverviewForm()
        {
            this.InitializeComponent();
            this.PaintCards();
            this.InitializeModernForm();
            this.RegisterRefreshShortcut(this.btnRefresh);
            this.DeferredResize = true;
            this.refreshTimer.Interval = 1000;
            this.refreshTimer.Tick += this.OnRefreshTick;
            this.bodyLayout.SizeChanged += this.OnBodySizeChanged;
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            if (IsDesignTime() || this.DesignMode)
            {
                return;
            }

            this.ConfigureMap();
            this.ConfigureAttention();
            this.nodeLayout = NodeLayout();
            this.linkLayout = LinkLayout();
            this.ShowPending();
            this.FitAttentionWidth();

            this.cboInterval.DisplayMember = RefreshInterval.LabelColumn;
            this.cboInterval.ValueMember = RefreshInterval.SecondsColumn;
            this.cboInterval.DataSource = RefreshInterval.Options(LabMonitorPresenter.IntervalChoices);
            this.cboInterval.SelectedValue = LabMonitorPresenter.DefaultIntervalSeconds;

            this.ready = true;
            this.ApplyInterval();
            this.ExecuteRefresh(false);
        }

        private void PaintCards()
        {
            foreach (Panel card in new Panel[] { this.mapCard, this.attentionCard, this.storageCard, this.selectedCard })
            {
                card.BackColor = Modern.Lab.Theming.ModernTheme.Surface;
            }
        }

        private void ConfigureAttention()
        {
            this.listAttention.KeyMember = LabMonitorPresenter.RowKeyColumn;
            this.listAttention.TitleMember = LabMonitorPresenter.TitleColumn;
            this.listAttention.SubtitleMember = LabMonitorPresenter.SubtitleColumn;
            this.listAttention.QtyMember = LabMonitorPresenter.QtyTextColumn;
            this.listAttention.BadgeTextMember = LabMonitorPresenter.BadgeTextColumn;
            this.listAttention.BadgeKindMember = LabMonitorPresenter.BadgeKindColumn;
            this.listAttention.EdgeKindMember = LabMonitorPresenter.EdgeKindColumn;
            this.listAttention.LockMember = LabMonitorPresenter.LockColumn;
            this.listAttention.TrailingMember = LabMonitorPresenter.TrailingColumn;
            this.listAttention.ToolTipMember = LabMonitorPresenter.ToolTipColumn;
            this.listAttention.TagMember = LabOverviewPresenter.KindTagColumn;
            this.listAttention.RowClick += this.OnAttentionRowClick;
            this.listAttention.SelectionChanged += this.OnAttentionSelectionChanged;
        }

        private void ShowPending()
        {
            this.mapProcess.SetMapSource(LabOverviewPresenter.PendingNodes(this.nodeLayout), this.linkLayout, LabOverviewPresenter.NewCellTable());
            this.listAttention.EmptyText = pendingAttention;
            this.BindAttention(null);
            this.lblAttentionCount.Text = "-";
            this.kpiStb.Value = "-";
            this.kpiStored.Value = "-";
            this.kpiStored.BottomLabel = string.Empty;
            this.kpiTrays.Value = "-";
            this.kpiTrays.BottomLabel = string.Empty;
            this.ShowSelected();
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            this.secondsLeft = this.intervalSeconds;
            this.UpdateCountdown();
            this.ExecuteRefresh(false);
        }

        private void OnRefreshTick(object sender, EventArgs e)
        {
            if (this.intervalSeconds <= 0 || !this.Visible)
            {
                this.refreshTimer.Stop();
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

        private void ApplyInterval()
        {
            this.intervalSeconds = RefreshInterval.Seconds(this.cboInterval.SelectedValue);
            this.secondsLeft = this.intervalSeconds;
            this.refreshTimer.Enabled = this.intervalSeconds > 0 && this.Visible;
            this.UpdateCountdown();
        }

        private void UpdateCountdown()
        {
            string text = RefreshInterval.CountdownText(this.secondsLeft, this.intervalSeconds);
            this.badgeCountdown.Text = text.Length == 0 ? "-" : text;
            this.badgeCountdown.Visible = text.Length > 0;
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (!this.ready)
            {
                return;
            }

            if (!this.Visible)
            {
                this.refreshTimer.Stop();
                return;
            }

            this.ResumeAfterShown();
        }

        private void ResumeAfterShown()
        {
            if (this.intervalSeconds <= 0)
            {
                this.UpdateCountdown();
                return;
            }

            double elapsed = this.lastSuccess == DateTime.MinValue ? double.MaxValue : (DateTime.Now - this.lastSuccess).TotalSeconds;

            if (elapsed >= this.intervalSeconds)
            {
                this.secondsLeft = this.intervalSeconds;
                this.UpdateCountdown();
                this.refreshTimer.Start();

                if (!this.QueryInProgress)
                {
                    this.ExecuteRefresh(true);
                }

                return;
            }

            this.secondsLeft = Math.Max(1, this.intervalSeconds - (int)elapsed);
            this.UpdateCountdown();
            this.refreshTimer.Start();
        }

        private async void ExecuteRefresh(bool silent)
        {
            IDisposable busy = silent ? null : this.Busy("Loading overview...", string.Empty);
            DataTable nodes = this.nodeLayout.Copy();
            DataTable links = this.linkLayout.Copy();
            LoadOutcome<LabOverviewPresenter.Snapshot> outcome;

            try
            {
                outcome = await this.FetchAsync(
                        channelOverview,
                        () => LabOverviewPresenter.Build(
                                this.GetMonitorLots(),
                                this.GetMonitorEquipments(),
                                this.GetMonitorDurables(),
                                nodes,
                                links,
                                DateTime.Now),
                        silent);
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

            if (outcome.Failure != null || outcome.Value == null)
            {
                this.badgeFailed.Text = "Refresh failed " + DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
                this.badgeFailed.Visible = true;
                return;
            }

            this.Apply(outcome.Value);
        }

        private void Apply(LabOverviewPresenter.Snapshot value)
        {
            this.snapshot = value;
            this.lastSuccess = value.At;
            this.mapProcess.SetMapSource(value.Nodes, value.Links, value.Cells);
            this.listAttention.EmptyText = emptyAttention;
            this.BindAttention(value.Attention);

            int shown = value.Attention.Rows.Count;
            this.lblAttentionCount.Text = value.AttentionTotal > shown
                    ? Count(shown) + " of " + Count(value.AttentionTotal)
                    : Count(value.AttentionTotal);

            this.kpiStb.Value = Count(value.Storage.StbFoups);
            this.kpiStored.Value = Count(value.Storage.StoredFoups);
            this.kpiStored.BottomLabel = Count(value.Storage.MixedFoups) + " mixed";
            this.kpiTrays.Value = Count(value.Storage.StockerTrays);
            this.kpiTrays.BottomLabel = Count(value.Storage.FreeTrays) + " free";

            this.lblUpdated.Text = "Updated " + value.At.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            this.badgeFailed.Visible = false;
            this.RestoreSelection();
        }

        private void RestoreSelection()
        {
            if (this.selectedTarget == LabOverviewPresenter.TargetAttention)
            {
                this.mapProcess.SelectedKey = string.Empty;

                if (!LabOverviewPresenter.ContainsAttention(this.snapshot, this.selectedKey))
                {
                    this.selectedTarget = string.Empty;
                    this.selectedKey = string.Empty;
                    this.ClearAttentionSelection();
                }
            }
            else if (this.selectedTarget == LabOverviewPresenter.TargetNode)
            {
                this.ClearAttentionSelection();
                this.mapProcess.SelectedKey = this.selectedKey;
            }
            else
            {
                this.ClearAttentionSelection();
                this.mapProcess.SelectedKey = string.Empty;
            }

            this.ShowSelected();
        }

        private void OnMapNodeClick(object sender, ProcessMapNodeEventArgs e)
        {
            this.selectedTarget = LabOverviewPresenter.TargetNode;
            this.selectedKey = e.Key;
            this.mapProcess.SelectedKey = e.Key;
            this.ClearAttentionSelection();
            this.ShowSelected();
        }

        private void OnAttentionRowClick(object sender, QueueRowEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Key))
            {
                return;
            }

            this.SelectAttention(e.Key);
        }

        private void OnAttentionSelectionChanged(object sender, EventArgs e)
        {
            if (this.suppressListEvent)
            {
                return;
            }

            string key = this.listAttention.SelectedKey;

            if (string.IsNullOrEmpty(key) || (this.selectedTarget == LabOverviewPresenter.TargetAttention && key == this.selectedKey))
            {
                return;
            }

            this.SelectAttention(key);
        }

        private void SelectAttention(string key)
        {
            this.selectedTarget = LabOverviewPresenter.TargetAttention;
            this.selectedKey = key;
            this.mapProcess.SelectedKey = string.Empty;
            this.ShowSelected();
        }

        private void BindAttention(DataTable rows)
        {
            this.suppressListEvent = true;

            try
            {
                this.listAttention.DataSource = rows;
            }
            finally
            {
                this.suppressListEvent = false;
            }
        }

        private void ClearAttentionSelection()
        {
            this.suppressListEvent = true;

            try
            {
                this.listAttention.SelectKey(string.Empty);
            }
            finally
            {
                this.suppressListEvent = false;
            }
        }

        private void ShowSelected()
        {
            LabOverviewPresenter.SelectedView view = LabOverviewPresenter.Selected(this.snapshot, this.selectedTarget, this.selectedKey);
            List<ModernFieldDefinition> fields = new List<ModernFieldDefinition>();
            List<string> values = new List<string>();

            if (view == null)
            {
                this.lblSelectedTitle.Text = "Selected";
                fields.Add(new ModernFieldDefinition("Select", "F1", 2));
                values.Add(selectedHint);
            }
            else
            {
                this.lblSelectedTitle.Text = "Selected · " + view.Title;

                for (int index = 0; index < view.Lines.Count; index++)
                {
                    fields.Add(new ModernFieldDefinition(view.Lines[index].Caption, "F" + Count(index + 1), view.Lines[index].Span));
                    values.Add(view.Lines[index].Value);
                }
            }

            List<string> shape = new List<string>();

            foreach (ModernFieldDefinition field in fields)
            {
                shape.Add(field.Caption + ":" + Count(field.ColumnSpan));
            }

            string shapeText = string.Join("|", shape.ToArray());

            if (shapeText != this.selectedShape)
            {
                this.fieldSelected.DefineFields(fields.ToArray());
                this.selectedShape = shapeText;
            }

            for (int index = 0; index < values.Count; index++)
            {
                this.fieldSelected.SetValue("F" + Count(index + 1), values[index]);
            }
        }

        private void OnBodySizeChanged(object sender, EventArgs e)
        {
            this.FitAttentionWidth();
        }

        private void FitAttentionWidth()
        {
            if (this.bodyLayout.ColumnStyles.Count < 2 || this.bodyLayout.Width <= 0)
            {
                return;
            }

            int minimum = (int)Math.Round(attentionMinWidth * ScreenDpi.Of(this) / 96d);
            int width = Math.Max(minimum, (int)Math.Round(this.bodyLayout.Width * attentionShare));

            if (Math.Abs(this.bodyLayout.ColumnStyles[1].Width - width) > 0.5f)
            {
                this.bodyLayout.ColumnStyles[1].Width = width;
            }
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
