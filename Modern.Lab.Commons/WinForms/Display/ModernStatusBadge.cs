using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Modern.Lab.WinForms.Rendering;

namespace Modern.Lab.WinForms.Controls.Display
{
    /// <summary>
    /// 상태 표시 pill 배지 (색 있는 Label 사용을 대체).
    ///
    /// 원래 WPF ModernStatusBadgeControl을 ElementHost로 호스팅했지만, 배지는
    /// 라벨과 함께 폼에 여러 개 놓이는 표시 전용 컨트롤이라 개당 HWND + WPF
    /// 컴포지션 비용이 아까웠다. ModernLabel과 같은 이유로 디자인 토큰을
    /// 미러링해 **GDI+로 직접 그리는 순수 WinForms 컨트롤**로 전환했다.
    /// 공개 API(Text/Color)와 시각 결과는 WPF 버전과 동일하다.
    ///
    /// 승인/반려/대기, 운영/개발 같은 상태를 색 있는 배지(기본 둥근 사각)로 표시한다.
    /// Color에 배경색만 주면 글자색은 배경과 같은 색상 계열로 자동 유도된다
    /// (요약 칩과 동일한 규칙 — ChipColorHelper의 HSL 규칙을 GDI+로 미러링).
    ///
    /// 색 지정 3경로 (우선순위: Color &gt; Semantic &gt; ColorValue):
    ///   - <see cref="Color"/>: "#DCFCE7" 직접 색 (완전 수동).
    ///   - <see cref="Semantic"/>: 의미색 enum — SemanticColors 표준색 (디자이너에서 헥사 없이).
    ///   - <see cref="ColorValue"/>: 값 문자열 — Palette.FromValue 유도색. 그리드/트리
    ///     배지의 자동색(BadgeAutoColor)과 같은 규칙이라 같은 값이면 화면 어디서든 같은 색.
    ///
    /// <see cref="Spinning"/>을 켜면 배지 테두리를 도는 하이라이트(코멧 빛띠)가
    /// 생긴다 — 그리드 배지의 BadgeSpinBorderMember와 같은 시각 언어(작업중 표시).
    /// <see cref="SpinValues"/>에 값 목록을 선언하면 Text가 그 중 하나일 때만
    /// 자동으로 켜진다 — 그리드 컬럼 BadgeSpinValues와 같은 선언형 경로.
    ///
    /// 텍스트가 배지 폭을 넘으면 말줄임(…)으로 그려지고, 이때 전체 텍스트가
    /// 자동으로 마우스 오버 툴팁에 노출된다 — 다이얼로그의 긴 오류 메시지도
    /// 배지 위에 커서를 올리면 전문을 읽을 수 있다.
    /// </summary>
    [ToolboxItem(true)]
    [DesignerCategory("Code")]
    public class ModernStatusBadge : Control
    {
        // 중립 배지 색은 중앙 팔레트(ModernTheme)에서 — 라이트/다크에 따라 바뀐다.
        private static Color NeutralBackground { get { return Modern.Lab.Theming.ModernTheme.NeutralBackground; } }
        private static Color NeutralText { get { return Modern.Lab.Theming.ModernTheme.NeutralText; } }

        // Font.Size.Helper 12 DIU = 9pt, 배지 텍스트는 SemiBold.
        private static readonly Font BadgeFont = ModernFonts.BodyStrong;

        // 배지 안쪽 패딩 — 세로 3px로 그리드 셀 배지(3px)와 같은 약 22px 높이다
        // (2026-10-12: 4px 의 24px 에서 2px 줄였다 — 글자 상자 16px 위아래에 1px 씩 남는 선이 최소다).
        private const int pillPaddingX = 10;
        private const int pillPaddingY = 3;

        // Rounded 모양의 모서리 반경 (Radius.Sm 4 미러).
        private const int roundedRadius = 4;

        // 위 작은 글씨 크기(DIU) — 버튼·라벨·그리드 헤더의 작은 글씨와 같은 10(토큰 Font.Size.SubLabel).
        private const double subLabelDiu = 10d;

        private string colorText;
        private string colorValue;
        private string topLabel = string.Empty;
        private HorizontalAlignment topLabelAlignment = HorizontalAlignment.Left;

        private string accentValues = string.Empty;
        private string autoColorValues = string.Empty;
        private string spinValues = string.Empty;
        private Modern.Lab.Theming.SemanticKind semantic;
        private Color pillBackground;
        private Color pillForeground;
        private double fontWidthRatio;
        private BadgeShape shape;
        private bool fillWidth;
        private bool fillHeight;

        // 빛띠가 한 바퀴 도는 데 걸리는 시간(ms).
        private const double spinPeriodMs = 2000.0;

        // 빛띠를 그릴 때 둘레를 다시 나누는 간격(px)과 그 상한. 간격이 촘촘할수록
        // 알파가 부드럽게 이어지고, 상한은 아주 큰 배지에서 선 개수가 폭주하는
        // 것을 막는다.
        private const float spinSampleStep = 2f;
        private const int spinSampleLimit = 1024;

        // 회전 하이라이트 상태 — 타이머는 Spinning이 켜지고 화면에 보일 때만 돈다.
        private bool spinning;
        private Timer spinTimer;

        // 위상은 틱 수가 아니라 **경과 시간**으로 잰다. WinForms 타이머는 메시지
        // 기반이라 UI 스레드가 바쁘면(조회 응답 처리 등) 틱을 거르는데, 틱마다
        // 같은 양을 더하면 거른 만큼 빛띠가 멈췄다가 이어져 끊겨 보였다.
        private readonly System.Diagnostics.Stopwatch spinClock = new System.Diagnostics.Stopwatch();

        // 잘림 자동 툴팁 — 텍스트가 배지 폭을 넘어 말줄임(…)되면 전체 텍스트를
        // 마우스 오버 툴팁으로 보여준다 (긴 오류 메시지가 좁은 다이얼로그에서도
        // 읽히게). 필요해질 때 생성하고, 마지막 적용 텍스트로 중복 적용을 막는다.
        private ToolTip overflowToolTip;
        private string appliedToolTipText;

        /// <summary>적절한 기본 크기로 컨트롤을 생성한다.</summary>
        public ModernStatusBadge()
        {
            this.SetStyle(
                ControlStyles.UserPaint
                | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw
                | ControlStyles.SupportsTransparentBackColor,
                true);

            this.Size = new Size(70, 26);
            this.BackColor = System.Drawing.Color.Transparent;
            this.colorText = string.Empty;
            this.colorValue = string.Empty;
            this.semantic = Modern.Lab.Theming.SemanticKind.None;
            this.appliedToolTipText = string.Empty;
            this.pillBackground = NeutralBackground;
            this.pillForeground = NeutralText;
            this.shape = BadgeShape.Rounded;
            this.Text = "Status";
        }

        /// <summary>배지 모양 — 둥근 사각(Rounded, 기본) 또는 알약(Pill).</summary>
        [Category("Modern")]
        [Description("Badge shape (Rounded = default / Pill)")]
        [DefaultValue(BadgeShape.Rounded)]
        public BadgeShape Shape
        {
            get
            {
                return this.shape;
            }
            set
            {
                this.shape = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 알약을 항상 컨트롤 폭으로 그릴지 여부 (기본 false = 텍스트 폭에 맞춤).
        /// KPI 스트립처럼 배지를 나란히 두는 화면에서 텍스트 길이와 무관하게
        /// 폭을 통일할 때 켠다 — 켜면 알약 폭이 컨트롤 Size.Width로 고정되고,
        /// 텍스트는 가운데, 넘치면 기존과 같이 말줄임(…) + 마우스 오버 툴팁이다.
        /// </summary>
        [Category("Modern")]
        [Description("Always draw the pill at the control width (default false = fit to text)")]
        [DefaultValue(false)]
        public bool FillWidth
        {
            get
            {
                return this.fillWidth;
            }
            set
            {
                this.fillWidth = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 배지를 항상 컨트롤 높이로 그릴지 여부 (기본 false = 글자 높이에 맞춤).
        /// <see cref="TopLabel"/>이 있는 배지와 없는 배지를 나란히 둘 때 높이를 통일한다 —
        /// 켜면 배지 높이가 컨트롤 Size.Height로 고정되고 글자는 세로 가운데에 놓인다.
        /// </summary>
        [Category("Modern")]
        [Description("Always draw the badge at the control height (default false = fit to text)")]
        [DefaultValue(false)]
        public bool FillHeight
        {
            get
            {
                return this.fillHeight;
            }
            set
            {
                this.fillHeight = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 글자 폭에 맞춰 컨트롤 폭을 스스로 늘릴지 여부 (기본 false = 디자이너 크기 고정).
        /// 켜면 본문(위 작은 글씨가 있으면 둘 중 넓은 쪽)에 좌우 여백을 더한 폭이 되고, 글자·위 글씨·글꼴·장평이
        /// 바뀔 때마다 다시 맞춘다. 높이는 바꾸지 않고 왼쪽을 기준으로 오른쪽으로만 는다. <see cref="Control.MaximumSize"/>의
        /// 폭이 있으면 그 안에서 멈추고 넘친 글자는 말줄임(…)이다. <see cref="FillWidth"/>와 함께 켜면 알약도 그 폭이 된다
        /// (2026-10-17 — 결정 패널의 긴 이벤트 이름이 잘려서).
        /// </summary>
        [Category("Modern")]
        [Description("Grow the control width to fit the text (default false = fixed size)")]
        [DefaultValue(false)]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override bool AutoSize
        {
            get
            {
                return base.AutoSize;
            }
            set
            {
                base.AutoSize = value;
                this.AdjustWidthToContent();
            }
        }

        /// <summary><see cref="AutoSize"/> 폭 — 본문과 위 작은 글씨 중 넓은 쪽에 좌우 여백을 더한 값. 높이는 지금 높이 그대로.</summary>
        public override Size GetPreferredSize(Size proposedSize)
        {
            return new Size(this.ContentWidth() + (pillPaddingX * 2), this.Height);
        }

        // AutoSize가 켜진 동안은 디자이너·코드가 준 폭을 글자 폭으로 바꾼다 — Label의 AutoSize와 같은 방식.
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if (this.AutoSize)
            {
                width = this.ClampedContentWidth();
            }

            base.SetBoundsCore(x, y, width, height, specified);
        }

        private void AdjustWidthToContent()
        {
            if (!this.AutoSize)
            {
                return;
            }

            int width = this.ClampedContentWidth();

            if (width != this.Width)
            {
                this.Width = width;
            }
        }

        private int ClampedContentWidth()
        {
            int width = this.ContentWidth() + (pillPaddingX * 2);
            int maximum = this.MaximumSize.Width;
            return maximum > 0 ? Math.Min(width, maximum) : width;
        }

        // OnPaint와 같은 글꼴·장평으로 잰다 — 측정과 그리기가 어긋나면 말줄임이 엉뚱한 곳에서 난다.
        private int ContentWidth()
        {
            double widthRatio = Modern.Lab.Theming.ModernTheme.ResolveFontWidthRatio(this.fontWidthRatio);
            TextFormatFlags flags = TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding;
            int width = ScaledTextRenderer.MeasureText(this.Text, BadgeFont, flags, widthRatio).Width + this.Padding.Horizontal;

            if (this.topLabel.Length > 0)
            {
                using (Font smallFont = ModernFonts.Create(subLabelDiu, false, FontStyle.Regular))
                {
                    width = Math.Max(width, ScaledTextRenderer.MeasureText(this.topLabel, smallFont, flags, widthRatio).Width);
                }
            }

            return width;
        }

        /// <summary>
        /// ElementHost 시절 .Designer.cs가 직렬화한 "Child = null" 대입과의
        /// 소스 호환용 무동작 속성. 새 코드에서는 사용하지 않는다.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object Child
        {
            get { return null; }
            set { }
        }

        /// <summary>배지 텍스트.</summary>
        [Category("Modern")]
        [Description("Text shown on the badge")]
        [Browsable(true)]
        [Localizable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue("Status")]
        public override string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        /// <summary>
        /// 본문 위의 작은 글씨(비어 있으면 없음) — 버튼·라벨의 작은 글씨와 같은 10 크기, 배지 글자색.
        /// 있으면 배지가 그 줄만큼 높아지므로 컨트롤 높이를 넉넉히(34 이상) 준다. 정렬은 <see cref="TopLabelAlignment"/>.
        /// </summary>
        [Category("Modern")]
        [Description("Small text above the badge text (empty = none)")]
        [Localizable(true)]
        [DefaultValue("")]
        public string TopLabel
        {
            get
            {
                return this.topLabel;
            }
            set
            {
                this.topLabel = value ?? string.Empty;
                this.AdjustWidthToContent();
                this.Invalidate();
            }
        }

        /// <summary>위 작은 글씨의 정렬 — 본문 글자의 왼쪽 끝(Left, 기본)·가운데·오른쪽 끝에 맞춘다.</summary>
        [Category("Modern")]
        [Description("Alignment of the top small text relative to the badge text (Left default)")]
        [DefaultValue(HorizontalAlignment.Left)]
        public HorizontalAlignment TopLabelAlignment
        {
            get
            {
                return this.topLabelAlignment;
            }
            set
            {
                this.topLabelAlignment = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 배경색 문자열 ("#DCFCE7" hex 또는 "SkyBlue" 색 이름).
        /// 글자색은 자동 유도. 비우면 중립 회색 배지.
        /// </summary>
        [Category("Modern")]
        [Description("Badge background (hex or color name string; empty = neutral gray, text color derived)")]
        [DefaultValue("")]
        public string Color
        {
            get
            {
                return this.colorText;
            }
            set
            {
                this.colorText = value ?? string.Empty;
                this.ResolvePillColors();
                this.Invalidate();
            }
        }

        /// <summary>
        /// 값 문자열에서 색을 자동 유도한다 (Palette.FromValue — 같은 값 = 항상 같은 색).
        /// 그리드/트리 배지의 자동색(BadgeAutoColor)과 같은 규칙이라, 여기에 같은
        /// 값을 넣으면 그리드 배지와 자동으로 같은 색이 된다 (KPI 집계 배지,
        /// 정보 카드의 단건 배지 등). 우선순위는 Color &gt; Semantic &gt; ColorValue.
        /// 비우면(기본) 이 경로는 쓰지 않는다.
        /// </summary>
        [Category("Modern")]
        [Description("Value string the color is derived from (same rule as grid auto colors — same value = same color)")]
        [DefaultValue("")]
        public string ColorValue
        {
            get
            {
                return this.colorValue;
            }
            set
            {
                this.colorValue = value ?? string.Empty;
                this.ResolvePillColors();
                this.Invalidate();
            }
        }

        /// <summary>
        /// **빨간 글자로 강조할 값 목록** (기본 빈 문자열 = 미사용). 세미콜론이나
        /// 콤마로 여러 개를 적는다(`"Down;OffLine"`). 배지의 <see cref="Text"/>가
        /// 이 중 하나면 배경과 글자를 오류 색 짝으로 바꾼다 — 색 우선순위 셋보다
        /// 앞선다.
        ///
        /// 옅은 색이 아니라 **진한 채움**을 쓰는 이유: 주변 배지가 전부 파스텔이라
        /// 같은 톤으로는 눈에 걸리지 않는다. 채도와 무게가 다른 칩이라야 종류가
        /// 다른 신호로 읽힌다. 글자색은 채움색에서 유도해 전 테마 대비를 지킨다.
        /// </summary>
        [Category("Modern")]
        [Description("Values shown with error-red text (semicolon or comma separated) — overrides the color paths")]
        [DefaultValue("")]
        public string AccentValues
        {
            get
            {
                return this.accentValues;
            }
            set
            {
                this.accentValues = value ?? string.Empty;
                this.ResolvePillColors();
                this.Invalidate();
            }
        }

        /// <summary>
        /// 의미색 지정 (기본 None = 미사용) — SemanticColors 표준색을 헥사 없이
        /// 지정한다 (Success/Warning/Error/Info/Neutral). 우선순위는
        /// Color &gt; Semantic &gt; ColorValue.
        /// </summary>
        [Category("Modern")]
        [Description("Semantic color (Success/Warning/Error/Info/Neutral — SemanticColors standard)")]
        [DefaultValue(Modern.Lab.Theming.SemanticKind.None)]
        public Modern.Lab.Theming.SemanticKind Semantic
        {
            get
            {
                return this.semantic;
            }
            set
            {
                this.semantic = value;
                this.ResolvePillColors();
                this.Invalidate();
            }
        }

        /// <summary>
        /// 배지 테두리를 도는 하이라이트(코멧 빛띠) 표시 여부 (기본 false) —
        /// 작업중/전송중 같은 활성 상태 표시. 그리드 배지의
        /// BadgeSpinBorderMember와 같은 시각 언어다. 컨트롤이 보일 때만
        /// 타이머가 돌고 숨김/Dispose 시 자동 정지한다.
        /// </summary>
        [Category("Modern")]
        [Description("Show a highlight traveling around the badge border (working indicator)")]
        [DefaultValue(false)]
        public bool Spinning
        {
            get
            {
                return this.spinning;
            }
            set
            {
                this.spinning = value;
                this.UpdateSpinTimer();
                this.Invalidate();
            }
        }

        /// <summary>
        /// **회전 하이라이트를 켤 값 목록** (기본 빈 문자열 = 미사용). 세미콜론이나
        /// 콤마로 여러 개를 적는다("Run;Sending"). 배지의 <see cref="Text"/>가
        /// 이 중 하나면(공백/대소문자 무시 — FromValue와 같은 정규화)
        /// <see cref="Spinning"/>이 자동으로 켜지고 벗어나면 꺼진다 — 그리드
        /// 컬럼 BadgeSpinValues와 같은 문법·의미의 선언형 경로다.
        ///
        /// 비어 있지 않으면 Text가 바뀔 때마다 Spinning을 다시 판정하므로,
        /// Spinning 직접 대입은 다음 Text 변경 때 덮어써진다. 비우면(기본)
        /// Spinning은 다시 수동 스위치가 된다.
        /// </summary>
        [Category("Modern")]
        [Description("Values that turn the spinning border on (semicolon or comma separated) — same rule as grid BadgeSpinValues")]
        [DefaultValue("")]
        public string SpinValues
        {
            get
            {
                return this.spinValues;
            }
            set
            {
                this.spinValues = value ?? string.Empty;
                this.ApplySpinValues();
            }
        }

        /// <summary>장평(글자 가로 비율) 재정의. 0 = 전역(ModernTheme.FontWidthRatio) 사용.</summary>
        [Category("Modern")]
        [Description("Font width ratio override — 0 = use global (ModernTheme.FontWidthRatio), allowed 0.8~1.2")]
        [DefaultValue(0d)]
        public double FontWidthRatio
        {
            get
            {
                return this.fontWidthRatio;
            }
            set
            {
                this.fontWidthRatio = value;
                this.AdjustWidthToContent();
                this.Invalidate();
            }
        }

        /// <summary>텍스트가 바뀌면 pill 폭이 달라지므로 다시 그린다.</summary>
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            // 강조 판정이 Text를 보므로 색을 다시 정한다.
            if (this.accentValues.Length > 0)
            {
                this.ResolvePillColors();
            }

            // 회전 값 판정도 Text를 본다.
            this.ApplySpinValues();

            this.AdjustWidthToContent();
            this.Invalidate();
        }

        /// <summary>글꼴이 바뀌면 글자 폭도 바뀐다 — AutoSize 폭을 다시 맞춘다.</summary>
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            this.AdjustWidthToContent();
        }

        /// <summary>텍스트 폭에 맞는 pill을 왼쪽 정렬로 그린다
        /// (<see cref="FillWidth"/>가 참이면 컨트롤 폭 고정).</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            double widthRatio = Modern.Lab.Theming.ModernTheme.ResolveFontWidthRatio(this.fontWidthRatio);

            Size textSize = ScaledTextRenderer.MeasureText(
                e.Graphics, this.Text, BadgeFont, Size.Empty,
                TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding,
                widthRatio);

            // 위 작은 글씨가 있으면 배지가 그 줄만큼 높아지고, 폭은 두 줄 중 넓은 쪽을 따른다.
            Size topSize = Size.Empty;

            if (this.topLabel.Length > 0)
            {
                using (Font smallFont = ModernFonts.Create(subLabelDiu, false, FontStyle.Regular))
                {
                    topSize = ScaledTextRenderer.MeasureText(
                        e.Graphics, this.topLabel, smallFont, Size.Empty,
                        TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding,
                        widthRatio);
                }
            }

            // Padding 은 글자 영역만 안쪽으로 밀어 넣는다(알약은 그대로) — 파생 칩이 왼쪽에 작은 배지를 그릴 자리.
            int contentWidth = Math.Max(textSize.Width, topSize.Width) + this.Padding.Horizontal;
            int pillHeight = this.fillHeight
                    ? this.Height
                    : Math.Min(this.Height, textSize.Height + topSize.Height + (pillPaddingY * 2));
            int pillWidth = this.fillWidth
                    ? this.Width
                    : Math.Min(this.Width, contentWidth + (pillPaddingX * 2));
            int pillTop = (this.Height - pillHeight) / 2;
            Rectangle pill = new Rectangle(0, pillTop, pillWidth, pillHeight);
            Rectangle textArea = new Rectangle(
                    pill.Left + this.Padding.Left, pill.Top, Math.Max(0, pill.Width - this.Padding.Horizontal), pill.Height);

            SmoothingMode originalMode = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath path = this.shape == BadgeShape.Rounded
                    ? CreateRoundedPath(pill, roundedRadius)
                    : CreateCapsulePath(pill))
            using (SolidBrush fillBrush = new SolidBrush(this.pillBackground))
            {
                e.Graphics.FillPath(fillBrush, path);

                // 강조 배지에는 테두리를 두른다 — 색만으로는 테마에 따라 일반
                // 배지와 겹칠 수 있지만, 없던 테두리가 생기는 것은 어디서나 읽힌다.
                if (MatchesValueList(this.accentValues, this.Text))
                {
                    using (Pen outlinePen = new Pen(this.pillForeground, 1.5f))
                    {
                        e.Graphics.DrawPath(outlinePen, path);
                    }
                }
            }

            e.Graphics.SmoothingMode = originalMode;

            if (topSize.Height > 0)
            {
                this.PaintStackedText(e.Graphics, textArea, textSize, topSize, widthRatio);
            }
            else
            {
                ScaledTextRenderer.DrawText(
                    e.Graphics, this.Text, BadgeFont, textArea, this.pillForeground,
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding
                    | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                    | TextFormatFlags.EndEllipsis,
                    widthRatio);
            }

            // 회전 하이라이트 — 배지 테두리를 도는 코멧 빛띠 (작업중 표시).
            if (this.spinning)
            {
                SmoothingMode textMode = e.Graphics.SmoothingMode;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                this.DrawSpinHighlight(e.Graphics, pill);
                e.Graphics.SmoothingMode = textMode;
            }

            // 텍스트가 말줄임됐는지는 방금 측정한 원폭으로 판정한다 — 잘렸으면
            // 전체 텍스트를 툴팁으로 노출하고, 다 보이면 툴팁을 거둔다.
            this.UpdateOverflowToolTip(textSize.Width + this.Padding.Horizontal + (pillPaddingX * 2) > this.Width);
        }

        // 위 작은 글씨 + 본문 두 줄 — 두 줄을 한 덩어리로 세로 가운데에 쌓는다. 본문은 배지 가운데,
        // 작은 글씨는 본문 글자의 왼쪽 끝·가운데·오른쪽 끝에 맞춘다(ModernLabel·ModernButton과 같은 규칙).
        private void PaintStackedText(Graphics graphics, Rectangle pill, Size textSize, Size topSize, double widthRatio)
        {
            TextFormatFlags flags = TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding
                    | TextFormatFlags.EndEllipsis;
            int inner = Math.Max(0, pill.Width - (pillPaddingX * 2));
            int mainWidth = Math.Min(textSize.Width, inner);
            int mainLeft = pill.Left + ((pill.Width - mainWidth) / 2);
            int top = pill.Top + ((pill.Height - textSize.Height - topSize.Height) / 2);
            int smallWidth = Math.Min(topSize.Width, inner);
            int smallLeft = mainLeft;

            if (this.topLabelAlignment == HorizontalAlignment.Right)
            {
                smallLeft = mainLeft + mainWidth - smallWidth;
            }
            else if (this.topLabelAlignment == HorizontalAlignment.Center)
            {
                smallLeft = mainLeft + ((mainWidth - smallWidth) / 2);
            }

            smallLeft = Math.Max(pill.Left + pillPaddingX, Math.Min(smallLeft, pill.Right - pillPaddingX - smallWidth));

            using (Font smallFont = ModernFonts.Create(subLabelDiu, false, FontStyle.Regular))
            {
                ScaledTextRenderer.DrawText(
                    graphics, this.topLabel, smallFont,
                    new Rectangle(smallLeft, top, smallWidth, topSize.Height), this.pillForeground, flags, widthRatio);
            }

            ScaledTextRenderer.DrawText(
                graphics, this.Text, BadgeFont,
                new Rectangle(mainLeft, top + topSize.Height, mainWidth, textSize.Height), this.pillForeground, flags, widthRatio);
        }

        // 테두리를 도는 코멧 빛띠를 그린다 — 그리드 ModernSpinBorderControl과
        // 같은 시각 언어를 GDI+로 미러링. pill 외곽 경로를 평탄화한 뒤 **둘레를
        // 고정 간격으로 다시 나눠**, 머리(현재 위상)에서 뒤로 갈수록 투명해지는
        // 짧은 꼬리를 그린다.
        private void DrawSpinHighlight(Graphics graphics, Rectangle pill)
        {
            // 빛띠가 배지 밖으로 잘리지 않게 테두리 안쪽 1px 경로를 쓴다.
            Rectangle inner = Rectangle.Inflate(pill, -1, -1);

            if (inner.Width <= 0 || inner.Height <= 0)
            {
                return;
            }

            using (GraphicsPath path = this.shape == BadgeShape.Rounded
                    ? CreateRoundedPath(inner, roundedRadius)
                    : CreateCapsulePath(inner))
            {
                path.Flatten(null, 0.25f);

                // Flatten은 **곡선만** 잘게 쪼갠다 — 직선 변은 길이와 상관없이 한
                // 구간으로 남는다. 구간 하나에 알파 하나를 주던 예전 그림은 그래서
                // 긴 위/아래 변이 통째로 켜졌다 꺼져, 빛띠가 흐르지 않고 변에서
                // 변으로 튀었다(2026-08-11 지적: "끊겨서 돈다"). 여기서 둘레를
                // 다시 나눠 알파가 픽셀 단위로 이어지게 한다.
                PointF[] points = ResampleClosedPath(path.PathPoints, spinSampleStep);

                if (points.Length < 2)
                {
                    return;
                }

                float[] lengths = new float[points.Length];
                float total = 0f;

                for (int index = 0; index < points.Length; index++)
                {
                    PointF from = points[index];
                    PointF to = points[(index + 1) % points.Length];
                    float dx = to.X - from.X;
                    float dy = to.Y - from.Y;
                    lengths[index] = (float)Math.Sqrt((dx * dx) + (dy * dy));
                    total += lengths[index];
                }

                if (total <= 0f)
                {
                    return;
                }

                float head = (float)(SpinPhaseOf(this.spinClock.Elapsed.TotalMilliseconds) * total);
                float tail = total * 0.35f;
                Color accent = Modern.Lab.Theming.ModernTheme.Accent;
                float traveled = 0f;

                // 펜은 하나만 만들고 색만 바꾼다 — 나눈 구간마다 펜을 새로 만들면
                // 초당 수천 개를 버리게 된다.
                using (Pen pen = new Pen(accent, 1.6f))
                {
                    for (int index = 0; index < points.Length; index++)
                    {
                        float segment = lengths[index];

                        if (segment > 0f)
                        {
                            // 구간 중점의 "머리 뒤 거리" — 꼬리 범위 안이면 알파를 준다.
                            float middle = traveled + (segment / 2f);
                            float behind = (head - middle + total) % total;

                            if (behind <= tail)
                            {
                                int alpha = (int)(230 * (1f - (behind / tail)));

                                if (alpha > 8)
                                {
                                    pen.Color = System.Drawing.Color.FromArgb(alpha, accent);
                                    graphics.DrawLine(
                                            pen, points[index], points[(index + 1) % points.Length]);
                                }
                            }
                        }

                        traveled += segment;
                    }
                }
            }
        }

        /// <summary>
        /// 닫힌 폴리라인을 <paramref name="step"/> 간격으로 다시 나눈다 — 원래
        /// 모양(꼭짓점)은 그대로 두고 긴 변만 쪼갠다. 결과의 이웃한 두 점은 항상
        /// step 이내이므로 구간별 알파가 곧 부드러운 그라디언트가 된다.
        /// </summary>
        internal static PointF[] ResampleClosedPath(PointF[] points, float step)
        {
            if (points == null || points.Length < 2 || step <= 0f)
            {
                return points == null ? new PointF[0] : points;
            }

            List<PointF> result = new List<PointF>();

            for (int index = 0; index < points.Length; index++)
            {
                PointF from = points[index];
                PointF to = points[(index + 1) % points.Length];

                result.Add(from);

                float dx = to.X - from.X;
                float dy = to.Y - from.Y;
                float length = (float)Math.Sqrt((dx * dx) + (dy * dy));
                int pieces = (int)Math.Ceiling(length / step);

                // 아주 큰 배지에서도 선 개수가 폭주하지 않게 자른다.
                if (result.Count + pieces > spinSampleLimit)
                {
                    pieces = Math.Max(1, spinSampleLimit - result.Count);
                }

                for (int piece = 1; piece < pieces; piece++)
                {
                    float ratio = (float)piece / pieces;
                    result.Add(new PointF(from.X + (dx * ratio), from.Y + (dy * ratio)));
                }

                if (result.Count >= spinSampleLimit)
                {
                    break;
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 경과 시간(ms) → 회전 위상(0~1). 틱 수가 아니라 시간을 보므로 타이머가
        /// 틱을 걸러도 빛띠의 **속도**는 변하지 않는다 — 프레임만 줄어든다.
        /// </summary>
        internal static double SpinPhaseOf(double elapsedMilliseconds)
        {
            if (elapsedMilliseconds <= 0.0)
            {
                return 0.0;
            }

            return (elapsedMilliseconds % spinPeriodMs) / spinPeriodMs;
        }

        // 위상은 그릴 때 시계에서 읽는다 — 틱은 다시 그리라는 신호일 뿐이다.
        private void OnSpinTick(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        // SpinValues가 선언돼 있으면 현재 Text로 Spinning을 다시 판정한다 —
        // 값 판정은 강조 목록과 같은 규칙(세미콜론/콤마 분리, NormalizeValue).
        private void ApplySpinValues()
        {
            if (this.spinValues.Length == 0)
            {
                return;
            }

            this.Spinning = MatchesValueList(this.spinValues, this.Text);
        }

        // Spinning·표시 상태에 맞춰 타이머를 켜고 끈다 — 숨은 배지가 CPU를
        // 쓰지 않게 하고, 디자인 서피스에서는 돌리지 않는다.
        private void UpdateSpinTimer()
        {
            bool shouldRun = this.spinning && this.Visible && !this.DesignMode;

            if (shouldRun)
            {
                if (this.spinTimer == null)
                {
                    this.spinTimer = new Timer();

                    // WinForms 타이머는 시스템 틱(약 15.6ms) 경계에서만 깨어난다 —
                    // 33을 주면 다음 경계인 약 47ms(초당 21프레임)에 떨어진다.
                    // 30이면 약 31ms(초당 32프레임)로, 같은 비용에 더 매끄럽다.
                    this.spinTimer.Interval = 30;
                    this.spinTimer.Tick += this.OnSpinTick;
                }

                this.spinClock.Start();
                this.spinTimer.Start();
            }
            else if (this.spinTimer != null)
            {
                this.spinTimer.Stop();

                // 시계는 멈추기만 한다(초기화하지 않는다) — 다시 보일 때 빛띠가
                // 처음으로 튀지 않고 있던 자리에서 이어진다.
                this.spinClock.Stop();
            }
        }

        /// <summary>숨김/표시 전환에 맞춰 회전 타이머를 켜고 끈다.</summary>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            this.UpdateSpinTimer();
        }

        // 잘림 상태에 맞춰 툴팁을 적용/해제한다. SetToolTip은 같은 값이면
        // 다시 부르지 않는다 (OnPaint에서 호출되므로 중복 적용을 피한다).
        private void UpdateOverflowToolTip(bool truncated)
        {
            string wanted = truncated ? this.Text : string.Empty;

            if (string.Equals(wanted, this.appliedToolTipText, StringComparison.Ordinal))
            {
                return;
            }

            if (this.overflowToolTip == null)
            {
                this.overflowToolTip = new ToolTip();
            }

            // 빈 문자열 적용은 툴팁 제거와 동일하다.
            this.overflowToolTip.SetToolTip(this, wanted);
            this.appliedToolTipText = wanted;
        }

        /// <summary>잘림 안내 툴팁과 회전 타이머를 함께 정리한다.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.overflowToolTip != null)
                {
                    this.overflowToolTip.Dispose();
                    this.overflowToolTip = null;
                }

                if (this.spinTimer != null)
                {
                    this.spinTimer.Stop();
                    this.spinTimer.Dispose();
                    this.spinTimer = null;
                }
            }

            base.Dispose(disposing);
        }

        // 색 3경로를 우선순위(Color > Semantic > ColorValue)로 해석해 pill
        // 배경/글자색을 확정한다. 빈 값/파싱 불가는 중립 회색 배지로 폴백하며
        // 예외를 던지지 않는다.
        // 값이 선언 목록에 걸리는가 — 강조(AccentValues)와 회전(SpinValues)이
        // 함께 쓰는 판정. 그리드 배지의 BadgeAccentValues/BadgeSpinValues와 같은
        // 규칙(세미콜론/콤마 분리, Palette.NormalizeValue로 정규화).
        internal static bool MatchesValueList(string valueList, string value)
        {
            if (string.IsNullOrEmpty(valueList))
            {
                return false;
            }

            string normalized = Modern.Lab.Theming.Palette.NormalizeValue(value);

            if (normalized.Length == 0)
            {
                return false;
            }

            foreach (string part in valueList.Split(new char[] { ';', ',' }))
            {
                if (Modern.Lab.Theming.Palette.NormalizeValue(part) == normalized)
                {
                    return true;
                }
            }

            return false;
        }

        private void ResolvePillColors()
        {
            Color background;

            // 강조는 색 경로 셋보다 앞선다 — 배경까지 오류 색으로 바꿔야 빨간
            // 글자가 전 테마에서 읽힌다.
            if (MatchesValueList(this.accentValues, this.Text))
            {
                Color fill = Modern.Lab.Theming.ModernTheme.ErrorFill;
                this.pillBackground = fill;
                this.pillForeground = DeriveForeground(fill);
                return;
            }

            string resolved = this.colorText.Trim();

            if (resolved.Length == 0 && this.semantic != Modern.Lab.Theming.SemanticKind.None)
            {
                resolved = Modern.Lab.Theming.SemanticColors.Of(this.semantic);
            }

            if (resolved.Length == 0
                && Modern.Lab.Theming.Palette.NormalizeValue(this.colorValue).Length > 0)
            {
                resolved = Modern.Lab.Theming.Palette.FromValue(this.colorValue);
            }

            if (resolved.Length == 0 || !TryParseColor(resolved, out background))
            {
                this.pillBackground = NeutralBackground;
                this.pillForeground = NeutralText;
                return;
            }

            this.pillBackground = background;
            this.pillForeground = DeriveForeground(background);
        }

        private static bool TryParseColor(string text, out Color color)
        {
            try
            {
                color = ColorTranslator.FromHtml(text);
                return true;
            }
            catch (Exception)
            {
                color = System.Drawing.Color.Empty;
                return false;
            }
        }

        // 모서리만 둥근 사각 경로 (Radius.Sm과 동일한 인상).
        private static GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = Math.Min(radius * 2, Math.Min(bounds.Height, bounds.Width));

            if (diameter <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180f, 90f);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270f, 90f);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0f, 90f);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90f, 90f);
            path.CloseFigure();
            return path;
        }

        // 좌우가 반원인 캡슐 모양 경로 (Radius.Pill과 동일한 인상).
        private static GraphicsPath CreateCapsulePath(Rectangle bounds)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = Math.Min(bounds.Height, bounds.Width);

            if (diameter <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 90f, 180f);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270f, 180f);
            path.CloseFigure();
            return path;
        }

        // ===== 글자색 유도 (Wpf.Common.ChipColorHelper의 규칙을 GDI+로 미러링) =====
        // 배경색에서 글자색을 유도한다: 색상(Hue)은 유지하고 명도만 반대쪽으로.
        // - 밝은 배경 → 같은 색상의 진한 톤 (채도 보강 + 명도 0.30)
        // - 어두운 배경 → 같은 색상의 아주 밝은 톤 (명도 0.95)
        // - 무채색 배경 → 밝기에 따라 중립 진회색 또는 흰색
        // 유도 결과가 WCAG 4.5:1에 못 미치면(중간톤 배경) 검정/흰색 중 대비가
        // 큰 쪽으로 강등한다 — ChipColorHelper와 같은 규칙.
        private static Color DeriveForeground(Color background)
        {
            double hue;
            double saturation;
            double lightness;
            RgbToHsl(background, out hue, out saturation, out lightness);

            // 상대 휘도(0~1)로 밝은/어두운 배경을 판정한다.
            double luminance = ((0.2126 * background.R) + (0.7152 * background.G) + (0.0722 * background.B)) / 255.0;

            Color derived;

            if (saturation < 0.15)
            {
                // 진회색 값은 ModernTheme.NeutralText(라이트)와 같지만 **팔레트에서
                // 가져오면 안 된다** — 배지 글자색은 테마가 아니라 배지 배경에서
                // 유도되기 때문이다. 다크 테마에서 ModernTheme.NeutralText는
                // 밝은 회색(208,208,208)이므로, 밝은 회색 배지 위에 쓰면 대비가
                // 사라진다. 즉 의도된 리터럴이다 (design-notes 8-5절).
                derived = luminance < 0.5
                        ? System.Drawing.Color.White
                        : System.Drawing.Color.FromArgb(0x37, 0x41, 0x51);
            }
            else if (luminance < 0.5)
            {
                derived = HslToRgb(hue, saturation, 0.95);
            }
            else
            {
                derived = HslToRgb(hue, Math.Max(saturation, 0.55), 0.30);
            }

            return EnsureMinimumContrast(derived, background, hue, saturation, luminance < 0.5);
        }

        // 유도된 전경이 4.5:1(WCAG 일반 텍스트 기준)에 못 미치면 같은 색상의 톤을 대비가 닿을 때까지 민다 —
        // 밝은 배경이면 더 진하게(명도 0.30 → 0), 어두운 배경이면 더 밝게(0.95 → 1). 그래도 안 되면 검정/흰색 중
        // 대비가 큰 쪽 (ChipColorHelper.EnsureMinimumContrast 미러 — 바꿀 때 함께 바꾼다. 2026-10-20).
        private static Color EnsureMinimumContrast(
                Color derived, Color background, double hue, double saturation, bool darkBackground)
        {
            if (WcagContrast(derived, background) >= 4.5)
            {
                return derived;
            }

            double tone = Math.Max(saturation, 0.55);

            for (int step = 1; step <= 6; step++)
            {
                double lightness = darkBackground
                        ? Math.Min(1.0, 0.95 + (step * 0.01))
                        : Math.Max(0.0, 0.30 - (step * 0.05));
                Color candidate = HslToRgb(hue, tone, lightness);

                if (WcagContrast(candidate, background) >= 4.5)
                {
                    return candidate;
                }
            }

            Color black = System.Drawing.Color.FromArgb(0, 0, 0);
            Color white = System.Drawing.Color.FromArgb(255, 255, 255);

            return WcagContrast(black, background) >= WcagContrast(white, background)
                    ? black
                    : white;
        }

        // WCAG 대비율(1~21) — 감마 역보정(선형화)한 상대 휘도로 계산한다.
        private static double WcagContrast(Color first, Color second)
        {
            double firstLuminance = WcagLuminance(first);
            double secondLuminance = WcagLuminance(second);
            double lighter = Math.Max(firstLuminance, secondLuminance);
            double darker = Math.Min(firstLuminance, secondLuminance);

            return (lighter + 0.05) / (darker + 0.05);
        }

        private static double WcagLuminance(Color color)
        {
            return (0.2126 * LinearChannel(color.R))
                    + (0.7152 * LinearChannel(color.G))
                    + (0.0722 * LinearChannel(color.B));
        }

        private static double LinearChannel(byte value)
        {
            double channel = value / 255.0;

            return channel <= 0.03928
                    ? channel / 12.92
                    : Math.Pow((channel + 0.055) / 1.055, 2.4);
        }

        // RGB → HSL 변환. hue/saturation/lightness 모두 0~1 범위.
        private static void RgbToHsl(Color color, out double hue, out double saturation, out double lightness)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;
            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            lightness = (max + min) / 2.0;

            if (delta == 0.0)
            {
                hue = 0.0;
                saturation = 0.0;
                return;
            }

            if (lightness > 0.5)
            {
                saturation = delta / (2.0 - max - min);
            }
            else
            {
                saturation = delta / (max + min);
            }

            if (max == r)
            {
                hue = (((g - b) / delta) + (g < b ? 6.0 : 0.0)) / 6.0;
            }
            else if (max == g)
            {
                hue = (((b - r) / delta) + 2.0) / 6.0;
            }
            else
            {
                hue = (((r - g) / delta) + 4.0) / 6.0;
            }
        }

        // HSL → RGB 변환. hue/saturation/lightness 모두 0~1 범위.
        private static Color HslToRgb(double hue, double saturation, double lightness)
        {
            double r;
            double g;
            double b;

            if (saturation == 0.0)
            {
                r = lightness;
                g = lightness;
                b = lightness;
            }
            else
            {
                double q;

                if (lightness < 0.5)
                {
                    q = lightness * (1.0 + saturation);
                }
                else
                {
                    q = lightness + saturation - (lightness * saturation);
                }

                double p = (2.0 * lightness) - q;
                r = HueToRgbChannel(p, q, hue + (1.0 / 3.0));
                g = HueToRgbChannel(p, q, hue);
                b = HueToRgbChannel(p, q, hue - (1.0 / 3.0));
            }

            return System.Drawing.Color.FromArgb(
                (int)Math.Round(r * 255.0),
                (int)Math.Round(g * 255.0),
                (int)Math.Round(b * 255.0));
        }

        // HSL 보조: 색상 위치 t에 해당하는 채널 값(0~1)을 구한다.
        private static double HueToRgbChannel(double p, double q, double t)
        {
            if (t < 0.0)
            {
                t = t + 1.0;
            }

            if (t > 1.0)
            {
                t = t - 1.0;
            }

            if (t < 1.0 / 6.0)
            {
                return p + ((q - p) * 6.0 * t);
            }

            if (t < 1.0 / 2.0)
            {
                return q;
            }

            if (t < 2.0 / 3.0)
            {
                return p + ((q - p) * ((2.0 / 3.0) - t) * 6.0);
            }

            return p;
        }
    }
}
