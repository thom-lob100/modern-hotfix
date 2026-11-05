using System.Data;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Report.Services;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Display;

namespace Modern.Lab.Report
{
    public partial class MonitorDetailDialogForm
    {
        // 상세 팝업의 표시 — 의뢰서 카드 필드 · 웨이퍼 표 · Lot 이력 표. 쿼리 컬럼이 바뀌면 여기만 고친다.
        // 무엇을 채울지(조회)는 .Server.cs 와 베이스의 GetRequestInfo 가 갖는다. Lot History 화면과 같은 엔티티(Schemas.Wafer · LotHistory)를 쓴다.

        protected override void DeclareColumns()
        {
            this.gridWafers.ConfigureColumns(WaferColumns);
            this.gridHistory.ConfigureColumns(HistoryColumns);
        }

        private static GridColumns WaferColumns(DataTable wafers)
        {
            return Schemas.Wafer.Columns(wafers);
        }

        // 이력 표 앞에는 시각과 구간 시간(프리젠터가 덧붙이는 DURATION)이 온다 — Lot History 화면과 같다.
        private static GridColumns HistoryColumns(DataTable history)
        {
            return Schemas.LotHistory.Columns(history)
                    .First(Schemas.LotHistory.EventTm.Name, LotHistoryPresenter.DurationColumn)
                    .Column(Schemas.LotHistory.LotStatTyp.Name, Schemas.LotHistory.MesProcStatCd.Name).Badge();
        }

        // 의뢰서 카드 — 응답 스키마에서 자동 구성하고 번호 칸만 뺀다(제목에 있다). Request Info 창과 같은 구성이다.
        private static ModernFieldDefinition[] RequestFieldDefinitions(DataTable overview)
        {
            if (overview == null)
            {
                return new ModernFieldDefinition[0];
            }

            ModernDataGridColumn[] columns = Schemas.Request.Columns(overview)
                    .Hide(Schemas.Request.ReqSerialNo.Name)
                    .ToArray();
            ModernFieldDefinition[] fields = new ModernFieldDefinition[columns.Length];

            for (int index = 0; index < columns.Length; index++)
            {
                ModernFieldDefinition field = new ModernFieldDefinition(columns[index].DataPropertyName).Caption(columns[index].HeaderText);

                if (!string.IsNullOrEmpty(columns[index].Format))
                {
                    field.Format = columns[index].Format;
                }

                fields[index] = field;
            }

            return fields;
        }
    }
}
