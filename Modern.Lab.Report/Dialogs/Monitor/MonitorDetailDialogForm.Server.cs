using System.Data;
using System.Text;

namespace Modern.Lab.Report
{
    public partial class MonitorDetailDialogForm
    {
        // EVENT_TM
        private DataTable GetLotHistory(string lotId)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetLotHistory");
            request.Append(" LOT_ID=").Append(lotId);
            return this.Request(request.ToString()).Table;
        }

        // WF_ID
        // SLOT_NO
        private DataTable GetWafers(string lotId)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetLotUnitList");
            request.Append(" LOT_ID=").Append(lotId);
            return this.Request(request.ToString()).Table;
        }
    }
}
