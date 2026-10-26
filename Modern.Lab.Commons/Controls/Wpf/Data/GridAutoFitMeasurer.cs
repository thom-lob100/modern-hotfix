using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Modern.Lab.Controls.Wpf.Common;

namespace Modern.Lab.Controls.Wpf.Data
{
    /// <summary>
    /// ModernDataGridControl의 AutoFitColumns 측정 엔진.
    /// 헤더 캡션과 데이터 내용의 실제 픽셀 폭을 FormattedText로 측정해
    /// 각 컬럼의 고정 픽셀 너비를 계산하고, 다중 줄 헤더("\n")에 맞춘
    /// 헤더 높이 계산도 담당한다. 폰트/DPI/토큰 리소스는 소유 컨트롤
    /// (owner)에서 읽는다.
    /// </summary>
    internal static class GridAutoFitMeasurer
    {
        // UI 스레드 진단에서 전후 차이로 읽는 누적 계수. 시간 임계값 없이 중복 작업을 검증한다.
        internal static long TextMeasurementCount { get; private set; }
        internal static long AutoFitPassCount { get; private set; }
        internal static long ValueScanCount { get; private set; }

        // ===== AutoFitColumns 측정 상수 =====
        // 컬럼당 실제 픽셀 측정할 후보 문자열 수 — 글자 수 상위 후보만 측정해
        // 큰 데이터에서도 비용을 일정하게 유지한다.
        private const int autoFitCandidateCount = 8;
        private const int maxMeasuredValues = 4096;
        // 자동 맞춤 너비의 하한/상한 (지나치게 좁은 컬럼과 폭주 컬럼 방지).
        private const double autoFitMinWidth = 48d;
        private const double autoFitMaxWidth = 600d;
        // 셀 좌우 패딩(Pad.Field 12,0) + 우측 구분선/렌더링 오차 여유.
        private const double autoFitCellPadding = 28d;
        // ===== 헤더 크롬 예약 (캡션 오른쪽에 붙는 것들) =====
        // 헤더 셀 좌우 패딩(Pad.Field 12,0) — 데이터 셀과 달리 여유를 섞지 않고
        // 실제 값만 쓴다. 헤더 예약은 아래 항목들을 하나씩 더해 만든다.
        private const double autoFitHeaderPadding = 24d;
        // 컬럼 구분 테두리(헤더 템플릿 BorderThickness 0,0,1,0).
        private const double autoFitHeaderBorder = 1d;
        // 글리프 앞 여백 — 헤더 템플릿의 SortGlyph/FilterButton Margin과 같은 값.
        private const double autoFitHeaderGlyphGap = 4d;
        // 소수점 폭 반올림 여유 (컬럼 폭은 Math.Ceiling으로 올림된다).
        private const double autoFitHeaderSafety = 2d;
        // ===== 트리 컬럼(ModernTreeListControl 첫 칸) 셀 템플릿 치수 =====
        // 펼침 화살표 버튼 폭(TreeCellTemplate 의 ExpanderButton Width)과 글자 앞 여백(TextBlock Margin 4,0,0,0).
        private const double treeExpanderWidth = 16d;
        private const double treeTextGap = 4d;
        // 정렬/필터 글리프 문자 — 실제 폭을 이 글리프로 측정한다. 상수로 폭을
        // 박아 두면 아이콘 크기 토큰(Font.Size.IconSm)이 바뀔 때 어긋난다.
        private const string autoFitSortGlyphSample = "";
        private const string autoFitFilterGlyphSample = "";
        // 배지 셀 추가 여유: 알약 좌우 패딩(10+10) + 곡률 여유.
        private const double autoFitBadgeReserve = 22d;
        // 버튼 셀 추가 여유: 버튼 좌우 패딩(14+14) + 테두리(1+1).
        private const double autoFitButtonChromeReserve = 30d;
        // 콤보 셀 추가 여유: 좌 패딩(10) + 셰브런 예약(26) + 테두리/여백.
        private const double autoFitComboChromeReserve = 44d;

        /// <summary>
        /// 각 컬럼 너비를 max(헤더 캡션 폭 + 글리프 여유, 데이터 최대 폭)으로
        /// 재계산해 고정 픽셀 너비로 넣는다. WPF DataGridLength.Auto는 가상화로
        /// 실체화된 행만 보고 계산해 스크롤 중 너비가 계속 변하므로 쓰지 않는다.
        /// </summary>
        /// <param name="grid">너비를 적용할 내부 DataGrid.</param>
        /// <param name="definitions">ApplyColumns로 받은 컬럼 정의 목록.</param>
        /// <param name="itemsSource">측정 대상 데이터 (null 허용).</param>
        /// <param name="owner">폰트/DPI/토큰 리소스를 읽을 소유 컨트롤.</param>
        /// <param name="widthRatio">유효 장평 — 측정 폭에 곱한다.</param>
        internal static void ApplyAutoFitWidths(
            DataGrid grid,
            IList<ModernDataGridColumn> definitions,
            IEnumerable itemsSource,
            Control owner,
            double widthRatio)
        {
            ApplyAutoFitWidths(grid, definitions, itemsSource, owner, widthRatio, 0);
        }

        /// <summary>
        /// 정의 목록이 그리드의 <paramref name="columnOffset"/>번째 컬럼부터 순서대로 대응할 때의 자동 폭 — 트리 리스트처럼
        /// 0번에 정의 밖의 컬럼(트리 컬럼)이 앞서는 표가 쓴다(2026-10-26). 측정은 정의의 원본 컬럼 이름으로 <paramref name="itemsSource"/>를
        /// 읽으므로, 셀 바인딩 경로를 바꾼 사본이 아니라 원본 정의를 넘긴다.
        /// </summary>
        internal static void ApplyAutoFitWidths(
            DataGrid grid,
            IList<ModernDataGridColumn> definitions,
            IEnumerable itemsSource,
            Control owner,
            double widthRatio,
            int columnOffset)
        {
            AutoFitPassCount++;
            double pixelsPerDip = VisualTreeHelper.GetDpi(owner).PixelsPerDip;
            Typeface headerTypeface = new Typeface(owner.FontFamily, FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal);
            Typeface bodyTypeface = new Typeface(owner.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            double headerFontSize = (double)owner.FindResource("Font.Size.Label");
            double headerTopFontSize = (double)owner.FindResource("Font.Size.SubLabel");
            double bodyFontSize = (double)owner.FindResource("Font.Size.Body");

            // 헤더 크롬 예약을 글리프 실측으로 만든다 — 아이콘 폰트/크기 토큰이
            // 바뀌어도 따라오고, 상수로 넉넉히 잡아 컬럼이 넓어지는 일도 없다.
            Typeface iconTypeface = new Typeface(
                (FontFamily)owner.FindResource("Font.Family.Icon"),
                FontStyles.Normal,
                FontWeights.Normal,
                FontStretches.Normal);
            double iconFontSize = (double)owner.FindResource("Font.Size.IconSm");

            // 정렬 글리프 폭 — 평상시 글리프는 빈 문자열이라 폭이 0이고 여백만
            // 소비하므로, 이 폭은 **정렬이 걸린 컬럼에만** 더한다 (컬럼을 항상
            // 넓게 잡지 않기 위한 선택 — 정렬을 켜고 끌 때 그 컬럼 폭이 이만큼
            // 움직인다). 여백은 상태와 무관하게 항상 소비된다.
            double sortGlyphWidth =
                MeasureText(autoFitSortGlyphSample, iconTypeface, iconFontSize, pixelsPerDip);
            // 깔때기(값 필터 버튼): Filterable 컬럼에만 붙는다.
            double filterGlyphReserve = autoFitHeaderGlyphGap
                + MeasureText(autoFitFilterGlyphSample, iconTypeface, iconFontSize, pixelsPerDip);

            int count = Math.Min(definitions.Count, grid.Columns.Count - columnOffset);

            for (int definitionIndex = 0; definitionIndex < count; definitionIndex++)
            {
                ModernDataGridColumn definition = definitions[definitionIndex];
                int index = definitionIndex + columnOffset;

                // 체크박스 컬럼은 내용 측정 대상이 아니다 — 정의 폭(없으면 최소 폭)을 유지한다.
                if (definition.Kind == GridColumnKind.CheckBox)
                {
                    grid.Columns[index].Width =
                        new DataGridLength(definition.Width > 0d ? definition.Width : autoFitMinWidth);
                    continue;
                }

                // 헤더 예약 = 패딩 + 구분 테두리 + 반올림 여유 + 정렬 글리프 여백
                // (+ 정렬이 걸린 컬럼은 글리프 폭, + 값 필터가 켜진 컬럼은 깔때기).
                // Filterable은 GridFilterController가 AttachHeader에서 설정하며,
                // 헤더 템플릿의 버튼 표시 트리거도 같은 값을 읽는다.
                double headerReserve = autoFitHeaderPadding + autoFitHeaderBorder
                    + autoFitHeaderSafety + autoFitHeaderGlyphGap;

                if (grid.Columns[index].SortDirection.HasValue)
                {
                    headerReserve += sortGlyphWidth;
                }

                if (GridFilterAttach.GetFilterable(grid.Columns[index]))
                {
                    headerReserve += filterGlyphReserve;
                }

                // 가운데 정렬 헤더는 왼쪽에 오른쪽 글리프 묶음과 같은 폭의 빈 칸이 생기므로 같은 폭을 더 잡는다.
                if (NeedsGlyphReserve(definition))
                {
                    headerReserve += autoFitHeaderGlyphGap
                        + (grid.Columns[index].SortDirection.HasValue ? sortGlyphWidth : 0d)
                        + (GridFilterAttach.GetFilterable(grid.Columns[index]) ? filterGlyphReserve : 0d);
                }

                double width = (MeasureHeaderText(definition, headerTypeface, headerFontSize, headerTopFontSize, pixelsPerDip) * widthRatio)
                    + headerReserve;

                if (definition.Kind == GridColumnKind.Combo)
                {
                    // 콤보 컬럼은 정의 폭 우선 — 없으면 선택지 중 최장 텍스트 폭 +
                    // 콤보 크롬(패딩/셰브런) 기준으로 잰다. 데이터 값은 선택지의
                    // 부분집합이라 따로 측정하지 않는다.
                    if (definition.Width > 0d)
                    {
                        grid.Columns[index].Width = new DataGridLength(definition.Width);
                        continue;
                    }

                    if (definition.ComboItems != null)
                    {
                        foreach (string item in definition.ComboItems)
                        {
                            double itemWidth = (MeasureText(item, bodyTypeface, bodyFontSize, pixelsPerDip) * widthRatio)
                                + autoFitCellPadding + autoFitComboChromeReserve;

                            if (itemWidth > width)
                            {
                                width = itemWidth;
                            }
                        }
                    }
                }
                else if (definition.Kind == GridColumnKind.Button)
                {
                    // 버튼 컬럼은 캡션 폭 기준 — 데이터 내용은 측정하지 않는다.
                    // 캡션이 SemiBold로 그려지므로 측정도 SemiBold 폭으로 한다.
                    // 행마다 캡션이 다르면(ButtonTextMember) 그 컬럼의 값들을 재서
                    // 가장 긴 캡션에 맞춘다 — 고정 캡션 하나로 재면 잘린다.
                    double captionText = string.IsNullOrEmpty(definition.ButtonTextMember)
                        ? MeasureText(definition.ButtonText, headerTypeface, bodyFontSize, pixelsPerDip)
                        : MeasureLongestValue(
                            itemsSource, definition.ButtonTextMember, string.Empty,
                            headerTypeface, bodyFontSize, pixelsPerDip);
                    double captionWidth = (captionText * widthRatio)
                        + autoFitCellPadding + autoFitButtonChromeReserve;

                    if (captionWidth > width)
                    {
                        width = captionWidth;
                    }
                }
                else
                {
                    // 배지 셀은 알약 좌우 패딩만큼 여유를 더한다.
                    double reserve = definition.Kind == GridColumnKind.Badge ? autoFitBadgeReserve : 0d;

                    // SemiBold 강조 컬럼은 실제 굵기 기준으로 잰다 (Regular보다 약간 넓음).
                    Typeface cellTypeface = definition.TextSemiBold ? headerTypeface : bodyTypeface;

                    foreach (string candidate in CollectLongestCellTexts(itemsSource, definition))
                    {
                        double cellWidth = (MeasureText(candidate, cellTypeface, bodyFontSize, pixelsPerDip) * widthRatio)
                            + autoFitCellPadding + reserve;

                        if (cellWidth > width)
                        {
                            width = cellWidth;
                        }
                    }
                }

                if (width < autoFitMinWidth)
                {
                    width = autoFitMinWidth;
                }

                if (width > autoFitMaxWidth)
                {
                    width = autoFitMaxWidth;
                }

                grid.Columns[index].Width = new DataGridLength(Math.Ceiling(width));
            }
        }

        /// <summary>
        /// 헤더 캡션의 최대 줄 수("\n" 기준)에 맞는 헤더 높이를 계산한다.
        /// HeaderText에 "Event\nTime"처럼 명시적 줄바꿈을 넣으면 2줄 이상 헤더가
        /// 되고, 줄바꿈이 없으면 토큰 기본 높이(Size.GridHeaderHeight) 그대로다.
        /// (헤더 문자열의 줄바꿈은 WPF TextBlock이 그대로 줄로 렌더링하므로
        /// 높이만 확보하면 된다. AutoFit 폭 측정도 FormattedText가 최장 줄 폭을
        /// 돌려주므로 별도 처리가 필요 없다.)
        /// </summary>
        /// <param name="owner">폰트/DPI/토큰 리소스를 읽을 소유 컨트롤.</param>
        /// <param name="definitions">컬럼 정의 목록 (null이면 기본 높이).</param>
        // 헤더 폭 계산에 글리프 묶음 폭을 한 번 더 잡아야 하는 정렬인지 — 본 캡션 Center 뿐이다. 작은 줄은 본 캡션 글자 폭 안에서 정렬되므로
        // (SubLabelPanel) 여백을 따로 잡지 않는다.
        private static bool NeedsGlyphReserve(ModernDataGridColumn definition)
        {
            return definition.HeaderAlignment == GridTextAlignment.Center;
        }

        internal static double ComputeHeaderHeight(Control owner, IList<ModernDataGridColumn> definitions)
        {
            int maxLines = 1;
            bool twoSmallLines = false;

            if (definitions != null)
            {
                foreach (ModernDataGridColumn definition in definitions)
                {
                    int lines = CountHeaderLines(definition.HeaderText);

                    if (lines > maxLines)
                    {
                        maxLines = lines;
                    }

                    if (GridHeaderCaption.Trimmed(definition.TopLabel).Length > 0
                        && GridHeaderCaption.Trimmed(definition.BottomLabel).Length > 0)
                    {
                        twoSmallLines = true;
                    }
                }
            }

            double baseHeight = (double)owner.FindResource("Size.GridHeaderHeight");

            if (maxLines == 1 && !twoSmallLines)
            {
                return baseHeight;
            }

            // 추가 줄마다 헤더 캡션 한 줄 높이(Label 크기 실측)만큼 늘린다 — 폰트 토큰이 바뀌어도 하드코딩 없이 따라온다.
            // 위·아래 작은 글씨가 모두 있는 컬럼이 하나라도 있으면 작은 글씨 한 줄 높이(GridMarker 크기 실측)를 더한다.
            double pixelsPerDip = VisualTreeHelper.GetDpi(owner).PixelsPerDip;
            Typeface headerTypeface = new Typeface(
                owner.FontFamily, FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal);
            double headerFontSize = (double)owner.FindResource("Font.Size.Label");
            double height = baseHeight;

            if (maxLines > 1)
            {
                height += (maxLines - 1) * LineHeight(headerTypeface, headerFontSize, pixelsPerDip);
            }

            if (twoSmallLines)
            {
                height += LineHeight(headerTypeface, (double)owner.FindResource("Font.Size.SubLabel"), pixelsPerDip);
            }

            return height;
        }

        private static double LineHeight(Typeface typeface, double fontSize, double pixelsPerDip)
        {
            FormattedText lineProbe = new FormattedText(
                "Ag",
                CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                typeface,
                fontSize,
                Brushes.Black,
                pixelsPerDip);

            return lineProbe.Height;
        }

        // 헤더 캡션 폭 — 위·아래 작은 글씨가 있으면 작은 글씨들과 본 캡션 중 가장 넓은 쪽.
        private static double MeasureHeaderText(
            ModernDataGridColumn definition, Typeface typeface, double fontSize, double smallFontSize, double pixelsPerDip)
        {
            double widest = MeasureText(definition.HeaderText, typeface, fontSize, pixelsPerDip);
            string top = GridHeaderCaption.Trimmed(definition.TopLabel);
            string bottom = GridHeaderCaption.Trimmed(definition.BottomLabel);

            if (top.Length > 0)
            {
                widest = Math.Max(widest, MeasureText(top, typeface, smallFontSize, pixelsPerDip));
            }

            if (bottom.Length > 0)
            {
                widest = Math.Max(widest, MeasureText(bottom, typeface, smallFontSize, pixelsPerDip));
            }

            return widest;
        }

        // 본 캡션의 줄 수 ("\n" 개수 + 1; 빈 캡션은 1줄). 위·아래 작은 글씨는 여기서 세지 않는다 — 작은 글씨 한 줄과
        // 12 크기 캡션 한 줄은 기본 헤더 높이(36, 위아래 여백 0) 안에 들어가고, 위·아래가 모두 있을 때만 ComputeHeaderHeight가 한 줄을 더한다.
        private static int CountHeaderLines(string headerText)
        {
            if (string.IsNullOrEmpty(headerText))
            {
                return 1;
            }

            int lines = 1;

            for (int index = 0; index < headerText.Length; index++)
            {
                if (headerText[index] == '\n')
                {
                    lines = lines + 1;
                }
            }

            return lines;
        }

        // 배지·행별 버튼의 정확한 최대 표시폭. 동일 표시 문자열만 실측을 재사용한다.
        // 선언된 값 목록을 그대로 잰다 — 행에서 컬럼을 읽는 경로와 달리 값 자체가 표시 문자열이다.
        internal static double MeasureLongestText(
            IEnumerable<string> texts, Typeface typeface, double fontSize, double pixelsPerDip)
        {
            double widest = 0d;

            if (texts == null)
            {
                return widest;
            }

            foreach (string text in texts)
            {
                if (string.IsNullOrEmpty(text))
                {
                    continue;
                }

                double width = MeasureText(text, typeface, fontSize, pixelsPerDip);

                if (width > widest)
                {
                    widest = width;
                }
            }

            return widest;
        }

        internal static double MeasureLongestValue(
            IEnumerable source, string member, string format, Typeface typeface, double fontSize, double pixelsPerDip)
        {
            double widest = 0d;

            if (source == null)
            {
                return widest;
            }

            ValueScanCount++;
            HashSet<string> measured = new HashSet<string>(StringComparer.Ordinal);
            foreach (object row in source)
            {
                string text = FormatCellText(MemberPathReader.Read(row, member), format);

                if (text.Length == 0 || measured.Contains(text))
                {
                    continue;
                }

                // 호출 안에서만 재사용한다. 상한 이후의 새 값도 실측하므로 최대폭은 정확하다.
                if (measured.Count < maxMeasuredValues)
                {
                    measured.Add(text);
                }
                double width = MeasureText(text, typeface, fontSize, pixelsPerDip);

                if (width > widest)
                {
                    widest = width;
                }
            }

            return widest;
        }

        // 일반 텍스트 컬럼은 기존 글자 수 상위 후보 선별 규칙을 유지한다.
        private static List<string> CollectLongestCellTexts(IEnumerable source, ModernDataGridColumn definition)
        {
            List<string> candidates = new List<string>();

            if (source == null)
            {
                return candidates;
            }

            ValueScanCount++;
            foreach (object row in source)
            {
                string text = FormatCellText(MemberPathReader.Read(row, definition.DataPropertyName), definition.Format);

                if (text.Length == 0 || candidates.Contains(text))
                {
                    continue;
                }

                if (candidates.Count < autoFitCandidateCount)
                {
                    candidates.Add(text);
                    continue;
                }

                // 가장 짧은 후보를 찾아 더 긴 텍스트로 교체한다.
                int shortestIndex = 0;

                for (int index = 1; index < candidates.Count; index++)
                {
                    if (candidates[index].Length < candidates[shortestIndex].Length)
                    {
                        shortestIndex = index;
                    }
                }

                if (text.Length > candidates[shortestIndex].Length)
                {
                    candidates[shortestIndex] = text;
                }
            }

            return candidates;
        }

        // 셀에 표시될 문자열을 만든다 — 컬럼 Format이 있으면 바인딩 StringFormat과
        // 같은 규칙("{0:형식}")으로 적용하고, 실패하면 기본 문자열 표현을 쓴다.
        internal static string FormatCellText(object value, string format)
        {
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            value = CoerceForFormat(value, format);

            if (!string.IsNullOrEmpty(format))
            {
                try
                {
                    return string.Format(CultureInfo.CurrentCulture, "{0:" + format + "}", value);
                }
                catch (FormatException)
                {
                    // 형식 오류는 기본 표현으로 폴백한다 (바인딩 표시와 동일한 완화).
                }
            }

            return value.ToString();
        }

        /// <summary>
        /// Format이 지정된 컬럼의 **문자열 값**을 형식 종류에 맞는 타입으로
        /// 해석한다 — 서버가 날짜/숫자를 문자열로 내려보내는 경우(예: java
        /// Timestamp 문자열 "2026-07-29 12:00:00.0")에도 컬럼 Format이
        /// 적용되게 하기 위해서다. 해석에 실패하면 원본을 그대로 돌려주므로
        /// 일반 문자열 컬럼에는 영향이 없다. Format이 비었으면 아무것도 하지
        /// 않는다 (기존 동작 유지).
        /// </summary>
        internal static object CoerceForFormat(object value, string format)
        {
            string text = value as string;

            if (string.IsNullOrEmpty(format) || string.IsNullOrEmpty(text))
            {
                return value;
            }

            if (IsDateLikeFormat(format))
            {
                DateTime parsed;

                if (DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsed))
                {
                    return parsed;
                }
            }
            else
            {
                decimal number;

                if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out number))
                {
                    return number;
                }
            }

            return value;
        }

        // Format이 날짜/시각 계열인지 — 날짜 토큰(y/M/d/H/h/s)이 있으면 날짜로
        // 본다. 숫자 형식("N0", "F2", "#,##0.00" 등)에는 이 문자들이 쓰이지 않는다.
        private static bool IsDateLikeFormat(string format)
        {
            return format.IndexOfAny(new char[] { 'y', 'M', 'd', 'H', 'h', 's' }) >= 0;
        }

        // FormattedText로 문자열의 실제 렌더링 폭을 잰다 (빈 문자열은 0).
        /// <summary>
        /// 트리 리스트의 트리 컬럼(첫 칸) 자동 폭(2026-10-26) — 헤더 캡션 폭과, 노드마다 들여쓰기 + 펼침 화살표 + 글자 여백 + 글자 폭에
        /// 셀 패딩을 더한 것 중 가장 넓은 값. 하한은 호출자가 넘긴 최소 폭, 상한은 값 컬럼과 같다. 노드가 많아도 비용이 일정하도록
        /// 들여쓰기와 글자 수로 고른 상위 후보만 실측한다. 트리 컬럼을 star 로 남기면 WPF DataGrid 가 공간이 모자랄 때
        /// 고정 폭 값 컬럼에서 MinWidth 까지 폭을 빼앗아 자동 폭이 무너지므로, AutoFit 이 켜진 트리는 첫 칸도 이 폭으로 고정한다.
        /// </summary>
        internal static double MeasureTreeColumnWidth(
            Control owner, string headerText, IEnumerable<TreeListNode> nodes, double minWidth)
        {
            double pixelsPerDip = VisualTreeHelper.GetDpi(owner).PixelsPerDip;
            Typeface headerTypeface = new Typeface(owner.FontFamily, FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal);
            Typeface bodyTypeface = new Typeface(owner.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            double headerFontSize = (double)owner.FindResource("Font.Size.Label");
            double bodyFontSize = (double)owner.FindResource("Font.Size.Body");

            double width = MeasureText(headerText, headerTypeface, headerFontSize, pixelsPerDip)
                + autoFitHeaderPadding + autoFitHeaderBorder + autoFitHeaderSafety;

            foreach (TreeListNode node in LongestTreeNodes(nodes, bodyFontSize))
            {
                double cellWidth = node.Indent + treeExpanderWidth + treeTextGap
                    + MeasureText(node.Text, bodyTypeface, bodyFontSize, pixelsPerDip) + autoFitCellPadding;

                if (cellWidth > width)
                {
                    width = cellWidth;
                }
            }

            if (width < minWidth)
            {
                width = minWidth;
            }

            if (width > autoFitMaxWidth)
            {
                width = autoFitMaxWidth;
            }

            return Math.Ceiling(width);
        }

        // 트리 노드 중 폭이 넓을 후보 — 들여쓰기 + 글자 수 × 글자 크기의 어림값이 큰 순으로 후보 수만큼. 실측은 호출자가 한다.
        private static List<TreeListNode> LongestTreeNodes(IEnumerable<TreeListNode> nodes, double fontSize)
        {
            List<TreeListNode> candidates = new List<TreeListNode>();
            List<double> scores = new List<double>();

            if (nodes == null)
            {
                return candidates;
            }

            foreach (TreeListNode node in nodes)
            {
                if (node == null || string.IsNullOrEmpty(node.Text))
                {
                    continue;
                }

                double score = node.Indent + (node.Text.Length * fontSize);

                if (candidates.Count < autoFitCandidateCount)
                {
                    candidates.Add(node);
                    scores.Add(score);
                    continue;
                }

                int lowestIndex = 0;

                for (int index = 1; index < scores.Count; index++)
                {
                    if (scores[index] < scores[lowestIndex])
                    {
                        lowestIndex = index;
                    }
                }

                if (score > scores[lowestIndex])
                {
                    candidates[lowestIndex] = node;
                    scores[lowestIndex] = score;
                }
            }

            return candidates;
        }

        internal static double MeasureText(string text, Typeface typeface, double fontSize, double pixelsPerDip)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0d;
            }

            TextMeasurementCount++;
            FormattedText formatted = new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                typeface,
                fontSize,
                Brushes.Black,
                pixelsPerDip);

            return formatted.WidthIncludingTrailingWhitespace;
        }
    }
}
