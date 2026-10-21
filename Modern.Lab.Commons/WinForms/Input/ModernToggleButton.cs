using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Modern.Lab.Theming;
using Modern.Lab.WinForms.Rendering;

namespace Modern.Lab.WinForms.Controls.Input
{
    /// <summary>
    /// 버튼 모양 토글 — `CheckBox.Appearance = Button`(버튼형 체크박스)의
    /// 드롭인 대체 컨트롤. 조회 조건 스트립의 필터 토글("내 작업만",
    /// "2020년 이후 입사")이나 뷰 전환처럼 **눌린 상태가 곧 켬**인 조작에 쓴다.
    ///
    /// 역할 구분: 설정성 켬/끔은 ModernToggleSwitch, 라디오 원형 배타 선택은
    /// ModernRadioGroup, 버튼 모양 토글이 이 컨트롤이다.
    ///
    /// ModernButton과 같은 이유(폼당 여러 개 놓이는 버튼류의 섬 비용 —
    /// docs/design-notes.md 7절)로 WPF 섬이 아니라 **GDI+로 직접 그리는
    /// 순수 WinForms 컨트롤**이다. 색은 ModernTokenColors로 WPF와 같은 토큰
    /// 사전에서 읽는다. 시각 규칙: 꺼짐 = Secondary 버튼, 켜짐 = Primary(액센트
    /// 채움) 버튼과 동일.
    ///
    /// 호환 멤버: Text(override, localizable), Checked, CheckedChanged, Click,
    /// Enabled — 기존 CheckBox 폼 코드가 그대로 동작한다.
    /// </summary>
    [ToolboxItem(true)]
    [DesignerCategory("Code")]
    [DefaultEvent("CheckedChanged")]
    public class ModernToggleButton : Control
    {
        // Radius.Md — 컨트롤 모서리 반경 (ModernButton과 동일).
        private const int cornerRadius = 4;

        // Pad.Button — 좌우 여백 16 (캡션 말줄임 판정용).
        private const int horizontalPadding = 16;

        private bool isChecked;
        private double fontWidthRatio;
        private bool hovered;
        private bool pressed;

        /// <summary><see cref="Checked"/>가 바뀔 때 발생한다 (CheckBox 호환 이름).</summary>
        public event EventHandler CheckedChanged;

        /// <summary>적절한 기본 크기로 컨트롤을 생성한다.</summary>
        public ModernToggleButton()
        {
            this.SetStyle(
                    ControlStyles.AllPaintingInWmPaint
                    | ControlStyles.OptimizedDoubleBuffer
                    | ControlStyles.UserPaint
                    | ControlStyles.ResizeRedraw
                    | ControlStyles.Selectable
                    | ControlStyles.SupportsTransparentBackColor,
                    true);

            this.isChecked = false;
            this.fontWidthRatio = 0d;

            this.Size = new Size(120, 32);
            this.TabStop = true;
            this.BackColor = Color.Transparent;
            base.Text = "Toggle";
            this.Cursor = Cursors.Hand;
        }

        /// <summary>버튼에 표시되는 캡션.</summary>
        [Category("Modern")]
        [Description("Text shown on the toggle button")]
        [Browsable(true)]
        [Localizable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue("Toggle")]
        public override string Text
        {
            get { return base.Text; }
            set
            {
                base.Text = value;
                this.Invalidate();
            }
        }

        /// <summary>켬(눌림) 상태 (CheckBox 호환 이름). 클릭할 때마다 반전된다.</summary>
        [Category("Modern")]
        [Description("Checked (pressed) state — toggles on every click")]
        [DefaultValue(false)]
        public bool Checked
        {
            get { return this.isChecked; }
            set
            {
                if (this.isChecked == value)
                {
                    return;
                }

                this.isChecked = value;
                this.Invalidate();

                if (this.CheckedChanged != null)
                {
                    this.CheckedChanged(this, EventArgs.Empty);
                }
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

        /// <summary>색은 토큰이 결정한다 — 속성 그리드에서 숨긴다.</summary>
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

        protected override void OnClick(EventArgs e)
        {
            // 클릭 = 토글. Checked setter가 다시 그리기와 CheckedChanged를 처리한다.
            if (this.Enabled)
            {
                this.Checked = !this.Checked;
            }

            base.OnClick(e);
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

        /// <summary>스페이스/엔터를 버튼 입력으로 받는다 (ModernButton과 동일한 조작).</summary>
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

            TogglePalette palette = this.ResolvePalette();

            // 배경 + 1px 테두리 — 반경은 Radius.Md(4). ModernButton과 같은 문법.
            Rectangle borderBounds = new Rectangle(0, 0, this.Width - 1, this.Height - 1);

            using (GraphicsPath path = BuildRoundedPath(borderBounds, cornerRadius))
            {
                if (palette.Background.A > 0)
                {
                    using (SolidBrush brush = new SolidBrush(palette.Background))
                    {
                        graphics.FillPath(brush, path);
                    }
                }

                if (palette.Border.A > 0)
                {
                    using (Pen pen = new Pen(palette.Border))
                    {
                        graphics.DrawPath(pen, path);
                    }
                }
            }

            // 키보드 포커스 링 — ModernButton 과 같은 규칙(2026-10-21).
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

            // 텍스트는 GDI(TextRenderer) 경로라 앤티에일리어싱 설정과 무관하다.
            graphics.SmoothingMode = SmoothingMode.Default;

            string caption = this.Text ?? string.Empty;

            if (caption.Length == 0)
            {
                return;
            }

            double ratio = ModernTheme.ResolveFontWidthRatio(this.fontWidthRatio);

            const TextFormatFlags flags = TextFormatFlags.NoPadding
                    | TextFormatFlags.SingleLine
                    | TextFormatFlags.NoPrefix
                    | TextFormatFlags.HorizontalCenter
                    | TextFormatFlags.VerticalCenter
                    | TextFormatFlags.EndEllipsis;

            // 좌우 여백을 뺀 영역에 GDI가 직접 가운데 정렬한다 (ModernButton의
            // 단독 캡션 경로와 같은 이유 — 측정 기반 좌표 계산은 3~4px 밀린다).
            Rectangle captionRect = new Rectangle(
                    horizontalPadding / 2, 0,
                    Math.Max(0, this.Width - horizontalPadding), this.Height);

            using (Font contentFont = ModernFonts.Create(ModernFonts.BodyDiu, false, FontStyle.Regular))
            {
                ScaledTextRenderer.DrawText(
                        graphics, caption, contentFont, captionRect,
                        palette.Foreground, flags, ratio);
            }
        }

        // 상태(켬/끔 × hover/pressed/비활성) → 색.
        // 꺼짐 = Secondary 버튼, 켜짐 = Primary(액센트 채움) 버튼과 같은 규칙.
        private TogglePalette ResolvePalette()
        {
            Color surface = Token("Brush.Surface", Color.White);
            TogglePalette palette = new TogglePalette();

            if (!this.Enabled)
            {
                palette.Background = Token("Brush.DisabledBackground", Color.FromArgb(243, 243, 243));
                palette.Border = Token("Brush.BorderSubtle", Color.FromArgb(230, 230, 230));
                palette.Foreground = Token("Brush.DisabledText", Color.FromArgb(161, 161, 161));
                return palette;
            }

            bool isPressed = this.pressed;
            bool isHovered = this.hovered && !this.pressed;

            if (this.isChecked)
            {
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

                return palette;
            }

            palette.Background = surface;
            palette.Border = Token("Brush.Border", Color.FromArgb(209, 209, 209));
            palette.Foreground = Token("Brush.TextPrimary", Color.Black);

            // hover = 중립 HoverBackground + 액센트 테두리 + 누름 글자색, pressed = 전용 누름 짝 — ModernButton 과 같은 규칙(2026-10-21).
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

            return palette;
        }

        private static Color Token(string key, Color fallback)
        {
            return ModernTokenColors.Get(key, fallback);
        }

        // 모서리가 둥근 사각형 경로 (ModernButton과 동일).
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
        private struct TogglePalette
        {
            internal Color Background;
            internal Color Border;
            internal Color Foreground;
        }
    }
}
