using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class CarrierMoveDialogForm : ModernFormBase
    {
        private readonly string verb;
        private readonly string durableTyp;
        private readonly DataRow source;
        private readonly DataRow target;
        private readonly DataTable plan;

        private const int MinVisibleRows = 3;
        private const int MaxVisibleRows = 10;
        private const int GridRowHeight = 28;
        private const int GridHeaderHeight = 36;
        private const int GridStatusHeight = 28;
        private const int GridChromeWidth = 24;
        private const int GridChromeHeight = 8;

        public CarrierMoveDialogForm()
        {
            this.InitializeComponent();
            this.InitializeModernForm(false);
            this.fieldSummary.Columns = 5;
            this.summaryCard.BackColor = Modern.Lab.Theming.ModernTheme.Surface;
        }

        public CarrierMoveDialogForm(string verb, string durableTyp, DataRow source, DataRow target, DataTable plan) : this()
        {
            this.verb = verb ?? string.Empty;
            this.durableTyp = durableTyp ?? string.Empty;
            this.source = source;
            this.target = target;
            this.plan = plan;

            if (plan != null)
            {
                this.FitToPlan();
            }
        }

        public string Description { get; private set; }

        private void OnFormLoad(object sender, EventArgs e)
        {
            if (IsDesignTime() || this.DesignMode || this.plan == null)
            {
                return;
            }

            string sourceId = Modern.Lab.Data.TableHelper.CellText(this.source, Schemas.Durable.DurableId.Name);
            string targetId = Modern.Lab.Data.TableHelper.CellText(this.target, Schemas.Durable.DurableId.Name);
            this.fieldSource.DefineFields(DurableDialogHelpers.SourceFields());
            this.fieldSource.SetRow(this.source);
            this.fieldTarget.DefineFields(DurableDialogHelpers.SourceFields());
            this.fieldTarget.SetRow(this.target);
            this.Text = "Carrier Editor — " + this.verb;
            this.btnOk.Text = this.verb;
            this.txtDescription.Text = this.DefaultDescription();
            this.lblNote.Text = string.Equals(this.verb, AlignVerb, StringComparison.Ordinal)
                    ? "This commits the new slots in " + targetId + "."
                    : "This commits the target carrier " + targetId + ".";

            DataTable summary = BuildSummary(sourceId, targetId, this.durableTyp, this.plan.Rows.Count);
            SummaryDefinitions(summary).Apply(this.fieldSummary);
            this.fieldSummary.SetRow(summary.Rows[0]);

            PlanColumns(this.plan, this.durableTyp).Bind(this.gridPlan);
            this.gridPlan.SelectRow(delegate(object item) { return true; });
            this.gridPlan.SelectedItem = null;
        }

        private const string AlignVerb = "Align";

        private string DefaultDescription()
        {
            if (string.Equals(this.verb, "Exchange", StringComparison.Ordinal))
            {
                return Services.DefaultDescriptions.ExchangeDurable;
            }

            if (string.Equals(this.verb, AlignVerb, StringComparison.Ordinal))
            {
                return Services.DefaultDescriptions.AlignDurable;
            }

            return string.Equals(this.durableTyp, Schemas.Durable.DurableTyp.Tray.Value, StringComparison.Ordinal)
                    ? Services.DefaultDescriptions.MoveChipLamella
                    : Services.DefaultDescriptions.MoveWafer;
        }

        private void FitToPlan()
        {
            int rows = Math.Min(Math.Max(this.plan.Rows.Count, MinVisibleRows), MaxVisibleRows);
            int gridWidth = (int)Math.Ceiling(PlanWidth(this.plan, this.durableTyp)) + GridChromeWidth;
            int gridHeight = GridHeaderHeight + rows * GridRowHeight + GridStatusHeight + GridChromeHeight;

            this.ClientSize = new Size(
                    gridWidth + this.Padding.Horizontal,
                    this.Padding.Vertical + this.summaryPanel.Height + gridHeight + this.descriptionPanel.Height + this.buttonPanel.Height);
        }

        private static DataTable BuildSummary(string sourceId, string targetId, string durableTyp, int count)
        {
            DataTable summary = new DataTable();
            summary.Locale = CultureInfo.InvariantCulture;
            summary.Columns.Add(SourceColumn, typeof(string));
            summary.Columns.Add(TargetColumn, typeof(string));
            summary.Columns.Add(Schemas.Durable.DurableTyp.Name, typeof(string));
            summary.Columns.Add(CountColumn, typeof(string));
            summary.Rows.Add(sourceId, targetId, durableTyp, count.ToString("N0", CultureInfo.InvariantCulture));
            return summary;
        }

        private void OnOkClick(object sender, EventArgs e)
        {
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
