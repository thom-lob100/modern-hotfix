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
            StringBuilder request = new StringBuilder();
            request.Append("GetLotTypeList");
            return this.Request(request.ToString()).Table;
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
        private DataTable GetLots(string subProdTyp, string crtFromDt, string crtToDt, string keyword)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetLotList");
            request.Append(" SUB_PROD_TYP=").Append(subProdTyp);
            request.Append(" CRT_FROM_DT=").Append(crtFromDt);
            request.Append(" CRT_TO_DT=").Append(crtToDt);
            request.Append(" KEYWORD=").Append(keyword);
            return this.Request(request.ToString()).Table;
        }

        // LOT_ID
        private DataTable GetLotIdCandidates(string keyword)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetLotIdCandidates");
            request.Append(" KEYWORD=").Append(keyword);
            return this.Request(request.ToString()).Table;
        }

        // WF_ID
        // SLOT_NO
        private DataTable GetWafers(string lotId)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetLotUnitList");
            request.Append(" LOT_ID=").Append(lotId);
            return this.Request(request.ToString()).Table;
        }

        private DataTable GetLotHistory(string lotId)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetLotHistory");
            request.Append(" LOT_ID=").Append(lotId);
            return this.Request(request.ToString()).Table;
        }

        // DURABLE_ID
        // STUB_LOT_QTY
        // LCC_LOT_QTY
        private DataTable GetTrays()
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetDurableList");
            request.Append(" DURABLE_TYP=").Append("TRAY");
            return this.Request(request.ToString()).Table;
        }

        private DataTable GetWaferHistory(string lotId, string wfId)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetLotUnitHistory");
            request.Append(" LOT_ID=").Append(lotId);
            request.Append(" WF_ID=").Append(wfId);
            return this.Request(request.ToString()).Table;
        }

        private DataActionResult CreateChildLot(string subProdTyp, DataRow lot, string trayId, DataTable judgeResults, string description)
        {
            StringBuilder request = new StringBuilder();

            if (subProdTyp == "Chip")
            {
                request.Append("CreateChipLot");
            }
            else
            {
                request.Append("CreateLamellaLot");
            }

            request.Append(" PARENT_LOT_ID=").Append(TableHelper.CellText(lot, "LOT_ID").Trim());
            request.Append(" TRAY_ID=").Append(trayId);
            request.Append(" USER_ID=").Append(this.CurrentUserId);
            request.Append(" DESCRIPTION=").Append(Modern.Lab.Hosting.Messaging.ServerMessageFormat.FormatValue(description));
            request.Append(" JUDGE_RESULTS=").Append(JudgeResultJson.ToJson(judgeResults));
            return this.Request(request.ToString());
        }

        private DataActionResult ChangeLotSpec(
                DataRow lot, string prodId, string flowId, string operId, string description)
        {
            return this.RequestFields("ChangeLotSpec", "LOT_ID", TableHelper.CellText(lot, "LOT_ID").Trim(),
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
