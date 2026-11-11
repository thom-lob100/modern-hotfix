using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Samples
{
    public partial class DurableChangeDestDialogForm : ModernFormBase
    {
        private const string LookupHint = "Close and try again, or contact support.";
        private const string NoRequestText = "No Manual In or Manual Out request is waiting.";
        private const string StockerNeedsNoPortText = "A stocker needs no port";
        private const string PortsEmptyText = "Select an equipment to list its ports";

        private readonly DurableChangeDestDialogOptions options;
        private DataTable transportData;
        private DataTable destinationData;
        private DataTable portData;

        public DurableChangeDestDialogForm()
        {
            this.InitializeComponent();
            this.InitializeModernForm(false);
        }

        public DurableChangeDestDialogForm(DurableChangeDestDialogOptions options) : this()
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataRow Transport { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string EqpId { get; private set; }

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
            this.txtDescription.Text = this.options.DefaultDescription ?? string.Empty;
            this.gridTransports.RowKeyMember = Schemas.Transport.DurableId.Name;
            this.gridDestinations.RowKeyMember = Schemas.EquipmentExt.EqpId.Name;
            this.gridPorts.RowKeyMember = Schemas.PortDet.PortNm.Name;

            this.LoadTransports();
            this.RefreshState();
        }

        private void LoadTransports()
        {
            DataTable transports;

            try
            {
                transports = this.GetTransports();
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage("Request lookup failed", "Manual In and Manual Out requests could not be loaded. " + LookupHint, this.ServerFailureText(ex));
                return;
            }

            if (transports == null)
            {
                this.ShowErrorMessage("Request lookup failed", "Manual In and Manual Out requests could not be loaded. " + LookupHint, "GetManualTransportList — the response has no table");
                return;
            }

            this.transportData = Candidates(transports, TransportRules);
            TransportColumns(this.transportData).Apply(this.gridTransports);
            this.gridTransports.DataSource = this.transportData;
            this.gridTransports.SelectedIndex = -1;
        }

        private void OnTransportSelectionChanged(object sender, EventArgs e)
        {
            this.LoadDestinations();
            this.RefreshState();
        }

        private void LoadDestinations()
        {
            DataRow transport = this.SelectedTransport();
            this.destinationData = null;
            this.gridDestinations.DataSource = null;
            this.ClearPorts();

            if (transport == null)
            {
                return;
            }

            bool manualOut = IsManualOut(transport);
            this.gridPorts.Enabled = !manualOut;
            this.gridPorts.EmptyText = manualOut ? StockerNeedsNoPortText : PortsEmptyText;
            this.gridDestinations.StatusCountFormat = manualOut ? "{0:N0} stockers" : "{0:N0} equipments";
            this.gridDestinations.EmptyText = manualOut ? "No other stocker" : "No other equipment is ready for Manual In";

            string subject = manualOut ? "Stockers" : "Equipments";
            string message = manualOut ? "GetStockerList" : "GetEqpList";
            DataTable destinations;

            try
            {
                destinations = manualOut ? this.GetStockers() : this.GetEquipments();
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage(subject + " lookup failed", subject + " could not be loaded. " + LookupHint, this.ServerFailureText(ex));
                return;
            }

            if (destinations == null)
            {
                this.ShowErrorMessage(subject + " lookup failed", subject + " could not be loaded. " + LookupHint, message + " — the response has no table");
                return;
            }

            this.destinationData = manualOut
                    ? OtherThanCurrent(destinations, transport)
                    : OtherThanCurrent(Candidates(destinations, EquipmentRules), transport);
            GridColumns columns = manualOut ? StockerColumns(this.destinationData) : EquipmentColumns(this.destinationData);
            columns.Apply(this.gridDestinations);
            this.gridDestinations.DataSource = this.destinationData;
            this.gridDestinations.SelectedIndex = -1;
        }

        private void OnDestinationSelectionChanged(object sender, EventArgs e)
        {
            this.LoadPorts();
            this.RefreshState();
        }

        private void ClearPorts()
        {
            this.portData = null;
            this.gridPorts.DataSource = null;
        }

        private void LoadPorts()
        {
            DataRow transport = this.SelectedTransport();
            DataRow equipment = this.SelectedDestination();
            this.ClearPorts();

            if (transport == null || equipment == null || IsManualOut(transport))
            {
                return;
            }

            string eqpId = Schemas.EquipmentExt.EqpId.Text(equipment);
            DataTable ports;

            try
            {
                ports = this.GetPorts(equipment);
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

            this.portData = Candidates(ports, PortRules);
            PortColumns(this.portData).Apply(this.gridPorts);
            this.gridPorts.DataSource = this.portData;
            this.gridPorts.SelectedIndex = -1;
        }

        private void OnPortSelectionChanged(object sender, EventArgs e)
        {
            this.RefreshState();
        }

        private DataRow SelectedTransport()
        {
            return SelectedRow(this.gridTransports);
        }

        private DataRow SelectedDestination()
        {
            return SelectedRow(this.gridDestinations);
        }

        private DataRow SelectedPort()
        {
            return SelectedRow(this.gridPorts);
        }

        private static DataRow SelectedRow(ModernDataGrid grid)
        {
            DataRowView selected = grid.SelectedItem as DataRowView;

            return selected == null ? null : selected.Row;
        }

        private static bool IsEmpty(DataTable rows)
        {
            return rows == null || rows.Rows.Count == 0;
        }

        private void RefreshState()
        {
            if (this.options == null)
            {
                return;
            }

            this.btnOk.Enabled = false;
            DataRow transport = this.SelectedTransport();

            if (transport == null)
            {
                this.lblStatus.Text = IsEmpty(this.transportData) ? NoRequestText : "Select a request.";
                return;
            }

            bool manualOut = IsManualOut(transport);
            string durableId = Schemas.Transport.DurableId.Text(transport);
            DataRow destination = this.SelectedDestination();

            if (destination == null)
            {
                this.lblStatus.Text = IsEmpty(this.destinationData)
                        ? (manualOut ? "No other stocker is available." : "No other equipment is ready for Manual In.")
                        : (manualOut ? "Select a stocker." : "Select an equipment.");
                return;
            }

            string eqpId = Schemas.EquipmentExt.EqpId.Text(destination);

            if (manualOut)
            {
                this.btnOk.Enabled = true;
                this.lblStatus.Text = durableId + " → " + eqpId;
                return;
            }

            DataRow port = this.SelectedPort();

            if (port == null)
            {
                this.lblStatus.Text = IsEmpty(this.portData)
                        ? "No port of " + eqpId + " is ready to load."
                        : "Select a port.";
                return;
            }

            this.btnOk.Enabled = true;
            this.lblStatus.Text = durableId + " → " + eqpId + " · " + Schemas.PortDet.PortNm.Text(port);
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            if (this.options == null)
            {
                return;
            }

            DataRow transport = this.SelectedTransport();
            DataRow destination = this.SelectedDestination();

            if (transport == null)
            {
                this.ShowWarningMessage("Select a request first.");
                return;
            }

            bool manualOut = IsManualOut(transport);

            if (destination == null)
            {
                this.ShowWarningMessage(manualOut ? "Select a stocker first." : "Select an equipment first.");
                return;
            }

            DataRow port = manualOut ? null : this.SelectedPort();

            if (!manualOut && port == null)
            {
                this.ShowWarningMessage("Select a port first.");
                return;
            }

            this.Transport = ActionRows.Copy(transport);
            this.EqpId = Schemas.EquipmentExt.EqpId.Text(destination);
            this.PortNm = port == null ? string.Empty : Schemas.PortDet.PortNm.Text(port);
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
