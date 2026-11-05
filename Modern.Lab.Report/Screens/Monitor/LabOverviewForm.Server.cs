using System.Data;
using System.Text;

namespace Modern.Lab.Report
{
    public partial class LabOverviewForm
    {
        // MON_STAGE
        // MON_STAT_CD
        // LOT_HOLD_STAT_CD
        // REWORK_YN
        // SCRAP_DUE_YN
        // DURABLE_ID
        private DataTable GetMonitorLots()
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetMonitorLotList");
            return this.Request(request.ToString()).Table;
        }

        // MON_STAGE
        // EQP_ID
        // PARENT_EQP_ID
        // MES_EQP_STAT_CD
        private DataTable GetMonitorEquipments()
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetMonitorEqpList");
            return this.Request(request.ToString()).Table;
        }

        // DURABLE_TYP
        // LOC_KIND
        // LOT_CNT
        // RETURN_YN
        // SWAP_YN
        private DataTable GetMonitorDurables()
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetMonitorDurableList");
            return this.Request(request.ToString()).Table;
        }
    }
}
