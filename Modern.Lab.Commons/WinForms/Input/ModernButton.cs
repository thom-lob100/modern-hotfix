using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Modern.Lab.Controls.Wpf.Input;
using Modern.Lab.Theming;
using Modern.Lab.WinForms.Rendering;

namespace Modern.Lab.WinForms.Controls.Input
{
    /// <summary>
    /// System.Windows.Forms.Button의 드롭인 대체 컨트롤.
    ///
    /// 원래 WPF ModernButtonControl을 ElementHost로 호스팅했지만, 버튼은 한 폼에
    /// 여러 개(실측: 샘플 14개 폼에 41개, 폼당 1~8개) 놓이는 컨트롤이라 개당
    /// HWND + WPF 컴포지션 비용이 폼 오픈과 리사이즈에 그대로 누적됐다
    /// (섬 1개당 생성 ~6~10ms, 리사이즈 스텝당 ~20ms — docs/design-notes.md 7절).
    /// 그래서 ModernLabel·ModernStatusBadge와 같은 방식으로 **GDI+로 직접 그리는
    /// 순수 WinForms 컨트롤**로 전환했다(7-1절 후보 ①). 공개 API와 시각 결과는
    /// WPF 버전과 동일하다.
    ///
    /// 색은 <see cref="ModernTokenColors"/>로 **WPF 컨트롤과 같은 토큰 사전에서**
    /// 읽는다 — 버튼 하나가 쓰는 브러시 토큰이 27종이고 테마 오버라이드들이 그것을 사실상
    /// 전부 오버라이드하므로, 팔레트를 손으로 미러하면 189개 값이 중복되고
    /// 드리프트가 생긴다. WPF 버튼은 그리드 필터 팝업에 계속 쓰이므로 그 드리프트는
    /// 같은 화면에서 눈에 보인다.
    ///
    /// 호환 멤버: Text(override, localizable), Click, Enabled,
    /// <b><see cref="IButtonControl"/>(DialogResult · PerformClick · NotifyDefault)</b>.
    /// Text는 `new`로 숨기지 않고 Control.Text를 override한다 — 숨기면
    /// Localizable 폼에서 ComponentResourceManager.ApplyResources가 깨지고,
    /// Control 참조로 Text를 할당하는 호스트 코드도 깨진다
    /// (docs/design-notes.md 3절).
    ///
    /// <b><see cref="IButtonControl"/>을 구현하는 이유(2026-08-04)</b>:
    /// <see cref="Form.AcceptButton"/>/<see cref="Form.CancelButton"/>의 타입이
    /// <see cref="IButtonControl"/>이라, 구현하지 않으면 기존 폼의
    /// <c>this.AcceptButton = this.btnOk;</c> 한 줄이 **컴파일되지 않는다** —
    /// 타입 선언만 바꾸면 되는 drop-in 계약(design-notes 6-1 룰 1)이 깨지는
    /// 자리였다. 구현 후에는 Enter/Esc를 폼이 처리하므로 화면마다 있던
    /// <c>ProcessDialogKey</c> 재정의가 필요 없다.
    /// </summary>
    [ToolboxItem(true)]
    [DesignerCategory("Code")]
    [DefaultEvent("Click")]
    public class ModernButton : Control, IButtonControl
    {
        // ===== 치수 (Tokens.xaml 미러) =====

        // Radius.Md — 컨트롤 모서리 반경.
        private const int cornerRadius = 4;

        // Pad.Button — 좌우 여백 16.
        private const int horizontalPadding = 16;

        // 위·아래 작은 라벨을 Left·Right 로 정렬할 때의 안쪽 여백(px) — 본문 좌우 여백의 절반이라 캡션보다 바깥에 선다.
        // 아이콘과 캡션 사이 간격 (WPF: IconText Margin 0,0,8,0).
        private const int iconGap = 8;

        // TopLabel·BottomLabel 글자 크기(DIU) — WPF: FontSize 10(토큰 Font.Size.SubLabel 과 같다, 2026-09-29).
        private const double topLabelDiu = 10d;

        // TopLabel 불투명도 — WPF: Opacity 0.8. GDI 텍스트는 알파를 무시하므로
        // 배경과 미리 섞는다.
        private const double topLabelOpacity = 0.8d;

        // 기본 버튼 링의 불투명도 — 캡션색을 배경과 섞어 은근한 링을 만든다.
        private const double defaultRingOpacity = 0.55d;

        // ===== 상태 =====

        private ButtonKind kind;
        // 우상단 카운트 배지 글자 크기(DIU)와 세로 높이 — 상단 라벨과 같은 축소 램프.
        private const double badgeDiu = 8d;
        private const int badgeHeight = 15;

        private string iconGlyph;
        private string topLabel;
        private string bottomLabel;
        private HorizontalAlignment topLabelAlignment;
        private HorizontalAlignment bottomLabelAlignment;
        private double glyphSize;
        private double fontWidthRatio;
        private int badgeCount;
        private bool hovered;
        private bool pressed;

        // IButtonControl — 폼이 이 버튼을 기본 버튼으로 지정하면(AcceptButton)
        // NotifyDefault(true)가 온다. 기본 버튼은 테두리 안쪽에 링을 하나 더 그려
        // "Enter가 여기로 간다"를 보이게 한다(WinForms 기본 버튼 관례).
        private DialogResult dialogResult;
        private bool defaultButton;

        /// <summary>적절한 기본 크기로 컨트롤을 생성한다.</summary>
        public ModernButton()
        {
            this.SetStyle(
                    ControlStyles.AllPaintingInWmPaint
                    | ControlStyles.OptimizedDoubleBuffer
                    | ControlStyles.UserPaint
                    | ControlStyles.ResizeRedraw
                    | ControlStyles.Selectable
                    | ControlStyles.SupportsTransparentBackColor,
                    true);

            this.kind = ButtonKind.Primary;
            this.iconGlyph = string.Empty;
            this.topLabel = string.Empty;
            this.bottomLabel = string.Empty;
            this.topLabelAlignment = HorizontalAlignment.Center;
            this.bottomLabelAlignment = HorizontalAlignment.Center;
            this.glyphSize = 0d;
            this.fontWidthRatio = 0d;
            this.dialogResult = DialogResult.None;
            this.defaultButton = false;

            this.Size = new Size(120, 32);
            this.TabStop = true;
            this.BackColor = Color.Transparent;
            base.Text = "Button";
            this.Cursor = Cursors.Hand;
        }

        /// <summary>버튼에 표시되는 캡션.</summary>
        [Category("Modern")]
        [Description("Text shown on the button")]
        [Browsable(true)]
        [Localizable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue("Button")]
        public override string Text
        {
            get { return base.Text; }
            set
            {
                base.Text = value;
                this.Invalidate();
            }
        }

        /// <summary>시각적 종류 (Primary/Secondary/Danger/Subtle/Execute/Excel).</summary>
        [Category("Modern")]
        [Description("Button kind (Primary/Secondary/Danger/Subtle/Execute/Excel)")]
        [DefaultValue(ButtonKind.Primary)]
        public ButtonKind Kind
        {
            get { return this.kind; }
            set
            {
                this.kind = value;
                this.Invalidate();
            }
        }

        /// <summary>캡션 앞에 표시되는 아이콘 글리프(Segoe MDL2 Assets).</summary>
        [Category("Modern")]
        [Description("Icon glyph before the button text (Segoe MDL2 Assets)")]
        [DefaultValue("")]
        public string IconGlyph
        {
            get { return this.iconGlyph; }
            set
            {
                this.iconGlyph = value ?? string.Empty;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 캡션/아이콘 글자 크기 재정의(px = DIU). 0 이하이면 토큰 기본값
        /// (Font.Size.Body)을 쓴다 — 화살표·기호 같은 아이콘형 캡션을 크게 보이게
        /// 할 때 쓴다.
        /// </summary>
        [Category("Modern")]
        [Description("Caption/icon font size override (px) — 0 = token default")]
        [DefaultValue(0d)]
        public double GlyphSize
        {
            get { return this.glyphSize; }
            set
            {
                this.glyphSize = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 글리프/캡션 위에 붙는 아주 작은 상단 라벨(비어 있으면 숨김) —
        /// 아이콘형 버튼에 "All"/"Selected" 같은 부가 설명을 얹을 때 쓴다.
        /// </summary>
        [Category("Modern")]
        [Description("Small label above the glyph/caption (hidden when empty)")]
        [DefaultValue("")]
        public string TopLabel
        {
            get { return this.topLabel; }
            set
            {
                this.topLabel = value ?? string.Empty;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 글리프/캡션 아래에 붙는 아주 작은 하단 라벨(비어 있으면 숨김) — 상단 라벨과 같은 규칙이고 함께 쓸 수 있다.
        /// </summary>
        [Category("Modern")]
        [Description("Small label below the glyph/caption (hidden when empty)")]
        [DefaultValue("")]
        public string BottomLabel
        {
            get { return this.bottomLabel; }
            set
            {
                this.bottomLabel = value ?? string.Empty;
                this.Invalidate();
            }
        }

        /// <summary>상단 라벨의 가로 정렬(기본 Center = 지금까지의 모양). Left·Right 는 본문 덩어리의 왼쪽·오른쪽 끝에 맞춘다.</summary>
        [Category("Modern")]
        [Description("Horizontal alignment of the top label (Center keeps the previous look)")]
        [DefaultValue(HorizontalAlignment.Center)]
        public HorizontalAlignment TopLabelAlignment
        {
            get { return this.topLabelAlignment; }
            set
            {
                this.topLabelAlignment = value;
                this.Invalidate();
            }
        }

        /// <summary>하단 라벨의 가로 정렬(기본 Center).</summary>
        [Category("Modern")]
        [Description("Horizontal alignment of the bottom label")]
        [DefaultValue(HorizontalAlignment.Center)]
        public HorizontalAlignment BottomLabelAlignment
        {
            get { return this.bottomLabelAlignment; }
            set
            {
                this.bottomLabelAlignment = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 장평(글자 가로 비율) 재정의. 0 = 전역(ModernTheme.FontWidthRatio) 사용,
        /// 양수는 0.8~1.2로 클램프.
        /// </summary>
        [Category("Modern")]
        [Description("Font width ratio override — 0 = use global, allowed 0.8~1.2")]
        [DefaultValue(0d)]
        public double FontWidthRatio
        {
            get { return this.fontWidthRatio; }
            set
            {
                this.fontWidthRatio = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 우상단 카운트 배지 (기본 0 = 없음) — 미처리 건수 알림용. 캡션에
        /// "(12)"를 붙이는 대신 빨간 알약 배지로 표시한다. 99 초과는 "99+".
        /// </summary>
        [Category("Modern")]
        [Description("Count badge at the top-right — 0 = none, above 99 shows 99+")]
        [DefaultValue(0)]
        public int BadgeCount
        {
            get { return this.badgeCount; }
            set
            {
                this.badgeCount = value < 0 ? 0 : value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 이 버튼을 눌렀을 때 폼에 설정될 <see cref="System.Windows.Forms.DialogResult"/>
        /// — <see cref="System.Windows.Forms.Button"/>과 같은 의미다.
        /// <see cref="System.Windows.Forms.DialogResult.None"/>(기본값)이면 폼의
        /// 결과를 건드리지 않는다.
        /// </summary>
        [Category("Modern")]
        [Description("DialogResult set on the form when clicked (None = not set)")]
        [DefaultValue(DialogResult.None)]
        public DialogResult DialogResult
        {
            get { return this.dialogResult; }
            set { this.dialogResult = value; }
        }

        /// <summary>
        /// 폼이 이 버튼을 기본 버튼(<see cref="Form.AcceptButton"/>)으로 지정했는지
        /// 알려 온다 — 폼이 호출하며, 표시만 바꾼다.
        /// </summary>
        /// <param name="value">기본 버튼이면 true.</param>
        public void NotifyDefault(bool value)
        {
            if (this.defaultButton == value)
            {
                return;
            }

            this.defaultButton = value;
            this.Invalidate();
        }

        /// <summary>
        /// 클릭을 프로그램적으로 발생시킨다 — 폼이 Enter(AcceptButton)/Esc
        /// (CancelButton)에서 호출한다. <b>고를 수 없는 버튼</b>(비활성이거나
        /// 숨겨진 버튼, 그런 부모 아래의 버튼)은 아무 일도 하지 않는다 —
        /// <see cref="System.Windows.Forms.Button"/>과 <b>같은 판단
        /// (<see cref="Control.CanSelect"/>)</b>이다.
        ///
        /// 처음에는 <see cref="Control.Enabled"/>만 봤다(2026-08-04). 그러면
        /// <b>숨겨진 Accept/Cancel 버튼이 Enter/Esc로 실행된다</b> — 순정 버튼은
        /// 실행하지 않는다(코덱스 리뷰 2026-08-05 실측: 숨긴 순정 0회 / 우리 1회).
        /// 자체 검사가 창을 띄우지 않아 무동작이 되는 문제는 <b>검사가 폼을
        /// 오프스크린으로 표시</b>해서 풀 일이고, 제품 의미를 바꿔서 맞출 일이
        /// 아니었다.
        /// </summary>
        public void PerformClick()
        {
            if (!this.CanSelect)
            {
                return;
            }

            this.OnClick(EventArgs.Empty);
        }

        /// <summary>
        /// 클릭이 폼의 <see cref="Form.DialogResult"/>를 설정한다 —
        /// <c>btnOk.DialogResult = DialogResult.OK</c>만 지정하면 이벤트 핸들러
        /// 없이 다이얼로그가 닫히는, WinForms 버튼과 같은 관례다.
        ///
        /// 순정 버튼과 한 가지가 다르다: <see cref="DialogResult"/>가
        /// <see cref="System.Windows.Forms.DialogResult.None"/>이면 폼의 결과를
        /// <b>건드리지 않는다</b>. 순정 버튼은 None도 그대로 대입해 이미 정해진
        /// 결과를 지우는데, 클릭 핸들러에서 결과를 정하는 기존 화면들이 그 대입에
        /// 걸릴 수 있어 보수적으로 뒀다.
        /// </summary>
        /// <param name="e">이벤트 인자.</param>
        protected override void OnClick(EventArgs e)
        {
            if (this.dialogResult != DialogResult.None)
            {
                Form owner = this.FindForm();

                if (owner != null)
                {
                    owner.DialogResult = this.dialogResult;
                }
            }

            base.OnClick(e);
        }

        /// <summary>
        /// 기존 `.Designer.cs`가 직렬화해 둔 `xxx.Child = null;` 라인이 계속
        /// 컴파일되도록 남겨 둔 무동작 속성 — 타입 선언만 바꾸면 되는 drop-in
        /// 계약(design-notes 6-1 룰 1)을 유지한다.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object Child
        {
            get { return null; }
            set { }
        }

        /// <summary>WPF 콘텐츠 시절과 같이 색은 토큰이 결정한다 — 속성 그리드에서 숨긴다.</summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color ForeColor
        {
            get { return base.ForeColor; }
            set { base.ForeColor = value; }
        }

        /// <summary>타이포그래피는 타입 램프 토큰이 결정한다 — 속성 그리드에서 숨긴다.</summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Font Font
        {
            get { return base.Font; }
            set { base.Font = value; }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            this.hovered = true;
            this.Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this.hovered = false;
            this.pressed = false;
            this.Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                this.pressed = true;
                this.Focus();
                this.Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left)
            {
                this.pressed = false;
                this.Invalidate();
            }
        }

        /// <summary>스페이스/엔터를 버튼 입력으로 받는다 (WinForms Button과 동일한 조작).</summary>
        protected override bool IsInputKey(Keys keyData)
        {
            Keys key = keyData & Keys.KeyCode;

            if (key == Keys.Space || key == Keys.Enter)
            {
                return true;
            }

            return base.IsInputKey(keyData);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            this.Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            this.Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Space)
            {
                this.pressed = true;
                this.Invalidate();
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                bool wasPressed = this.pressed || e.KeyCode == Keys.Enter;
                this.pressed = false;
                this.Invalidate();

                if (wasPressed && this.Enabled)
                {
                    this.OnClick(EventArgs.Empty);
                }
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            // WPF 템플릿과 동일: 비활성은 기본 커서.
            this.Cursor = this.Enabled ? Cursors.Hand : Cursors.Arrow;
            this.hovered = false;
            this.pressed = false;
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (this.Width <= 0 || this.Height <= 0)
            {
                return;
            }

            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            ButtonPalette palette = this.ResolvePalette();

            // 배경 + 1px 테두리 — 반경은 Radius.Md(4).
            Rectangle bounds = new Rectangle(0, 0, this.Width - 1, this.Height - 1);

            using (GraphicsPath path = BuildRoundedPath(bounds, cornerRadius))
            {
                if (palette.Background.A > 0)
                {
                    using (SolidBrush fill = new SolidBrush(palette.Background))
                    {
                        graphics.FillPath(fill, path);
                    }
                }

                if (palette.Border.A > 0)
                {
                    using (Pen borderPen = new Pen(palette.Border))
                    {
                        graphics.DrawPath(borderPen, path);
                    }
                }
            }

            // 키보드 포커스 링 — Tab 으로 왔을 때만(ShowFocusCues) 테두리 안쪽에 2px 액센트 링. 전에는 버튼류에
            // 포커스 표시가 없어 키보드만으로는 어디에 있는지 알 수 없었다(2026-10-21 점검).
            if (this.Focused && this.ShowFocusCues && this.Enabled)
            {
                Rectangle focus = new Rectangle(1, 1, this.Width - 3, this.Height - 3);

                if (focus.Width > 0 && focus.Height > 0)
                {
                    using (GraphicsPath focusPath = BuildRoundedPath(focus, cornerRadius - 1))
                    using (Pen focusPen = new Pen(ModernTokenColors.Get("Brush.FocusBorder", palette.Foreground), 2f))
                    {
                        graphics.DrawPath(focusPen, focusPath);
                    }
                }
            }

            // 기본 버튼(폼의 AcceptButton) 표시 — 테두리 안쪽에 링 하나. 색은
            // 캡션색을 배경과 섞어 만들어 어떤 종류/테마에서도 대비가 유지된다.
            if (this.defaultButton && this.Enabled)
            {
                Rectangle inner = new Rectangle(2, 2, this.Width - 5, this.Height - 5);

                if (inner.Width > 0 && inner.Height > 0)
                {
                    using (GraphicsPath ringPath = BuildRoundedPath(inner, cornerRadius - 1))
                    using (Pen ringPen = new Pen(ModernTokenColors.Blend(
                            palette.Foreground, palette.OpacityBase, defaultRingOpacity)))
                    {
                        graphics.DrawPath(ringPen, ringPath);
                    }
                }
            }

            // 텍스트는 GDI(TextRenderer) 경로라 앤티에일리어싱 설정과 무관하다.
            graphics.SmoothingMode = SmoothingMode.Default;
            this.PaintContent(graphics, palette);

            if (this.badgeCount > 0)
            {
                this.PaintBadge(graphics);
            }
        }

        // 우상단 카운트 배지 — 빨간 알약 + 흰 숫자 + 표면색 1px 링 (어떤 버튼
        // 색 위에서도 경계가 읽히게). Fluent InfoBadge 문법.
        private void PaintBadge(Graphics graphics)
        {
            string text = this.badgeCount > 99
                    ? "99+"
                    : this.badgeCount.ToString(System.Globalization.CultureInfo.InvariantCulture);
            const TextFormatFlags flags = TextFormatFlags.NoPadding
                    | TextFormatFlags.SingleLine
                    | TextFormatFlags.NoPrefix
                    | TextFormatFlags.HorizontalCenter
                    | TextFormatFlags.VerticalCenter;
            double ratio = ModernTheme.ResolveFontWidthRatio(this.fontWidthRatio);

            using (Font badgeFont = ModernFonts.Create(badgeDiu, false, FontStyle.Regular))
            {
                Size textSize = ScaledTextRenderer.MeasureText(text, badgeFont, TextFormatFlags.NoPadding
                        | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix, ratio);
                int width = Math.Max(badgeHeight, textSize.Width + 8);
                Rectangle bounds = new Rectangle(this.Width - width - 4, 3, width, badgeHeight);

                graphics.SmoothingMode = SmoothingMode.AntiAlias;

                using (GraphicsPath pill = BuildRoundedPath(bounds, badgeHeight / 2))
                {
                    using (SolidBrush fill = new SolidBrush(
                            Token("Brush.CountBadge", Color.FromArgb(196, 43, 28))))
                    {
                        graphics.FillPath(fill, pill);
                    }

                    using (Pen ring = new Pen(Token("Brush.Surface", Color.White)))
                    {
                        graphics.DrawPath(ring, pill);
                    }
                }

                graphics.SmoothingMode = SmoothingMode.Default;
                ScaledTextRenderer.DrawText(
                        graphics, text, badgeFont, bounds,
                        Token("Brush.OnCountBadge", Color.White), flags, ratio);
            }
        }

        // 캡션(+아이콘, +상단 라벨)을 가운데에 그린다.
        private void PaintContent(Graphics graphics, ButtonPalette palette)
        {
            double contentDiu = this.glyphSize > 0d ? this.glyphSize : ModernFonts.BodyDiu;
            double ratio = ModernTheme.ResolveFontWidthRatio(this.fontWidthRatio);
            string caption = this.Text ?? string.Empty;
            bool hasIcon = this.iconGlyph.Length > 0;
            bool hasTopLabel = this.topLabel.Length > 0;
            bool hasBottomLabel = this.bottomLabel.Length > 0;

            const TextFormatFlags flags = TextFormatFlags.NoPadding
                    | TextFormatFlags.SingleLine
                    | TextFormatFlags.NoPrefix;

            using (Font contentFont = ModernFonts.Create(contentDiu, false, FontStyle.Regular))
            using (Font iconFont = ModernFonts.CreateIcon(contentDiu))
            using (Font topFont = ModernFonts.Create(topLabelDiu, false, FontStyle.Regular))
            {
                Size captionSize = caption.Length > 0
                        ? ScaledTextRenderer.MeasureText(caption, contentFont, flags, ratio)
                        : Size.Empty;
                Size iconSize = hasIcon
                        ? ScaledTextRenderer.MeasureText(this.iconGlyph, iconFont, flags, ratio)
                        : Size.Empty;
                Size topSize = hasTopLabel
                        ? ScaledTextRenderer.MeasureText(this.topLabel, topFont, flags, ratio)
                        : Size.Empty;
                Size bottomSize = hasBottomLabel
                        ? ScaledTextRenderer.MeasureText(this.bottomLabel, topFont, flags, ratio)
                        : Size.Empty;

                int lineWidth = captionSize.Width + iconSize.Width + (hasIcon && caption.Length > 0 ? iconGap : 0);
                int lineHeight = Math.Max(captionSize.Height, iconSize.Height);

                // 위·아래 작은 라벨이 있으면 [위][본문][아래]를 한 덩어리로 세로 중앙에 둔다(WPF: 세로 StackPanel, 라벨 Margin -1).
                // 덩어리 폭은 본문(아이콘+캡션) 폭이고 작은 라벨은 그 양 끝·가운데를 기준으로 정렬된다 — 본문보다 길면 밖으로 나간다.
                int blockHeight = lineHeight
                        + (hasTopLabel ? topSize.Height - 1 : 0)
                        + (hasBottomLabel ? bottomSize.Height - 1 : 0);
                int blockTop = (this.Height - blockHeight) / 2;
                int lineLeft = Math.Max(horizontalPadding, (this.Width - lineWidth) / 2);

                if (hasTopLabel)
                {
                    this.PaintSubLabel(
                            graphics, this.topLabel, topFont, palette, blockTop, topSize,
                            lineLeft, lineWidth, this.topLabelAlignment, flags, ratio);
                }

                int lineTop = hasTopLabel ? blockTop + topSize.Height - 1 : blockTop;

                if (hasBottomLabel)
                {
                    this.PaintSubLabel(
                            graphics, this.bottomLabel, topFont, palette, lineTop + lineHeight - 1, bottomSize,
                            lineLeft, lineWidth, this.bottomLabelAlignment, flags, ratio);
                }

                // 요소가 하나면 **GDI가 직접 가운데 정렬**하게 맡긴다.
                // TextRenderer.MeasureText는 어드밴스 폭에 뒤쪽 여분을 포함해
                // 돌려준다(실측: "Search" 9pt → 측정 42px, 실제 잉크 35px).
                // 그 값으로 좌표를 계산해 중앙을 잡으면 글자가 왼쪽으로 3~4px
                // 밀린다 — 순정 WinForms 버튼처럼 DT_CENTER에 맡기면 어긋나지 않는다.
                if (!hasIcon && caption.Length > 0)
                {
                    Rectangle captionRect = new Rectangle(0, lineTop, this.Width, lineHeight);
                    ScaledTextRenderer.DrawText(
                            graphics, caption, contentFont, captionRect, palette.Foreground,
                            flags | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                                | TextFormatFlags.EndEllipsis,
                            ratio);
                    return;
                }

                if (hasIcon && caption.Length == 0)
                {
                    Rectangle glyphRect = new Rectangle(0, lineTop, this.Width, lineHeight);
                    ScaledTextRenderer.DrawText(
                            graphics, this.iconGlyph, iconFont, glyphRect, palette.Foreground,
                            flags | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter,
                            ratio);
                    return;
                }

                // 아이콘 + 캡션: 두 요소를 한 덩어리로 가운데 두고 순서대로 그린다
                // (WPF의 가로 StackPanel과 같은 배치).

                Rectangle iconRect = new Rectangle(
                        lineLeft, lineTop, iconSize.Width, lineHeight);
                ScaledTextRenderer.DrawText(
                        graphics, this.iconGlyph, iconFont, iconRect, palette.Foreground,
                        flags | TextFormatFlags.VerticalCenter, ratio);
                lineLeft += iconSize.Width + iconGap;

                // 남는 폭이 부족하면 말줄임한다 — 글자가 테두리를 넘는 것보다 낫다.
                int captionWidth = Math.Max(0, this.Width - horizontalPadding - lineLeft);
                Rectangle iconCaptionRect = new Rectangle(
                        lineLeft, lineTop, Math.Max(captionSize.Width, captionWidth), lineHeight);
                ScaledTextRenderer.DrawText(
                        graphics, caption, contentFont, iconCaptionRect, palette.Foreground,
                        flags | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis, ratio);
            }
        }

        // 작은 라벨 한 줄 — 본문(아이콘+캡션) 글자 폭이 기준이다: Left 는 본문 왼쪽 끝, Right 는 본문 오른쪽 끝에 맞추고 본문보다 길면 기준을
        // 지킨 채 밖으로 나간다. Center 는 버튼 전체 폭의 가운데(본문도 가운데라 같은 축; GDI 가 직접 가운데 정렬해 잉크 기준이 어긋나지 않는다).
        // WPF 는 SubLabelPanel 이 같은 규칙을 맡고, 그리드 헤더의 위·아래 줄도 같다(2026-09-30).
        private void PaintSubLabel(
                Graphics graphics, string text, Font font, ButtonPalette palette, int top, Size size,
                int lineLeft, int lineWidth, HorizontalAlignment alignment, TextFormatFlags flags, double ratio)
        {
            Color color = ModernTokenColors.Blend(palette.Foreground, palette.OpacityBase, topLabelOpacity);

            if (alignment == HorizontalAlignment.Center)
            {
                ScaledTextRenderer.DrawText(graphics, text, font, new Rectangle(0, top, this.Width, size.Height), color,
                        flags | TextFormatFlags.HorizontalCenter, ratio);
                return;
            }

            int left = alignment == HorizontalAlignment.Right ? lineLeft + lineWidth - size.Width : lineLeft;
            ScaledTextRenderer.DrawText(graphics, text, font, new Rectangle(left, top, size.Width, size.Height), color,
                    flags | TextFormatFlags.Left, ratio);
        }

        // 종류 × 상태 → 색. WPF 스타일의 트리거 조합을 그대로 옮긴 것이다.
        private ButtonPalette ResolvePalette()
        {
            Color surface = Token("Brush.Surface", Color.White);
            ButtonPalette palette = new ButtonPalette();

            // 비활성은 종류와 무관하게 동일한 회색 (WPF: 마지막 트리거가 덮어쓴다).
            if (!this.Enabled)
            {
                palette.Background = Token("Brush.DisabledBackground", Color.FromArgb(243, 243, 243));
                palette.Border = Token("Brush.BorderSubtle", Color.FromArgb(230, 230, 230));
                palette.Foreground = Token("Brush.DisabledText", Color.FromArgb(161, 161, 161));
                palette.OpacityBase = palette.Background;
                return palette;
            }

            bool isPressed = this.pressed;
            bool isHovered = this.hovered && !this.pressed;

            switch (this.kind)
            {
                case ButtonKind.Secondary:
                    palette.Background = surface;
                    palette.Border = Token("Brush.Border", Color.FromArgb(209, 209, 209));
                    palette.Foreground = Token("Brush.TextPrimary", Color.Black);

                    // hover = 중립 HoverBackground + 액센트 테두리 + 누름 글자색, pressed = 전용 누름 짝 — WPF 템플릿과 같은 규칙(2026-10-21).
                    if (isHovered)
                    {
                        palette.Background = Token("Brush.HoverBackground", surface);
                        palette.Border = Token("Brush.Accent", palette.Border);
                        palette.Foreground = Token("Brush.PressedText", palette.Foreground);
                    }
                    else if (isPressed)
                    {
                        palette.Background = Token("Brush.PressedBackground", surface);
                        palette.Border = Token("Brush.Accent", palette.Border);
                        palette.Foreground = Token("Brush.PressedText", palette.Foreground);
                    }

                    break;

                case ButtonKind.Danger:
                    palette.Background = surface;
                    palette.Border = Token("Brush.ErrorBorderSoft", Color.FromArgb(245, 198, 203));
                    palette.Foreground = Token("Brush.ErrorText", Color.FromArgb(196, 43, 28));

                    if (isHovered)
                    {
                        palette.Background = Token("Brush.ErrorBackground", surface);
                        palette.Border = Token("Brush.ErrorBorder", palette.Border);
                    }
                    else if (isPressed)
                    {
                        palette.Background = Token("Brush.ErrorBackground", surface);
                        palette.Border = Token("Brush.DangerPressed", palette.Border);
                        palette.Foreground = Token("Brush.ErrorText", palette.Foreground);
                    }

                    break;

                case ButtonKind.Subtle:
                    palette.Background = Color.Transparent;
                    palette.Border = Color.Transparent;
                    palette.Foreground = Token("Brush.TextSecondary", Color.FromArgb(96, 96, 96));

                    if (isHovered)
                    {
                        palette.Background = Token("Brush.HoverBackground", surface);
                        palette.Foreground = Token("Brush.TextPrimary", palette.Foreground);
                    }
                    else if (isPressed)
                    {
                        palette.Background = Token("Brush.PressedBackground", surface);
                        palette.Foreground = Token("Brush.PressedText", palette.Foreground);
                    }

                    break;

                case ButtonKind.Execute:
                    palette.Background = Token("Brush.Success", Color.FromArgb(16, 124, 16));
                    palette.Border = palette.Background;
                    palette.Foreground = Token("Brush.OnAccent", Color.White);

                    if (isHovered)
                    {
                        palette.Background = Token("Brush.SuccessHover", palette.Background);
                        palette.Border = palette.Background;
                    }
                    else if (isPressed)
                    {
                        palette.Background = Token("Brush.SuccessPressed", palette.Background);
                        palette.Border = palette.Background;
                    }

                    break;

                case ButtonKind.Excel:
                    palette.Background = surface;
                    palette.Border = Token("Brush.ExcelBorderSoft", Color.FromArgb(183, 224, 195));
                    palette.Foreground = Token("Brush.ExcelText", Color.FromArgb(16, 124, 65));

                    if (isHovered)
                    {
                        palette.Background = Token("Brush.ExcelBackground", surface);
                        palette.Border = Token("Brush.ExcelBorder", palette.Border);
                    }
                    else if (isPressed)
                    {
                        palette.Background = Token("Brush.ExcelBorderSoft", surface);
                        palette.Border = Token("Brush.ExcelPressed", palette.Border);
                        palette.Foreground = Token("Brush.ExcelPressed", palette.Foreground);
                    }

                    break;

                default:
                    palette.Background = Token("Brush.Accent", Color.FromArgb(0, 120, 212));
                    palette.Border = palette.Background;
                    palette.Foreground = Token("Brush.OnAccent", Color.White);

                    if (isHovered)
                    {
                        palette.Background = Token("Brush.AccentHover", palette.Background);
                        palette.Border = palette.Background;
                    }
                    else if (isPressed)
                    {
                        palette.Background = Token("Brush.AccentPressed", palette.Background);
                        palette.Border = palette.Background;
                    }

                    break;
            }

            // 상단 라벨 반투명 합성의 바탕 — 투명 배경(Subtle)이면 부모 색을 쓴다.
            palette.OpacityBase = palette.Background.A > 0
                    ? palette.Background
                    : (this.Parent != null ? this.Parent.BackColor : ModernTheme.Background);

            return palette;
        }

        private static Color Token(string key, Color fallback)
        {
            return ModernTokenColors.Get(key, fallback);
        }

        // 모서리가 둥근 사각형 경로.
        private static GraphicsPath BuildRoundedPath(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();

            if (radius <= 0 || bounds.Width <= 0 || bounds.Height <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            int diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));

            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180f, 90f);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270f, 90f);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0f, 90f);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90f, 90f);
            path.CloseFigure();
            return path;
        }

        // 한 번의 그리기에 쓰이는 색 묶음.
        private struct ButtonPalette
        {
            internal Color Background;
            internal Color Border;
            internal Color Foreground;

            // 반투명 텍스트를 섞을 바탕색.
            internal Color OpacityBase;
        }
    }
}
