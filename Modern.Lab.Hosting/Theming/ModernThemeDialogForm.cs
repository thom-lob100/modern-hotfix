using System;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Theming;

namespace Modern.Lab.Hosting.Theming
{
    /// <summary>
    /// 테마 설정 창 — 테마 목록에서 하나를 골라 <see cref="ModernThemeSetting"/> 파일에 저장한다(2026-10-23).
    /// 정적 테마라 지금 창은 바뀌지 않고 다음 실행부터 적용된다. 모든 모던 폼에서 <see cref="ModernFormBase.ThemeDialogShortcut"/>(기본 Ctrl+Shift+T)로
    /// 열리고, 회사 셸 메뉴에서는 <see cref="ShowFor"/> 를 부른다.
    /// </summary>
    public partial class ModernThemeDialogForm : ModernFormBase
    {
        private const string NameColumn = "NAME";
        private const string ValueColumn = "VALUE";
        private const string RestartNote = "Applies the next time the program starts.";
        private const string SavedNote = "Saved. Restart the program to apply.";

        public ModernThemeDialogForm()
        {
            this.InitializeComponent();
            this.InitializeModernForm(false);
        }

        /// <summary>창을 모달로 연다 — 셸 메뉴나 단축키에서 부르는 하나의 입구.</summary>
        public static void ShowFor(IWin32Window owner)
        {
            using (ModernThemeDialogForm dialog = new ModernThemeDialogForm())
            {
                dialog.ShowDialog(owner);
            }
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            if (IsDesignTime() || this.DesignMode)
            {
                return;
            }

            this.cboTheme.DisplayMember = NameColumn;
            this.cboTheme.ValueMember = ValueColumn;
            this.cboTheme.DataSource = BuildThemeTable();

            ModernTheme.ThemeMode saved;
            this.cboTheme.SelectedValue = (ModernThemeSetting.TryLoad(out saved) ? saved : ModernTheme.Mode).ToString();
            this.lblPath.Text = ModernThemeSetting.FilePath;
            this.lblNote.Text = RestartNote;
        }

        private static DataTable BuildThemeTable()
        {
            DataTable table = new DataTable();
            table.Locale = System.Globalization.CultureInfo.InvariantCulture;
            table.Columns.Add(NameColumn, typeof(string));
            table.Columns.Add(ValueColumn, typeof(string));

            foreach (string name in Enum.GetNames(typeof(ModernTheme.ThemeMode)))
            {
                table.Rows.Add(name, name);
            }

            return table;
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            string name = Convert.ToString(this.cboTheme.SelectedValue) ?? string.Empty;
            ModernTheme.ThemeMode mode;

            if (!Enum.TryParse(name, true, out mode))
            {
                this.ShowWarningMessage("Select a theme.");
                return;
            }

            try
            {
                ModernThemeSetting.Save(mode);
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage("Theme not saved", "The theme file could not be written.", ex.ToString());
                return;
            }

            this.lblNote.Text = SavedNote;
        }

        private void OnCloseClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
