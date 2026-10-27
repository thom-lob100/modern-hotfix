using System.Data;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Samples
{
    public partial class DurableDeassignDialogForm
    {
        // Durable 정보 카드의 항목은 DurableDialogHelpers.SourceFields 가 정한다(Adjust · Scrap 창과 같은 항목).
        // 내용물 표는 선택 지속재의 내용물 전부(GetDurableSlotList SUB_PROD_TYP= 빈 값)다 — 엔티티 Schemas.DurableWafer 로
        // Lot 상태 세 열까지 Wafer List 탭과 같은 모양이고 단위 Id 캡션만 "Wafer Id" 다. 표의 컬럼은 조회 결과가 정한다.
        // Wafer 만 든 지속재(FOUP)에서는 TRAY 용 열(Type · Finger Id · Finger Index · 출처 웨이퍼)을 숨긴다.
        private const string SourceWaferColumn = "SRC_WF_ID";

        private static GridColumns ContentColumns(DataTable contents)
        {
            GridColumns columns = Schemas.DurableWafer.Columns(contents)
                    .Column(Schemas.DurableWafer.WfId.Name).Caption("Wafer Id");

            if (!HasNonWafer(contents))
            {
                columns.Hide(
                        Schemas.DurableWafer.SubProdTyp.Name,
                        Schemas.DurableLamella.FingerId.Name, Schemas.DurableLamella.FingerIndex.Name,
                        SourceWaferColumn);
            }

            return columns;
        }

        private static bool HasNonWafer(DataTable contents)
        {
            if (contents == null)
            {
                return false;
            }

            foreach (DataRow row in contents.Rows)
            {
                string subProdTyp = TableHelper.CellText(row, Schemas.DurableWafer.SubProdTyp.Name).Trim();

                if (subProdTyp.Length > 0 && subProdTyp != Schemas.DurableWafer.SubProdTyp.Wafer.Value)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
