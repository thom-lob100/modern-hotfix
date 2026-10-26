using System;
using System.ComponentModel;
using System.Drawing;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Data
{
    /// <summary>
    /// 트리 + 컬럼 하이브리드 (DevExpress TreeList 대응; WPF
    /// ModernTreeListControl을 ElementHost로 호스팅). Lot 계보처럼 계층이 있는
    /// 행에 수량·상태 같은 값 컬럼을 나란히 붙일 때 쓴다 — TreeView(계층)와
    /// DataGridView(컬럼)를 한 컨트롤로 합친 자리다.
    ///
    /// 데이터는 ModernTreeView와 같은 평면 자기참조 테이블을 DataSource로
    /// 받는다(키/부모키/명칭 컬럼). 값 컬럼은 ModernDataGrid와 같은
    /// ConfigureColumns(ModernDataGridColumn) 문법이며 Text/Badge Kind만
    /// 지원한다 (그 외 Kind는 Text로 강등). 노드는 기본 전부 펼쳐진다.
    ///
    /// 계약 동작 (docs/design-notes.md 6-1절):
    /// - SelectedValue를 DataSource보다 먼저 설정해도 트리 구성 시 적용된다.
    /// - 재할당 시 기존 SelectedValue가 새 트리에 없으면 미선택으로 초기화된다.
    /// - null/빈 데이터는 빈 트리로 렌더링되며 절대 예외를 던지지 않는다.
    ///
    /// 우클릭 메뉴는 그리드와 같은 규칙이다 — 폼이 지정한 ContextMenuStrip 끝에
    /// 복사 항목(Copy row / Copy cell)이 붙고, 지정하지 않았으면 복사 항목만 있는
    /// 기본 메뉴가 뜬다. Ctrl+C / Ctrl+Shift+C 단축키도 같다.
    /// </summary>
    [ToolboxItem(true)]
    [DefaultEvent("SelectedValueChanged")]
    public class ModernTreeList : WpfElementHostBase<ModernTreeListControl>, IExcelExportGrid
    {
        private object dataSource;

        // ConfigureColumns로 선언한 컬럼 정의 사본 — DataSource 할당 시 누락 컬럼
        // 자동 보장(EnsureDeclaredColumns)에 쓴다.
        private ModernDataGridColumn[] configuredColumns;

        // 디자인 타임 WPF 생성이 실패한 경우(Wpf == null)에도 속성 그리드가
        // 동작하도록 하는 폴백 저장소.
        private string fallbackIdMember;
        private string fallbackParentIdMember;
        private string fallbackDisplayMember;
        private string fallbackTreeHeaderText;
        private double fallbackTreeColumnWidth;
        private string fallbackEmptyText = "No data";
        private bool fallbackShowStatusBar;
        private string fallbackStatusText = string.Empty;
        private string fallbackStatusCountFormat = "{0:N0} rows";
        private string fallbackRowColorMember = string.Empty;
        private string fallbackRowHeaderMember = string.Empty;
        private bool fallbackAllowColumnFilters = true;
        private bool fallbackAutoFitColumns;
        private bool fallbackFreezeTreeColumn = true;
        private string fallbackRowForegroundMember = string.Empty;
        private Func<object, string> fallbackRowMarkerSelector;
        private bool fallbackAllowFindPanel = true;
        private bool showExportMenu = true;
        private string exportSheetName = string.Empty;
        private string exportFileNamePrefix = string.Empty;

        // 우클릭 메뉴에 붙이는 공통 항목(복사 2종)의 표식 — 떼고 다시 붙일 때 이것으로 찾는다.
        private const string copyItemTag = "ModernTreeList.CopyItem";

        private System.Windows.Forms.ContextMenuStrip copyMenu;
        private System.Windows.Forms.ContextMenuStrip lastInjectedMenu;
        private bool showCopyMenu = true;

        /// <summary>선택 노드가 바뀔 때 발생한다.</summary>
        public event EventHandler SelectedValueChanged;

        /// <summary>행을 더블클릭했을 때 — 그 행이 먼저 선택된 뒤 발생한다(그리드와 같은 이벤트, 2026-10-26).</summary>
        public event EventHandler RowDoubleClick;

        /// <summary>Link 컬럼의 글자를 눌렀을 때 — 인자의 행은 원본 행(DataRowView 등), 컬럼은 원본 컬럼 이름(그리드와 같은 이벤트, 2026-10-26).</summary>
        public event EventHandler<Modern.Lab.Controls.Wpf.Data.GridButtonClickEventArgs> CellLinkClick;

        /// <summary>적절한 기본 크기로 컨트롤을 생성한다.</summary>
        public ModernTreeList()
        {
            this.Size = new Size(480, 300);
            this.fallbackIdMember = string.Empty;
            this.fallbackParentIdMember = string.Empty;
            this.fallbackDisplayMember = string.Empty;
            this.fallbackTreeHeaderText = string.Empty;
            this.fallbackTreeColumnWidth = -1d;

            if (this.Wpf != null)
            {
                this.Wpf.SelectedValueChanged += this.OnWpfSelectedValueChanged;
                this.Wpf.RowRightClick += this.OnWpfRowRightClick;
                this.Wpf.RowDoubleClick += this.OnWpfRowDoubleClick;
                this.Wpf.CellLinkClick += this.OnWpfCellLinkClick;
            }
        }

        private void OnWpfRowDoubleClick(object sender, EventArgs e)
        {
            if (this.RowDoubleClick != null)
            {
                this.RowDoubleClick(this, EventArgs.Empty);
            }
        }

        private void OnWpfCellLinkClick(object sender, Modern.Lab.Controls.Wpf.Data.GridButtonClickEventArgs e)
        {
            if (this.CellLinkClick != null)
            {
                this.CellLinkClick(this, e);
            }
        }

        /// <summary>
        /// 트리를 구성할 행 목록: 키/부모키/명칭 컬럼을 가진 DataTable, DataView,
        /// IList 또는 임의의 IEnumerable. null이면 트리를 비운다.
        /// 재할당 시 기존 SelectedValue가 새 트리에 없으면 미선택으로 초기화된다.
        /// DataTable/DataView 소스는 Id/ParentId/Display 멤버와 선언된 값 컬럼이
        /// 없으면 빈 컬럼으로 자동 보장한다.
        /// </summary>
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
                this.ApplyColumnBuilder(value);
                this.EnsureDeclaredColumns(value);

                if (this.Wpf != null)
                {
                    this.Wpf.ItemsSource = DataSourceConverter.ToItemsSource(value);
                }
            }
        }

        // ConfigureColumns(Func)로 받은 컬럼 함수 — 그리드와 같은 규칙(ModernDataGrid.columnBuilder).
        private Func<System.Data.DataTable, GridColumns> columnBuilder;
        private bool applyingColumnBuilder;

        /// <summary>
        /// 값 컬럼을 **표에서 만드는 함수**로 선언한다 — <c>tree.ConfigureColumns(LotTreeColumns)</c>(2026-10-19). 부르는 즉시 null 표로
        /// 헤더를 만들어 조회 전에도 컬럼이 보이고, 그 뒤 <see cref="DataSource"/>가 올 때마다 그 표로 함수를 다시 불러 컬럼을 맞춘다.
        /// <c>null</c>을 대입할 때는 지금 컬럼을 그대로 둔다. 배열을 받는 <see cref="ConfigureColumns(ModernDataGridColumn[])"/>를 다시
        /// 부르면 함수는 해제된다.
        /// </summary>
        public void ConfigureColumns(Func<System.Data.DataTable, GridColumns> columns)
        {
            this.columnBuilder = columns;

            if (columns != null)
            {
                this.ApplyColumnBuilder(this.dataSource);
            }
        }

        private void ApplyColumnBuilder(object value)
        {
            if (this.columnBuilder == null)
            {
                return;
            }

            if (value == null && this.configuredColumns != null)
            {
                return;
            }

            GridColumns columns = this.columnBuilder(TableOf(value));

            if (columns == null)
            {
                return;
            }

            this.applyingColumnBuilder = true;

            try
            {
                columns.Apply(this);
            }
            finally
            {
                this.applyingColumnBuilder = false;
            }
        }

        private static System.Data.DataTable TableOf(object value)
        {
            System.Data.DataTable table = value as System.Data.DataTable;

            if (table != null)
            {
                return table;
            }

            System.Data.DataView view = value as System.Data.DataView;

            return view == null ? null : view.Table;
        }

        /// <summary>노드 키로 사용할 컬럼/속성 이름.</summary>
        [Category("Modern")]
        [Description("Column/property name used as the node key")]
        [DefaultValue("")]
        public string IdMember
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.IdMemberPath;
                }

                return this.fallbackIdMember;
            }
            set
            {
                this.fallbackIdMember = value;

                if (this.Wpf != null)
                {
                    this.Wpf.IdMemberPath = value;
                }
            }
        }

        /// <summary>부모 키로 사용할 컬럼/속성 이름.</summary>
        [Category("Modern")]
        [Description("Column/property name used as the parent key")]
        [DefaultValue("")]
        public string ParentIdMember
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.ParentIdMemberPath;
                }

                return this.fallbackParentIdMember;
            }
            set
            {
                this.fallbackParentIdMember = value;

                if (this.Wpf != null)
                {
                    this.Wpf.ParentIdMemberPath = value;
                }
            }
        }

        /// <summary>트리 컬럼 표시 텍스트로 사용할 컬럼/속성 이름(WinForms 호환 이름).</summary>
        [Category("Modern")]
        [Description("Column/property name for the tree column display text")]
        [DefaultValue("")]
        public string DisplayMember
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.DisplayMemberPath;
                }

                return this.fallbackDisplayMember;
            }
            set
            {
                this.fallbackDisplayMember = value;

                if (this.Wpf != null)
                {
                    this.Wpf.DisplayMemberPath = value;
                }
            }
        }

        /// <summary>트리 컬럼(첫 컬럼)의 헤더 캡션.</summary>
        [Category("Modern")]
        [Description("Header caption of the tree (first) column")]
        [Localizable(true)]
        [DefaultValue("")]
        public string TreeHeaderText
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.TreeHeaderText;
                }

                return this.fallbackTreeHeaderText;
            }
            set
            {
                this.fallbackTreeHeaderText = value;

                if (this.Wpf != null)
                {
                    this.Wpf.TreeHeaderText = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>트리 컬럼 픽셀 폭. 0 이하(기본 -1)면 남은 공간 채움(star).</summary>
        [Category("Modern")]
        [Description("Tree column width in pixels (0 or less = fill remaining space)")]
        [DefaultValue(-1d)]
        public double TreeColumnWidth
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.TreeColumnWidth;
                }

                return this.fallbackTreeColumnWidth;
            }
            set
            {
                this.fallbackTreeColumnWidth = value;

                if (this.Wpf != null)
                {
                    this.Wpf.TreeColumnWidth = value;
                }
            }
        }

        /// <summary>
        /// 값 컬럼 너비 자동 계산(기본 false) — 그리드의 <c>AutoFitColumns</c>와 같은 뜻(2026-10-26). 켜면 폭을 적지 않은
        /// 값 컬럼이 남은 공간을 채우는 대신 헤더·내용 폭의 고정 픽셀이 되고, 합이 넓이를 넘으면 가로 스크롤이 생긴다.
        /// 데이터가 바뀔 때마다 다시 잰다. 트리 컬럼(첫 칸)은 <see cref="TreeColumnWidth"/> 그대로다.
        /// </summary>
        [Category("Modern")]
        [Description("Auto-size value column widths to header and data (ConfigureColumns columns only)")]
        [DefaultValue(false)]
        public bool AutoFitColumns
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.AutoFitColumns;
                }

                return this.fallbackAutoFitColumns;
            }
            set
            {
                this.fallbackAutoFitColumns = value;

                if (this.Wpf != null)
                {
                    this.Wpf.AutoFitColumns = value;
                }
            }
        }

        /// <summary>트리 컬럼(첫 칸) 고정(기본 true, 2026-10-26) — 값 컬럼이 넓어 가로로 스크롤해도 첫 칸이 밀려 나가지 않는다. 그리드의 <c>FrozenColumnCount</c>=1 과 같다.</summary>
        [Category("Modern")]
        [Description("Keep the tree column (first column) frozen while scrolling horizontally")]
        [DefaultValue(true)]
        public bool FreezeTreeColumn
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.FreezeTreeColumn;
                }

                return this.fallbackFreezeTreeColumn;
            }
            set
            {
                this.fallbackFreezeTreeColumn = value;

                if (this.Wpf != null)
                {
                    this.Wpf.FreezeTreeColumn = value;
                }
            }
        }

        /// <summary>행 글자색 컬럼/속성 이름(기본 빈 값 = 끔, 2026-10-26) — <c>#RRGGBB</c> 같은 색 문자열. 그리드의 <c>RowForegroundMember</c> 와 같은 문법. 선택·hover 중에는 테마 글자색이 우선한다.</summary>
        [Category("Modern")]
        [Description("Column/property name for the row text color — #RRGGBB color string (empty keeps the default)")]
        [DefaultValue("")]
        public string RowForegroundMember
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.RowForegroundMemberPath;
                }

                return this.fallbackRowForegroundMember;
            }
            set
            {
                this.fallbackRowForegroundMember = value;

                if (this.Wpf != null)
                {
                    this.Wpf.RowForegroundMemberPath = value;
                }
            }
        }

        /// <summary>첫 열(트리 칸) 셀 모서리에 짧은 표식(예: "New")을 겹쳐 그린다(2026-10-26) — 원본 행을 받아 글자를 돌려주고 빈 문자열이면 없음. 컬럼을 추가하지 않는다. 그리드의 <c>RowMarkerSelector</c> 와 같은 문법.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<object, string> RowMarkerSelector
        {
            get { return this.Wpf == null ? this.fallbackRowMarkerSelector : this.Wpf.RowMarkerSelector; }
            set
            {
                this.fallbackRowMarkerSelector = value;

                if (this.Wpf != null)
                {
                    this.Wpf.RowMarkerSelector = value;
                }
            }
        }

        /// <summary>
        /// 노드가 하나도 없을 때 가운데에 표시할 안내 문구.
        /// 기본 "No data" — 화면 문맥에 맞게 바꾸거나 빈 문자열로 끈다.
        /// </summary>
        [Category("Modern")]
        [Description("Message shown when there are no nodes (empty string = hidden)")]
        [DefaultValue("No data")]
        [Localizable(true)]
        public string EmptyText
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.EmptyText;
                }

                return this.fallbackEmptyText;
            }
            set
            {
                this.fallbackEmptyText = value;

                if (this.Wpf != null)
                {
                    this.Wpf.EmptyText = value;
                }
            }
        }

        /// <summary>트리 하단 상태바 표시 여부 (보이는 노드 수 자동 표기 + StatusText). 그리드와 같은 기본값(꺼짐)이다.</summary>
        [Category("Modern")]
        [Description("Show the tree status bar — node count on the left, StatusText on the right")]
        [DefaultValue(false)]
        public bool ShowStatusBar
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.ShowStatusBar;
                }

                return this.fallbackShowStatusBar;
            }
            set
            {
                this.fallbackShowStatusBar = value;

                if (this.Wpf != null)
                {
                    this.Wpf.ShowStatusBar = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>상태바 오른쪽에 표시할 자유 텍스트 (빈 문자열이면 표시 없음).</summary>
        [Category("Modern")]
        [Description("Free text shown on the right of the status bar")]
        [DefaultValue("")]
        [Localizable(true)]
        public string StatusText
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.StatusText;
                }

                return this.fallbackStatusText;
            }
            set
            {
                this.fallbackStatusText = value;

                if (this.Wpf != null)
                {
                    this.Wpf.StatusText = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>상태바 왼쪽 건수 표기 형식. {0}에 보이는 노드 수가 들어간다 — 접힌 자식은 세지 않는다.</summary>
        [Category("Modern")]
        [Description("Status bar count format — {0} is the visible node count")]
        [DefaultValue("{0:N0} rows")]
        [Localizable(true)]
        public string StatusCountFormat
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.StatusCountFormat;
                }

                return this.fallbackStatusCountFormat;
            }
            set
            {
                this.fallbackStatusCountFormat = value;

                if (this.Wpf != null)
                {
                    this.Wpf.StatusCountFormat = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>행 배경색으로 쓸 컬럼/속성 이름 (선택 사항). "#FEE2E2" 같은 색 문자열이나 색 이름이며, 비었거나 해석 불가한 행은 기본 배경을 유지한다. 선택·hover 가 이 색보다 우선한다.</summary>
        [Category("Modern")]
        [Description("Column/property name for the row background — #RRGGBB color string (empty keeps the default)")]
        [DefaultValue("")]
        public string RowColorMember
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.RowColorMemberPath;
                }

                return this.fallbackRowColorMember;
            }
            set
            {
                this.fallbackRowColorMember = value;

                if (this.Wpf != null)
                {
                    this.Wpf.RowColorMemberPath = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>
        /// 소제목 행을 가리키는 컬럼/속성 이름 — 값이 참(bool true · "Y" · "true" · "1")인 행은 굵은 글씨 + 컬럼 헤더와 같은 띠 +
        /// 위쪽 1px 구분선으로 그려 묶음의 제목으로 읽힌다(2026-10-20). 비우면 없음. 선택·hover 가 우선이고 <see cref="RowColorMember"/>
        /// 색이 있으면 배경은 그 색이되 굵기·구분선은 남는다.
        /// </summary>
        [Category("Modern")]
        [Description("Column/property name that marks heading rows (true/Y/1) — drawn semibold on the header band with a top divider")]
        [DefaultValue("")]
        public string RowHeaderMember
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.RowHeaderMemberPath;
                }

                return this.fallbackRowHeaderMember;
            }
            set
            {
                this.fallbackRowHeaderMember = value;

                if (this.Wpf != null)
                {
                    this.Wpf.RowHeaderMemberPath = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>
        /// 값 컬럼 헤더의 깔때기(컬럼 값 필터) 사용 여부 (기본 `true` — 그리드와 같다).
        /// 고른 값에 걸린 노드와 **그 조상**이 남아 계보가 끊기지 않고, 거르는 동안에는
        /// 남은 가지가 접힘과 무관하게 보인다. 트리 컬럼에는 깔때기가 붙지 않는다.
        /// </summary>
        [Category("Modern")]
        [Description("Show the column value filter funnel on value columns (matches keep their ancestors)")]
        [DefaultValue(true)]
        public bool AllowColumnFilters
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.AllowColumnFilters;
                }

                return this.fallbackAllowColumnFilters;
            }
            set
            {
                this.fallbackAllowColumnFilters = value;

                if (this.Wpf != null)
                {
                    this.Wpf.AllowColumnFilters = value;
                }
            }
        }

        /// <summary>
        /// 찾기(Ctrl+F) 사용 여부 (기본 `true` — 그리드와 같다). 노드를 거르지 않고
        /// 일치 셀로 이동한다. **보이는 노드**만 훑으므로 전체를 찾으려면 `ExpandAll()`을 먼저 부른다.
        /// </summary>
        [Category("Modern")]
        [Description("Enable Ctrl+F find — jumps to matches in the visible nodes")]
        [DefaultValue(true)]
        public bool AllowFindPanel
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.AllowFindPanel;
                }

                return this.fallbackAllowFindPanel;
            }
            set
            {
                this.fallbackAllowFindPanel = value;

                if (this.Wpf != null)
                {
                    this.Wpf.AllowFindPanel = value;
                }
            }
        }

        /// <summary>값이 걸린 컬럼이 하나라도 있는지.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HasActiveColumnFilters
        {
            get { return this.Wpf != null && this.Wpf.HasActiveColumnFilters; }
        }

        /// <summary>모든 컬럼의 값 필터를 해제한다.</summary>
        public void ClearColumnFilters()
        {
            if (this.Wpf != null)
            {
                this.Wpf.ClearColumnFilters();
            }
        }

        /// <summary>찾기 창을 연다 (AllowFindPanel 이 꺼져 있으면 아무 일도 없다).</summary>
        public void ShowFindPanel()
        {
            if (this.Wpf != null)
            {
                this.Wpf.ShowFindPanel();
            }
        }

        /// <summary>
        /// 우클릭 메뉴에 "Export to Excel..."을 넣을지 (기본 `true`). 값 컬럼을
        /// 선언(`ConfigureColumns`)한 트리에만 항목이 나타난다 — 내보내기가 그
        /// 정의를 엑셀 컬럼의 단일 원천으로 쓰기 때문이다. 트리 컬럼은 첫 컬럼으로
        /// 같이 나간다(헤더는 `TreeHeaderText`, 값은 `DisplayMember`).
        /// </summary>
        [Category("Modern")]
        [Description("Add Export to Excel to the right-click menu (ConfigureColumns trees only, default true)")]
        [DefaultValue(true)]
        public bool ShowExportMenu
        {
            get { return this.showExportMenu; }
            set { this.showExportMenu = value; }
        }

        /// <summary>우클릭 내보내기의 시트 이름 (빈 값 = "Data").</summary>
        [Category("Modern")]
        [Description("Excel sheet name for right-click export (empty = Data)")]
        [DefaultValue("")]
        public string ExportSheetName
        {
            get { return this.exportSheetName; }
            set { this.exportSheetName = value ?? string.Empty; }
        }

        /// <summary>우클릭 내보내기의 파일명 접두사 — 뒤에 "_yyyyMMdd_HHmmss.xlsx"가 붙는다 (빈 값 = "Export").</summary>
        [Category("Modern")]
        [Description("File name prefix for right-click export (empty = Export)")]
        [DefaultValue("")]
        public string ExportFileNamePrefix
        {
            get { return this.exportFileNamePrefix; }
            set { this.exportFileNamePrefix = value ?? string.Empty; }
        }

        /// <summary>
        /// 선택 노드의 키 (IdMember 기준). null = 미선택.
        /// DataSource보다 먼저 설정해도 트리 구성 시 적용되고,
        /// 설정 시 선택 노드가 보이도록 조상이 자동 펼쳐진다.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedValue
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.SelectedValue;
                }

                return null;
            }
            set
            {
                if (this.Wpf != null)
                {
                    this.Wpf.SelectedValue = value;
                }
            }
        }

        /// <summary>선택 노드의 원본 행 (DataRowView 등). 미선택이면 null.</summary>
        [Browsable(false)]
        public object SelectedItem
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.SelectedItem;
                }

                return null;
            }
        }

        /// <summary>
        /// 트리 컬럼 오른쪽에 붙는 값 컬럼을 정의한다 (ModernDataGrid와 같은
        /// ModernDataGridColumn 문법, Text/Badge Kind만 지원 — 그 외 Kind는
        /// Text로 강등). DataSource 할당 전에 호출한다.
        /// </summary>
        public void ConfigureColumns(params ModernDataGridColumn[] columns)
        {
            if (!this.applyingColumnBuilder)
            {
                this.columnBuilder = null;
            }

            this.configuredColumns = columns;

            if (this.Wpf != null)
            {
                this.Wpf.ApplyColumns(columns);
            }
        }

        /// <summary>
        /// 우클릭 메뉴에 복사 항목(Copy row / Copy cell)을 넣을지 여부. 기본 true.
        /// 폼이 지정한 <see cref="System.Windows.Forms.Control.ContextMenuStrip"/>이 있으면
        /// 그 메뉴 맨 끝에 구분선과 함께 붙고, 없으면 복사 항목만 있는 메뉴가 뜬다.
        /// false면 어느 쪽도 붙지 않는다(단축키 Ctrl+C / Ctrl+Shift+C는 그대로 동작).
        /// </summary>
        [Category("Modern")]
        [Description("Add copy items to the right-click menu (appended to an existing ContextMenuStrip)")]
        [DefaultValue(true)]
        public bool ShowCopyMenu
        {
            get { return this.showCopyMenu; }
            set { this.showCopyMenu = value; }
        }

        /// <summary>현재 노드 행을 탭 구분 한 줄로 클립보드에 복사한다 (Ctrl+C와 같은 동작).</summary>
        public void CopySelectedRow()
        {
            if (this.Wpf != null)
            {
                this.Wpf.CopyRowToClipboard();
            }
        }

        /// <summary>현재 셀 하나의 표시 텍스트만 클립보드에 복사한다 (Ctrl+Shift+C와 같은 동작).</summary>
        public void CopyCurrentCell()
        {
            if (this.Wpf != null)
            {
                this.Wpf.CopyCellToClipboard();
            }
        }

        /// <summary>
        /// 선언한 컬럼 그대로(트리 컬럼 + 값 컬럼 순서·캡션·형식) 데이터를 .xlsx 로 저장한다.
        /// 트리 컬럼은 첫 컬럼으로 나가며 헤더는 <see cref="TreeHeaderText"/>, 값은
        /// <see cref="DisplayMember"/> 다 — 엑셀에는 계보 들여쓰기가 아니라 값이 실린다.
        /// </summary>
        public void ExportXlsx(string path, string sheetName, System.Data.DataTable data)
        {
            if (this.configuredColumns == null || this.configuredColumns.Length == 0)
            {
                throw new InvalidOperationException("ExportXlsx는 ConfigureColumns로 컬럼을 선언한 뒤에만 쓸 수 있다.");
            }

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            Modern.Lab.Export.GridXlsxExporter.Write(path, sheetName, this.BuildExportColumns(), data);
        }

        // 트리 컬럼(선언했으면) + 값 컬럼 — 화면 순서 그대로.
        private System.Collections.Generic.List<ModernDataGridColumn> BuildExportColumns()
        {
            System.Collections.Generic.List<ModernDataGridColumn> columns =
                    new System.Collections.Generic.List<ModernDataGridColumn>();
            string member = this.DisplayMember;

            if (!string.IsNullOrEmpty(member))
            {
                columns.Add(new ModernDataGridColumn(member, this.TreeHeaderText ?? string.Empty, 0d));
            }

            foreach (ModernDataGridColumn column in this.configuredColumns)
            {
                if (column != null)
                {
                    columns.Add(column);
                }
            }

            return columns;
        }

        /// <summary>내보낼 원본 표 — 트리에 넣은 DataSource 그대로다(펼침·필터와 무관).</summary>
        public System.Data.DataTable ResolveExportData()
        {
            System.Data.DataTable table = this.dataSource as System.Data.DataTable;

            if (table != null)
            {
                return table;
            }

            System.Data.DataView view = this.dataSource as System.Data.DataView;
            return view == null ? null : view.ToTable();
        }

        /// <summary>
        /// 저장 위치를 묻고 .xlsx 로 내보낸다 — 우클릭 "Export to Excel..."과 같은 동작.
        /// 저장했으면 true, 취소·데이터 없음·실패면 false 다.
        /// </summary>
        public bool ExportXlsxInteractive()
        {
            if (this.configuredColumns == null || this.configuredColumns.Length == 0)
            {
                return false;
            }

            System.Windows.Forms.Form owner = this.FindForm();
            System.Data.DataTable data = this.ResolveExportData();

            if (data == null || data.Rows.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show(
                        owner, "No data to export.", "Export to Excel",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Information);
                return false;
            }

            string prefix = this.exportFileNamePrefix.Length > 0 ? this.exportFileNamePrefix : "Export";
            string sheet = this.exportSheetName.Length > 0 ? this.exportSheetName : "Data";

            using (System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog())
            {
                dialog.Filter = "Excel Workbook (*.xlsx)|*.xlsx";
                dialog.FileName = prefix + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";

                if (dialog.ShowDialog(owner) != System.Windows.Forms.DialogResult.OK)
                {
                    return false;
                }

                try
                {
                    this.ExportXlsx(dialog.FileName, sheet, data);
                    return true;
                }
                catch (Exception exception)
                {
                    // 파일이 열려 있거나 권한이 없는 경우 등 — 화면은 죽지 않는다.
                    System.Windows.Forms.MessageBox.Show(
                            owner, "Export failed: " + exception.Message, "Export to Excel",
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        /// <summary>모든 노드를 펼친다.</summary>
        public void ExpandAll()
        {
            if (this.Wpf != null)
            {
                this.Wpf.ExpandAll();
            }
        }

        /// <summary>모든 노드를 접는다 (루트만 남는다).</summary>
        public void CollapseAll()
        {
            if (this.Wpf != null)
            {
                this.Wpf.CollapseAll();
            }
        }

        // 트리 구성 멤버와 선언된 값 컬럼을 DataTable/DataView 소스에 빈 컬럼으로
        // 보장한다 (ModernDataGrid의 EnsureDeclaredColumns와 같은 이유 —
        // 서버가 null 컬럼을 생략해도 폼이 컬럼을 다시 나열할 필요가 없다).
        private void EnsureDeclaredColumns(object value)
        {
            System.Collections.Generic.List<string> members =
                    new System.Collections.Generic.List<string>();
            members.Add(this.IdMember);
            members.Add(this.ParentIdMember);
            members.Add(this.DisplayMember);

            if (this.configuredColumns != null)
            {
                foreach (ModernDataGridColumn column in this.configuredColumns)
                {
                    if (column != null)
                    {
                        members.Add(column.DataPropertyName);
                        AddMember(members, column.BadgeColorMember);
                        AddMember(members, column.BadgeSemanticMember);
                        AddMember(members, column.BadgeGroupMember);
                        AddMember(members, column.BadgeSpinBorderMember);
                    }
                }
            }

            DataSourceConverter.EnsureColumns(value, members);
        }

        private static void AddMember(System.Collections.Generic.List<string> members, string member)
        {
            if (!string.IsNullOrEmpty(member))
            {
                members.Add(member);
            }
        }

        private void OnWpfSelectedValueChanged(object sender, EventArgs e)
        {
            if (this.SelectedValueChanged != null)
            {
                this.SelectedValueChanged(this, EventArgs.Empty);
            }
        }

        // 노드 우클릭: WPF 쪽이 그 노드를 선택한 뒤 알려주면, 표준 WinForms 방식대로
        // 이 컨트롤에 지정된 ContextMenuStrip을 커서 위치에 띄운다 — ModernDataGrid와
        // 같은 사용법(폼은 ContextMenuStrip만 지정하고 Opening에서 선택 노드를 읽는다).
        // 복사 항목은 폼 메뉴 끝에 붙이고, 폼 메뉴가 없으면 복사 항목만 있는 메뉴를 띄운다.
        private void OnWpfRowRightClick(object sender, EventArgs e)
        {
            if (this.ContextMenuStrip != null)
            {
                this.AttachCopyItems(this.ContextMenuStrip);
                this.ContextMenuStrip.Show(System.Windows.Forms.Control.MousePosition);
                return;
            }

            System.Windows.Forms.ContextMenuStrip menu = this.EnsureDefaultMenu();

            if (menu.Items.Count > 0)
            {
                menu.Show(System.Windows.Forms.Control.MousePosition);
            }
        }

        // 폼이 만든 메뉴 끝에 복사 항목을 붙인다 — 매번 떼고 다시 붙여 멱등하게
        // (여러 번 우클릭해도 늘지 않고, 메뉴를 여러 컨트롤이 공유해도 이 트리 기준).
        private void AttachCopyItems(System.Windows.Forms.ContextMenuStrip menu)
        {
            if (this.lastInjectedMenu != null && !object.ReferenceEquals(this.lastInjectedMenu, menu))
            {
                RemoveInjectedItems(this.lastInjectedMenu);
            }

            this.lastInjectedMenu = menu;
            RemoveInjectedItems(menu);

            bool find = this.AllowFindPanel;
            bool export = this.ExportMenuAvailable();

            if (!this.showCopyMenu && !find && !export)
            {
                return;
            }

            if (menu.Items.Count > 0)
            {
                System.Windows.Forms.ToolStripSeparator separator =
                        new System.Windows.Forms.ToolStripSeparator();
                separator.Tag = copyItemTag;
                menu.Items.Add(separator);
            }

            if (this.showCopyMenu)
            {
                menu.Items.Add(this.CreateCopyRowItem());
                menu.Items.Add(this.CreateCopyCellItem());
            }

            if (find)
            {
                menu.Items.Add(this.CreateFindItem());
            }

            if (export)
            {
                menu.Items.Add(this.CreateExportItem());
            }
        }

        private System.Windows.Forms.ToolStripMenuItem CreateFindItem()
        {
            System.Windows.Forms.ToolStripMenuItem item =
                    new System.Windows.Forms.ToolStripMenuItem("Find...");
            item.ShortcutKeyDisplayString = "Ctrl+F";
            item.Tag = copyItemTag;
            item.Click += delegate { this.ShowFindPanel(); };
            return item;
        }

        // 내보내기 항목은 값 컬럼을 선언한 트리에만 붙는다 — ExportXlsx 가 그 정의를
        // 엑셀 컬럼의 단일 원천으로 쓰기 때문이다(그리드와 같은 규칙).
        private bool ExportMenuAvailable()
        {
            return this.showExportMenu
                    && this.configuredColumns != null
                    && this.configuredColumns.Length > 0;
        }

        private System.Windows.Forms.ToolStripMenuItem CreateExportItem()
        {
            System.Windows.Forms.ToolStripMenuItem item =
                    new System.Windows.Forms.ToolStripMenuItem("Export to Excel...");
            item.Tag = copyItemTag;
            item.Click += delegate { this.ExportXlsxInteractive(); };
            return item;
        }

        private static void RemoveInjectedItems(System.Windows.Forms.ContextMenuStrip menu)
        {
            if (menu.IsDisposed)
            {
                return;
            }

            for (int index = menu.Items.Count - 1; index >= 0; index--)
            {
                if (object.Equals(menu.Items[index].Tag, copyItemTag))
                {
                    System.Windows.Forms.ToolStripItem stale = menu.Items[index];
                    menu.Items.RemoveAt(index);
                    stale.Dispose();
                }
            }
        }

        // 복사 항목만 있는 기본 메뉴 — 항목은 열 때마다 다시 만든다(ShowCopyMenu 즉시 반영).
        // 단축키는 표시만 한다(ShortcutKeyDisplayString) — 실제 키 처리는 WPF 쪽이 한다.
        private System.Windows.Forms.ContextMenuStrip EnsureDefaultMenu()
        {
            if (this.copyMenu == null)
            {
                this.copyMenu = new System.Windows.Forms.ContextMenuStrip();
                this.copyMenu.Renderer = new Modern.Lab.WinForms.Rendering.ModernMenuRenderer();
            }

            for (int index = this.copyMenu.Items.Count - 1; index >= 0; index--)
            {
                System.Windows.Forms.ToolStripItem stale = this.copyMenu.Items[index];
                this.copyMenu.Items.RemoveAt(index);
                stale.Dispose();
            }

            if (this.showCopyMenu)
            {
                this.copyMenu.Items.Add(this.CreateCopyRowItem());
                this.copyMenu.Items.Add(this.CreateCopyCellItem());
            }

            if (this.AllowFindPanel)
            {
                this.copyMenu.Items.Add(this.CreateFindItem());
            }

            if (this.ExportMenuAvailable())
            {
                this.copyMenu.Items.Add(this.CreateExportItem());
            }

            return this.copyMenu;
        }

        private System.Windows.Forms.ToolStripMenuItem CreateCopyRowItem()
        {
            System.Windows.Forms.ToolStripMenuItem item =
                    new System.Windows.Forms.ToolStripMenuItem("Copy row");
            item.ShortcutKeyDisplayString = "Ctrl+C";
            item.Tag = copyItemTag;
            item.Click += delegate { this.CopySelectedRow(); };
            return item;
        }

        private System.Windows.Forms.ToolStripMenuItem CreateCopyCellItem()
        {
            System.Windows.Forms.ToolStripMenuItem item =
                    new System.Windows.Forms.ToolStripMenuItem("Copy cell");
            item.ShortcutKeyDisplayString = "Ctrl+Shift+C";
            item.Tag = copyItemTag;
            item.Click += delegate { this.CopyCurrentCell(); };
            return item;
        }

        /// <summary>기본 복사 메뉴와 폼 메뉴에 붙인 항목을 정리한다.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.lastInjectedMenu != null)
                {
                    RemoveInjectedItems(this.lastInjectedMenu);
                    this.lastInjectedMenu = null;
                }

                if (this.copyMenu != null)
                {
                    this.copyMenu.Dispose();
                    this.copyMenu = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
