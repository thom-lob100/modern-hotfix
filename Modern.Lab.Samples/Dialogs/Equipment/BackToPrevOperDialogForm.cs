using System;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Data;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Samples.Services;

namespace Modern.Lab.Samples
{

    public partial class BackToPrevOperDialogForm : ModernFormBase
    {
        private readonly DataRow lot;

        public BackToPrevOperDialogForm()
        {
            this.InitializeComponent();

            this.InitializeModernForm(false);

            this.fieldSource.Columns = 4;
        }

        public BackToPrevOperDialogForm(DataRow lot) : this()
        {
            if (lot == null)
            {
                throw new ArgumentNullException(nameof(lot));
            }

            DataTable copy = lot.Table.Clone();
            copy.Rows.Add(lot.ItemArray);
            this.lot = copy.Rows[0];
            this.CurrentOperId = TableHelper.CellText(this.lot, Schemas.LotPriority.OperId.Name).Trim();
        }

        public string ProdId { get; private set; }

        public string FlowId { get; private set; }

        public string OperId { get; private set; }

        public string CurrentOperId { get; private set; }

        public string Description { get; private set; }

        private void OnFormLoad(object sender, EventArgs e)
        {
            if (IsDesignTime() || this.DesignMode || this.lot == null)
            {
                return;
            }

            SourceDefinitions(this.lot.Table).Apply(this.fieldSource);
            this.fieldSource.SetRow(this.lot);
            this.Text = "Back to Prev Oper — " + TableHelper.CellText(this.lot, Schemas.LotPriority.LotId.Name).Trim();
            this.txtDescription.Text = DefaultDescriptions.BackToPrevOper;
            this.cboProd.DisplayMember = Schemas.Product.DisplayField.Name;
            this.cboProd.ValueMember = Schemas.Product.ValueField.Name;
            this.cboFlow.DisplayMember = Schemas.Flow.DisplayField.Name;
            this.cboFlow.ValueMember = Schemas.Flow.ValueField.Name;
            this.cboOper.DisplayMember = Schemas.Oper.DisplayField.Name;
            this.cboOper.ValueMember = Schemas.Oper.ValueField.Name;

            string prodId = TableHelper.CellText(this.lot, Schemas.LotPriority.ProdId.Name).Trim();
            string flowId = TableHelper.CellText(this.lot, Schemas.LotPriority.FlowId.Name).Trim();

            try
            {
                this.cboProd.DataSource = this.GetProducts();
                this.cboProd.SelectedValue = prodId;
                this.cboFlow.DataSource = prodId.Length == 0 ? null : this.GetFlows(prodId);
                this.cboFlow.SelectedValue = flowId;
                this.cboOper.DataSource = prodId.Length == 0 || flowId.Length == 0
                        ? null
                        : WithoutOper(this.GetOpers(prodId, flowId), this.CurrentOperId);
                this.cboOper.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage("Spec lookup failed", this.ServerFailureText(ex));
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

        private string SelectedValue(Modern.Lab.WinForms.Controls.Selection.ModernComboBox combo)
        {
            return (Convert.ToString(combo.SelectedValue) ?? string.Empty).Trim();
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            string prodId = this.SelectedValue(this.cboProd);
            string flowId = this.SelectedValue(this.cboFlow);
            string operId = this.SelectedValue(this.cboOper);

            if (operId.Length == 0)
            {
                this.ShowWarningMessage("Select an Oper Id.");
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
