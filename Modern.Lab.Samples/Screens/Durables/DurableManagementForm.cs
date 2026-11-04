using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Data;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.ResponseContracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.Samples.Contracts;
using Modern.Lab.WinForms.Controls.Selection;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Display;
using Modern.Lab.Hosting.NewItems;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{

    public partial class DurableManagementForm : ModernFormBase
    {
        private DataTable durableData;
        private DataTable boundDurables;
        private readonly NewItemTracker newItems = new NewItemTracker();
        private readonly NewItemTracker durableHistoryNewItems = new NewItemTracker();
        private readonly NewItemTracker durableChipHistoryNewItems = new NewItemTracker();
        private readonly NewItemTracker durableLamellaHistoryNewItems = new NewItemTracker();
        private int durableSearchVersion;
        private int durableSelectionVersion;
        private string newItemCriteria;
        private DataTable slotData;
        private DataTable durableHistoryData;
        private DataTable durableChipHistoryData;
        private DataTable durableLamellaHistoryData;

        private const string channelTypes = "types";
        private const string channelFacilities = "facilities";
        private const string channelCandidates = "candidates";
        private const string channelFabFilter = "fabFilter";
        private const string channelDurableIdFilter = "durableIdFilter";
        private const string channelLotIdFilter = "lotIdFilter";
        private const string channelSerialNoFilter = "serialNoFilter";
        private const string channelDurables = "durables";
        private const string channelSlots = "slots";
        private const string channelDurableWafers = "durableWafers";
        private const string channelDurableChips = "durableChips";
        private const string channelDurableLamellas = "durableLamellas";
        private const string channelDurableHistory = "durableHistory";
        private const string channelDurableChipHistory = "durableChipHistory";
        private const string channelDurableLamellaHistory = "durableLamellaHistory";
        private const string channelCategories = "categories";
        private const string channelEditContents = "editContents";
        private const string durableHistoryTitle = "Durable History";
        private const string stubHistoryTitle = "Stub History";
        private const string lccHistoryTitle = "Lcc History";

        private DataTable typeData;
        private bool typeSetup;
        private DataTable candidateData;
        private string candidateKey;
        private bool candidateBinding;
        private string appliedFabId = string.Empty;

        private static readonly string[] CandidateColumns =
        {
            Schemas.Durable.DurableId.Name, Schemas.Lot.LotId.Name, Schemas.Lot.ReqSerialNo.Name,
        };
        private bool resettingCriteria;
        private bool gridBinding;

        private string layoutType = string.Empty;

        private static readonly ResponseContractSet Contracts = DurableContracts.Build();

        private DataTable categoryData;

        private TableResponse durableCurrent;

        private readonly DependentCover contentCover;
        private readonly DependentCover historyCover;

        public DurableManagementForm()
        {
            this.InitializeComponent();
            SearchBandLayout.Attach(this.searchCard,
                    new SearchBandLayout.Field[]
                    {
                        SearchBandLayout.Pair(this.lblType, this.cboType),
                        SearchBandLayout.Pair(this.lblFab, this.cboFab),
                        SearchBandLayout.Pair(this.lblDurableId, this.cboDurableId),
                        SearchBandLayout.Pair(this.lblLotId, this.cboLotId),
                        SearchBandLayout.Pair(this.lblSerialNo, this.cboSerialNo),
                    },
                    new Control[] { this.btnReset, this.btnRefresh },
                    HorizontalAlignment.Right);

            this.contentCover = new DependentCover(this.contentBusy);
            this.historyCover = new DependentCover(this.historyBusy);

            this.InitializeModernForm(this.midPanel);
            this.RegisterRefreshShortcut(this.btnRefresh);

            this.DeferredResize = true;

            this.RegisterFindShortcut(this.gridDurables, this.gridDurableHistory, this.gridDurableChipHistory, this.gridDurableLamellaHistory);

            this.menuDurable.Renderer = new Modern.Lab.WinForms.Rendering.ModernMenuRenderer();
        }

        protected override void ShowRequestInfoDialog(DataTable requestInfo)
        {
            using (RequestInfoDialogForm dialog = new RequestInfoDialogForm())
            {
                dialog.SetRequest(requestInfo, RequestSampleFirstColumn, RequestPurposeColumn);
                dialog.ShowDialog(this);
            }
        }

        private void OnDurableIdLinkClick(object sender, GridButtonClickEventArgs e)
        {
            string durableId = DurableInfoDialogForm.LinkedDurableId(e, Schemas.Durable.DurableId.Name);

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

        private void OnFormLoad(object sender, EventArgs e)
        {
            this.cboType.UseEntity(Schemas.CommonTypeDet);
            this.cboFab.UseEntity(Schemas.CommonTypeDet);
            this.cboDurableId.DisplayMember = Schemas.Durable.DurableId.Name;
            this.cboDurableId.ValueMember = Schemas.Durable.DurableId.Name;
            this.cboLotId.DisplayMember = Schemas.Lot.LotId.Name;
            this.cboLotId.ValueMember = Schemas.Lot.LotId.Name;
            this.cboSerialNo.DisplayMember = Schemas.Lot.ReqSerialNo.Name;
            this.cboSerialNo.ValueMember = Schemas.Lot.ReqSerialNo.Name;
            this.WatchCandidateText(this.cboFab, this.OnFabTextChanged);
            this.WatchCandidateText(this.cboDurableId, this.OnDurableIdTextChanged);
            this.WatchCandidateText(this.cboLotId, this.OnLotIdTextChanged);
            this.WatchCandidateText(this.cboSerialNo, this.OnSerialNoTextChanged);

            this.gridDurables.RowKeyMember = Schemas.Durable.RowKey;
            this.gridDurables.CellLinkClick += this.OnDurableIdLinkClick;

            this.gridDurableWafers.RowKeyMember = Schemas.DurableWafer.RowKey;
            this.gridDurableChips.RowKeyMember = Schemas.DurableChip.RowKey;
            this.gridDurableLamellas.RowKeyMember = Schemas.DurableLamella.RowKey;
            this.ApplyTypeLayout(Schemas.Durable.DurableTyp.Foup.Value);

            this.fieldInfo.Columns = 7;

            this.gridDurableHistory.RowKeyMember = Schemas.DurableHistory.RowKey;

            this.PrepareUnitHistory(this.gridDurableChipHistory, Schemas.DurableChipHistory.RowKey);
            this.PrepareUnitHistory(this.gridDurableLamellaHistory, Schemas.DurableLamellaHistory.RowKey);

            this.ConfigureActionDropDown(this.ddbRegister);
            this.ConfigureActionDropDown(this.ddbHold);
            this.ConfigureActionDropDown(this.ddbMoveWafer);
            this.ConfigureActionDropDown(this.ddbTransport);
            this.ConfigureActionDropDown(this.ddbScrap);
            this.PopulateMenus();
            this.RefreshActionStates();

            this.LoadFacilities();
            this.LoadTypes();
        }

        private void ApplyTypeLayout(string durableType)
        {
            if (this.layoutType == durableType)
            {
                return;
            }

            this.layoutType = durableType;
            bool tray = durableType == Schemas.Durable.DurableTyp.Tray.Value;

            this.tabContent.SuspendLayout();
            this.tabHistory.SuspendLayout();

            try
            {
                this.tabContent.Controls.Remove(this.pageWaferList);
                this.tabContent.Controls.Remove(this.pageChipList);
                this.tabContent.Controls.Remove(this.pageLamellaList);
                this.tabHistory.Controls.Remove(this.pageStubHistory);
                this.tabHistory.Controls.Remove(this.pageLccHistory);

                if (tray)
                {
                    this.tabContent.Controls.Add(this.pageChipList);
                    this.tabContent.Controls.Add(this.pageLamellaList);
                    this.tabHistory.Controls.Add(this.pageStubHistory);
                    this.tabHistory.Controls.Add(this.pageLccHistory);
                }
                else
                {
                    this.tabContent.Controls.Add(this.pageWaferList);
                }
            }
            finally
            {
                this.tabContent.ResumeLayout(true);
                this.tabHistory.ResumeLayout(true);
            }

            this.tabContent.SelectedIndex = 0;
            this.tabHistory.SelectedIndex = 0;
        }

        private async void LoadTypes()
        {
            LoadOutcome<DataTable> outcome = await this.FetchAsync(
                    channelTypes, () => this.GetDurableTypes());

            if (!outcome.IsCurrent || outcome.Failure != null)
            {
                return;
            }

            this.typeSetup = true;

            try
            {
                this.typeData = outcome.Value;
                this.cboType.DataSource = outcome.Value;
                this.cboType.SelectedValue = Schemas.Durable.DurableTyp.Foup.Value;
            }
            finally
            {
                this.typeSetup = false;
            }

            this.ApplyFabEnabled();
            this.LoadCandidates();
            this.ExecuteSearch();
        }

        private void OnTypeChanged(object sender, EventArgs e)
        {
            if (this.typeSetup)
            {
                return;
            }

            this.ApplyFabEnabled();
            this.LoadCandidates();
            this.ExecuteSearch();
        }

        private async void LoadFacilities()
        {
            LoadOutcome<DataTable> outcome = await this.FetchAsync(
                    channelFacilities, () => this.GetSendFacilities(), true);

            if (!outcome.IsCurrent || outcome.Failure != null)
            {
                return;
            }

            this.candidateBinding = true;

            try
            {
                this.cboFab.DataSource = outcome.Value;
                this.cboFab.SelectedIndex = -1;
                this.cboFab.Text = string.Empty;
            }
            finally
            {
                this.candidateBinding = false;
            }
        }

        private void ApplyFabEnabled()
        {
            bool foup = this.SelectedType() == Schemas.Durable.DurableTyp.Foup.Value;

            if (!foup && this.cboFab.Text.Length > 0)
            {
                this.SetCandidateText(this.cboFab, string.Empty);
            }

            this.cboFab.Enabled = foup;
            this.appliedFabId = this.SelectedFabId();
        }

        private string SelectedFabId()
        {
            if (!this.cboFab.Enabled)
            {
                return string.Empty;
            }

            return DurableManagementPresenter.CandidateValue(
                    this.cboFab.DataSource as DataTable, Schemas.CommonTypeDet.TypVal.Name, this.cboFab.Text.Trim());
        }

        private void WatchCandidateText(Modern.Lab.WinForms.Controls.Selection.ModernComboBox combo, System.Windows.Controls.TextChangedEventHandler handler)
        {
            if (combo.Child != null)
            {
                combo.Child.AddHandler(System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent, handler, true);
            }
        }

        private void OnFabTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!this.candidateBinding)
            {
                this.Debounce(channelFabFilter, 300, new MethodInvoker(this.ApplyFabChange));
            }
        }

        private void ApplyFabChange()
        {
            string fabId = this.SelectedFabId();

            if (fabId == this.appliedFabId)
            {
                return;
            }

            this.appliedFabId = fabId;
            this.LoadCandidates();
            this.ExecuteSearch();
        }

        private void OnDurableIdTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!this.candidateBinding)
            {
                this.Debounce(channelDurableIdFilter, 300, new MethodInvoker(this.ApplyDurableIdChange));
            }
        }

        private void OnLotIdTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!this.candidateBinding)
            {
                this.Debounce(channelLotIdFilter, 300, new MethodInvoker(this.ApplyLotIdChange));
            }
        }

        private void OnSerialNoTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!this.candidateBinding)
            {
                this.Debounce(channelSerialNoFilter, 300, new MethodInvoker(this.ApplySerialNoChange));
            }
        }

        private void ApplyDurableIdChange()
        {
            this.ApplyPickerChange(0);
        }

        private void ApplyLotIdChange()
        {
            this.ApplyPickerChange(1);
        }

        private void ApplySerialNoChange()
        {
            this.ApplyPickerChange(2);
        }

        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox PickerCombo(int index)
        {
            switch (index)
            {
                case 0:
                    return this.cboDurableId;

                case 1:
                    return this.cboLotId;

                default:
                    return this.cboSerialNo;
            }
        }

        private string KnownPickerValue(int index)
        {
            return DurableManagementPresenter.CandidateValue(
                    this.candidateData, CandidateColumns[index], this.PickerCombo(index).Text.Trim());
        }

        private string[] KnownPickerValues()
        {
            return new string[] { this.KnownPickerValue(0), this.KnownPickerValue(1), this.KnownPickerValue(2) };
        }

        private static string[] Without(string[] values, int index)
        {
            string[] copy = (string[])values.Clone();
            copy[index] = string.Empty;
            return copy;
        }

        private void ApplyPickerChange(int changed)
        {
            string[] values = this.KnownPickerValues();
            string picked = values[changed];

            if (this.PickerCombo(changed).Text.Trim().Length == 0)
            {
                for (int index = changed + 1; index < values.Length; index++)
                {
                    if (this.PickerCombo(index).Text.Trim().Length > 0)
                    {
                        this.SetCandidateText(this.PickerCombo(index), string.Empty);
                    }

                    values[index] = string.Empty;
                }
            }

            if (picked.Length > 0)
            {
                for (int index = 0; index < values.Length; index++)
                {
                    if (index == changed || values[index].Length == 0)
                    {
                        continue;
                    }

                    string[] pair = new string[values.Length];
                    pair[changed] = picked;
                    pair[index] = values[index];

                    if (!DurableManagementPresenter.CompatibleCandidates(this.candidateData, CandidateColumns, pair))
                    {
                        this.SetCandidateText(this.PickerCombo(index), string.Empty);
                        values[index] = string.Empty;
                    }
                }

                for (int index = 0; index < values.Length; index++)
                {
                    if (index == changed || values[index].Length > 0)
                    {
                        continue;
                    }

                    string single = DurableManagementPresenter.SingleCandidate(
                            this.candidateData, CandidateColumns[index], CandidateColumns, Without(values, index));

                    if (single.Length > 0)
                    {
                        this.SetCandidateText(this.PickerCombo(index), single);
                        values[index] = single;
                    }
                }
            }

            this.BindPickerLists(values);
        }

        private void BindPickerLists(string[] values)
        {
            for (int index = 0; index < values.Length; index++)
            {
                this.BindCandidates(this.PickerCombo(index), DurableManagementPresenter.CandidateList(
                        this.candidateData, CandidateColumns[index], CandidateColumns, Without(values, index)));
            }
        }

        private async void LoadCandidates()
        {
            string durableType = this.SelectedType();
            string fabId = this.SelectedFabId();
            string key = durableType + "|" + fabId;

            if (this.candidateKey == key)
            {
                return;
            }

            this.candidateKey = key;
            this.ClearCandidateText();

            LoadOutcome<DataTable> outcome = await this.FetchAsync(
                    channelCandidates,
                    () => this.GetDurableCandidates(RequestParameters.Of(
                            Schemas.Durable.DurableTyp, durableType,
                            Schemas.Durable.FrFabId, fabId)),
                    true);

            if (!outcome.IsCurrent)
            {
                return;
            }

            if (outcome.Failure != null)
            {
                this.candidateKey = null;
                return;
            }

            this.candidateData = outcome.Value;
            this.BindPickerLists(new string[] { string.Empty, string.Empty, string.Empty });
        }

        private void ClearCandidateText()
        {
            this.CancelDebounce(channelDurableIdFilter);
            this.CancelDebounce(channelLotIdFilter);
            this.CancelDebounce(channelSerialNoFilter);
            this.SetCandidateText(this.cboDurableId, string.Empty);
            this.SetCandidateText(this.cboLotId, string.Empty);
            this.SetCandidateText(this.cboSerialNo, string.Empty);
        }

        private void SetCandidateText(Modern.Lab.WinForms.Controls.Selection.ModernComboBox combo, string text)
        {
            this.candidateBinding = true;

            try
            {
                combo.SelectedIndex = -1;
                combo.Text = text;
            }
            finally
            {
                this.candidateBinding = false;
            }
        }

        private void BindCandidates(Modern.Lab.WinForms.Controls.Selection.ModernComboBox combo, DataTable items)
        {
            string typed = combo.Text;
            this.candidateBinding = true;

            try
            {
                combo.DataSource = items;
                combo.SelectedIndex = -1;
                combo.Text = typed;
            }
            finally
            {
                this.candidateBinding = false;
            }
        }

        private void OnResetClick(object sender, EventArgs e)
        {
            this.resettingCriteria = true;
            this.typeSetup = true;

            try
            {
                this.cboType.SelectedValue = Schemas.Durable.DurableTyp.Foup.Value;
                this.CancelDebounce(channelFabFilter);
                this.SetCandidateText(this.cboFab, string.Empty);
                this.ClearCandidateText();
            }
            finally
            {
                this.typeSetup = false;
                this.resettingCriteria = false;
            }

            this.ApplyFabEnabled();
            this.LoadCandidates();
            this.ExecuteSearch();
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            this.ExecuteSearch();
        }

        private string SelectedType()
        {
            string durableTyp = (Convert.ToString(this.cboType.SelectedValue) ?? string.Empty).Trim();

            return durableTyp.Length == 0 ? Schemas.Durable.DurableTyp.Foup.Value : durableTyp;
        }

        private string SelectedDurableId()
        {
            DataRowView durable = this.gridDurables.SelectedItem as DataRowView;

            return durable == null ? string.Empty : DurableManagementPresenter.Id(durable.Row);
        }

        private async void ExecuteSearch(bool silent = false, string focusDurableId = null, bool selectNewItems = true)
        {
            this.durableSearchVersion++;
            this.InvalidateChannel(channelEditContents);
            string durableType = this.SelectedType();
            string fabId = this.SelectedFabId();
            string durableId = this.KnownPickerValue(0);
            string lotId = this.KnownPickerValue(1);
            string reqSerialNo = this.KnownPickerValue(2);
            this.newItems.BeginQuery();
            string previousDurableId = this.SelectedDurableId();
            string keepDurableId = previousDurableId;
            int selectionVersion = this.durableSelectionVersion;

            IDisposable busy = silent ? null : this.Busy("Loading durables...", durableType);

            LoadOutcome<DataTable> outcome;

            try
            {
                outcome = await this.FetchAsync(
                        channelDurables,
                        () => this.GetDurables(RequestParameters.Of(
                                Schemas.Durable.DurableTyp, durableType,
                                Schemas.Durable.FrFabId, fabId,
                                Schemas.Durable.DurableId, durableId,
                                Schemas.Lot.LotId, lotId,
                                Schemas.Lot.ReqSerialNo, reqSerialNo)),
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
                this.RefreshActionStates();
                return;
            }

            this.newItemCriteria = NewItemTracker.Criteria(durableType, fabId, durableId, lotId, reqSerialNo);
            if (this.durableSelectionVersion != selectionVersion)
            {
                keepDurableId = this.SelectedDurableId();
                focusDurableId = null;
                selectNewItems = false;
            }
            this.ApplyTypeLayout(durableType);
            this.BindDurables(
                    outcome.Value, keepDurableId,
                    focusDurableId, silent, selectNewItems);

            if (!string.IsNullOrEmpty(focusDurableId) && this.SelectedDurableId() != focusDurableId)
            {
                this.ShowWarningMessage("Created durable " + focusDurableId + " is not visible in the current list. Check the filters or refresh.");
            }
        }

        private void PrepareUnitHistory(Modern.Lab.WinForms.Controls.Data.ModernDataGrid grid, string rowKey)
        {
            grid.RowKeyMember = rowKey;
            grid.RowForegroundMember = JudgeHistoryColors.ColorColumn;
            grid.CellLinkClick += this.OnReqSerialNoLinkClick;
        }

        private void BindDurables(DataTable durables, string keepDurableId, string createdId, bool silent, bool selectNewItems = true)
        {
            this.durableData = this.BindJudged(
                    DurableContracts.DurableTable, ref this.durableCurrent, durables, silent);

            bool judged = Judged(this.durableCurrent);
            this.durableData = this.newItems.Accept(this.durableData, Schemas.Durable.RowKey, this.newItemCriteria, judged);
            DataTable bound;

            if (!judged)
            {
                bound = this.durableData == null ? null : this.durableData.Copy();
                GridColumns.Of(bound).Apply(this.gridDurables);
            }
            else
            {
                bound = this.durableData;
                DurableColumns(this.durableData).Apply(this.gridDurables);
            }

            DurableFields(judged ? this.durableCurrent.Table : this.durableData).Apply(this.fieldInfo);

            string selectId = DurableManagementPresenter.SelectionAfterBind(this.durableData, keepDurableId);

            this.gridBinding = true;

            try
            {
                this.boundDurables = bound;
                this.gridDurables.DataSource = bound;

                if (selectId.Length > 0)
                {
                    this.SelectDurable(selectId);
                }
                else
                {
                    this.gridDurables.SelectedIndex = -1;
                }
                NewItemTint.Bind(this.gridDurables, this.newItems, Schemas.Durable.RowKey, createdId, selectNewItems);
            }
            finally
            {
                this.gridBinding = false;
            }

            this.durableCard.TitleRightText = this.durableData == null
                    ? string.Empty
                    : this.durableData.Rows.Count.ToString("N0") + " durables";

            this.ApplyDurableSelection(silent);
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
                    tableId, incoming, Contracts, DurableManagementPresenter.ScreenColumns);

            if (reception.IsMissingRequired)
            {
                current = null;
                this.ShowMissingColumns(reception.Response, silent);
                return null;
            }

            current = reception.Response;
            return current.Table;
        }

        private static bool Judged(TableResponse current)
        {
            return current != null;
        }

        private void SelectDurable(string durableId)
        {
            DataTable table = this.boundDurables ?? this.durableData;

            if (table == null || durableId.Length == 0)
            {
                return;
            }

            DataView view = table.DefaultView;

            for (int index = 0; index < view.Count; index++)
            {
                DataRow row = view[index].Row;

                if (DurableManagementPresenter.Id(row) == durableId)
                {
                    this.gridDurables.SelectedIndex = index;
                    return;
                }
            }

            this.gridDurables.SelectedIndex = -1;
        }

        private void OnDurableSelectionChanged(object sender, EventArgs e)
        {
            if (this.gridBinding)
            {
                return;
            }

            this.durableSelectionVersion++;
            this.ApplyDurableSelection(false);
        }

        private void ApplyDurableSelection(bool silent)
        {
            DataRowView durable = this.gridDurables.SelectedItem as DataRowView;

            if (durable == null)
            {
                this.fieldInfo.ClearValues();
                this.ClearDependents();
                this.RefreshActionStates();
                return;
            }

            this.fieldInfo.SetRow(durable.Row);

            bool loadable = Judged(this.durableCurrent) && DurableManagementPresenter.Id(durable.Row).Length > 0;

            if (loadable)
            {
                this.InvalidateDependents();
            }
            else
            {
                this.ClearDependents();
            }

            this.RefreshActionStates();

            if (!loadable)
            {
                return;
            }

            string durableId = DurableManagementPresenter.Id(durable.Row);

            this.pageHistory.Text = durableHistoryTitle + " — " + durableId;
            this.pageStubHistory.Text = stubHistoryTitle + " — " + durableId;
            this.pageLccHistory.Text = lccHistoryTitle + " — " + durableId;
            this.LoadDependents(durable.Row, silent);
        }

        private void InvalidateDependents()
        {
            this.InvalidateChannel(channelEditContents);
            this.InvalidateChannel(channelSlots);
            this.InvalidateChannel(channelDurableWafers);
            this.InvalidateChannel(channelDurableChips);
            this.InvalidateChannel(channelDurableLamellas);
            this.InvalidateChannel(channelDurableHistory);
            this.InvalidateChannel(channelDurableChipHistory);
            this.InvalidateChannel(channelDurableLamellaHistory);
        }

        private void ClearDependents()
        {
            this.contentCover.Reset();
            this.historyCover.Reset();
            this.InvalidateDependents();

            this.slotData = null;
            this.gridDurableWafers.DataSource = null;
            this.gridDurableChips.DataSource = null;
            this.gridDurableLamellas.DataSource = null;
            this.pageWaferList.BadgeCount = 0;
            this.pageChipList.BadgeCount = 0;
            this.pageLamellaList.BadgeCount = 0;
            this.durableHistoryData = null;
            this.gridDurableHistory.DataSource = null;
            this.durableChipHistoryData = null;
            this.gridDurableChipHistory.DataSource = null;
            this.durableLamellaHistoryData = null;
            this.gridDurableLamellaHistory.DataSource = null;
            this.pageHistory.Text = durableHistoryTitle;
            this.pageStubHistory.Text = stubHistoryTitle;
            this.pageLccHistory.Text = lccHistoryTitle;
        }

        private void LoadDependents(DataRow durable, bool silent)
        {
            string durableId = DurableManagementPresenter.Id(durable);
            this.LoadDependent(channelSlots, () => this.GetDurableSlots(durable), this.BindSlots, silent, this.contentCover);
            this.LoadDependent(channelDurableHistory, () => this.GetDurableHistory(durable),
                    table => this.BindDurableHistory(table, NewItemTracker.Criteria(durableId)), silent, this.historyCover);

            if (this.SelectedType() != Schemas.Durable.DurableTyp.Tray.Value)
            {
                this.LoadDependent(
                        channelDurableWafers,
                        () => this.GetDurableWafers(durable),
                        this.BindDurableWafers, silent, this.contentCover);
                return;
            }

            this.LoadDependent(
                    channelDurableChips,
                    () => this.GetDurableChips(durable),
                    this.BindDurableChips, silent, this.contentCover);
            this.LoadDependent(
                    channelDurableLamellas,
                    () => this.GetDurableLamellas(durable),
                    this.BindDurableLamellas, silent, this.contentCover);
            this.LoadDependent(
                    channelDurableChipHistory,
                    () => this.GetDurableChipHistory(durable),
                    table => this.BindDurableChipHistory(table, NewItemTracker.Criteria(durableId)), silent, this.historyCover);
            this.LoadDependent(
                    channelDurableLamellaHistory,
                    () => this.GetDurableLamellaHistory(durable),
                    table => this.BindDurableLamellaHistory(table, NewItemTracker.Criteria(durableId)), silent, this.historyCover);
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

        private void BindSlots(DataTable slots)
        {
            this.slotData = slots;
            this.RefreshActionStates();
        }

        private void BindDurableWafers(DataTable wafers)
        {
            this.BindContent(this.gridDurableWafers, this.pageWaferList, DurableWaferColumns(wafers), wafers);
        }

        private void BindDurableChips(DataTable chips)
        {
            this.BindContent(this.gridDurableChips, this.pageChipList, DurableChipColumns(chips), chips);
        }

        private void BindDurableLamellas(DataTable lamellas)
        {
            this.BindContent(this.gridDurableLamellas, this.pageLamellaList, DurableLamellaColumns(lamellas), lamellas);
        }

        private void BindContent(
                Modern.Lab.WinForms.Controls.Data.ModernDataGrid grid,
                Modern.Lab.WinForms.Controls.Layout.ModernTabPage page,
                GridColumns columns, DataTable table)
        {
            columns.Bind(grid);

            page.BadgeCount = table == null ? 0 : table.Rows.Count;
        }

        private void BindDurableHistory(DataTable history, string criteria)
        {
            this.durableHistoryData = this.durableHistoryNewItems.Accept(history, this.gridDurableHistory.RowKeyMember, criteria);

            DurableHistoryColumns(this.durableHistoryData).Bind(this.gridDurableHistory);
            NewItemTint.Bind(this.gridDurableHistory, this.durableHistoryNewItems, this.gridDurableHistory.RowKeyMember);
        }

        private void BindDurableChipHistory(DataTable history, string criteria)
        {
            this.durableChipHistoryData = DurableManagementPresenter.MarkJudgeColors(
                    this.durableChipHistoryNewItems.Accept(history, this.gridDurableChipHistory.RowKeyMember, criteria));
            this.BindUnitHistory(
                    this.gridDurableChipHistory, DurableChipHistoryColumns(this.durableChipHistoryData), this.durableChipHistoryNewItems);
        }

        private void BindDurableLamellaHistory(DataTable history, string criteria)
        {
            this.durableLamellaHistoryData = DurableManagementPresenter.MarkJudgeColors(
                    this.durableLamellaHistoryNewItems.Accept(history, this.gridDurableLamellaHistory.RowKeyMember, criteria));
            this.BindUnitHistory(
                    this.gridDurableLamellaHistory, DurableLamellaHistoryColumns(this.durableLamellaHistoryData), this.durableLamellaHistoryNewItems);
        }

        private void BindUnitHistory(
                Modern.Lab.WinForms.Controls.Data.ModernDataGrid grid, GridColumns columns, NewItemTracker tracker)
        {
            columns.Bind(grid);
            NewItemTint.Bind(grid, tracker, grid.RowKeyMember);
        }

        private void ConfigureActionDropDown(Modern.Lab.WinForms.Controls.Input.ModernDropDownButton dropDown)
        {
            dropDown.DisplayMember = DurableManagementPresenter.MenuLabelColumn;
            dropDown.ValueMember = DurableManagementPresenter.MenuValueColumn;
            dropDown.EnabledMember = DurableManagementPresenter.MenuEnabledColumn;
            dropDown.GlyphMember = DurableManagementPresenter.MenuGlyphColumn;
            dropDown.AccentTextMember = DurableManagementPresenter.MenuAccentColumn;
            dropDown.IconGlyph = MenuGlyphs.For(dropDown.Text);
        }

        private void RefreshActionStates()
        {
            DataRowView durable = this.gridDurables.SelectedItem as DataRowView;
            DataRow row = durable == null ? null : durable.Row;

            this.ddbRegister.DataSource = DurableManagementPresenter.BuildGatedMenuTable(
                    DurableManagementPresenter.RegisterActions, key => this.ActionEnabled(key, row));
            this.ddbRegister.Enabled = this.newItems.ActionsReady;
            this.RefreshActionDropDown(this.ddbHold, DurableManagementPresenter.HoldActions, row);
            this.RefreshActionDropDown(this.ddbMoveWafer, DurableManagementPresenter.MoveWaferActions, row);
            this.RefreshActionDropDown(this.ddbTransport, DurableManagementPresenter.TransportActions, row);
            this.RefreshActionDropDown(this.ddbScrap, DurableManagementPresenter.ScrapActions, row);

            this.lblTarget.Text = DurableManagementPresenter.StatusLineText(row);
        }

        private void RefreshActionDropDown(
                Modern.Lab.WinForms.Controls.Input.ModernDropDownButton dropDown, IList<DurableAction> actions, DataRow row)
        {
            dropDown.DataSource = DurableManagementPresenter.BuildGatedMenuTable(actions, key => this.ActionEnabled(key, row));
            dropDown.Enabled = this.newItems.ActionsReady && row != null;
        }

        private string ActionReason(string key, DataRow row)
        {
            return DurableActionReason(key, row, this.slotData, this.newItems.ActionsReady);
        }

        private bool ActionEnabled(string key, DataRow row)
        {
            return this.ActionReason(key, row) == null;
        }

        private void OnActionClicked(object sender, Modern.Lab.Controls.Wpf.Input.DropDownItemClickedEventArgs e)
        {
            this.ExecuteAction(e.Value as string);
        }

        private void ExecuteAction(string key)
        {
            DataRowView durable = this.gridDurables.SelectedItem as DataRowView;
            DataRow row = durable == null ? null : durable.Row;

            bool allowed = DurableManagementPresenter.IsActionKey(key) && this.ActionEnabled(key, row);

            if (!allowed)
            {
                return;
            }

            switch (key)
            {
                case DurableManagementPresenter.ActionCreate:
                    this.ShowCreateDialog();
                    break;

                case DurableManagementPresenter.ActionHold:
                    this.ShowHoldDialog(row, false);
                    break;

                case DurableManagementPresenter.ActionRelease:
                    this.ShowHoldDialog(row, true);
                    break;

                case DurableManagementPresenter.ActionAdjust:
                    this.ShowAdjustDialog(row);
                    break;

                case DurableManagementPresenter.ActionScrap:
                    this.ShowScrapDialog(row);
                    break;

                case DurableManagementPresenter.ActionScrapCancel:
                    this.ShowScrapCancelDialog(row);
                    break;

                case DurableManagementPresenter.ActionRfId:
                    this.ShowRfIdDialog(row);
                    break;

                case DurableManagementPresenter.ActionExchange:
                    this.OpenCarrierEditor(row, CarrierEditEntryMode.DurableExchange);
                    break;

                case DurableManagementPresenter.ActionEdit:
                    this.OpenCarrierEditor(row);
                    break;

                case DurableManagementPresenter.ActionAssign:
                    this.ShowAssignDialog(row);
                    break;

                case DurableManagementPresenter.ActionDeassign:
                    this.ShowDeassignDialog(row);
                    break;

                case DurableManagementPresenter.ActionManualIn:
                    this.ShowManualInDialog(row);
                    break;

                case DurableManagementPresenter.ActionManualOut:
                    this.ShowManualOutDialog(row);
                    break;

                case DurableManagementPresenter.ActionManualInCancel:
                    this.ShowManualCancelDialog(row, DurableManagementPresenter.ActionManualInCancel);
                    break;

                case DurableManagementPresenter.ActionManualOutCancel:
                    this.ShowManualCancelDialog(row, DurableManagementPresenter.ActionManualOutCancel);
                    break;
            }
        }

        private async void ShowCreateDialog()
        {
            LoadOutcome<DataTable> outcome = await this.FetchAsync(
                    channelCategories, () => this.GetSubCategories());

            if (!outcome.IsCurrent || outcome.Failure != null)
            {
                return;
            }

            if (this.typeData == null || outcome.Value == null)
            {
                this.ShowListUnavailable("Create Durable", "CreateDurable candidates");
                return;
            }

            this.categoryData = outcome.Value;

            DurableCreateDialogOptions options = new DurableCreateDialogOptions();
            options.Title = "Create Durable";
            options.OkText = "Create";
            options.Types = this.typeData;
            options.SelectedType = this.SelectedType();
            options.Categories = this.categoryData;

            this.OpenCreateDialog(options);
        }

        protected virtual void OpenCreateDialog(DurableCreateDialogOptions options)
        {
            using (DurableCreateDialogForm dialog = new DurableCreateDialogForm(options))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    this.ProcessCreate(dialog.DurableTyp, RequestParameters.Of(
                            Schemas.Durable.DurableTyp, dialog.DurableTyp,
                            Schemas.SubCategory.SubCatgCd, dialog.Category,
                            Schemas.Criteria.Description, dialog.Description));
                }
            }
        }

        protected virtual void ShowRfIdDialog(DataRow durable)
        {
            DurableRfIdDialogOptions options = new DurableRfIdDialogOptions();
            options.Title = "Assign RFID";
            options.OkText = "Assign";
            options.Source = ActionRows.Copy(durable);

            using (DurableRfIdDialogForm dialog = new DurableRfIdDialogForm(options))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    this.ProcessRfId(options.Source, RequestParameters.Of(
                            Schemas.Durable.RfId, dialog.RfId,
                            Schemas.Criteria.Description, dialog.Description));
                }
            }
        }

        protected virtual void ShowHoldDialog(DataRow durable, bool releaseMode)
        {
            DataRow source = ActionRows.Copy(durable);

            using (DurableHoldDialogForm dialog = new DurableHoldDialogForm(source, releaseMode))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                RequestParameters parameters = RequestParameters.Of(
                        Schemas.ReasonCode.ReasonCd, dialog.ReasonCode,
                        Schemas.User.UserId, dialog.EngrUserId,
                        Schemas.Criteria.Description, dialog.Description);

                if (releaseMode)
                {
                    this.ProcessRelease(source, parameters);
                }
                else
                {
                    this.ProcessHold(source, parameters);
                }
            }
        }

        protected virtual void ShowManualOutDialog(DataRow durable)
        {
            DurableScrapDialogOptions options = new DurableScrapDialogOptions();
            options.Title = "Manual Out — " + DurableManagementPresenter.Id(durable)
                    + " · " + Schemas.Durable.EqpId.Text(durable) + " " + Schemas.Durable.PortNm.Text(durable);
            options.OkText = "Manual Out";
            options.DefaultDescription = Services.DefaultDescriptions.ManualOutDurable;
            options.Fields = ManualOutFields();
            options.Source = ActionRows.Copy(durable);

            using (DurableScrapDialogForm dialog = new DurableScrapDialogForm(options))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                this.ProcessManualOut(options.Source, RequestParameters.Of(Schemas.Criteria.Description, dialog.Description));
            }
        }

        protected virtual void ShowManualCancelDialog(DataRow durable, string actionKey)
        {
            bool inCancel = actionKey == DurableManagementPresenter.ActionManualInCancel;
            string label = inCancel ? DurableManagementPresenter.ManualInCancelLabel : DurableManagementPresenter.ManualOutCancelLabel;
            DurableScrapDialogOptions options = new DurableScrapDialogOptions();
            options.Title = label + " — " + DurableManagementPresenter.Id(durable);
            options.OkText = label;
            options.DefaultDescription = inCancel ? Services.DefaultDescriptions.ManualInCancel : Services.DefaultDescriptions.ManualOutCancel;
            options.Fields = ManualOutFields();
            options.Source = ActionRows.Copy(durable);

            using (DurableScrapDialogForm dialog = new DurableScrapDialogForm(options))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                this.ProcessManualCancel(options.Source, actionKey, RequestParameters.Of(Schemas.Criteria.Description, dialog.Description));
            }
        }

        private static Modern.Lab.WinForms.Controls.Display.ModernFieldDefinition[] ManualOutFields()
        {
            Modern.Lab.WinForms.Controls.Display.ModernFieldDefinition[] source = DurableDialogHelpers.SourceFields();
            Modern.Lab.WinForms.Controls.Display.ModernFieldDefinition[] fields =
                    new Modern.Lab.WinForms.Controls.Display.ModernFieldDefinition[source.Length + 2];
            source.CopyTo(fields, 0);
            fields[source.Length] = Services.CardFields.Of(Schemas.Durable.EqpId);
            fields[source.Length + 1] = Services.CardFields.Of(Schemas.Durable.PortNm);
            return fields;
        }

        protected virtual void ShowScrapDialog(DataRow durable)
        {
            DurableScrapDialogOptions options = new DurableScrapDialogOptions();
            options.Title = "Scrap — " + DurableManagementPresenter.Id(durable);
            options.OkText = "Scrap";
            options.DefaultDescription = Services.DefaultDescriptions.ScrapDurable;
            options.Source = ActionRows.Copy(durable);

            using (DurableScrapDialogForm dialog = new DurableScrapDialogForm(options))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                this.ProcessScrap(options.Source, RequestParameters.Of(Schemas.Criteria.Description, dialog.Description));
            }
        }

        protected virtual void ShowScrapCancelDialog(DataRow durable)
        {
            DurableScrapDialogOptions options = new DurableScrapDialogOptions();
            options.Title = DurableManagementPresenter.ScrapCancelLabel + " — " + DurableManagementPresenter.Id(durable);
            options.OkText = DurableManagementPresenter.ScrapCancelLabel;
            options.DefaultDescription = Services.DefaultDescriptions.ScrapDurableCancel;
            options.Source = ActionRows.Copy(durable);

            using (DurableScrapDialogForm dialog = new DurableScrapDialogForm(options))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                this.ProcessScrapCancel(options.Source, RequestParameters.Of(Schemas.Criteria.Description, dialog.Description));
            }
        }

        protected virtual void ShowAdjustDialog(DataRow durable)
        {
            DurableAdjustDialogOptions options = new DurableAdjustDialogOptions();
            options.Title = "Adjust — " + DurableManagementPresenter.Id(durable);
            options.OkText = "Adjust";
            options.DefaultDescription = Services.DefaultDescriptions.AdjustDurable;
            options.Source = ActionRows.Copy(durable);

            using (DurableAdjustDialogForm dialog = new DurableAdjustDialogForm(options))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                this.ProcessAdjust(options.Source, RequestParameters.Of(Schemas.Criteria.Description, dialog.Description));
            }
        }

        protected virtual void ShowDeassignDialog(DataRow durable)
        {
            DurableDeassignDialogOptions options = new DurableDeassignDialogOptions();
            options.Title = "Deassign — " + DurableManagementPresenter.Id(durable);
            options.OkText = "Deassign";
            options.DefaultDescription = Services.DefaultDescriptions.DeassignDurable;
            options.Source = ActionRows.Copy(durable);
            options.Contents = this.slotData == null ? null : this.slotData.Copy();

            using (DurableDeassignDialogForm dialog = new DurableDeassignDialogForm(options))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                this.ProcessDeassign(options.Source, RequestParameters.Of(
                        Schemas.Criteria.LotIds, string.Join(",", dialog.LotIds ?? new string[0]),
                        Schemas.Criteria.Description, dialog.Description));
            }
        }

        protected virtual void ShowAssignDialog(DataRow durable)
        {
            DurableAssignDialogOptions options = new DurableAssignDialogOptions();
            options.Title = "Assign — " + DurableManagementPresenter.Id(durable);
            options.OkText = "Assign";
            options.DefaultDescription = Services.DefaultDescriptions.AssignDurable;
            options.Source = ActionRows.Copy(durable);
            options.Contents = this.slotData == null ? null : this.slotData.Copy();

            using (DurableAssignDialogForm dialog = new DurableAssignDialogForm(options))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                this.ProcessAssign(options.Source, RequestParameters.Of(
                        Schemas.Lot.LotId, dialog.LotId,
                        Schemas.Criteria.SlotNos, dialog.SlotNos,
                        Schemas.Criteria.WfIds, dialog.WfIds,
                        Schemas.Criteria.Description, dialog.Description));
            }
        }

        private DataTable TargetCandidates()
        {
            return this.durableData;
        }

        protected virtual void OpenCarrierEditor(DataRow durable)
        {
            this.OpenCarrierEditor(durable, CarrierEditEntryMode.DurableEdit);
        }

        protected virtual void OpenCarrierEditor(DataRow durable, CarrierEditEntryMode entryMode)
        {
            using (CarrierEditForm editor = new CarrierEditForm(
                    DurableManagementPresenter.Type(durable),
                    DurableManagementPresenter.Id(durable),
                    entryMode))
            {
                editor.ShowDialog(this);

                if (editor.SourceUnavailable || editor.DurableChanged)
                {
                    this.ExecuteSearch(true);
                }
            }
        }

        protected virtual void ShowManualInDialog(DataRow durable)
        {
            DurableManualInDialogOptions options = new DurableManualInDialogOptions();
            options.Title = "Manual In — " + DurableManagementPresenter.Id(durable);
            options.OkText = "Manual In";
            options.DefaultDescription = Services.DefaultDescriptions.ManualInDurable;
            options.Source = ActionRows.Copy(durable);
            options.Contents = this.slotData == null ? null : this.slotData.Copy();

            using (DurableManualInDialogForm dialog = new DurableManualInDialogForm(options))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                this.ProcessManualIn(options.Source, RequestParameters.Of(
                        Schemas.EquipmentExt.EqpId, dialog.EqpId,
                        Schemas.PortDet.PortNm, dialog.PortNm,
                        Schemas.Criteria.Description, dialog.Description));
            }
        }

        private DataRow DurableRow(string durableId)
        {
            if (this.durableData == null || string.IsNullOrEmpty(durableId))
            {
                return null;
            }

            foreach (DataRow row in this.durableData.Rows)
            {
                if (DurableManagementPresenter.Id(row) == durableId.Trim())
                {
                    return row;
                }
            }

            return null;
        }

        private void ShowListUnavailable(string title, string subject)
        {
            this.ShowErrorMessage(
                    title,
                    "The list needed for this action could not be used. Try again, or contact support.",
                    subject + " — the response has no table");
        }

        private void ProcessCreate(string durableType, RequestParameters parameters)
        {
            string reason = this.ActionReason(DurableManagementPresenter.ActionCreate, null);
            if (reason != null)
            {
                this.ShowWarningMessage(reason);
                return;
            }

            if (ResponseColumns.IsBlank(durableType) || ResponseColumns.IsBlank(parameters.Value(Schemas.SubCategory.SubCatgCd)))
            {
                return;
            }

            int searchVersion = this.durableSearchVersion;
            int selectionVersion = this.durableSelectionVersion;
            string selectedId = this.SelectedDurableId();
            this.Process(
                    () => this.CreateDurable(parameters),
                    "Creating " + parameters.Value(Schemas.SubCategory.SubCatgCd) + " durable…",
                    reply => this.RefreshCreatedDurable(reply, durableType, searchVersion, selectedId, selectionVersion));
        }

        private void RefreshCreatedDurable(DataActionResult reply, string durableType, int searchVersion, string selectedId, int selectionVersion)
        {
            if (this.durableSearchVersion != searchVersion || this.durableSelectionVersion != selectionVersion
                    || this.SelectedDurableId() != selectedId)
            {
                this.ExecuteSearch(true, null, false);
                return;
            }
            string[] values = new string[0];
            try
            {
                if (!string.IsNullOrWhiteSpace(reply.Text(ServerMessageFormat.FieldResultMessage)))
                {
                    values = reply.Values;
                }
            }
            catch (Exception)
            {
                this.ExecuteSearch(true);
                return;
            }
            string createdId = values.Length == 1 ? (values[0] ?? string.Empty).Trim() : string.Empty;
            if (createdId.Length == 0)
            {
                this.ExecuteSearch(true);
                return;
            }
            this.typeSetup = true;
            try { this.cboType.SelectedValue = durableType; }
            finally { this.typeSetup = false; }
            this.ExecuteSearch(true, createdId);
        }

        private void ProcessRfId(DataRow durable, RequestParameters parameters)
        {
            string durableId = DurableManagementPresenter.Id(durable);
            string reason = this.ActionReason(DurableManagementPresenter.ActionRfId, this.DurableRow(durableId));
            if (reason != null)
            {
                this.ShowWarningMessage(reason);
                return;
            }

            this.Process(
                    () => this.SetRfId(durable, parameters),
                    "Assigning RFID…",
                    () => this.ExecuteSearch(true));
        }

        private void ProcessHold(DataRow durable, RequestParameters parameters)
        {
            if (!this.StillAllows(DurableManagementPresenter.ActionHold, durable))
            {
                return;
            }

            this.Process(
                    () => this.HoldDurable(durable, parameters),
                    "Holding…",
                    () => this.ExecuteSearch(true));
        }

        private void ProcessRelease(DataRow durable, RequestParameters parameters)
        {
            if (!this.StillAllows(DurableManagementPresenter.ActionRelease, durable))
            {
                return;
            }

            this.Process(
                    () => this.ReleaseDurable(durable, parameters),
                    "Releasing…",
                    () => this.ExecuteSearch(true));
        }

        private void ProcessAdjust(DataRow durable, RequestParameters parameters)
        {
            if (!this.StillAllows(DurableManagementPresenter.ActionAdjust, durable))
            {
                return;
            }

            this.Process(
                    () => this.AdjustDurable(durable, parameters),
                    "Adjusting…",
                    () => this.ExecuteSearch(true));
        }

        private void ProcessScrap(DataRow durable, RequestParameters parameters)
        {
            if (!this.StillAllows(DurableManagementPresenter.ActionScrap, durable))
            {
                return;
            }

            this.Process(
                    () => this.ScrapDurable(durable, parameters),
                    "Scrapping…",
                    () => this.ExecuteSearch(true));
        }

        private void ProcessScrapCancel(DataRow durable, RequestParameters parameters)
        {
            if (!this.StillAllows(DurableManagementPresenter.ActionScrapCancel, durable))
            {
                return;
            }

            this.Process(
                    () => this.CancelScrapDurable(durable, parameters),
                    "Cancelling the scrap…",
                    () => this.ExecuteSearch(true));
        }

        private void ProcessDeassign(DataRow durable, RequestParameters parameters)
        {
            if (!this.StillAllows(DurableManagementPresenter.ActionDeassign, durable))
            {
                return;
            }

            this.Process(
                    () => this.DeassignDurable(durable, parameters),
                    "Deassigning…",
                    () => this.ExecuteSearch(true));
        }

        private void ProcessAssign(DataRow durable, RequestParameters parameters)
        {
            if (!this.StillAllows(DurableManagementPresenter.ActionAssign, durable))
            {
                return;
            }

            this.Process(
                    () => this.AssignDurable(durable, parameters),
                    "Assigning…",
                    () => this.ExecuteSearch(true));
        }

        private bool StillAllows(string key, DataRow durable)
        {
            string reason = this.ActionReason(key, this.DurableRow(DurableManagementPresenter.Id(durable)));

            if (reason != null)
            {
                this.ShowWarningMessage(reason);
                return false;
            }

            return true;
        }

        private void ProcessManualIn(DataRow durable, RequestParameters parameters)
        {
            if (!this.StillAllows(DurableManagementPresenter.ActionManualIn, durable))
            {
                return;
            }

            string eqpId = parameters.Value(Schemas.EquipmentExt.EqpId);
            string portNm = parameters.Value(Schemas.PortDet.PortNm);

            if (ResponseColumns.IsBlank(eqpId) || ResponseColumns.IsBlank(portNm))
            {
                return;
            }

            this.Process(
                    () => this.ManualInDurable(durable, parameters),
                    "Manual In " + DurableManagementPresenter.Id(durable) + " → " + eqpId + " " + portNm + "…",
                    () => this.ExecuteSearch(true));
        }

        private void ProcessManualOut(DataRow durable, RequestParameters parameters)
        {
            if (!this.StillAllows(DurableManagementPresenter.ActionManualOut, durable))
            {
                return;
            }

            string eqpId = Schemas.Durable.EqpId.Text(durable);
            string portNm = Schemas.Durable.PortNm.Text(durable);

            if (ResponseColumns.IsBlank(eqpId) || ResponseColumns.IsBlank(portNm))
            {
                return;
            }

            this.Process(
                    () => this.ManualOutDurable(durable, parameters),
                    "Manual Out " + DurableManagementPresenter.Id(durable) + " ← " + eqpId + " " + portNm + "…",
                    () => this.ExecuteSearch(true));
        }

        private void ProcessManualCancel(DataRow durable, string actionKey, RequestParameters parameters)
        {
            if (!this.StillAllows(actionKey, durable))
            {
                return;
            }

            bool inCancel = actionKey == DurableManagementPresenter.ActionManualInCancel;

            this.Process(
                    () => inCancel ? this.ManualInCancelDurable(durable, parameters) : this.ManualOutCancelDurable(durable, parameters),
                    (inCancel ? DurableManagementPresenter.ManualInCancelLabel : DurableManagementPresenter.ManualOutCancelLabel)
                            + " " + DurableManagementPresenter.Id(durable) + "…",
                    () => this.ExecuteSearch(true));
        }

        private void Process(Func<DataActionResult> call, string busyText, Action<DataActionResult> refresh)
        {
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

        private void Process(Func<DataActionResult> call, string busyText, Action refresh)
        {
            if (!this.newItems.ActionsReady)
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
                        refresh();
                    },
                    busyText);
        }

        private void PopulateMenus()
        {
            this.menuDurable.Items.Clear();
            this.AppendMenuGroup(this.ddbRegister.Text, DurableManagementPresenter.RegisterActions);
            this.AppendMenuGroup(this.ddbHold.Text, DurableManagementPresenter.HoldActions);
            this.AppendMenuGroup(this.ddbMoveWafer.Text, DurableManagementPresenter.MoveWaferActions);
            this.AppendMenuGroup(this.ddbTransport.Text, DurableManagementPresenter.TransportActions);
            this.AppendMenuGroup(this.ddbScrap.Text, DurableManagementPresenter.ScrapActions);
        }

        private void AppendMenuGroup(string groupText, IList<DurableAction> actions)
        {
            ToolStripMenuItem group = MenuGlyphs.CreateItem(groupText);
            group.DropDown.Renderer = this.menuDurable.Renderer;

            foreach (DurableAction action in actions)
            {
                group.DropDownItems.Add(this.CreateMenuItem(action));
            }

            this.menuDurable.Items.Add(group);
        }

        private ToolStripMenuItem CreateMenuItem(DurableAction action)
        {
            ToolStripMenuItem item = MenuGlyphs.CreateItem(action.Label);
            item.Tag = action.Key;
            item.Click += this.OnDurableMenuItemClick;
            return item;
        }

        private void OnMenuDurableOpening(object sender, CancelEventArgs e)
        {
            DataRowView durable = this.gridDurables.SelectedItem as DataRowView;

            if (durable == null)
            {
                e.Cancel = true;
                return;
            }

            foreach (object entry in this.menuDurable.Items)
            {
                ToolStripMenuItem item = entry as ToolStripMenuItem;

                if (item == null)
                {
                    continue;
                }

                this.ApplyMenuItemReason(item, durable.Row);

                foreach (object child in item.DropDownItems)
                {
                    this.ApplyMenuItemReason(child as ToolStripMenuItem, durable.Row);
                }
            }
        }

        private void ApplyMenuItemReason(ToolStripMenuItem item, DataRow row)
        {
            string key = item == null ? null : item.Tag as string;

            if (key != null && DurableManagementPresenter.IsActionKey(key))
            {
                ApplyActionReason(item, this.ActionReason(key, row));
            }
        }

        private void OnDurableMenuItemClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;

            if (item != null)
            {
                this.ExecuteAction(item.Tag as string);
            }
        }
    }
}
