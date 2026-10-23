using System;
using System.Windows.Forms;
using Modern.Lab.Hosting.Theming;

namespace Modern.Lab.Hosting
{
    /// <summary>테마 설정 창 연결 — <see cref="ThemeDialogShortcut"/> 를 누르면 <see cref="ModernThemeDialogForm"/> 을 연다(2026-10-23).
    /// 키는 두 길로 받는다: ① 폼의 <c>ProcessCmdKey</c>(포커스가 이 폼 안에 있을 때), ② 프로세스 전체 메시지 필터(포커스가 셸의
    /// 다른 컨트롤에 있거나 아무 컨트롤에도 없을 때 — 셸에 띄운 화면에서 Ctrl+Shift+T 가 안 먹던 2026-10-23 보고). 필터는 첫 모던 폼이
    /// 만들어질 때 한 번 등록되고, 활성 창을 소유자로 창을 연다. 이 파일을 함께 컴파일하지 않는 프로젝트(갤러리)에서는 단축키가 없다.</summary>
    public partial class ModernFormBase
    {
        private static bool themeShortcutFilterInstalled;
        private static bool themeDialogOpen;

        partial void ThemeDialogProcessCmdKey(Keys keyData, ref bool handled)
        {
            Keys shortcut = ThemeDialogShortcut;

            if (shortcut != Keys.None && keyData == shortcut && !this.IsDisposed)
            {
                ShowThemeDialog(this);
                handled = true;
            }
        }

        partial void ThemeShortcutInstall()
        {
            if (themeShortcutFilterInstalled)
            {
                return;
            }

            themeShortcutFilterInstalled = true;
            Application.AddMessageFilter(new ThemeShortcutFilter());
        }

        // 한 번에 하나만 — 열려 있는 동안의 재입력은 무시한다.
        private static void ShowThemeDialog(IWin32Window owner)
        {
            if (themeDialogOpen)
            {
                return;
            }

            themeDialogOpen = true;

            try
            {
                ModernThemeDialogForm.ShowFor(owner);
            }
            finally
            {
                themeDialogOpen = false;
            }
        }

        // WM_KEYDOWN / WM_SYSKEYDOWN 에서 단축키를 잡는다 — 포커스가 어디에 있든 메시지 루프를 지나는 키는 모두 본다.
        private sealed class ThemeShortcutFilter : IMessageFilter
        {
            private const int WmKeyDown = 0x0100;
            private const int WmSysKeyDown = 0x0104;

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg != WmKeyDown && m.Msg != WmSysKeyDown)
                {
                    return false;
                }

                Keys shortcut = ThemeDialogShortcut;

                if (shortcut == Keys.None)
                {
                    return false;
                }

                Keys pressed = ((Keys)(int)(long)m.WParam & Keys.KeyCode) | Control.ModifierKeys;

                if (pressed != shortcut)
                {
                    return false;
                }

                Form active = Form.ActiveForm;
                ShowThemeDialog(active != null && !active.IsDisposed ? active : null);
                return true;
            }
        }
    }
}
