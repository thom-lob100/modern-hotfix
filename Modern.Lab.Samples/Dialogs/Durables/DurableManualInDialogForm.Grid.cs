using System.Data;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Display;

namespace Modern.Lab.Samples
{
    public partial class DurableManualInDialogForm
    {
        private static ModernFieldDefinition[] LotFields()
        {
            return new ModernFieldDefinition[]
            {
                CardFields.Of(Schemas.Lot.LotId),
                CardFields.Of(Schemas.Lot.SubProdTyp),
                CardFields.Of(Schemas.Lot.ProdId),
                CardFields.Of(Schemas.Lot.FlowId),
                CardFields.Of(Schemas.Lot.OperId),
                CardFields.Of(Schemas.Lot.ReqSerialNo)
            };
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
