using System.Data;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.WinForms.Controls.Display;

namespace Modern.Lab.Samples
{
    public partial class DurableTransportDialogForm
    {
        // 위쪽 Durable 카드에 보일 항목과 순서. 캡션·형식·배지는 엔티티(Schemas.Durable)에서 온다.
        private static ModernFieldDefinition[] SourceFields()
        {
            return DurableDialogHelpers.SourceFields();
        }

        // 고른 목적지(장비 또는 Area)의 카드에 보일 항목과 순서. 응답에 없는 컬럼은 건너뛴다.
        private static FieldDefinitions DestinationDefinitions(DataTable destinations)
        {
            return Schemas.EquipmentExt.Fields(destinations)
                    .Only(
                            Schemas.EquipmentExt.EqpId.Name, Schemas.EquipmentExt.EqpGrpId.Name,
                            Schemas.EquipmentExt.CommStatTyp.Name, Schemas.EquipmentExt.MesEqpStatCd.Name,
                            Schemas.EquipmentExt.AutoYn.Name, Schemas.EquipmentExt.Location.Name,
                            Schemas.Area.AreaId.Name);
        }
    }
}
