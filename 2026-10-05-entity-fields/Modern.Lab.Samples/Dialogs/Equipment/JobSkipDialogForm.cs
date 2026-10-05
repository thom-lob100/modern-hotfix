using System;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Data;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting.ResponseContracts;
using Modern.Lab.WinForms.Controls.Selection;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class JobSkipDialogForm : ModernFormBase
    {
        private const string lookupChannel = "skipLookups";
        private readonly DataRow lot;
        private readonly DataRow job;
        private bool ready;
        private bool closed;
        private bool refreshPending;

        public JobSkipDialogForm()
        {
            this.InitializeComponent();
            this.InitializeModernForm(false);
            if (this.cboCode.Child != null)
            {
                this.cboCode.Child.AddHandler(System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent,
                        new System.Windows.Controls.TextChangedEventHandler(this.OnComboTextChanged), true);
            }
        }

        public JobSkipDialogForm(DataRow lot, DataRow job = null) : this()
        {
            if (lot == null) { throw new ArgumentNullException(nameof(lot)); }
            DataTable copy = lot.Table.Clone();
            copy.Rows.Add(lot.ItemArray);
            this.lot = copy.Rows[0];
            this.job = job;
            this.LotId = TableHelper.CellText(this.lot, Schemas.LotPriority.LotId.Name).Trim();
            this.OperId = TableHelper.CellText(this.lot, Schemas.LotPriority.OperId.Name).Trim();
        }

        public string LotId { get; private set; }
        public string OperId { get; private set; }
        public string ReasonCode { get; private set; }
        public string Description { get; private set; }

        public JobSkipDialogResult Result
        {
            get
            {
                return this.DialogResult == DialogResult.OK
                        ? new JobSkipDialogResult(this.LotId, this.OperId, this.ReasonCode, this.Description)
                        : null;
            }
        }

        private async void OnFormLoad(object sender, EventArgs e)
        {
            if (IsDesignTime() || this.DesignMode || this.lot == null) { return; }
            this.Text = "Skip — " + this.LotId;
            this.txtDescription.Text = Services.DefaultDescriptions.SkipOper;
            SourceDefinitions(this.lot.Table).Apply(this.fieldSource);
            this.fieldSource.SetRow(this.lot);
            this.jobCard.Visible = this.job != null;
            this.fieldJob.DefineFields(EquipmentLotForm.JobSummaryFields());
            this.fieldJob.SetRow(this.job);
            ConfigureCodes(this.cboCode);
            this.SetLookupReady(false);
            if (string.IsNullOrWhiteSpace(this.CurrentFacId))
            {
                this.lblStatus.Text = "Sign in with a facility to continue.";
                this.ShowErrorMessage("Lookup unavailable", this.lblStatus.Text, "CurrentFacId is empty.");
                return;
            }
            LoadOutcome<DataTable> outcome = await this.FetchAsync(lookupChannel, () => this.GetSkipReasonCodes());
            if (!outcome.IsCurrent || this.closed || this.IsDisposed) { return; }
            if (outcome.Failure != null)
            {
                this.lblStatus.Text = "Lookup failed. Close and try again.";
                return;
            }
            DataTable codes = outcome.Value;
            string missing = codes != null && codes.Rows.Count == 0 ? string.Empty
                    : ResponseColumns.Missing(codes, Schemas.ReasonCode.ReasonCd.Name, Schemas.ReasonCode.CtnDesc.Name);
            if (missing.Length > 0)
            {
                this.lblStatus.Text = "The skip codes could not be loaded.";
                this.ShowErrorMessage("Lookup unavailable", this.lblStatus.Text, "Reason codes: " + missing);
                return;
            }
            this.cboCode.DataSource = codes;
            this.cboCode.SelectedIndex = -1;
            this.SetLookupReady(true);
            this.lblStatus.Text = codes.Rows.Count == 0 ? "No skip codes are registered." : string.Empty;
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
            this.btnOk.Enabled = this.ready && !this.closed
                    && SelectedKey(this.cboCode).Length > 0
                    && this.LotId.Length > 0;
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            this.RefreshSelection();
            if (!this.btnOk.Enabled) { return; }
            this.ReasonCode = SelectedKey(this.cboCode);
            this.Description = this.txtDescription.Text ?? string.Empty;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            this.closed = true;
            this.ready = false;
            this.InvalidateChannel(lookupChannel);
        }
    }
}
