using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Modern.Lab.Theming;
using Modern.Lab.WinForms.Rendering;

namespace Modern.Lab.WinForms.Controls.Display
{
    /// <summary>
    /// 테마 일치 툴팁 — `System.Windows.Forms.ToolTip`의 드롭인 대체.
    /// 기본 노란 시스템 툴팁 대신 모던 표면(Surface 배경 + Border 1px 테두리 +
    /// TextPrimary 글자, 여백 12×8)으로 그린다. 사용법은 표준 ToolTip 그대로
    /// (`SetToolTip(control, text)`), 여러 줄 텍스트(\n)도 지원한다.
    ///
    /// 글자 한 줄로 모자라면 <see cref="SetCard"/>로 호버 카드를 건다 — 왼쪽 상태색 띠 + 굵은 제목 +
    /// 라벨 · 값 줄 + 맨 아래 안내 한 줄(<see cref="ModernToolTipCard"/>). 좁은 칩(자식 장비)의 상태를
    /// 마우스를 올려 읽게 하려는 것이다(2026-10-22 사용자 요청). 카드를 건 컨트롤에 `SetToolTip` 을 다시
    /// 부르면 카드는 걷히고 글자 툴팁이 된다.
    ///
    /// 꺼진 컨트롤(Enabled = false)은 마우스 메시지를 받지 못해 표준 툴팁이 뜨지 않는다 — 꺼진 버튼 · 칩의 "왜 꺼졌나"
    /// 사유가 바로 그 자리라서, 이 툴팁은 부모의 마우스 이동을 보고 꺼진 자식 위에 오면 직접 띄운다(2026-10-22).
    /// </summary>
    [ToolboxItem(true)]
    public class ModernToolTip : ToolTip
    {
        private const int PadX = 12;
        private const int PadY = 8;
        // 카드 — 왼쪽 띠 4px, 띠와 내용 사이 10px, 줄 사이 3px, 라벨과 값 사이 12px, 안내 위 구분선 여백 6px.
        private const int CardStripeWidth = 4;
        private const int CardStripeGap = 10;
        private const int CardRowGap = 3;
        private const int CardColumnGap = 12;
        private const int CardNoteGap = 6;
        private const int CardMaxNoteWidth = 320;

        private readonly Dictionary<Control, ModernToolTipCard> cards = new Dictionary<Control, ModernToolTipCard>();
        private readonly HashSet<Control> watchedParents = new HashSet<Control>();
        private readonly HashSet<Control> registered = new HashSet<Control>();
        private Control disabledShown;

        /// <summary>테마 스타일 툴팁을 만든다.</summary>
        public ModernToolTip()
        {
            this.Initialize();
        }

        /// <summary>디자이너 컨테이너용 생성자 (표준 ToolTip과 동일 시그니처).</summary>
        public ModernToolTip(IContainer container)
            : base(container)
        {
            this.Initialize();
        }

        /// <summary>
        /// 컨트롤에 호버 카드를 건다. <paramref name="card"/>가 null 이면 카드와 툴팁을 모두 걷는다.
        /// 카드의 제목(없으면 첫 값)이 툴팁 글자로 등록되어 표준 `GetToolTip` 에도 그 글자가 나온다.
        /// </summary>
        public void SetCard(Control control, ModernToolTipCard card)
        {
            if (control == null)
            {
                return;
            }

            if (card == null)
            {
                this.cards.Remove(control);
                base.SetToolTip(control, string.Empty);
                return;
            }

            this.cards[control] = card;
            base.SetToolTip(control, card.TriggerText());
            this.Register(control);
        }

        /// <summary>컨트롤에 걸린 호버 카드. 없으면 null.</summary>
        public ModernToolTipCard GetCard(Control control)
        {
            ModernToolTipCard card;
            return control != null && this.cards.TryGetValue(control, out card) ? card : null;
        }

        /// <summary>글자 툴팁을 건다 — 카드가 걸려 있었다면 걷는다(표준 ToolTip 과 같은 호출).</summary>
        public new void SetToolTip(Control control, string caption)
        {
            if (control != null)
            {
                this.cards.Remove(control);
            }

            base.SetToolTip(control, caption);

            if (control != null && !string.IsNullOrEmpty(caption))
            {
                this.Register(control);
            }
        }

        private void Initialize()
        {
            this.OwnerDraw = true;
            this.Popup += this.OnToolTipPopup;
            this.Draw += this.OnToolTipDraw;
        }

        // 꺼진 자식도 툴팁이 뜨게 부모의 마우스 이동을 본다. 부모가 아직 없으면 붙는 순간 본다.
        private void Register(Control control)
        {
            if (!this.registered.Add(control))
            {
                this.WatchParent(control.Parent);
                return;
            }

            control.ParentChanged += this.OnRegisteredParentChanged;
            control.Disposed += this.OnRegisteredDisposed;
            this.WatchParent(control.Parent);
        }

        private void OnRegisteredParentChanged(object sender, EventArgs e)
        {
            Control control = sender as Control;

            if (control != null)
            {
                this.WatchParent(control.Parent);
            }
        }

        private void OnRegisteredDisposed(object sender, EventArgs e)
        {
            Control control = sender as Control;

            if (control == null)
            {
                return;
            }

            control.ParentChanged -= this.OnRegisteredParentChanged;
            control.Disposed -= this.OnRegisteredDisposed;
            this.registered.Remove(control);
            this.cards.Remove(control);

            if (this.disabledShown == control)
            {
                this.disabledShown = null;
            }
        }

        private void WatchParent(Control parent)
        {
            if (parent == null || !this.watchedParents.Add(parent))
            {
                return;
            }

            parent.MouseMove += this.OnParentMouseMove;
            parent.MouseLeave += this.OnParentMouseLeave;
        }

        // 꺼진 자식 위에 들어오면 그 자식 아래에 띄우고, 벗어나거나 켜진 자식으로 옮기면 거둔다.
        private void OnParentMouseMove(object sender, MouseEventArgs e)
        {
            Control parent = sender as Control;

            if (parent == null)
            {
                return;
            }

            Control child = parent.GetChildAtPoint(e.Location, GetChildAtPointSkip.Invisible);
            bool over = child != null && !child.Enabled && this.registered.Contains(child)
                    && !string.IsNullOrEmpty(this.GetToolTip(child));

            if (!over)
            {
                this.HideDisabled();
                return;
            }

            if (this.disabledShown == child)
            {
                return;
            }

            this.HideDisabled();
            this.disabledShown = child;
            this.Show(this.GetToolTip(child), child, this.PlaceBelowOrAbove(child), this.AutoPopDelay);
        }

        // 꺼진 컨트롤 아래에 붙이되, 화면 아래가 모자라면 위에, 오른쪽이 모자라면 왼쪽으로 당긴다 — 직접 띄우는 툴팁은
        // 시스템이 자리를 잡아 주지 않는다.
        private Point PlaceBelowOrAbove(Control control)
        {
            Size size = this.MeasureFor(control);
            Rectangle screen = Screen.FromControl(control).WorkingArea;
            Point origin = control.PointToScreen(Point.Empty);
            int x = 0;
            int y = control.Height + 2;

            if (origin.Y + y + size.Height > screen.Bottom)
            {
                y = -(size.Height + 2);
            }

            if (origin.X + size.Width > screen.Right)
            {
                x = Math.Max(screen.Left - origin.X, screen.Right - size.Width - origin.X);
            }

            return new Point(x, y);
        }

        // Popup 이 재는 것과 같은 크기 — 카드면 카드, 아니면 글자 + 여백.
        private Size MeasureFor(Control control)
        {
            ModernToolTipCard card = this.GetCard(control);

            if (card != null)
            {
                return MeasureCard(card);
            }

            Size textSize = TextRenderer.MeasureText(
                    this.GetToolTip(control) ?? string.Empty, ModernFonts.Body, Size.Empty,
                    TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            return new Size(textSize.Width + PadX * 2, textSize.Height + PadY * 2);
        }

        private void OnParentMouseLeave(object sender, EventArgs e)
        {
            this.HideDisabled();
        }

        private void HideDisabled()
        {
            Control shown = this.disabledShown;

            if (shown == null)
            {
                return;
            }

            this.disabledShown = null;

            if (!shown.IsDisposed)
            {
                this.Hide(shown);
            }
        }

        // 크기 측정 — 본문 글꼴 + 여백. 여러 줄(\n)은 줄바꿈 그대로 잰다. 카드는 제목 · 줄 · 안내를 쌓아 잰다.
        private void OnToolTipPopup(object sender, PopupEventArgs e)
        {
            ModernToolTipCard card = this.GetCard(e.AssociatedControl);

            if (card != null)
            {
                e.ToolTipSize = MeasureCard(card);
                return;
            }

            string text = this.GetToolTip(e.AssociatedControl) ?? string.Empty;
            Size textSize = TextRenderer.MeasureText(
                    text, ModernFonts.Body, Size.Empty,
                    TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            e.ToolTipSize = new Size(textSize.Width + PadX * 2, textSize.Height + PadY * 2);
        }

        // 그리기 — Surface 배경 + Border 1px 테두리 + TextPrimary 글자. 카드는 그 위에 띠 · 제목 · 줄 · 안내를 얹는다.
        private void OnToolTipDraw(object sender, DrawToolTipEventArgs e)
        {
            Color surface = ModernTokenColors.Get("Brush.Surface", Color.White);
            Color border = ModernTokenColors.Get("Brush.Border", Color.FromArgb(209, 213, 219));
            Color ink = ModernTheme.TextPrimary;

            using (SolidBrush fill = new SolidBrush(surface))
            {
                e.Graphics.FillRectangle(fill, e.Bounds);
            }

            using (Pen edge = new Pen(border))
            {
                e.Graphics.DrawRectangle(
                        edge, e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
            }

            ModernToolTipCard card = this.GetCard(e.AssociatedControl);

            if (card != null)
            {
                DrawCard(e.Graphics, e.Bounds, card, ink);
                return;
            }

            Rectangle textBounds = new Rectangle(
                    e.Bounds.X + PadX, e.Bounds.Y + PadY,
                    e.Bounds.Width - PadX * 2, e.Bounds.Height - PadY * 2);
            TextRenderer.DrawText(
                    e.Graphics, e.ToolTipText, ModernFonts.Body, textBounds, ink,
                    TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }

        private static TextFormatFlags LineFlags
        {
            get { return TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine; }
        }

        private static TextFormatFlags NoteFlags
        {
            get { return TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak; }
        }

        // 카드 크기 — 라벨 열은 가장 긴 라벨에 맞추고, 값 열은 가장 긴 값에 맞춘다. 안내는 최대 폭 안에서 접는다.
        private static Size MeasureCard(ModernToolTipCard card)
        {
            int labelWidth = 0;
            int valueWidth = 0;
            int rowHeight = 0;
            int height = PadY;
            int width = 0;

            if (card.Title.Length > 0)
            {
                Size titleSize = TextRenderer.MeasureText(card.Title, ModernFonts.BodyStrong, Size.Empty, LineFlags);
                width = Math.Max(width, titleSize.Width);
                height += titleSize.Height + CardRowGap;
            }

            for (int index = 0; index < card.RowCount; index++)
            {
                Size labelSize = TextRenderer.MeasureText(card.LabelAt(index), ModernFonts.Body, Size.Empty, LineFlags);
                Size valueSize = TextRenderer.MeasureText(card.ValueAt(index), ModernFonts.BodyStrong, Size.Empty, LineFlags);
                labelWidth = Math.Max(labelWidth, labelSize.Width);
                valueWidth = Math.Max(valueWidth, valueSize.Width);
                rowHeight = Math.Max(rowHeight, Math.Max(labelSize.Height, valueSize.Height));
            }

            if (card.RowCount > 0)
            {
                width = Math.Max(width, labelWidth + CardColumnGap + valueWidth);
                height += (rowHeight + CardRowGap) * card.RowCount;
            }

            if (card.Note.Length > 0)
            {
                int noteWidth = Math.Max(width, Math.Min(CardMaxNoteWidth, TextRenderer.MeasureText(card.Note, ModernFonts.Body, Size.Empty, LineFlags).Width));
                Size noteSize = TextRenderer.MeasureText(card.Note, ModernFonts.Body, new Size(noteWidth, int.MaxValue), NoteFlags);
                width = Math.Max(width, noteSize.Width);
                height += CardNoteGap + 1 + CardNoteGap + noteSize.Height;
            }

            height += PadY;
            int stripe = card.AccentColorToken.Length > 0 ? CardStripeWidth + CardStripeGap : 0;
            return new Size(PadX + stripe + width + PadX, height);
        }

        private static void DrawCard(Graphics graphics, Rectangle bounds, ModernToolTipCard card, Color ink)
        {
            Color secondary = ModernTheme.TextSecondary;
            Color subtle = ModernTokenColors.Get("Brush.BorderSubtle", Color.FromArgb(229, 231, 235));
            int left = bounds.X + PadX;

            if (card.AccentColorToken.Length > 0)
            {
                Color accent = ModernTokenColors.Get(card.AccentColorToken, secondary);
                Rectangle stripe = new Rectangle(left, bounds.Y + PadY, CardStripeWidth, bounds.Height - (PadY * 2));

                using (SolidBrush brush = new SolidBrush(accent))
                {
                    graphics.FillRectangle(brush, stripe);
                }

                left += CardStripeWidth + CardStripeGap;
            }

            int right = bounds.Right - PadX;
            int top = bounds.Y + PadY;

            if (card.Title.Length > 0)
            {
                Size titleSize = TextRenderer.MeasureText(card.Title, ModernFonts.BodyStrong, Size.Empty, LineFlags);
                TextRenderer.DrawText(
                        graphics, card.Title, ModernFonts.BodyStrong,
                        new Rectangle(left, top, right - left, titleSize.Height), ink, LineFlags);
                top += titleSize.Height + CardRowGap;
            }

            int labelWidth = 0;
            int rowHeight = 0;

            for (int index = 0; index < card.RowCount; index++)
            {
                Size labelSize = TextRenderer.MeasureText(card.LabelAt(index), ModernFonts.Body, Size.Empty, LineFlags);
                Size valueSize = TextRenderer.MeasureText(card.ValueAt(index), ModernFonts.BodyStrong, Size.Empty, LineFlags);
                labelWidth = Math.Max(labelWidth, labelSize.Width);
                rowHeight = Math.Max(rowHeight, Math.Max(labelSize.Height, valueSize.Height));
            }

            for (int index = 0; index < card.RowCount; index++)
            {
                TextRenderer.DrawText(
                        graphics, card.LabelAt(index), ModernFonts.Body,
                        new Rectangle(left, top, labelWidth, rowHeight), secondary, LineFlags | TextFormatFlags.VerticalCenter);
                string token = card.ValueColorTokenAt(index);
                Color valueInk = token.Length > 0 ? ModernTokenColors.Get(token, ink) : ink;
                int valueLeft = left + labelWidth + CardColumnGap;
                TextRenderer.DrawText(
                        graphics, card.ValueAt(index), ModernFonts.BodyStrong,
                        new Rectangle(valueLeft, top, Math.Max(0, right - valueLeft), rowHeight), valueInk, LineFlags | TextFormatFlags.VerticalCenter);
                top += rowHeight + CardRowGap;
            }

            if (card.Note.Length > 0)
            {
                top += CardNoteGap;

                using (Pen pen = new Pen(subtle))
                {
                    graphics.DrawLine(pen, left, top, right, top);
                }

                top += 1 + CardNoteGap;
                TextRenderer.DrawText(
                        graphics, card.Note, ModernFonts.Body,
                        new Rectangle(left, top, right - left, bounds.Bottom - PadY - top), secondary, NoteFlags);
            }
        }
    }
}
