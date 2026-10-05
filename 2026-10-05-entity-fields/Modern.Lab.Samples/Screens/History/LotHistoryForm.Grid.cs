using System.Data;
using Modern.Lab.Data;
using Modern.Lab.Controls.Wpf.Data;

using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Display;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class LotHistoryForm
    {
        // 표에 무엇을 어떻게 보일지 — 트리에 남길 컬럼과 폭·배지. 쿼리 컬럼이 바뀌면 여기만 고친다.
        // 무엇을 채울지(조회)는 .Server.cs 가 갖는다. 이력 표의 나머지 손질은 HistoryColumns 가 이어서 한다.
        // 트리는 Schemas.LotTree, 목록·이력은 Lot Management와 같은 Schemas.Wafer · LotHistory · WaferHistory를 쓴다.

        private const string NodeColorColumn = "NODE_COLOR";

        // 폐기로 강조할 상태와 색은 여기서 함께 변경한다. Emptied는 기존처럼 강조하지 않는다.
        private static string ScrapForeColor
        {
            get { return Modern.Lab.Theming.ModernTheme.IsDarkBased ? "#5C2B2F" : "#FDE7E9"; }
        }

        private static void ApplyScrapColor(DataTable tree)
        {
            if (tree == null)
            {
                return;
            }

            if (!tree.Columns.Contains(NodeColorColumn))
            {
                tree.Columns.Add(NodeColorColumn, typeof(string));
            }

            foreach (DataRow row in tree.Rows)
            {
                if (TableHelper.CellText(row, Schemas.LotTree.LotStatTyp.Name) == Schemas.LotTree.LotStatTyp.Scrapped.Value)
                {
                    row[NodeColorColumn] = ScrapForeColor;
                }
            }
        }

        // 배지는 엔티티(SUB_PROD_TYP · LOT_STAT_TYP)가 갖는다 — 트리에 남길 컬럼과 폭만 여기서 정한다.
        private static GridColumns LotTreeColumns(DataTable tree)
        {
            return Schemas.LotTree.Columns(tree)
                    .Only(Schemas.LotTree.SubProdTyp.Name, Schemas.LotTree.LotStatTyp.Name, Schemas.LotTree.Qty.Name)
                    .Column(Schemas.LotTree.SubProdTyp.Name).BadgeWidth(Schemas.LotTree.SubProdTyp.ValueCodes())
                    .Column(Schemas.LotTree.LotStatTyp.Name).BadgeWidth(Schemas.LotTree.LotStatTyp.ValueCodes())
                    .Column(Schemas.LotTree.Qty.Name).Width(56d);
        }

        private static FieldDefinitions LotTreeFields(DataTable tree)
        {
            return Schemas.LotTree.Fields(tree)
                    .Field(Schemas.LotTree.SubProdTyp.Name, Schemas.LotTree.LotStatTyp.Name, Schemas.LotTree.MesProcStatCd.Name).Badge()
                    .Field(Schemas.LotTree.ReqSerialNo.Name).Link()
                    .Field(Schemas.LotTree.Description.Name).Span(3).StartAt(3);
        }

        // Lot 이력·Wafer 이력 공용 — 표마다 자기 엔티티(Schemas.LotHistory · WaferHistory)를 넘긴다.
        private static GridColumns HistoryBaseColumns(EntityDefinition entity, DataTable history)
        {
            return entity.Columns(history)
                    .First(Schemas.LotHistory.EventTm.Name, LotHistoryPresenter.DurationColumn)
                    .Column(Schemas.LotHistory.LotStatTyp.Name, Schemas.LotHistory.MesProcStatCd.Name).Badge();
        }

        private static GridColumns WaferColumns(DataTable wafers)
        {
            return Schemas.Wafer.Columns(wafers);
        }

        private static ModernDataGridColumn[] HistoryColumns(EntityDefinition entity, DataTable history)
        {
            ModernDataGridColumn[] columns = HistoryBaseColumns(entity, history).ToArray();

            foreach (ModernDataGridColumn column in columns)
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
