using System;
using System.Data;
using Modern.Lab.Data;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.ResponseContracts;
using Modern.Lab.Samples.Contracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.Theming;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Samples
{

    public partial class RequestManagementForm : ModernFormBase
    {
        private const string channelRequests = "requests";
        private const string channelSamples = "requestSamples";

        private static readonly ResponseContractSet Contracts = RequestContracts.Build();

        private DataTable requestData;
        private TableResponse requestCurrent;
        private int requestsInFlight;
        private bool gridBinding;
        private string sampleRequestKey = string.Empty;
        private string searchKey = string.Empty;
        private string nextStateCode = string.Empty;
        private string actionState = string.Empty;
        private bool submitting;

        public RequestManagementForm()
        {
            this.InitializeComponent();

            this.InitializeModernForm(this.mainZone);
            this.RegisterRefreshShortcut(this.btnRefresh);

            this.DeferredResize = true;

            this.RegisterFindShortcut(this.gridRequests, this.gridSamples);
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            this.gridRequests.RowKeyMember = Schemas.Request.RowKey;
            this.gridSamples.RowKeyMember = Schemas.Sample.RowKey;

            this.cboState.DisplayMember = RequestManagementPresenter.OptionLabelColumn;
            this.cboState.ValueMember = RequestManagementPresenter.OptionValueColumn;
            this.cboState.DataSource = RequestManagementPresenter.StateOptions();

            this.ClearStatus();
            this.ApplyDefaultRequestRange();
            this.UpdateSearchState();
            this.ExecuteSearch(false);
        }

        private void ApplyDefaultRequestRange()
        {
            DateTime today = DateTime.Today;
            this.dtpRequest.SetRange(RequestManagementPresenter.DefaultRequestFrom(today), today);
        }

        private void OnRequestRangeChanged(object sender, EventArgs e)
        {
            this.UpdateSearchState();
        }

        private bool RequestRangeComplete
        {
            get { return this.dtpRequest.FromDate.HasValue && this.dtpRequest.ToDate.HasValue; }
        }

        private void UpdateSearchState()
        {
            this.btnRefresh.Enabled = this.RequestRangeComplete;
            this.UpdateActionState();
        }

        private void UpdateActionState()
        {
            this.ApplyActionReason(this.btnNextStep, NextStepReason(this.actionState, this.submitting, this.RequestRangeComplete));
            this.ApplyActionReason(this.btnReject, RejectReason(this.actionState, this.submitting, this.RequestRangeComplete));
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            this.ExecuteSearch(false);
        }

        private void OnResetClick(object sender, EventArgs e)
        {
            if (this.submitting)
            {
                return;
            }

            this.txtReqSerialNo.Text = string.Empty;
            this.txtRequester.Text = string.Empty;
            this.cboState.UncheckAll();
            this.ApplyDefaultRequestRange();
            this.dtpAccept.SetRange(null, null);
            this.ExecuteSearch(false);
        }

        private SearchConditions ReadConditions()
        {
            SearchConditions conditions = new SearchConditions();
            conditions.ReqSerialNo = this.txtReqSerialNo.Text.Trim();
            conditions.Requester = this.txtRequester.Text.Trim();
            conditions.StateCodes = RequestManagementPresenter.StateFilter(this.cboState.CheckedValues);
            conditions.ReqFromDt = RequestManagementPresenter.DateText(this.dtpRequest.FromDate);
            conditions.ReqToDt = RequestManagementPresenter.DateText(this.dtpRequest.ToDate);
            conditions.AcceptFromDt = RequestManagementPresenter.DateText(this.dtpAccept.FromDate);
            conditions.AcceptToDt = RequestManagementPresenter.DateText(this.dtpAccept.ToDate);

            return conditions;
        }

        private async void ExecuteSearch(bool silent)
        {
            if (this.submitting || !this.RequestRangeComplete)
            {
                return;
            }

            SearchConditions conditions = this.ReadConditions();
            string key = conditions.Key;
            string keepKey = key == this.searchKey ? this.SelectedRequestId() : string.Empty;

            this.InvalidateChannel(channelSamples);
            this.requestsInFlight++;

            IDisposable busy = silent
                    ? null
                    : this.Busy(
                            "Loading requests...",
                            RequestManagementPresenter.FilteredRequestsText);

            LoadOutcome<DataTable> outcome;

            try
            {
                outcome = await this.FetchAsync(channelRequests, () => this.GetRequests(
                        conditions.ReqSerialNo, conditions.Requester, conditions.StateCodes,
                        conditions.ReqFromDt, conditions.ReqToDt, conditions.AcceptFromDt, conditions.AcceptToDt), silent);
            }
            finally
            {
                this.requestsInFlight--;

                if (busy != null)
                {
                    busy.Dispose();
                }
            }

            if (!outcome.IsCurrent)
            {
                return;
            }

            this.searchKey = key;
            this.BindRequests(outcome.Failure != null ? null : outcome.Value, keepKey, silent);
        }

        private void BindRequests(DataTable requests, string keepKey, bool silent)
        {
            this.requestData = this.BindJudged(
                    RequestContracts.RequestTable, ref this.requestCurrent, requests, silent);

            this.gridBinding = true;

            try
            {
                if (this.requestData == null)
                {
                    this.gridRequests.DataSource = null;
                }
                else
                {
                    bool judged = this.requestCurrent != null;
                    this.gridRequests.RowKeyMember = judged ? Schemas.Request.RowKey : string.Empty;

                    GridColumns columns = judged
                            ? RequestColumns(this.requestData)
                            : GridColumns.Of(this.requestData);

                    columns.Apply(this.gridRequests);

                    this.gridRequests.DataSource = this.requestData;

                    this.SelectRequest(keepKey);
                }
            }
            finally
            {
                this.gridBinding = false;
            }

            this.ApplySelection(silent);
        }

        private void SelectRequest(string keepKey)
        {
            if (this.requestData == null || this.requestData.Rows.Count == 0)
            {
                this.gridRequests.SelectedIndex = -1;
                return;
            }

            if (keepKey.Length > 0)
            {
                foreach (DataRowView row in this.requestData.DefaultView)
                {
                    if (RequestId(row.Row) == keepKey)
                    {
                        this.gridRequests.SelectedItem = row;
                        return;
                    }
                }

                this.gridRequests.SelectedIndex = -1;
                return;
            }

            this.gridRequests.SelectedIndex = 0;
        }

        private void OnRequestSelectionChanged(object sender, EventArgs e)
        {
            if (this.gridBinding)
            {
                return;
            }

            if (this.requestsInFlight > 0 || this.submitting)
            {
                this.RestoreSelection();
                return;
            }

            string key = this.SelectedRequestId();

            if (key.Length > 0 && key == this.sampleRequestKey)
            {
                return;
            }

            this.ApplySelection(false);
        }

        private void RestoreSelection()
        {
            if (this.SelectedRequestId() == this.sampleRequestKey)
            {
                return;
            }

            this.gridBinding = true;

            try
            {
                if (this.sampleRequestKey.Length == 0)
                {
                    this.gridRequests.SelectedIndex = -1;
                }
                else
                {
                    this.SelectRequest(this.sampleRequestKey);
                }
            }
            finally
            {
                this.gridBinding = false;
            }
        }

        private void ApplySelection(bool silent)
        {
            DataRowView request = this.gridRequests.SelectedItem as DataRowView;

            if (request == null)
            {
                this.sampleRequestKey = string.Empty;
                this.ClearDependents();
                return;
            }

            string key = RequestId(request.Row);

            if (key.Length == 0)
            {
                this.sampleRequestKey = string.Empty;
                this.ClearDependents();
                return;
            }

            this.InvalidateChannel(channelSamples);
            this.sampleRequestKey = key;
            this.lblRequestKey.Text = key;
            this.ApplyActions(request.Row);
            this.ApplyStatus(this.StateOf(request.Row));

            this.LoadSamples(key, silent);
        }

        private async void LoadSamples(string reqSerialNo, bool silent)
        {
            IDisposable cover = this.Busy(RequestManagementPresenter.LoadingSamplesText, reqSerialNo, this.sampleCard);
            LoadOutcome<DataTable> outcome;

            try
            {
                outcome = await this.FetchAsync(channelSamples, () => this.GetRequestSamples(reqSerialNo), silent);
            }
            finally
            {
                cover.Dispose();
            }

            if (!outcome.IsCurrent)
            {
                return;
            }

            this.BindSamples(outcome.Failure != null ? null : outcome.Value);
        }

        private void BindSamples(DataTable samples)
        {
            SampleColumns(samples).Bind(this.gridSamples);
        }

        private void ApplyStatus(string stateCode)
        {
            this.stepStatus.DataSource = RequestManagementPresenter.Steps(stateCode);

            string text = RequestManagementPresenter.StateText(stateCode);

            bool cancelled = RequestManagementPresenter.StateSemantic(stateCode) == SemanticKind.Error;

            this.badgeState.Text = text;
            this.badgeState.Semantic = cancelled ? SemanticKind.Error : SemanticKind.None;
            this.badgeState.ColorValue = cancelled ? string.Empty : (stateCode ?? string.Empty).Trim();
            this.badgeState.Visible = text.Length > 0;
        }

        private void ClearDependents()
        {
            this.InvalidateChannel(channelSamples);

            this.gridSamples.DataSource = null;

            this.ClearStatus();
        }

        private void ApplyActions(DataRow request)
        {
            string state = request == null ? string.Empty : this.StateOf(request);

            this.actionState = state;
            this.nextStateCode = NextState(state);

            this.btnNextStep.TopLabel = RequestManagementPresenter.NextStepTopLabel(this.nextStateCode);
            this.btnNextStep.Text = RequestManagementPresenter.NextStepButtonText(this.nextStateCode);
            this.UpdateActionState();
        }

        private void OnNextStepClick(object sender, EventArgs e)
        {
            string state = this.SelectedState();

            if (NextStepReason(state, this.submitting, this.RequestRangeComplete) != null)
            {
                return;
            }

            string next = NextState(state);

            this.Submit(
                    next,
                    RequestManagementPresenter.NextStepConfirm(this.sampleRequestKey, next),
                    RequestManagementPresenter.NextStepCaption,
                    RequestManagementPresenter.NextStepBusyText);
        }

        private void OnRejectClick(object sender, EventArgs e)
        {
            if (RejectReason(this.SelectedState(), this.submitting, this.RequestRangeComplete) != null)
            {
                return;
            }

            this.Submit(
                    Schemas.Request.StateCode.Draft0.Value,
                    RequestManagementPresenter.RejectConfirm(this.sampleRequestKey),
                    RequestManagementPresenter.RejectCaption,
                    RequestManagementPresenter.RejectBusyText);
        }

        private void Submit(string stateCode, string confirm, string caption, string busyText)
        {
            string reqSerialNo = this.sampleRequestKey;
            DataRowView request = this.gridRequests.SelectedItem as DataRowView;

            if (reqSerialNo.Length == 0 || this.submitting || request == null)
            {
                return;
            }

            DataRow sentRow = request.Row;
            DataRow actionRow = ActionRows.Copy(sentRow);

            if (!this.Confirm(confirm, caption))
            {
                return;
            }

            this.submitting = true;
            this.UpdateActionState();

            this.RunAction(
                    () => this.SetRequestState(
                            actionRow, Schemas.Request.StateCode.StatusOf(stateCode), stateCode),
                    delegate(DataActionResult success)
                    {
                        this.submitting = false;
                        this.ExecuteSearch(false);
                    },
                    delegate(DataActionResult rejected)
                    {
                        this.submitting = false;
                        this.ApplyActions(sentRow);
                        this.ShowActionFailure(rejected);
                    },
                    busyText);
        }

        private void ClearStatus()
        {
            this.ApplyActions(null);

            this.stepStatus.DataSource = RequestManagementPresenter.Steps(string.Empty);

            this.badgeState.Text = string.Empty;
            this.badgeState.Semantic = SemanticKind.None;
            this.badgeState.ColorValue = string.Empty;
            this.badgeState.Visible = false;

            this.lblRequestKey.Text = RequestManagementPresenter.NoSelectionText;
        }

        private string SelectedRequestId()
        {
            DataRowView request = this.gridRequests.SelectedItem as DataRowView;

            return request == null ? string.Empty : RequestId(request.Row);
        }

        private string SelectedState()
        {
            DataRowView request = this.gridRequests.SelectedItem as DataRowView;
            return request == null ? string.Empty : this.StateOf(request.Row);
        }

        private string StateOf(DataRow request)
        {
            return request.Table.Columns.Contains(Schemas.Request.StateCode.Name)
                    ? TableHelper.CellText(request, Schemas.Request.StateCode.Name)
                    : string.Empty;
        }

        private static string RequestId(DataRow request)
        {
            return RequestManagementPresenter.Normalize(
                    TableHelper.CellText(request, Schemas.Request.ReqSerialNo.Name));
        }

        private DataTable BindJudged(
                string tableId, ref TableResponse current, DataTable incoming, bool silent)
        {
            if (incoming == null)
            {
                current = null;
                return null;
            }

            TableReception reception = TableJudgment.Receive(
                    tableId, incoming, Contracts, RequestManagementPresenter.ScreenColumns);

            if (reception.IsMissingRequired)
            {
                current = null;
                this.ShowMissingColumns(reception.Response, silent);
                return null;
            }

            current = reception.Response;
            return current.Table;
        }

        private sealed class SearchConditions
        {
            public string ReqSerialNo { get; set; }
            public string Requester { get; set; }
            public string StateCodes { get; set; }
            public string ReqFromDt { get; set; }
            public string ReqToDt { get; set; }
            public string AcceptFromDt { get; set; }
            public string AcceptToDt { get; set; }

            public SearchConditions()
            {
                this.ReqSerialNo = string.Empty;
                this.Requester = string.Empty;
                this.StateCodes = string.Empty;
                this.ReqFromDt = string.Empty;
                this.ReqToDt = string.Empty;
                this.AcceptFromDt = string.Empty;
                this.AcceptToDt = string.Empty;
            }

            public string Key
            {
                get
                {
                    return string.Join(
                            "|",
                            this.ReqSerialNo, this.Requester, this.StateCodes,
                            this.ReqFromDt, this.ReqToDt, this.AcceptFromDt, this.AcceptToDt);
                }
            }

            public bool IsEmpty
            {
                get
                {
                    return this.ReqSerialNo.Length + this.Requester.Length + this.StateCodes.Length
                            + this.ReqFromDt.Length + this.ReqToDt.Length
                            + this.AcceptFromDt.Length + this.AcceptToDt.Length == 0;
                }
            }
        }
    }
}
