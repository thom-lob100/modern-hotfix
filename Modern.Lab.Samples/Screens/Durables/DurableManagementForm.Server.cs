using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Controls.Wpf.Display;
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
    public partial class DurableManagementForm
    {
        // TYP_VAL
        private DataTable GetDurableTypes()
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetDurableTypeList");
            return this.Request(request.ToString()).Table;
        }

        // TYP_VAL
        private DataTable GetLoadStatusCodes()
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetCommonCodeList");
            request.Append(" GRP_CD=").Append("WF_LOAD_STAT_CD");
            return this.Request(request.ToString()).Table;
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
        private DataTable GetDurables(string durableTyp, string wfLoadStatCd, bool fromFab, string durableIdLike)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetDurableList");
            request.Append(" DURABLE_TYP=").Append(durableTyp);
            request.Append(" WF_LOAD_STAT_CD=").Append(wfLoadStatCd);
            request.Append(" fromFab=").Append(fromFab ? "Y" : "N");
            request.Append(" DURABLE_ID=").Append(durableIdLike);
            return this.Request(request.ToString()).Table;
        }

        // SUB_PROD_TYP
        // SLOT_NO
        // FINGER_ID
        private DataTable GetDurableSlots(string durableId)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetDurableSlotList");
            request.Append(" DURABLE_ID=").Append(durableId);
            request.Append(" SUB_PROD_TYP=").Append(string.Empty);

            return this.Request(request.ToString()).Table;
        }

        // SUB_PROD_TYP
        // SLOT_NO
        private DataTable GetDurableWafers(string durableId)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetDurableSlotList");
            request.Append(" DURABLE_ID=").Append(durableId);
            request.Append(" SUB_PROD_TYP=")
                    .Append("Wafer");

            return this.Request(request.ToString()).Table;
        }

        // SUB_PROD_TYP
        // SLOT_NO
        private DataTable GetDurableChips(string durableId)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetDurableSlotList");
            request.Append(" DURABLE_ID=").Append(durableId);
            request.Append(" SUB_PROD_TYP=")
                    .Append("Chip");

            return this.Request(request.ToString()).Table;
        }

        // SUB_PROD_TYP
        // SLOT_NO
        // FINGER_ID
        private DataTable GetDurableLamellas(string durableId)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetDurableSlotList");
            request.Append(" DURABLE_ID=").Append(durableId);
            request.Append(" SUB_PROD_TYP=")
                    .Append("Lamella");

            return this.Request(request.ToString()).Table;
        }

        private DataTable GetDurableHistory(string durableId)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetDurableHistory");
            request.Append(" DURABLE_ID=").Append(durableId);
            return this.Request(request.ToString()).Table;
        }

        private DataTable GetDurableChipHistory(string durableId)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetUnitHistory");
            request.Append(" DURABLE_ID=").Append(durableId);
            request.Append(" SUB_PROD_TYP=")
                    .Append("Chip");
            return this.Request(request.ToString()).Table;
        }

        private DataTable GetDurableLamellaHistory(string durableId)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetUnitHistory");
            request.Append(" DURABLE_ID=").Append(durableId);
            request.Append(" SUB_PROD_TYP=")
                    .Append("Lamella");
            return this.Request(request.ToString()).Table;
        }

        // SUB_CATG_CD
        // DURABLE_TYP
        private DataTable GetSubCategories()
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetDurableCategoryList");
            return this.Request(request.ToString()).Table;
        }

        private DataActionResult CreateDurable(string durableTyp, string subCatgCd, string description)
        {
            StringBuilder request = new StringBuilder();
            request.Append("CreateDurable");
            request.Append(" DURABLE_TYP=").Append(durableTyp);
            request.Append(" SUB_CATG_CD=").Append(subCatgCd);
            request.Append(" DESCRIPTION=").Append(description);
            request.Append(" USER_ID=").Append(this.CurrentUserId);
            return this.Request(request.ToString());
        }

        private DataActionResult SetRfId(DataRow durable, string rfid, string description)
        {
            StringBuilder request = new StringBuilder();
            request.Append("SetDurableRfId");
            request.Append(" DURABLE_ID=").Append(TableHelper.CellText(durable, "DURABLE_ID").Trim());
            request.Append(" RFID=").Append(rfid);
            request.Append(" DESCRIPTION=").Append(description);
            request.Append(" USER_ID=").Append(this.CurrentUserId);
            return this.Request(request.ToString());
        }

        private DataActionResult HoldDurable(DataRow durable, string holdCode, string engrUserId, string description)
        {
            StringBuilder request = new StringBuilder();
            request.Append("HoldDurable");
            request.Append(" DURABLE_ID=").Append(TableHelper.CellText(durable, "DURABLE_ID").Trim());
            request.Append(" holdCode=").Append(holdCode);
            request.Append(" engrUserId=").Append(engrUserId);
            request.Append(" description=").Append(description);
            request.Append(" USER_ID=").Append(this.CurrentUserId);
            return this.Request(request.ToString());
        }

        private DataActionResult ReleaseDurable(DataRow durable, string releaseCode, string engrUserId, string description)
        {
            StringBuilder request = new StringBuilder();
            request.Append("ReleaseDurable");
            request.Append(" DURABLE_ID=").Append(TableHelper.CellText(durable, "DURABLE_ID").Trim());
            request.Append(" releaseCode=").Append(releaseCode);
            request.Append(" engrUserId=").Append(engrUserId);
            request.Append(" description=").Append(description);
            request.Append(" USER_ID=").Append(this.CurrentUserId);
            return this.Request(request.ToString());
        }

        private DataActionResult AdjustDurable(DataRow durable, string description)
        {
            StringBuilder request = new StringBuilder();
            request.Append("AdjustDurable");
            request.Append(" DURABLE_ID=").Append(TableHelper.CellText(durable, "DURABLE_ID").Trim());
            request.Append(" DESCRIPTION=").Append(description);
            request.Append(" USER_ID=").Append(this.CurrentUserId);
            return this.Request(request.ToString());
        }

        private DataActionResult ScrapDurable(DataRow durable, string description)
        {
            StringBuilder request = new StringBuilder();
            request.Append("ScrapDurable");
            request.Append(" DURABLE_ID=").Append(TableHelper.CellText(durable, "DURABLE_ID").Trim());
            request.Append(" DESCRIPTION=").Append(description);
            request.Append(" USER_ID=").Append(this.CurrentUserId);
            return this.Request(request.ToString());
        }

        private DataActionResult CancelScrapDurable(DataRow durable, string description)
        {
            StringBuilder request = new StringBuilder();
            request.Append("CancelScrapDurable");
            request.Append(" DURABLE_ID=").Append(TableHelper.CellText(durable, "DURABLE_ID").Trim());
            request.Append(" DESCRIPTION=").Append(description);
            request.Append(" USER_ID=").Append(this.CurrentUserId);
            return this.Request(request.ToString());
        }

        private DataActionResult TransportDurable(DataRow durable, string locNm, string location, string description)
        {
            StringBuilder request = new StringBuilder();
            request.Append("TransportDurable");
            request.Append(" DURABLE_ID=").Append(TableHelper.CellText(durable, "DURABLE_ID").Trim());
            request.Append(" LOC_NM=").Append(locNm);
            request.Append(" LOCATION=").Append(location);
            request.Append(" DESCRIPTION=").Append(description);
            request.Append(" USER_ID=").Append(this.CurrentUserId);
            return this.Request(request.ToString());
        }
    }
}
