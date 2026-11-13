using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Dialogs;

namespace Modern.Lab.Samples
{
    public partial class DurableChangeDestDialogForm : ModernFormBase
    {
        private const string LookupHint = "Close and try again, or contact support.";
        private const string NoTransportText = "No transport can change its destination.";
        private const string PortsEmptyText = "Select a destination to list its ports";
        private const string EquipmentKindText = "Equipment";
        private const string StbKindText = "STB";
        private const string StockerKindText = "Stocker";
        private const string NoValueText = "-";

        private readonly DurableChangeDestDialogOptions options;
        private DataTable transportData;
        private DataTable equipmentData;
        private DataTable destinationData;
        private DataTable portData;
        private string storageKindText;
        private bool suppressKindEvent;

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

            this.ApplyKindChoices(null);
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
                this.ShowErrorMessage("Transport lookup failed", "Transports could not be loaded. " + LookupHint, this.ServerFailureText(ex));
                return;
            }

            if (transports == null)
            {
                this.ShowErrorMessage("Transport lookup failed", "Transports could not be loaded. " + LookupHint, "GetManualTransportList — the response has no table");
                return;
            }

            this.transportData = Candidates(transports, TransportRules);
            TransportColumns(this.transportData).Apply(this.gridTransports);
            this.gridTransports.DataSource = this.transportData;
        }

        private void OnTransportSelectionChanged(object sender, EventArgs e)
        {
            this.ApplyKindChoices(this.SelectedTransport());
            this.LoadDestinations();
            this.RefreshState();
        }

        private void OnDestinationKindChanged(object sender, EventArgs e)
        {
            if (this.suppressKindEvent)
            {
                return;
            }

            this.LoadDestinations();
            this.RefreshState();
        }

        private void ApplyKindChoices(DataRow transport)
        {
            string storage = UsesStb(transport) ? StbKindText : StockerKindText;
            this.cboDestinationKind.Enabled = transport != null;

            if (string.Equals(storage, this.storageKindText, StringComparison.Ordinal))
            {
                return;
            }

            int kind = this.cboDestinationKind.SelectedIndex < 0 ? 0 : this.cboDestinationKind.SelectedIndex;
            this.storageKindText = storage;
            this.suppressKindEvent = true;

            try
            {
                this.cboDestinationKind.DataSource = new string[] { EquipmentKindText, storage };
                this.cboDestinationKind.SelectedIndex = kind;
            }
            finally
            {
                this.suppressKindEvent = false;
            }
        }

        private bool StorageSelected()
        {
            return this.cboDestinationKind.SelectedIndex == 1;
        }

        private string DestinationNoun(bool plural)
        {
            if (!this.StorageSelected())
            {
                return plural ? "equipments" : "equipment";
            }

            if (string.Equals(this.storageKindText, StbKindText, StringComparison.Ordinal))
            {
                return plural ? "STBs" : "STB";
            }

            return plural ? "stockers" : "stocker";
        }

        private void LoadDestinations()
        {
            DataRow transport = this.SelectedTransport();
            this.destinationData = null;
            this.gridDestinations.StatusCountFormat = "{0:N0} " + this.DestinationNoun(true);
            this.gridDestinations.EmptyText = transport == null
                    ? "Select a transport to list its destinations"
                    : "No other " + this.DestinationNoun(false) + " is available";

            if (transport == null)
            {
                this.gridDestinations.DataSource = null;
                return;
            }

            DataTable equipments = this.Equipments();

            if (equipments == null)
            {
                this.gridDestinations.DataSource = null;
                return;
            }

            bool storage = this.StorageSelected();
            this.destinationData = OtherThanCurrent(Candidates(equipments, storage ? StorageRules(transport) : EquipmentRules), transport);
            GridColumns columns = storage ? StorageColumns(this.destinationData) : EquipmentColumns(this.destinationData);
            columns.Apply(this.gridDestinations);
            this.gridDestinations.DataSource = this.destinationData;
        }

        private DataTable Equipments()
        {
            if (this.equipmentData != null)
            {
                return this.equipmentData;
            }

            DataTable equipments;

            try
            {
                equipments = this.GetEquipments();
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage("Destination lookup failed", "Equipments, STBs and stockers could not be loaded. " + LookupHint, this.ServerFailureText(ex));
                return null;
            }

            if (equipments == null)
            {
                this.ShowErrorMessage("Destination lookup failed", "Equipments, STBs and stockers could not be loaded. " + LookupHint, "GetEqpList — the response has no table");
                return null;
            }

            this.equipmentData = equipments;
            return equipments;
        }

        private void OnDestinationSelectionChanged(object sender, EventArgs e)
        {
            this.LoadPorts();
            this.RefreshState();
        }

        private void LoadPorts()
        {
            DataRow destination = this.SelectedDestination();
            this.portData = null;

            if (destination == null)
            {
                this.gridPorts.EmptyText = PortsEmptyText;
                this.gridPorts.DataSource = null;
                return;
            }

            string eqpId = Schemas.EquipmentExt.EqpId.Text(destination);
            DataTable ports;

            try
            {
                ports = this.GetPorts(destination);
            }
            catch (Exception ex)
            {
                this.gridPorts.DataSource = null;
                this.ShowErrorMessage("Port lookup failed", "Ports of " + eqpId + " could not be loaded. " + LookupHint, this.ServerFailureText(ex));
                return;
            }

            if (ports == null)
            {
                this.gridPorts.DataSource = null;
                this.ShowErrorMessage("Port lookup failed", "Ports of " + eqpId + " could not be loaded. " + LookupHint, "GetEqpPortList — the response has no table");
                return;
            }

            this.portData = Candidates(ports, PortRules);
            PortColumns(this.portData).Apply(this.gridPorts);
            this.gridPorts.EmptyText = "No port of " + eqpId + " is ready to load";
            this.gridPorts.DataSource = this.portData;
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
                this.lblStatus.Text = IsEmpty(this.transportData) ? NoTransportText : "Select a transport.";
                return;
            }

            DataRow destination = this.SelectedDestination();

            if (destination == null)
            {
                this.lblStatus.Text = IsEmpty(this.destinationData)
                        ? "No other " + this.DestinationNoun(false) + " is available."
                        : "Select a destination.";
                return;
            }

            string eqpId = Schemas.EquipmentExt.EqpId.Text(destination);
            DataRow port = this.SelectedPort();

            if (port == null)
            {
                this.lblStatus.Text = IsEmpty(this.portData)
                        ? "No port of " + eqpId + " is ready to load."
                        : "Select a port.";
                return;
            }

            this.btnOk.Enabled = true;
            this.lblStatus.Text = Schemas.Transport.DurableId.Text(transport) + " → " + Place(eqpId, Schemas.PortDet.PortNm.Text(port));
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            if (this.options == null)
            {
                return;
            }

            DataRow transport = this.SelectedTransport();
            DataRow destination = this.SelectedDestination();
            DataRow port = this.SelectedPort();

            if (transport == null)
            {
                this.ShowWarningMessage("Select a transport first.");
                return;
            }

            if (destination == null)
            {
                this.ShowWarningMessage("Select a destination first.");
                return;
            }

            if (port == null)
            {
                this.ShowWarningMessage("Select a port first.");
                return;
            }

            string eqpId = Schemas.EquipmentExt.EqpId.Text(destination);
            string portNm = Schemas.PortDet.PortNm.Text(port);

            if (!this.Confirm(ConfirmText(transport, eqpId, portNm), this.Text))
            {
                return;
            }

            this.Transport = ActionRows.Copy(transport);
            this.EqpId = eqpId;
            this.PortNm = portNm;
            this.Description = this.txtDescription.Text ?? string.Empty;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private static string ConfirmText(DataRow transport, string eqpId, string portNm)
        {
            return "Durable Id: " + Shown(Schemas.Transport.DurableId.Text(transport))
                    + "\n" + "Lot Id: " + Shown(Schemas.Transport.LotId.Text(transport))
                    + "\n" + "Destination: " + Place(Schemas.Transport.EndGoalVal.Text(transport), Schemas.Transport.EndLocCd.Text(transport))
                    + "\n" + "New Destination: " + ModernMessageDialog.Emphasis(Place(eqpId, portNm));
        }

        private static string Place(string eqpId, string portNm)
        {
            if (string.IsNullOrEmpty(eqpId))
            {
                return NoValueText;
            }

            return string.IsNullOrEmpty(portNm) ? eqpId : eqpId + " · " + portNm;
        }

        private static string Shown(string value)
        {
            return string.IsNullOrEmpty(value) ? NoValueText : value;
        }

        private void OnCancelClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
