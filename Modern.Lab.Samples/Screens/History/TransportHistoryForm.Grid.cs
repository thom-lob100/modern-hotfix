using System.Data;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Display;

namespace Modern.Lab.Samples
{
    public partial class TransportHistoryForm
    {
        // 표에 무엇을 어떻게 보일지 — 목록 표의 배지·폭, 정보 카드, 이력 표의 앞 컬럼. 쿼리 컬럼이 바뀌면 여기만 고친다.
        // 무엇을 채울지(조회)는 .Server.cs 가 갖는다. 목록은 Schemas.Transport, 이력은 Schemas.TransportHistory 를 쓴다.

        // 어느 컨트롤이 어느 컬럼 함수를 쓰는지 — 폼이 열릴 때 한 번 선언하면 조회 전에도 헤더가 보이고,
        // 그 뒤 폼은 DataSource = 표 만 대입한다.
        protected override void DeclareColumns()
        {
            this.gridTransports.ConfigureColumns(TransportColumns);
            TransportFields(null).Apply(this.fieldDetail);
            this.gridHistory.ConfigureColumns(HistoryColumns);
        }

        // 배지는 엔티티(DURABLE_TYP)가 갖는다 — 폭만 여기서 정한다.
        private static GridColumns TransportColumns(DataTable transports)
        {
            return Schemas.Transport.Columns(transports)
                    .Column(Schemas.Transport.DurableTyp.Name).BadgeWidth(Schemas.Transport.DurableTyp.ValueCodes());
        }

        private static FieldDefinitions TransportFields(DataTable transports)
        {
            return Schemas.Transport.Fields(transports)
                    .Field(Schemas.Transport.DurableTyp.Name, Schemas.Transport.LastEventCd.Name).Badge();
        }

        // Duration 컬럼은 조회 결과에 프리젠터가 덧붙이는 컬럼이라 첫 조회 뒤에 나타난다. 색·굵기는 여기서만 정한다.
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
