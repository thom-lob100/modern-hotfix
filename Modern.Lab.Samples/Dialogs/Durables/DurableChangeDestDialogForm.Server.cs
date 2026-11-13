using System.Data;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class DurableChangeDestDialogForm
    {
        // DURABLE_ID
        // LOT_ID
        // MOVE_STAT_CD
        // END_GOAL_VAL
        // END_LOC_CD
        // DURABLE_TYP
        // DURABLE_STAT_CD
        private DataTable GetTransports()
        {
            return this.RequestFields("GetManualTransportList").Table;
        }

        // EQP_ID
        // EQP_MAIN_GRP_ID
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
        // CURR_DURABLE_ID
        // VIRT_PORT_YN
        private DataTable GetPorts(DataRow equipment)
        {
            return this.RequestFields("GetEqpPortList",
                    "EqpId", Schemas.EquipmentExt.EqpId.Text(equipment)).Table;
        }
    }
}
