using System.Data;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class DurableAssignDialogForm
    {
        // LOT_ID
        private DataTable GetLots(RequestParameters parameters)
        {
            return this.RequestFields("GetLotList",
                    parameters.Take("SUB_PROD_TYP")).Table;
        }

        // WF_ID
        // SLOT_NO
        private DataTable GetLotWafers(RequestParameters parameters)
        {
            return this.RequestFields("GetLotUnitList",
                    parameters.Take(Schemas.Lot.LotId)).Table;
        }
    }
}
