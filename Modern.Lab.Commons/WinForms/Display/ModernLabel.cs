using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.WinForms.Rendering;

namespace Modern.Lab.WinForms.Controls.Display
{
    /// <summary>
    /// System.Windows.Forms.Label의 드롭인 대체 컨트롤.
    ///
    /// 원래 WPF ModernLabelControl을 ElementHost로 호스팅했지만, 라벨은 한 폼에
    /// 수십 개씩 놓이는 컨트롤이라 개당 HWND + WPF 컴포지션 비용이 폼 리사이즈를
    /// 눈에 띄게 무겁게 만들었다(실측: Item History 폼 리사이즈 1스텝 ~556ms).
    /// 그래서 ModernGroupBox/ModernCardPanel과 같은 방식으로 디자인 토큰을
    /// 미러링해 **GDI+로 직접 그리는 순수 WinForms 컨트롤**로 전환했다.
    /// 공개 API(Text/Kind/Required/TitleBar)와 시각 결과는 WPF 버전과 동일하다.
    ///
    /// 호환 멤버: Text(override, localizable), Enabled.
    /// Label과 달리 AutoSize가 없으므로 컨트롤에 명시적 크기를 지정한다
    /// (긴 텍스트는 말줄임표로 잘린다).
    /// Hyperlink=true면 LinkLabel 대체 — 액센트색+밑줄+손 커서로 그려지고
    /// 클릭은 표준 Click 이벤트로 받는다 (LinkClicked 아님).
    /// </summary>
    [ToolboxItem(true)]
    [DesignerCategory("Code")]
    public class ModernLabel : Control
    {
        // 색은 중앙 팔레트(ModernTheme)에서 읽는다 — 라이트/다크 테마에 따라 값이 바뀐다.
        // (GDI+는 XAML 리소스를 읽을 수 없으므로 Themes/Tokens.xaml 값을 팔레트로 미러링.)
        private static Color TextPrimaryColor { get { return Modern.Lab.Theming.ModernTheme.TextPrimary; } }
        private static Color TextSecondaryColor { get { return Modern.Lab.Theming.ModernTheme.TextSecondary; } }
        private static Color DisabledTextColor { get { return Modern.Lab.Theming.ModernTheme.DisabledText; } }
        private static Color AccentColor { get { return Modern.Lab.Theming.ModernTheme.Accent; } }
        private static Color AccentHoverColor
        {
            get
            {
                return Modern.Lab.Theming.ModernTokenColors.Get(
                        "Brush.AccentHover", Modern.Lab.Theming.ModernTheme.Accent);
            }
        }
        private static Color RequiredColor { get { return Modern.Lab.Theming.ModernTheme.RequiredRed; } }

        // 타입 램프는 ModernFonts(Tokens.xaml Font.Size.* 미러) 한 곳에서 온다 —
        // 서체 이름과 pt를 여기서 다시 적지 않는다.
        // 구조 요소(Title/Label)는 SemiBold, 본문/도움말은 Regular.
        private static readonly Font BodyFont = ModernFonts.Body;
        private static readonly Font TitleFont = ModernFonts.Title;
        private static readonly Font LabelFont = ModernFonts.BodyStrong;
        private static readonly Font HelperFont = ModernFonts.Body;

        // 타이틀 바(액센트 세로 막대) 치수: 폭 3 + 오른쪽 여백 8 (WPF 버전과 동일).
        private const int titleBarWidth = 3;
        private const int titleBarGap = 8;

        // 필수 별표 앞 여백 (WPF 버전 Margin 3,0,0,0과 동일).
        private const int requiredGap = 3;

        // 남는 세로 여백이 홀수일 때 글자를 1px 내려 그린다 — 같은 줄에 놓이는 WPF 필드와
        // 높이를 맞추기 위해서다. GDI의 VerticalCenter는 남는 여백을 내림하고 WPF는 올림해
        // 반씩 남는 자리에서 서로 반대로 간다. 실측(2026-09-22, 96dpi · 32px 칸 · "Request Id"):
        // 대문자 밴드 중심이 라벨 15.0 · ModernTextBox 16.0 — 라벨이 1px 위였고, 조회조건처럼
        // 라벨과 플레이스홀더가 같은 말을 나란히 놓는 자리에서 그 차이가 드러난다.
        // 짝수(나누어떨어지는) 높이에서는 둘이 이미 같은 자리라 보정하지 않는다 — 상수로 박으면
        // 배율이 바뀌어 여백이 짝수가 되는 순간 이번에는 반대로 1px 어긋난다.
        // 홀짝 판정의 기준은 GDI가 DT_VCENTER에서 쓰는 줄 높이다 — Font.Height(줄 간격까지 더한 값)나
        // NoPadding 측정값(잉크 높이)이 아니라 TextRenderer 기본 측정의 높이다(96dpi Segoe UI 9pt: 각각 16 · 8 · 15).
        private static int TextTopNudge(Control label, Font font)
        {
            return ((label.Height - LineHeight(font)) & 1) == 1 ? 1 : 0;
        }

        // 줄 높이는 폰트당 상수라 한 번만 잰다 — 램프 폰트가 정적(4종 + 밑줄 변형)이라
        // 캐시는 그 수를 넘지 않는다(밑줄 폰트 캐시와 같은 근거).
        private static readonly System.Collections.Generic.Dictionary<Font, int> lineHeightCache =
                new System.Collections.Generic.Dictionary<Font, int>();

        private static int LineHeight(Font font)
        {
            int height;

            if (!lineHeightCache.TryGetValue(font, out height))
            {
                height = TextRenderer.MeasureText("H", font).Height;
                lineHeightCache[font] = height;
            }

            return height;
        }

        // 하이퍼링크 모드의 밑줄 폰트 캐시 — 원본 폰트(정적 램프)당 하나만 만든다.
        private static readonly System.Collections.Generic.Dictionary<Font, Font> underlineFontCache =
                new System.Collections.Generic.Dictionary<Font, Font>();

        private LabelKind kind;
        private bool required;
        private bool titleBar;
        private double fontWidthRatio;
        private bool hyperlink;
        private bool hovered;
        private string captionMember = string.Empty;
        private string topLabel = string.Empty;
        private string bottomLabel = string.Empty;
        private System.Windows.Forms.HorizontalAlignment textAlignment = System.Windows.Forms.HorizontalAlignment.Left;
        private System.Windows.Forms.HorizontalAlignment topLabelAlignment = System.Windows.Forms.HorizontalAlignment.Left;
        private System.Windows.Forms.HorizontalAlignment bottomLabelAlignment = System.Windows.Forms.HorizontalAlignment.Left;
        // 위·아래 작은 글씨 크기(DIU) — 버튼·그리드 헤더·그룹 제목의 작은 글씨와 같은 10(토큰 Font.Size.SubLabel).
        private const double subLabelDiu = 10d;

        /// <summary>적절한 기본 크기로 컨트롤을 생성한다.</summary>
        public ModernLabel()
        {
            this.SetStyle(
                ControlStyles.UserPaint
                | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw
                | ControlStyles.SupportsTransparentBackColor,
                true);

            this.Size = new Size(120, 24);
            this.BackColor = Color.Transparent;
            this.kind = LabelKind.Body;
            this.required = false;
            this.titleBar = false;
            this.Text = "Label";
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

        /// <summary>Kind가 Title일 때 텍스트 왼쪽에 액센트색 세로 타이틀 바를 표시한다.</summary>
        [Category("Modern")]
        [Description("Show an accent vertical title bar on the left when Kind is Title")]
        [DefaultValue(false)]
        public bool TitleBar
        {
            get
            {
                return this.titleBar;
            }
            set
            {
                this.titleBar = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 하이퍼링크 모드 — 텍스트를 액센트색 + 밑줄로 그리고 손 커서를 쓴다
        /// (호버 시 한 단계 진한 액센트). 클릭은 표준 Click 이벤트로 받는다 —
        /// "Lot ID 클릭 → 이력 화면" 같은 셀 값 네비게이션 관례의 라벨판이며
        /// LinkLabel을 대체한다 (LinkClicked 대신 Click 사용).
        /// </summary>
        [Category("Modern")]
        [Description("Hyperlink mode — accent color + underline, hand cursor, click raises the Click event")]
        [DefaultValue(false)]
        public bool Hyperlink
        {
            get
            {
                return this.hyperlink;
            }
            set
            {
                this.hyperlink = value;
                this.Cursor = value ? Cursors.Hand : Cursors.Default;
                this.Invalidate();
            }
        }

        /// <summary>텍스트 뒤에 빨간 별표를 표시한다(필수 필드 표시).</summary>
        [Category("Modern")]
        [Description("Append a required marker (red asterisk) after the text")]
        [DefaultValue(false)]
        public bool Required
        {
            get
            {
                return this.required;
            }
            set
            {
                this.required = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 본문 위의 작은 글씨(비어 있으면 없음) — 그리드 헤더·버튼과 같은 10 크기, 보조 텍스트 색.
        /// 위·아래 중 하나라도 있으면 세 줄을 한 덩어리로 세로 가운데에 쌓는다. 본문은 <see cref="TextAlignment"/>, 작은 글씨는
        /// <see cref="TopLabelAlignment"/>·<see cref="BottomLabelAlignment"/>(본문 글자 기준)로 정렬된다.
        /// </summary>
        [Category("Modern")]
        [Description("Small text above the main text (empty = none)")]
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
                this.Invalidate();
            }
        }

        /// <summary>본문 아래의 작은 글씨(비어 있으면 없음) — <see cref="TopLabel"/>과 같은 규칙.</summary>
        [Category("Modern")]
        [Description("Small text below the main text (empty = none)")]
        [Localizable(true)]
        [DefaultValue("")]
        public string BottomLabel
        {
            get
            {
                return this.bottomLabel;
            }
            set
            {
                this.bottomLabel = value ?? string.Empty;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 본문(과 필수 별표)의 가로 정렬 — 기본 Left. Right 는 컨트롤 오른쪽 끝에 붙이고(조회 조건에서 입력 상자 바로 앞에 라벨을 둘 때),
        /// Center 는 가운데다. 위·아래 작은 글씨는 이 정렬이 아니라 <see cref="TopLabelAlignment"/>·<see cref="BottomLabelAlignment"/>로
        /// 본문 글자를 기준으로 정렬된다.
        /// </summary>
        [Category("Modern")]
        [Description("Horizontal alignment of the main text (Left default)")]
        [DefaultValue(System.Windows.Forms.HorizontalAlignment.Left)]
        public System.Windows.Forms.HorizontalAlignment TextAlignment
        {
            get
            {
                return this.textAlignment;
            }
            set
            {
                this.textAlignment = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 위 작은 글씨의 정렬 — 본문 글자 기준이다(버튼·그리드 헤더와 같은 규칙, 2026-09-30): Left 는 본문 왼쪽 끝, Right 는 본문 오른쪽 끝,
        /// Center 는 본문 가운데에 맞추고 본문보다 길면 기준을 지킨 채 밖으로 나간다(컨트롤 밖으로는 못 나가 그때만 말줄임). 기본 Left(기존 모양).
        /// </summary>
        [Category("Modern")]
        [Description("Alignment of the top small text relative to the main text (Left default)")]
        [DefaultValue(System.Windows.Forms.HorizontalAlignment.Left)]
        public System.Windows.Forms.HorizontalAlignment TopLabelAlignment
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

        /// <summary>아래 작은 글씨의 정렬 — <see cref="TopLabelAlignment"/>와 같은 규칙.</summary>
        [Category("Modern")]
        [Description("Alignment of the bottom small text relative to the main text (Left default)")]
        [DefaultValue(System.Windows.Forms.HorizontalAlignment.Left)]
        public System.Windows.Forms.HorizontalAlignment BottomLabelAlignment
        {
            get
            {
                return this.bottomLabelAlignment;
            }
            set
            {
                this.bottomLabelAlignment = value;
                this.Invalidate();
            }
        }

        /// <summary>표시할 텍스트.</summary>
        [Category("Modern")]
        [Description("Text to display")]
        [Browsable(true)]
        [Localizable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue("Label")]
        public override string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        /// <summary>
        /// 캡션을 용어사전에서 읽어 올 컬럼/속성 이름 (2026-08-29 추가). 비어 있으면(기본)
        /// <see cref="Text"/>를 그대로 표시하고, 지정하면 그리드 컬럼과 같은 규칙
        /// (<c>GridCaptionCatalog</c> → <c>ModernDataGridColumn.CaptionResolver</c> →
        /// 폴백 "EQP_ID" → "Eqp Id")으로 해석한 캡션을 <b>그릴 때</b> 쓴다 — Text 값은
        /// 건드리지 않으므로 디자이너 직렬화 순서와 무관하고, 앱 시작 후 등록된 사전을
        /// 그대로 따른다. ModernDetailTable의 캡션 셀처럼 라벨이 곧 캡션인 자리용.
        /// </summary>
        [Category("Modern")]
        [Description("Column/property name whose dictionary caption is displayed instead of Text (empty = use Text)")]
        [DefaultValue("")]
        public string CaptionMember
        {
            get
            {
                return this.captionMember;
            }
            set
            {
                this.captionMember = value ?? string.Empty;
                this.Invalidate();
            }
        }

        // 그릴 텍스트 — CaptionMember가 있으면 사전 캡션, 없으면 Text.
        private string DisplayText
        {
            get
            {
                if (this.captionMember.Length == 0)
                {
                    return this.Text;
                }

                return Modern.Lab.Controls.Wpf.Data.GridCaptionCatalog.PlainCaption(
                        Modern.Lab.Controls.Wpf.Data.GridCaptionCatalog.ResolveTopLabel(this.captionMember),
                        Modern.Lab.Controls.Wpf.Data.ModernDataGridColumn.ResolveCaption(this.captionMember),
                        Modern.Lab.Controls.Wpf.Data.GridCaptionCatalog.ResolveBottomLabel(this.captionMember));
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
                this.Invalidate();
            }
        }

        /// <summary>타이포그래피 역할 (Body/Title/Label/Helper).</summary>
        [Category("Modern")]
        [Description("Typography role (Body/Title/Label/Helper)")]
        [DefaultValue(LabelKind.Body)]
        public LabelKind Kind
        {
            get
            {
                return this.kind;
            }
            set
            {
                this.kind = value;
                this.Invalidate();
            }
        }

        /// <summary>텍스트가 바뀌면 다시 그린다.</summary>
        /// <summary>
        /// 글자가 다 보이는 데 필요한 높이 — 본문 한 줄에 <see cref="TopLabel"/>·<see cref="BottomLabel"/>이 있으면 그 작은 줄만큼 더한다.
        /// 재기만 한다 — 컨트롤 크기나 WinForms 자동 배치(<c>GetPreferredSize</c>)에는 관여하지 않으므로, 라벨을 행에 놓는 쪽
        /// (기준정보 편집기)이 이 값을 읽어 행 높이를 직접 정한다(2026-10-04).
        /// </summary>
        /// <summary>
        /// 글자가 다 보이는 데 필요한 폭 — 본문과 <see cref="TopLabel"/>·<see cref="BottomLabel"/> 중 가장 넓은 것. 재기만 하고 컨트롤 크기는 바꾸지 않는다
        /// (조회 띠가 오른쪽 정렬 라벨 앞에 체크박스를 글자 바로 왼쪽에 붙일 때 쓴다, 2026-11-02).
        /// </summary>
        public int MeasureTextWidth()
        {
            int width = TextRenderer.MeasureText(this.Text ?? string.Empty, this.GetKindFont()).Width;

            if (this.topLabel.Length > 0 || this.bottomLabel.Length > 0)
            {
                using (Font smallFont = ModernFonts.Create(subLabelDiu, false, FontStyle.Regular))
                {
                    if (this.topLabel.Length > 0)
                    {
                        width = Math.Max(width, TextRenderer.MeasureText(this.topLabel, smallFont).Width);
                    }

                    if (this.bottomLabel.Length > 0)
                    {
                        width = Math.Max(width, TextRenderer.MeasureText(this.bottomLabel, smallFont).Width);
                    }
                }
            }

            return width;
        }

        public int MeasureTextHeight()
        {
            int height = LineHeight(this.GetKindFont());

            if (this.topLabel.Length > 0 || this.bottomLabel.Length > 0)
            {
                using (Font smallFont = ModernFonts.Create(subLabelDiu, false, FontStyle.Regular))
                {
                    int smallHeight = TextRenderer.MeasureText("H", smallFont).Height;
                    height = height + (this.topLabel.Length > 0 ? smallHeight : 0) + (this.bottomLabel.Length > 0 ? smallHeight : 0);
                }
            }

            return height;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            this.Invalidate();
        }

        /// <summary>Enabled 변경 시 색이 바뀌므로 다시 그린다.</summary>
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            this.Invalidate();
        }

        /// <summary>하이퍼링크 모드의 호버 색 전환.</summary>
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            if (this.hyperlink)
            {
                this.hovered = true;
                this.Invalidate();
            }
        }

        /// <summary>하이퍼링크 모드의 호버 색 해제.</summary>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (this.hovered)
            {
                this.hovered = false;
                this.Invalidate();
            }
        }

        /// <summary>토큰 타입 램프에 따라 텍스트(+타이틀 바/필수 별표)를 그린다.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Font font = this.GetKindFont();
            Color color = this.Enabled ? this.GetKindColor() : DisabledTextColor;
            double widthRatio = Modern.Lab.Theming.ModernTheme.ResolveFontWidthRatio(this.fontWidthRatio);
            int textTopNudge = TextTopNudge(this, font);
            int left = 0;

            // 세로 타이틀 바: Kind=Title이면서 TitleBar=true일 때만.
            if (this.titleBar && this.kind == LabelKind.Title)
            {
                int barHeight = font.Height - 2;
                int barTop = textTopNudge + (this.Height - barHeight) / 2;

                using (SolidBrush accentBrush = new SolidBrush(AccentColor))
                {
                    e.Graphics.FillRectangle(accentBrush, 0, barTop, titleBarWidth, barHeight);
                }

                left = titleBarWidth + titleBarGap;
            }

            // 위·아래 작은 글씨가 있으면 세 줄 경로로 그린다 — 필수 별표는 본문 줄 뒤에만 붙는다.
            if (this.topLabel.Length > 0 || this.bottomLabel.Length > 0)
            {
                this.PaintStackedLines(e.Graphics, font, color, widthRatio, left);
                return;
            }

            // 필수 별표가 있으면 그 폭만큼 텍스트 영역을 줄여 별표가 잘리지 않게 한다.
            Size starSize = Size.Empty;

            if (this.required)
            {
                starSize = ScaledTextRenderer.MeasureText(
                    e.Graphics, "*", LabelFont, Size.Empty, textFlags, widthRatio);
            }

            int available = Math.Max(0, this.Width - left - starSize.Width - (this.required ? requiredGap : 0));
            string displayText = this.DisplayText;
            Size textSize = ScaledTextRenderer.MeasureText(
                e.Graphics, displayText, font, Size.Empty, textFlags, widthRatio);
            int textWidth = Math.Min(textSize.Width, available);
            int textLeft = AlignedLeft(
                left, this.Width - left, textWidth + starSize.Width + (this.required ? requiredGap : 0), this.textAlignment);
            Rectangle textBounds = new Rectangle(textLeft, textTopNudge, textWidth, this.Height);

            ScaledTextRenderer.DrawText(
                e.Graphics, displayText, font, textBounds, color,
                textFlags | TextFormatFlags.EndEllipsis, widthRatio);

            // 필수 별표: 실제 그려진 텍스트 폭 바로 뒤 (텍스트가 잘리면 영역 끝).
            if (this.required)
            {
                int starLeft = textLeft + textWidth + requiredGap;

                ScaledTextRenderer.DrawText(
                    e.Graphics, "*", LabelFont,
                    new Rectangle(starLeft, textTopNudge, starSize.Width + 2, this.Height),
                    RequiredColor, textFlags, widthRatio);
            }
        }

        // [위 작은 글씨][본문][아래 작은 글씨]를 한 덩어리로 높이 안에 세로 가운데로 쌓는다. 본문은 TextAlignment 대로 놓이고 작은 글씨는
        // 본문 글자의 왼쪽 끝·오른쪽 끝·가운데를 기준으로 정렬된다(버튼·그리드 헤더와 같은 규칙) — 본문보다 길면 기준을 지킨 채 밖으로
        // 나가되 컨트롤 밖으로는 못 나가므로 그때만 말줄임한다. 타이틀 바가 있으면 그 오른쪽부터다. 작은 글씨는 보조 텍스트 색, 비활성이면 비활성 색이다.
        private void PaintStackedLines(Graphics graphics, Font font, Color color, double widthRatio, int left)
        {
            const TextFormatFlags lineFlags = TextFormatFlags.SingleLine
                    | TextFormatFlags.Left
                    | TextFormatFlags.NoPrefix
                    | TextFormatFlags.NoPadding;
            Color smallColor = this.Enabled ? TextSecondaryColor : DisabledTextColor;
            string mainText = this.DisplayText;
            int width = Math.Max(0, this.Width - left);

            using (Font smallFont = ModernFonts.Create(subLabelDiu, false, FontStyle.Regular))
            {
                Size topSize = this.topLabel.Length > 0
                        ? ScaledTextRenderer.MeasureText(graphics, this.topLabel, smallFont, Size.Empty, lineFlags, widthRatio)
                        : Size.Empty;
                Size bottomSize = this.bottomLabel.Length > 0
                        ? ScaledTextRenderer.MeasureText(graphics, this.bottomLabel, smallFont, Size.Empty, lineFlags, widthRatio)
                        : Size.Empty;
                Size mainSize = ScaledTextRenderer.MeasureText(graphics, mainText, font, Size.Empty, lineFlags, widthRatio);
                Size starSize = this.required
                        ? ScaledTextRenderer.MeasureText(graphics, "*", LabelFont, Size.Empty, lineFlags, widthRatio)
                        : Size.Empty;
                int starPart = starSize.Width + (this.required ? requiredGap : 0);
                int mainWidth = Math.Min(mainSize.Width, Math.Max(0, width - starPart));
                int mainLeft = AlignedLeft(left, width, mainWidth + starPart, this.textAlignment);
                int y = Math.Max(0, (this.Height - topSize.Height - mainSize.Height - bottomSize.Height) / 2);

                if (topSize.Height > 0)
                {
                    this.PaintSubLabel(graphics, this.topLabel, smallFont, smallColor, topSize, y, left, width,
                            mainLeft, mainWidth, this.topLabelAlignment, lineFlags, widthRatio);
                    y = y + topSize.Height;
                }

                Rectangle mainBounds = new Rectangle(mainLeft, y, mainWidth, mainSize.Height);
                ScaledTextRenderer.DrawText(
                        graphics, mainText, font, mainBounds, color, lineFlags | TextFormatFlags.EndEllipsis, widthRatio);

                if (this.required)
                {
                    int starLeft = mainLeft + mainWidth + requiredGap;
                    ScaledTextRenderer.DrawText(
                            graphics, "*", LabelFont, new Rectangle(starLeft, y, starSize.Width + 2, mainSize.Height),
                            RequiredColor, lineFlags, widthRatio);
                }

                y = y + mainSize.Height;

                if (bottomSize.Height > 0)
                {
                    this.PaintSubLabel(graphics, this.bottomLabel, smallFont, smallColor, bottomSize, y, left, width,
                            mainLeft, mainWidth, this.bottomLabelAlignment, lineFlags, widthRatio);
                }
            }
        }

        // 작은 글씨 한 줄 — 본문 글자[mainLeft, mainLeft+mainWidth]를 기준으로 놓고, 컨트롤 영역[areaLeft, areaLeft+areaWidth]을 벗어나는 만큼만 잘라 말줄임한다.
        private void PaintSubLabel(
                Graphics graphics, string text, Font font, Color color, Size size, int top, int areaLeft, int areaWidth,
                int mainLeft, int mainWidth, System.Windows.Forms.HorizontalAlignment alignment, TextFormatFlags flags, double widthRatio)
        {
            int x = alignment == System.Windows.Forms.HorizontalAlignment.Right ? mainLeft + mainWidth - size.Width
                    : alignment == System.Windows.Forms.HorizontalAlignment.Center ? mainLeft + (mainWidth - size.Width) / 2
                    : mainLeft;
            x = Math.Max(areaLeft, x);
            int drawWidth = Math.Max(0, Math.Min(size.Width, areaLeft + areaWidth - x));
            ScaledTextRenderer.DrawText(
                    graphics, text, font, new Rectangle(x, top, drawWidth, size.Height), color,
                    flags | TextFormatFlags.EndEllipsis, widthRatio);
        }

        // 정렬에 따른 덩어리의 왼쪽 — Left 는 시작점, Right 는 영역 오른쪽 끝에 붙이고, Center 는 가운데다. 덩어리가 영역보다 넓으면 시작점이다.
        private static int AlignedLeft(int start, int available, int blockWidth, System.Windows.Forms.HorizontalAlignment alignment)
        {
            int slack = Math.Max(0, available - blockWidth);
            return alignment == System.Windows.Forms.HorizontalAlignment.Right ? start + slack
                    : alignment == System.Windows.Forms.HorizontalAlignment.Center ? start + slack / 2
                    : start;
        }

        // 공통 텍스트 플래그: 한 줄, 세로 중앙, 니모닉(&) 해석 없음.
        // NoPadding: GDI 기본 좌우 여백을 빼 WPF TextBlock과 같은 폭으로 그린다
        // (없으면 같은 크기의 컨트롤에서 WPF에는 들어가던 텍스트가 말줄임된다).
        private const TextFormatFlags textFlags =
            TextFormatFlags.SingleLine
            | TextFormatFlags.VerticalCenter
            | TextFormatFlags.Left
            | TextFormatFlags.NoPrefix
            | TextFormatFlags.NoPadding;

        private Font GetKindFont()
        {
            Font baseFont;

            switch (this.kind)
            {
                case LabelKind.Title:
                    baseFont = TitleFont;
                    break;
                case LabelKind.Label:
                    baseFont = LabelFont;
                    break;
                case LabelKind.Helper:
                    baseFont = HelperFont;
                    break;
                default:
                    baseFont = BodyFont;
                    break;
            }

            // 하이퍼링크는 항상 밑줄 (LinkLabel 기본 AlwaysUnderline과 동일 체감).
            return this.hyperlink ? GetUnderlinedFont(baseFont) : baseFont;
        }

        // 원본 폰트당 밑줄 변형을 하나만 만들어 재사용한다 — 램프 폰트가
        // 정적(BodyFont 등 4종)이라 캐시는 그 수를 넘지 않는다.
        private static Font GetUnderlinedFont(Font source)
        {
            Font underlined;

            if (!underlineFontCache.TryGetValue(source, out underlined))
            {
                underlined = new Font(source, source.Style | FontStyle.Underline);
                underlineFontCache[source] = underlined;
            }

            return underlined;
        }

        private Color GetKindColor()
        {
            // 하이퍼링크는 역할 색보다 우선 — 액센트(호버 시 한 단계 진하게).
            if (this.hyperlink)
            {
                return this.hovered ? AccentHoverColor : AccentColor;
            }

            switch (this.kind)
            {
                case LabelKind.Label:
                case LabelKind.Helper:
                    return TextSecondaryColor;
                default:
                    return TextPrimaryColor;
            }
        }
    }
}
