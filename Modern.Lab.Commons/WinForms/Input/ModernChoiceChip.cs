using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Modern.Lab.Theming;
using Modern.Lab.WinForms.Controls.Display;
using Modern.Lab.WinForms.Rendering;

namespace Modern.Lab.WinForms.Controls.Input
{
    /// <summary>
    /// 골라 누르는 상태 칩 — 상태 배지(<see cref="ModernStatusBadge"/>)의 색 · 스핀 · 글자 규칙을 그대로 물려받고,
    /// 눌림(<see cref="Checked"/>) · 클릭 토글 · 꺼짐 흐림을 더한 **선택용** 컨트롤이다. 배지는 보여 주기만 하고
    /// 칩은 고르게 한다 — 그래서 배지 쪽 코드는 손대지 않고 파생으로 만들었다(2026-10-20 사용자 결정).
    ///
    /// 쓰임: 핸들러의 자식 장비처럼 "상태가 보이는 후보 몇 개 중 하나를 누르는" 자리(Equipment / Lots 의 Port List
    /// 아래 자식 칩 줄). 배타 선택은 칩을 놓은 쪽이 맡는다 — 하나를 누르면 나머지를 끄는 것은 폼의 일이다
    /// (<see cref="ModernToggleGroup"/>처럼 한 덩어리가 아니라 칩마다 상태 색이 다르기 때문).
    ///
    /// 시각 규칙: 안 눌린 칩은 배지 그대로(기본 중립색 — <see cref="ModernStatusBadge.ColorValue"/> 등을 주면 그 색),
    /// **눌린 칩은 액센트 채움**(토글 버튼의 켬과 같은 문법 — 배지의 Color 경로로 칠하므로 빛띠는 그 위에 그대로 돈다.
    /// 2026-10-20 사용자: 자식마다 색이 다른 것은 어지럽다), 마우스 오버면 옅은 액센트 1px 테두리,
    /// 꺼짐(Enabled=false)이면 표면색 반투명을 덮어 흐리게 한다. 그래서 <see cref="ModernStatusBadge.Color"/> 직접 대입은
    /// 칩에서 쓰지 않는다 — 눌림이 바뀔 때마다 칩이 다시 정한다. 알약 폭 · 높이는 컨트롤 크기를 꽉 채운다
    /// (FillWidth · FillHeight 켬) — 칩을 나란히 둘 때 폭을 통일하기 위해서다.
    ///
    /// 조작: 클릭 · Space · Enter 가 <see cref="Checked"/>를 반전한다(ModernToggleButton 과 같다).
    ///
    /// 앞 배지: <see cref="BadgeText"/> 를 주면 알약 왼쪽에 작은 원형 배지(순위 번호)를 그리고 글자는 그만큼 오른쪽으로 민다.
    /// 색은 <see cref="BadgeColor"/>(표의 순위 배지와 같은 색), 비면 테두리색 중립 — 순위 없음("–")과 꺼짐(흐림)을 따로
    /// 읽게 하려는 것이다(2026-10-22 사용자: 자식 칩에서 순위 없음과 비활성이 구분되지 않음).
    ///
    /// 뒤 상태 점: <see cref="StatusColorToken"/> 에 토큰 키(<c>"Brush.Success"</c> 등)를 주면 알약 오른쪽에 작은 원을
    /// 그 색으로 그리고 글자는 그만큼 왼쪽으로 민다. 상태(Idle · Run · Down)를 글자 대신 색으로 읽게 하려는 것이라
    /// 꺼짐 덮개 **위에** 그려 흐려지지 않는다 — 못 고르는 칩도 왜 못 고르는지 색이 먼저 말한다(2026-10-22 사용자 요청).
    /// </summary>
    [ToolboxItem(true)]
    [DesignerCategory("Code")]
    [DefaultEvent("CheckedChanged")]
    public class ModernChoiceChip : ModernStatusBadge
    {
        // Radius.Sm 4 — 배지 Rounded 모서리와 같은 반경으로 테두리를 두른다.
        private const int ringRadius = 4;

        // 꺼짐 덮개의 알파 — 글자가 읽히되 눌리지 않는 것으로 보일 만큼.
        private const int disabledVeilAlpha = 150;

        // 앞 배지 지름은 20px 고정(표의 순위 배지와 같은 크기) — 칩이 높아져도 배지는 커지지 않는다(2026-10-22 사용자 지적).
        // 칩이 그보다 낮으면 위아래 4px 씩 띄운 값까지 줄어든다. 배지와 글자 사이는 6px.
        private const int badgeDiameterMax = 20;
        private const int badgeInset = 4;
        private const int badgeGap = 6;
        private const double badgeFontDiu = 9d;
        // 뒤 상태 점 지름 8px, 오른쪽 가장자리에서 10px 안쪽, 글자와 점 사이 4px — 글자 영역은 점 전체(안쪽 여백 + 지름 + 간격)만큼 비운다.
        private const int statusDotDiameter = 8;
        private const int statusDotInset = 10;
        private const int statusDotGap = 4;

        private bool isChecked;
        private bool hovered;
        private bool pressed;
        private string badgeText = string.Empty;
        private string badgeColor = string.Empty;
        private string statusColorToken = string.Empty;

        /// <summary><see cref="Checked"/>가 바뀔 때 발생한다 (CheckBox 호환 이름).</summary>
        public event EventHandler CheckedChanged;

        /// <summary>칩 기본 크기와 배지 채움 옵션을 켜서 만든다.</summary>
        public ModernChoiceChip()
        {
            this.SetStyle(ControlStyles.Selectable, true);
            this.TabStop = true;
            this.FillWidth = true;
            this.FillHeight = true;
            this.Size = new Size(96, 28);
            this.Cursor = Cursors.Hand;
            this.Text = "Chip";
        }

        /// <summary>눌림 상태. 클릭 · Space · Enter 로 반전되고 코드로도 둘 수 있다.</summary>
        [Category("Modern")]
        [Description("Checked (pressed) state — toggles on click")]
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
                this.ApplyCheckedColor();
                this.Invalidate();

                if (this.CheckedChanged != null)
                {
                    this.CheckedChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>알약 왼쪽 원형 배지의 글자(순위 번호 등). 비면 배지를 그리지 않고 글자 자리도 돌려준다.</summary>
        [Category("Modern")]
        [Description("Small round badge drawn before the text (e.g. priority number); empty hides it")]
        [DefaultValue("")]
        public string BadgeText
        {
            get { return this.badgeText; }
            set
            {
                string text = value ?? string.Empty;

                if (this.badgeText == text)
                {
                    return;
                }

                this.badgeText = text;
                this.UpdateBadgePadding();
                this.Invalidate();
            }
        }

        /// <summary>앞 배지 채움색(#RRGGBB). 비면 테두리 토큰색의 중립 배지 — 순위 없음 표시.</summary>
        [Category("Modern")]
        [Description("Fill colour of the leading badge (#RRGGBB); empty uses the neutral border colour")]
        [DefaultValue("")]
        public string BadgeColor
        {
            get { return this.badgeColor; }
            set
            {
                string color = value ?? string.Empty;

                if (this.badgeColor == color)
                {
                    return;
                }

                this.badgeColor = color;
                this.Invalidate();
            }
        }

        /// <summary>알약 오른쪽 상태 점의 색 토큰 키(예: <c>"Brush.Success"</c>). 비면 점을 그리지 않는다. 꺼진 칩에서도 흐려지지 않는다.</summary>
        [Category("Modern")]
        [Description("Token key of the trailing status dot colour (e.g. Brush.Success); empty hides the dot")]
        [DefaultValue("")]
        public string StatusColorToken
        {
            get { return this.statusColorToken; }
            set
            {
                string token = value ?? string.Empty;

                if (this.statusColorToken == token)
                {
                    return;
                }

                this.statusColorToken = token;
                this.UpdateBadgePadding();
                this.Invalidate();
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.UpdateBadgePadding();
        }

        // 배지가 있으면 글자 영역을 배지 지름 + 간격만큼 오른쪽으로, 상태 점이 있으면 점 지름 + 간격만큼 왼쪽으로 민다
        // (배지의 Padding 규칙 — 알약은 그대로고 글자만 안쪽으로 든다).
        private void UpdateBadgePadding()
        {
            int left = this.badgeText.Length == 0 ? 0 : this.BadgeDiameter() + badgeGap;
            int right = this.statusColorToken.Length == 0 ? 0 : statusDotInset + statusDotDiameter + statusDotGap;

            if (this.Padding.Left != left || this.Padding.Right != right)
            {
                this.Padding = new Padding(left, 0, right, 0);
            }
        }

        private int BadgeDiameter()
        {
            return Math.Max(8, Math.Min(badgeDiameterMax, this.Height - (badgeInset * 2)));
        }

        protected override void OnClick(EventArgs e)
        {
            if (this.Enabled)
            {
                this.Checked = !this.Checked;
            }

            base.OnClick(e);
        }

        // 눌림 = 액센트 채움, 안 눌림 = 배지의 다른 색 경로(ColorValue · Semantic, 없으면 중립). 배지의 Color 가
        // 가장 앞서는 경로라 여기에 액센트를 넣으면 나머지 경로를 덮고, 비우면 되돌아온다.
        private void ApplyCheckedColor()
        {
            this.Color = this.isChecked
                    ? ColorTranslator.ToHtml(ModernTokenColors.Get("Brush.Accent", System.Drawing.Color.FromArgb(0, 120, 212)))
                    : string.Empty;
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

            if (e.Button == MouseButtons.Left && this.Enabled)
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

        /// <summary>Space · Enter 를 칩 입력으로 받는다 (ModernToggleButton 과 같은 조작).</summary>
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

        /// <summary>배지를 그대로 그린 뒤 눌림 · 오버 테두리와 꺼짐 덮개를 얹는다.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (this.Width <= 2 || this.Height <= 2)
            {
                return;
            }

            Graphics graphics = e.Graphics;
            SmoothingMode originalMode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // 배지는 FillWidth · FillHeight 로 컨트롤을 꽉 채우므로 알약 = 컨트롤 영역이다.
            Rectangle pill = new Rectangle(0, 0, this.Width, this.Height);

            if (this.badgeText.Length > 0)
            {
                this.DrawLeadingBadge(graphics);
            }

            if (!this.Enabled)
            {
                System.Drawing.Color surface = ModernTokenColors.Get("Brush.Surface", System.Drawing.Color.White);

                using (GraphicsPath veil = this.BuildPillPath(pill))
                using (SolidBrush brush = new SolidBrush(System.Drawing.Color.FromArgb(disabledVeilAlpha, surface)))
                {
                    graphics.FillPath(brush, veil);
                }

                this.DrawStatusDot(graphics);
                graphics.SmoothingMode = originalMode;
                return;
            }

            System.Drawing.Color accent = ModernTokenColors.Get("Brush.Accent", System.Drawing.Color.FromArgb(0, 120, 212));

            if (!this.isChecked && (this.hovered || this.pressed))
            {
                Rectangle ring = new Rectangle(0, 0, this.Width - 1, this.Height - 1);

                using (GraphicsPath path = this.BuildPillPath(ring))
                using (Pen pen = new Pen(System.Drawing.Color.FromArgb(this.pressed ? 220 : 140, accent), 1f))
                {
                    graphics.DrawPath(pen, path);
                }
            }

            // 키보드 포커스 링 — Tab 으로 왔을 때만 2px 액센트 링(ModernButton 과 같은 규칙, 2026-10-21).
            if (this.Focused && this.ShowFocusCues)
            {
                Rectangle focus = new Rectangle(1, 1, this.Width - 3, this.Height - 3);

                using (GraphicsPath path = this.BuildPillPath(focus))
                using (Pen pen = new Pen(ModernTokenColors.Get("Brush.FocusBorder", accent), 2f))
                {
                    graphics.DrawPath(pen, path);
                }
            }

            this.DrawStatusDot(graphics);
            graphics.SmoothingMode = originalMode;
        }

        // 알약 오른쪽 상태 점 — 토큰색 그대로. 눌린 칩(액센트 채움)에서는 흰 1px 테두리를 둘러 바탕과 가른다.
        private void DrawStatusDot(Graphics graphics)
        {
            if (this.statusColorToken.Length == 0)
            {
                return;
            }

            System.Drawing.Color fill = ModernTokenColors.Get(this.statusColorToken, ModernTheme.TextSecondary);
            Rectangle dot = new Rectangle(
                    this.Width - statusDotInset - statusDotDiameter, (this.Height - statusDotDiameter) / 2,
                    statusDotDiameter, statusDotDiameter);

            using (SolidBrush brush = new SolidBrush(fill))
            {
                graphics.FillEllipse(brush, dot);
            }

            if (this.isChecked)
            {
                using (Pen pen = new Pen(ModernTokenColors.Get("Brush.OnAccent", System.Drawing.Color.White), 1f))
                {
                    graphics.DrawEllipse(pen, dot);
                }
            }
        }

        // 알약 왼쪽 원형 배지 — 표의 순위 배지와 같은 색을 받아 번호를 가운데 적는다. 글자색은 채움의 밝기로 고른다.
        private void DrawLeadingBadge(Graphics graphics)
        {
            int diameter = this.BadgeDiameter();
            Rectangle circle = new Rectangle(badgeInset + 2, (this.Height - diameter) / 2, diameter, diameter);
            System.Drawing.Color neutral = ModernTokenColors.Get("Brush.Border", System.Drawing.Color.FromArgb(209, 213, 219));
            System.Drawing.Color fill = neutral;

            if (this.badgeColor.Length > 0)
            {
                try
                {
                    fill = ColorTranslator.FromHtml(this.badgeColor);
                }
                catch (Exception)
                {
                    fill = neutral;
                }
            }

            double luminance = ((0.299 * fill.R) + (0.587 * fill.G) + (0.114 * fill.B)) / 255d;
            System.Drawing.Color ink = luminance > 0.6
                    ? ModernTokenColors.Get("Brush.TextPrimary", System.Drawing.Color.FromArgb(32, 31, 30))
                    : System.Drawing.Color.White;

            using (SolidBrush brush = new SolidBrush(fill))
            {
                graphics.FillEllipse(brush, circle);
            }

            // 글자는 직접 재서 원 한가운데에 놓는다 — 정렬 플래그만 믿으면 굵은 숫자가 왼쪽 위로 치우친다(2026-10-22 사용자 지적).
            using (Font font = ModernFonts.Create(badgeFontDiu, true, FontStyle.Regular))
            {
                TextFormatFlags flags = TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding;
                Size textSize = TextRenderer.MeasureText(graphics, this.badgeText, font, Size.Empty, flags);
                Point origin = new Point(
                        circle.X + (int)Math.Round((circle.Width - textSize.Width) / 2.0),
                        circle.Y + (int)Math.Round((circle.Height - textSize.Height) / 2.0));
                TextRenderer.DrawText(graphics, this.badgeText, font, origin, ink, flags);
            }
        }

        // 배지 모양(Rounded · Pill)과 같은 외곽 경로 — 테두리와 덮개가 배지 윤곽을 벗어나지 않게.
        private GraphicsPath BuildPillPath(Rectangle bounds)
        {
            GraphicsPath path = new GraphicsPath();

            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                path.AddRectangle(new Rectangle(0, 0, 1, 1));
                return path;
            }

            int diameter = this.Shape == BadgeShape.Pill
                    ? Math.Min(bounds.Height, bounds.Width)
                    : Math.Min(ringRadius * 2, Math.Min(bounds.Height, bounds.Width));

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
    }
}
