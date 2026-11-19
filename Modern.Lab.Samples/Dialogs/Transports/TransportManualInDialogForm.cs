using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class TransportManualInDialogForm : ModernFormBase
    {
        private const string LookupHint = "Close and try again, or contact support.";
        private const string StbKindText = "STB";
        private const string StockerKindText = "Stocker";

        private readonly TransportManualInDialogOptions options;
        private DataTable equipmentData;
        private DataTable portData;

        public TransportManualInDialogForm()
        {
            this.InitializeComponent();
            this.InitializeModernForm(false);
        }

        public TransportManualInDialogForm(TransportManualInDialogOptions options) : this()
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
            this.fieldSource.DefineFields(TransportDialogHelpers.SourceFields());
            this.fieldSource.SetRow(this.options.Source);
            this.fieldLot.DefineFields(LotFields());
            this.txtDescription.Text = this.options.DefaultDescription ?? string.Empty;
            this.gridEquipments.RowKeyMember = Schemas.EquipmentExt.EqpId.Name;
            this.gridEquipments.EmptyText = "No " + this.DestinationNoun() + " is ready for Manual In";
            this.gridEquipments.StatusCountFormat = "{0:N0} " + this.DestinationNoun() + "s";
            this.gridPorts.RowKeyMember = Schemas.PortDet.PortNm.Name;
            this.gridPorts.EmptyText = "Select " + this.DestinationArticle() + " to list its ports";

            this.LoadLot();
            this.LoadEquipments();
            this.RefreshState();
        }

        private static bool UsesStb(DataRow transport)
        {
            return transport != null && Schemas.Transport.DurableTyp.Is(transport, Schemas.Transport.DurableTyp.Foup);
        }

        private string DestinationKind()
        {
            return UsesStb(this.options.Source) ? StbKindText : StockerKindText;
        }

        private string DestinationNoun()
        {
            return UsesStb(this.options.Source) ? "STB" : "stocker";
        }

        private string DestinationArticle()
        {
            return UsesStb(this.options.Source) ? "an STB" : "a stocker";
        }

        private void LoadLot()
        {
            string lotId = Schemas.Transport.LotId.Text(this.options.Source);

            if (lotId.Length == 0)
            {
                this.lotCard.Text = "Lot — none in this durable";
                return;
            }

            DataTable lots;

            try
            {
                lots = this.GetLot(RequestParameters.Of(Schemas.Lot.LotId, lotId));
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage("Lot lookup failed", "Lot " + lotId + " could not be loaded. " + LookupHint, this.ServerFailureText(ex));
                return;
            }

            if (lots == null || lots.Rows.Count == 0)
            {
                this.lotCard.Text = "Lot " + lotId + " — not found";
                return;
            }

            this.lotCard.Text = "Lot " + lotId;
            this.fieldLot.SetRow(lots.Rows[0]);
        }

        private void LoadEquipments()
        {
            DataTable equipments;
            string failure = this.DestinationKind() + "s could not be loaded. " + LookupHint;

            try
            {
                equipments = this.GetEquipments(this.options.Source, RequestParameters.Of(Schemas.Criteria.Destination, this.DestinationKind()));
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage("Destination lookup failed", failure, this.ServerFailureText(ex));
                return;
            }

            if (equipments == null)
            {
                this.ShowErrorMessage("Destination lookup failed", failure, "GetEqpList — the response has no table");
                return;
            }

            this.equipmentData = equipments;
            EquipmentColumns(this.equipmentData).Apply(this.gridEquipments);
            this.gridEquipments.DataSource = this.equipmentData;
            this.gridEquipments.SelectedIndex = -1;
        }

        private void OnEquipmentSelectionChanged(object sender, EventArgs e)
        {
            this.LoadPorts();
            this.RefreshState();
        }

        private void LoadPorts()
        {
            DataRow equipment = this.SelectedEquipment();
            this.portData = null;
            this.gridPorts.DataSource = null;

            if (equipment == null)
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

            this.portData = ports;
            PortColumns(this.portData).Apply(this.gridPorts);
            this.gridPorts.DataSource = this.portData;
            this.gridPorts.SelectedIndex = -1;
        }

        private void OnPortSelectionChanged(object sender, EventArgs e)
        {
            this.RefreshState();
        }

        private DataRow SelectedEquipment()
        {
            DataRowView selected = this.gridEquipments.SelectedItem as DataRowView;

            return selected == null ? null : selected.Row;
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

            DataRow equipment = this.SelectedEquipment();
            DataRow port = this.SelectedPort();
            this.btnOk.Enabled = equipment != null && port != null;

            if (equipment == null)
            {
                this.lblStatus.Text = this.equipmentData == null || this.equipmentData.Rows.Count == 0
                        ? "No " + this.DestinationNoun() + " is ready for Manual In."
                        : "Select " + this.DestinationArticle() + ".";
                return;
            }

            string eqpId = Schemas.EquipmentExt.EqpId.Text(equipment);

            if (port == null)
            {
                this.lblStatus.Text = this.portData == null || this.portData.Rows.Count == 0
                        ? "No port of " + eqpId + " is ready to load."
                        : "Select a port.";
                return;
            }

            this.lblStatus.Text = eqpId + " · " + Schemas.PortDet.PortNm.Text(port);
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            if (this.options == null)
            {
                return;
            }

            DataRow equipment = this.SelectedEquipment();
            DataRow port = this.SelectedPort();

            if (equipment == null || port == null)
            {
                this.ShowWarningMessage(equipment == null ? "Select " + this.DestinationArticle() + " first." : "Select a port first.");
                return;
            }

            this.EqpId = Schemas.EquipmentExt.EqpId.Text(equipment);
            this.PortNm = Schemas.PortDet.PortNm.Text(port);
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
