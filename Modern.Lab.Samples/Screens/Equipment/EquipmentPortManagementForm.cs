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
using Modern.Lab.Samples.Services;
using Modern.Lab.Samples.Contracts;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Display;

using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.NewItems;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.WinForms.Controls.Selection;

namespace Modern.Lab.Samples
{

    public partial class EquipmentPortManagementForm : ModernFormBase
    {
        private static readonly ResponseContractSet Contracts = EquipmentPortContracts.Build();

        private DataTable equipmentData;
        private readonly NewItemTracker newItems = new NewItemTracker();
        private readonly NewItemTracker eqpHistoryNewItems = new NewItemTracker();
        private readonly NewItemTracker portHistoryNewItems = new NewItemTracker();
        private string newItemCriteria;
        private DataTable portData;
        private DataTable eqpHistoryData;
        private DataTable portHistoryData;

        private DataTable equipmentView;

        private const string treeKeyBase = "TREE_KEY";
        private const string treeParentBase = "TREE_PARENT";
        private string treeKeyColumn = treeKeyBase;
        private string treeParentColumn = treeParentBase;

        private TableResponse equipmentCurrent;
        private TableResponse portCurrent;

        private const string channelGroups = "groups";
        private const string channelEquipments = "equipments";
        private const string channelPorts = "ports";
        private const string channelEqpHistory = "eqpHistory";
        private const string channelPortHistory = "portHistory";
        private const int portSelectionDelay = 300;
        private const string eqpHistoryTitle = "Eqp History";
        private const string portHistoryTitle = "Port History";

        private bool groupSetup;
        private bool treeBinding;

        private readonly DependentCover portCover;
        private readonly DependentCover historyCover;

        public EquipmentPortManagementForm()
        {
            this.InitializeComponent();

            this.portCover = new DependentCover(this.portBusy);
            this.historyCover = new DependentCover(this.historyBusy);

            this.InitializeModernForm(this.midPanel);
            this.RegisterRefreshShortcut(this.btnRefresh);

            this.DeferredResize = true;

            this.RegisterFindShortcut(this.gridEquipmentExtHistory, this.gridPortExtHistory);

            this.menuEqp.Renderer = new Modern.Lab.WinForms.Rendering.ModernMenuRenderer();
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            this.cboGroup.UseEntity(Schemas.EquipmentGroup);

            this.treeEquipmentExts.IdMember = treeKeyColumn;
            this.treeEquipmentExts.ParentIdMember = treeParentColumn;
            this.treeEquipmentExts.DisplayMember = Schemas.EquipmentExt.DisplayField.Name;
            this.gridPortExts.RowKeyMember = Schemas.PortExt.RowKey;
            this.gridPortExts.CellLinkClick += this.OnDurableIdLinkClick;
            this.gridEquipmentExtHistory.RowKeyMember = Schemas.EquipmentExtHistory.RowKey;
            this.gridPortExtHistory.RowKeyMember = Schemas.PortExtHistory.RowKey;
            this.gridPortExtHistory.CellLinkClick += this.OnDurableIdLinkClick;

            this.PopulateMenu(this.menuEqp, "Mode", EquipmentPortManagementPresenter.EquipmentActions, this.OnEquipmentMenuItemClick);

            this.RefreshActionStates();

            this.LoadGroups();
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
                this.cboGroup.CheckAll();
            }
            finally
            {
                this.groupSetup = false;
            }

            this.ExecuteSearch();
        }

        private void OnDurableIdLinkClick(object sender, GridButtonClickEventArgs e)
        {
            string durableId = DurableInfoDialogForm.LinkedDurableId(
                    e,
                    Schemas.PortExt.DurableId.Name,
                    Schemas.PortExtHistory.DurableId.Name);

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

        private void OnGroupChanged(object sender, EventArgs e)
        {
            if (this.groupSetup)
            {
                return;
            }

            this.ExecuteSearch();
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            this.ExecuteSearch();
        }

        private void OnResetClick(object sender, EventArgs e)
        {
            this.groupSetup = true;

            try
            {
                this.cboGroup.CheckAll();
            }
            finally
            {
                this.groupSetup = false;
            }

            this.ExecuteSearch();
        }

        private string SelectedEqpGrpIds()
        {
            return EquipmentPortManagementPresenter.GroupFilter(this.cboGroup.CheckedValues);
        }

        private string SelectedEquipmentId()
        {
            return Schemas.EquipmentExt.EqpId.Text(this.SelectedEquipmentRow());
        }

        private DataRowView SelectedEquipmentView()
        {
            DataRowView selected = this.treeEquipmentExts.SelectedItem as System.Data.DataRowView;

            if (selected == null || this.equipmentView == null || this.equipmentData == null)
            {
                return null;
            }

            int index = this.equipmentView.Rows.IndexOf(selected.Row);

            if (index < 0 || index >= this.equipmentData.Rows.Count)
            {
                return null;
            }

            return this.equipmentData.DefaultView[index];
        }

        private DataRow SelectedEquipmentRow()
        {
            DataRowView view = this.SelectedEquipmentView();
            return view == null ? null : view.Row;
        }

        private async void ExecuteSearch(bool silent = false)
        {
            string eqpGrpIds = this.SelectedEqpGrpIds();
            this.newItems.BeginQuery();
            this.RefreshActionStates();
            string keepEquipmentId = this.SelectedEquipmentId();

            IDisposable busy = silent ? null : this.Busy("Loading equipment...", this.cboGroup.Text);

            LoadOutcome<DataTable> outcome;

            try
            {
                outcome = await this.FetchAsync(
                        channelEquipments, () => this.GetEquipmentExts(eqpGrpIds), silent);
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

            this.newItemCriteria = NewItemTracker.Criteria(NewItemTracker.SelectionSet(eqpGrpIds));
            this.BindEquipmentExts(outcome.Value, keepEquipmentId, silent);
        }

        private void BindEquipmentExts(DataTable equipments, string keepEquipmentId, bool silent)
        {
            this.equipmentData = this.BindJudged(
                    EquipmentPortContracts.EquipmentTable, ref this.equipmentCurrent, equipments, silent);
            bool judged = Judged(this.equipmentCurrent);
            this.equipmentData = this.newItems.Accept(this.equipmentData, Schemas.EquipmentExt.RowKey, this.newItemCriteria, judged);

            bool flat = !judged;
            this.treeKeyColumn = UniqueColumn(this.equipmentData, treeKeyBase);
            this.treeParentColumn = UniqueColumn(this.equipmentData, treeParentBase);
            this.treeEquipmentExts.IdMember = this.treeKeyColumn;
            this.treeEquipmentExts.ParentIdMember = this.treeParentColumn;
            this.equipmentView = EquipmentTreeView(this.equipmentData, flat, this.treeKeyColumn, this.treeParentColumn);

            EquipmentExtColumns(this.equipmentView).Apply(this.treeEquipmentExts);

            EquipmentFields(judged ? this.equipmentCurrent.Table : this.equipmentData)
                    .Apply(this.fieldEqpInfo);

            string selectId = EquipmentPortManagementPresenter.SelectionAfterBind(this.equipmentData, keepEquipmentId);

            this.treeBinding = true;

            try
            {
                this.treeEquipmentExts.DataSource = this.equipmentView;
                this.treeEquipmentExts.SelectedValue = this.TreeKeyOf(selectId);
            }
            finally
            {
                this.treeBinding = false;
            }

            this.eqpCard.TitleRightText = this.equipmentData == null
                    ? string.Empty
                    : this.equipmentData.Rows.Count.ToString("N0") + " units";

            this.ApplyEquipmentSelection(silent);
        }

        private static DataTable EquipmentTreeView(DataTable source, bool flat, string keyColumn, string parentColumn)
        {
            if (source == null)
            {
                return null;
            }

            DataTable view = source.Copy();
            view.Columns.Add(keyColumn, typeof(string));
            view.Columns.Add(parentColumn, typeof(string));

            for (int index = 0; index < view.Rows.Count; index++)
            {
                DataRow row = view.Rows[index];
                row[keyColumn] = flat
                        ? index.ToString(System.Globalization.CultureInfo.InvariantCulture)
                        : Schemas.EquipmentExt.EqpId.Text(row);
                row[parentColumn] = flat ? string.Empty : Schemas.EquipmentExt.ParentEqpId.Text(row);
            }

            return view;
        }

        private static string UniqueColumn(DataTable table, string baseName)
        {
            HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (table != null)
            {
                foreach (DataColumn column in table.Columns)
                {
                    names.Add(column.ColumnName);
                }
            }

            if (!names.Contains(baseName))
            {
                return baseName;
            }

            for (int suffix = 1; ; suffix++)
            {
                string candidate = baseName + "_" + suffix.ToString(System.Globalization.CultureInfo.InvariantCulture);

                if (!names.Contains(candidate))
                {
                    return candidate;
                }
            }
        }

        private object TreeKeyOf(string eqpId)
        {
            if (string.IsNullOrEmpty(eqpId) || this.equipmentData == null || this.equipmentView == null)
            {
                return null;
            }

            for (int index = 0; index < this.equipmentData.Rows.Count; index++)
            {
                DataRow row = this.equipmentData.Rows[index];

                if (Schemas.EquipmentExt.EqpId.Text(row) == eqpId)
                {
                    return this.equipmentView.Rows[index][treeKeyColumn];
                }
            }

            return null;
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
                    tableId, incoming, Contracts, EquipmentPortManagementPresenter.ScreenColumns);

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

        private void OnEquipmentSelectionChanged(object sender, EventArgs e)
        {
            if (this.treeBinding)
            {
                return;
            }

            this.ApplyEquipmentSelection(false);
        }

        private void ApplyEquipmentSelection(bool silent)
        {
            DataRowView equipment = this.SelectedEquipmentView();

            if (equipment == null)
            {
                this.fieldEqpInfo.ClearValues();
                this.portCard.Text = "Port List";
                this.ClearDependents();
                this.RefreshActionStates();
                return;
            }

            string eqpId = Schemas.EquipmentExt.EqpId.Text(equipment.Row);

            this.fieldEqpInfo.SetRow(equipment.Row);
            this.fieldEqpInfo.SetValue(Schemas.EquipmentExt.AutoYn.Name, EquipmentPortManagementPresenter.AutoText(equipment.Row));
            this.portCard.Text = "Port List — " + eqpId;

            bool loadable = Judged(this.equipmentCurrent) && eqpId.Length > 0;

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

            this.pageEqpHistory.Text = eqpHistoryTitle + " — " + eqpId;
            this.LoadDependents(eqpId, silent);
        }

        private void InvalidateDependents()
        {
            this.InvalidateChannel(channelPorts);
            this.InvalidateChannel(channelEqpHistory);
            this.InvalidateChannel(channelPortHistory);
        }

        private void ClearDependents()
        {
            this.portCover.Reset();
            this.historyCover.Reset();
            this.InvalidateChannel(channelPorts);
            this.InvalidateChannel(channelEqpHistory);
            this.InvalidateChannel(channelPortHistory);

            this.portData = null;
            this.portCurrent = null;
            this.gridPortExts.DataSource = null;
            this.eqpHistoryData = null;
            this.gridEquipmentExtHistory.DataSource = null;
            this.portHistoryData = null;
            this.gridPortExtHistory.DataSource = null;
            this.pageEqpHistory.Text = eqpHistoryTitle;
            this.pagePortHistory.Text = portHistoryTitle;
        }

        private void LoadDependents(string eqpId, bool silent)
        {
            this.LoadDependent(channelPorts, () => this.GetPortExts(eqpId), table => this.BindPortExts(table, silent), silent, this.portCover);
            this.LoadDependent(channelEqpHistory, () => this.GetEquipmentExtHistory(eqpId),
                    table => this.BindEquipmentExtHistory(table, NewItemTracker.Criteria(eqpId)), silent, this.historyCover);
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

        private void BindPortExts(DataTable ports, bool silent)
        {
            this.portData = this.BindJudged(
                    EquipmentPortContracts.PortTable, ref this.portCurrent, ports, silent);

            if (!Judged(this.portCurrent))
            {
                GridColumns.Of(this.portData == null ? null : this.portData.Copy()).Bind(this.gridPortExts);
            }
            else
            {
                PortExtColumns(this.portData).Bind(this.gridPortExts);
            }

            this.RefreshActionStates();
            this.ApplyPortSelection(silent);
        }

        private void BindEquipmentExtHistory(DataTable history, string criteria)
        {
            this.eqpHistoryData = this.eqpHistoryNewItems.Accept(history, this.gridEquipmentExtHistory.RowKeyMember, criteria);

            EquipmentExtHistoryColumns(this.eqpHistoryData).Bind(this.gridEquipmentExtHistory);
            NewItemTint.Bind(this.gridEquipmentExtHistory, this.eqpHistoryNewItems, this.gridEquipmentExtHistory.RowKeyMember);
        }

        private void BindPortExtHistory(DataTable history, string criteria)
        {
            this.portHistoryData = this.portHistoryNewItems.Accept(history, this.gridPortExtHistory.RowKeyMember, criteria);

            PortExtHistoryColumns(this.portHistoryData).Bind(this.gridPortExtHistory);
            NewItemTint.Bind(this.gridPortExtHistory, this.portHistoryNewItems, this.gridPortExtHistory.RowKeyMember);
        }

        private void OnPortSelectionChanged(object sender, EventArgs e)
        {
            this.RefreshActionStates();
            this.Debounce(channelPortHistory, portSelectionDelay, new MethodInvoker(delegate { this.ApplyPortSelection(false); }));
        }

        private void ApplyPortSelection(bool silent)
        {
            this.CancelDebounce(channelPortHistory);

            DataRowView equipment = this.SelectedEquipmentView();
            DataRowView port = this.gridPortExts.SelectedItem as DataRowView;
            string portNm = port == null ? string.Empty : Schemas.PortExt.PortNm.Text(port.Row);

            if (equipment == null || portNm.Length == 0)
            {
                this.InvalidateChannel(channelPortHistory);
                this.portHistoryData = null;
                this.gridPortExtHistory.DataSource = null;
                this.pagePortHistory.Text = portHistoryTitle;
                return;
            }

            string eqpId = Schemas.EquipmentExt.EqpId.Text(equipment.Row);

            this.pagePortHistory.Text = portHistoryTitle + " — " + portNm;
            this.LoadDependent(
                    channelPortHistory,
                    () => this.GetPortExtHistory(eqpId, portNm),
                    table => this.BindPortExtHistory(table, NewItemTracker.Criteria(eqpId, portNm)), silent, this.historyCover);
        }

        private void RefreshActionStates()
        {
            DataRowView equipment = this.SelectedEquipmentView();
            DataRowView port = this.gridPortExts.SelectedItem as DataRowView;
            DataRow equipmentRow = equipment == null ? null : equipment.Row;
            DataRow portRow = port == null ? null : port.Row;
            this.lblTarget.Text = EquipmentPortManagementPresenter.StatusLineText(equipmentRow, portRow);
        }

        private void ExecuteEquipmentAction(string key)
        {
            DataRowView equipment = this.SelectedEquipmentView();

            if (equipment == null
                    || EquipmentActionReason(key, equipment.Row, this.newItems.ActionsReady) != null)
            {
                return;
            }

            string eqpId = Schemas.EquipmentExt.EqpId.Text(equipment.Row);

            if (key == EquipmentPortManagementPresenter.ActionAuto)
            {
                bool turnOn = !EquipmentPortManagementForm.IsAuto(equipment.Row);

                if (!this.Confirm(
                        "Turn Auto " + Modern.Lab.WinForms.Controls.Dialogs.ModernMessageDialog.Emphasis(turnOn ? "on" : "off")
                                + " for " + eqpId + "?",
                        "Confirm"))
                {
                    return;
                }

                if (!this.EquipmentStillAllows(key, eqpId))
                {
                    return;
                }

                DataRow autoEquipment = ActionRows.Copy(equipment.Row);
                this.Process(
                        () => this.SetAuto(autoEquipment, turnOn),
                        turnOn ? "Turning Auto on…" : "Turning Auto off…",
                        () => this.ExecuteSearch(true));
                return;
            }

            if (!this.Confirm(
                    "Change " + eqpId + " from " + EquipmentPortManagementPresenter.Mode(equipment.Row) + " to "
                            + Modern.Lab.WinForms.Controls.Dialogs.ModernMessageDialog.Emphasis(key) + "?",
                    "Confirm"))
            {
                return;
            }

            if (!this.EquipmentStillAllows(key, eqpId))
            {
                return;
            }

            DataRow actionEquipment = ActionRows.Copy(equipment.Row);
            this.Process(
                    () => this.SetMode(actionEquipment, key),
                    "Changing mode to " + key + "…",
                    () => this.ExecuteSearch(true));
        }

        private bool EquipmentStillAllows(string key, string eqpId)
        {
            DataRowView equipment = this.SelectedEquipmentView();
            string reason = equipment == null || Schemas.EquipmentExt.EqpId.Text(equipment.Row) != eqpId
                    ? NoEquipmentSelected
                    : EquipmentActionReason(key, equipment.Row, this.newItems.ActionsReady);

            if (reason == null)
            {
                return true;
            }

            this.ShowWarningMessage(reason);
            return false;
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

        private void OnMenuEqpOpening(object sender, CancelEventArgs e)
        {
            DataRowView equipment = this.SelectedEquipmentView();
            DataRow equipmentRow = equipment == null ? null : equipment.Row;

            e.Cancel = !ApplyMenuStates(
                    this.menuEqp, EquipmentPortManagementPresenter.EquipmentActions,
                    equipmentRow, equipment != null, this.newItems.ActionsReady);
        }

        private static bool ApplyMenuStates(
                ContextMenuStrip menu, IList<EquipmentPortAction> actions,
                DataRow equipment, bool hasTarget, bool listReady)
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
                        ApplyActionReason(item, EquipmentActionReason(key, equipment, listReady));
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
    }
}
