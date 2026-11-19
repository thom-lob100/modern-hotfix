using System.Data;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class TransportManualInDialogForm
    {
        // LOT_ID
        private DataTable GetLot(RequestParameters parameters)
        {
            return this.RequestFields("GetLotList",
                    "LOT_ID", parameters.Value(Schemas.Lot.LotId)).Table;
        }

        // EQP_ID
        private DataTable GetEquipments(DataRow transport, RequestParameters parameters)
        {
            return this.RequestFields("GetEqpList",
                    "START_GOAL_VAL", Schemas.Durable.EqpId.Text(transport),
                    "DESTINATION", parameters.Value(Schemas.Criteria.Destination)).Table;
        }

        // PORT_NM
        private DataTable GetPorts(DataRow equipment)
        {
            return this.RequestFields("GetEqpPortList",
                    "EqpId", Schemas.EquipmentExt.EqpId.Text(equipment)).Table;
        }
    }
}
