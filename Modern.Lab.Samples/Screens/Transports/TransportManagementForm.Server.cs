using System.Data;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class TransportManagementForm
    {
        // DURABLE_ID
        private DataTable GetTransports(RequestParameters parameters)
        {
            return this.RequestFields("GetManualTransportList",
                    "DurableTyp", parameters.Value(Schemas.Transport.DurableTyp),
                    "DurableId", parameters.Value(Schemas.Transport.DurableId)).Table;
        }

        // SUB_PROD_TYP
        // SLOT_NO
        private DataTable GetTransportWafers(DataRow transport)
        {
            return this.RequestFields("GetDurableSlotList",
                    "DURABLE_ID", Schemas.Transport.DurableId.Text(transport),
                    "SUB_PROD_TYP", "Wafer").Table;
        }

        // SUB_PROD_TYP
        // SLOT_NO
        private DataTable GetTransportChips(DataRow transport)
        {
            return this.RequestFields("GetDurableSlotList",
                    "DURABLE_ID", Schemas.Transport.DurableId.Text(transport),
                    "SUB_PROD_TYP", "Chip").Table;
        }

        // SUB_PROD_TYP
        // SLOT_NO
        // FINGER_ID
        private DataTable GetTransportLamellas(DataRow transport)
        {
            return this.RequestFields("GetDurableSlotList",
                    "DURABLE_ID", Schemas.Transport.DurableId.Text(transport),
                    "SUB_PROD_TYP", "Lamella").Table;
        }

        private DataTable GetTransportHistory(DataRow transport)
        {
            return this.RequestFields("GetTransportHistory",
                    "DurableId", Schemas.Transport.DurableId.Text(transport)).Table;
        }

        private DataActionResult ManualInTransport(DataRow transport, RequestParameters parameters)
        {
            return this.RequestFields("ManualInDurable",
                    "DURABLE_ID", Schemas.Transport.DurableId.Text(transport),
                    "EQP_ID", parameters.Value(Schemas.EquipmentExt.EqpId),
                    "PORT_NM", parameters.Value(Schemas.PortDet.PortNm),
                    "DESCRIPTION", parameters.Value(Schemas.Criteria.Description),
                    "USER_ID", this.CurrentUserId);
        }

        private DataActionResult ManualOutTransport(DataRow transport, RequestParameters parameters)
        {
            return this.RequestFields("ManualOutDurable",
                    "DURABLE_ID", Schemas.Transport.DurableId.Text(transport),
                    "EQP_ID", Schemas.Durable.EqpId.Text(transport),
                    "PORT_NM", Schemas.Durable.PortNm.Text(transport),
                    "DESCRIPTION", parameters.Value(Schemas.Criteria.Description),
                    "USER_ID", this.CurrentUserId);
        }

        private DataActionResult ManualInCancelTransport(DataRow transport, RequestParameters parameters)
        {
            return this.RequestFields("ManualInCancelDurable",
                    "DURABLE_ID", Schemas.Transport.DurableId.Text(transport),
                    "DESCRIPTION", parameters.Value(Schemas.Criteria.Description),
                    "USER_ID", this.CurrentUserId);
        }

        private DataActionResult ManualOutCancelTransport(DataRow transport, RequestParameters parameters)
        {
            return this.RequestFields("ManualOutCancelDurable",
                    "DURABLE_ID", Schemas.Transport.DurableId.Text(transport),
                    "DESCRIPTION", parameters.Value(Schemas.Criteria.Description),
                    "USER_ID", this.CurrentUserId);
        }

        private DataActionResult ChangeDestTransport(DataRow transport, RequestParameters parameters)
        {
            return this.RequestFields("ChangeDestDurable",
                    "DURABLE_ID", Schemas.Transport.DurableId.Text(transport),
                    "EQP_ID", parameters.Value(Schemas.EquipmentExt.EqpId),
                    "PORT_NM", parameters.Value(Schemas.PortDet.PortNm),
                    "DESCRIPTION", parameters.Value(Schemas.Criteria.Description),
                    "USER_ID", this.CurrentUserId);
        }
    }
}
