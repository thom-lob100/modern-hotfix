using System.Data;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Samples
{
    public partial class LabMonitorForm
    {
        // 펼친 열(열 머리를 누르면 전체 폭 표로 펼쳐지는 그리드)의 칸 꾸밈 — 랏 단계와 지속재 가지(Return · Swap)가 같은 그리드를 번갈아 쓴다.
        // 쿼리 컬럼이 바뀌면 여기만 고친다. 무엇을 채울지(조회)는 .Server.cs 가 갖는다. 카드의 랏 줄 문법은 프레젠터(LabMonitorPresenter)가 칸으로 만든다.

        protected override void DeclareColumns()
        {
            this.gridFocus.ConfigureColumns(FocusColumns);
        }

        private static GridColumns FocusColumns(DataTable table)
        {
            if (table != null && table.Columns.Contains(Schemas.MonitorLot.MonStage.Name))
            {
                return FocusLotColumns(table);
            }

            return FocusDurableColumns(table);
        }

        // 랏 단계 — 단계 · 의뢰서 · Lot · 수량 · Hold · 상태 · 대기시간이 앞에 오고, 프레젠터가 만든 그리기용 칸은 숨긴다.
        private static GridColumns FocusLotColumns(DataTable lots)
        {
            return Schemas.MonitorLot.Columns(lots)
                    .Hide(LabMonitorPresenter.RowKeyColumn, LabMonitorPresenter.RowKindColumn, LabMonitorPresenter.TitleColumn,
                            LabMonitorPresenter.SubtitleColumn, LabMonitorPresenter.QtyTextColumn, LabMonitorPresenter.BadgeTextColumn,
                            LabMonitorPresenter.BadgeKindColumn, LabMonitorPresenter.EdgeKindColumn, LabMonitorPresenter.LockColumn,
                            LabMonitorPresenter.ToolTipColumn, LabMonitorPresenter.IndentColumn, LabMonitorPresenter.WaitSecondsColumn,
                            LabMonitorPresenter.PrioritySortColumn)
                    .First(Schemas.MonitorLot.MonStage.Name, Schemas.MonitorLot.ReqSerialNo.Name, Schemas.MonitorLot.LotId.Name,
                            Schemas.MonitorLot.Qty.Name, Schemas.MonitorLot.LotHoldStatCd.Name, Schemas.MonitorLot.MonStatCd.Name,
                            LabMonitorPresenter.TrailingColumn)
                    .Column(LabMonitorPresenter.TrailingColumn).Caption("Waiting").Width(90d)
                    .Column(Schemas.MonitorLot.Qty.Name).Width(56d).Right()
                    .Column(Schemas.MonitorLot.LotHoldStatCd.Name).BadgeWidth(Schemas.MonitorLot.LotHoldStatCd.ValueCodes())
                    .Column(Schemas.MonitorLot.MonStatCd.Name).BadgeWidth(Schemas.MonitorLot.MonStatCd.ValueCodes());
        }

        // 지속재 가지(Return · Swap) — 지속재 · 반송 · 교체 · 위치 · 랏 수가 앞에 온다.
        private static GridColumns FocusDurableColumns(DataTable durables)
        {
            return Schemas.MonitorDurable.Columns(durables)
                    .Hide(LabMonitorPresenter.RowKeyColumn, LabMonitorPresenter.RowKindColumn, LabMonitorPresenter.TitleColumn,
                            LabMonitorPresenter.SubtitleColumn, LabMonitorPresenter.QtyTextColumn, LabMonitorPresenter.BadgeKindColumn,
                            LabMonitorPresenter.EdgeKindColumn, LabMonitorPresenter.LockColumn, LabMonitorPresenter.ToolTipColumn,
                            LabMonitorPresenter.IndentColumn, LabMonitorPresenter.WaitSecondsColumn, LabMonitorPresenter.PrioritySortColumn)
                    .First(Schemas.MonitorDurable.DurableId.Name, LabMonitorPresenter.BadgeTextColumn, LabMonitorPresenter.TrailingColumn)
                    .Column(LabMonitorPresenter.BadgeTextColumn).Caption("State").Badge()
                    .Column(LabMonitorPresenter.TrailingColumn).Caption("Since").Width(90d);
        }
    }
}
