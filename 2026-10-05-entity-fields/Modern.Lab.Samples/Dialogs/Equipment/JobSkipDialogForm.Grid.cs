using System.Data;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.WinForms.Controls.Display;
using Modern.Lab.WinForms.Controls.Selection;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class JobSkipDialogForm
    {
        // Lot 정보 표시 순서와 콤보의 코드·명칭 컬럼은 회사에서 이 파일을 변경한다.
        // Lot 행은 Equipment / Lots 의 Lot 목록 행(Schemas.LotPriority)이다 — Hold 창과 같은 구성으로 보인다.
        // 콤보의 표시·값 필드와 펼친 목록 컬럼은 엔티티(Schemas.ReasonCode)에서 오고, 캡션·폭만 여기서 정한다.
        private static FieldDefinitions SourceDefinitions(DataTable lot)
        {
            return Schemas.LotPriority.Fields(lot).Only(Schemas.LotPriority.LotId.Name, Schemas.LotPriority.SubProdTyp.Name,
                    Schemas.LotPriority.LotStatTyp.Name, Schemas.LotPriority.LotHoldStatCd.Name,
                    Schemas.LotPriority.ProdId.Name, Schemas.LotPriority.FlowId.Name, Schemas.LotPriority.OperId.Name,
                    Schemas.LotPriority.LastEventCd.Name);
        }

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
                    column.Caption(caption).Width(width);
                }
            }
        }
    }
}
