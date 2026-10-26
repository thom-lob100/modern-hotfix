using System.Data;

using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Display;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class LotEquipmentForm
    {
        // Job 확인 창과 Skip 창의 Job 요약 카드에 보일 항목과 순서. 캡션·형식·배지는 엔티티에서 오고,
        // 엔티티와 다르게 보일 캡션만 여기서 적는다. 회사에서 항목을 바꾸면 이 목록만 고친다.
        internal static ModernFieldDefinition[] JobSummaryFields()
        {
            return new ModernFieldDefinition[]
            {
                CardFields.Of(Schemas.EquipmentPriority.EqpId),
                CardFields.Of(Schemas.EquipmentPriority.CommStatTyp),
                CardFields.Of(Schemas.LotPriority.PortNm).Caption("Port"),
                CardFields.Of(Schemas.LotPriority.GoalPortNm).Caption("Goal Port"),
                CardFields.Of(Schemas.LotPriority.LotId),
                CardFields.Of(Schemas.LotPriority.DurableId).Caption("Durable Id"),
                CardFields.Of(Schemas.LotPriority.GoalDurableId).Caption("Goal Durable Id"),
                CardFields.Of(Schemas.DurablePriority.DurableTyp).Caption("Goal Durable Type"),
                CardFields.Of(Schemas.LotPriority.LastEventCd)
            };
        }

        // 표에 무엇을 어떻게 보일지 — 컬럼 배지·색·링크. 컬럼 목록·캡션·상태 배지는 엔티티(Schemas.XxxPriority)가 정하고,
        // 여기에는 이 화면만의 꾸밈(우선순위 색·배지 폭·Spin·Link·YesNo)을 이어 적는다.
        // 무엇을 채울지(조회·판정)는 .Server.cs 와 .Rules.cs 가 갖는다.

        private static GridColumns EquipmentPriorityColumns(DataTable table)
        {
            EquipmentPriorityEntity equipment = Schemas.EquipmentPriority;

            return AutoVocabulary(equipment.Columns(table)
                    .Column(equipment.Priority.Name).BadgeColor(EquipmentLotPresenter.PriorityColorColumn).NoFilter()
                    .Column(equipment.CommStatTyp.Name).BadgeWidth(equipment.CommStatTyp.ValueCodes())
                    .Column(equipment.MesEqpStatCd.Name).BadgeWidth(equipment.MesEqpStatCd.ValueCodes())
                    .Column(equipment.AutoYn.Name).YesNo(EquipmentLotPresenter.AutoCanColumn));
        }

        private static GridColumns PortPriorityColumns(DataTable table)
        {
            PortPriorityEntity port = Schemas.PortPriority;

            return port.Columns(table).Hide(port.EqpId.Name)
                    .Column(port.Priority.Name).BadgeColor(EquipmentLotPresenter.PriorityColorColumn).NoFilter()
                    .Column(port.PortTyp.Name).BadgeWidth(port.PortTyp.ValueCodes())
                    .Column(port.TransferStatCd.Name).BadgeWidth(port.TransferStatCd.ValueCodes());
        }

        private static GridColumns LotPriorityColumns(DataTable table)
        {
            LotPriorityEntity lot = Schemas.LotPriority;

            return lot.Columns(table)
                    .Column(lot.Priority.Name).BadgeColor(EquipmentLotPresenter.PriorityColorColumn).NoFilter()
                    .Column(lot.Priority.Name).BadgeSpin(EquipmentLotPresenter.PrioritySpinColumn)
                    .Column(lot.MesProcStatCd.Name).BadgeWidth(lot.MesProcStatCd.ValueCodes())
                    .Column(lot.ReqSerialNo.Name).Link()
                    .Column(lot.DurableId.Name, lot.GoalDurableId.Name).Link();
        }

        // Sample 목록 — 공유 엔티티(Schemas.Sample)를 그대로 쓰고, 위 요청 정보 카드가 이미 보이는 REQ_SERIAL_NO 만 이 화면에서 숨긴다.
        // 숨김은 표시에만 적용한다 — 데이터의 복합 키(REQ_SERIAL_NO + REQ_SUBSERIAL_NO)는 그대로 남는다.
        private static GridColumns SampleColumns(DataTable table)
        {
            return Schemas.Sample.Columns(table)
                    .Hide(Schemas.Sample.ReqSerialNo.Name);
        }

        // 요청 정보 카드의 회사 표시 설정 — 공유 엔티티(Schemas.Request)에 화면 계산 컬럼 숨김만 더한다.
        private static GridColumns RequestOverviewColumns(DataTable overview)
        {
            return HideScreenColumns(Schemas.Request.Columns(overview));
        }

        // 화면이 계산해 붙인 컬럼(서버 순서 · Auto 가능 여부)은 어느 표에서도 열로 보이지 않는다.
        private static GridColumns HideScreenColumns(GridColumns columns)
        {
            return columns.Hide(
                    EquipmentLotPresenter.ServerOrderColumn, EquipmentLotPresenter.AutoCanColumn, EquipmentLotPresenter.JobColorColumn,
                    EquipmentLotPresenter.PriorityColorColumn, EquipmentLotPresenter.PrioritySpinColumn);
        }
    }
}
