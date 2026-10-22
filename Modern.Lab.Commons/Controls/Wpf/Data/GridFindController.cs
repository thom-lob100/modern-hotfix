using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Modern.Lab.Controls.Wpf.Common;
using Modern.Lab.Controls.Wpf.Input;

namespace Modern.Lab.Controls.Wpf.Data
{
    /// <summary>
    /// ModernDataGridControl의 찾기(Ctrl+F) 컨트롤러 — 행을 거르는 필터가 아니라 **일치 위치로 이동**한다:
    /// - 그리드 <b>머리글 위에 끼어드는 인라인 찾기 바</b>(🔍 검색어 · 일치 수 · 이전/다음 · All · ✕) — 표는 그만큼 내려가고
    ///   셀을 가리지 않는다. 바는 그리드 비주얼 트리 안의 요소라 별도 hwnd 가 없다.
    /// - Find Next(검색창 Enter/F3/↓): 다음 일치 셀로 선택·스크롤 이동 (끝나면 처음부터)
    /// - Shift+F3 / Shift+Enter / ↑: 이전 일치로 이동 (표준 찾기 단축키)
    /// - All: 일치 목록을 바 아래에 펼친다 — 항목을 클릭하면 그 셀로 이동
    /// - 화면에 실체화된 행의 일치 텍스트는 노란 배경(Brush.WarningBorder)으로 강조
    ///
    /// 2026-08-02 부터 2026-10-22 까지는 호스트 최상위 창에 소유된 별도 Window(엑셀식 모덜리스 찾기 창)였다 —
    /// 별도 창이라 셸에서 화면을 바꿔도 남고(2026-08-07 · 08-25 현장 보고) 첫 열기에 창 생성 비용이 들어,
    /// 사용자 결정(2026-10-23)으로 그리드 안의 바가 됐다. 가시성·z-순서 감시와 소유자 hwnd 연결은 더 필요 없다.
    ///
    /// 일치 판정은 화면 표시 텍스트 기준(Format 적용, 부분 일치, 대소문자 무시)이고
    /// 대상은 텍스트를 담은 컬럼(Text/Link/Badge)이다. 검색 순서는 현재 뷰의
    /// 행 순서(정렬·컬럼 필터 반영) × 컬럼 순서다.
    ///
    /// 텍스트 강조는 셀 TextBlock의 Inlines만 바꾼다 — Text 속성(바인딩)은
    /// 건드리지 않으므로, 컨테이너 재활용/재바인딩 시 바인딩이 평문을 다시
    /// 밀어 넣어 강조가 저절로 걷힌다. 명시적 해제는 바인딩 UpdateTarget으로 한다.
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

        // 결과 목록 표시 상한 — 일치가 비정상적으로 많을 때 창이 멈추는 것만 막는다.
        private const int maxResultItems = 500;

        // 바 안 요소 치수 — 검색창 폭, 작은 버튼 한 변, 결과 목록 최대 높이.
        private const double searchBoxWidth = 260d;
        private const double searchBoxMinWidth = 120d;
        private const double smallButtonSize = 28d;
        private const double resultsMaxHeight = 160d;

        // 글리프 칸 + 검색창 최소 폭 + 일치 수 글자 폭 — 이보다 좁으면 일치 수를 숨긴다.
        private const double countTextRoom = 22d + 8d + searchBoxMinWidth + 10d + 88d;

        // 검색 대상 컬럼 하나 — 그리드 컬럼과 값 읽기 경로/서식/캡션의 묶음.
        private sealed class SearchColumn
        {
            internal readonly DataGridColumn Column;
            internal readonly string MemberPath;
            internal readonly string Format;
            internal readonly string Caption;

            internal SearchColumn(DataGridColumn column, string memberPath, string format, string caption)
            {
                this.Column = column;
                this.MemberPath = memberPath;
                this.Format = format;
                this.Caption = caption;
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

        private readonly DataGrid grid;
        private readonly FrameworkElement resourceSource;
        private readonly Func<IList<ModernDataGridColumn>> definitionsGetter;

        private readonly Border host;
        private FrameworkElement bar;
        private ModernButtonControl allButton;
        private string rememberedText = string.Empty;
        private ModernTextBoxControl searchBox;
        private TextBlock countText;
        private StackPanel resultsPanel;
        private ScrollViewer resultsScroll;

        private readonly List<SearchColumn> searchColumns = new List<SearchColumn>();
        private readonly List<FindMatch> matches = new List<FindMatch>();
        private string searchText = string.Empty;
        private bool matchesValid;
        private bool columnsValid;
        private int currentIndex = -1;

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
        }

        /// <summary>찾기 바가 열려 있는지.</summary>
        internal bool IsOpen
        {
            get { return this.bar != null && this.host.Visibility == Visibility.Visible; }
        }

        /// <summary>찾기 바를 열고(이미 열려 있으면 검색창 재포커스) 지난 검색어가 있으면 되살린다.</summary>
        internal void Show()
        {
            if (this.bar == null)
            {
                this.bar = this.BuildBar();
                this.host.Child = this.bar;
            }

            bool reopened = !this.IsOpen;
            this.host.Visibility = Visibility.Visible;

            if (reopened && this.rememberedText.Length > 0)
            {
                // 지난 검색어 복원 — TextChanged 경로가 일치 수집과 강조까지 처리한다.
                this.searchBox.Text = this.rememberedText;
            }

            this.searchBox.FocusEditor();
        }

        /// <summary>찾기 바를 닫고 강조를 걷는다. 검색어는 세션 동안 기억해 다시 열면 되살린다.</summary>
        internal void Hide()
        {
            if (!this.IsOpen)
            {
                return;
            }

            this.rememberedText = this.searchText;

            if (this.searchText.Length > 0 && this.searchBox != null)
            {
                // 검색어를 비우면 TextChanged 경로가 강조 해제까지 처리한다.
                this.searchBox.Text = string.Empty;
            }

            this.host.Visibility = Visibility.Collapsed;
        }

        // 바를 버린다 — 호스트 최종 폐기 경로. 다음 Show 가 새로 만든다.
        internal void Release()
        {
            this.Hide();
            this.host.Child = null;
            this.bar = null;
            this.searchBox = null;
            this.countText = null;
            this.resultsPanel = null;
            this.resultsScroll = null;
            this.allButton = null;
            this.searchText = string.Empty;
            this.rememberedText = string.Empty;
        }

        /// <summary>다음 일치로 이동한다 (끝나면 처음부터). 검색창 Enter/F3과 동일.</summary>
        internal void FindNext()
        {
            this.EnsureMatches();

            if (this.matches.Count == 0)
            {
                this.UpdateCountText(null);
                return;
            }

            this.currentIndex = (this.currentIndex + 1) % this.matches.Count;
            this.NavigateTo(this.matches[this.currentIndex]);
            this.UpdateCountText(this.currentIndex);
        }

        /// <summary>이전 일치로 이동한다 (처음이면 끝으로). Shift+F3/Shift+Enter와 동일.</summary>
        internal void FindPrevious()
        {
            this.EnsureMatches();

            if (this.matches.Count == 0)
            {
                this.UpdateCountText(null);
                return;
            }

            this.currentIndex = this.currentIndex <= 0
                    ? this.matches.Count - 1
                    : this.currentIndex - 1;
            this.NavigateTo(this.matches[this.currentIndex]);
            this.UpdateCountText(this.currentIndex);
        }

        /// <summary>행이 화면에 실체화될 때(가상화) 그 행의 일치 텍스트를 강조한다.</summary>
        internal void OnRowRealized(DataGridRow row)
        {
            if (!this.IsOpen || this.searchText.Length == 0)
            {
                return;
            }

            // LoadingRow 시점에는 셀이 아직 없다 — 레이아웃 후에 건다.
            this.grid.Dispatcher.BeginInvoke(
                    new Action(delegate { this.ApplyHighlightToRow(row); }),
                    System.Windows.Threading.DispatcherPriority.Loaded);
        }

        /// <summary>데이터(소스/뷰)가 바뀌었다 — 일치 목록·현재 위치를 무효화한다.</summary>
        internal void OnDataChanged()
        {
            this.matchesValid = false;
            this.currentIndex = -1;
            this.QueueRefresh();
        }

        /// <summary>컬럼이 다시 구성됐다 — 대상 컬럼·일치 목록을 무효화한다.</summary>
        internal void OnColumnsRebuilt()
        {
            this.columnsValid = false;
            this.matchesValid = false;
            this.currentIndex = -1;
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
                    System.Windows.Threading.DispatcherPriority.Loaded);
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
            this.UpdateCountText(null);
            this.RefreshHighlights();
        }

        // ===== 찾기 바 UI =====

        // [🔍][검색어────────][n / N]   [↑][↓][All][✕]
        // [결과 목록(All 을 켜면) — 클릭하면 그 셀로 이동]
        // 바탕은 카드 면색, 아래 1px 구분선 — 그리드 머리글과 같은 톤이라 한 몸으로 읽힌다. 바는 트리 안의 요소라
        // 열고 닫는 비용이 없고 그리드가 숨거나 가려지면 함께 숨는다.
        private FrameworkElement BuildBar()
        {
            Brush secondary = (Brush)this.resourceSource.FindResource("Brush.TextSecondary");

            TextBlock searchGlyph = new TextBlock();
            searchGlyph.Text = "\uE721";
            searchGlyph.FontFamily = (FontFamily)this.resourceSource.FindResource("Font.Family.Icon");
            searchGlyph.FontSize = (double)this.resourceSource.FindResource("Font.Size.Icon");
            searchGlyph.Foreground = secondary;
            searchGlyph.VerticalAlignment = VerticalAlignment.Center;
            searchGlyph.Margin = new Thickness(2d, 1d, 8d, 0d);

            this.searchBox = new ModernTextBoxControl();
            this.searchBox.Placeholder = "Find in table";
            this.searchBox.MaxWidth = searchBoxWidth;
            this.searchBox.MinWidth = searchBoxMinWidth;
            this.searchBox.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.searchBox.VerticalAlignment = VerticalAlignment.Center;
            this.searchBox.TextChanged += this.OnSearchTextChanged;
            this.searchBox.EnterPressed += delegate { this.FindNext(); };

            this.countText = new TextBlock();
            this.countText.VerticalAlignment = VerticalAlignment.Center;
            this.countText.Margin = new Thickness(10d, 0d, 8d, 0d);
            this.countText.TextTrimming = TextTrimming.CharacterEllipsis;
            this.countText.Foreground = secondary;

            ModernButtonControl previous = this.BuildSmallButton("\uE70E", "Previous match (Shift+F3)");
            previous.Click += delegate { this.FindPrevious(); };

            ModernButtonControl next = this.BuildSmallButton("\uE70D", "Next match (F3)");
            next.Click += delegate { this.FindNext(); };

            this.allButton = new ModernButtonControl();
            this.allButton.Text = "All";
            this.allButton.Kind = ButtonKind.Secondary;
            this.allButton.FontSizeOverride = 12d;
            this.allButton.HeightOverride = smallButtonSize;
            this.allButton.Width = 48d;
            this.allButton.Margin = new Thickness(8d, 0d, 0d, 0d);
            this.allButton.VerticalAlignment = VerticalAlignment.Center;
            this.allButton.ToolTip = "List every match";
            this.allButton.Click += delegate { this.ToggleResults(); };

            ModernButtonControl close = this.BuildSmallButton("\uE711", "Close (Esc)");
            close.Margin = new Thickness(8d, 0d, 0d, 0d);
            close.Click += delegate { this.Hide(); this.FocusGridContent(); };

            // 왼쪽 묶음: 글리프 | 검색창(120~260, 남는 폭을 먼저 받는다) | 일치 수(남는 폭, 좁으면 말줄임·숨김).
            // 오른쪽 묶음: 버튼 넷(고정). 좁은 그리드(Lot List 등)에서 검색창과 버튼이 잘리지 않게 — DockPanel 고정 폭이던 때의
            // 2026-10-23 사용자 보고.
            // 검색창이 남는 폭을 먼저 받고(120~260) 일치 수는 그 뒤 Auto — 좁아서 둘이 못 들어가면 일치 수를 숨기고
            // 검색창 툴팁으로 보인다("1…" 같은 말줄임을 보이지 않는다, 2026-10-23 사용자 지적).
            Grid left = new Grid();
            left.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            left.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1d, GridUnitType.Star), MinWidth = searchBoxMinWidth, MaxWidth = searchBoxWidth });
            left.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(searchGlyph, 0);
            Grid.SetColumn(this.searchBox, 1);
            Grid.SetColumn(this.countText, 2);
            left.Children.Add(searchGlyph);
            left.Children.Add(this.searchBox);
            left.Children.Add(this.countText);
            left.SizeChanged += delegate { this.UpdateCountVisibility(left.ActualWidth); };

            StackPanel buttons = new StackPanel();
            buttons.Orientation = Orientation.Horizontal;
            buttons.VerticalAlignment = VerticalAlignment.Center;
            buttons.Children.Add(previous);
            buttons.Children.Add(next);
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
            barStrip.Padding = new Thickness(8d, 4d, 8d, 4d);
            barStrip.Child = row;

            this.resultsPanel = new StackPanel();
            this.resultsScroll = new ScrollViewer();
            this.resultsScroll.Content = this.resultsPanel;
            this.resultsScroll.MaxHeight = resultsMaxHeight;
            this.resultsScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            this.resultsScroll.Visibility = Visibility.Collapsed;

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
            layout.PreviewKeyDown += this.OnBarKeyDown;

            System.Windows.Documents.TextElement.SetFontSize(
                    layout, (double)this.resourceSource.FindResource("Font.Size.Label"));
            System.Windows.Documents.TextElement.SetForeground(
                    layout, (Brush)this.resourceSource.FindResource("Brush.TextPrimary"));
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
            button.Margin = new Thickness(4d, 0d, 0d, 0d);
            button.VerticalAlignment = VerticalAlignment.Center;
            button.ToolTip = toolTip;
            return button;
        }

        // 포커스를 표 내용으로 돌린다 — 선택 행이 있으면 그 행, 없으면 그리드. 그리드 자체에 포커스가 가면
        // 기본 포커스 사각형이 표 둘레에 그려지므로(2026-10-23 사용자 보고) 행을 먼저 찾고, 그리드 쪽은 XAML 에서 FocusVisualStyle 을 뺐다.
        private void FocusGridContent()
        {
            object item = this.grid.CurrentItem ?? this.grid.SelectedItem;
            DataGridRow row = item == null ? null : this.grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;

            if (row != null && row.Focusable)
            {
                row.Focus();
                return;
            }

            this.grid.Focus();
        }

        // 좁은 그리드에서는 일치 수를 숨긴다 — 검색창이 120 아래로 줄거나 글자가 말줄임되는 것보다 낫다. 값은 검색창 툴팁에 남는다.
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

        // 찾기 바의 표준 단축키 — Preview 터널링이라 검색창보다 먼저 받는다.
        private void OnBarKeyDown(object sender, KeyEventArgs e)
        {
            bool shift = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
            bool control = (Keyboard.Modifiers & ModifierKeys.Control) != 0;

            if (e.Key == Key.Escape)
            {
                // 닫고 그리드에 포커스 복귀.
                this.Hide();
                this.FocusGridContent();
                e.Handled = true;
            }
            else if (e.Key == Key.F3)
            {
                if (shift)
                {
                    this.FindPrevious();
                }
                else
                {
                    this.FindNext();
                }

                e.Handled = true;
            }
            else if (e.Key == Key.Enter && shift)
            {
                // Shift+Enter = 이전 일치 (Enter = 다음은 검색창 EnterPressed가 처리).
                this.FindPrevious();
                e.Handled = true;
            }
            else if (e.Key == Key.F && control)
            {
                // 열린 상태의 Ctrl+F = 검색창 재포커스 (표준 재입력 동작).
                this.searchBox.FocusEditor();
                e.Handled = true;
            }
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
            this.UpdateCountText(null);
            this.RefreshHighlights();
        }

        // 건수 표기 — 이동 중이면 "i / N", 아니면 "N matches"(검색어 없으면 빈 값).
        private void UpdateCountText(int? position)
        {
            if (this.searchText.Length == 0)
            {
                this.countText.Text = string.Empty;
                return;
            }

            if (this.matches.Count == 0)
            {
                this.countText.Text = "No matches";
                return;
            }

            this.countText.Text = position.HasValue
                    ? string.Format("{0} / {1:N0}", position.Value + 1, this.matches.Count)
                    : string.Format("{0:N0} matches", this.matches.Count);
            this.searchBox.ToolTip = this.countText.Text;
        }

        // ===== Find All 결과 목록 =====

        private void FindAll()
        {
            this.EnsureMatches();
            this.ClearResultsPanel();
            this.UpdateCountText(null);

            if (this.matches.Count == 0)
            {
                this.resultsScroll.Visibility = Visibility.Collapsed;
                return;
            }

            this.allButton.Kind = ButtonKind.Primary;
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

        // 결과 한 줄: "Row 3 · Lot ID · IT10003ED" — 클릭하면 그 셀로 이동.
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
            item.MouseLeftButtonDown += delegate
            {
                this.currentIndex = this.matches.IndexOf(match);
                this.NavigateTo(match);
                this.UpdateCountText(this.currentIndex >= 0 ? this.currentIndex : (int?)null);
            };

            return item;
        }

        private void ClearResultsPanel()
        {
            if (this.resultsPanel != null)
            {
                this.resultsPanel.Children.Clear();
                this.resultsScroll.Visibility = Visibility.Collapsed;
                this.allButton.Kind = ButtonKind.Secondary;
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
                        column, memberPath, format, caption.Replace("\n", " ")));
            }

            this.columnsValid = true;
        }

        // 현재 뷰(정렬·컬럼 필터 반영)의 행 순서 × 컬럼 순서로 일치를 모은다.
        private void EnsureMatches()
        {
            if (this.matchesValid)
            {
                return;
            }

            this.EnsureSearchColumns();
            this.matches.Clear();

            if (this.searchText.Length > 0)
            {
                for (int rowIndex = 0; rowIndex < this.grid.Items.Count; rowIndex++)
                {
                    object item = this.grid.Items[rowIndex];

                    if (item == CollectionView.NewItemPlaceholder)
                    {
                        continue;
                    }

                    foreach (SearchColumn column in this.searchColumns)
                    {
                        string cellText = GridAutoFitMeasurer.FormatCellText(
                                MemberPathReader.Read(item, column.MemberPath), column.Format);

                        if (cellText.IndexOf(this.searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            this.matches.Add(new FindMatch(item, rowIndex, column, cellText));
                        }
                    }
                }
            }

            this.matchesValid = true;
        }

        // ===== 이동 =====

        // 일치 셀로 이동 — 행 선택 + 스크롤 + 현재 셀 지정. 포커스는 검색창에
        // 남겨 Enter 연타로 계속 이동할 수 있다 (엑셀 동작).
        private void NavigateTo(FindMatch match)
        {
            this.grid.SelectedItem = match.Item;
            this.grid.ScrollIntoView(match.Item, match.Column.Column);

            try
            {
                this.grid.CurrentCell = new DataGridCellInfo(match.Item, match.Column.Column);
            }
            catch (Exception)
            {
                // 이동 사이에 소스가 바뀌어 항목이 사라진 경우 — 이동만 포기한다.
            }
        }

        // ===== 일치 텍스트 강조 =====

        // 실체화된 모든 행의 강조를 현재 검색어로 다시 건다 (없으면 걷는다).
        private void RefreshHighlights()
        {
            for (int index = 0; index < this.grid.Items.Count; index++)
            {
                DataGridRow row = this.grid.ItemContainerGenerator.ContainerFromIndex(index) as DataGridRow;

                if (row != null)
                {
                    this.ApplyHighlightToRow(row);
                }
            }
        }

        // 한 행의 검색 대상 셀에서 TextBlock을 찾아 일치 구간을 강조한다.
        private void ApplyHighlightToRow(DataGridRow row)
        {
            this.EnsureSearchColumns();

            foreach (SearchColumn column in this.searchColumns)
            {
                FrameworkElement content = column.Column.GetCellContent(row);

                if (content != null)
                {
                    this.HighlightTextBlocks(content);
                }
            }
        }

        // 셀 내용 아래의 TextBlock을 전부 찾아 강조를 적용/해제한다
        // (Text 셀은 내용 자체, Link/Badge 셀은 템플릿 안에 있다).
        private void HighlightTextBlocks(DependencyObject root)
        {
            TextBlock textBlock = root as TextBlock;

            if (textBlock != null)
            {
                this.ApplyInlineHighlight(textBlock);
                return;
            }

            int count = VisualTreeHelper.GetChildrenCount(root);

            for (int index = 0; index < count; index++)
            {
                this.HighlightTextBlocks(VisualTreeHelper.GetChild(root, index));
            }
        }

        // TextBlock의 Inlines를 [일반][강조][일반]… Run으로 재구성한다.
        // Text 속성(바인딩)은 건드리지 않는다 — 해제는 바인딩 UpdateTarget이
        // 평문을 다시 밀어 넣는 것으로 한다.
        private void ApplyInlineHighlight(TextBlock textBlock)
        {
            string keyword = this.searchText;
            string text = textBlock.Text;
            bool hasMatch = keyword.Length > 0
                    && text.Length > 0
                    && text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;

            if (!hasMatch)
            {
                if ((bool)textBlock.GetValue(IsHighlightedProperty))
                {
                    ClearInlineHighlight(textBlock, text);
                }

                return;
            }

            Brush highlight = (Brush)this.resourceSource.FindResource("Brush.WarningBorder");

            textBlock.SetValue(IsHighlightedProperty, true);
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
                    Background = highlight
                });
                position = matchIndex + keyword.Length;
            }
        }

        // 강조 해제 — 바인딩이 있으면 원본 값을 다시 밀어 넣고(서식 포함),
        // 없으면 합쳐진 텍스트를 단일 Run으로 되돌린다.
        private static void ClearInlineHighlight(TextBlock textBlock, string combinedText)
        {
            textBlock.SetValue(IsHighlightedProperty, false);

            BindingExpression expression = textBlock.GetBindingExpression(TextBlock.TextProperty);

            if (expression != null)
            {
                expression.UpdateTarget();
            }
            else
            {
                textBlock.Text = combinedText;
            }
        }
    }
}
