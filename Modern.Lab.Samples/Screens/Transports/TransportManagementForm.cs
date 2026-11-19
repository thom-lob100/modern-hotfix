using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Controls.Wpf.Input;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.ResponseContracts;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Input;
using Modern.Lab.WinForms.Controls.Layout;

namespace Modern.Lab.Samples
{

    public partial class TransportManagementForm : ModernFormBase
    {

        private const string channelList = "list";
        private const string channelWafers = "wafers";
        private const string channelChips = "chips";
        private const string channelLamellas = "lamellas";
        private const string channelHistory = "history";
        private const string historyTitle = "Transport History";

        private readonly DependentCover contentCover;
        private readonly DependentCover historyCover;

        private DataTable transportData;

        private bool searchReady;
        private bool listReady;
        private bool listBinding;

        private string queryType = string.Empty;
        private string queryDurableId = string.Empty;
        private string layoutType;

        private static string DurationColor
        {
            get { return Modern.Lab.Theming.ModernTheme.IsDarkBased ? "#4DD0C2" : "#0F7B6C"; }
        }

        public TransportManagementForm()
        {
            this.InitializeComponent();
            SearchBandLayout.Attach(this.searchCard,
                    new SearchBandLayout.Field[]
                    {
                        SearchBandLayout.Pair(this.lblType, this.cboType),
                        SearchBandLayout.Pair(this.lblDurableId, this.txtDurableId),
                    },
                    new Control[] { this.btnReset, this.btnRefresh },
                    HorizontalAlignment.Right);

            this.contentCover = new DependentCover(this.contentBusy);
            this.historyCover = new DependentCover(this.historyBusy);

            this.InitializeModernForm(this.midPanel);
            this.RegisterRefreshShortcut(this.btnRefresh);

            this.DeferredResize = true;

            this.RegisterFindShortcut(this.gridTransports, this.gridHistory);

            this.menuTransport.Renderer = new Modern.Lab.WinForms.Rendering.ModernMenuRenderer();
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            this.cboType.DisplayMember = Schemas.Transport.DurableTyp.Name;
            this.cboType.ValueMember = Schemas.Transport.DurableTyp.Name;
            this.cboType.DataSource = TypeTable();
            this.cboType.SelectedIndex = -1;

            this.gridTransports.RowKeyMember = Schemas.Transport.RowKey;
            this.gridWafers.RowKeyMember = Schemas.DurableWafer.RowKey;
            this.gridChips.RowKeyMember = Schemas.DurableChip.RowKey;
            this.gridLamellas.RowKeyMember = Schemas.DurableLamella.RowKey;
            this.gridHistory.RowKeyMember = Schemas.TransportHistory.RowKey;
            this.ApplyTypeLayout(Schemas.Transport.DurableTyp.Foup.Value);

            this.ConfigureActionDropDown(this.ddbManualIn);
            this.ConfigureActionDropDown(this.ddbManualOut);
            this.btnChangeDest.IconGlyph = MenuGlyphs.For(TransportManagementPresenter.ChangeDestLabel);
            this.PopulateMenu();

            this.fieldInfo.ClearValues();
            this.ClearDependents();
            this.RefreshActionStates();
            this.searchReady = true;
        }

        private static DataTable TypeTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add(Schemas.Transport.DurableTyp.Name, typeof(string));

            foreach (string code in Schemas.Transport.DurableTyp.ValueCodes())
            {
                table.Rows.Add(code);
            }

            return table;
        }

        private string SelectedType()
        {
            DataRowView selected = this.cboType.SelectedItem as DataRowView;

            return selected == null ? string.Empty : Schemas.Transport.DurableTyp.Text(selected.Row);
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            if (!this.searchReady)
            {
                return;
            }

            this.queryType = this.SelectedType();
            this.queryDurableId = this.txtDurableId.Text.Trim();
            this.ExecuteSearch(false);
        }

        private void OnResetClick(object sender, EventArgs e)
        {
            this.InvalidateChannel(channelList);
            this.ResetBusy();
            this.txtDurableId.Text = string.Empty;
            this.cboType.SelectedIndex = -1;
            this.queryType = string.Empty;
            this.queryDurableId = string.Empty;
            this.transportData = null;
            this.listReady = false;
            this.listBinding = true;

            try
            {
                this.gridTransports.DataSource = null;
            }
            finally
            {
                this.listBinding = false;
            }

            this.ApplySelection(false);
        }

        private async void ExecuteSearch(bool silent)
        {
            string type = this.queryType;
            string durableId = this.queryDurableId;
            this.listReady = false;

            IDisposable busy = silent ? null : this.Busy("Loading transports...", durableId.Length == 0 ? type : durableId);
            LoadOutcome<DataTable> outcome;

            try
            {
                outcome = await this.FetchAsync(
                        channelList,
                        () => this.GetTransports(RequestParameters.Of(
                                Schemas.Transport.DurableTyp, type,
                                Schemas.Transport.DurableId, durableId)),
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
                this.RefreshActionStates();
                return;
            }

            this.BindTransports(outcome.Value, silent ? string.Empty : durableId, silent);
        }

        private void BindTransports(DataTable incoming, string typedId, bool silent)
        {
            string keepId = this.SelectedDurableId();
            DataTable transports = incoming.Copy();

            this.transportData = transports;
            this.listReady = true;
            TransportFields(transports).Apply(this.fieldInfo);

            this.listBinding = true;

            try
            {
                this.gridTransports.DataSource = transports;
                this.gridTransports.SelectedIndex = -1;

                if (!this.SelectTransport(typedId) && !this.SelectTransport(keepId) && transports.Rows.Count > 0)
                {
                    this.gridTransports.SelectedIndex = 0;
                }
            }
            finally
            {
                this.listBinding = false;
            }

            this.ApplySelection(silent);
        }

        private bool SelectTransport(string durableId)
        {
            if (this.transportData == null || durableId.Length == 0)
            {
                return false;
            }

            foreach (DataRowView row in this.transportData.DefaultView)
            {
                if (string.Equals(TransportManagementPresenter.Id(row.Row), durableId, StringComparison.OrdinalIgnoreCase))
                {
                    this.gridTransports.SelectedItem = row;
                    return true;
                }
            }

            return false;
        }

        private void OnListSelectionChanged(object sender, EventArgs e)
        {
            if (!this.searchReady || this.listBinding)
            {
                return;
            }

            this.ApplySelection(false);
        }

        private void ApplySelection(bool silent)
        {
            DataRow transport = this.SelectedTransport();

            if (transport == null)
            {
                this.fieldInfo.ClearValues();
                this.ClearDependents();
                this.RefreshActionStates();
                return;
            }

            this.fieldInfo.SetRow(transport);
            this.ApplyTypeLayout(Schemas.Transport.DurableTyp.Text(transport));
            this.InvalidateDependents();
            this.RefreshActionStates();
            this.pageHistory.Text = historyTitle + " — " + TransportManagementPresenter.Id(transport);
            this.LoadDependents(transport, silent);
        }

        private static bool IsTray(string durableType)
        {
            return durableType == Schemas.Transport.DurableTyp.Tray.Value;
        }

        private void ApplyTypeLayout(string durableType)
        {
            string layout = IsTray(durableType)
                    ? Schemas.Transport.DurableTyp.Tray.Value
                    : Schemas.Transport.DurableTyp.Foup.Value;

            if (this.layoutType == layout)
            {
                return;
            }

            this.layoutType = layout;
            this.tabContent.SuspendLayout();

            try
            {
                this.tabContent.Controls.Remove(this.pageWaferList);
                this.tabContent.Controls.Remove(this.pageChipList);
                this.tabContent.Controls.Remove(this.pageLamellaList);

                if (IsTray(layout))
                {
                    this.tabContent.Controls.Add(this.pageChipList);
                    this.tabContent.Controls.Add(this.pageLamellaList);
                }
                else
                {
                    this.tabContent.Controls.Add(this.pageWaferList);
                }
            }
            finally
            {
                this.tabContent.ResumeLayout(true);
            }

            this.tabContent.SelectedIndex = 0;
            this.ClearContents();
        }

        private void InvalidateDependents()
        {
            this.InvalidateChannel(channelWafers);
            this.InvalidateChannel(channelChips);
            this.InvalidateChannel(channelLamellas);
            this.InvalidateChannel(channelHistory);
        }

        private void ClearDependents()
        {
            this.contentCover.Reset();
            this.historyCover.Reset();
            this.InvalidateDependents();
            this.ClearContents();
            this.ClearHistory();
            this.pageHistory.Text = historyTitle;
        }

        private void ClearContents()
        {
            this.BindWafers(null);
            this.BindChips(null);
            this.BindLamellas(null);
        }

        private void ClearHistory()
        {
            this.gridHistory.DataSource = null;
            this.gridHistory.StatusText = string.Empty;
        }

        private void LoadDependents(DataRow transport, bool silent)
        {
            string durableId = TransportManagementPresenter.Id(transport);

            if (IsTray(this.layoutType))
            {
                this.LoadDependent(channelChips, () => this.GetTransportChips(transport), this.BindChips, silent, this.contentCover);
                this.LoadDependent(channelLamellas, () => this.GetTransportLamellas(transport), this.BindLamellas, silent, this.contentCover);
            }
            else
            {
                this.LoadDependent(channelWafers, () => this.GetTransportWafers(transport), this.BindWafers, silent, this.contentCover);
            }

            this.LoadDependent(channelHistory, () => this.GetTransportHistory(transport),
                    table => this.BindHistory(table, durableId), silent, this.historyCover);
        }

        private async void LoadDependent(
                string channel, Func<DataTable> request, Action<DataTable> bind, bool silent, DependentCover cover)
        {
            if (!silent)
            {
                cover.Begin();
            }

            LoadOutcome<DataTable> outcome;

            try
            {
                outcome = await this.FetchAsync(channel, request, silent);
            }
            finally
            {
                if (!silent)
                {
                    cover.End();
                }
            }

            if (!outcome.IsCurrent)
            {
                return;
            }

            bind(outcome.Failure != null ? null : outcome.Value);
        }

        private void BindWafers(DataTable wafers)
        {
            this.BindContent(this.gridWafers, this.pageWaferList, WaferColumns(wafers), wafers);
        }

        private void BindChips(DataTable chips)
        {
            this.BindContent(this.gridChips, this.pageChipList, ChipColumns(chips), chips);
        }

        private void BindLamellas(DataTable lamellas)
        {
            this.BindContent(this.gridLamellas, this.pageLamellaList, LamellaColumns(lamellas), lamellas);
        }

        private void BindContent(ModernDataGrid grid, ModernTabPage page, GridColumns columns, DataTable table)
        {
            columns.Bind(grid);

            page.BadgeCount = table == null ? 0 : table.Rows.Count;
        }

        private void BindHistory(DataTable history, string durableId)
        {
            if (history == null)
            {
                this.ClearHistory();
                return;
            }

            LotHistoryPresenter.AddDurationColumn(history);
            this.gridHistory.DataSource = history;
            this.gridHistory.StatusText = durableId + LotHistoryPresenter.CycleTimeSuffix(history);
        }

        private DataRow SelectedTransport()
        {
            DataRowView selected = this.gridTransports.SelectedItem as DataRowView;

            return selected == null || TransportManagementPresenter.Id(selected.Row).Length == 0 ? null : selected.Row;
        }

        private string SelectedDurableId()
        {
            return TransportManagementPresenter.Id(this.SelectedTransport());
        }

        private DataRow TransportRow(string durableId)
        {
            if (this.transportData == null || string.IsNullOrEmpty(durableId))
            {
                return null;
            }

            foreach (DataRow row in this.transportData.Rows)
            {
                if (string.Equals(TransportManagementPresenter.Id(row), durableId, StringComparison.Ordinal))
                {
                    return row;
                }
            }

            return null;
        }

        private void ConfigureActionDropDown(ModernDropDownButton dropDown)
        {
            dropDown.DisplayMember = TransportManagementPresenter.MenuLabelColumn;
            dropDown.ValueMember = TransportManagementPresenter.MenuValueColumn;
            dropDown.EnabledMember = TransportManagementPresenter.MenuEnabledColumn;
            dropDown.GlyphMember = TransportManagementPresenter.MenuGlyphColumn;
            dropDown.AccentTextMember = TransportManagementPresenter.MenuAccentColumn;
            dropDown.IconGlyph = MenuGlyphs.For(dropDown.Text);
        }

        private void RefreshActionStates()
        {
            DataRow row = this.SelectedTransport();

            this.RefreshActionDropDown(this.ddbManualIn, TransportManagementPresenter.ManualInActions, row);
            this.RefreshActionDropDown(this.ddbManualOut, TransportManagementPresenter.ManualOutActions, row);
            this.ApplyActionReason(this.btnChangeDest, this.ActionReason(TransportManagementPresenter.ActionChangeDest, row));

            this.lblTarget.Text = TransportManagementPresenter.StatusLineText(row);
        }

        private void RefreshActionDropDown(ModernDropDownButton dropDown, IList<DurableAction> actions, DataRow row)
        {
            dropDown.DataSource = TransportManagementPresenter.BuildGatedMenuTable(actions, key => this.ActionEnabled(key, row));
            dropDown.Enabled = this.listReady && row != null;
        }

        private string ActionReason(string key, DataRow transport)
        {
            return TransportActionReason(key, transport, this.listReady);
        }

        private bool ActionEnabled(string key, DataRow transport)
        {
            return this.ActionReason(key, transport) == null;
        }

        private void OnActionClicked(object sender, DropDownItemClickedEventArgs e)
        {
            this.ExecuteAction(e.Value as string);
        }

        private void OnChangeDestClick(object sender, EventArgs e)
        {
            this.ExecuteAction(TransportManagementPresenter.ActionChangeDest);
        }

        private void ExecuteAction(string key)
        {
            DataRow row = this.SelectedTransport();

            if (!TransportManagementPresenter.IsActionKey(key) || !this.ActionEnabled(key, row))
            {
                return;
            }

            switch (key)
            {
                case TransportManagementPresenter.ActionManualIn:
                    this.ShowManualInDialog(row);
                    break;

                case TransportManagementPresenter.ActionManualOut:
                    this.ShowManualOutDialog(row);
                    break;

                case TransportManagementPresenter.ActionManualInCancel:
                case TransportManagementPresenter.ActionManualOutCancel:
                    this.ShowManualCancelDialog(row, key);
                    break;

                case TransportManagementPresenter.ActionChangeDest:
                    this.ShowChangeDestDialog(row);
                    break;
            }
        }

        protected virtual void ShowManualInDialog(DataRow transport)
        {
            TransportManualInDialogOptions options = new TransportManualInDialogOptions();
            options.Title = TransportManagementPresenter.ManualInLabel + " — " + TransportManagementPresenter.Id(transport);
            options.OkText = TransportManagementPresenter.ManualInLabel;
            options.DefaultDescription = DefaultDescriptions.ManualInDurable;
            options.Source = ActionRows.Copy(transport);

            using (TransportManualInDialogForm dialog = new TransportManualInDialogForm(options))
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

        protected virtual void ShowManualOutDialog(DataRow transport)
        {
            TransportManualOutDialogOptions options = new TransportManualOutDialogOptions();
            options.Title = TransportManagementPresenter.ManualOutLabel + " — " + TransportManagementPresenter.Id(transport)
                    + " · " + Schemas.Durable.EqpId.Text(transport);
            options.OkText = TransportManagementPresenter.ManualOutLabel;
            options.DefaultDescription = DefaultDescriptions.ManualOutDurable;
            options.Source = ActionRows.Copy(transport);

            using (TransportManualOutDialogForm dialog = new TransportManualOutDialogForm(options))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                this.ProcessManualOut(options.Source, RequestParameters.Of(
                        Schemas.PortDet.PortNm, dialog.PortNm,
                        Schemas.Criteria.Description, dialog.Description));
            }
        }

        protected virtual void ShowManualCancelDialog(DataRow transport, string actionKey)
        {
            bool inCancel = actionKey == TransportManagementPresenter.ActionManualInCancel;
            string label = inCancel ? TransportManagementPresenter.ManualInCancelLabel : TransportManagementPresenter.ManualOutCancelLabel;
            TransportManualCancelDialogOptions options = new TransportManualCancelDialogOptions();
            options.Title = label + " — " + TransportManagementPresenter.Id(transport);
            options.OkText = label;
            options.DefaultDescription = inCancel ? DefaultDescriptions.ManualInCancel : DefaultDescriptions.ManualOutCancel;
            options.Source = ActionRows.Copy(transport);

            using (TransportManualCancelDialogForm dialog = new TransportManualCancelDialogForm(options))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                this.ProcessManualCancel(options.Source, actionKey, RequestParameters.Of(Schemas.Criteria.Description, dialog.Description));
            }
        }

        protected virtual void ShowChangeDestDialog(DataRow transport)
        {
            TransportChangeDestDialogOptions options = new TransportChangeDestDialogOptions();
            options.Title = TransportManagementPresenter.ChangeDestLabel + " — " + TransportManagementPresenter.Id(transport);
            options.OkText = TransportManagementPresenter.ChangeDestLabel;
            options.DefaultDescription = DefaultDescriptions.ChangeDestination;
            options.Source = ActionRows.Copy(transport);

            using (TransportChangeDestDialogForm dialog = new TransportChangeDestDialogForm(options))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                this.ProcessChangeDest(options.Source, RequestParameters.Of(
                        Schemas.EquipmentExt.EqpId, dialog.EqpId,
                        Schemas.PortDet.PortNm, dialog.PortNm,
                        Schemas.Criteria.Description, dialog.Description));
            }
        }

        private bool StillAllows(string key, DataRow transport)
        {
            string reason = this.ActionReason(key, this.TransportRow(TransportManagementPresenter.Id(transport)));

            if (reason != null)
            {
                this.ShowWarningMessage(reason);
                return false;
            }

            return true;
        }

        private void ProcessManualIn(DataRow transport, RequestParameters parameters)
        {
            if (!this.StillAllows(TransportManagementPresenter.ActionManualIn, transport))
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
                    () => this.ManualInTransport(transport, parameters),
                    TransportManagementPresenter.ManualInLabel + " " + TransportManagementPresenter.Id(transport)
                            + " → " + eqpId + " " + portNm + "…");
        }

        private void ProcessManualOut(DataRow transport, RequestParameters parameters)
        {
            if (!this.StillAllows(TransportManagementPresenter.ActionManualOut, transport))
            {
                return;
            }

            string eqpId = Schemas.Durable.EqpId.Text(transport);
            string portNm = parameters.Value(Schemas.PortDet.PortNm);

            if (ResponseColumns.IsBlank(eqpId) || ResponseColumns.IsBlank(portNm))
            {
                return;
            }

            this.Process(
                    () => this.ManualOutTransport(transport, parameters),
                    TransportManagementPresenter.ManualOutLabel + " " + TransportManagementPresenter.Id(transport)
                            + " → " + eqpId + " " + portNm + "…");
        }

        private void ProcessManualCancel(DataRow transport, string actionKey, RequestParameters parameters)
        {
            if (!this.StillAllows(actionKey, transport))
            {
                return;
            }

            if (actionKey == TransportManagementPresenter.ActionManualInCancel)
            {
                this.Process(
                        () => this.ManualInCancelTransport(transport, parameters),
                        TransportManagementPresenter.ManualInCancelLabel + " " + TransportManagementPresenter.Id(transport) + "…");
                return;
            }

            this.Process(
                    () => this.ManualOutCancelTransport(transport, parameters),
                    TransportManagementPresenter.ManualOutCancelLabel + " " + TransportManagementPresenter.Id(transport) + "…");
        }

        private void ProcessChangeDest(DataRow transport, RequestParameters parameters)
        {
            if (!this.StillAllows(TransportManagementPresenter.ActionChangeDest, transport))
            {
                return;
            }

            string durableId = TransportManagementPresenter.Id(transport);
            string eqpId = parameters.Value(Schemas.EquipmentExt.EqpId);

            if (ResponseColumns.IsBlank(durableId) || ResponseColumns.IsBlank(eqpId))
            {
                return;
            }

            string destination = (eqpId + " " + parameters.Value(Schemas.PortDet.PortNm)).Trim();

            this.Process(
                    () => this.ChangeDestTransport(transport, parameters),
                    TransportManagementPresenter.ChangeDestLabel + " " + durableId + " → " + destination + "…");
        }

        private void Process(Func<DataActionResult> call, string busyText)
        {
            if (!this.listReady)
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
                        this.ExecuteSearch(true);
                    },
                    busyText);
        }

        private void PopulateMenu()
        {
            this.menuTransport.Items.Clear();
            this.AppendMenuGroup(this.ddbManualIn.Text, TransportManagementPresenter.ManualInActions);
            this.AppendMenuGroup(this.ddbManualOut.Text, TransportManagementPresenter.ManualOutActions);
            this.menuTransport.Items.Add(
                    this.CreateMenuItem(TransportManagementPresenter.Of(TransportManagementPresenter.ActionChangeDest)));
        }

        private void AppendMenuGroup(string groupText, IList<DurableAction> actions)
        {
            ToolStripMenuItem group = MenuGlyphs.CreateItem(groupText);
            group.DropDown.Renderer = this.menuTransport.Renderer;

            foreach (DurableAction action in actions)
            {
                group.DropDownItems.Add(this.CreateMenuItem(action));
            }

            this.menuTransport.Items.Add(group);
        }

        private ToolStripMenuItem CreateMenuItem(DurableAction action)
        {
            ToolStripMenuItem item = MenuGlyphs.CreateItem(action.Label);
            item.Tag = action.Key;
            item.Click += this.OnMenuActionClick;
            return item;
        }

        private void OnMenuTransportOpening(object sender, CancelEventArgs e)
        {
            DataRow row = this.SelectedTransport();

            foreach (object entry in this.menuTransport.Items)
            {
                ToolStripMenuItem top = entry as ToolStripMenuItem;

                if (top == null)
                {
                    continue;
                }

                this.ApplyMenuReason(top, row);

                foreach (object child in top.DropDownItems)
                {
                    this.ApplyMenuReason(child as ToolStripMenuItem, row);
                }
            }
        }

        private void ApplyMenuReason(ToolStripMenuItem item, DataRow row)
        {
            string key = item == null ? null : item.Tag as string;

            if (key != null && TransportManagementPresenter.IsActionKey(key))
            {
                ApplyActionReason(item, this.ActionReason(key, row));
            }
        }

        private void OnMenuActionClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;

            if (item != null)
            {
                this.ExecuteAction(item.Tag as string);
            }
        }
    }
}
