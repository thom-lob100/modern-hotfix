using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using Modern.Lab.Controls.Wpf.Common;
using Modern.Lab.Controls.Wpf.Input;

namespace Modern.Lab.Controls.Wpf.Data
{
    /// <summary>
    /// ModernDataGridControl의 컬럼 값 필터 컨트롤러 — AllowColumnFilters가
    /// 켜지면 텍스트/배지 컬럼에 GridFilterAttach.Filterable을 설정해 헤더
    /// 템플릿의 깔때기 버튼(헤더 셀 맨 오른쪽 고정, 정렬 글리프 바로 다음)이
    /// 나타나고, 클릭 시 그 컬럼의 고유 값 체크리스트 팝업이 **헤더 셀 바로
    /// 아래, 컬럼 왼쪽에 맞춰** 열리고 검색창에 커서가 간다. 팝업의 검색 입력은
    /// 초성 매칭으로 체크리스트를 좁히고 선택도 그 결과로 맞춘다(별도 제안
    /// 드롭다운 없음).
    /// 체크는 팝업 안에서만 모이고 **Apply(또는 Enter)를 눌러야 반영**된다(엑셀식) —
    /// 즉시 반영하면 페이징 화면의 재바인딩이 팝업을 닫아 여러 값을 이어서
    /// 체크할 수 없기 때문이다. Cancel·Esc·바깥 클릭으로 닫으면 변경은 버려지고,
    /// Clear는 그 컬럼 필터를 즉시 해제하고 닫는다. 필터가 걸린 컬럼은
    /// 깔때기가 액센트색(IsActive)으로 칠해진다.
    ///
    /// 값 목록은 항목 모델 + 가상화 목록(VirtualizingStackPanel, Recycling)이라 고유
    /// 값이 수천 개여도 전부 싣는다(상한 없음). 표시 글자는 셀과 같은 서식(Format ·
    /// 셀 문화권)이고 숫자·날짜는 값 순서, (Blanks)는 맨 끝이다. 표시 글자가 같은
    /// 값들은 한 줄로 묶는다. **필터 키는 표시 글자가 아니라 원래 값의 문자열**
    /// (selections)이고 일치 판정도 그 키로 한다.
    ///
    /// 필터는 데이터 소스를 바꾸지 않고 **뷰에만** 적용한다:
    /// - DataTable/DataView 소스(BindingListCollectionView)는 CustomFilter
    ///   식으로, 그 외 IList 소스는 Filter 조건자로 거른다.
    /// - 상태바 행 수/빈 안내(EmptyText)는 뷰 기준이라 자동으로 따라온다.
    /// - 선택 상태는 컬럼 이름 기준으로 유지되어 DataSource 재할당(재조회)
    ///   후에도 같은 필터가 다시 적용된다.
    /// </summary>
    internal sealed class GridFilterController
    {
        // 헤더 셀 아래 간격 — 콤보 드롭다운과 같은 2px. 카드 Margin 은 팝업 배치가 상쇄하므로 VerticalOffset 으로 띄운다.
        private const double popupGap = 2d;

        // 팝업 폭 — 컬럼 폭을 따르되 이 범위로 묶는다.
        private const double popupMinWidth = 250d;
        private const double popupMaxWidth = 360d;

        // 값 목록 최대 높이 — 항목 한 줄(약 28) 열 줄.
        private const double listMaxHeight = 280d;

        // 팝업 버튼 높이·글자 크기 — 체크리스트와 같은 12px 밀도.
        private const double buttonHeight = 28d;
        private const double buttonFontSize = 12d;
        private const double buttonWidth = 64d;

        private readonly DataGrid grid;
        private readonly FrameworkElement resourceSource;
        private readonly Func<IEnumerable> itemsSourceGetter;

        // 컬럼 이름 → 선택된 값 집합. 항목이 없으면 그 컬럼은 필터 없음(전체).
        private readonly Dictionary<string, HashSet<string>> selections =
                new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        // 필터 대상 컬럼 → 데이터 컬럼 이름 (팝업 대상 해석 + 활성 표시 갱신용).
        private readonly Dictionary<DataGridColumn, string> filterColumns =
                new Dictionary<DataGridColumn, string>();

        // 데이터 컬럼 이름 → 표시 서식(Format) — 팝업 값 글자를 셀과 같게 만든다.
        private readonly Dictionary<string, string> columnFormats =
                new Dictionary<string, string>(StringComparer.Ordinal);

        // 재사용 팝업과 현재 팝업이 다루는 컬럼/요소.
        private Popup popup;
        private string popupColumn;
        private ModernTextBoxControl popupSearch;
        private CheckBox popupAll;
        private ListBox popupList;
        private TextBlock popupCount;
        private ModernButtonControl popupApply;

        // 팝업의 전체 값 항목과 지금 검색으로 보이는 항목.
        private List<FilterValueItem> popupItems;
        private List<FilterValueItem> popupVisible;

        // 팝업을 열 때의 확정 선택 — 지금 데이터에 없는 키를 Apply 뒤에도 남기는 데 쓴다.
        private HashSet<string> popupOriginal;
        private bool popupKeepAbsent;

        // 버튼·키로 닫을 때만 표로 포커스를 돌려준다 — 바깥 클릭은 클릭한 곳이 포커스를 갖는다.
        private bool returnFocusOnClose;

        /// <summary>소유자가 직접 거르는 모드 — 켜면 뷰 필터를 설치하지 않는다.</summary>
        internal bool ExternalFiltering;

        private ICollectionView filteredView;
        private Predicate<object> ownedFilter;
        private ViewFilter filterLease;

        // 편집 중인 외부 뷰는 Filter 교체가 금지된다. 연결만 끊어도 그리드는 수거될 수 있어야 한다.
        private sealed class ViewFilter
        {
            internal GridFilterController Owner;

            internal bool Matches(object item)
            {
                return this.Owner == null || this.Owner.MatchesAllFilters(item);
            }
        }

        // 팝업 값 목록의 한 줄 — 표시 글자 하나에 그 글자로 보이는 필터 키들을 묶는다.
        // 바인딩 대상이라 속성은 public 이다(형식 자체는 이 컨트롤러 안에만 있다).
        private sealed class FilterValueItem : INotifyPropertyChanged
        {
            private bool isChecked;

            internal FilterValueItem(string text, bool isBlank, object sortValue)
            {
                this.Text = text;
                this.IsBlank = isBlank;
                this.SortValue = sortValue;
                this.Keys = new List<string>();
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public string Text { get; private set; }

            public bool IsBlank { get; private set; }

            public bool IsChecked
            {
                get
                {
                    return this.isChecked;
                }
                set
                {
                    if (this.isChecked == value)
                    {
                        return;
                    }

                    this.isChecked = value;

                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("IsChecked"));
                    }
                }
            }

            internal object SortValue { get; private set; }

            internal List<string> Keys { get; private set; }

            // 목록의 글자 검색(TextSearch)과 접근성 이름이 표시 글자를 쓰게 한다.
            public override string ToString()
            {
                return this.Text;
            }
        }

        /// <summary>값 필터 상태가 바뀔 때(Apply 확정) 발생한다 — 페이징
        /// 화면이 전체 결과에 같은 필터를 적용해 페이지를 재계산할 때 쓴다.</summary>
        internal event EventHandler FiltersChanged;

        /// <summary>팝업 고유 값의 원천 — 페이징 화면은 페이지 조각 대신 전체
        /// 결과(DataTable 등)를 지정한다. null이면 현재 ItemsSource에서 모은다.</summary>
        internal object ValueSource;

        internal GridFilterController(
                DataGrid grid, FrameworkElement resourceSource, Func<IEnumerable> itemsSourceGetter)
        {
            this.grid = grid;
            this.resourceSource = resourceSource;
            this.itemsSourceGetter = itemsSourceGetter;
        }

        /// <summary>행이 현재 컬럼 필터를 전부 통과하는지 판정한다 — 페이징
        /// 화면이 전체 결과를 직접 거를 때 쓴다 (필터 없으면 항상 true).</summary>
        internal bool Matches(object item)
        {
            return this.selections.Count == 0 || this.MatchesAllFilters(item);
        }

        /// <summary>값이 걸린 컬럼(활성 필터)이 하나라도 있는지.</summary>
        internal bool HasActiveFilters
        {
            get { return this.selections.Count > 0; }
        }

        /// <summary>
        /// 모든 컬럼의 값 필터를 한 번에 해제한다 — 우클릭 메뉴 "Clear Filters".
        /// 팝업의 Clear를 전 컬럼에 적용한 것과 동일하며, 깔때기 활성 표시도
        /// 함께 걷고 FiltersChanged를 1회 알린다.
        /// </summary>
        internal void ClearAllFilters()
        {
            if (this.selections.Count == 0)
            {
                return;
            }

            List<string> cleared = new List<string>(this.selections.Keys);
            this.selections.Clear();

            if (this.popup != null)
            {
                this.popup.IsOpen = false;
            }

            foreach (string columnName in cleared)
            {
                this.UpdateActiveFlags(columnName);
            }

            this.ApplyFilters();
            this.RaiseFiltersChanged();
        }

        private void RaiseFiltersChanged()
        {
            if (this.FiltersChanged != null)
            {
                this.FiltersChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>컬럼 재구성 시 컬럼 매핑을 비운다 (선택 상태는 유지).</summary>
        internal void OnColumnsRebuilt()
        {
            this.filterColumns.Clear();
            this.columnFormats.Clear();

            if (this.popup != null)
            {
                this.popup.IsOpen = false;
            }
        }

        /// <summary>데이터 소스가 바뀌면 유지 중인 선택을 새 뷰에 다시 적용한다.</summary>
        internal void OnItemsSourceChanged()
        {
            this.ReleaseOwnedFilter();
            this.ApplyFilters();
        }

        internal void Detach()
        {
            this.ReleaseOwnedFilter();
            if (this.popup != null)
            {
                this.popup.IsOpen = false;
            }
            this.ValueSource = null;
        }

        // 다른 소비자가 교체한 필터는 건드리지 않고 자신이 설치한 조건자만 해제한다.
        private void ReleaseOwnedFilter()
        {
            ICollectionView view = this.filteredView;
            Predicate<object> filter = this.ownedFilter;
            if (this.filterLease != null)
            {
                this.filterLease.Owner = null;
                this.filterLease = null;
            }
            this.filteredView = null;
            this.ownedFilter = null;
            IEditableCollectionView editable = view as IEditableCollectionView;
            bool editing = editable != null && (editable.IsAddingNew || editable.IsEditingItem);
            if (view != null && filter != null && view.Filter == filter && !editing)
            {
                view.Filter = null;
            }
        }

        /// <summary>
        /// 컬럼을 값 필터 대상으로 등록한다 — 체크박스/버튼 컬럼은 대상이
        /// 아니므로 건너뛴다. 깔때기 버튼 자체는 헤더 템플릿에 있고, 여기서는
        /// Filterable 첨부 속성으로 표시를 켜고 활성 상태를 되비춘다.
        /// </summary>
        internal void AttachHeader(DataGridColumn column, ModernDataGridColumn definition)
        {
            if (definition.Kind == GridColumnKind.CheckBox || definition.Kind == GridColumnKind.Button || !definition.AllowFilter)
            {
                return;
            }

            this.filterColumns[column] = definition.DataPropertyName;
            this.columnFormats[definition.DataPropertyName] = definition.Format;
            GridFilterAttach.SetFilterable(column, true);
            this.UpdateActiveFlags(definition.DataPropertyName);
        }

        // 필터 활성 여부를 컬럼 첨부 속성(IsActive)에 되비춘다 — 헤더 템플릿이
        // 이 값으로 깔때기를 액센트색으로 칠해 걸린 컬럼이 한눈에 보인다.
        private void UpdateActiveFlags(string columnName)
        {
            bool active = this.selections.ContainsKey(columnName);

            foreach (KeyValuePair<DataGridColumn, string> entry in this.filterColumns)
            {
                if (entry.Value == columnName)
                {
                    GridFilterAttach.SetIsActive(entry.Key, active);
                }
            }
        }

        // ===== 팝업 =====

        /// <summary>
        /// 헤더 깔때기 버튼 클릭 — 그 컬럼의 값 체크리스트 팝업을 연다. anchor는
        /// 헤더 셀(DataGridColumnHeader)이고 팝업은 그 아래에 왼쪽을 맞춰 열린다.
        /// </summary>
        internal void OpenPopupFor(DataGridColumn column, UIElement anchor)
        {
            string columnName;

            if (column == null || !this.filterColumns.TryGetValue(column, out columnName))
            {
                return;
            }

            if (this.popup == null)
            {
                this.popup = new Popup();
                this.popup.StaysOpen = false;
                this.popup.AllowsTransparency = true;
                this.popup.Placement = PlacementMode.Bottom;
                this.popup.Opened += this.OnPopupOpened;
                this.popup.Closed += this.OnPopupClosed;
            }

            if (this.popup.IsOpen && this.popupColumn == columnName)
            {
                this.popup.IsOpen = false;
                return;
            }

            double width = Math.Min(popupMaxWidth, Math.Max(popupMinWidth, column.ActualWidth));

            this.popupColumn = columnName;
            this.returnFocusOnClose = false;
            this.popup.Child = this.BuildPopupContent(columnName, width);
            this.popup.PlacementTarget = anchor;
            this.popup.HorizontalOffset = this.LeftClampOffset(anchor);
            this.popup.VerticalOffset = popupGap;
            this.popup.IsOpen = true;
        }

        // 가로 스크롤로 헤더 왼쪽이 그리드 밖으로 나가 있으면 그리드 왼쪽 끝에서 연다.
        private double LeftClampOffset(UIElement anchor)
        {
            if (anchor == null || anchor == this.grid || !anchor.IsVisible || !this.grid.IsAncestorOf(anchor))
            {
                return 0d;
            }

            Point origin = anchor.TranslatePoint(new Point(0d, 0d), this.grid);
            return origin.X < 0d ? -origin.X : 0d;
        }

        // 팝업 창(별도 HwndSource)이 뜬 뒤 입력 우선순위로 검색창에 포커스를 준다 —
        // 열기 직후에는 팝업 창이 아직 없어 포커스가 들어가지 않는다.
        private void OnPopupOpened(object sender, EventArgs e)
        {
            this.popup.Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(this.FocusPopupSearch));
        }

        private void FocusPopupSearch()
        {
            if (this.popup == null || !this.popup.IsOpen || this.popupSearch == null)
            {
                return;
            }

            this.popupSearch.FocusEditor();
        }

        private void OnPopupClosed(object sender, EventArgs e)
        {
            bool restore = this.returnFocusOnClose;
            this.returnFocusOnClose = false;

            if (restore)
            {
                this.FocusGrid();
            }
        }

        // 표로 돌아간다 — 현재 셀이 화면에 있으면 그 셀, 없으면 그리드에 포커스.
        private void FocusGrid()
        {
            if (!this.grid.IsVisible)
            {
                return;
            }

            DataGridCellInfo current = this.grid.CurrentCell;

            if (current.Column != null && current.Item != null)
            {
                DataGridRow row = this.grid.ItemContainerGenerator.ContainerFromItem(current.Item) as DataGridRow;
                FrameworkElement content = row == null ? null : current.Column.GetCellContent(row);
                DependencyObject node = content;

                while (node != null && !(node is DataGridCell))
                {
                    node = VisualTreeHelper.GetParent(node);
                }

                DataGridCell cell = node as DataGridCell;

                if (cell != null && cell.Focus())
                {
                    return;
                }
            }

            this.grid.Focus();
        }

        // 팝업 내용(위에서 아래로, Tab 순서도 같다):
        // [검색(Search, ✕ 지우기)] / [(All) — 검색 중이면 (All results)] / [값 목록(가상화) + 검색 결과 수]
        // / [Clear ……… Apply · Cancel]. 구분선은 BorderSubtle 1px.
        private UIElement BuildPopupContent(string columnName, double width)
        {
            HashSet<string> selected;
            this.selections.TryGetValue(columnName, out selected);

            this.popupItems = this.CollectFilterItems(columnName);
            this.popupVisible = this.popupItems;
            this.popupOriginal = selected;
            this.popupKeepAbsent = true;

            foreach (FilterValueItem item in this.popupItems)
            {
                item.IsChecked = selected == null || ContainsAny(selected, item.Keys);
            }

            Brush subtleLine = (Brush)this.resourceSource.FindResource("Brush.BorderSubtle");

            this.popupSearch = new ModernTextBoxControl();
            this.popupSearch.Placeholder = "Search";
            this.popupSearch.ShowClearButton = true;
            this.popupSearch.TextChanged += this.OnPopupSearchTextChanged;

            Border searchRow = new Border();
            searchRow.Padding = new Thickness(10d, 10d, 10d, 8d);
            searchRow.Child = this.popupSearch;

            // (All)·값 체크박스는 공용 파츠(체크 콤보의 (All) 머리와 같은 문법)라
            // 글자·박스·부분 선택 대시가 테마 색을 따른다.
            this.popupAll = new CheckBox();
            this.popupAll.Style = (Style)this.resourceSource.FindResource("Parts.CheckBoxStyle");
            this.popupAll.Content = "(All)";
            this.popupAll.Focusable = false;
            this.popupAll.Click += this.OnAllClick;

            Border allRow = new Border();
            allRow.BorderBrush = subtleLine;
            allRow.BorderThickness = new Thickness(0d, 0d, 0d, 1d);
            allRow.Padding = new Thickness(12d, 6d, 12d, 8d);
            allRow.Child = this.popupAll;

            this.popupList = this.BuildValueList();
            this.popupList.ItemsSource = this.popupVisible;

            this.popupCount = new TextBlock();
            this.popupCount.FontSize = (double)this.resourceSource.FindResource("Font.Size.Helper");
            this.popupCount.Foreground = (Brush)this.resourceSource.FindResource("Brush.TextSecondary");
            this.popupCount.Margin = new Thickness(38d, 0d, 12d, 6d);
            this.popupCount.TextTrimming = TextTrimming.CharacterEllipsis;
            this.popupCount.SetBinding(FrameworkElement.LayoutTransformProperty, TextWidthBinding());

            StackPanel listArea = new StackPanel();
            listArea.Children.Add(this.popupList);
            listArea.Children.Add(this.popupCount);

            Border buttonRow = new Border();
            buttonRow.BorderBrush = subtleLine;
            buttonRow.BorderThickness = new Thickness(0d, 1d, 0d, 0d);
            buttonRow.Padding = new Thickness(6d, 8d, 10d, 8d);
            buttonRow.Child = this.BuildButtonBar(this.selections.ContainsKey(columnName));

            Grid layout = new Grid();
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetRow(allRow, 1);
            Grid.SetRow(listArea, 2);
            Grid.SetRow(buttonRow, 3);
            layout.Children.Add(searchRow);
            layout.Children.Add(allRow);
            layout.Children.Add(listArea);
            layout.Children.Add(buttonRow);
            CornerClip.SetIsEnabled(layout, true);

            Border card = new Border();
            card.Background = (Brush)this.resourceSource.FindResource("Brush.Surface");
            card.BorderBrush = (Brush)this.resourceSource.FindResource("Brush.Border");
            card.BorderThickness = new Thickness(1d);
            card.CornerRadius = (CornerRadius)this.resourceSource.FindResource("Radius.Sm");
            card.Width = width;
            card.Child = layout;
            card.PreviewKeyDown += this.OnPopupPreviewKeyDown;
            KeyboardNavigation.SetTabNavigation(card, KeyboardNavigationMode.Cycle);
            KeyboardNavigation.SetDirectionalNavigation(card, KeyboardNavigationMode.Contained);

            // 팝업은 배치 대상(헤더 셀)의 상속 값을 받는다 — 헤더의 SemiBold·Label 크기가 값 글자로
            // 내려오지 않게 글꼴·굵기·크기·글자색을 토큰으로 다시 건다.
            System.Windows.Documents.TextElement.SetForeground(card, (Brush)this.resourceSource.FindResource("Brush.TextPrimary"));
            System.Windows.Documents.TextElement.SetFontFamily(card, (FontFamily)this.resourceSource.FindResource("Font.Family"));
            System.Windows.Documents.TextElement.SetFontSize(card, (double)this.resourceSource.FindResource("Font.Size.Body"));
            System.Windows.Documents.TextElement.SetFontWeight(card, FontWeights.Normal);

            this.UpdatePopupState();
            return card;
        }

        // 아래 줄: 왼쪽 Clear(Subtle — 이 컬럼에 필터가 걸려 있을 때만 켜짐),
        // 오른쪽 Apply(Primary — 하나 이상 골랐을 때만 켜짐) · Cancel(Secondary).
        private UIElement BuildButtonBar(bool columnFiltered)
        {
            ModernButtonControl clear = new ModernButtonControl();
            clear.Text = "Clear";
            clear.Kind = ButtonKind.Subtle;
            clear.FontSizeOverride = buttonFontSize;
            clear.HeightOverride = buttonHeight;
            clear.HorizontalAlignment = HorizontalAlignment.Left;
            clear.IsEnabled = columnFiltered;
            clear.Click += this.OnClearClick;

            this.popupApply = new ModernButtonControl();
            this.popupApply.Text = "Apply";
            this.popupApply.Kind = ButtonKind.Primary;
            this.popupApply.FontSizeOverride = buttonFontSize;
            this.popupApply.HeightOverride = buttonHeight;
            this.popupApply.Width = buttonWidth;
            this.popupApply.Margin = new Thickness(0d, 0d, 6d, 0d);
            this.popupApply.Click += this.OnApplyClick;

            ModernButtonControl cancel = new ModernButtonControl();
            cancel.Text = "Cancel";
            cancel.Kind = ButtonKind.Secondary;
            cancel.FontSizeOverride = buttonFontSize;
            cancel.HeightOverride = buttonHeight;
            cancel.Width = buttonWidth;
            cancel.Click += this.OnCancelClick;

            Grid bar = new Grid();
            bar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            bar.ColumnDefinitions.Add(new ColumnDefinition());
            bar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            bar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(this.popupApply, 2);
            Grid.SetColumn(cancel, 3);
            bar.Children.Add(clear);
            bar.Children.Add(this.popupApply);
            bar.Children.Add(cancel);
            return bar;
        }

        // 값 목록 — 항목 모델 + 바인딩의 가상화 ListBox. Tab 정거장은 하나이고(↑↓ 이동,
        // Space 체크), 줄을 누르면 체크가 바뀐다. 체크박스는 표시용이라 클릭을 받지 않는다.
        private ListBox BuildValueList()
        {
            ListBox list = new ListBox();
            list.MaxHeight = listMaxHeight;
            list.Background = Brushes.Transparent;
            list.BorderThickness = new Thickness(0d);
            list.SelectionMode = SelectionMode.Single;
            list.Template = BuildListTemplate();
            list.ItemContainerStyle = this.BuildItemContainerStyle();
            list.ItemTemplate = this.BuildItemTemplate();
            VirtualizingPanel.SetIsVirtualizing(list, true);
            VirtualizingPanel.SetVirtualizationMode(list, VirtualizationMode.Recycling);
            ScrollViewer.SetCanContentScroll(list, true);
            KeyboardNavigation.SetTabNavigation(list, KeyboardNavigationMode.Once);
            KeyboardNavigation.SetDirectionalNavigation(list, KeyboardNavigationMode.Contained);
            list.GotKeyboardFocus += this.OnListGotKeyboardFocus;
            return list;
        }

        // 목록 틀: 테두리 없는 ScrollViewer(가로 고정 — 긴 값은 … 로 잘린다) + ItemsPresenter.
        private static ControlTemplate BuildListTemplate()
        {
            FrameworkElementFactory scroll = new FrameworkElementFactory(typeof(ScrollViewer));
            scroll.SetValue(UIElement.FocusableProperty, false);
            scroll.SetValue(ScrollViewer.CanContentScrollProperty, true);
            scroll.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            scroll.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
            scroll.SetValue(Control.PaddingProperty, new Thickness(0d, 4d, 0d, 4d));
            scroll.AppendChild(new FrameworkElementFactory(typeof(ItemsPresenter)));

            ControlTemplate template = new ControlTemplate(typeof(ListBox));
            template.VisualTree = scroll;
            return template;
        }

        // 항목 줄: Padding 12,5(줄 높이 약 28), 마우스를 올리면 HoverBackground,
        // 키보드로 옮긴 줄에는 FocusBorder 1px 테두리(키보드 이동일 때만 보이는 FocusVisual).
        private Style BuildItemContainerStyle()
        {
            FrameworkElementFactory presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(
                    FrameworkElement.HorizontalAlignmentProperty,
                    new TemplateBindingExtension(Control.HorizontalContentAlignmentProperty));
            presenter.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Control.BackgroundProperty));
            border.SetValue(Border.PaddingProperty, new TemplateBindingExtension(Control.PaddingProperty));
            border.AppendChild(presenter);

            ControlTemplate template = new ControlTemplate(typeof(ListBoxItem));
            template.VisualTree = border;

            FrameworkElementFactory focusBorder = new FrameworkElementFactory(typeof(Border));
            focusBorder.SetValue(Border.BorderBrushProperty, this.resourceSource.FindResource("Brush.FocusBorder"));
            focusBorder.SetValue(Border.BorderThicknessProperty, new Thickness(1d));

            ControlTemplate focusTemplate = new ControlTemplate(typeof(Control));
            focusTemplate.VisualTree = focusBorder;

            Style focusVisual = new Style(typeof(Control));
            focusVisual.Setters.Add(new Setter(Control.TemplateProperty, focusTemplate));

            Style style = new Style(typeof(ListBoxItem));
            style.Setters.Add(new Setter(Control.TemplateProperty, template));
            style.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Transparent));
            style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(12d, 5d, 12d, 5d)));
            style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
            style.Setters.Add(new Setter(FrameworkElement.CursorProperty, Cursors.Hand));
            style.Setters.Add(new Setter(FrameworkElement.FocusVisualStyleProperty, focusVisual));
            style.Setters.Add(new EventSetter(
                    UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(this.OnValueItemMouseDown)));

            Trigger hover = new Trigger();
            hover.Property = UIElement.IsMouseOverProperty;
            hover.Value = true;
            hover.Setters.Add(new Setter(
                    Control.BackgroundProperty, this.resourceSource.FindResource("Brush.HoverBackground")));
            style.Triggers.Add(hover);
            return style;
        }

        // 항목 내용: [체크박스(표시용)][값 글자 — 남는 폭에서 … 로 잘리고, 잘렸을 때만 전체 값 툴팁].
        // (Blanks)는 흐린 글자(TextSecondary)다.
        private DataTemplate BuildItemTemplate()
        {
            FrameworkElementFactory check = new FrameworkElementFactory(typeof(CheckBox));
            check.SetValue(FrameworkElement.StyleProperty, this.resourceSource.FindResource("Parts.CheckBoxStyle"));
            check.SetValue(UIElement.IsHitTestVisibleProperty, false);
            check.SetValue(UIElement.FocusableProperty, false);
            check.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            check.SetValue(DockPanel.DockProperty, Dock.Left);
            check.SetBinding(ToggleButton.IsCheckedProperty, new Binding("IsChecked") { Mode = BindingMode.OneWay });

            FrameworkElementFactory text = new FrameworkElementFactory(typeof(TextBlock));
            text.Name = "ValueText";
            text.SetBinding(TextBlock.TextProperty, new Binding("Text") { Mode = BindingMode.OneWay });
            text.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);
            text.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            text.SetValue(TextBlock.ForegroundProperty, this.resourceSource.FindResource("Brush.TextPrimary"));
            text.SetBinding(
                    FrameworkElement.ToolTipProperty,
                    new Binding("Text") { RelativeSource = new RelativeSource(RelativeSourceMode.Self) });
            text.AddHandler(ToolTipService.ToolTipOpeningEvent, new ToolTipEventHandler(OnValueToolTipOpening));
            text.SetBinding(FrameworkElement.LayoutTransformProperty, TextWidthBinding());

            FrameworkElementFactory row = new FrameworkElementFactory(typeof(DockPanel));
            row.SetValue(DockPanel.LastChildFillProperty, true);
            row.AppendChild(check);
            row.AppendChild(text);

            DataTemplate template = new DataTemplate();
            template.VisualTree = row;

            DataTrigger blank = new DataTrigger();
            blank.Binding = new Binding("IsBlank");
            blank.Value = true;
            blank.Setters.Add(new Setter(
                    TextBlock.ForegroundProperty, this.resourceSource.FindResource("Brush.TextSecondary"), "ValueText"));
            template.Triggers.Add(blank);
            return template;
        }

        // 장평 — 상속된 TextWidthTransform 을 글자 요소가 스스로 받는다((All)은 공용 체크박스 파츠가 받는다).
        private static Binding TextWidthBinding()
        {
            Binding binding = new Binding();
            binding.Path = new PropertyPath("(0)", FontWidthScaling.TextWidthTransformProperty);
            binding.RelativeSource = new RelativeSource(RelativeSourceMode.Self);
            return binding;
        }

        // 값 글자가 실제로 잘렸을 때만 툴팁을 연다 — 셀 툴팁과 같은 규칙.
        private static void OnValueToolTipOpening(object sender, ToolTipEventArgs e)
        {
            TextBlock text = sender as TextBlock;

            if (text == null || !IsTextTrimmed(text))
            {
                e.Handled = true;
            }
        }

        private static bool IsTextTrimmed(TextBlock text)
        {
            if (string.IsNullOrEmpty(text.Text) || text.ActualWidth <= 0d)
            {
                return false;
            }

            Typeface typeface = new Typeface(text.FontFamily, text.FontStyle, text.FontWeight, text.FontStretch);
            FormattedText formatted = new FormattedText(
                    text.Text, CultureInfo.CurrentCulture, text.FlowDirection, typeface,
                    text.FontSize, Brushes.Black, VisualTreeHelper.GetDpi(text).PixelsPerDip);
            return formatted.WidthIncludingTrailingWhitespace > text.ActualWidth + 0.5d;
        }

        // 검색 입력이 바뀌면 보이는 값 목록을 좁히고 **선택도 그 결과로
        // 맞춘다** — 초성 매칭, 대상은 화면에 보이는 값 글자. 좁혀 놓고 체크는
        // "(All)"로 두면 Apply가 전체를 내보내서 검색이 아무 일도 하지 않는다
        // (2026-09-20). 검색어를 지우면 다시 전체 선택으로 돌아간다. 제안
        // 드롭다운은 쓰지 않는다(목록 자체가 좁혀지는데 드롭다운이 겹치면 혼란).
        private void OnPopupSearchTextChanged(object sender, EventArgs e)
        {
            string keyword = this.SearchKeyword();

            if (keyword.Length == 0)
            {
                this.popupVisible = this.popupItems;

                foreach (FilterValueItem item in this.popupItems)
                {
                    item.IsChecked = true;
                }
            }
            else
            {
                List<FilterValueItem> visible = new List<FilterValueItem>();

                foreach (FilterValueItem item in this.popupItems)
                {
                    bool match = HangulTextMatcher.Contains(item.Text, keyword);
                    item.IsChecked = match;

                    if (match)
                    {
                        visible.Add(item);
                    }
                }

                this.popupVisible = visible;
            }

            this.popupKeepAbsent = false;
            this.popupList.ItemsSource = this.popupVisible;
            this.UpdatePopupState();
        }

        private string SearchKeyword()
        {
            string keyword = this.popupSearch == null ? null : this.popupSearch.Text;
            return keyword == null || keyword.Trim().Length == 0 ? string.Empty : keyword;
        }

        // "(All)" / 검색 중 "(All results)": **보이는 값만** 켜고 끈다 — 보이는 값이
        // 전부 체크돼 있으면 전부 해제, 아니면 전부 체크(체크 콤보와 같은 방식 —
        // 부분 선택 대시는 코드가 IsChecked=null 로 그리고 결정은 이 처리기가 한다).
        // 검색으로 숨은 값의 체크는 건드리지 않는다.
        private void OnAllClick(object sender, RoutedEventArgs e)
        {
            bool allChecked = this.popupVisible.Count > 0;

            foreach (FilterValueItem item in this.popupVisible)
            {
                if (!item.IsChecked)
                {
                    allChecked = false;
                    break;
                }
            }

            foreach (FilterValueItem item in this.popupVisible)
            {
                item.IsChecked = !allChecked;
            }

            this.popupKeepAbsent = false;
            this.UpdatePopupState();
        }

        // 값 줄 클릭 — 체크를 뒤집는다. 처리를 끝내지 않아 포커스·키보드 위치는 그 줄로 간다.
        private void OnValueItemMouseDown(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem container = sender as ListBoxItem;
            FilterValueItem item = container == null ? null : container.DataContext as FilterValueItem;

            if (item != null)
            {
                this.ToggleItem(item);
            }
        }

        private void ToggleItem(FilterValueItem item)
        {
            item.IsChecked = !item.IsChecked;
            this.UpdatePopupState();
        }

        // (All) 표시(전부 체크 · 없음 · 부분 대시)와 이름, Apply 활성(하나 이상 체크), 검색 결과 수 줄을 맞춘다.
        private void UpdatePopupState()
        {
            bool searching = this.SearchKeyword().Length > 0;
            int visibleChecked = 0;

            foreach (FilterValueItem item in this.popupVisible)
            {
                if (item.IsChecked)
                {
                    visibleChecked = visibleChecked + 1;
                }
            }

            bool anyChecked = visibleChecked > 0;

            if (!anyChecked)
            {
                foreach (FilterValueItem item in this.popupItems)
                {
                    if (item.IsChecked)
                    {
                        anyChecked = true;
                        break;
                    }
                }
            }

            this.popupAll.Content = searching ? "(All results)" : "(All)";
            this.popupAll.IsEnabled = this.popupVisible.Count > 0;

            if (visibleChecked == 0)
            {
                this.popupAll.IsChecked = false;
            }
            else if (visibleChecked == this.popupVisible.Count)
            {
                this.popupAll.IsChecked = true;
            }
            else
            {
                this.popupAll.IsChecked = null;
            }

            this.popupApply.IsEnabled = anyChecked;

            if (this.popupVisible.Count == 0)
            {
                this.popupCount.Text = "No matching values";
                this.popupCount.Margin = new Thickness(12d, 6d, 12d, 6d);
                this.popupCount.Visibility = Visibility.Visible;
            }
            else if (searching)
            {
                this.popupCount.Text = string.Format(
                        CultureInfo.CurrentCulture, "{0:N0} of {1:N0} values", this.popupVisible.Count, this.popupItems.Count);
                this.popupCount.Margin = new Thickness(38d, 0d, 12d, 6d);
                this.popupCount.Visibility = Visibility.Visible;
            }
            else
            {
                this.popupCount.Visibility = Visibility.Collapsed;
            }
        }

        // 팝업 키: Enter = Apply(버튼에 포커스가 있으면 그 버튼), Esc = 취소하고 표로,
        // 검색창 ↓ = 목록 첫 줄, 목록 첫 줄 ↑ = 검색창, 목록 Space = 체크.
        // 모두 여기서 처리를 끝내 폼의 기본 버튼·단축키로 새지 않게 한다.
        private void OnPopupPreviewKeyDown(object sender, KeyEventArgs e)
        {
            DependencyObject focused = Keyboard.FocusedElement as DependencyObject;
            ListBoxItem focusedItem = focused as ListBoxItem;
            bool inList = focusedItem != null && ItemsControl.ItemsControlFromItemContainer(focusedItem) == this.popupList;
            bool inSearch = this.popupSearch != null && this.popupSearch.IsKeyboardFocusWithin;

            switch (e.Key)
            {
                case Key.Escape:
                    e.Handled = true;
                    this.ClosePopup(true);
                    break;

                case Key.Enter:
                    if (focused is ButtonBase)
                    {
                        return;
                    }

                    e.Handled = true;

                    if (this.popupApply.IsEnabled)
                    {
                        this.ApplyPopup();
                    }

                    break;

                case Key.Down:
                    if (inSearch)
                    {
                        e.Handled = true;
                        this.FocusListItem(0);
                    }

                    break;

                case Key.Up:
                    if (inList && this.popupList.ItemContainerGenerator.IndexFromContainer(focusedItem) == 0)
                    {
                        e.Handled = true;
                        this.popupSearch.FocusEditor();
                    }

                    break;

                case Key.Space:
                    if (inList)
                    {
                        e.Handled = true;
                        FilterValueItem item = focusedItem.DataContext as FilterValueItem;

                        if (item != null)
                        {
                            this.ToggleItem(item);
                        }
                    }

                    break;
            }
        }

        // Tab 으로 목록 자체에 포커스가 오면 지난 줄(없으면 첫 줄)로 옮긴다 — 목록은 한 정거장이다.
        private void OnListGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (e.NewFocus == this.popupList)
            {
                this.FocusListItem(Math.Max(0, this.popupList.SelectedIndex));
            }
        }

        private void FocusListItem(int index)
        {
            if (this.popupVisible.Count == 0)
            {
                return;
            }

            int target = Math.Min(Math.Max(0, index), this.popupVisible.Count - 1);
            this.popupList.SelectedIndex = target;
            this.popupList.ScrollIntoView(this.popupVisible[target]);

            ListBoxItem container = this.popupList.ItemContainerGenerator.ContainerFromIndex(target) as ListBoxItem;

            if (container == null)
            {
                this.popupList.UpdateLayout();
                container = this.popupList.ItemContainerGenerator.ContainerFromIndex(target) as ListBoxItem;
            }

            if (container != null)
            {
                container.Focus();
            }
        }

        private void OnApplyClick(object sender, RoutedEventArgs e)
        {
            this.ApplyPopup();
        }

        // Apply — 팝업에서 모은 선택을 확정하고 한 번에 반영한다. 값이 전부 체크면
        // 필터 없음, 아니면 체크된 줄의 키 집합(열 때 걸려 있던 키 중 지금 데이터에
        // 없는 것은 검색·(All) 조작이 없었을 때만 그대로 남긴다).
        private void ApplyPopup()
        {
            bool allChecked = true;
            bool anyChecked = false;

            foreach (FilterValueItem item in this.popupItems)
            {
                if (item.IsChecked)
                {
                    anyChecked = true;
                }
                else
                {
                    allChecked = false;
                }
            }

            if (!anyChecked)
            {
                return;
            }

            if (allChecked)
            {
                this.selections.Remove(this.popupColumn);
            }
            else
            {
                HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);

                if (this.popupKeepAbsent && this.popupOriginal != null)
                {
                    HashSet<string> present = new HashSet<string>(StringComparer.Ordinal);

                    foreach (FilterValueItem item in this.popupItems)
                    {
                        present.UnionWith(item.Keys);
                    }

                    foreach (string key in this.popupOriginal)
                    {
                        if (!present.Contains(key))
                        {
                            keys.Add(key);
                        }
                    }
                }

                foreach (FilterValueItem item in this.popupItems)
                {
                    if (item.IsChecked)
                    {
                        keys.UnionWith(item.Keys);
                    }
                }

                this.selections[this.popupColumn] = keys;
            }

            this.ApplyFilters();
            this.UpdateActiveFlags(this.popupColumn);
            this.RaiseFiltersChanged();
            this.ClosePopup(true);
        }

        // Clear — 이 컬럼의 필터를 **즉시 해제**하고 닫는다 ("(All) 체크 +
        // Apply"와 같은 결과의 단축 버튼). 필터가 걸린 컬럼에서만 켜진다.
        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            this.selections.Remove(this.popupColumn);
            this.ApplyFilters();
            this.UpdateActiveFlags(this.popupColumn);
            this.RaiseFiltersChanged();
            this.ClosePopup(true);
        }

        // Cancel — 편집 중 변경을 버리고 닫는다 (Esc·바깥 클릭으로 닫는 것과 동일).
        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            this.ClosePopup(true);
        }

        private void ClosePopup(bool returnFocus)
        {
            if (this.popup == null)
            {
                return;
            }

            this.returnFocusOnClose = returnFocus;
            this.popup.IsOpen = false;
        }

        private static bool ContainsAny(HashSet<string> set, List<string> keys)
        {
            foreach (string key in keys)
            {
                if (set.Contains(key))
                {
                    return true;
                }
            }

            return false;
        }

        // ===== 값 수집 + 필터 적용 =====

        // 컬럼의 값 목록 — 다른 컬럼 필터와 무관하게 원본 전체에서 모은다(상한 없음).
        // 표시 글자는 셀과 같은 서식, 같은 글자로 보이는 키는 한 줄로 묶고, 빈 글자는
        // 맨 끝 (Blanks) 한 줄이다. 정렬은 값이 모두 숫자면 수, 모두 날짜면 시각, 아니면 글자 순이다.
        private List<FilterValueItem> CollectFilterItems(string columnName)
        {
            string format;
            this.columnFormats.TryGetValue(columnName, out format);
            CultureInfo culture = this.CellCulture();

            Dictionary<string, FilterValueItem> byText = new Dictionary<string, FilterValueItem>(StringComparer.Ordinal);
            HashSet<string> seenKeys = new HashSet<string>(StringComparer.Ordinal);
            List<FilterValueItem> items = new List<FilterValueItem>();
            FilterValueItem blank = null;
            IEnumerable rows = this.ResolveValueItems();

            if (rows != null)
            {
                foreach (object row in rows)
                {
                    object raw = ReadRawValue(row, columnName);
                    string key = KeyOf(raw);

                    if (!seenKeys.Add(key))
                    {
                        continue;
                    }

                    object typed = key.Length == 0 ? null : GridAutoFitMeasurer.CoerceForFormat(raw, format);
                    string text = key.Length == 0 ? string.Empty : FormatDisplayText(typed, format, culture);

                    if (text.Trim().Length == 0)
                    {
                        if (blank == null)
                        {
                            blank = new FilterValueItem("(Blanks)", true, null);
                        }

                        blank.Keys.Add(key);
                        continue;
                    }

                    FilterValueItem item;

                    if (!byText.TryGetValue(text, out item))
                    {
                        item = new FilterValueItem(text, false, SortValueOf(typed));
                        byText.Add(text, item);
                        items.Add(item);
                    }

                    item.Keys.Add(key);
                }
            }

            SortItems(items);

            if (blank != null)
            {
                items.Add(blank);
            }

            return items;
        }

        // 셀 바인딩이 쓰는 문화권 — 바인딩은 대상 요소의 Language 로 StringFormat 을 적용한다.
        private CultureInfo CellCulture()
        {
            XmlLanguage language = this.grid.Language;

            if (language != null)
            {
                try
                {
                    return language.GetSpecificCulture();
                }
                catch (InvalidOperationException)
                {
                    return CultureInfo.CurrentCulture;
                }
            }

            return CultureInfo.CurrentCulture;
        }

        // 셀과 같은 표시 글자 — Format 이 있으면 "{0:Format}", 없으면 기본 문자열 변환(셀 문화권).
        private static string FormatDisplayText(object value, string format, CultureInfo culture)
        {
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(format))
            {
                try
                {
                    return string.Format(culture, "{0:" + format + "}", value);
                }
                catch (FormatException)
                {
                    // 형식 오류는 기본 표현으로 폴백한다 (셀 표시와 같은 완화).
                }
            }

            return Convert.ToString(value, culture) ?? string.Empty;
        }

        // 정렬 값 — 숫자는 double, 날짜는 DateTime, 그 밖은 null(글자 순).
        private static object SortValueOf(object value)
        {
            if (value is DateTime)
            {
                return value;
            }

            if (value is DateTimeOffset)
            {
                return ((DateTimeOffset)value).UtcDateTime;
            }

            if (value is byte || value is sbyte || value is short || value is ushort
                    || value is int || value is uint || value is long || value is ulong
                    || value is float || value is double || value is decimal)
            {
                return Convert.ToDouble(value, CultureInfo.InvariantCulture);
            }

            return null;
        }

        private static void SortItems(List<FilterValueItem> items)
        {
            bool allNumbers = items.Count > 0;
            bool allDates = items.Count > 0;

            foreach (FilterValueItem item in items)
            {
                allNumbers = allNumbers && item.SortValue is double;
                allDates = allDates && item.SortValue is DateTime;
            }

            if (allNumbers)
            {
                items.Sort(delegate(FilterValueItem left, FilterValueItem right)
                {
                    int order = ((double)left.SortValue).CompareTo((double)right.SortValue);
                    return order != 0 ? order : CompareText(left, right);
                });
            }
            else if (allDates)
            {
                items.Sort(delegate(FilterValueItem left, FilterValueItem right)
                {
                    int order = ((DateTime)left.SortValue).CompareTo((DateTime)right.SortValue);
                    return order != 0 ? order : CompareText(left, right);
                });
            }
            else
            {
                items.Sort(CompareText);
            }
        }

        private static int CompareText(FilterValueItem left, FilterValueItem right)
        {
            int order = StringComparer.OrdinalIgnoreCase.Compare(left.Text, right.Text);
            return order != 0 ? order : StringComparer.Ordinal.Compare(left.Text, right.Text);
        }

        // 고유 값을 모을 행 목록을 정한다 — ValueSource(전체 결과)가 지정돼
        // 있으면 그것을, 아니면 현재 ItemsSource를 쓴다. DataView는 CustomFilter가
        // 이미 걸려 있을 수 있으므로 원본 테이블에서 모은다.
        private IEnumerable ResolveValueItems()
        {
            object source = this.ValueSource;

            if (source == null)
            {
                source = this.itemsSourceGetter();
            }

            DataTable table = source as DataTable;

            if (table != null)
            {
                return table.Rows;
            }

            DataView view = source as DataView;

            if (view != null)
            {
                return view.Table.Rows;
            }

            return source as IEnumerable;
        }

        // 행(DataRow/DataRowView/POCO)에서 컬럼 값을 필터 키 문자열로 읽는다.
        private static string ReadValue(object item, string columnName)
        {
            return KeyOf(ReadRawValue(item, columnName));
        }

        // 필터 키 — 원래 값의 ToString()(빈 값·DBNull 은 빈 문자열). 표시 서식과 무관하다.
        private static string KeyOf(object value)
        {
            return value == null || value == DBNull.Value ? string.Empty : value.ToString();
        }

        // 행(DataRow/DataRowView/POCO)에서 컬럼의 원래 값을 읽는다.
        private static object ReadRawValue(object item, string columnName)
        {
            DataRow row = item as DataRow;
            DataRowView rowView = item as DataRowView;

            if (rowView != null)
            {
                row = rowView.Row;
            }

            if (row != null)
            {
                if (!row.Table.Columns.Contains(columnName))
                {
                    return null;
                }

                return row[columnName];
            }

            if (item == null)
            {
                return null;
            }

            // 일반 객체는 멤버 경로로 읽는다 — 점 경로("Row.QTY")도 해석되므로
            // 트리리스트의 노드처럼 원본 행을 품은 항목도 값이 나온다.
            return MemberPathReader.Read(item, columnName);
        }

        // 현재 선택을 뷰에 적용한다 — DataView는 CustomFilter 식, 그 외는
        // Filter 조건자. 소스 자체는 건드리지 않는다.
        internal void ApplyFilters()
        {
            // 소유자가 스스로 거르는 경우(트리리스트 — 걸린 노드의 조상을 남겨야
            // 계보가 끊기지 않는다)에는 뷰 필터를 걸지 않는다. 선택 상태와
            // FiltersChanged 알림만 쓰고 무엇을 감출지는 소유자가 정한다.
            if (this.ExternalFiltering)
            {
                return;
            }

            IEnumerable source = this.itemsSourceGetter();

            if (source == null)
            {
                return;
            }

            ICollectionView view = CollectionViewSource.GetDefaultView(source);
            BindingListCollectionView bindingView = view as BindingListCollectionView;

            if (bindingView != null)
            {
                if (bindingView.CanCustomFilter)
                {
                    bindingView.CustomFilter = this.BuildDataViewFilter();
                }

                return;
            }

            if (view == null || !view.CanFilter)
            {
                return;
            }

            if (this.filterLease != null)
            {
                this.filterLease.Owner = null;
            }
            this.filterLease = new ViewFilter { Owner = this };
            this.ownedFilter = this.selections.Count == 0
                    ? (Predicate<object>)null
                    : new Predicate<object>(this.filterLease.Matches);
            this.filteredView = view;
            view.Filter = this.ownedFilter;
            view.Refresh();
        }

        private bool MatchesAllFilters(object item)
        {
            foreach (KeyValuePair<string, HashSet<string>> entry in this.selections)
            {
                if (!entry.Value.Contains(ReadValue(item, entry.Key)))
                {
                    return false;
                }
            }

            return true;
        }

        // DataView CustomFilter 식 — 컬럼별 "값 중 하나" 절을 AND로 잇는다.
        private string BuildDataViewFilter()
        {
            if (this.selections.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder filter = new StringBuilder();

            foreach (KeyValuePair<string, HashSet<string>> entry in this.selections)
            {
                if (filter.Length > 0)
                {
                    filter.Append(" AND ");
                }

                filter.Append(BuildColumnClause(entry.Key, entry.Value));
            }

            return filter.ToString();
        }

        // 한 컬럼의 값 절 — 타입 컬럼(int 등)도 문자열로 변환해 비교하고,
        // 빈 값 선택은 NULL/빈 문자열을 함께 포함한다. 아무 값도 선택하지
        // 않았으면 모두 숨긴다. 값 목록은 "= … OR = …" 사슬 대신 IN 하나로 쓴다 —
        // 비교 의미(DataTable 의 문자열 비교)는 같고, 값 3000개 사슬은 행 5000개에
        // 15초가 걸렸지만 IN 은 1초 안팎이다(2026-10-30 실측).
        private static string BuildColumnClause(string columnName, HashSet<string> values)
        {
            string safeColumn = "[" + columnName.Replace("]", "]]") + "]";
            string asText = "CONVERT(" + safeColumn + ", 'System.String')";
            StringBuilder list = new StringBuilder();
            bool includeBlank = false;

            foreach (string value in values)
            {
                if (value.Length == 0)
                {
                    includeBlank = true;
                    continue;
                }

                if (list.Length > 0)
                {
                    list.Append(", ");
                }

                list.Append("'").Append(value.Replace("'", "''")).Append("'");
            }

            StringBuilder clause = new StringBuilder("(");

            if (list.Length > 0)
            {
                clause.Append(asText).Append(" IN (").Append(list).Append(")");
            }

            if (includeBlank)
            {
                if (list.Length > 0)
                {
                    clause.Append(" OR ");
                }

                clause.Append(safeColumn).Append(" IS NULL OR ").Append(asText).Append(" = ''");
            }

            if (list.Length == 0 && !includeBlank)
            {
                clause.Append("1 = 0");
            }

            clause.Append(")");
            return clause.ToString();
        }
    }
}
