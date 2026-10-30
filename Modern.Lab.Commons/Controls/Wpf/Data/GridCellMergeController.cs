using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Modern.Lab.Controls.Wpf.Common;

namespace Modern.Lab.Controls.Wpf.Data
{
    /// <summary>
    /// 같은 값 세로 셀 병합(MergeCells) 담당 — 현재 뷰 순서에서 연속으로 같은
    /// 값인 구간을 **실제 스팬 셀**(불투명 표면 + 구간 세로 중앙 값)로 그리드
    /// 위 오버레이 캔버스에 그린다. WPF DataGrid는 셀 스팬을 지원하지 않으므로
    /// 셀을 조작하는 대신 위에 덮는다 — FarPoint/엑셀 병합과 같은 시각이 된다.
    ///
    /// 좌표는 "행 높이 × 인덱스 − 세로 스크롤 오프셋"의 순수 계산이다(행 높이
    /// 균일 + 픽셀 단위 스크롤 전제) — 행 실체화(가상화)와 무관하게 항상 옳고,
    /// 구간이 화면보다 길어도 값은 구간 전체의 세로 중앙에 위치한다(엑셀과
    /// 동일하게 스크롤을 따라간다). 컬럼 폭/가로 스크롤/고정(Frozen) 구간
    /// 동기화는 그룹 헤더·집계 푸터 스트립과 같은 문법을 쓴다.
    ///
    /// 표시 전용 원칙: 클릭/선택/복사/엑셀 내보내기는 행 단위 그대로다
    /// (오버레이는 IsHitTestVisible=false). 선택된 행과 마우스가 올라간 행은 스팬 셀 안에서도
    /// 그 행 높이의 띠로 보여 행 하이라이트가 병합 칸을 가로질러 이어지고(글자는 구간 가운데 그대로),
    /// 병합 구간의 모든 행이 선택되면 스팬 셀 전체를 선택색으로 칠한다(2026-10-30 — 전에는 일부
    /// 선택과 hover 가 스팬에 가려 행 하이라이트가 끊겨 보였다).
    ///
    /// 스팬 글자는 그 아래 셀과 같은 글자(Format · 셀 문화권)이고, 찾기 바가 열려 있으면 셀과 같은 찾기
    /// 강조(일치 노랑 · 지금 일치 주황)를 그린다 — 스팬이 셀을 덮어 셀의 강조가 보이지 않기 때문이다.
    /// </summary>
    internal sealed class GridCellMergeController
    {
        private readonly DataGrid grid;
        private readonly Canvas canvas;
        private readonly FrameworkElement resourceSource;
        private readonly Func<double> widthRatioGetter;
        private readonly Func<IList<ModernDataGridColumn>> definitionsProvider;

        // 병합이 켜진 컬럼 인덱스 목록 — 컬럼 재구성 때마다 갱신된다.
        private readonly List<int> mergeColumnIndexes = new List<int>();

        // 컬럼 폭 감시 대상 — 재구성 시 걷고 다시 건다.
        private readonly List<DataGridColumn> watchedColumns = new List<DataGridColumn>();

        private ScrollViewer scrollViewer;
        private bool updateQueued;
        private bool detached;

        // 편집 중인 병합 컬럼 인덱스 (-1 = 없음) — 편집기가 오버레이에 가리지
        // 않도록 그 컬럼의 스팬 셀을 잠시 걷는다.
        private int editingColumnIndex = -1;

        // 찾기 강조 — 찾기 바의 검색어(닫혀 있으면 빈 값)와 지금 일치(행 항목 + 컬럼).
        private string findKeyword = string.Empty;
        private object findCurrentItem;
        private DataGridColumn findCurrentColumn;

        // 마우스가 올라간 행 항목(없으면 null) — 그 행의 띠를 스팬 셀에 그린다.
        private object hoveredItem;

        internal GridCellMergeController(
                DataGrid grid,
                Canvas canvas,
                FrameworkElement resourceSource,
                Func<double> widthRatioGetter,
                Func<IList<ModernDataGridColumn>> definitionsProvider)
        {
            this.grid = grid;
            this.canvas = canvas;
            this.resourceSource = resourceSource;
            this.widthRatioGetter = widthRatioGetter;
            this.definitionsProvider = definitionsProvider;

            this.canvas.SizeChanged += delegate { this.QueueRefresh(); };
        }

        /// <summary>병합 컬럼이 하나라도 있는지 — 없으면 모든 경로가 즉시 반환한다.</summary>
        internal bool HasMergeColumns
        {
            get { return this.mergeColumnIndexes.Count > 0; }
        }

        /// <summary>ApplyColumns 후 호출 — 병합 컬럼 인덱스를 다시 수집한다.</summary>
        internal void OnColumnsRebuilt()
        {
            this.DetachColumnWatchers();
            this.mergeColumnIndexes.Clear();
            this.canvas.Children.Clear();

            IList<ModernDataGridColumn> definitions = this.definitionsProvider();

            if (definitions != null)
            {
                for (int index = 0; index < definitions.Count; index++)
                {
                    if (definitions[index].MergeCells)
                    {
                        this.mergeColumnIndexes.Add(index);
                    }
                }
            }

            if (!this.HasMergeColumns)
            {
                return;
            }

            this.AttachColumnWatchers();
            this.EnsureScrollHook();
            this.QueueRefresh();
        }

        /// <summary>
        /// 병합 컬럼 셀의 편집 시작/종료 — 편집 중에는 그 컬럼의 스팬 셀을
        /// 걷어 편집기(TextBox)가 보이게 한다. columnIndex = -1이면 종료.
        /// </summary>
        internal void OnEditingColumnChanged(int columnIndex)
        {
            if (this.editingColumnIndex != columnIndex)
            {
                this.editingColumnIndex = columnIndex;
                this.QueueRefresh();
            }
        }

        /// <summary>
        /// 찾기 강조가 바뀌었다 — 검색어가 든 스팬 글자를 강조하고, 지금 일치가 그 구간이면 주황으로 그린다.
        /// </summary>
        internal void SetFindHighlight(string keyword, object currentItem, DataGridColumn currentColumn)
        {
            string normalized = keyword ?? string.Empty;

            if (normalized == this.findKeyword
                    && object.ReferenceEquals(currentItem, this.findCurrentItem)
                    && currentColumn == this.findCurrentColumn)
            {
                return;
            }

            this.findKeyword = normalized;
            this.findCurrentItem = currentItem;
            this.findCurrentColumn = currentColumn;
            this.QueueRefresh();
        }

        /// <summary>행의 hover 진입 · 이탈 — 마우스가 올라간 행이 바뀌면 스팬 셀의 띠를 다시 그린다.</summary>
        internal void OnRowHoverChanged(DataGridRow row)
        {
            object hovered = row.IsMouseOver
                    ? row.Item
                    : (object.ReferenceEquals(this.hoveredItem, row.Item) ? null : this.hoveredItem);

            if (!object.ReferenceEquals(hovered, this.hoveredItem))
            {
                this.hoveredItem = hovered;
                this.QueueRefresh();
            }
        }

        /// <summary>
        /// 스팬 셀 전체를 다시 그린다 — 데이터/정렬/필터/선택/스크롤/폭 변화 뒤에
        /// 호출한다. 연속 호출은 렌더 프레임당 1회로 합쳐진다.
        /// </summary>
        internal void QueueRefresh()
        {
            if (this.detached || !this.HasMergeColumns || this.updateQueued)
            {
                return;
            }

            this.updateQueued = true;
            this.canvas.Dispatcher.BeginInvoke(
                new Action(delegate
                {
                    this.updateQueued = false;
                    if (!this.detached)
                    {
                        this.Rebuild();
                    }
                }),
                System.Windows.Threading.DispatcherPriority.Render);
        }

        // ===== 스팬 셀 배치 =====

        private void Rebuild()
        {
            this.canvas.Children.Clear();

            if (!this.HasMergeColumns || this.grid.Items.Count == 0)
            {
                return;
            }

            IList<ModernDataGridColumn> definitions = this.definitionsProvider();

            if (definitions == null || this.scrollViewer == null)
            {
                this.EnsureScrollHook();

                if (this.scrollViewer == null)
                {
                    return;
                }
            }

            double rowHeight = this.grid.RowHeight;

            if (double.IsNaN(rowHeight) || rowHeight <= 0d)
            {
                return;
            }

            double headerHeight = double.IsNaN(this.grid.ColumnHeaderHeight)
                    ? 0d
                    : this.grid.ColumnHeaderHeight;
            double verticalOffset = this.scrollViewer.VerticalOffset;
            double viewportHeight = this.scrollViewer.ViewportHeight;
            double viewportWidth = this.scrollViewer.ViewportWidth;

            // 오버레이가 헤더/스크롤바 위를 덮지 않게 데이터 뷰포트로 클리핑한다.
            RectangleGeometry clip = new RectangleGeometry(
                    new Rect(0d, headerHeight, Math.Max(0d, viewportWidth), Math.Max(0d, viewportHeight)));
            clip.Freeze();
            this.canvas.Clip = clip;

            int itemCount = this.grid.Items.Count;
            int columnCount = this.grid.Columns.Count;
            int frozen = this.grid.FrozenColumnCount;

            // 현재 셀 위치 — 병합 컬럼 안에 있으면 그 구간의 스팬 셀에 현재 셀
            // 언더라인을 단다 (엑셀/DevExpress 관례: 병합 셀 전체가 현재 셀).
            int currentColumnIndex = -1;
            int currentItemIndex = -1;
            DataGridCellInfo currentCell = this.grid.CurrentCell;

            if (currentCell.IsValid && currentCell.Column != null && currentCell.Item != null)
            {
                currentColumnIndex = this.grid.Columns.IndexOf(currentCell.Column);
                currentItemIndex = this.grid.Items.IndexOf(currentCell.Item);
            }

            // 찾기의 지금 일치 위치 — 그 구간의 스팬 글자를 주황으로 강조한다.
            int findColumnIndex = -1;
            int findItemIndex = -1;

            if (this.findKeyword.Length > 0 && this.findCurrentItem != null && this.findCurrentColumn != null)
            {
                findColumnIndex = this.grid.Columns.IndexOf(this.findCurrentColumn);
                findItemIndex = this.grid.Items.IndexOf(this.findCurrentItem);
            }

            // 마우스가 올라간 행 — 그 행의 띠를 스팬 셀에 그린다.
            int hoveredIndex = this.hoveredItem == null ? -1 : this.grid.Items.IndexOf(this.hoveredItem);

            CultureInfo culture = GridFindController.CellCultureOf(this.grid);

            // 컬럼 왼쪽 경계 누적 폭.
            double[] lefts = new double[columnCount + 1];

            for (int index = 0; index < columnCount; index++)
            {
                lefts[index + 1] = lefts[index] + this.grid.Columns[index].ActualWidth;
            }

            double frozenWidth = lefts[Math.Min(frozen, columnCount)];

            // 화면에 걸치는 행 범위 — 이 범위와 겹치는 병합 구간만 그린다.
            int firstVisible = Math.Max(0, (int)Math.Floor(verticalOffset / rowHeight));
            int lastVisible = Math.Min(
                    itemCount - 1,
                    (int)Math.Floor((verticalOffset + viewportHeight) / rowHeight));

            foreach (int columnIndex in this.mergeColumnIndexes)
            {
                if (columnIndex >= columnCount || columnIndex >= definitions.Count
                        || columnIndex == this.editingColumnIndex)
                {
                    continue;
                }

                ModernDataGridColumn definition = definitions[columnIndex];
                string path = definition.DataPropertyName;

                double columnLeft = lefts[columnIndex];
                double columnWidth = lefts[columnIndex + 1] - columnLeft;

                if (columnIndex >= frozen)
                {
                    columnLeft = columnLeft - this.scrollViewer.HorizontalOffset;
                }

                if (columnLeft + columnWidth <= (columnIndex >= frozen ? frozenWidth : 0d)
                        || columnLeft >= viewportWidth)
                {
                    continue;
                }

                int index = firstVisible;

                while (index <= lastVisible)
                {
                    object value = MemberPathReader.Read(this.grid.Items[index], path);

                    // 구간 경계 탐색 — 위쪽은 화면 밖까지 거슬러 올라가야 구간
                    // 전체 높이(값의 세로 중앙 위치)가 맞는다.
                    int runStart = index;

                    while (runStart > 0 && AreMergeValuesEqual(
                            MemberPathReader.Read(this.grid.Items[runStart - 1], path), value))
                    {
                        runStart--;
                    }

                    int runEnd = index;

                    while (runEnd < itemCount - 1 && AreMergeValuesEqual(
                            MemberPathReader.Read(this.grid.Items[runEnd + 1], path), value))
                    {
                        runEnd++;
                    }

                    int runLength = runEnd - runStart + 1;

                    if (runLength > 1)
                    {
                        double top = headerHeight + runStart * rowHeight - verticalOffset;
                        bool isCurrent = columnIndex == currentColumnIndex
                                && currentItemIndex >= runStart
                                && currentItemIndex <= runEnd;
                        bool isFindCurrent = columnIndex == findColumnIndex
                                && findItemIndex >= runStart
                                && findItemIndex <= runEnd;

                        // 높이 -1: 구간 아래 경계의 행 구분선은 남긴다 (구간 위
                        // 경계 선은 이전 행이 그린 것이 그대로 보인다).
                        this.canvas.Children.Add(this.BuildSpanCell(
                                definition, value, columnLeft, top,
                                columnWidth, runLength * rowHeight - 1d,
                                columnIndex >= frozen ? frozenWidth : 0d,
                                runStart, runEnd, isCurrent, isFindCurrent, culture,
                                rowHeight, Math.Max(runStart, firstVisible), Math.Min(runEnd, lastVisible), hoveredIndex));
                    }

                    index = runEnd + 1;
                }
            }
        }

        // 스팬 셀 하나 — 불투명 표면(아래 행들의 괘선을 덮는다) + 구간 세로 중앙 값.
        // 구간의 모든 행이 선택돼 있으면 선택색으로 칠하고, 아니면 선택된 행과 마우스가 올라간 행만
        // 그 행 높이의 띠(선택색 · hover 색)로 글자 아래에 그려 행 하이라이트가 병합 칸을 가로질러
        // 이어진다. 현재 셀이 구간 안에 있으면 셀 공통 문법(AccentSoft 2px 하단 언더라인)을 스팬 셀
        // 전체 폭에 단다. 찾기 검색어가 든 글자는 셀과 같은 찾기 강조를 건다.
        private UIElement BuildSpanCell(
                ModernDataGridColumn definition,
                object value,
                double left,
                double top,
                double width,
                double height,
                double frozenClipLeft,
                int runStart,
                int runEnd,
                bool isCurrent,
                bool isFindCurrent,
                CultureInfo culture,
                double rowHeight,
                int firstStripeRow,
                int lastStripeRow,
                int hoveredIndex)
        {
            bool fullySelected = this.IsRunFullySelected(runStart, runEnd);

            Border cell = new Border();
            cell.Width = Math.Max(0d, width);
            cell.Height = Math.Max(0d, height);
            cell.Background = (Brush)this.resourceSource.FindResource(
                    fullySelected ? "Brush.SelectedBackground" : "Brush.Surface");
            cell.ClipToBounds = true;
            cell.IsHitTestVisible = false;

            // 스크롤 구간 셀이 고정 구간 밑으로 들어가면 실제 셀처럼 왼쪽을
            // 잘라 낸다 (텍스트 위치는 유지 — 고정 경계에서 실제 컬럼과 동일).
            if (frozenClipLeft > 0d && left < frozenClipLeft)
            {
                RectangleGeometry cellClip = new RectangleGeometry(new Rect(
                        frozenClipLeft - left, 0d,
                        Math.Max(0d, width - (frozenClipLeft - left)), Math.Max(0d, height)));
                cellClip.Freeze();
                cell.Clip = cellClip;
            }

            Grid layout = new Grid();

            if (!fullySelected)
            {
                // 화면에 걸친 행만 본다 — 구간이 길어도 보이는 행의 띠만 필요하다.
                for (int rowIndex = firstStripeRow; rowIndex <= lastStripeRow; rowIndex++)
                {
                    bool selected = this.grid.SelectedItems.Count > 0
                            && this.grid.SelectedItems.Contains(this.grid.Items[rowIndex]);

                    if (!selected && rowIndex != hoveredIndex)
                    {
                        continue;
                    }

                    System.Windows.Shapes.Rectangle stripe = new System.Windows.Shapes.Rectangle();
                    stripe.Height = rowHeight;
                    stripe.VerticalAlignment = VerticalAlignment.Top;
                    stripe.Margin = new Thickness(0d, (rowIndex - runStart) * rowHeight, 0d, 0d);
                    stripe.Fill = (Brush)this.resourceSource.FindResource(
                            selected ? "Brush.SelectedBackground" : "Brush.HoverBackground");
                    layout.Children.Add(stripe);
                }
            }

            TextBlock text = new TextBlock();
            text.Text = GridFindController.CellDisplayText(value, definition.Format, culture);
            text.FontSize = (double)this.resourceSource.FindResource("Font.Size.Body");
            text.Foreground = (Brush)this.resourceSource.FindResource("Brush.TextPrimary");
            text.TextTrimming = TextTrimming.CharacterEllipsis;
            text.VerticalAlignment = VerticalAlignment.Center;
            text.Margin = (Thickness)this.resourceSource.FindResource("Pad.Field");

            if (this.findKeyword.Length > 0
                    && text.Text.IndexOf(this.findKeyword, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                GridFindController.FillHighlightRuns(text, text.Text, this.findKeyword, isFindCurrent, this.resourceSource);
            }

            if (definition.TextAlignment == GridTextAlignment.Center)
            {
                text.TextAlignment = TextAlignment.Center;
            }
            else if (definition.TextAlignment == GridTextAlignment.Right)
            {
                text.TextAlignment = TextAlignment.Right;
            }

            double widthRatio = this.widthRatioGetter();

            if (Math.Abs(widthRatio - 1d) >= 0.001)
            {
                text.LayoutTransform = GridColumnFactory.CreateWidthTransform(widthRatio);
            }

            layout.Children.Add(text);

            if (isCurrent)
            {
                // 현재 셀 언더라인 — 그리드 셀의 현재 셀 표식과 같은 시각 언어.
                System.Windows.Shapes.Rectangle underline = new System.Windows.Shapes.Rectangle();
                underline.Height = 2d;
                underline.VerticalAlignment = VerticalAlignment.Bottom;
                underline.Fill = this.CurrentCellUnderlineBrush();
                layout.Children.Add(underline);
            }

            cell.Child = layout;
            Canvas.SetLeft(cell, left);
            Canvas.SetTop(cell, top);
            return cell;
        }

        // 현재 셀 표식 브러시 (AccentSoft) — 처음 필요할 때 토큰에서 만든다.
        private Brush currentUnderlineBrush;

        private Brush CurrentCellUnderlineBrush()
        {
            if (this.currentUnderlineBrush == null)
            {
                Color accentSoft = (Color)this.resourceSource.FindResource("Color.AccentSoft");
                SolidColorBrush brush = new SolidColorBrush(accentSoft);
                brush.Freeze();
                this.currentUnderlineBrush = brush;
            }

            return this.currentUnderlineBrush;
        }

        // 구간의 모든 행이 선택돼 있는지 — 스팬 셀의 선택색 되비침용.
        private bool IsRunFullySelected(int runStart, int runEnd)
        {
            if (this.grid.SelectedItems.Count == 0
                    || this.grid.SelectedItems.Count < runEnd - runStart + 1)
            {
                return false;
            }

            for (int index = runStart; index <= runEnd; index++)
            {
                if (!this.grid.SelectedItems.Contains(this.grid.Items[index]))
                {
                    return false;
                }
            }

            return true;
        }

        // 병합 판정용 값 비교 — DataTable의 빈 값(DBNull)과 null을 같게 본다.
        private static bool AreMergeValuesEqual(object left, object right)
        {
            object normalizedLeft = left == DBNull.Value ? null : left;
            object normalizedRight = right == DBNull.Value ? null : right;

            return object.Equals(normalizedLeft, normalizedRight);
        }

        // ===== 동기화 훅 (그룹 헤더/집계 푸터 스트립과 같은 문법) =====

        private void AttachColumnWatchers()
        {
            foreach (DataGridColumn column in this.grid.Columns)
            {
                DependencyPropertyDescriptor
                        .FromProperty(DataGridColumn.ActualWidthProperty, typeof(DataGridColumn))
                        .AddValueChanged(column, this.OnColumnWidthChanged);
                this.watchedColumns.Add(column);
            }
        }

        internal void Detach()
        {
            this.detached = true;
            this.DetachColumnWatchers();
            this.grid.Loaded -= this.OnGridLoaded;
            if (this.scrollViewer != null)
            {
                this.scrollViewer.ScrollChanged -= this.OnScrollChanged;
                this.scrollViewer = null;
            }
        }
        private void DetachColumnWatchers()
        {
            foreach (DataGridColumn column in this.watchedColumns)
            {
                DependencyPropertyDescriptor
                        .FromProperty(DataGridColumn.ActualWidthProperty, typeof(DataGridColumn))
                        .RemoveValueChanged(column, this.OnColumnWidthChanged);
            }

            this.watchedColumns.Clear();
        }

        private void OnColumnWidthChanged(object sender, EventArgs e)
        {
            this.QueueRefresh();
        }

        private void EnsureScrollHook()
        {
            if (this.detached || this.scrollViewer != null)
            {
                return;
            }

            this.scrollViewer = this.grid.Template == null
                    ? null
                    : this.grid.Template.FindName("DG_ScrollViewer", this.grid) as ScrollViewer;

            if (this.scrollViewer == null)
            {
                this.grid.Loaded += this.OnGridLoaded;
                return;
            }

            this.scrollViewer.ScrollChanged += this.OnScrollChanged;
        }

        private void OnGridLoaded(object sender, RoutedEventArgs e)
        {
            this.grid.Loaded -= this.OnGridLoaded;
            this.EnsureScrollHook();
            this.QueueRefresh();
        }

        // 스팬 셀은 세로/가로 스크롤 모두 따라가야 한다 (푸터는 가로만 봤다).
        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (Math.Abs(e.VerticalChange) > 0.001
                    || Math.Abs(e.HorizontalChange) > 0.001
                    || Math.Abs(e.ViewportHeightChange) > 0.001
                    || Math.Abs(e.ViewportWidthChange) > 0.001)
            {
                this.QueueRefresh();
            }
        }
    }
}
