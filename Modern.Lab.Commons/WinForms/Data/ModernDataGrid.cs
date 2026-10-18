using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Data
{
    /// <summary>
    /// System.Windows.Forms.DataGridView의 드롭인 대체 컨트롤(읽기 전용 목록
    /// 시나리오; WPF ModernDataGridControl을 ElementHost로 호스팅).
    ///
    /// 호환 멤버: DataSource(DataTable/DataView/IList/IEnumerable),
    /// AutoGenerateColumns, RowCount, SelectionChanged, Enabled.
    /// 추가 기능: 컬럼 값 필터(AllowColumnFilters), Ctrl+F 찾기 — 엑셀식
    /// 이동 팝업(AllowFindPanel), 행/셀 복사(Ctrl+C/Ctrl+Shift+C), 상태바, 자동 폭.
    ///
    /// 계약 동작 (docs/design-notes.md 6-1절):
    /// - DataSource를 할당하면 선택이 초기화되고, 데이터가 있으면 첫 행이
    ///   선택되며(DataGridView의 초기 현재 행과 동일), 할당 한 번당
    ///   SelectionChanged가 정확히 한 번 발생한다.
    /// - null/빈 데이터는 빈 그리드로 렌더링되며 절대 예외를 던지지 않는다.
    /// </summary>
    [ToolboxItem(true)]
    [DefaultEvent("SelectionChanged")]
    public class ModernDataGrid : WpfElementHostBase<ModernDataGridControl>, IExcelExportGrid
    {
        private object dataSource;
        private bool suppressSelectionChanged;

        // ConfigureColumns로 선언한 컬럼 정의 사본 — DataSource 할당 시 누락 컬럼
        // 자동 보장(EnsureDeclaredColumns)에 쓴다.
        private ModernDataGridColumn[] configuredColumns;

        // 지금 컬럼이 데이터에서 자동으로 나온 것인지 — ConfigureColumns로 사람이
        // 선언하면 false가 되고, 그 뒤로는 자동 생성이 끼어들지 않는다.
        private bool autoColumns;

        // ConfigureColumns(Func)로 받은 컬럼 함수 — 조회 전에는 null 표로 헤더를, DataSource가 올 때마다 그 표로
        // 컬럼을 다시 만든다(구성이 같으면 GridColumns.Apply가 건너뛴다). 배열로 ConfigureColumns를 부르면 해제된다.
        private Func<System.Data.DataTable, GridColumns> columnBuilder;
        private bool applyingColumnBuilder;

        // 디자인 타임 WPF 생성이 실패한 경우(Wpf == null)에도 속성 그리드가
        // 동작하도록 하는 폴백 저장소.
        private bool fallbackAutoGenerateColumns;
        private bool fallbackReadOnly;
        private bool fallbackMultiSelect;
        private int fallbackFrozenColumnCount;
        private bool fallbackAutoFitColumns;
        private bool fallbackEnableColumnVirtualization;
        private bool fallbackAllowColumnFilters;
        private bool fallbackAllowFindPanel;
        private bool fallbackAlternatingRowColors;
        private bool fallbackShowStatusBar;
        private string fallbackStatusText;
        private string fallbackStatusCountFormat;
        private string fallbackRowColorMember;
        private Func<object, string> fallbackRowColorSelector;
        private Func<object, string> fallbackRowMarkerSelector;

        private string fallbackRowForegroundMember;
        private string fallbackEmptyText = "No data";

        // 재바인딩 시 선택을 되찾는 키. 이 둘은 WPF 컨트롤이 아니라 래퍼가 들고
        // 있는다 — 포착·복원 판단이 전부 DataSource 세터(래퍼) 안에서 일어난다.
        private string rowKeyMember = string.Empty;

        private bool restoreSelectionByIndex;

        // 우클릭 기본 메뉴(복사 2종 + 찾기) — 처음 필요할 때 만든다.
        private System.Windows.Forms.ContextMenuStrip copyMenu;
        private bool showCopyMenu = true;
        private bool showExportMenu = true;
        private string exportSheetName = string.Empty;
        private string exportFileNamePrefix = string.Empty;

        // 폼 메뉴에 붙인 그리드 공통 항목을 알아보는 표식 — 열기 직전에 떼고 다시 붙인다.
        private const string copyItemTag = "ModernDataGrid.CopyItem";

        // 마지막으로 공통 항목을 주입한 외부 메뉴 — ContextMenuStrip 교체나
        // 그리드 Dispose 시 주입 항목(이 그리드를 캡처한 핸들러 포함)을 걷어내기
        // 위한 추적. 걷어내지 않으면 폐기된 그리드가 메뉴에 계속 붙잡혀 있게 된다.
        private System.Windows.Forms.ContextMenuStrip lastInjectedMenu;
        private double fallbackFontWidthRatio;

        /// <summary>행 선택이 바뀔 때 발생한다(WinForms 호환 이름).</summary>
        public event EventHandler SelectionChanged;

        /// <summary>
        /// 행 위에서 왼쪽 더블클릭할 때 발생한다 (2026-08-29 추가). 그 행이 먼저 현재 행(SelectedItem)으로
        /// 선택된 뒤 발생하므로 SelectedItem을 그대로 쓰면 된다 — "더블클릭 → 결정 패널" 관례용.
        /// 행 밖(헤더/빈 영역)에서는 발생하지 않는다.
        /// </summary>
        [Category("Modern")]
        [Description("Raised when a row is double-clicked with the left button; the row is selected first.")]
        public event EventHandler RowDoubleClick;

        /// <summary>
        /// 버튼 컬럼(GridColumnKind.Button) 셀을 클릭할 때 발생한다.
        /// e.Item이 클릭된 행(DataTable 소스면 DataRowView), e.DataPropertyName이
        /// 버튼 컬럼의 DataPropertyName이다.
        /// </summary>
        public event EventHandler<GridButtonClickEventArgs> CellButtonClick;

        /// <summary>
        /// 링크 컬럼(GridColumnKind.Link) 셀을 클릭할 때 발생한다.
        /// 클릭한 행이 먼저 현재 행(SelectedItem)으로 선택된 뒤 발생하며,
        /// e.Item이 클릭된 행(DataTable 소스면 DataRowView), e.DataPropertyName이
        /// 링크 컬럼의 DataPropertyName이다 — "Lot ID 클릭 → 이력 화면" 관례용.
        /// </summary>
        public event EventHandler<GridButtonClickEventArgs> CellLinkClick;

        /// <summary>
        /// 체크박스 컬럼 셀 값이 클릭으로 바뀔 때 발생한다 (내부 WPF 컨트롤의
        /// CellCheckChanged 재노출). 토글을 곧바로 저장/서버 반영하는 화면용.
        /// </summary>
        [Category("Modern")]
        [Description("Raised when a check-box column value changes.")]
        public event EventHandler<GridCheckChangedEventArgs> CellCheckChanged;

        /// <summary>
        /// 텍스트 셀 편집이 커밋되어 값이 실제로 바뀌었을 때 발생한다
        /// (ReadOnly = false인 편집 그리드 전용). e.Item이 편집된 행
        /// (DataTable 소스면 DataRowView), e.OldValue/e.NewValue가 변경 전후
        /// 값이다. 같은 값으로 확정하거나 Esc로 취소하면 발생하지 않는다.
        /// </summary>
        [Category("Modern")]
        [Description("Raised when a cell edit actually changed the value.")]
        public event EventHandler<GridCellValueChangedEventArgs> CellValueChanged;

        /// <summary>
        /// 텍스트 셀 편집을 커밋하기 직전에 발생한다 — e.Cancel = true로 두면
        /// 커밋이 취소되고 셀이 편집 상태로 남는다 (DataGridView.CellValidating
        /// 대응). 타입 오류(숫자 컬럼에 문자 입력 등)는 이 이벤트 없이 그리드가
        /// 스스로 막으므로(빨간 테두리 + 커밋 거부) 값 범위·중복 같은 업무 규칙
        /// 검증만 하면 된다.
        /// </summary>
        [Category("Modern")]
        [Description("Raised just before a cell edit is committed — set Cancel to reject the commit.")]
        public event EventHandler<GridCellValidatingEventArgs> CellValidating;

        /// <summary>높이 변화로 표시 가능 행 수(VisibleRowCapacity)가 바뀔 때 발생한다.</summary>
        public event EventHandler VisibleRowCapacityChanged;

        /// <summary>
        /// 컬럼 값 필터 상태가 바뀔 때(체크/해제/Clear) 발생한다 — 페이지
        /// 슬라이스를 바인딩하는 화면이 전체 결과에 같은 필터를 적용해
        /// (MatchesColumnFilters) 페이지를 재계산할 때 쓴다.
        /// </summary>
        public event EventHandler ColumnFiltersChanged;

        /// <summary>적절한 기본 크기로 컨트롤을 생성한다.</summary>
        public ModernDataGrid()
        {
            this.Size = new Size(480, 240);
            this.fallbackAutoGenerateColumns = true;
            this.fallbackReadOnly = true;
            this.fallbackAutoFitColumns = false;
            this.fallbackEnableColumnVirtualization = false;
            this.fallbackAllowColumnFilters = true;
            this.fallbackAllowFindPanel = true;
            this.fallbackAlternatingRowColors = false;
            this.fallbackShowStatusBar = false;
            this.fallbackStatusText = string.Empty;
            this.fallbackStatusCountFormat = "{0:N0} rows";
            this.fallbackRowColorMember = string.Empty;
            this.fallbackRowForegroundMember = string.Empty;

            if (this.Wpf != null)
            {
                this.Wpf.SelectionChanged += this.OnWpfSelectionChanged;
                this.Wpf.VisibleRowCapacityChanged += this.OnWpfVisibleRowCapacityChanged;
                this.Wpf.CellButtonClick += this.OnWpfCellButtonClick;
                this.Wpf.CellLinkClick += this.OnWpfCellLinkClick;
                this.Wpf.CellCheckChanged += this.OnWpfCellCheckChanged;
                this.Wpf.RowRightClick += this.OnWpfRowRightClick;
                this.Wpf.RowDoubleClick += this.OnWpfRowDoubleClick;
                this.Wpf.ColumnFiltersChanged += this.OnWpfColumnFiltersChanged;
                this.Wpf.CellValueChanged += this.OnWpfCellValueChanged;
                this.Wpf.CellValidating += this.OnWpfCellValidating;
            }
        }

        private void OnWpfCellValueChanged(object sender, GridCellValueChangedEventArgs e)
        {
            if (this.CellValueChanged != null)
            {
                this.CellValueChanged(this, e);
            }
        }

        private void OnWpfCellValidating(object sender, GridCellValidatingEventArgs e)
        {
            if (this.CellValidating != null)
            {
                this.CellValidating(this, e);
            }
        }

        private void OnWpfColumnFiltersChanged(object sender, EventArgs e)
        {
            if (this.ColumnFiltersChanged != null)
            {
                this.ColumnFiltersChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 필터 팝업의 고유 값 원천 — 페이지 슬라이스를 바인딩하는 화면은
        /// 전체 결과(DataTable)를 지정해 체크리스트에 전체 값이 나오게 한다.
        /// null이면 현재 DataSource에서 모은다.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object FilterValueSource
        {
            get
            {
                return this.Wpf != null ? this.Wpf.FilterValueSource : null;
            }
            set
            {
                if (this.Wpf != null)
                {
                    this.Wpf.FilterValueSource = value;
                }
            }
        }

        /// <summary>행(DataRow/DataRowView 등)이 현재 컬럼 필터를 전부
        /// 통과하는지 판정한다 (필터 없으면 항상 true).</summary>
        public bool MatchesColumnFilters(object item)
        {
            return this.Wpf == null || this.Wpf.MatchesColumnFilters(item);
        }

        /// <summary>값이 걸린 컬럼 필터(깔때기)가 하나라도 있는지.</summary>
        [Browsable(false)]
        public bool HasActiveColumnFilters
        {
            get { return this.Wpf != null && this.Wpf.HasActiveColumnFilters; }
        }

        /// <summary>
        /// 모든 컬럼의 값 필터를 한 번에 해제한다 — 우클릭 메뉴의
        /// "Clear Filters"와 동일. 화면의 초기화(Reset) 버튼에서 조회 조건과
        /// 함께 그리드 필터를 걷을 때도 쓴다.
        /// </summary>
        public void ClearColumnFilters()
        {
            if (this.Wpf != null)
            {
                this.Wpf.ClearColumnFilters();
            }
        }

        /// <summary>
        /// 모든 행을 선택한다 (DataGridView.SelectAll 대응) — 다중 선택
        /// (<see cref="MultiSelect"/> = true)일 때만 동작하고, 단일 선택
        /// 그리드에서는 무시된다. Ctrl+A 단축키와 동일.
        /// </summary>
        public void SelectAll()
        {
            if (this.Wpf != null)
            {
                this.Wpf.SelectAllRows();
            }
        }

        // 행 우클릭: WPF 쪽이 우클릭한 행을 현재 행으로 선택한 뒤 알려주면,
        // 표준 WinForms 방식대로 이 컨트롤에 지정된 ContextMenuStrip을 커서
        // 위치에 띄운다 — DataGridView + ContextMenuStrip 조합과 같은 사용법이다.
        //
        // 복사 항목은 **어느 그리드에서나** 우클릭으로 닿을 수 있어야 한다.
        // 폼이 자기 ContextMenuStrip을 지정했으면 그 메뉴 **맨 끝에** 구분선 +
        // 복사 항목을 붙여 띄우고, 지정하지 않았으면 복사 항목만 있는 기본 메뉴를
        // 띄운다. 폼마다 "복사 항목을 넣어라"라고 지시할 필요가 없다.
        private void OnWpfRowRightClick(object sender, EventArgs e)
        {
            if (this.ContextMenuStrip != null)
            {
                this.AttachGridMenuItems(this.ContextMenuStrip);
                this.ContextMenuStrip.Show(System.Windows.Forms.Control.MousePosition);
                return;
            }

            System.Windows.Forms.ContextMenuStrip menu = this.EnsureDefaultMenu();

            if (menu.Items.Count > 0)
            {
                menu.Show(System.Windows.Forms.Control.MousePosition);
            }
        }

        // 폼이 만든 메뉴 끝에 그리드 공통 항목(복사 2종 + 찾기)을 붙인다.
        //
        // 매번 **떼고 다시 붙인다**. 캐시하지 않는 이유:
        // (1) 여러 번 우클릭해도 항목이 늘어나지 않아야 한다(멱등).
        // (2) 하나의 ContextMenuStrip을 여러 그리드가 공유할 수 있다 — 그때 핸들러가
        //     먼저 붙인 그리드를 가리키면 다른 그리드에서 엉뚱한 행이 복사된다.
        //     열기 직전에 이 그리드 기준으로 다시 만들면 그 문제가 사라진다.
        // (3) 폼이 런타임에 자기 항목을 넣고 빼도 공통 항목은 항상 맨 끝에 온다.
        private void AttachGridMenuItems(System.Windows.Forms.ContextMenuStrip menu)
        {
            // 폼이 ContextMenuStrip을 런타임에 교체했으면 이전 메뉴에 남은
            // 공통 항목부터 걷어낸다.
            if (this.lastInjectedMenu != null && !object.ReferenceEquals(this.lastInjectedMenu, menu))
            {
                RemoveInjectedItems(this.lastInjectedMenu);
            }

            this.lastInjectedMenu = menu;
            RemoveInjectedItems(menu);

            bool hasConditionalItems = this.MultiSelect
                    || (this.AllowColumnFilters && this.HasActiveColumnFilters)
                    || this.FrozenColumnCount > 0
                    || (this.Wpf != null && this.Wpf.CurrentCellColumnIndex() >= 0);

            if (!this.showCopyMenu && !this.AllowFindPanel && !hasConditionalItems
                    && !this.ExportMenuAvailable())
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

            this.AppendCommonItems(menu);
        }

        // 공통 항목의 모양 — **복사 2종만 최상위**, 나머지(찾기·내보내기·표 조작)는
        // `More ▸` 하위 메뉴 하나로 접는다. 폼 액션 3개짜리 작은 표에도 항목이 8~9개
        // 쌓여 폼 액션이 눌리던 것을 줄인다(2026-08-29). 하위 메뉴가 비면 `More`도 없다.
        private void AppendCommonItems(System.Windows.Forms.ContextMenuStrip menu)
        {
            if (this.showCopyMenu)
            {
                menu.Items.Add(this.CreateCopyRowItem());
                menu.Items.Add(this.CreateCopyCellItem());
            }

            System.Windows.Forms.ToolStripMenuItem more = this.CreateMoreItem(menu);

            if (more != null)
            {
                menu.Items.Add(more);
            }
        }

        // `More ▸` — [Find… / Export to Excel…] ─ [Select all / Clear filters / Freeze / Unfreeze].
        // 하위 드롭다운은 부모 메뉴와 같은 렌더러로 그린다(ModernMenuRenderer를 꽂은 폼에서
        // 하위 메뉴만 시스템 모양이 되지 않게). 항목이 하나도 없으면 null.
        private System.Windows.Forms.ToolStripMenuItem CreateMoreItem(System.Windows.Forms.ContextMenuStrip menu)
        {
            System.Windows.Forms.ToolStripMenuItem more =
                    new System.Windows.Forms.ToolStripMenuItem("More");
            more.Tag = copyItemTag;

            if (this.AllowFindPanel)
            {
                more.DropDownItems.Add(this.CreateFindItem());
            }

            if (this.ExportMenuAvailable())
            {
                more.DropDownItems.Add(this.CreateExportItem());
            }

            int toolCount = more.DropDownItems.Count;
            this.AppendConditionalItems(more.DropDownItems);

            if (toolCount > 0 && more.DropDownItems.Count > toolCount)
            {
                more.DropDownItems.Insert(toolCount, new System.Windows.Forms.ToolStripSeparator());
            }

            if (more.DropDownItems.Count == 0)
            {
                more.Dispose();
                return null;
            }

            more.DropDown.Renderer = menu.Renderer;
            return more;
        }

        // 메뉴에서 그리드 공통 항목(표식 태그)을 떼어 Dispose한다.
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

        /// <summary>
        /// 외부 메뉴에 주입한 공통 항목과 내부 기본 메뉴를 정리한다 — 폐기된
        /// 그리드를 캡처한 Click 핸들러가 살아 있는 메뉴에 남지 않게 한다.
        /// </summary>
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

                // 조상 구독을 걷는다 — 살아 있는 조상의 이벤트가 폐기된
                // 그리드를 계속 붙들지 않게 한다 (compat의 Dispose 후 GC 검사).
                foreach (System.Windows.Forms.Control ancestor in this.watchedAncestors)
                {
                    ancestor.VisibleChanged -= this.OnAncestorVisibilityChanged;
                    ancestor.ParentChanged -= this.OnAncestorParentChanged;
                    ancestor.Layout -= this.OnAncestorLayout;
                }

                this.watchedAncestors.Clear();
            }

            base.Dispose(disposing);
        }

        // 상태에 따라 붙는 항목 — 다중 선택 그리드의 Select All, 필터가 걸려
        // 있을 때의 Clear Filters, 컬럼 고정/해제. 열기 직전에 만들므로 현재
        // 상태가 반영된다. `More ▸` 하위 메뉴의 항목 컬렉션에 붙인다.
        private void AppendConditionalItems(System.Windows.Forms.ToolStripItemCollection items)
        {
            if (this.MultiSelect)
            {
                items.Add(this.CreateSelectAllItem());
            }

            if (this.AllowColumnFilters && this.HasActiveColumnFilters)
            {
                items.Add(this.CreateClearFiltersItem());
            }

            // 컬럼 고정 — 우클릭이 클릭한 셀을 현재 셀로 갱신하므로(언더라인이
            // 그 셀에 보인다) "이 컬럼까지 고정"의 기준이 눈에 보인다.
            // 엑셀 틀 고정과 달리 **컬럼만** 고정된다 (행 고정은 미지원).
            if (this.Wpf != null && this.Wpf.CurrentCellColumnIndex() >= 0)
            {
                items.Add(this.CreateFreezeItem());
            }

            if (this.FrozenColumnCount > 0)
            {
                items.Add(this.CreateUnfreezeItem());
            }
        }

        private System.Windows.Forms.ToolStripMenuItem CreateFreezeItem()
        {
            System.Windows.Forms.ToolStripMenuItem item =
                    new System.Windows.Forms.ToolStripMenuItem("Freeze columns up to here");
            item.Tag = copyItemTag;
            item.Click += delegate
            {
                int index = this.Wpf == null ? -1 : this.Wpf.CurrentCellColumnIndex();

                if (index >= 0)
                {
                    this.FrozenColumnCount = index + 1;
                }
            };
            return item;
        }

        private System.Windows.Forms.ToolStripMenuItem CreateUnfreezeItem()
        {
            System.Windows.Forms.ToolStripMenuItem item =
                    new System.Windows.Forms.ToolStripMenuItem("Unfreeze columns");
            item.Tag = copyItemTag;
            item.Click += delegate { this.FrozenColumnCount = 0; };
            return item;
        }

        private System.Windows.Forms.ToolStripMenuItem CreateSelectAllItem()
        {
            System.Windows.Forms.ToolStripMenuItem item =
                    new System.Windows.Forms.ToolStripMenuItem("Select all");
            item.ShortcutKeyDisplayString = "Ctrl+A";
            item.Tag = copyItemTag;
            item.Click += delegate { this.SelectAll(); };
            return item;
        }

        private System.Windows.Forms.ToolStripMenuItem CreateClearFiltersItem()
        {
            System.Windows.Forms.ToolStripMenuItem item =
                    new System.Windows.Forms.ToolStripMenuItem("Clear filters");
            item.Tag = copyItemTag;
            item.Click += delegate { this.ClearColumnFilters(); };
            return item;
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

        private System.Windows.Forms.ToolStripMenuItem CreateFindItem()
        {
            System.Windows.Forms.ToolStripMenuItem item =
                    new System.Windows.Forms.ToolStripMenuItem("Find...");
            item.ShortcutKeyDisplayString = "Ctrl+F";
            item.Tag = copyItemTag;
            item.Click += delegate { this.ShowFindPanel(); };
            return item;
        }

        // 내보내기 항목은 컬럼 정의(ConfigureColumns)가 있는 그리드에만 붙는다 —
        // ExportXlsx가 컬럼 정의를 엑셀 컬럼의 단일 원천으로 쓰기 때문이다.
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

        /// <summary>
        /// 현재 행을 탭 구분 한 줄로 클립보드에 복사한다 (Ctrl+C와 같은 동작).
        /// 체크박스·버튼·스피너처럼 데이터가 아닌 컬럼은 제외된다.
        /// </summary>
        public void CopySelectedRow()
        {
            if (this.Wpf != null)
            {
                this.Wpf.CopyRowToClipboard();
            }
        }

        /// <summary>
        /// 현재 셀 하나의 표시 텍스트만 클립보드에 복사한다 (Ctrl+Shift+C와 같은 동작).
        /// </summary>
        public void CopyCurrentCell()
        {
            if (this.Wpf != null)
            {
                this.Wpf.CopyCellToClipboard();
            }
        }

        /// <summary>
        /// 우클릭 메뉴에 복사 항목(Copy row / Copy cell)을 넣을지 여부. 기본 true.
        ///
        /// <see cref="System.Windows.Forms.Control.ContextMenuStrip"/>을 지정한
        /// 그리드에서는 그 메뉴 **맨 끝에** 구분선과 함께 붙고, 지정하지 않은
        /// 그리드에는 복사 항목만 있는 메뉴가 뜬다. false로 두면 어느 쪽도 붙지
        /// 않는다(단축키 Ctrl+C / Ctrl+Shift+C는 이 값과 무관하게 동작한다).
        /// </summary>
        [Category("Modern")]
        [Description("Add copy items to the right-click menu (appended to an existing ContextMenuStrip)")]
        [DefaultValue(true)]
        public bool ShowCopyMenu
        {
            get { return this.showCopyMenu; }
            set { this.showCopyMenu = value; }
        }

        // 그리드 공통 항목(복사 2종 + 찾기)만 있는 기본 메뉴. 항목은 열 때마다
        // 다시 만든다 — ShowCopyMenu/AllowFindPanel 변경이 즉시 반영되게.
        // 항목의 단축키는 **표시만** 한다(ShortcutKeyDisplayString) — 실제 키 처리는
        // WPF 쪽이 하므로 ShortcutKeys로 등록하면 이중 처리가 된다.
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

            this.AppendCommonItems(this.copyMenu);

            return this.copyMenu;
        }

        /// <summary>
        /// 현재 높이에서 세로 스크롤 없이 표시 가능한 행 수 (최소 1).
        /// ModernPagination.PageSize에 넣으면 페이지 크기가 화면 높이를 따라간다 —
        /// VisibleRowCapacityChanged에서 갱신하면 리사이즈에도 자동 대응.
        /// </summary>
        [Browsable(false)]
        public int VisibleRowCapacity
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.VisibleRowCapacity;
                }

                return 1;
            }
        }

        private void OnWpfVisibleRowCapacityChanged(object sender, EventArgs e)
        {
            if (this.VisibleRowCapacityChanged != null)
            {
                this.VisibleRowCapacityChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 데이터 소스: DataTable, DataView, IList 또는 임의의 IEnumerable.
        /// 할당하면 선택이 초기화되고, 데이터가 있으면 첫 행이 선택되며,
        /// SelectionChanged가 한 번 발생한다. null을 할당하면 그리드를 비운다.
        /// DataTable/DataView 소스는 ConfigureColumns로 선언한 컬럼과
        /// RowColorMember가 없으면 빈 컬럼으로 자동 보장한다 — 폼에서 컬럼 목록을
        /// 다시 나열할 필요가 없다.
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
                this.ApplyAutoColumns(value);
                this.EnsureDeclaredColumns(value);

                if (this.Wpf == null)
                {
                    return;
                }

                // 복원용 포착은 선택을 지우기 **전에** 해야 한다.
                string[] keyMembers = this.RowKeyMemberPaths();
                string keepKey = keyMembers.Length == 0
                        ? string.Empty
                        : this.Wpf.SelectedRowKey(keyMembers);
                int keepIndex = keyMembers.Length == 0 ? -1 : this.Wpf.SelectedIndex;
                double keepOffset = keyMembers.Length == 0 ? 0d : this.Wpf.GetVerticalOffset();

                this.suppressSelectionChanged = true;

                try
                {
                    this.Wpf.SelectedItem = null;
                    this.Wpf.ItemsSource = DataSourceConverter.ToItemsSource(value);

                    if (this.Wpf.RowCount > 0)
                    {
                        // DataGridView와 동일: 바인딩 시 첫 행이 현재 행이 된다.
                        // RowKeyMember를 준 그리드는 보던 행을 되찾는다
                        // (docs/state-contracts/grid-selection-restore.md).
                        this.Wpf.SelectedIndex = this.ResolveRestoredIndex(keyMembers, keepKey, keepIndex);
                    }
                }
                finally
                {
                    this.suppressSelectionChanged = false;
                }

                this.RaiseSelectionChanged();

                // 스크롤 복원은 레이아웃 뒤라야 가능해 미뤄서 실행된다 — 선택은
                // 건드리지 않으므로 위 통지가 한 번인 것은 그대로다.
                if (keyMembers.Length > 0)
                {
                    this.Wpf.RestoreVerticalOffset(keepOffset);
                }
            }
        }

        // 콤마로 나눈 키 컬럼 목록. RowKeyMember가 비면 빈 배열 —
        // 그러면 포착·복원을 통째로 건너뛰어 기존 동작 그대로가 된다.
        private string[] RowKeyMemberPaths()
        {
            string members = this.RowKeyMember;

            if (string.IsNullOrEmpty(members))
            {
                return emptyKeyMembers;
            }

            List<string> paths = new List<string>();

            foreach (string part in members.Split(new char[] { ',', ';' }))
            {
                string trimmed = part.Trim();

                if (trimmed.Length > 0)
                {
                    paths.Add(trimmed);
                }
            }

            return paths.ToArray();
        }

        private static readonly string[] emptyKeyMembers = new string[0];

        // 폴백 순서: 키 일치 → (옵션) 같은 번호 → 첫 행.
        private int ResolveRestoredIndex(string[] keyMembers, string keepKey, int keepIndex)
        {
            if (keyMembers.Length == 0)
            {
                return 0;
            }

            int found = this.Wpf.IndexOfRowKey(keepKey, keyMembers);

            if (found >= 0)
            {
                return found;
            }

            if (!this.restoreSelectionByIndex || keepIndex < 0)
            {
                return 0;
            }

            int last = this.Wpf.RowCount - 1;

            return keepIndex > last ? last : keepIndex;
        }

        /// <summary>데이터 소스로부터 컬럼을 자동 생성할지 여부(WinForms 호환).</summary>
        [Category("Modern")]
        [Description("Auto-generate columns from the data source")]
        [DefaultValue(true)]
        public bool AutoGenerateColumns
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.AutoGenerateColumns;
                }

                return this.fallbackAutoGenerateColumns;
            }
            set
            {
                this.fallbackAutoGenerateColumns = value;

                if (this.Wpf != null)
                {
                    this.Wpf.AutoGenerateColumns = value;
                }
            }
        }

        /// <summary>
        /// 읽기 전용 여부 (기본 true — DataGridView와 달리 기본이 잠김이다;
        /// 기존 조회 화면이 코드 변경 없이 그대로 동작하게 하기 위해서다).
        /// false로 두면 텍스트 컬럼을 더블클릭/F2/타이핑으로 편집할 수 있다:
        /// - 숫자/날짜 컬럼은 표시 서식("N0", "yyyy-MM-dd") 그대로 편집되고,
        ///   타입에 안 맞는 입력은 빨간 테두리와 함께 커밋이 거부된다.
        /// - 키 컬럼처럼 잠글 컬럼은 컬럼 정의의 ReadOnly = true로 지정한다.
        /// - 값이 실제로 바뀌면 CellValueChanged, 커밋 직전엔 CellValidating이
        ///   발생한다. 저장 전에는 EndEdit()로 열려 있는 편집을 확정한다.
        /// </summary>
        [Category("Modern")]
        [Description("Read-only — false enables text-column cell editing (default true)")]
        [DefaultValue(true)]
        public bool ReadOnly
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.ReadOnly;
                }

                return this.fallbackReadOnly;
            }
            set
            {
                this.fallbackReadOnly = value;

                if (this.Wpf != null)
                {
                    this.Wpf.ReadOnly = value;
                }
            }
        }

        /// <summary>
        /// 다중 행 선택 허용 여부 (기본 false — DataGridView 기본과 반대다;
        /// 단일 선택 전제(SelectedItem)로 만든 기존 화면 보호). true면
        /// Shift+클릭(범위)/Ctrl+클릭(개별 토글)으로 여러 행을 고를 수 있고,
        /// 결과는 <see cref="SelectedItems"/>로 읽는다. Ctrl+C 행 복사는 선택된
        /// 행 전부를 여러 줄로 복사한다. <see cref="SelectedItem"/>/
        /// <see cref="SelectedIndex"/>는 현재 행(마지막 클릭) 의미 그대로다.
        /// </summary>
        [Category("Modern")]
        [Description("Allow multi-row selection — Shift/Ctrl+click, results in SelectedItems (default false)")]
        [DefaultValue(false)]
        public bool MultiSelect
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.MultiSelect;
                }

                return this.fallbackMultiSelect;
            }
            set
            {
                this.fallbackMultiSelect = value;

                if (this.Wpf != null)
                {
                    this.Wpf.MultiSelect = value;
                }
            }
        }

        /// <summary>
        /// 가로 스크롤 시 왼쪽에 고정할 컬럼 수 (기본 0 = 없음). 명시적 컬럼
        /// 정의 화면은 이 속성 대신 <c>ModernDataGridColumn.Frozen = true</c>
        /// (DataGridViewColumn.Frozen 대응)를 권장 — ConfigureColumns가 마지막
        /// Frozen 컬럼까지의 개수로 이 값을 자동 설정한다. 고정은 왼쪽부터
        /// 연속이다 (중간 컬럼만 고정하면 그 왼쪽까지 함께 고정).
        /// </summary>
        [Category("Modern")]
        [Description("Number of columns frozen on the left for horizontal scrolling (default 0; prefer Frozen=true on column definitions)")]
        [DefaultValue(0)]
        public int FrozenColumnCount
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.FrozenColumnCount;
                }

                return this.fallbackFrozenColumnCount;
            }
            set
            {
                this.fallbackFrozenColumnCount = value;

                if (this.Wpf != null)
                {
                    this.Wpf.FrozenColumnCount = value;
                }
            }
        }

        /// <summary>
        /// 선택된 행들의 복사본 — **현재 뷰 순서**(정렬·컬럼 필터 반영).
        /// DataTable 소스면 각 요소가 DataRowView다. 단일 선택 그리드에서는
        /// 0개 또는 1개짜리 배열이 온다 (기존 `SelectedRows` 순회 코드 대응).
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object[] SelectedItems
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.SelectedItemsInViewOrder();
                }

                return new object[0];
            }
        }

        /// <summary>
        /// 열려 있는 셀/행 편집을 확정한다 — 저장 버튼 처리 첫 줄에서 호출하면
        /// 편집 중이던 값이 DataTable에 반영된 뒤 GetChanges를 읽을 수 있다
        /// (DataGridView.EndEdit 대응).
        /// </summary>
        public void EndEdit()
        {
            if (this.Wpf != null)
            {
                this.Wpf.CommitPendingEdit();
            }
        }

        /// <summary>열려 있는 셀/행 편집을 버린다 (Esc와 동일).</summary>
        public void CancelEdit()
        {
            if (this.Wpf != null)
            {
                this.Wpf.CancelPendingEdit();
            }
        }

        /// <summary>
        /// 새 행을 추가하고 선택한다. 반환값은 새 행 객체(DataTable 소스면
        /// DataRowView — <c>row["COL"] = value</c>로 초기값을 넣는다).
        /// DataTable/DataView 또는 IBindingList 소스에서만 지원한다.
        /// </summary>
        public object AddRow()
        {
            return this.AddRow(null);
        }

        /// <summary>
        /// 새 행을 추가하고 초기값을 채운 뒤 선택한다 (DataGridView.Rows.Add
        /// 대응). values는 ConfigureColumns 선언 순서의 데이터 컬럼(Text/Badge/
        /// CheckBox/Combo/Link — Button·Spinner 같은 상태 컬럼 제외)에 차례로
        /// 들어가며, 남는 컬럼은 비워 둔다.
        /// </summary>
        public object AddRow(params object[] values)
        {
            if (this.Wpf == null)
            {
                return null;
            }

            object row = this.Wpf.AddRow();

            if (values != null && values.Length > 0)
            {
                System.Data.DataRowView rowView = row as System.Data.DataRowView;
                List<string> members = this.DataMemberNames();

                int count = Math.Min(values.Length, members.Count);

                for (int index = 0; index < count; index++)
                {
                    object value = values[index];

                    // DataRowView는 null 대신 DBNull을 요구한다.
                    if (value == null && rowView != null)
                    {
                        value = DBNull.Value;
                    }

                    Modern.Lab.Controls.Wpf.Common.MemberPathReader.Write(row, members[index], value);
                }

                if (rowView != null)
                {
                    rowView.EndEdit();
                }
            }

            // 새 행을 현재 행으로 — 보이도록 스크롤까지 (DataGridView 관례).
            this.Wpf.SelectedItem = row;
            return row;
        }

        /// <summary>
        /// 현재 선택된 행을 제거하고 이웃 행을 선택한다. DataTable/DataView
        /// 소스는 Delete로 표시되어 저장 로직(GetChanges)이 삭제를 그대로 본다.
        /// </summary>
        public void RemoveSelectedRow()
        {
            if (this.Wpf == null || this.Wpf.SelectedItem == null)
            {
                return;
            }

            int index = this.Wpf.SelectedIndex;

            this.Wpf.RemoveRow(this.Wpf.SelectedItem);

            int count = this.Wpf.RowCount;

            if (count > 0)
            {
                this.Wpf.SelectedIndex = Math.Min(Math.Max(index, 0), count - 1);
            }
        }

        /// <summary>행 하나를 소스에서 제거한다 (SelectedItem 등에서 얻은 행 객체).</summary>
        public void RemoveRow(object item)
        {
            if (this.Wpf != null)
            {
                this.Wpf.RemoveRow(item);
            }
        }

        // ConfigureColumns 선언 순서의 데이터 컬럼 이름 목록 — AddRow(values)의
        // 값 매핑 대상 (Button/Spinner 같은 상태 컬럼과 빈 이름은 제외).
        private List<string> DataMemberNames()
        {
            List<string> members = new List<string>();

            if (this.configuredColumns != null)
            {
                foreach (ModernDataGridColumn column in this.configuredColumns)
                {
                    if (column == null
                            || string.IsNullOrEmpty(column.DataPropertyName)
                            || column.Kind == GridColumnKind.Button
                            || column.Kind == GridColumnKind.Spinner)
                    {
                        continue;
                    }

                    members.Add(column.DataPropertyName);
                }
            }

            return members;
        }

        /// <summary>
        /// 컬럼 너비를 헤더 캡션과 데이터 내용 중 더 넓은 쪽에 맞춰 자동 계산할지
        /// 여부. ConfigureColumns로 정의한 컬럼에만 적용되며, 켜져 있으면
        /// DataSource가 바뀔 때마다 각 컬럼이 잘림 없이 표시되는 너비로
        /// 재계산된다 (컬럼 정의의 Width는 무시). 사용자가 마우스로 너비를
        /// 조절하는 것은 그대로 가능하다.
        /// </summary>
        [Category("Modern")]
        [Description("Auto-size column widths to header and data (ConfigureColumns columns only)")]
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

        /// <summary>
        /// 컬럼 가상화(기본 false) — WPF DataGrid의 EnableColumnVirtualization을 그대로 노출한다.
        /// 켜면 가로 뷰포트 밖 컬럼의 셀을 만들지 않아 컬럼이 많은 표의 첫 표시와 세로 스크롤이
        /// 빨라지고, 대신 가로로 크게 옮길 때 새 컬럼의 셀을 그때 만든다. 컬럼 폭이 절대값일 때만
        /// 효과가 있으므로(AutoFitColumns를 켠 표가 그 조건) 세로 목록 위주의 넓은 읽기 전용 표에서
        /// 화면 단위로 켠다. 실측과 조건은 docs/migration/ModernDataGrid.md.
        /// </summary>
        [Category("Modern")]
        [Description("Virtualize columns outside the horizontal viewport (WPF EnableColumnVirtualization). Effective only with absolute column widths.")]
        [DefaultValue(false)]
        public bool EnableColumnVirtualization
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.EnableColumnVirtualization;
                }

                return this.fallbackEnableColumnVirtualization;
            }
            set
            {
                this.fallbackEnableColumnVirtualization = value;

                if (this.Wpf != null)
                {
                    this.Wpf.EnableColumnVirtualization = value;
                }
            }
        }

        /// <summary>
        /// 컬럼 값 필터(헤더 깔때기) 사용 여부 (기본 true). ConfigureColumns로
        /// 정의한 텍스트/배지 컬럼 헤더에 깔때기 버튼이 붙고, 클릭 시 고유 값
        /// 체크리스트로 행을 거른다 (엑셀식 값 필터 — 화면 뷰에만 적용되고
        /// 원본 데이터는 그대로다). 필터 선택은 DataSource 재할당(재조회)
        /// 후에도 유지된다. 어울리지 않는 그리드는 false로 끈다.
        /// </summary>
        [Category("Modern")]
        [Description("Enable the column-header funnel value filter (text/badge columns, default true)")]
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
        /// 찾기(Ctrl+F) 사용 여부 (기본 true). 엑셀의 찾기 대화상자 방식 —
        /// 행을 거르는 필터가 아니라 **일치 위치로 이동**한다. Ctrl+F(또는
        /// 우클릭 메뉴의 Find...)로 그리드 오른쪽 위에 **소유된 모덜리스 찾기
        /// 창**이 열린다 — 호스트 창 위에만 항상 표시(다른 앱 위로는 안 감),
        /// 최소화 동행, 작업표시줄/Alt-Tab 미표시, 제목줄 드래그로 이동(위치는
        /// 세션 동안 유지). Find Next(검색창 Enter/F3)는 다음 일치 셀로
        /// 선택·스크롤 이동(끝나면 처음부터), Shift+F3/Shift+Enter는 이전 일치,
        /// Find All은 일치 목록을 나열하며 항목을 클릭하면 그 셀로 이동한다.
        /// 화면에 보이는 일치 텍스트는 노란 배경으로 강조된다.
        /// 대상은 텍스트를 담은 컬럼(Text/Link/Badge), 화면 표시 텍스트 기준
        /// 부분 일치·대소문자 무시. Esc/✕로 닫고, 열린 채 Ctrl+F는 검색창 재포커스.
        /// </summary>
        [Category("Modern")]
        [Description("Enable Ctrl+F find — Excel-style owned modeless find window (match highlight + Find Next/Find All, default true)")]
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

        /// <summary>찾기 창을 열고(열려 있으면 활성화) 검색어 입력에 포커스를 준다 (Ctrl+F와 동일).</summary>
        public void ShowFindPanel()
        {
            if (this.Wpf != null)
            {
                this.Wpf.ShowFindPanel();
            }
        }

        /// <summary>찾기 창을 닫고 일치 강조를 걷는다 (Esc와 동일).</summary>
        public void HideFindPanel()
        {
            if (this.Wpf != null)
            {
                this.Wpf.HideFindPanel();
            }
        }

        /// <summary>
        /// 실효 가시성이 꺼지면 찾기 창을 함께 내린다 (자기 자신이 직접 숨겨지는 경우).
        /// </summary>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (!this.Visible)
            {
                this.HideFindPanel();
            }
        }

        // ===== 조상 가시성 감시 — 찾기 창 연동 (2026-08-07 결함) =====
        //
        // 셸/탭/MDI에서 다른 메뉴로 전환하면 **조상 컨테이너만** 숨는다. 이때
        // 그리드의 Visible 값은 false가 되지만 WinForms는 자손에게 VisibleChanged를
        // 전파하지 않고(실측 — 이벤트 미발화), WPF 쪽 IsVisibleChanged(그리드 hwnd
        // 기준)도 오지 않아 찾기 창이 화면에 남는다. 조상 체인 전체의
        // VisibleChanged/ParentChanged를 직접 구독하는 것이 모든 컨테이너 구도를
        // 덮는 유일한 지점이다. 재부모화(폼이 나중에 셸에 붙는 경우)에 대비해
        // 조상의 ParentChanged에서 체인을 다시 짠다.

        private readonly List<System.Windows.Forms.Control> watchedAncestors =
                new List<System.Windows.Forms.Control>();

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            this.RewireAncestorVisibilityWatch();
        }

        private void RewireAncestorVisibilityWatch()
        {
            foreach (System.Windows.Forms.Control ancestor in this.watchedAncestors)
            {
                ancestor.VisibleChanged -= this.OnAncestorVisibilityChanged;
                ancestor.ParentChanged -= this.OnAncestorParentChanged;
                ancestor.Layout -= this.OnAncestorLayout;
            }

            this.watchedAncestors.Clear();

            System.Windows.Forms.Control node = this.Parent;

            while (node != null)
            {
                this.watchedAncestors.Add(node);
                node.VisibleChanged += this.OnAncestorVisibilityChanged;
                node.ParentChanged += this.OnAncestorParentChanged;
                node.Layout += this.OnAncestorLayout;
                node = node.Parent;
            }
        }

        private void OnAncestorVisibilityChanged(object sender, EventArgs e)
        {
            if (!this.Visible)
            {
                this.HideFindPanel();
            }
        }

        private void OnAncestorParentChanged(object sender, EventArgs e)
        {
            this.RewireAncestorVisibilityWatch();

            if (!this.Visible)
            {
                this.HideFindPanel();
            }
        }

        // ===== z-순서 가림 감시 — 찾기 창 연동 (2026-08-25 현장 보고) =====
        //
        // 셸의 홈 버튼처럼 화면을 숨기지 않고 **형제 화면을 앞으로 가져와 덮는**
        // 전환은 어떤 가시성/재부모화 이벤트도 만들지 않는다 — 그리드의 Visible은
        // 계속 true인데 소유된 최상위 창인 찾기 창만 홈 위에 남는다. 형제
        // z-순서 변경(BringToFront = SetChildIndex)은 그 부모의 Layout 이벤트를
        // 발화시키고, 그 부모는 반드시 조상 체인 안에 있으므로 조상 Layout에서
        // "완전히 가려졌는가"를 실측(GetClipBox)해 찾기 창을 내린다.

        private void OnAncestorLayout(object sender, System.Windows.Forms.LayoutEventArgs e)
        {
            if (!this.IsFindPanelOpen)
            {
                return;
            }

            if (this.Visible && this.IsFullyObscured())
            {
                this.HideFindPanel();
            }
        }

        // 완전 가림 판정: 형제 클리핑만 반영한 DC(자식 클리핑 제외 — ElementHost는
        // WPF 자식 hwnd가 전체를 덮어 DCX_USESTYLE로는 항상 빈 영역이 된다)의
        // 클립 박스가 NULLREGION이면 화면에 한 픽셀도 보이지 않는 상태다.
        private bool IsFullyObscured()
        {
            if (!this.IsHandleCreated)
            {
                return false;
            }

            IntPtr dc = GetDCEx(this.Handle, IntPtr.Zero, DcxCache | DcxClipSiblings);

            if (dc == IntPtr.Zero)
            {
                return false;
            }

            try
            {
                NativeRect box;
                return GetClipBox(dc, out box) == NullRegionComplexity;
            }
            finally
            {
                ReleaseDC(this.Handle, dc);
            }
        }

        private const int DcxCache = 0x2;
        private const int DcxClipSiblings = 0x10;
        private const int NullRegionComplexity = 1;

        [System.Runtime.InteropServices.StructLayout(
                System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct NativeRect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetDCEx(IntPtr hWnd, IntPtr hrgnClip, int flags);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern int GetClipBox(IntPtr hdc, out NativeRect rect);

        /// <summary>찾기 창이 열려 있는지 (읽기 전용 런타임 상태 — 자체 검사용).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsFindPanelOpen
        {
            get { return this.Wpf != null && this.Wpf.IsFindPanelOpen; }
        }

        /// <summary>
        /// 교차 행 배경(줄무늬) 표시 여부. 켜면 홀수 행이 테마 교차색
        /// (Brush.GridRowAlt)으로 칠해진다. 기본 false — 수평 구분선만으로 행이
        /// 구분되므로 줄무늬는 행이 많고 가로로 긴 그리드에서만 켜는 것을 권장.
        /// </summary>
        [Category("Modern")]
        [Description("Show alternating row background (stripes) — odd rows use the theme alternate color")]
        [DefaultValue(false)]
        public bool AlternatingRowColors
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.AlternatingRowColors;
                }

                return this.fallbackAlternatingRowColors;
            }
            set
            {
                this.fallbackAlternatingRowColors = value;

                if (this.Wpf != null)
                {
                    this.Wpf.AlternatingRowColors = value;
                }
            }
        }

        /// <summary>그리드 하단 상태바 표시 여부 (행 수 자동 표기 + StatusText).</summary>
        [Category("Modern")]
        [Description("Show the grid status bar — row count on the left, StatusText on the right")]
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

        /// <summary>상태바 오른쪽에 표시할 자유 텍스트 (선택 대상·조회 조건 등).</summary>
        [Category("Modern")]
        [Description("Free text shown on the right of the status bar")]
        [Localizable(true)]
        [DefaultValue("")]
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

        /// <summary>상태바 왼쪽 행 수 표기 형식. {0}에 현재 행 수가 들어간다.</summary>
        [Category("Modern")]
        [Description("Status bar row count format — {0} is the current row count")]
        [Localizable(true)]
        [DefaultValue("{0:N0} rows")]
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

        /// <summary>첫 표시 열의 셀 모서리에 짧은 표식을 겹쳐 그린다. 컬럼을 추가하지 않는다.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<object, string> RowMarkerSelector
        {
            get { return this.Wpf == null ? this.fallbackRowMarkerSelector : this.Wpf.RowMarkerSelector; }
            set
            {
                this.fallbackRowMarkerSelector = value;
                if (this.Wpf != null) { this.Wpf.RowMarkerSelector = value; }
            }
        }

        /// <summary>항목으로 행 색을 계산한다. 컬럼을 추가하지 않으며 선택·호버 색이 우선한다.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<object, string> RowColorSelector
        {
            get { return this.Wpf == null ? this.fallbackRowColorSelector : this.Wpf.RowColorSelector; }
            set
            {
                this.fallbackRowColorSelector = value;
                if (this.Wpf != null) { this.Wpf.RowColorSelector = value; }
            }
        }

        /// <summary>현재 정렬·필터 순서에서 첫 일치 행으로 선택·스크롤한다. 대상이 없으면 선택을 유지한다.</summary>
        public bool SelectRow(Func<object, bool> predicate)
        {
            return this.Wpf != null && this.Wpf.SelectRow(predicate);
        }

        /// <summary>
        /// 행 배경색으로 쓸 컬럼/속성 이름 (선택 사항). 값은 "#FEE2E2" 같은 색
        /// 문자열 또는 색 이름. 비었거나 해석 불가한 행은 기본 배경을 유지한다.
        /// 상태(Scrap 등)에 따라 행을 색으로 구분할 때 쓴다.
        /// </summary>
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
            }
        }

        /// <summary>
        /// 행 글자색으로 쓸 컬럼/속성 이름 (선택 사항). 값은 "#DC2626" 같은 색
        /// 문자열 또는 색 이름. 비었거나 해석 불가한 행은 기본 글자색을 유지한다.
        /// 판정 FAIL 행처럼 한 행 전체의 글자를 색으로 구분할 때 쓴다 — 배경을
        /// 칠하는 RowColorMember와 같은 문법(2026-08-29).
        /// </summary>
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

        /// <summary>
        /// 장평(글자 가로 비율) 재정의. 0 = 전역(ModernTheme.FontWidthRatio) 사용,
        /// 양수는 0.8~1.2로 클램프. 셀/헤더/배지/버튼 캡션/상태바 텍스트와
        /// AutoFitColumns 측정에 반영된다.
        /// </summary>
        [Category("Modern")]
        [Description("Font width ratio override — 0 = use global (ModernTheme.FontWidthRatio), allowed 0.8~1.2")]
        [DefaultValue(0d)]
        public override double FontWidthRatio
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.FontWidthRatio;
                }

                return this.fallbackFontWidthRatio;
            }
            set
            {
                this.fallbackFontWidthRatio = value;

                if (this.Wpf != null)
                {
                    // 그리드는 자체 DP 경로를 쓴다 — 값이 바뀌면 컬럼을 새 비율로
                    // 다시 만들고, 첨부 속성(상태바 등 XAML 텍스트)도 함께 갱신된다.
                    this.Wpf.FontWidthRatio = value;
                }
            }
        }

        /// <summary>현재 표시 중인 행 수(WinForms 호환 이름).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int RowCount
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.RowCount;
                }

                return 0;
            }
        }

        /// <summary>현재 선택된 행 항목(DataTable 소스의 경우 DataRowView).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
            set
            {
                if (this.Wpf != null)
                {
                    this.Wpf.SelectedItem = value;
                }
            }
        }

        /// <summary>선택된 행의 인덱스(아무것도 선택되지 않았으면 -1).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.SelectedIndex;
                }

                return -1;
            }
            set
            {
                if (this.Wpf != null)
                {
                    this.Wpf.SelectedIndex = value;
                }
            }
        }

        /// <summary>
        /// ConfigureColumns로 선언한 컬럼 정의의 복사본. 화면 그리드와 동일한
        /// 컬럼 구성(순서·캡션·형식)으로 파생 출력(엑셀 내보내기 등)을 만들 때
        /// 단일 원천으로 쓴다. 선언 전이면 빈 배열.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ModernDataGridColumn[] ColumnDefinitions
        {
            get
            {
                if (this.configuredColumns == null)
                {
                    return new ModernDataGridColumn[0];
                }

                ModernDataGridColumn[] copy = new ModernDataGridColumn[this.configuredColumns.Length];
                this.configuredColumns.CopyTo(copy, 0);
                return copy;
            }
        }

        /// <summary>
        /// 데이터가 0건일 때 데이터 영역 가운데에 표시할 안내 문구.
        /// 기본 "No data" — 화면 문맥에 맞게 바꾸거나("품번으로 조회하세요" 등)
        /// 빈 문자열로 끈다.
        /// </summary>
        [Category("Modern")]
        [Description("Message shown when there is no data (empty string = hidden)")]
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

        /// <summary>
        /// 행을 알아보는 컬럼 — 지정하면 <see cref="DataSource"/> 재할당 시
        /// **보던 행과 보던 구간을 되찾는다**(재조회마다 첫 행으로 튀지 않는다).
        /// 여러 컬럼이 있어야 행이 유일해지면 **콤마로 나열**한다
        /// (`"EQP_ID,PORT_SLOT"`).
        ///
        /// 비워 두면(기본) 기존 동작 그대로 — 재할당 시 첫 행이 선택된다.
        /// 새 데이터에 그 키가 없으면 <see cref="RestoreSelectionByIndex"/>에 따라
        /// 같은 번호 행 또는 첫 행으로 떨어진다.
        /// 계약: docs/state-contracts/grid-selection-restore.md
        /// </summary>
        [Category("Modern")]
        [Description("Column(s) identifying a row — restores selection and scroll on DataSource reassign (comma-separated for a composite key)")]
        [DefaultValue("")]
        public string RowKeyMember
        {
            get
            {
                return this.rowKeyMember;
            }
            set
            {
                this.rowKeyMember = value ?? string.Empty;
            }
        }

        /// <summary>
        /// 키가 새 데이터에 없을 때 **같은 행 번호**를 유지할지 (기본 false = 첫 행).
        /// 처리된 항목이 목록에서 빠지는 화면에서 "있던 자리"를 지키는 용도다.
        /// <see cref="RowKeyMember"/>를 지정해야 의미가 있다.
        /// </summary>
        [Category("Modern")]
        [Description("When the key is gone from the new data, keep the same row position instead of selecting the first row")]
        [DefaultValue(false)]
        public bool RestoreSelectionByIndex
        {
            get
            {
                return this.restoreSelectionByIndex;
            }
            set
            {
                this.restoreSelectionByIndex = value;
            }
        }

        /// <summary>
        /// 화면 컬럼 정의(ConfigureColumns)를 그대로 엑셀 컬럼으로 써서 데이터
        /// 전체를 .xlsx로 저장한다 — 내보내기 컬럼/헤더 목록을 폼이 따로 관리하지
        /// 않는다. CheckBox/Button 컬럼은 제외되고, Format은 화면 표시와 같은
        /// 규칙으로 적용된다. 데이터는 그리드 DataSource가 아니라 인자로 받는다 —
        /// 페이지 화면(그리드에는 현재 페이지 조각만 바인딩)에서도 전체가 나간다.
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

            Modern.Lab.Export.GridXlsxExporter.Write(path, sheetName, this.configuredColumns, data);
        }

        /// <summary>
        /// 우클릭 메뉴에 "Export to Excel..." 항목을 넣을지 여부 (기본 true).
        /// 컬럼 정의(ConfigureColumns)가 있는 그리드에만 항목이 나타난다.
        /// 화면의 Excel 버튼(폼이 파일명·알림까지 다듬는 경로)과 별개로,
        /// 그리드 어디서든 우클릭 한 번으로 그 그리드를 저장하는 보조 경로다.
        /// </summary>
        [Category("Modern")]
        [Description("Add Export to Excel to the right-click menu (ConfigureColumns grids only, default true)")]
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
        /// 우클릭 내보내기가 저장할 데이터를 해석한다 — <b>전체 결과 우선</b>:
        /// ① <see cref="FilterValueSource"/>(페이지 화면이 깔때기 체크리스트용으로
        /// 넣어 둔 조회 결과 전체) → ② <see cref="DataSource"/>가 DataTable이면
        /// 그대로 → ③ DataView는 표로 변환 → ④ DataRow/DataRowView 목록(페이지
        /// 조각 등)은 원본 스키마로 복제해 담는다. 해석 불가면 null.
        /// </summary>
        /// <returns>저장할 데이터 (없으면 null).</returns>
        public System.Data.DataTable ResolveExportData()
        {
            System.Data.DataTable full = this.FilterValueSource as System.Data.DataTable;

            if (full != null)
            {
                return full;
            }

            System.Data.DataTable table = this.dataSource as System.Data.DataTable;

            if (table != null)
            {
                return table;
            }

            System.Data.DataView view = this.dataSource as System.Data.DataView;

            if (view != null)
            {
                return view.ToTable();
            }

            System.Collections.IEnumerable items = this.dataSource as System.Collections.IEnumerable;

            if (items == null)
            {
                return null;
            }

            System.Data.DataTable copy = null;

            foreach (object item in items)
            {
                System.Data.DataRowView rowView = item as System.Data.DataRowView;
                System.Data.DataRow row = rowView != null ? rowView.Row : item as System.Data.DataRow;

                if (row == null)
                {
                    return null;   // DataRow 계열이 아닌 소스는 해석하지 않는다
                }

                if (copy == null)
                {
                    copy = row.Table.Clone();
                }

                copy.ImportRow(row);
            }

            return copy;
        }

        /// <summary>
        /// 저장 대화상자를 띄워 이 그리드를 엑셀로 저장한다 — 우클릭 메뉴
        /// "Export to Excel..."과 동일 동작. 데이터는 <see cref="ResolveExportData"/>
        /// 규칙으로 해석하고(페이지 화면도 전체 결과), 시트/파일명은
        /// <see cref="ExportSheetName"/>/<see cref="ExportFileNamePrefix"/>를 쓴다.
        /// 폼 차원의 알림(토스트)·화면별 파일명이 필요하면 이 경로 대신 폼의
        /// 내보내기 헬퍼(예: ModernFormBase.ExportToExcel)를 쓴다.
        /// </summary>
        /// <returns>실제로 저장했으면 true (취소·빈 데이터·실패는 false).</returns>
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
                    // 파일이 열려 있거나 경로 권한이 없는 경우 등 — 화면은 죽지 않는다.
                    System.Windows.Forms.MessageBox.Show(
                            owner, "Export failed: " + exception.Message, "Export to Excel",
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        /// <summary>
        /// 컬럼을 명시적 정의로 교체하고 자동 생성을 끈다.
        /// DataSource 할당 전에 호출한다.
        /// </summary>
        public void ConfigureColumns(params ModernDataGridColumn[] columns)
        {
            if (!this.applyingColumnBuilder)
            {
                this.columnBuilder = null;
            }

            this.SetColumns(columns, false);
        }

        /// <summary>
        /// 컬럼을 **표에서 만드는 함수**로 선언한다 — <c>grid.ConfigureColumns(DurableColumns)</c>처럼 화면의 <c>.Grid.cs</c>에 있는
        /// <c>GridColumns XxxColumns(DataTable)</c>를 그대로 넘긴다(2026-10-19). 부르는 즉시 null 표로 헤더를 만들어 조회 전에도
        /// 컬럼이 보이고, 그 뒤 <see cref="DataSource"/>가 올 때마다 그 표로 함수를 다시 불러 컬럼을 맞춘다 — 구성이 같으면 다시
        /// 만들지 않아 값 필터·찾기 상태가 유지되고, <c>null</c>을 대입할 때는 지금 컬럼을 그대로 둔다. 폼은 <c>DataSource = 표</c>만
        /// 대입하면 된다. 배열을 받는 <see cref="ConfigureColumns(ModernDataGridColumn[])"/>를 다시 부르면 함수는 해제된다.
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

            // null 대입(비우기)은 컬럼을 건드리지 않는다 — 비웠다 다시 채워도 값 필터·찾기 상태가 남는다.
            // 아직 컬럼이 없을 때만 null 표로 헤더를 만든다.
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

        private void SetColumns(ModernDataGridColumn[] columns, bool auto)
        {
            this.fallbackAutoGenerateColumns = false;
            this.configuredColumns = columns;
            this.autoColumns = auto;

            if (this.Wpf != null)
            {
                this.Wpf.ApplyColumns(columns);
            }
        }

        /// <summary>
        /// 컬럼을 선언하지 않은 그리드는 **데이터가 컬럼을 정한다** — 조회 결과의 컬럼이
        /// 그대로 표가 되고, 캡션은 용어사전이 푼다(규칙은 <see cref="AutoColumns"/>).
        /// 폼에서 컬럼 목록을 다시 나열할 필요가 없다.
        ///
        /// <c>ConfigureColumns</c>를 한 번이라도 부르면 그쪽이 이긴다 — 화면이 정한
        /// 컬럼을 데이터가 덮지 않는다. 자동으로 나온 컬럼은 조회 결과의 컬럼 구성이
        /// 달라질 때만 다시 만든다(같으면 그대로 둬 값 필터·찾기 상태가 풀리지 않는다).
        /// </summary>
        private void ApplyAutoColumns(object value)
        {
            if (this.configuredColumns != null && !this.autoColumns)
            {
                return;
            }

            ModernDataGridColumn[] generated = AutoColumns.Of(value);

            if (generated.Length == 0 || SameMembers(this.configuredColumns, generated))
            {
                return;
            }

            this.SetColumns(generated, true);
        }

        private static bool SameMembers(ModernDataGridColumn[] current, ModernDataGridColumn[] other)
        {
            if (current == null || other == null || current.Length != other.Length)
            {
                return false;
            }

            for (int index = 0; index < current.Length; index++)
            {
                if (current[index].DataPropertyName != other[index].DataPropertyName)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 선언된 컬럼(DataPropertyName)과 RowColorMember를 DataTable/DataView 소스에
        /// 빈 컬럼으로 보장한다. JSON→DataTable 변환은 값이 전부 null인 컬럼을 만들지
        /// 않으므로(서버가 null 키 생략), 그리드가 자기 정의로 직접 보장해 폼별
        /// EnsureColumns 하드코딩을 없앤다.
        /// </summary>
        private void EnsureDeclaredColumns(object value)
        {
            List<string> members = new List<string>();

            if (this.configuredColumns != null)
            {
                foreach (ModernDataGridColumn column in this.configuredColumns)
                {
                    if (column != null)
                    {
                        members.Add(column.DataPropertyName);
                    }
                }
            }

            members.Add(this.RowColorMember);
            members.Add(this.RowForegroundMember);

            // 키 컬럼은 화면에 표시되지 않을 수도 있다(포트 표의 EQP_ID) —
            // 선언 컬럼과 별개로 보장해야 MemberPathReader가 값을 읽을 수 있다.
            foreach (string key in this.RowKeyMemberPaths())
            {
                members.Add(key);
            }

            DataSourceConverter.EnsureColumns(value, members);
        }

        private void OnWpfCellCheckChanged(object sender, GridCheckChangedEventArgs e)
        {
            if (this.CellCheckChanged != null)
            {
                this.CellCheckChanged(this, e);
            }
        }

        private void OnWpfCellButtonClick(object sender, GridButtonClickEventArgs e)
        {
            if (this.CellButtonClick != null)
            {
                this.CellButtonClick(this, e);
            }
        }

        private void OnWpfCellLinkClick(object sender, GridButtonClickEventArgs e)
        {
            if (this.CellLinkClick != null)
            {
                this.CellLinkClick(this, e);
            }
        }

        private void OnWpfRowDoubleClick(object sender, EventArgs e)
        {
            if (this.RowDoubleClick != null)
            {
                this.RowDoubleClick(this, EventArgs.Empty);
            }
        }

        private void OnWpfSelectionChanged(object sender, EventArgs e)
        {
            if (!this.suppressSelectionChanged)
            {
                this.RaiseSelectionChanged();
            }
        }

        private void RaiseSelectionChanged()
        {
            if (this.SelectionChanged != null)
            {
                this.SelectionChanged(this, EventArgs.Empty);
            }
        }
    }
}
