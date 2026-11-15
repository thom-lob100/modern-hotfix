using System.Data;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class TransportChangeDestDialogForm
    {
        // EQP_ID
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
