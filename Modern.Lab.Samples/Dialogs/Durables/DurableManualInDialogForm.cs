using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Data;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class DurableManualInDialogForm : ModernFormBase
    {
        private const string LookupHint = "Close and try again, or contact support.";
        private const string NoEquipmentText = "No equipment is ready for Manual In.";

        private readonly DurableManualInDialogOptions options;
        private DataTable equipmentData;
        private DataTable portData;

        public DurableManualInDialogForm()
        {
            this.InitializeComponent();
            this.InitializeModernForm(false);
        }

        public DurableManualInDialogForm(DurableManualInDialogOptions options) : this()
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
            this.fieldSource.DefineFields(DurableDialogHelpers.SourceFields());
            this.fieldSource.SetRow(this.options.Source);
            this.fieldLot.DefineFields(LotFields());
            this.txtDescription.Text = this.options.DefaultDescription ?? string.Empty;
            this.gridEquipments.RowKeyMember = Schemas.EquipmentExt.EqpId.Name;
            this.gridPorts.RowKeyMember = Schemas.PortDet.PortNm.Name;

            this.LoadLot();
            this.LoadEquipments();
            this.RefreshState();
        }

        private void LoadLot()
        {
            string lotId = FirstLotId(this.options.Contents);

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

        private static string FirstLotId(DataTable contents)
        {
            if (contents == null)
            {
                return string.Empty;
            }

            foreach (DataRow row in contents.Rows)
            {
                string lotId = TableHelper.CellText(row, Schemas.DurableWafer.LotId.Name).Trim();

                if (lotId.Length > 0)
                {
                    return lotId;
                }
            }

            return string.Empty;
        }

        private void LoadEquipments()
        {
            DataTable equipments;

            try
            {
                equipments = this.GetEquipments();
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage("Equipment lookup failed", "Equipments could not be loaded. " + LookupHint, this.ServerFailureText(ex));
                return;
            }

            if (equipments == null)
            {
                this.ShowErrorMessage("Equipment lookup failed", "Equipments could not be loaded. " + LookupHint, "GetEqpList — the response has no table");
                return;
            }

            this.equipmentData = Candidates(equipments, EquipmentRules);
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

            this.portData = Candidates(ports, PortRules);
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
                        ? NoEquipmentText
                        : "Select an equipment.";
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
                this.ShowWarningMessage(equipment == null ? "Select an equipment first." : "Select a port first.");
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
