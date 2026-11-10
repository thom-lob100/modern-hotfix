using System.Data;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class DurableManagementForm
    {
        // TYP_VAL
        private DataTable GetDurableTypes()
        {
            return this.RequestFields("GetDurableTypeList").Table;
        }

        // TYP_VAL
        private DataTable GetSendFacilities()
        {
            return this.RequestFields("GetSendFacList").Table;
        }

        // DURABLE_ID
        // LOT_ID
        // REQ_SERIAL_NO
        private DataTable GetDurableCandidates(RequestParameters parameters)
        {
            return this.RequestFields("GetDurableCandidates",
                    "DURABLE_TYP", parameters.Value(Schemas.Durable.DurableTyp),
                    "FR_FAB_ID", parameters.Value(Schemas.Durable.FrFabId)).Table;
        }

        // DURABLE_ID
        // LOT_QTY
        // CAPA
        // FOUP_LOT_QTY
        // FOUP_CAPA
        // STUB_LOT_QTY
        // STUB_CAPA
        // LCC_LOT_QTY
        // LCC_CAPA
        // LAST_EVENT_CD
        // EQP_ID
        // PORT_NM
        // TRANS_MOVE_STAT_CD
        // TRANS_LAST_EVENT_DESC
        // MCS_ITG_YN
        // COMM_STAT_TYP
        // EQP_MES_STAT_TYP
        // ACSS_TYP
        // PORT_STAT_NM
        // VIRT_PORT_YN
        // TRANS_YN
        private DataTable GetDurables(RequestParameters parameters)
        {
            return this.RequestFields("GetDurableList",
                    "DURABLE_TYP", parameters.Value(Schemas.Durable.DurableTyp),
                    "FR_FAB_ID", parameters.Value(Schemas.Durable.FrFabId),
                    "DURABLE_ID", parameters.Value(Schemas.Durable.DurableId),
                    "LOT_ID", parameters.Value(Schemas.Lot.LotId),
                    "REQ_SERIAL_NO", parameters.Value(Schemas.Lot.ReqSerialNo)).Table;
        }

        // SUB_PROD_TYP
        // SLOT_NO
        // FINGER_ID
        private DataTable GetDurableSlots(DataRow durable)
        {
            return this.RequestFields("GetDurableSlotList",
                    "DURABLE_ID", Schemas.Durable.DurableId.Text(durable),
                    "SUB_PROD_TYP", string.Empty).Table;
        }

        // SUB_PROD_TYP
        // SLOT_NO
        private DataTable GetDurableWafers(DataRow durable)
        {
            return this.RequestFields("GetDurableSlotList",
                    "DURABLE_ID", Schemas.Durable.DurableId.Text(durable),
                    "SUB_PROD_TYP", "Wafer").Table;
        }

        // SUB_PROD_TYP
        // SLOT_NO
        private DataTable GetDurableChips(DataRow durable)
        {
            return this.RequestFields("GetDurableSlotList",
                    "DURABLE_ID", Schemas.Durable.DurableId.Text(durable),
                    "SUB_PROD_TYP", "Chip").Table;
        }

        // SUB_PROD_TYP
        // SLOT_NO
        // FINGER_ID
        private DataTable GetDurableLamellas(DataRow durable)
        {
            return this.RequestFields("GetDurableSlotList",
                    "DURABLE_ID", Schemas.Durable.DurableId.Text(durable),
                    "SUB_PROD_TYP", "Lamella").Table;
        }

        private DataTable GetDurableHistory(DataRow durable)
        {
            return this.RequestFields("GetDurableHistory",
                    "DURABLE_ID", Schemas.Durable.DurableId.Text(durable)).Table;
        }

        private DataTable GetDurableChipHistory(DataRow durable)
        {
            return this.RequestFields("GetUnitHistory",
                    "DURABLE_ID", Schemas.Durable.DurableId.Text(durable),
                    "SUB_PROD_TYP", "Chip").Table;
        }

        private DataTable GetDurableLamellaHistory(DataRow durable)
        {
            return this.RequestFields("GetUnitHistory",
                    "DURABLE_ID", Schemas.Durable.DurableId.Text(durable),
                    "SUB_PROD_TYP", "Lamella").Table;
        }

        // SUB_CATG_CD
        // DURABLE_TYP
        private DataTable GetSubCategories()
        {
            return this.RequestFields("GetDurableCategoryList").Table;
        }

        private DataActionResult CreateDurable(RequestParameters parameters)
        {
            return this.RequestFields("CreateDurable",
                    "DURABLE_TYP", parameters.Value(Schemas.Durable.DurableTyp),
                    "SUB_CATG_CD", parameters.Value(Schemas.SubCategory.SubCatgCd),
                    "DESCRIPTION", parameters.Value(Schemas.Criteria.Description),
                    "USER_ID", this.CurrentUserId);
        }

        private DataActionResult SetRfId(DataRow durable, RequestParameters parameters)
        {
            return this.RequestFields("SetDurableRfId",
                    "DURABLE_ID", Schemas.Durable.DurableId.Text(durable),
                    "RFID", parameters.Value(Schemas.Durable.RfId),
                    "DESCRIPTION", parameters.Value(Schemas.Criteria.Description),
                    "USER_ID", this.CurrentUserId);
        }

        private DataActionResult HoldDurable(DataRow durable, RequestParameters parameters)
        {
            return this.RequestFields("HoldDurable",
                    "DURABLE_ID", Schemas.Durable.DurableId.Text(durable),
                    "holdCode", parameters.Value(Schemas.ReasonCode.ReasonCd),
                    "engrUserId", parameters.Value(Schemas.User.UserId),
                    "description", parameters.Value(Schemas.Criteria.Description),
                    "USER_ID", this.CurrentUserId);
        }

        private DataActionResult ReleaseDurable(DataRow durable, RequestParameters parameters)
        {
            return this.RequestFields("ReleaseDurable",
                    "DURABLE_ID", Schemas.Durable.DurableId.Text(durable),
                    "releaseCode", parameters.Value(Schemas.ReasonCode.ReasonCd),
                    "engrUserId", parameters.Value(Schemas.User.UserId),
                    "description", parameters.Value(Schemas.Criteria.Description),
                    "USER_ID", this.CurrentUserId);
        }

        private DataActionResult AdjustDurable(DataRow durable, RequestParameters parameters)
        {
            return this.RequestFields("AdjustDurable",
                    "DURABLE_ID", Schemas.Durable.DurableId.Text(durable),
                    "DESCRIPTION", parameters.Value(Schemas.Criteria.Description),
                    "USER_ID", this.CurrentUserId);
        }

        private DataActionResult ScrapDurable(DataRow durable, RequestParameters parameters)
        {
            return this.RequestFields("ScrapDurable",
                    "DURABLE_ID", Schemas.Durable.DurableId.Text(durable),
                    "DESCRIPTION", parameters.Value(Schemas.Criteria.Description),
                    "USER_ID", this.CurrentUserId);
        }

        private DataActionResult CancelScrapDurable(DataRow durable, RequestParameters parameters)
        {
            return this.RequestFields("CancelScrapDurable",
                    "DURABLE_ID", Schemas.Durable.DurableId.Text(durable),
                    "DESCRIPTION", parameters.Value(Schemas.Criteria.Description),
                    "USER_ID", this.CurrentUserId);
        }

        private DataActionResult DeassignDurable(DataRow durable, RequestParameters parameters)
        {
            return this.RequestFields("DeassignDurable",
                    "DurableId", Schemas.Durable.DurableId.Text(durable),
                    "LotIds", parameters.Value(Schemas.Criteria.LotIds),
                    "description", parameters.Value(Schemas.Criteria.Description),
                    "UserId", this.CurrentUserId);
        }

        private DataActionResult AssignDurable(DataRow durable, RequestParameters parameters)
        {
            return this.RequestFields("AssignDurable",
                    "DurableId", Schemas.Durable.DurableId.Text(durable),
                    "LotId", parameters.Value(Schemas.Lot.LotId),
                    "SlotNos", parameters.Value(Schemas.Criteria.SlotNos),
                    "WfIds", parameters.Value(Schemas.Criteria.WfIds),
                    "description", parameters.Value(Schemas.Criteria.Description),
                    "UserId", this.CurrentUserId);
        }

        private DataActionResult ManualInDurable(DataRow durable, RequestParameters parameters)
        {
            return this.RequestFields("ManualInDurable",
                    "DURABLE_ID", Schemas.Durable.DurableId.Text(durable),
                    "EQP_ID", parameters.Value(Schemas.EquipmentExt.EqpId),
                    "PORT_NM", parameters.Value(Schemas.PortDet.PortNm),
                    "DESCRIPTION", parameters.Value(Schemas.Criteria.Description),
                    "USER_ID", this.CurrentUserId);
        }

        private DataActionResult ManualOutDurable(DataRow durable, RequestParameters parameters)
        {
            return this.RequestFields("ManualOutDurable",
                    "DURABLE_ID", Schemas.Durable.DurableId.Text(durable),
                    "EQP_ID", Schemas.Durable.EqpId.Text(durable),
                    "PORT_NM", Schemas.Durable.PortNm.Text(durable),
                    "DESCRIPTION", parameters.Value(Schemas.Criteria.Description),
                    "USER_ID", this.CurrentUserId);
        }

        private DataActionResult ManualInCancelDurable(DataRow durable, RequestParameters parameters)
        {
            return this.RequestFields("ManualInCancelDurable",
                    "DURABLE_ID", Schemas.Durable.DurableId.Text(durable),
                    "DESCRIPTION", parameters.Value(Schemas.Criteria.Description),
                    "USER_ID", this.CurrentUserId);
        }

        private DataActionResult ManualOutCancelDurable(DataRow durable, RequestParameters parameters)
        {
            return this.RequestFields("ManualOutCancelDurable",
                    "DURABLE_ID", Schemas.Durable.DurableId.Text(durable),
                    "DESCRIPTION", parameters.Value(Schemas.Criteria.Description),
                    "USER_ID", this.CurrentUserId);
        }
    }
}
