using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Modern.Lab.Controls.Wpf.Common;
using Modern.Lab.Theming;

namespace Modern.Lab.Controls.Wpf.Display
{
    /// <summary>
    /// 공정 맵 — 단계 노드를 격자에 놓고 흐름 · 가지 · 되돌림 연결선으로 잇는 한 장짜리 도식이다.
    ///
    /// - 잎 위젯이다: 노드 · 연결 · 내부 칸 세 소스를 <see cref="SetMapSource"/> 로 통째로 받아 그리기만 한다(재조회 = 재구성).
    ///   어느 컬럼을 읽을지는 <c>Node*Member</c> · <c>Link*Member</c> · <c>Cell*Member</c> 가 정하고, 상태 판정 · 건수 · 문구는 폼이 넣는다.
    /// - 노드 자리는 소스의 열 · 행 · 걸침 값이 정하고 격자 크기는 <see cref="ColumnWidths"/> · <see cref="RowHeights"/> 가 정한다.
    /// - 톤은 <see cref="SemanticKind"/> 이름이다(Success · Warning · Error · Info · Neutral). 모든 상태는 글자와 함께 그리고 색만으로 말하지 않는다.
    /// - 선 굵기와 노드 크기는 물량과 무관하다. 연결선은 노드 배치가 끝난 뒤 노드 경계에서 다시 계산한다.
    /// - 노드가 좁아지면 장비 Id 를 다음 줄로 내리고, 단계 요약을 툴팁으로 보내고, 내부 칸을 2열로 접고, 아래 상세 줄은 들어가는 만큼만 보인다.
    /// - <see cref="SelectedKey"/> 대입과 소스 교체는 이벤트를 내지 않는다. 사용자가 누를 때만 <see cref="NodeClick"/> 이 난다.
    ///   소스를 바꿔도 같은 키의 노드가 있으면 선택이 남고 없으면 비워진다.
    /// </summary>
    public partial class ModernProcessMapControl : UserControl
    {
        // 장비 노드 폭이 이보다 좁으면 장비 Id 를 제목 줄에서 다음 줄로 내린다.
        private const double compactEquipmentWidth = 170d;

        // 단계 노드 폭이 이보다 좁으면 오른쪽 요약을 숨기고 툴팁으로만 보인다.
        private const double compactFlowWidth = 150d;

        // 내부 칸 하나의 최소 폭 — 네 칸이 이 폭으로 들어가지 않으면 2열로 접는다.
        private const double cellMinWidth = 50d;

        // 단계 노드 칸이 이보다 낮으면 이름 · 건수를 한 줄로 접는다.
        private const double shortFlowHeight = 52d;

        // 가지 노드 칸이 이보다 낮으면 요약 줄을 숨긴다.
        private const double shortBranchHeight = 58d;

        // 높이가 모자랄 때 고정 행을 줄이는 하한과, 남겨 둘 * 행 높이.
        private const double rowFloor = 46d;
        private const double starReserve = 90d;

        private const double cellGap = 6d;
        private const double linkThickness = 1.5d;
        private const double reworkLift = 30d;

        private readonly Dictionary<string, string> members = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly List<MapNode> nodes = new List<MapNode>();
        private readonly List<MapLink> links = new List<MapLink>();
        private readonly List<MapCell> cells = new List<MapCell>();
        private readonly Dictionary<string, NodeView> views = new Dictionary<string, NodeView>(StringComparer.Ordinal);
        private readonly List<GridLength> baseHeights = new List<GridLength>();

        private IEnumerable nodeSource;
        private IEnumerable linkSource;
        private IEnumerable cellSource;
        private bool linksPending;

        /// <summary>선택된 노드 키. 빈 값이면 선택 없음. 대입은 이벤트를 내지 않는다.</summary>
        public static readonly DependencyProperty SelectedKeyProperty =
            DependencyProperty.Register(
                "SelectedKey", typeof(string), typeof(ModernProcessMapControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedKeyChanged));

        /// <summary>열 폭 목록 — 공백이나 쉼표로 나눈 GridLength 표기(예: "1.7* 2.8* 4.4* 2.8* 1*"). 모자란 열은 1* 로 채운다.</summary>
        public static readonly DependencyProperty ColumnWidthsProperty =
            DependencyProperty.Register(
                "ColumnWidths", typeof(string), typeof(ModernProcessMapControl),
                new PropertyMetadata(string.Empty, OnLayoutChanged));

        /// <summary>행 높이 목록 — 공백이나 쉼표로 나눈 GridLength 표기(예: "56 56 56 56 *"). 모자란 행은 1* 로 채운다.</summary>
        public static readonly DependencyProperty RowHeightsProperty =
            DependencyProperty.Register(
                "RowHeights", typeof(string), typeof(ModernProcessMapControl),
                new PropertyMetadata(string.Empty, OnLayoutChanged));

        /// <summary>사용자가 노드를 누르면 발생한다 — 이미 선택된 노드를 다시 눌러도 난다.</summary>
        public event EventHandler<ProcessMapNodeEventArgs> NodeClick;

        /// <summary>컨트롤을 초기화한다.</summary>
        public ModernProcessMapControl()
        {
            this.InitializeComponent();
            this.MapRoot.SizeChanged += this.OnMapSizeChanged;
        }

        /// <summary>선택된 노드 키.</summary>
        public string SelectedKey
        {
            get { return (string)this.GetValue(SelectedKeyProperty); }
            set { this.SetValue(SelectedKeyProperty, value ?? string.Empty); }
        }

        /// <summary>열 폭 목록(GridLength 표기).</summary>
        public string ColumnWidths
        {
            get { return (string)this.GetValue(ColumnWidthsProperty); }
            set { this.SetValue(ColumnWidthsProperty, value ?? string.Empty); }
        }

        /// <summary>행 높이 목록(GridLength 표기).</summary>
        public string RowHeights
        {
            get { return (string)this.GetValue(RowHeightsProperty); }
            set { this.SetValue(RowHeightsProperty, value ?? string.Empty); }
        }

        /// <summary>노드 행에서 키를 읽을 컬럼/속성 이름.</summary>
        public string NodeKeyMember
        {
            get { return this.Member("NodeKey"); }
            set { this.SetMember("NodeKey", value); }
        }

        /// <summary>노드 행에서 모양(<see cref="ProcessMapNodeKind"/> 이름)을 읽을 컬럼/속성 이름.</summary>
        public string NodeKindMember
        {
            get { return this.Member("NodeKind"); }
            set { this.SetMember("NodeKind", value); }
        }

        /// <summary>노드 행에서 격자 열 번호(0부터)를 읽을 컬럼/속성 이름.</summary>
        public string NodeColumnMember
        {
            get { return this.Member("NodeColumn"); }
            set { this.SetMember("NodeColumn", value); }
        }

        /// <summary>노드 행에서 격자 행 번호(0부터)를 읽을 컬럼/속성 이름.</summary>
        public string NodeRowMember
        {
            get { return this.Member("NodeRow"); }
            set { this.SetMember("NodeRow", value); }
        }

        /// <summary>노드 행에서 걸치는 열 수를 읽을 컬럼/속성 이름. 비면 1.</summary>
        public string NodeColumnSpanMember
        {
            get { return this.Member("NodeColumnSpan"); }
            set { this.SetMember("NodeColumnSpan", value); }
        }

        /// <summary>노드 행에서 걸치는 행 수를 읽을 컬럼/속성 이름. 비면 1.</summary>
        public string NodeRowSpanMember
        {
            get { return this.Member("NodeRowSpan"); }
            set { this.SetMember("NodeRowSpan", value); }
        }

        /// <summary>노드 행에서 이름(PREP 등)을 읽을 컬럼/속성 이름.</summary>
        public string NodeTitleMember
        {
            get { return this.Member("NodeTitle"); }
            set { this.SetMember("NodeTitle", value); }
        }

        /// <summary>노드 행에서 장비 Id 처럼 이름 옆에 붙는 작은 글자를 읽을 컬럼/속성 이름.</summary>
        public string NodeIdMember
        {
            get { return this.Member("NodeId"); }
            set { this.SetMember("NodeId", value); }
        }

        /// <summary>노드 행에서 상태 줄(예: "Run · 18m")을 읽을 컬럼/속성 이름. 톤 색으로 그린다.</summary>
        public string NodeStateMember
        {
            get { return this.Member("NodeState"); }
            set { this.SetMember("NodeState", value); }
        }

        /// <summary>노드 행에서 톤(<see cref="SemanticKind"/> 이름)을 읽을 컬럼/속성 이름 — 램프 · 상태 줄 · 가지 테두리 색.</summary>
        public string NodeToneMember
        {
            get { return this.Member("NodeTone"); }
            set { this.SetMember("NodeTone", value); }
        }

        /// <summary>노드 행에서 큰 숫자(건수)를 읽을 컬럼/속성 이름.</summary>
        public string NodeCountMember
        {
            get { return this.Member("NodeCount"); }
            set { this.SetMember("NodeCount", value); }
        }

        /// <summary>노드 행에서 건수 단위(lots · FOUP 등)를 읽을 컬럼/속성 이름.</summary>
        public string NodeCountWordMember
        {
            get { return this.Member("NodeCountWord"); }
            set { this.SetMember("NodeCountWord", value); }
        }

        /// <summary>노드 행에서 짧은 요약을 읽을 컬럼/속성 이름. 좁은 단계 노드에서는 툴팁으로만 보인다.</summary>
        public string NodeSummaryMember
        {
            get { return this.Member("NodeSummary"); }
            set { this.SetMember("NodeSummary", value); }
        }

        /// <summary>노드 행에서 Hold 글자(예: "1 lot")를 읽을 컬럼/속성 이름. 값이 있으면 노드 오른쪽 위에 자물쇠와 함께 붙는다.</summary>
        public string NodeHoldMember
        {
            get { return this.Member("NodeHold"); }
            set { this.SetMember("NodeHold", value); }
        }

        /// <summary>
        /// 노드 행에서 상세 줄을 읽을 컬럼/속성 이름 — 장비 노드 아래쪽에 그린다. 빈 줄로 묶음을 나누고 묶음의 첫 줄은 소제목이다
        /// (예: "Running\nLOT-A\n25 wafers · 18m\n\nQueue\nLOT-B · Next"). 노드에 들어가지 않는 줄은 아래부터 숨는다.
        /// </summary>
        public string NodeDetailMember
        {
            get { return this.Member("NodeDetail"); }
            set { this.SetMember("NodeDetail", value); }
        }

        /// <summary>노드 행에서 툴팁 글자를 읽을 컬럼/속성 이름.</summary>
        public string NodeToolTipMember
        {
            get { return this.Member("NodeToolTip"); }
            set { this.SetMember("NodeToolTip", value); }
        }

        /// <summary>내부 칸 행에서 소속 노드 키를 읽을 컬럼/속성 이름.</summary>
        public string CellNodeMember
        {
            get { return this.Member("CellNode"); }
            set { this.SetMember("CellNode", value); }
        }

        /// <summary>내부 칸 행에서 칸 이름(F1 등)을 읽을 컬럼/속성 이름.</summary>
        public string CellIdMember
        {
            get { return this.Member("CellId"); }
            set { this.SetMember("CellId", value); }
        }

        /// <summary>내부 칸 행에서 상태 글자를 읽을 컬럼/속성 이름.</summary>
        public string CellStateMember
        {
            get { return this.Member("CellState"); }
            set { this.SetMember("CellState", value); }
        }

        /// <summary>내부 칸 행에서 랏 글자를 읽을 컬럼/속성 이름 — 예약 랏 표기 등 문구는 폼이 정한다.</summary>
        public string CellLotMember
        {
            get { return this.Member("CellLot"); }
            set { this.SetMember("CellLot", value); }
        }

        /// <summary>내부 칸 행에서 톤(<see cref="SemanticKind"/> 이름)을 읽을 컬럼/속성 이름.</summary>
        public string CellToneMember
        {
            get { return this.Member("CellTone"); }
            set { this.SetMember("CellTone", value); }
        }

        /// <summary>연결 행에서 시작 노드 키를 읽을 컬럼/속성 이름.</summary>
        public string LinkFromMember
        {
            get { return this.Member("LinkFrom"); }
            set { this.SetMember("LinkFrom", value); }
        }

        /// <summary>연결 행에서 끝 노드 키를 읽을 컬럼/속성 이름.</summary>
        public string LinkToMember
        {
            get { return this.Member("LinkTo"); }
            set { this.SetMember("LinkTo", value); }
        }

        /// <summary>연결 행에서 모양(<see cref="ProcessMapLinkStyle"/> 이름)을 읽을 컬럼/속성 이름.</summary>
        public string LinkStyleMember
        {
            get { return this.Member("LinkStyle"); }
            set { this.SetMember("LinkStyle", value); }
        }

        /// <summary>연결 행에서 이름표 글자를 읽을 컬럼/속성 이름 — 되돌림 곡선 위에 붙는다.</summary>
        public string LinkLabelMember
        {
            get { return this.Member("LinkLabel"); }
            set { this.SetMember("LinkLabel", value); }
        }

        /// <summary>연결 행에서 톤(<see cref="SemanticKind"/> 이름)을 읽을 컬럼/속성 이름. 비면 회색.</summary>
        public string LinkToneMember
        {
            get { return this.Member("LinkTone"); }
            set { this.SetMember("LinkTone", value); }
        }

        /// <summary>그리고 있는 노드 수.</summary>
        public int NodeCount
        {
            get { return this.nodes.Count; }
        }

        /// <summary>
        /// 노드 · 연결 · 내부 칸 소스를 통째로 바꾸고 다시 그린다. null 은 빈 목록이다. 이벤트를 내지 않으며,
        /// 바꾼 뒤에도 같은 키의 노드가 있으면 선택이 남고 없으면 비워진다.
        /// </summary>
        /// <param name="nodeRows">노드 행 목록.</param>
        /// <param name="linkRows">연결 행 목록.</param>
        /// <param name="cellRows">내부 칸 행 목록.</param>
        public void SetMapSource(IEnumerable nodeRows, IEnumerable linkRows, IEnumerable cellRows)
        {
            this.nodeSource = nodeRows;
            this.linkSource = linkRows;
            this.cellSource = cellRows;
            this.Reload();
        }

        /// <summary>노드 하나의 화면상 경계(이 컨트롤 기준). 배치 전이거나 없는 키면 <see cref="Rect.Empty"/>.</summary>
        /// <param name="key">노드 키.</param>
        /// <returns>노드 경계.</returns>
        public Rect NodeBounds(string key)
        {
            NodeView view;

            if (key == null || !this.views.TryGetValue(key, out view) || view.Root.ActualWidth <= 0d)
            {
                return Rect.Empty;
            }

            Point origin = view.Root.TranslatePoint(new Point(0d, 0d), this);
            return new Rect(origin, view.Root.RenderSize);
        }

        private string Member(string name)
        {
            string value;
            return this.members.TryGetValue(name, out value) ? value : string.Empty;
        }

        private void SetMember(string name, string value)
        {
            this.members[name] = value ?? string.Empty;
            this.Reload();
        }

        private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernProcessMapControl control = d as ModernProcessMapControl;

            if (control != null)
            {
                control.Rebuild();
            }
        }

        private static void OnSelectedKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernProcessMapControl control = d as ModernProcessMapControl;

            if (control != null)
            {
                control.PaintSelection();
            }
        }

        // ===== 소스 읽기 =====

        private void Reload()
        {
            this.nodes.Clear();
            this.links.Clear();
            this.cells.Clear();

            if (this.nodeSource != null)
            {
                foreach (object row in this.nodeSource)
                {
                    MapNode node = new MapNode();
                    node.Key = this.Read(row, "NodeKey");
                    node.Kind = ParseEnum(this.Read(row, "NodeKind"), ProcessMapNodeKind.Flow);
                    node.Column = ParseInt(this.Read(row, "NodeColumn"), 0, 0);
                    node.Row = ParseInt(this.Read(row, "NodeRow"), 0, 0);
                    node.ColumnSpan = ParseInt(this.Read(row, "NodeColumnSpan"), 1, 1);
                    node.RowSpan = ParseInt(this.Read(row, "NodeRowSpan"), 1, 1);
                    node.Title = this.Read(row, "NodeTitle");
                    node.Id = this.Read(row, "NodeId");
                    node.State = this.Read(row, "NodeState");
                    node.Tone = ParseEnum(this.Read(row, "NodeTone"), SemanticKind.None);
                    node.Count = this.Read(row, "NodeCount");
                    node.CountWord = this.Read(row, "NodeCountWord");
                    node.Summary = this.Read(row, "NodeSummary");
                    node.Hold = this.Read(row, "NodeHold");
                    node.Detail = this.Read(row, "NodeDetail");
                    node.ToolTip = this.Read(row, "NodeToolTip");

                    if (node.Key.Length > 0)
                    {
                        this.nodes.Add(node);
                    }
                }
            }

            if (this.linkSource != null)
            {
                foreach (object row in this.linkSource)
                {
                    MapLink link = new MapLink();
                    link.From = this.Read(row, "LinkFrom");
                    link.To = this.Read(row, "LinkTo");
                    link.Style = ParseEnum(this.Read(row, "LinkStyle"), ProcessMapLinkStyle.Main);
                    link.Label = this.Read(row, "LinkLabel");
                    link.Tone = ParseEnum(this.Read(row, "LinkTone"), SemanticKind.None);
                    this.links.Add(link);
                }
            }

            if (this.cellSource != null)
            {
                foreach (object row in this.cellSource)
                {
                    MapCell cell = new MapCell();
                    cell.Node = this.Read(row, "CellNode");
                    cell.Id = this.Read(row, "CellId");
                    cell.State = this.Read(row, "CellState");
                    cell.Lot = this.Read(row, "CellLot");
                    cell.Tone = ParseEnum(this.Read(row, "CellTone"), SemanticKind.None);
                    this.cells.Add(cell);
                }
            }

            string selected = this.SelectedKey ?? string.Empty;

            if (selected.Length > 0 && this.FindNode(selected) == null)
            {
                this.SetCurrentValue(SelectedKeyProperty, string.Empty);
            }

            this.Rebuild();
        }

        private string Read(object row, string memberName)
        {
            string path = this.Member(memberName);

            if (path.Length == 0)
            {
                return string.Empty;
            }

            return (MemberPathReader.ReadDisplayText(row, path) ?? string.Empty).Trim();
        }

        private MapNode FindNode(string key)
        {
            foreach (MapNode node in this.nodes)
            {
                if (string.Equals(node.Key, key, StringComparison.Ordinal))
                {
                    return node;
                }
            }

            return null;
        }

        private static T ParseEnum<T>(string text, T fallback) where T : struct
        {
            T value;
            return text.Length > 0 && Enum.TryParse(text, true, out value) ? value : fallback;
        }

        private static int ParseInt(string text, int fallback, int minimum)
        {
            int value;

            if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                return fallback;
            }

            return Math.Max(minimum, value);
        }

        // ===== 격자 · 노드 =====

        private void Rebuild()
        {
            this.NodeGrid.Children.Clear();
            this.NodeGrid.ColumnDefinitions.Clear();
            this.NodeGrid.RowDefinitions.Clear();
            this.views.Clear();

            int columnCount = 0;
            int rowCount = 0;

            foreach (MapNode node in this.nodes)
            {
                columnCount = Math.Max(columnCount, node.Column + node.ColumnSpan);
                rowCount = Math.Max(rowCount, node.Row + node.RowSpan);
            }

            List<GridLength> widths = ParseLengths(this.ColumnWidths);
            List<GridLength> heights = ParseLengths(this.RowHeights);

            for (int index = 0; index < Math.Max(columnCount, widths.Count); index++)
            {
                ColumnDefinition column = new ColumnDefinition();
                column.Width = index < widths.Count ? widths[index] : new GridLength(1d, GridUnitType.Star);
                this.NodeGrid.ColumnDefinitions.Add(column);
            }

            this.baseHeights.Clear();

            for (int index = 0; index < Math.Max(rowCount, heights.Count); index++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = index < heights.Count ? heights[index] : new GridLength(1d, GridUnitType.Star);
                this.baseHeights.Add(row.Height);
                this.NodeGrid.RowDefinitions.Add(row);
            }

            this.AdjustRows();

            foreach (MapNode node in this.nodes)
            {
                NodeView view = this.BuildNode(node);
                Grid.SetColumn(view.Root, node.Column);
                Grid.SetRow(view.Root, node.Row);
                Grid.SetColumnSpan(view.Root, node.ColumnSpan);
                Grid.SetRowSpan(view.Root, node.RowSpan);
                this.NodeGrid.Children.Add(view.Root);
                this.views[node.Key] = view;
            }

            this.PaintSelection();
            this.ScheduleLinks();
        }

        private static List<GridLength> ParseLengths(string text)
        {
            List<GridLength> lengths = new List<GridLength>();

            if (string.IsNullOrEmpty(text))
            {
                return lengths;
            }

            GridLengthConverter converter = new GridLengthConverter();

            foreach (string token in text.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    lengths.Add((GridLength)converter.ConvertFromInvariantString(token));
                }
                catch (FormatException)
                {
                    lengths.Add(new GridLength(1d, GridUnitType.Star));
                }
                catch (NotSupportedException)
                {
                    lengths.Add(new GridLength(1d, GridUnitType.Star));
                }
            }

            return lengths;
        }

        private NodeView BuildNode(MapNode node)
        {
            double gap = (double)this.FindResource("Space.Md");
            NodeView view = new NodeView();
            view.Node = node;
            view.Root = new Grid();
            view.Root.Margin = new Thickness(gap / 2d);
            view.Root.Cursor = Cursors.Hand;
            view.Root.Background = Brushes.Transparent;
            view.Root.VerticalAlignment = node.Kind == ProcessMapNodeKind.Branch ? VerticalAlignment.Top : VerticalAlignment.Stretch;

            view.Frame = new Rectangle();
            view.Frame.Fill = this.Brush("Brush.Surface");
            view.Frame.RadiusX = ((CornerRadius)this.FindResource("Radius.Lg")).TopLeft;
            view.Frame.RadiusY = view.Frame.RadiusX;

            if (node.Kind == ProcessMapNodeKind.Branch)
            {
                view.Frame.StrokeDashArray = new DoubleCollection(new double[] { 4d, 3d });
            }

            view.Root.Children.Add(view.Frame);

            view.Content = new Border();
            view.Content.Padding = node.Kind == ProcessMapNodeKind.Equipment ? new Thickness(14d, 10d, 14d, 8d)
                    : node.Kind == ProcessMapNodeKind.Branch ? new Thickness(8d, 5d, 8d, 6d)
                    : new Thickness(10d, 6d, 10d, 6d);
            view.Root.Children.Add(view.Content);

            if (node.Hold.Length > 0)
            {
                view.Hold = this.BuildHoldBadge(node.Hold);
                Panel.SetZIndex(view.Hold, 2);
                view.Root.Children.Add(view.Hold);
            }

            if (node.ToolTip.Length > 0)
            {
                view.Root.ToolTip = node.ToolTip;
                ToolTipService.SetShowDuration(view.Root, 30000);
            }

            view.Root.MouseLeftButtonUp += delegate
            {
                this.SelectedKey = node.Key;
                EventHandler<ProcessMapNodeEventArgs> handler = this.NodeClick;

                if (handler != null)
                {
                    handler(this, new ProcessMapNodeEventArgs(node.Key));
                }
            };

            view.Root.SizeChanged += delegate
            {
                this.FitNode(view, false);
                this.ScheduleLinks();
            };

            this.FitNode(view, true);
            return view;
        }

        // 높이가 모자라면 고정 행을 같은 비율로 줄인다(하한 rowFloor) — * 행(가지 · 장비 아래쪽)에 최소 높이를 남긴다.
        private void AdjustRows()
        {
            double available = this.MapRoot.ActualHeight;
            double fixedSum = 0d;
            bool hasStar = false;

            foreach (GridLength length in this.baseHeights)
            {
                if (length.IsAbsolute)
                {
                    fixedSum += length.Value;
                }
                else if (length.IsStar)
                {
                    hasStar = true;
                }
            }

            double factor = 1d;

            if (available > 0d && fixedSum > 0d)
            {
                factor = Math.Min(1d, Math.Max(0d, available - (hasStar ? starReserve : 0d)) / fixedSum);
            }

            for (int index = 0; index < this.baseHeights.Count && index < this.NodeGrid.RowDefinitions.Count; index++)
            {
                GridLength length = this.baseHeights[index];
                GridLength target = length.IsAbsolute ? new GridLength(Math.Max(Math.Min(rowFloor, length.Value), length.Value * factor)) : length;

                if (!this.NodeGrid.RowDefinitions[index].Height.Equals(target))
                {
                    this.NodeGrid.RowDefinitions[index].Height = target;
                }
            }
        }

        private void FitAll()
        {
            foreach (NodeView view in this.views.Values)
            {
                this.FitNode(view, false);
            }
        }

        // 노드 칸의 높이(여백 제외) — 위로 붙는 가지 노드는 자기 높이가 아니라 칸 높이로 접을지 정한다.
        private double CellHeight(NodeView view)
        {
            double height = 0d;

            for (int row = view.Node.Row; row < view.Node.Row + view.Node.RowSpan && row < this.NodeGrid.RowDefinitions.Count; row++)
            {
                height += this.NodeGrid.RowDefinitions[row].ActualHeight;
            }

            return height - view.Root.Margin.Top - view.Root.Margin.Bottom;
        }

        // 노드 폭 · 칸 높이에 맞는 모양(좁은 줄 배치 · 한 줄 접기 · 내부 칸 열 수)이 바뀌었을 때만 안쪽을 다시 만든다.
        private void FitNode(NodeView view, bool force)
        {
            double width = view.Root.ActualWidth;
            bool equipment = view.Node.Kind == ProcessMapNodeKind.Equipment;
            bool compact = width > 0d && width < (equipment ? compactEquipmentWidth : compactFlowWidth);
            double cell = this.CellHeight(view);
            bool shortCell = cell > 0d && cell < (view.Node.Kind == ProcessMapNodeKind.Branch ? shortBranchHeight : shortFlowHeight);

            if (equipment)
            {
                view.Content.Padding = compact ? new Thickness(10d, 8d, 10d, 6d) : new Thickness(14d, 10d, 14d, 8d);
            }
            else if (view.Node.Kind == ProcessMapNodeKind.Flow)
            {
                view.Content.Padding = shortCell ? new Thickness(8d, 4d, 8d, 4d) : new Thickness(10d, 6d, 10d, 6d);
            }

            double inner = width - view.Content.Padding.Left - view.Content.Padding.Right;
            int cellColumns = width <= 0d || inner >= 4d * cellMinWidth + 3d * cellGap ? 4 : 2;

            if (!force && compact == view.Compact && cellColumns == view.CellColumns && shortCell == view.ShortCell)
            {
                return;
            }

            view.Compact = compact;
            view.CellColumns = cellColumns;
            view.ShortCell = shortCell;

            if (equipment)
            {
                view.Content.Child = this.BuildEquipmentContent(view.Node, compact, cellColumns);
            }
            else if (view.Node.Kind == ProcessMapNodeKind.Branch)
            {
                view.Content.Child = this.BuildBranchContent(view.Node, shortCell);
            }
            else
            {
                view.Content.Child = shortCell ? this.BuildShortFlowContent(view.Node) : this.BuildFlowContent(view.Node, compact);
            }

            if (view.Hold != null)
            {
                view.Hold.Margin = compact ? new Thickness(0d, -10d, 8d, 0d) : new Thickness(0d, 6d, 8d, 0d);
            }
        }

        private UIElement BuildFlowContent(MapNode node, bool compact)
        {
            Grid content = new Grid();
            RowDefinition head = new RowDefinition();
            head.Height = GridLength.Auto;
            content.RowDefinitions.Add(head);
            content.RowDefinitions.Add(new RowDefinition());
            content.ColumnDefinitions.Add(new ColumnDefinition());
            ColumnDefinition side = new ColumnDefinition();
            side.Width = GridLength.Auto;
            content.ColumnDefinitions.Add(side);

            TextBlock title = this.Text(node.Title, "Font.Size.Label", this.Brush("Brush.TextSecondary"), true);
            title.VerticalAlignment = VerticalAlignment.Top;
            content.Children.Add(title);

            bool showSummary = !compact && node.Summary.Length > 0;

            if (showSummary && node.Hold.Length == 0)
            {
                TextBlock summary = this.Text(node.Summary, "Font.Size.Helper", this.Brush("Brush.TextSecondary"), false);
                summary.Margin = new Thickness(8d, 0d, 0d, 0d);
                summary.VerticalAlignment = VerticalAlignment.Top;
                Grid.SetColumn(summary, 1);
                content.Children.Add(summary);
            }

            FrameworkElement count = this.BuildCount(node, "Font.Size.Heading", this.Brush("Brush.TextPrimary"));
            count.VerticalAlignment = VerticalAlignment.Bottom;
            Grid.SetRow(count, 1);
            Grid.SetColumnSpan(count, showSummary && node.Hold.Length > 0 ? 1 : 2);
            content.Children.Add(count);

            if (showSummary && node.Hold.Length > 0)
            {
                TextBlock summary = this.Text(node.Summary, "Font.Size.Helper", this.Brush("Brush.TextSecondary"), false);
                summary.VerticalAlignment = VerticalAlignment.Bottom;
                summary.HorizontalAlignment = HorizontalAlignment.Right;
                summary.Margin = new Thickness(8d, 0d, 0d, 3d);
                Grid.SetRow(summary, 1);
                Grid.SetColumn(summary, 1);
                content.Children.Add(summary);
            }

            return content;
        }

        // 칸이 낮을 때의 단계 노드 — 이름 · 건수 · 단위를 한 줄에 둔다. 요약은 툴팁으로만 보인다.
        // 건수가 한 줄에 다 들어가지 않으면 높이가 되는 한 건수 · 단위를 둘째 줄로 내린다(ShortFlowPanel).
        private UIElement BuildShortFlowContent(MapNode node)
        {
            ShortFlowPanel line = new ShortFlowPanel();

            TextBlock title = this.Text(node.Title, "Font.Size.Label", this.Brush("Brush.TextSecondary"), true);
            title.VerticalAlignment = VerticalAlignment.Center;
            line.Children.Add(title);

            TextBlock count = this.Text(node.Count.Length == 0 ? "-" : node.Count, "Font.Size.BodyLg", this.Brush("Brush.TextPrimary"), true);
            count.VerticalAlignment = VerticalAlignment.Center;
            TextBlock word = null;

            if (node.CountWord.Length > 0)
            {
                word = this.Text(node.CountWord, "Font.Size.Helper", this.Brush("Brush.TextSecondary"), false);
                word.VerticalAlignment = VerticalAlignment.Center;
                word.Margin = new Thickness(4d, 0d, 0d, 0d);
            }

            line.Children.Add(CountLine(count, word));
            return line;
        }

        private UIElement BuildBranchContent(MapNode node, bool shortCell)
        {
            StackPanel content = new StackPanel();
            content.Children.Add(this.Text(node.Title, "Font.Size.Helper", this.Brush("Brush.TextSecondary"), true));

            Brush valueBrush = this.ToneText(node.Tone, "Brush.TextPrimary");
            TextBlock count = this.Text(node.Count.Length == 0 ? "-" : node.Count, "Font.Size.Body", valueBrush, true);
            TextBlock word = null;

            if (node.CountWord.Length > 0)
            {
                word = this.Text(node.CountWord, "Font.Size.Body", valueBrush, true);
                word.Margin = new Thickness(4d, 0d, 0d, 0d);
            }

            FrameworkElement value = CountLine(count, word);
            value.Margin = new Thickness(0d, 2d, 0d, 0d);
            content.Children.Add(value);

            if (node.Summary.Length > 0 && !shortCell)
            {
                content.Children.Add(this.Text(node.Summary, "Font.Size.SubLabel", this.Brush("Brush.TextSecondary"), false));
            }

            return content;
        }

        private UIElement BuildEquipmentContent(MapNode node, bool compact, int cellColumns)
        {
            FitStackPanel content = new FitStackPanel();

            DockPanel header = new DockPanel();
            header.LastChildFill = true;
            Ellipse lamp = new Ellipse();
            lamp.Width = 10d;
            lamp.Height = 10d;
            lamp.Fill = this.ToneDot(node.Tone);
            lamp.Margin = new Thickness(0d, 0d, 8d, 0d);
            lamp.VerticalAlignment = VerticalAlignment.Center;
            DockPanel.SetDock(lamp, Dock.Left);
            header.Children.Add(lamp);

            if (!compact && node.Id.Length > 0)
            {
                TextBlock id = this.Text(node.Id, "Font.Size.Helper", this.Brush("Brush.TextSecondary"), false);
                id.VerticalAlignment = VerticalAlignment.Center;
                id.Margin = new Thickness(8d, 0d, node.Hold.Length > 0 ? 64d : 0d, 0d);
                DockPanel.SetDock(id, Dock.Right);
                header.Children.Add(id);
            }

            TextBlock title = this.Text(node.Title, "Font.Size.BodyLg", this.Brush("Brush.TextPrimary"), true);
            title.VerticalAlignment = VerticalAlignment.Center;
            header.Children.Add(title);
            content.Children.Add(header);

            if (compact && node.Id.Length > 0)
            {
                TextBlock id = this.Text(node.Id, "Font.Size.Helper", this.Brush("Brush.TextSecondary"), false);
                id.Margin = new Thickness(0d, 2d, 0d, 0d);
                content.Children.Add(id);
            }

            if (node.State.Length > 0)
            {
                TextBlock state = this.Text(node.State, "Font.Size.Body", this.ToneText(node.Tone, "Brush.TextSecondary"), true);
                state.Margin = new Thickness(0d, 4d, 0d, 0d);
                content.Children.Add(state);
            }

            FrameworkElement count = this.BuildCount(node, "Font.Size.Display", this.Brush("Brush.TextPrimary"));
            count.Margin = new Thickness(0d, 4d, 0d, 0d);
            content.Children.Add(count);

            if (node.Summary.Length > 0)
            {
                TextBlock summary = this.Text(node.Summary, "Font.Size.Body", this.Brush("Brush.TextSecondary"), false);
                summary.TextWrapping = TextWrapping.Wrap;
                summary.TextTrimming = TextTrimming.None;
                content.Children.Add(summary);
            }

            Rectangle divider = new Rectangle();
            divider.Height = 1d;
            divider.Fill = this.Brush("Brush.BorderSubtle");
            divider.Margin = new Thickness(0d, 8d, 0d, 4d);
            content.Children.Add(divider);

            List<MapCell> nodeCells = this.CellsOf(node.Key);

            if (nodeCells.Count > 0)
            {
                UniformGrid grid = new UniformGrid();
                grid.Columns = Math.Min(cellColumns, nodeCells.Count);
                grid.Margin = new Thickness(-cellGap / 2d, 0d, -cellGap / 2d, 2d);

                foreach (MapCell cell in nodeCells)
                {
                    grid.Children.Add(this.BuildCell(cell));
                }

                content.Children.Add(grid);
            }

            this.AddDetail(content, node.Detail);
            return content;
        }

        private FrameworkElement BuildCount(MapNode node, string sizeKey, Brush brush)
        {
            TextBlock count = this.Text(node.Count.Length == 0 ? "-" : node.Count, sizeKey, brush, true);
            TextBlock countWord = null;

            if (node.CountWord.Length > 0)
            {
                countWord = this.Text(node.CountWord, "Font.Size.Body", this.Brush("Brush.TextSecondary"), false);
                countWord.VerticalAlignment = VerticalAlignment.Bottom;
                countWord.Margin = new Thickness(5d, 0d, 0d, (double)this.FindResource(sizeKey) * 0.14d);
            }

            return CountLine(count, countWord);
        }

        // 건수와 단위를 한 줄로 묶는다 — 단위가 있으면 그 자리를 먼저 잡고 건수만 말줄임한다.
        private static FrameworkElement CountLine(TextBlock count, TextBlock word)
        {
            CountLinePanel line = new CountLinePanel();
            line.Children.Add(count);

            if (word != null)
            {
                line.Children.Add(word);
            }

            return line;
        }

        private UIElement BuildCell(MapCell cell)
        {
            Border box = new Border();
            box.CornerRadius = (CornerRadius)this.FindResource("Radius.Sm");
            box.Padding = new Thickness(6d, 3d, 6d, 4d);
            box.Margin = new Thickness(cellGap / 2d);

            if (cell.Tone == SemanticKind.Error)
            {
                box.Background = this.Brush("Brush.ErrorBackground");
                box.BorderBrush = this.Brush("Brush.ErrorBorder");
                box.BorderThickness = new Thickness(1.5d);
            }
            else if (cell.Tone == SemanticKind.Warning)
            {
                box.Background = this.Brush("Brush.WarningBackground");
                box.BorderBrush = this.Brush("Brush.WarningBorder");
                box.BorderThickness = new Thickness(1.5d);
            }
            else if (cell.Tone == SemanticKind.Success)
            {
                box.Background = this.Brush("Brush.Surface");
                box.BorderBrush = this.Brush("Brush.Success");
                box.BorderThickness = new Thickness(1.5d);
            }
            else
            {
                box.Background = this.Brush("Brush.Surface");
                box.BorderBrush = this.Brush("Brush.Border");
                box.BorderThickness = new Thickness(1d);
            }

            string state = cell.State.Length == 0 ? "-" : cell.State;
            StackPanel lines = new StackPanel();
            lines.Children.Add(this.Text(cell.Id, "Font.Size.Body", this.Brush("Brush.TextPrimary"), true));
            lines.Children.Add(this.Text(state, "Font.Size.Helper", this.ToneText(cell.Tone, "Brush.TextSecondary"), true));
            lines.Children.Add(this.Text(cell.Lot.Length == 0 ? " " : cell.Lot, "Font.Size.SubLabel", this.Brush("Brush.TextSecondary"), false));
            box.Child = lines;
            box.ToolTip = cell.Id + " — " + state + (cell.Lot.Length == 0 ? string.Empty : " · " + cell.Lot);
            return box;
        }

        private void AddDetail(Panel content, string detail)
        {
            if (string.IsNullOrEmpty(detail))
            {
                return;
            }

            string[] sections = detail.Replace("\r\n", "\n").Split(new string[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string section in sections)
            {
                string[] lines = section.Split('\n');

                for (int index = 0; index < lines.Length; index++)
                {
                    if (index == 0)
                    {
                        TextBlock caption = this.Text(lines[index], "Font.Size.Helper", this.Brush("Brush.TextSecondary"), false);
                        caption.Margin = new Thickness(0d, 6d, 0d, 1d);
                        FitStackPanel.SetHeading(caption, lines.Length > 1);
                        content.Children.Add(caption);
                    }
                    else
                    {
                        content.Children.Add(this.Text(lines[index], "Font.Size.Body", this.Brush("Brush.TextPrimary"), index == 1));
                    }
                }
            }
        }

        private Border BuildHoldBadge(string text)
        {
            Border badge = new Border();
            badge.Background = this.Brush("Brush.WarningBackground");
            badge.BorderBrush = this.Brush("Brush.WarningBorder");
            badge.BorderThickness = new Thickness(1d);
            badge.CornerRadius = (CornerRadius)this.FindResource("Radius.Pill");
            badge.Padding = new Thickness(7d, 1d, 8d, 2d);
            badge.HorizontalAlignment = HorizontalAlignment.Right;
            badge.VerticalAlignment = VerticalAlignment.Top;
            badge.IsHitTestVisible = false;

            StackPanel line = new StackPanel();
            line.Orientation = Orientation.Horizontal;
            TextBlock glyph = new TextBlock();
            glyph.Text = "";
            glyph.FontFamily = (FontFamily)this.FindResource("Font.Family.Icon");
            glyph.FontSize = (double)this.FindResource("Font.Size.Icon");
            glyph.Foreground = this.Brush("Brush.WarningText");
            glyph.VerticalAlignment = VerticalAlignment.Center;
            glyph.Margin = new Thickness(0d, 0d, 4d, 0d);
            line.Children.Add(glyph);
            line.Children.Add(this.Text(text, "Font.Size.Helper", this.Brush("Brush.WarningText"), true));
            badge.Child = line;
            return badge;
        }

        private List<MapCell> CellsOf(string nodeKey)
        {
            List<MapCell> result = new List<MapCell>();

            foreach (MapCell cell in this.cells)
            {
                if (string.Equals(cell.Node, nodeKey, StringComparison.Ordinal))
                {
                    result.Add(cell);
                }
            }

            return result;
        }

        private void PaintSelection()
        {
            string selected = this.SelectedKey ?? string.Empty;

            foreach (NodeView view in this.views.Values)
            {
                bool isSelected = selected.Length > 0 && string.Equals(view.Node.Key, selected, StringComparison.Ordinal);

                if (isSelected)
                {
                    view.Frame.Stroke = this.Brush("Brush.Accent");
                    view.Frame.StrokeThickness = 2d;
                }
                else if (view.Node.Kind == ProcessMapNodeKind.Branch && view.Node.Tone == SemanticKind.Warning)
                {
                    view.Frame.Stroke = this.Brush("Brush.WarningBorder");
                    view.Frame.StrokeThickness = 1.2d;
                }
                else
                {
                    view.Frame.Stroke = this.Brush("Brush.Border");
                    view.Frame.StrokeThickness = 1d;
                }
            }
        }

        // ===== 연결선 =====

        private void OnMapSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.AdjustRows();
            this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(this.FitAll));
            this.ScheduleLinks();
        }

        private void ScheduleLinks()
        {
            if (this.linksPending)
            {
                return;
            }

            this.linksPending = true;
            this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(this.DrawLinks));
        }

        private void DrawLinks()
        {
            this.linksPending = false;
            this.LinkCanvas.Children.Clear();

            foreach (MapLink link in this.links)
            {
                Rect from = this.BoundsInCanvas(link.From);
                Rect to = this.BoundsInCanvas(link.To);

                if (from.IsEmpty || to.IsEmpty)
                {
                    continue;
                }

                if (link.Style == ProcessMapLinkStyle.Rework)
                {
                    this.DrawRework(from, to, link);
                }
                else
                {
                    this.DrawStraight(from, to, link);
                }
            }
        }

        private Rect BoundsInCanvas(string key)
        {
            NodeView view;

            if (key == null || !this.views.TryGetValue(key, out view) || view.Root.ActualWidth <= 0d)
            {
                return Rect.Empty;
            }

            Point origin = view.Root.TranslatePoint(new Point(0d, 0d), this.LinkCanvas);
            return new Rect(origin, view.Root.RenderSize);
        }

        private void DrawStraight(Rect from, Rect to, MapLink link)
        {
            Brush stroke = link.Tone == SemanticKind.None || link.Tone == SemanticKind.Neutral ? this.Brush("Brush.TextPlaceholder") : this.ToneDot(link.Tone);
            List<Point> points = new List<Point>();

            if (to.Left >= from.Right - 1d)
            {
                double top = Math.Max(from.Top, to.Top);
                double bottom = Math.Min(from.Bottom, to.Bottom);
                double y = top < bottom ? (top + bottom) / 2d : from.Top + from.Height / 2d;
                double endY = top < bottom ? y : to.Top + to.Height / 2d;
                points.Add(new Point(from.Right, y));

                if (Math.Abs(endY - y) > 0.5d)
                {
                    double middle = (from.Right + to.Left) / 2d;
                    points.Add(new Point(middle, y));
                    points.Add(new Point(middle, endY));
                }

                points.Add(new Point(to.Left - 1d, endY));
            }
            else if (to.Top >= from.Bottom - 1d)
            {
                double left = Math.Max(from.Left, to.Left);
                double right = Math.Min(from.Right, to.Right);
                double x = left < right ? (left + right) / 2d : from.Left + from.Width / 2d;
                double endX = left < right ? x : to.Left + to.Width / 2d;
                points.Add(new Point(x, from.Bottom));

                if (Math.Abs(endX - x) > 0.5d)
                {
                    double middle = (from.Bottom + to.Top) / 2d;
                    points.Add(new Point(x, middle));
                    points.Add(new Point(endX, middle));
                }

                points.Add(new Point(endX, to.Top - 1d));
            }
            else
            {
                return;
            }

            Polyline line = new Polyline();
            line.Points = new PointCollection(points);
            line.Stroke = stroke;
            line.StrokeThickness = linkThickness;

            if (link.Style == ProcessMapLinkStyle.Branch)
            {
                line.StrokeDashArray = new DoubleCollection(new double[] { 3d, 2d });
            }

            this.LinkCanvas.Children.Add(line);
            this.LinkCanvas.Children.Add(this.ArrowHead(points[points.Count - 2], points[points.Count - 1], stroke));
        }

        private void DrawRework(Rect from, Rect to, MapLink link)
        {
            bool active = link.Tone != SemanticKind.None && link.Tone != SemanticKind.Neutral;
            Brush stroke = active ? this.ToneDot(link.Tone) : this.Brush("Brush.TextPlaceholder");
            Point start = new Point(from.Left + from.Width * 0.35d, from.Top);
            Point end = new Point(to.Left + to.Width * 0.65d, to.Top - 1d);
            double controlY = Math.Max(2d, Math.Min(from.Top, to.Top) - reworkLift * 1.4d);

            PathFigure figure = new PathFigure();
            figure.StartPoint = start;
            figure.Segments.Add(new BezierSegment(new Point(start.X, controlY), new Point(end.X, controlY), end, true));
            PathGeometry geometry = new PathGeometry();
            geometry.Figures.Add(figure);

            Path curve = new Path();
            curve.Data = geometry;
            curve.Stroke = stroke;
            curve.StrokeThickness = linkThickness;
            curve.StrokeDashArray = new DoubleCollection(new double[] { 4d, 3d });
            this.LinkCanvas.Children.Add(curve);
            this.LinkCanvas.Children.Add(this.ArrowHead(new Point(end.X, end.Y - 6d), end, stroke));

            if (link.Label.Length == 0)
            {
                return;
            }

            Border pill = new Border();
            pill.Background = active ? this.ToneBackground(link.Tone) : this.Brush("Brush.NeutralBackground");
            pill.CornerRadius = (CornerRadius)this.FindResource("Radius.Pill");
            pill.Padding = new Thickness(8d, 1d, 8d, 2d);
            pill.Child = this.Text(link.Label, "Font.Size.Helper", active ? this.ToneText(link.Tone, "Brush.TextSecondary") : this.Brush("Brush.NeutralText"), true);
            pill.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            double middleX = (start.X + end.X) / 2d;
            double middleY = 0.125d * start.Y + 0.75d * controlY + 0.125d * end.Y;
            Canvas.SetLeft(pill, middleX - pill.DesiredSize.Width / 2d);
            Canvas.SetTop(pill, Math.Max(0d, middleY - pill.DesiredSize.Height / 2d));
            this.LinkCanvas.Children.Add(pill);
        }

        private Polygon ArrowHead(Point tail, Point tip, Brush fill)
        {
            Vector direction = tip - tail;

            if (direction.Length < 0.01d)
            {
                direction = new Vector(1d, 0d);
            }

            direction.Normalize();
            Vector normal = new Vector(-direction.Y, direction.X);
            Point back = tip - direction * 7d;

            Polygon head = new Polygon();
            head.Points = new PointCollection(new Point[] { tip, back + normal * 3.5d, back - normal * 3.5d });
            head.Fill = fill;
            return head;
        }

        // ===== 글자 · 색 =====

        private TextBlock Text(string text, string sizeKey, Brush brush, bool semiBold)
        {
            TextBlock block = new TextBlock();
            block.Text = text ?? string.Empty;
            block.FontSize = (double)this.FindResource(sizeKey);
            block.Foreground = brush;
            block.FontWeight = semiBold ? FontWeights.SemiBold : FontWeights.Normal;
            block.TextTrimming = TextTrimming.CharacterEllipsis;
            return block;
        }

        private Brush ToneDot(SemanticKind tone)
        {
            switch (tone)
            {
                case SemanticKind.Success:
                    return this.Brush("Brush.Success");
                case SemanticKind.Warning:
                    return this.Brush("Brush.Warning");
                case SemanticKind.Error:
                    return this.Brush("Brush.ErrorBorder");
                case SemanticKind.Info:
                    return this.Brush("Brush.Accent");
                default:
                    return this.Brush("Brush.TextPlaceholder");
            }
        }

        private Brush ToneText(SemanticKind tone, string fallbackKey)
        {
            switch (tone)
            {
                case SemanticKind.Success:
                    return this.Brush("Brush.SuccessText");
                case SemanticKind.Warning:
                    return this.Brush("Brush.WarningText");
                case SemanticKind.Error:
                    return this.Brush("Brush.ErrorText");
                case SemanticKind.Info:
                    return this.Brush("Brush.InfoText");
                default:
                    return this.Brush(fallbackKey);
            }
        }

        private Brush ToneBackground(SemanticKind tone)
        {
            switch (tone)
            {
                case SemanticKind.Success:
                    return this.Brush("Brush.SuccessBackground");
                case SemanticKind.Warning:
                    return this.Brush("Brush.WarningBackground");
                case SemanticKind.Error:
                    return this.Brush("Brush.ErrorBackground");
                case SemanticKind.Info:
                    return this.Brush("Brush.InfoBackground");
                default:
                    return this.Brush("Brush.NeutralBackground");
            }
        }

        private Brush Brush(string tokenKey)
        {
            return (Brush)this.FindResource(tokenKey);
        }

        // ===== 내부 모델 =====

        private sealed class MapNode
        {
            public string Key = string.Empty;
            public ProcessMapNodeKind Kind;
            public int Column;
            public int Row;
            public int ColumnSpan = 1;
            public int RowSpan = 1;
            public string Title = string.Empty;
            public string Id = string.Empty;
            public string State = string.Empty;
            public SemanticKind Tone;
            public string Count = string.Empty;
            public string CountWord = string.Empty;
            public string Summary = string.Empty;
            public string Hold = string.Empty;
            public string Detail = string.Empty;
            public string ToolTip = string.Empty;
        }

        private sealed class MapLink
        {
            public string From = string.Empty;
            public string To = string.Empty;
            public ProcessMapLinkStyle Style;
            public string Label = string.Empty;
            public SemanticKind Tone;
        }

        private sealed class MapCell
        {
            public string Node = string.Empty;
            public string Id = string.Empty;
            public string State = string.Empty;
            public string Lot = string.Empty;
            public SemanticKind Tone;
        }

        private sealed class NodeView
        {
            public MapNode Node;
            public Grid Root;
            public Rectangle Frame;
            public Border Content;
            public Border Hold;
            public bool Compact;
            public bool ShortCell;
            public int CellColumns;
        }

        // 건수 줄 — 단위(둘째 자식) 자리를 먼저 잡고 숫자(첫째 자식)는 남은 폭에서 말줄임한다. 단위는 숫자 바로 뒤에 붙는다.
        private sealed class CountLinePanel : Panel
        {
            protected override Size MeasureOverride(Size availableSize)
            {
                UIElement count = this.InternalChildren.Count > 0 ? this.InternalChildren[0] : null;
                UIElement word = this.InternalChildren.Count > 1 ? this.InternalChildren[1] : null;
                Size wordSize = new Size(0d, 0d);

                if (word != null)
                {
                    word.Measure(new Size(double.PositiveInfinity, availableSize.Height));
                    wordSize = word.DesiredSize;
                }

                Size countSize = new Size(0d, 0d);

                if (count != null)
                {
                    double room = double.IsInfinity(availableSize.Width) ? double.PositiveInfinity : Math.Max(0d, availableSize.Width - wordSize.Width);
                    count.Measure(new Size(room, availableSize.Height));
                    countSize = count.DesiredSize;
                }

                return new Size(countSize.Width + wordSize.Width, Math.Max(countSize.Height, wordSize.Height));
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                UIElement count = this.InternalChildren.Count > 0 ? this.InternalChildren[0] : null;
                UIElement word = this.InternalChildren.Count > 1 ? this.InternalChildren[1] : null;
                double wordWidth = word == null ? 0d : word.DesiredSize.Width;
                double countWidth = count == null ? 0d : Math.Min(count.DesiredSize.Width, Math.Max(0d, finalSize.Width - wordWidth));

                if (count != null)
                {
                    count.Arrange(new Rect(0d, 0d, countWidth, finalSize.Height));
                }

                if (word != null)
                {
                    word.Arrange(new Rect(countWidth, 0d, wordWidth, finalSize.Height));
                }

                return finalSize;
            }
        }

        // 낮은 단계 노드의 이름(첫째 자식) · 건수 줄(둘째 자식) — 건수까지 한 줄에 다 들어가면 한 줄로 둔다.
        // 다 들어가지 않으면 두 줄 높이가 되는 한 건수 줄을 이름 아래로 내리고, 그것도 안 되면 한 줄에서 건수만 말줄임한다.
        private sealed class ShortFlowPanel : Panel
        {
            private const double titleGap = 6d;

            private bool stacked;

            protected override Size MeasureOverride(Size availableSize)
            {
                UIElement title = this.InternalChildren.Count > 0 ? this.InternalChildren[0] : null;
                UIElement count = this.InternalChildren.Count > 1 ? this.InternalChildren[1] : null;
                Size titleSize = new Size(0d, 0d);

                if (title != null)
                {
                    title.Measure(availableSize);
                    titleSize = title.DesiredSize;
                }

                if (count == null)
                {
                    this.stacked = false;
                    return titleSize;
                }

                count.Measure(new Size(double.PositiveInfinity, availableSize.Height));
                Size natural = count.DesiredSize;
                double gap = title == null ? 0d : titleGap;
                bool fitsOneLine = double.IsInfinity(availableSize.Width) || titleSize.Width + gap + natural.Width <= availableSize.Width;
                bool fitsTwoLines = double.IsInfinity(availableSize.Height) || titleSize.Height + natural.Height <= availableSize.Height;
                this.stacked = !fitsOneLine && fitsTwoLines;

                if (this.stacked)
                {
                    count.Measure(new Size(availableSize.Width, availableSize.Height));
                    return new Size(Math.Max(titleSize.Width, count.DesiredSize.Width), titleSize.Height + count.DesiredSize.Height);
                }

                double room = double.IsInfinity(availableSize.Width) ? double.PositiveInfinity : Math.Max(0d, availableSize.Width - titleSize.Width - gap);
                count.Measure(new Size(room, availableSize.Height));
                return new Size(titleSize.Width + gap + count.DesiredSize.Width, Math.Max(titleSize.Height, count.DesiredSize.Height));
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                UIElement title = this.InternalChildren.Count > 0 ? this.InternalChildren[0] : null;
                UIElement count = this.InternalChildren.Count > 1 ? this.InternalChildren[1] : null;
                double titleWidth = title == null ? 0d : Math.Min(title.DesiredSize.Width, finalSize.Width);
                double titleHeight = title == null ? 0d : title.DesiredSize.Height;
                double countHeight = count == null ? 0d : count.DesiredSize.Height;

                if (this.stacked && count != null)
                {
                    double top = Math.Max(0d, (finalSize.Height - titleHeight - countHeight) / 2d);

                    if (title != null)
                    {
                        title.Arrange(new Rect(0d, top, titleWidth, titleHeight));
                    }

                    count.Arrange(new Rect(0d, top + titleHeight, finalSize.Width, countHeight));
                    return finalSize;
                }

                if (title != null)
                {
                    title.Arrange(new Rect(0d, 0d, titleWidth, finalSize.Height));
                }

                if (count != null)
                {
                    double left = title == null ? 0d : titleWidth + titleGap;
                    count.Arrange(new Rect(left, 0d, Math.Max(0d, finalSize.Width - left), finalSize.Height));
                }

                return finalSize;
            }
        }

        // 위에서부터 자식을 쌓되 남은 높이에 다 들어가는 자식까지만 보인다 — 장비 노드의 아래쪽 상세 줄이 반쯤 잘리지 않게 한다.
        // 소제목으로 표시한 자식은 아래 줄이 하나도 들어가지 않으면 함께 숨는다.
        private sealed class FitStackPanel : Panel
        {
            private static readonly DependencyProperty HeadingProperty =
                DependencyProperty.RegisterAttached("Heading", typeof(bool), typeof(FitStackPanel), new PropertyMetadata(false));

            public FitStackPanel()
            {
                this.ClipToBounds = true;
            }

            public static void SetHeading(UIElement element, bool value)
            {
                element.SetValue(HeadingProperty, value);
            }

            protected override Size MeasureOverride(Size availableSize)
            {
                double width = 0d;
                double height = 0d;

                foreach (UIElement child in this.InternalChildren)
                {
                    child.Measure(new Size(availableSize.Width, double.PositiveInfinity));
                    width = Math.Max(width, child.DesiredSize.Width);
                    height += child.DesiredSize.Height;
                }

                if (!double.IsInfinity(availableSize.Height))
                {
                    height = Math.Min(height, availableSize.Height);
                }

                return new Size(width, height);
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                int shown = 0;
                double used = 0d;

                while (shown < this.InternalChildren.Count && used + this.InternalChildren[shown].DesiredSize.Height <= finalSize.Height + 0.5d)
                {
                    used += this.InternalChildren[shown].DesiredSize.Height;
                    shown++;
                }

                if (shown > 0 && shown < this.InternalChildren.Count && (bool)this.InternalChildren[shown - 1].GetValue(HeadingProperty))
                {
                    shown--;
                }

                double top = 0d;

                for (int index = 0; index < this.InternalChildren.Count; index++)
                {
                    UIElement child = this.InternalChildren[index];
                    double height = child.DesiredSize.Height;

                    if (index < shown)
                    {
                        child.Arrange(new Rect(0d, top, finalSize.Width, height));
                        top += height;
                    }
                    else
                    {
                        child.Arrange(new Rect(0d, finalSize.Height + 1d, finalSize.Width, height));
                    }
                }

                return finalSize;
            }
        }
    }
}
