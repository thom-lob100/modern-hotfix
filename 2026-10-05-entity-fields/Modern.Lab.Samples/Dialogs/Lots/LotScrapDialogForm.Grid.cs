using System.Data;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Display;
using Modern.Lab.WinForms.Controls.Selection;

namespace Modern.Lab.Samples
{
    public partial class LotScrapDialogForm
    {
        // Scrap 전에 보여 줄 Lot 정보와 순서. 회사에서 쿼리 컬럼이 바뀌면 여기만 고친다.
        private static FieldDefinitions SourceDefinitions(DataTable lot)
        {
            return Schemas.Lot.Fields(lot).Only(Schemas.Lot.LotId.Name, Schemas.Lot.SubProdTyp.Name,
                    Schemas.Lot.LotStatTyp.Name, Schemas.Lot.LotHoldStatCd.Name,
                    Schemas.Lot.ProdId.Name, Schemas.Lot.FlowId.Name, Schemas.Lot.OperId.Name, Schemas.Lot.Qty.Name);
        }

        // 부모 화면(Lot Management)의 단위 목록을 그대로 쓴다 — 표시 컬럼은 부모와 같은 엔티티 정의이고,
        // 맨 앞의 선택 칸(헤더 전체 선택)만 이 창이 붙인다. 모두 선택 = 전체 Scrap, 일부 선택 = 부분 Scrap.
        private static GridColumns WaferColumns(DataTable wafers, string idCaption, string selectColumn)
        {
            return Schemas.Wafer.Columns(wafers)
                    .Column(Schemas.Wafer.WfId.Name).Caption(idCaption)
                    .Column(selectColumn).SelectBox(false);
        }

        // 콤보의 표시·값 필드와 펼친 목록 컬럼은 엔티티(Schemas.ReasonCode)에서 오고, 캡션·폭만 여기서 정한다.
        private static void ConfigureCodes(ModernComboBox combo)
        {
            combo.UseEntity(Schemas.ReasonCode);
            combo.CharacterCasing = CharacterCasing.Upper;
            combo.AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-";
            combo.DropDownStyle = ComboBoxStyle.DropDown;
            combo.AutoCompleteMode = AutoCompleteMode.Suggest;
            combo.AutoCompleteSource = AutoCompleteSource.ListItems;

            ModernDataGridColumn[] columns = ComboEntityExtensions.DropDownColumnsOf(Schemas.ReasonCode);
            DropDownColumn(columns, Schemas.ReasonCode.ReasonCd.Name, "Code", 150d);
            DropDownColumn(columns, Schemas.ReasonCode.CtnDesc.Name, "Description", 240d);
            combo.ConfigureDropDownColumns(columns);
        }

        private static void DropDownColumn(ModernDataGridColumn[] columns, string name, string caption, double width)
        {
            foreach (ModernDataGridColumn column in columns)
            {
                if (column.DataPropertyName == name)
                {
                    column.HeaderText = caption;
                    column.Width = width;
                }
            }
        }
    }
}
