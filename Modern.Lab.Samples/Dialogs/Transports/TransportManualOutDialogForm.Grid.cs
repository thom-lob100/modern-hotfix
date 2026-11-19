using System.Data;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Samples
{
    public partial class TransportManualOutDialogForm
    {
        private static GridColumns PortColumns(DataTable ports)
        {
            PortDetEntity port = Schemas.PortDet;

            return port.Columns(ports)
                    .Hide(port.EqpId.Name)
                    .Column(port.PortTyp.Name).BadgeWidth(port.PortTyp.ValueCodes())
                    .Column(port.AcssTyp.Name).BadgeWidth(port.AcssTyp.ValueCodes())
                    .Column(port.TransferStatCd.Name).BadgeWidth(port.TransferStatCd.ValueCodes());
        }
    }
}
