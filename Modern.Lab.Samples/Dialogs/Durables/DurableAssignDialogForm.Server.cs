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
                    "SubProdTyp", parameters.Value(Schemas.Lot.SubProdTyp)).Table;
        }

        // WF_ID
        // SLOT_NO
        private DataTable GetLotWafers(DataRow lot)
        {
            return this.RequestFields("GetLotUnitList",
                    "LotId", Schemas.Lot.LotId.Text(lot)).Table;
        }
    }
}
