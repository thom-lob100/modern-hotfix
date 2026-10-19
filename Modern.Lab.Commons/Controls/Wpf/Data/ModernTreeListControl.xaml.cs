using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Modern.Lab.Controls.Wpf.Common;

namespace Modern.Lab.Controls.Wpf.Data
{
    /// <summary>
    /// 모던 트리 리스트 (트리 + 컬럼 하이브리드) — Lot 계보처럼 계층이 있는
    /// 행에 수량·상태 같은 값 컬럼을 나란히 붙일 때 쓴다 (DevExpress TreeList
    /// 대응). 트리를 그리드에 평탄화하는 방식이라 컬럼 헤더·폭 조절·행 선택은
    /// ModernDataGrid와 같은 문법이다.
    ///
    /// 데이터는 ModernTreeView와 같은 **평면 자기참조 테이블**을 받는다:
    /// - ItemsSource: 임의의 IEnumerable (DataView, IList, ...)
    /// - IdMemberPath / ParentIdMemberPath / DisplayMemberPath: 키/부모키/명칭 컬럼
    /// - 부모 키가 비어 있거나 목록에 없으면 루트 노드가 된다
    /// - ApplyColumns: 트리 컬럼 오른쪽에 붙는 값 컬럼 정의 (Text/Badge만 지원 —
    ///   그 외 Kind는 Text로 강등된다)
    /// - SelectedValue: 선택 노드의 키 (null = 미선택). 설정 시 조상이 자동 펼쳐진다
    ///
    /// 할당 순서 내성: SelectedValue를 ItemsSource보다 먼저 설정해도 되고,
    /// 재할당 시 값이 새 트리에 없으면 미선택으로 초기화된다. 노드는 기본
    /// 전부 펼쳐진 상태로 구성된다 (계보 조회 용도 — CollapseAll로 접는다).
    /// </summary>
    public partial class ModernTreeListControl : UserControl, IHostDisposalAware
    {
        /// <summary>트리를 구성할 행 목록 (평면 자기참조 테이블).</summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
                typeof(IEnumerable),
                typeof(ModernTreeListControl),
                new PropertyMetadata(null, OnDataShapeChanged));

        /// <summary>노드 키 컬럼/속성 이름.</summary>
        public static readonly DependencyProperty IdMemberPathProperty =
            DependencyProperty.Register(
                "IdMemberPath",
                typeof(string),
                typeof(ModernTreeListControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>부모 키 컬럼/속성 이름.</summary>
        public static readonly DependencyProperty ParentIdMemberPathProperty =
            DependencyProperty.Register(
                "ParentIdMemberPath",
                typeof(string),
                typeof(ModernTreeListControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>트리 컬럼 표시 텍스트 컬럼/속성 이름.</summary>
        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register(
                "DisplayMemberPath",
                typeof(string),
                typeof(ModernTreeListControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>트리 컬럼(첫 컬럼)의 헤더 캡션. 기본 빈 문자열.</summary>
        public static readonly DependencyProperty TreeHeaderTextProperty =
            DependencyProperty.Register(
                "TreeHeaderText",
                typeof(string),
                typeof(ModernTreeListControl),
                new PropertyMetadata(string.Empty, OnColumnShapeChanged));

        /// <summary>트리 컬럼 픽셀 폭. 0 이하(기본 -1)면 남은 공간 채움(star).</summary>
        public static readonly DependencyProperty TreeColumnWidthProperty =
            DependencyProperty.Register(
                "TreeColumnWidth",
                typeof(double),
                typeof(ModernTreeListControl),
                new PropertyMetadata(-1d, OnColumnShapeChanged));

        /// <summary>
        /// 노드가 하나도 없을 때 가운데에 표시할 안내 문구.
        /// 기본 "No data" — 화면 문맥에 맞게 바꾸거나 빈 문자열로 끈다.
        /// </summary>
        public static readonly DependencyProperty EmptyTextProperty =
            DependencyProperty.Register(
                "EmptyText",
                typeof(string),
                typeof(ModernTreeListControl),
                new PropertyMetadata("No data", OnEmptyTextChanged));

        /// <summary>
        /// 컬럼 값 필터 사용 여부 (기본 켬 — 그리드와 같은 기본값). 켜면 값 컬럼
        /// 헤더에 깔때기가 붙고, 고른 값에 걸린 노드와 **그 조상**이 남는다 —
        /// 조상을 지우면 계보가 끊겨 자식이 어디 붙는지 알 수 없기 때문이다.
        /// 트리 컬럼 자체에는 붙지 않는다(계보 축이라 값으로 거를 대상이 아니다).
        /// </summary>
        public static readonly DependencyProperty AllowColumnFiltersProperty =
            DependencyProperty.Register(
                "AllowColumnFilters",
                typeof(bool),
                typeof(ModernTreeListControl),
                new PropertyMetadata(true, OnAllowColumnFiltersChanged));

        /// <summary>
        /// 찾기(Ctrl+F) 사용 여부 (기본 켬 — 그리드와 같은 기본값). 행을 거르지
        /// 않고 일치 셀로 이동한다. 대상은 **보이는 노드**의 트리 텍스트와 값
        /// 컬럼이다 — 접힌 자식은 훑지 않으므로 전체를 찾으려면 ExpandAll 을 먼저 부른다.
        /// </summary>
        public static readonly DependencyProperty AllowFindPanelProperty =
            DependencyProperty.Register(
                "AllowFindPanel",
                typeof(bool),
                typeof(ModernTreeListControl),
                new PropertyMetadata(true, OnAllowFindPanelChanged));

        /// <summary>하단 상태바 표시 여부. 기본 꺼짐 — ModernDataGridControl 과 같은 기본값이다.</summary>
        public static readonly DependencyProperty ShowStatusBarProperty =
            DependencyProperty.Register(
                "ShowStatusBar",
                typeof(bool),
                typeof(ModernTreeListControl),
                new PropertyMetadata(false, OnStatusBarAppearanceChanged));

        /// <summary>상태바 오른쪽에 표시할 자유 텍스트 (빈 문자열이면 표시 없음).</summary>
        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register(
                "StatusText",
                typeof(string),
                typeof(ModernTreeListControl),
                new PropertyMetadata(string.Empty, OnStatusBarAppearanceChanged));

        /// <summary>
        /// 상태바 왼쪽 건수 표기 형식. {0}에 현재 보이는 노드 수가 들어간다 —
        /// 접힌 자식은 세지 않는다(화면에 보이는 것을 말하는 표기다).
        /// 기본 "{0:N0} rows" — 그리드·페이지네이션과 같은 문형.
        /// </summary>
        public static readonly DependencyProperty StatusCountFormatProperty =
            DependencyProperty.Register(
                "StatusCountFormat",
                typeof(string),
                typeof(ModernTreeListControl),
                new PropertyMetadata("{0:N0} rows", OnStatusBarAppearanceChanged));

        /// <summary>
        /// 행 배경색을 담은 멤버 경로. 값이 색이면 그 색으로 칠하고, 비었거나
        /// 색이 아니면 기본 배경이다. 선택·hover 가 이 색보다 우선한다
        /// (ModernDataGridControl 과 같은 우선순위).
        /// </summary>
        public static readonly DependencyProperty RowColorMemberPathProperty =
            DependencyProperty.Register(
                "RowColorMemberPath",
                typeof(string),
                typeof(ModernTreeListControl),
                new PropertyMetadata(string.Empty, OnRowColorMemberPathChanged));

        /// <summary>
        /// 소제목 행 멤버 경로 — 값이 참(bool true · "Y" · "true" · "1")인 행은 **소제목**으로 그린다: 글자 SemiBold,
        /// 배경은 컬럼 헤더와 같은 띠(<c>Brush.GridHeaderBackground</c>), 위쪽 1px 구분선(<c>Brush.Border</c>).
        /// 규칙 안내 창의 액션 줄처럼 "묶음의 제목" 행이 조건 줄과 구분돼야 할 때 쓴다(2026-10-20) — 라이트 테마에서는
        /// 옅은 배경색끼리 구분되지 않아(서로 1.1:1) 색이 아니라 굵기·띠·선으로 나눈다. 선택·hover 가 우선이고,
        /// 행 색(<see cref="RowColorMemberPath"/>)이 있으면 배경은 그 색이되 굵기·구분선은 남는다.
        /// </summary>
        public static readonly DependencyProperty RowHeaderMemberPathProperty =
            DependencyProperty.Register(
                "RowHeaderMemberPath",
                typeof(string),
                typeof(ModernTreeListControl),
                new PropertyMetadata(string.Empty, OnRowColorMemberPathChanged));

        /// <summary>선택 노드의 키. null은 미선택을 의미한다.</summary>
        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register(
                "SelectedValue",
                typeof(object),
                typeof(ModernTreeListControl),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedValuePropertyChanged));

        // 전체 노드(데이터 순서)와 루트 목록 — 펼침 상태와 무관한 원본 구조.
        private readonly List<TreeListNode> allNodes = new List<TreeListNode>();
        private readonly List<TreeListNode> rootNodes = new List<TreeListNode>();

        // 그리드에 바인딩되는 "보이는 노드" 평탄화 목록 — 펼침 상태를 반영한다.
        private readonly ObservableCollection<TreeListNode> visibleNodes =
                new ObservableCollection<TreeListNode>();

        // 컬럼 값 필터·찾기 — 그리드와 같은 컨트롤러를 그대로 쓴다. 값은 노드가
        // 품은 원본 행에서 읽는다(셀 바인딩과 같은 "Row." 경로).
        private readonly GridFilterController filterController;
        private readonly GridFindController findController;

        // 찾기 컨트롤러에 넘기는, 그리드 컬럼 순서와 같은 정의 목록
        // (0번은 트리 컬럼 — 노드 텍스트를 찾기 대상으로 삼는다).
        private readonly List<ModernDataGridColumn> searchDefinitions =
                new List<ModernDataGridColumn>();

        // ApplyColumns로 받은 값 컬럼 정의 (트리 컬럼 오른쪽에 붙는다).
        private IList<ModernDataGridColumn> valueColumnDefinitions;

        // 그리드 선택 ↔ SelectedValue 동기화의 순환을 막는 가드.
        private bool suppressSelectionSync;

        /// <summary>선택이 바뀔 때 발생한다.</summary>
        public event EventHandler SelectedValueChanged;

        /// <summary>
        /// 행을 우클릭했을 때 발생한다 — 그 행이 먼저 선택된 뒤 알린다
        /// (ModernDataGridControl과 같은 계약). 호스트가 컨텍스트 메뉴를 띄우는 신호다.
        /// </summary>
        public event EventHandler RowRightClick;

        public ModernTreeListControl()
        {
            this.InitializeComponent();
            this.InnerDataGrid.ItemsSource = this.visibleNodes;

            // 행 우클릭: 그 행을 먼저 선택하고 RowRightClick을 알린다 —
            // "우클릭 → 그 행 대상 컨텍스트 메뉴" 동선(그리드와 동일).
            this.InnerDataGrid.PreviewMouseRightButtonDown += this.OnGridRightButtonDown;

            // 클립보드 복사: Ctrl+C = 행, Ctrl+Shift+C = 셀 하나 (그리드와 같은 단축키).
            this.InnerDataGrid.PreviewKeyDown += this.OnGridPreviewKeyDown;

            // 현재 셀 표식 — 그리드와 같은 옅은 액센트 언더라인(아래 2px).
            this.InnerDataGrid.CurrentCellChanged += this.OnCurrentCellChanged;

            // 선택 강조는 행 로컬 배경으로 칠한다 — 행 스타일 트리거는
            // RowBackground 전달값에 밀려 적용되지 않고(XAML 주석), 셀 트리거만
            // 있으면 별(*) 폭 반올림으로 셀 사이에 생기는 1px 틈이 선택 행에서
            // 흰 세로 선으로 보인다 (ModernDataGridControl과 동일한 결함/해법).
            this.InnerDataGrid.LoadingRow += this.OnLoadingRow;

            // 값 필터: 무엇을 감출지는 이 컨트롤이 정한다(걸린 노드 + 조상) —
            // 뷰 필터로 평탄 목록을 거르면 조상이 사라져 계보가 끊긴다.
            this.filterController = new GridFilterController(
                this.InnerDataGrid, this, delegate { return this.visibleNodes; });
            this.filterController.ExternalFiltering = true;
            this.filterController.FiltersChanged += this.OnFiltersChanged;

            this.findController = new GridFindController(
                this.InnerDataGrid, this, delegate { return this.searchDefinitions; });

            // Ctrl+F(찾기)/Esc(닫기)/F3(다음)은 트리 어디에 포커스가 있어도 동작한다.
            this.PreviewKeyDown += this.OnRootPreviewKeyDown;

            // 상태바 건수는 보이는 노드 수를 따른다 — 펼침·접힘·재조회 모두 반영된다.
            this.visibleNodes.CollectionChanged += this.OnVisibleNodesChanged;

            // 카드 모서리: 내부를 카드 반경에 맞춰 클리핑한다 (XAML 주석 참고).
            this.ContentHost.SizeChanged += this.OnContentHostSizeChanged;

            this.RebuildColumns();
            this.RefreshEmptyLabel();
        }

        // 내부 컨테이너를 카드 반경(테두리 1px 안쪽)에 맞춰 둥글게 클리핑한다 —
        // WPF Border는 자식을 CornerRadius로 잘라 주지 않으므로 직접 자른다.
        private void OnContentHostSizeChanged(object sender, SizeChangedEventArgs e)
        {
            CornerRadius cardRadius = (CornerRadius)this.FindResource("Radius.Sm");
            double radius = Math.Max(0d, cardRadius.TopLeft - 1d);

            RectangleGeometry clip = new RectangleGeometry(
                    new Rect(0d, 0d, e.NewSize.Width, e.NewSize.Height), radius, radius);
            clip.Freeze();
            this.ContentHost.Clip = clip;
        }

        // 행이 실체화될 때 선택 상태에 맞는 배경을 칠한다 (가상화 대응).
        private void OnLoadingRow(object sender, DataGridRowEventArgs e)
        {
            this.PaintRowBackground(e.Row);

            // 현재 셀이 이 행에 있으면(스크롤 복귀/재활용) 표식을 다시 건다 —
            // 셀 컨테이너는 레이아웃 뒤에 생기므로 한 틱 미룬다 (그리드와 동일).
            this.Dispatcher.BeginInvoke(
                    new Action(this.RefreshCurrentCellMarker),
                    System.Windows.Threading.DispatcherPriority.Loaded);
        }

        // ===== 현재 셀 표식 (ModernDataGridControl과 같은 문법) =====

        private DataGridCell markedCurrentCell;

        private Brush currentCellFrameBrush;

        private void OnCurrentCellChanged(object sender, EventArgs e)
        {
            this.RefreshCurrentCellMarker();
        }

        // 현재 셀(CurrentCell)의 컨테이너를 찾아 옅은 액센트 언더라인(아래 2px)을
        // 옮겨 단다 — 셀 BorderBrush/BorderThickness 로컬 값을 설정하면 셀 템플릿의
        // CurrentFrame이 그린다. 컨테이너가 아직 없으면 LoadingRow가 다시 시도한다.
        private void RefreshCurrentCellMarker()
        {
            if (this.markedCurrentCell != null)
            {
                this.markedCurrentCell.ClearValue(Control.BorderBrushProperty);
                this.markedCurrentCell.ClearValue(Control.BorderThicknessProperty);
                this.markedCurrentCell = null;
            }

            DataGridCellInfo current = this.InnerDataGrid.CurrentCell;

            if (!current.IsValid || current.Column == null)
            {
                return;
            }

            FrameworkElement content = current.Column.GetCellContent(current.Item);
            DataGridCell cell = content == null ? null : content.Parent as DataGridCell;

            if (cell != null)
            {
                cell.BorderBrush = this.CurrentCellFrameBrush();
                cell.BorderThickness = new Thickness(0d, 0d, 0d, 2d);
                this.markedCurrentCell = cell;
            }
        }

        private Brush CurrentCellFrameBrush()
        {
            if (this.currentCellFrameBrush == null)
            {
                Color accentSoft = (Color)this.FindResource("Color.AccentSoft");
                SolidColorBrush brush = new SolidColorBrush(accentSoft);
                brush.Freeze();
                this.currentCellFrameBrush = brush;
            }

            return this.currentCellFrameBrush;
        }

        // ===== 클립보드 복사 (그리드와 같은 서식 — 탭 구분, 표시 서식 적용) =====

        /// <summary>
        /// 현재 노드 행을 탭 구분 한 줄로 클립보드에 복사한다 (Ctrl+C와 같은 동작).
        /// 첫 칸은 트리 컬럼 텍스트, 이어서 값 컬럼이 정의 순서대로 온다.
        /// </summary>
        public void CopyRowToClipboard()
        {
            TreeListNode node = this.CurrentNode();

            if (node != null)
            {
                SetClipboardText(this.RowText(node));
            }
        }

        /// <summary>
        /// 현재 셀 하나의 표시 텍스트만 클립보드에 복사한다 (Ctrl+Shift+C와 같은 동작).
        /// 현재 셀이 없으면 트리 컬럼(노드 텍스트)을 복사한다.
        /// </summary>
        public void CopyCellToClipboard()
        {
            TreeListNode node = this.CurrentNode();

            if (node == null)
            {
                return;
            }

            DataGridCellInfo current = this.InnerDataGrid.CurrentCell;
            int columnIndex = current.IsValid && current.Column != null
                    ? this.InnerDataGrid.Columns.IndexOf(current.Column)
                    : 0;

            SetClipboardText(this.CellText(node, columnIndex));
        }

        private TreeListNode CurrentNode()
        {
            TreeListNode node = this.InnerDataGrid.CurrentItem as TreeListNode;

            return node ?? this.FindNodeByValue(this.SelectedValue);
        }

        // 행 한 줄 — 트리 컬럼 + 값 컬럼(정의 순서), 탭 구분.
        private string RowText(TreeListNode node)
        {
            StringBuilder text = new StringBuilder(node.Text ?? string.Empty);

            if (this.valueColumnDefinitions != null)
            {
                foreach (ModernDataGridColumn definition in this.valueColumnDefinitions)
                {
                    if (definition == null || string.IsNullOrEmpty(definition.DataPropertyName))
                    {
                        continue;
                    }

                    text.Append('\t');
                    text.Append(ValueText(node, definition));
                }
            }

            return text.ToString();
        }

        // 셀 하나 — 0은 트리 컬럼, 1부터는 값 컬럼 정의 순서.
        private string CellText(TreeListNode node, int columnIndex)
        {
            if (columnIndex <= 0 || this.valueColumnDefinitions == null)
            {
                return node.Text ?? string.Empty;
            }

            int valueIndex = 0;

            foreach (ModernDataGridColumn definition in this.valueColumnDefinitions)
            {
                if (definition == null || string.IsNullOrEmpty(definition.DataPropertyName))
                {
                    continue;
                }

                valueIndex = valueIndex + 1;

                if (valueIndex == columnIndex)
                {
                    return ValueText(node, definition);
                }
            }

            return node.Text ?? string.Empty;
        }

        // 값 컬럼 표시 텍스트 — 화면과 같은 서식(Format)을 적용한다.
        private static string ValueText(TreeListNode node, ModernDataGridColumn definition)
        {
            return GridAutoFitMeasurer.FormatCellText(
                    MemberPathReader.Read(node.Row, definition.DataPropertyName), definition.Format);
        }

        // 클립보드는 다른 프로세스가 잡고 있으면 예외를 던진다 — 복사 실패로 화면이
        // 죽지 않게 삼킨다 (그리드와 동일).
        private static void SetClipboardText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            try
            {
                System.Windows.Clipboard.SetText(text);
            }
            catch (Exception)
            {
            }
        }

        private void OnGridPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.C
                    || (System.Windows.Input.Keyboard.Modifiers
                        & System.Windows.Input.ModifierKeys.Control) == 0)
            {
                return;
            }

            bool shift = (System.Windows.Input.Keyboard.Modifiers
                    & System.Windows.Input.ModifierKeys.Shift) != 0;

            if (shift)
            {
                this.CopyCellToClipboard();
            }
            else
            {
                this.CopyRowToClipboard();
            }

            e.Handled = true;
        }

        /// <summary>컬럼 값 필터 사용 여부.</summary>
        public bool AllowColumnFilters
        {
            get { return (bool)this.GetValue(AllowColumnFiltersProperty); }
            set { this.SetValue(AllowColumnFiltersProperty, value); }
        }

        /// <summary>찾기(Ctrl+F) 사용 여부.</summary>
        public bool AllowFindPanel
        {
            get { return (bool)this.GetValue(AllowFindPanelProperty); }
            set { this.SetValue(AllowFindPanelProperty, value); }
        }

        /// <summary>값이 걸린 컬럼이 하나라도 있는지.</summary>
        public bool HasActiveColumnFilters
        {
            get { return this.filterController.HasActiveFilters; }
        }

        /// <summary>모든 컬럼의 값 필터를 해제한다.</summary>
        public void ClearColumnFilters()
        {
            this.filterController.ClearAllFilters();
        }

        /// <summary>찾기 창을 연다 (AllowFindPanel 이 꺼져 있으면 아무 일도 없다).</summary>
        public void ShowFindPanel()
        {
            if (!this.AllowFindPanel)
            {
                return;
            }

            this.findController.Show();
        }

        /// <summary>찾기 창을 닫는다.</summary>
        public void HideFindPanel()
        {
            this.findController.Hide();
        }

        /// <summary>찾기 창이 열려 있는지.</summary>
        public bool IsFindPanelOpen
        {
            get { return this.findController.IsOpen; }
        }

        // 호스트의 최종 폐기만 정리한다 — 찾기 창과 필터 팝업은 트리보다 오래
        // 살아남을 수 있는 창이라 여기서 닫는다(그리드와 같은 규칙).
        void IHostDisposalAware.OnHostDisposing()
        {
            this.filterController.Detach();
            this.findController.CloseWindow();
        }

        private static void OnAllowColumnFiltersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernTreeListControl control = (ModernTreeListControl)d;

            if (!(bool)e.NewValue)
            {
                control.filterController.ClearAllFilters();
            }

            control.RebuildColumns();
        }

        private static void OnAllowFindPanelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                ((ModernTreeListControl)d).findController.Hide();
            }
        }

        // 값 필터가 바뀌면 보이는 목록을 다시 만든다 — 걸린 노드와 그 조상만 남는다.
        private void OnFiltersChanged(object sender, EventArgs e)
        {
            this.RebuildVisible();
        }

        private void OnRootPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F
                    && (System.Windows.Input.Keyboard.Modifiers
                        & System.Windows.Input.ModifierKeys.Control) != 0
                    && this.AllowFindPanel)
            {
                this.ShowFindPanel();
                e.Handled = true;
                return;
            }

            if (!this.findController.IsOpen)
            {
                return;
            }

            if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.HideFindPanel();
                this.InnerDataGrid.Focus();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.F3)
            {
                // Shift+F3 = 이전 일치 (표준 찾기 단축키 — 그리드와 같다).
                if ((System.Windows.Input.Keyboard.Modifiers
                        & System.Windows.Input.ModifierKeys.Shift) != 0)
                {
                    this.findController.FindPrevious();
                }
                else
                {
                    this.findController.FindNext();
                }

                e.Handled = true;
            }
        }

        /// <summary>하단 상태바 표시 여부.</summary>
        public bool ShowStatusBar
        {
            get { return (bool)this.GetValue(ShowStatusBarProperty); }
            set { this.SetValue(ShowStatusBarProperty, value); }
        }

        /// <summary>상태바 오른쪽 자유 텍스트.</summary>
        public string StatusText
        {
            get { return (string)this.GetValue(StatusTextProperty); }
            set { this.SetValue(StatusTextProperty, value); }
        }

        /// <summary>상태바 왼쪽 건수 표기 형식.</summary>
        public string StatusCountFormat
        {
            get { return (string)this.GetValue(StatusCountFormatProperty); }
            set { this.SetValue(StatusCountFormatProperty, value); }
        }

        /// <summary>행 배경색을 담은 멤버 경로.</summary>
        public string RowColorMemberPath
        {
            get { return (string)this.GetValue(RowColorMemberPathProperty); }
            set { this.SetValue(RowColorMemberPathProperty, value); }
        }

        /// <summary>소제목 행 여부를 담은 멤버 경로(참이면 굵은 글씨 + 헤더 띠 + 위 구분선).</summary>
        public string RowHeaderMemberPath
        {
            get { return (string)this.GetValue(RowHeaderMemberPathProperty); }
            set { this.SetValue(RowHeaderMemberPathProperty, value); }
        }

        private void OnVisibleNodesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.RefreshStatusBar();
        }

        private static void OnStatusBarAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernTreeListControl)d).RefreshStatusBar();
        }

        private static void OnRowColorMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernTreeListControl control = (ModernTreeListControl)d;

            // 이미 실체화된 행에 새 경로를 반영하려면 다시 실체화시켜 LoadingRow를 재발생시킨다.
            if (control.InnerDataGrid.Items.Count > 0)
            {
                control.InnerDataGrid.Items.Refresh();
            }
        }

        // 상태바 표시/텍스트를 현재 속성과 보이는 노드 수로 다시 그린다.
        // 형식 문자열 오류는 형식 그대로 출력하는 것으로 완화한다(예외 없음).
        private void RefreshStatusBar()
        {
            this.StatusBarStrip.Visibility = this.ShowStatusBar ? Visibility.Visible : Visibility.Collapsed;

            if (!this.ShowStatusBar)
            {
                return;
            }

            string format = this.StatusCountFormat;
            string countText;

            try
            {
                countText = string.Format(format ?? string.Empty, this.visibleNodes.Count);
            }
            catch (FormatException)
            {
                countText = format;
            }

            this.StatusCountText.Text = countText;
            this.StatusRightText.Text = this.StatusText ?? string.Empty;
        }

        // 상태별 행 배경 — 선택 > hover > 행 색 > 소제목 띠 > 기본 (그리드와 같은 규칙 + 소제목).
        // 소제목의 굵기·위 구분선은 배경과 따로 걸어 선택·hover·행 색 위에서도 남는다.
        private void PaintRowBackground(DataGridRow row)
        {
            bool header = this.IsHeaderRow(row);

            if (header)
            {
                row.FontWeight = FontWeights.SemiBold;
                row.SetResourceReference(Control.BorderBrushProperty, "Brush.Border");
                row.BorderThickness = new Thickness(0d, 1d, 0d, 0d);
            }
            else
            {
                row.ClearValue(Control.FontWeightProperty);
                row.ClearValue(Control.BorderBrushProperty);
                row.ClearValue(Control.BorderThicknessProperty);
            }

            if (row.IsSelected)
            {
                row.SetResourceReference(Control.BackgroundProperty, "Brush.SelectedBackground");
                row.ClearValue(Control.ForegroundProperty);
            }
            else if (row.IsMouseOver)
            {
                row.SetResourceReference(Control.BackgroundProperty, "Brush.HoverBackground");
                row.ClearValue(Control.ForegroundProperty);
            }
            else if (!this.TryPaintRowColor(row))
            {
                if (header)
                {
                    row.SetResourceReference(Control.BackgroundProperty, "Brush.GridHeaderBackground");
                }
                else
                {
                    row.ClearValue(Control.BackgroundProperty);
                }

                row.ClearValue(Control.ForegroundProperty);
            }
        }

        // RowHeaderMemberPath 값이 참인 행이 소제목이다 — 참 판정은 체크 칸과 같은 어휘(TruthyToBooleanConverter.IsTruthy).
        private bool IsHeaderRow(DataGridRow row)
        {
            string path = this.RowHeaderMemberPath;

            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            TreeListNode node = row.Item as TreeListNode;
            object value = MemberPathReader.Read(node == null ? row.Item : node.Row, path);
            return TruthyToBooleanConverter.IsTruthy(value);
        }

        // RowColorMemberPath 값이 색이면 그 색으로 칠하고 true 를 돌려준다. 글자색은 배경에서 유도한다 —
        // 어두운 테마의 밝은 파스텔 행 색(CrimsonGray 의 Error 칩) 위에 테마 기본 글자(흰색)가 놓여 안 보였다
        // (2026-10-15 사용자 보고, 규칙 안내 창). ModernDataGridControl 의 행 색 규칙과 같다.
        private bool TryPaintRowColor(DataGridRow row)
        {
            string path = this.RowColorMemberPath;

            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            TreeListNode node = row.Item as TreeListNode;
            object value = MemberPathReader.Read(node == null ? row.Item : node.Row, path);
            Color color;

            if (!ChipColorHelper.TryParseColor(value == null ? string.Empty : value.ToString(), out color))
            {
                return false;
            }

            SolidColorBrush brush = new SolidColorBrush(color);
            brush.Freeze();
            row.Background = brush;

            SolidColorBrush foreground = new SolidColorBrush(ChipColorHelper.DeriveForeground(color));
            foreground.Freeze();
            row.Foreground = foreground;
            return true;
        }

        // 행 스타일 EventSetter — Selected/Unselected는 최초 로딩의 자동 선택
        // 행(컨테이너 준비 시점)도 잡는다.
        private void OnRowSelectionStateChanged(object sender, RoutedEventArgs e)
        {
            this.PaintRowBackground((DataGridRow)sender);
        }

        // 행 스타일 EventSetter — 마우스 hover 진입/이탈.
        private void OnRowMouseStateChanged(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.PaintRowBackground((DataGridRow)sender);
        }

        // 우클릭 지점에서 시각 트리를 거슬러 행(DataGridRow)을 찾아 선택한 뒤
        // RowRightClick을 발생시킨다. 행 밖 우클릭은 무시한다. 셰브런(펼침 버튼)
        // 위 우클릭도 행 선택으로 본다 — 펼침/접기는 좌클릭만 담당한다. 좌클릭과
        // 같이 **클릭한 셀을 현재 셀로도 갱신**해 언더라인이 옮겨가고 Copy cell의
        // 기준이 된다(그리드와 동일).
        private void OnGridRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DependencyObject source = e.OriginalSource as DependencyObject;
            DataGridCell cellContainer = null;

            while (source != null && !(source is DataGridRow))
            {
                if (cellContainer == null)
                {
                    cellContainer = source as DataGridCell;
                }

                source = source is Visual
                        ? VisualTreeHelper.GetParent(source)
                        : LogicalTreeHelper.GetParent(source);
            }

            DataGridRow row = source as DataGridRow;

            if (row == null)
            {
                return;
            }

            if (!row.IsSelected)
            {
                this.InnerDataGrid.SelectedItem = row.Item;
            }

            if (cellContainer != null && cellContainer.Column != null)
            {
                this.InnerDataGrid.CurrentCell =
                        new DataGridCellInfo(cellContainer.DataContext, cellContainer.Column);
            }

            e.Handled = true;

            if (this.RowRightClick != null)
            {
                this.RowRightClick(this, EventArgs.Empty);
            }
        }

        /// <summary>트리를 구성할 행 목록.</summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)this.GetValue(ItemsSourceProperty); }
            set { this.SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>노드 키 컬럼/속성 이름.</summary>
        public string IdMemberPath
        {
            get { return (string)this.GetValue(IdMemberPathProperty); }
            set { this.SetValue(IdMemberPathProperty, value); }
        }

        /// <summary>부모 키 컬럼/속성 이름.</summary>
        public string ParentIdMemberPath
        {
            get { return (string)this.GetValue(ParentIdMemberPathProperty); }
            set { this.SetValue(ParentIdMemberPathProperty, value); }
        }

        /// <summary>트리 컬럼 표시 텍스트 컬럼/속성 이름.</summary>
        public string DisplayMemberPath
        {
            get { return (string)this.GetValue(DisplayMemberPathProperty); }
            set { this.SetValue(DisplayMemberPathProperty, value); }
        }

        /// <summary>트리 컬럼(첫 컬럼)의 헤더 캡션.</summary>
        public string TreeHeaderText
        {
            get { return (string)this.GetValue(TreeHeaderTextProperty); }
            set { this.SetValue(TreeHeaderTextProperty, value); }
        }

        /// <summary>트리 컬럼 픽셀 폭 (0 이하 = 남은 공간 채움).</summary>
        public double TreeColumnWidth
        {
            get { return (double)this.GetValue(TreeColumnWidthProperty); }
            set { this.SetValue(TreeColumnWidthProperty, value); }
        }

        /// <summary>노드 0개일 때의 안내 문구 (기본 "No data", 빈 문자열 = 끔).</summary>
        public string EmptyText
        {
            get { return (string)this.GetValue(EmptyTextProperty); }
            set { this.SetValue(EmptyTextProperty, value); }
        }

        /// <summary>선택 노드의 키. null은 미선택.</summary>
        public object SelectedValue
        {
            get { return this.GetValue(SelectedValueProperty); }
            set { this.SetValue(SelectedValueProperty, value); }
        }

        /// <summary>선택 노드의 원본 행 (미선택이면 null).</summary>
        public object SelectedItem
        {
            get
            {
                TreeListNode node = this.FindNodeByValue(this.SelectedValue);
                return node != null ? node.Row : null;
            }
        }

        /// <summary>
        /// 값 컬럼을 명시적 정의로 교체한다 (트리 컬럼 오른쪽에 순서대로 붙는다).
        /// Text/Badge Kind만 지원하며 그 외 Kind는 Text로 강등된다. null이나 빈
        /// 목록을 넘기면 트리 컬럼만 남는다.
        /// </summary>
        public void ApplyColumns(IList<ModernDataGridColumn> columns)
        {
            this.valueColumnDefinitions = columns == null
                    ? null
                    : new List<ModernDataGridColumn>(columns);
            this.RebuildColumns();
        }

        /// <summary>모든 노드를 펼친다.</summary>
        public void ExpandAll()
        {
            foreach (TreeListNode node in this.allNodes)
            {
                node.IsExpanded = true;
            }

            this.RebuildVisible();
        }

        /// <summary>모든 노드를 접는다 (루트만 남는다).</summary>
        public void CollapseAll()
        {
            foreach (TreeListNode node in this.allNodes)
            {
                node.IsExpanded = false;
            }

            this.RebuildVisible();
        }

        // ===== 컬럼 구성 =====

        private static void OnColumnShapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernTreeListControl)d).RebuildColumns();
        }

        // 트리 컬럼(항상 첫 컬럼) + 값 컬럼들을 다시 만든다. 값 컬럼 생성은
        // GridColumnFactory를 재사용한다 — 셀 DataContext가 TreeListNode이므로
        // 바인딩 경로에 "Row." 접두어를 붙인 사본 정의를 넘긴다.
        private void RebuildColumns()
        {
            this.InnerDataGrid.Columns.Clear();
            this.filterController.OnColumnsRebuilt();
            this.searchDefinitions.Clear();
            this.InnerDataGrid.Columns.Add(this.BuildTreeColumn());

            // 0번(트리 컬럼)의 찾기 대상은 노드가 만든 표시 텍스트다 — 원본 컬럼이
            // 아니라 들여쓰기 없는 그 글자를 찾는 것이 화면과 같은 어휘다.
            ModernDataGridColumn treeSearch = new ModernDataGridColumn(
                    "Text", this.TreeHeaderText ?? string.Empty, 0d);
            treeSearch.Kind = GridColumnKind.Text;
            this.searchDefinitions.Add(treeSearch);

            if (this.valueColumnDefinitions == null)
            {
                return;
            }

            foreach (ModernDataGridColumn definition in this.valueColumnDefinitions)
            {
                if (definition == null || string.IsNullOrEmpty(definition.DataPropertyName))
                {
                    continue;
                }

                // 배지 컬럼은 그리드와 같은 규칙 — 자동색 값을 선등록하고, 같은 컬럼의
                // 배지는 가장 긴 표시값 기준으로 같은 폭을 쓴다(행마다 들쭉날쭉하지 않게).
                // 측정은 원본 행(ItemsSource)과 원본 컬럼 이름으로 한다 — 노드 경로
                // 사본("Row.")은 셀 바인딩용이다. 데이터가 바뀌면 RebuildTree가 다시 부른다.
                double cellMinWidth = 0d;

                if (definition.Kind == GridColumnKind.Badge)
                {
                    cellMinWidth = GridColumnFactory.CalculateBadgeMinWidth(
                            definition, this.ItemsSource, this, 1d);
                }

                ModernDataGridColumn prefixed = PrefixDefinition(definition);

                DataGridColumn column = GridColumnFactory.CreateColumn(
                        prefixed,
                        1d,
                        this,
                        null,
                        null,
                        null,
                        delegate { },
                        cellMinWidth,
                        null);

                // 값 필터: 헤더 오른쪽 깔때기 — 값은 노드가 품은 원본 행에서 읽는다
                // ("Row." 경로). 트리 컬럼에는 붙이지 않는다.
                if (this.AllowColumnFilters)
                {
                    this.filterController.AttachHeader(column, prefixed);
                }

                if (definition.HeaderAlignment != GridTextAlignment.Center || GridHeaderCaption.IsCaptionHeader(column.Header)
                        || definition.TopLabelAlignment.HasValue || definition.BottomLabelAlignment.HasValue)
                {
                    ApplyHeaderAlignment(column, prefixed);
                }

                this.searchDefinitions.Add(prefixed);
                this.InnerDataGrid.Columns.Add(column);
            }

            // 위·아래 작은 글씨가 둘 다 있거나 여러 줄 캡션이 있으면 헤더 높이를 그리드와 같은 계산으로 늘린다.
            this.InnerDataGrid.ColumnHeaderHeight =
                    GridAutoFitMeasurer.ComputeHeaderHeight(this, this.valueColumnDefinitions);
        }

        // 헤더 캡션 정렬 — 그리드와 같은 규칙. 작은 글씨 없는 문자열 헤더는 Center·Left 에서 그대로 두고,
        // Right 만 TextBlock 으로 바꾼다. 세로 패널 헤더는 줄마다 정렬한다(위·아래 줄은 자기 정렬, 없으면 본 캡션 정렬).
        private static void ApplyHeaderAlignment(DataGridColumn column, ModernDataGridColumn definition)
        {
            GridTextAlignment caption = definition.HeaderAlignment;
            string plain = column.Header as string;

            if (plain != null)
            {
                if (caption == GridTextAlignment.Center)
                {
                    return;
                }

                if (caption == GridTextAlignment.Right)
                {
                    TextBlock text = new TextBlock();
                    text.Text = plain;
                    column.Header = text;
                }
            }

            TextBlock single = column.Header as TextBlock;

            if (single != null)
            {
                single.TextAlignment = ToTextAlignment(caption);
            }
            else if (GridHeaderCaption.IsCaptionHeader(column.Header))
            {
                GridTextAlignment top = definition.TopLabelAlignment ?? caption;
                GridTextAlignment bottom = definition.BottomLabelAlignment ?? caption;

                foreach (TextBlock line in GridHeaderCaption.Lines((Panel)column.Header))
                {
                    GridTextAlignment lineAlignment = GridHeaderCaption.IsTopLabel(line) ? top
                            : GridHeaderCaption.IsBottomLabel(line) ? bottom
                            : caption;
                    line.TextAlignment = ToTextAlignment(lineAlignment);
                    line.HorizontalAlignment = ToHorizontalAlignment(lineAlignment);
                }
            }
        }

        private static HorizontalAlignment ToHorizontalAlignment(GridTextAlignment alignment)
        {
            return alignment == GridTextAlignment.Right ? HorizontalAlignment.Right
                    : alignment == GridTextAlignment.Center ? HorizontalAlignment.Center
                    : HorizontalAlignment.Left;
        }

        private static TextAlignment ToTextAlignment(GridTextAlignment alignment)
        {
            return alignment == GridTextAlignment.Right ? TextAlignment.Right
                    : alignment == GridTextAlignment.Center ? TextAlignment.Center
                    : TextAlignment.Left;
        }

        private DataGridColumn BuildTreeColumn()
        {
            DataGridTemplateColumn column = new DataGridTemplateColumn();
            column.Header = this.TreeHeaderText;
            column.CellTemplate = (DataTemplate)this.FindResource("TreeCellTemplate");
            column.CanUserSort = false;

            double width = this.TreeColumnWidth;
            column.Width = width > 0d
                    ? new DataGridLength(width)
                    : new DataGridLength(1d, DataGridLengthUnitType.Star);
            return column;
        }

        // 값 컬럼 정의를 노드 바인딩 경로("Row.컬럼명")로 바꾼 사본.
        // Text/Badge 외 Kind(체크박스/버튼/콤보 등 상호작용 셀)는 트리 리스트가
        // 지원하지 않으므로 Text로 강등한다 — 예외를 던지지 않는다(계약 규칙).
        //
        // 배지 옵션은 그리드와 같은 것을 전부 옮긴다 — 컬럼 이름을 가리키는 멤버
        // (색/의미/그룹/스핀 판정 컬럼)는 "Row." 접두어를 붙이고, 값 선언형
        // (자동색·도메인·스핀 값·강조 값)은 그대로 복사한다. 예전에는 BadgeColorMember만
        // 옮겨 트리의 상태 배지가 자동색·스핀을 쓸 수 없었다(2026-08-29 Equipment Port
        // 화면에서 발견).
        private static ModernDataGridColumn PrefixDefinition(ModernDataGridColumn source)
        {
            ModernDataGridColumn clone = new ModernDataGridColumn(
                    "Row." + source.DataPropertyName, source.HeaderText, source.Width);
            clone.Kind = source.Kind == GridColumnKind.Badge
                    ? GridColumnKind.Badge
                    : GridColumnKind.Text;
            // 헤더의 위·아래 작은 글씨와 정렬도 그리드와 같이 옮긴다 — 전에는 본 캡션만 옮겨 엔티티의
            // TopLabel·BottomLabel 이 트리에서는 보이지 않았다(2026-10-18 사용자 보고).
            clone.TopLabel = source.TopLabel;
            clone.BottomLabel = source.BottomLabel;
            clone.HeaderAlignment = source.HeaderAlignment;
            clone.TopLabelAlignment = source.TopLabelAlignment;
            clone.BottomLabelAlignment = source.BottomLabelAlignment;
            clone.TextAlignment = source.TextAlignment;
            clone.Format = source.Format;
            clone.TextColor = source.TextColor;
            clone.TextSemiBold = source.TextSemiBold;
            clone.BadgeShape = source.BadgeShape;
            clone.BadgeAutoColor = source.BadgeAutoColor;
            clone.BadgeSpinValues = source.BadgeSpinValues;
            clone.BadgeAccentValues = source.BadgeAccentValues;
            clone.BadgeColorMember = PrefixMember(source.BadgeColorMember);
            clone.BadgeSemanticMember = PrefixMember(source.BadgeSemanticMember);
            clone.BadgeGroupMember = PrefixMember(source.BadgeGroupMember);
            clone.BadgeSpinBorderMember = PrefixMember(source.BadgeSpinBorderMember);

            return clone;
        }

        private static string PrefixMember(string member)
        {
            return string.IsNullOrEmpty(member) ? member : "Row." + member;
        }

        // ===== 트리 구성 =====

        private static void OnDataShapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernTreeListControl)d).RebuildTree();
        }

        // 평면 행 목록을 트리로 재구성한다 — ModernTreeView와 같은 규칙:
        // ① 키 → 노드 사전 구성, ② 부모 키로 연결(부모가 없거나 자기 자신이면
        // 루트), ③ 깊이 계산 후 보이는 목록 평탄화. 키 없는/중복 행은 건너뛰고
        // null/빈 소스는 빈 트리 — 예외를 던지지 않는다.
        private void RebuildTree()
        {
            this.allNodes.Clear();
            this.rootNodes.Clear();

            IEnumerable source = this.ItemsSource;

            if (source != null && !string.IsNullOrEmpty(this.IdMemberPath))
            {
                Dictionary<string, TreeListNode> nodesByKey = new Dictionary<string, TreeListNode>();
                List<KeyValuePair<TreeListNode, string>> parentKeys =
                        new List<KeyValuePair<TreeListNode, string>>();

                foreach (object row in source)
                {
                    object idValue = MemberPathReader.Read(row, this.IdMemberPath);
                    string key = ToKeyString(idValue);

                    if (key.Length == 0 || nodesByKey.ContainsKey(key))
                    {
                        continue;
                    }

                    string displayText = MemberPathReader.ReadDisplayText(row, this.DisplayMemberPath);
                    TreeListNode node = new TreeListNode(idValue, displayText, row);

                    nodesByKey.Add(key, node);
                    this.allNodes.Add(node);

                    string parentKey = ToKeyString(MemberPathReader.Read(row, this.ParentIdMemberPath));
                    parentKeys.Add(new KeyValuePair<TreeListNode, string>(node, parentKey));
                }

                foreach (KeyValuePair<TreeListNode, string> entry in parentKeys)
                {
                    TreeListNode node = entry.Key;
                    string parentKey = entry.Value;
                    TreeListNode parent;

                    // 순환 데이터(A→B→A 등) 방어: 순환을 닫는 연결이면 부모를 잇지
                    // 않고 루트로 승격한다 — 행이 사라지거나 조상 탐색이 무한 루프에
                    // 빠지는 것을 막는다. 이 가드로 Parent 사슬은 항상 비순환이다.
                    if (parentKey.Length > 0 &&
                        parentKey != ToKeyString(node.Id) &&
                        nodesByKey.TryGetValue(parentKey, out parent) &&
                        !WouldCreateCycle(node, parent))
                    {
                        node.Parent = parent;
                        parent.Children.Add(node);
                    }
                    else
                    {
                        this.rootNodes.Add(node);
                    }
                }

                foreach (TreeListNode root in this.rootNodes)
                {
                    AssignDepth(root, 0);
                }
            }

            this.RebuildVisible();

            // 배지 통일 폭은 현재 데이터의 가장 긴 값에 기대므로 데이터가 바뀌면 컬럼을 다시 만든다.
            this.RebuildColumns();

            // 보류/기존 SelectedValue 적용. 새 트리에 없는 값이면 미선택으로 정리.
            this.ApplySelectedValueToGrid();

            if (this.SelectedValue != null && this.FindNodeByValue(this.SelectedValue) == null)
            {
                this.SelectedValue = null;
            }
        }

        // 제안된 부모의 조상 사슬에 자신이 이미 있으면 이 연결은 순환을 닫는다.
        // 기존 연결은 항상 비순환이므로 탐색은 유한하다.
        private static bool WouldCreateCycle(TreeListNode node, TreeListNode proposedParent)
        {
            TreeListNode ancestor = proposedParent;

            while (ancestor != null)
            {
                if (ReferenceEquals(ancestor, node))
                {
                    return true;
                }

                ancestor = ancestor.Parent;
            }

            return false;
        }

        private static void AssignDepth(TreeListNode node, int depth)
        {
            node.Depth = depth;

            foreach (TreeListNode child in node.Children)
            {
                AssignDepth(child, depth + 1);
            }
        }

        // 펼침 상태를 반영해 보이는 노드 목록을 처음부터 다시 만든다.
        private void RebuildVisible()
        {
            this.visibleNodes.Clear();

            bool filtered = this.filterController.HasActiveFilters;

            foreach (TreeListNode root in this.rootNodes)
            {
                if (filtered)
                {
                    this.AppendFiltered(root);
                }
                else
                {
                    this.AppendVisible(root);
                }
            }

            this.RefreshEmptyLabel();
        }

        private void AppendVisible(TreeListNode node)
        {
            this.visibleNodes.Add(node);

            if (!node.IsExpanded)
            {
                return;
            }

            foreach (TreeListNode child in node.Children)
            {
                this.AppendVisible(child);
            }
        }

        // 값 필터가 걸린 동안의 평탄화 — 걸린 노드와 그 조상만 남기고, 남긴 가지는
        // 접힘과 무관하게 펼쳐 보여 준다(거르는 중에 접혀 있으면 결과가 숨는다).
        private void AppendFiltered(TreeListNode node)
        {
            if (!this.SubtreeMatches(node))
            {
                return;
            }

            this.visibleNodes.Add(node);

            foreach (TreeListNode child in node.Children)
            {
                this.AppendFiltered(child);
            }
        }

        // 자기가 걸렸거나 자손 중 하나라도 걸리면 남는다.
        private bool SubtreeMatches(TreeListNode node)
        {
            if (this.filterController.Matches(node))
            {
                return true;
            }

            foreach (TreeListNode child in node.Children)
            {
                if (this.SubtreeMatches(child))
                {
                    return true;
                }
            }

            return false;
        }

        // ===== 펼침/접기 =====

        // 셰브런 클릭 — 그 노드만 펼치거나 접는다. 전체 재구성 대신 보이는
        // 목록에 자손 구간을 삽입/제거해 스크롤 위치를 보존한다.
        private void OnExpanderClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            TreeListNode node = ((FrameworkElement)sender).DataContext as TreeListNode;

            if (node == null || !node.HasChildren)
            {
                return;
            }

            int index = this.visibleNodes.IndexOf(node);

            if (index < 0)
            {
                return;
            }

            if (node.IsExpanded)
            {
                node.IsExpanded = false;

                // 노드 뒤에 이어지는, 이 노드보다 깊은 행이 전부 자손이다.
                while (index + 1 < this.visibleNodes.Count
                        && this.visibleNodes[index + 1].Depth > node.Depth)
                {
                    this.visibleNodes.RemoveAt(index + 1);
                }
            }
            else
            {
                node.IsExpanded = true;

                List<TreeListNode> expanded = new List<TreeListNode>();
                CollectVisibleDescendants(node, expanded);

                for (int offset = 0; offset < expanded.Count; offset++)
                {
                    this.visibleNodes.Insert(index + 1 + offset, expanded[offset]);
                }
            }
        }

        // 펼쳐진 하위 트리만 따라 내려가며 보이게 될 자손을 모은다.
        private static void CollectVisibleDescendants(TreeListNode node, List<TreeListNode> target)
        {
            foreach (TreeListNode child in node.Children)
            {
                target.Add(child);

                if (child.IsExpanded)
                {
                    CollectVisibleDescendants(child, target);
                }
            }
        }

        // ===== 선택 동기화 =====

        private void InnerDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 상태별 행 배경 다시 칠하기는 행 스타일의 Selected/Unselected
            // EventSetter가 담당한다 (컨테이너가 아직 없는 최초 로딩도 커버).
            if (this.suppressSelectionSync)
            {
                return;
            }

            TreeListNode node = this.InnerDataGrid.SelectedItem as TreeListNode;

            this.suppressSelectionSync = true;

            try
            {
                this.SelectedValue = node != null ? node.Id : null;
            }
            finally
            {
                this.suppressSelectionSync = false;
            }
        }

        private static void OnSelectedValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernTreeListControl control = (ModernTreeListControl)d;

            if (!control.suppressSelectionSync)
            {
                control.ApplySelectedValueToGrid();
            }

            if (control.SelectedValueChanged != null)
            {
                control.SelectedValueChanged(control, EventArgs.Empty);
            }
        }

        // SelectedValue에 해당하는 노드를 그리드 선택으로 되비춘다 — 접힌
        // 조상은 펼쳐 노드가 보이게 하고 스크롤한다.
        private void ApplySelectedValueToGrid()
        {
            TreeListNode target = this.FindNodeByValue(this.SelectedValue);

            this.suppressSelectionSync = true;

            try
            {
                if (target == null)
                {
                    this.InnerDataGrid.SelectedItem = null;
                    return;
                }

                bool expandedAny = false;
                TreeListNode ancestor = target.Parent;

                while (ancestor != null)
                {
                    if (!ancestor.IsExpanded)
                    {
                        ancestor.IsExpanded = true;
                        expandedAny = true;
                    }

                    ancestor = ancestor.Parent;
                }

                if (expandedAny || !this.visibleNodes.Contains(target))
                {
                    this.RebuildVisible();
                }

                this.InnerDataGrid.SelectedItem = target;
                this.InnerDataGrid.ScrollIntoView(target);
            }
            finally
            {
                this.suppressSelectionSync = false;
            }
        }

        private TreeListNode FindNodeByValue(object value)
        {
            if (value == null)
            {
                return null;
            }

            string key = ToKeyString(value);

            foreach (TreeListNode node in this.allNodes)
            {
                if (ToKeyString(node.Id) == key)
                {
                    return node;
                }
            }

            return null;
        }

        // ===== 공통 =====

        // 빈 상태 안내: 노드 0개 + EmptyText 비어있지 않을 때만 표시한다.
        private void RefreshEmptyLabel()
        {
            string text = this.EmptyText;
            bool show = this.visibleNodes.Count == 0 && !string.IsNullOrEmpty(text);

            this.EmptyLabel.Text = text;
            this.EmptyLabel.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private static void OnEmptyTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernTreeListControl)d).RefreshEmptyLabel();
        }

        // 키 비교는 문자열 기준 — DataTable의 박싱 타입 차이(int vs string)에 관대.
        private static string ToKeyString(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            return value.ToString().Trim();
        }
    }
}
