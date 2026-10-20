using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using Modern.Lab.Hosting;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Data;

using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.ResponseContracts;

namespace Modern.Lab.Samples
{

    public partial class LogisticsRequestForm : ModernFormBase
    {
        private DataTable resultData;

        private readonly System.Windows.Forms.Timer refreshTimer = new System.Windows.Forms.Timer();

        private int intervalSeconds;

        private int secondsLeft;

        private List<DataRow> viewRows;

        private const string channelLogistics = "board";
        private const string channelRequestWafers = "wafers";

        private bool logisticsSelectionSilent;

        private string logisticsColumnSchema = string.Empty;

        public LogisticsRequestForm()
        {
            this.InitializeComponent();

            this.InitializeModernForm(this.midPanel);
            this.RegisterRefreshShortcut(this.btnRefresh);
            this.boardMenu.Renderer = new Modern.Lab.WinForms.Rendering.ModernMenuRenderer();

            this.DeferredResize = true;

            this.RegisterFindShortcut(this.gridLogistics, this.gridRequestWafers);

            this.badgeSent.ColorValue = LogisticsRequestPresenter.StatusNames[LogisticsRequestPresenter.StatusSent];
            this.badgeReceived.ColorValue = LogisticsRequestPresenter.StatusNames[LogisticsRequestPresenter.StatusReceived];
            this.badgeUnlinked.ColorValue = LogisticsRequestPresenter.StatusNames[LogisticsRequestPresenter.StatusUnlinked];
            this.badgeLinked.ColorValue = LogisticsRequestPresenter.StatusNames[LogisticsRequestPresenter.StatusLinked];
            this.badgeCompleted.ColorValue = LogisticsRequestPresenter.StatusNames[LogisticsRequestPresenter.StatusCompleted];
            this.badgeUnmatched.Semantic = Modern.Lab.Theming.SemanticKind.Error;

            this.refreshTimer.Interval = 1000;
            this.refreshTimer.Tick += this.OnRefreshTick;
            this.Disposed += this.OnFormDisposed;
        }

        private void OnFormDisposed(object sender, EventArgs e)
        {
            this.refreshTimer.Stop();
            this.refreshTimer.Dispose();
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
            this.ExecuteSearch(string.Empty, true, true);
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
            this.lblInterval.Visible = text.Length == 0;
        }

        private HashSet<string> CheckedLotIds()
        {
            HashSet<string> lotIds = new HashSet<string>(StringComparer.Ordinal);

            if (this.resultData == null)
            {
                return lotIds;
            }

            foreach (DataRow row in this.resultData.Rows)
            {
                if (row.RowState != DataRowState.Deleted && row.RowState != DataRowState.Detached
                        && TableHelper.FlagSet(row, LogisticsRequestPresenter.CheckColumn))
                {
                    lotIds.Add(Schemas.Logistics.LotId.Text(row));
                }
            }

            return lotIds;
        }

        private static void RestoreChecks(DataTable logistics, HashSet<string> lotIds)
        {
            if (logistics == null || lotIds.Count == 0 || !logistics.Columns.Contains(LogisticsRequestPresenter.CheckColumn))
            {
                return;
            }

            foreach (DataRow row in logistics.Rows)
            {
                if (lotIds.Contains(Schemas.Logistics.LotId.Text(row)))
                {
                    row[LogisticsRequestPresenter.CheckColumn] = true;
                }
            }
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            this.cboInterval.DisplayMember = RefreshInterval.LabelColumn;
            this.cboInterval.ValueMember = RefreshInterval.SecondsColumn;
            this.cboInterval.DataSource = RefreshInterval.Options();
            this.cboInterval.SelectedValue = 0;
            this.ApplyInterval();

            DataTable statusTable = new DataTable();
            statusTable.Columns.Add("VALUE", typeof(string));
            statusTable.Columns.Add("LABEL", typeof(string));

            foreach (string statusName in LogisticsRequestPresenter.StatusNames)
            {
                statusTable.Rows.Add(statusName, statusName);
            }

            this.cboStatus.DisplayMember = "LABEL";
            this.cboStatus.ValueMember = "VALUE";
            this.cboStatus.PlaceholderText = "All";
            this.cboStatus.DataSource = statusTable;
            this.CheckDefaultStatuses();

            this.badgeSent.Color = LogisticsRequestPresenter.StatusColors[LogisticsRequestPresenter.StatusSent];
            this.badgeReceived.Color = LogisticsRequestPresenter.StatusColors[LogisticsRequestPresenter.StatusReceived];
            this.badgeUnlinked.Color = LogisticsRequestPresenter.StatusColors[LogisticsRequestPresenter.StatusUnlinked];
            this.badgeLinked.Color = LogisticsRequestPresenter.StatusColors[LogisticsRequestPresenter.StatusLinked];
            this.badgeCompleted.Color = LogisticsRequestPresenter.StatusColors[LogisticsRequestPresenter.StatusCompleted];

            this.gridLogistics.RowKeyMember = Schemas.Logistics.RowKey;
            this.gridLogistics.RestoreSelectionByIndex = true;
            this.gridRequestWafers.RowKeyMember = Schemas.RequestWafer.RowKey;

            this.ExecuteSearch();
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            this.ExecuteSearch();
        }

        private void OnResetClick(object sender, EventArgs e)
        {
            this.txtLotId.Text = string.Empty;
            this.txtReqSerialNo.Text = string.Empty;
            this.CheckDefaultStatuses();
            this.ExecuteSearch();
        }

        private void OnSearchEnterPressed(object sender, EventArgs e)
        {
            this.ExecuteSearch();
        }

        private void CheckDefaultStatuses()
        {
            this.cboStatus.CheckedValues = LogisticsRequestPresenter.DefaultStatusNames;
        }

        private List<string> GetStatusFilter()
        {
            return CheckedStrings(this.cboStatus);
        }

        private static List<string> CheckedStrings(
                Modern.Lab.WinForms.Controls.Selection.ModernCheckComboBox combo)
        {
            List<string> values = new List<string>();

            foreach (object value in combo.CheckedValues)
            {
                string text = value == null ? string.Empty : value.ToString();

                if (text.Length > 0)
                {
                    values.Add(text);
                }
            }

            return values;
        }

        private void ExecuteSearch()
        {
            this.ExecuteSearch(string.Empty);
        }

        private async void ExecuteSearch(
                string focusLotId, bool silent = false, bool keepChecks = false)
        {
            HashSet<string> checkedLots = keepChecks ? this.CheckedLotIds() : new HashSet<string>(StringComparer.Ordinal);
            string keyword = this.txtLotId.Text.Trim();
            string reqSerialNo = this.txtReqSerialNo.Text.Trim();
            List<string> statusFilter = this.GetStatusFilter();

            IDisposable busy = silent
                    ? null
                    : this.Busy("Searching requests...", "Checking transfer status");

            this.InvalidateChannel(channelRequestWafers);

            LoadOutcome<DataTable> outcome;

            try
            {
                outcome = await this.FetchAsync(
                        channelLogistics,
                        () => this.GetLogistics(keyword, reqSerialNo, statusFilter),
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

            if (outcome.Failure != null)
            {
                return;
            }

            DataTable logistics = outcome.Value ?? new DataTable();

            DateTime daysBasis = DateTime.Now;

            LogisticsRequestPresenter.ApplyWorkflowColumns(logistics, ApplyActionRules);
            RestoreChecks(logistics, checkedLots);

            if (this.resultData != null)
            {
                this.resultData.ColumnChanged -= this.OnResultColumnChanged;
            }

            this.resultData = logistics;
            this.resultData.ColumnChanged += this.OnResultColumnChanged;

            this.gridLogistics.FilterValueSource = this.resultData;
            this.RefreshViewRows();

            this.boardCard.TitleRightText =
                    "Days as of " + daysBasis.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            this.BindLogistics(silent);
            this.RefreshSummary();

            if (!string.IsNullOrEmpty(focusLotId))
            {
                this.FocusLogisticsRow(focusLotId);
            }
        }

        private void BindLogistics(bool silent = false)
        {
            if (this.resultData == null || this.viewRows == null)
            {
                this.gridLogistics.DataSource = null;
                this.gridLogistics.StatusText = string.Empty;
                return;
            }

            DataTable view = this.resultData.Clone();

            for (int index = 0; index < this.viewRows.Count; index++)
            {
                view.ImportRow(this.viewRows[index]);
            }

            view.ColumnChanged += this.OnLogisticsColumnChanged;

            this.logisticsSelectionSilent = silent;
            try
            {
                this.ConfigureLogisticsColumns(view);
                this.gridLogistics.DataSource = view;
            }
            finally
            {
                this.logisticsSelectionSilent = false;
            }
            this.UpdateLogisticsStatus();
        }

        private void ConfigureLogisticsColumns(DataTable view)
        {
            string schema = string.Join(",", TableJudgment.ColumnNames(view).ToArray());

            if (schema == this.logisticsColumnSchema)
            {
                return;
            }

            this.logisticsColumnSchema = schema;
            this.gridLogistics.ConfigureColumns(LogisticsColumns(view));
        }

        private void UpdateLogisticsStatus()
        {
            if (this.resultData == null || this.viewRows == null)
            {
                this.gridLogistics.StatusText = string.Empty;
                return;
            }

            int total = this.resultData.Rows.Count;
            int shown = this.viewRows.Count;
            int checkedCount = 0;

            foreach (DataRow row in this.resultData.Rows)
            {
                if (TableHelper.FlagSet(row, LogisticsRequestPresenter.CheckColumn))
                {
                    checkedCount = checkedCount + 1;
                }
            }

            string text = string.Empty;

            if (shown < total)
            {
                text = "filtered from " + total.ToString("N0", CultureInfo.InvariantCulture);
            }

            if (checkedCount > 0)
            {
                text = text.Length > 0
                        ? text + "  ·  " + checkedCount.ToString("N0", CultureInfo.InvariantCulture)
                                + " selected"
                        : checkedCount.ToString("N0", CultureInfo.InvariantCulture) + " selected";
            }

            this.gridLogistics.StatusText = text;
        }

        private void RefreshViewRows()
        {
            this.viewRows = new List<DataRow>();

            if (this.resultData == null)
            {
                return;
            }

            foreach (DataRow row in this.resultData.Rows)
            {
                if (this.gridLogistics.MatchesColumnFilters(row))
                {
                    this.viewRows.Add(row);
                }
            }
        }

        private void FocusLogisticsRow(string lotId)
        {
            if (this.viewRows == null)
            {
                return;
            }

            for (int index = 0; index < this.viewRows.Count; index++)
            {
                if (string.Equals(
                        Schemas.Logistics.LotId.Text(this.viewRows[index]),
                        lotId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    this.gridLogistics.SelectedIndex = index;
                    return;
                }
            }
        }

        private void OnLogisticsColumnFiltersChanged(object sender, EventArgs e)
        {
            this.RefreshViewRows();

            this.BindLogistics();
        }

        private void OnLogisticsColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Column.ColumnName != LogisticsRequestPresenter.CheckColumn)
            {
                return;
            }

            DataRow source = this.FindResultRow(Schemas.Logistics.LotId.Text(e.Row));

            if (source != null)
            {
                source[LogisticsRequestPresenter.CheckColumn] = e.ProposedValue;
            }
        }

        private DataRow FindResultRow(string lotId)
        {
            if (this.resultData == null || lotId.Length == 0)
            {
                return null;
            }

            foreach (DataRow row in this.resultData.Rows)
            {
                if (Schemas.Logistics.LotId.Text(row) == lotId)
                {
                    return row;
                }
            }

            return null;
        }

        private void RefreshSummary()
        {
            LogisticsRequestPresenter.LogisticsSummary summary =
                    LogisticsRequestPresenter.Aggregate(this.resultData);

            this.badgeSent.Text = "Sent "
                    + summary.StatusCounts[LogisticsRequestPresenter.StatusSent].ToString("N0");
            this.badgeReceived.Text = "Received "
                    + summary.StatusCounts[LogisticsRequestPresenter.StatusReceived].ToString("N0");
            this.badgeUnlinked.Text = "Unlinked "
                    + summary.StatusCounts[LogisticsRequestPresenter.StatusUnlinked].ToString("N0");
            this.badgeLinked.Text = "Linked "
                    + summary.StatusCounts[LogisticsRequestPresenter.StatusLinked].ToString("N0");
            this.badgeCompleted.Text = "Completed "
                    + summary.StatusCounts[LogisticsRequestPresenter.StatusCompleted].ToString("N0");
            this.badgeUnmatched.Text = "Unmatched " + summary.UnmatchedCount.ToString("N0");

            this.badgeAvg.Text = summary.UnlinkedCount > 0
                    ? "Avg " + summary.DaysAverage.ToString("0.0", CultureInfo.InvariantCulture) + " d"
                    : "Avg -";
            this.badgeOldest.Text = summary.UnlinkedCount > 0
                    ? "Oldest " + summary.DaysMax.ToString("N0") + " d"
                    : "Oldest -";

            this.UpdateActionButtons();
        }

        private void UpdateActionButtons()
        {
            int receiveCount = 0;
            int createCount = 0;

            if (this.resultData != null)
            {
                foreach (DataRow row in this.resultData.Rows)
                {
                    if (!TableHelper.FlagSet(row, LogisticsRequestPresenter.CheckColumn))
                    {
                        continue;
                    }

                    if (TableHelper.FlagSet(row, LogisticsRequestPresenter.ReceiveCanColumn))
                    {
                        receiveCount = receiveCount + 1;
                    }

                    if (TableHelper.FlagSet(row, LogisticsRequestPresenter.CreateCanColumn))
                    {
                        createCount = createCount + 1;
                    }
                }
            }

            this.btnReceive.Enabled = receiveCount > 0;
            this.btnReceive.BadgeCount = receiveCount;
            this.btnCreate.Enabled = createCount > 0;
            this.btnCreate.BadgeCount = createCount;
            this.btnManualReceive.Enabled = true;
        }

        private void OnResultColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Column != null && e.Column.ColumnName == LogisticsRequestPresenter.CheckColumn)
            {
                this.Debounce("logisticsActions", 50, new MethodInvoker(this.UpdateActionButtons));
            }
        }

        private void OnLogisticsSelectionChanged(object sender, EventArgs e)
        {
            DataRowView row = this.gridLogistics.SelectedItem as DataRowView;

            if (row == null)
            {
                this.stepPipeline.DataSource = null;
                this.gridRequestWafers.DataSource = null;
                this.waferCard.Text = "Wafers";
                return;
            }

            this.UpdatePipeline(row.Row);

            string lotId = Schemas.Logistics.LotId.Text(row.Row);

            if (string.IsNullOrEmpty(lotId))
            {
                return;
            }

            this.LoadRequestWafers(lotId, this.logisticsSelectionSilent);
        }

        private void UpdatePipeline(DataRow row)
        {
            int status = Array.IndexOf(
                    LogisticsRequestPresenter.StatusNames, TableHelper.CellText(row, LogisticsRequestPresenter.StatusColumn));

            if (status < 0)
            {
                this.stepPipeline.DataSource = null;
                return;
            }

            bool notified = LogisticsRequestPresenter.IsNotified(row);

            DataTable steps = new DataTable();
            steps.Columns.Add("LABEL", typeof(string));
            steps.Columns.Add("STATE", typeof(string));

            for (int index = 0; index < LogisticsRequestPresenter.StatusNames.Length; index++)
            {
                if (index == LogisticsRequestPresenter.StatusSent && !notified)
                {
                    steps.Rows.Add(LogisticsRequestPresenter.StatusNames[index], "Pending");
                    continue;
                }

                string state;

                if (index < status)
                {
                    state = "Completed";
                }
                else if (index == status)
                {
                    state = "Current";
                }
                else
                {
                    state = "Pending";
                }

                steps.Rows.Add(
                        LogisticsRequestPresenter.StatusNames[index].Replace(' ', '\n'), state);
            }

            this.stepPipeline.DataSource = steps;
        }

        private async void LoadRequestWafers(string lotId, bool silent)
        {
            LoadOutcome<DataTable> outcome = await this.FetchAsync(
                    channelRequestWafers, () => this.GetRequestWafers(lotId), silent);

            if (!outcome.IsCurrent)
            {
                return;
            }

            if (outcome.Failure != null)
            {
                this.gridRequestWafers.DataSource = null;
                this.waferCard.Text = "Wafers";
                return;
            }

            RequestWaferColumns(outcome.Value).Bind(this.gridRequestWafers);
            this.waferCard.Text = "Wafers — " + lotId;
        }

        private DataTable BuildCheckedList(string actionFlag)
        {
            DataTable list = this.resultData != null ? this.resultData.Clone() : new DataTable();

            if (this.resultData != null)
            {
                foreach (DataRow row in this.resultData.Rows)
                {
                    if (TableHelper.FlagSet(row, LogisticsRequestPresenter.CheckColumn)
                            && TableHelper.FlagSet(row, actionFlag))
                    {
                        list.ImportRow(row);
                    }
                }
            }

            return list;
        }

        private bool StillAllows(DataTable checkedList, string step)
        {
            foreach (DataRow checkedRow in checkedList.Rows)
            {
                string lotId = Schemas.Logistics.LotId.Text(checkedRow);
                DataRow current = this.CurrentLogisticsRow(lotId);
                string reason = current == null ? "The lot is no longer in the list." : LogisticsActionReason(current, step);

                if (reason != null)
                {
                    this.ShowWarningMessage(lotId + ": " + reason);
                    return false;
                }
            }

            return true;
        }

        private DataRow CurrentLogisticsRow(string lotId)
        {
            if (this.resultData == null || lotId.Length == 0)
            {
                return null;
            }

            foreach (DataRow row in this.resultData.Rows)
            {
                if (row.RowState != DataRowState.Deleted && row.RowState != DataRowState.Detached
                        && Schemas.Logistics.LotId.Text(row) == lotId)
                {
                    return row;
                }
            }

            return null;
        }

        private void OnReceiveClick(object sender, EventArgs e)
        {
            DataTable receiveList = this.BuildCheckedList(LogisticsRequestPresenter.ReceiveCanColumn);

            if (receiveList.Rows.Count == 0)
            {
                this.ShowWarningMessage("Check lots to receive first.");
                return;
            }

            using (LogisticsReceiveDialogForm dialog = new LogisticsReceiveDialogForm(receiveList))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK || !this.StillAllows(receiveList, "Receive"))
                {
                    return;
                }

                this.ProcessLogisticsLots(CollectLotRows(receiveList), "Receive", dialog.Description);
            }
        }

        private void OnManualReceiveClick(object sender, EventArgs e)
        {
            using (LogisticsManualReceiveDialogForm dialog = new LogisticsManualReceiveDialogForm())
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                this.ShowToast(
                        dialog.ReceivedMessage.Length > 0
                                ? dialog.ReceivedMessage
                                : "Lot " + dialog.ReceivedLotId + " received.",
                        ToastKind.Success);
                this.ExecuteSearch(dialog.ReceivedLotId);
            }
        }

        private void OnCreateClick(object sender, EventArgs e)
        {
            DataTable createList = this.BuildCheckedList(LogisticsRequestPresenter.CreateCanColumn);

            if (createList.Rows.Count == 0)
            {
                this.ShowWarningMessage("Check lots to create first.");
                return;
            }

            using (LogisticsCreateDialogForm dialog = new LogisticsCreateDialogForm(createList))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK || !this.StillAllows(createList, "Create"))
                {
                    return;
                }

                this.ProcessLogisticsLots(CollectLotRows(createList), "Create", dialog.Description);
            }
        }

        private void OnGridCellButtonClick(object sender, GridButtonClickEventArgs e)
        {
            if (e.DataPropertyName != LogisticsRequestPresenter.ActionColumn)
            {
                return;
            }

            DataRowView row = e.Item as DataRowView;

            if (row == null)
            {
                return;
            }

            string step = LogisticsRequestPresenter.NextStep(row.Row);

            if (step.Length == 0 || !CanProcessRow(row.Row, step))
            {
                return;
            }

            List<DataRow> single = new List<DataRow> { ActionRows.Copy(row.Row) };

            this.ProcessLogisticsLots(
                    single, step,
                    step == "Receive" ? DefaultDescriptions.ReceiveLogistics : DefaultDescriptions.CreateLogistics);
        }

        protected override void ShowRequestInfoDialog(DataTable requestInfo)
        {
            using (RequestInfoDialogForm dialog = new RequestInfoDialogForm())
            {
                dialog.SetRequest(requestInfo, RequestSampleFirstColumn, RequestPurposeColumn);
                dialog.ShowDialog(this);
            }
        }

        private void OnLogisticsMenuOpening(object sender, CancelEventArgs e)
        {
            DataRowView row = this.gridLogistics.SelectedItem as DataRowView;

            if (row == null)
            {
                e.Cancel = true;
                return;
            }

            ApplyActionReason(this.miReceive, LogisticsActionReason(row.Row, "Receive"));
            ApplyActionReason(this.miCreate, LogisticsActionReason(row.Row, "Create"));
        }

        private void OnMenuReceiveClick(object sender, EventArgs e)
        {
            this.ProcessSelectedLogisticsRow("Receive");
        }

        private void OnMenuCreateClick(object sender, EventArgs e)
        {
            this.ProcessSelectedLogisticsRow("Create");
        }

        private void ProcessSelectedLogisticsRow(string step)
        {
            DataRowView row = this.gridLogistics.SelectedItem as DataRowView;

            if (row == null || !CanProcessRow(row.Row, step))
            {
                return;
            }

            List<DataRow> single = new List<DataRow> { ActionRows.Copy(row.Row) };

            this.ProcessLogisticsLots(
                    single, step,
                    step == "Receive" ? DefaultDescriptions.ReceiveLogistics : DefaultDescriptions.CreateLogistics);
        }

        private void ProcessLogisticsLots(List<DataRow> lots, string step, string description)
        {
            if (lots == null || this.ActionInProgress)
            {
                return;
            }
            List<DataRow> keys = lots.FindAll(row => Schemas.Logistics.LotId.Text(row).Length > 0);
            if (keys.Count == 0)
            {
                return;
            }
            string lastServerMessage = string.Empty;

            this.RunActionBatch(
                    keys,
                    delegate(DataRow lot)
                    {
                        return step == "Receive" ? this.Receive(lot, description) : this.Create(lot, description);
                    },
                    delegate(BatchProgress progress)
                    {
                        if (progress.Total > 1)
                        {
                            this.gridLogistics.StatusText = "processing " + progress.Text;
                        }

                        if (progress.Last != null && progress.Last.Success
                                && progress.Last.Message.Length > 0)
                        {
                            lastServerMessage = progress.Last.Message;
                        }
                    },
                    delegate(BatchOutcome outcome)
                    {
                        if (outcome.Succeeded == 0)
                        {
                            string reason = outcome.FirstFailure == null
                                            || outcome.FirstFailure.Message.Length == 0
                                    ? "Nothing was processed."
                                    : outcome.FirstFailure.Message;

                            this.ShowErrorMessage(step + " failed", reason);
                            this.UpdateLogisticsStatus();
                            return;
                        }

                        string doneMessage = outcome.Total == 1 && lastServerMessage.Length > 0
                                ? lastServerMessage
                                : outcome.Succeeded.ToString("N0") + " lot(s) "
                                        + DoneWord(step)
                                        + (outcome.Failed > 0
                                                ? " " + outcome.Failed.ToString("N0")
                                                        + " failed."
                                                : string.Empty);

                        if (outcome.Failed > 0)
                        {
                            this.ShowWarningMessage(doneMessage);
                        }
                        else
                        {
                            this.ShowToast(doneMessage, ToastKind.Success);
                        }

                        this.ExecuteSearch(string.Empty, true);
                    },
                    step + "ing…");
        }

        private static string DoneWord(string step)
        {
            return step == "Receive" ? "received." : "created.";
        }

        private static List<DataRow> CollectLotRows(DataTable checkedList)
        {
            List<DataRow> lots = new List<DataRow>();

            foreach (DataRow row in checkedList.Rows)
            {
                string lotId = Schemas.Logistics.LotId.Text(row);

                if (lotId.Length > 0)
                {
                    lots.Add(ActionRows.Copy(row));
                }
            }

            return lots;
        }
    }
}
