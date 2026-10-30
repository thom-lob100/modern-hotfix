using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using Modern.Lab.Controls.Wpf.Common;
using Modern.Lab.Controls.Wpf.Input;

namespace Modern.Lab.Controls.Wpf.Data
{
    /// <summary>
    /// ModernDataGridControl·ModernTreeListControl의 찾기(Ctrl+F) 컨트롤러 — 행을 거르는 필터가 아니라 **일치 위치로 이동**한다:
    /// - 그리드 <b>머리글 위에 끼어드는 인라인 찾기 바</b> [검색어 ⓧ][2 of 19][↑][↓] … [All][✕] — 표는 그만큼 내려가고
    ///   셀을 가리지 않는다. 바는 그리드 비주얼 트리 안의 요소라 별도 hwnd 가 없다.
    /// - 이동은 행 단위다: Enter · F3 · ↓ 는 <b>지금 선택 행 다음</b>의 일치 행으로, Shift+Enter · Shift+F3 · ↑ 는 앞의 일치
    ///   행으로 간다(끝에서 처음으로 돈다). 같은 행의 다른 칸 일치는 건너뛰고, 현재 셀은 그 행의 첫 일치 칸이다. 개수도 일치
    ///   행 수다("2 of 19"). 재조회·자동 새로고침·정렬·필터·행 추가 뒤에도 지금 선택 행에서 이어 찾는다.
    /// - 병합 칸(MergeCells)의 일치는 구간당 한 번(구간 첫 행)으로 센다.
    /// - All: 일치 목록(칸 단위)을 바 아래에 펼친다 — 항목을 누르면 그 칸으로 이동.
    /// - 화면에 실체화된 행의 일치 글자는 노랑(Brush.FindMatch), 지금 일치는 주황(Brush.FindMatchCurrent)이고 글자색은
    ///   테마와 무관하게 Brush.OnFindMatch 로 고정한다.
    /// - 키는 그리드·트리 루트의 PreviewKeyDown 이 HandleRootKey 한 곳으로 넘긴다 — Ctrl+F 열기(지난 검색어 전체 선택),
    ///   검색창의 Enter·Shift+Enter, 어디서나 F3·Shift+F3, Esc 닫기(셀 편집 중이면 편집 취소가 먼저). 닫으면 찾은 셀에
    ///   포커스가 남는다. 바의 버튼과 결과 목록은 포커스를 가져가지 않아 누른 뒤에도 Enter 가 계속 이동한다.
    ///
    /// 2026-08-02 부터 2026-10-22 까지는 호스트 최상위 창에 소유된 별도 Window(엑셀식 모덜리스 찾기 창)였다 —
    /// 별도 창이라 셸에서 화면을 바꿔도 남고(2026-08-07 · 08-25 현장 보고) 첫 열기에 창 생성 비용이 들어,
    /// 사용자 결정(2026-10-23)으로 그리드 안의 바가 됐다. 가시성·z-순서 감시와 소유자 hwnd 연결은 더 필요 없다.
    ///
    /// 일치 판정은 셀에 보이는 글자 기준(Format 과 셀의 문화권 적용, 부분 일치, 대소문자 무시)이고
    /// 대상은 텍스트를 담은 컬럼(Text/Link/Badge)이다. 검색 순서는 현재 뷰의
    /// 행 순서(정렬·컬럼 필터 반영) × 컬럼 순서다.
    ///
    /// 강조는 셀 TextBlock의 Inlines를 바꾼다 — 그러면 WPF 가 Text 에 로컬 값을 써서 원래 바인딩이 끊긴다
    /// (재활용된 행에 다른 행의 값과 가짜 강조가 남던 원인, 2026-10-30 측정). 그래서 강조하기 전에 Text 값의
    /// 출처(바인딩 · 로컬 글자 · 템플릿)를 붙임 속성에 보관하고, 강조를 걷을 때와 행이 재활용·실체화될 때
    /// 그 출처를 되돌린 뒤 다시 강조한다.
    /// </summary>
    internal sealed class GridFindController
    {
        // 강조를 걸어 둔 TextBlock 표식 — 키워드가 바뀌거나 닫을 때 원복 대상 판별.
        private static readonly DependencyProperty IsHighlightedProperty =
            DependencyProperty.RegisterAttached(
                "GridFindIsHighlighted",
                typeof(bool),
                typeof(GridFindController),
                new PropertyMetadata(false));

        // 강조 전 Text 값의 출처 — 강조를 걷을 때 그대로 되돌린다.
        private static readonly DependencyProperty TextOriginProperty =
            DependencyProperty.RegisterAttached(
                "GridFindTextOrigin",
                typeof(object),
                typeof(GridFindController),
                new PropertyMetadata(null));

        // 결과 목록 표시 상한 — 일치가 비정상적으로 많을 때 창이 멈추는 것만 막는다.
        private const int maxResultItems = 500;

        // 바 안 요소 치수 — 검색창 폭, 작은 버튼 한 변, 결과 목록 최대 높이, 개수 글자 최소 폭.
        private const double searchBoxWidth = 260d;
        private const double searchBoxMinWidth = 120d;
        private const double smallButtonSize = 28d;
        private const double resultsMaxHeight = 160d;
        private const double countTextMinWidth = 56d;

        // 검색창 최소 폭 + 개수 글자 자리 + ↑↓ 두 버튼 — 이보다 좁으면 개수를 숨긴다.
        private const double countTextRoom = searchBoxMinWidth + 10d + 88d + 6d + 2d * (smallButtonSize + 4d);

        // 찾기 이동의 가장자리 여유(줄) — 찾은 행이 화면 밖이거나 위·아래 이 줄 수 안이면 가운데쯤으로 옮긴다.
        private const int edgeRows = 2;

        // 검색 대상 컬럼 하나 — 그리드 컬럼과 값 읽기 경로/서식/캡션/병합 여부의 묶음.
        private sealed class SearchColumn
        {
            internal readonly DataGridColumn Column;
            internal readonly string MemberPath;
            internal readonly string Format;
            internal readonly string Caption;
            internal readonly bool Merged;

            internal SearchColumn(DataGridColumn column, string memberPath, string format, string caption, bool merged)
            {
                this.Column = column;
                this.MemberPath = memberPath;
                this.Format = format;
                this.Caption = caption;
                this.Merged = merged;
            }
        }

        // 일치 하나 — 행 항목 + 대상 컬럼 + 표시 텍스트.
        private sealed class FindMatch
        {
            internal readonly object Item;
            internal readonly int RowIndex;
            internal readonly SearchColumn Column;
            internal readonly string CellText;

            internal FindMatch(object item, int rowIndex, SearchColumn column, string cellText)
            {
                this.Item = item;
                this.RowIndex = rowIndex;
                this.Column = column;
                this.CellText = cellText;
            }
        }

        // 강조 전 Text 의 출처 — 로컬 바인딩이면 그 바인딩, 로컬 글자면 그 글자, 템플릿·스타일이면 로컬 값을 지워 되돌린다.
        private sealed class TextOrigin
        {
            internal readonly BindingBase Binding;
            internal readonly string Text;
            internal readonly bool IsLocalText;

            internal TextOrigin(BindingBase binding, string text, bool isLocalText)
            {
                this.Binding = binding;
                this.Text = text;
                this.IsLocalText = isLocalText;
            }
        }

        private readonly DataGrid grid;
        private readonly FrameworkElement resourceSource;
        private readonly Func<IList<ModernDataGridColumn>> definitionsGetter;

        private readonly Border host;
        private FrameworkElement bar;
        private ModernButtonControl allButton;
        private ModernButtonControl previousButton;
        private ModernButtonControl nextButton;
        private string rememberedText = string.Empty;
        private ModernTextBoxControl searchBox;
        private TextBlock countText;
        private StackPanel resultsPanel;
        private ScrollViewer resultsScroll;

        private readonly List<SearchColumn> searchColumns = new List<SearchColumn>();

        // 칸 단위 일치(All 목록) — 행 순서 × 컬럼 순서.
        private readonly List<FindMatch> matches = new List<FindMatch>();

        // 이동 단위 — 일치가 있는 행마다 그 행 첫 일치의 matches 인덱스.
        private readonly List<int> rowStops = new List<int>();

        // 강조를 걸어 둔 TextBlock — 걷을 때(닫기·검색어 비움) 재활용 대기 행까지 빠짐없이 되돌린다.
        private readonly HashSet<TextBlock> highlightedBlocks = new HashSet<TextBlock>();

        private string searchText = string.Empty;
        private bool matchesValid;
        private bool columnsValid;

        // 지금 일치(주황) — matches 인덱스, -1 = 아직 이동 전.
        private int currentIndex = -1;

        // 닫을 때 포커스를 돌려줄 칸 — 바를 연 때의 현재 셀 칸, 이동하면 찾은 칸.
        private DataGridColumn lastCellColumn;

        // 데이터 변경 폭주(행 단위 CollectionChanged) 중 강조/카운트 갱신을
        // 한 번으로 접는 플래그.
        private bool refreshQueued;

        /// <param name="host">바가 들어갈 자리 — 그리드 XAML 의 <c>FindBarHost</c>(DockPanel 위쪽, 평소 Collapsed).</param>
        internal GridFindController(
                DataGrid grid,
                FrameworkElement resourceSource,
                Border host,
                Func<IList<ModernDataGridColumn>> definitionsGetter)
        {
            this.grid = grid;
            this.resourceSource = resourceSource;
            this.host = host;
            this.definitionsGetter = definitionsGetter;

            // 재활용될 행은 강조를 먼저 되돌려 둔다 — 다음 항목으로 쓰일 때 바인딩이 새 값을 그린다.
            this.grid.UnloadingRow += this.OnRowUnloading;
        }

        /// <summary>검색어·지금 일치가 바뀌었다 — 병합 스팬처럼 셀 밖에서 그리는 글자가 강조를 다시 그린다.</summary>
        internal event EventHandler HighlightChanged;

        /// <summary>찾기 바가 열려 있는지.</summary>
        internal bool IsOpen
        {
            get { return this.bar != null && this.host.Visibility == Visibility.Visible; }
        }

        /// <summary>강조할 검색어 — 바가 닫혀 있으면 빈 값.</summary>
        internal string HighlightKeyword
        {
            get { return this.IsOpen ? this.searchText : string.Empty; }
        }

        /// <summary>지금 일치의 행 항목(없으면 null).</summary>
        internal object CurrentMatchItem
        {
            get
            {
                FindMatch current = this.CurrentMatch;
                return current == null ? null : current.Item;
            }
        }

        /// <summary>지금 일치의 컬럼(없으면 null).</summary>
        internal DataGridColumn CurrentMatchColumn
        {
            get
            {
                FindMatch current = this.CurrentMatch;
                return current == null ? null : current.Column.Column;
            }
        }

        private FindMatch CurrentMatch
        {
            get
            {
                return this.currentIndex >= 0 && this.currentIndex < this.matches.Count
                        ? this.matches[this.currentIndex]
                        : null;
            }
        }

        /// <summary>찾기 바를 열고(이미 열려 있으면 그대로) 검색창에 커서를 두며, 지난 검색어를 되살려 전체 선택한다.</summary>
        internal void Show()
        {
            if (this.bar == null)
            {
                this.bar = this.BuildBar();
                this.host.Child = this.bar;
            }

            // 표 셀에서 열었으면 그 칸을 기억한다 — 닫을 때 그 칸으로 돌아간다(검색창으로 포커스가 가면 현재 셀이 비워진다).
            if (this.grid.CurrentCell.Column != null)
            {
                this.lastCellColumn = this.grid.CurrentCell.Column;
            }

            bool reopened = !this.IsOpen;
            this.host.Visibility = Visibility.Visible;

            if (reopened && this.rememberedText.Length > 0)
            {
                // 지난 검색어 복원 — TextChanged 경로가 일치 수집과 강조까지 처리한다.
                this.searchBox.Text = this.rememberedText;
            }

            // 처음 만든 바는 레이아웃 전이라 검색창이 아직 화면 트리 밖이고, 그대로 Focus 하면 거절된다
            // (그리드마다 첫 Ctrl+F 가 실패하던 원인, 2026-10-30 측정). 레이아웃을 먼저 끝낸다.
            this.host.UpdateLayout();
            this.FocusSearchSelectAll();
        }

        /// <summary>찾기 바를 닫고 강조를 걷는다. 검색어는 세션 동안 기억해 다시 열면 되살린다.</summary>
        internal void Hide()
        {
            if (!this.IsOpen)
            {
                return;
            }

            this.rememberedText = this.searchText;
            this.ClearResultsPanel();

            if (this.searchText.Length > 0 && this.searchBox != null)
            {
                // 검색어를 비우면 TextChanged 경로가 강조 해제까지 처리한다.
                this.searchBox.Text = string.Empty;
            }

            this.host.Visibility = Visibility.Collapsed;
            this.RestoreAllHighlights();
            this.RaiseHighlightChanged();
        }

        // 바를 버린다 — 호스트 최종 폐기 경로. 다음 Show 가 새로 만든다.
        internal void Release()
        {
            this.Hide();
            this.RestoreAllHighlights();
            this.host.Child = null;
            this.bar = null;
            this.searchBox = null;
            this.countText = null;
            this.resultsPanel = null;
            this.resultsScroll = null;
            this.allButton = null;
            this.previousButton = null;
            this.nextButton = null;
            this.searchText = string.Empty;
            this.rememberedText = string.Empty;
        }

        /// <summary>지금 선택 행 다음의 일치 행으로 이동한다 (끝나면 처음부터). 검색창 Enter/F3과 동일.</summary>
        internal void FindNext()
        {
            this.Step(true);
        }

        /// <summary>지금 선택 행 앞의 일치 행으로 이동한다 (처음이면 끝으로). Shift+F3/Shift+Enter와 동일.</summary>
        internal void FindPrevious()
        {
            this.Step(false);
        }

        /// <summary>
        /// 그리드·트리 루트의 PreviewKeyDown — 찾기 키를 모두 여기서 처리한다(루트가 바보다 먼저 받는다).
        /// Ctrl+F 는 열기(이미 열려 있으면 검색창 재포커스·전체 선택), F3/Shift+F3 은 컨트롤 어디서나 다음/이전,
        /// Enter/Shift+Enter 는 검색창에서만 다음/이전, Esc 는 바나 표에서 닫기 — 셀 편집 중이면 편집 취소가 먼저다.
        /// 처리한 키는 Handled 로 끝내 WinForms 폼의 기본 버튼(AcceptButton)·단축키로 새지 않게 한다.
        /// </summary>
        internal void HandleRootKey(KeyEventArgs e, bool allowOpen)
        {
            ModifierKeys modifiers = Keyboard.Modifiers;
            bool shift = (modifiers & ModifierKeys.Shift) != 0;

            if (e.Key == Key.F && (modifiers & ModifierKeys.Control) != 0)
            {
                if (allowOpen)
                {
                    this.Show();
                    e.Handled = true;
                }

                return;
            }

            if (!this.IsOpen)
            {
                return;
            }

            if (e.Key == Key.F3)
            {
                this.StepByKey(shift);
                e.Handled = true;
                return;
            }

            Visual source = e.OriginalSource as Visual;
            bool inBar = source != null && this.host.IsAncestorOf(source);

            if (e.Key == Key.Enter && inBar)
            {
                this.StepByKey(shift);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape)
            {
                bool inGrid = source != null && this.grid.IsAncestorOf(source);

                if (inBar || (inGrid && !IsEditingCell(source)))
                {
                    this.Close();
                    e.Handled = true;
                }
            }
        }

        private void StepByKey(bool backward)
        {
            if (backward)
            {
                this.FindPrevious();
            }
            else
            {
                this.FindNext();
            }
        }

        /// <summary>
        /// 행이 화면에 실체화될 때(가상화·재활용) — 그 행에 남은 강조를 먼저 되돌리고(바가 닫혀 있어도),
        /// 바가 열려 있으면 새 값으로 다시 강조한다.
        /// </summary>
        internal void OnRowRealized(DataGridRow row)
        {
            this.RestoreHighlightsUnder(row);

            if (!this.IsOpen || this.searchText.Length == 0)
            {
                return;
            }

            // LoadingRow 시점에는 셀이 아직 없다 — 레이아웃 후에 건다.
            this.grid.Dispatcher.BeginInvoke(
                    new Action(delegate
                    {
                        if (this.IsOpen && this.searchText.Length > 0)
                        {
                            this.ApplyHighlightToRow(row);
                        }
                    }),
                    DispatcherPriority.Loaded);
        }

        /// <summary>데이터(소스/뷰)가 바뀌었다 — 일치 목록을 무효화한다. 지금 위치는 다음 수집 때 선택 행 기준으로 다시 맞춘다.</summary>
        internal void OnDataChanged()
        {
            this.matchesValid = false;
            this.QueueRefresh();
        }

        /// <summary>컬럼이 다시 구성됐다 — 대상 컬럼·일치 목록을 무효화한다.</summary>
        internal void OnColumnsRebuilt()
        {
            this.columnsValid = false;
            this.matchesValid = false;
            this.lastCellColumn = null;
            this.RestoreAllHighlights();
            this.QueueRefresh();
        }

        // 변경 폭주를 디스패처 한 틱으로 접어 결과 목록/카운트/강조를 갱신한다.
        private void QueueRefresh()
        {
            if (!this.IsOpen || this.refreshQueued)
            {
                return;
            }

            this.refreshQueued = true;
            this.grid.Dispatcher.BeginInvoke(
                    new Action(this.RefreshAfterChange),
                    DispatcherPriority.Loaded);
        }

        private void RefreshAfterChange()
        {
            this.refreshQueued = false;

            if (!this.IsOpen)
            {
                return;
            }

            this.ClearResultsPanel();
            this.EnsureMatches();
            this.UpdateCountText();
            this.RefreshHighlights();
            this.RaiseHighlightChanged();
        }

        private void RaiseHighlightChanged()
        {
            EventHandler handler = this.HighlightChanged;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        // ===== 찾기 바 UI =====

        // [검색어────────ⓧ][2 of 19][↑][↓]                     [All][✕]
        // [결과 목록(All 을 켜면) — 클릭하면 그 셀로 이동]
        // 바탕은 카드 면색, 아래 1px 구분선 — 그리드 머리글과 같은 톤이라 한 몸으로 읽힌다. 바는 트리 안의 요소라
        // 열고 닫는 비용이 없고 그리드가 숨거나 가려지면 함께 숨는다. 이동 버튼은 개수 바로 옆에 두어 입력과 이동이
        // 한 묶음으로 읽히고, 목록·닫기만 오른쪽 끝에 둔다.
        private FrameworkElement BuildBar()
        {
            Brush secondary = (Brush)this.resourceSource.FindResource("Brush.TextSecondary");

            this.searchBox = new ModernTextBoxControl();
            this.searchBox.Placeholder = "Find in table";
            this.searchBox.ShowClearButton = true;
            this.searchBox.MaxWidth = searchBoxWidth;
            this.searchBox.MinWidth = searchBoxMinWidth;
            this.searchBox.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.searchBox.VerticalAlignment = VerticalAlignment.Center;
            this.searchBox.TextChanged += this.OnSearchTextChanged;

            this.countText = new TextBlock();
            this.countText.VerticalAlignment = VerticalAlignment.Center;
            this.countText.Margin = new Thickness(10d, 0d, 6d, 0d);
            this.countText.MinWidth = countTextMinWidth;
            this.countText.TextTrimming = TextTrimming.CharacterEllipsis;
            this.countText.Foreground = secondary;

            this.previousButton = this.BuildSmallButton("\uE70E", "Previous match (Shift+F3)");
            this.previousButton.Click += delegate
            {
                this.FindPrevious();
                this.FocusSearch();
            };

            this.nextButton = this.BuildSmallButton("\uE70D", "Next match (F3)");
            this.nextButton.Click += delegate
            {
                this.FindNext();
                this.FocusSearch();
            };

            this.allButton = new ModernButtonControl();
            this.allButton.Text = "All";
            this.allButton.Kind = ButtonKind.Subtle;
            this.allButton.FontSizeOverride = 12d;
            this.allButton.HeightOverride = smallButtonSize;
            this.allButton.Width = 48d;
            this.allButton.VerticalAlignment = VerticalAlignment.Center;
            this.allButton.ToolTip = "List every match";
            this.allButton.Click += delegate
            {
                this.ToggleResults();
                this.FocusSearch();
            };
            MakeNonFocusable(this.allButton);

            ModernButtonControl close = this.BuildSmallButton("\uE711", "Close (Esc)");
            close.Click += delegate { this.Close(); };

            // 왼쪽 묶음: 검색창(120~260, 남는 폭을 먼저 받는다) | 개수 | ↑ | ↓. 좁은 그리드(Lot List 등)에서 검색창과
            // 버튼이 잘리지 않게 — 둘이 못 들어가면 개수를 숨기고 검색창 툴팁으로 보인다("1…" 같은 말줄임을 보이지 않는다,
            // 2026-10-23 사용자 지적).
            Grid left = new Grid();
            left.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1d, GridUnitType.Star), MinWidth = searchBoxMinWidth, MaxWidth = searchBoxWidth });
            left.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            left.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            left.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(this.searchBox, 0);
            Grid.SetColumn(this.countText, 1);
            Grid.SetColumn(this.previousButton, 2);
            Grid.SetColumn(this.nextButton, 3);
            left.Children.Add(this.searchBox);
            left.Children.Add(this.countText);
            left.Children.Add(this.previousButton);
            left.Children.Add(this.nextButton);
            left.SizeChanged += delegate { this.UpdateCountVisibility(left.ActualWidth); };

            StackPanel buttons = new StackPanel();
            buttons.Orientation = Orientation.Horizontal;
            buttons.VerticalAlignment = VerticalAlignment.Center;
            buttons.Margin = new Thickness(8d, 0d, 0d, 0d);
            buttons.Children.Add(this.allButton);
            buttons.Children.Add(close);

            Grid row = new Grid();
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1d, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(left, 0);
            Grid.SetColumn(buttons, 1);
            row.Children.Add(left);
            row.Children.Add(buttons);

            Border barStrip = new Border();
            barStrip.Background = (Brush)this.resourceSource.FindResource("Brush.Surface");
            barStrip.BorderBrush = (Brush)this.resourceSource.FindResource("Brush.BorderSubtle");
            barStrip.BorderThickness = new Thickness(0d, 0d, 0d, 1d);
            barStrip.Padding = new Thickness(8d, 4d, 6d, 4d);
            barStrip.Child = row;

            this.resultsPanel = new StackPanel();
            this.resultsScroll = new ScrollViewer();
            this.resultsScroll.Content = this.resultsPanel;
            this.resultsScroll.MaxHeight = resultsMaxHeight;
            this.resultsScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            this.resultsScroll.Visibility = Visibility.Collapsed;

            // 목록을 눌러도 포커스는 검색창에 남는다 — 스크롤뷰어가 포커스를 가져가면 다음 Enter 가 먹힌다.
            this.resultsScroll.Focusable = false;

            Border resultsStrip = new Border();
            resultsStrip.Background = (Brush)this.resourceSource.FindResource("Brush.SurfaceAlt");
            resultsStrip.BorderBrush = (Brush)this.resourceSource.FindResource("Brush.BorderSubtle");
            resultsStrip.BorderThickness = new Thickness(0d, 0d, 0d, 1d);
            resultsStrip.Padding = new Thickness(8d, 2d, 8d, 2d);
            resultsStrip.Child = this.resultsScroll;
            resultsStrip.SetBinding(UIElement.VisibilityProperty, new Binding("Visibility") { Source = this.resultsScroll });

            StackPanel layout = new StackPanel();
            layout.Children.Add(barStrip);
            layout.Children.Add(resultsStrip);

            TextElement.SetFontSize(layout, (double)this.resourceSource.FindResource("Font.Size.Label"));
            TextElement.SetForeground(layout, (Brush)this.resourceSource.FindResource("Brush.TextPrimary"));
            this.UpdateCountText();
            return layout;
        }

        // 글리프만 있는 정사각 Subtle 버튼 — 이전·다음·닫기. 캡션 없이 IconGlyph 만 두고(FontSizeOverride 가 아이콘형 패딩 4 를 준다),
        // 캡션 옆 글리프에 붙는 오른쪽 여백 8 은 같은 어셈블리라 직접 걷어 글리프를 가운데 둔다.
        private ModernButtonControl BuildSmallButton(string glyph, string toolTip)
        {
            ModernButtonControl button = new ModernButtonControl();
            button.Text = string.Empty;
            button.IconGlyph = glyph;
            button.IconText.Margin = new Thickness(0d, 1d, 0d, 0d);
            button.FontSizeOverride = 12d;
            button.Kind = ButtonKind.Subtle;
            button.HeightOverride = smallButtonSize;
            button.Width = smallButtonSize + 2d;
            button.Margin = new Thickness(2d, 0d, 0d, 0d);
            button.VerticalAlignment = VerticalAlignment.Center;
            button.ToolTip = toolTip;
            MakeNonFocusable(button);
            return button;
        }

        // 바의 버튼은 포커스를 가져가지 않는다 — 누른 뒤에도 커서가 검색창에 남아 Enter 가 계속 이동한다.
        private static void MakeNonFocusable(ModernButtonControl button)
        {
            button.Focusable = false;
            button.IsTabStop = false;
            button.InnerButton.Focusable = false;
            button.InnerButton.IsTabStop = false;
        }

        // 닫고 표로 돌아간다 — Esc 와 ✕ 가 같은 길을 쓴다.
        private void Close()
        {
            this.Hide();
            this.FocusGridContent();
        }

        // 검색창에 커서를 둔다(버튼·목록을 누른 뒤).
        private void FocusSearch()
        {
            if (this.searchBox != null)
            {
                this.searchBox.FocusEditor();
            }
        }

        // 검색창에 커서를 두고 글자를 전부 선택한다 — 바로 치면 지난 검색어를 덮어 쓴다. 아직 포커스를 못 받았으면
        // (호스트가 막 보이게 된 경우 등) 입력 차례에 한 번 더 시도한다.
        private void FocusSearchSelectAll()
        {
            this.searchBox.FocusEditor();
            this.searchBox.SelectAllText();

            if (this.searchBox.IsKeyboardFocusWithin)
            {
                return;
            }

            this.grid.Dispatcher.BeginInvoke(
                    new Action(delegate
                    {
                        if (this.IsOpen && this.searchBox != null)
                        {
                            this.searchBox.FocusEditor();
                            this.searchBox.SelectAllText();
                        }
                    }),
                    DispatcherPriority.Input);
        }

        // 포커스를 찾은 셀(현재 셀)로 돌린다 — 현재 셀이 없으면 선택 행의 셀(바를 열기 전 칸, 없으면 첫 칸), 그것도 없으면
        // 그리드. 행(DataGridRow)은 Focusable=false 라 행 포커스는 거절되고, 표 셀에서 검색창으로 포커스가 가면 현재 셀이
        // 비워지므로(2026-10-30 측정) 열기 전 칸을 기억해 둔다. 그리드 자체 포커스 사각형은 XAML 에서 뺐다(2026-10-23 사용자 보고).
        private void FocusGridContent()
        {
            DataGridCellInfo current = this.grid.CurrentCell;
            bool hasCurrent = current.IsValid && current.Column != null && current.Item != null;
            object item = hasCurrent ? current.Item : this.grid.SelectedItem;
            DataGridColumn column = hasCurrent ? current.Column : this.lastCellColumn;

            if (column != null && !this.grid.Columns.Contains(column))
            {
                column = null;
            }

            if (column == null && this.grid.Columns.Count > 0)
            {
                column = this.grid.ColumnFromDisplayIndex(0);
            }

            if (item != null && column != null)
            {
                DataGridCell cell = FindAncestor<DataGridCell>(column.GetCellContent(item));

                if (cell != null && cell.Focus())
                {
                    return;
                }
            }

            this.grid.Focus();
        }

        // 좁은 그리드에서는 개수를 숨긴다 — 검색창이 120 아래로 줄거나 글자가 말줄임되는 것보다 낫다. 값은 검색창 툴팁에 남는다.
        private void UpdateCountVisibility(double leftWidth)
        {
            if (this.countText != null)
            {
                this.countText.Visibility = leftWidth >= countTextRoom ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        // All 토글 — 펼쳐져 있으면 접고, 아니면 일치 목록을 만든다.
        private void ToggleResults()
        {
            if (this.resultsScroll.Visibility == Visibility.Visible)
            {
                this.ClearResultsPanel();
                return;
            }

            this.FindAll();
        }

        // 검색어 변경 — 일치를 무효화하고, 보이는 행의 강조와 건수를 즉시 갱신한다
        // (이동은 Enter/Find Next가 한다).
        private void OnSearchTextChanged(object sender, EventArgs e)
        {
            this.searchText = this.searchBox.Text ?? string.Empty;
            this.matchesValid = false;
            this.currentIndex = -1;
            this.ClearResultsPanel();
            this.EnsureMatches();
            this.UpdateCountText();
            this.RefreshHighlights();
            this.RaiseHighlightChanged();
        }

        // 개수 표기 — 이동했으면 "i of N", 아니면 "N results", 없으면 "No results"(검색어 없으면 빈 값). N 은 일치 행 수다.
        // 좁은 그리드에서 개수를 숨겨도 읽을 수 있게 검색창 툴팁을 늘 같은 값으로 맞추고, 일치가 없으면 ↑↓ 를 끈다.
        private void UpdateCountText()
        {
            if (this.countText == null)
            {
                return;
            }

            int rows = this.rowStops.Count;
            string text;

            if (this.searchText.Length == 0)
            {
                text = string.Empty;
            }
            else if (rows == 0)
            {
                text = "No results";
            }
            else
            {
                int position = this.RowPositionOf(this.currentIndex);

                if (position >= 0)
                {
                    text = string.Format("{0:N0} of {1:N0}", position + 1, rows);
                }
                else
                {
                    text = rows == 1 ? "1 result" : string.Format("{0:N0} results", rows);
                }
            }

            this.countText.Text = text;
            this.searchBox.ToolTip = text.Length > 0 ? text : null;
            this.previousButton.IsEnabled = rows > 0;
            this.nextButton.IsEnabled = rows > 0;
        }

        // ===== Find All 결과 목록 =====

        private void FindAll()
        {
            this.EnsureMatches();
            this.ClearResultsPanel();
            this.UpdateCountText();

            if (this.matches.Count == 0)
            {
                this.resultsScroll.Visibility = Visibility.Collapsed;
                return;
            }

            this.allButton.Kind = ButtonKind.Secondary;
            Brush secondary = (Brush)this.resourceSource.FindResource("Brush.TextSecondary");
            Brush hover = (Brush)this.resourceSource.FindResource("Brush.HoverBackground");
            int shown = 0;

            foreach (FindMatch match in this.matches)
            {
                if (shown >= maxResultItems)
                {
                    TextBlock more = new TextBlock();
                    more.Text = string.Format("… {0:N0} more", this.matches.Count - shown);
                    more.Foreground = secondary;
                    more.Margin = new Thickness(4d, 2d, 4d, 2d);
                    this.resultsPanel.Children.Add(more);
                    break;
                }

                this.resultsPanel.Children.Add(this.BuildResultItem(match, secondary, hover));
                shown = shown + 1;
            }

            this.resultsScroll.Visibility = Visibility.Visible;
        }

        // 결과 한 줄: "Row 3 · Lot ID · IT10003ED" — 클릭하면 그 셀로 이동하고 커서는 검색창에 남는다.
        private UIElement BuildResultItem(FindMatch match, Brush secondary, Brush hoverBackground)
        {
            TextBlock text = new TextBlock();
            text.TextTrimming = TextTrimming.CharacterEllipsis;
            text.Inlines.Add(new Run(string.Format("Row {0:N0} · ", match.RowIndex + 1)) { Foreground = secondary });
            text.Inlines.Add(new Run(match.Column.Caption + " · ") { Foreground = secondary });
            text.Inlines.Add(new Run(match.CellText));

            Border item = new Border();
            item.Padding = new Thickness(4d, 3d, 4d, 3d);
            item.Background = Brushes.Transparent;
            item.Cursor = Cursors.Hand;
            item.Child = text;

            item.MouseEnter += delegate { item.Background = hoverBackground; };
            item.MouseLeave += delegate { item.Background = Brushes.Transparent; };
            item.MouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs e)
            {
                e.Handled = true;
                int index = this.matches.IndexOf(match);

                if (index >= 0)
                {
                    this.MoveTo(index);
                }

                this.FocusSearch();
            };

            return item;
        }

        private void ClearResultsPanel()
        {
            if (this.resultsPanel != null)
            {
                this.resultsPanel.Children.Clear();
                this.resultsScroll.Visibility = Visibility.Collapsed;
                this.allButton.Kind = ButtonKind.Subtle;
            }
        }

        // ===== 일치 수집 =====

        // 대상 컬럼 목록을 굳힌다 — 텍스트를 담은 컬럼(Text/Link/Badge)만.
        // 자동 생성 그리드(정의 없음)는 바인딩 경로가 있는 컬럼 전부가 대상이다.
        private void EnsureSearchColumns()
        {
            if (this.columnsValid)
            {
                return;
            }

            this.searchColumns.Clear();
            IList<ModernDataGridColumn> definitions = this.definitionsGetter();

            for (int index = 0; index < this.grid.Columns.Count; index++)
            {
                DataGridColumn column = this.grid.Columns[index];
                ModernDataGridColumn definition =
                        definitions != null && index < definitions.Count ? definitions[index] : null;
                string memberPath;
                string format = string.Empty;
                string caption = null;
                bool merged = false;

                if (definition != null)
                {
                    bool searchable = definition.Kind == GridColumnKind.Text
                            || definition.Kind == GridColumnKind.Link
                            || definition.Kind == GridColumnKind.Badge;

                    if (!searchable || string.IsNullOrEmpty(definition.DataPropertyName))
                    {
                        continue;
                    }

                    memberPath = definition.DataPropertyName;
                    format = definition.Format;
                    caption = definition.PlainHeaderText;
                    merged = definition.MergeCells;
                }
                else
                {
                    DataGridBoundColumn bound = column as DataGridBoundColumn;
                    Binding binding = bound != null ? bound.Binding as Binding : null;

                    if (binding == null || binding.Path == null || string.IsNullOrEmpty(binding.Path.Path))
                    {
                        continue;
                    }

                    memberPath = binding.Path.Path;
                    caption = column.Header as string;
                }

                if (string.IsNullOrEmpty(caption))
                {
                    caption = memberPath;
                }

                this.searchColumns.Add(new SearchColumn(
                        column, memberPath, format, caption.Replace("\n", " "), merged));
            }

            this.columnsValid = true;
        }

        // 현재 뷰(정렬·컬럼 필터 반영)의 행 순서 × 컬럼 순서로 일치를 모은다. 병합 칸은 바로 위 행과 값이 같으면
        // 같은 구간의 이어진 칸이라 세지 않는다 — 구간 첫 행이 그 구간을 대표한다(병합 오버레이와 같은 판정).
        // 이동한 적이 있으면 새 목록에서 지금 선택 행으로 위치를 다시 맞춘다(재조회·정렬·필터 뒤 이어 찾기).
        private void EnsureMatches()
        {
            if (this.matchesValid)
            {
                return;
            }

            bool hadPosition = this.currentIndex >= 0;
            this.EnsureSearchColumns();
            this.matches.Clear();
            this.rowStops.Clear();

            if (this.searchText.Length > 0)
            {
                CultureInfo culture = CellCultureOf(this.grid);
                object previousItem = null;

                for (int rowIndex = 0; rowIndex < this.grid.Items.Count; rowIndex++)
                {
                    object item = this.grid.Items[rowIndex];

                    if (item == CollectionView.NewItemPlaceholder)
                    {
                        previousItem = null;
                        continue;
                    }

                    int stop = -1;

                    foreach (SearchColumn column in this.searchColumns)
                    {
                        object value = MemberPathReader.Read(item, column.MemberPath);

                        if (column.Merged && previousItem != null
                                && AreSameMergeValue(value, MemberPathReader.Read(previousItem, column.MemberPath)))
                        {
                            continue;
                        }

                        string cellText = CellDisplayText(value, column.Format, culture);

                        if (cellText.IndexOf(this.searchText, StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            continue;
                        }

                        if (stop < 0)
                        {
                            stop = this.matches.Count;
                            this.rowStops.Add(stop);
                        }

                        this.matches.Add(new FindMatch(item, rowIndex, column, cellText));
                    }

                    previousItem = item;
                }
            }

            this.matchesValid = true;
            this.currentIndex = hadPosition ? this.StopOfItem(this.grid.SelectedItem) : -1;
        }

        // 병합 판정용 값 비교 — DataTable의 빈 값(DBNull)과 null을 같게 본다(병합 오버레이와 같은 규칙).
        private static bool AreSameMergeValue(object left, object right)
        {
            object normalizedLeft = left == DBNull.Value ? null : left;
            object normalizedRight = right == DBNull.Value ? null : right;

            return object.Equals(normalizedLeft, normalizedRight);
        }

        /// <summary>
        /// 셀 바인딩이 쓰는 문화권 — 바인딩은 대상 요소의 Language(기본 en-US)로 StringFormat 과 기본 변환을 한다.
        /// 찾기 기준 글자를 이 문화권으로 만들어야 셀에 보이는 글자와 같다(서식 없는 날짜 칸, 2026-10-30 측정).
        /// </summary>
        internal static CultureInfo CellCultureOf(FrameworkElement element)
        {
            XmlLanguage language = element == null ? null : element.Language;

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

        /// <summary>셀에 보이는 글자 — Format 이 있으면 "{0:Format}", 없으면 기본 문자열 변환(둘 다 셀 문화권).</summary>
        internal static string CellDisplayText(object value, string format, CultureInfo culture)
        {
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            object typed = GridAutoFitMeasurer.CoerceForFormat(value, format);

            if (!string.IsNullOrEmpty(format))
            {
                try
                {
                    return string.Format(culture, "{0:" + format + "}", typed);
                }
                catch (FormatException)
                {
                    // 형식 오류는 기본 표현으로 폴백한다 (바인딩 표시와 동일한 완화).
                }
            }

            return Convert.ToString(typed, culture) ?? string.Empty;
        }

        // ===== 이동 =====

        private void Step(bool forward)
        {
            this.EnsureMatches();

            if (this.rowStops.Count == 0)
            {
                this.UpdateCountText();
                return;
            }

            int anchorRow = this.AnchorRow();
            this.MoveTo(forward ? this.NextStop(anchorRow) : this.PreviousStop(anchorRow));
        }

        // 이어 찾기의 기준 행 — 지금 선택 행(뷰 순서). 선택이 없으면 -1(처음부터).
        private int AnchorRow()
        {
            object selected = this.grid.SelectedItem;

            if (selected == null)
            {
                return -1;
            }

            FindMatch current = this.CurrentMatch;

            if (current != null && SameItem(current.Item, selected))
            {
                return current.RowIndex;
            }

            return this.grid.Items.IndexOf(selected);
        }

        private int NextStop(int anchorRow)
        {
            foreach (int stop in this.rowStops)
            {
                if (this.matches[stop].RowIndex > anchorRow)
                {
                    return stop;
                }
            }

            return this.rowStops[0];
        }

        private int PreviousStop(int anchorRow)
        {
            for (int index = this.rowStops.Count - 1; index >= 0; index--)
            {
                int stop = this.rowStops[index];

                if (this.matches[stop].RowIndex < anchorRow)
                {
                    return stop;
                }
            }

            return this.rowStops[this.rowStops.Count - 1];
        }

        // 행 항목의 이동 단위(그 행 첫 일치) — 없으면 -1.
        private int StopOfItem(object item)
        {
            if (item == null)
            {
                return -1;
            }

            foreach (int stop in this.rowStops)
            {
                if (SameItem(this.matches[stop].Item, item))
                {
                    return stop;
                }
            }

            return -1;
        }

        // 일치(칸)가 속한 행이 몇 번째 일치 행인지 — 개수 표기용. 없으면 -1.
        private int RowPositionOf(int matchIndex)
        {
            if (matchIndex < 0 || matchIndex >= this.matches.Count)
            {
                return -1;
            }

            int row = this.matches[matchIndex].RowIndex;
            int low = 0;
            int high = this.rowStops.Count - 1;

            while (low <= high)
            {
                int middle = (low + high) / 2;
                int middleRow = this.matches[this.rowStops[middle]].RowIndex;

                if (middleRow == row)
                {
                    return middle;
                }

                if (middleRow < row)
                {
                    low = middle + 1;
                }
                else
                {
                    high = middle - 1;
                }
            }

            return -1;
        }

        // 일치 하나로 이동하고 위치·강조를 갱신한다. 화면이 선택을 되돌리면(조회 중 선택 고정 등) 위치·개수를
        // 앞으로 옮기지 않는다.
        private void MoveTo(int index)
        {
            FindMatch target = this.matches[index];
            FindMatch previous = this.CurrentMatch;

            if (!this.NavigateTo(target))
            {
                this.UpdateCountText();
                return;
            }

            this.currentIndex = index;
            this.UpdateCountText();
            this.RefreshHighlightOf(previous);

            if (previous == null || !SameItem(previous.Item, target.Item))
            {
                this.RefreshHighlightOf(target);
            }

            this.RaiseHighlightChanged();
        }

        // 일치 칸으로 이동 — 행 선택 + 스크롤 + 현재 셀 지정. 포커스는 검색창에
        // 남겨 Enter 연타로 계속 이동할 수 있다 (엑셀 동작). 선택이 대상이 아니게 끝나면(화면이 되돌림) false.
        private bool NavigateTo(FindMatch match)
        {
            this.grid.SelectedItem = match.Item;

            if (!SameItem(this.grid.SelectedItem, match.Item))
            {
                return false;
            }

            this.ScrollToMatch(match);
            this.lastCellColumn = match.Column.Column;

            try
            {
                this.grid.CurrentCell = new DataGridCellInfo(match.Item, match.Column.Column);
            }
            catch (Exception)
            {
                // 이동 사이에 소스가 바뀌어 항목이 사라진 경우 — 이동만 포기한다.
            }

            return true;
        }

        // 찾은 행이 화면 밖이거나 위·아래 가장자리 두 줄 안이면 가운데쯤으로 옮긴다 — 픽셀 단위 스크롤에서
        // ScrollIntoView 는 모자란 만큼만 움직여 찾은 행이 늘 맨 아래 줄에 붙고, 이웃 행이 연달아 일치하면 선택 띠가
        // 제자리에 있는 것처럼 보였다(2026-10-30 측정). 찾기 이동에만 쓰고 키보드·마우스의 최소 스크롤 규칙은 그대로다.
        // 행 높이가 고정이라 위치는 "행 번호 × 행 높이"로 계산한다(병합 오버레이와 같은 전제).
        private void ScrollToMatch(FindMatch match)
        {
            ScrollViewer viewer = this.FindScrollViewer();
            double rowHeight = this.grid.RowHeight;

            if (viewer != null && !double.IsNaN(rowHeight) && rowHeight > 0d && viewer.ViewportHeight > 0d)
            {
                double rowTop = match.RowIndex * rowHeight;
                double top = viewer.VerticalOffset;
                double bottom = top + viewer.ViewportHeight;
                double edge = edgeRows * rowHeight;

                if (rowTop < top + edge || rowTop + rowHeight > bottom - edge)
                {
                    double target = rowTop - ((viewer.ViewportHeight - rowHeight) / 2d);
                    target = Math.Max(0d, Math.Min(target, viewer.ScrollableHeight));

                    if (Math.Abs(target - top) >= 0.5d)
                    {
                        viewer.ScrollToVerticalOffset(target);
                        viewer.UpdateLayout();
                    }
                }
            }

            this.grid.ScrollIntoView(match.Item, match.Column.Column);
        }

        // DataGrid 템플릿 안의 스크롤뷰어 — 병합 오버레이·스크롤 복원과 같은 이름으로 찾는다.
        private ScrollViewer FindScrollViewer()
        {
            if (this.grid.Template == null)
            {
                return null;
            }

            return this.grid.Template.FindName("DG_ScrollViewer", this.grid) as ScrollViewer;
        }

        // ===== 일치 텍스트 강조 =====

        // 실체화된 모든 행의 강조를 현재 검색어로 다시 건다 (없으면 걷는다).
        private void RefreshHighlights()
        {
            if (!this.IsOpen || this.searchText.Length == 0)
            {
                this.RestoreAllHighlights();
                return;
            }

            for (int index = 0; index < this.grid.Items.Count; index++)
            {
                DataGridRow row = this.grid.ItemContainerGenerator.ContainerFromIndex(index) as DataGridRow;

                if (row != null)
                {
                    this.ApplyHighlightToRow(row);
                }
            }
        }

        // 일치 하나가 든 행의 강조만 다시 건다 — 지금 일치가 옮겨 갈 때 옛 행(주황 → 노랑)과 새 행(노랑 → 주황).
        private void RefreshHighlightOf(FindMatch match)
        {
            if (match == null)
            {
                return;
            }

            DataGridRow row = this.grid.ItemContainerGenerator.ContainerFromItem(match.Item) as DataGridRow;

            if (row != null)
            {
                this.ApplyHighlightToRow(row);
            }
        }

        // 한 행의 검색 대상 셀에서 TextBlock을 찾아 일치 구간을 강조한다. 지금 일치 칸은 주황이다.
        private void ApplyHighlightToRow(DataGridRow row)
        {
            object item = this.grid.ItemContainerGenerator.ItemFromContainer(row);

            if (item == null || item == DependencyProperty.UnsetValue)
            {
                return;
            }

            this.EnsureSearchColumns();
            FindMatch current = this.CurrentMatch;
            bool currentRow = current != null && SameItem(current.Item, item);

            foreach (SearchColumn column in this.searchColumns)
            {
                FrameworkElement content = column.Column.GetCellContent(row);

                if (content != null)
                {
                    this.HighlightTextBlocks(content, currentRow && current.Column.Column == column.Column);
                }
            }
        }

        // 셀 내용 아래의 TextBlock을 전부 찾아 강조를 적용/해제한다
        // (Text 셀은 내용 자체, Link/Badge 셀은 템플릿 안에 있다).
        private void HighlightTextBlocks(DependencyObject root, bool isCurrent)
        {
            TextBlock textBlock = root as TextBlock;

            if (textBlock != null)
            {
                this.ApplyInlineHighlight(textBlock, isCurrent);
                return;
            }

            int count = VisualTreeHelper.GetChildrenCount(root);

            for (int index = 0; index < count; index++)
            {
                this.HighlightTextBlocks(VisualTreeHelper.GetChild(root, index), isCurrent);
            }
        }

        // TextBlock의 Inlines를 [일반][강조][일반]… Run으로 재구성한다. 이미 강조돼 있으면 원래 Text 출처를 먼저
        // 되돌려(바인딩이면 지금 행의 값으로 다시 평가된다) 그 글자로 판정한다.
        private void ApplyInlineHighlight(TextBlock textBlock, bool isCurrent)
        {
            if ((bool)textBlock.GetValue(IsHighlightedProperty))
            {
                this.RestoreText(textBlock);
            }

            string keyword = this.searchText;
            string text = textBlock.Text ?? string.Empty;

            if (keyword.Length == 0 || text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) < 0)
            {
                return;
            }

            RememberTextOrigin(textBlock);
            textBlock.SetValue(IsHighlightedProperty, true);
            this.highlightedBlocks.Add(textBlock);
            FillHighlightRuns(textBlock, text, keyword, isCurrent, this.resourceSource);
        }

        /// <summary>
        /// TextBlock 글자를 [일반][강조][일반]… Run 으로 채운다 — 강조 Run 은 배경(일치 노랑 · 지금 일치 주황)과 글자색
        /// (Brush.OnFindMatch)을 함께 걸어 어떤 테마·선택 행에서도 읽힌다. 병합 스팬 글자도 이 문법을 쓴다.
        /// </summary>
        internal static void FillHighlightRuns(
                TextBlock textBlock, string text, string keyword, bool isCurrent, FrameworkElement resourceSource)
        {
            Brush background = (Brush)resourceSource.FindResource(isCurrent ? "Brush.FindMatchCurrent" : "Brush.FindMatch");
            Brush foreground = (Brush)resourceSource.FindResource("Brush.OnFindMatch");

            textBlock.Inlines.Clear();

            int position = 0;

            while (position < text.Length)
            {
                int matchIndex = text.IndexOf(keyword, position, StringComparison.OrdinalIgnoreCase);

                if (matchIndex < 0)
                {
                    textBlock.Inlines.Add(new Run(text.Substring(position)));
                    break;
                }

                if (matchIndex > position)
                {
                    textBlock.Inlines.Add(new Run(text.Substring(position, matchIndex - position)));
                }

                textBlock.Inlines.Add(new Run(text.Substring(matchIndex, keyword.Length))
                {
                    Background = background,
                    Foreground = foreground
                });
                position = matchIndex + keyword.Length;
            }
        }

        // 강조 전 Text 의 출처를 보관한다 — 로컬 바인딩이면 그 바인딩, 로컬 글자면 그 글자, 그 밖(템플릿·스타일)은 로컬 값을 지워 되돌린다.
        private static void RememberTextOrigin(TextBlock textBlock)
        {
            ValueSource source = DependencyPropertyHelper.GetValueSource(textBlock, TextBlock.TextProperty);
            TextOrigin origin;

            if (source.BaseValueSource == BaseValueSource.Local)
            {
                BindingBase binding = BindingOperations.GetBindingBase(textBlock, TextBlock.TextProperty);

                origin = binding != null
                        ? new TextOrigin(binding, null, false)
                        : new TextOrigin(null, textBlock.Text, true);
            }
            else
            {
                origin = new TextOrigin(null, null, false);
            }

            textBlock.SetValue(TextOriginProperty, origin);
        }

        // 강조 해제 — 보관한 출처로 Text 를 되돌린다(바인딩이면 다시 걸어 지금 행의 값과 서식으로 그린다).
        private void RestoreText(TextBlock textBlock)
        {
            this.highlightedBlocks.Remove(textBlock);
            TextOrigin origin = textBlock.GetValue(TextOriginProperty) as TextOrigin;
            textBlock.ClearValue(TextOriginProperty);
            textBlock.ClearValue(IsHighlightedProperty);

            if (origin == null)
            {
                return;
            }

            if (origin.Binding != null)
            {
                BindingOperations.SetBinding(textBlock, TextBlock.TextProperty, origin.Binding);
            }
            else if (origin.IsLocalText)
            {
                textBlock.Text = origin.Text;
            }
            else
            {
                textBlock.ClearValue(TextBlock.TextProperty);
            }
        }

        // 걸어 둔 강조를 전부 되돌린다 — 화면 밖 재활용 대기 행의 TextBlock 까지.
        private void RestoreAllHighlights()
        {
            if (this.highlightedBlocks.Count == 0)
            {
                return;
            }

            List<TextBlock> blocks = new List<TextBlock>(this.highlightedBlocks);

            foreach (TextBlock textBlock in blocks)
            {
                this.RestoreText(textBlock);
            }

            this.highlightedBlocks.Clear();
        }

        // 한 행 안에 걸어 둔 강조만 되돌린다 — 행이 재활용되거나 다시 실체화될 때.
        private void RestoreHighlightsUnder(DataGridRow row)
        {
            if (this.highlightedBlocks.Count == 0 || row == null)
            {
                return;
            }

            List<TextBlock> under = null;

            foreach (TextBlock textBlock in this.highlightedBlocks)
            {
                if (row.IsAncestorOf(textBlock))
                {
                    if (under == null)
                    {
                        under = new List<TextBlock>();
                    }

                    under.Add(textBlock);
                }
            }

            if (under == null)
            {
                return;
            }

            foreach (TextBlock textBlock in under)
            {
                this.RestoreText(textBlock);
            }
        }

        private void OnRowUnloading(object sender, DataGridRowEventArgs e)
        {
            this.RestoreHighlightsUnder(e.Row);
        }

        // ===== 보조 =====

        // 행 항목 동일성 — DataRowView 등은 참조로, 값 형식 항목은 Equals 로 비교한다.
        private static bool SameItem(object left, object right)
        {
            return object.ReferenceEquals(left, right) || (left != null && left.Equals(right));
        }

        // 셀 편집 중인지 — 키를 받은 요소가 편집 중인 셀 안에 있으면 Esc 는 편집 취소가 먼저다.
        private static bool IsEditingCell(DependencyObject source)
        {
            DataGridCell cell = FindAncestor<DataGridCell>(source);
            return cell != null && cell.IsEditing;
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
    }
}
