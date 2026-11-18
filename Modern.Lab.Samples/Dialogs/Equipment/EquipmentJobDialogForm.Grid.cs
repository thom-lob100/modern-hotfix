using System.Data;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Samples
{
    public partial class EquipmentJobDialogForm
    {
        // Job Prep 에서 고르는 Lot 의 목록 — 표시 컬럼은 Lot Management 의 목록과 같은 엔티티 정의이고,
        // 맨 앞의 선택 칸(헤더 전체 선택)만 이 창이 붙인다. 모두 선택 = Lot 전체(WfIds 빈 값), 일부 선택 = 고른 WF_ID 만.
        private static GridColumns WaferColumns(DataRow lot, DataTable wafers, string selectColumn)
        {
            return Schemas.Wafer.Columns(wafers)
                    .Column(Schemas.Wafer.WfId.Name).Caption(WaferIdCaption(lot, wafers))
                    .Column(selectColumn).SelectBox(false);
        }

        // Lot 하나는 한 종류(Wafer · Chip · Lamella)다. 결정 Lot 행(GetJobLotList)의 SUB_PROD_TYP 로 캡션을 고르고,
        // 그 칸이 없으면 목록 첫 행의 SUB_PROD_TYP, 둘 다 없으면 Wafer 다.
        private static string WaferIdCaption(DataRow lot, DataTable wafers)
        {
            string kind = KindOf(lot, wafers);

            if (kind == Schemas.Wafer.SubProdTyp.Chip.Value)
            {
                return "Chip Id";
            }

            if (kind == Schemas.Wafer.SubProdTyp.Lamella.Value)
            {
                return "Lamella Id";
            }

            return "Wafer Id";
        }

        private static string WaferCardCaption(DataRow lot, DataTable wafers)
        {
            return WaferIdCaption(lot, wafers).Replace(" Id", string.Empty) + " List";
        }

        private static string KindOf(DataRow lot, DataTable wafers)
        {
            if (lot != null && lot.Table.Columns.Contains(Schemas.LotPriority.SubProdTyp.Name)
                    && Schemas.LotPriority.SubProdTyp.Text(lot).Length > 0)
            {
                return Schemas.LotPriority.SubProdTyp.Text(lot);
            }

            if (wafers == null || wafers.Rows.Count == 0 || !wafers.Columns.Contains(Schemas.Wafer.SubProdTyp.Name))
            {
                return string.Empty;
            }

            return Schemas.Wafer.SubProdTyp.Text(wafers.Rows[0]);
        }
    }
}
