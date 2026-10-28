using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Data;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Selection;

namespace Modern.Lab.Samples
{
    public partial class DurableAssignDialogForm : ModernFormBase
    {
        private const string LookupHint = "Close and try again, or contact support.";

        private readonly DurableAssignDialogOptions options;
        private DataTable plan;
        private List<int> freeSlots;
        private string loadedLotId = string.Empty;
        private bool refreshPending;

        public DurableAssignDialogForm()
        {
            this.InitializeComponent();
            this.InitializeModernForm(false);
        }

        public DurableAssignDialogForm(DurableAssignDialogOptions options) : this()
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string LotId { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SlotNos { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string WfIds { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Description { get; private set; }

        private void OnFormLoad(object sender, EventArgs e)
        {
            if (IsDesignTime() || this.DesignMode || this.options == null)
            {
                return;
            }

            this.Text = this.options.Title ?? this.Text;
            this.btnOk.Text = this.options.OkText ?? this.btnOk.Text;
            this.fieldSource.DefineFields(DurableDialogHelpers.SourceFields());
            this.fieldSource.SetRow(this.options.Source);
            this.txtDescription.Text = this.options.DefaultDescription ?? string.Empty;
            this.cboLot.UseEntity(Schemas.Lot);

            if (this.cboLot.Child != null)
            {
                this.cboLot.Child.AddHandler(System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent,
                        new System.Windows.Controls.TextChangedEventHandler(this.OnLotTextChanged), true);
            }

            this.gridWafers.RowKeyMember = Schemas.Wafer.WfId.Name;
            this.freeSlots = DurableAssignPresenter.FreeSlots(this.options.Source, this.options.Contents);
            this.LoadLots();
            this.RefreshState();
        }

        private void LoadLots()
        {
            DataTable lots;

            try
            {
                lots = this.GetLots(RequestParameters.Of(Schemas.Lot.SubProdTyp, Schemas.Lot.SubProdTyp.Wafer.Value));
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage("Lot lookup failed", "Lots could not be loaded. " + LookupHint, this.ServerFailureText(ex));
                return;
            }

            if (lots == null)
            {
                this.ShowErrorMessage("Lot lookup failed", "Lots could not be loaded. " + LookupHint, "GetLotList — the response has no table");
                return;
            }

            this.cboLot.DataSource = lots;
            this.cboLot.SelectedIndex = -1;
            this.lblStatus.Text = lots.Rows.Count == 0 ? "No lot was returned." : "Select a lot.";
        }

        private void OnLotChanged(object sender, EventArgs e)
        {
            this.QueueLotRefresh();
        }

        private void OnLotTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.QueueLotRefresh();
        }

        private void QueueLotRefresh()
        {
            if (IsDesignTime() || this.DesignMode || this.options == null
                    || this.IsDisposed || !this.IsHandleCreated || this.refreshPending)
            {
                return;
            }

            this.refreshPending = true;
            this.BeginInvoke(new MethodInvoker(delegate
            {
                this.refreshPending = false;

                if (!this.IsDisposed)
                {
                    this.RefreshLot();
                }
            }));
        }

        private void RefreshLot()
        {
            DataRow lot = this.SelectedLot();
            string lotId = lot == null ? string.Empty : Schemas.Lot.LotId.Text(lot);

            if (!string.Equals(lotId, this.loadedLotId, StringComparison.Ordinal))
            {
                this.loadedLotId = lotId;
                this.LoadWafers(lot);

                if (lot == null)
                {
                    this.lblStatus.Text = "Select a lot.";
                }
            }

            this.RefreshState();
        }

        private DataRow SelectedLot()
        {
            DataRowView selected = this.cboLot.SelectedItem as DataRowView;

            if (selected == null || !string.Equals(this.cboLot.Text,
                    TableHelper.CellText(selected.Row, this.cboLot.DisplayMember), StringComparison.Ordinal))
            {
                return null;
            }

            return selected.Row;
        }

        private void LoadWafers(DataRow lot)
        {
            this.plan = null;
            this.gridWafers.DataSource = null;

            if (lot == null)
            {
                return;
            }

            string lotId = Schemas.Lot.LotId.Text(lot);
            DataTable wafers;

            try
            {
                wafers = this.GetLotWafers(lot);
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage("Wafer lookup failed", "Wafers of " + lotId + " could not be loaded. " + LookupHint, this.ServerFailureText(ex));
                return;
            }

            this.plan = DurableAssignPresenter.BuildPlan(wafers, this.freeSlots);
            this.gridWafers.ConfigureColumns(PlanColumns(this.plan));
            this.gridWafers.DataSource = this.plan;
        }

        private void OnWafersCellValueChanged(object sender, EventArgs e)
        {
            this.RefreshState();
        }

        private void RefreshState()
        {
            if (this.options == null)
            {
                return;
            }

            if (this.plan == null)
            {
                this.btnOk.Enabled = false;
                return;
            }

            int count = this.plan.Rows.Count;
            this.lblStatus.Text = count == 0 ? "The lot has no wafer to assign."
                    : count + (count == 1 ? " wafer" : " wafers") + " to assign.";
            this.btnOk.Enabled = count > 0;
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            if (this.options == null)
            {
                return;
            }

            this.gridWafers.EndEdit();
            DataRow lot = this.SelectedLot();

            if (lot == null || this.plan == null || this.plan.Rows.Count == 0)
            {
                this.ShowWarningMessage(lot == null ? "Select a lot first." : "The lot has no wafer to assign.");
                return;
            }

            string missing = DurableAssignPresenter.FirstWithoutSlot(this.plan);

            if (missing != null)
            {
                this.ShowWarningMessage("Enter a slot for " + missing + ".");
                this.gridWafers.SelectRow(item => item is DataRowView
                        && string.Equals(Schemas.Wafer.WfId.Text(((DataRowView)item).Row), missing, StringComparison.Ordinal));
                return;
            }

            this.LotId = Schemas.Lot.LotId.Text(lot);
            this.SlotNos = DurableAssignPresenter.JoinColumn(this.plan, Schemas.Wafer.SlotNo);
            this.WfIds = DurableAssignPresenter.JoinColumn(this.plan, Schemas.Wafer.WfId);
            this.Description = this.txtDescription.Text ?? string.Empty;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void OnCancelClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
