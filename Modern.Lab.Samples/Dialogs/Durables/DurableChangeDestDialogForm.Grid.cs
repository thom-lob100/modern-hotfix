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
                    .Column(transport.DurableTyp.Name).BadgeWidth(transport.DurableTyp.ValueCodes());
        }

        private static GridColumns EquipmentColumns(DataTable equipments)
        {
            EquipmentExtEntity equipment = Schemas.EquipmentExt;

            return equipment.Columns(equipments)
                    .Only(equipment.EqpId.Name, equipment.EqpGrpId.Name,
                            equipment.CommStatTyp.Name, equipment.MesStatTyp.Name, equipment.EqpTyp.Name)
                    .Column(equipment.CommStatTyp.Name).BadgeWidth(equipment.CommStatTyp.ValueCodes())
                    .Column(equipment.MesStatTyp.Name).BadgeWidth(equipment.MesStatTyp.ValueCodes());
        }

        private static GridColumns StockerColumns(DataTable stockers)
        {
            return Schemas.EquipmentExt.Columns(stockers);
        }

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
