using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Data;
using Modern.Lab.Samples.Contracts;
using Modern.Lab.Samples.Services;

using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{

    public partial class CarrierEditForm : ModernFormBase
    {
        private DataTable sourceData;
        private DataTable targetData;

        private bool loadingLists;

        private readonly string initialDurableTyp;
        private readonly string initialSourceId;
        private readonly CarrierEditEntryMode entryMode;

        private const string channelCarriers = "carriers";
        private const string channelSourceMap = "sourceMap";
        private const string channelTargetMap = "targetMap";

        private DataTable stagedWafers;

        private bool carrierListLoading;

        private string loadedType = string.Empty;

        private string loadedSourceId = string.Empty;

        private string loadedTargetId = string.Empty;

        private static string LotColor(string lotId)
        {
            return string.IsNullOrEmpty(lotId)
                    ? string.Empty
                    : Modern.Lab.Theming.Palette.FromValue(lotId);
        }

        public CarrierEditForm()
                : this(string.Empty, string.Empty, CarrierEditEntryMode.Standalone)
        {
        }

        public CarrierEditForm(string initialDurableTyp, string initialSourceId)
                : this(initialDurableTyp, initialSourceId, CarrierEditEntryMode.DurableEdit)
        {
        }

        public CarrierEditForm(string initialDurableTyp, string initialSourceId, CarrierEditEntryMode entryMode)
        {
            this.initialDurableTyp = (initialDurableTyp ?? string.Empty).Trim();
            this.initialSourceId = (initialSourceId ?? string.Empty).Trim();
            this.entryMode = entryMode;
            this.InitializeComponent();
            this.moveMenu.Renderer = new Modern.Lab.WinForms.Rendering.ModernMenuRenderer();

            this.cboType.Enabled = !this.SourceLocked;
            this.cboSource.Enabled = !this.SourceLocked;
            this.cboTarget.Enabled = true;

            if (this.IsExchange)
            {
                this.ApplyExchangeChrome();
            }

            this.InitializeModernForm(this.midPanel, false);
            this.RegisterRefreshShortcut(this.btnRefresh);
        }

        private void ApplyExchangeChrome()
        {
            this.Text = "Carrier Exchange";
        }

        private bool SourceLocked
        {
            get { return this.entryMode != CarrierEditEntryMode.Standalone; }
        }

        private bool IsExchange
        {
            get { return this.entryMode == CarrierEditEntryMode.DurableExchange; }
        }

        private bool ExchangeNeedsFoup
        {
            get
            {
                return this.IsExchange && ExchangeTypeReason(this.initialDurableTyp).Length > 0;
            }
        }

        internal bool SourceUnavailable { get; private set; }

        internal bool ExchangeUnavailable { get; private set; }

        internal bool DurableChanged { get; private set; }

        private void OnFormLoad(object sender, EventArgs e)
        {
            if (this.ExchangeNeedsFoup)
            {
                this.ExchangeUnavailable = true;
                this.ShowMessage(
                        ModernMessageKind.Warning,
                        "Carrier Exchange",
                        ExchangeTypeReason(this.initialDurableTyp));
                this.Close();
                return;
            }

            DataTable typeTable = new DataTable();
            typeTable.Columns.Add("VALUE", typeof(string));
            typeTable.Rows.Add(Schemas.Durable.DurableTyp.Foup.Value);
            typeTable.Rows.Add(Schemas.Durable.DurableTyp.Tray.Value);

            this.cboSource.ItemColorPath = CarrierEditPresenter.StatColorColumn;
            this.cboTarget.ItemColorPath = CarrierEditPresenter.StatColorColumn;

            this.loadingLists = true;

            try
            {
                this.cboType.DisplayMember = "VALUE";
                this.cboType.ValueMember = "VALUE";
                this.cboType.DataSource = typeTable;

                if (this.initialDurableTyp == Schemas.Durable.DurableTyp.Foup.Value
                        || this.initialDurableTyp == Schemas.Durable.DurableTyp.Tray.Value)
                {
                    this.cboType.SelectedValue = this.initialDurableTyp;
                }
            }
            finally
            {
                this.loadingLists = false;
            }

            if (this.SelectedDurableTyp().Length == 0)
            {
                this.ReleaseLoadCover();
                return;
            }

            string sourceId = this.SelectedDurableTyp() == this.initialDurableTyp
                    ? this.initialSourceId
                    : string.Empty;
            this.ReloadCarrierLists(sourceId, null);
        }

        private string SelectedDurableTyp()
        {
            string value = this.cboType.SelectedValue as string;
            return value ?? string.Empty;
        }

        private void OnTypeChanged(object sender, EventArgs e)
        {
            if (this.loadingLists || this.SelectedDurableTyp().Length == 0)
            {
                return;
            }

            if (string.Equals(this.SelectedDurableTyp(), this.loadedType, StringComparison.Ordinal))
            {
                return;
            }

            this.DiscardCarrierState();
            this.ReloadCarrierLists(null, null);
        }

        private void DiscardCarrierState()
        {
            this.InvalidateChannel(channelSourceMap);
            this.InvalidateChannel(channelTargetMap);
            this.ResetActionContractState();

            this.sourceData = null;
            this.targetData = null;

            this.loadingLists = true;

            try
            {
                this.cboSource.DataSource = null;
                this.cboTarget.DataSource = null;
            }
            finally
            {
                this.loadingLists = false;
            }

            this.mapSource.SetSections(null);
            this.mapTarget.SetSections(null);

            this.sourceCard.Text = BuildCardTitle("Source", string.Empty);
            this.targetCard.Text = BuildCardTitle("Target", string.Empty);
            this.sourceCard.TitleRightText = string.Empty;
            this.targetCard.TitleRightText = string.Empty;

            ApplyOccupancy(this.barSource, null);
            ApplyOccupancy(this.barTarget, null);
            this.UpdateItemBadge(this.badgeSourceLot, null);
            this.UpdateItemBadge(this.badgeTargetLot, null);
            this.PositionTitleBadge(this.sourceCard, this.badgeSourceLot);
            this.PositionTitleBadge(this.targetCard, this.badgeTargetLot);

            this.ClearPreview();
        }

        private void OnSourceChanged(object sender, EventArgs e)
        {
            if (this.loadingLists)
            {
                return;
            }

            string selected = this.cboSource.SelectedValue as string ?? string.Empty;

            if (string.Equals(selected, this.loadedSourceId, StringComparison.Ordinal))
            {
                return;
            }

            this.ReloadCarrierLists(selected, null);
        }

        private void OnTargetChanged(object sender, EventArgs e)
        {
            if (this.loadingLists)
            {
                return;
            }

            string selected = this.cboTarget.SelectedValue as string ?? string.Empty;

            if (string.Equals(selected, this.loadedTargetId, StringComparison.Ordinal))
            {
                return;
            }

            this.LoadTargetMap();
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            this.ReloadCarrierLists(
                    this.SourceLocked ? this.initialSourceId : this.cboSource.SelectedValue as string,
                    this.cboTarget.SelectedValue as string);
        }

        private void ReloadCarrierLists(string keepSourceId, string keepTargetId)
        {
            this.ReloadCarrierLists(keepSourceId, keepTargetId, null);
        }

        private async void ReloadCarrierLists(string keepSourceId, string keepTargetId, MethodInvoker onComplete)
        {
            string durableTyp = this.SelectedDurableTyp();

            if (durableTyp.Length == 0)
            {
                this.ReleaseLoadCover();
                return;
            }

            if (this.carrierListLoading)
            {
                return;
            }

            this.carrierListLoading = true;
            this.loadedType = durableTyp;

            this.DiscardCarrierState();
            this.BeginCarrierListContractLoad(durableTyp);

            LoadOutcome<DataTable> outcome = await this.FetchAsync(
                    channelCarriers,
                    new Func<DataTable>(delegate { return GetCarriers(durableTyp); }));

            this.carrierListLoading = false;

            if (!outcome.IsCurrent)
            {
                return;
            }

            if (outcome.Failure != null)
            {
                this.SetCarrierFailure(durableTyp, outcome.Failure);
                this.ReleaseLoadCover();
                this.UpdateActionStates();
                return;
            }

            this.ApplyCarrierLists(outcome.Value, durableTyp, keepSourceId, keepTargetId, onComplete);
        }

        private static string TokenColorText(string brushKey, System.Drawing.Color fallback)
        {
            return System.Drawing.ColorTranslator.ToHtml(Modern.Lab.Theming.ModernTokenColors.Get(brushKey, fallback));
        }

        private void ApplyCarrierLists(
                DataTable carriers, string durableTyp, string keepSourceId, string keepTargetId,
                MethodInvoker onComplete)
        {
            carriers = this.BindCarrierResponse(carriers, durableTyp);
            carriers.Columns.Add(CarrierEditPresenter.LabelColumn, typeof(string));
            carriers.Columns.Add(CarrierEditPresenter.StatColorColumn, typeof(string));

            string emptyColor = TokenColorText("Brush.TextSecondary", System.Drawing.Color.FromArgb(107, 114, 128));
            string fullColor = TokenColorText("Brush.ErrorBorder", System.Drawing.Color.FromArgb(220, 38, 38));
            string nearFullColor = TokenColorText("Brush.Warning", System.Drawing.Color.FromArgb(217, 119, 6));
            string roomColor = TokenColorText("Brush.Success", System.Drawing.Color.FromArgb(22, 163, 74));

            foreach (DataRow row in carriers.Rows)
            {
                string durableId = TableHelper.CellText(row, Schemas.Durable.DurableId.Name);
                int lotQty = TableHelper.ParseInt(TableHelper.CellText(row, Schemas.Durable.LotQty.Name));
                int capa = TableHelper.ParseInt(TableHelper.CellText(row, Schemas.Durable.Capa.Name));

                if (durableTyp == Schemas.Durable.DurableTyp.Foup.Value)
                {
                    int foupLotQty = TableHelper.ParseInt(TableHelper.CellText(row, Schemas.Durable.FoupLotQty.Name));
                    int foupCapa = TableHelper.ParseInt(TableHelper.CellText(row, Schemas.Durable.FoupCapa.Name));
                    row[CarrierEditPresenter.LabelColumn] = durableId + " · "
                            + WithCapacity(foupLotQty, foupCapa);
                }
                else
                {
                    int stub = TableHelper.ParseInt(TableHelper.CellText(row, Schemas.Durable.StubLotQty.Name));
                    int lcc = TableHelper.ParseInt(TableHelper.CellText(row, Schemas.Durable.LccLotQty.Name));
                    int stubCapa = TableHelper.ParseInt(TableHelper.CellText(row, Schemas.Durable.StubCapa.Name));
                    int lccCapa = TableHelper.ParseInt(TableHelper.CellText(row, Schemas.Durable.LccCapa.Name));
                    row[CarrierEditPresenter.LabelColumn] = durableId
                            + " · " + Schemas.DurableLamella.SubProdTyp.Chip.Value + " "
                            + WithCapacity(stub, stubCapa)
                            + " · " + Schemas.DurableLamella.SubProdTyp.Lamella.Value + " "
                            + WithCapacity(lcc, lccCapa);
                }

                double ratio = capa > 0 ? (double)lotQty / capa : 0d;

                if (lotQty == 0)
                {
                    row[CarrierEditPresenter.StatColorColumn] = emptyColor;
                }
                else if (lotQty >= capa)
                {
                    row[CarrierEditPresenter.StatColorColumn] = fullColor;
                }
                else if (ratio >= 0.75d)
                {
                    row[CarrierEditPresenter.StatColorColumn] = nearFullColor;
                }
                else
                {
                    row[CarrierEditPresenter.StatColorColumn] = roomColor;
                }
            }

            this.loadingLists = true;

            try
            {
                this.cboSource.DisplayMember = CarrierEditPresenter.LabelColumn;
                this.cboSource.ValueMember = Schemas.Durable.ValueField.Name;
                DataTable sources = carriers.Clone();
                foreach (DataRow row in carriers.Rows)
                {
                    if (CarrierEditPresenter.IsSourceCandidate(row))
                    {
                        sources.ImportRow(row);
                    }
                }

                this.cboSource.DataSource = sources;

                if (!string.IsNullOrEmpty(keepSourceId))
                {
                    this.cboSource.SelectedValue = keepSourceId;
                }

                DataRowView selectedSource = this.cboSource.SelectedItem as DataRowView;
                string sourceId = selectedSource == null
                        ? string.Empty
                        : TableHelper.CellText(selectedSource.Row, Schemas.Durable.DurableId.Name);

                this.loadedSourceId = sourceId;

                if (this.SourceLocked
                        && !string.Equals(sourceId, this.initialSourceId, StringComparison.Ordinal))
                {
                    this.cboSource.SelectedValue = null;
                    this.cboTarget.DataSource = null;
                    this.SourceUnavailable = true;
                }
                else
                {
                    DataTable targets = CarrierEditPresenter.BuildTargetCandidates(
                            carriers,
                            sourceId,
                            this.IsExchange
                                    ? (selectedSource == null ? null : selectedSource.Row)
                                    : null);
                    this.RemoveOtherFabTargets(targets);

                    this.cboTarget.DisplayMember = CarrierEditPresenter.LabelColumn;
                    this.cboTarget.ValueMember = Schemas.Durable.ValueField.Name;
                    this.cboTarget.DataSource = targets;
                    this.loadedTargetId = this.cboTarget.SelectedValue as string ?? string.Empty;

                    if (!string.IsNullOrEmpty(keepTargetId))
                    {
                        this.cboTarget.SelectedValue = keepTargetId;
                        this.loadedTargetId = this.cboTarget.SelectedValue as string ?? string.Empty;
                    }
                }
            }
            finally
            {
                this.loadingLists = false;
            }

            if (this.SourceUnavailable)
            {
                this.ReleaseLoadCover();
                this.UpdateActionStates();
                this.ShowMessage(
                        ModernMessageKind.Warning,
                        "Carrier Edit",
                        "Durable Id " + this.initialSourceId
                                + " is no longer available. Refresh Durable Management and try again.");
                this.Close();
                return;
            }

            this.LoadMaps(new MethodInvoker(delegate
            {
                this.ClearPreview();

                if (onComplete != null)
                {
                    onComplete();
                }
            }));
        }

        private MethodInvoker pendingMapsDone;
        private bool sourceMapSettled;
        private bool targetMapSettled;

        private void LoadMaps(MethodInvoker onBothApplied)
        {
            this.sourceMapSettled = false;
            this.targetMapSettled = false;
            this.pendingMapsDone = new MethodInvoker(delegate
            {
                this.ReleaseLoadCover();

                if (onBothApplied != null)
                {
                    onBothApplied();
                }
            });

            this.LoadSourceMap();
            this.LoadTargetMap();
        }

        private void OnMapSettled(bool sourceSide, bool current)
        {
            if (!current)
            {
                return;
            }

            if (sourceSide)
            {
                this.sourceMapSettled = true;
            }
            else
            {
                this.targetMapSettled = true;
            }

            if (this.pendingMapsDone != null && this.sourceMapSettled && this.targetMapSettled)
            {
                MethodInvoker done = this.pendingMapsDone;
                this.pendingMapsDone = null;
                done();
            }

            this.RebuildExchangeStaging();
        }

        private void LoadSourceMap()
        {
            this.LoadSourceMap(null);
        }

        private async void LoadSourceMap(MethodInvoker onApplied)
        {
            string durableTyp = this.SelectedDurableTyp();
            string carrierId = this.cboSource.SelectedValue as string ?? string.Empty;

            this.loadedSourceId = carrierId;
            this.ClearSourceMapState();
            this.BeginSourceMapContractLoad(durableTyp, carrierId);

            LoadOutcome<DataTable> outcome = await this.FetchAsync(
                    channelSourceMap,
                    new Func<DataTable>(delegate { return GetDurableWafers(carrierId); }));

            if (!outcome.IsCurrent)
            {
                this.OnMapSettled(true, false);
                return;
            }

            if (outcome.Failure != null)
            {
                this.SetMapFailure(CarrierEditContracts.SourceMapTable, durableTyp, carrierId, outcome.Failure);
                this.UpdateActionStates();
                this.OnMapSettled(true, true);
                return;
            }

            this.sourceData = this.BindMapResponse(
                    CarrierEditContracts.SourceMapTable, outcome.Value, durableTyp, carrierId, this.cboSource);
            this.mapSource.SetSections(BuildSections(this.sourceData, durableTyp));

            this.sourceCard.Text = BuildCardTitle("Source", carrierId);
            this.sourceCard.TitleRightText = BuildFilledText(this.cboSource, durableTyp);
            ApplyOccupancy(this.barSource, this.cboSource);
            this.UpdateItemBadge(this.badgeSourceLot, this.sourceData);
            this.PositionTitleBadge(this.sourceCard, this.badgeSourceLot);
            this.UpdateActionStates();
            this.OnMapSettled(true, true);

            if (onApplied != null)
            {
                onApplied();
            }
        }

        private void LoadTargetMap()
        {
            this.LoadTargetMap(null);
        }

        private async void LoadTargetMap(MethodInvoker onApplied)
        {
            string durableTyp = this.SelectedDurableTyp();
            string carrierId = this.cboTarget.SelectedValue as string ?? string.Empty;

            this.loadedTargetId = carrierId;
            this.ClearTargetMapState();
            this.BeginTargetMapContractLoad(durableTyp, carrierId);

            LoadOutcome<DataTable> outcome = await this.FetchAsync(
                    channelTargetMap,
                    new Func<DataTable>(delegate { return GetDurableWafers(carrierId); }));

            if (!outcome.IsCurrent)
            {
                this.OnMapSettled(false, false);
                return;
            }

            if (outcome.Failure != null)
            {
                this.SetMapFailure(CarrierEditContracts.TargetMapTable, durableTyp, carrierId, outcome.Failure);
                this.UpdateActionStates();
                this.OnMapSettled(false, true);
                return;
            }

            this.targetData = this.BindMapResponse(
                    CarrierEditContracts.TargetMapTable, outcome.Value, durableTyp, carrierId, this.cboTarget);
            this.mapTarget.SetSections(BuildSections(this.targetData, durableTyp));

            this.targetCard.Text = BuildCardTitle("Target", carrierId);
            this.targetCard.TitleRightText = BuildFilledText(this.cboTarget, durableTyp);
            ApplyOccupancy(this.barTarget, this.cboTarget);
            this.UpdateItemBadge(this.badgeTargetLot, this.targetData);
            this.PositionTitleBadge(this.targetCard, this.badgeTargetLot);
            this.UpdateActionStates();
            this.OnMapSettled(false, true);

            if (onApplied != null)
            {
                onApplied();
            }
        }

        private static string ExchangeStatusText(bool sourceHasWafers, int targetFilled, int stagedCount)
        {
            if (!sourceHasWafers)
            {
                return "Source carrier is empty — nothing to exchange.";
            }

            if (targetFilled > 0)
            {
                return "Target carrier is no longer empty — pick an empty carrier.";
            }

            if (stagedCount == 0)
            {
                return "Pick an empty target carrier to stage the whole source.";
            }

            return stagedCount.ToString("N0") + " item(s) staged — the whole source moves as one.";
        }

        private void ClearSourceMapState()
        {
            this.sourceData = null;
            this.ResetSourceMapContractState();
            this.mapSource.SetSections(null);
            this.sourceCard.Text = BuildCardTitle("Source", string.Empty);
            this.sourceCard.TitleRightText = string.Empty;
            ApplyOccupancy(this.barSource, null);
            this.UpdateItemBadge(this.badgeSourceLot, null);
            this.ClearPreview();
        }

        private void ClearTargetMapState()
        {
            this.targetData = null;
            this.ResetTargetMapContractState();
            this.mapTarget.SetSections(null);
            this.targetCard.Text = BuildCardTitle("Target", string.Empty);
            this.targetCard.TitleRightText = string.Empty;
            ApplyOccupancy(this.barTarget, null);
            this.UpdateItemBadge(this.badgeTargetLot, null);
            this.ClearPreview();
        }

        private void UpdateActionStates()
        {
            int targetFilled = CarrierEditPresenter.CountFilled(this.targetData);
            int targetCap = this.targetData != null ? this.targetData.Rows.Count : 0;
            bool sourceHasWafers = CarrierEditPresenter.CountFilled(this.sourceData) > 0;
            bool hasUnstagedSourceWafers = this.HasUnstagedSourceWafers();
            bool selectedUnstaged = this.HasSelectedSourceKeys(false);
            bool selectedTargetPreview = this.selectedTargetPreviewKeys.Count > 0;
            bool anyStaged = this.stagedKeys.Count > 0;
            bool hasStagedWafers = this.stagedWafers != null && this.stagedWafers.Rows.Count > 0;
            int stagedCount = hasStagedWafers ? this.stagedWafers.Rows.Count : 0;
            string moveReason = this.CarrierActionReason(CarrierEditPresenter.ActionMove);
            bool moveAllowed = moveReason.Length == 0;

            bool selRight = !this.IsExchange && selectedUnstaged;
            bool allRight = !this.IsExchange && sourceHasWafers && hasUnstagedSourceWafers;
            bool selLeft = !this.IsExchange && selectedTargetPreview;
            bool allLeft = !this.IsExchange && anyStaged;

            this.btnSelRight.Enabled = selRight;
            this.btnAllRight.Enabled = allRight;
            this.btnSelLeft.Enabled = selLeft;
            this.btnAllLeft.Enabled = allLeft;

            this.miMoveSelRight.Enabled = selRight;
            this.miMoveAllRight.Enabled = allRight;
            this.miMoveSelLeft.Enabled = selLeft;
            this.miMoveAllLeft.Enabled = allLeft;

            this.ApplyActionReason(this.btnMove, moveAllowed ? null : moveReason);

            this.targetCard.TitleAccent = anyStaged;
            this.UpdateTransferPlanText(stagedCount);
            this.btnMove.Kind = this.btnMove.Enabled
                    ? Modern.Lab.Controls.Wpf.Input.ButtonKind.Primary
                    : Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnMove.Text = this.IsExchange ? "Exchange" : "Move";

            if (!moveAllowed)
            {
                this.lblActionStatus.Text = moveReason;
            }
            else if (this.stageCapacityReason.Length > 0)
            {
                this.lblActionStatus.Text = this.stageCapacityReason;
            }
            else if (this.IsExchange)
            {
                this.lblActionStatus.Text = ExchangeStatusText(sourceHasWafers, targetFilled, stagedCount);
            }
            else if (selectedTargetPreview)
            {
                this.lblActionStatus.Text = this.selectedTargetPreviewKeys.Count.ToString("N0")
                        + " target previews selected · Press ‹ to return.";
            }
            else
            {
                this.lblActionStatus.Text = stagedCount.ToString("N0") + " wafers staged · Target has "
                        + (targetCap - targetFilled).ToString("N0") + " open positions.";
            }
        }

        private bool HasUnstagedSourceWafers()
        {
            if (this.sourceData == null)
            {
                return false;
            }

            foreach (DataRow row in this.sourceData.Rows)
            {
                string waferId = TableHelper.CellText(row, Schemas.DurableWafer.WfId.Name);

                if (waferId.Length == 0)
                {
                    continue;
                }

                string key = TableHelper.CellText(row, Schemas.DurableWafer.SubProdTyp.Name)
                        + "|" + TableHelper.CellText(row, Schemas.DurableWafer.SlotNo.Name);

                if (!this.stagedKeys.Contains(key))
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdateTransferPlanText(int stagedCount)
        {
            if (this.SelectedDurableTyp() != Schemas.Durable.DurableTyp.Tray.Value)
            {
                this.lblTransfer.Text = "PLAN";
                this.lblTransferHint.Text = stagedCount > 0
                        ? stagedCount.ToString("N0") + " staged"
                        : "stage wafers";
                return;
            }

            int stubCount = 0;
            int lccCount = 0;

            if (this.stagedWafers != null)
            {
                foreach (DataRow row in this.stagedWafers.Rows)
                {
                    string subProdTyp = TableHelper.CellText(row, Schemas.DurableWafer.SubProdTyp.Name);

                    if (subProdTyp == Schemas.DurableLamella.SubProdTyp.Chip.Value)
                    {
                        stubCount++;
                    }
                    else if (subProdTyp == Schemas.DurableLamella.SubProdTyp.Lamella.Value)
                    {
                        lccCount++;
                    }
                }
            }

            this.lblTransfer.Text = "Chip / Lamella";
            this.lblTransferHint.Text = stubCount.ToString("N0") + " / "
                    + lccCount.ToString("N0") + " staged";
        }

        private static string BuildCardTitle(string role, string carrierId)
        {
            if (carrierId.Length == 0)
            {
                return role;
            }

            return role + " — " + carrierId;
        }

        private static string WithCapacity(int filled, int capacity)
        {
            return capacity > 0
                    ? filled.ToString("N0") + "/" + capacity.ToString("N0")
                    : filled.ToString("N0");
        }

        private static string BuildFilledText(
                Modern.Lab.WinForms.Controls.Selection.ModernComboBox combo,
                string durableTyp)
        {
            DataRowView selected = combo == null ? null : combo.SelectedItem as DataRowView;

            if (selected == null)
            {
                return string.Empty;
            }

            if (durableTyp == Schemas.Durable.DurableTyp.Foup.Value)
            {
                return WithCapacity(
                        TableHelper.ParseInt(TableHelper.CellText(
                                selected.Row, Schemas.Durable.FoupLotQty.Name)),
                        TableHelper.ParseInt(TableHelper.CellText(
                                selected.Row, Schemas.Durable.FoupCapa.Name)));
            }

            return Schemas.DurableLamella.SubProdTyp.Chip.Value + " "
                    + WithCapacity(
                            TableHelper.ParseInt(TableHelper.CellText(
                                    selected.Row, Schemas.Durable.StubLotQty.Name)),
                            TableHelper.ParseInt(TableHelper.CellText(
                                    selected.Row, Schemas.Durable.StubCapa.Name)))
                    + "   ·   " + Schemas.DurableLamella.SubProdTyp.Lamella.Value + " "
                    + WithCapacity(
                            TableHelper.ParseInt(TableHelper.CellText(
                                    selected.Row, Schemas.Durable.LccLotQty.Name)),
                            TableHelper.ParseInt(TableHelper.CellText(
                                    selected.Row, Schemas.Durable.LccCapa.Name)));
        }

        private static void ApplyOccupancy(
                Modern.Lab.WinForms.Controls.Display.ModernProgressBar bar,
                Modern.Lab.WinForms.Controls.Selection.ModernComboBox combo)
        {
            DataRowView selected = combo == null ? null : combo.SelectedItem as DataRowView;
            int total = selected == null
                    ? 0
                    : TableHelper.ParseInt(TableHelper.CellText(
                            selected.Row, Schemas.Durable.Capa.Name));
            int used = selected == null
                    ? 0
                    : TableHelper.ParseInt(TableHelper.CellText(
                            selected.Row, Schemas.Durable.LotQty.Name));

            bar.Minimum = 0;
            bar.Maximum = total;
            bar.Value = used;
        }

        private void PositionTitleBadge(
                Modern.Lab.WinForms.Controls.Layout.ModernGroupBox card,
                Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badge)
        {
            string title = card.Text ?? string.Empty;
            int x = 12;

            using (System.Drawing.Graphics g = this.CreateGraphics())
            using (System.Drawing.Font font =
                    new System.Drawing.Font("Segoe UI Semibold", card.TitleFontSize))
            {
                double ratio = Modern.Lab.Theming.ModernTheme.ResolveFontWidthRatio(card.FontWidthRatio);
                float width = (float)(g.MeasureString(title, font).Width * ratio);
                x = 12 + (int)System.Math.Ceiling(width) + 8;
            }

            badge.Location = new System.Drawing.Point(x, 4);
        }

        private void UpdateItemBadge(
                Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badge, DataTable wafers)
        {
            if (wafers == null)
            {
                badge.Text = string.Empty;
                badge.Color = string.Empty;
                badge.Visible = false;
                this.tipLots.SetToolTip(badge, null);
                return;
            }

            badge.Visible = true;

            List<string> lots = DistinctLotIds(wafers);

            if (lots.Count == 0)
            {
                badge.Text = "Empty";
                badge.Color = string.Empty;
            }
            else if (lots.Count == 1)
            {
                badge.Text = lots[0];
                badge.Color = LotColor(lots[0]);
            }
            else
            {
                badge.Text = lots[0] + " +" + (lots.Count - 1);
                badge.Color = LotColor(lots[0]);
            }

            this.tipLots.SetToolTip(
                    badge,
                    lots.Count > 1 ? string.Join(", ", lots.ToArray()) : null);
        }

        private static List<string> DistinctLotIds(DataTable wafers)
        {
            List<string> lots = new List<string>();

            if (wafers == null)
            {
                return lots;
            }

            foreach (DataRow row in wafers.Rows)
            {
                string lotId = TableHelper.CellText(row, Schemas.DurableWafer.LotId.Name).Trim();

                if (lotId.Length > 0 && !lots.Contains(lotId))
                {
                    lots.Add(lotId);
                }
            }

            return lots;
        }

        private SlotMapSection[] BuildSections(DataTable wafers, string durableTyp)
        {
            if (durableTyp == Schemas.Durable.DurableTyp.Foup.Value)
            {
                SlotMapSection slots = new SlotMapSection();
                slots.Title = "Slots";
                slots.Columns = 1;
                slots.Kind = SlotMapSectionKind.WaferEdge;

                foreach (DataRow row in wafers.Rows)
                {
                    SlotMapCell cell = new SlotMapCell();
                    cell.Key = Schemas.DurableWafer.SubProdTyp.Wafer.Value + "|"
                            + TableHelper.CellText(row, Schemas.DurableWafer.SlotNo.Name);
                    cell.Label = TableHelper.CellText(row, Schemas.DurableWafer.SlotNo.Name);
                    cell.WaferId = TableHelper.CellText(row, Schemas.DurableWafer.WfId.Name);

                    string lotId = TableHelper.CellText(row, Schemas.DurableWafer.LotId.Name);

                    if (lotId.Length > 0)
                    {
                        cell.Color = LotColor(lotId);
                        cell.ToolTip = cell.WaferId + " — " + lotId;
                    }

                    slots.Cells.Add(cell);
                }

                return new SlotMapSection[] { slots };
            }

            SlotMapSection stubs = new SlotMapSection();
            stubs.Title = Schemas.DurableLamella.SubProdTyp.Chip.Value;
            stubs.Columns = 3;
            stubs.CellFontSize = 12d;
            stubs.Kind = SlotMapSectionKind.PinStub;

            SlotMapSection lccs = new SlotMapSection();
            lccs.Title = Schemas.DurableLamella.SubProdTyp.Lamella.Value;
            lccs.Columns = 5;
            lccs.Kind = SlotMapSectionKind.LamellaPost;

            SlotMapCell current = null;

            foreach (DataRow row in wafers.Rows)
            {
                string subProdTyp = TableHelper.CellText(row, Schemas.DurableWafer.SubProdTyp.Name);
                string pos = TableHelper.CellText(row, Schemas.DurableWafer.SlotNo.Name);
                string lotId = TableHelper.CellText(row, Schemas.DurableWafer.LotId.Name);

                if (subProdTyp == Schemas.DurableLamella.SubProdTyp.Chip.Value)
                {
                    SlotMapCell stub = new SlotMapCell();
                    stub.Key = Schemas.DurableLamella.SubProdTyp.Chip.Value + "|" + pos;
                    stub.Label = pos;
                    stub.WaferId = TableHelper.CellText(row, Schemas.DurableWafer.WfId.Name);

                    if (lotId.Length > 0)
                    {
                        stub.Color = LotColor(lotId);
                        stub.ToolTip = stub.WaferId + " — " + lotId;
                    }

                    stubs.Cells.Add(stub);
                    continue;
                }

                if (current == null || current.Key != Schemas.DurableLamella.SubProdTyp.Lamella.Value + "|" + pos)
                {
                    current = new SlotMapCell();
                    current.Key = Schemas.DurableLamella.SubProdTyp.Lamella.Value + "|" + pos;
                    current.Label = pos;
                    current.SubCells = new System.Collections.Generic.List<SlotMapSubCell>();
                    lccs.Cells.Add(current);
                }

                SlotMapSubCell finger = new SlotMapSubCell();
                finger.Name = TableHelper.CellText(row, Schemas.DurableLamella.FingerId.Name);
                finger.WaferId = TableHelper.CellText(row, Schemas.DurableWafer.WfId.Name);
                finger.Marker = TableHelper.CellText(row, Schemas.DurableLamella.FingerIndex.Name);

                if (lotId.Length > 0)
                {
                    finger.Color = LotColor(lotId);
                    finger.Detail = lotId;
                }

                current.SubCells.Add(finger);
            }

            return new SlotMapSection[] { stubs, lccs };
        }

        private static int ReadCapa(
                Modern.Lab.WinForms.Controls.Selection.ModernComboBox combo, string column)
        {
            DataRowView row = combo.SelectedItem as DataRowView;
            return row == null ? 0 : TableHelper.ParseInt(TableHelper.CellText(row.Row, column));
        }

        private DataTable NormalizeWafers(
                DataTable raw, string durableTyp, Modern.Lab.WinForms.Controls.Selection.ModernComboBox combo)
        {
            bool foup = durableTyp == Schemas.Durable.DurableTyp.Foup.Value;

            return SlotMapWaferTable.Normalize(
                    raw,
                    ReadCapa(combo, Schemas.Durable.FoupCapa.Name),
                    ReadCapa(combo, Schemas.Durable.StubCapa.Name),
                    ReadCapa(combo, Schemas.Durable.LccCapa.Name),
                    foup ? SlotMapWaferTable.DefaultSlotCount : 0,
                    foup ? 0 : SlotMapWaferTable.DefaultStubCount,
                    foup ? 0 : SlotMapWaferTable.DefaultLccCount);
        }

        private readonly List<string> stagedKeys = new List<string>();
        private readonly List<string> selectedSourceKeys = new List<string>();
        private readonly List<string> selectedTargetPreviewKeys = new List<string>();
        private readonly Dictionary<string, string> previewSourceKeyByTargetKey =
                new Dictionary<string, string>(StringComparer.Ordinal);
        private string stageCapacityReason = string.Empty;

        private void OnSourceCellClicked(object sender, SlotMapCellEventArgs e)
        {
            if (this.IsExchange)
            {
                return;
            }

            if (!this.SourceKeyHasWafer(e.Key))
            {
                return;
            }

            if (this.stagedKeys.Contains(e.Key))
            {
                return;
            }

            this.stageCapacityReason = string.Empty;
            this.selectedTargetPreviewKeys.Clear();

            if (this.selectedSourceKeys.Contains(e.Key))
            {
                this.selectedSourceKeys.Remove(e.Key);
            }
            else
            {
                this.selectedSourceKeys.Add(e.Key);
            }

            this.RenderSelections();
        }

        private bool SourceKeyHasWafer(string key)
        {
            if (this.sourceData == null || string.IsNullOrEmpty(key))
            {
                return false;
            }

            foreach (DataRow row in this.sourceData.Rows)
            {
                string rowKey = TableHelper.CellText(row, Schemas.DurableWafer.SubProdTyp.Name)
                        + "|" + TableHelper.CellText(row, Schemas.DurableWafer.SlotNo.Name);

                if (string.Equals(rowKey, key, StringComparison.Ordinal)
                        && TableHelper.CellText(row, Schemas.DurableWafer.WfId.Name).Trim().Length > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasSelectedSourceKeys(bool staged)
        {
            foreach (string key in this.selectedSourceKeys)
            {
                if (this.stagedKeys.Contains(key) == staged)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnTargetCellClicked(object sender, SlotMapCellEventArgs e)
        {
            if (this.IsExchange)
            {
                return;
            }

            if (!this.previewSourceKeyByTargetKey.ContainsKey(e.Key))
            {
                return;
            }

            this.stageCapacityReason = string.Empty;
            this.selectedSourceKeys.Clear();

            if (this.selectedTargetPreviewKeys.Contains(e.Key))
            {
                this.selectedTargetPreviewKeys.Remove(e.Key);
            }
            else
            {
                this.selectedTargetPreviewKeys.Add(e.Key);
            }

            this.RenderSelections();
        }

        private void OnSourceCellRightClick(object sender, SlotMapCellEventArgs e)
        {
            if (this.SourceKeyHasWafer(e.Key))
            {
                this.stageCapacityReason = string.Empty;
                this.selectedTargetPreviewKeys.Clear();
                this.selectedSourceKeys.Clear();
                this.selectedSourceKeys.Add(e.Key);
                this.RenderSelections();
            }

            this.moveMenu.Show(System.Windows.Forms.Cursor.Position);
        }

        private void OnTargetCellRightClick(object sender, SlotMapCellEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Key))
            {
                this.OnTargetCellClicked(sender, e);
            }

            this.moveMenu.Show(System.Windows.Forms.Cursor.Position);
        }

        private void RebuildStagedPreview()
        {
            if (this.stagedKeys.Count == 0)
            {
                this.stagedWafers = null;
                this.previewSourceKeyByTargetKey.Clear();
                this.selectedTargetPreviewKeys.Clear();
                this.mapTarget.SetPreview(null);
                this.mapTarget.SetPreviewMarkers(null);
                return;
            }

            this.stagedWafers = this.CollectStagedWafers();

            Dictionary<string, string> preview = this.BuildLocalPreview(this.stagedWafers);
            this.mapTarget.SetPreview(preview);
            this.mapTarget.SetPreviewMarkers(this.BuildPreviewMarkers(this.stagedWafers, preview));

            for (int index = this.selectedTargetPreviewKeys.Count - 1; index >= 0; index--)
            {
                if (!this.previewSourceKeyByTargetKey.ContainsKey(this.selectedTargetPreviewKeys[index]))
                {
                    this.selectedTargetPreviewKeys.RemoveAt(index);
                }
            }
        }

        private Dictionary<string, string> BuildMirrorPreview(
                DataTable staged, Dictionary<string, string> preview)
        {
            foreach (DataRow row in staged.Rows)
            {
                string waferId = TableHelper.CellText(row, Schemas.DurableWafer.WfId.Name).Trim();

                if (waferId.Length == 0)
                {
                    continue;
                }

                string subProdTyp = TableHelper.CellText(row, Schemas.DurableWafer.SubProdTyp.Name);
                string position = TableHelper.CellText(row, Schemas.DurableWafer.SlotNo.Name);
                string key = subProdTyp + "|" + position;

                if (subProdTyp == Schemas.DurableLamella.SubProdTyp.Lamella.Value)
                {
                    key = key + "|" + TableHelper.CellText(row, Schemas.DurableLamella.FingerId.Name);
                }

                preview[key] = waferId;
                this.previewSourceKeyByTargetKey[key] = subProdTyp + "|" + position;
            }

            return preview;
        }

        private void RebuildExchangeStaging()
        {
            if (!this.IsExchange)
            {
                return;
            }

            List<string> proposed = this.CollectFilledSourceKeys();
            this.stageCapacityReason = string.Empty;

            if (proposed.Count > 0)
            {
                if (this.targetData == null || this.TargetId().Length == 0)
                {
                    proposed.Clear();
                }
                else
                {
                    this.stageCapacityReason = CarrierEditPresenter.StagingCapacityReason(
                            this.SelectedDurableTyp(), this.targetData, this.CollectWafers(proposed));

                    if (this.stageCapacityReason.Length > 0)
                    {
                        proposed.Clear();
                    }
                }
            }

            if (SameKeys(this.stagedKeys, proposed))
            {
                return;
            }

            this.stagedKeys.Clear();
            this.stagedKeys.AddRange(proposed);
            this.selectedSourceKeys.Clear();
            this.selectedTargetPreviewKeys.Clear();
            this.RebuildStagedPreview();
            this.RenderSelections();
        }

        private List<string> CollectFilledSourceKeys()
        {
            List<string> keys = new List<string>();

            if (this.sourceData == null)
            {
                return keys;
            }

            foreach (DataRow row in this.sourceData.Rows)
            {
                if (TableHelper.CellText(row, Schemas.DurableWafer.WfId.Name).Trim().Length == 0)
                {
                    continue;
                }

                string key = TableHelper.CellText(row, Schemas.DurableWafer.SubProdTyp.Name)
                        + "|" + TableHelper.CellText(row, Schemas.DurableWafer.SlotNo.Name);

                if (!keys.Contains(key))
                {
                    keys.Add(key);
                }
            }

            return keys;
        }

        private static bool SameKeys(List<string> first, List<string> second)
        {
            if (first.Count != second.Count)
            {
                return false;
            }

            foreach (string key in first)
            {
                if (!second.Contains(key))
                {
                    return false;
                }
            }

            return true;
        }

        private Dictionary<string, string> BuildLocalPreview(DataTable staged)
        {
            Dictionary<string, string> preview =
                    new Dictionary<string, string>(StringComparer.Ordinal);
            this.previewSourceKeyByTargetKey.Clear();

            if (staged == null || staged.Rows.Count == 0
                    || this.targetData == null || this.TargetId().Length == 0)
            {
                return preview;
            }

            if (this.IsExchange)
            {
                return this.BuildMirrorPreview(staged, preview);
            }

            List<string> emptySlots = new List<string>();
            List<string> emptyStubs = new List<string>();
            List<string> lccOrder = new List<string>();
            Dictionary<string, bool> lccEmpty =
                    new Dictionary<string, bool>(StringComparer.Ordinal);

            foreach (DataRow row in this.targetData.Rows)
            {
                string subProdTyp = TableHelper.CellText(row, Schemas.DurableWafer.SubProdTyp.Name);
                string pos = TableHelper.CellText(row, Schemas.DurableWafer.SlotNo.Name);
                bool filled = TableHelper.CellText(row, Schemas.DurableWafer.WfId.Name).Trim().Length > 0;

                if (subProdTyp == Schemas.DurableLamella.SubProdTyp.Lamella.Value)
                {
                    if (!lccEmpty.ContainsKey(pos))
                    {
                        lccEmpty[pos] = true;
                        lccOrder.Add(pos);
                    }

                    if (filled)
                    {
                        lccEmpty[pos] = false;
                    }
                }
                else if (!filled)
                {
                    if (subProdTyp == Schemas.DurableLamella.SubProdTyp.Chip.Value)
                    {
                        emptyStubs.Add(pos);
                    }
                    else
                    {
                        emptySlots.Add(pos);
                    }
                }
            }

            List<string> emptyLccs = new List<string>();

            foreach (string pos in lccOrder)
            {
                if (lccEmpty[pos])
                {
                    emptyLccs.Add(pos);
                }
            }

            int slotNext = 0;
            int stubNext = 0;
            int lccNext = 0;
            Dictionary<string, string> lccAssigned =
                    new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (DataRow row in staged.Rows)
            {
                string subProdTyp = TableHelper.CellText(row, Schemas.DurableWafer.SubProdTyp.Name);
                string waferId = TableHelper.CellText(row, Schemas.DurableWafer.WfId.Name).Trim();
                string sourcePosition = TableHelper.CellText(row, Schemas.DurableWafer.SlotNo.Name);
                string sourceKey = subProdTyp + "|" + sourcePosition;

                if (waferId.Length == 0)
                {
                    continue;
                }

                if (subProdTyp == Schemas.DurableLamella.SubProdTyp.Chip.Value)
                {
                    if (stubNext < emptyStubs.Count)
                    {
                        string targetKey = Schemas.DurableLamella.SubProdTyp.Chip.Value + "|" + emptyStubs[stubNext];
                        preview[targetKey] = waferId;
                        this.previewSourceKeyByTargetKey[targetKey] = sourceKey;
                        stubNext = stubNext + 1;
                    }
                }
                else if (subProdTyp == Schemas.DurableLamella.SubProdTyp.Lamella.Value)
                {
                    string tgtPos;

                    if (!lccAssigned.TryGetValue(sourcePosition, out tgtPos))
                    {
                        if (lccNext >= emptyLccs.Count)
                        {
                            continue;
                        }

                        tgtPos = emptyLccs[lccNext];
                        lccNext = lccNext + 1;
                        lccAssigned[sourcePosition] = tgtPos;
                    }

                    preview[Schemas.DurableLamella.SubProdTyp.Lamella.Value + "|" + tgtPos + "|"
                            + TableHelper.CellText(row, Schemas.DurableLamella.FingerId.Name)] = waferId;
                    this.previewSourceKeyByTargetKey[
                            Schemas.DurableLamella.SubProdTyp.Lamella.Value + "|" + tgtPos] = sourceKey;
                }
                else
                {
                    if (slotNext < emptySlots.Count)
                    {
                        string targetKey = Schemas.DurableWafer.SubProdTyp.Wafer.Value + "|" + emptySlots[slotNext];
                        preview[targetKey] = waferId;
                        this.previewSourceKeyByTargetKey[targetKey] = sourceKey;
                        slotNext = slotNext + 1;
                    }
                }
            }

            return preview;
        }

        private System.Collections.Generic.Dictionary<string, string> BuildPreviewMarkers(
                DataTable wafers, System.Collections.Generic.Dictionary<string, string> preview)
        {
            System.Collections.Generic.Dictionary<string, string> markers =
                    new System.Collections.Generic.Dictionary<string, string>(StringComparer.Ordinal);
            System.Collections.Generic.Dictionary<string, string> markerByWaferId =
                    new System.Collections.Generic.Dictionary<string, string>(StringComparer.Ordinal);

            if (wafers == null || preview == null)
            {
                return markers;
            }

            foreach (DataRow row in wafers.Rows)
            {
                string waferId = TableHelper.CellText(row, Schemas.DurableWafer.WfId.Name);
                string marker = TableHelper.CellText(row, Schemas.DurableLamella.FingerIndex.Name);

                if (waferId.Length > 0 && marker.Length > 0)
                {
                    markerByWaferId[waferId] = marker;
                }
            }

            foreach (System.Collections.Generic.KeyValuePair<string, string> entry in preview)
            {
                string marker;

                if (markerByWaferId.TryGetValue(entry.Value, out marker))
                {
                    markers[entry.Key] = marker;
                }
            }

            return markers;
        }

        private void RenderSelections()
        {
            string[] staged = new string[this.stagedKeys.Count];
            this.stagedKeys.CopyTo(staged);
            string[] selected = new string[this.selectedSourceKeys.Count];
            this.selectedSourceKeys.CopyTo(selected);
            string[] selectedTarget = new string[this.selectedTargetPreviewKeys.Count];
            this.selectedTargetPreviewKeys.CopyTo(selectedTarget);
            this.mapSource.SetSelectedKeys(staged);
            this.mapTarget.SetSelectedKeys(new string[0]);

            this.mapSource.SetClickKeys(selected);
            this.mapTarget.SetClickKeys(selectedTarget);

            this.UpdateActionStates();
        }

        private void ClearPreview()
        {
            this.stageCapacityReason = string.Empty;
            this.stagedKeys.Clear();
            this.selectedSourceKeys.Clear();
            this.selectedTargetPreviewKeys.Clear();
            this.previewSourceKeyByTargetKey.Clear();
            this.stagedWafers = null;
            this.mapSource.SetSelectedKeys(new string[0]);
            this.mapTarget.SetSelectedKeys(new string[0]);
            this.mapSource.SetPreview(null);
            this.mapTarget.SetPreview(null);
            this.mapTarget.SetPreviewMarkers(null);
            this.mapSource.SetClickKeys(new string[0]);
            this.mapTarget.SetClickKeys(new string[0]);
            this.UpdateActionStates();
        }

        private DataTable CollectStagedWafers()
        {
            return this.CollectWafers(this.stagedKeys);
        }

        private DataTable CollectWafers(IEnumerable<string> keys)
        {
            DataTable selected = this.sourceData.Clone();

            Dictionary<string, List<DataRow>> byKey =
                    new Dictionary<string, List<DataRow>>(StringComparer.Ordinal);

            foreach (DataRow row in this.sourceData.Rows)
            {
                if (TableHelper.CellText(row, Schemas.DurableWafer.WfId.Name).Trim().Length == 0)
                {
                    continue;
                }

                string key = TableHelper.CellText(row, Schemas.DurableWafer.SubProdTyp.Name)
                        + "|" + TableHelper.CellText(row, Schemas.DurableWafer.SlotNo.Name);
                List<DataRow> rows;

                if (!byKey.TryGetValue(key, out rows))
                {
                    rows = new List<DataRow>();
                    byKey.Add(key, rows);
                }

                rows.Add(row);
            }

            foreach (string key in keys)
            {
                List<DataRow> rows;

                if (!byKey.TryGetValue(key, out rows))
                {
                    continue;
                }

                foreach (DataRow row in rows)
                {
                    selected.ImportRow(row);
                }
            }

            return selected;
        }

        private DataTable CollectAllWafers(DataTable data)
        {
            DataTable selected = data.Clone();

            foreach (DataRow row in data.Rows)
            {
                if (TableHelper.CellText(row, Schemas.DurableWafer.WfId.Name).Trim().Length > 0)
                {
                    selected.ImportRow(row);
                }
            }

            return selected;
        }

        private void OnMoveAllRight(object sender, EventArgs e)
        {
            if (this.IsExchange)
            {
                return;
            }

            if (this.CollectAllWafers(this.sourceData).Rows.Count == 0)
            {
                this.ShowWarningMessage("Source carrier is empty.");
                return;
            }

            List<string> proposedKeys = new List<string>();

            foreach (DataRow row in this.sourceData.Rows)
            {
                if (TableHelper.CellText(row, Schemas.DurableWafer.WfId.Name).Trim().Length > 0)
                {
                    string key = TableHelper.CellText(row, Schemas.DurableWafer.SubProdTyp.Name)
                            + "|" + TableHelper.CellText(row, Schemas.DurableWafer.SlotNo.Name);

                    if (!proposedKeys.Contains(key))
                    {
                        proposedKeys.Add(key);
                    }
                }
            }

            if (!this.CanApplyStaging(proposedKeys))
            {
                return;
            }

            this.stagedKeys.Clear();
            this.stagedKeys.AddRange(proposedKeys);
            this.selectedSourceKeys.Clear();
            this.selectedTargetPreviewKeys.Clear();

            this.RebuildStagedPreview();
            this.RenderSelections();
        }

        private void OnMoveSelRight(object sender, EventArgs e)
        {
            if (this.IsExchange)
            {
                return;
            }

            if (!this.HasSelectedSourceKeys(false))
            {
                this.ShowWarningMessage("Select one or more source wafers first.");
                return;
            }

            List<string> proposedKeys = new List<string>(this.stagedKeys);

            foreach (string key in this.selectedSourceKeys)
            {
                if (!proposedKeys.Contains(key))
                {
                    proposedKeys.Add(key);
                }
            }

            if (!this.CanApplyStaging(proposedKeys))
            {
                return;
            }

            this.stagedKeys.Clear();
            this.stagedKeys.AddRange(proposedKeys);

            this.selectedSourceKeys.Clear();
            this.selectedTargetPreviewKeys.Clear();
            this.RebuildStagedPreview();
            this.RenderSelections();
        }

        private bool CanApplyStaging(IList<string> proposedKeys)
        {
            this.stageCapacityReason = string.Empty;

            if (this.TargetId().Length == 0 || this.targetData == null)
            {
                return true;
            }

            DataTable proposed = this.CollectWafers(proposedKeys);
            this.stageCapacityReason = CarrierEditPresenter.StagingCapacityReason(
                    this.SelectedDurableTyp(), this.targetData, proposed);

            if (this.stageCapacityReason.Length == 0)
            {
                return true;
            }

            this.UpdateActionStates();
            this.ShowMessage(
                    ModernMessageKind.Warning,
                    "Carrier Editor — Move",
                    this.stageCapacityReason);
            return false;
        }

        private void OnMoveSelLeft(object sender, EventArgs e)
        {
            if (this.IsExchange)
            {
                return;
            }

            if (this.selectedTargetPreviewKeys.Count == 0)
            {
                this.ShowWarningMessage("Select one or more target previews first.");
                return;
            }

            this.stageCapacityReason = string.Empty;

            foreach (string targetKey in this.selectedTargetPreviewKeys)
            {
                string sourceKey;

                if (this.previewSourceKeyByTargetKey.TryGetValue(targetKey, out sourceKey))
                {
                    this.stagedKeys.Remove(sourceKey);
                }
            }

            this.selectedSourceKeys.Clear();
            this.selectedTargetPreviewKeys.Clear();
            this.RebuildStagedPreview();
            this.RenderSelections();
        }

        private void OnMoveAllLeft(object sender, EventArgs e)
        {
            if (this.IsExchange)
            {
                return;
            }

            this.ClearPreview();
        }

        private void OnMoveClick(object sender, EventArgs e)
        {
            if (!this.EnsureCarrierAction(CarrierEditPresenter.ActionMove))
            {
                return;
            }

            string exchangeLotId = string.Empty;

            if (this.IsExchange && !this.TryExchangeLotId(this.stagedWafers, out exchangeLotId))
            {
                return;
            }

            DataTable plan = CarrierEditPresenter.BuildMovePlan(this.stagedWafers, this.previewSourceKeyByTargetKey);

            if (!this.IsExchange && !CarrierEditPresenter.PlanIsComplete(plan))
            {
                this.ShowWarningMessage("The target positions are not ready. Preview the move again.");
                return;
            }

            DataRow source = ActionRows.Copy(SelectedCarrier(this.cboSource));
            DataRow target = ActionRows.Copy(SelectedCarrier(this.cboTarget));

            if (!this.ConfirmCommit(CarrierEditPresenter.ActionMove, plan, source, target))
            {
                return;
            }

            DataTable wafers = this.stagedWafers == null ? new DataTable() : this.stagedWafers.Copy();

            if (!this.CanExecuteCarrierAction(CarrierEditPresenter.ActionMove, wafers))
            {
                this.ShowWarningMessage(this.CarrierActionReasonFor(CarrierEditPresenter.ActionMove, wafers));
                return;
            }

            this.stagedWafers = null;
            this.MoveBetween(source, target, wafers, plan, "Nothing to move.");
        }

        private bool TryExchangeLotId(DataTable wafers, out string lotId)
        {
            List<string> lots = DistinctLotIds(wafers);

            if (lots.Count == 1)
            {
                lotId = lots[0];
                return true;
            }

            lotId = string.Empty;

            this.ShowWarningMessage(
                    lots.Count == 0
                            ? "The wafers to exchange have no lot. Exchange was not sent."
                            : "The wafers to exchange belong to " + lots.Count.ToString("N0") + " lots ("
                                    + string.Join(", ", lots.ToArray())
                                    + "). Exchange sends one lot, so it was not sent. Contact support to confirm which lot to exchange.");
            return false;
        }

        private bool ConfirmCommit(string action, DataTable plan, DataRow source, DataRow target)
        {
            string verb = this.IsExchange && action == CarrierEditPresenter.ActionMove
                    ? "Exchange"
                    : action;

            return this.ConfirmMovePlan(verb, plan, source, target);
        }

        protected virtual bool ConfirmMovePlan(string verb, DataTable plan, DataRow source, DataRow target)
        {
            using (CarrierMoveDialogForm dialog = new CarrierMoveDialogForm(
                    verb, this.SelectedDurableTyp(), source, target, plan))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return false;
                }

                this.moveDescription = dialog.Description ?? string.Empty;
                return true;
            }
        }

        private bool EnsureCarrierAction(string action)
        {
            if (this.CanExecuteCarrierAction(action))
            {
                return true;
            }

            this.ShowWarningMessage(this.CarrierActionReason(action));
            return false;
        }

        private string moveDescription = string.Empty;

        private string Description()
        {
            return this.moveDescription;
        }

        private string SourceId()
        {
            return this.cboSource.SelectedValue as string ?? string.Empty;
        }

        private string TargetId()
        {
            return this.cboTarget.SelectedValue as string ?? string.Empty;
        }

        private void MoveBetween(
                DataRow source, DataRow target, DataTable wafers, DataTable plan, string emptyMessage)
        {
            string fromId = TableHelper.CellText(source, Schemas.Durable.DurableId.Name).Trim();
            string toId = TableHelper.CellText(target, Schemas.Durable.DurableId.Name).Trim();
            if (wafers.Rows.Count == 0 || plan == null || plan.Rows.Count == 0)
            {
                this.ShowWarningMessage(emptyMessage);
                return;
            }

            if (fromId.Length == 0 || toId.Length == 0)
            {
                this.ShowWarningMessage("Select both carriers first.");
                return;
            }

            string durableTyp = this.SelectedDurableTyp();

            string description = this.Description();

            this.RunAction(
                    delegate { return MoveWafers(durableTyp, source, target, wafers, plan, description); },
                    delegate(DataActionResult reply)
                    {
                        if (this.SourceLocked)
                        {
                            this.DurableChanged = true;
                        }

                        this.ShowToast(
                                reply.Count.ToString("N0") + " wafers moved to " + toId + ".",
                                ToastKind.Success);

                        if (this.SourceLocked)
                        {
                            this.Close();
                            return;
                        }

                        this.ReloadCarrierLists(this.SourceId(), this.TargetId());
                    },
                    this.HandleCarrierActionRejected,
                    "Moving…");
        }

        private void HandleCarrierActionRejected(DataActionResult reply)
        {
            string sourceId = this.SourceId();
            string targetId = this.TargetId();

            this.mapSource.ClearSelection();
            this.mapTarget.ClearSelection();
            this.ClearPreview();

            if (reply != null && reply.CommunicationFailure)
            {
                this.DiscardCarrierState();
                this.ReloadCarrierLists(sourceId, targetId);
            }

            this.ShowWarningMessage(
                    reply == null || reply.Message.Length == 0
                            ? "Carrier action failed."
                            : reply.Message);
        }

        private DataTable GetCarriers(string durableTyp)
        {
            DataTable received = this.GetDurables(durableTyp);

            return this.SourceLocked ? SelectSourceAndTargets(received) : received;
        }

        private DataActionResult MoveWafers(
                string durableTyp, DataRow source, DataRow target, DataTable wafers, DataTable plan, string description)
        {
            if (this.IsExchange)
            {
                return this.ExchangeDurableWafers(source, target, wafers, description);
            }

            return this.MoveDurableWafers(source, target, durableTyp, plan, description);
        }

        private void RemoveOtherFabTargets(DataTable targets)
        {
            if (targets == null)
            {
                return;
            }

            for (int index = targets.Rows.Count - 1; index >= 0; index--)
            {
                if (!IsTargetOfCurrentFab(targets.Rows[index], this.CurrentFacId))
                {
                    targets.Rows.RemoveAt(index);
                }
            }
        }

        private DataTable SelectSourceAndTargets(DataTable received)
        {
            DataTable selected = received == null ? new DataTable() : received.Clone();
            DataRow source = this.FindDurable(received, this.initialSourceId);

            if (source != null)
            {
                selected.ImportRow(source);
                DataTable targets = this.IsExchange
                        ? DurableManagementPresenter.ExchangeTargets(received, source)
                        : DurableManagementPresenter.EditTargets(received, source);

                foreach (DataRow target in targets.Rows)
                {
                    selected.ImportRow(target);
                }
            }

            return selected;
        }

        private static DataRow SelectedCarrier(Modern.Lab.WinForms.Controls.Selection.ModernComboBox combo)
        {
            DataRowView selected = combo == null ? null : combo.SelectedItem as DataRowView;
            return selected == null ? null : selected.Row;
        }

        private DataRow FindDurable(DataTable durables, string durableId)
        {
            if (durables == null)
            {
                return null;
            }

            foreach (DataRow row in durables.Rows)
            {
                if (string.Equals(
                        TableHelper.CellText(row, Schemas.Durable.DurableId.Name).Trim(),
                        durableId,
                        StringComparison.Ordinal))
                {
                    return row;
                }
            }

            return null;
        }

        internal static bool CanExecute(
                string action, CarrierEditEntryMode mode, DataRow sourceCarrier, DataRow targetCarrier,
                string durableTyp, string sourceId, string targetId,
                DataTable source, DataTable target, DataTable staged)
        {
            return ActionReason(action, mode, sourceCarrier, targetCarrier,
                    durableTyp, sourceId, targetId, source, target, staged).Length == 0;
        }
        internal static string ActionReason(
                string action, CarrierEditEntryMode mode, DataRow sourceCarrier, DataRow targetCarrier,
                string durableTyp, string sourceId, string targetId,
                DataTable source, DataTable target, DataTable staged)
        {
            string reason = RequiredReason(action, durableTyp, sourceId, targetId, source, target, staged);
            if (reason.Length > 0)
            {
                return reason;
            }

            reason = CommonReason(mode, sourceCarrier, targetCarrier, durableTyp, source, target, staged);
            if (reason.Length > 0)
            {
                return reason;
            }

            return mode == CarrierEditEntryMode.DurableExchange
                    ? ExchangeReason(mode, sourceCarrier, targetCarrier, durableTyp, source, target, staged)
                    : MoveReason(mode, sourceCarrier, targetCarrier, durableTyp, source, target, staged);
        }
        private static string RequiredReason(
                string action, string durableTyp, string sourceId, string targetId,
                DataTable source, DataTable target, DataTable staged)
        {
            if (action != CarrierEditPresenter.ActionMove)
            {
                return "This action is not supported.";
            }
            if (string.IsNullOrWhiteSpace(sourceId))
            {
                return "Select a source carrier first.";
            }
            if (!CarrierEditPresenter.MapIsStable(durableTyp, source))
            {
                return "Source map is not ready.";
            }
            if (staged == null || CarrierEditPresenter.CountFilled(staged) == 0)
            {
                return "Preview the wafers to move first.";
            }
            if (!CarrierEditPresenter.StagedRowsAreCurrent(source, staged))
            {
                return "The source map changed. Preview the move again.";
            }
            if (string.IsNullOrWhiteSpace(targetId))
            {
                return "Select a target carrier first.";
            }
            if (string.Equals(sourceId, targetId, StringComparison.Ordinal))
            {
                return "Source and target carriers must be different.";
            }
            if (!CarrierEditPresenter.MapIsStable(durableTyp, target))
            {
                return "Target map is not ready.";
            }
            if (CarrierEditPresenter.CountEmptyRows(target, Schemas.DurableWafer.SubProdTyp.Wafer.Value)
                    + CarrierEditPresenter.CountEmptyRows(target, Schemas.DurableLamella.SubProdTyp.Chip.Value)
                    + CarrierEditPresenter.EmptyLccPositions(target) == 0)
            {
                return "The target carrier has no empty positions.";
            }
            if (!CarrierEditPresenter.CanPlaceAll(durableTyp, target, staged))
            {
                return "The target does not have enough compatible empty positions.";
            }
            return string.Empty;
        }
    }
}
