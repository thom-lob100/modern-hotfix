using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Messaging;
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
        private int planCapacity;
        private int planFreeCount;
        private Dictionary<int, string> planOccupied = new Dictionary<int, string>();
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
            this.gridUnits.RowKeyMember = Schemas.Wafer.WfId.Name;
            this.gridUnits.ReadOnly = false;
            this.LoadLots();
            this.RefreshState();
        }

        private void LoadLots()
        {
            DataTable lots;

            try
            {
                lots = this.GetLots(RequestParameters.Of(
                        Schemas.Lot.SubProdTyp.Name, Schemas.Lot.SubProdTyp.Wafer.Value,
                        "CRT_FROM_DT", string.Empty,
                        "CRT_TO_DT", string.Empty,
                        "KEYWORD", string.Empty));
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
                this.cboLot.DataSource = Candidates(lots, this.options.Source, this.options.Contents);
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
            this.planCapacity = 0;
            this.planFreeCount = 0;
            this.planOccupied = new Dictionary<int, string>();
            this.gridUnits.DataSource = null;

            DataRowView selected = this.cboLot.SelectedItem as DataRowView;

            if (selected == null)
            {
                return;
            }

            string lotId = Schemas.Lot.LotId.Text(selected.Row);
            DataTable units;

            try
            {
                units = this.GetLotUnits(RequestParameters.Of().From(Schemas.Lot, selected.Row));
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage(
                        "Unit lookup failed",
                        "Wafers of " + lotId + " could not be loaded. " + LookupHint,
                        this.ServerFailureText(ex));
                return;
            }

            this.planCapacity = Capacity(this.options.Source);
            this.planOccupied = Occupied(this.options.Contents);
            List<int> freeSlots = this.planCapacity > 0 ? FreeSlots(this.planCapacity, this.planOccupied) : null;
            this.planFreeCount = freeSlots == null ? 0 : freeSlots.Count;
            this.plan = BuildPlan(units, freeSlots);
            this.gridUnits.StatusCountFormat = "{0:N0} wafers";
            this.gridUnits.ConfigureColumns(PlanColumns(this.plan));
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

            this.issues = PlanIssues(this.plan, this.planCapacity, this.planOccupied, this.planFreeCount, this.issueRows);
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
                this.lblStatus.Text = this.plan.Rows.Count + " " + WaferWord(this.plan.Rows.Count)
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

            return view != null && this.issueRows.Contains(Schemas.Wafer.WfId.Text(view.Row))
                    ? IssueRowColor()
                    : string.Empty;
        }

        private string IssueMarkerOf(object item)
        {
            DataRowView view = item as DataRowView;

            return view != null && this.issueRows.Contains(Schemas.Wafer.WfId.Text(view.Row))
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

            this.LotId = Schemas.Lot.LotId.Text(selected.Row);
            this.SlotNos = JoinColumn(this.plan, Schemas.Wafer.SlotNo.Name);
            this.WfIds = JoinColumn(this.plan, Schemas.Wafer.WfId.Name);
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
