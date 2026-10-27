using System.Data;
using System.Text;
using Modern.Lab.Hosting.Contracts;

namespace Modern.Lab.Samples
{
    public partial class DurableAssignDialogForm
    {
        // LOT_ID
        // SUB_PROD_TYP
        // LOT_STAT_TYP
        // LOT_HOLD_STAT_CD
        // DURABLE_ID
        private DataTable GetLots(string subProdTyps)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetLotList");
            request.Append(" SUB_PROD_TYP=").Append(subProdTyps);
            request.Append(" CRT_FROM_DT=").Append(string.Empty);
            request.Append(" CRT_TO_DT=").Append(string.Empty);
            request.Append(" KEYWORD=").Append(string.Empty);
            return this.Request(request.ToString()).Table;
        }

        // WF_ID
        // SLOT_NO
        // FINGER_ID
        private DataTable GetLotUnits(string lotId)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetLotUnitList");
            request.Append(" LOT_ID=").Append(lotId);
            return this.Request(request.ToString()).Table;
        }
    }
}
