using System;
using System.Windows.Threading;

namespace Modern.Lab.WinForms.Controls.Hosting
{
    /// <summary>
    /// WPF 렌더링이 한가해진 뒤(<c>DispatcherPriority.ApplicationIdle</c>) 할 일을 미룬다 — 상위 Common(Hosting)이
    /// WPF 어셈블리를 참조하지 않고도 같은 순서로 일을 미루도록 여기 둔다(2026-10-30).
    /// </summary>
    public static class WpfIdle
    {
        /// <summary>현재 UI 스레드의 WPF 디스패처가 한가해지면 <paramref name="action"/>을 한 번 부른다.</summary>
        public static void Post(Action action)
        {
            if (action == null)
            {
                return;
            }

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, action);
        }
    }
}
