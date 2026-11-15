using System.Data;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class DurableChangeDestDialogForm
    {
        // TYP_VAL
        private DataTable GetDurableTypes()
        {
            return this.RequestFields("GetDurableTypeList").Table;
        }

        // DURABLE_ID
        // LOT_ID
        // MOVE_STAT_CD
        // START_GOAL_VAL
        // END_GOAL_VAL
        // END_LOC_CD
        // DURABLE_TYP
        // DURABLE_STAT_CD
        private DataTable GetTransports(RequestParameters parameters)
        {
            return this.RequestFields("GetManualTransportList",
                    "DURABLE_TYP", parameters.Value(Schemas.Transport.DurableTyp)).Table;
        }

        // EQP_ID
        // COMM_STAT_TYP
        private DataTable GetEquipments(DataRow transport, RequestParameters parameters)
        {
            return this.RequestFields("GetEqpList",
                    "START_GOAL_VAL", Schemas.Transport.StartGoalVal.Text(transport),
                    "END_GOAL_VAL", Schemas.Transport.EndGoalVal.Text(transport),
                    "DESTINATION", parameters.Value(Schemas.Criteria.Destination)).Table;
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
