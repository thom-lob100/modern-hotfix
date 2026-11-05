using System.Data;
using System.Text;

namespace Modern.Lab.Report
{
    public partial class LabMonitorForm
    {
        // MON_STAGE
        // REQ_SERIAL_NO
        // LOT_ID
        // QTY
        // LOT_HOLD_STAT_CD
        // MON_STAT_CD
        // STAGE_TM
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

        // EQP_ID
        // PORT_NM
        // PORT_TYP
        // TRANSFER_STAT_CD
        private DataTable GetMonitorPorts()
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetMonitorPortList");
            return this.Request(request.ToString()).Table;
        }

        // DURABLE_ID
        // DURABLE_TYP
        // LOC_KIND
        // LOT_CNT
        private DataTable GetMonitorDurables()
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetMonitorDurableList");
            return this.Request(request.ToString()).Table;
        }
    }
}
