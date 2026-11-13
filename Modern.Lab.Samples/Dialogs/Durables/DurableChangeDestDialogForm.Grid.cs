using System.Data;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Samples
{
    public partial class DurableChangeDestDialogForm
    {
        private static GridColumns TransportColumns(DataTable transports)
        {
            TransportEntity transport = Schemas.Transport;

            return transport.Columns(transports)
                    .Column(transport.DurableTyp.Name).BadgeWidth(transport.DurableTyp.ValueCodes())
                    .Column(transport.DurableStatCd.Name).BadgeWidth(transport.DurableStatCd.ValueCodes());
        }
    }
}
