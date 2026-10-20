using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Display
{
    /// <summary>
    /// 자동 소멸 알림 토스트 (MessageBox 남용을 대체; WPF ModernToastControl을
    /// ElementHost로 호스팅).
    ///
    /// 폼에 하나 배치해 두고 Show(message, kind)를 호출하면 부모의 우하단에
    /// 나타난다 — 단, 부모 아래에 붙은(Dock=Bottom) 보이는 컨트롤(실행 버튼 줄)이 있으면 그 높이만큼
    /// 위로 올라가 버튼을 가리지 않는다(2026-10-20 현장 보고: 토스트가 떠 있는 동안 우하단 실행 버튼을 못 눌렀다).
    /// 수명은 종류별이다 (2026-08-23 현장 보고: 에러 문구가 잘리고
    /// 읽기 전에 사라졌다):
    /// - Info/Success: DurationMs(기본 2500) 후 자동 소멸 — "저장되었습니다" 류.
    /// - Warning: 최소 6초 보장 — 읽고 대처할 시간이 필요하다.
    /// - Error: 자동 소멸 없음(스티키) — 닫기 ✕ 또는 카드 클릭으로 닫는다.
    /// 마우스를 올리면 읽는 동안 타이머가 멈추고, 카드 클릭은 즉시 닫는다.
    /// Error/Warning은 복사 버튼으로 문구 전문을 클립보드에 담을 수 있다.
    /// 사용자의 확인(예/아니오)이 필요한 경우에는 계속 MessageBox를 쓴다.
    /// </summary>
    [ToolboxItem(true)]
    public class ModernToast : WpfElementHostBase<Modern.Lab.Controls.Wpf.Display.ModernToastControl>
    {
        private const int EdgeMargin = 16;

        // Warning은 읽고 대처할 시간이 필요해 최소 유지 시간을 보장한다.
        private const int WarningMinimumDurationMs = 6000;

        private readonly Timer hideTimer;
        private Control trackedParent;
        private int durationMs;
        private bool wpfWired;
        private bool sticky;

        /// <summary>기본 크기와 숨김 상태로 컨트롤을 생성한다.</summary>
        public ModernToast()
            : base(true)
        {
            this.Size = new Size(280, 44);
            this.durationMs = 2500;

            this.hideTimer = new Timer();
            this.hideTimer.Tick += this.OnHideTimerTick;

            // 토스트는 Show가 호출될 때만 나타난다. 디자인 타임에는 배치를 위해
            // 보이는 상태를 유지한다.
            if (!this.DesignMode)
            {
                this.Visible = false;
            }
        }

        /// <summary>
        /// 표시 유지 시간(밀리초). 기본 2500. Info/Success에 그대로 적용되고,
        /// Warning은 이 값과 6초 중 큰 쪽, Error는 무시된다(수동 닫기).
        /// </summary>
        [Category("Modern")]
        [Description("Toast display duration (ms) — applies to Info/Success; Warning holds at least 6s; Error stays until closed")]
        [DefaultValue(2500)]
        public int DurationMs
        {
            get
            {
                return this.durationMs;
            }
            set
            {
                this.durationMs = Math.Max(500, value);
            }
        }

        /// <summary>정보(Info) 종류로 알림을 표시한다.</summary>
        public void Show(string message)
        {
            this.Show(message, Modern.Lab.Controls.Wpf.Display.ToastKind.Info);
        }

        /// <summary>
        /// 알림을 표시한다: 내용에 맞게 크기를 조정하고 부모 우하단에 배치한다.
        /// Error가 아니면 종류별 시간 후 자동으로 숨는다. 연속 호출 시 마지막
        /// 메시지로 교체되고 타이머가 다시 시작된다.
        /// </summary>
        public void Show(string message, Modern.Lab.Controls.Wpf.Display.ToastKind kind)
        {
            // 종류가 정하는 기본 수명 — Error만 스티키다.
            this.Show(message, kind, kind == Modern.Lab.Controls.Wpf.Display.ToastKind.Error);
        }

        /// <summary>
        /// 알림을 표시하고 **지속 여부를 종류와 따로** 정한다. 사라지는 알림으로는 지속 상태
        /// (낡은 데이터·계약 위반)를 알릴 수 없고, 그렇다고 그 상태에 실패의 빨강을 쓸 이유도
        /// 없다 — 톤(<paramref name="kind"/>)과 수명(<paramref name="sticky"/>)은 직교한다.
        /// <paramref name="sticky"/>가 참이면 사용자가 닫을 때까지 남는다.
        /// </summary>
        public void Show(
                string message, Modern.Lab.Controls.Wpf.Display.ToastKind kind, bool sticky)
        {
            // 여기가 토스트의 "첫 사용" 지점 — WPF 섬을 이때 만든다(지연 생성).
            this.EnsureWpf();

            if (this.Wpf == null)
            {
                return;
            }

            this.WireWpfEvents();

            this.Wpf.Message = message;
            this.Wpf.Kind = kind;

            // 내용에 맞는 크기 계산 — 본문 TextBlock의 MaxWidth가 긴 문구를
            // 여러 줄로 접으므로 폭에는 자연 상한이 있다.
            this.Wpf.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
            int width = (int)Math.Ceiling(this.Wpf.DesiredSize.Width);
            int height = (int)Math.Ceiling(this.Wpf.DesiredSize.Height);

            // 그래도 부모보다 넓어질 수는 없다 — 부모 폭을 상한으로 잡고 높이를 다시 잰다.
            if (this.Parent != null)
            {
                int maxWidth = Math.Max(160, this.Parent.ClientSize.Width - (EdgeMargin * 2));

                if (width > maxWidth)
                {
                    this.Wpf.Measure(new System.Windows.Size(maxWidth, double.PositiveInfinity));
                    width = maxWidth;
                    height = (int)Math.Ceiling(this.Wpf.DesiredSize.Height);
                }
            }

            this.Size = new Size(Math.Max(160, width), Math.Max(40, height));

            this.RepositionBottomRight();

            this.Visible = true;
            this.BringToFront();

            // 수명 — 스티키면 사용자가 닫을 때까지 남는다(자동 소멸 없음).
            this.sticky = sticky;
            this.hideTimer.Stop();

            if (!this.sticky)
            {
                this.hideTimer.Interval = this.EffectiveDuration(kind);
                this.hideTimer.Start();
            }
        }

        /// <summary>표시 중인 알림을 즉시 숨긴다.</summary>
        public void HideToast()
        {
            this.hideTimer.Stop();
            this.Visible = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.hideTimer.Dispose();
            }

            base.Dispose(disposing);
        }

        private int EffectiveDuration(Modern.Lab.Controls.Wpf.Display.ToastKind kind)
        {
            if (kind == Modern.Lab.Controls.Wpf.Display.ToastKind.Warning)
            {
                return Math.Max(this.durationMs, WarningMinimumDurationMs);
            }

            return this.durationMs;
        }

        // WPF 자식 이벤트 연결 — 지연 생성 뒤 첫 Show에서 한 번만.
        private void WireWpfEvents()
        {
            if (this.wpfWired || this.Wpf == null)
            {
                return;
            }

            this.wpfWired = true;

            // 읽는 중에는 사라지지 않게 — 호버 동안 타이머를 멈춘다.
            this.Wpf.MouseEnter += this.OnWpfMouseEnter;
            this.Wpf.MouseLeave += this.OnWpfMouseLeave;

            // 카드 클릭 = 닫기(사라지는 알림에만). 복사/닫기 버튼 클릭은 버튼이 이벤트를
            // 소비하므로 여기로 버블링되지 않는다.
            this.Wpf.MouseLeftButtonUp += this.OnWpfCardClick;

            this.Wpf.CopyRequested += this.OnWpfCopyRequested;
            this.Wpf.CloseRequested += this.OnWpfCloseRequested;
        }

        private void OnWpfMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.hideTimer.Stop();
        }

        private void OnWpfMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // 남은 시간을 추적하지 않고 전체 시간으로 다시 센다 — 방금까지 읽던
            // 사용자에게는 남은 반 박자보다 후하게 주는 쪽이 안전하다.
            if (this.Visible && !this.sticky)
            {
                this.hideTimer.Stop();
                this.hideTimer.Start();
            }
        }

        private void OnWpfCardClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // 지속 알림(스티키)은 우발적 클릭으로 닫히지 않는다 — 낡은 데이터·계약 위반을 알리는
            // 유일한 자리라, 닫는 것은 닫기 버튼을 누른 사람의 뜻이어야 한다.
            if (this.sticky)
            {
                return;
            }

            this.HideToast();
        }

        private void OnWpfCopyRequested(object sender, EventArgs e)
        {
            // 클립보드 잠금(원격 데스크톱·클립보드 관리 도구)으로 실패할 수 있다 —
            // 성공했을 때만 체크 피드백을 보여준다.
            if (SafeClipboard.TrySetText(this.Wpf.Message))
            {
                this.Wpf.ShowCopyConfirmation();
            }
        }

        private void OnWpfCloseRequested(object sender, EventArgs e)
        {
            this.HideToast();
        }

        // 부모 크기가 바뀌면 우하단을 다시 잡는다 — 스티키 알림은 창을 최대화·복원하는 동안에도
        // 떠 있으므로, Show 시점의 좌표에 머물면 화면 밖으로 나가거나 가운데에 남는다.
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            if (this.trackedParent != null)
            {
                this.trackedParent.ClientSizeChanged -= this.OnParentClientSizeChanged;
            }

            this.trackedParent = this.Parent;

            if (this.trackedParent != null)
            {
                this.trackedParent.ClientSizeChanged += this.OnParentClientSizeChanged;
            }
        }

        private void OnParentClientSizeChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                this.RepositionBottomRight();
            }
        }

        private void RepositionBottomRight()
        {
            if (this.Parent == null)
            {
                return;
            }

            this.Location = new Point(
                Math.Max(0, this.Parent.ClientSize.Width - this.Width - EdgeMargin),
                Math.Max(0, this.Parent.ClientSize.Height - this.BottomReserve() - this.Height - EdgeMargin));
        }

        // 부모 아래에 붙어 있는 형제 컨트롤(실행 버튼 줄 · 그 위 간격)의 높이 합 — 토스트는 그 위에 뜬다.
        // 다이얼로그처럼 아래에 붙은 것이 없으면 0 이라 예전과 같은 자리다.
        private int BottomReserve()
        {
            int reserve = 0;

            foreach (Control sibling in this.Parent.Controls)
            {
                if (sibling != this && sibling.Visible && sibling.Dock == DockStyle.Bottom)
                {
                    reserve += sibling.Height;
                }
            }

            return reserve;
        }

        private void OnHideTimerTick(object sender, EventArgs e)
        {
            this.HideToast();
        }
    }
}
