using System.Data;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Display;

namespace Modern.Lab.Samples
{
    public partial class TransportManagementForm
    {
        // 표에 무엇을 어떻게 보일지 — 반송 목록 · Transport Info 카드 · 좌하 내용물 탭 · 우하 반송 이력. 쿼리 컬럼이 바뀌면 여기만 고친다.
        // 무엇을 채울지(조회)는 .Server.cs 가 갖는다. 목록과 카드는 Schemas.Transport, 내용물은 Schemas.DurableWafer · DurableChip · DurableLamella,
        // 이력은 Schemas.TransportHistory 를 쓰고 응답에 온 컬럼을 그대로 보인다.

        // 어느 컨트롤이 어느 컬럼 함수를 쓰는지 — 폼이 열릴 때 한 번 선언하면 조회 전에도 헤더가 보이고, 그 뒤 폼은 DataSource = 표 만 대입한다.
        // 내용물 탭 셋은 조회마다 컬럼과 표를 함께 싣는다(BindContent).
        protected override void DeclareColumns()
        {
            this.gridTransports.ConfigureColumns(TransportColumns);
            TransportFields(null).Apply(this.fieldInfo);
            this.gridHistory.ConfigureColumns(HistoryColumns);
        }

        // Transport History 목록에만 오는 칸 — 이 목록에는 오지 않아 조회 전 헤더에서 뺀다.
        private static string[] HistoryOnlyColumns()
        {
            TransportEntity transport = Schemas.Transport;

            return new string[] { transport.Location.Name, transport.LastEventCd.Name, transport.LastTimeKey.Name };
        }

        // 버튼 조건(Manual Out)만 읽는 장비 · 포트 상태 칸 — 목록 응답에 같이 오지만 표와 카드에는 보이지 않는다.
        // 지금 위치(EQP_ID · PORT_NM)와 반송 여부(TRANS_YN)는 보인다.
        private static string[] ConditionOnlyColumns()
        {
            DurableEntity durable = Schemas.Durable;

            return new string[]
            {
                durable.McsItgYn.Name, durable.EqpMesStatTyp.Name, durable.CommStatTyp.Name,
                durable.AcssTyp.Name, durable.PortTyp.Name, durable.PortStatNm.Name, durable.VirtPortYn.Name
            };
        }

        // 반송 표에 없는 Durable 칸 — 캡션은 Durable 엔티티의 것을 쓴다.
        private static Field[] LocationFields()
        {
            DurableEntity durable = Schemas.Durable;

            return new Field[] { durable.EqpId, durable.PortNm, durable.TransYn };
        }

        // 배지는 엔티티(DURABLE_TYP)가 갖는다 — 폭만 여기서 정한다.
        private static GridColumns TransportColumns(DataTable transports)
        {
            TransportEntity transport = Schemas.Transport;
            GridColumns columns = transport.Columns(transports).Hide(HistoryOnlyColumns()).Hide(ConditionOnlyColumns());

            columns.Column(transport.DurableTyp.Name).BadgeWidth(transport.DurableTyp.ValueCodes());
            columns.Column(transport.DurableStatCd.Name).BadgeWidth(transport.DurableStatCd.ValueCodes());

            foreach (Field field in LocationFields())
            {
                if (field.CaptionSetting != null)
                {
                    columns.Column(field.Name).Caption(field.CaptionSetting);
                }
            }

            return columns;
        }

        private static FieldDefinitions TransportFields(DataTable transports)
        {
            TransportEntity transport = Schemas.Transport;
            FieldDefinitions fields = transport.Fields(transports).Hide(HistoryOnlyColumns()).Hide(ConditionOnlyColumns());

            fields.Field(transport.DurableTyp.Name, transport.MoveStatCd.Name, transport.DurableStatCd.Name).Badge();

            foreach (Field field in LocationFields())
            {
                if (field.CaptionSetting != null)
                {
                    fields.Field(field.Name).Caption(field.CaptionSetting);
                }
            }

            return fields;
        }

        // 좌하 탭의 내용물 — Durable Management 의 내용물 탭과 같다. 단위 Id 캡션은 탭에 맞춘다("Wafer Id" · "Chip Id" · "Lamella Id").
        private static GridColumns WaferColumns(DataTable wafers)
        {
            return Schemas.DurableWafer.Columns(wafers)
                    .Column(Schemas.DurableWafer.WfId.Name).Caption("Wafer Id");
        }

        // Lot 상태 세 열은 Wafer List 에만 보이고 Chip · Lamella 탭에서는 숨긴다.
        private static GridColumns ChipColumns(DataTable chips)
        {
            GridColumns columns = Schemas.DurableChip.Columns(chips)
                    .Column(Schemas.DurableChip.WfId.Name).Caption("Chip Id");
            columns.Hide(LotStateColumns);
            return columns;
        }

        private static GridColumns LamellaColumns(DataTable lamellas)
        {
            GridColumns columns = Schemas.DurableLamella.Columns(lamellas)
                    .Column(Schemas.DurableLamella.WfId.Name).Caption("Lamella Id");
            columns.Hide(LotStateColumns);
            return columns;
        }

        private static readonly string[] LotStateColumns =
        {
            Schemas.DurableWafer.LotStatTyp.Name,
            Schemas.DurableWafer.LotHoldStatCd.Name,
            Schemas.DurableWafer.MesProcStatCd.Name
        };

        // 우하 반송 이력 — Transport History 화면의 이력 표와 같다. Duration 컬럼은 조회 결과에 프리젠터가 덧붙이는 컬럼이라 첫 조회 뒤에 나타난다.
        private static GridColumns HistoryColumns(DataTable history)
        {
            GridColumns columns = Schemas.TransportHistory.Columns(history)
                    .First(Schemas.TransportHistory.EventTm.Name, LotHistoryPresenter.DurationColumn)
                    .Column(Schemas.TransportHistory.DurableTyp.Name).BadgeWidth(Schemas.TransportHistory.DurableTyp.ValueCodes());

            foreach (ModernDataGridColumn column in columns.ToArray())
            {
                if (column.DataPropertyName == LotHistoryPresenter.DurationColumn)
                {
                    column.TextColor = DurationColor;
                    column.TextSemiBold = true;
                }
            }

            return columns;
        }
    }
}
