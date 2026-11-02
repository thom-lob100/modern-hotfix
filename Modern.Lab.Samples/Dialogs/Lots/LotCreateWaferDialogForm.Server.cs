using System.Data;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting.Messaging;
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
                    "KEYWORD", parameters.Value(Schemas.Criteria.Keyword),
                    "SEND_FAC", parameters.Value(Schemas.Lot.SendFac)).Table;
        }

        private DataTable GetIncomingLotInfo(RequestParameters parameters)
        {
            return this.RequestFields("GetIncomingLotInfo",
                    "LOT_ID", parameters.Value(Schemas.Lot.LotId),
                    "SEND_FAC", parameters.Value(Schemas.Lot.SendFac)).Table;
        }

        // WF_ID
        private DataTable GetIncomingWafers(RequestParameters parameters)
        {
            return this.RequestFields("GetIncomingWaferList",
                    "LOT_ID", parameters.Value(Schemas.Lot.LotId),
                    "SEND_FAC", parameters.Value(Schemas.Lot.SendFac)).Table;
        }

        private DataActionResult CreateWaferLot(RequestParameters parameters)
        {
            return this.RequestFields("CreateWaferLot",
                    "LOT_ID", parameters.Value(Schemas.Lot.LotId),
                    "SEND_FAC", parameters.Value(Schemas.Lot.SendFac),
                    "DESCRIPTION", parameters.Value(Schemas.Criteria.Description),
                    "USER_ID", this.CurrentUserId);
        }
    }
}
