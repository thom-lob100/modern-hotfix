using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Modern.Lab.Theming;
using Modern.Lab.WinForms.Rendering;

namespace Modern.Lab.Hosting.Theming
{
    /// <summary>
    /// 테마 설정 창의 테마 타일 — 테마 하나를 그 테마의 바탕색 · 카드색 · 액센트 · 글자색으로 미리 보여 주고,
    /// 클릭 · Space 로 고른다(2026-10-23). 라디오 문법이다 — 누르면 켜지고 스스로 꺼지지 않는다. 배타 선택은
    /// 타일을 놓은 창(<see cref="ModernThemeDialogForm"/>)이 맡는다.
    ///
    /// 그리는 것: 테마 Background 로 채운 둥근 판 위에 Surface 색 미니 카드(액센트 세로 바 + "Ag" 글자 견본 + 상태 낱말),
    /// 아래에 테마 이름. 마우스 오버는 현재 테마 액센트 1px 링, 선택은 FocusBorder 2px 링 + 오른쪽 위 체크 배지.
    /// 상태 낱말은 하나만 — Current(지금 켜진 테마) 가 Saved(파일에 저장된 테마) 보다 먼저다.
    ///
    /// 키: ←→↑↓ · Home · End 는 입력 키로 받아 KeyDown 으로 올린다(이동은 창이 처리). Enter 는 창의 AcceptButton(Save)으로 간다.
    /// </summary>
    public sealed class ThemeTile : Control
    {
        private const int CornerRadius = 8;
        private const int CardRadius = 4;
        private const int Inset = 10;
        private const int CardHeight = 30;
        private const int AccentBarWidth = 3;
        private const int CheckBadgeDiameter = 18;
        private const string SampleText = "Ag";
        private const string CheckGlyph = "";
        private const string CurrentWord = "Current";
        private const string SavedWord = "Saved";

        private ModernTheme.ThemeMode mode;
        private bool isChecked;
        private bool isCurrent;
        private bool isSaved;
        private bool hovered;

        /// <summary><see cref="Checked"/>가 바뀔 때 발생한다.</summary>
        public event EventHandler CheckedChanged;

        /// <summary>기본 크기와 더블 버퍼로 만든다.</summary>
        public ThemeTile()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.Selectable, true);
            this.TabStop = true;
            this.Size = new Size(140, 80);
            this.Cursor = Cursors.Hand;
        }

        /// <summary>이 타일이 보여 주는 테마.</summary>
        [Category("Modern")]
        [Description("Theme shown by this tile")]
        public ModernTheme.ThemeMode Mode
        {
            get { return this.mode; }
            set
            {
                this.mode = value;
                this.Invalidate();
            }
        }

        /// <summary>선택 상태. 클릭 · Space 로 켜지고 코드로 끈다.</summary>
        [Category("Modern")]
        [Description("Selected state")]
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
                this.OnCheckedChanged(EventArgs.Empty);
            }
        }

        /// <summary>지금 프로그램에 켜진 테마인지 — 상태 낱말 "Current".</summary>
        [Category("Modern")]
        [Description("Marks the theme the program is running with")]
        [DefaultValue(false)]
        public bool IsCurrent
        {
            get { return this.isCurrent; }
            set
            {
                this.isCurrent = value;
                this.Invalidate();
            }
        }

        /// <summary>설정 파일에 저장된 테마인지 — 상태 낱말 "Saved"(Current 가 아닐 때만 보인다).</summary>
        [Category("Modern")]
        [Description("Marks the theme saved in the settings file")]
        [DefaultValue(false)]
        public bool IsSaved
        {
            get { return this.isSaved; }
            set
            {
                this.isSaved = value;
                this.Invalidate();
            }
        }

        /// <summary><see cref="CheckedChanged"/>를 발생시킨다.</summary>
        private void OnCheckedChanged(EventArgs e)
        {
            EventHandler handler = this.CheckedChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>화살표 · Home · End 를 입력 키로 받아 KeyDown 으로 올린다.</summary>
        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                case Keys.Home:
                case Keys.End:
                    return true;
                default:
                    return base.IsInputKey(keyData);
            }
        }

        /// <summary>Space 로 고른다.</summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!e.Handled && e.KeyCode == Keys.Space)
            {
                this.Checked = true;
                e.Handled = true;
            }
        }

        /// <summary>클릭하면 포커스를 받고 고른다.</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                this.Focus();
                this.Checked = true;
            }
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
            this.Invalidate();
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

        /// <summary>부모 바탕을 깔고(둥근 모서리 밖) 테마 미리보기를 그린다.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (this.Width <= 4 || this.Height <= 4)
            {
                return;
            }

            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Color background = ModernTheme.BackgroundOf(this.mode);
            Color surface = ModernTheme.SurfaceOf(this.mode);
            Color accent = ModernTheme.AccentOf(this.mode);
            Color text = ModernTheme.TextPrimaryOf(this.mode);
            Color currentAccent = ModernTokenColors.Get("Brush.Accent", ModernTheme.Accent);
            Color focusBorder = ModernTokenColors.Get("Brush.FocusBorder", currentAccent);
            Color tileBorder = ModernTokenColors.Get("Brush.BorderSubtle", ModernTheme.BorderSubtle);

            Rectangle plate = new Rectangle(0, 0, this.Width - 1, this.Height - 1);

            using (GraphicsPath path = CreateRoundedPath(plate, CornerRadius))
            using (SolidBrush brush = new SolidBrush(background))
            using (Pen pen = new Pen(tileBorder, 1f))
            {
                graphics.FillPath(brush, path);
                graphics.DrawPath(pen, path);
            }

            // 미니 카드 — 그 테마의 Surface 위에 액센트 바 · 글자 견본 · 상태 낱말.
            Rectangle card = new Rectangle(Inset, Inset, this.Width - Inset * 2, CardHeight);

            using (GraphicsPath path = CreateRoundedPath(card, CardRadius))
            using (SolidBrush brush = new SolidBrush(surface))
            {
                graphics.FillPath(brush, path);
            }

            Rectangle bar = new Rectangle(card.X + 8, card.Y + 7, AccentBarWidth, card.Height - 14);

            using (SolidBrush brush = new SolidBrush(accent))
            {
                graphics.FillRectangle(brush, bar);
            }

            // 선택 타일은 오른쪽 위 체크 배지가 카드 모서리를 덮으므로 상태 낱말을 그만큼 왼쪽으로 비킨다.
            int statusReserve = this.isChecked ? CheckBadgeDiameter - 4 : 0;
            Rectangle sample = new Rectangle(bar.Right + 7, card.Y, card.Width - (bar.Right + 7 - card.X) - 8 - statusReserve, card.Height);
            TextRenderer.DrawText(graphics, SampleText, ModernFonts.BodyStrong, sample, text,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);

            string status = this.isCurrent ? CurrentWord : (this.isSaved ? SavedWord : string.Empty);

            if (status.Length > 0)
            {
                TextRenderer.DrawText(graphics, status, ModernFonts.Body, sample, accent,
                        TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
            }

            // 테마 이름 — 판 아래쪽, 그 테마의 글자색.
            Rectangle name = new Rectangle(Inset, card.Bottom + 4, this.Width - Inset * 2, this.Height - card.Bottom - 4 - Inset + 2);
            TextRenderer.DrawText(graphics, this.mode.ToString(), ModernFonts.BodyStrong, name, text,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);

            if (!this.isChecked && this.hovered)
            {
                using (GraphicsPath path = CreateRoundedPath(plate, CornerRadius))
                using (Pen pen = new Pen(Color.FromArgb(180, currentAccent), 1f))
                {
                    graphics.DrawPath(pen, path);
                }
            }

            if (this.isChecked)
            {
                Rectangle ring = new Rectangle(1, 1, this.Width - 3, this.Height - 3);

                using (GraphicsPath path = CreateRoundedPath(ring, CornerRadius - 1))
                using (Pen pen = new Pen(focusBorder, 2f))
                {
                    graphics.DrawPath(pen, path);
                }

                this.DrawCheckBadge(graphics, focusBorder);
            }
        }

        // 오른쪽 위 체크 배지 — 링과 같은 색 원 + 대비색 체크 글리프.
        private void DrawCheckBadge(Graphics graphics, Color fill)
        {
            Rectangle badge = new Rectangle(this.Width - Inset - CheckBadgeDiameter + 4, Inset - 4, CheckBadgeDiameter, CheckBadgeDiameter);

            using (SolidBrush brush = new SolidBrush(fill))
            {
                graphics.FillEllipse(brush, badge);
            }

            using (Font icon = ModernFonts.CreateIcon(9d))
            {
                TextRenderer.DrawText(graphics, CheckGlyph, icon, badge, ContrastOn(fill),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
            }
        }

        // 채움색 밝기에 따라 검정/흰색 — 밝은 액센트(다크 테마) 위에서도 글리프가 보이게.
        private static Color ContrastOn(Color fill)
        {
            double luminance = (0.299 * fill.R + 0.587 * fill.G + 0.114 * fill.B) / 255d;
            return luminance > 0.6 ? Color.FromArgb(17, 24, 39) : Color.White;
        }

        private static GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = Math.Max(1, radius * 2);
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
