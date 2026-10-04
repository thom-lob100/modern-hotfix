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
using Modern.Lab.Samples.Contracts;
using Modern.Lab.WinForms.Controls.Selection;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.ResponseContracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Display;
using Modern.Lab.Hosting.NewItems;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{

    public partial class LotManagementForm : ModernFormBase
    {
        private DataTable lotData;
        private bool waferBinding;
        private readonly NewItemTracker newItems = new NewItemTracker();
        private readonly NewItemTracker lotHistoryNewItems = new NewItemTracker();
        private readonly NewItemTracker waferHistoryNewItems = new NewItemTracker();
        private string newItemCriteria;
        private DataTable waferData;
        private DataTable lotHistoryData;
        private DataTable waferHistoryData;
        private string waferHistoryOwner;

        private const string channelTypes = "subProdTyps";
        private const string channelLots = "lots";
        private const string channelSuggest = "suggest";
        private const string channelWafers = "wafers";
        private const string channelLotHistory = "lotHistory";
        private const string channelWaferHistory = "waferHistory";
        private const string channelTrays = "trays";

        private const int waferSelectionDelay = 300;
        private const string lotHistoryTitle = "Lot History";

        private bool typeSetup;
        private bool gridBinding;

        private bool searchReady;
        private string waferSubProdTyp = string.Empty;

        private readonly DependentCover waferCover;
        private readonly DependentCover historyCover;

        public LotManagementForm()
        {
            this.InitializeComponent();

            this.waferCover = new DependentCover(this.waferBusy);
            this.historyCover = new DependentCover(this.historyBusy);

            this.InitializeModernForm(this.midPanel);

            this.DeferredResize = true;

            this.RegisterFindShortcut(this.gridLotHistory, this.gridWaferHistory);

            this.menuLot.Renderer = new Modern.Lab.WinForms.Rendering.ModernMenuRenderer();
        }

        protected override void ShowRequestInfoDialog(DataTable requestInfo)
        {
            using (RequestInfoDialogForm dialog = new RequestInfoDialogForm())
            {
                dialog.SetRequest(requestInfo, RequestSampleFirstColumn, RequestPurposeColumn);
                dialog.ShowDialog(this);
            }
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            this.cboType.UseEntity(Schemas.CommonTypeDet);

            DateTime today = DateTime.Today;
            this.dtpCreate.SetRange(today.AddMonths(-1), today);

            this.txtLotId.AutoCompleteMode = AutoCompleteMode.Suggest;
            this.txtLotId.AutoCompleteSource = AutoCompleteSource.CustomSource;
            this.txtLotId.SuggestionFilterMode = Modern.Lab.Controls.Wpf.Input.SuggestionFilterMode.None;

            this.gridLots.RowKeyMember = Schemas.Lot.RowKey;
            this.gridLots.CellLinkClick += this.OnReqSerialNoLinkClick;
            this.fieldInfo.FieldLinkClick += this.OnInfoLinkClick;
            this.gridWaferHistory.CellLinkClick += this.OnReqSerialNoLinkClick;

            this.gridWafers.RowKeyMember = Schemas.Wafer.RowKey;
            this.ApplyWaferSubProdTyp(Schemas.Lot.SubProdTyp.Wafer.Code);

            this.fieldInfo.Columns = 7;

            this.gridLotHistory.RowKeyMember = Schemas.LotHistory.RowKey;
            this.gridWaferHistory.RowKeyMember = Schemas.WaferHistory.RowKey;
            this.gridWaferHistory.RowForegroundMember = JudgeResult.ColorColumn;

            this.PopulateMenu();

            this.ddbCreate.DisplayMember = "LABEL";
            this.ddbCreate.ValueMember = "VALUE";
            this.ddbCreate.EnabledMember = "CAN";
            this.ddbHold.DisplayMember = "LABEL";
            this.ddbHold.ValueMember = "VALUE";
            this.ddbHold.EnabledMember = "CAN";

            this.RefreshActionStates();

            this.LoadTypes();
        }

        private async void LoadTypes()
        {
            LoadOutcome<DataTable> outcome = await this.FetchAsync(
                    channelTypes, () => this.GetLotTypes());

            if (!outcome.IsCurrent || outcome.Failure != null)
            {
                return;
            }

            this.typeSetup = true;

            try
            {
                this.cboType.DataSource = outcome.Value;
                this.cboType.CheckAll();
            }
            finally
            {
                this.typeSetup = false;
            }

            this.searchReady = true;
            this.ExecuteSearch();
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            if (!this.searchReady)
            {
                return;
            }

            this.CancelDebounce(channelSuggest);
            this.InvalidateChannel(channelSuggest);
            this.txtLotId.CloseSuggestions();
            this.ExecuteSearch();
        }

        private void OnResetClick(object sender, EventArgs e)
        {
            if (!this.searchReady)
            {
                return;
            }

            this.typeSetup = true;

            try
            {
                this.cboType.CheckAll();
            }
            finally
            {
                this.typeSetup = false;
            }

            DateTime today = DateTime.Today;
            this.dtpCreate.SetRange(today.AddMonths(-1), today);
            this.txtLotId.Text = string.Empty;
            this.CancelDebounce(channelSuggest);
            this.InvalidateChannel(channelSuggest);
            this.txtLotId.CloseSuggestions();
            this.ExecuteSearch();
        }

        private void OnLotIdTextChanged(object sender, EventArgs e)
        {
            if (!this.searchReady)
            {
                return;
            }

            if (this.IsExistingCandidate(this.txtLotId.Text.Trim()))
            {
                this.CancelDebounce(channelSuggest);
                return;
            }

            this.Debounce(channelSuggest, 300, new MethodInvoker(this.LoadIdCandidatesAsync));
        }

        private bool IsExistingCandidate(string text)
        {
            AutoCompleteStringCollection source = this.txtLotId.AutoCompleteCustomSource;

            if (source == null || text.Length == 0)
            {
                return false;
            }

            foreach (string candidate in source)
            {
                if (string.Equals(candidate, text, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private async void LoadIdCandidatesAsync()
        {
            string keyword = this.txtLotId.Text.Trim();

            if (keyword.Length == 0)
            {
                this.txtLotId.AutoCompleteCustomSource = null;
                return;
            }

            LoadOutcome<DataTable> outcome = await this.FetchAsync(
                    channelSuggest, () => this.GetLotIdCandidates(keyword));

            if (!outcome.IsCurrent || outcome.Failure != null || outcome.Value == null)
            {
                return;
            }

            AutoCompleteStringCollection collection = new AutoCompleteStringCollection();

            foreach (DataRow row in outcome.Value.Rows)
            {
                collection.Add(TableHelper.CellText(row, Schemas.Lot.LotId.Name).Trim());
            }

            this.txtLotId.AutoCompleteCustomSource = collection;
        }

        private string SelectedSubProdTyps()
        {
            return LotManagementPresenter.TypeFilter(this.cboType.CheckedValues);
        }

        private string SelectedLotId()
        {
            DataRowView lot = this.gridLots.SelectedItem as DataRowView;

            return lot == null ? string.Empty : LotManagementPresenter.Id(lot.Row);
        }

        private async void ExecuteSearch(bool silent = false, string focusLotId = null)
        {
            string subProdTyps = this.SelectedSubProdTyps();
            this.newItems.BeginQuery();
            this.RefreshActionStates();
            string fromDate = LotManagementPresenter.DateText(this.dtpCreate.FromDate);
            string toDate = LotManagementPresenter.DateText(this.dtpCreate.ToDate);
            string lotId = this.txtLotId.Text.Trim();
            string keepLotId = this.SelectedLotId();

            IDisposable busy = silent ? null : this.Busy("Loading lots...", fromDate + " ~ " + toDate);

            LoadOutcome<DataTable> outcome;

            try
            {
                outcome = await this.FetchAsync(
                        channelLots, () => this.GetLots(subProdTyps, fromDate, toDate, lotId), silent);
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
                this.RefreshActionStates();
                return;
            }

            this.newItemCriteria = NewItemTracker.Criteria(NewItemTracker.SelectionSet(subProdTyps), fromDate, toDate, lotId);
            this.BindLots(outcome.Value, keepLotId, silent, focusLotId);
        }

        private void BindLots(DataTable lots, string keepLotId, bool silent, string createdId = null)
        {
            DataTable canonical = this.ReadResponse(lots, silent, Schemas.Lot.LotId.Name);
            this.newItems.BeginQuery();
            this.lotData = this.newItems.Accept(canonical, Schemas.Lot.LotId.Name, this.newItemCriteria);
            this.gridLots.RowKeyMember = Schemas.Lot.RowKey;
            LotColumns(this.lotData).Apply(this.gridLots);
            LotFields(canonical).Apply(this.fieldInfo);
            string selectId = LotManagementPresenter.SelectionAfterBind(canonical, keepLotId);

            this.gridBinding = true;

            try
            {
                this.gridLots.DataSource = this.lotData;
                this.gridLots.SelectedIndex = -1;
                this.SelectLot(selectId);
                NewItemTint.Bind(this.gridLots, this.newItems, Schemas.Lot.LotId.Name, createdId);
            }
            finally
            {
                this.gridBinding = false;
            }

            this.lotCard.TitleRightText = this.lotData == null
                    ? string.Empty
                    : this.lotData.Rows.Count.ToString("N0") + " lots";

            this.ApplyLotSelection(silent);
        }

        private void SelectLot(string lotId)
        {
            if (this.lotData == null || lotId.Length == 0)
            {
                return;
            }

            DataRowView first = null;
            foreach (DataRowView row in this.lotData.DefaultView)
            {
                if (LotManagementPresenter.Id(row.Row).Length == 0)
                {
                    continue;
                }
                if (first == null)
                {
                    first = row;
                }
                if (LotManagementPresenter.Id(row.Row) == lotId)
                {
                    this.gridLots.SelectedItem = row;
                    return;
                }
            }
            this.gridLots.SelectedItem = first;
        }

        private void OnLotSelectionChanged(object sender, EventArgs e)
        {
            if (this.gridBinding)
            {
                return;
            }

            this.ApplyLotSelection(false);
        }

        private void ApplyLotSelection(bool silent)
        {
            DataRowView lot = this.gridLots.SelectedItem as DataRowView;

            if (lot == null)
            {
                this.fieldInfo.ClearValues();
                this.ClearDependents();
                this.RefreshActionStates();
                return;
            }

            string lotId = LotManagementPresenter.Id(lot.Row);

            this.fieldInfo.SetRow(lot.Row);
            this.ApplyWaferSubProdTyp(LotManagementPresenter.Type(lot.Row));

            bool loadable = this.lotData != null && lotId.Length > 0;

            if (loadable)
            {
                this.InvalidateDependents();
            }
            else
            {
                this.ClearDependents();
            }

            this.RefreshActionStates();
            this.pageLotHistory.Text = lotHistoryTitle + " — " + lotId;
            this.ApplyWaferCardTitle();

            if (loadable)
            {
                this.LoadDependents(lotId, silent);
            }
        }

        private void ApplyWaferSubProdTyp(string subProdTyp)
        {
            if (this.waferSubProdTyp == subProdTyp)
            {
                return;
            }

            this.waferSubProdTyp = subProdTyp;
            string caption = LotManagementPresenter.SubProdTypeCaption(subProdTyp);
            this.waferBusy.Message = "Loading " + caption + "...";
            this.gridWafers.EmptyText = "No " + caption;
            this.gridWaferHistory.EmptyText = "No " + caption + " history";
            this.gridWafers.StatusCountFormat = "{0:N0} " + caption;
            this.waferCard.Text = LotManagementPresenter.WaferListTitle(subProdTyp);
            this.ApplyWaferCardTitle();
            this.pageWaferHistory.Text = LotManagementPresenter.WaferHistoryTitle(subProdTyp);
        }

        private void ApplyWaferCardTitle()
        {
            DataRowView lot = this.gridLots.SelectedItem as DataRowView;
            string title = LotManagementPresenter.WaferListTitle(this.waferSubProdTyp);
            this.waferCard.Text = lot == null ? title : title + " — " + LotManagementPresenter.Id(lot.Row);
        }

        private void InvalidateDependents()
        {
            this.InvalidateChannel(channelWafers);
            this.InvalidateChannel(channelLotHistory);
            this.InvalidateChannel(channelWaferHistory);
            this.waferHistoryOwner = null;
        }

        private void ClearDependents()
        {
            this.waferCover.Reset();
            this.historyCover.Reset();
            this.InvalidateChannel(channelWafers);
            this.InvalidateChannel(channelLotHistory);
            this.InvalidateChannel(channelWaferHistory);

            this.waferData = null;
            this.gridWafers.DataSource = null;
            this.lotHistoryData = null;
            this.gridLotHistory.DataSource = null;
            this.waferHistoryData = null;
            this.waferHistoryOwner = null;
            this.gridWaferHistory.DataSource = null;
            this.pageLotHistory.Text = lotHistoryTitle;
            this.waferCard.Text = LotManagementPresenter.WaferListTitle(this.waferSubProdTyp);
            this.pageWaferHistory.Text = LotManagementPresenter.WaferHistoryTitle(this.waferSubProdTyp);
        }

        private void LoadDependents(string lotId, bool silent)
        {
            this.LoadDependent(channelWafers, () => this.GetWafers(lotId), table => this.BindWafers(table, silent), silent, this.waferCover);
            this.LoadDependent(channelLotHistory, () => this.GetLotHistory(lotId),
                    table => this.BindLotHistory(table, NewItemTracker.Criteria(lotId)), silent, this.historyCover);
        }

        private async void LoadDependent(
                string channel, Func<DataTable> request, Action<DataTable> bind, bool silent, DependentCover cover)
        {
            cover.Begin();
            LoadOutcome<DataTable> outcome;

            try
            {
                outcome = await this.FetchAsync(channel, request, silent);
            }
            finally
            {
                cover.End();
            }

            if (!outcome.IsCurrent)
            {
                return;
            }

            bind(outcome.Failure != null ? null : outcome.Value);
        }

        private void BindWafers(DataTable wafers, bool silent)
        {
            this.waferData = this.ReadResponse(wafers, silent, Schemas.Wafer.WfId.Name, Schemas.Wafer.SlotNo.Name);
            this.gridWafers.RowKeyMember = Schemas.Wafer.RowKey;
            WaferColumns(this.waferData, LotManagementPresenter.SubProdTypeCaption(this.waferSubProdTyp) + " Id")
                    .Apply(this.gridWafers);
            this.waferBinding = true;
            try
            {
                this.gridWafers.DataSource = this.waferData;
                this.gridWafers.SelectedIndex = -1;
                if (this.waferData != null)
                {
                    foreach (DataRowView row in this.waferData.DefaultView)
                    {
                        if (TableHelper.CellText(row.Row, Schemas.Wafer.WfId.Name).Trim().Length > 0)
                        {
                            this.gridWafers.SelectedItem = row;
                            break;
                        }
                    }
                }
            }
            finally
            {
                this.waferBinding = false;
            }
            this.ApplyWaferSelection(silent);
        }

        private void BindLotHistory(DataTable history, string criteria)
        {
            this.lotHistoryData = this.lotHistoryNewItems.Accept(history, this.gridLotHistory.RowKeyMember, criteria);

            LotHistoryColumns(this.lotHistoryData).Bind(this.gridLotHistory);
            NewItemTint.Bind(this.gridLotHistory, this.lotHistoryNewItems, this.gridLotHistory.RowKeyMember);
        }

        private void BindWaferHistory(DataTable history, string criteria)
        {
            this.waferHistoryData = this.waferHistoryNewItems.Accept(history, this.gridWaferHistory.RowKeyMember, criteria);
            JudgeResult.Mark(this.waferHistoryData);

            WaferHistoryColumns(this.waferHistoryData, LotManagementPresenter.SubProdTypeCaption(this.waferSubProdTyp) + " Id")
                    .Bind(this.gridWaferHistory);
            NewItemTint.Bind(this.gridWaferHistory, this.waferHistoryNewItems, this.gridWaferHistory.RowKeyMember);
        }

        private void OnWaferSelectionChanged(object sender, EventArgs e)
        {
            if (this.waferBinding)
            {
                return;
            }
            this.Debounce(channelWaferHistory, waferSelectionDelay, new MethodInvoker(delegate { this.ApplyWaferSelection(false); }));
        }

        private void ApplyWaferSelection(bool silent)
        {
            this.CancelDebounce(channelWaferHistory);

            DataRowView lot = this.gridLots.SelectedItem as DataRowView;
            DataRowView wafer = this.gridWafers.SelectedItem as DataRowView;
            string waferId = wafer == null ? string.Empty : TableHelper.CellText(wafer.Row, Schemas.Wafer.WfId.Name).Trim();

            if (lot == null || LotManagementPresenter.Id(lot.Row).Length == 0 || waferId.Length == 0
                    || this.lotData == null || this.waferData == null)
            {
                this.InvalidateChannel(channelWaferHistory);
                this.waferHistoryData = null;
                this.waferHistoryOwner = null;
                this.gridWaferHistory.DataSource = null;
                this.pageWaferHistory.Text = LotManagementPresenter.WaferHistoryTitle(this.waferSubProdTyp);
                return;
            }

            string lotId = LotManagementPresenter.Id(lot.Row);

            this.waferHistoryOwner = waferId;
            this.pageWaferHistory.Text = LotManagementPresenter.WaferHistoryTitle(this.waferSubProdTyp) + " — " + waferId;
            this.LoadDependent(
                    channelWaferHistory,
                    () => this.GetWaferHistory(lotId, waferId),
                    table => this.BindWaferHistory(table, NewItemTracker.Criteria(lotId, waferId)), silent, this.historyCover);
        }

        private void RefreshActionStates()
        {
            DataRowView lot = this.gridLots.SelectedItem as DataRowView;
            DataRow row = lot == null || !this.newItems.ActionsReady ? null : lot.Row;

            this.ddbCreate.DataSource = LotManagementPresenter.BuildGatedMenuTable(LotManagementPresenter.CreateActions, key => this.ActionEnabled(key, row));
            this.ddbHold.DataSource = LotManagementPresenter.BuildGatedMenuTable(LotManagementPresenter.HoldActions, key => this.ActionEnabled(key, row));
            this.ddbHold.Enabled = this.newItems.ActionsReady && row != null;
            this.ApplyActionReason(
                    this.btnChangeSpec, this.ActionReason(LotManagementPresenter.ActionChangeSpec, row));
            this.ApplyActionReason(
                    this.btnScrap, this.ActionReason(LotManagementPresenter.ActionScrap, row));

            this.lblTarget.Text = LotManagementPresenter.StatusLineText(row);
        }

        private void OnActionClicked(object sender, DropDownItemClickedEventArgs e)
        {
            this.ExecuteAction(e.Value as string);
        }

        private void OnChangeSpecClick(object sender, EventArgs e)
        {
            this.ExecuteAction(LotManagementPresenter.ActionChangeSpec);
        }

        private void OnScrapClick(object sender, EventArgs e)
        {
            this.ExecuteAction(LotManagementPresenter.ActionScrap);
        }

        private void ExecuteAction(string key)
        {
            DataRowView lot = this.gridLots.SelectedItem as DataRowView;
            DataRow row = lot == null ? null : lot.Row;

            if (!LotManagementPresenter.IsActionKey(key) || !this.ActionEnabled(key, row)
                    || (key != LotManagementPresenter.ActionCreateWafer && !this.CanProcess(key, LotManagementPresenter.Id(row))))
            {
                return;
            }

            switch (key)
            {
                case LotManagementPresenter.ActionCreateWafer:
                    this.ShowCreateWaferDialog();
                    break;

                case LotManagementPresenter.ActionCreateChip:
                case LotManagementPresenter.ActionCreateLamella:
                    this.ExecuteCreateChild(row, LotManagementPresenter.ChildTypeOf(key));
                    break;

                case LotManagementPresenter.ActionHold:
                    this.ExecuteHold(row);
                    break;

                case LotManagementPresenter.ActionNotOnHold:
                    this.ExecuteNotOnHold(row);
                    break;

                case LotManagementPresenter.ActionChangeSpec:
                    this.ShowChangeSpecDialog(row);
                    break;

                case LotManagementPresenter.ActionScrap:
                    this.ExecuteScrap(row);
                    break;
            }
        }

        private void OnInfoLinkClick(object sender, ModernFieldLinkClickEventArgs e)
        {
            if (e.Member == ReqSerialNoColumn)
            {
                this.ShowRequestInfo(e.Value);
            }
        }

        private void ShowCreateWaferDialog()
        {
            using (CreateWaferDialogForm dialog = new CreateWaferDialogForm())
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                this.ShowToast(
                        dialog.CreatedMessage.Length > 0
                                ? dialog.CreatedMessage
                                : "Lot " + dialog.CreatedLotId + " created.",
                        ToastKind.Success);
                this.ExecuteSearch(true, dialog.CreatedLotId);
            }
        }

        private async void ExecuteCreateChild(DataRow lot, string childType)
        {
            string parentLotId = LotManagementPresenter.Id(lot);
            string key = childType == Schemas.Lot.SubProdTyp.Chip.Code ? LotManagementPresenter.ActionCreateChip : LotManagementPresenter.ActionCreateLamella;
            LoadOutcome<DataTable> outcome = await this.FetchAsync(channelTrays, () => this.GetTrays());

            if (!outcome.IsCurrent || outcome.Failure != null || !this.CanProcess(key, parentLotId))
            {
                return;
            }

            JudgeKind kind = childType == Schemas.Lot.SubProdTyp.Chip.Code ? JudgeKind.Chip : JudgeKind.Lamella;
            string missing = ResponseColumns.Missing(
                    outcome.Value, Schemas.Durable.DurableId.Name, TrayRoom.CountColumn(kind));

            if (missing.Length > 0)
            {
                this.ShowErrorMessage(
                        "Create " + childType + " Lot",
                        "The tray list could not be used. Try again, or contact support.",
                        "Tray list — missing column " + missing);
                return;
            }

            LotCreateChildDialogOptions options = new LotCreateChildDialogOptions();
            options.Title = "Create " + childType + " Lot — " + parentLotId;
            options.OkText = "Create";

            options.ValueReadOnly = childType;

            options.Targets = TrayRoom.EmptyOnly(outcome.Value, kind);
            options.TargetDisplayMember = Schemas.Durable.DurableId.Name;
            options.TargetValueMember = Schemas.Durable.DurableId.Name;

            this.OpenCreateChildDialog(parentLotId, childType, options);
        }

        protected virtual void OpenCreateChildDialog(
                string parentLotId, string childType, LotCreateChildDialogOptions options)
        {
            this.CreateChildWithJudge(childType, parentLotId, string.Empty, options.Targets, options.TargetValueMember);
        }

        private void ExecuteHold(DataRow lot)
        {
            this.OpenHoldDialog(lot, false);
        }

        private void ExecuteNotOnHold(DataRow lot)
        {
            this.OpenHoldDialog(lot, true);
        }

        protected virtual void OpenHoldDialog(DataRow lot, bool releaseMode)
        {
            string lotId = LotManagementPresenter.Id(lot);
            using (LotHoldDialogForm dialog = new LotHoldDialogForm(lot, releaseMode))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) { return; }
                if (releaseMode)
                {
                    this.ProcessNotOnHold(lotId, dialog.CurrentOperId, dialog.ReasonCode, dialog.EngrUserId, dialog.Description);
                }
                else
                {
                    this.ProcessHold(lotId, dialog.CurrentOperId, dialog.ReasonCode, dialog.EngrUserId, dialog.Description);
                }
            }
        }

        private void ShowChangeSpecDialog(DataRow lot)
        {
            string lotId = LotManagementPresenter.Id(lot);
            using (ChangeSpecDialogForm dialog = new ChangeSpecDialogForm(lot))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    this.ProcessChangeSpec(lotId, dialog.ProdId, dialog.FlowId, dialog.OperId, dialog.CurrentOperId, dialog.Description);
                }
            }
        }

        private void ExecuteScrap(DataRow lot)
        {
            this.OpenScrapDialog(lot);
        }

        protected virtual void OpenScrapDialog(DataRow lot)
        {
            string lotId = LotManagementPresenter.Id(lot);
            string idCaption = LotManagementPresenter.SubProdTypeCaption(this.waferSubProdTyp) + " Id";
            using (LotScrapDialogForm dialog = new LotScrapDialogForm(lot, this.waferData, idCaption))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    this.ProcessScrap(lotId, dialog.ReasonCode, dialog.WaferIds, dialog.Description);
                }
            }
        }

        private void ProcessCreateChild(string childType, string parentLotId, string trayId)
        {
            if (ResponseColumns.IsBlank(trayId))
            {
                return;
            }

            this.CreateChildWithJudge(childType, parentLotId, trayId, null, null);
        }

        private void CreateChildWithJudge(string childType, string parentLotId, string trayId, DataTable trays, string trayMember)
        {
            string key = childType == Schemas.Lot.SubProdTyp.Chip.Code ? LotManagementPresenter.ActionCreateChip : LotManagementPresenter.ActionCreateLamella;
            DataRow parent = FindLotRow(this.lotData, parentLotId);
            JudgeResultDialogOptions judgeOptions = new JudgeResultDialogOptions();
            judgeOptions.Title = "Create " + childType + " Lot — " + parentLotId;
            judgeOptions.OkText = "Create";
            judgeOptions.Kind = childType == Schemas.Lot.SubProdTyp.Chip.Code ? JudgeKind.Chip : JudgeKind.Lamella;
            judgeOptions.GoalDurableId = (trayId ?? string.Empty).Trim();
            judgeOptions.Targets = trays;
            judgeOptions.TargetMember = trayMember;
            judgeOptions.ReqSerialNo = parent == null ? string.Empty : Schemas.Lot.ReqSerialNo.Text(parent);
            judgeOptions.ShowDescription = false;

            JudgeResultDialogResult judged = this.OpenJudgeDialog(judgeOptions);

            if (judged == null)
            {
                return;
            }

            string judgedTrayId = judged.GoalDurableId.Length > 0 ? judged.GoalDurableId : judgeOptions.GoalDurableId;

            if (ResponseColumns.IsBlank(judgedTrayId))
            {
                return;
            }

            this.Process(
                    key, parentLotId,
                    () => this.CreateChildLot(childType, parentLotId, judgedTrayId, judged.Rows),
                    "Creating " + childType + " lot from " + parentLotId + "…",
                    reply => this.ExecuteSearch(true));
        }

        private static DataRow FindLotRow(DataTable lots, string lotId)
        {
            if (lots == null)
            {
                return null;
            }

            foreach (DataRow row in lots.Rows)
            {
                if (row.RowState != DataRowState.Deleted && Schemas.Lot.LotId.Text(row) == (lotId ?? string.Empty).Trim())
                {
                    return row;
                }
            }

            return null;
        }

        protected virtual JudgeResultDialogResult OpenJudgeDialog(JudgeResultDialogOptions options)
        {
            using (JudgeResultDialogForm dialog = new JudgeResultDialogForm(options))
            {
                return dialog.ShowDialog(this) == DialogResult.OK ? dialog.Result : null;
            }
        }

        private void ProcessChangeSpec(
                string lotId, string prodId, string flowId, string operId, string currentOperId, string description)
        {
            if (string.IsNullOrWhiteSpace(prodId) || string.IsNullOrWhiteSpace(flowId) || string.IsNullOrWhiteSpace(operId))
            {
                this.ShowWarningMessage("ChangeSpec: select Prod Id, Flow Id and Oper Id.");
                return;
            }

            this.Process(
                    LotManagementPresenter.ActionChangeSpec, lotId,
                    () => this.ChangeLotSpec(lotId, prodId, flowId, operId, currentOperId, description),
                    "Changing spec of " + lotId + "…",
                    reply => this.ExecuteSearch(true));
        }

        private static bool HoldInputComplete(string lotId, string code, string engrUserId)
        {
            return !string.IsNullOrWhiteSpace(lotId) && !string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(engrUserId);
        }

        private void ProcessHold(string lotId, string currentOperId, string holdCode, string engrUserId, string description)
        {
            if (!HoldInputComplete(lotId, holdCode, engrUserId)) { return; }
            this.Process(LotManagementPresenter.ActionHold, lotId,
                    () => this.HoldLot(lotId, currentOperId, holdCode, engrUserId, description),
                    "Holding " + lotId + "…", reply => this.ExecuteSearch(true));
        }

        private void ProcessNotOnHold(string lotId, string currentOperId, string releaseCode, string engrUserId, string description)
        {
            if (!HoldInputComplete(lotId, releaseCode, engrUserId)) { return; }
            this.Process(LotManagementPresenter.ActionNotOnHold, lotId,
                    () => this.NotOnHoldLot(lotId, currentOperId, releaseCode, engrUserId, description),
                    "Setting " + lotId + " to NotOnHold…", reply => this.ExecuteSearch(true));
        }

        private void ProcessScrap(string lotId, string scrapCode, string[] waferIds, string description)
        {
            string[] targets = waferIds ?? new string[0];
            this.Process(
                    LotManagementPresenter.ActionScrap, lotId,
                    () => this.ScrapLot(lotId, scrapCode, targets, description),
                    targets.Length == 0 ? "Scrapping " + lotId + "…" : "Scrapping " + targets.Length + " of " + lotId + "…",
                    reply => this.ExecuteSearch(true));
        }

        private void Process(string key, string lotId, Func<DataActionResult> call, string busyText, Action<DataActionResult> refresh)
        {
            if (!this.CanProcess(key, lotId))
            {
                return;
            }

            this.RunAction(
                    call,
                    delegate(DataActionResult reply)
                    {
                        this.ShowToast(
                                reply.Message.Length > 0 ? reply.Message : "Done.",
                                ToastKind.Success);
                        refresh(reply);
                    },
                    busyText);
        }

        private void PopulateMenu()
        {
            this.menuLot.Items.Clear();
            this.AppendMenuSection("Create", LotManagementPresenter.CreateActions);
            this.AppendMenuSection("Hold/NotOnHold", LotManagementPresenter.HoldActions);
            this.AppendMenuSection("Execute", LotManagementPresenter.ExecuteActions);
        }

        private void AppendMenuSection(string headerText, IList<LotAction> actions)
        {
            this.menuLot.Items.Add(new ToolStripLabel(headerText));

            foreach (LotAction action in actions)
            {
                this.menuLot.Items.Add(this.CreateMenuItem(action));
            }
        }

        private ToolStripMenuItem CreateMenuItem(LotAction action)
        {
            ToolStripMenuItem item = MenuGlyphs.CreateItem(action.Label);
            item.Tag = action.Key;
            item.Click += this.OnLotMenuItemClick;
            return item;
        }

        private void OnMenuLotOpening(object sender, CancelEventArgs e)
        {
            DataRowView lot = this.gridLots.SelectedItem as DataRowView;
            DataRow row = lot == null ? null : lot.Row;

            foreach (object entry in this.menuLot.Items)
            {
                ToolStripMenuItem item = entry as ToolStripMenuItem;
                string key = item == null ? null : item.Tag as string;

                if (key != null && LotManagementPresenter.IsActionKey(key))
                {
                    ApplyActionReason(item, this.ActionReason(key, row));
                }
            }
        }

        private void OnLotMenuItemClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;

            if (item != null)
            {
                this.ExecuteAction(item.Tag as string);
            }
        }
        private bool ActionEnabled(string key, DataRow lot)
        {
            return this.ActionReason(key, lot) == null;
        }
        private string ActionReason(string key, DataRow lot)
        {
            return LotActionReason(key, lot, this.newItems.ActionsReady);
        }
        private static DataRow FindRow(DataTable table, string keyColumn, string wanted)
        {
            if (table == null || string.IsNullOrEmpty(keyColumn)
                    || !table.Columns.Contains(keyColumn))
            {
                return null;
            }

            string key = (wanted ?? string.Empty).Trim();

            if (key.Length == 0)
            {
                return null;
            }

            foreach (DataRow row in table.Rows)
            {
                if (row.RowState == DataRowState.Deleted || row.RowState == DataRowState.Detached)
                {
                    continue;
                }
                if (string.Equals(TableHelper.CellText(row, keyColumn).Trim(), key, StringComparison.Ordinal))
                {
                    return row;
                }
            }

            return null;
        }
        private bool CanProcess(string key, string lotId)
        {
            if (!this.newItems.ActionsReady || this.lotData == null || string.IsNullOrWhiteSpace(lotId))
            {
                return false;
            }
            DataRow target = FindRow(this.lotData, Schemas.Lot.LotId.Name, lotId);
            return target != null && this.ActionReason(key, target) == null;
        }

        private DataTable ReadResponse(DataTable incoming, bool silent, params string[] required)
        {
            if (incoming == null)
            {
                return null;
            }
            if (incoming.Columns.Count == 0 && incoming.Rows.Count == 0)
            {
                DataTable empty = incoming.Clone();
                foreach (string column in required)
                {
                    empty.Columns.Add(column, typeof(string));
                }
                return empty;
            }
            string missing = ResponseColumns.Missing(incoming, required);
            if (missing.Length == 0)
            {
                return incoming.Copy();
            }
            if (!silent)
            {
                this.ShowErrorMessage("Query failed", ResponseColumns.MissingMessage, "Lot — missing column " + missing);
            }
            return null;
        }
    }
}
