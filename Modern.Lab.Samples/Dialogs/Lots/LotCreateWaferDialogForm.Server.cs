using System;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Data;
using Modern.Lab.Hosting;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Display;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class LotCreateWaferDialogForm
    {
        // FAC_ID
        private DataTable GetSendFacilities()
        {
            return this.RequestFields("GetSendFacList").Table;
        }

        // LOT_ID
        private DataTable GetIncomingLotIdCandidates(RequestParameters parameters)
        {
            return this.RequestFields("GetIncomingLotIdCandidates",
                    parameters.Take("KEYWORD"),
                    parameters.Take("SEND_FAC")).Table;
        }

        private DataTable GetIncomingLotInfo(RequestParameters parameters)
        {
            return this.RequestFields("GetIncomingLotInfo",
                    parameters.Take("LOT_ID"),
                    parameters.Take("SEND_FAC")).Table;
        }

        // WF_ID
        private DataTable GetIncomingWafers(RequestParameters parameters)
        {
            return this.RequestFields("GetIncomingWaferList",
                    parameters.Take("LOT_ID"),
                    parameters.Take("SEND_FAC")).Table;
        }

        private DataActionResult CreateWaferLot(string lotId, string sendFac, string remark)
        {
            return this.RequestFields("CreateWaferLot",
                    "LotId", lotId,
                    "SendFac", sendFac,
                    "Description", remark,
                    "UserId", this.CurrentUserId);
        }
    }
}
