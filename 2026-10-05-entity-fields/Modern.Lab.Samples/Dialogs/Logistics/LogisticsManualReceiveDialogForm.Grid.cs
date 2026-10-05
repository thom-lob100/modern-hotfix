using System.Data;

using Modern.Lab.Hosting.Contracts;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Display;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class LogisticsManualReceiveDialogForm
    {
        // 표·정보 카드에 무엇을 어떻게 보일지. 쿼리 컬럼이 바뀌면 여기만 고친다.
        // Ehub 조회 결과도 Lot Management와 같은 Schemas.Wafer · Schemas.Lot 구조다 — 이 팝업에 보일 필드만 Only로 고른다.
        // slotColumn 은 화면이 Wafer Id에서 계산해 넣는 슬롯 표시 컬럼이다(서버 SLOT_NO가 아니다).

        private static GridColumns WaferColumns(DataTable wafers, string slotColumn)
        {
            return Schemas.Wafer.Columns(wafers)
                    .Only(Schemas.Wafer.WfId.Name, slotColumn)
                    .Column(slotColumn).Width(64d);
        }

        private static FieldDefinitions LotInfoDefinitions(DataTable lotInfo)
        {
            return Schemas.Lot.Fields(lotInfo)
                    .Only(
                            Schemas.Lot.ModelId.Name, Schemas.Lot.StatTyp.Name, Schemas.Lot.EventCd.Name, Schemas.Lot.BoxId.Name,
                            Schemas.Lot.StoreId.Name, Schemas.Lot.EventTm.Name, Schemas.Lot.MesStat.Name)
                    .Field(Schemas.Lot.ModelId.Name).Span(2);
        }
    }
}
