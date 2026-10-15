using System;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Data;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.WinForms.Controls.Selection;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class DurableHoldDialogForm : ModernFormBase
    {
        private const string lookupChannel = "holdLookups";
        private readonly DataRow durable;
        private readonly bool releaseMode;
        private bool ready;
        private bool closed;
        private bool refreshPending;

        public DurableHoldDialogForm()
        {
            this.InitializeComponent();
            this.InitializeModernForm(false);
            if (this.cboCode.Child != null)
            {
                this.cboCode.Child.AddHandler(System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent,
                        new System.Windows.Controls.TextChangedEventHandler(this.OnComboTextChanged), true);
            }
            if (this.cboEngineer.Child != null)
            {
                this.cboEngineer.Child.AddHandler(System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent,
                        new System.Windows.Controls.TextChangedEventHandler(this.OnComboTextChanged), true);
            }
        }

        public DurableHoldDialogForm(DataRow durable, bool releaseMode) : this()
        {
            if (durable == null) { throw new ArgumentNullException(nameof(durable)); }
            DataTable copy = durable.Table.Clone();
            copy.Rows.Add(durable.ItemArray);
            this.durable = copy.Rows[0];
            this.releaseMode = releaseMode;
            this.DurableId = TableHelper.CellText(this.durable, Schemas.Durable.DurableId.Name).Trim();
        }

        public string DurableId { get; private set; }
        public string ReasonCode { get; private set; }
        public string EngrUserId { get; private set; }
        public string Description { get; private set; }

        private async void OnFormLoad(object sender, EventArgs e)
        {
            if (IsDesignTime() || this.DesignMode || this.durable == null) { return; }
            this.Text = (this.releaseMode ? "Release" : "Hold") + " — " + this.DurableId;
            this.lblCode.Text = this.releaseMode ? "Release Code" : "Hold Code";
            this.btnOk.Text = this.releaseMode ? "Release" : "Hold";
            this.txtDescription.Text = this.releaseMode
                    ? Services.DefaultDescriptions.ReleaseDurable
                    : Services.DefaultDescriptions.HoldDurable;
            this.fieldSource.DefineFields(DurableDialogHelpers.SourceFields());
            this.fieldSource.SetRow(this.durable);
            ConfigureCodes(this.cboCode);
            ConfigureEngineers(this.cboEngineer);
            this.SetLookupReady(false);
            if (string.IsNullOrWhiteSpace(this.CurrentFacId))
            {
                this.lblStatus.Text = "Sign in with a facility to continue.";
                this.ShowErrorMessage("Lookup unavailable", this.lblStatus.Text, "CurrentFacId is empty.");
                return;
            }
            LoadOutcome<DataTable[]> outcome = await this.FetchAsync(lookupChannel,
                    () => new DataTable[] { this.GetReasonCodes(this.releaseMode), this.GetEngineers() });
            if (!outcome.IsCurrent || this.closed || this.IsDisposed) { return; }
            if (outcome.Failure != null)
            {
                this.lblStatus.Text = "Lookup failed. Close and try again.";
                return;
            }
            DataTable codes = outcome.Value[0];
            DataTable users = outcome.Value[1];
            if (codes == null || users == null)
            {
                this.lblStatus.Text = "The selection lists could not be loaded.";
                this.ShowErrorMessage("Lookup unavailable", this.lblStatus.Text, "The response has no table.");
                return;
            }
            this.cboCode.DataSource = codes;
            this.cboCode.SelectedIndex = -1;
            this.cboEngineer.DataSource = users;
            this.cboEngineer.SelectedIndex = -1;
            this.SetLookupReady(true);
            this.lblStatus.Text = codes.Rows.Count == 0 ? "No reason codes are registered."
                    : users.Rows.Count == 0 ? "No users are registered." : string.Empty;
        }

        private void SetLookupReady(bool value)
        {
            this.ready = value;
            this.cboCode.Enabled = value;
            this.cboEngineer.Enabled = value;
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
            string engineer = SelectedKey(this.cboEngineer);
            this.EngrUserId = engineer;
            DataRowView row = this.cboEngineer.SelectedItem as DataRowView;
            this.txtEngineerName.Text = engineer.Length == 0 || row == null ? string.Empty
                    : TableHelper.CellText(row.Row, Schemas.User.UserNm.Name);
            this.btnOk.Enabled = this.ready && !this.closed && engineer.Length > 0
                    && SelectedKey(this.cboCode).Length > 0
                    && this.DurableId.Length > 0;
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            this.RefreshSelection();
            if (!this.btnOk.Enabled) { return; }
            this.ReasonCode = SelectedKey(this.cboCode);
            this.EngrUserId = SelectedKey(this.cboEngineer);
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
