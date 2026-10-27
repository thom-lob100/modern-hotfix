using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.WinForms.Controls.Selection;

namespace Modern.Lab.Samples
{
    public partial class DurableAssignDialogForm : ModernFormBase
    {
        private const string LookupHint = "Close and try again, or contact support.";

        private readonly DurableAssignDialogOptions options;
        private DataTable plan;
        private string planSubProdTyp = string.Empty;
        private bool lotSetup;

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
        public DataTable Plan { get; private set; }

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
            this.gridUnits.RowKeyMember = Schemas.Wafer.WfId.Name;
            this.gridUnits.ReadOnly = false;
            this.LoadLots();
            this.RefreshState();
        }

        private void LoadLots()
        {
            string[] types = CandidateTypes(this.options.Source);
            DataTable lots;

            try
            {
                lots = this.GetLots(string.Join(",", types));
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage(
                        "Lot lookup failed",
                        "Lots could not be loaded. " + LookupHint,
                        this.ServerFailureText(ex));
                return;
            }

            if (lots == null)
            {
                this.ShowErrorMessage("Lot lookup failed", "Lots could not be loaded. " + LookupHint, "GetLotList — the response has no table");
                return;
            }

            this.lotSetup = true;

            try
            {
                this.cboLot.DataSource = Candidates(lots, types);
                this.cboLot.SelectedIndex = -1;
            }
            finally
            {
                this.lotSetup = false;
            }

            this.lblStatus.Text = this.cboLot.Items.Count == 0 ? "No lot is ready to assign." : "Select a lot.";
        }

        private void OnLotChanged(object sender, EventArgs e)
        {
            if (IsDesignTime() || this.DesignMode || this.options == null || this.lotSetup)
            {
                return;
            }

            this.LoadUnits();
            this.RefreshState();
        }

        private void LoadUnits()
        {
            this.plan = null;
            this.planSubProdTyp = string.Empty;
            this.gridUnits.DataSource = null;

            DataRowView selected = this.cboLot.SelectedItem as DataRowView;

            if (selected == null)
            {
                return;
            }

            string lotId = Schemas.Lot.LotId.Text(selected.Row).Trim();
            string subProdTyp = Schemas.Lot.SubProdTyp.Text(selected.Row).Trim();
            DataTable units;

            try
            {
                units = this.GetLotUnits(lotId);
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage(
                        "Unit lookup failed",
                        "Units of " + lotId + " could not be loaded. " + LookupHint,
                        this.ServerFailureText(ex));
                return;
            }

            this.planSubProdTyp = subProdTyp;
            this.plan = BuildPlan(units, subProdTyp);
            this.gridUnits.StatusCountFormat = UnitsCaption(subProdTyp);
            this.gridUnits.ConfigureColumns(PlanColumns(this.plan, subProdTyp));
            this.gridUnits.DataSource = this.plan;
        }

        private void OnUnitsCellValueChanged(object sender, EventArgs e)
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

            string problem = PlanProblem(this.plan, this.planSubProdTyp, Capacity(this.options.Source, this.planSubProdTyp));
            this.lblStatus.Text = problem ?? this.plan.Rows.Count + " unit(s) ready.";
            this.btnOk.Enabled = problem == null;
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            if (this.options == null || this.plan == null)
            {
                return;
            }

            this.gridUnits.EndEdit();
            this.RefreshState();

            if (!this.btnOk.Enabled)
            {
                return;
            }

            DataRowView selected = this.cboLot.SelectedItem as DataRowView;

            if (selected == null)
            {
                this.ShowWarningMessage("Select a lot first.");
                return;
            }

            this.LotId = Schemas.Lot.LotId.Text(selected.Row).Trim();
            this.Plan = NormalizedPlan(this.plan, this.planSubProdTyp);
            this.Description = (this.txtDescription.Text ?? string.Empty).Trim();
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
