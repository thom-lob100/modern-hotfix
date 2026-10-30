using System;
using System.Drawing;
using System.Windows.Forms;

namespace Modern.Lab.WinForms.Controls.Hosting
{
    /// <summary>
    /// 지금 이 프로세스가 보는 화면 DPI. WinForms 의 <see cref="Control.DeviceDpi"/>는 첫 컨트롤을 만들 때 읽은 값을 끝까지
    /// 외운다 — DPI 매니페스트 없이(비인식으로) 시작한 셸에서 모던 컨트롤의 WPF 가 처음 로드되며 프로세스를 DPI 인식으로 바꾸면,
    /// 글꼴과 WPF 는 125% 로 커지는데 <c>DeviceDpi</c> 는 96 에 머물러 배율 보정이 한 번도 돌지 않았다(2026-10-30, 회사 125%:
    /// 배치는 100% 그대로고 글자만 커져 잘림). 화면 DC 는 바뀐 뒤의 값을 돌려주므로 둘 중 큰 쪽을 쓴다 — 매니페스트가 있는 exe 와
    /// 100% 에서는 두 값이 같다.
    /// </summary>
    public static class ScreenDpi
    {
        /// <summary><paramref name="control"/>이 놓인 프로세스의 지금 DPI(96 = 100%).</summary>
        public static int Of(Control control)
        {
            int cached = control == null ? 96 : control.DeviceDpi;
            return Math.Max(cached, FromScreen());
        }

        /// <summary>100% 기준 길이 <paramref name="logical"/>을 지금 DPI 로 키운다.</summary>
        public static int Scale(Control control, int logical)
        {
            return logical * Of(control) / 96;
        }

        private static int FromScreen()
        {
            try
            {
                using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
                {
                    return graphics.DpiX > 0f ? (int)Math.Round(graphics.DpiX) : 96;
                }
            }
            catch (Exception)
            {
                return 96;
            }
        }
    }
}
