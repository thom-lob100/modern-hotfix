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
    /// 그리는 것: 테마 Background 로 채운 둥근 판 위에 Surface 색 미니 그리드(헤더 행 · 선택 행(SelectionBackground + 액센트 띠 + 작은 액센트 버튼) ·
    /// 교대 행), 아래에 테마 이름(액센트색)과 상태 낱말. 마우스 오버는 현재 테마 액센트 1px 링, 선택은 FocusBorder 2px 링 + 오른쪽 위 체크 배지.
    /// 상태 낱말은 하나만 — Current(지금 켜진 테마) 가 Saved(파일에 저장된 테마) 보다 먼저다.
    ///
    /// 키: ←→↑↓ · Home · End 는 입력 키로 받아 KeyDown 으로 올린다(이동은 창이 처리). Enter 는 창의 AcceptButton(Save)으로 간다.
    /// </summary>
    public sealed class ThemeTile : Control
    {
        private const int CornerRadius = 8;
        private const int CardRadius = 4;
        private const int Inset = 10;
        private const int GridRowHeight = 12;
        private const int CellBarHeight = 2;
        private const int MiniButtonWidth = 22;
        private const int AccentBarWidth = 3;
        private const int CheckBadgeDiameter = 18;
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

            // 미니 그리드 — 그 테마의 Surface 카드 안에 헤더 행 · 선택 행(액센트 띠 + 작은 액센트 버튼) · 교대 행.
            // 다크 테마끼리는 바탕 · 카드가 다 회색 계열이라, 테마마다 다른 선택 행 색과 액센트를 넓게 보여야 갈린다(2026-10-23).
            Rectangle card = new Rectangle(Inset, Inset, this.Width - Inset * 2, GridRowHeight * 3);
            Rectangle header = new Rectangle(card.X, card.Y, card.Width, GridRowHeight);
            Rectangle selected = new Rectangle(card.X, card.Y + GridRowHeight, card.Width, GridRowHeight);
            Rectangle alternate = new Rectangle(card.X, card.Y + GridRowHeight * 2, card.Width, GridRowHeight);
            GraphicsState state = graphics.Save();

            using (GraphicsPath path = CreateRoundedPath(card, CardRadius))
            {
                graphics.SetClip(path);
            }

            FillRow(graphics, card, surface);
            FillRow(graphics, header, ModernTheme.GridHeaderBackgroundOf(this.mode));
            FillRow(graphics, selected, ModernTheme.SelectionBackgroundOf(this.mode));
            FillRow(graphics, alternate, ModernTheme.GridRowAltOf(this.mode));

            // 행 안의 짧은 가로 막대 = 글자 자리. 헤더 · 교대 행은 그 테마 글자색(옅게), 선택 행은 선택 글자색.
            DrawCells(graphics, header, Color.FromArgb(150, text));
            DrawCells(graphics, selected, ModernTheme.SelectedTextOf(this.mode));
            DrawCells(graphics, alternate, Color.FromArgb(110, text));
            FillRow(graphics, new Rectangle(card.X, selected.Y, AccentBarWidth, selected.Height), accent);
            DrawMiniButton(graphics, selected, accent);
            graphics.Restore(state);

            // 테마 이름은 그 테마의 액센트색 — 글자 자체가 시그니처다. 상태 낱말은 같은 줄 오른쪽, 그 테마의 글자색.
            string status = this.isCurrent ? CurrentWord : (this.isSaved ? SavedWord : string.Empty);
            Rectangle line = new Rectangle(Inset, card.Bottom + 4, this.Width - Inset * 2, this.Height - card.Bottom - 4 - Inset + 2);
            int statusWidth = 0;

            if (status.Length > 0)
            {
                statusWidth = TextRenderer.MeasureText(graphics, status, ModernFonts.Body, line.Size, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine).Width;
                TextRenderer.DrawText(graphics, status, ModernFonts.Body, line, text,
                        TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
            }

            Rectangle name = new Rectangle(line.X, line.Y, Math.Max(0, line.Width - statusWidth - (statusWidth > 0 ? 8 : 0)), line.Height);
            TextRenderer.DrawText(graphics, this.mode.ToString(), ModernFonts.BodyStrong, name, accent,
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

        private static void FillRow(Graphics graphics, Rectangle row, Color fill)
        {
            using (SolidBrush brush = new SolidBrush(fill))
            {
                graphics.FillRectangle(brush, row);
            }
        }

        // 글자 자리 막대 세 개 — 폭 18 · 12 · 10, 간격 6. 선택 행의 액센트 띠 뒤에서 시작한다.
        private static void DrawCells(Graphics graphics, Rectangle row, Color color)
        {
            int[] widths = { 18, 12, 10 };
            int x = row.X + AccentBarWidth + 6;
            int y = row.Y + (row.Height - CellBarHeight) / 2;

            using (SolidBrush brush = new SolidBrush(color))
            {
                foreach (int width in widths)
                {
                    graphics.FillRectangle(brush, new Rectangle(x, y, width, CellBarHeight));
                    x += width + 6;
                }
            }
        }

        // 선택 행 오른쪽의 작은 액센트 버튼 — 액센트 채움 + 대비색 글자 막대. 액센트가 면으로 보이게.
        private static void DrawMiniButton(Graphics graphics, Rectangle row, Color accent)
        {
            Rectangle button = new Rectangle(row.Right - 6 - MiniButtonWidth, row.Y + 2, MiniButtonWidth, row.Height - 4);

            using (GraphicsPath path = CreateRoundedPath(button, 2))
            using (SolidBrush brush = new SolidBrush(accent))
            {
                graphics.FillPath(brush, path);
            }

            Rectangle label = new Rectangle(button.X + 6, button.Y + (button.Height - CellBarHeight) / 2, button.Width - 12, CellBarHeight);
            FillRow(graphics, label, ContrastOn(accent));
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
