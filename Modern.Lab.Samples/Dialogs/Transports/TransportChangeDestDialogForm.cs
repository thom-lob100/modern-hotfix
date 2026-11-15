using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Dialogs;
using Modern.Lab.WinForms.Controls.Selection;

namespace Modern.Lab.Samples
{
    public partial class TransportChangeDestDialogForm : ModernFormBase
    {
        private const string LookupHint = "Close and try again, or contact support.";
        private const string MachineKindText = "Machine";
        private const string StbKindText = "STB";
        private const string StockerKindText = "Stocker";
        private const string NoValueText = "-";

        private readonly TransportChangeDestDialogOptions options;
        private DataTable destinationData;
        private DataTable portData;
        private string storageKindText;
        private bool suppressKindEvent;

        public TransportChangeDestDialogForm()
        {
            this.InitializeComponent();
            this.InitializeModernForm(false);
        }

        public TransportChangeDestDialogForm(TransportChangeDestDialogOptions options) : this()
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

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
            this.fieldSource.DefineFields(TransportDialogHelpers.RouteFields());
            this.fieldSource.SetRow(this.options.Source);
            this.txtDescription.Text = this.options.DefaultDescription ?? string.Empty;
            this.cboDestination.DisplayMember = Schemas.EquipmentExt.EqpId.Name;
            this.cboDestination.ValueMember = Schemas.EquipmentExt.EqpId.Name;
            this.cboPort.DisplayMember = Schemas.PortDet.PortNm.Name;
            this.cboPort.ValueMember = Schemas.PortDet.PortNm.Name;

            this.ApplyKindChoices();
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

        private void ApplyKindChoices()
        {
            this.storageKindText = UsesStb(this.options.Source) ? StbKindText : StockerKindText;
            this.cboDestinationKind.Enabled = this.options.Source != null;
            this.suppressKindEvent = true;

            try
            {
                this.cboDestinationKind.DataSource = new string[] { MachineKindText, this.storageKindText };
                this.cboDestinationKind.SelectedIndex = 0;
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

        private string SelectedKind()
        {
            return this.StorageSelected() ? this.storageKindText : MachineKindText;
        }

        private string DestinationNoun()
        {
            if (!this.StorageSelected())
            {
                return "machine";
            }

            if (string.Equals(this.storageKindText, StbKindText, StringComparison.Ordinal))
            {
                return "STB";
            }

            return "stocker";
        }

        private void LoadDestinations()
        {
            DataRow transport = this.options.Source;
            this.destinationData = null;

            if (transport == null)
            {
                this.cboDestination.DataSource = null;
                return;
            }

            DataTable equipments;

            try
            {
                equipments = this.GetEquipments(transport, RequestParameters.Of(Schemas.Criteria.Destination, this.SelectedKind()));
            }
            catch (Exception ex)
            {
                this.cboDestination.DataSource = null;
                this.ShowErrorMessage("Destination lookup failed", "Machines, STBs and stockers could not be loaded. " + LookupHint, this.ServerFailureText(ex));
                return;
            }

            if (equipments == null)
            {
                this.cboDestination.DataSource = null;
                this.ShowErrorMessage("Destination lookup failed", "Machines, STBs and stockers could not be loaded. " + LookupHint, "GetEqpList — the response has no table");
                return;
            }

            this.destinationData = equipments;
            this.cboDestination.DataSource = this.destinationData;
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
                this.cboPort.DataSource = null;
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
                this.cboPort.DataSource = null;
                this.ShowErrorMessage("Port lookup failed", "Ports of " + eqpId + " could not be loaded. " + LookupHint, this.ServerFailureText(ex));
                return;
            }

            if (ports == null)
            {
                this.cboPort.DataSource = null;
                this.ShowErrorMessage("Port lookup failed", "Ports of " + eqpId + " could not be loaded. " + LookupHint, "GetEqpPortList — the response has no table");
                return;
            }

            this.portData = ports;
            this.cboPort.DataSource = this.portData;
        }

        private void OnPortSelectionChanged(object sender, EventArgs e)
        {
            this.RefreshState();
        }

        private DataRow SelectedDestination()
        {
            return SelectedRow(this.cboDestination);
        }

        private DataRow SelectedPort()
        {
            return SelectedRow(this.cboPort);
        }

        private static DataRow SelectedRow(ModernComboBox combo)
        {
            DataRowView selected = combo.SelectedItem as DataRowView;

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

            DataRow transport = this.options.Source;
            DataRow destination = this.SelectedDestination();
            DataRow port = this.SelectedPort();
            this.ApplyActionReason(this.btnOk, ChangeDestinationReason(transport, destination, port));

            if (transport == null)
            {
                this.lblStatus.Text = "Select a transport first.";
                return;
            }

            if (destination == null)
            {
                this.lblStatus.Text = IsEmpty(this.destinationData)
                        ? "No other " + this.DestinationNoun() + " is available."
                        : "Select a destination.";
                return;
            }

            string eqpId = Schemas.EquipmentExt.EqpId.Text(destination);

            if (port == null)
            {
                this.lblStatus.Text = IsEmpty(this.portData)
                        ? "No port of " + eqpId + " is ready to load."
                        : "Select a port.";
                return;
            }

            this.lblStatus.Text = Schemas.Transport.DurableId.Text(transport) + " → "
                    + TransportManagementPresenter.Place(eqpId, Schemas.PortDet.PortNm.Text(port));
        }

        private static string ChangeDestinationReason(DataRow transport, DataRow destination, DataRow port)
        {
            if (transport == null)
            {
                return "Select a transport first.";
            }

            if (destination == null)
            {
                return "Select a destination first.";
            }

            if (port == null)
            {
                return "Select a port first.";
            }

            return ActionRule.Check(ChangeDestinationRules, port);
        }

        protected override DataRow RuleGuideRow(DataRow selectedRow)
        {
            return this.SelectedPort();
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            if (this.options == null)
            {
                return;
            }

            DataRow transport = this.options.Source;
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
                    + "\n" + "Destination: " + TransportManagementPresenter.Place(
                            Schemas.Transport.EndGoalVal.Text(transport), Schemas.Transport.EndLocCd.Text(transport))
                    + "\n" + "New Destination: " + ModernMessageDialog.Emphasis(TransportManagementPresenter.Place(eqpId, portNm));
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
