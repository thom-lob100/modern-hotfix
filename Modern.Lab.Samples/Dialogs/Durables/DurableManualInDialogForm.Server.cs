using System.Data;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class DurableManualInDialogForm
    {
        // LOT_ID
        private DataTable GetLot(RequestParameters parameters)
        {
            return this.RequestFields("GetLotList",
                    "LOT_ID", parameters.Value(Schemas.Lot.LotId)).Table;
        }

        // EQP_ID
        // COMM_STAT_TYP
        // MCS_ITG_YN
        // MES_STAT_TYP
        // EQP_TYP
        private DataTable GetEquipments()
        {
            return this.RequestFields("GetEqpList").Table;
        }

        // PORT_NM
        // PORT_TYP
        // ACSS_TYP
        // PORT_STAT_NM
        // RESRC_STAT_TYP
        // TRANSFER_STAT_CD
        // CURR_CARRIER_ID
        // VIRT_PORT_YN
        private DataTable GetPorts(DataRow equipment)
        {
            return this.RequestFields("GetEqpPortList",
                    "EqpId", Schemas.EquipmentExt.EqpId.Text(equipment)).Table;
        }
    }
}
