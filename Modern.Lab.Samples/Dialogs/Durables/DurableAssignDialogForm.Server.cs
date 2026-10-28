using System.Data;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class DurableAssignDialogForm
    {
        // LOT_ID
        // LOT_STAT_TYP
        // LOT_HOLD_STAT_CD
        // DURABLE_ID
        private DataTable GetLots(RequestParameters parameters)
        {
            return this.RequestFields("GetLotList",
                    parameters.Take("SUB_PROD_TYP"),
                    parameters.Take("CRT_FROM_DT"),
                    parameters.Take("CRT_TO_DT"),
                    parameters.Take("KEYWORD")).Table;
        }

        // WF_ID
        // SLOT_NO
        private DataTable GetLotUnits(RequestParameters parameters)
        {
            return this.RequestFields("GetLotUnitList",
                    parameters.Take(Schemas.Lot.LotId)).Table;
        }
    }
}
