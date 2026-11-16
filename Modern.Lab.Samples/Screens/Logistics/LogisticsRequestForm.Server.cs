using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Data;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.Hosting.Messaging;

namespace Modern.Lab.Samples
{
    public partial class LogisticsRequestForm
    {
        // LOT_ID
        // SEND_YN
        // SEND_TM
        // RECV_YN
        // RECV_TM
        // PROC_YN
        // REQ_SERIAL_NO
        // LOT_REQ_SERIAL_NO
        // SUB_PROD_TYP
        private DataTable GetLogistics(string keyword, string reqSerialNo, IList<string> statusFilter, string reqFromDt, string reqToDt)
        {
            string status = statusFilter == null ? string.Empty : string.Join(",", statusFilter);

            return this.RequestFields("/api/pending/board",
                    "keyword", keyword ?? string.Empty,
                    "REQ_SERIAL_NO", reqSerialNo ?? string.Empty,
                    "STATUS", status,
                    "REQ_FROM_DT", reqFromDt,
                    "REQ_TO_DT", reqToDt).Table;
        }

        private DataTable GetRequestWafers(DataRow lot)
        {
            string lotId = TableHelper.CellText(lot, "LOT_ID").Trim();
            string reqSerialNo = TableHelper.CellText(lot, "REQ_SERIAL_NO").Trim();
            return this.RequestFields("/api/items/units", "itemId", lotId, "reqSerialNo", reqSerialNo).Table;
        }

        private DataActionResult Receive(DataRow lot, string description)
        {
            return this.RequestFields("/api/pending/receive", "itemId", TableHelper.CellText(lot, "LOT_ID").Trim(), "description", description ?? string.Empty);
        }

        private DataActionResult Create(DataRow lot, string description)
        {
            return this.RequestFields("/api/pending/create", "itemId", TableHelper.CellText(lot, "LOT_ID").Trim(), "description", description ?? string.Empty);
        }

        private DataActionResult AddRequest(DataRow lot, string description)
        {
            return this.RequestFields("/api/pending/add-request","itemId", Schemas.Logistics.LotId.Text(lot), "description", description ?? string.Empty);
        }

        // 임시 — Lot 하나를 초기화한다. 전문 이름과 키는 회사에서 채운다.
        private DataActionResult InitializeLot(DataRow lot)
        {
            return this.RequestFields("/api/pending/initialize", "itemId", Schemas.Logistics.LotId.Text(lot));
        }
    }
}
