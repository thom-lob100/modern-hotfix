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
        private DataTable GetLogistics(string keyword, string reqSerialNo, IList<string> statusFilter)
        {
            string status = statusFilter == null ? string.Empty : string.Join(",", statusFilter);

            return this.RequestFields("/api/pending/board",
                    "keyword", keyword ?? string.Empty,
                    "REQ_SERIAL_NO", reqSerialNo ?? string.Empty,
                    "STATUS", status).Table;
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
    }
}
