using System;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.ResponseContracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.Samples.Contracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Display;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{

    public partial class ChangeSpecDialogForm : ModernFormBase
    {
        private readonly DataRow lot;
        private readonly bool cancelMode;
        private bool cascading;

        public ChangeSpecDialogForm()
        {
            this.InitializeComponent();

            this.InitializeModernForm(false);

            this.fieldSource.Columns = 5;
        }

        public ChangeSpecDialogForm(DataRow lot) : this(lot, false)
        {
        }

        public ChangeSpecDialogForm(DataRow lot, bool cancelMode) : this()
        {
            this.lot = lot;
            this.cancelMode = cancelMode;
            this.CurrentOperId = lot == null ? string.Empty : TableHelper.CellText(lot, Schemas.Lot.OperId.Name).Trim();
        }

        public string ProdId { get; private set; }

        public string FlowId { get; private set; }

        public string OperId { get; private set; }

        public string CurrentOperId { get; private set; }

        public string Description { get; private set; }

        private void OnFormLoad(object sender, EventArgs e)
        {
            if (IsDesignTime() || this.DesignMode)
            {
                return;
            }

            if (this.lot != null)
            {
                SourceDefinitions(this.lot.Table).Apply(this.fieldSource);
                this.fieldSource.SetRow(this.lot);
                this.Text = (this.cancelMode ? LotManagementPresenter.ChangeSpecCancelLabel : "ChangeSpec")
                        + " — " + TableHelper.CellText(this.lot, Schemas.Lot.LotId.Name).Trim();
            }

            if (this.cancelMode)
            {
                this.btnOk.Text = LotManagementPresenter.ChangeSpecCancelLabel;
                this.lblProd.Required = false;
                this.lblFlow.Required = false;
                this.lblOper.Required = false;
                this.cboProd.Enabled = false;
                this.cboFlow.Enabled = false;
                this.cboOper.Enabled = false;
            }

            this.txtDescription.Text = this.cancelMode
                    ? Services.DefaultDescriptions.ChangeSpecLotCancel
                    : Services.DefaultDescriptions.ChangeSpecLot;
            this.cboProd.DisplayMember = Schemas.Product.DisplayField.Name;
            this.cboProd.ValueMember = Schemas.Product.ValueField.Name;
            this.cboFlow.DisplayMember = Schemas.Flow.DisplayField.Name;
            this.cboFlow.ValueMember = Schemas.Flow.ValueField.Name;
            this.cboOper.DisplayMember = Schemas.Oper.DisplayField.Name;
            this.cboOper.ValueMember = Schemas.Oper.ValueField.Name;

            this.cascading = true;

            try
            {
                this.cboProd.DataSource = this.GetProducts();
                this.cboProd.SelectedValue = TableHelper.CellText(this.lot, Schemas.Lot.ProdId.Name).Trim();
                this.LoadFlows(TableHelper.CellText(this.lot, Schemas.Lot.FlowId.Name).Trim());
                this.LoadOpers(TableHelper.CellText(this.lot, Schemas.Lot.OperId.Name).Trim());
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage("Spec lookup failed", this.ServerFailureText(ex));
            }
            finally
            {
                this.cascading = false;
            }
        }

        private void OnProdChanged(object sender, EventArgs e)
        {
            if (IsDesignTime() || this.DesignMode || this.cascading)
            {
                return;
            }

            this.cascading = true;

            try
            {
                this.LoadFlows(string.Empty);
                this.LoadOpers(string.Empty);
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage("Spec lookup failed", this.ServerFailureText(ex));
            }
            finally
            {
                this.cascading = false;
            }
        }

        private void OnFlowChanged(object sender, EventArgs e)
        {
            if (IsDesignTime() || this.DesignMode || this.cascading)
            {
                return;
            }

            this.cascading = true;

            try
            {
                this.LoadOpers(string.Empty);
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage("Spec lookup failed", this.ServerFailureText(ex));
            }
            finally
            {
                this.cascading = false;
            }
        }

        private void LoadFlows(string keepFlowId)
        {
            string prodId = this.SelectedValue(this.cboProd);
            this.ResetOpers();
            this.cboFlow.DataSource = null;

            if (prodId.Length == 0)
            {
                return;
            }

            this.cboFlow.DataSource = this.GetFlows(RequestParameters.Of("PROD_ID", prodId));

            if (keepFlowId.Length > 0)
            {
                this.cboFlow.SelectedValue = keepFlowId;
            }
            else
            {
                this.cboFlow.SelectedIndex = -1;
            }
        }

        private void LoadOpers(string keepOperId)
        {
            string prodId = this.SelectedValue(this.cboProd);
            string flowId = this.SelectedValue(this.cboFlow);
            this.ResetOpers();

            if (prodId.Length == 0 || flowId.Length == 0)
            {
                return;
            }

            DataTable opers = this.GetOpers(RequestParameters.Of("PROD_ID", prodId, "FLOW_ID", flowId));

            if (!this.cancelMode
                    && prodId == TableHelper.CellText(this.lot, Schemas.Lot.ProdId.Name).Trim()
                    && flowId == TableHelper.CellText(this.lot, Schemas.Lot.FlowId.Name).Trim())
            {
                opers = WithoutOper(opers, this.CurrentOperId);
            }

            this.cboOper.DataSource = opers;

            if (keepOperId.Length > 0 && (this.cancelMode || keepOperId != this.CurrentOperId))
            {
                this.cboOper.SelectedValue = keepOperId;
            }
            else
            {
                this.cboOper.SelectedIndex = -1;
            }
        }

        private static DataTable WithoutOper(DataTable opers, string operId)
        {
            if (opers == null || operId.Length == 0 || !opers.Columns.Contains(Schemas.Oper.ValueField.Name))
            {
                return opers;
            }

            for (int index = opers.Rows.Count - 1; index >= 0; index--)
            {
                if (TableHelper.CellText(opers.Rows[index], Schemas.Oper.ValueField.Name).Trim() == operId)
                {
                    opers.Rows.RemoveAt(index);
                }
            }

            return opers;
        }

        private void ResetOpers()
        {
            this.cboOper.DataSource = null;
        }

        private string SelectedValue(Modern.Lab.WinForms.Controls.Selection.ModernComboBox combo)
        {
            return (Convert.ToString(combo.SelectedValue) ?? string.Empty).Trim();
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            if (this.cancelMode)
            {
                if (this.lot == null || TableHelper.CellText(this.lot, Schemas.Lot.LotId.Name).Trim().Length == 0)
                {
                    return;
                }

                this.Description = this.txtDescription.Text ?? string.Empty;
                this.DialogResult = DialogResult.OK;
                this.Close();
                return;
            }

            string prodId = this.SelectedValue(this.cboProd);
            string flowId = this.SelectedValue(this.cboFlow);
            string operId = this.SelectedValue(this.cboOper);

            if (prodId.Length == 0 || flowId.Length == 0 || operId.Length == 0)
            {
                this.ShowWarningMessage("Select Prod Id, Flow Id and Oper Id.");
                return;
            }

            this.ProdId = prodId;
            this.FlowId = flowId;
            this.OperId = operId;
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
