using System.Data;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class LotManagementForm
    {
        // TYP_VAL
        private DataTable GetLotTypes()
        {
            return this.RequestFields("GetLotTypeList").Table;
        }

        // LOT_ID
        // OPER_ID
        // LOT_STAT_TYP
        // MES_PROC_STAT_CD
        // LOT_HOLD_STAT_CD
        // LOT_PROC_STAT_CD
        // LOC_NM
        // EQP_ID
        // LAST_EVENT_CD
        private DataTable GetLots(RequestParameters parameters)
        {
            return this.RequestFields("GetLotList",
                    "SUB_PROD_TYP", parameters.Value(Schemas.Lot.SubProdTyp),
                    "CRT_FROM_DT", parameters.Value(Schemas.Criteria.CrtFromDt),
                    "CRT_TO_DT", parameters.Value(Schemas.Criteria.CrtToDt),
                    "KEYWORD", parameters.Value(Schemas.Criteria.Keyword)).Table;
        }

        // LOT_ID
        private DataTable GetLotIdCandidates(RequestParameters parameters)
        {
            return this.RequestFields("GetLotIdCandidates",
                    "KEYWORD", parameters.Value(Schemas.Criteria.Keyword)).Table;
        }

        // WF_ID
        // SLOT_NO
        private DataTable GetWafers(DataRow lot)
        {
            return this.RequestFields("GetLotUnitList",
                    "LOT_ID", Schemas.Lot.LotId.Text(lot)).Table;
        }

        private DataTable GetLotHistory(DataRow lot)
        {
            return this.RequestFields("GetLotHistory",
                    "LOT_ID", Schemas.Lot.LotId.Text(lot)).Table;
        }

        // DURABLE_ID
        // STUB_LOT_QTY
        // LCC_LOT_QTY
        private DataTable GetTrays()
        {
            return this.RequestFields("GetDurableList",
                    "DURABLE_TYP", "TRAY").Table;
        }

        private DataTable GetWaferHistory(DataRow lot, DataRow wafer)
        {
            return this.RequestFields("GetLotUnitHistory",
                    "LOT_ID", Schemas.Lot.LotId.Text(lot),
                    "WF_ID", Schemas.Wafer.WfId.Text(wafer)).Table;
        }

        private DataActionResult CreateChipLot(DataRow lot, RequestParameters parameters)
        {
            return this.RequestFields("CreateChipLot",
                    "PARENT_LOT_ID", Schemas.Lot.LotId.Text(lot),
                    "TRAY_ID", parameters.Value(Schemas.Durable.DurableId),
                    "USER_ID", this.CurrentUserId,
                    "DESCRIPTION", parameters.Value(Schemas.Criteria.Description),
                    "JUDGE_RESULTS", parameters.Value(Schemas.Criteria.JudgeResults));
        }

        private DataActionResult CreateLamellaLot(DataRow lot, RequestParameters parameters)
        {
            return this.RequestFields("CreateLamellaLot",
                    "PARENT_LOT_ID", Schemas.Lot.LotId.Text(lot),
                    "TRAY_ID", parameters.Value(Schemas.Durable.DurableId),
                    "USER_ID", this.CurrentUserId,
                    "DESCRIPTION", parameters.Value(Schemas.Criteria.Description),
                    "JUDGE_RESULTS", parameters.Value(Schemas.Criteria.JudgeResults));
        }

        private DataActionResult ChangeSpecLot(DataRow lot, RequestParameters parameters)
        {
            return this.RequestFields("ChangeSpecLot",
                    "LOT_ID", Schemas.Lot.LotId.Text(lot),
                    "PROD_ID", parameters.Value(Schemas.Lot.ProdId),
                    "FLOW_ID", parameters.Value(Schemas.Lot.FlowId),
                    "OPER_ID", parameters.Value(Schemas.Lot.OperId),
                    "currentOperId", Schemas.Lot.OperId.Text(lot),
                    "description", parameters.Value(Schemas.Criteria.Description),
                    "USER_ID", this.CurrentUserId);
        }

        private DataActionResult HoldLot(DataRow lot, RequestParameters parameters)
        {
            return this.RequestFields("HoldLot",
                    "LOT_ID", Schemas.Lot.LotId.Text(lot),
                    "currentOperId", Schemas.Lot.OperId.Text(lot),
                    "holdCode", parameters.Value(Schemas.ReasonCode.ReasonCd),
                    "engrUserId", parameters.Value(Schemas.User.UserId),
                    "description", parameters.Value(Schemas.Criteria.Description),
                    "USER_ID", this.CurrentUserId);
        }

        private DataActionResult NotOnHoldLot(DataRow lot, RequestParameters parameters)
        {
            return this.RequestFields("NotOnHoldLot",
                    "LOT_ID", Schemas.Lot.LotId.Text(lot),
                    "currentOperId", Schemas.Lot.OperId.Text(lot),
                    "releaseCode", parameters.Value(Schemas.ReasonCode.ReasonCd),
                    "engrUserId", parameters.Value(Schemas.User.UserId),
                    "description", parameters.Value(Schemas.Criteria.Description),
                    "USER_ID", this.CurrentUserId);
        }

        private DataActionResult CancelChangeSpecLot(DataRow lot, RequestParameters parameters)
        {
            return this.RequestFields("CancelChangeSpecLot",
                    "LOT_ID", Schemas.Lot.LotId.Text(lot),
                    "DESCRIPTION", parameters.Value(Schemas.Criteria.Description),
                    "USER_ID", this.CurrentUserId);
        }

        private DataActionResult CancelScrapLot(DataRow lot, RequestParameters parameters)
        {
            return this.RequestFields("CancelScrapLot",
                    "LOT_ID", Schemas.Lot.LotId.Text(lot),
                    "DESCRIPTION", parameters.Value(Schemas.Criteria.Description),
                    "USER_ID", this.CurrentUserId);
        }

        private DataActionResult ScrapLot(DataRow lot, RequestParameters parameters)
        {
            if (parameters.Value(Schemas.Criteria.WfIds).Length == 0)
            {
                return this.RequestFields("ScrapLot",
                        "LOT_ID", Schemas.Lot.LotId.Text(lot),
                        "scrapCode", parameters.Value(Schemas.ReasonCode.ReasonCd),
                        "description", parameters.Value(Schemas.Criteria.Description),
                        "USER_ID", this.CurrentUserId);
            }

            return this.RequestFields("ScrapLot",
                    "LOT_ID", Schemas.Lot.LotId.Text(lot),
                    "WF_ID", parameters.Value(Schemas.Criteria.WfIds),
                    "scrapCode", parameters.Value(Schemas.ReasonCode.ReasonCd),
                    "description", parameters.Value(Schemas.Criteria.Description),
                    "USER_ID", this.CurrentUserId);
        }
    }
}
