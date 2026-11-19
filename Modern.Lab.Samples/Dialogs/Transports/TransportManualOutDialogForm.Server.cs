using System.Data;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class TransportManualOutDialogForm
    {
        // PORT_NM
        private DataTable GetPorts(DataRow transport)
        {
            return this.RequestFields("GetEqpPortList",
                    "EqpId", Schemas.Durable.EqpId.Text(transport)).Table;
        }
    }
}
