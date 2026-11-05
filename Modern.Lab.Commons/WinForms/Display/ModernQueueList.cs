using System;
using System.ComponentModel;
using System.Drawing;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Display
{
    /// <summary>
    /// 큐 목록(새 개념으로, 직접 대응하는 WinForms 컨트롤 없음; WPF ModernQueueListControl 을 ElementHost 로 호스팅).
    /// 보드 카드 안에서 랏 · FOUP 한 줄씩을 같은 문법으로 보인다 — 왼쪽 띠 · 자물쇠 · 제목 · 부제 · 수량 · 상태 배지 · 꼬리(2026-11-04).
    ///
    /// 데이터 계약: <see cref="DataSource"/> 가 행을 담고 멤버 아홉이 어느 칸을 어디에 그릴지 가리킨다. 글자 · 종류 값(hold · job · rework /
    /// accent · success · error · warning · neutral)은 폼의 프레젠터가 칸으로 만들어 넣고 컨트롤은 그리기만 한다. 멤버 할당 순서는 상관없다.
    /// </summary>
    [ToolboxItem(true)]
    public class ModernQueueList : WpfElementHostBase<ModernQueueListControl>
    {
        private object dataSource;

        // 디자인 타임 WPF 생성이 실패한 경우(Wpf == null)에도 속성 그리드가 동작하도록 하는 폴백 저장소.
        private string fallbackKeyMember = string.Empty;
        private string fallbackTitleMember = string.Empty;
        private string fallbackSubtitleMember = string.Empty;
        private string fallbackQtyMember = string.Empty;
        private string fallbackBadgeTextMember = string.Empty;
        private string fallbackBadgeKindMember = string.Empty;
        private string fallbackEdgeKindMember = string.Empty;
        private string fallbackLockMember = string.Empty;
        private string fallbackTrailingMember = string.Empty;
        private string fallbackToolTipMember = string.Empty;
        private string fallbackIndentMember = string.Empty;
        private string fallbackTagMember = string.Empty;
        private string fallbackEmptyText = "None";
        private int fallbackTwoLineBelowWidth = 230;

        /// <summary>고른 줄이 바뀌면 발생한다 — 재바인딩 한 번에 많아야 한 번.</summary>
        public event EventHandler SelectionChanged;

        /// <summary>줄을 클릭하면 발생한다.</summary>
        public event EventHandler<QueueRowEventArgs> RowClick;

        /// <summary>줄을 더블클릭하면 발생한다.</summary>
        public event EventHandler<QueueRowEventArgs> RowDoubleClick;

        /// <summary>적절한 기본 크기로 컨트롤을 생성한다.</summary>
        public ModernQueueList()
        {
            this.Size = new Size(240, 200);

            if (this.Wpf != null)
            {
                this.Wpf.SelectionChanged += this.OnInnerSelectionChanged;
                this.Wpf.RowClick += this.OnInnerRowClick;
                this.Wpf.RowDoubleClick += this.OnInnerRowDoubleClick;
            }
        }

        /// <summary>줄 목록: DataTable / DataView / IList / IEnumerable. null 이면 비우고 EmptyText 를 보인다.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object DataSource
        {
            get
            {
                return this.dataSource;
            }
            set
            {
                this.dataSource = value;

                if (this.Wpf != null)
                {
                    this.Wpf.ItemsSource = DataSourceConverter.ToItemsSource(value);
                }
            }
        }

        /// <summary>행 키 컬럼 — 재바인딩 뒤 같은 키의 줄을 다시 고른다.</summary>
        [Category("Modern")]
        [Description("Column/property name of the row key (selection is restored by this key)")]
        [DefaultValue("")]
        public string KeyMember
        {
            get { return this.Wpf != null ? this.Wpf.KeyMemberPath : this.fallbackKeyMember; }
            set { this.fallbackKeyMember = value; if (this.Wpf != null) { this.Wpf.KeyMemberPath = value; } }
        }

        /// <summary>제목 컬럼(굵은 글자, 예: 의뢰서 번호).</summary>
        [Category("Modern")]
        [Description("Column/property name of the bold title (e.g. request number)")]
        [DefaultValue("")]
        public string TitleMember
        {
            get { return this.Wpf != null ? this.Wpf.TitleMemberPath : this.fallbackTitleMember; }
            set { this.fallbackTitleMember = value; if (this.Wpf != null) { this.Wpf.TitleMemberPath = value; } }
        }

        /// <summary>부제 컬럼(보조색 글자, 예: Lot Id).</summary>
        [Category("Modern")]
        [Description("Column/property name of the subtitle (e.g. Lot Id)")]
        [DefaultValue("")]
        public string SubtitleMember
        {
            get { return this.Wpf != null ? this.Wpf.SubtitleMemberPath : this.fallbackSubtitleMember; }
            set { this.fallbackSubtitleMember = value; if (this.Wpf != null) { this.Wpf.SubtitleMemberPath = value; } }
        }

        /// <summary>수량 컬럼.</summary>
        [Category("Modern")]
        [Description("Column/property name of the quantity")]
        [DefaultValue("")]
        public string QtyMember
        {
            get { return this.Wpf != null ? this.Wpf.QtyMemberPath : this.fallbackQtyMember; }
            set { this.fallbackQtyMember = value; if (this.Wpf != null) { this.Wpf.QtyMemberPath = value; } }
        }

        /// <summary>상태 배지 글자 컬럼. 비면 배지를 그리지 않는다.</summary>
        [Category("Modern")]
        [Description("Column/property name of the badge text (empty = no badge)")]
        [DefaultValue("")]
        public string BadgeTextMember
        {
            get { return this.Wpf != null ? this.Wpf.BadgeTextMemberPath : this.fallbackBadgeTextMember; }
            set { this.fallbackBadgeTextMember = value; if (this.Wpf != null) { this.Wpf.BadgeTextMemberPath = value; } }
        }

        /// <summary>배지 종류 컬럼 — accent · success · error · warning · neutral.</summary>
        [Category("Modern")]
        [Description("Column/property name of the badge kind (accent, success, error, warning, neutral)")]
        [DefaultValue("")]
        public string BadgeKindMember
        {
            get { return this.Wpf != null ? this.Wpf.BadgeKindMemberPath : this.fallbackBadgeKindMember; }
            set { this.fallbackBadgeKindMember = value; if (this.Wpf != null) { this.Wpf.BadgeKindMemberPath = value; } }
        }

        /// <summary>왼쪽 띠 종류 컬럼 — hold · job · rework, 그 밖은 띠 없음.</summary>
        [Category("Modern")]
        [Description("Column/property name of the left edge kind (hold, job, rework; anything else = none)")]
        [DefaultValue("")]
        public string EdgeKindMember
        {
            get { return this.Wpf != null ? this.Wpf.EdgeKindMemberPath : this.fallbackEdgeKindMember; }
            set { this.fallbackEdgeKindMember = value; if (this.Wpf != null) { this.Wpf.EdgeKindMemberPath = value; } }
        }

        /// <summary>자물쇠 컬럼 — Y · true · 1 이면 자물쇠를 그린다.</summary>
        [Category("Modern")]
        [Description("Column/property name of the lock flag (Y / true / 1 draws a lock glyph)")]
        [DefaultValue("")]
        public string LockMember
        {
            get { return this.Wpf != null ? this.Wpf.LockMemberPath : this.fallbackLockMember; }
            set { this.fallbackLockMember = value; if (this.Wpf != null) { this.Wpf.LockMemberPath = value; } }
        }

        /// <summary>꼬리 글자 컬럼(오른쪽 끝, 예: 대기시간).</summary>
        [Category("Modern")]
        [Description("Column/property name of the trailing text (e.g. waiting time)")]
        [DefaultValue("")]
        public string TrailingMember
        {
            get { return this.Wpf != null ? this.Wpf.TrailingMemberPath : this.fallbackTrailingMember; }
            set { this.fallbackTrailingMember = value; if (this.Wpf != null) { this.Wpf.TrailingMemberPath = value; } }
        }

        /// <summary>툴팁 글자 컬럼(여러 줄 가능). 비면 툴팁 없음.</summary>
        [Category("Modern")]
        [Description("Column/property name of the tooltip text (multi-line allowed; empty = no tooltip)")]
        [DefaultValue("")]
        public string ToolTipMember
        {
            get { return this.Wpf != null ? this.Wpf.ToolTipMemberPath : this.fallbackToolTipMember; }
            set { this.fallbackToolTipMember = value; if (this.Wpf != null) { this.Wpf.ToolTipMemberPath = value; } }
        }

        /// <summary>제목 앞 작은 태그 칩 컬럼(예: 단계 이름 Send · Arrival · Exchange). 비면 칩 없음.</summary>
        [Category("Modern")]
        [Description("Column/property name of the small tag chip before the title (empty = no chip)")]
        [DefaultValue("")]
        public string TagMember
        {
            get { return this.Wpf != null ? this.Wpf.TagMemberPath : this.fallbackTagMember; }
            set { this.fallbackTagMember = value; if (this.Wpf != null) { this.Wpf.TagMemberPath = value; } }
        }

        /// <summary>들여쓰기 단계 컬럼(0 · 1 · 2 …) — FOUP 아래 랏처럼 묶인 줄에 쓴다.</summary>
        [Category("Modern")]
        [Description("Column/property name of the indent level (0, 1, 2 ...)")]
        [DefaultValue("")]
        public string IndentMember
        {
            get { return this.Wpf != null ? this.Wpf.IndentMemberPath : this.fallbackIndentMember; }
            set { this.fallbackIndentMember = value; if (this.Wpf != null) { this.Wpf.IndentMemberPath = value; } }
        }

        /// <summary>줄이 없을 때 보이는 글자.</summary>
        [Category("Modern")]
        [Description("Text shown when there are no rows")]
        [Localizable(true)]
        [DefaultValue("None")]
        public string EmptyText
        {
            get { return this.Wpf != null ? this.Wpf.EmptyText : this.fallbackEmptyText; }
            set { this.fallbackEmptyText = value; if (this.Wpf != null) { this.Wpf.EmptyText = value; } this.InvalidateDesignTimePreview(); }
        }

        /// <summary>이 폭(픽셀)보다 좁으면 두 줄로 그린다. 0 이면 늘 한 줄.</summary>
        [Category("Modern")]
        [Description("Below this width the row is drawn on two lines (0 = always one line)")]
        [DefaultValue(230)]
        public int TwoLineBelowWidth
        {
            get { return this.Wpf != null ? (int)this.Wpf.TwoLineBelowWidth : this.fallbackTwoLineBelowWidth; }
            set { this.fallbackTwoLineBelowWidth = value; if (this.Wpf != null) { this.Wpf.TwoLineBelowWidth = value; } }
        }

        /// <summary>고른 줄의 원본 항목(DataRowView 등). 없으면 null.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedItem
        {
            get { return this.Wpf != null ? this.Wpf.SelectedItem : null; }
            set { if (this.Wpf != null) { this.Wpf.SelectedItem = value; } }
        }

        /// <summary>고른 줄의 행 키. 없으면 빈 문자열.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedKey
        {
            get { return this.Wpf != null ? this.Wpf.SelectedKey : string.Empty; }
        }

        /// <summary>줄 수.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int RowCount
        {
            get { return this.Wpf != null ? this.Wpf.RowCount : 0; }
        }

        /// <summary>같은 키의 줄을 고른다. 없으면 선택을 비우고 false.</summary>
        public bool SelectKey(string key)
        {
            return this.Wpf != null && this.Wpf.SelectKey(key);
        }

        private void OnInnerSelectionChanged(object sender, EventArgs e)
        {
            EventHandler handler = this.SelectionChanged;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void OnInnerRowClick(object sender, QueueRowEventArgs e)
        {
            EventHandler<QueueRowEventArgs> handler = this.RowClick;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnInnerRowDoubleClick(object sender, QueueRowEventArgs e)
        {
            EventHandler<QueueRowEventArgs> handler = this.RowDoubleClick;

            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
