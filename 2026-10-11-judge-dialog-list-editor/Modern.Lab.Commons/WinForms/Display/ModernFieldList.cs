using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

using Modern.Lab.Theming;
using Modern.Lab.WinForms.Rendering;

namespace Modern.Lab.WinForms.Controls.Display
{
    /// <summary>
    /// 단건 상세 필드 목록 — Lot 정보 카드처럼 "한 건의 레코드"를 캡션/값
    /// 쌍으로 보여준다. 레거시 "괘선 상세 표"(캡션 셀 회색 배경 +
    /// TableLayoutPanel + 라벨 2N개, 또는 ModernDetailTable)의 모던 대안으로,
    /// **캡션(작은 회색) 위 + 값(SemiBold) 아래**의 스택형 배치를 쓴다 —
    /// 괘선과 셀 배경 없이 여백과 굵기로 위계를 만든다 (DevExpress Detail
    /// View / Fluent 설정 화면의 관례).
    ///
    /// 사용법 (수동형 데이터 계약 — 폼이 조회해서 넣는다):
    ///   fieldLotInfo.Columns = 2;
    ///   fieldLotInfo.DefineFields(
    ///       new ModernFieldDefinition("Product", "MODEL_ID"), ...);
    ///   fieldLotInfo.SetRow(row);      // DataRow — 필드의 Member 컬럼을 읽는다
    ///   fieldLotInfo.ClearValues();    // 전부 "-"
    ///
    /// 배지 필드(IsBadge, 2026-09-15): 값 자리에 ModernStatusBadge가 놓인다 — 배경색은 값에서
    /// 유도되므로(ColorValue) 같은 값이면 그리드 배지와 색이 맞고, BadgeAccentValues로 강조할 값을,
    /// BadgeSpinValues로 테두리가 도는 값을 선언한다. 값이 비면 배지 대신 "-"를 그린다.
    ///
    /// 링크 필드(IsLink, 2026-08-29): 값이 액센트색으로 그려지고 마우스 오버 시 밑줄·손 커서,
    /// 클릭하면 FieldLinkClick(Member, Value)가 난다 — 그리드의 Link 컬럼과 같은 역할이라
    /// 의뢰 번호처럼 표에서 링크인 값이 상세 카드에서도 같은 팝업을 연다. 빈 값("-")은 링크가 아니다.
    ///
    /// 표시 전용이라 ModernLabel과 같은 GDI+ 직접 그리기 컨트롤이다 —
    /// 폼당 라벨 2N개(WPF 섬 2N개)를 쓰던 자리를 컨트롤 1개로 줄인다.
    /// 값이 비면 "-"로 표기한다. 행 높이는 컨트롤 Size를 행 수로 등분한다.
    ///
    /// 캡션 배치(LabelPlacement, 2026-09-30): 기본 Top 은 위 설명대로이고, Left 는 캡션 칸(LabelWidth)을 왼쪽에 두고 값을
    /// 오른쪽에 — 조회 조건 라벨처럼 보인다. 캡션은 두 배치 모두 **한 줄**이다(2026-10-04) — 캡션 사전의 위·본·아래 글씨는
    /// 공백으로 이은 한 줄("Request Serial No")로 쓴다. Left 에서는 캡션이 CaptionAlignment 대로 칸 안에 놓이고 값·배지는 칸 높이 가운데다.
    /// </summary>
    [ToolboxItem(true)]
    [DesignerCategory("Code")]
    public class ModernFieldList : Control
    {
        // 캡션과 값 사이 간격/캡션 줄 높이 — 스택형 쌍의 내부 리듬. Top 캡션을 10 크기로 줄이며(2026-09-30) 17에서 14로 좁혔다.
        private const int captionHeight = 14;

        // Top 배치 캡션 글꼴 크기 — 본문(12)보다 한 단계 작은 10(토큰 Font.Size.SubLabel 과 같은 값, 2026-09-30).
        private const double topCaptionDiu = 10d;

        // 셀 좌우 여백.
        private const int cellPadding = 2;

        // 배지 필드의 배지 높이(96 DPI 기준) — 화면 배율만큼 키우고, 값 영역이 이보다 낮으면 값 영역에 맞춘다.
        // 배지 글자는 배율을 따라 커지므로 높이를 픽셀로 고정하면 125% 이상에서 위아래 여백이 사라진다(2026-10-05).
        // 22 는 ModernStatusBadge 의 글자 16 + 세로 여백 3×2 와 같다(2026-10-12: 24 에서 줄임 — 그리드 배지와 같은 높이).
        private const int badgeHeight = 22;

        private readonly List<ModernFieldDefinition> definitions;
        private readonly List<string> values;

        // 배지 필드의 자식 배지 — 배지가 아닌 자리는 null이다. 배지 시각을 여기서 다시
        // 그리지 않고 ModernStatusBadge를 그대로 얹는다(동작의 출처는 하나).
        private readonly List<ModernStatusBadge> badges;

        // 배지 필드가 하나라도 있는지 — 없으면 동기화 자체를 건너뛴다(기존 화면 비용 0).
        private bool hasBadges;

        // 지금 자식 배지가 어떤 정의로 만들어졌는지 — 정의가 그대로면 다시 만들지 않는다.
        private readonly List<string> badgeKeys;

        private int columns;
        private double fontWidthRatio;

        // 행 높이(96 DPI 기준 px) — 0(기본)이면 autoRowHeight 를 쓰고, 컨트롤이 그보다 낮으면 높이를 줄 수로 등분한다.
        // 정하면 줄 수와 무관하게 그 높이다. 어느 쪽이든 남는 공간은 위아래로 반씩 나뉜다(세로 가운데) —
        // 줄 수·카드 높이가 화면마다 달라도 같은 밀도·같은 위아래 여백으로 보이게 한다(2026-10-11 사용자).
        private int rowHeight;

        // 기본 행 높이 — 캡션 줄 14 + 값 줄(배지 24 가 온전히 들어가는 높이). 전에는 컨트롤 높이를 줄 수로 등분해
        // 1줄·2줄·3줄 카드의 줄 간격이 제각각이었다.
        private const int autoRowHeight = 38;

        // 캡션 배치 — Top(기본) 또는 Left. Left 배치의 캡션 칸 폭·정렬(칸 기준).
        private FieldLabelPlacement labelPlacement = FieldLabelPlacement.Top;
        private int labelWidth = 96;
        private HorizontalAlignment captionAlignment = HorizontalAlignment.Left;

        // Left 배치에서 캡션 칸과 값 사이 간격.
        private const int labelGap = 8;

        // 마우스가 올라가 있는 링크 필드 — 없으면 -1. 밑줄과 손 커서의 기준.
        private int hoverIndex = -1;

        /// <summary>적절한 기본 크기로 컨트롤을 생성한다.</summary>
        public ModernFieldList()
        {
            this.SetStyle(
                    ControlStyles.AllPaintingInWmPaint
                    | ControlStyles.OptimizedDoubleBuffer
                    | ControlStyles.UserPaint
                    | ControlStyles.ResizeRedraw
                    | ControlStyles.SupportsTransparentBackColor,
                    true);

            this.definitions = new List<ModernFieldDefinition>();
            this.values = new List<string>();
            this.badges = new List<ModernStatusBadge>();
            this.badgeKeys = new List<string>();
            this.columns = 2;
            this.fontWidthRatio = 0d;

            this.Size = new Size(400, 160);
            this.TabStop = false;
            this.BackColor = Color.Transparent;
        }

        /// <summary>
        /// 링크 필드(<see cref="ModernFieldDefinition.IsLink"/>)의 값을 클릭했을 때 — 인자에 Member와 값.
        /// 값이 비어("-") 있으면 나지 않는다.
        /// </summary>
        [Category("Modern")]
        [Description("Raised when the value of a link field (IsLink) is clicked")]
        public event EventHandler<ModernFieldLinkClickEventArgs> FieldLinkClick;

        /// <summary>열 수 (기본 2). 필드는 왼쪽→오른쪽, 위→아래로 채워진다.</summary>
        [Category("Modern")]
        [Description("Number of columns — fields flow left to right, top to bottom")]
        [DefaultValue(2)]
        public int Columns
        {
            get { return this.columns; }
            set
            {
                this.columns = Math.Max(1, value);
                this.SyncBadges();
                this.Invalidate();
            }
        }

        /// <summary>행 높이(96 DPI 기준 px, 배율 비례). 0(기본) = 기본 높이 38 — 컨트롤이 그보다 낮으면 줄 수로 등분해 잘리지 않게 한다.
        /// 정하면 줄 수와 무관하게 그 높이이고 줄이 넘치면 아래가 잘린다. 어느 쪽이든 줄 묶음은 세로 가운데에 놓인다.</summary>
        [Category("Modern")]
        [Description("Row height in px at 96 DPI; 0 uses the default 38 (shrinks to fit when the control is shorter). Rows are centered vertically")]
        [DefaultValue(0)]
        public int RowHeight
        {
            get { return this.rowHeight; }
            set
            {
                this.rowHeight = Math.Max(0, value);
                this.SyncBadges();
                this.Invalidate();
            }
        }

        /// <summary>캡션 배치(2026-09-30) — Top(기본) = 캡션 위·값 아래, Left = 캡션 칸 왼쪽·값 오른쪽.</summary>
        [Category("Modern")]
        [Description("Caption placement — Top (above value) or Left (caption column beside value)")]
        [DefaultValue(FieldLabelPlacement.Top)]
        public FieldLabelPlacement LabelPlacement
        {
            get { return this.labelPlacement; }
            set
            {
                this.labelPlacement = value;
                this.SyncBadges();
                this.Invalidate();
            }
        }

        /// <summary>Left 배치의 캡션 칸 폭(px, 기본 96) — 셀 폭에서 이 폭과 간격 8 을 뺀 나머지가 값 영역이다.</summary>
        [Category("Modern")]
        [Description("Caption column width for Left placement (px)")]
        [DefaultValue(96)]
        public int LabelWidth
        {
            get { return this.labelWidth; }
            set
            {
                this.labelWidth = Math.Max(0, value);
                this.SyncBadges();
                this.Invalidate();
            }
        }

        /// <summary>Left 배치에서 본 캡션이 캡션 칸 안에 놓이는 자리(기본 Left) — Right 면 값에 붙는다.</summary>
        [Category("Modern")]
        [Description("Alignment of the main caption inside the caption column (Left placement)")]
        [DefaultValue(HorizontalAlignment.Left)]
        public HorizontalAlignment CaptionAlignment
        {
            get { return this.captionAlignment; }
            set
            {
                this.captionAlignment = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 장평(글자 가로 비율) 재정의. 0 = 전역(ModernTheme.FontWidthRatio) 사용.
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
                this.SyncBadges();
                this.Invalidate();
            }
        }

        /// <summary>
        /// 기존 `.Designer.cs`가 직렬화해 둔 `xxx.Child = null;` 라인이 계속
        /// 컴파일되도록 남겨 둔 무동작 속성 (drop-in 계약).
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

        /// <summary>
        /// 필드 배치를 정의한다 — 캡션과 값을 읽어 올 컬럼 이름의 쌍.
        /// 값은 전부 "-"로 초기화된다.
        /// </summary>
        public void DefineFields(params ModernFieldDefinition[] fields)
        {
            this.definitions.Clear();
            this.values.Clear();

            if (fields != null)
            {
                foreach (ModernFieldDefinition field in fields)
                {
                    if (field != null)
                    {
                        this.definitions.Add(field);
                        this.values.Add(string.Empty);
                    }
                }
            }

            this.RebuildBadges();
            this.SyncBadges();
            this.Invalidate();
        }

        /// <summary>
        /// 한 건의 행에서 각 필드의 Member 컬럼 값을 읽어 채운다.
        /// 없는 컬럼/빈 값은 "-"로 표기한다 (예외 없음 — 계약 룰 3).
        /// </summary>
        public void SetRow(DataRow row)
        {
            // 정의가 없으면 행이 속한 표가 정한다 — 화면은 SetRow만 부르면 되고,
            // 골라 보여 줄 때만 DefineFields로 선언한다 (규칙은 AutoColumns).
            if (this.definitions.Count == 0 && row != null && row.Table != null)
            {
                string[] members = Modern.Lab.Controls.Wpf.Data.AutoColumns.MembersOf(row.Table);
                ModernFieldDefinition[] fields = new ModernFieldDefinition[members.Length];

                for (int index = 0; index < members.Length; index++)
                {
                    fields[index] = new ModernFieldDefinition(members[index]);
                }

                this.DefineFields(fields);
            }

            for (int index = 0; index < this.definitions.Count; index++)
            {
                this.values[index] = ReadCell(row, this.definitions[index].Member, this.definitions[index].Format);
            }

            this.SyncBadges();
            this.Invalidate();
        }

        /// <summary>특정 필드의 값을 직접 지정한다 (행에 없는 파생 값용).</summary>
        public void SetValue(string member, string value)
        {
            for (int index = 0; index < this.definitions.Count; index++)
            {
                if (string.Equals(this.definitions[index].Member, member, StringComparison.Ordinal))
                {
                    this.values[index] = value ?? string.Empty;
                }
            }

            this.SyncBadges();
            this.Invalidate();
        }

        /// <summary>모든 값을 "-"로 되돌린다 (선택 해제/초기화).</summary>
        public void ClearValues()
        {
            for (int index = 0; index < this.values.Count; index++)
            {
                this.values[index] = string.Empty;
            }

            this.SyncBadges();
            this.Invalidate();
        }

        // 형식이 있으면 그리드 셀과 같은 서식 함수를 쓴다 — 같은 값이 표와 카드에서 다르게 보이지 않게.
        private static string ReadCell(DataRow row, string member, string format)
        {
            if (row == null || string.IsNullOrEmpty(member)
                    || !row.Table.Columns.Contains(member) || row.IsNull(member))
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(format))
            {
                return Modern.Lab.Controls.Wpf.Data.GridAutoFitMeasurer.FormatCellText(row[member], format).Trim();
            }

            return Convert.ToString(row[member]).Trim();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (this.definitions.Count == 0 || this.Width <= 0 || this.Height <= 0)
            {
                return;
            }

            Graphics graphics = e.Graphics;
            double ratio = ModernTheme.ResolveFontWidthRatio(this.fontWidthRatio);
            Rectangle[] valueRects = this.ValueRectangles();
            Rectangle[] cells = this.CellRectangles();

            Color captionColor = Token("Brush.TextSecondary", Color.FromArgb(96, 96, 96));
            Color valueColor = this.Enabled
                ? Token("Brush.TextPrimary", Color.Black)
                : Token("Brush.DisabledText", Color.FromArgb(161, 161, 161));
            Color linkColor = this.Enabled
                ? Token("Brush.Accent", ModernTheme.Accent)
                : valueColor;
            Color linkHoverColor = this.Enabled
                ? Token("Brush.AccentHover", linkColor)
                : valueColor;

            const TextFormatFlags flags = TextFormatFlags.NoPadding
                    | TextFormatFlags.SingleLine
                    | TextFormatFlags.NoPrefix
                    | TextFormatFlags.EndEllipsis;

            // 캡션은 본문 크기 Regular 회색, 값은 같은 크기 SemiBold — 크기가
            // 아니라 굵기·색으로 위계를 만든다 (Fluent Body / Body Strong).
            // 링크 값은 액센트색 + 항상 밑줄 — 마우스를 올리기 전에도 클릭하면 팝업이
            // 열린다는 것을 알 수 있어야 한다(그리드 Link 컬럼·ModernLabel Hyperlink와 같은 체감).
            // 호버는 색만 한 단계 진하게(AccentHover) 바꾸고 밑줄은 그대로 둔다.
            using (Font captionFont = ModernFonts.Create(ModernFonts.BodyDiu, false, FontStyle.Regular))
            using (Font valueFont = ModernFonts.Create(ModernFonts.BodyDiu, true, FontStyle.Regular))
            using (Font linkFont = ModernFonts.Create(ModernFonts.BodyDiu, true, FontStyle.Underline))
            {
                for (int index = 0; index < this.definitions.Count; index++)
                {
                    Rectangle valueRect = valueRects[index];

                    if (this.labelPlacement == FieldLabelPlacement.Left)
                    {
                        this.PaintLeftCaption(graphics, this.definitions[index], captionFont, captionColor, cells[index], flags, ratio);
                    }
                    else
                    {
                        Rectangle captionRect = new Rectangle(
                                valueRect.Left, valueRect.Top - captionHeight, valueRect.Width, captionHeight);

                        using (Font topCaptionFont = ModernFonts.Create(topCaptionDiu, false, FontStyle.Regular))
                        {
                            ScaledTextRenderer.DrawText(
                                    graphics, this.definitions[index].Caption, topCaptionFont,
                                    captionRect, captionColor, flags | TextFormatFlags.Bottom, ratio);
                        }
                    }

                    // 배지 필드의 값은 자식 배지가 그린다 — 캡션만 여기서 그리고 넘어간다.
                    if (this.IsBadgeAt(index))
                    {
                        continue;
                    }

                    string value = this.values[index];
                    bool link = this.IsLinkAt(index);

                    Color textColor = valueColor;

                    if (link)
                    {
                        textColor = index == this.hoverIndex ? linkHoverColor : linkColor;
                    }

                    // 값은 값 영역의 세로 가운데 — 배지(24px)와 글 값이 같은 높이에 서야 배지가 있는 줄과 없는 줄의
                    // 간격이 같아 보인다(2026-10-11 사용자 보고: 1·2줄과 2·3줄 간격이 다르다). Top 배치도 같다.
                    ScaledTextRenderer.DrawText(
                            graphics, value.Length > 0 ? value : "-",
                            link ? linkFont : valueFont,
                            valueRect, textColor,
                            flags | TextFormatFlags.VerticalCenter,
                            ratio);
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            this.UpdateHover(this.HitLink(e.Location));
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this.UpdateHover(-1);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            int index = this.HitLink(e.Location);

            if (index < 0)
            {
                return;
            }

            EventHandler<ModernFieldLinkClickEventArgs> handler = this.FieldLinkClick;

            if (handler != null)
            {
                handler(this, new ModernFieldLinkClickEventArgs(
                        this.definitions[index].Member, this.values[index]));
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            this.SyncBadges();
            this.UpdateHover(-1);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.SyncBadges();
        }

        // 링크로 동작하는 필드인지 — IsLink이고 값이 있고(빈 "-"는 링크가 아니다) 컨트롤이 활성일 때.
        private bool IsLinkAt(int index)
        {
            return this.Enabled
                    && this.definitions[index].IsLink
                    && !this.IsBadgeAt(index)
                    && this.values[index].Length > 0;
        }

        // 좌표가 어느 링크 필드의 값 영역에 있는지 — 없으면 -1.
        private int HitLink(Point location)
        {
            if (this.definitions.Count == 0 || this.Width <= 0 || this.Height <= 0)
            {
                return -1;
            }

            Rectangle[] valueRects = this.ValueRectangles();

            for (int index = 0; index < this.definitions.Count; index++)
            {
                if (this.IsLinkAt(index) && valueRects[index].Contains(location))
                {
                    return index;
                }
            }

            return -1;
        }

        private void UpdateHover(int index)
        {
            if (index == this.hoverIndex)
            {
                return;
            }

            this.hoverIndex = index;
            this.Cursor = index >= 0 ? Cursors.Hand : Cursors.Default;
            this.Invalidate();
        }

        // 각 필드의 값 영역 — 그리기와 히트테스트가 같은 배치를 쓴다. Top 은 칸의 캡션 줄 아래, Left 는 캡션 칸(LabelWidth + 간격) 오른쪽이다.
        private Rectangle[] ValueRectangles()
        {
            Rectangle[] cells = this.CellRectangles();
            Rectangle[] rects = new Rectangle[cells.Length];

            for (int index = 0; index < cells.Length; index++)
            {
                Rectangle cell = cells[index];
                rects[index] = this.labelPlacement == FieldLabelPlacement.Left
                        ? new Rectangle(cell.Left + this.labelWidth + labelGap, cell.Top, Math.Max(0, cell.Width - this.labelWidth - labelGap), cell.Height)
                        : new Rectangle(cell.Left, cell.Top + captionHeight, cell.Width, Math.Max(0, cell.Height - captionHeight));
            }

            return rects;
        }

        // 각 필드의 칸(좌우 여백을 뺀 셀 전체) — 열 병합을 고려해 (열 위치, 행, 폭)을 먼저 정한다: 남은 열보다 넓은 필드는
        // 다음 줄로 내리고 줄 끝까지 채운다. 시작 열(StartColumn)을 정한 필드는 그 열까지 칸을 비우고, 이미 지났으면 다음 줄의 그 열로 간다.
        private Rectangle[] CellRectangles()
        {
            int count = this.definitions.Count;
            int[] cellColumn = new int[count];
            int[] cellRow = new int[count];
            int[] cellSpan = new int[count];

            int cursorColumn = 0;
            int cursorRow = 0;

            for (int index = 0; index < count; index++)
            {
                int span = Math.Min(this.columns, this.definitions[index].ColumnSpan);
                int startColumn = this.definitions[index].StartColumn;

                if (startColumn > 0)
                {
                    int target = Math.Min(startColumn, this.columns) - 1;

                    if (cursorColumn > target)
                    {
                        cursorRow = cursorRow + 1;
                    }

                    cursorColumn = target;
                }

                if (cursorColumn > 0 && cursorColumn + span > this.columns)
                {
                    cursorColumn = 0;
                    cursorRow = cursorRow + 1;
                }

                cellColumn[index] = cursorColumn;
                cellRow[index] = cursorRow;
                cellSpan[index] = Math.Min(span, this.columns - cursorColumn);

                cursorColumn = cursorColumn + cellSpan[index];

                if (cursorColumn >= this.columns)
                {
                    cursorColumn = 0;
                    cursorRow = cursorRow + 1;
                }
            }

            int rows = cursorColumn > 0 ? cursorRow + 1 : cursorRow;
            int rowCount = Math.Max(1, rows);
            int rowHeight = this.rowHeight > 0
                    ? (int)Math.Ceiling(this.rowHeight * (this.DeviceDpi / 96d))
                    : Math.Min((int)Math.Ceiling(autoRowHeight * (this.DeviceDpi / 96d)), this.Height / rowCount);
            int originTop = Math.Max(0, (this.Height - rowHeight * rowCount) / 2);
            Rectangle[] rects = new Rectangle[count];

            for (int index = 0; index < count; index++)
            {
                int left = this.Width * cellColumn[index] / this.columns;
                int right = this.Width * (cellColumn[index] + cellSpan[index]) / this.columns;
                int top = originTop + cellRow[index] * rowHeight;

                rects[index] = new Rectangle(left + cellPadding, top, right - left - cellPadding * 2, rowHeight);
            }

            return rects;
        }

        // Left 배치의 캡션 칸 — 한 줄 캡션을 칸 높이 가운데에, CaptionAlignment 대로 칸 안에 놓는다. 칸보다 길면 말줄임한다.
        private void PaintLeftCaption(
                Graphics graphics, ModernFieldDefinition field, Font captionFont, Color color, Rectangle cell,
                TextFormatFlags flags, double ratio)
        {
            Rectangle area = new Rectangle(cell.Left, cell.Top, Math.Min(this.labelWidth, cell.Width), cell.Height);
            string caption = field.Caption ?? string.Empty;

            // 재는 동안은 말줄임 플래그를 뺀다 — 제안 크기 0 에 EndEllipsis 가 붙으면 "…" 폭만 돌아온다.
            TextFormatFlags measureFlags = flags & ~TextFormatFlags.EndEllipsis;
            Size size = ScaledTextRenderer.MeasureText(graphics, caption, captionFont, Size.Empty, measureFlags, ratio);
            int width = Math.Min(size.Width, area.Width);
            int left = AlignedLeft(area.Left, area.Width, width, this.captionAlignment);
            int top = area.Top + Math.Max(0, (area.Height - size.Height) / 2);

            ScaledTextRenderer.DrawText(
                    graphics, caption, captionFont, new Rectangle(left, top, width, size.Height), color, flags, ratio);
        }

        // 정렬에 따른 덩어리의 왼쪽 — Left 는 시작점, Right 는 영역 오른쪽 끝에 붙이고, Center 는 가운데다. 덩어리가 영역보다 넓으면 시작점이다.
        private static int AlignedLeft(int start, int available, int blockWidth, HorizontalAlignment alignment)
        {
            int slack = Math.Max(0, available - blockWidth);
            return alignment == HorizontalAlignment.Right ? start + slack
                    : alignment == HorizontalAlignment.Center ? start + slack / 2
                    : start;
        }

        // 배지로 그려지는 필드인지 — IsBadge이고 값이 있을 때. 값이 비면 배지 대신 "-"를 그린다.
        private bool IsBadgeAt(int index)
        {
            return this.hasBadges
                    && index < this.badges.Count
                    && this.badges[index] != null
                    && this.values[index].Length > 0;
        }

        // 정의가 바뀌면 자식 배지를 다시 만든다. 값에 따라 달라지는 것(Text·ColorValue)은
        // SyncBadges가 맡고, 정의에 매인 것(강조·회전 값 목록)만 여기서 준다.
        private void RebuildBadges()
        {
            string[] keys = new string[this.definitions.Count];

            for (int index = 0; index < this.definitions.Count; index++)
            {
                ModernFieldDefinition field = this.definitions[index];

                keys[index] = field.IsBadge
                        ? field.Member + "|" + (field.BadgeAccentValues ?? string.Empty)
                                + "|" + (field.BadgeSpinValues ?? string.Empty)
                        : string.Empty;
            }

            // 조회마다 FieldDefinitions.Apply 를 부르는 것이 이 카드의 표준 사용법이다.
            // 정의가 그대로인데 자식을 버리고 다시 만들면 회전 위상이 0으로 튀고 핸들이
            // 매 갱신마다 다시 생긴다 — 같은 지문이면 쓰던 배지를 그대로 둔다.
            if (this.SameBadgeKeys(keys))
            {
                return;
            }

            foreach (ModernStatusBadge existing in this.badges)
            {
                if (existing != null)
                {
                    this.Controls.Remove(existing);
                    existing.Dispose();
                }
            }

            this.badges.Clear();
            this.hasBadges = false;

            foreach (ModernFieldDefinition field in this.definitions)
            {
                ModernStatusBadge badge = null;

                if (field.IsBadge)
                {
                    badge = new ModernStatusBadge();
                    badge.TabStop = false;
                    badge.Visible = false;

                    // 생성자 기본 Text("Status")를 지운다 — 첫 값이 마침 "Status"면
                    // 아래 SyncBadges 의 중복 대입 가드에 걸려 ColorValue 가 비게 된다.
                    badge.Text = string.Empty;
                    badge.AccentValues = field.BadgeAccentValues ?? string.Empty;
                    badge.SpinValues = field.BadgeSpinValues ?? string.Empty;
                    this.Controls.Add(badge);
                    this.hasBadges = true;
                }

                this.badges.Add(badge);
            }

            this.badgeKeys.Clear();
            this.badgeKeys.AddRange(keys);
        }

        // 지금 자식 배지가 이 정의로 만들어진 것인지 — 지문이 하나라도 다르면 다시 만든다.
        private bool SameBadgeKeys(string[] keys)
        {
            if (this.badgeKeys.Count != keys.Length || this.badges.Count != keys.Length)
            {
                return false;
            }

            for (int index = 0; index < keys.Length; index++)
            {
                if (!string.Equals(this.badgeKeys[index], keys[index], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        // 값과 배치를 자식 배지에 옮긴다. 색은 값에서 유도되므로(ColorValue) 같은 값이면
        // 그리드 배지와 색이 맞는다. 값이 비면 배지를 숨겨 "-"가 보이게 둔다.
        private void SyncBadges()
        {
            if (!this.hasBadges || this.badges.Count != this.definitions.Count
                    || this.Width <= 0 || this.Height <= 0)
            {
                return;
            }

            Rectangle[] valueRects = this.ValueRectangles();

            for (int index = 0; index < this.badges.Count; index++)
            {
                ModernStatusBadge badge = this.badges[index];

                if (badge == null)
                {
                    continue;
                }

                string value = this.values[index];

                if (value.Length == 0)
                {
                    badge.Visible = false;
                    continue;
                }

                // 같은 값을 다시 넣지 않는다 — Text 대입이 회전 판정과 다시 그리기를 부른다.
                if (!string.Equals(badge.Text, value, StringComparison.Ordinal))
                {
                    badge.Text = value;
                    badge.ColorValue = value;
                }

                Rectangle valueRect = valueRects[index];

                badge.FontWidthRatio = this.fontWidthRatio;
                badge.Enabled = this.Enabled;
                int scaledBadgeHeight = (int)Math.Ceiling(badgeHeight * (this.DeviceDpi / 96d));
                int badgeTall = Math.Min(valueRect.Height, scaledBadgeHeight);
                // 배지도 값 영역의 세로 가운데 — 글 값과 같은 기준선(위 DrawText 와 짝).
                int badgeTop = valueRect.Top + (valueRect.Height - badgeTall) / 2;
                badge.Bounds = new Rectangle(valueRect.Left, badgeTop, valueRect.Width, badgeTall);
                badge.Visible = true;
            }
        }

        private static Color Token(string key, Color fallback)
        {
            return ModernTokenColors.Get(key, fallback);
        }
    }
}
