using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Modern.Lab.Controls.Wpf.Common;

namespace Modern.Lab.Controls.Wpf.Display
{
    /// <summary>
    /// 큐 목록 — 보드 카드 안에서 랏(또는 FOUP) 한 줄씩을 같은 문법으로 보인다(2026-11-04).
    /// 한 줄 = 왼쪽 띠(EdgeKind: hold · job · rework) + 자물쇠(Lock) + 제목(Title) · 부제(Subtitle) · 수량(Qty) + 상태 배지(BadgeText · BadgeKind) + 꼬리(Trailing).
    /// 폭이 <see cref="TwoLineBelowWidth"/> 보다 좁으면 부제 · 수량 · 꼬리가 둘째 줄로 내려간다(폼은 모른다).
    ///
    /// - ItemsSource: 임의의 IEnumerable (DataView, IList, ...)
    /// - *MemberPath: 각 요소로 읽을 컬럼/속성 이름. 글자 · 종류 값은 소스가 만들어 넣고 이 컨트롤은 그리기만 한다(데이터 무관).
    /// - KeyMemberPath: 행 키 — 재바인딩 뒤 같은 키의 줄을 다시 고른다(그리드의 RowKeyMember 와 같은 계약).
    ///
    /// 소스나 멤버 경로가 바뀔 때마다 다시 만들므로 할당 순서는 상관없다. 색 · 글리프는 디자인 토큰(Themes/Tokens.xaml)에서만 온다.
    /// </summary>
    public partial class ModernQueueListControl : UserControl
    {
        /// <summary>왼쪽 띠 종류 — Hold.</summary>
        public const string EdgeHold = "hold";
        /// <summary>왼쪽 띠 종류 — 작업 중.</summary>
        public const string EdgeJob = "job";
        /// <summary>왼쪽 띠 종류 — 재작업.</summary>
        public const string EdgeRework = "rework";

        /// <summary>배지 종류 — 액센트(파랑, 다음 차례).</summary>
        public const string BadgeAccent = "accent";
        /// <summary>배지 종류 — 성공(초록, 작업 중).</summary>
        public const string BadgeSuccess = "success";
        /// <summary>배지 종류 — 오류(빨강, 재작업 · Down).</summary>
        public const string BadgeError = "error";
        /// <summary>배지 종류 — 경고(노랑, Hold · 기한).</summary>
        public const string BadgeWarning = "warning";
        /// <summary>배지 종류 — 중립(회색, 대기).</summary>
        public const string BadgeNeutral = "neutral";

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(ModernQueueListControl),
                new PropertyMetadata(null, OnDataShapeChanged));

        public static readonly DependencyProperty KeyMemberPathProperty =
            DependencyProperty.Register("KeyMemberPath", typeof(string), typeof(ModernQueueListControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        public static readonly DependencyProperty TitleMemberPathProperty =
            DependencyProperty.Register("TitleMemberPath", typeof(string), typeof(ModernQueueListControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        public static readonly DependencyProperty SubtitleMemberPathProperty =
            DependencyProperty.Register("SubtitleMemberPath", typeof(string), typeof(ModernQueueListControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        public static readonly DependencyProperty QtyMemberPathProperty =
            DependencyProperty.Register("QtyMemberPath", typeof(string), typeof(ModernQueueListControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        public static readonly DependencyProperty BadgeTextMemberPathProperty =
            DependencyProperty.Register("BadgeTextMemberPath", typeof(string), typeof(ModernQueueListControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        public static readonly DependencyProperty BadgeKindMemberPathProperty =
            DependencyProperty.Register("BadgeKindMemberPath", typeof(string), typeof(ModernQueueListControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        public static readonly DependencyProperty EdgeKindMemberPathProperty =
            DependencyProperty.Register("EdgeKindMemberPath", typeof(string), typeof(ModernQueueListControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        public static readonly DependencyProperty LockMemberPathProperty =
            DependencyProperty.Register("LockMemberPath", typeof(string), typeof(ModernQueueListControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        public static readonly DependencyProperty TrailingMemberPathProperty =
            DependencyProperty.Register("TrailingMemberPath", typeof(string), typeof(ModernQueueListControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        public static readonly DependencyProperty ToolTipMemberPathProperty =
            DependencyProperty.Register("ToolTipMemberPath", typeof(string), typeof(ModernQueueListControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>제목 앞 작은 태그 칩(예: 단계 이름)으로 읽을 컬럼/속성 이름. 비면 칩 없음.</summary>
        public static readonly DependencyProperty TagMemberPathProperty =
            DependencyProperty.Register("TagMemberPath", typeof(string), typeof(ModernQueueListControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        public static readonly DependencyProperty IndentMemberPathProperty =
            DependencyProperty.Register("IndentMemberPath", typeof(string), typeof(ModernQueueListControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>이 폭(DIU)보다 좁으면 두 줄로 그린다. 0 이면 늘 한 줄.</summary>
        public static readonly DependencyProperty TwoLineBelowWidthProperty =
            DependencyProperty.Register("TwoLineBelowWidth", typeof(double), typeof(ModernQueueListControl),
                new PropertyMetadata(230d, OnTwoLineBelowWidthChanged));

        /// <summary>지금 두 줄로 그리는지 — 폭에서 유도되며 XAML 트리거가 읽는다.</summary>
        public static readonly DependencyProperty TwoLineProperty =
            DependencyProperty.Register("TwoLine", typeof(bool), typeof(ModernQueueListControl),
                new PropertyMetadata(false));

        /// <summary>줄이 없을 때 가운데에 보이는 글자.</summary>
        public static readonly DependencyProperty EmptyTextProperty =
            DependencyProperty.Register("EmptyText", typeof(string), typeof(ModernQueueListControl),
                new PropertyMetadata("None"));

        /// <summary>고른 줄의 원본 항목(DataRowView 등). 밖에서 넣으면 같은 키의 줄을 고른다.</summary>
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(ModernQueueListControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

        /// <summary>고른 줄이 바뀌면 발생한다 — 재바인딩 한 번에 많아야 한 번.</summary>
        public event EventHandler SelectionChanged;

        /// <summary>줄을 클릭하면(마우스를 뗄 때) 발생한다 — FOUP 줄을 펼치는 데 쓴다.</summary>
        public event EventHandler<QueueRowEventArgs> RowClick;

        /// <summary>줄을 더블클릭하면 발생한다 — 상세 팝업을 여는 데 쓴다.</summary>
        public event EventHandler<QueueRowEventArgs> RowDoubleClick;

        private readonly ObservableCollection<QueueRow> rows;
        private bool syncingSelection;

        public ModernQueueListControl()
        {
            this.rows = new ObservableCollection<QueueRow>();
            this.InitializeComponent();
            this.RowList.ItemsSource = this.rows;
            this.SizeChanged += this.OnControlSizeChanged;
            this.UpdateEmptyLabel();
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)this.GetValue(ItemsSourceProperty); }
            set { this.SetValue(ItemsSourceProperty, value); }
        }

        public string KeyMemberPath
        {
            get { return (string)this.GetValue(KeyMemberPathProperty); }
            set { this.SetValue(KeyMemberPathProperty, value); }
        }

        public string TitleMemberPath
        {
            get { return (string)this.GetValue(TitleMemberPathProperty); }
            set { this.SetValue(TitleMemberPathProperty, value); }
        }

        public string SubtitleMemberPath
        {
            get { return (string)this.GetValue(SubtitleMemberPathProperty); }
            set { this.SetValue(SubtitleMemberPathProperty, value); }
        }

        public string QtyMemberPath
        {
            get { return (string)this.GetValue(QtyMemberPathProperty); }
            set { this.SetValue(QtyMemberPathProperty, value); }
        }

        public string BadgeTextMemberPath
        {
            get { return (string)this.GetValue(BadgeTextMemberPathProperty); }
            set { this.SetValue(BadgeTextMemberPathProperty, value); }
        }

        public string BadgeKindMemberPath
        {
            get { return (string)this.GetValue(BadgeKindMemberPathProperty); }
            set { this.SetValue(BadgeKindMemberPathProperty, value); }
        }

        public string EdgeKindMemberPath
        {
            get { return (string)this.GetValue(EdgeKindMemberPathProperty); }
            set { this.SetValue(EdgeKindMemberPathProperty, value); }
        }

        public string LockMemberPath
        {
            get { return (string)this.GetValue(LockMemberPathProperty); }
            set { this.SetValue(LockMemberPathProperty, value); }
        }

        public string TrailingMemberPath
        {
            get { return (string)this.GetValue(TrailingMemberPathProperty); }
            set { this.SetValue(TrailingMemberPathProperty, value); }
        }

        public string ToolTipMemberPath
        {
            get { return (string)this.GetValue(ToolTipMemberPathProperty); }
            set { this.SetValue(ToolTipMemberPathProperty, value); }
        }

        public string TagMemberPath
        {
            get { return (string)this.GetValue(TagMemberPathProperty); }
            set { this.SetValue(TagMemberPathProperty, value); }
        }

        public string IndentMemberPath
        {
            get { return (string)this.GetValue(IndentMemberPathProperty); }
            set { this.SetValue(IndentMemberPathProperty, value); }
        }

        public double TwoLineBelowWidth
        {
            get { return (double)this.GetValue(TwoLineBelowWidthProperty); }
            set { this.SetValue(TwoLineBelowWidthProperty, value); }
        }

        public bool TwoLine
        {
            get { return (bool)this.GetValue(TwoLineProperty); }
            private set { this.SetValue(TwoLineProperty, value); }
        }

        public string EmptyText
        {
            get { return (string)this.GetValue(EmptyTextProperty); }
            set { this.SetValue(EmptyTextProperty, value); }
        }

        public object SelectedItem
        {
            get { return this.GetValue(SelectedItemProperty); }
            set { this.SetValue(SelectedItemProperty, value); }
        }

        /// <summary>고른 줄의 행 키. 없으면 빈 문자열.</summary>
        public string SelectedKey
        {
            get
            {
                QueueRow row = this.RowList.SelectedItem as QueueRow;
                return row == null ? string.Empty : row.Key;
            }
        }

        /// <summary>줄 수.</summary>
        public int RowCount
        {
            get { return this.rows.Count; }
        }

        /// <summary>같은 키의 줄을 고른다. 없으면 선택을 비우고 false.</summary>
        public bool SelectKey(string key)
        {
            QueueRow match = this.FindRow(key);
            this.RowList.SelectedItem = match;

            if (match != null)
            {
                this.RowList.ScrollIntoView(match);
            }

            return match != null;
        }

        private static void OnDataShapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernQueueListControl)d).RebuildRows();
        }

        private static void OnTwoLineBelowWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernQueueListControl)d).UpdateTwoLine();
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernQueueListControl control = (ModernQueueListControl)d;

            if (control.syncingSelection)
            {
                return;
            }

            QueueRow match = null;

            foreach (QueueRow row in control.rows)
            {
                if (ReferenceEquals(row.Item, e.NewValue))
                {
                    match = row;
                    break;
                }
            }

            control.RowList.SelectedItem = match;
        }

        private void OnControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateTwoLine();
        }

        private void UpdateTwoLine()
        {
            double limit = this.TwoLineBelowWidth;
            this.TwoLine = limit > 0 && this.ActualWidth > 0 && this.ActualWidth < limit;
        }

        // 소스 행을 줄 모델로 바꾼다 — 고르고 있던 키를 기억해 같은 키의 줄을 다시 고른다. null · 빈 소스 · 없는 컬럼은 빈 값으로 그리고 예외를 내지 않는다.
        private void RebuildRows()
        {
            string keepKey = this.SelectedKey;
            object keepItem = this.SelectedItem;

            this.syncingSelection = true;

            try
            {
                this.rows.Clear();

                IEnumerable source = this.ItemsSource;

                if (source != null)
                {
                    foreach (object item in source)
                    {
                        this.rows.Add(this.ToRow(item));
                    }
                }

                QueueRow match = keepKey.Length == 0 ? null : this.FindRow(keepKey);
                this.RowList.SelectedItem = match;
                this.SetValue(SelectedItemProperty, match == null ? null : match.Item);
            }
            finally
            {
                this.syncingSelection = false;
            }

            this.UpdateEmptyLabel();

            if (!ReferenceEquals(keepItem, this.SelectedItem))
            {
                this.RaiseSelectionChanged();
            }
        }

        private QueueRow ToRow(object item)
        {
            QueueRow row = new QueueRow();
            row.Item = item;
            row.Key = Text(item, this.KeyMemberPath);
            row.Title = Text(item, this.TitleMemberPath);
            row.Tag = Text(item, this.TagMemberPath);
            row.Subtitle = Text(item, this.SubtitleMemberPath);
            row.Qty = Text(item, this.QtyMemberPath);
            row.BadgeText = Text(item, this.BadgeTextMemberPath);
            row.Trailing = Text(item, this.TrailingMemberPath);
            row.IsLocked = IsYes(Text(item, this.LockMemberPath));

            string tooltip = Text(item, this.ToolTipMemberPath);
            row.ToolTipText = tooltip.Length == 0 ? null : tooltip;

            int indent;
            int.TryParse(Text(item, this.IndentMemberPath), NumberStyles.Integer, CultureInfo.InvariantCulture, out indent);
            row.Indent = new Thickness(indent <= 0 ? 0 : indent * 14, 0, 0, 0);

            row.EdgeBrush = this.EdgeBrushOf(Text(item, this.EdgeKindMemberPath));

            Brush badgeForeground;
            row.BadgeBrush = this.BadgeBrushOf(Text(item, this.BadgeKindMemberPath), out badgeForeground);
            row.BadgeForeground = badgeForeground;

            return row;
        }

        private Brush EdgeBrushOf(string kind)
        {
            switch ((kind ?? string.Empty).Trim().ToLowerInvariant())
            {
                case EdgeHold:
                    return this.Token("Brush.Warning");
                case EdgeJob:
                    return this.Token("Brush.Success");
                case EdgeRework:
                    return this.Token("Brush.ErrorBorder");
                default:
                    return Brushes.Transparent;
            }
        }

        private Brush BadgeBrushOf(string kind, out Brush foreground)
        {
            switch ((kind ?? string.Empty).Trim().ToLowerInvariant())
            {
                case BadgeAccent:
                    foreground = this.Token("Brush.OnAccent");
                    return this.Token("Brush.Accent");
                case BadgeSuccess:
                    foreground = this.Token("Brush.OnAccent");
                    return this.Token("Brush.Success");
                case BadgeError:
                    foreground = this.Token("Brush.OnAccent");
                    return this.Token("Brush.ErrorBorder");
                case BadgeWarning:
                    foreground = this.Token("Brush.WarningText");
                    return this.Token("Brush.WarningBackground");
                default:
                    foreground = this.Token("Brush.NeutralText");
                    return this.Token("Brush.NeutralBackground");
            }
        }

        private Brush Token(string key)
        {
            Brush brush = this.TryFindResource(key) as Brush;
            return brush ?? Brushes.Transparent;
        }

        private QueueRow FindRow(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            foreach (QueueRow row in this.rows)
            {
                if (string.Equals(row.Key, key, StringComparison.Ordinal))
                {
                    return row;
                }
            }

            return null;
        }

        private void UpdateEmptyLabel()
        {
            this.EmptyLabel.Visibility = this.rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnRowListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.syncingSelection)
            {
                return;
            }

            QueueRow row = this.RowList.SelectedItem as QueueRow;

            this.syncingSelection = true;

            try
            {
                this.SetValue(SelectedItemProperty, row == null ? null : row.Item);
            }
            finally
            {
                this.syncingSelection = false;
            }

            this.RaiseSelectionChanged();
        }

        private void RaiseSelectionChanged()
        {
            EventHandler handler = this.SelectionChanged;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void OnRowMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.RaiseRowEvent(this.RowClick, sender);
        }

        private void OnRowMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            this.RaiseRowEvent(this.RowDoubleClick, sender);
        }

        private void RaiseRowEvent(EventHandler<QueueRowEventArgs> handler, object sender)
        {
            ListBoxItem container = sender as ListBoxItem;
            QueueRow row = container == null ? null : container.DataContext as QueueRow;

            if (row == null || handler == null)
            {
                return;
            }

            handler(this, new QueueRowEventArgs(row.Item, row.Key));
        }

        private static string Text(object item, string memberPath)
        {
            if (string.IsNullOrEmpty(memberPath))
            {
                return string.Empty;
            }

            object value = MemberPathReader.Read(item, memberPath);
            return value == null ? string.Empty : Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        private static bool IsYes(string text)
        {
            string value = (text ?? string.Empty).Trim();
            return value.Equals("Y", StringComparison.OrdinalIgnoreCase)
                    || value.Equals("true", StringComparison.OrdinalIgnoreCase)
                    || value == "1";
        }

        /// <summary>줄 하나의 그리기 재료 — 소스에서 읽어 둔 글자 · 브러시. 소스가 바뀌면 통째로 다시 만든다.</summary>
        private sealed class QueueRow
        {
            public object Item { get; set; }
            public string Key { get; set; }
            public string Title { get; set; }
            public string Tag { get; set; }
            public string Subtitle { get; set; }
            public string Qty { get; set; }
            public string BadgeText { get; set; }
            public Brush BadgeBrush { get; set; }
            public Brush BadgeForeground { get; set; }
            public Brush EdgeBrush { get; set; }
            public bool IsLocked { get; set; }
            public string Trailing { get; set; }
            public string ToolTipText { get; set; }
            public Thickness Indent { get; set; }

            public bool HasTag
            {
                get { return !string.IsNullOrEmpty(this.Tag); }
            }

            public bool HasBadge
            {
                get { return !string.IsNullOrEmpty(this.BadgeText); }
            }
        }
    }
}
