using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.WinForms.Controls.Selection;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class DurableHoldDialogForm
    {
        // Durable 정보 카드의 항목은 DurableDialogHelpers.SourceFields 가 정한다(RfId · Transport 창과 같은 항목).
        // 콤보의 표시·값 필드와 펼친 목록 컬럼은 엔티티(Schemas.ReasonCode · Schemas.User)에서 오고, 캡션·폭만 여기서 정한다.
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

        private static void ConfigureEngineers(ModernComboBox combo)
        {
            combo.UseEntity(Schemas.User);
            combo.DropDownStyle = ComboBoxStyle.DropDown;
            combo.AutoCompleteMode = AutoCompleteMode.Suggest;
            combo.AutoCompleteSource = AutoCompleteSource.ListItems;

            ModernDataGridColumn[] columns = ComboEntityExtensions.DropDownColumnsOf(Schemas.User);
            DropDownColumn(columns, Schemas.User.UserId.Name, "User Id", 150d);
            DropDownColumn(columns, Schemas.User.UserNm.Name, "Name", 240d);
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
