using System.Data;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class DurableManualOutDialogForm
    {
        // PORT_NM
        private DataTable GetPorts(DataRow durable)
        {
            return this.RequestFields("GetEqpPortList",
                    "EqpId", Schemas.Durable.EqpId.Text(durable)).Table;
        }
    }
}
