using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class DurableManualOutDialogForm : ModernFormBase
    {
        private const string LookupHint = "Close and try again, or contact support.";

        private readonly DurableManualOutDialogOptions options;
        private DataTable portData;

        public DurableManualOutDialogForm()
        {
            this.InitializeComponent();
            this.InitializeModernForm(false);
        }

        public DurableManualOutDialogForm(DurableManualOutDialogOptions options) : this()
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string PortNm { get; private set; }

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
            this.fieldSource.DefineFields(DurableDialogHelpers.TransportFields());
            this.fieldSource.SetRow(this.options.Source);
            this.txtDescription.Text = this.options.DefaultDescription ?? string.Empty;
            this.gridPorts.RowKeyMember = Schemas.PortDet.PortNm.Name;

            this.LoadPorts();
            this.RefreshState();
        }

        private string StockerId()
        {
            return Schemas.Durable.EqpId.Text(this.options.Source);
        }

        private void LoadPorts()
        {
            string eqpId = this.StockerId();
            this.portData = null;
            this.gridPorts.DataSource = null;

            if (eqpId.Length == 0)
            {
                return;
            }

            DataTable ports;

            try
            {
                ports = this.GetPorts(this.options.Source);
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage("Port lookup failed", "Ports of " + eqpId + " could not be loaded. " + LookupHint, this.ServerFailureText(ex));
                return;
            }

            if (ports == null)
            {
                this.ShowErrorMessage("Port lookup failed", "Ports of " + eqpId + " could not be loaded. " + LookupHint, "GetEqpPortList — the response has no table");
                return;
            }

            this.portData = ports;
            PortColumns(this.portData).Apply(this.gridPorts);
            this.gridPorts.DataSource = this.portData;
            this.gridPorts.SelectedIndex = -1;
        }

        private void OnPortSelectionChanged(object sender, EventArgs e)
        {
            this.RefreshState();
        }

        private DataRow SelectedPort()
        {
            DataRowView selected = this.gridPorts.SelectedItem as DataRowView;

            return selected == null ? null : selected.Row;
        }

        private void RefreshState()
        {
            if (this.options == null)
            {
                return;
            }

            string eqpId = this.StockerId();
            DataRow port = this.SelectedPort();
            this.btnOk.Enabled = eqpId.Length > 0 && port != null;

            if (eqpId.Length == 0)
            {
                this.lblStatus.Text = "The durable is not in an STB or a stocker.";
                return;
            }

            if (port == null)
            {
                this.lblStatus.Text = this.portData == null || this.portData.Rows.Count == 0
                        ? "No out port of " + eqpId + " is available."
                        : "Select an out port.";
                return;
            }

            this.lblStatus.Text = eqpId + " · " + Schemas.PortDet.PortNm.Text(port);
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            if (this.options == null || !this.btnOk.Enabled)
            {
                return;
            }

            DataRow port = this.SelectedPort();

            if (port == null)
            {
                this.ShowWarningMessage("Select an out port first.");
                return;
            }

            this.PortNm = Schemas.PortDet.PortNm.Text(port);
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
