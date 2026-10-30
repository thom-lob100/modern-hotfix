using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

using Modern.Lab.Theming;

namespace Modern.Lab.Hosting
{

    /// <summary>
    /// 모든 업무 화면 · 다이얼로그의 베이스. 상속할 폼 클래스(홈은 <c>Form</c>)는 <c>ModernFormBase.Host.cs</c> 한 파일에만
    /// 적혀 있다 — 회사는 그 파일을 자기 폼 베이스와 <see cref="AdaptToHost"/> 구현으로 바꿔 둔다
    /// (2026-10-21 현장 보고: 본체가 바뀔 때마다 상속과 <c>UseSkin = false</c> 를 다시 넣어야 했다).
    /// </summary>
    public partial class ModernFormBase
    {
        private readonly Dictionary<string, int> loadVersions = new Dictionary<string, int>();

        /// <summary>생성 직후, 파생 폼의 <c>InitializeComponent</c> 보다 먼저 호스트(회사 폼 베이스) 맞춤을 한 번 부른다.</summary>
        protected ModernFormBase()
        {
            this.AdaptToHost();
            this.ThemeShortcutInstall();
        }

        // 호스트 맞춤 자리 — 구현은 ModernFormBase.Host.cs 에만 둔다. 홈은 구현이 없어 컴파일러가 호출을 지운다.
        partial void AdaptToHost();

        private Modern.Lab.WinForms.Controls.Display.ModernToast sharedToast;

        private Modern.Lab.WinForms.Controls.Display.ModernToolTip actionReasonTip;

        private Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay sharedBusyOverlay;

        private bool connectionLost;

        private bool failureNoticeShowing;

        private bool stickyNoticeShowing;

        private bool loadFailureSilent;

        private string contractNoticeText = string.Empty;

        private string renderedNoticeBasis = string.Empty;

        private readonly List<Exception> pendingFailures = new List<Exception>();
        private string showingErrorDetails = string.Empty;

        private readonly Dictionary<string, DateTime> lastLoadSuccessTimes =
                new Dictionary<string, DateTime>(StringComparer.Ordinal);
        private readonly HashSet<string> staleChannels =
                new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> silentFailureChannels =
                new HashSet<string>(StringComparer.Ordinal);
        private DateTime lastAnyLoadSuccessTime;

        private const string failureDialogKey = "modernformbase.load-failure";

        private readonly Dictionary<string, System.Windows.Forms.Timer> debounceTimers =
                new Dictionary<string, System.Windows.Forms.Timer>();

        protected void InitializeModernForm(bool useLoadCover = true)
        {
            if (useLoadCover)
            {
                ModernLoadCover.Attach(this);
            }

            this.EnsureFormInitialized();
        }

        protected void InitializeModernForm(Control coverTarget)
        {
            this.InitializeModernForm(coverTarget, true);
        }

        protected void InitializeModernForm(Control coverTarget, bool autoReleaseOnIdle)
        {
            this.InitializeModernForm(coverTarget, autoReleaseOnIdle, ModernLoadCover.DefaultMessage);
        }

        protected void InitializeModernForm(
                Control coverTarget, bool autoReleaseOnIdle, string coverMessage)
        {
            ModernLoadCover.Attach(this, coverTarget, coverMessage, autoReleaseOnIdle);
            this.EnsureFormInitialized();
        }

        protected void InitializeModernFormWithMessage(string coverMessage)
        {
            ModernLoadCover.Attach(this, null, coverMessage, true);
            this.EnsureFormInitialized();
        }

        protected void ReleaseLoadCover()
        {
            ModernLoadCover.Release(this);
        }

        private readonly List<Modern.Lab.WinForms.Controls.Data.ModernDataGrid> findShortcutGrids =
                new List<Modern.Lab.WinForms.Controls.Data.ModernDataGrid>();

        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid lastFocusedFindGrid;

        /// <summary>
        /// 폼 어디서나 쓰는 찾기 단축키에 그리드를 등록한다 — 본 목록을 첫 인자로 둔다. <b>Ctrl+F</b>는 마지막으로 포커스를 받은
        /// 등록 그리드(없으면 보이는 첫 그리드)에 찾기 바를 연다. <b>F3 / Shift+F3</b>은 조회 조건 칸이나 다른 그리드에 포커스가
        /// 있어도 찾기 바가 열린 등록 그리드가 다음/이전 일치 행으로 이동한다(2026-10-30). 바가 열린 그리드 안의 F3은 그리드가 먼저 처리한다.
        /// </summary>
        protected void RegisterFindShortcut(
                params Modern.Lab.WinForms.Controls.Data.ModernDataGrid[] grids)
        {
            if (grids == null)
            {
                return;
            }

            foreach (Modern.Lab.WinForms.Controls.Data.ModernDataGrid grid in grids)
            {
                if (grid == null)
                {
                    continue;
                }

                Modern.Lab.WinForms.Controls.Data.ModernDataGrid captured = grid;
                this.findShortcutGrids.Add(captured);

                captured.Enter += delegate { this.lastFocusedFindGrid = captured; };
            }
        }

        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid ResolveFindShortcutGrid()
        {
            if (this.lastFocusedFindGrid != null
                    && this.lastFocusedFindGrid.Visible
                    && this.lastFocusedFindGrid.AllowFindPanel)
            {
                return this.lastFocusedFindGrid;
            }

            foreach (Modern.Lab.WinForms.Controls.Data.ModernDataGrid grid in this.findShortcutGrids)
            {
                if (grid.Visible && grid.AllowFindPanel)
                {
                    return grid;
                }
            }

            return null;
        }

        // F3 · Shift+F3 을 받을 그리드 — 찾기 바가 열려 있고 보이는 등록 그리드. 마지막으로 포커스를 받은 그리드가 먼저다.
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid ResolveOpenFindGrid()
        {
            if (IsOpenFindGrid(this.lastFocusedFindGrid))
            {
                return this.lastFocusedFindGrid;
            }

            foreach (Modern.Lab.WinForms.Controls.Data.ModernDataGrid grid in this.findShortcutGrids)
            {
                if (IsOpenFindGrid(grid))
                {
                    return grid;
                }
            }

            return null;
        }

        private static bool IsOpenFindGrid(Modern.Lab.WinForms.Controls.Data.ModernDataGrid grid)
        {
            return grid != null && !grid.IsDisposed && grid.Visible && grid.IsFindPanelOpen;
        }

        /// <summary>테마 설정 창(<c>ModernThemeDialogForm</c>)을 여는 단축키 — 기본 Ctrl+Shift+T, <see cref="Keys.None"/> 이면 없음(2026-10-23).
        /// 회사는 <c>ModernFormBase.Host.cs</c> 의 <c>AdaptToHost()</c> 에서 겹치지 않는 키로 바꾼다. 테마는 정적 값이라 저장 뒤 다음 실행부터 적용된다.</summary>
        public static Keys ThemeDialogShortcut { get; set; } = Keys.Control | Keys.Shift | Keys.T;

        // 테마 설정 창 연결(ModernFormBase.Theme.cs) — 그 파일을 함께 컴파일하지 않는 프로젝트(갤러리)에서는 빈 호출이다.
        partial void ThemeDialogProcessCmdKey(Keys keyData, ref bool handled);

        partial void ThemeShortcutInstall();

        // 규칙 안내 창 연결(ModernFormBase.RuleGuide.cs) — 그 파일을 함께 컴파일하지 않는 프로젝트(갤러리)에서는 빈 호출이다.
        partial void RuleGuideProcessCmdKey(Keys keyData, ref bool handled);

        partial void RuleGuideOnLoad();

        partial void RuleGuideRememberControl(Control control);

        partial void RuleGuideForgetControl(Control control);

        partial void RuleGuideDispose();

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            bool handled = false;
            this.RuleGuideProcessCmdKey(keyData, ref handled);

            if (handled)
            {
                return true;
            }

            this.ThemeDialogProcessCmdKey(keyData, ref handled);

            if (handled)
            {
                return true;
            }

            if (keyData == (Keys.Control | Keys.F))
            {
                Modern.Lab.WinForms.Controls.Data.ModernDataGrid target =
                        this.ResolveFindShortcutGrid();

                if (target != null)
                {
                    target.ShowFindPanel();
                    return true;
                }
            }

            if (keyData == Keys.F3 || keyData == (Keys.Shift | Keys.F3))
            {
                Modern.Lab.WinForms.Controls.Data.ModernDataGrid open = this.ResolveOpenFindGrid();

                if (open != null)
                {
                    if (keyData == Keys.F3)
                    {
                        open.FindNext();
                    }
                    else
                    {
                        open.FindPrevious();
                    }

                    return true;
                }
            }

            if (this.TryRefreshShortcut(keyData))
            {
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private Modern.Lab.WinForms.Controls.Input.ModernButton refreshShortcutButton;

        /// <summary>
        /// 조회 단축키를 등록한다(2026-10-21 점검) — <b>F5</b>는 어디서나, <b>Enter</b>는 조회 조건 칸(한 줄 텍스트·숫자·날짜·월·시각
        /// 입력)에 포커스가 있을 때 등록한 Refresh 버튼을 누른 것과 같다. 그리드·콤보·여러 줄 텍스트·버튼에서의 Enter 는 그 컨트롤의
        /// 몫이라 건드리지 않는다. 버튼이 꺼져 있으면(조회 중·쓰기 중) 아무 일도 없다. 화면 생성자에서 한 줄:
        /// <c>this.RegisterRefreshShortcut(this.btnRefresh);</c> 기준정보 화면은 베이스가 등록한다.
        /// </summary>
        protected void RegisterRefreshShortcut(Modern.Lab.WinForms.Controls.Input.ModernButton refresh)
        {
            this.refreshShortcutButton = refresh;
        }

        private bool TryRefreshShortcut(Keys keyData)
        {
            Modern.Lab.WinForms.Controls.Input.ModernButton refresh = this.refreshShortcutButton;

            if (refresh == null || refresh.IsDisposed || !refresh.Enabled || !refresh.Visible)
            {
                return false;
            }

            if (keyData == Keys.F5 || (keyData == Keys.Enter && IsCriteriaInput(DeepActiveControl(this))))
            {
                refresh.PerformClick();
                return true;
            }

            return false;
        }

        // 포커스가 든 가장 안쪽 컨트롤 — ContainerControl 의 ActiveControl 을 끝까지 따라간다.
        private static Control DeepActiveControl(ContainerControl container)
        {
            Control active = container == null ? null : container.ActiveControl;

            while (active is ContainerControl && ((ContainerControl)active).ActiveControl != null)
            {
                active = ((ContainerControl)active).ActiveControl;
            }

            return active;
        }

        // Enter 가 조회로 이어지는 칸 — 한 줄 텍스트·숫자·날짜·월·시각 입력. 여러 줄 텍스트는 줄바꿈, 콤보·그리드·버튼은 자기 처리.
        private static bool IsCriteriaInput(Control control)
        {
            Modern.Lab.WinForms.Controls.Input.ModernTextBox textBox = control as Modern.Lab.WinForms.Controls.Input.ModernTextBox;

            if (textBox != null)
            {
                return !textBox.Multiline;
            }

            return control is Modern.Lab.WinForms.Controls.Input.ModernNumericTextBox
                    || control is Modern.Lab.WinForms.Controls.Input.ModernDatePicker
                    || control is Modern.Lab.WinForms.Controls.Input.ModernDateRangePicker
                    || control is Modern.Lab.WinForms.Controls.Input.ModernMonthPicker
                    || control is Modern.Lab.WinForms.Controls.Input.ModernTimePicker;
        }

        private bool deferredResizeActive;

        [Category("Modern")]
        [Description("Defer child layout while dragging the window border and apply once on release (recommended for screens with many WPF islands)")]
        [DefaultValue(false)]
        public bool DeferredResize { get; set; }

        protected override void OnResizeBegin(EventArgs e)
        {
            base.OnResizeBegin(e);

            if (this.DeferredResize && !this.deferredResizeActive)
            {
                this.deferredResizeActive = true;
                this.SuspendLayout();
            }
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            if (this.deferredResizeActive)
            {
                this.deferredResizeActive = false;
                this.ResumeLayout(true);
            }

            base.OnResizeEnd(e);
        }

        private bool formInitialized;

        private void EnsureFormInitialized()
        {
            if (this.formInitialized)
            {
                return;
            }

            if (IsDesignTime())
            {
                return;
            }

            this.formInitialized = true;
            this.InitializeMessaging();
            this.ApplyThemeIfEnabled();
            this.DeclareColumns();
        }

        /// <summary>
        /// 화면의 <c>.Grid.cs</c>가 재정의해 그리드·트리·정보 카드에 컬럼 함수를 선언하는 자리(2026-10-19) —
        /// <c>this.gridLots.ConfigureColumns(LotColumns)</c> · <c>LotFields(null).Apply(this.fieldInfo)</c>. 폼이 열릴 때
        /// (<c>InitializeModernForm</c> 또는 핸들 생성) 한 번 불리고 디자인 타임에는 불리지 않는다. 선언한 컨트롤은 조회 전에도
        /// 헤더가 보이고, 그리드·트리는 <c>DataSource</c>가 올 때 같은 함수로 컬럼을 맞추므로 폼은 <c>DataSource = 표</c>만 대입한다.
        /// 재정의하지 않으면 아무 일도 없다 — 기존 화면은 그대로다.
        /// </summary>
        protected virtual void DeclareColumns()
        {
        }

        protected static bool IsDesignTime()
        {
            return System.ComponentModel.LicenseManager.UsageMode
                    == System.ComponentModel.LicenseUsageMode.Designtime;
        }

        protected virtual bool ApplyModernTheme
        {
            get { return true; }
        }

        private void ApplyThemeIfEnabled()
        {
            if (IsDesignTime() || this.DesignMode || !this.ApplyModernTheme)
            {
                return;
            }

            this.ApplyTheme();
        }

        protected virtual void ApplyTheme()
        {
            ModernThemeWinForms.Apply(this);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (IsDesignTime() || this.DesignMode)
            {
                return;
            }

            this.EnsureFormInitialized();
        }

        protected override void OnLoad(EventArgs e)
        {
            this.ApplyThemeIfEnabled();

            if (!IsDesignTime() && !this.DesignMode)
            {
                this.RuleGuideOnLoad();
                this.FitToWorkingAreaIfNeeded();
            }

            base.OnLoad(e);
        }

        /// <summary>
        /// 최상위 창으로 열릴 때 작업 영역보다 크면 맞출지(기본 true, 2026-10-21). 1366×768 이나 150% 배율(유효 1280×720)에서
        /// 디자이너 크기 그대로 열려 아래 버튼이 화면 밖으로 나가던 것 — 창을 작업 영역 안으로 줄이고, 디자이너의
        /// <c>MinimumSize</c>가 작업 영역보다 크면 그만큼 낮추되 <c>AutoScrollMinSize</c>로 남겨 내용이 잘리지 않고 스크롤된다.
        /// 셸 안에 자식 폼(<c>TopLevel = false</c>)으로 들어간 화면은 셸이 크기를 정하므로 건드리지 않는다 — 2026-10-30 에 칸에 맞춰
        /// 최소 크기 · 스크롤 · 크기를 바꾸는 코드를 넣었다가 회사 셸(화면을 최대화해 띄움)에서 폼이 최소 크기에 굳고 최대화가 풀려
        /// 같은 날 뺐다. 125% 에서 셸 칸보다 커진 화면의 처리는 셸 몫이다.
        /// </summary>
        protected virtual bool FitToWorkingArea
        {
            get { return true; }
        }

        private void FitToWorkingAreaIfNeeded()
        {
            if (!this.TopLevel || !this.FitToWorkingArea || this.WindowState != FormWindowState.Normal)
            {
                return;
            }

            System.Drawing.Rectangle area = Screen.FromControl(this).WorkingArea;
            System.Drawing.Size limit = new System.Drawing.Size(Math.Max(320, area.Width - 16), Math.Max(240, area.Height - 16));
            System.Drawing.Size minimum = this.MinimumSize;
            bool changed = false;

            if (minimum.Width > limit.Width || minimum.Height > limit.Height)
            {
                this.MinimumSize = new System.Drawing.Size(Math.Min(minimum.Width, limit.Width), Math.Min(minimum.Height, limit.Height));
                this.AutoScroll = true;
                this.AutoScrollMinSize = new System.Drawing.Size(
                        minimum.Width - (this.Width - this.ClientSize.Width),
                        minimum.Height - (this.Height - this.ClientSize.Height));
                changed = true;
            }

            if (this.Width > limit.Width || this.Height > limit.Height)
            {
                this.Size = new System.Drawing.Size(Math.Min(this.Width, limit.Width), Math.Min(this.Height, limit.Height));
                changed = true;
            }

            if (!changed)
            {
                return;
            }

            System.Drawing.Point location = this.Location;
            location.X = Math.Max(area.Left, Math.Min(location.X, area.Right - this.Width));
            location.Y = Math.Max(area.Top, Math.Min(location.Y, area.Bottom - this.Height));
            this.Location = location;
        }

        private bool dpiScaleSettled;

        private bool dpiFontReset;

        private bool dpiFontPassRunning;

        private System.Drawing.SizeF dpiDesignedDimensions;

        private System.Drawing.SizeF dpiExpectedDimensions;

        private System.Drawing.SizeF dpiAlignedFactor;

        /// <summary>
        /// DPI 인식 exe 에서 배율(125% · 150% …)이면 디자이너 배치를 100% 모습의 배율 그대로 키울지(기본 true, 2026-10-30).
        /// WinForms 의 글꼴 기준 자동 크기 조정은 디자이너가 <c>AutoScaleMode</c> 뒤에 <c>Font</c> 를 적으면 그 대입이 컨트롤의
        /// 크기 조정 예정을 배율 1 로 소모해 배치를 전혀 키우지 않고(글자만 커져 라벨이 잘리고 버튼이 꽉 찼다), 글꼴을 안 적은
        /// 폼도 평균 글자 폭이 정수로 반올림돼 가로는 덜 · 세로는 더 커진다. 보정은 <c>InitializeComponent</c> 끝의 자동 크기
        /// 조정 안에서 한 번 끝나므로, 생성자가 그 뒤에 만드는 컨트롤(지금처럼 <c>DeviceDpi</c> 를 곱해 둔 것)은 건드리지 않는다.
        /// SplitContainer 의 분할 위치 · 패널 최소 크기도 디자이너 배치라 함께 키운다 — WinForms 자동 크기 조정은 이 둘을
        /// 키우지 않는다. 100% 와 DPI 비인식 exe 에서는 아무것도 바뀌지 않는다. 화면 하나가 이 보정과 맞지 않으면 그 폼에서
        /// false 로 재정의해 예전 동작으로 되돌린다. 베이스 책임인 까닭: 폼 생성 흐름(디자이너 배치의 자동 크기 조정)을 고치는
        /// 일이라 화면마다 둘 수 없다. 배율은 <c>Control.DeviceDpi</c> 가 아니라 <c>ScreenDpi.Of</c> 로 읽는다 — DPI 매니페스트 없이
        /// 시작한 셸은 모던 컨트롤의 WPF 가 처음 로드될 때 DPI 인식으로 바뀌는데, WinForms 가 외운 <c>DeviceDpi</c> 는 96 에 머물러
        /// 보정이 한 번도 돌지 않았다(2026-10-30 회사 125%).
        /// </summary>
        protected virtual bool ExactDpiScaling
        {
            get { return true; }
        }

        protected override void SetClientSizeCore(int x, int y)
        {
            base.SetClientSizeCore(x, y);
            this.AlignAutoScaleToDpi();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            bool pending = !this.dpiScaleSettled && this.AutoScaleMode == AutoScaleMode.Font;
            this.dpiFontPassRunning = pending;
            try
            {
                base.OnFontChanged(e);
            }
            finally
            {
                this.dpiFontPassRunning = false;
            }

            if (pending)
            {
                // 글꼴 대입이 기준을 현재 글꼴로 바꾸고 잎 컨트롤의 크기 조정 예정을 소모했다 — 100% 에서도 배치는 그대로다.
                this.dpiFontReset = true;
                this.dpiExpectedDimensions = this.AutoScaleDimensions;
            }
        }

        protected override void ScaleControl(System.Drawing.SizeF factor, BoundsSpecified specified)
        {
            bool settling = !this.dpiScaleSettled && !this.dpiFontPassRunning && this.AutoScaleMode == AutoScaleMode.Font;
            if (!settling)
            {
                base.ScaleControl(factor, specified);
                return;
            }

            // InitializeComponent 끝(ResumeLayout)의 자동 크기 조정 — 폼 자신이 먼저 불리고 자식은 그 뒤다.
            this.dpiScaleSettled = true;
            if (!this.DpiScalingWanted())
            {
                base.ScaleControl(factor, specified);
                return;
            }

            float scale = Modern.Lab.WinForms.Controls.Hosting.ScreenDpi.Of(this) / 96f;
            if (this.dpiFontReset)
            {
                if (!SameScale(factor, new System.Drawing.SizeF(1f, 1f)))
                {
                    base.ScaleControl(factor, specified);
                    return;
                }

                // 프레임워크가 키울 것이 남지 않았으므로 폼과 자식 전체를 배율 그대로 한 번 키운다.
                System.Drawing.SizeF exact = new System.Drawing.SizeF(scale, scale);
                base.ScaleControl(exact, specified);
                foreach (Control child in this.Controls)
                {
                    child.Scale(exact);
                }

                RestoreNestedAutoScale(this, scale, exact);
                ScaleSplitterLayout(this, exact);
                return;
            }

            base.ScaleControl(factor, specified);
            if (SameScale(factor, this.dpiAlignedFactor))
            {
                RestoreNestedAutoScale(this, scale, new System.Drawing.SizeF(1f, 1f));
                ScaleSplitterLayout(this, factor);
            }
        }

        private bool DpiScalingWanted()
        {
            return Modern.Lab.WinForms.Controls.Hosting.ScreenDpi.Of(this) != 96
                    && this.ExactDpiScaling && !IsDesignTime() && !this.DesignMode;
        }

        // 글꼴을 안 적은 폼 — 기준(AutoScaleDimensions)을 맞춰 두면 프레임워크가 처음부터 배율 그대로 키운다. 컨테이너는
        // 자기 ResumeLayout 때 먼저 키워지므로(OnChildLayoutResuming) 키운 뒤에 고치는 방식은 이중으로 커진다.
        private void AlignAutoScaleToDpi()
        {
            if (this.dpiScaleSettled || this.AutoScaleMode != AutoScaleMode.Font || !this.DpiScalingWanted())
            {
                return;
            }

            System.Drawing.SizeF now = this.AutoScaleDimensions;
            if (now.Width <= 0f || now.Height <= 0f)
            {
                return;
            }

            if (!SameScale(now, this.dpiExpectedDimensions))
            {
                this.dpiDesignedDimensions = now;
                this.dpiFontReset = false;
            }

            if (this.dpiFontReset)
            {
                return;
            }

            float scale = Modern.Lab.WinForms.Controls.Hosting.ScreenDpi.Of(this) / 96f;
            System.Drawing.SizeF at96 = FontAutoScaleDimensionsAt96(this.Font);
            System.Drawing.SizeF target = new System.Drawing.SizeF(
                    scale * at96.Width / this.dpiDesignedDimensions.Width,
                    scale * at96.Height / this.dpiDesignedDimensions.Height);
            System.Drawing.SizeF current = this.CurrentAutoScaleDimensions;
            System.Drawing.SizeF aligned = new System.Drawing.SizeF(current.Width / target.Width, current.Height / target.Height);
            if (!SameScale(aligned, now))
            {
                this.AutoScaleDimensions = aligned;
            }

            this.dpiExpectedDimensions = this.AutoScaleDimensions;
            this.dpiAlignedFactor = target;
        }

        // 자기 AutoScaleMode 로 이미 크기를 맞춘 안쪽 컨테이너(사용자 컨트롤)의 내용을 그 컨테이너 글꼴 기준의 배율로 맞춘다.
        // applied 는 지금까지 그 내용에 곱해진 배율이다.
        private static void RestoreNestedAutoScale(Control parent, float scale, System.Drawing.SizeF applied)
        {
            foreach (Control child in parent.Controls)
            {
                System.Drawing.SizeF next = applied;
                ContainerControl container = child as ContainerControl;
                if (container != null && container.AutoScaleMode != AutoScaleMode.Inherit)
                {
                    next = container.AutoScaleMode == AutoScaleMode.Font
                            ? NestedFontCorrection(container, scale)
                            : new System.Drawing.SizeF(1f, 1f);
                    System.Drawing.SizeF extra = new System.Drawing.SizeF(next.Width / applied.Width, next.Height / applied.Height);
                    if (!SameScale(extra, new System.Drawing.SizeF(1f, 1f)))
                    {
                        foreach (Control inner in container.Controls)
                        {
                            inner.Scale(extra);
                        }
                    }
                }

                RestoreNestedAutoScale(child, scale, next);
            }
        }

        private static System.Drawing.SizeF NestedFontCorrection(ContainerControl container, float scale)
        {
            System.Drawing.SizeF current = container.CurrentAutoScaleDimensions;
            if (current.Width <= 0f || current.Height <= 0f)
            {
                return new System.Drawing.SizeF(1f, 1f);
            }

            System.Drawing.SizeF at96 = FontAutoScaleDimensionsAt96(container.Font);
            return new System.Drawing.SizeF(scale * at96.Width / current.Width, scale * at96.Height / current.Height);
        }

        // 그 글꼴의 96 DPI 자동 크기 기준 — 같은 크기의 픽셀 글꼴로 프레임워크 계산을 그대로 빌린다.
        private static System.Drawing.SizeF FontAutoScaleDimensionsAt96(System.Drawing.Font font)
        {
            using (System.Drawing.Font pixelFont = new System.Drawing.Font(
                    font.FontFamily, font.SizeInPoints * 96f / 72f, font.Style, System.Drawing.GraphicsUnit.Pixel,
                    font.GdiCharSet, font.GdiVerticalFont))
            using (ContainerControl probe = new ContainerControl())
            {
                probe.SuspendLayout();
                probe.AutoScaleMode = AutoScaleMode.Font;
                probe.Font = pixelFont;
                System.Drawing.SizeF dimensions = probe.CurrentAutoScaleDimensions;
                probe.ResumeLayout(false);
                return dimensions;
            }
        }

        private static void ScaleSplitterLayout(Control parent, System.Drawing.SizeF factor)
        {
            foreach (Control child in parent.Controls)
            {
                SplitContainer split = child as SplitContainer;
                ContainerControl container = child as ContainerControl;
                if (split != null)
                {
                    ScaleSplitter(split, split.Orientation == Orientation.Vertical ? factor.Width : factor.Height);
                }
                else if (container != null && container.AutoScaleMode != AutoScaleMode.Inherit)
                {
                    continue;
                }

                ScaleSplitterLayout(child, factor);
            }
        }

        private static void ScaleSplitter(SplitContainer split, float factor)
        {
            // KeepRatio 분할은 크기가 바뀔 때 스스로 비율대로 위치를 옮겼다 — 위치에 배율을 또 곱하면 비율이 틀어진다.
            Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer modern =
                    split as Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer;
            bool keepsRatio = modern != null && modern.KeepRatio;
            int panel1 = (int)Math.Round(split.Panel1MinSize * factor);
            int panel2 = (int)Math.Round(split.Panel2MinSize * factor);
            int distance = (int)Math.Round(split.SplitterDistance * factor);
            try
            {
                split.Panel1MinSize = panel1;
                split.Panel2MinSize = panel2;
                if (!keepsRatio)
                {
                    split.SplitterDistance = distance;
                }
            }
            catch (InvalidOperationException)
            {
                // 공간이 모자라면 SplitContainer 가 맞춘 위치를 그대로 둔다.
            }
            catch (ArgumentOutOfRangeException)
            {
                // 최소 크기 제약에 걸리면 SplitContainer 가 맞춘 위치를 그대로 둔다.
            }
        }

        private static bool SameScale(System.Drawing.SizeF first, System.Drawing.SizeF second)
        {
            return Math.Abs(first.Width - second.Width) < 0.0005f && Math.Abs(first.Height - second.Height) < 0.0005f;
        }

        protected virtual void InitializeMessaging()
        {
        }

        protected virtual void DisposeMessaging()
        {
        }

        protected void ShowToast(string message)
        {
            this.ShowToast(message, Modern.Lab.Controls.Wpf.Display.ToastKind.Info);
        }

        protected void ShowToast(string message, Modern.Lab.Controls.Wpf.Display.ToastKind kind)
        {
            if (kind == Modern.Lab.Controls.Wpf.Display.ToastKind.Error)
            {
                this.ShowErrorMessage(this.NoticeErrorCaption, message);
                return;
            }

            if (kind == Modern.Lab.Controls.Wpf.Display.ToastKind.Warning)
            {
                this.ShowMessage(
                        Modern.Lab.Controls.Wpf.Display.ModernMessageKind.Warning,
                        this.NoticeWarningCaption,
                        message);
                return;
            }

            this.ShowTransientNotice(message, kind);
        }

        protected virtual void ShowStickyNotice(
                string message, Modern.Lab.Controls.Wpf.Display.ToastKind kind)
        {
            this.ShowNotice(message, kind, true);
        }

        protected virtual void ShowTransientNotice(
                string message, Modern.Lab.Controls.Wpf.Display.ToastKind kind)
        {
            if (this.stickyNoticeShowing && this.sharedToast != null && this.sharedToast.Visible
                    && (this.staleChannels.Count > 0 || this.contractNoticeText.Length > 0))
            {
                return;
            }

            this.ShowNotice(message, kind, false);
        }

        protected virtual void HideNotice()
        {
            this.stickyNoticeShowing = false;

            if (this.sharedToast != null)
            {
                this.sharedToast.HideToast();
            }
        }

        protected void SetContractNotice(string text)
        {
            this.contractNoticeText = text ?? string.Empty;
            this.RenderStickyNotice();
        }

        private string NoticeBasis()
        {
            List<string> stale = new List<string>(this.silentFailureChannels);
            stale.Sort(StringComparer.Ordinal);

            return string.Join(",", stale.ToArray())
                    + "|" + this.contractNoticeText;
        }

        private void RenderStickyNotice()
        {
            string basis = this.NoticeBasis();

            if (basis == this.renderedNoticeBasis)
            {
                return;
            }

            this.renderedNoticeBasis = basis;

            if (this.silentFailureChannels.Count > 0)
            {
                this.ShowStickyNotice(
                        this.BuildStaleNoticeText(this.StaleLeadText(this.silentFailureChannels.Count), this.silentFailureChannels)
                                + " " + this.RetryingText,
                        Modern.Lab.Controls.Wpf.Display.ToastKind.Error);
                return;
            }

            if (this.contractNoticeText.Length > 0)
            {
                this.ShowStickyNotice(
                        this.contractNoticeText,
                        Modern.Lab.Controls.Wpf.Display.ToastKind.Warning);
                return;
            }

            this.HideNotice();
        }

        protected virtual string StaleLeadText(int staleCount)
        {
            return staleCount == 1
                    ? "1 data set not refreshed"
                    : staleCount.ToString(System.Globalization.CultureInfo.InvariantCulture)
                            + " data sets not refreshed";
        }

        protected virtual string NoticeErrorCaption
        {
            get { return "Error"; }
        }

        protected virtual string NoticeWarningCaption
        {
            get { return "Warning"; }
        }

        protected virtual string QueryFailedCaption
        {
            get { return "Lookup failed"; }
        }

        /// <summary>화면 코드가 낸 예외(널 참조 · 잘못된 호출)의 창 제목 — 서버 탓이 아님을 제목에서 가른다.</summary>
        protected virtual string ScreenErrorCaption
        {
            get { return "Screen error"; }
        }

        /// <summary>화면 내부 오류의 Why 줄. 조치는 <see cref="SupportNextText"/>, 예외 원문은 상세로 간다.</summary>
        protected virtual string ScreenErrorText
        {
            get { return "This screen ran into an internal error."; }
        }

        /// <summary>응답이 오지 않은 통신 실패의 Why 줄(시간 초과는 그 예외의 문구를 쓴다).</summary>
        protected virtual string ServerUnreachableText
        {
            get { return "The server could not be reached."; }
        }

        /// <summary>오류 창 본문의 첫 줄 라벨 — 무엇이 잘못됐는가.</summary>
        protected virtual string WhyLabel
        {
            get { return "Why:"; }
        }

        /// <summary>오류 창 본문의 둘째 줄 라벨 — 현업이 다음에 할 일.</summary>
        protected virtual string NextLabel
        {
            get { return "Next:"; }
        }

        /// <summary>Next 줄 — 화면 내부 오류 · 인자 오류 · 안 잡힌 예외처럼 현업이 고칠 수 없는 실패.</summary>
        protected virtual string SupportNextText
        {
            get { return "Copy the details and send them to support."; }
        }

        /// <summary>Next 줄 — 조회 중 서버에 닿지 못했을 때.</summary>
        protected virtual string QueryUnreachableNextText
        {
            get { return "Check the connection, then refresh."; }
        }

        /// <summary>Next 줄 — 조회가 시간 안에 돌아오지 않았을 때.</summary>
        protected virtual string QueryTimeoutNextText
        {
            get { return "Wait a moment, then refresh."; }
        }

        /// <summary>Next 줄 — 조회를 서버가 코드로 거절했을 때.</summary>
        protected virtual string QueryRejectedNextText
        {
            get { return "Fix the cause above, then refresh."; }
        }

        /// <summary>Next 줄 — 실행을 서버가 코드로 거절했을 때.</summary>
        protected virtual string ActionRejectedNextText
        {
            get { return "Fix the cause above, then run the action again."; }
        }

        /// <summary>Why 줄 — 실행 중 응답이 오지 않았을 때. 서버에서는 처리됐을 수 있어 조치(<see cref="ActionLostNextText"/>)가 조회와 다르다.</summary>
        protected virtual string ActionLostText
        {
            get { return "The server did not reply."; }
        }

        /// <summary>Next 줄 — 실행 중 응답이 오지 않았을 때.</summary>
        protected virtual string ActionLostNextText
        {
            get { return "Refresh to see whether it went through, then run it again if needed."; }
        }

        /// <summary>
        /// 오류 창 본문의 틀 — "Why:" 줄과 "Next:" 줄. 라벨은 다이얼로그의 강조 표기(<c>**</c>)로 액센트 굵게 그려진다.
        /// 사유와 조치를 한 문단에 이어 쓰면 현업이 무엇을 하라는지 놓친다(2026-10-28 사용자).
        /// </summary>
        /// <param name="why">무엇이 잘못됐는가 — 한 문장.</param>
        /// <param name="next">다음에 할 일 — 한 문장.</param>
        /// <returns>두 줄 본문.</returns>
        protected string WhyNextText(string why, string next)
        {
            string marker = Modern.Lab.Controls.Wpf.Display.ModernMessageDialogControl.EmphasisMarker;

            return marker + this.WhyLabel + marker + " " + (why ?? string.Empty)
                    + Environment.NewLine
                    + marker + this.NextLabel + marker + " " + (next ?? string.Empty);
        }

        protected virtual string RetryingText
        {
            get { return "Retrying automatically…"; }
        }

        /// <summary>로그인 시설. 회사는 로그인 정보로 재정의하며 기본값은 홈 예제 전용이다.</summary>
        protected virtual string CurrentFacId
        {
            get { return "FAC-DEMO-01"; }
        }

        /// <summary>
        /// 전문의 <c>USER_ID</c> 에 싣는 값 — 회사는 이 속성 하나를 로그인 사번으로 재정의한다.
        /// 기본값은 홈 실행용이다. 값을 화면마다 적지 않는 이유는 회사 적용에서 고칠 자리를
        /// 하나로 두기 위해서다(2026-09-18).
        /// </summary>
        protected virtual string CurrentUserId
        {
            get { return "operator"; }
        }

        private void ShowNotice(
                string message, Modern.Lab.Controls.Wpf.Display.ToastKind kind, bool sticky)
        {
            if (this.IsDisposed || this.Disposing)
            {
                return;
            }

            if (this.sharedToast == null)
            {
                this.sharedToast = new Modern.Lab.WinForms.Controls.Display.ModernToast();
                this.Controls.Add(this.sharedToast);
            }

            this.stickyNoticeShowing = sticky;
            this.sharedToast.BringToFront();
            this.sharedToast.Show(message, kind, sticky);
        }

        private int queryDepth;

        private bool actionInProgress;

        protected bool QueryInProgress
        {
            get { return Thread.VolatileRead(ref this.queryDepth) > 0; }
        }

        private void BeginQuery()
        {
            Interlocked.Increment(ref this.queryDepth);
        }

        private void EndQuery()
        {
            if (Interlocked.Decrement(ref this.queryDepth) < 0)
            {
                Interlocked.Exchange(ref this.queryDepth, 0);
            }
        }

        protected bool ActionInProgress
        {
            get { return this.actionInProgress; }
        }

        private void MarkActionInProgress(bool running)
        {
            this.actionInProgress = running;

            if (!this.IsDisposed && !this.Disposing)
            {
                this.OnActionStateChanged(running);
            }
        }

        /// <summary>
        /// 쓰기 액션(<c>RunAction</c>·<c>RunActionBatch</c>)이 시작되거나 끝난 직후 UI 스레드에서 불린다.
        /// <see cref="ActionInProgress"/>는 이미 새 값이다. 끝남은 성공·실패·예외 모두에서 결과 처리보다 먼저 온다.
        /// 기본은 아무것도 하지 않는다 — 쓰는 동안 입력을 잠가야 하는 화면이 재정의한다.
        /// </summary>
        protected virtual void OnActionStateChanged(bool running)
        {
        }

        private void NotifyActionCommunicationFailure()
        {
            this.NotifyLoadFailure(null);
        }

        private void NotifyActionCommunicationSuccess()
        {
            this.SettleNoticeAfterServerContact(false);
        }

        protected virtual bool CanStartAction()
        {
            return !this.actionInProgress && !this.QueryInProgress;
        }

        /// <summary>
        /// 버튼·메뉴가 왜 꺼졌는지 마우스를 올리면 보이게 한다 — <see cref="ActionRule"/> 의
        /// 사유를 그대로 넘긴다. <paramref name="reason"/> 이 <c>null</c> 이면 열려 있다는 뜻이라
        /// 툴팁을 지우고 컨트롤을 켠다.
        ///
        /// 조건을 화면 코드에서 눈으로 읽어 기억하지 않아도 되게 하려는 것이다 — 회사에서 고치는
        /// 사람이 조건을 모두 들고 있을 수 없다(2026-09-20).
        ///
        /// 여기를 지난 버튼의 글자는 규칙 안내 창의 액션 이름이 되고, 열려 있는 안내 창은 다시 채워진다
        /// (<c>ModernFormBase.RuleGuide.cs</c>). 툴팁은 테마 툴팁(<c>ModernToolTip</c>)이다(2026-10-22).
        /// </summary>
        protected void ApplyActionReason(Control control, string reason)
        {
            this.ApplyActionReason(control, reason, null);
        }

        /// <summary>
        /// 호버 카드 판 — 글자 한 줄로 모자란 컨트롤(자식 장비 칩)은 <paramref name="card"/> 로 제목 · 상태 줄을 함께 보인다.
        /// 꺼진 사유는 카드 맨 아래 안내 줄에 들어가고, 열려 있으면(<paramref name="reason"/> null) 안내 줄 없이 카드만 뜬다.
        /// <paramref name="card"/> 가 null 이면 글자 툴팁과 같다.
        /// </summary>
        protected void ApplyActionReason(Control control, string reason, Modern.Lab.WinForms.Controls.Display.ModernToolTipCard card)
        {
            if (control == null)
            {
                return;
            }

            control.Enabled = reason == null;

            if (this.actionReasonTip == null)
            {
                this.actionReasonTip = new Modern.Lab.WinForms.Controls.Display.ModernToolTip();
                this.actionReasonTip.AutoPopDelay = 10000;
                this.actionReasonTip.InitialDelay = 400;
                this.actionReasonTip.ReshowDelay = 100;
            }

            if (card != null)
            {
                card.Note = reason ?? string.Empty;
                this.actionReasonTip.SetCard(control, card);
            }
            else
            {
                this.actionReasonTip.SetToolTip(control, reason ?? string.Empty);
            }

            this.RuleGuideRememberControl(control);
        }

        /// <summary>
        /// 동적으로 만든 액션 컨트롤(자식 장비 칩 등)을 버리기 전에 부른다 — 사유 툴팁과 규칙 안내 창의 등록을 거둬
        /// 다시 만들 때마다 등록이 쌓이지 않게 한다(2026-10-22).
        /// </summary>
        protected void ClearActionReason(Control control)
        {
            if (control == null)
            {
                return;
            }

            if (this.actionReasonTip != null)
            {
                this.actionReasonTip.SetToolTip(control, string.Empty);
            }

            this.RuleGuideForgetControl(control);
        }

        /// <summary>메뉴 항목 판 — <see cref="ToolStripItem"/> 은 자기 툴팁을 갖는다.</summary>
        protected static void ApplyActionReason(ToolStripItem item, string reason)
        {
            if (item == null)
            {
                return;
            }

            item.Enabled = reason == null;
            item.ToolTipText = reason ?? string.Empty;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (this.actionInProgress && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.ShowToast(
                        this.ActionRunningText,
                        Modern.Lab.Controls.Wpf.Display.ToastKind.Warning);
                return;
            }

            base.OnFormClosing(e);
        }

        protected virtual string ActionRunningText
        {
            get { return "Another action is still running."; }
        }

        private int busyDepth;

        private int busyGeneration;

        protected IDisposable Busy(string message)
        {
            return this.Busy(message, string.Empty, null);
        }

        protected IDisposable Busy(string message, string subMessage)
        {
            return this.Busy(message, subMessage, null);
        }

        protected IDisposable Busy(string message, string subMessage, Control region)
        {
            this.busyDepth = this.busyDepth + 1;
            this.ShowBusy(message, subMessage, region);

            return new BusyScope(this, this.busyGeneration);
        }

        protected void ResetBusy()
        {
            this.busyGeneration = this.busyGeneration + 1;
            this.busyDepth = 0;
            this.HideBusy();
        }

        private void ReleaseBusy(int generation)
        {
            if (generation != this.busyGeneration)
            {
                return;
            }

            this.busyDepth = this.busyDepth - 1;

            if (this.busyDepth <= 0)
            {
                this.busyDepth = 0;
                this.HideBusy();
            }
        }

        private sealed class BusyScope : IDisposable
        {
            private readonly int generation;
            private ModernFormBase owner;

            internal BusyScope(ModernFormBase owner, int generation)
            {
                this.owner = owner;
                this.generation = generation;
            }

            public void Dispose()
            {
                if (this.owner == null)
                {
                    return;
                }

                ModernFormBase target = this.owner;
                this.owner = null;
                target.ReleaseBusy(this.generation);
            }
        }

        private void ShowBusy(string message)
        {
            this.ShowBusy(message, string.Empty, null);
        }

        private void ShowBusy(string message, string subMessage)
        {
            this.ShowBusy(message, subMessage, null);
        }

        private void ShowBusy(string message, string subMessage, Control region)
        {
            if (this.IsDisposed || this.Disposing)
            {
                return;
            }

            Control host = region ?? (Control)this;

            if (this.sharedBusyOverlay == null)
            {
                this.sharedBusyOverlay = new Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay();
            }

            if (!object.ReferenceEquals(this.sharedBusyOverlay.Parent, host))
            {
                if (this.sharedBusyOverlay.Parent != null)
                {
                    this.sharedBusyOverlay.Parent.Controls.Remove(this.sharedBusyOverlay);
                }

                host.Controls.Add(this.sharedBusyOverlay);
            }

            this.sharedBusyOverlay.Message = message;
            this.sharedBusyOverlay.SubMessage = subMessage ?? string.Empty;
            this.sharedBusyOverlay.BringToFront();
            this.sharedBusyOverlay.Busy = true;
        }

        private void HideBusy()
        {
            if (this.sharedBusyOverlay != null)
            {
                this.sharedBusyOverlay.Busy = false;
            }
        }

        protected virtual string ServerFailurePrefix
        {
            get { return "Server call failed: "; }
        }

        protected virtual string ServerFailureText(Exception failure)
        {
            return this.ServerFailurePrefix + (failure == null ? string.Empty : failure.Message);
        }

        protected virtual void ShowServerFailure(Exception failure)
        {
            if (this.LoadFailureSilent)
            {
                this.RenderStickyNotice();
                return;
            }

            this.RenderStickyNotice();
            this.pendingFailures.Add(failure);
            this.Debounce(failureDialogKey, 150, new MethodInvoker(this.FlushFailureDialog));
        }

        /// <summary>
        /// 실패 창의 Why 줄 — 무엇이 잘못됐는지 한 문장이다. 서버가 코드로 거절했으면 그 문구 그대로, 응답이 오지 않았으면
        /// 고정 문구(시간 초과는 그 예외의 문구), 그 밖의 예외(널 참조 · 잘못된 호출)는 화면 내부 오류 문구다.
        /// 조치는 <see cref="FailureNextStep"/>, 예외 원문 · 종류 · 스택은 <see cref="FailureDetails"/> 로 간다. 한국어 화면은 재정의한다.
        /// </summary>
        /// <param name="failure">실패 예외.</param>
        /// <returns>Why 한 문장.</returns>
        protected virtual string FailureSummary(Exception failure)
        {
            string replyKind = ServerReplyKind(failure);

            if (replyKind == "rejected")
            {
                return failure.Message;
            }

            if (this.IsLostReply(failure))
            {
                return this.ConnectionFailureText(replyKind == "lost" ? failure.InnerException : failure);
            }

            return this.ScreenErrorText;
        }

        // 시간 초과는 자기 문구가 이미 현업용이라 그대로("The server did not reply within 30 seconds."), 나머지 통신 실패는 고정 문구.
        private string ConnectionFailureText(Exception cause)
        {
            TimeoutException timeout = cause as TimeoutException;

            return timeout != null && timeout.Message.Length > 0
                    ? timeout.Message
                    : this.ServerUnreachableText;
        }

        /// <summary>
        /// 실패 창의 Next 줄 — 현업이 다음에 할 일 한 문장. 서버 거절은 "원인을 고치고 다시", 응답 없음은 조회면 "연결 확인 뒤 새로고침"
        /// (시간 초과는 "잠시 뒤 새로고침"), 실행이면 "새로고침으로 처리 여부 확인 뒤 필요하면 다시", 그 밖의 예외는 "상세를 복사해 지원팀에".
        /// </summary>
        /// <param name="failure">실패 예외.</param>
        /// <param name="afterAction">실행(RunAction) 뒤의 실패면 true, 조회 실패면 false.</param>
        /// <returns>Next 한 문장.</returns>
        protected virtual string FailureNextStep(Exception failure, bool afterAction)
        {
            string replyKind = ServerReplyKind(failure);

            if (replyKind == "rejected")
            {
                return afterAction ? this.ActionRejectedNextText : this.QueryRejectedNextText;
            }

            if (this.IsLostReply(failure))
            {
                if (afterAction)
                {
                    return this.ActionLostNextText;
                }

                Exception cause = replyKind == "lost" ? failure.InnerException : failure;

                return cause is TimeoutException ? this.QueryTimeoutNextText : this.QueryUnreachableNextText;
            }

            return this.SupportNextText;
        }

        // 응답이 오지 않은 통신 실패인지 — 읽기 실패 "lost" 가운데 원인이 통신 예외(또는 원인 없음)인 것, 또는 통신 계층 예외 자체.
        // "lost" 라도 원인이 응답 형식 오류 · 전송 코드 안의 화면 예외면 통신 실패가 아니다 — 조회도 실행처럼 원인을 본다(코덱스 검토 2026-10-29).
        private bool IsLostReply(Exception failure)
        {
            if (ServerReplyKind(failure) == "lost")
            {
                return failure.InnerException == null || this.IsTransportFailure(failure.InnerException);
            }

            return this.IsTransportFailure(failure);
        }

        /// <summary>
        /// 통신 계층이 낸 예외인지 — 예외와 그 안쪽 원인을 따라가며 시간 초과 · 웹 · 소켓 · IO · DB 예외를 찾는다. 화면 코드의 예외(널 참조 ·
        /// 인자 오류)와 응답 형식 오류는 여기 들지 않아 "Screen error" 로 간다. 회사 전송 계층이 자기 예외 타입을 던지면 재정의해 더한다.
        /// </summary>
        /// <param name="failure">실패 예외.</param>
        /// <returns>통신 실패면 true.</returns>
        protected virtual bool IsTransportFailure(Exception failure)
        {
            for (Exception current = failure; current != null; current = current.InnerException)
            {
                if (current is TimeoutException
                        || current is System.Net.WebException
                        || current is System.Net.Sockets.SocketException
                        || current is System.IO.IOException
                        || current is System.Data.Common.DbException)
                {
                    return true;
                }
            }

            return false;
        }

        // 응답 읽기 실패(ServerReplyException)가 Data["ServerReply"] 로 알리는 갈래 — "rejected"(서버 거절) · "lost"(응답 없음) · 빈 값(그 밖).
        // 타입으로 보지 않는 이유: 갤러리 exe 가 이 파일만 링크해 Messaging 폴더 없이 컴파일한다.
        private static string ServerReplyKind(Exception failure)
        {
            return failure == null ? string.Empty : (Convert.ToString(failure.Data["ServerReply"]) ?? string.Empty);
        }

        // 서버 · 통신 쪽 실패인지(제목 "Lookup failed" + 마지막 성공 시각 줄), 아니면 화면 내부 오류인지(제목 "Screen error").
        private bool IsServerSideFailure(Exception failure)
        {
            return ServerReplyKind(failure) == "rejected" || this.IsLostReply(failure);
        }

        /// <summary>
        /// 실패 창의 Details — 개발자가 받아 볼 원문이다. 어느 채널(<c>Data["Channel"]</c>)의 어느 전문(<c>Data["Request"]</c>)이
        /// 실패했는지, 서버 반환 코드, 그리고 예외 원문(종류 · 메시지 · 안쪽 원인 · 스택). 서버 거절은 스택이 뜻이 없어 문구만 적는다.
        /// </summary>
        /// <param name="failure">실패 예외.</param>
        /// <returns>상세 글자(없으면 빈 값).</returns>
        protected static string FailureDetails(Exception failure)
        {
            if (failure == null)
            {
                return string.Empty;
            }

            string channel = Convert.ToString(failure.Data["Channel"]) ?? string.Empty;
            string request = Convert.ToString(failure.Data["Request"]) ?? string.Empty;
            string code = Convert.ToString(failure.Data["Code"]) ?? string.Empty;
            List<string> lines = new List<string>();

            if (channel.Length > 0)
            {
                lines.Add("channel: " + channel);
            }

            if (request.Length > 0)
            {
                lines.Add("request: " + request);
            }

            if (code.Length > 0)
            {
                lines.Add("code: " + code);
            }

            if (ServerReplyKind(failure) == "rejected")
            {
                lines.Add("reply: " + failure.Message);
                return string.Join(Environment.NewLine, lines.ToArray());
            }

            string head = string.Join(Environment.NewLine, lines.ToArray());
            string trace = failure.ToString();

            return head.Length == 0 ? trace : head + Environment.NewLine + Environment.NewLine + trace;
        }

        private void FlushFailureDialog()
        {
            if (this.IsDisposed || this.Disposing || this.pendingFailures.Count == 0)
            {
                return;
            }

            if (this.QueryInProgress)
            {
                this.Debounce(failureDialogKey, 150, new MethodInvoker(this.FlushFailureDialog));
                return;
            }

            Exception first = this.pendingFailures[0];
            int count = this.pendingFailures.Count;
            List<string> detailBlocks = new List<string>();

            foreach (Exception failure in this.pendingFailures)
            {
                string detail = FailureDetails(failure);

                if (detail.Length > 0)
                {
                    detailBlocks.Add(detail);
                }
            }

            this.pendingFailures.Clear();

            string body = this.WhyNextText(this.FailureSummary(first), this.FailureNextStep(first, false));

            if (count > 1)
            {
                body = count.ToString(System.Globalization.CultureInfo.InvariantCulture)
                        + " queries failed." + Environment.NewLine + body;
            }

            // 마지막 성공 시각 안내는 서버 쪽 실패에만, 본문 아래 별도 줄로 — 사유와 한 문단에 이으면 읽기 어려웠다(2026-10-28).
            bool serverSide = this.IsServerSideFailure(first);

            this.ShowErrorMessage(
                    serverSide ? this.QueryFailedCaption : this.ScreenErrorCaption,
                    serverSide ? body + Environment.NewLine + Environment.NewLine + this.StaleBasisText() : body,
                    string.Join(Environment.NewLine + Environment.NewLine, detailBlocks.ToArray()));
        }

        protected bool LoadFailureSilent
        {
            get { return this.loadFailureSilent; }
        }

        protected virtual bool Confirm(string message, string caption)
        {
            return Modern.Lab.WinForms.Controls.Dialogs.ModernMessageDialog.Confirm(
                    this, caption, message);
        }

        internal static Action<Modern.Lab.Controls.Wpf.Display.ModernMessageKind, string, string>
                MessageInterceptor;

        protected virtual void ShowMessage(
                Modern.Lab.Controls.Wpf.Display.ModernMessageKind kind,
                string caption,
                string message)
        {
            Action<Modern.Lab.Controls.Wpf.Display.ModernMessageKind, string, string> interceptor =
                    MessageInterceptor;

            if (interceptor != null)
            {
                interceptor(kind, caption, message);
                return;
            }

            if (kind == Modern.Lab.Controls.Wpf.Display.ModernMessageKind.Error
                    && this.showingErrorDetails.Length > 0)
            {
                Modern.Lab.WinForms.Controls.Dialogs.ModernMessageDialog.ShowError(
                        this, caption, message, this.showingErrorDetails);
                return;
            }

            Modern.Lab.WinForms.Controls.Dialogs.ModernMessageDialog.Show(
                    this,
                    kind,
                    caption,
                    message,
                    MessageBoxButtons.OK);
        }

        /// <summary>사용자가 무언가 해야 하는 경고 — 토스트가 아니라 다이얼로그로 올린다.</summary>
        protected void ShowWarningMessage(string message)
        {
            this.ShowMessage(
                    Modern.Lab.Controls.Wpf.Display.ModernMessageKind.Warning,
                    this.NoticeWarningCaption,
                    message);
        }

        protected void ShowErrorMessage(string caption, string message)
        {
            this.ShowMessage(
                    Modern.Lab.Controls.Wpf.Display.ModernMessageKind.Error, caption, message);
        }

        /// <summary>
        /// 기술 상세를 접어 둔 오류 알림 — <paramref name="message"/>는 현업이 읽고
        /// 행동할 수 있는 안내로 쓰고, 서버 응답 원문·전문 이름·누락 컬럼처럼 개발자가
        /// 받아 볼 것은 <paramref name="details"/>로 넘긴다. 다이얼로그의 "Details"를
        /// 눌러야 보이고, "Copy"가 제목·본문·상세를 한 번에 담는다.
        ///
        /// 가로채기(자체 검사)가 걸려 있으면 본문만 전달한다 — 검사가 보는 것은
        /// "무엇을 알렸는가"이지 원문이 아니다.
        /// </summary>
        /// <param name="caption">제목.</param>
        /// <param name="message">현업이 읽을 안내.</param>
        /// <param name="details">개발자가 받을 원문 (빈 값이면 상세 버튼이 생기지 않는다).</param>
        protected virtual void ShowErrorMessage(string caption, string message, string details)
        {
            // 알림의 자리는 ShowMessage 하나다 — 재정의·가로채기가 거기만 보므로 상세는 필드로 건네 함께 띄운다.
            this.showingErrorDetails = details ?? string.Empty;

            try
            {
                this.ShowMessage(
                        Modern.Lab.Controls.Wpf.Display.ModernMessageKind.Error, caption, message);
            }
            finally
            {
                this.showingErrorDetails = string.Empty;
            }
        }

        /// <summary>
        /// Why · Next 두 줄 본문의 오류 알림 — 화면이 쓰는 틀. <paramref name="why"/>는 무엇이 잘못됐는지,
        /// <paramref name="next"/>는 현업이 다음에 할 일을 한 문장씩 적고, 원문은 <paramref name="details"/>로 넘긴다.
        /// </summary>
        /// <param name="caption">제목 — 두세 단어 명사형.</param>
        /// <param name="why">무엇이 잘못됐는가.</param>
        /// <param name="next">다음에 할 일.</param>
        /// <param name="details">개발자가 받을 원문 (빈 값이면 상세 버튼이 생기지 않는다).</param>
        protected void ShowErrorMessage(string caption, string why, string next, string details)
        {
            this.ShowErrorMessage(caption, this.WhyNextText(why, next), details);
        }

        protected void Debounce(string key, int delayMs, MethodInvoker action)
        {
            if (action == null || this.IsDisposed || this.Disposing)
            {
                return;
            }

            string name = key ?? string.Empty;
            System.Windows.Forms.Timer timer;

            if (!this.debounceTimers.TryGetValue(name, out timer))
            {
                timer = new System.Windows.Forms.Timer();
                timer.Tick += this.OnDebounceTick;
                this.debounceTimers[name] = timer;
            }

            timer.Stop();
            timer.Interval = delayMs < 1 ? 1 : delayMs;

            timer.Tag = action;
            timer.Start();
        }

        protected void CancelDebounce(string key)
        {
            System.Windows.Forms.Timer timer;

            if (this.debounceTimers.TryGetValue(key ?? string.Empty, out timer))
            {
                timer.Stop();
            }
        }

        private void OnDebounceTick(object sender, EventArgs e)
        {
            System.Windows.Forms.Timer timer = sender as System.Windows.Forms.Timer;

            if (timer == null)
            {
                return;
            }

            timer.Stop();

            MethodInvoker action = timer.Tag as MethodInvoker;

            if (action != null && !this.IsDisposed && !this.Disposing)
            {
                action();
            }
        }

        protected virtual string ExcelDialogFilter
        {
            get { return "Excel Workbook|*.xlsx"; }
        }

        protected virtual string ExportEmptyText()
        {
            return "Nothing to export.";
        }

        protected virtual string ExportDoneText(int rowCount, string rowNoun)
        {
            return rowCount.ToString("N0") + " " + rowNoun + " exported.";
        }

        /// <summary>엑셀 저장 실패의 Why 줄. 조치는 <see cref="ExportFailedNextText"/>, 예외 원문은 다이얼로그 상세로 넘긴다(2026-10-21).</summary>
        protected virtual string ExportFailedText(Exception failure)
        {
            return "The file could not be written.";
        }

        /// <summary>엑셀 저장 실패의 Next 줄.</summary>
        protected virtual string ExportFailedNextText
        {
            get { return "Close the file if it is open in another program, then export again."; }
        }

        protected bool ExportToExcel(
            Modern.Lab.WinForms.Controls.Data.IExcelExportGrid grid,
            string sheetName,
            System.Data.DataTable data,
            string fileNamePrefix)
        {
            return this.ExportToExcel(grid, sheetName, data, fileNamePrefix, "rows");
        }

        protected bool ExportToExcel(
            Modern.Lab.WinForms.Controls.Data.IExcelExportGrid grid,
            string sheetName,
            System.Data.DataTable data,
            string fileNamePrefix,
            string rowNoun)
        {
            if (grid == null)
            {
                return false;
            }

            if (data == null || data.Rows.Count == 0)
            {
                this.ShowToast(
                    this.ExportEmptyText(),
                    Modern.Lab.Controls.Wpf.Display.ToastKind.Warning);
                return false;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = this.ExcelDialogFilter;
                dialog.FileName = (fileNamePrefix ?? "Export")
                        + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return false;
                }

                try
                {
                    grid.ExportXlsx(dialog.FileName, sheetName, data);
                    this.ShowToast(
                        this.ExportDoneText(data.Rows.Count, rowNoun ?? "rows"),
                        Modern.Lab.Controls.Wpf.Display.ToastKind.Success);
                    return true;
                }
                catch (Exception exception)
                {
                    // 실패는 토스트가 아니라 다이얼로그 — 예외 원문은 상세로(현업이 복사해 개발자에게 보낼 수 있게).
                    this.ShowErrorMessage(
                            "Export failed", this.ExportFailedText(exception), this.ExportFailedNextText, exception.ToString());
                    return false;
                }
            }
        }

        protected void PostToUi(MethodInvoker action)
        {
            this.TryPostToUi(action);
        }

        private bool TryPostToUi(MethodInvoker action)
        {
            if (action == null || this.IsDisposed || this.Disposing || !this.IsHandleCreated)
            {
                return false;
            }

            try
            {
                this.BeginInvoke(action);
                return true;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        // 조회 기한 — FetchAsync/LoadAsync의 조회가 이 시간 안에 돌아오지 않으면 그 조회만
        // 실패(TimeoutException)로 끝내 로딩 표시와 조회 중 상태를 푼다. 원래 오래 걸리는 조회가
        // 있는 화면은 재정의하고, TimeSpan.Zero 이하면 기한을 두지 않는다. 실행(RunAction)에는
        // 쓰지 않는다 — 늦은 응답도 서버에서는 처리됐을 수 있어 실패로 끊으면 안 된다.
        protected virtual TimeSpan QueryTimeout
        {
            get { return TimeSpan.FromSeconds(30d); }
        }

        private readonly object queryDeadlineLock = new object();

        private readonly Dictionary<string, int> abandonedQueries = new Dictionary<string, int>(StringComparer.Ordinal);

        // 조회 하나를 백그라운드에서 돌리고 결과(또는 기한 초과)를 settle로 정확히 한 번 넘긴다.
        // 기한을 넘겨 버려진 조회가 전송에 붙잡혀 있는 동안 같은 채널의 새 조회는 전문을 보내지
        // 않고 바로 실패한다 — 자동 새로고침이 응답 없는 호출을 계속 쌓지 않게 하기 위해서다.
        private void QueueQuery<T>(string key, Func<T> fetch, Action<T, Exception> settle)
        {
            lock (this.queryDeadlineLock)
            {
                if (this.abandonedQueries.ContainsKey(key))
                {
                    ThreadPool.QueueUserWorkItem(delegate(object state)
                    {
                        settle(default(T), new TimeoutException(
                                "The server has not replied to the previous request yet."));
                    });
                    return;
                }
            }

            bool[] finished = new bool[1];
            TimeSpan limit = this.QueryTimeout;
            System.Threading.Timer deadline = null;

            if (limit > TimeSpan.Zero)
            {
                deadline = new System.Threading.Timer(delegate(object state)
                {
                    lock (this.queryDeadlineLock)
                    {
                        if (finished[0])
                        {
                            return;
                        }

                        finished[0] = true;
                        int count;
                        this.abandonedQueries.TryGetValue(key, out count);
                        this.abandonedQueries[key] = count + 1;
                    }

                    settle(default(T), new TimeoutException(string.Format(
                            System.Globalization.CultureInfo.InvariantCulture,
                            "The server did not reply within {0:0} seconds.",
                            limit.TotalSeconds)));
                }, null, limit, System.Threading.Timeout.InfiniteTimeSpan);
            }

            System.Threading.Timer started = deadline;

            ThreadPool.QueueUserWorkItem(delegate(object state)
            {
                T result = default(T);
                Exception failure = null;

                try
                {
                    result = fetch();
                }
                catch (Exception exception)
                {
                    failure = exception;
                }

                bool won;

                lock (this.queryDeadlineLock)
                {
                    won = !finished[0];
                    finished[0] = true;

                    if (!won)
                    {
                        int count;

                        if (this.abandonedQueries.TryGetValue(key, out count))
                        {
                            if (count <= 1)
                            {
                                this.abandonedQueries.Remove(key);
                            }
                            else
                            {
                                this.abandonedQueries[key] = count - 1;
                            }
                        }
                    }
                }

                if (started != null)
                {
                    started.Dispose();
                }

                if (won)
                {
                    settle(result, failure);
                }
            });
        }

        protected void LoadAsync<T>(string channel, Func<T> fetch, Action<T> apply, MethodInvoker onComplete)
        {
            this.LoadAsync(channel, fetch, apply,
                    onComplete == null
                            ? (Action<bool>)null
                            : delegate(bool current) { onComplete(); });
        }

        protected void LoadAsync<T>(string channel, Func<T> fetch, Action<T> apply, Action<bool> onComplete)
        {
            if (fetch == null)
            {
                if (onComplete != null)
                {
                    onComplete(false);
                }

                return;
            }

            string key = channel ?? string.Empty;
            int version = this.NextLoadVersion(key);

            this.BeginQuery();

            this.QueueQuery(key, fetch, delegate(T result, Exception failure)
            {
                bool posted = this.TryPostToUi(new MethodInvoker(delegate
                {
                    try
                    {
                        bool current = this.SettleLoad(channel, key, version, failure);

                        if (current && failure == null && apply != null)
                        {
                            apply(result);
                        }

                        if (onComplete != null)
                        {
                            onComplete(current);
                        }
                    }
                    finally
                    {
                        this.EndQuery();
                    }
                }));

                if (!posted)
                {
                    this.EndQuery();
                }
            });
        }

        protected void LoadAsync<T>(string channel, Func<T> fetch, Action<T> apply)
        {
            this.LoadAsync(channel, fetch, apply, (Action<bool>)null);
        }

        protected System.Threading.Tasks.Task<LoadOutcome<T>> FetchAsync<T>(
            string channel, Func<T> fetch)
        {
            return this.FetchAsync(channel, fetch, false);
        }

        protected System.Threading.Tasks.Task<LoadOutcome<T>> FetchAsync<T>(
            string channel, Func<T> fetch, bool silent)
        {
            string key = channel ?? string.Empty;

            System.Threading.Tasks.TaskCompletionSource<LoadOutcome<T>> completion =
                    new System.Threading.Tasks.TaskCompletionSource<LoadOutcome<T>>();

            if (fetch == null)
            {
                completion.SetResult(new LoadOutcome<T>(false, default(T), null));
                return completion.Task;
            }

            int version = this.NextLoadVersion(key);

            this.BeginQuery();

            this.QueueQuery(key, fetch, delegate(T result, Exception failure)
            {
                bool posted = this.TryPostToUi(new MethodInvoker(delegate
                {
                    LoadOutcome<T> outcome;

                    try
                    {
                        bool current = this.SettleLoad(channel, key, version, failure, silent);
                        outcome = new LoadOutcome<T>(current, result, failure);
                    }
                    catch (Exception settleFailure)
                    {
                        outcome = new LoadOutcome<T>(false, result, failure ?? settleFailure);
                    }

                    this.EndQuery();
                    completion.TrySetResult(outcome);
                }));

                if (!posted)
                {
                    this.EndQuery();
                    completion.TrySetResult(new LoadOutcome<T>(false, result, failure));
                }
            });

            return completion.Task;
        }

        protected void InvalidateChannel(string channel)
        {
            string key = channel ?? string.Empty;
            this.NextLoadVersion(key);

            this.staleChannels.Remove(key);
            this.silentFailureChannels.Remove(key);
            this.RenderStickyNotice();
        }

        protected MethodInvoker WhenAll(int count, MethodInvoker onAll)
        {
            if (count < 1)
            {
                if (onAll != null)
                {
                    onAll();
                }

                return new MethodInvoker(delegate { });
            }

            int[] remaining = new int[] { count };

            return new MethodInvoker(delegate
            {
                if (remaining[0] <= 0)
                {
                    return;
                }

                remaining[0]--;

                if (remaining[0] == 0 && onAll != null)
                {
                    onAll();
                }
            });
        }

        protected virtual void OnLoadFailed(string channel, Exception failure)
        {
            if (failure != null && !string.IsNullOrEmpty(channel))
            {
                failure.Data["Channel"] = channel;
            }

            this.ShowServerFailure(failure);
        }

        private bool SettleLoad(string channel, string key, int version, Exception failure)
        {
            return this.SettleLoad(channel, key, version, failure, false);
        }

        private bool SettleLoad(
                string channel, string key, int version, Exception failure, bool silent)
        {
            bool current = !this.IsDisposed
                    && !this.Disposing
                    && this.IsCurrentLoadVersion(key, version);

            if (current)
            {
                if (failure != null)
                {
                    this.NotifyLoadFailure(key, silent);
                    this.loadFailureSilent = silent;

                    try
                    {
                        this.OnLoadFailed(channel, failure);
                    }
                    finally
                    {
                        this.loadFailureSilent = false;
                    }
                }
                else
                {
                    this.NotifyLoadSuccess(key);
                }
            }

            return current;
        }

        private int NextLoadVersion(string channel)
        {
            int version;
            this.loadVersions.TryGetValue(channel, out version);
            version++;
            this.loadVersions[channel] = version;
            return version;
        }

        private bool IsCurrentLoadVersion(string channel, int version)
        {
            int current;
            return this.loadVersions.TryGetValue(channel, out current) && current == version;
        }

        protected bool IsServerConnectionLost
        {
            get { return this.connectionLost; }
        }

        protected virtual void OnServerConnectionChanged(bool lost)
        {
        }

        private void NotifyLoadFailure(string channel, bool silent = false)
        {
            if (this.IsDisposed || this.Disposing)
            {
                return;
            }

            if (channel != null)
            {
                this.staleChannels.Add(channel);
                if (silent)
                {
                    this.silentFailureChannels.Add(channel);
                }
                else
                {
                    this.silentFailureChannels.Remove(channel);
                }
            }

            bool wasLost = this.connectionLost;
            this.connectionLost = true;
            this.failureNoticeShowing = true;

            if (!wasLost)
            {
                this.OnServerConnectionChanged(true);
            }
        }

        // 실패 창 본문 아래 줄 — "No data received yet." 또는 "Showing data as of HH:mm:ss."
        private string StaleBasisText()
        {
            DateTime basis = this.StaleBasis(this.staleChannels);

            return basis == DateTime.MinValue || basis == DateTime.MaxValue
                    ? "No data received yet."
                    : "Showing data as of "
                            + basis.ToString("HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)
                            + ".";
        }

        private string BuildStaleNoticeText(string lead, ICollection<string> channels)
        {
            DateTime basis = this.StaleBasis(channels);

            return basis == DateTime.MinValue || basis == DateTime.MaxValue
                    ? lead + " — no data received yet."
                    : lead + " — showing data as of "
                            + basis.ToString(
                                    "HH:mm:ss",
                                    System.Globalization.CultureInfo.InvariantCulture)
                            + ".";
        }

        // 실패한 채널들의 마지막 성공 중 가장 오래된 시각. 성공 이력이 없는 채널이 있으면 MinValue, 채널이 없으면 전체 마지막 성공.
        private DateTime StaleBasis(ICollection<string> channels)
        {
            if (channels.Count == 0)
            {
                return this.lastAnyLoadSuccessTime;
            }

            DateTime basis = DateTime.MaxValue;

            foreach (string staleChannel in channels)
            {
                DateTime channelTime;

                if (!this.lastLoadSuccessTimes.TryGetValue(staleChannel, out channelTime))
                {
                    return DateTime.MinValue;
                }

                if (channelTime < basis)
                {
                    basis = channelTime;
                }
            }

            return basis;
        }

        private void NotifyLoadSuccess(string channel)
        {
            DateTime now = DateTime.Now;
            this.lastLoadSuccessTimes[channel] = now;
            this.lastAnyLoadSuccessTime = now;
            this.staleChannels.Remove(channel);
            this.silentFailureChannels.Remove(channel);
            this.SettleNoticeAfterServerContact(true);
        }

        private void SettleNoticeAfterServerContact(bool refreshedByData)
        {
            if (this.IsDisposed || this.Disposing)
            {
                return;
            }

            bool wasLost = this.connectionLost;
            this.connectionLost = false;

            if (this.failureNoticeShowing)
            {
                if (this.staleChannels.Count == 0)
                {
                    this.failureNoticeShowing = false;
                    this.renderedNoticeBasis = string.Empty;

                    if (this.contractNoticeText.Length == 0)
                    {
                        this.ShowTransientNotice(
                                refreshedByData
                                        ? "Reconnected — data refreshed."
                                        : "Reconnected — server responded.",
                                Modern.Lab.Controls.Wpf.Display.ToastKind.Success);
                        this.renderedNoticeBasis = this.NoticeBasis();
                        return;
                    }

                    this.RenderStickyNotice();
                }
                else
                {
                    this.RenderStickyNotice();
                }
            }

            if (wasLost)
            {
                this.OnServerConnectionChanged(false);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.DisposeMessaging();
                this.RuleGuideDispose();

                foreach (KeyValuePair<string, System.Windows.Forms.Timer> entry in this.debounceTimers)
                {
                    entry.Value.Stop();
                    entry.Value.Dispose();
                }

                this.debounceTimers.Clear();
            }

            base.Dispose(disposing);
        }
    }
}
