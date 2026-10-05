using System.Data;
using Modern.Lab.WinForms.Controls.Display;

using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.NewItems;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Samples
{
    public partial class EquipmentPortManagementForm
    {
        // 표에 무엇을 어떻게 보일지 — 컬럼 순서·폭·캡션·배지·숨김. 쿼리 컬럼이 바뀌면 여기만 고친다.
        // 무엇을 채울지(조회·판정)는 .Server.cs 와 .Rules.cs 가 갖는다.

        // 장비·포트·이력의 키·배지는 엔티티(Schemas.EquipmentExt · PortExt · EquipmentExtHistory · PortExtHistory)가 갖는다.
        // 이 화면만의 꾸밈(표시 목록·폭·가운데·배지 폭·회전)은 그 뒤에 이어 적는다.
        // 포트 표·포트 이력의 EQP_ID 는 행 키로만 쓰고 표에는 보이지 않는다 — 장비는 왼쪽 트리에서 이미 골랐다.

        private static GridColumns EquipmentExtColumns(DataTable equipments)
        {
            EquipmentExtEntity equipment = Schemas.EquipmentExt;
            GridColumns columns = equipment.Columns(equipments)
                    .Only(equipment.EqpGrpId.Name,
                            equipment.EqpNm.Name, equipment.CommStatTyp.Name,
                            equipment.MesEqpStatCd.Name, equipment.AutoYn.Name);

            return columns
                    .Column(equipment.EqpGrpId.Name).Width(84d)
                    .Column(equipment.EqpNm.Name).Width(150d)
                    .Column(equipment.AutoYn.Name).Width(56d)
                    .Column(equipment.CommStatTyp.Name).BadgeWidth(equipment.CommStatTyp.ValueCodes())
                    .Column(equipment.MesEqpStatCd.Name).BadgeWidth(equipment.MesEqpStatCd.ValueCodes()).Spin(equipment.MesEqpStatCd.Run.Value);
        }

        private static GridColumns PortExtColumns(DataTable ports)
        {
            PortExtEntity port = Schemas.PortExt;
            return port.Columns(ports)
                    .Hide(port.EqpId.Name)
                    .Column(port.PortTyp.Name).BadgeWidth(port.PortTyp.ValueCodes())
                    .Column(port.TransferStatCd.Name).BadgeWidth(port.TransferStatCd.ValueCodes()).Spin(port.TransferStatCd.Processing.Value)
                    .Column(port.DurableId.Name).Link();
        }

        private static GridColumns EquipmentExtHistoryColumns(DataTable history)
        {
            return Schemas.EquipmentExtHistory.Columns(history);
        }

        private static GridColumns PortExtHistoryColumns(DataTable history)
        {
            // 포트 표·포트 이력의 Durable Id 링크 — 누르면 Durable 정보 팝업(슬롯 맵)을 연다.
            return Schemas.PortExtHistory.Columns(history)
                    .Hide(Schemas.PortExtHistory.EqpId.Name)
                    .Column(Schemas.PortExtHistory.DurableId.Name).Link();
        }
        private static FieldDefinitions EquipmentFields(DataTable equipment)
        {
            return Schemas.EquipmentExt.Fields(equipment).Field("DESCRIPTION").Span(3).StartAt(3);
        }
    }
}
