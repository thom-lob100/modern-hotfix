using System.Windows.Forms;
using Modern.Lab.Hosting.Theming;

namespace Modern.Lab.Hosting
{
    /// <summary>테마 설정 창 연결 — <see cref="ThemeDialogShortcut"/> 를 누르면 <see cref="ModernThemeDialogForm"/> 을 연다(2026-10-23).
    /// 이 파일을 함께 컴파일하지 않는 프로젝트(갤러리)에서는 단축키가 아무것도 하지 않는다.</summary>
    public partial class ModernFormBase
    {
        partial void ThemeDialogProcessCmdKey(Keys keyData, ref bool handled)
        {
            Keys shortcut = ThemeDialogShortcut;

            if (shortcut != Keys.None && keyData == shortcut && !this.IsDisposed)
            {
                ModernThemeDialogForm.ShowFor(this);
                handled = true;
            }
        }
    }
}
