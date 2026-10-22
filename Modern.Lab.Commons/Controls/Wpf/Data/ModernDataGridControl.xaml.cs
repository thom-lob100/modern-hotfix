using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Modern.Lab.Controls.Wpf.Common;

namespace Modern.Lab.Controls.Wpf.Data
{
    /// <summary>
    /// 모던 읽기 전용 데이터 그리드.
    /// - ItemsSource: 임의의 IEnumerable (DataView, IList, ...)
    /// - SelectedItem: 현재 행 (양방향)
    /// - AutoGenerateColumns: 기본값 true; ApplyColumns 호출 시 명시적 컬럼으로 전환
    /// - SelectionChanged: 행 선택이 바뀔 때 발생
    /// - ShowStatusBar: 그리드 하단 상태바 표시 여부 (행 수 자동 표기 + StatusText)
    /// - AllowFindPanel: Ctrl+F 찾기 — 엑셀식 소유된 모덜리스 찾기 창(일치 강조 + Find Next/Find All 이동)
    /// </summary>
    public partial class ModernDataGridControl : UserControl, IHostDisposalAware
    {
        /// <summary>표시할 행 목록. 임의의 IEnumerable (DataView, IList, ...).</summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
                typeof(IEnumerable),
                typeof(ModernDataGridControl),
                new PropertyMetadata(null, OnItemsSourcePropertyChanged));

        /// <summary>
        /// 데이터가 0건일 때 데이터 영역 가운데에 표시할 안내 문구.
        /// 기본 "No data" — 화면 문맥에 맞게 바꾸거나 빈 문자열로 끈다.
        /// </summary>
        public static readonly DependencyProperty EmptyTextProperty =
            DependencyProperty.Register(
                "EmptyText",
                typeof(string),
                typeof(ModernDataGridControl),
                new PropertyMetadata("No data", OnEmptyTextChanged));

        /// <summary>
        /// 컬럼 너비를 헤더 캡션과 데이터 내용 중 더 넓은 쪽에 맞춰 자동 계산할지
        /// 여부. ApplyColumns로 정의된 컬럼에만 적용되며, 켜져 있으면 데이터가
        /// 바뀔 때마다 각 컬럼이 잘림 없이 표시되는 최소 너비로 재계산된다
        /// (정의의 Width는 무시). 스크롤 중 너비가 흔들리는 WPF Auto 크기 대신
        /// 전체 데이터를 미리 측정해 고정 픽셀 너비를 넣는다.
        /// </summary>
        public static readonly DependencyProperty AutoFitColumnsProperty =
            DependencyProperty.Register(
                "AutoFitColumns",
                typeof(bool),
                typeof(ModernDataGridControl),
                new PropertyMetadata(false, OnAutoFitColumnsChanged));

        /// <summary>
        /// 컬럼 가상화(기본 false) — WPF DataGrid의 EnableColumnVirtualization을 그대로 노출한다.
        /// 켜면 가로 뷰포트 밖 컬럼의 셀을 만들지 않아 컬럼이 많은 표의 첫 표시와 세로 스크롤이
        /// 빨라진다(2026-09-12 실측: 64컬럼·2000행에서 bind 1279→479ms, 세로 스크롤 496→162ms).
        /// 대신 가로로 크게 옮길 때 새 컬럼의 셀을 그때 만들어 200ms 안팎이 든다. 그리고 **컬럼 폭이
        /// 절대값일 때만** 효과가 있다 — 헤더 기준·자동 폭은 폭을 재려고 셀을 어차피 다 만든다.
        /// AutoFitColumns가 켜진 표(측정 뒤 고정 픽셀 폭)가 그 조건이다. 기본을 끈 채 두는 이유는
        /// 가로 스크롤 비용과 셀 재생성이 병합 오버레이·고정 컬럼 같은 좌표 기반 장식과 상호작용하기
        /// 때문이다 — 세로 목록 위주의 넓은 읽기 전용 표에서 화면 단위로 켠다.
        /// </summary>
        public static readonly DependencyProperty EnableColumnVirtualizationProperty =
            DependencyProperty.Register(
                "EnableColumnVirtualization",
                typeof(bool),
                typeof(ModernDataGridControl),
                new PropertyMetadata(false));

        /// <summary>
        /// 컬럼 값 필터(헤더 깔때기) 사용 여부 (기본 true). ApplyColumns로
        /// 정의한 텍스트/배지 컬럼 헤더에 깔때기 버튼이 붙고, 클릭 시 고유 값
        /// 체크리스트로 행을 거른다 (엑셀식 값 필터 — 뷰에만 적용, 원본
        /// 데이터는 그대로). 선택 상태는 DataSource 재할당 후에도 유지된다.
        /// 필터가 어울리지 않는 그리드(짧은 고정 목록 등)는 끄면 된다.
        /// </summary>
        public static readonly DependencyProperty AllowColumnFiltersProperty =
            DependencyProperty.Register(
                "AllowColumnFilters",
                typeof(bool),
                typeof(ModernDataGridControl),
                new PropertyMetadata(true, OnAllowColumnFiltersChanged));

        /// <summary>
        /// 찾기(Ctrl+F) 사용 여부 (기본 true). 켜져 있으면 Ctrl+F로 그리드
        /// 오른쪽 위에 엑셀식 찾기 팝업이 열린다 — 행을 거르지 않고 **일치
        /// 위치로 이동**한다: Find Next(Enter/F3)는 다음 일치 셀로 선택·스크롤,
        /// Find All은 일치 목록을 나열하고 클릭하면 그 셀로 이동. 화면에 보이는
        /// 일치 텍스트는 노란 배경으로 강조된다. 대상은 텍스트를 담은 컬럼
        /// (Text/Link/Badge), 부분 일치·대소문자 무시. Esc/✕로 닫는다.
        /// </summary>
        public static readonly DependencyProperty AllowFindPanelProperty =
            DependencyProperty.Register(
                "AllowFindPanel",
                typeof(bool),
                typeof(ModernDataGridControl),
                new PropertyMetadata(true, OnAllowFindPanelChanged));

        /// <summary>
        /// 가로 스크롤 시 왼쪽에 고정할 컬럼 수 (기본 0 = 없음). 컬럼 정의의
        /// Frozen 플래그로도 설정된다 (ApplyColumns가 마지막 Frozen 컬럼까지의
        /// 개수로 이 값을 세팅) — 명시적 컬럼 정의 화면은 플래그 방식을 권장.
        /// </summary>
        public static readonly DependencyProperty FrozenColumnCountProperty =
            DependencyProperty.Register(
                "FrozenColumnCount",
                typeof(int),
                typeof(ModernDataGridControl),
                new PropertyMetadata(0, OnFrozenColumnCountChanged));

        /// <summary>
        /// 다중 행 선택 허용 여부 (기본 false = 단일 선택). true면 Shift+클릭
        /// (범위)/Ctrl+클릭(개별 토글)으로 여러 행을 고를 수 있다 — 선택 결과는
        /// SelectedItemsInViewOrder()로 읽는다. SelectedItem/SelectedIndex는
        /// 현재 행(마지막 클릭) 의미 그대로 유지된다.
        /// </summary>
        public static readonly DependencyProperty MultiSelectProperty =
            DependencyProperty.Register(
                "MultiSelect",
                typeof(bool),
                typeof(ModernDataGridControl),
                new PropertyMetadata(false, OnMultiSelectChanged));

        /// <summary>
        /// 읽기 전용 여부 (기본 true). false면 텍스트 셀을 더블클릭/F2/타이핑으로
        /// 편집할 수 있다 — 컬럼 정의의 ReadOnly로 개별 컬럼만 잠글 수 있다.
        /// 숫자/날짜 컬럼은 표시 서식 그대로 편집되고, 타입에 안 맞는 입력은
        /// 빨간 테두리와 함께 커밋이 거부된다.
        /// </summary>
        public static readonly DependencyProperty ReadOnlyProperty =
            DependencyProperty.Register(
                "ReadOnly",
                typeof(bool),
                typeof(ModernDataGridControl),
                new PropertyMetadata(true));

        /// <summary>현재 선택된 행 항목. 기본적으로 양방향 바인딩.</summary>
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                "SelectedItem",
                typeof(object),
                typeof(ModernDataGridControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>데이터 소스로부터 컬럼을 자동 생성할지 여부.</summary>
        public static readonly DependencyProperty AutoGenerateColumnsProperty =
            DependencyProperty.Register(
                "AutoGenerateColumns",
                typeof(bool),
                typeof(ModernDataGridControl),
                new PropertyMetadata(true));

        /// <summary>그리드 하단 상태바 표시 여부.</summary>
        public static readonly DependencyProperty ShowStatusBarProperty =
            DependencyProperty.Register(
                "ShowStatusBar",
                typeof(bool),
                typeof(ModernDataGridControl),
                new PropertyMetadata(false, OnStatusBarAppearanceChanged));

        /// <summary>상태바 오른쪽에 표시할 자유 텍스트 (빈 문자열이면 표시 없음).</summary>
        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register(
                "StatusText",
                typeof(string),
                typeof(ModernDataGridControl),
                new PropertyMetadata(string.Empty, OnStatusBarAppearanceChanged));

        /// <summary>
        /// 상태바 왼쪽 행 수 표기 형식. {0}에 현재 행 수가 들어간다.
        /// 기본 "{0:N0} rows" — 같은 화면에 놓이는 ModernPagination의 표기와
        /// 같은 문형이다(둘이 다른 언어로 나오면 한 화면 안에서 어긋나 보인다).
        /// </summary>
        public static readonly DependencyProperty StatusCountFormatProperty =
            DependencyProperty.Register(
                "StatusCountFormat",
                typeof(string),
                typeof(ModernDataGridControl),
                new PropertyMetadata("{0:N0} rows", OnStatusBarAppearanceChanged));

        /// <summary>
        /// 교차 행 배경(줄무늬) 표시 여부. 켜면 홀수 행이 테마 교차색
        /// (Brush.GridRowAlt)으로 칠해진다. 기본 꺼짐 — 행 높이·수평 구분선만으로
        /// 행이 구분되는 밀도에서는 줄무늬가 시각적 소음이 되기 쉽다.
        /// </summary>
        public static readonly DependencyProperty AlternatingRowColorsProperty =
            DependencyProperty.Register(
                "AlternatingRowColors",
                typeof(bool),
                typeof(ModernDataGridControl),
                new PropertyMetadata(false, OnAlternatingRowColorsChanged));

        /// <summary>
        /// 행 배경색으로 쓸 컬럼/속성 이름 (선택 사항). 값은 "#FEE2E2" 같은 색
        /// 문자열 또는 색 이름. 비어 있거나 해석 불가한 행은 기본 배경을
        /// 유지한다. 상태(Scrap 등)에 따라 행을 색으로 구분할 때 쓴다.
        /// </summary>
        public static readonly DependencyProperty RowColorMemberPathProperty =
            DependencyProperty.Register(
                "RowColorMemberPath",
                typeof(string),
                typeof(ModernDataGridControl),
                new PropertyMetadata(string.Empty, OnRowColorMemberPathChanged));

        /// <summary>첫 표시 열의 셀 모서리에 표시할 짧은 문자열을 계산한다.</summary>
        public static readonly DependencyProperty RowMarkerSelectorProperty = DependencyProperty.Register(
                "RowMarkerSelector", typeof(Func<object, string>), typeof(ModernDataGridControl),
                new PropertyMetadata(null, OnRowMarkerSelectorChanged));

        private readonly HashSet<object> pulsedMarkerItems = new HashSet<object>();
        private DataGridColumn markerColumn;
        internal event EventHandler RowMarkersChanged;

        /// <summary>행 배경색을 컬럼 추가 없이 계산하는 함수.</summary>
        public static readonly DependencyProperty RowColorSelectorProperty =
            DependencyProperty.Register("RowColorSelector", typeof(Func<object, string>), typeof(ModernDataGridControl),
                    new PropertyMetadata(null, OnRowColorMemberPathChanged));

        /// <summary>
        /// 행 글자색으로 쓸 컬럼/속성 이름 (선택 사항). 값은 "#DC2626" 같은 색
        /// 문자열 또는 색 이름. 비어 있거나 해석 불가한 행은 기본 글자색을 유지한다.
        /// 판정 FAIL 행처럼 한 행 전체의 글자를 색으로 구분할 때 쓴다 (2026-08-29).
        /// </summary>
        public static readonly DependencyProperty RowForegroundMemberPathProperty =
            DependencyProperty.Register(
                "RowForegroundMemberPath",
                typeof(string),
                typeof(ModernDataGridControl),
                new PropertyMetadata(string.Empty, OnRowForegroundMemberPathChanged));

        /// <summary>
        /// 장평(글자 가로 비율) 재정의 — 0(기본) = 전역(ModernTheme.FontWidthRatio)
        /// 사용, 양수는 0.8~1.2로 클램프. 셀/헤더/배지/버튼 캡션/상태바 텍스트에
        /// 가로 스케일이 적용되고 AutoFitColumns 측정에도 반영된다.
        /// </summary>
        public static readonly DependencyProperty FontWidthRatioProperty =
            DependencyProperty.Register(
                "FontWidthRatio",
                typeof(double),
                typeof(ModernDataGridControl),
                new PropertyMetadata(0d, OnFontWidthRatioChanged));

        /// <summary>행 선택이 바뀔 때 발생한다.</summary>
        public event EventHandler SelectionChanged;

        /// <summary>버튼 컬럼(GridColumnKind.Button) 셀을 클릭할 때 발생한다.</summary>
        public event EventHandler<GridButtonClickEventArgs> CellButtonClick;

        /// <summary>링크 컬럼(GridColumnKind.Link) 셀을 클릭할 때 발생한다.
        /// e.Item이 클릭된 행, e.DataPropertyName이 링크 컬럼 이름이다.</summary>
        public event EventHandler<GridButtonClickEventArgs> CellLinkClick;

        /// <summary>
        /// 체크박스 컬럼(GridColumnKind.CheckBox) 셀 값이 클릭으로 바뀔 때
        /// 발생한다. 헤더 체크박스로 일괄 변경하면 값이 실제로 바뀐 행마다
        /// 한 번씩 발생한다 — 토글을 곧바로 저장/서버 반영해야 하는 화면용이다.
        /// </summary>
        public event EventHandler<GridCheckChangedEventArgs> CellCheckChanged;

        /// <summary>
        /// 텍스트 셀 편집이 커밋되어 값이 실제로 바뀌었을 때 발생한다
        /// (같은 값 확정/Esc 취소에는 발생하지 않는다). NewValue는 원본 컬럼
        /// 타입으로 변환된 값이다.
        /// </summary>
        public event EventHandler<GridCellValueChangedEventArgs> CellValueChanged;

        /// <summary>
        /// 텍스트 셀 편집을 커밋하기 직전에 발생한다. Cancel을 true로 두면
        /// 커밋이 취소되고 셀이 편집 상태로 남는다 (업무 규칙 검증용 —
        /// 타입 오류는 이 이벤트 없이 그리드가 스스로 막는다).
        /// </summary>
        public event EventHandler<GridCellValidatingEventArgs> CellValidating;

        /// <summary>높이 변화로 표시 가능 행 수(VisibleRowCapacity)가 바뀔 때 발생한다.</summary>
        public event EventHandler VisibleRowCapacityChanged;

        /// <summary>
        /// 행 위에서 우클릭할 때 발생한다 — 그 행이 먼저 현재 행(SelectedItem)으로
        /// 선택된 뒤 발생하므로, 호스트는 SelectedItem을 대상으로 컨텍스트 메뉴를
        /// 띄우면 된다. 행 밖(헤더/빈 영역) 우클릭에는 발생하지 않는다.
        /// </summary>
        public event EventHandler RowRightClick;

        /// <summary>
        /// 행 위에서 왼쪽 더블클릭할 때 발생한다 (2026-08-29 추가) — 그 행이 먼저 현재 행(SelectedItem)으로
        /// 선택된 뒤 발생하므로, 호스트는 SelectedItem을 "결정"으로 넘기면 된다("더블클릭 → 결정 패널"
        /// 관례). 행 밖(헤더/빈 영역) 더블클릭에는 발생하지 않는다.
        /// </summary>
        public event EventHandler RowDoubleClick;

        /// <summary>
        /// 컬럼 값 필터 상태가 바뀔 때(체크/해제/Clear) 발생한다 — 페이지
        /// 슬라이스를 바인딩하는 화면이 전체 결과에 같은 필터를 적용해
        /// (MatchesColumnFilters) 페이지를 재계산할 때 쓴다.
        /// </summary>
        public event EventHandler ColumnFiltersChanged;

        /// <summary>
        /// 필터 팝업의 고유 값 원천 — 페이지 슬라이스를 바인딩하는 화면은
        /// 전체 결과(DataTable)를 지정해 체크리스트에 전체 값이 나오게 한다.
        /// null이면 현재 ItemsSource에서 모은다.
        /// </summary>
        public object FilterValueSource
        {
            get { return this.filterController.ValueSource; }
            set { this.filterController.ValueSource = value; }
        }

        /// <summary>행(DataRow/DataRowView/POCO)이 현재 컬럼 필터를 전부
        /// 통과하는지 판정한다 (필터 없으면 항상 true).</summary>
        public bool MatchesColumnFilters(object item)
        {
            return this.filterController.Matches(item);
        }

        /// <summary>값이 걸린 컬럼 필터(깔때기)가 하나라도 있는지.</summary>
        public bool HasActiveColumnFilters
        {
            get { return this.filterController.HasActiveFilters; }
        }

        /// <summary>모든 컬럼의 값 필터를 한 번에 해제한다.</summary>
        public void ClearColumnFilters()
        {
            this.filterController.ClearAllFilters();
        }

        /// <summary>모든 행을 선택한다 — 다중 선택(MultiSelect)일 때만 동작.</summary>
        public void SelectAllRows()
        {
            if (this.MultiSelect)
            {
                this.InnerDataGrid.SelectAll();
            }
        }

        // 마지막으로 통지한 표시 가능 행 수 — 같은 값이면 이벤트를 삼킨다.
        private int lastCapacity;

        // 표시 가능 행 수 통지 디바운스 타이머 — 창 드래그 중에는 행 높이를
        // 지날 때마다 행 수가 바뀌는데, 그때마다 즉시 통지하면 페이지 연동
        // 폼이 매번 페이지 조각을 다시 바인딩(전체 재구성 + AutoFit 재실측)
        // 해 리사이즈가 눈에 띄게 느려진다. 크기 변화가 멎은 뒤 1회만 알린다.
        private readonly System.Windows.Threading.DispatcherTimer capacityDebounce;
        private bool hostDisposed;

        // ApplyColumns로 받은 컬럼 정의 — AutoFitColumns 재계산의 원천.
        private IList<ModernDataGridColumn> columnDefinitions;

        // HeaderCheckBox 옵션이 켜진 체크박스 컬럼의 헤더 체크박스 목록 —
        // 행 값 변화(전체/일부/없음)에 맞춰 상태를 갱신하기 위해 들고 있는다.
        private readonly List<CheckBox> headerCheckBoxes = new List<CheckBox>();

        // 컬럼 값 필터(헤더 깔때기) 상태/팝업 담당 — AllowColumnFilters가 켜진
        // 동안 헤더에 버튼을 붙이고 뷰 필터를 적용한다.
        private readonly GridFilterController filterController;

        // 찾기(Ctrl+F) 바/이동/강조 담당 — 머리글 위에 끼어드는 인라인 바.
        private readonly GridFindController findController;

        // 2단 그룹 헤더 스트립 담당 — HeaderGroup 구간 셀 배치/폭/스크롤 동기화.
        private readonly GridGroupHeaderController groupHeaderController;

        // 집계 푸터 담당 — Aggregate 컬럼 값 계산 + 셀 배치/폭/스크롤 동기화.
        private readonly GridFooterController footerController;

        // 같은 값 세로 셀 병합 담당 — MergeCells 컬럼의 이어짐 셀 표시.
        private readonly GridCellMergeController mergeController;

        public ModernDataGridControl()
        {
            this.InitializeComponent();
            this.filterController = new GridFilterController(
                this.InnerDataGrid, this, delegate { return this.ItemsSource; });
            this.filterController.FiltersChanged += this.OnFilterControllerChanged;
            this.findController = new GridFindController(
                this.InnerDataGrid, this, this.FindBarHost, delegate { return this.columnDefinitions; });
            this.groupHeaderController = new GridGroupHeaderController(
                this.InnerDataGrid,
                this.GroupHeaderStrip,
                this.GroupHeaderCanvas,
                this,
                this.ResolvedFontWidthRatio);
            this.footerController = new GridFooterController(
                this.InnerDataGrid,
                this.FooterStrip,
                this.FooterCanvas,
                this,
                this.ResolvedFontWidthRatio);
            this.mergeController = new GridCellMergeController(
                this.InnerDataGrid,
                this.MergeCanvas,
                this,
                this.ResolvedFontWidthRatio,
                delegate { return this.columnDefinitions; });
            this.SizeChanged += this.OnControlSizeChanged;

            // 카드 모서리: 내부(헤더/상태바)를 카드 반경에 맞춰 클리핑한다
            // (XAML의 ContentHost 주석 참고).
            this.ContentHost.SizeChanged += this.OnContentHostSizeChanged;

            // Ctrl+F(찾기 열기)/Esc(닫기)/F3(다음 찾기)는 그리드 어디에 포커스가
            // 있어도 동작해야 하므로 컨트롤 루트에서 받는다.
            this.PreviewKeyDown += this.OnRootPreviewKeyDown;

            // 리사이즈가 멎은 뒤에만 행 수 변화를 통지한다 (위 필드 주석 참고).
            // 200ms — 드래그 프레임 간격보다 길고, 손을 뗀 뒤 지연은 체감되지
            // 않는 값이다.
            this.capacityDebounce = new System.Windows.Threading.DispatcherTimer();
            this.capacityDebounce.Interval = TimeSpan.FromMilliseconds(200);
            this.capacityDebounce.Tick += this.OnCapacityDebounceTick;

            // 행 수 자동 갱신: 소스 교체/필터 변경 등으로 Items가 바뀔 때마다
            // 상태바의 카운트 텍스트를 다시 쓴다.
            ((INotifyCollectionChanged)this.InnerDataGrid.Items).CollectionChanged += this.OnItemsCollectionChanged;

            // 초기(데이터 할당 전) 빈 상태 안내를 바로 표시한다.
            this.RefreshEmptyLabel();

            // 행 색상: 행이 화면에 실체화될 때마다 RowColorMember 값으로 배경을 칠한다.
            this.InnerDataGrid.LoadingRow += this.OnLoadingRow;
            this.InnerDataGrid.LayoutUpdated += this.OnMarkerLayoutUpdated;

            // 행 우클릭: 그 행을 먼저 선택하고 RowRightClick을 알린다 —
            // "우클릭 → 그 행 대상 컨텍스트 메뉴" 동선(DataGridView 관례).
            this.InnerDataGrid.PreviewMouseRightButtonDown += this.OnGridRightButtonDown;

            // 행 더블클릭: 그 행을 먼저 선택하고 RowDoubleClick을 알린다.
            this.InnerDataGrid.MouseDoubleClick += this.OnGridMouseDoubleClick;

            // 클립보드 복사: Ctrl+C = 행, Ctrl+Shift+C = 셀 하나.
            // WPF 기본 Copy 명령을 쓰지 않고 직접 처리하는 이유는
            // CopyRowToClipboard 주석에 있다.
            this.InnerDataGrid.PreviewKeyDown += this.OnGridPreviewKeyDown;

            // 정렬 상태가 바뀌면 자동 맞춤 폭을 다시 계산한다 — 정렬 글리프(▲/▼)
            // 폭은 정렬이 걸린 컬럼에만 예약하므로, 그 컬럼 폭이 정렬을 켜고 끌 때
            // 글리프 폭만큼 움직여야 캡션이 잘리지 않는다.
            this.InnerDataGrid.Sorting += this.OnGridSorting;

            // 셀 편집 (ReadOnly=false일 때만 실제로 동작):
            // 편집 시작 시 원래 값을 기억했다가 커밋 후 비교해 CellValueChanged를
            // 올리고, 커밋 직전에는 CellValidating으로 업무 규칙 검증 기회를 준다.
            this.InnerDataGrid.PreparingCellForEdit += this.OnPreparingCellForEdit;
            this.InnerDataGrid.CellEditEnding += this.OnCellEditEnding;

            // 현재 셀 표식 — 현재 셀이 바뀔 때마다 해당 셀 컨테이너에
            // GridCellAttach.IsCurrent를 세워 옅은 액센트 프레임을 켠다.
            this.InnerDataGrid.CurrentCellChanged += this.OnCurrentCellChanged;
        }

        // 호스트의 최종 폐기만 정리한다. 순수 WPF의 임시 Unloaded에는 적용하지 않는다.
        void IHostDisposalAware.OnHostDisposing()
        {
            if (this.hostDisposed)
            {
                return;
            }

            this.hostDisposed = true;
            this.InnerDataGrid.LayoutUpdated -= this.OnMarkerLayoutUpdated;
            this.RowMarkersChanged = null;
            this.pulsedMarkerItems.Clear();
            this.ObserveBindingList(null);
            this.capacityDebounce.Stop();
            this.capacityDebounce.Tick -= this.OnCapacityDebounceTick;
            this.SizeChanged -= this.OnControlSizeChanged;
            ((INotifyCollectionChanged)this.InnerDataGrid.Items).CollectionChanged -= this.OnItemsCollectionChanged;
            this.filterController.Detach();
            this.findController.Release();
            this.groupHeaderController.Detach();
            this.footerController.Detach();
            this.mergeController.Detach();
        }

        // ===== 현재 셀 표식 =====

        // 표식이 켜져 있는 셀 컨테이너 — 다음 갱신 때 끄기 위해 들고 있는다.
        private DataGridCell markedCurrentCell;

        // 표식 브러시 (AccentSoft 90%) — 처음 필요할 때 토큰에서 만든다.
        private Brush currentCellFrameBrush;

        private void OnCurrentCellChanged(object sender, EventArgs e)
        {
            this.RefreshCurrentCellMarker();

            // 병합 컬럼이면 스팬 셀의 현재 셀 언더라인을 옮겨 단다.
            this.mergeController.QueueRefresh();
        }

        // 현재 셀(CurrentCell)의 컨테이너를 찾아 옅은 액센트 **언더라인**
        // (아래쪽 2px — 현재 탭 언더라인과 같은 문법)을 옮겨 단다 — 셀의
        // BorderBrush/BorderThickness 로컬 값을 설정하면 셀 템플릿의
        // CurrentFrame이 그대로 그린다. 가상화로 컨테이너가 아직 없으면 표식
        // 없이 두고, 행이 실체화될 때(LoadingRow) 다시 시도한다. 재활용된
        // 컨테이너에 남은 낡은 표식도 이 갱신이 걷는다.
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

        // 언더라인 브러시 — AccentSoft(옅은 액센트) 토큰 색.
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

        // ===== 셀 편집 =====

        // 편집 시작 시점의 원래 값 — 커밋 후 실제 변경 여부 판정에 쓴다.
        private object pendingEditOldValue;

        private void OnPreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            ModernDataGridColumn definition = this.DefinitionOf(e.Column);

            this.pendingEditOldValue = definition == null
                    ? null
                    : MemberPathReader.Read(e.Row.Item, definition.DataPropertyName);

            // 병합 컬럼을 편집하면 그 컬럼의 스팬 셀을 걷어 편집기가 보이게 한다.
            if (definition != null && definition.MergeCells)
            {
                this.mergeController.OnEditingColumnChanged(
                        this.InnerDataGrid.Columns.IndexOf(e.Column));
            }

            // 기존 값 전체 선택 — 더블클릭/F2로 들어가면 바로 덮어쓸 수 있게
            // (DataGridView/엑셀 관례).
            TextBox editor = e.EditingElement as TextBox;

            if (editor != null)
            {
                editor.SelectAll();
            }
        }

        private void OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // 병합 컬럼 편집이 끝나면(커밋/취소 모두) 스팬 셀을 되돌린다.
            this.mergeController.OnEditingColumnChanged(-1);

            if (e.EditAction != DataGridEditAction.Commit)
            {
                return;
            }

            ModernDataGridColumn definition = this.DefinitionOf(e.Column);

            // 자동 생성 컬럼(정의 없음)은 기본 커밋만 하고 이벤트는 올리지 않는다.
            if (definition == null)
            {
                return;
            }

            // 커밋 직전 검증 — 취소하면 셀이 편집 상태로 남는다.
            if (this.CellValidating != null)
            {
                TextBox editor = e.EditingElement as TextBox;
                GridCellValidatingEventArgs validating = new GridCellValidatingEventArgs(
                        e.Row.Item, definition.DataPropertyName, editor == null ? string.Empty : editor.Text);

                this.CellValidating(this, validating);

                if (validating.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            object item = e.Row.Item;
            string path = definition.DataPropertyName;
            object oldValue = this.pendingEditOldValue;

            // 값 비교/이벤트는 커밋이 끝난 뒤에 — 이 핸들러 시점에는 아직 원본에
            // 쓰이기 전이다. 같은 자리에서 행 편집(IEditableObject)까지 확정해
            // DataTable에 즉시 반영한다(저장 버튼이 GetChanges로 읽는 패턴 유지).
            // 변환 실패로 커밋이 거부되면 값이 그대로라 이벤트도 나가지 않는다.
            this.Dispatcher.BeginInvoke(
                new Action(delegate
                {
                    this.InnerDataGrid.CommitEdit(DataGridEditingUnit.Row, true);

                    object newValue = MemberPathReader.Read(item, path);

                    if (!AreCellValuesEqual(oldValue, newValue))
                    {
                        // 집계 푸터 — 편집으로 값이 바뀌면 즉시 다시 계산한다.
                        this.footerController.Recalculate();

                        // 셀 병합 — 병합 컬럼 값이 바뀌면 구간이 갈라지거나
                        // 새로 이어질 수 있다.
                        this.mergeController.QueueRefresh();

                        if (this.CellValueChanged != null)
                        {
                            this.CellValueChanged(this,
                                    new GridCellValueChangedEventArgs(item, path, oldValue, newValue));
                        }
                    }
                }),
                System.Windows.Threading.DispatcherPriority.Loaded);
        }

        // 셀 값 동등 비교 — DataTable의 빈 값(DBNull)과 null을 같은 것으로 본다.
        private static bool AreCellValuesEqual(object left, object right)
        {
            object normalizedLeft = left == DBNull.Value ? null : left;
            object normalizedRight = right == DBNull.Value ? null : right;

            return object.Equals(normalizedLeft, normalizedRight);
        }

        // 그리드 컬럼 → ApplyColumns로 받은 정의 (자동 생성 컬럼이면 null).
        private ModernDataGridColumn DefinitionOf(DataGridColumn column)
        {
            if (this.columnDefinitions == null || column == null)
            {
                return null;
            }

            int index = this.InnerDataGrid.Columns.IndexOf(column);

            if (index < 0 || index >= this.columnDefinitions.Count)
            {
                return null;
            }

            return this.columnDefinitions[index];
        }

        /// <summary>
        /// 열려 있는 셀/행 편집을 확정한다 — 저장 직전에 호출하면 편집 중이던
        /// 값이 원본(DataTable 등)에 반영된다 (DataGridView.EndEdit 대응).
        /// </summary>
        public void CommitPendingEdit()
        {
            this.InnerDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
        }

        /// <summary>열려 있는 셀/행 편집을 버린다 (Esc와 동일).</summary>
        public void CancelPendingEdit()
        {
            this.InnerDataGrid.CancelEdit(DataGridEditingUnit.Row);
        }

        /// <summary>
        /// 새 행을 소스에 추가하고 그 행 객체를 돌려준다. DataTable/DataView
        /// 소스는 DataRowView, IBindingList 소스는 AddNew 결과다. 그 외 소스는
        /// 지원하지 않는다(InvalidOperationException).
        /// </summary>
        public object AddRow()
        {
            // 열려 있던 편집 먼저 확정 — 편집 중 행 추가로 뷰가 어긋나지 않게.
            this.CommitPendingEdit();

            System.Data.DataView view = this.ItemsSource as System.Data.DataView;

            if (view != null)
            {
                System.Data.DataRowView row = view.AddNew();
                row.EndEdit();
                return row;
            }

            System.ComponentModel.IBindingList bindingList =
                    this.ItemsSource as System.ComponentModel.IBindingList;

            if (bindingList != null && bindingList.AllowNew)
            {
                return bindingList.AddNew();
            }

            throw new InvalidOperationException(
                    "AddRow는 DataTable/DataView 또는 IBindingList 소스에서만 지원한다.");
        }

        /// <summary>
        /// 행 하나를 소스에서 제거한다. DataTable/DataView 소스는 Delete로
        /// 표시되어 폼의 GetChanges/저장 로직이 삭제를 그대로 본다.
        /// </summary>
        public void RemoveRow(object item)
        {
            if (item == null)
            {
                return;
            }

            // 삭제할 행에 열려 있을 수 있는 편집은 버린다.
            this.CancelPendingEdit();

            System.Data.DataRowView rowView = item as System.Data.DataRowView;

            if (rowView != null)
            {
                rowView.Delete();
                return;
            }

            System.ComponentModel.IBindingList bindingList =
                    this.ItemsSource as System.ComponentModel.IBindingList;

            if (bindingList != null && bindingList.AllowRemove)
            {
                bindingList.Remove(item);
                return;
            }

            IList list = this.ItemsSource as IList;

            if (list != null && !list.IsFixedSize && !list.IsReadOnly)
            {
                list.Remove(item);
                return;
            }

            throw new InvalidOperationException(
                    "RemoveRow는 DataTable/DataView, IBindingList 또는 고정 크기가 아닌 IList 소스에서만 지원한다.");
        }

        // 헤더 클릭 정렬 — DataGrid가 정렬을 적용하고 SortDirection을 세운 **뒤에**
        // 폭을 다시 계산해야 하므로 한 틱 미뤄서 실행한다. 큐에 넣은 작업은 현재
        // 호출 스택(정렬 적용)이 끝난 뒤에 실행되므로 순서는 보장된다.
        // 우선순위는 Loaded — 렌더보다 먼저 처리되어 "잘린 프레임"이 한 번
        // 그려지는 일을 피한다 (Background는 렌더 뒤라 깜빡일 수 있다).
        private void OnGridSorting(object sender, DataGridSortingEventArgs e)
        {
            this.Dispatcher.BeginInvoke(
                new Action(this.AutoFitColumnWidths),
                System.Windows.Threading.DispatcherPriority.Loaded);

            // 셀 병합 — 정렬로 뷰 순서가 바뀌면 병합 구간을 다시 계산한다
            // (컨테이너가 재활용돼 LoadingRow가 오지 않는 행도 있다).
            this.mergeController.QueueRefresh();
        }

        // ===== 클립보드 복사 =====

        /// <summary>
        /// 선택 행을 탭 구분 텍스트로 클립보드에 복사한다 (Excel 붙여넣기 호환).
        /// 다중 선택(MultiSelect)이면 선택된 행 전부가 **뷰 순서대로** 여러 줄이
        /// 되고, 아니면 현재 행 한 줄이다.
        ///
        /// WPF 기본 Copy 명령을 그대로 쓰지 않는 이유: 기본 동작은 화면의 모든
        /// 컬럼을 대상으로 하므로 체크박스·버튼·스피너처럼 <b>데이터가 아닌 컬럼</b>이
        /// 빈 칸으로 끼어들어 붙여넣은 표의 열이 밀린다. 여기서는 컬럼 정의
        /// (ApplyColumns로 받은 것)를 보고 데이터 컬럼만 골라 같은 서식으로 만든다.
        /// </summary>
        public void CopyRowToClipboard()
        {
            if (this.columnDefinitions == null)
            {
                return;
            }

            object[] rows = this.SelectedItemsInViewOrder();

            if (rows.Length == 0)
            {
                object current = this.InnerDataGrid.CurrentItem ?? this.SelectedItem;

                if (current == null)
                {
                    return;
                }

                rows = new object[] { current };
            }

            StringBuilder text = new StringBuilder();

            for (int index = 0; index < rows.Length; index++)
            {
                if (index > 0)
                {
                    text.AppendLine();
                }

                bool first = true;

                foreach (ModernDataGridColumn definition in this.columnDefinitions)
                {
                    if (!IsCopyableColumn(definition))
                    {
                        continue;
                    }

                    if (!first)
                    {
                        text.Append('\t');
                    }

                    text.Append(CellText(rows[index], definition));
                    first = false;
                }
            }

            SetClipboardText(text.ToString());
        }

        /// <summary>
        /// 현재 셀 하나의 표시 텍스트만 클립보드에 복사한다 (탭·개행 없음).
        /// 데이터가 아닌 컬럼(체크박스·버튼·스피너)에서는 아무것도 하지 않는다.
        /// </summary>
        public void CopyCellToClipboard()
        {
            ModernDataGridColumn definition = this.CurrentColumnDefinition();
            object row = this.InnerDataGrid.CurrentItem ?? this.SelectedItem;

            if (definition == null || row == null || !IsCopyableColumn(definition))
            {
                return;
            }

            SetClipboardText(CellText(row, definition));
        }

        // 현재 셀이 속한 컬럼의 정의. 찾지 못하면 null.
        private ModernDataGridColumn CurrentColumnDefinition()
        {
            return this.DefinitionOf(this.InnerDataGrid.CurrentCell.Column);
        }

        // 데이터를 담은 컬럼인지 — 체크박스/버튼/스피너는 상태 표시라 복사 대상이 아니다.
        private static bool IsCopyableColumn(ModernDataGridColumn definition)
        {
            if (definition == null || string.IsNullOrEmpty(definition.DataPropertyName))
            {
                return false;
            }

            return definition.Kind != GridColumnKind.CheckBox
                    && definition.Kind != GridColumnKind.Button
                    && definition.Kind != GridColumnKind.Spinner;
        }

        // 셀 표시 텍스트 — 화면과 같은 서식(Format)을 적용한다.
        private static string CellText(object row, ModernDataGridColumn definition)
        {
            return GridAutoFitMeasurer.FormatCellText(
                    MemberPathReader.Read(row, definition.DataPropertyName), definition.Format);
        }

        // 클립보드는 다른 프로세스가 잡고 있으면 예외를 던진다 — 복사 실패로 화면이
        // 죽지 않게 삼킨다.
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

        // 우클릭 지점에서 시각/논리 트리를 거슬러 행(DataGridRow)을 찾은 뒤
        // 탐색기/엑셀 표준 규칙으로 선택을 처리하고 RowRightClick을 발생시킨다:
        // - 선택 안 된 행 → 그 행을 선택(기존 선택 대체)
        // - 이미 선택된 행(다중 선택 포함) → 선택 보존 — 메뉴가 "선택 전체"를
        //   대상으로 할 수 있어야 한다 (Copy row가 선택 행 전부를 복사)
        // 행 밖 우클릭은 무시한다. 좌클릭과 동일하게 **클릭한 셀을 현재 셀로도
        // 갱신**한다(선택은 안 건드림) — 언더라인이 그 셀로 옮겨가고,
        // Copy cell/컬럼 고정(우클릭 메뉴)의 기준이 된다.
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

        // 더블클릭 지점에서 행(DataGridRow)을 찾아 선택한 뒤 RowDoubleClick을 발생시킨다.
        // 왼쪽 버튼만, 행 밖(헤더·빈 영역)은 무시한다.
        private void OnGridMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton != System.Windows.Input.MouseButton.Left)
            {
                return;
            }

            DependencyObject source = e.OriginalSource as DependencyObject;

            while (source != null && !(source is DataGridRow))
            {
                if (source is DataGridColumnHeader)
                {
                    return;
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

            e.Handled = true;
            this.RaiseRowDoubleClick();
        }

        // 계약 검사가 리플렉션으로 부른다 — 실제 더블클릭 없이 이벤트 경로만 확인하기 위해.
        private void RaiseRowDoubleClick()
        {
            if (this.RowDoubleClick != null)
            {
                this.RowDoubleClick(this, EventArgs.Empty);
            }
        }

        /// <summary>현재 셀 컬럼의 표시 인덱스 (현재 셀이 없으면 -1) —
        /// 우클릭 메뉴의 "이 컬럼까지 고정"이 기준으로 쓴다.</summary>
        public int CurrentCellColumnIndex()
        {
            DataGridCellInfo current = this.InnerDataGrid.CurrentCell;

            if (!current.IsValid || current.Column == null)
            {
                return -1;
            }

            return current.Column.DisplayIndex;
        }

        /// <summary>표시할 행 목록.</summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)this.GetValue(ItemsSourceProperty); }
            set { this.SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>데이터 0건일 때의 안내 문구 (기본 "No data", 빈 문자열 = 끔).</summary>
        public string EmptyText
        {
            get { return (string)this.GetValue(EmptyTextProperty); }
            set { this.SetValue(EmptyTextProperty, value); }
        }

        /// <summary>현재 선택된 행 항목. 프로그램으로 설정하면 그 행이 보이도록
        /// 스크롤한다 (DataGridView의 CurrentCell 설정과 같은 체감).</summary>
        public object SelectedItem
        {
            get { return this.GetValue(SelectedItemProperty); }
            set
            {
                this.SetValue(SelectedItemProperty, value);
                this.ScrollSelectionIntoView();
            }
        }

        /// <summary>데이터 소스로부터 컬럼을 자동 생성할지 여부.</summary>
        public bool AutoGenerateColumns
        {
            get { return (bool)this.GetValue(AutoGenerateColumnsProperty); }
            set { this.SetValue(AutoGenerateColumnsProperty, value); }
        }

        /// <summary>읽기 전용 여부 (기본 true). false면 텍스트 셀 편집이 켜진다.</summary>
        public bool ReadOnly
        {
            get { return (bool)this.GetValue(ReadOnlyProperty); }
            set { this.SetValue(ReadOnlyProperty, value); }
        }

        /// <summary>다중 행 선택 허용 여부 (기본 false = 단일 선택).</summary>
        public bool MultiSelect
        {
            get { return (bool)this.GetValue(MultiSelectProperty); }
            set { this.SetValue(MultiSelectProperty, value); }
        }

        /// <summary>가로 스크롤 시 왼쪽에 고정할 컬럼 수 (기본 0 = 없음).</summary>
        public int FrozenColumnCount
        {
            get { return (int)this.GetValue(FrozenColumnCountProperty); }
            set { this.SetValue(FrozenColumnCountProperty, value); }
        }

        private static void OnFrozenColumnCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernDataGridControl)d).ApplyFrozenColumnCount();
        }

        // 고정 컬럼 수를 내부 DataGrid로 전달한다 — WPF DataGrid는 컬럼 수보다
        // 큰 값을 받지 못하므로 현재 컬럼 수로 클램프한다. 원하는 값은 DP에
        // 그대로 남아 있어, 컬럼이 나중에 구성되면(순서 내성) 다시 적용된다.
        private void ApplyFrozenColumnCount()
        {
            int desired = this.FrozenColumnCount;

            if (desired < 0)
            {
                desired = 0;
            }

            int max = this.InnerDataGrid.Columns.Count;

            this.InnerDataGrid.FrozenColumnCount = desired > max ? max : desired;
        }

        // 다중 선택 켬/끔 — WPF DataGrid의 선택 모드로 그대로 전달한다
        // (Single로 되돌리면 DataGrid가 스스로 선택을 하나로 줄인다).
        private static void OnMultiSelectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernDataGridControl control = (ModernDataGridControl)d;

            control.InnerDataGrid.SelectionMode = (bool)e.NewValue
                    ? DataGridSelectionMode.Extended
                    : DataGridSelectionMode.Single;
        }

        /// <summary>
        /// 선택된 행들을 **현재 뷰 순서**(정렬·컬럼 필터 반영)의 배열로 돌려준다.
        /// WPF의 SelectedItems는 클릭 순서라 화면 순서와 다를 수 있어, 벌크
        /// 처리/복사가 화면과 같은 순서가 되도록 여기서 정렬해 준다.
        /// 선택이 없으면 빈 배열.
        /// </summary>
        public object[] SelectedItemsInViewOrder()
        {
            int count = this.InnerDataGrid.SelectedItems.Count;

            if (count == 0)
            {
                return new object[0];
            }

            HashSet<object> selected = new HashSet<object>();

            foreach (object item in this.InnerDataGrid.SelectedItems)
            {
                selected.Add(item);
            }

            List<object> ordered = new List<object>(count);

            foreach (object item in this.InnerDataGrid.Items)
            {
                if (selected.Contains(item))
                {
                    ordered.Add(item);
                }
            }

            return ordered.ToArray();
        }

        /// <summary>컬럼 너비를 헤더와 데이터 내용에 맞춰 자동 계산할지 여부.</summary>
        public bool AutoFitColumns
        {
            get { return (bool)this.GetValue(AutoFitColumnsProperty); }
            set { this.SetValue(AutoFitColumnsProperty, value); }
        }

        /// <summary>컬럼 가상화 여부(기본 false). 조건과 트레이드오프는 <see cref="EnableColumnVirtualizationProperty"/> 설명.</summary>
        public bool EnableColumnVirtualization
        {
            get { return (bool)this.GetValue(EnableColumnVirtualizationProperty); }
            set { this.SetValue(EnableColumnVirtualizationProperty, value); }
        }

        /// <summary>컬럼 값 필터(헤더 깔때기) 사용 여부.</summary>
        public bool AllowColumnFilters
        {
            get { return (bool)this.GetValue(AllowColumnFiltersProperty); }
            set { this.SetValue(AllowColumnFiltersProperty, value); }
        }

        /// <summary>찾기(Ctrl+F, 엑셀식 소유된 모덜리스 찾기 창) 사용 여부 (기본 true).</summary>
        public bool AllowFindPanel
        {
            get { return (bool)this.GetValue(AllowFindPanelProperty); }
            set { this.SetValue(AllowFindPanelProperty, value); }
        }

        // ===== 찾기(Ctrl+F) =====

        /// <summary>찾기 창을 열고(열려 있으면 활성화) 검색어 입력에 포커스를 준다 (Ctrl+F와 동일).</summary>
        public void ShowFindPanel()
        {
            if (!this.AllowFindPanel)
            {
                return;
            }

            this.findController.Show();
        }

        /// <summary>찾기 창을 닫고 일치 강조를 걷는다 (Esc/✕와 동일).</summary>
        public void HideFindPanel()
        {
            this.findController.Hide();
        }

        /// <summary>찾기 창이 열려 있는지 (읽기 전용 — 자체 검사·상태 질의용).</summary>
        public bool IsFindPanelOpen
        {
            get { return this.findController.IsOpen; }
        }

        // 끄면 열려 있던 팝업·강조도 함께 정리한다.
        private static void OnAllowFindPanelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernDataGridControl control = (ModernDataGridControl)d;

            if (!(bool)e.NewValue)
            {
                control.HideFindPanel();
            }
        }

        // Ctrl+F/Esc/F3 — 그리드/팝업 어디서든 (Preview 터널링이라 루트가 먼저 받는다).
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
                // Shift+F3 = 이전 일치 (표준 찾기 단축키).
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

        /// <summary>그리드 하단 상태바 표시 여부.</summary>
        public bool ShowStatusBar
        {
            get { return (bool)this.GetValue(ShowStatusBarProperty); }
            set { this.SetValue(ShowStatusBarProperty, value); }
        }

        /// <summary>상태바 오른쪽에 표시할 자유 텍스트.</summary>
        public string StatusText
        {
            get { return (string)this.GetValue(StatusTextProperty); }
            set { this.SetValue(StatusTextProperty, value); }
        }

        /// <summary>상태바 왼쪽 행 수 표기 형식 ({0} = 행 수).</summary>
        public string StatusCountFormat
        {
            get { return (string)this.GetValue(StatusCountFormatProperty); }
            set { this.SetValue(StatusCountFormatProperty, value); }
        }

        /// <summary>교차 행 배경(줄무늬) 표시 여부. 기본 꺼짐.</summary>
        public bool AlternatingRowColors
        {
            get { return (bool)this.GetValue(AlternatingRowColorsProperty); }
            set { this.SetValue(AlternatingRowColorsProperty, value); }
        }

        /// <summary>행·열 크기에 영향을 주지 않는 모서리 표식. 빈 값·예외는 표시하지 않는다.</summary>
        public Func<object, string> RowMarkerSelector
        {
            get { return (Func<object, string>)this.GetValue(RowMarkerSelectorProperty); }
            set { this.SetValue(RowMarkerSelectorProperty, value); }
        }

        internal string MarkerText(object item, DataGridColumn column)
        {
            if (this.RowMarkerSelector == null || item == null || column == null || column != this.FirstMarkerColumn())
            {
                return string.Empty;
            }
            try { return this.RowMarkerSelector(item) ?? string.Empty; }
            catch (Exception) { return string.Empty; }
        }

        internal bool BeginMarkerPulse(object item)
        {
            return this.pulsedMarkerItems.Add(item);
        }

        private DataGridColumn FirstMarkerColumn()
        {
            DataGridColumn first = null;
            foreach (DataGridColumn column in this.InnerDataGrid.Columns)
            {
                if (column.Visibility == Visibility.Visible && (first == null || column.DisplayIndex < first.DisplayIndex))
                {
                    first = column;
                }
            }
            return first;
        }

        private void OnMarkerLayoutUpdated(object sender, EventArgs e)
        {
            if (this.RowMarkerSelector == null) { return; }
            DataGridColumn first = this.FirstMarkerColumn();
            if (first == this.markerColumn) { return; }
            this.markerColumn = first;
            this.RaiseRowMarkersChanged();
        }

        private static void OnRowMarkerSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernDataGridControl control = (ModernDataGridControl)d;
            control.pulsedMarkerItems.Clear();
            control.markerColumn = control.FirstMarkerColumn();
            control.RaiseRowMarkersChanged();
        }

        private void RaiseRowMarkersChanged()
        {
            if (this.RowMarkersChanged != null) { this.RowMarkersChanged(this, EventArgs.Empty); }
        }

        /// <summary>행에서 배경색을 계산한다. 빈 값·잘못된 색·예외는 컬럼 색으로 폴백한다.</summary>
        public Func<object, string> RowColorSelector
        {
            get { return (Func<object, string>)this.GetValue(RowColorSelectorProperty); }
            set { this.SetValue(RowColorSelectorProperty, value); }
        }

        /// <summary>현재 정렬·필터 뷰에서 조건을 만족하는 첫 행을 선택하고 보이게 한다. 없으면 선택을 유지한다.</summary>
        public bool SelectRow(Func<object, bool> predicate)
        {
            if (predicate == null) { return false; }
            foreach (object item in this.InnerDataGrid.Items)
            {
                if (item == System.Windows.Data.CollectionView.NewItemPlaceholder || !predicate(item)) { continue; }
                this.InnerDataGrid.SelectedItem = item;
                this.InnerDataGrid.ScrollIntoView(item);
                return true;
            }
            return false;
        }

        /// <summary>행 배경색으로 쓸 컬럼/속성 이름 (선택 사항).</summary>
        public string RowColorMemberPath
        {
            get { return (string)this.GetValue(RowColorMemberPathProperty); }
            set { this.SetValue(RowColorMemberPathProperty, value); }
        }

        /// <summary>행 글자색으로 쓸 컬럼/속성 이름 (선택 사항).</summary>
        public string RowForegroundMemberPath
        {
            get { return (string)this.GetValue(RowForegroundMemberPathProperty); }
            set { this.SetValue(RowForegroundMemberPathProperty, value); }
        }

        /// <summary>장평(글자 가로 비율) 재정의. 0 = 전역(ModernTheme.FontWidthRatio) 사용.</summary>
        public double FontWidthRatio
        {
            get { return (double)this.GetValue(FontWidthRatioProperty); }
            set { this.SetValue(FontWidthRatioProperty, value); }
        }

        /// <summary>현재 표시 중인 행 수.</summary>
        public int RowCount
        {
            get { return this.InnerDataGrid.Items.Count; }
        }

        /// <summary>
        /// 현재 높이에서 세로 스크롤 없이 표시 가능한 행 수 (최소 1).
        /// 행/헤더 높이가 토큰으로 고정되어 있어 결정적으로 계산된다.
        /// 페이지 크기를 화면 높이에 맞추는 용도(ModernPagination.PageSize 연동).
        /// </summary>
        public int VisibleRowCapacity
        {
            get
            {
                double rowHeight = (double)this.FindResource("Size.RowHeight");

                // 다중 줄 헤더로 높이가 조정됐을 수 있으므로 실제 적용 값을 쓴다.
                double headerHeight = this.InnerDataGrid.ColumnHeaderHeight;

                if (double.IsNaN(headerHeight) || headerHeight <= 0d)
                {
                    headerHeight = (double)this.FindResource("Size.GridHeaderHeight");
                }

                // 바깥 카드 테두리 위아래 1px씩 제외
                double available = this.ActualHeight - headerHeight - 2.0;

                // 상태바가 보이면 그 높이만큼 행 표시 영역이 줄어든다.
                if (this.StatusBarStrip.Visibility == Visibility.Visible)
                {
                    available = available - this.StatusBarStrip.ActualHeight;
                }

                // 2단 그룹 헤더/집계 푸터 스트립도 행 표시 영역을 줄인다.
                if (this.GroupHeaderStrip.Visibility == Visibility.Visible)
                {
                    available = available - this.GroupHeaderStrip.ActualHeight;
                }

                if (this.FooterStrip.Visibility == Visibility.Visible)
                {
                    available = available - this.FooterStrip.ActualHeight;
                }

                if (available < rowHeight)
                {
                    return 1;
                }

                return (int)(available / rowHeight);
            }
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

        private void OnControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.VisibleRowCapacity == this.lastCapacity)
            {
                return;
            }

            // 드래그 중 연속 변화는 타이머 재시작으로 흡수하고, 변화가 멎은
            // 200ms 뒤 틱에서 한 번만 통지한다.
            this.capacityDebounce.Stop();
            this.capacityDebounce.Start();
        }

        // 디바운스 틱 — 최종 행 수가 마지막 통지와 다를 때만 알린다.
        private void OnCapacityDebounceTick(object sender, EventArgs e)
        {
            this.capacityDebounce.Stop();

            int capacity = this.VisibleRowCapacity;

            if (capacity == this.lastCapacity)
            {
                return;
            }

            this.lastCapacity = capacity;

            if (this.VisibleRowCapacityChanged != null)
            {
                this.VisibleRowCapacityChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>선택된 행의 인덱스(아무것도 선택되지 않았으면 -1).
        /// 프로그램으로 설정하면 그 행이 보이도록 스크롤한다.</summary>
        public int SelectedIndex
        {
            get { return this.InnerDataGrid.SelectedIndex; }
            set
            {
                this.InnerDataGrid.SelectedIndex = value;
                this.ScrollSelectionIntoView();
            }
        }

        // 프로그램 선택 후 선택 행을 화면 안으로 스크롤한다 — WPF DataGrid는
        // 마우스 클릭과 달리 프로그램 선택에는 자동 스크롤하지 않으므로,
        // DataGridView(CurrentCell 설정 시 스크롤)와 체감을 맞추려면 명시
        // 호출이 필요하다.
        private void ScrollSelectionIntoView()
        {
            object item = this.InnerDataGrid.SelectedItem;

            if (item != null)
            {
                this.InnerDataGrid.ScrollIntoView(item);
            }
        }

        // ── 재바인딩 시 선택·스크롤 복원 (docs/state-contracts/grid-selection-restore.md)

        // 행을 알아보는 키를 만든다. 여러 컬럼이면 데이터에 들어갈 수 없는 구분자로
        // 잇는다 — 콤마로 이으면 값 안의 콤마 때문에 서로 다른 행이 같은 키가 된다.
        private const char keyJoiner = '\u001F';

        /// <summary>
        /// 행에서 키 문자열을 만든다. <paramref name="memberPaths"/>가 비었거나
        /// 행이 null이면 빈 문자열(= 키 없음)이다.
        /// </summary>
        internal static string BuildRowKey(object row, string[] memberPaths)
        {
            if (row == null || memberPaths == null || memberPaths.Length == 0)
            {
                return string.Empty;
            }

            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            for (int index = 0; index < memberPaths.Length; index++)
            {
                if (index > 0)
                {
                    builder.Append(keyJoiner);
                }

                builder.Append(
                        Modern.Lab.Controls.Wpf.Common.MemberPathReader.ReadDisplayText(
                                row, memberPaths[index]));
            }

            return builder.ToString();
        }

        /// <summary>키가 같은 행의 인덱스. 없으면 -1.</summary>
        internal int IndexOfRowKey(string key, string[] memberPaths)
        {
            if (string.IsNullOrEmpty(key) || memberPaths == null || memberPaths.Length == 0)
            {
                return -1;
            }

            System.Collections.IList items = this.InnerDataGrid.Items;

            for (int index = 0; index < items.Count; index++)
            {
                if (BuildRowKey(items[index], memberPaths) == key)
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>지금 선택된 행의 키. 선택이 없으면 빈 문자열.</summary>
        internal string SelectedRowKey(string[] memberPaths)
        {
            object item = this.InnerDataGrid.SelectedItem;
            System.Data.DataRowView rowView = item as System.Data.DataRowView;

            // 표에서 이미 지워진(Rows.Remove) 행이 선택으로 남아 있을 수 있다 — 그 행의 셀을 읽으면
            // RowNotInTableException이 나므로 키 없음으로 본다(2026-08-29, 조용한 갱신 뒤 재바인딩에서 발견).
            if (rowView != null
                    && (rowView.Row.RowState == System.Data.DataRowState.Detached
                            || rowView.Row.RowState == System.Data.DataRowState.Deleted))
            {
                return string.Empty;
            }

            return BuildRowKey(item, memberPaths);
        }

        // 세대 번호 — 재바인딩마다 올린다. 미뤄 둔 스크롤 복원이 자기 세대가
        // 아니면 아무것도 하지 않게 해, 다음 조회가 끝난 화면을 낡은 위치로
        // 덮어쓰는 것을 막는다.
        private int scrollRestoreGeneration;

        /// <summary>현재 세로 스크롤 위치. 스크롤뷰어를 못 찾으면 0.</summary>
        internal double GetVerticalOffset()
        {
            ScrollViewer viewer = this.FindScrollViewer();
            return viewer == null ? 0d : viewer.VerticalOffset;
        }

        /// <summary>
        /// 세로 스크롤 위치를 되돌린다. 행 가상화 때문에 레이아웃이 끝난 뒤에야
        /// 가능하므로 미뤄서 실행하며, **선택은 건드리지 않는다** — 건드리면
        /// SelectionChanged가 한 번 더 나가 "할당당 1회" 계약이 깨진다.
        ///
        /// 위치를 되돌린 뒤 **선택 행이 화면 밖이면 선택 쪽으로 다시 맞춘다.**
        /// 재조회로 그 행이 멀리 이동했을 때(정렬 기준이 처리로 바뀌는 목록 등)
        /// 보던 위치를 지키면 선택이 안 보이게 되는데, 그때는 "무엇이 선택돼
        /// 있는지"가 "어디를 보고 있었는지"보다 중요하다.
        /// </summary>
        internal void RestoreVerticalOffset(double offset)
        {
            this.scrollRestoreGeneration++;

            if (offset <= 0d)
            {
                return;
            }

            int generation = this.scrollRestoreGeneration;

            this.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Loaded,
                    new Action(delegate
                    {
                        if (generation != this.scrollRestoreGeneration)
                        {
                            return;
                        }

                        ScrollViewer viewer = this.FindScrollViewer();

                        if (viewer == null)
                        {
                            return;
                        }

                        // 범위를 넘으면 WPF가 끝으로 잘라 준다.
                        viewer.ScrollToVerticalOffset(offset);

                        // 되돌린 위치를 확정한 뒤에 선택 가시성을 본다 —
                        // UpdateLayout 없이 부르면 ScrollIntoView가 이전 위치를
                        // 기준으로 판단해 "이미 보인다"고 착각한다.
                        viewer.UpdateLayout();
                        this.ScrollSelectionIntoView();
                    }));
        }

        // DataGrid 템플릿 안의 스크롤뷰어 — GridCellMergeController 등이 쓰는
        // 방식과 같다. 템플릿이 아직 적용되지 않았으면 null이고, 그때는 복원을
        // 건너뛴다(첫 바인딩이라 되돌릴 위치도 없다).
        private ScrollViewer FindScrollViewer()
        {
            if (this.InnerDataGrid.Template == null)
            {
                return null;
            }

            return this.InnerDataGrid.Template.FindName("DG_ScrollViewer", this.InnerDataGrid)
                    as ScrollViewer;
        }

        /// <summary>
        /// 컬럼을 명시적 정의로 교체하고 자동 생성을 끈다.
        /// null이나 빈 목록을 넘기면 컬럼만 비운다.
        /// </summary>
        public void ApplyColumns(IList<ModernDataGridColumn> columns)
        {
            this.AutoGenerateColumns = false;
            this.InnerDataGrid.Columns.Clear();
            this.columnDefinitions = null;
            this.headerCheckBoxes.Clear();
            this.filterController.OnColumnsRebuilt();
            this.findController.OnColumnsRebuilt();

            if (columns == null)
            {
                this.ApplyHeaderHeight();
                this.groupHeaderController.OnColumnsRebuilt(null);
                this.footerController.OnColumnsRebuilt(null);
                return;
            }

            this.columnDefinitions = new List<ModernDataGridColumn>(columns);

            // 장평은 컬럼 구성 시점에 확정된다 — 전역/재정의 값이 바뀌면
            // OnFontWidthRatioChanged가 컬럼을 다시 만든다.
            double widthRatio = this.ResolvedFontWidthRatio();

            foreach (ModernDataGridColumn definition in columns)
            {
                // 자동색 값은 컬럼 구성 시점에 일괄 선등록한다 — 충돌 값의 벌려

                // 컬럼 생성은 GridColumnFactory가 담당한다 — 리소스 조회 기준(this),
                // 클릭 핸들러, 헤더 체크박스 등록 콜백만 넘기고 결과를 받는다.
                // 배지/행별 캡션 버튼은 가장 긴 표시값 기준의 통일 최소 폭을 계산해 넘긴다.
                double cellMinWidth = 0d;

                if (definition.Kind == GridColumnKind.Badge && !definition.BadgeWidthByValue)
                {
                    cellMinWidth = GridColumnFactory.CalculateBadgeMinWidth(
                            definition, this.ItemsSource, this, widthRatio);
                }
                else if (definition.Kind == GridColumnKind.Button)
                {
                    cellMinWidth = GridColumnFactory.CalculateButtonMinWidth(
                            definition, this.ItemsSource, this, widthRatio);
                }
                DataGridColumn column = GridColumnFactory.CreateColumn(
                    definition,
                    widthRatio,
                    this,
                    this.OnHeaderCheckBoxClick,
                    this.OnCellCheckBoxClick,
                    this.OnCellButtonClick,
                    this.headerCheckBoxes.Add,
                    cellMinWidth,
                    this.OnCellLinkClick);

                // 헤더 캡션도 같은 장평으로 — 문자열 대신 스케일된 TextBlock을 쓴다.
                // 위·아래 작은 글씨가 있는 패널은 패널째 같은 변환을 건다.
                // (헤더가 문자열이 아닌 컬럼 — 헤더 체크박스 — 은 건드리지 않는다.)
                if (Math.Abs(widthRatio - 1d) >= 0.001 && column.Header is string)
                {
                    TextBlock headerText = new TextBlock();
                    headerText.Text = definition.HeaderText;
                    headerText.LayoutTransform = GridColumnFactory.CreateWidthTransform(widthRatio);
                    column.Header = headerText;
                }
                else if (Math.Abs(widthRatio - 1d) >= 0.001 && GridHeaderCaption.IsCaptionHeader(column.Header))
                {
                    ((FrameworkElement)column.Header).LayoutTransform = GridColumnFactory.CreateWidthTransform(widthRatio);
                }

                if (definition.HeaderAlignment != GridTextAlignment.Center || GridHeaderCaption.IsCaptionHeader(column.Header)
                        || definition.TopLabelAlignment.HasValue || definition.BottomLabelAlignment.HasValue)
                {
                    this.ApplyHeaderAlignment(column, definition);
                }

                // 컬럼 값 필터: 헤더 오른쪽에 깔때기 버튼을 붙인다 (텍스트/배지
                // 컬럼만 — 체크박스/버튼 컬럼은 컨트롤러가 알아서 건너뛴다).
                if (this.AllowColumnFilters)
                {
                    this.filterController.AttachHeader(column, definition);
                }

                this.InnerDataGrid.Columns.Add(column);
            }

            // 다중 줄 헤더("\n")가 있으면 헤더 높이를 줄 수에 맞춰 늘린다.
            this.ApplyHeaderHeight();

            // 컬럼 정의의 Frozen 플래그 → 고정 컬럼 수 (마지막 Frozen 컬럼까지 —
            // 고정은 왼쪽부터 연속이라는 DataGridView 규칙 그대로).
            int frozenCount = 0;

            for (int index = 0; index < columns.Count; index++)
            {
                if (columns[index] != null && columns[index].Frozen)
                {
                    frozenCount = index + 1;
                }
            }

            if (frozenCount > 0)
            {
                this.FrozenColumnCount = frozenCount;
            }

            // 컬럼 재구성 후 클램프를 다시 계산한다 (그리드 속성 방식 포함).
            this.ApplyFrozenColumnCount();

            // 2단 그룹 헤더/집계 푸터 — 구간·집계 컬럼을 다시 계산한다
            // (고정 컬럼 수 확정 뒤에 호출해야 고정 구간 마스크가 맞는다).
            this.groupHeaderController.OnColumnsRebuilt(this.columnDefinitions);
            this.footerController.OnColumnsRebuilt(this.columnDefinitions);

            // 셀 병합 — MergeCells 컬럼 인덱스를 다시 수집한다.
            this.mergeController.OnColumnsRebuilt();

            // 데이터가 이미 할당돼 있으면(할당 순서 내성) 즉시 자동 맞춤을 적용한다.
            this.AutoFitColumnWidths();
        }

        // 헤더 캡션 정렬 — 본 캡션은 HeaderAlignment, 위·아래 작은 글씨는 TopLabelAlignment·BottomLabelAlignment(없으면 본 캡션을 따른다).
        // 세 줄은 본 캡션 폭의 세로 묶음이고 묶음 전체를 헤더 칸의 내용 정렬로 놓는다(Center = 왼쪽 빈 칸 + 헤더 전체 폭의 가운데 · Right = 글리프 앞).
        // 기본은 Center(헤더 스타일의 기본값, 2026-09-30) — Left·Right 만 컬럼별 스타일을 만들고, 작은 글씨 없는 Center 캡션은 문자열 그대로 둔다.
        // 작은 줄의 Left·Right·Center 는 본 캡션 글자의 왼쪽 끝·오른쪽 끝·가운데 기준이다(SubLabelPanel — 버튼의 TopLabel·BottomLabel 과 같은
        // 규칙, 2026-09-30). 작은 줄이 본 캡션보다 길면 기준을 지킨 채 밖으로 나간다. 역할은 자식 순서가 아니라 표식으로 가른다.
        // 체크박스 헤더처럼 글자가 아닌 헤더는 건드리지 않는다.
        private void ApplyHeaderAlignment(DataGridColumn column, ModernDataGridColumn definition)
        {
            GridTextAlignment caption = definition.HeaderAlignment;
            string plain = column.Header as string;

            // 작은 글씨 없는 문자열 헤더는 Center(기본)·Left 에서 문자열 그대로 둔다 — 내용 정렬만으로 충분하고 검사·내보내기 경로가 문자열을 읽는다.
            // Right 만 TextBlock 으로 바꿔 "\n" 여러 줄 캡션의 줄 정렬까지 맞춘다.
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
            else if (plain == null)
            {
                return;
            }

            if (caption == GridTextAlignment.Center || column.HeaderStyle != null)
            {
                return;
            }

            Style headerStyle = new Style(typeof(DataGridColumnHeader));
            headerStyle.BasedOn = (Style)this.FindResource("ModernGridColumnHeaderStyle");
            headerStyle.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty,
                    caption == GridTextAlignment.Left ? HorizontalAlignment.Left : HorizontalAlignment.Right));
            column.HeaderStyle = headerStyle;
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

        /// <summary>
        /// 헤더 캡션의 최대 줄 수("\n" 기준)에 맞춰 헤더 높이를 조정한다.
        /// 계산 자체(줄 수 세기 + 한 줄 높이 실측)는 GridAutoFitMeasurer가 담당한다.
        /// </summary>
        private void ApplyHeaderHeight()
        {
            this.InnerDataGrid.ColumnHeaderHeight =
                GridAutoFitMeasurer.ComputeHeaderHeight(this, this.columnDefinitions);
        }

        /// <summary>재정의 값(0 = 전역)을 유효 장평으로 해석한다.</summary>
        private double ResolvedFontWidthRatio()
        {
            return Modern.Lab.Theming.ModernTheme.ResolveFontWidthRatio(this.FontWidthRatio);
        }

        // 장평이 바뀌면 이미 구성된 컬럼을 새 비율로 다시 만들고,
        // 공용 첨부 속성도 갱신해 상태바 등 XAML 텍스트 요소에도 반영한다.
        private static void OnFontWidthRatioChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernDataGridControl control = (ModernDataGridControl)d;

            Common.FontWidthScaling.SetFontWidthRatio(control, (double)e.NewValue);

            if (control.columnDefinitions != null)
            {
                control.ApplyColumns(new List<ModernDataGridColumn>(control.columnDefinitions));
            }
        }

        // 헤더 체크박스 클릭: 현재 그리드에 표시 중인 모든 행의 값을 일괄 설정한다.
        // 페이지 화면이라면 현재 페이지 조각이 대상이다 — DataTable 소스는 행 값
        // 변경이 ColumnChanged로 전파되므로 폼의 원본 동기화 로직이 그대로 동작한다.
        private void OnHeaderCheckBoxClick(object sender, RoutedEventArgs e)
        {
            CheckBox header = (CheckBox)sender;
            ModernDataGridColumn definition = header.Tag as ModernDataGridColumn;

            if (definition == null)
            {
                return;
            }

            string memberPath = definition.DataPropertyName;
            bool check = header.IsChecked == true;

            // 쓸 값은 컬럼이 정한다 — bool 컬럼이면 bool, "Y"/"N" 어휘 컬럼이면
            // 그 문자열. 여기서 bool을 쓰면 문자열 컬럼에 "True"가 들어간다.
            object writeValue = GridColumnFactory.CheckWriteValue(definition, check);

            foreach (object item in this.InnerDataGrid.Items)
            {
                // 값이 실제로 바뀐 행만 알린다 — 이미 같은 값이던 행까지
                // 서버로 보내면 일괄 토글 한 번이 불필요한 호출을 낳는다.
                object before = MemberPathReader.Read(item, memberPath);
                bool changed = !object.Equals(before, writeValue);

                MemberPathReader.Write(item, memberPath, writeValue);
                CommitRowEdit(item);

                if (changed)
                {
                    this.RaiseCheckChanged(item, memberPath, check);
                }
            }

            this.RefreshHeaderCheckBoxes();
        }

        // 셀 체크박스 클릭: 헤더 체크박스 상태(전체/일부/없음)를 다시 계산하고,
        // 값 변경을 이벤트로 알린다(즉시 저장이 필요한 화면용).
        private void OnCellCheckBoxClick(object sender, RoutedEventArgs e)
        {
            this.RefreshHeaderCheckBoxes();

            CheckBox cell = sender as CheckBox;

            if (cell == null)
            {
                return;
            }

            // 셀 체크박스는 바인딩으로 원본 행에 값을 쓰는데, DataRowView는
            // 그 쓰기로 **편집 상태**에 들어간 뒤 커밋될 때까지 열려 있다.
            // 읽기 전용 그리드(IsReadOnly)는 행 편집을 스스로 끝내 주지 않아,
            // 열린 편집이 남은 채 다른 행을 고르면 DataView가 그 행을 다시
            // 자리잡으며 목록이 출렁인다. 값이 바뀐 그 자리에서 커밋한다.
            CommitRowEdit(cell.DataContext);

            this.RaiseCheckChanged(
                    cell.DataContext,
                    BindingPathOf(cell),
                    cell.IsChecked == true);
        }

        // 행 편집을 즉시 확정한다 (DataRowView 등 IEditableObject 소스).
        private static void CommitRowEdit(object item)
        {
            System.ComponentModel.IEditableObject editable =
                    item as System.ComponentModel.IEditableObject;

            if (editable != null)
            {
                editable.EndEdit();
            }
        }

        // 체크박스 셀에 걸린 바인딩 경로(= 컬럼의 DataPropertyName).
        private static string BindingPathOf(CheckBox cell)
        {
            System.Windows.Data.BindingExpression expression =
                    cell.GetBindingExpression(CheckBox.IsCheckedProperty);

            return expression == null || expression.ParentBinding == null
                    ? string.Empty
                    : expression.ParentBinding.Path.Path;
        }

        private void RaiseCheckChanged(object item, string memberPath, bool isChecked)
        {
            // 집계 푸터 — 체크 토글도 값 변화이므로 다시 계산한다 (Count 집계 등).
            this.footerController.Recalculate();

            if (this.CellCheckChanged != null)
            {
                this.CellCheckChanged(
                        this, new GridCheckChangedEventArgs(item, memberPath, isChecked));
            }
        }

        // 헤더 체크박스 상태를 행 값으로부터 다시 계산한다 —
        // 전체 체크 = 체크, 전체 해제/빈 그리드 = 해제, 일부만 체크 = 중간 상태.
        private void RefreshHeaderCheckBoxes()
        {
            foreach (CheckBox header in this.headerCheckBoxes)
            {
                ModernDataGridColumn definition = header.Tag as ModernDataGridColumn;

                if (definition == null)
                {
                    continue;
                }

                string memberPath = definition.DataPropertyName;
                int total = 0;
                int checkedCount = 0;

                foreach (object item in this.InnerDataGrid.Items)
                {
                    total = total + 1;

                    object value = MemberPathReader.Read(item, memberPath);

                    // 셀과 같은 판정 규칙 — 어휘 컬럼이면 Y/N을 읽는다.
                    if (GridColumnFactory.ReadCheck(definition, value))
                    {
                        checkedCount = checkedCount + 1;
                    }
                }

                if (total == 0 || checkedCount == 0)
                {
                    header.IsChecked = false;
                }
                else if (checkedCount == total)
                {
                    header.IsChecked = true;
                }
                else
                {
                    header.IsChecked = null;
                }
            }
        }

        private void OnCellButtonClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            FrameworkElement element = (FrameworkElement)sender;

            if (this.CellButtonClick != null)
            {
                this.CellButtonClick(this, new GridButtonClickEventArgs(element.DataContext, element.Tag as string));
            }
        }

        // 링크 셀 클릭 — 클릭한 행을 먼저 현재 행으로 선택한 뒤 알린다
        // (핸들러가 SelectedItem 기준 로직과 섞여도 어긋나지 않게).
        private void OnCellLinkClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            FrameworkElement element = (FrameworkElement)sender;

            if (element.DataContext != null)
            {
                this.InnerDataGrid.SelectedItem = element.DataContext;
            }

            if (this.CellLinkClick != null)
            {
                this.CellLinkClick(this, new GridButtonClickEventArgs(element.DataContext, element.Tag as string));
            }
        }

        private static void OnStatusBarAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernDataGridControl)d).RefreshStatusBar();
        }

        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernDataGridControl control = (ModernDataGridControl)d;
            control.pulsedMarkerItems.Clear();
            if (control.hostDisposed)
            {
                return;
            }

            control.ObserveBindingList(e.NewValue as System.ComponentModel.IBindingList);
            if (!control.RefreshBadgeColumnWidths())
            {
                control.AutoFitColumnWidths();
            }

            // 유지 중인 컬럼 필터를 새 소스의 뷰에 다시 적용한다 (재조회 내성).
            control.filterController.OnItemsSourceChanged();
        }

        private static void OnAutoFitColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernDataGridControl)d).AutoFitColumnWidths();
        }

        private void OnFilterControllerChanged(object sender, EventArgs e)
        {
            if (this.ColumnFiltersChanged != null)
            {
                this.ColumnFiltersChanged(this, EventArgs.Empty);
            }
        }

        // 헤더 깔때기 버튼 클릭 — 헤더 템플릿의 필터 버튼에서 올라온다. 버튼이
        // 속한 헤더의 컬럼을 찾아 값 필터 팝업을 연다 (헤더 클릭 정렬과 분리).
        private void OnHeaderFilterClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            DependencyObject source = sender as DependencyObject;

            while (source != null
                    && !(source is System.Windows.Controls.Primitives.DataGridColumnHeader))
            {
                source = VisualTreeHelper.GetParent(source);
            }

            System.Windows.Controls.Primitives.DataGridColumnHeader header =
                    source as System.Windows.Controls.Primitives.DataGridColumnHeader;

            if (header != null)
            {
                this.filterController.OpenPopupFor(header.Column, (UIElement)sender);
            }
        }

        // 필터 사용 여부가 바뀌면 헤더(깔때기 유무)를 다시 만든다.
        private static void OnAllowColumnFiltersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernDataGridControl control = (ModernDataGridControl)d;

            if (control.columnDefinitions != null)
            {
                control.ApplyColumns(new List<ModernDataGridColumn>(control.columnDefinitions));
            }
        }

        // ===== 컬럼 자동 맞춤 (헤더 + 데이터 최대 폭) =====

        // 각 컬럼 너비를 헤더/데이터 실측 폭으로 재계산해 고정 픽셀 너비로 넣는다.
        // 측정 엔진은 GridAutoFitMeasurer가 담당하고, 여기서는 적용 조건
        // (AutoFitColumns 켜짐 + 명시적 컬럼 정의 존재)만 판정한다.
        private void AutoFitColumnWidths()
        {
            if (!this.AutoFitColumns || this.columnDefinitions == null)
            {
                return;
            }

            // 장평이 적용된 텍스트는 실제 폭이 비율만큼 달라지므로 측정에도 곱한다.
            GridAutoFitMeasurer.ApplyAutoFitWidths(
                this.InnerDataGrid,
                this.columnDefinitions,
                this.ItemsSource,
                this,
                this.ResolvedFontWidthRatio());
        }

        // 소스가 IBindingList(DataView)면 그 ListChanged도 듣는다 — 표를 다시 꽂지 않고 행을 제자리에서
        // 지우거나 더하는 갱신(조용한 재조회)에서, 실체화된 그리드는 Items.CollectionChanged만으로는
        // 상태바 건수가 옛 값으로 남는 경우가 있었다(2026-08-29). 건수·빈 안내를 한 번 미뤄서 다시 그린다.
        private System.ComponentModel.IBindingList observedBindingList;

        private void ObserveBindingList(System.ComponentModel.IBindingList list)
        {
            if (this.observedBindingList != null)
            {
                this.observedBindingList.ListChanged -= this.OnSourceListChanged;
            }

            this.observedBindingList = list != null && list.SupportsChangeNotification ? list : null;

            if (this.observedBindingList != null)
            {
                this.observedBindingList.ListChanged += this.OnSourceListChanged;
            }
        }

        private void OnSourceListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
        {
            if (e.ListChangedType == System.ComponentModel.ListChangedType.ItemAdded
                    || e.ListChangedType == System.ComponentModel.ListChangedType.ItemDeleted
                    || e.ListChangedType == System.ComponentModel.ListChangedType.Reset)
            {
                this.ScheduleStatusRefresh();
            }
        }

        private void ScheduleStatusRefresh()
        {
            if (this.hostDisposed)
            {
                return;
            }

            this.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.DataBind,
                    new Action(delegate
                    {
                        if (this.hostDisposed)
                        {
                            return;
                        }

                        this.RefreshStatusBar();
                        this.RefreshEmptyLabel();
                    }));
        }

        private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.RefreshStatusBar();
            this.RefreshEmptyLabel();
            this.ScheduleStatusRefresh();

            // 소스 교체/행 변경에 맞춰 헤더 체크박스(전체 선택) 상태를 되비춘다.
            this.RefreshHeaderCheckBoxes();

            // 찾기가 열려 있으면 일치 목록/강조도 뷰 변화를 따라간다.
            this.findController.OnDataChanged();

            // 집계 푸터 — 뷰(재할당/필터/행 추가·삭제) 변화를 따라간다.
            this.footerController.Recalculate();

            // 셀 병합 — 행 추가/삭제/재정렬로 이웃 관계가 바뀌면 실체화된 행
            // 전체를 다시 입힌다 (삭제된 행의 위아래가 새로 이어질 수 있다).
            this.mergeController.QueueRefresh();
        }

        // 데이터가 바뀌면 Badge 컬럼의 최대 표시값도 달라질 수 있다. 템플릿의
        // 공통 MinWidth를 갱신하려면 컬럼을 다시 만드는 것이 가장 안전하다.
        private bool RefreshBadgeColumnWidths()
        {
            if (this.columnDefinitions == null)
            {
                return false;
            }

            // 배지 컬럼과 행별 캡션 버튼 컬럼은 통일 폭(가장 긴 표시값 기준)이
            // 데이터에 매이고, 자동색이면 색 선등록도 데이터에 매인다 — 어느
            // 쪽이든 있으면 새 소스 기준으로 컬럼을 다시 만든다.
            bool needsRebuild = false;

            foreach (ModernDataGridColumn definition in this.columnDefinitions)
            {
                if (definition.Kind == GridColumnKind.Badge
                        || (definition.Kind == GridColumnKind.Button
                            && !string.IsNullOrEmpty(definition.ButtonTextMember)))
                {
                    needsRebuild = true;
                    break;
                }
            }

            if (!needsRebuild)
            {
                return false;
            }

            this.ApplyColumns(new List<ModernDataGridColumn>(this.columnDefinitions));
            return true;
        }

        // 빈 상태 안내: 데이터 0건 + EmptyText 비어있지 않을 때만 표시한다.
        private void RefreshEmptyLabel()
        {
            string text = this.EmptyText;
            bool show = this.InnerDataGrid.Items.Count == 0 && !string.IsNullOrEmpty(text);

            this.EmptyLabel.Text = text;
            this.EmptyLabel.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private static void OnEmptyTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernDataGridControl)d).RefreshEmptyLabel();
        }

        // 교차색은 테마 전환을 따라가도록 리소스 참조로 걸고, 끌 때는 로컬 값을
        // 지워 XAML 기본(RowBackground 단색)으로 되돌린다.
        private static void OnAlternatingRowColorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernDataGridControl control = (ModernDataGridControl)d;

            if ((bool)e.NewValue)
            {
                control.InnerDataGrid.SetResourceReference(DataGrid.AlternatingRowBackgroundProperty, "Brush.GridRowAlt");
            }
            else
            {
                control.InnerDataGrid.ClearValue(DataGrid.AlternatingRowBackgroundProperty);
            }
        }

        private static void OnRowColorMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernDataGridControl control = (ModernDataGridControl)d;

            // 이미 실체화된 행에 새 경로를 반영하려면 다시 실체화시켜 LoadingRow를 재발생시킨다.
            if (control.InnerDataGrid.Items.Count > 0)
            {
                control.InnerDataGrid.Items.Refresh();
            }
        }

        private static void OnRowForegroundMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernDataGridControl control = (ModernDataGridControl)d;

            if (control.InnerDataGrid.Items.Count > 0)
            {
                control.InnerDataGrid.Items.Refresh();
            }
        }

        // 행 글자색 — RowForegroundMember 값이 색이면 행 Foreground를 로컬 값으로 두고
        // (셀 스타일에 Foreground 세터가 없어 셀·텍스트가 상속한다), 아니면 지워 그리드
        // 기본(Brush.TextPrimary)으로 되돌린다. 선택 셀의 글자색(셀 스타일 트리거)은
        // 그대로 우선한다. 배지/링크/버튼처럼 자기 색을 가진 셀은 영향을 받지 않는다.
        private void PaintRowForeground(DataGridRow row)
        {
            string path = this.RowForegroundMemberPath;

            if (!string.IsNullOrEmpty(path))
            {
                Color color;

                if (ChipColorHelper.TryParseColor(ToText(MemberPathReader.Read(row.Item, path)), out color))
                {
                    SolidColorBrush brush = new SolidColorBrush(color);
                    brush.Freeze();
                    row.Foreground = brush;
                    return;
                }
            }

            // 행 색(RowColorMember/RowColorSelector)만 있고 글자색 지정이 없으면 배경에서 글자색을 유도한다 —
            // 어두운 테마의 밝은 파스텔 행 색(CrimsonGray 의 Error 칩) 위에 테마 기본 글자(흰색)가 놓여 안 보였다
            // (2026-10-15 사용자 보고). 선택·hover 중에는 테마의 선택 배경이 깔리므로 기본 글자색으로 돌린다.
            Color tint;

            if (!row.IsSelected && !row.IsMouseOver && this.TryGetRowTint(row, out tint))
            {
                SolidColorBrush derived = new SolidColorBrush(ChipColorHelper.DeriveForeground(tint));
                derived.Freeze();
                row.Foreground = derived;
                return;
            }

            row.ClearValue(Control.ForegroundProperty);
        }

        // RowColorSelector → RowColorMember 순으로 행 색을 읽는다(PaintRowBackground 와 같은 순서).
        private bool TryGetRowTint(DataGridRow row, out Color tint)
        {
            string selectedColor = string.Empty;

            if (this.RowColorSelector != null)
            {
                try { selectedColor = this.RowColorSelector(row.Item); }
                catch (Exception) { selectedColor = string.Empty; }
            }

            if (ChipColorHelper.TryParseColor(selectedColor, out tint))
            {
                return true;
            }

            string path = this.RowColorMemberPath;

            if (!string.IsNullOrEmpty(path)
                    && ChipColorHelper.TryParseColor(ToText(MemberPathReader.Read(row.Item, path)), out tint))
            {
                return true;
            }

            tint = default(Color);
            return false;
        }

        // 각 행이 화면에 실체화될 때 배경을 칠한다 (선택 > RowColorMember > 기본).
        private void OnLoadingRow(object sender, DataGridRowEventArgs e)
        {
            // 찾기가 열려 있으면 새로 실체화된 행의 일치 텍스트도 강조한다.
            this.findController.OnRowRealized(e.Row);

            // 마우스 클릭의 가로 점프를 막는 핸들러 — 재활용된 행에 두 번 붙지 않게 떼고 붙인다.
            e.Row.RemoveHandler(FrameworkElement.RequestBringIntoViewEvent,
                    new RequestBringIntoViewEventHandler(this.OnRowRequestBringIntoView));
            e.Row.AddHandler(FrameworkElement.RequestBringIntoViewEvent,
                    new RequestBringIntoViewEventHandler(this.OnRowRequestBringIntoView));

            this.PaintRowBackground(e.Row);
            this.PaintRowForeground(e.Row);

            // 현재 셀이 이 행에 있으면(스크롤 복귀/재활용) 표식을 다시 건다 —
            // 셀 컨테이너는 레이아웃 뒤에 생기므로 한 틱 미룬다.
            this.Dispatcher.BeginInvoke(
                new Action(this.RefreshCurrentCellMarker),
                System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private bool narrowingBringIntoView;

        /// <summary>
        /// 마우스 클릭으로 현재 셀이 바뀔 때는 표를 **가로로 밀지 않는다** (2026-10-10 사용자 결정).
        /// WPF DataGrid 는 현재 셀을 온전히 보이게 스크롤하는데, 그 규칙은 키보드 이동을 위한 것이다 —
        /// 마우스는 사용자가 이미 보고 찍은 셀이라 옆으로 밀리면 보던 컬럼이 사라져 거슬린다(엑셀도 안 민다).
        /// 셀의 요청을 행에서 가로채, 지금 가로로 보이는 부분만 대상으로 다시 요청한다 — 세로는 그대로
        /// (픽셀 단위라 모자란 만큼만 움직인다). 키보드 이동·프로그램 선택·찾기 이동은 표준대로 둔다.
        /// 가로로 전혀 안 보이는 셀(클릭할 수 없는 셀)의 요청도 표준대로 둔다.
        /// </summary>
        private void OnRowRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            if (this.narrowingBringIntoView
                    || (Mouse.LeftButton != MouseButtonState.Pressed && Mouse.RightButton != MouseButtonState.Pressed))
            {
                return;
            }

            DataGridCell cell = e.TargetObject as DataGridCell;
            ScrollContentPresenter viewport = cell == null ? null : FindAncestor<ScrollContentPresenter>(cell);

            if (viewport == null)
            {
                return;
            }

            Rect cellBounds = cell.TransformToAncestor(viewport).TransformBounds(
                    new Rect(0, 0, cell.ActualWidth, cell.ActualHeight));
            double visibleLeft = Math.Max(cellBounds.Left, 0);
            double visibleRight = Math.Min(cellBounds.Right, viewport.ActualWidth);

            if (visibleRight <= visibleLeft)
            {
                return;
            }

            e.Handled = true;
            this.narrowingBringIntoView = true;

            try
            {
                cell.BringIntoView(new Rect(
                        visibleLeft - cellBounds.Left, 0, visibleRight - visibleLeft, cell.ActualHeight));
            }
            finally
            {
                this.narrowingBringIntoView = false;
            }
        }

        private static T FindAncestor<T>(DependencyObject start) where T : DependencyObject
        {
            DependencyObject current = start;

            while (current != null)
            {
                T found = current as T;

                if (found != null)
                {
                    return found;
                }

                current = current is Visual ? VisualTreeHelper.GetParent(current) : LogicalTreeHelper.GetParent(current);
            }

            return null;
        }

        /// <summary>
        /// 행 배경을 상태에 맞춰 **로컬 값**으로 칠한다 — 우선순위:
        /// 선택(선택색; 편집 중에도 유지) > 마우스 hover(hover색) >
        /// RowColorSelector 색 > RowColorMember 색 > 기본(로컬 값 제거 → RowBackground/교차색).
        ///
        /// 행 스타일 트리거가 아니라 여기서 칠하는 이유: DataGrid가
        /// RowBackground를 행 Background에 전달하는 값이 스타일 트리거보다
        /// 우선이라 트리거가 적용되지 않는다 (실측; XAML 행 스타일 주석 참고).
        /// 셀 트리거가 셀 영역을 칠하지만, 별(*) 폭 반올림으로 셀 사이에 생기는
        /// 1px 틈은 행 배경만이 덮을 수 있다 — 안 덮으면 선택 행 한가운데
        /// 흰 세로 선으로 보인다 (배지 컬럼 오른쪽 경계에서 재현되던 결함).
        /// </summary>
        private void PaintRowBackground(DataGridRow row)
        {
            if (row.IsSelected)
            {
                // 리소스 참조로 걸어 테마 브러시를 그대로 따른다.
                row.SetResourceReference(Control.BackgroundProperty, "Brush.SelectedBackground");
                return;
            }

            if (row.IsMouseOver)
            {
                row.SetResourceReference(Control.BackgroundProperty, "Brush.HoverBackground");
                return;
            }

            string selectedColor = string.Empty;
            if (this.RowColorSelector != null)
            {
                try { selectedColor = this.RowColorSelector(row.Item); }
                catch (Exception) { selectedColor = string.Empty; }
            }
            Color selectedTint;
            if (ChipColorHelper.TryParseColor(selectedColor, out selectedTint))
            {
                SolidColorBrush brush = new SolidColorBrush(selectedTint);
                brush.Freeze();
                row.Background = brush;
                return;
            }

            string path = this.RowColorMemberPath;

            if (!string.IsNullOrEmpty(path))
            {
                Color color;

                if (ChipColorHelper.TryParseColor(ToText(MemberPathReader.Read(row.Item, path)), out color))
                {
                    SolidColorBrush brush = new SolidColorBrush(color);
                    brush.Freeze();
                    row.Background = brush;
                    return;
                }
            }

            row.ClearValue(Control.BackgroundProperty);
        }

        // 행 스타일 EventSetter에서 온다 — Selected/Unselected는 최초 로딩 시
        // 자동 선택 행(컨테이너 준비 시점에 IsSelected가 켜진다)도 잡는다.
        private void OnRowSelectionStateChanged(object sender, RoutedEventArgs e)
        {
            this.PaintRowBackground((DataGridRow)sender);
            this.PaintRowForeground((DataGridRow)sender);
        }

        // 행 스타일 EventSetter에서 온다 — 마우스 hover 진입/이탈.
        private void OnRowMouseStateChanged(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.PaintRowBackground((DataGridRow)sender);
            this.PaintRowForeground((DataGridRow)sender);
        }

        private static string ToText(object value)
        {
            return value == null ? string.Empty : value.ToString();
        }

        // 상태바 표시/텍스트를 현재 속성과 행 수로 다시 그린다.
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
                countText = string.Format(format ?? string.Empty, this.InnerDataGrid.Items.Count);
            }
            catch (FormatException)
            {
                countText = format;
            }

            this.StatusCountText.Text = countText;
            this.StatusRightText.Text = this.StatusText ?? string.Empty;
        }

        private void InnerDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 상태별 행 배경 다시 칠하기는 행 스타일의 Selected/Unselected
            // EventSetter가 담당한다 (컨테이너가 아직 없는 최초 로딩도 커버).
            if (this.SelectionChanged != null)
            {
                this.SelectionChanged(this, EventArgs.Empty);
            }

            // 셀 병합 — 구간의 모든 행이 선택되면 스팬 셀도 선택색으로 되비춘다.
            this.mergeController.QueueRefresh();
        }
    }
}
