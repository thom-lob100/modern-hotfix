using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using Modern.Lab.Controls.Wpf.Common;

namespace Modern.Lab.Controls.Wpf.Data
{
    /// <summary>
    /// ModernDataGridControl의 컬럼 정의(ModernDataGridColumn)를 실제
    /// DataGridColumn으로 만드는 팩토리. 컨트롤 인스턴스 상태(리소스 조회,
    /// 이벤트 핸들러, 헤더 체크박스 등록)는 전부 파라미터/콜백으로 받아
    /// 순수 생성만 담당한다.
    /// </summary>
    internal static class GridColumnFactory
    {
        // ===== 배지/버튼 셀이 쓰는 값 변환기 (공유 인스턴스) =====

        private static readonly IValueConverter badgeBackgroundConverter = new BadgeBackgroundConverter();
        private static readonly IValueConverter badgeForegroundConverter = new BadgeForegroundConverter();
        private static readonly IValueConverter truthyConverter = new TruthyToBooleanConverter();
        private static readonly IValueConverter spinnerVisibilityConverter = new TruthyToVisibilityConverter();
        private static readonly IValueConverter selectedCheckVisibilityConverter = new SelectedToCheckVisibilityConverter();

        /// <summary>
        /// 컬럼 정의의 Kind에 따라 텍스트/체크박스/배지/버튼 컬럼을 만든다.
        /// </summary>
        /// <param name="definition">컬럼 정의.</param>
        /// <param name="widthRatio">장평(글자 가로 비율) — 1이 아니면 셀 텍스트에 가로 스케일 적용.</param>
        /// <param name="resourceSource">스타일/토큰 리소스를 조회할 기준 요소 (그리드 컨트롤).</param>
        /// <param name="headerCheckBoxClick">헤더 체크박스(전체 선택/해제) 클릭 콜백.</param>
        /// <param name="cellCheckBoxClick">셀 체크박스 클릭 콜백 (헤더 상태 재계산용).</param>
        /// <param name="cellButtonClick">버튼 셀 클릭 콜백 (CellButtonClick 이벤트 중계).</param>
        /// <param name="registerHeaderCheckBox">생성된 헤더 체크박스를 컨트롤 목록에 등록하는 콜백.</param>
        /// <param name="cellMinWidth">배지/버튼 셀의 통일 최소 폭 — 가장 긴 표시값 기준으로 컨트롤이 계산해 넘긴다 (0 = 미적용).</param>
        /// <param name="cellLinkClick">링크 셀 클릭 콜백 (CellLinkClick 이벤트 중계).</param>
        internal static DataGridColumn CreateColumn(
            ModernDataGridColumn definition,
            double widthRatio,
            FrameworkElement resourceSource,
            RoutedEventHandler headerCheckBoxClick,
            RoutedEventHandler cellCheckBoxClick,
            RoutedEventHandler cellButtonClick,
            Action<CheckBox> registerHeaderCheckBox,
            double cellMinWidth,
            RoutedEventHandler cellLinkClick)
        {
            switch (definition.Kind)
            {
                case GridColumnKind.CheckBox:
                    return MarkNonEditable(CreateCheckBoxColumn(definition, resourceSource, headerCheckBoxClick, cellCheckBoxClick, registerHeaderCheckBox));
                case GridColumnKind.Badge:
                    return MarkNonEditable(CreateBadgeColumn(definition, widthRatio, resourceSource, cellMinWidth));
                case GridColumnKind.Button:
                    return MarkNonEditable(CreateButtonColumn(definition, widthRatio, resourceSource, cellButtonClick, cellMinWidth));
                case GridColumnKind.Combo:
                    return MarkNonEditable(CreateComboColumn(definition, resourceSource));
                case GridColumnKind.Spinner:
                    return MarkNonEditable(CreateSpinnerColumn(definition));
                case GridColumnKind.Link:
                    return MarkNonEditable(CreateLinkColumn(definition, widthRatio, resourceSource, cellLinkClick));
                default:
                    return CreateTextColumn(definition, widthRatio, resourceSource);
            }
        }

        // 템플릿 컬럼(체크박스/배지/버튼/콤보/스피너/링크)은 그리드가 편집
        // 모드여도 셀 편집 상태에 들어가지 않게 컬럼 단위로 잠근다 — 편집
        // 템플릿이 없어 들어가 봐야 빈 상태가 될 뿐이고, 셀 안 컨트롤
        // (체크박스·콤보 등)은 컬럼 잠금과 무관하게 동작한다.
        private static DataGridColumn MarkNonEditable(DataGridColumn column)
        {
            column.IsReadOnly = true;
            return column;
        }

        /// <summary>장평 적용용 가로 ScaleTransform (Freeze해 컬럼/행 간 공유 가능).</summary>
        internal static ScaleTransform CreateWidthTransform(double widthRatio)
        {
            ScaleTransform transform = new ScaleTransform(widthRatio, 1d);
            transform.Freeze();
            return transform;
        }

        private static DataGridTextColumn CreateTextColumn(
            ModernDataGridColumn definition, double widthRatio, FrameworkElement resourceSource)
        {
            DataGridTextColumn column = new DataGridTextColumn();
            column.Header = GridHeaderCaption.CreateHeader(definition);

            // 컬럼 잠금 — 그리드가 편집 모드(ReadOnly=false)일 때 키 컬럼처럼
            // 값이 바뀌면 안 되는 컬럼만 잠근다.
            column.IsReadOnly = definition.ReadOnly;

            Binding binding = new Binding(definition.DataPropertyName);

            // 표시 형식: "N0" 같은 단순 형식은 "{0:N0}"으로 해석된다(WPF 규칙).
            // 원본이 타입 컬럼(int/decimal/DateTime)일 때만 효과가 있다.
            if (!string.IsNullOrEmpty(definition.Format))
            {
                binding.StringFormat = definition.Format;
            }

            // 편집 커밋: 표시 문자열("1,234")을 원본 값으로 되돌리고, 변환/타입
            // 오류는 검증 오류(빨간 테두리 + 커밋 거부)로 만든다. 읽기 전용
            // 그리드에서는 편집 자체가 시작되지 않으므로 표시 동작에 영향이 없다.
            binding.Converter = new GridEditValueConverter(definition.Format);
            binding.ValidatesOnExceptions = true;

            column.Binding = binding;
            column.EditingElementStyle = BuildCellEditorStyle(definition, widthRatio, resourceSource);
            ApplyColumnWidth(column, definition);

            Style elementStyle = new Style(typeof(TextBlock));

            if (definition.TextAlignment != GridTextAlignment.Left)
            {
                TextAlignment alignment = definition.TextAlignment == GridTextAlignment.Right
                    ? TextAlignment.Right
                    : TextAlignment.Center;

                elementStyle.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, alignment));
            }

            // 장평: 셀 TextBlock에 가로 LayoutTransform을 걸어 측정·배치까지
            // 줄어든/늘어난 폭 기준으로 동작하게 한다.
            if (Math.Abs(widthRatio - 1d) >= 0.001)
            {
                elementStyle.Setters.Add(
                    new Setter(FrameworkElement.LayoutTransformProperty, CreateWidthTransform(widthRatio)));
            }

            // 컬럼 강조색: 지정된 경우 컬럼 전체 텍스트에 적용한다 (해석 불가하면 기본색).
            if (!string.IsNullOrEmpty(definition.TextColor))
            {
                Brush textBrush = ChipColorHelper.TryCreateBrush(definition.TextColor);

                if (textBrush != null)
                {
                    elementStyle.Setters.Add(new Setter(TextBlock.ForegroundProperty, textBrush));
                }
            }

            // 컬럼 굵기 강조: 파생 지표를 색+굵기로 강조할 때 쓴다.
            if (definition.TextSemiBold)
            {
                elementStyle.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.SemiBold));
            }

            // 잘린 셀에만 전체 텍스트 툴팁 — 컬럼 폭이 좁아 "…"로 잘리면 호버 시
            // 전체 값을 보여 준다. ToolTip은 셀 자신의 Text에 바인딩하고, 실제로
            // 잘리지 않았으면 ToolTipOpening에서 취소해 불필요한 툴팁을 막는다.
            elementStyle.Setters.Add(new Setter(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis));
            elementStyle.Setters.Add(new Setter(
                TextBlock.ToolTipProperty,
                new Binding("Text") { RelativeSource = new RelativeSource(RelativeSourceMode.Self) }));
            elementStyle.Setters.Add(new EventSetter(
                ToolTipService.ToolTipOpeningEvent, new ToolTipEventHandler(OnCellToolTipOpening)));

            if (elementStyle.Setters.Count > 0)
            {
                column.ElementStyle = elementStyle;
            }

            return column;
        }

        // 셀 편집기(TextBox) 스타일 — 셀 안에 앉는 무테 편집기. 셀 템플릿의
        // 패딩 안쪽을 Surface 배경으로 채워 "이 셀이 편집 중"이 드러나고,
        // 정렬/장평은 표시 텍스트와 같은 규칙을 따른다. 변환·타입 오류는
        // WPF 기본 검증 어도너(빨간 테두리)가 표시한다.
        private static Style BuildCellEditorStyle(
            ModernDataGridColumn definition, double widthRatio, FrameworkElement resourceSource)
        {
            Style style = new Style(typeof(TextBox));
            style.Setters.Add(new Setter(Control.BackgroundProperty, resourceSource.FindResource("Brush.Surface")));
            style.Setters.Add(new Setter(Control.ForegroundProperty, resourceSource.FindResource("Brush.TextPrimary")));
            style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0d)));
            style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(0d)));
            style.Setters.Add(new Setter(Control.FontSizeProperty, resourceSource.FindResource("Font.Size.Body")));
            style.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));

            if (definition.TextAlignment != GridTextAlignment.Left)
            {
                style.Setters.Add(new Setter(
                    TextBox.TextAlignmentProperty,
                    definition.TextAlignment == GridTextAlignment.Right
                        ? TextAlignment.Right
                        : TextAlignment.Center));
            }

            if (Math.Abs(widthRatio - 1d) >= 0.001)
            {
                style.Setters.Add(new Setter(
                    FrameworkElement.LayoutTransformProperty, CreateWidthTransform(widthRatio)));
            }

            return style;
        }

        // 셀 텍스트가 실제로 잘렸을 때만 툴팁을 연다 — 안 잘렸으면 취소.
        private static void OnCellToolTipOpening(object sender, ToolTipEventArgs e)
        {
            TextBlock text = sender as TextBlock;

            if (text == null || !IsTextTrimmed(text))
            {
                e.Handled = true;
            }
        }

        // TextBlock의 전체 텍스트 폭이 실제 표시 폭보다 크면(=잘림) true.
        private static bool IsTextTrimmed(TextBlock text)
        {
            if (string.IsNullOrEmpty(text.Text) || text.ActualWidth <= 0d)
            {
                return false;
            }

            Typeface typeface = new Typeface(
                text.FontFamily, text.FontStyle, text.FontWeight, text.FontStretch);

            FormattedText formatted = new FormattedText(
                text.Text, CultureInfo.CurrentCulture, text.FlowDirection, typeface,
                text.FontSize, Brushes.Black, VisualTreeHelper.GetDpi(text).PixelsPerDip);

            double available = text.ActualWidth - text.Padding.Left - text.Padding.Right;
            return formatted.WidthIncludingTrailingWhitespace > available + 0.5d;
        }

        /// <summary>
        /// 체크박스 값 어휘 모드인가 — 참/거짓 값이 <b>둘 다</b> 채워졌을 때만
        /// 켠다. 한쪽만 채우면 반대편에 무엇을 써야 할지 정해지지 않으므로
        /// bool 컬럼으로 취급한다(조용히 빈 문자열을 쓰지 않는다).
        /// </summary>
        internal static bool UsesCheckVocabulary(ModernDataGridColumn definition)
        {
            return !string.IsNullOrEmpty(definition.CheckTrueValue)
                    && !string.IsNullOrEmpty(definition.CheckFalseValue);
        }

        /// <summary>
        /// 행 값 → 체크 여부. 셀(바인딩 변환기)과 헤더(전체 선택·중간 상태)가
        /// 같은 답을 내도록 판정을 한 곳에 둔다. bool 모드는 예전 그대로
        /// <b>bool 값만</b> 참으로 본다 — 여기서 문자열까지 참으로 넓히면 셀은
        /// 비어 있는데 헤더만 체크되는 어긋남이 생긴다.
        /// </summary>
        internal static bool ReadCheck(ModernDataGridColumn definition, object value)
        {
            if (UsesCheckVocabulary(definition))
            {
                return GridCheckValueConverter.Read(
                        value, definition.CheckTrueValue, definition.CheckFalseValue);
            }

            return value is bool && (bool)value;
        }

        /// <summary>체크 여부 → 행에 쓸 값. 어휘 모드면 문자열, 아니면 bool.</summary>
        internal static object CheckWriteValue(ModernDataGridColumn definition, bool isChecked)
        {
            if (UsesCheckVocabulary(definition))
            {
                return isChecked ? definition.CheckTrueValue : definition.CheckFalseValue;
            }

            return isChecked;
        }

        // 체크박스 컬럼: 값 컬럼에 양방향 바인딩(bool 또는 Y/N 어휘). 그리드가 읽기 전용이어도
        // CellTemplate 안의 체크박스는 살아 있으므로 한 번의 클릭으로 토글되고,
        // UpdateSourceTrigger=PropertyChanged로 원본 행 값이 즉시 갱신된다.
        private static DataGridColumn CreateCheckBoxColumn(
            ModernDataGridColumn definition,
            FrameworkElement resourceSource,
            RoutedEventHandler headerCheckBoxClick,
            RoutedEventHandler cellCheckBoxClick,
            Action<CheckBox> registerHeaderCheckBox)
        {
            DataGridTemplateColumn column = new DataGridTemplateColumn();
            column.CanUserSort = false;

            // 헤더 체크박스(전체 선택/해제): HeaderCheckBox 옵션이 켜지면 헤더
            // 캡션 대신 체크박스를 올린다. 클릭 시 현재 표시 중인 모든 행의 값을
            // 일괄 설정하고, 행 값 상태(전체/일부/없음)를 중간 상태로 되비춘다.
            if (definition.HeaderCheckBox)
            {
                CheckBox headerCheck = new CheckBox();

                // 컬럼 이름이 아니라 정의를 통째로 들려 보낸다 — 전체 선택이
                // 값 어휘(Y/N)까지 알아야 행에 옳은 값을 쓸 수 있다.
                headerCheck.Tag = definition;
                headerCheck.Style = (Style)resourceSource.FindResource("Parts.GridCheckBoxStyle");
                headerCheck.HorizontalAlignment = HorizontalAlignment.Center;
                headerCheck.VerticalAlignment = VerticalAlignment.Center;

                // 왼쪽 여백을 따로 주지 않는다 — 가운데 정렬 헤더는 템플릿의 HeaderLeadSpacer 가 오른쪽 글리프 묶음과
                // 같은 폭을 왼쪽에 두어 헤더 전체 폭의 가운데(셀 가운데)에 놓는다(2026-09-29). 전에 두던 7px 보정은
                // 그 위에 더해져 헤더 체크박스가 셀보다 3px 오른쪽에 있었다(2026-10-10 현장 보고).
                headerCheck.Click += headerCheckBoxClick;
                column.Header = headerCheck;
                registerHeaderCheckBox(headerCheck);

                // 좁은 체크박스 컬럼에서 헤더 기본 좌우 패딩(Pad.Field)이 내용
                // 영역을 잠식해 체크박스 오른쪽이 잘린다 — 이 컬럼만 패딩을 없애
                // 체크박스가 온전히 가운데 오게 한다.
                Style headerStyle = new Style(typeof(DataGridColumnHeader));
                headerStyle.BasedOn = (Style)resourceSource.FindResource("ModernGridColumnHeaderStyle");
                headerStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(0d)));
                column.HeaderStyle = headerStyle;
            }
            else
            {
                column.Header = GridHeaderCaption.CreateHeader(definition);
            }

            Binding binding = new Binding(definition.DataPropertyName);
            binding.Mode = BindingMode.TwoWay;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            // 값 컬럼이 "Y"/"N" 같은 문자열 어휘면 변환기를 끼운다. 없으면 체크가
            // 표시되지 않고(문자열 → bool 변환 실패), 토글이 그 컬럼에 "True"를
            // 써서 조회가 준 어휘를 오염시킨다.
            if (UsesCheckVocabulary(definition))
            {
                binding.Converter = new GridCheckValueConverter(
                        definition.CheckTrueValue, definition.CheckFalseValue);
            }

            FrameworkElementFactory check = new FrameworkElementFactory(typeof(CheckBox));
            check.SetBinding(CheckBox.IsCheckedProperty, binding);
            check.SetValue(
                    FrameworkElement.StyleProperty,
                    resourceSource.FindResource(
                            definition.CheckStyle == GridCheckStyle.Switch
                                    ? "Parts.GridSwitchStyle"
                                    : "Parts.GridCheckBoxStyle"));
            check.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            check.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

            // 행별 토글 가능 여부 — 거짓인 행은 잠긴 모양이 되고 클릭해도
            // 값이 바뀌지 않는다(조건이 맞을 때만 바꿀 수 있는 설정용).
            if (!string.IsNullOrEmpty(definition.CheckEnabledMember))
            {
                Binding enabledBinding = new Binding(definition.CheckEnabledMember);
                enabledBinding.Converter = truthyConverter;
                check.SetBinding(UIElement.IsEnabledProperty, enabledBinding);
            }

            // 셀 토글이 헤더 체크박스 상태에 즉시 반영되도록 클릭을 구독한다.
            check.AddHandler(ButtonBase.ClickEvent, cellCheckBoxClick);

            DataTemplate template = new DataTemplate();
            template.VisualTree = check;
            column.CellTemplate = template;

            ApplyColumnWidth(column, definition);
            return column;
        }

        // 배지 컬럼: 값 텍스트를 BadgeColorMember 색의 레티클(둥근 사각,
        // Radius.Sm) 배지로 감싼다. 글자색은 배경색에서 자동 유도
        // (ChipColorHelper.DeriveForeground)한다.
        // 강조 값 판정은 **셀 값**을 보고, 평소 색은 **색 경로**를 본다 — 둘이
        // 다른 컬럼일 수 있으므로(BadgeColorMember 등) 두 값을 함께 받는
        // MultiBinding으로 묶는다. 색 경로가 없으면 셀 값만 두 번 넘긴다.
        private static MultiBinding CreateAccentBinding(
                ModernDataGridColumn definition,
                string colorPath,
                IValueConverter innerConverter,
                bool foreground)
        {
            MultiBinding binding = new MultiBinding();
            binding.Bindings.Add(new Binding(colorPath ?? definition.DataPropertyName));
            binding.Bindings.Add(new Binding(definition.DataPropertyName));
            binding.Converter = new BadgeAccentColorConverter(
                    definition.BadgeAccentValues, innerConverter, foreground);

            return binding;
        }

        private static DataGridColumn CreateBadgeColumn(
            ModernDataGridColumn definition,
            double widthRatio,
            FrameworkElement resourceSource,
            double badgeMinWidth)
        {
            DataGridTemplateColumn column = new DataGridTemplateColumn();
            column.Header = GridHeaderCaption.CreateHeader(definition);
            column.SortMemberPath = definition.DataPropertyName;

            // 여백 10,3 — 배지 높이 약 22px(글자 16 + 위아래 3)로 28px 행의 세로 중앙에 앉는다
            // (2026-10-03: 10,2 의 20px 는 글자가 배지 위아래에 붙어 보였다 — 행 높이는 그대로 두고 2px 를 더 썼다). 배지/버튼 컬럼이 나란히 있어도
            // 배지가 버튼을 넘지 않는다. 모양은 BadgeShape가 정한다 —
            // 기본 둥근 사각(Radius.Sm), Pill이면 알약.
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.CornerRadiusProperty, resourceSource.FindResource(
                    definition.BadgeShape == ChipShape.Pill ? "Radius.Pill" : "Radius.Sm"));
            border.SetValue(Border.PaddingProperty, new Thickness(10d, 3d, 10d, 3d));
            border.SetValue(FrameworkElement.HorizontalAlignmentProperty, ToHorizontalAlignment(definition.TextAlignment));
            border.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

            if (badgeMinWidth > 0d)
            {
                border.SetValue(FrameworkElement.MinWidthProperty, badgeMinWidth);
            }

            // 색 결정 경로 — 명시적일수록 우선한다 (예외 없이 하나만 적용):
            //   BadgeColorMember(직접 색) > BadgeSemanticMember(의미 어휘)
            //   > BadgeGroupMember(그룹 키) > BadgeAutoColor(값 자동).
            //   그 위에 BadgeAccentValues(강조 값)가 있다 — 아래 CreateAccentBinding 참고.
            string colorPath = null;
            IValueConverter backgroundConverter = null;
            IValueConverter foregroundConverter = null;

            if (!string.IsNullOrEmpty(definition.BadgeColorMember))
            {
                colorPath = definition.BadgeColorMember;
                backgroundConverter = badgeBackgroundConverter;
                foregroundConverter = badgeForegroundConverter;
            }
            else if (!string.IsNullOrEmpty(definition.BadgeSemanticMember))
            {
                colorPath = definition.BadgeSemanticMember;
                backgroundConverter = new BadgeDerivedColorConverter(BadgeColorSource.Semantic, false);
                foregroundConverter = new BadgeDerivedColorConverter(BadgeColorSource.Semantic, true);
            }
            else if (!string.IsNullOrEmpty(definition.BadgeGroupMember))
            {
                colorPath = definition.BadgeGroupMember;
                backgroundConverter = new BadgeDerivedColorConverter(BadgeColorSource.Group, false);
                foregroundConverter = new BadgeDerivedColorConverter(BadgeColorSource.Group, true);
            }
            else if (definition.BadgeAutoColor)
            {
                colorPath = definition.DataPropertyName;
                backgroundConverter = new BadgeDerivedColorConverter(BadgeColorSource.Auto, false);
                foregroundConverter = new BadgeDerivedColorConverter(BadgeColorSource.Auto, true);
            }

            // 강조 값(BadgeAccentValues)은 색 경로 넷보다 앞선다 — 배경과 글자를
            // 오류 색 짝으로 함께 바꾼다. 글자만 빨갛게 하면 값마다 다른 자동색
            // 배경 위에서 테마 대부분이 대비 4.5:1을 못 넘는다.
            if (!string.IsNullOrEmpty(definition.BadgeAccentValues))
            {
                border.SetBinding(
                        Border.BackgroundProperty,
                        CreateAccentBinding(
                                definition, colorPath, backgroundConverter, false));
            }
            else if (colorPath != null)
            {
                Binding backgroundBinding = new Binding(colorPath);
                backgroundBinding.Converter = backgroundConverter;
                border.SetBinding(Border.BackgroundProperty, backgroundBinding);
            }

            // 강조 배지에만 테두리를 두른다 — 색은 테마마다 일반 칩과 겹칠 수 있지만
            // (HighContrast·CrimsonGray는 채움색이 칩 밝기 범위 안에 든다) **없던
            // 테두리가 생기는 것**은 어느 테마에서도 종류가 다른 신호로 읽힌다.
            // 두께는 모든 행에 고정으로 주고 브러시만 바꾼다 — 강조 행에서만
            // 두꺼워지면 글자가 밀려 표가 흔들린다.
            if (!string.IsNullOrEmpty(definition.BadgeAccentValues))
            {
                border.SetValue(Border.BorderThicknessProperty, new Thickness(1.5d));

                Binding outlineBinding = new Binding(definition.DataPropertyName);
                outlineBinding.Converter =
                        new BadgeAccentOutlineConverter(definition.BadgeAccentValues);
                border.SetBinding(Border.BorderBrushProperty, outlineBinding);
            }

            Binding textBinding = new Binding(definition.DataPropertyName);

            if (!string.IsNullOrEmpty(definition.Format))
            {
                textBinding.StringFormat = definition.Format;

                // 문자열로 내려온 날짜/숫자에도 Format이 적용되게 해석한다
                // (텍스트 컬럼과 같은 규칙 — GridEditValueConverter.Convert).
                textBinding.Converter = new GridEditValueConverter(definition.Format);
            }

            FrameworkElementFactory text = new FrameworkElementFactory(typeof(TextBlock));
            text.SetBinding(TextBlock.TextProperty, textBinding);
            text.SetValue(TextBlock.FontSizeProperty, resourceSource.FindResource("Font.Size.Label"));
            text.SetValue(TextBlock.FontWeightProperty, FontWeights.Normal);
            // 배지 폭은 컬럼 공통(MinWidth = 가장 긴 표시값)으로 늘어나므로,
            // 배지 안 텍스트 정렬도 컬럼 TextAlignment를 그대로 따른다 —
            // 숫자 배지(경과일)는 Center로 중앙에, 단어 배지(상태)는 Left로
            // 왼쪽 기준선에 맞출 수 있다.
            text.SetValue(FrameworkElement.HorizontalAlignmentProperty,
                    ToHorizontalAlignment(definition.TextAlignment));
            text.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

            // 장평: 배지 텍스트에도 같은 가로 스케일을 적용한다 (알약은 텍스트 폭을 따라간다).
            if (Math.Abs(widthRatio - 1d) >= 0.001)
            {
                text.SetValue(FrameworkElement.LayoutTransformProperty, CreateWidthTransform(widthRatio));
            }

            if (!string.IsNullOrEmpty(definition.BadgeAccentValues))
            {
                text.SetBinding(
                        TextBlock.ForegroundProperty,
                        CreateAccentBinding(
                                definition, colorPath, foregroundConverter, true));
            }
            else if (colorPath != null)
            {
                Binding foregroundBinding = new Binding(colorPath);
                foregroundBinding.Converter = foregroundConverter;
                text.SetBinding(TextBlock.ForegroundProperty, foregroundBinding);
            }

            border.AppendChild(text);

            DataTemplate template = new DataTemplate();

            // 회전 하이라이트 판정 경로 — 명시적 판정 컬럼(BadgeSpinBorderMember)이
            // 있으면 그쪽, 없으면 값 선언(BadgeSpinValues — 배지 값 자체로 판정,
            // 더미 컬럼 불필요)을 쓴다.
            string spinPath = null;
            IValueConverter spinConverter = null;

            if (!string.IsNullOrEmpty(definition.BadgeSpinBorderMember))
            {
                spinPath = definition.BadgeSpinBorderMember;
                spinConverter = spinnerVisibilityConverter;
            }
            else if (!string.IsNullOrEmpty(definition.BadgeSpinValues))
            {
                spinPath = definition.DataPropertyName;
                spinConverter = new SpinValuesVisibilityConverter(definition.BadgeSpinValues);
            }

            // 테두리를 도는 하이라이트를 줄 컬럼이면 배지를 Grid로 감싼다. Grid는
            // 배지 크기(내용)에 맞춰 줄고(정렬만 배지 정렬을 따름), 오버레이는
            // Stretch로 그 Grid를 꽉 채워 배지와 같은 크기로 겹친다. 활성(작업중)
            // 행만 보이도록 Visibility를 판정 경로에 묶고, 오버레이는
            // 히트 테스트를 통과한다.
            if (spinPath != null)
            {
                HorizontalAlignment align = ToHorizontalAlignment(definition.TextAlignment);
                border.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);

                FrameworkElementFactory spin =
                        new FrameworkElementFactory(typeof(Modern.Lab.Controls.Wpf.Display.ModernSpinBorderControl));
                spin.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                spin.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Stretch);

                Binding spinVisible = new Binding(spinPath);
                spinVisible.Converter = spinConverter;
                spin.SetBinding(UIElement.VisibilityProperty, spinVisible);

                FrameworkElementFactory host = new FrameworkElementFactory(typeof(Grid));
                host.SetValue(FrameworkElement.HorizontalAlignmentProperty, align);
                host.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
                host.AppendChild(border);
                host.AppendChild(spin);
                template.VisualTree = host;
            }
            else
            {
                template.VisualTree = border;
            }

            column.CellTemplate = template;

            ApplyColumnWidth(column, definition);

            // 셀 기본 좌우 패딩까지 감안해, 정의 폭이 짧아도 가장 긴 배지가
            // 잘리지 않도록 컬럼의 최소 폭을 보장한다.
            if (badgeMinWidth > 0d)
            {
                Thickness cellPadding = (Thickness)resourceSource.FindResource("Pad.Field");
                double fitted = badgeMinWidth + cellPadding.Left + cellPadding.Right;
                column.MinWidth = fitted;

                // 폭을 적지 않은 배지 컬럼은 **가장 긴 값에 맞춘 고정 폭**이 기본이다 —
                // 별(*) 너비로 늘어나면 표마다 배지 컬럼 폭이 달라지고, 화면이 숫자를
                // 손으로 적어 맞춰야 했다. 적어 준 폭이 있으면 그 폭을 그대로 쓴다.
                if (definition.Width <= 0d)
                {
                    column.Width = new DataGridLength(fitted);
                }
            }

            return column;
        }

        // 같은 배지 컬럼은 가장 긴 표시 텍스트를 기준으로 같은 폭을 쓴다.
        // 행마다 배지가 들쭉날쭉하면 표가 산만해지므로, 텍스트 폭과 알약의
        // 좌우 패딩을 한 번 계산해 각 셀 Border.MinWidth에 공통 적용한다.
        // 배지 폭 기준으로 선언된 값 목록 — 세미콜론/쉼표로 나누고 빈 칸은 버린다.
        private static List<string> BadgeWidthValues(ModernDataGridColumn definition)
        {
            List<string> values = new List<string>();

            if (definition == null || string.IsNullOrEmpty(definition.BadgeWidthValues))
            {
                return values;
            }

            foreach (string piece in definition.BadgeWidthValues.Split(new char[] { ';', ',' }))
            {
                string text = piece.Trim();

                if (text.Length > 0)
                {
                    values.Add(text);
                }
            }

            return values;
        }

        internal static double CalculateBadgeMinWidth(
            ModernDataGridColumn definition,
            IEnumerable itemsSource,
            FrameworkElement resourceSource,
            double widthRatio)
        {
            if (itemsSource == null)
            {
                return 0d;
            }

            double pixelsPerDip = VisualTreeHelper.GetDpi(resourceSource).PixelsPerDip;
            FontFamily fontFamily = (FontFamily)resourceSource.FindResource("Font.Family");
            Typeface typeface = new Typeface(
                    fontFamily,
                    FontStyles.Normal,
                    FontWeights.Normal,
                    FontStretches.Normal);
            double fontSize = (double)resourceSource.FindResource("Font.Size.Label");
            // 폭 기준이 선언돼 있으면 데이터가 아니라 그 어휘로 잰다 — 조회마다 폭이
            // 흔들리지 않는다. 선언이 없으면 지금까지대로 들어온 데이터로 잰다.
            List<string> declared = BadgeWidthValues(definition);

            double widest = declared.Count > 0
                    ? GridAutoFitMeasurer.MeasureLongestText(declared, typeface, fontSize, pixelsPerDip) * widthRatio
                    : GridAutoFitMeasurer.MeasureLongestValue(itemsSource, definition.DataPropertyName,
                            definition.Format, typeface, fontSize, pixelsPerDip) * widthRatio;

            // 배지 좌우 패딩 10+10과 렌더링 오차 여유 2px을 더한다.
            return widest > 0d ? Math.Ceiling(widest + 22d) : 0d;
        }

        // 같은 버튼 컬럼은 가장 긴 캡션 기준으로 같은 폭을 쓴다 — 행마다 캡션이
        // 다르면(ButtonTextMember) 버튼이 들쭉날쭉해 표가 산만하고, 같은 컬럼의
        // 버튼은 같은 물건으로 읽혀야 한다 (배지 폭 통일과 같은 규칙).
        internal static double CalculateButtonMinWidth(
            ModernDataGridColumn definition,
            IEnumerable itemsSource,
            FrameworkElement resourceSource,
            double widthRatio)
        {
            if (itemsSource == null || string.IsNullOrEmpty(definition.ButtonTextMember))
            {
                return 0d;
            }

            double pixelsPerDip = VisualTreeHelper.GetDpi(resourceSource).PixelsPerDip;
            FontFamily fontFamily = (FontFamily)resourceSource.FindResource("Font.Family");
            Typeface typeface = new Typeface(
                    fontFamily,
                    FontStyles.Normal,
                    FontWeights.Normal,
                    FontStretches.Normal);
            double fontSize = (double)resourceSource.FindResource("Font.Size.Label");
            double widest = GridAutoFitMeasurer.MeasureLongestValue(itemsSource, definition.ButtonTextMember,
                    string.Empty, typeface, fontSize, pixelsPerDip) * widthRatio;

            // 버튼 좌우 패딩 10+10, 테두리 1+1, 렌더링 오차 여유 2px.
            return widest > 0d ? Math.Ceiling(widest + 24d) : 0d;
        }

        // 버튼 컬럼: 행 단위 액션 버튼. 클릭은 CellButtonClick 이벤트로 전달되고,
        // ButtonEnabledMember 컬럼 값(bool/Y/N)이 행별 활성화를 제어한다.
        private static DataGridColumn CreateButtonColumn(
            ModernDataGridColumn definition,
            double widthRatio,
            FrameworkElement resourceSource,
            RoutedEventHandler cellButtonClick,
            double buttonMinWidth)
        {
            DataGridTemplateColumn column = new DataGridTemplateColumn();
            column.Header = GridHeaderCaption.CreateHeader(definition);
            column.CanUserSort = false;

            FrameworkElementFactory button = new FrameworkElementFactory(typeof(Button));

            // 캡션은 TextBlock으로 감싸 장평(가로 스케일)을 적용한다 —
            // 버튼 테두리는 스케일하지 않고 글자만 조절된다.
            FrameworkElementFactory caption = new FrameworkElementFactory(typeof(TextBlock));

            if (string.IsNullOrEmpty(definition.ButtonTextMember))
            {
                caption.SetValue(TextBlock.TextProperty, definition.ButtonText);
            }
            else
            {
                caption.SetBinding(
                        TextBlock.TextProperty, new Binding(definition.ButtonTextMember));
            }

            if (Math.Abs(widthRatio - 1d) >= 0.001)
            {
                caption.SetValue(FrameworkElement.LayoutTransformProperty, CreateWidthTransform(widthRatio));
            }

            button.AppendChild(caption);
            button.SetValue(FrameworkElement.TagProperty, definition.DataPropertyName);
            button.SetValue(FrameworkElement.HorizontalAlignmentProperty, ToHorizontalAlignment(definition.TextAlignment));
            button.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            button.SetValue(FrameworkElement.StyleProperty,
                    definition.ButtonAutoColor
                            ? BuildAutoColorCellButtonStyle(resourceSource, definition)
                            : BuildCellButtonStyle(resourceSource));

            if (!string.IsNullOrEmpty(definition.ButtonEnabledMember))
            {
                Binding enabledBinding = new Binding(definition.ButtonEnabledMember);
                enabledBinding.Converter = truthyConverter;
                button.SetBinding(UIElement.IsEnabledProperty, enabledBinding);
            }

            // 행마다 캡션이 다르면 캡션이 빈 행에는 버튼 자체를 그리지 않는다.
            if (!string.IsNullOrEmpty(definition.ButtonTextMember))
            {
                Binding visibleBinding = new Binding(definition.ButtonTextMember);
                visibleBinding.Converter = new TextPresentVisibilityConverter();
                button.SetBinding(UIElement.VisibilityProperty, visibleBinding);
            }

            button.AddHandler(ButtonBase.ClickEvent, cellButtonClick);

            // 행별 캡션 컬럼은 가장 긴 캡션 기준으로 버튼 폭을 통일한다 —
            // 셀 기본 패딩까지 감안해 컬럼 최소 폭도 함께 보장한다 (배지와 동일).
            if (buttonMinWidth > 0d)
            {
                button.SetValue(FrameworkElement.MinWidthProperty, buttonMinWidth);

                Thickness cellPadding = (Thickness)resourceSource.FindResource("Pad.Field");
                column.MinWidth = buttonMinWidth + cellPadding.Left + cellPadding.Right;
            }

            DataTemplate template = new DataTemplate();
            template.VisualTree = button;
            column.CellTemplate = template;

            ApplyColumnWidth(column, definition);
            return column;
        }

        // 하이퍼링크 컬럼: 값 텍스트를 액센트색 링크로 그린다 (호버 시 밑줄 +
        // 한 단계 진한 액센트). 클릭은 CellLinkClick 이벤트로 전달된다 —
        // "셀 값 클릭 → 상세/이력 화면" 관례용. 정렬은 텍스트 셀과 동일하게
        // 원본 값 기준(SortMemberPath)으로 동작한다.
        private static DataGridColumn CreateLinkColumn(
            ModernDataGridColumn definition,
            double widthRatio,
            FrameworkElement resourceSource,
            RoutedEventHandler cellLinkClick)
        {
            DataGridTemplateColumn column = new DataGridTemplateColumn();
            column.Header = GridHeaderCaption.CreateHeader(definition);
            column.SortMemberPath = definition.DataPropertyName;

            Binding textBinding = new Binding(definition.DataPropertyName);

            if (!string.IsNullOrEmpty(definition.Format))
            {
                textBinding.StringFormat = definition.Format;

                // 문자열로 내려온 날짜/숫자에도 Format이 적용되게 해석한다
                // (텍스트 컬럼과 같은 규칙 — GridEditValueConverter.Convert).
                textBinding.Converter = new GridEditValueConverter(definition.Format);
            }

            FrameworkElementFactory text = new FrameworkElementFactory(typeof(TextBlock));
            text.SetBinding(TextBlock.TextProperty, textBinding);
            text.SetValue(FrameworkElement.StyleProperty, BuildLinkTextStyle(resourceSource));

            if (Math.Abs(widthRatio - 1d) >= 0.001)
            {
                text.SetValue(FrameworkElement.LayoutTransformProperty, CreateWidthTransform(widthRatio));
            }

            // 클릭 라우팅은 셀 버튼과 같은 Button 파이프라인을 쓴다 — 크롬 없는
            // 템플릿이라 화면에는 링크 텍스트만 보인다. 값이 빈 행은 내용이
            // 없어 크기가 0이 되므로 클릭 대상 자체가 생기지 않는다.
            FrameworkElementFactory button = new FrameworkElementFactory(typeof(Button));
            button.SetValue(FrameworkElement.TagProperty, definition.DataPropertyName);
            button.SetValue(FrameworkElement.CursorProperty, System.Windows.Input.Cursors.Hand);
            button.SetValue(UIElement.FocusableProperty, false);
            button.SetValue(FrameworkElement.HorizontalAlignmentProperty, ToHorizontalAlignment(definition.TextAlignment));
            button.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            button.SetValue(Control.TemplateProperty, BuildBareButtonTemplate());
            button.AppendChild(text);
            button.AddHandler(ButtonBase.ClickEvent, cellLinkClick);

            DataTemplate template = new DataTemplate();
            template.VisualTree = button;
            column.CellTemplate = template;

            ApplyColumnWidth(column, definition);
            return column;
        }

        // 링크 텍스트 스타일 — 액센트색 + 항상 밑줄, 호버 시 AccentHover. 밑줄을
        // 호버에만 두면 마우스를 올려보기 전까지 클릭할 수 있는 값인지 알 길이 없다
        // (ModernLabel의 Hyperlink·LinkLabel AlwaysUnderline과 같은 체감). 좁은 컬럼의
        // 잘림 툴팁은 텍스트 컬럼과 같은 규칙을 쓴다.
        private static Style BuildLinkTextStyle(FrameworkElement resourceSource)
        {
            Style style = new Style(typeof(TextBlock));
            style.Setters.Add(new Setter(TextBlock.ForegroundProperty, resourceSource.FindResource("Brush.Accent")));
            style.Setters.Add(new Setter(TextBlock.TextDecorationsProperty, TextDecorations.Underline));
            style.Setters.Add(new Setter(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis));
            style.Setters.Add(new Setter(
                TextBlock.ToolTipProperty,
                new Binding("Text") { RelativeSource = new RelativeSource(RelativeSourceMode.Self) }));
            style.Setters.Add(new EventSetter(
                ToolTipService.ToolTipOpeningEvent, new ToolTipEventHandler(OnCellToolTipOpening)));

            Trigger hover = new Trigger();
            hover.Property = UIElement.IsMouseOverProperty;
            hover.Value = true;
            hover.Setters.Add(new Setter(TextBlock.ForegroundProperty, resourceSource.FindResource("Brush.AccentHover")));
            style.Triggers.Add(hover);

            return style;
        }

        // 크롬 없는 버튼 템플릿 — 내용(링크 텍스트)만 그대로 내보낸다.
        private static ControlTemplate BuildBareButtonTemplate()
        {
            ControlTemplate template = new ControlTemplate(typeof(Button));
            template.VisualTree = new FrameworkElementFactory(typeof(ContentPresenter));
            return template;
        }

        // 콤보 입력 컬럼: ComboItems의 고정 선택지 중 하나를 고르면 원본 행
        // 컬럼 값이 즉시 갱신된다 (체크박스 컬럼과 같은 양방향 규칙 — 그리드가
        // 읽기 전용이어도 CellTemplate 안의 콤보는 살아 있다). 행별 입력 가능
        // 여부는 ComboEnabledMember 컬럼 값(bool/Y/N)이 제어한다.
        private static DataGridColumn CreateComboColumn(
            ModernDataGridColumn definition,
            FrameworkElement resourceSource)
        {
            DataGridTemplateColumn column = new DataGridTemplateColumn();
            column.Header = GridHeaderCaption.CreateHeader(definition);
            column.SortMemberPath = definition.DataPropertyName;

            FrameworkElementFactory combo = new FrameworkElementFactory(typeof(ComboBox));
            combo.SetValue(FrameworkElement.StyleProperty, resourceSource.FindResource("Parts.GridComboBoxStyle"));

            // 선택지 목록 — ComboItemsMember(행별)가 지정되면 그 컬럼 값을 각
            // 행의 목록으로 바인딩하고(행마다 범위가 다를 때), 없으면 컬럼 공통
            // ComboItems 고정 목록을 쓴다.
            if (!string.IsNullOrEmpty(definition.ComboItemsMember))
            {
                combo.SetBinding(ItemsControl.ItemsSourceProperty,
                        new Binding(definition.ComboItemsMember));
            }
            else
            {
                combo.SetValue(ItemsControl.ItemsSourceProperty, definition.ComboItems ?? new string[0]);
            }

            combo.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            combo.SetValue(FrameworkElement.MarginProperty, new Thickness(2d, 0d, 2d, 0d));

            // 배지형 콤보 — 선택지별 색이 지정되면 선택 값과 드롭다운 항목을
            // 레티클 배지로 그리고 (배지 컬럼과 같은 글자색 유도 규칙),
            // **필드 표면 전체**도 선택 값의 배지 색으로 칠한다 — 미선택이면
            // 기본 필드(Surface)로 떨어진다.
            if (definition.ComboItemColors != null && definition.ComboItemColors.Length > 0)
            {
                combo.SetValue(
                        ItemsControl.ItemTemplateProperty,
                        BuildComboBadgeTemplate(definition, resourceSource));
            }
            else
            {
                // 텍스트 콤보도 드롭다운 선택 항목에 체크(✓) 표식을 붙인다 —
                // 배지형과 같은 문법 (체크 자리는 항목 전체에 예약되어 정렬 유지).
                combo.SetValue(
                        ItemsControl.ItemTemplateProperty,
                        BuildComboTextItemTemplate(resourceSource));
            }

            if (definition.ComboItemColors != null && definition.ComboItemColors.Length > 0)
            {

                Binding surfaceBinding = new Binding(definition.DataPropertyName);
                surfaceBinding.Converter = new ComboItemBadgeConverter(
                        definition.ComboItems, definition.ComboItemColors, false);
                surfaceBinding.FallbackValue = resourceSource.FindResource("Brush.Surface");
                combo.SetBinding(Control.BackgroundProperty, surfaceBinding);
            }

            Binding valueBinding = new Binding(definition.DataPropertyName);
            valueBinding.Mode = BindingMode.TwoWay;
            valueBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            combo.SetBinding(Selector.SelectedItemProperty, valueBinding);

            if (!string.IsNullOrEmpty(definition.ComboEnabledMember))
            {
                Binding enabledBinding = new Binding(definition.ComboEnabledMember);
                enabledBinding.Converter = truthyConverter;
                combo.SetBinding(UIElement.IsEnabledProperty, enabledBinding);
            }

            DataTemplate template = new DataTemplate();
            template.VisualTree = combo;
            column.CellTemplate = template;

            ApplyColumnWidth(column, definition);
            return column;
        }

        // 배지형 콤보의 항목 템플릿 — 항목 문자열을 (ComboItems ↔
        // ComboItemColors) 매핑 색의 배지(모양은 BadgeShape, 기본 둥근 사각)로
        // 감싼다. 배지는 행 전체 폭을 채워(가운데 텍스트) 필드 표면 색과
        // 이어져 보인다. ItemTemplate 하나로 선택 값 표시(SelectionBox)와
        // 드롭다운 항목이 함께 배지가 된다.
        private static DataTemplate BuildComboBadgeTemplate(
            ModernDataGridColumn definition,
            FrameworkElement resourceSource)
        {
            IValueConverter backgroundConverter = new ComboItemBadgeConverter(
                    definition.ComboItems, definition.ComboItemColors, false);
            IValueConverter foregroundConverter = new ComboItemBadgeConverter(
                    definition.ComboItems, definition.ComboItemColors, true);

            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.CornerRadiusProperty, resourceSource.FindResource(
                    definition.BadgeShape == ChipShape.Pill ? "Radius.Pill" : "Radius.Sm"));

            // 좌우 4px — 배지 컬럼(10px)보다 좁다. 콤보 표면 전체가 이미 배지
            // 색으로 칠해지고 선택 표시 영역 바깥에 콤보 자체 여백(좌 10 +
            // 드롭 글리프 26)이 있어서, 여기까지 10을 주면 좁은 컬럼(예: 110px)
            // 에서 "PASS"의 마지막 글자가 잘린다.
            border.SetValue(Border.PaddingProperty, new Thickness(4d, 1d, 4d, 1d));
            border.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            border.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

            Binding backgroundBinding = new Binding();
            backgroundBinding.Converter = backgroundConverter;
            border.SetBinding(Border.BackgroundProperty, backgroundBinding);

            FrameworkElementFactory text = new FrameworkElementFactory(typeof(TextBlock));
            text.SetBinding(TextBlock.TextProperty, new Binding());
            text.SetValue(TextBlock.FontSizeProperty, resourceSource.FindResource("Font.Size.Label"));
            text.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold);
            text.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            text.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

            Binding foregroundBinding = new Binding();
            foregroundBinding.Converter = foregroundConverter;
            text.SetBinding(TextBlock.ForegroundProperty, foregroundBinding);

            border.AppendChild(text);

            // 선택 표식 — 칩 테두리를 훑는 하이라이트(옅은 액센트 링 + 좌→우
            // 코멧 빛띠; 그리드 배지의 BadgeSpinBorderMember와 같은 시각 언어)를
            // 칩 위에 겹친다. 드롭다운의 선택 항목에만 보이고, 필드(선택 값
            // 표시 영역 — ComboBoxItem 조상이 없다)에서는 FallbackValue로
            // 아예 빠진다. 팝업이 닫히면 항목이 언로드되어 모션도 함께 멎는다.
            FrameworkElementFactory spin = new FrameworkElementFactory(
                    typeof(Modern.Lab.Controls.Wpf.Display.ModernSpinBorderControl));
            spin.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            spin.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Stretch);

            Binding spinVisible = new Binding("IsSelected");
            spinVisible.RelativeSource = new RelativeSource(
                    RelativeSourceMode.FindAncestor, typeof(ComboBoxItem), 1);
            spinVisible.Converter = spinnerVisibilityConverter;
            spinVisible.FallbackValue = Visibility.Collapsed;
            spin.SetBinding(UIElement.VisibilityProperty, spinVisible);

            FrameworkElementFactory host = new FrameworkElementFactory(typeof(Grid));
            host.AppendChild(border);
            host.AppendChild(spin);

            DataTemplate template = new DataTemplate();
            template.VisualTree = host;
            return template;
        }

        // 텍스트 콤보(색 지정 없는 ComboItems)의 항목 템플릿 — 왼쪽 체크(✓)
        // 선택 표식 + 값 텍스트. 체크 자리는 항목 전체에 예약되어 정렬이 유지된다.
        private static DataTemplate BuildComboTextItemTemplate(FrameworkElement resourceSource)
        {
            FrameworkElementFactory check = BuildComboCheckGlyph(resourceSource);
            check.SetValue(TextBlock.ForegroundProperty, resourceSource.FindResource("Brush.TextSecondary"));

            FrameworkElementFactory text = new FrameworkElementFactory(typeof(TextBlock));
            text.SetBinding(TextBlock.TextProperty, new Binding());
            text.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

            FrameworkElementFactory content = new FrameworkElementFactory(typeof(StackPanel));
            content.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            content.AppendChild(check);
            content.AppendChild(text);

            DataTemplate template = new DataTemplate();
            template.VisualTree = content;
            return template;
        }

        // 콤보 항목 선택 표식 체크(✓) 공용 팩토리 — 드롭다운에서 선택된
        // 항목이면 Visible, 아니면 Hidden(자리 유지로 정렬 보존), 필드(선택
        // 값 표시 영역 — ComboBoxItem 조상이 없다)에서는 Collapsed로 아예
        // 빠진다. 글자와의 간격 6px. 색은 호출부가 지정한다.
        private static FrameworkElementFactory BuildComboCheckGlyph(FrameworkElement resourceSource)
        {
            FrameworkElementFactory check = new FrameworkElementFactory(typeof(TextBlock));
            check.SetValue(TextBlock.TextProperty, "");
            check.SetValue(TextBlock.FontFamilyProperty, resourceSource.FindResource("Font.Family.Icon"));
            check.SetValue(TextBlock.FontSizeProperty, resourceSource.FindResource("Font.Size.IconSm"));
            check.SetValue(FrameworkElement.MarginProperty, new Thickness(0d, 0d, 6d, 0d));
            check.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

            Binding visible = new Binding("IsSelected");
            visible.RelativeSource = new RelativeSource(
                    RelativeSourceMode.FindAncestor, typeof(ComboBoxItem), 1);
            visible.Converter = selectedCheckVisibilityConverter;
            visible.FallbackValue = Visibility.Collapsed;
            check.SetBinding(UIElement.VisibilityProperty, visible);

            return check;
        }

        // 셀 안 버튼: ModernButton Secondary와 같은 문법의 차분한 버튼 —
        // 평상시 흰 배경 + 회색 테두리 + 진한 글자, hover는 옅은 파랑 틴트 +
        // 액센트 테두리/글자, pressed는 한 단계 진한 틴트. 행마다 반복 노출되는
        // 액션이라 평상시엔 중립을 유지하고 상호작용 시에만 액센트가 드러난다.
        // 캡션이 갑갑하지 않도록 상하 패딩을 확보해 행 높이(32) 안에서 위아래
        // 중앙에 온다. 비활성 행은 회색 채움/텍스트로 눌 수 없음을 드러낸다.
        private static Style BuildCellButtonStyle(FrameworkElement resourceSource)
        {
            Style style = new Style(typeof(Button));
            style.Setters.Add(new Setter(Control.ForegroundProperty, resourceSource.FindResource("Brush.TextPrimary")));
            style.Setters.Add(new Setter(Control.BackgroundProperty, resourceSource.FindResource("Brush.Surface")));
            style.Setters.Add(new Setter(Control.BorderBrushProperty, resourceSource.FindResource("Brush.Border")));
            style.Setters.Add(new Setter(Control.FontSizeProperty, resourceSource.FindResource("Font.Size.Label")));
            style.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.Normal));
            // 좌우 10px — 행마다 반복되는 버튼이라 컬럼 폭을 잡아먹지 않게
            // 일반 버튼(14px)보다 한 단계 좁힌다.
            style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(10d, 4d, 10d, 4d)));
            style.Setters.Add(new Setter(FrameworkElement.CursorProperty, System.Windows.Input.Cursors.Hand));
            style.Setters.Add(new Setter(Control.TemplateProperty, BuildCellButtonTemplate(resourceSource)));

            Trigger hover = new Trigger();
            hover.Property = UIElement.IsMouseOverProperty;
            hover.Value = true;
            hover.Setters.Add(new Setter(Control.BackgroundProperty, resourceSource.FindResource("Brush.InfoBackground")));
            hover.Setters.Add(new Setter(Control.BorderBrushProperty, resourceSource.FindResource("Brush.Accent")));
            hover.Setters.Add(new Setter(Control.ForegroundProperty, resourceSource.FindResource("Brush.Accent")));
            style.Triggers.Add(hover);

            Trigger pressed = new Trigger();
            pressed.Property = ButtonBase.IsPressedProperty;
            pressed.Value = true;
            pressed.Setters.Add(new Setter(Control.BackgroundProperty, resourceSource.FindResource("Brush.SelectedBackground")));
            pressed.Setters.Add(new Setter(Control.BorderBrushProperty, resourceSource.FindResource("Brush.AccentHover")));
            pressed.Setters.Add(new Setter(Control.ForegroundProperty, resourceSource.FindResource("Brush.AccentHover")));
            style.Triggers.Add(pressed);

            Trigger disabled = new Trigger();
            disabled.Property = UIElement.IsEnabledProperty;
            disabled.Value = false;
            disabled.Setters.Add(new Setter(Control.BackgroundProperty, resourceSource.FindResource("Brush.DisabledBackground")));
            disabled.Setters.Add(new Setter(Control.ForegroundProperty, resourceSource.FindResource("Brush.DisabledText")));
            disabled.Setters.Add(new Setter(Control.BorderBrushProperty, resourceSource.FindResource("Brush.BorderSubtle")));
            style.Triggers.Add(disabled);

            return style;
        }

        // 자동색 셀 버튼(ButtonAutoColor): 캡션 값에서 유도한 색으로 그리는 변형 —
        // 색 정체(Hue)는 배지 자동색과 같은 Palette.FromValue 칩이지만, 배경은
        // 칩 전면 채움이 아니라 **표면에 칩을 옅게 섞은 틴트**다(칩 채움은 버튼
        // 면적에서 너무 강렬하다 — ButtonDerivedColorConverter 주석 참고). 글자와
        // 테두리가 칩 색 계열을 쥐고, hover/pressed는 틴트를 단계적으로 짙게 해
        // 중립 버튼과 같은 상호작용 문법을 유지한다(글자는 각 단계 배경에 대비
        // 4.5:1 재보장). 비활성 상태는 중립 버튼과 같은 회색 처리, 캡션이 비거나
        // 해석 불가한 행은 중립 버튼 색으로 폴백한다.
        private static Style BuildAutoColorCellButtonStyle(
            FrameworkElement resourceSource,
            ModernDataGridColumn definition)
        {
            Style style = new Style(typeof(Button));
            style.Setters.Add(new Setter(Control.ForegroundProperty, AutoColorValue(
                    definition, ButtonColorPart.Foreground,
                    (Brush)resourceSource.FindResource("Brush.TextPrimary"))));
            style.Setters.Add(new Setter(Control.BackgroundProperty, AutoColorValue(
                    definition, ButtonColorPart.Background,
                    (Brush)resourceSource.FindResource("Brush.Surface"))));
            style.Setters.Add(new Setter(Control.BorderBrushProperty, AutoColorValue(
                    definition, ButtonColorPart.Border,
                    (Brush)resourceSource.FindResource("Brush.Border"))));
            style.Setters.Add(new Setter(Control.FontSizeProperty, resourceSource.FindResource("Font.Size.Label")));
            style.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.Normal));
            style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(10d, 4d, 10d, 4d)));
            style.Setters.Add(new Setter(FrameworkElement.CursorProperty, System.Windows.Input.Cursors.Hand));
            style.Setters.Add(new Setter(Control.TemplateProperty, BuildCellButtonTemplate(resourceSource)));

            Trigger hover = new Trigger();
            hover.Property = UIElement.IsMouseOverProperty;
            hover.Value = true;
            hover.Setters.Add(new Setter(Control.BackgroundProperty, AutoColorValue(
                    definition, ButtonColorPart.HoverBackground,
                    (Brush)resourceSource.FindResource("Brush.InfoBackground"))));
            hover.Setters.Add(new Setter(Control.BorderBrushProperty, AutoColorValue(
                    definition, ButtonColorPart.EmphasisBorder,
                    (Brush)resourceSource.FindResource("Brush.Accent"))));
            hover.Setters.Add(new Setter(Control.ForegroundProperty, AutoColorValue(
                    definition, ButtonColorPart.HoverForeground,
                    (Brush)resourceSource.FindResource("Brush.Accent"))));
            style.Triggers.Add(hover);

            Trigger pressed = new Trigger();
            pressed.Property = ButtonBase.IsPressedProperty;
            pressed.Value = true;
            pressed.Setters.Add(new Setter(Control.BackgroundProperty, AutoColorValue(
                    definition, ButtonColorPart.PressedBackground,
                    (Brush)resourceSource.FindResource("Brush.SelectedBackground"))));
            pressed.Setters.Add(new Setter(Control.BorderBrushProperty, AutoColorValue(
                    definition, ButtonColorPart.EmphasisBorder,
                    (Brush)resourceSource.FindResource("Brush.AccentHover"))));
            pressed.Setters.Add(new Setter(Control.ForegroundProperty, AutoColorValue(
                    definition, ButtonColorPart.PressedForeground,
                    (Brush)resourceSource.FindResource("Brush.AccentHover"))));
            style.Triggers.Add(pressed);

            Trigger disabled = new Trigger();
            disabled.Property = UIElement.IsEnabledProperty;
            disabled.Value = false;
            disabled.Setters.Add(new Setter(Control.BackgroundProperty, resourceSource.FindResource("Brush.DisabledBackground")));
            disabled.Setters.Add(new Setter(Control.ForegroundProperty, resourceSource.FindResource("Brush.DisabledText")));
            disabled.Setters.Add(new Setter(Control.BorderBrushProperty, resourceSource.FindResource("Brush.BorderSubtle")));
            style.Triggers.Add(disabled);

            return style;
        }

        // 캡션 → 부위별 유도색 세터 값. 행별 캡션(ButtonTextMember)이면 행 값
        // 바인딩으로, 고정 캡션(ButtonText)이면 지금 계산한 고정 브러시로 만든다.
        private static object AutoColorValue(
            ModernDataGridColumn definition,
            ButtonColorPart part,
            Brush fallback)
        {
            if (!string.IsNullOrEmpty(definition.ButtonTextMember))
            {
                Binding binding = new Binding(definition.ButtonTextMember);
                binding.Converter = new ButtonDerivedColorConverter(part, fallback);
                return binding;
            }

            return ButtonDerivedColorConverter.Resolve(definition.ButtonText, part, fallback);
        }

        // 셀 버튼 공용 템플릿 — 둥근 사각 테두리 + 중앙 캡션. 색은 스타일
        // (중립 BuildCellButtonStyle / 자동색 BuildAutoColorCellButtonStyle)이 정한다.
        private static ControlTemplate BuildCellButtonTemplate(FrameworkElement resourceSource)
        {
            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.CornerRadiusProperty, resourceSource.FindResource("Radius.Sm"));
            border.SetValue(Border.BorderThicknessProperty, new Thickness(1d));
            border.SetBinding(Border.BackgroundProperty, TemplateBinding(Control.BackgroundProperty));
            border.SetBinding(Border.BorderBrushProperty, TemplateBinding(Control.BorderBrushProperty));
            border.SetBinding(Border.PaddingProperty, TemplateBinding(Control.PaddingProperty));

            FrameworkElementFactory presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            presenter.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            border.AppendChild(presenter);
            template.VisualTree = border;

            return template;
        }

        // 코드 생성 템플릿에서 TemplateBinding과 같은 효과를 내는 바인딩 헬퍼.
        private static Binding TemplateBinding(DependencyProperty property)
        {
            Binding binding = new Binding(property.Name);
            binding.RelativeSource = RelativeSource.TemplatedParent;
            return binding;
        }

        // 정의 폭(양수)이 있으면 고정 픽셀, 없으면 별(*) 너비로 채운다.
        private static void ApplyColumnWidth(DataGridColumn column, ModernDataGridColumn definition)
        {
            if (definition.Width > 0d)
            {
                column.Width = new DataGridLength(definition.Width);
            }
            else
            {
                column.Width = new DataGridLength(1d, DataGridLengthUnitType.Star);
            }
        }

        // 스피너 컬럼: 인디터미네이트 회전 원(작업 진행중 표시). SpinnerActiveMember
        // 컬럼 값이 참인 행만 스피너가 보이고, 나머지는 빈 셀이다. 값 표시가
        // 아니라 상태 표시라 정렬 대상(SortMemberPath)을 두지 않는다.
        private static DataGridColumn CreateSpinnerColumn(ModernDataGridColumn definition)
        {
            DataGridTemplateColumn column = new DataGridTemplateColumn();
            column.Header = GridHeaderCaption.CreateHeader(definition);

            FrameworkElementFactory spinner =
                    new FrameworkElementFactory(typeof(Modern.Lab.Controls.Wpf.Display.ModernSpinnerControl));
            spinner.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            spinner.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

            // 행별 표시 — SpinnerActiveMember가 참인 행만 스피너가 보인다.
            if (!string.IsNullOrEmpty(definition.SpinnerActiveMember))
            {
                Binding visibleBinding = new Binding(definition.SpinnerActiveMember);
                visibleBinding.Converter = spinnerVisibilityConverter;
                spinner.SetBinding(UIElement.VisibilityProperty, visibleBinding);
            }

            DataTemplate template = new DataTemplate();
            template.VisualTree = spinner;
            column.CellTemplate = template;

            ApplyColumnWidth(column, definition);
            return column;
        }

        // GridTextAlignment → WPF HorizontalAlignment 변환.
        private static HorizontalAlignment ToHorizontalAlignment(GridTextAlignment alignment)
        {
            if (alignment == GridTextAlignment.Center)
            {
                return HorizontalAlignment.Center;
            }

            if (alignment == GridTextAlignment.Right)
            {
                return HorizontalAlignment.Right;
            }

            return HorizontalAlignment.Left;
        }
    }
}
