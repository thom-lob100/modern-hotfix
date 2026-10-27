using System;
using System.Collections.Generic;
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
        private const int IssueLinesInDialog = 8;

        private readonly DurableAssignDialogOptions options;
        private readonly HashSet<string> issueRows = new HashSet<string>(StringComparer.Ordinal);
        private DataTable plan;
        private string planSubProdTyp = string.Empty;
        private int planCapacity;
        private int planFreeCount;
        private Dictionary<string, string> planOccupied = new Dictionary<string, string>(StringComparer.Ordinal);
        private List<string> issues = new List<string>();
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
                this.cboLot.DataSource = Candidates(lots, types, this.options.Source, this.options.Contents);
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
            this.planCapacity = 0;
            this.planFreeCount = 0;
            this.planOccupied = new Dictionary<string, string>(StringComparer.Ordinal);
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
            this.planCapacity = Capacity(this.options.Source, subProdTyp);
            this.planOccupied = Occupied(this.options.Contents, subProdTyp);
            List<string[]> freePositions = this.planCapacity > 0
                    ? FreePositions(this.planCapacity, subProdTyp, this.planOccupied)
                    : null;
            this.planFreeCount = freePositions == null ? 0 : freePositions.Count;
            this.plan = BuildPlan(units, subProdTyp, freePositions);
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
                this.issues = new List<string>();
                this.issueRows.Clear();
                this.badgeStatus.Visible = false;
                this.btnOk.Enabled = false;
                return;
            }

            this.issues = PlanIssues(
                    this.plan, this.planSubProdTyp, this.planCapacity, this.planOccupied, this.planFreeCount, this.issueRows);
            this.gridUnits.RowColorSelector = item => this.IssueRowColorOf(item);
            this.gridUnits.RowMarkerSelector = item => this.IssueMarkerOf(item);

            if (this.issues.Count > 0)
            {
                this.badgeStatus.Semantic = Modern.Lab.Theming.SemanticKind.Error;
                this.badgeStatus.Text = this.issues.Count == 1 ? "1 issue" : this.issues.Count + " issues";
                this.lblStatus.Text = PlainText(this.issues[0]);
            }
            else
            {
                this.badgeStatus.Semantic = Modern.Lab.Theming.SemanticKind.Success;
                this.badgeStatus.Text = this.plan.Rows.Count + " ready";
                this.lblStatus.Text = this.plan.Rows.Count + " " + UnitWord(this.planSubProdTyp, this.plan.Rows.Count)
                        + " go to " + this.FreeSlotsText() + ".";
            }

            this.badgeStatus.Visible = true;
            this.btnOk.Enabled = this.issues.Count == 0;
        }

        private string FreeSlotsText()
        {
            if (this.planCapacity <= 0)
            {
                return "the slots entered";
            }

            int left = this.planFreeCount - this.plan.Rows.Count;
            return "free slots (" + left + " left)";
        }

        private string IssueRowColorOf(object item)
        {
            DataRowView view = item as DataRowView;

            return view != null && this.issueRows.Contains(Schemas.Wafer.WfId.Text(view.Row).Trim())
                    ? IssueRowColor()
                    : string.Empty;
        }

        private string IssueMarkerOf(object item)
        {
            DataRowView view = item as DataRowView;

            return view != null && this.issueRows.Contains(Schemas.Wafer.WfId.Text(view.Row).Trim())
                    ? IssueMarker
                    : string.Empty;
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            if (this.options == null || this.plan == null)
            {
                return;
            }

            this.gridUnits.EndEdit();
            this.RefreshState();

            if (this.issues.Count > 0)
            {
                this.ShowWarningMessage(IssueSummary(this.issues, IssueLinesInDialog));
                this.gridUnits.SelectRow(item => this.IssueMarkerOf(item).Length > 0);
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
