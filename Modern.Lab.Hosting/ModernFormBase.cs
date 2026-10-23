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

        private readonly List<string> pendingFailureTexts = new List<string>();
        private readonly List<string> pendingFailureDetails = new List<string>();
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
        /// 셸 안에 자식 폼(<c>TopLevel = false</c>)으로 들어간 화면은 셸이 크기를 정하므로 건드리지 않는다.
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
            this.pendingFailureTexts.Add(this.ServerFailureText(failure));
            this.pendingFailureDetails.Add(FailureDetails(failure));
            this.Debounce(failureDialogKey, 150, new MethodInvoker(this.FlushFailureDialog));
        }

        /// <summary>실패 다이얼로그의 Details — 어느 채널(<c>Data["Channel"]</c>)의 어느 전문(<c>Data["Request"]</c>)이 실패했는지.
        /// 본문은 현업이 읽는 문구 그대로이고, 내부 이름은 여기로만 간다.</summary>
        private static string FailureDetails(Exception failure)
        {
            if (failure == null)
            {
                return string.Empty;
            }

            string channel = Convert.ToString(failure.Data["Channel"]) ?? string.Empty;
            string request = Convert.ToString(failure.Data["Request"]) ?? string.Empty;
            List<string> lines = new List<string>();

            if (channel.Length > 0)
            {
                lines.Add("channel: " + channel);
            }

            if (request.Length > 0)
            {
                lines.Add("request: " + request);
            }

            return string.Join(Environment.NewLine, lines.ToArray());
        }

        private void FlushFailureDialog()
        {
            if (this.IsDisposed || this.Disposing || this.pendingFailureTexts.Count == 0)
            {
                return;
            }

            if (this.QueryInProgress)
            {
                this.Debounce(failureDialogKey, 150, new MethodInvoker(this.FlushFailureDialog));
                return;
            }

            string first = this.pendingFailureTexts[0];
            int count = this.pendingFailureTexts.Count;
            this.pendingFailureTexts.Clear();

            List<string> detailLines = new List<string>();

            foreach (string detail in this.pendingFailureDetails)
            {
                if (detail.Length > 0)
                {
                    detailLines.Add(detail);
                }
            }

            this.pendingFailureDetails.Clear();

            string message = count == 1
                    ? first
                    : count.ToString(System.Globalization.CultureInfo.InvariantCulture)
                            + " queries failed. " + first;

            this.ShowErrorMessage(
                    this.QueryFailedCaption,
                    this.BuildStaleNoticeText(message),
                    string.Join(Environment.NewLine + Environment.NewLine, detailLines.ToArray()));
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

        /// <summary>엑셀 저장 실패의 사용자 문구 — 행동할 수 있는 한 문장. 예외 원문은 다이얼로그 상세로 넘긴다(2026-10-21).</summary>
        protected virtual string ExportFailedText(Exception failure)
        {
            return "Export failed. Close the file if it is open in another program and try again.";
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
                    this.ShowErrorMessage("Export failed", this.ExportFailedText(exception), exception.ToString());
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
                                "The server has not replied to the previous request yet. Try again later."));
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
                            "The server did not reply within {0:0} seconds. Try again later.",
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

        private string BuildStaleNoticeText(string lead)
        {
            return this.BuildStaleNoticeText(lead, this.staleChannels);
        }

        private string BuildStaleNoticeText(string lead, ICollection<string> channels)
        {
            DateTime basis;

            if (channels.Count > 0)
            {
                basis = DateTime.MaxValue;

                foreach (string staleChannel in channels)
                {
                    DateTime channelTime;

                    if (!this.lastLoadSuccessTimes.TryGetValue(staleChannel, out channelTime))
                    {
                        basis = DateTime.MinValue;
                        break;
                    }

                    if (channelTime < basis)
                    {
                        basis = channelTime;
                    }
                }
            }
            else
            {
                basis = this.lastAnyLoadSuccessTime;
            }

            return basis == DateTime.MinValue || basis == DateTime.MaxValue
                    ? lead + " — no data received yet."
                    : lead + " — showing data as of "
                            + basis.ToString(
                                    "HH:mm:ss",
                                    System.Globalization.CultureInfo.InvariantCulture)
                            + ".";
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
