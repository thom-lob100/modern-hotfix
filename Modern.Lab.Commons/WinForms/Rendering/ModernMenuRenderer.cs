using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Modern.Lab.Theming;

namespace Modern.Lab.WinForms.Rendering
{
    /// <summary>
    /// <see cref="ContextMenuStrip"/>·<see cref="ToolStripDropDown"/>용 모던 렌더러.
    /// 기본 Professional 렌더러의 그라데이션 이미지 여백, 파란 상자가 둘린 체크 표시,
    /// 시스템 하이라이트를 테마 토큰 표면으로 바꾼다 — 체크 표시는 상자 없이 액센트색
    /// 체크 글리프(Segoe MDL2 Assets)만 그리고, hover는 옅은 표면색 둥근 사각이다.
    /// 메뉴 안의 <see cref="ToolStripLabel"/>은 <b>구획 제목</b>으로 본다 — 그리드 헤더와
    /// 같은 옅은 띠 배경 위에 SemiBold 본문색으로 그려 비활성 항목(회색)과 구분된다.
    /// 색은 그릴 때 <see cref="ModernTheme"/>에서 읽으므로 전 테마 자동 대응한다.
    /// 사용법: <c>menu.Renderer = new ModernMenuRenderer();</c> 한 줄.
    /// </summary>
    public sealed class ModernMenuRenderer : ToolStripProfessionalRenderer
    {
        // hover 사각의 좌우 안쪽 여백과 반경 — 메뉴 폭에 딱 붙지 않게 2px 띄운다.
        // hover 틴트 — 액센트를 표면 위에 12% 섞는다.
        private const double HoverTintRatio = 0.12;

        private const int HoverInset = 2;
        private const int HoverRadius = 4;

        // 체크 글리프 크기(DIU) — 메뉴 본문(12 DIU)과 같은 크기라 글자와 나란히 읽힌다.
        private const double CheckGlyphDiu = 12d;

        // 항목 글리프 크기(DIU) — 본문보다 한 단계 커야 작은 획이 뭉개지지 않는다. 글자와의 간격,
        // 글리프를 그릴 수 있는 최소 여백 폭(px).
        private const double ItemGlyphDiu = 14d;
        private const int GlyphTextGap = 4;
        private const int GlyphMinMargin = 14;

        // 구획 제목 글자 — 본문(12)보다 한 단계 작은 SemiBold(2026-08-29 확정: 띠는 그대로, 글자만 작게).
        private const double SectionHeaderDiu = 11d;

        // Segoe MDL2 Assets CheckMark.
        private const string CheckGlyph = "";

        public ModernMenuRenderer()
            : base(new ModernMenuColorTable())
        {
            this.RoundedEdges = false;
        }

        /// <summary>메뉴 표면 — 테마 Surface 단색 + 구획 제목 띠.</summary>
        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(ModernTheme.Surface))
            {
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
            }

            PaintSectionHeaderBands(e.Graphics, e.ToolStrip);
        }

        /// <summary>이미지 여백 — 그라데이션 없이 표면과 같은 색. 구획 제목 띠는 여백 위로도 이어진다.</summary>
        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(ModernTheme.Surface))
            {
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
            }

            PaintSectionHeaderBands(e.Graphics, e.ToolStrip);
        }

        // 구획 제목 띠는 항목 그리기 단계가 아니라 메뉴 배경 단계에서 **메뉴 폭 전체**로
        // 칠한다 — 항목 그리기는 자기 영역으로 잘려 라벨 폭만큼만 칠해지고, 이미지
        // 여백이 뒤에 다시 칠해져 띠가 끊기기 때문이다. 좌표는 메뉴 기준(항목 Bounds).
        // (서브헤더식 — 작은 회색 대문자 + 액센트 바 — 시안은 사용자가 반려했다(2026-08-29);
        //  띠는 그대로 두고 제목 글자만 한 단계 작게 해 명령과 구분한다.)
        private static void PaintSectionHeaderBands(Graphics graphics, ToolStrip toolStrip)
        {
            if (!(toolStrip is ToolStripDropDown))
            {
                return;
            }

            using (SolidBrush brush = new SolidBrush(ModernTheme.GridHeaderBackground))
            {
                foreach (ToolStripItem item in toolStrip.Items)
                {
                    if (item is ToolStripLabel && item.Visible && item.Bounds.Height > 0)
                    {
                        graphics.FillRectangle(
                                brush, 1, item.Bounds.Y, toolStrip.Width - 2, item.Bounds.Height);
                    }
                }
            }
        }

        /// <summary>드롭다운 테두리 — 테마 Border 1px.</summary>
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            if (!(e.ToolStrip is ToolStripDropDown))
            {
                base.OnRenderToolStripBorder(e);
                return;
            }

            using (Pen pen = new Pen(ModernTheme.Border))
            {
                e.Graphics.DrawRectangle(
                        pen, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
            }
        }

        /// <summary>hover 항목 — 옅은 표면색 둥근 사각. 비활성 항목은 반응하지 않는다.</summary>
        // hover(Selected) = 액센트 12% 틴트(흰 표면에서 #E0EFFA) — 버튼 hover의 "옅은 파랑 틴트"와 같은
        // 언어. 누름(Pressed) = PressedBackground(누름 전용 짝, 2026-10-21 — 선택색이 진한 테마에서 글자가 묻혀 선택색과 분리) — 클릭 순간이 보인다.
        // 처음엔 SurfaceAlt(#F9FAFB)를 썼는데 흰 표면과 6단계 차이라 hover가 없는 것처럼 보였고,
        // HoverBackground(#F3F4F6)도 순백 메뉴 위에서는 흐렸다(2026-08-29).
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (!e.Item.Enabled || (!e.Item.Selected && !e.Item.Pressed))
            {
                return;
            }

            Rectangle bounds = new Rectangle(
                    HoverInset, 0, e.Item.Width - HoverInset * 2, e.Item.Height - 1);
            Color fill = e.Item.Pressed
                    ? ModernTheme.PressedBackground
                    : ModernTokenColors.Blend(ModernTheme.Accent, ModernTheme.Surface, HoverTintRatio);

            using (GraphicsPath path = RoundedRectangle(bounds, HoverRadius))
            using (SolidBrush brush = new SolidBrush(fill))
            {
                SmoothingMode previous = e.Graphics.SmoothingMode;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillPath(brush, path);
                e.Graphics.SmoothingMode = previous;
            }
        }

        /// <summary>
        /// 구획 제목(<see cref="ToolStripLabel"/>) 배경 — 띠는 배경 단계(PaintSectionHeaderBands)가
        /// 이미 전폭으로 칠했으므로 여기서는 덧칠하지 않는다. 드롭다운 밖(툴바)의 라벨은 기본 그대로다.
        /// </summary>
        protected override void OnRenderLabelBackground(ToolStripItemRenderEventArgs e)
        {
            if (!IsSectionHeader(e.Item))
            {
                base.OnRenderLabelBackground(e);
            }
        }

        /// <summary>체크 표시 — 상자 없이 액센트색 체크 글리프만.</summary>
        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            using (Font glyphFont = ModernFonts.CreateIcon(CheckGlyphDiu))
            {
                TextRenderer.DrawText(
                        e.Graphics,
                        CheckGlyph,
                        glyphFont,
                        e.ImageRectangle,
                        ModernTheme.Accent,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                                | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
            }
        }

        /// <summary>
        /// 글자 — 기본은 TextPrimary, 비활성은 DisabledText, 항목이 직접 지정한 색은 존중한다.
        /// 구획 제목(<see cref="ToolStripLabel"/>)은 한 단계 작은 SemiBold 본문색으로 그린다(구조 요소 규칙) —
        /// 명령과 같은 크기면 제목이 명령으로 읽혔다(2026-08-29).
        /// </summary>
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            if (IsSectionHeader(e.Item))
            {
                using (Font headerFont = ModernFonts.Create(SectionHeaderDiu, true, FontStyle.Regular))
                {
                    e.TextFont = headerFont;
                    e.TextColor = ModernTheme.TextPrimary;
                    base.OnRenderItemText(e);
                }

                return;
            }

            RenderGlyph(e);

            if (!e.Item.Enabled)
            {
                e.TextColor = ModernTheme.DisabledText;
            }
            else if (e.Item.ForeColor == SystemColors.ControlText)
            {
                e.TextColor = ModernTheme.TextPrimary;
            }

            ModernMenuItem accented = e.Item as ModernMenuItem;

            if (accented != null && e.Item.Enabled && HasAccentSuffix(accented, e.Text))
            {
                RenderAccentedText(e, accented);
                return;
            }

            base.OnRenderItemText(e);
        }

        // 항목 글리프 — 글자 왼쪽의 이미지 여백 가운데에 보조 글자색(비활성은 DisabledText)으로 그린다.
        // 글리프는 ModernMenuGlyphs 가 정한다(항목의 Glyph → 항목 글자 사전). 체크된 항목은 그 자리에 체크가
        // 그려지고, 이미지를 직접 가진 항목은 그 이미지가 그려지므로 건너뛴다. 여백이 없는 메뉴(ShowImageMargin=false)도 건너뛴다.
        private static void RenderGlyph(ToolStripItemTextRenderEventArgs e)
        {
            ToolStripMenuItem item = e.Item as ToolStripMenuItem;

            if (item == null || !(item.Owner is ToolStripDropDown) || item.Checked || item.Image != null)
            {
                return;
            }

            string glyph = ModernMenuGlyphs.Resolve(item);
            int marginWidth = e.TextRectangle.Left - GlyphTextGap - HoverInset;

            if (glyph.Length == 0 || marginWidth < GlyphMinMargin)
            {
                return;
            }

            Rectangle bounds = new Rectangle(HoverInset, 0, marginWidth, item.Height);

            using (Font glyphFont = ModernFonts.CreateIcon(ItemGlyphDiu))
            {
                TextRenderer.DrawText(
                        e.Graphics,
                        glyph,
                        glyphFont,
                        bounds,
                        item.Enabled ? ModernTheme.TextSecondary : ModernTheme.DisabledText,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                                | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
            }
        }

        // 텍스트가 강조 단어로 끝나고 그 앞에 글자가 더 있을 때만 두 조각으로 그린다.
        private static bool HasAccentSuffix(ModernMenuItem item, string text)
        {
            return item.AccentText.Length > 0 && text != null && text.Length > item.AccentText.Length
                    && text.EndsWith(item.AccentText, System.StringComparison.Ordinal);
        }

        // 앞부분은 항목 색, 끝 단어는 강조색(비어 있으면 토큰 Brush.ErrorText). 앞부분 폭은 NoPadding 으로 재야 두 조각이 한 글줄로 이어진다
        // (기본 여백을 포함해 재면 끝 단어가 여백 폭만큼 더 밀린다).
        private static void RenderAccentedText(ToolStripItemTextRenderEventArgs e, ModernMenuItem item)
        {
            string prefix = e.Text.Substring(0, e.Text.Length - item.AccentText.Length);
            Color accent = item.AccentColor.IsEmpty ? ModernTokenColors.Get("Brush.ErrorText", Color.FromArgb(196, 43, 28)) : item.AccentColor;
            Size prefixSize = TextRenderer.MeasureText(e.Graphics, prefix, e.TextFont, e.TextRectangle.Size, e.TextFormat | TextFormatFlags.NoPadding);
            TextRenderer.DrawText(e.Graphics, prefix, e.TextFont, e.TextRectangle, e.TextColor, e.TextFormat);
            Rectangle rest = new Rectangle(
                    e.TextRectangle.X + prefixSize.Width, e.TextRectangle.Y,
                    Math.Max(0, e.TextRectangle.Width - prefixSize.Width), e.TextRectangle.Height);
            TextRenderer.DrawText(e.Graphics, item.AccentText, e.TextFont, rest, accent, e.TextFormat);
        }

        // 메뉴(드롭다운) 안의 ToolStripLabel만 구획 제목이다 — 툴바의 라벨은 평범한 글자다.
        private static bool IsSectionHeader(ToolStripItem item)
        {
            return item is ToolStripLabel && item.Owner is ToolStripDropDown;
        }

        /// <summary>하위 메뉴 화살표 — 본문색(비활성은 DisabledText). 시스템 검정 화살표가 새지 않게.</summary>
        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = e.Item != null && !e.Item.Enabled
                    ? ModernTheme.DisabledText
                    : ModernTheme.TextPrimary;
            base.OnRenderArrow(e);
        }

        /// <summary>구분선 — BorderSubtle 1px, 좌우 8px 띄움.</summary>
        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            if (e.Vertical)
            {
                base.OnRenderSeparator(e);
                return;
            }

            int y = e.Item.Height / 2;

            using (Pen pen = new Pen(ModernTheme.BorderSubtle))
            {
                e.Graphics.DrawLine(pen, 8, y, e.Item.Width - 8, y);
            }
        }

        private static GraphicsPath RoundedRectangle(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            if (diameter <= 0 || bounds.Width <= diameter || bounds.Height <= diameter)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180f, 90f);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270f, 90f);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0f, 90f);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90f, 90f);
            path.CloseFigure();
            return path;
        }

        // Professional 렌더러가 직접 그리는 나머지(눌림 배경·체크 상자 등)도 테마 표면으로 —
        // 위 재정의가 다루지 않는 경로가 남아도 시스템 파랑이 새지 않게 한다.
        private sealed class ModernMenuColorTable : ProfessionalColorTable
        {
            public override Color ToolStripDropDownBackground
            {
                get { return ModernTheme.Surface; }
            }

            public override Color ImageMarginGradientBegin
            {
                get { return ModernTheme.Surface; }
            }

            public override Color ImageMarginGradientMiddle
            {
                get { return ModernTheme.Surface; }
            }

            public override Color ImageMarginGradientEnd
            {
                get { return ModernTheme.Surface; }
            }

            public override Color MenuBorder
            {
                get { return ModernTheme.Border; }
            }

            public override Color MenuItemBorder
            {
                get { return Color.Transparent; }
            }

            public override Color MenuItemSelected
            {
                get { return ModernTheme.SurfaceAlt; }
            }

            public override Color MenuItemSelectedGradientBegin
            {
                get { return ModernTheme.SurfaceAlt; }
            }

            public override Color MenuItemSelectedGradientEnd
            {
                get { return ModernTheme.SurfaceAlt; }
            }

            public override Color MenuItemPressedGradientBegin
            {
                get { return ModernTheme.SurfaceAlt; }
            }

            public override Color MenuItemPressedGradientMiddle
            {
                get { return ModernTheme.SurfaceAlt; }
            }

            public override Color MenuItemPressedGradientEnd
            {
                get { return ModernTheme.SurfaceAlt; }
            }

            public override Color SeparatorDark
            {
                get { return ModernTheme.BorderSubtle; }
            }

            public override Color SeparatorLight
            {
                get { return ModernTheme.BorderSubtle; }
            }

            public override Color CheckBackground
            {
                get { return Color.Transparent; }
            }

            public override Color CheckSelectedBackground
            {
                get { return Color.Transparent; }
            }

            public override Color CheckPressedBackground
            {
                get { return Color.Transparent; }
            }
        }
    }
}
