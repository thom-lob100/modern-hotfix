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

        private DataActionResult CreateChildLot(RequestParameters parameters)
        {
            return this.RequestFields(parameters.Text("SUB_PROD_TYP") == "Chip" ? "CreateChipLot" : "CreateLamellaLot",
                    parameters.Take(Schemas.Lot.LotId, "ParentLotId"),
                    parameters.Take("TRAY_ID"),
                    "UserId", this.CurrentUserId,
                    parameters.Take("DESCRIPTION"),
                    parameters.Take("JUDGE_RESULTS"));
        }

        private DataActionResult ChangeSpecLot(RequestParameters parameters)
        {
            return this.RequestFields("ChangeSpecLot",
                    parameters.Take(Schemas.Lot.LotId),
                    parameters.Take("PROD_ID"),
                    parameters.Take("FLOW_ID"),
                    parameters.Take("OPER_ID"),
                    parameters.Take(Schemas.Lot.OperId, "currentOperId"),
                    parameters.Take("description"),
                    "UserId", this.CurrentUserId);
        }

        private DataActionResult HoldLot(RequestParameters parameters)
        {
            return this.RequestFields("HoldLot",
                    parameters.Take(Schemas.Lot.LotId),
                    parameters.Take(Schemas.Lot.OperId, "currentOperId"),
                    parameters.Take("holdCode"),
                    parameters.Take("engrUserId"),
                    parameters.Take("description"),
                    "UserId", this.CurrentUserId);
        }

        private DataActionResult NotOnHoldLot(RequestParameters parameters)
        {
            return this.RequestFields("NotOnHoldLot",
                    parameters.Take(Schemas.Lot.LotId),
                    parameters.Take(Schemas.Lot.OperId, "currentOperId"),
                    parameters.Take("releaseCode"),
                    parameters.Take("engrUserId"),
                    parameters.Take("description"),
                    "UserId", this.CurrentUserId);
        }

        private DataActionResult CancelChangeSpecLot(RequestParameters parameters)
        {
            return this.RequestFields("CancelChangeSpecLot",
                    parameters.Take(Schemas.Lot.LotId),
                    parameters.Take("DESCRIPTION"),
                    "UserId", this.CurrentUserId);
        }

        private DataActionResult CancelScrapLot(RequestParameters parameters)
        {
            return this.RequestFields("CancelScrapLot",
                    parameters.Take(Schemas.Lot.LotId),
                    parameters.Take("DESCRIPTION"),
                    "UserId", this.CurrentUserId);
        }

        private DataActionResult ScrapLot(RequestParameters parameters)
        {
            return this.RequestFields("ScrapLot",
                    parameters.Take(Schemas.Lot.LotId),
                    parameters.Take("WF_ID"),
                    parameters.Take("scrapCode"),
                    parameters.Take("description"),
                    "UserId", this.CurrentUserId);
        }
    }
}
