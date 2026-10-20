using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Data;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.WinForms.Controls.Selection;

namespace Modern.Lab.Samples
{
    public partial class LotScrapDialogForm : ModernFormBase
    {
        private const string lookupChannel = "scrapLookups";
        private const string selectColumn = "CHK";
        private readonly DataRow lot;
        private readonly DataTable wafers;
        private readonly string idCaption;
        private DataTable waferRows;
        private bool ready;
        private bool closed;
        private bool refreshPending;

        public LotScrapDialogForm()
        {
            this.InitializeComponent();
            this.InitializeModernForm(false);
            this.WaferIds = new string[0];
            if (this.cboCode.Child != null)
            {
                this.cboCode.Child.AddHandler(System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent,
                        new System.Windows.Controls.TextChangedEventHandler(this.OnComboTextChanged), true);
            }
        }

        public LotScrapDialogForm(DataRow lot, DataTable wafers, string idCaption) : this()
        {
            if (lot == null) { throw new ArgumentNullException(nameof(lot)); }
            DataTable copy = lot.Table.Clone();
            copy.Rows.Add(lot.ItemArray);
            this.lot = copy.Rows[0];
            this.wafers = wafers == null ? null : wafers.Copy();
            this.idCaption = string.IsNullOrEmpty(idCaption) ? "Wafer Id" : idCaption;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ReasonCode { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Description { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string[] WaferIds { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool WholeLot { get; private set; }

        private string LotId
        {
            get { return this.lot == null ? string.Empty : TableHelper.CellText(this.lot, Schemas.Lot.LotId.Name).Trim(); }
        }

        private async void OnFormLoad(object sender, EventArgs e)
        {
            if (IsDesignTime() || this.DesignMode || this.lot == null) { return; }
            this.Text = "Scrap — " + this.LotId;
            this.txtDescription.Text = Services.DefaultDescriptions.ScrapLot;
            SourceDefinitions(this.lot.Table).Apply(this.fieldSource);
            this.fieldSource.SetRow(this.lot);
            ConfigureCodes(this.cboCode);
            this.BindWafers();
            this.SetLookupReady(false);
            if (string.IsNullOrWhiteSpace(this.CurrentFacId))
            {
                this.lblStatus.Text = "Sign in with a facility to continue.";
                this.ShowErrorMessage("Lookup failed", this.lblStatus.Text, "CurrentFacId is empty.");
                return;
            }
            LoadOutcome<DataTable> outcome = await this.FetchAsync(lookupChannel, () => this.GetScrapReasonCodes());
            if (!outcome.IsCurrent || this.closed || this.IsDisposed) { return; }
            if (outcome.Failure != null)
            {
                this.lblStatus.Text = "Lookup failed. Close and try again.";
                return;
            }
            DataTable codes = outcome.Value;
            if (codes == null)
            {
                this.lblStatus.Text = "The reason codes could not be loaded.";
                this.ShowErrorMessage("Lookup failed", this.lblStatus.Text, "Reason codes: the response has no table.");
                return;
            }
            this.cboCode.DataSource = codes;
            this.cboCode.SelectedIndex = -1;
            this.SetLookupReady(true);
            this.lblStatus.Text = codes.Rows.Count == 0 ? "No scrap reason codes are registered." : string.Empty;
        }

        private void BindWafers()
        {
            this.waferCard.Text = this.idCaption.Replace(" Id", string.Empty) + " List";
            if (this.wafers == null || this.wafers.Rows.Count == 0
                    || !this.wafers.Columns.Contains(Schemas.Wafer.WfId.Name))
            {
                this.waferRows = null;
                this.gridWafers.DataSource = null;
                this.gridWafers.EmptyText = "No units are listed. The whole lot will be scrapped.";
                return;
            }
            WaferColumns(this.wafers, this.idCaption, selectColumn).Bind(this.gridWafers);
            object source = this.gridWafers.DataSource;
            DataView view = source as DataView;
            this.waferRows = view != null ? view.Table : source as DataTable;
            if (this.waferRows != null)
            {
                foreach (DataRow row in this.waferRows.Rows)
                {
                    row[selectColumn] = true;
                }
                this.waferRows.ColumnChanged += this.OnWaferColumnChanged;
            }
        }

        private void OnWaferColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Column != null && e.Column.ColumnName == selectColumn)
            {
                this.QueueSelectionRefresh();
            }
        }

        private List<string> CheckedWaferIds(out int total)
        {
            List<string> ids = new List<string>();
            total = 0;
            if (this.waferRows == null) { return ids; }
            foreach (DataRow row in this.waferRows.Rows)
            {
                if (row.RowState == DataRowState.Deleted) { continue; }
                total++;
                if (row[selectColumn] is bool && (bool)row[selectColumn])
                {
                    string id = TableHelper.CellText(row, Schemas.Wafer.WfId.Name).Trim();
                    if (id.Length > 0) { ids.Add(id); }
                }
            }
            return ids;
        }

        private void SetLookupReady(bool value)
        {
            this.ready = value;
            this.cboCode.Enabled = value;
            this.RefreshSelection();
        }

        private static string SelectedKey(ModernComboBox combo)
        {
            DataRowView selected = combo.SelectedItem as DataRowView;
            if (selected == null || !string.Equals(combo.Text,
                    TableHelper.CellText(selected.Row, combo.DisplayMember), StringComparison.Ordinal))
            {
                return string.Empty;
            }
            return TableHelper.CellText(selected.Row, combo.ValueMember).Trim();
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            this.QueueSelectionRefresh();
        }

        private void OnComboTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.QueueSelectionRefresh();
        }

        private void QueueSelectionRefresh()
        {
            if (this.closed || this.IsDisposed || !this.IsHandleCreated || this.refreshPending) { return; }
            this.refreshPending = true;
            this.BeginInvoke(new MethodInvoker(delegate
            {
                this.refreshPending = false;
                if (!this.closed && !this.IsDisposed) { this.RefreshSelection(); }
            }));
        }

        private void RefreshSelection()
        {
            int total;
            List<string> ids = this.CheckedWaferIds(out total);
            bool whole = total == 0 || ids.Count == total;
            this.lblScope.Text = total == 0 ? "Whole lot"
                    : whole ? "Whole lot — all " + total + " selected"
                    : ids.Count == 0 ? "No units selected"
                    : "Partial — " + ids.Count + " of " + total + " selected";
            this.btnOk.Enabled = this.ready && !this.closed && this.LotId.Length > 0
                    && SelectedKey(this.cboCode).Length > 0
                    && (total == 0 || ids.Count > 0);
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            this.RefreshSelection();
            if (!this.btnOk.Enabled) { return; }
            int total;
            List<string> ids = this.CheckedWaferIds(out total);
            this.WholeLot = total == 0 || ids.Count == total;
            this.WaferIds = this.WholeLot ? new string[0] : ids.ToArray();
            this.ReasonCode = SelectedKey(this.cboCode);
            this.Description = (this.txtDescription.Text ?? string.Empty).Trim();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            this.closed = true;
            this.ready = false;
            if (this.waferRows != null)
            {
                this.waferRows.ColumnChanged -= this.OnWaferColumnChanged;
            }
            this.InvalidateChannel(lookupChannel);
        }
    }
}
