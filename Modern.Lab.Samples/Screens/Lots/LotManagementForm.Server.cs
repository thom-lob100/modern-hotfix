using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Controls.Wpf.Input;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.ResponseContracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.Samples.Contracts;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Display;
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
                    parameters.Take("SUB_PROD_TYP"),
                    parameters.Take("CRT_FROM_DT"),
                    parameters.Take("CRT_TO_DT"),
                    parameters.Take("KEYWORD")).Table;
        }

        // LOT_ID
        private DataTable GetLotIdCandidates(RequestParameters parameters)
        {
            return this.RequestFields("GetLotIdCandidates",
                    parameters.Take("KEYWORD")).Table;
        }

        // WF_ID
        // SLOT_NO
        private DataTable GetWafers(RequestParameters parameters)
        {
            return this.RequestFields("GetLotUnitList",
                    parameters.Take(Schemas.Lot.LotId)).Table;
        }

        private DataTable GetLotHistory(RequestParameters parameters)
        {
            return this.RequestFields("GetLotHistory",
                    parameters.Take(Schemas.Lot.LotId)).Table;
        }

        // DURABLE_ID
        // STUB_LOT_QTY
        // LCC_LOT_QTY
        private DataTable GetTrays()
        {
            return this.RequestFields("GetDurableList",
                    "DurableTyp", "TRAY").Table;
        }

        private DataTable GetWaferHistory(RequestParameters parameters)
        {
            return this.RequestFields("GetLotUnitHistory",
                    parameters.Take(Schemas.Lot.LotId),
                    parameters.Take(Schemas.Wafer.WfId)).Table;
        }

        private DataActionResult CreateChildLot(string subProdTyp, DataRow lot, string trayId, DataTable judgeResults, string description)
        {
            return this.RequestFields(subProdTyp == "Chip" ? "CreateChipLot" : "CreateLamellaLot",
                    "ParentLotId", TableHelper.CellText(lot, "LOT_ID"),
                    "TrayId", trayId,
                    "UserId", this.CurrentUserId,
                    "Description", description,
                    "JudgeResults", JudgeResultJson.ToJson(judgeResults));
        }

        private DataActionResult ChangeSpecLot(
                DataRow lot, string prodId, string flowId, string operId, string description)
        {
            return this.RequestFields("ChangeSpecLot", "LOT_ID", TableHelper.CellText(lot, "LOT_ID").Trim(),
                    "PROD_ID", prodId, "FLOW_ID", flowId, "OPER_ID", operId,
                    "currentOperId", TableHelper.CellText(lot, "OPER_ID").Trim(), "description", description, "USER_ID", this.CurrentUserId);
        }

        private DataActionResult HoldLot(DataRow lot, string holdCode, string engrUserId, string description)
        {
            return this.RequestFields("HoldLot", "LOT_ID", TableHelper.CellText(lot, "LOT_ID").Trim(),
                    "currentOperId", TableHelper.CellText(lot, "OPER_ID").Trim(), "holdCode", holdCode, "engrUserId", engrUserId,
                    "description", description, "USER_ID", this.CurrentUserId);
        }

        private DataActionResult NotOnHoldLot(DataRow lot, string releaseCode, string engrUserId, string description)
        {
            return this.RequestFields("NotOnHoldLot", "LOT_ID", TableHelper.CellText(lot, "LOT_ID").Trim(),
                    "currentOperId", TableHelper.CellText(lot, "OPER_ID").Trim(), "releaseCode", releaseCode, "engrUserId", engrUserId,
                    "description", description, "USER_ID", this.CurrentUserId);
        }

        private DataActionResult CancelChangeSpecLot(DataRow lot, string description)
        {
            return this.RequestFields("CancelChangeSpecLot", "LOT_ID", TableHelper.CellText(lot, "LOT_ID").Trim(),
                    "DESCRIPTION", description, "USER_ID", this.CurrentUserId);
        }

        private DataActionResult CancelScrapLot(DataRow lot, string description)
        {
            return this.RequestFields("CancelScrapLot", "LOT_ID", TableHelper.CellText(lot, "LOT_ID").Trim(),
                    "DESCRIPTION", description, "USER_ID", this.CurrentUserId);
        }

        private DataActionResult ScrapLot(DataRow lot, string scrapCode, string[] waferIds, string description)
        {
            if (waferIds == null || waferIds.Length == 0)
            {
                return this.RequestFields("ScrapLot", "LOT_ID", TableHelper.CellText(lot, "LOT_ID").Trim(),
                        "scrapCode", scrapCode, "description", description, "USER_ID", this.CurrentUserId);
            }

            return this.RequestFields("ScrapLot", "LOT_ID", TableHelper.CellText(lot, "LOT_ID").Trim(),
                    "WF_ID", string.Join(",", waferIds),
                    "scrapCode", scrapCode, "description", description, "USER_ID", this.CurrentUserId);
        }
    }
}
