using System.Data;

using Modern.Lab.Samples.Contracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Display;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class DurableManagementForm
    {
        // 표에 무엇을 어떻게 보일지 — 컬럼 순서·캡션·배지·숨김. 쿼리 컬럼이 바뀌면 여기만 고친다.
        // 무엇을 채울지(조회·판정)는 .Server.cs 와 .Rules.cs 가 갖는다.
        // 각 목록은 엔티티(Schemas.Durable·DurableHistory·DurableWafer·DurableChip·DurableLamella·
        // DurableChipHistory·DurableLamellaHistory)가 키·배지를 갖는다 — 이 화면만의 꾸밈은 그 뒤에 이어 적는다.

        // Durable 목록의 DURABLE_ID 링크 — 누르면 Durable 정보 팝업(요약 + 슬롯 맵)을 연다.
        private static GridColumns DurableColumns(DataTable durables)
        {
            return Schemas.Durable.Columns(durables)
                    .Column(Schemas.Durable.DurableId.Name).Link();
        }

        private static FieldDefinitions DurableFields(DataTable durable)
        {
            return Schemas.Durable.Fields(durable)
                    .Hide(
                            Schemas.Durable.FoupLotQty.Name, Schemas.Durable.FoupCapa.Name,
                            Schemas.Durable.StubLotQty.Name, Schemas.Durable.StubCapa.Name,
                            Schemas.Durable.LccLotQty.Name, Schemas.Durable.LccCapa.Name,
                            Schemas.Durable.FromFac.Name)
                    .Field(Schemas.Durable.Description.Name).Span(3).StartAt(3);
        }

        private static GridColumns DurableHistoryColumns(DataTable history)
        {
            return Schemas.DurableHistory.Columns(history);
        }

        // 좌하 탭의 내용물 — 단위 Id 캡션은 탭에 맞춘 화면 몫("Wafer Id" · "Chip Id" · "Lamella Id").
        private static GridColumns DurableWaferColumns(DataTable wafers)
        {
            return Schemas.DurableWafer.Columns(wafers)
                    .Column(Schemas.DurableWafer.WfId.Name).Caption("Wafer Id");
        }

        // Lot 상태 세 열(Deassign 판정용)은 Wafer List 에만 보이고 Chip · Lamella 탭에서는 숨긴다(2026-10-27).
        private static GridColumns DurableChipColumns(DataTable chips)
        {
            GridColumns columns = Schemas.DurableChip.Columns(chips)
                    .Column(Schemas.DurableChip.WfId.Name).Caption("Chip Id");
            columns.Hide(LotStateColumns);
            return columns;
        }

        private static GridColumns DurableLamellaColumns(DataTable lamellas)
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

        // 우하 탭의 단위 이력 — REQ_SERIAL_NO 링크를 누르면 Request Info 가 열린다.
        private static GridColumns DurableChipHistoryColumns(DataTable history)
        {
            return Schemas.DurableChipHistory.Columns(history)
                    .Column(Schemas.DurableChipHistory.WfId.Name).Caption("Chip Id")
                    .Column(Schemas.DurableChipHistory.ReqSerialNo.Name).Link();
        }

        private static GridColumns DurableLamellaHistoryColumns(DataTable history)
        {
            return Schemas.DurableLamellaHistory.Columns(history)
                    .Column(Schemas.DurableLamellaHistory.WfId.Name).Caption("Lamella Id")
                    .Column(Schemas.DurableLamellaHistory.ReqSerialNo.Name).Link();
        }
    }
}
