using System;
using System.Threading;
using System.Windows.Forms;

using Modern.Lab.Controls.Wpf.Display;

namespace Modern.Lab.WinForms.Controls.Dialogs
{
    /// <summary>
    /// UI 스레드에서 아무도 잡지 않은 예외를 .NET 기본 창("Unhandled exception has occurred…" + 스택) 대신
    /// <see cref="ModernMessageDialog"/> 로 보인다 — 본문은 현업이 읽을 한 문장이고, 예외 원문과 스택은
    /// 접힌 상세로 내려가 "Details → Copy" 로 개발자에게 보낼 수 있다. 앱은 닫히지 않고 이어진다.
    ///
    /// 진입점(<c>Program.Main</c>)에서 <b>첫 컨트롤을 만들기 전에</b> <see cref="Install"/> 을 한 번 부른다 —
    /// <see cref="Application.SetUnhandledExceptionMode(UnhandledExceptionMode)"/> 는 컨트롤 핸들이 생긴 뒤에는 거부된다.
    /// 백그라운드 스레드의 예외는 여기로 오지 않는다(.NET 이 프로세스를 끝낸다) — 폼 베이스의 조회·실행은 자기 스레드에서
    /// 잡아 같은 다이얼로그로 보이므로 이 처리기는 이벤트 핸들러 안의 예외만 받는다.
    /// </summary>
    public static class UnhandledExceptionNotice
    {
        private static bool installed;
        private static bool showing;

        /// <summary>창 제목 — 기본은 영어이고 한국어 화면은 진입점에서 바꾼다.</summary>
        public static string Caption { get; set; }

        /// <summary>본문 — "Why:" 줄과 "Next:" 줄(라벨은 다이얼로그 강조 표기로 액센트 굵게). 한국어 화면은 통째로 바꾼다.</summary>
        public static string Message { get; set; }

        static UnhandledExceptionNotice()
        {
            string marker = ModernMessageDialogControl.EmphasisMarker;

            Caption = "Unexpected error";
            Message = marker + "Why:" + marker + " An unexpected error interrupted the last action."
                    + Environment.NewLine
                    + marker + "Next:" + marker + " Check the screen, then copy the details and send them to support.";
        }

        /// <summary>UI 스레드의 잡히지 않은 예외를 모던 다이얼로그로 돌린다. 두 번 불러도 한 번만 건다.</summary>
        public static void Install()
        {
            if (installed)
            {
                return;
            }

            installed = true;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += OnThreadException;
        }

        /// <summary>예외 하나를 같은 모양으로 보인다 — 직접 잡은 곳에서도 쓸 수 있다.</summary>
        /// <param name="failure">보일 예외(null 이면 상세 없이 본문만).</param>
        public static void Show(Exception failure)
        {
            // 다이얼로그를 그리다 또 터지면 되풀이하지 않는다 — 두 번째 예외는 버린다.
            if (showing)
            {
                return;
            }

            showing = true;

            try
            {
                ModernMessageDialog.ShowError(
                        Form.ActiveForm,
                        Caption,
                        Message,
                        failure == null ? string.Empty : failure.ToString());
            }
            catch (Exception)
            {
                // 창조차 못 띄우는 상태(핸들 소멸 · 리소스 고갈) — 기본 창으로 되돌아가면 같은 자리에서 다시 터진다.
            }
            finally
            {
                showing = false;
            }
        }

        private static void OnThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Show(e.Exception);
        }
    }
}
