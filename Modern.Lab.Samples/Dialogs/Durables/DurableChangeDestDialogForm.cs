using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Dialogs;
using Modern.Lab.WinForms.Controls.Selection;

namespace Modern.Lab.Samples
{
    public partial class DurableChangeDestDialogForm : ModernFormBase
    {
        private const string LookupHint = "Close and try again, or contact support.";
        private const string NoTransportText = "No transport can change its destination.";
        private const string MachineKindText = "Machine";
        private const string StbKindText = "STB";
        private const string StockerKindText = "Stocker";
        private const string NoValueText = "-";

        private readonly DurableChangeDestDialogOptions options;
        private DataTable transportData;
        private DataTable destinationData;
        private DataTable portData;
        private string storageKindText;
        private bool suppressKindEvent;
        private bool suppressTypeEvent;

        public DurableChangeDestDialogForm()
        {
            this.InitializeComponent();
            this.InitializeModernForm(false);
            SearchBandLayout.Attach(this.typePanel,
                    new SearchBandLayout.Field[] { SearchBandLayout.Pair(this.lblType, this.cboType) },
                    new Control[0],
                    HorizontalAlignment.Right);
            SearchBandLayout.Attach(this.destinationPanel,
                    new SearchBandLayout.Field[]
                    {
                        SearchBandLayout.Pair(this.lblDestinationKind, this.cboDestinationKind),
                        SearchBandLayout.Pair(this.lblDestination, this.cboDestination),
                        SearchBandLayout.Pair(this.lblPort, this.cboPort),
                    },
                    new Control[0],
                    HorizontalAlignment.Right);
            SearchBandLayout.Attach(this.inputPanel,
                    new SearchBandLayout.Field[] { SearchBandLayout.Pair(this.lblDescription, this.txtDescription) },
                    new Control[0],
                    HorizontalAlignment.Right);
            this.inputPanel.Resize += this.OnDescriptionBandChanged;
            this.txtDescription.LocationChanged += this.OnDescriptionBandChanged;
            this.destinationPanel.SizeChanged += this.OnBandHeightChanged;
            this.inputPanel.SizeChanged += this.OnBandHeightChanged;
        }

        private void OnBandHeightChanged(object sender, EventArgs e)
        {
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                this.BeginInvoke(new MethodInvoker(this.PerformLayout));
            }
        }

        private void OnDescriptionBandChanged(object sender, EventArgs e)
        {
            int width = this.inputPanel.ClientSize.Width - this.inputPanel.Padding.Right - this.txtDescription.Left;

            if (width > 0 && width != this.txtDescription.Width)
            {
                this.txtDescription.Width = width;
            }
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
            this.cboType.UseEntity(Schemas.CommonTypeDet);
            this.cboDestination.UseEntity(Schemas.EquipmentExt);
            this.cboPort.UseEntity(Schemas.PortDet);

            this.LoadTypes();
            this.ApplyKindChoices();
            this.LoadTransports();
            this.RefreshState();
        }

        private void LoadTypes()
        {
            DataTable types;

            try
            {
                types = this.GetDurableTypes();
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage("Type lookup failed", "Durable types could not be loaded. " + LookupHint, this.ServerFailureText(ex));
                return;
            }

            if (types == null)
            {
                this.ShowErrorMessage("Type lookup failed", "Durable types could not be loaded. " + LookupHint, "GetDurableTypeList — the response has no table");
                return;
            }

            this.suppressTypeEvent = true;

            try
            {
                this.cboType.DataSource = types;
                this.cboType.SelectedValue = Schemas.Transport.DurableTyp.Foup.Value;
            }
            finally
            {
                this.suppressTypeEvent = false;
            }
        }

        private string SelectedType()
        {
            return this.cboType.SelectedValue == null ? string.Empty : Convert.ToString(this.cboType.SelectedValue);
        }

        private void OnTypeChanged(object sender, EventArgs e)
        {
            if (this.suppressTypeEvent)
            {
                return;
            }

            this.ApplyKindChoices();
            this.LoadTransports();
            this.RefreshState();
        }

        private void LoadTransports()
        {
            this.transportData = null;
            this.gridTransports.DataSource = null;
            this.destinationData = null;
            this.cboDestination.DataSource = null;
            this.portData = null;
            this.cboPort.DataSource = null;
            this.btnOk.Enabled = false;

            string durableTyp = this.SelectedType();

            if (durableTyp.Length == 0)
            {
                return;
            }

            DataTable transports;

            try
            {
                transports = this.GetTransports(RequestParameters.Of(Schemas.Transport.DurableTyp, durableTyp));
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

            this.transportData = transports;
            TransportColumns(this.transportData).Apply(this.gridTransports);
            this.gridTransports.DataSource = this.transportData;
        }

        private void OnTransportSelectionChanged(object sender, EventArgs e)
        {
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
            string durableTyp = this.SelectedType();
            string storage = UsesStb(durableTyp) ? StbKindText : StockerKindText;
            this.cboDestinationKind.Enabled = durableTyp.Length > 0;

            if (string.Equals(storage, this.storageKindText, StringComparison.Ordinal))
            {
                return;
            }

            int kind = this.cboDestinationKind.SelectedIndex < 0 ? 0 : this.cboDestinationKind.SelectedIndex;
            this.storageKindText = storage;
            this.suppressKindEvent = true;

            try
            {
                this.cboDestinationKind.DataSource = new string[] { MachineKindText, storage };
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
            DataRow transport = this.SelectedTransport();
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

        private DataRow SelectedTransport()
        {
            return SelectedRow(this.gridTransports);
        }

        private DataRow SelectedDestination()
        {
            return SelectedRow(this.cboDestination);
        }

        private DataRow SelectedPort()
        {
            return SelectedRow(this.cboPort);
        }

        private static DataRow SelectedRow(ModernDataGrid grid)
        {
            DataRowView selected = grid.SelectedItem as DataRowView;

            return selected == null ? null : selected.Row;
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

            DataRow transport = this.SelectedTransport();
            DataRow destination = this.SelectedDestination();
            DataRow port = this.SelectedPort();
            this.ApplyActionReason(this.btnOk, ChangeDestinationReason(transport, destination, port));

            if (transport == null)
            {
                this.lblStatus.Text = IsEmpty(this.transportData) ? NoTransportText : "Select a transport.";
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

            this.lblStatus.Text = Schemas.Transport.DurableId.Text(transport) + " → " + Place(eqpId, Schemas.PortDet.PortNm.Text(port));
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

            return ActionRule.Check(ChangeDestinationRules, RuleRow(transport, port));
        }

        protected override DataRow RuleGuideRow(DataRow selectedRow)
        {
            return RuleRow(this.SelectedTransport(), this.SelectedPort());
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
            return "Durable Id:\t" + Shown(Schemas.Transport.DurableId.Text(transport))
                    + "\n" + "Lot Id:\t" + Shown(Schemas.Transport.LotId.Text(transport))
                    + "\n" + "Destination:\t" + Place(Schemas.Transport.EndGoalVal.Text(transport), Schemas.Transport.EndLocCd.Text(transport))
                    + "\n" + "New Destination:\t" + ModernMessageDialog.Emphasis(Place(eqpId, portNm));
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
