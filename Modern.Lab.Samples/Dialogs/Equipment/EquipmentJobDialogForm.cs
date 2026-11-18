using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Data;
using Modern.Lab.Hosting;
using Modern.Lab.Samples.Services;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.WinForms.Controls.Display;

namespace Modern.Lab.Samples
{
    public partial class EquipmentJobDialogForm : ModernFormBase
    {
        private const string waferChannel = "jobWafers";
        private const string selectColumn = "CHK";
        private readonly EquipmentJobDialogOptions options;
        private DataTable waferRows;
        private bool ready;
        private bool closed;
        private bool refreshPending;

        public EquipmentJobDialogForm()
        {
            this.InitializeComponent();
            this.InitializeModernForm(false);
            this.WfIds = string.Empty;
        }

        public EquipmentJobDialogForm(EquipmentJobDialogOptions options) : this()
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Description { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string WfIds { get; private set; }

        private bool SelectWafers
        {
            get { return this.options != null && this.options.SelectWafers; }
        }

        private async void OnFormLoad(object sender, EventArgs e)
        {
            if (IsDesignTime() || this.DesignMode || this.options == null)
            {
                return;
            }

            this.Text = this.options.Title ?? this.Text;
            this.btnOk.Text = this.options.OkText ?? this.btnOk.Text;
            this.btnOk.TopLabel = this.options.OkTopLabel ?? string.Empty;
            this.sourceCard.Text = this.options.SourceCaption ?? this.Text;
            this.fieldSource.DefineFields(this.options.SourceFields ?? DefaultSourceFields());
            this.fieldSource.SetRow(this.options.Source);
            this.txtRemark.Text = this.options.DefaultDescription ?? string.Empty;

            if (!this.SelectWafers)
            {
                this.waferCard.Visible = false;
                this.lblScope.Visible = false;
                this.ClientSize = new System.Drawing.Size(this.ClientSize.Width,
                        this.sourceCard.Height + this.inputPanel.Height + this.buttonPanel.Height + this.Padding.Vertical);
                return;
            }

            this.ClientSize = new System.Drawing.Size(this.ClientSize.Width * 4 / 3, this.ClientSize.Height);
            this.lblScope.Text = "Loading the list…";
            this.RefreshSelection();
            DataRow source = this.options.Source;
            LoadOutcome<DataTable> outcome = await this.FetchAsync(waferChannel, () => this.GetWafers(source));

            if (!outcome.IsCurrent || this.closed || this.IsDisposed)
            {
                return;
            }

            if (outcome.Failure != null)
            {
                this.lblScope.Text = "The list could not be loaded. Close and try again.";
                return;
            }

            this.BindWafers(outcome.Value);
        }

        internal static ModernFieldDefinition[] DefaultSourceFields()
        {
            return new ModernFieldDefinition[]
            {
                CardFields.Of(Schemas.EquipmentPriority.EqpId),
                CardFields.Of(Schemas.EquipmentPriority.CommStatTyp),
                CardFields.Of(Schemas.LotPriority.PortNm).Caption("Port"),
                CardFields.Of(Schemas.LotPriority.GoalPortNm).Caption("Goal Port"),
                CardFields.Of(Schemas.LotPriority.LotId),
                CardFields.Of(Schemas.LotPriority.DurableId).Caption("Durable Id"),
                CardFields.Of(Schemas.LotPriority.GoalDurableId).Caption("Goal Durable Id"),
                CardFields.Of(Schemas.DurablePriority.DurableTyp).Caption("Goal Durable Type"),
                CardFields.Of(Schemas.LotPriority.LastEventCd)
            };
        }

        private void BindWafers(DataTable wafers)
        {
            DataRow lot = this.options.Target == null ? null : this.options.Target.Lot;
            this.waferCard.Text = WaferCardCaption(lot, wafers);

            if (wafers != null && wafers.Rows.Count == 0)
            {
                this.gridWafers.DataSource = null;
                this.gridWafers.EmptyText = "Nothing is listed for this lot.";
                this.lblScope.Text = "Nothing is listed for this lot.";
                return;
            }

            if (wafers == null || !wafers.Columns.Contains(Schemas.Wafer.WfId.Name))
            {
                this.gridWafers.DataSource = null;
                this.lblScope.Text = "The list could not be read. Close and try again.";
                this.ShowErrorMessage("Lookup failed", this.lblScope.Text, wafers == null
                        ? "The list response has no table."
                        : "The list response has no " + Schemas.Wafer.WfId.Name + " column.");
                return;
            }

            WaferColumns(lot, wafers, selectColumn).Bind(this.gridWafers);
            object bound = this.gridWafers.DataSource;
            DataView view = bound as DataView;
            this.waferRows = view != null ? view.Table : bound as DataTable;

            if (this.waferRows == null)
            {
                return;
            }

            foreach (DataRow row in this.waferRows.Rows)
            {
                row[selectColumn] = true;
            }

            this.waferRows.ColumnChanged += this.OnWaferColumnChanged;
            this.ready = true;
            this.RefreshSelection();
        }

        private void OnWaferColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Column != null && e.Column.ColumnName == selectColumn)
            {
                this.QueueSelectionRefresh();
            }
        }

        private List<string> CheckedWfIds(out int total)
        {
            List<string> ids = new List<string>();
            total = 0;

            if (this.waferRows == null)
            {
                return ids;
            }

            foreach (DataRow row in this.waferRows.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                {
                    continue;
                }

                total++;

                if (row[selectColumn] is bool && (bool)row[selectColumn])
                {
                    string id = TableHelper.CellText(row, Schemas.Wafer.WfId.Name).Trim();

                    if (id.Length > 0)
                    {
                        ids.Add(id);
                    }
                }
            }

            return ids;
        }

        private void QueueSelectionRefresh()
        {
            if (this.closed || this.IsDisposed || !this.IsHandleCreated || this.refreshPending)
            {
                return;
            }

            this.refreshPending = true;
            this.BeginInvoke(new MethodInvoker(delegate
            {
                this.refreshPending = false;

                if (!this.closed && !this.IsDisposed)
                {
                    this.RefreshSelection();
                }
            }));
        }

        private void RefreshSelection()
        {
            if (!this.SelectWafers)
            {
                return;
            }

            int total;
            List<string> ids = this.CheckedWfIds(out total);

            if (this.ready)
            {
                this.lblScope.Text = ids.Count == total ? "Whole lot — all " + total + " selected"
                        : ids.Count == 0 ? "Nothing selected"
                        : "Partial — " + ids.Count + " of " + total + " selected";
            }

            this.btnOk.Enabled = this.ready && !this.closed && total > 0 && ids.Count > 0;
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            if (this.options == null)
            {
                return;
            }

            if (this.SelectWafers)
            {
                this.RefreshSelection();
            }

            if (!this.btnOk.Enabled)
            {
                return;
            }

            if (this.SelectWafers)
            {
                int total;
                List<string> ids = this.CheckedWfIds(out total);
                this.WfIds = ids.Count == total ? string.Empty : string.Join(",", ids.ToArray());
            }

            this.Description = this.txtRemark.Text.Trim();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void OnCancelClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
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

            this.InvalidateChannel(waferChannel);
        }
    }
}
