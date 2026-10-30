using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Modern.Lab.Controls.Wpf.Input
{
    /// <summary>
    /// 모던 한 줄 텍스트 입력.
    /// - Text: 입력 텍스트 (양방향)
    /// - Placeholder: 텍스트가 비어 있는 동안 표시되는 힌트
    /// - IsReadOnly: 읽기 전용 상태
    /// - CharacterCasing: 입력 즉시 대문자/소문자 강제 (코드/ID 입력용)
    /// - AllowedCharacters: 허용 문자 집합 — 그 외 문자는 입력/붙여넣기 모두 제거
    /// - Mask: MaskedTextBox 마스크 언어의 입력 마스크 (예: "0000-00-00") —
    ///   지정하면 IME/자동완성이 꺼지고 모든 편집이 마스크를 통과한다
    /// - AutoCompleteItemsSource: 추천 드롭다운의 후보 항목
    /// - TextChanged: Text가 바뀔 때마다 발생
    /// - EnterPressed: Enter 키를 눌렀을 때 발생 (엔터로 검색)
    ///
    /// IME 참고: placeholder 표시와 추천 필터링은 내부 TextBox.TextChanged에서
    /// 동작하는데, 이 이벤트는 한글 조합 중에도 발생하므로 자음 하나만 입력해도
    /// placeholder가 숨겨지고 추천이 필터링된다.
    /// (Text DP 자체는 조합이 확정될 때 갱신된다 — WPF는 조합이 진행 중인 동안
    /// 바인딩 소스 업데이트를 미룬다.)
    /// </summary>
    public partial class ModernTextBoxControl : UserControl
    {
        // 후보 개수의 안전 상한 — 화면 표시 제한이 아니다. 매칭된 후보는 모두
        // 목록에 담기고 드롭다운의 MaxHeight + 스크롤로 보여준다. 이 값은
        // 로컬 소스가 비정상적으로 클 때(수만 건) UI가 멈추는 것만 막는다.
        private const int MaxSuggestionCount = 100;

        /// <summary>입력 텍스트. 기본적으로 양방향 바인딩.</summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(ModernTextBoxControl),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnTextPropertyChanged));

        /// <summary>입력이 비어 있는 동안 표시되는 힌트 텍스트.</summary>
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(
                "Placeholder",
                typeof(string),
                typeof(ModernTextBoxControl),
                new PropertyMetadata(string.Empty));

        /// <summary>에디터의 읽기 전용 상태.</summary>
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(
                "IsReadOnly",
                typeof(bool),
                typeof(ModernTextBoxControl),
                new PropertyMetadata(false));

        /// <summary>입력 즉시 대문자/소문자로 강제 변환 (기본 Normal = 변환 없음).</summary>
        public static readonly DependencyProperty CharacterCasingProperty =
            DependencyProperty.Register(
                "CharacterCasing",
                typeof(CharacterCasing),
                typeof(ModernTextBoxControl),
                new PropertyMetadata(CharacterCasing.Normal, OnCharacterCasingChanged));

        /// <summary>
        /// 허용 문자 집합. 비어 있으면 제한 없음. 지정하면 타이핑·붙여넣기·IME를
        /// 가리지 않고 집합에 없는 문자가 제거된다 (예: 대문자 ID 입력용
        /// "ABC...XYZ0123456789.-").
        /// </summary>
        public static readonly DependencyProperty AllowedCharactersProperty =
            DependencyProperty.Register(
                "AllowedCharacters",
                typeof(string),
                typeof(ModernTextBoxControl),
                new PropertyMetadata(string.Empty));

        /// <summary>필수 입력 필드 표시 — 필드 왼쪽에 빨간 세로 바를 그린다.</summary>
        public static readonly DependencyProperty RequiredProperty =
            DependencyProperty.Register(
                "Required",
                typeof(bool),
                typeof(ModernTextBoxControl),
                new PropertyMetadata(false));

        /// <summary>
        /// 추천 드롭다운의 후보 항목. 임의의 IEnumerable; 각 항목의 ToString()이
        /// 입력한 텍스트와 매칭된다(contains, 대소문자 무시).
        /// null이면 자동완성이 비활성화된다.
        /// </summary>
        public static readonly DependencyProperty AutoCompleteItemsSourceProperty =
            DependencyProperty.Register(
                "AutoCompleteItemsSource",
                typeof(IEnumerable),
                typeof(ModernTextBoxControl),
                new PropertyMetadata(null, OnAutoCompleteItemsSourceChanged));

        /// <summary>
        /// 추천 후보의 클라이언트 측 재대조 방식 (기본 Contains).
        /// None이면 AutoCompleteItemsSource에 할당된 목록을 재대조 없이 그대로
        /// 띄운다 — 서버가 후보 텍스트에 보이지 않는 컬럼(의뢰번호 등)으로
        /// 매칭해 돌려주는 디바운스 typeahead에서 contains 재대조가 후보를
        /// 전부 걸러내는 것을 막는다. 음소거 래치·최대 표시 건수·"입력과
        /// 동일한 후보 제외"는 모드와 무관하게 동작한다.
        /// </summary>
        public static readonly DependencyProperty SuggestionFilterModeProperty =
            DependencyProperty.Register(
                "SuggestionFilterMode",
                typeof(SuggestionFilterMode),
                typeof(ModernTextBoxControl),
                new PropertyMetadata(SuggestionFilterMode.Contains));

        /// <summary>
        /// 여러 줄 입력 모드 (비고/특이사항용). 켜면 Enter가 줄바꿈이 되고
        /// (EnterPressed 미발생), 자동 줄바꿈 + 세로 스크롤바가 붙으며 높이는
        /// 래퍼가 준 크기를 그대로 채운다. 자동완성 드롭다운은 비활성화된다.
        /// </summary>
        public static readonly DependencyProperty MultilineProperty =
            DependencyProperty.Register(
                "Multiline",
                typeof(bool),
                typeof(ModernTextBoxControl),
                new PropertyMetadata(false, OnMultilineChanged));

        /// <summary>최대 입력 글자 수. 0 = 제한 없음 (WPF TextBox.MaxLength 의미 그대로).</summary>
        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register(
                "MaxLength",
                typeof(int),
                typeof(ModernTextBoxControl),
                new PropertyMetadata(0, OnMaxLengthChanged));

        /// <summary>
        /// 필드 오른쪽 안에 표시되는 트레일링 버튼의 글리프(Segoe MDL2 Assets).
        /// 비어 있으면(기본) 버튼이 없다 — "입력칸 + 조회/찾기 버튼 일체형"
        /// 레거시 패턴 대체용. 클릭은 ButtonClick으로 올라온다.
        /// </summary>
        public static readonly DependencyProperty ButtonGlyphProperty =
            DependencyProperty.Register(
                "ButtonGlyph",
                typeof(string),
                typeof(ModernTextBoxControl),
                new PropertyMetadata(string.Empty, OnButtonGlyphChanged));

        /// <summary>
        /// 값이 있을 때 지우기(✕) 버튼을 표시할지 여부 (기본 false). 검색어나
        /// 파일 경로처럼 "한 번에 비우기" 동선이 필요한 필드에 켠다 — 값이
        /// 비면 버튼도 사라지고, 트레일링 버튼과 함께 쓰면 그 왼쪽에 선다.
        /// </summary>
        public static readonly DependencyProperty ShowClearButtonProperty =
            DependencyProperty.Register(
                "ShowClearButton",
                typeof(bool),
                typeof(ModernTextBoxControl),
                new PropertyMetadata(false, OnShowClearButtonChanged));

        private static void OnShowClearButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernTextBoxControl)d).UpdateTrailingLayout();
        }

        /// <summary>
        /// 입력 마스크 (빈 문자열 = 마스크 없음). WinForms MaskedTextBox와 같은
        /// 마스크 언어(System.ComponentModel.MaskedTextProvider)를 그대로 쓴다 —
        /// 0(필수 숫자), 9(선택 숫자), L(필수 영문), A(필수 영숫자), &amp;(임의 문자),
        /// &gt;/&lt;(이후 대/소문자 강제), 그 외 문자는 리터럴. 자릿수 고정 코드
        /// 입력용이다(예: 레시피 "&gt;LLL-0000") — 모양만 강제하고 값 유효성은
        /// 검증하지 않으므로 날짜 입력은 날짜 선택기가 담당한다.
        /// 지정하면 미입력 자리는 프롬프트 문자('_')로 표시되고, 타이핑/붙여넣기/
        /// 삭제가 전부 마스크를 통과한다. IME·자동완성은 마스크 입력과 충돌하므로
        /// 마스크가 걸린 동안 비활성화된다. Text는 화면 표시 문자열 그대로
        /// (리터럴 + 프롬프트 포함)이며, 전부 채워졌는지는 MaskCompleted로 판정한다.
        /// </summary>
        public static readonly DependencyProperty MaskProperty =
            DependencyProperty.Register(
                "Mask",
                typeof(string),
                typeof(ModernTextBoxControl),
                new PropertyMetadata(string.Empty, OnMaskChanged));

        private readonly ObservableCollection<string> suggestionItems;
        private bool suppressSuggestions;

        // 추천 음소거 래치 — 추천 확정·Esc 닫기·폼의 CloseSuggestions 호출 뒤
        // 사용자가 다시 타이핑하기 전까지 typeahead 소스 재할당이 드롭다운을
        // 다시 열지 못하게 한다. 확정된 텍스트가 다른 후보의 접두사이면
        // (예: 분할 자식 랏 IT10001ED → IT10001ED-S1/-S2) 디바운스 재조회
        // 도착이 목록을 도로 여는 결함의 방지 장치. 텍스트가 바뀌면 해제.
        private bool suggestionsMuted;

        // 마스크 엔진 — Mask가 비어 있으면 null(일반 텍스트 동작).
        // WinForms MaskedTextBox가 쓰는 것과 같은 BCL 파서라 마스크 의미가
        // 레거시 폼과 1:1로 일치한다.
        private System.ComponentModel.MaskedTextProvider maskProvider;

        // SetMaskedText가 표시 문자열을 쓰는 동안 TextChanged 재진입(재정규화)을 막는 플래그.
        private bool applyingMaskText;

        /// <summary><see cref="Text"/>가 바뀔 때마다 발생한다.</summary>
        public event EventHandler TextChanged;

        /// <summary>에디터 안에서 Enter 키를 눌렀을 때 발생한다.</summary>
        public event EventHandler EnterPressed;

        /// <summary>
        /// 자동완성 후보를 마우스로 클릭해 확정했을 때 발생한다 — 텍스트가 후보로 바뀐 뒤다.
        /// 키보드 확정(↓/↑ + Enter)은 확정 뒤 EnterPressed 로 오므로, "고르면 바로 조회"는
        /// 이 이벤트와 EnterPressed 를 같은 핸들러에 건다.
        /// </summary>
        public event EventHandler SuggestionClicked;

        /// <summary>트레일링 버튼을 클릭했을 때 발생한다 (ButtonGlyph 지정 시).</summary>
        public event EventHandler ButtonClick;

        public ModernTextBoxControl()
        {
            this.suggestionItems = new ObservableCollection<string>();
            this.InitializeComponent();
            this.SuggestionList.ItemsSource = this.suggestionItems;

            // 마스크 모드의 붙여넣기/잘라내기 가로채기 — 마스크가 없으면 무동작.
            DataObject.AddPastingHandler(this.InnerTextBox, this.OnMaskedPasting);
            System.Windows.Input.CommandManager.AddPreviewExecutedHandler(
                    this.InnerTextBox, this.OnEditorPreviewExecuted);

            // 지우기 버튼(ShowClearButton)은 값이 있는 동안만 보인다 —
            // 값 변화를 따라 표시/여백을 갱신한다.
            this.InnerTextBox.TextChanged += delegate { this.UpdateTrailingLayout(); };

            // 잘린 값 툴팁 — 내용이 표시 폭보다 길 때만 호버 시 전체 값을
            // 보여준다 (그리드 셀의 잘림 툴팁과 같은 문법). ToolTip은 열기
            // 시점에 채우고, 안 잘렸으면 열지 않는다.
            this.InnerTextBox.ToolTip = string.Empty;
            this.InnerTextBox.ToolTipOpening += this.OnEditorToolTipOpening;
        }

        // 에디터 내용이 실제로 잘렸을 때만 전체 값 툴팁을 연다 — TextBox의
        // 스크롤 범위(ExtentWidth)가 표시 폭(ViewportWidth)보다 크면 잘림이다.
        private void OnEditorToolTipOpening(object sender, ToolTipEventArgs e)
        {
            bool truncated = this.InnerTextBox.Text.Length > 0
                    && this.InnerTextBox.ExtentWidth > this.InnerTextBox.ViewportWidth + 0.5;

            if (!truncated)
            {
                e.Handled = true;
                return;
            }

            this.InnerTextBox.ToolTip = this.InnerTextBox.Text;
        }

        /// <summary>입력 텍스트.</summary>
        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        /// <summary>입력이 비어 있는 동안 표시되는 힌트 텍스트.</summary>
        public string Placeholder
        {
            get { return (string)this.GetValue(PlaceholderProperty); }
            set { this.SetValue(PlaceholderProperty, value); }
        }

        /// <summary>에디터의 읽기 전용 상태.</summary>
        public bool IsReadOnly
        {
            get { return (bool)this.GetValue(IsReadOnlyProperty); }
            set { this.SetValue(IsReadOnlyProperty, value); }
        }

        /// <summary>입력 즉시 대문자/소문자로 강제 변환 (기본 Normal).</summary>
        public CharacterCasing CharacterCasing
        {
            get { return (CharacterCasing)this.GetValue(CharacterCasingProperty); }
            set { this.SetValue(CharacterCasingProperty, value); }
        }

        /// <summary>허용 문자 집합 (빈 문자열 = 제한 없음).</summary>
        public string AllowedCharacters
        {
            get { return (string)this.GetValue(AllowedCharactersProperty); }
            set { this.SetValue(AllowedCharactersProperty, value); }
        }

        /// <summary>필수 입력 필드 표시(필드 왼쪽 빨간 세로 바).</summary>
        public bool Required
        {
            get { return (bool)this.GetValue(RequiredProperty); }
            set { this.SetValue(RequiredProperty, value); }
        }

        /// <summary>추천 드롭다운의 후보 항목. null이면 자동완성이 비활성화된다.</summary>
        public IEnumerable AutoCompleteItemsSource
        {
            get { return (IEnumerable)this.GetValue(AutoCompleteItemsSourceProperty); }
            set { this.SetValue(AutoCompleteItemsSourceProperty, value); }
        }

        public SuggestionFilterMode SuggestionFilterMode
        {
            get { return (SuggestionFilterMode)this.GetValue(SuggestionFilterModeProperty); }
            set { this.SetValue(SuggestionFilterModeProperty, value); }
        }

        /// <summary>여러 줄 입력 모드. Enter = 줄바꿈, 자동완성 비활성.</summary>
        public bool Multiline
        {
            get { return (bool)this.GetValue(MultilineProperty); }
            set { this.SetValue(MultilineProperty, value); }
        }

        /// <summary>최대 입력 글자 수. 0 = 제한 없음.</summary>
        public int MaxLength
        {
            get { return (int)this.GetValue(MaxLengthProperty); }
            set { this.SetValue(MaxLengthProperty, value); }
        }

        /// <summary>트레일링 버튼 글리프(Segoe MDL2). 비어 있으면 버튼 없음.</summary>
        public string ButtonGlyph
        {
            get { return (string)this.GetValue(ButtonGlyphProperty); }
            set { this.SetValue(ButtonGlyphProperty, value); }
        }

        /// <summary>값이 있을 때 지우기(✕) 버튼 표시 여부 (기본 false).</summary>
        public bool ShowClearButton
        {
            get { return (bool)this.GetValue(ShowClearButtonProperty); }
            set { this.SetValue(ShowClearButtonProperty, value); }
        }

        /// <summary>입력 마스크 (MaskedTextBox 마스크 언어, 빈 문자열 = 마스크 없음).</summary>
        public string Mask
        {
            get { return (string)this.GetValue(MaskProperty); }
            set { this.SetValue(MaskProperty, value); }
        }

        /// <summary>
        /// 마스크의 모든 필수 자리가 채워졌는지 여부 (마스크가 없으면 항상 true).
        /// 선택 자리(9, ?, a)는 비어 있어도 true다 — MaskedTextBox.MaskCompleted와 동일.
        /// </summary>
        public bool MaskCompleted
        {
            get { return this.maskProvider == null || this.maskProvider.MaskCompleted; }
        }

        /// <summary>키보드 포커스를 에디터로 이동한다.</summary>
        public void FocusEditor()
        {
            this.InnerTextBox.Focus();
        }

        // 에디터 글자를 전부 선택한다 — 그리드 찾기 바가 Ctrl+F 마다 지난 검색어를 바로 덮어 쓰게 할 때 쓴다(같은 어셈블리 전용).
        internal void SelectAllText()
        {
            this.InnerTextBox.SelectAll();
        }

        // 대소문자 강제는 WPF TextBox 자체 기능(CharacterCasing)에 위임한다 —
        // 입력 시점에 변환되므로 TextChanged에는 이미 변환된 텍스트가 들어온다.
        private static void OnCharacterCasingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernTextBoxControl control = (ModernTextBoxControl)d;
            control.InnerTextBox.CharacterCasing = (CharacterCasing)e.NewValue;
        }

        // 요구 높이 계약 — 자동 배치(무한 높이)에서는 설계 높이(32), 유한
        // 슬롯에서는 min(설계, 가용). 여러 줄 모드는 내용/슬롯 높이 그대로.
        // 근거: Common/FieldSlotHeight.cs.
        protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
        {
            if (this.Multiline)
            {
                return base.MeasureOverride(constraint);
            }

            double preferred = (double)this.FindResource("Size.ControlHeight");
            double height = Modern.Lab.Controls.Wpf.Common.FieldSlotHeight.Resolve(
                    constraint.Height, preferred);
            System.Windows.Size desired = base.MeasureOverride(
                    new System.Windows.Size(constraint.Width, height));

            return new System.Windows.Size(desired.Width, height);
        }

        // 여러 줄 모드 전환 — 스타일을 갈아끼우지 않고 요소 속성만 바꾼다.
        private static void OnMultilineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernTextBoxControl control = (ModernTextBoxControl)d;
            bool multiline = (bool)e.NewValue;

            // 한 줄 모드는 토큰 높이(32)까지, 여러 줄은 래퍼가 준 크기를 채운다.
            // 고정 Height가 아니라 MaxHeight: 호스트가 32 DIP보다 작은 슬롯을 주면
            // (스케일되지 않은 폼의 고DPI) 필드가 줄어들어 잘림을 막는다.
            control.OuterBorder.MaxHeight = multiline
                ? double.PositiveInfinity
                : (double)control.FindResource("Size.ControlHeight");

            control.InnerTextBox.AcceptsReturn = multiline;
            control.InnerTextBox.TextWrapping = multiline ? TextWrapping.Wrap : TextWrapping.NoWrap;
            control.InnerTextBox.VerticalScrollBarVisibility = multiline
                ? ScrollBarVisibility.Auto
                : ScrollBarVisibility.Hidden;
            control.InnerTextBox.VerticalContentAlignment = multiline
                ? VerticalAlignment.Top
                : VerticalAlignment.Center;

            // 플레이스홀더·필수 점도 첫 줄 기준선에 맞춘다.
            control.PlaceholderText.VerticalAlignment = multiline
                ? VerticalAlignment.Top
                : VerticalAlignment.Center;
            control.RequiredDot.VerticalAlignment = multiline
                ? VerticalAlignment.Top
                : VerticalAlignment.Center;
            control.RequiredDot.Margin = multiline
                ? new Thickness(0, 12, 10, 0)
                : new Thickness(0, 0, 10, 0);

            // 여러 줄 모드에서는 자동완성이 의미가 없다 — 열려 있으면 닫는다.
            if (multiline)
            {
                control.CloseSuggestionsCore();
            }
        }

        private static void OnMaxLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernTextBoxControl control = (ModernTextBoxControl)d;
            control.InnerTextBox.MaxLength = Math.Max(0, (int)e.NewValue);
        }

        private static void OnButtonGlyphChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernTextBoxControl)d).UpdateTrailingLayout();
        }

        // 트레일링/지우기 버튼 표시 전환 — 보이는 버튼들이 차지하는 폭만큼
        // 에디터 오른쪽 여백과 필수 점 위치를 밀어 글자가 버튼 밑으로 들어가지
        // 않게 하고, 지우기 버튼은 트레일링 버튼 왼쪽에 자리잡는다.
        private void UpdateTrailingLayout()
        {
            bool trailing = !string.IsNullOrEmpty(this.ButtonGlyph);
            bool clear = this.ShowClearButton && this.InnerTextBox.Text.Length > 0;

            this.TrailingButton.Visibility = trailing ? Visibility.Visible : Visibility.Collapsed;
            this.ClearButton.Visibility = clear ? Visibility.Visible : Visibility.Collapsed;

            Thickness basePadding = (Thickness)this.FindResource("Pad.Field");
            const double buttonWidth = 26d;
            double rightExtra = (trailing ? buttonWidth : 0d) + (clear ? buttonWidth : 0d);

            this.InnerTextBox.Padding = new Thickness(
                    basePadding.Left, basePadding.Top, basePadding.Right + rightExtra, basePadding.Bottom);
            this.RequiredDot.Margin = new Thickness(0, 0, 10 + rightExtra, 0);
            this.ClearButton.Margin = trailing
                    ? new Thickness(0, 0, buttonWidth, 0)
                    : new Thickness(0, 0, 0, 0);
        }

        // 지우기 버튼 — 값을 비우고 에디터에 포커스를 돌려준다 (연속 입력 동선).
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            this.InnerTextBox.Clear();
            this.InnerTextBox.Focus();
        }

        // ===== 입력 마스크 =====

        // 마스크 전환 — 엔진을 만들고(해제하면 버리고), 기존 텍스트를 마스크에
        // 통과시켜 표시 문자열로 정규화한다. 마스크가 걸린 동안에는 IME 조합·
        // 자동완성·Undo가 마스크 상태와 어긋날 수 있어 전부 끈다.
        private static void OnMaskChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernTextBoxControl control = (ModernTextBoxControl)d;
            string mask = (string)e.NewValue;

            if (string.IsNullOrEmpty(mask))
            {
                control.maskProvider = null;
                System.Windows.Input.InputMethod.SetIsInputMethodEnabled(control.InnerTextBox, true);
                control.InnerTextBox.IsUndoEnabled = true;
                return;
            }

            control.maskProvider = new System.ComponentModel.MaskedTextProvider(mask);
            System.Windows.Input.InputMethod.SetIsInputMethodEnabled(control.InnerTextBox, false);
            control.InnerTextBox.IsUndoEnabled = false;
            control.CloseSuggestionsCore();
            control.ResyncMaskFromText(control.InnerTextBox.Text);
        }

        // 임의의 텍스트(프로그램 할당·초기값)를 마스크에 순서대로 통과시켜
        // 표시 문자열을 다시 만든다. 마스크에 안 맞는 문자와 프롬프트 문자는
        // 그 문자만 버린다 — 리터럴 포함/미포함 어느 형태의 값도 수용한다
        // ("20260729"와 "2026-07-29" 둘 다 마스크 "0000-00-00"에 안착).
        private void ResyncMaskFromText(string text)
        {
            this.maskProvider.Clear();

            if (!string.IsNullOrEmpty(text))
            {
                foreach (char current in text)
                {
                    if (current == this.maskProvider.PromptChar)
                    {
                        continue;
                    }

                    this.maskProvider.Add(current);
                }
            }

            // 캐럿은 다음 입력이 들어갈 자리(마지막 배정 다음의 첫 편집 자리)로.
            int next = this.maskProvider.LastAssignedPosition + 1;
            int caret = next >= this.maskProvider.Length
                    ? this.maskProvider.Length
                    : this.maskProvider.FindEditPositionFrom(next, true);
            this.SetMaskedText(caret < 0 ? this.maskProvider.Length : caret);
        }

        // 마스크 엔진의 표시 문자열을 에디터에 쓰고 캐럿을 옮긴다.
        // applyingMaskText 플래그로 TextChanged의 재정규화 재진입을 막는다.
        private void SetMaskedText(int caret)
        {
            this.applyingMaskText = true;

            try
            {
                this.InnerTextBox.Text = this.maskProvider.ToDisplayString();
            }
            finally
            {
                this.applyingMaskText = false;
            }

            this.InnerTextBox.CaretIndex = Math.Max(0, Math.Min(caret, this.InnerTextBox.Text.Length));
        }

        // 타이핑/붙여넣기 문자열을 캐럿 위치부터 덮어쓰기(Replace)로 넣는다.
        // 마스크에 안 맞는 문자(붙여넣은 리터럴 구분자 등)는 건너뛴다 —
        // "12/34"를 "00/00" 마스크에 붙여넣어도 그대로 안착한다.
        private void InsertMaskedText(string input)
        {
            int caret = this.InnerTextBox.CaretIndex;

            if (this.InnerTextBox.SelectionLength > 0)
            {
                caret = this.InnerTextBox.SelectionStart;
                this.RemoveMaskedRange(caret, caret + this.InnerTextBox.SelectionLength - 1);
            }

            foreach (char current in input)
            {
                int position = this.maskProvider.FindEditPositionFrom(
                        Math.Min(caret, this.maskProvider.Length - 1), true);

                if (position < 0 || caret >= this.maskProvider.Length)
                {
                    break;
                }

                int testPosition;
                System.ComponentModel.MaskedTextResultHint hint;

                if (this.maskProvider.Replace(current, position, out testPosition, out hint))
                {
                    caret = testPosition + 1;
                }
            }

            this.SetMaskedText(caret);
        }

        // 지정 구간의 배정 문자를 지운다 (뒤 문자는 마스크가 허용하는 만큼 왼쪽으로 당겨진다).
        private void RemoveMaskedRange(int start, int end)
        {
            int safeStart = Math.Max(0, start);
            int safeEnd = Math.Min(end, this.maskProvider.Length - 1);

            if (safeStart > safeEnd)
            {
                return;
            }

            int testPosition;
            System.ComponentModel.MaskedTextResultHint hint;
            this.maskProvider.RemoveAt(safeStart, safeEnd, out testPosition, out hint);
        }

        // Backspace: 선택이 있으면 선택 구간을, 없으면 캐럿 바로 앞 편집 자리를 지운다.
        private void HandleMaskedBackspace()
        {
            if (this.InnerTextBox.SelectionLength > 0)
            {
                int start = this.InnerTextBox.SelectionStart;
                this.RemoveMaskedRange(start, start + this.InnerTextBox.SelectionLength - 1);
                this.SetMaskedText(start);
                return;
            }

            int caret = this.InnerTextBox.CaretIndex;

            if (caret <= 0)
            {
                return;
            }

            int position = this.maskProvider.FindEditPositionFrom(caret - 1, false);

            if (position < 0)
            {
                return;
            }

            this.RemoveMaskedRange(position, position);
            this.SetMaskedText(position);
        }

        // Delete: 선택이 있으면 선택 구간을, 없으면 캐럿 이후 첫 편집 자리를 지운다.
        private void HandleMaskedDelete()
        {
            if (this.InnerTextBox.SelectionLength > 0)
            {
                int start = this.InnerTextBox.SelectionStart;
                this.RemoveMaskedRange(start, start + this.InnerTextBox.SelectionLength - 1);
                this.SetMaskedText(start);
                return;
            }

            int caret = this.InnerTextBox.CaretIndex;

            if (caret >= this.maskProvider.Length)
            {
                return;
            }

            int position = this.maskProvider.FindEditPositionFrom(caret, true);

            if (position < 0)
            {
                return;
            }

            this.RemoveMaskedRange(position, position);
            this.SetMaskedText(caret);
        }

        // 마스크 모드의 문자 입력 — 기본 TextBox 편집을 막고 마스크 엔진으로 넣는다.
        private void InnerTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (this.maskProvider == null || this.IsReadOnly)
            {
                return;
            }

            e.Handled = true;
            this.InsertMaskedText(e.Text);
        }

        // 붙여넣기: 기본 명령을 취소하고 마스크 엔진으로 넣는다 (마스크 없으면 무동작).
        private void OnMaskedPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (this.maskProvider == null)
            {
                return;
            }

            e.CancelCommand();

            if (this.IsReadOnly)
            {
                return;
            }

            string pasted = e.DataObject.GetData(DataFormats.UnicodeText, true) as string;

            if (!string.IsNullOrEmpty(pasted))
            {
                this.InsertMaskedText(pasted);
            }
        }

        // 잘라내기: 기본 동작은 마스크를 우회해 표시 문자열을 직접 찢는다 —
        // 선택 텍스트를 클립보드에 복사한 뒤 마스크 규칙으로 지우는 동작으로 바꾼다.
        private void OnEditorPreviewExecuted(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (this.maskProvider == null || e.Command != ApplicationCommands.Cut)
            {
                return;
            }

            e.Handled = true;

            if (this.IsReadOnly || this.InnerTextBox.SelectionLength == 0)
            {
                return;
            }

            try
            {
                Clipboard.SetText(this.InnerTextBox.SelectedText);
            }
            catch (Exception)
            {
                // 클립보드 점유 실패는 무시 — 잘라내기 대신 지우기만 된다.
            }

            int start = this.InnerTextBox.SelectionStart;
            this.RemoveMaskedRange(start, start + this.InnerTextBox.SelectionLength - 1);
            this.SetMaskedText(start);
        }

        private void TrailingButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.ButtonClick != null)
            {
                this.ButtonClick(this, EventArgs.Empty);
            }
        }

        // 후보 목록이 입력 중에 비동기로 도착하는 시나리오(서버 typeahead) 지원:
        // 에디터에 포커스가 있는 동안 소스를 재할당하면 추천을 즉시 다시 필터링한다.
        // 포커스가 없으면(코드로 텍스트/소스만 바꾼 경우) 드롭다운을 열지 않고,
        // 음소거 래치가 걸려 있으면(추천 확정·명시적 닫기 직후) 사용자가 다시
        // 타이핑하기 전까지 열지 않는다 — 확정이 유발한 재조회 응답 차단.
        private static void OnAutoCompleteItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernTextBoxControl control = (ModernTextBoxControl)d;

            if (control.suggestionsMuted)
            {
                return;
            }

            if (control.InnerTextBox.IsKeyboardFocused)
            {
                control.RefreshSuggestions();
            }
        }

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernTextBoxControl control = (ModernTextBoxControl)d;

            if (control.TextChanged != null)
            {
                control.TextChanged(control, EventArgs.Empty);
            }
        }

        // IME 조합 중에도 발생한다 — placeholder와 추천 드롭다운을 구동하여
        // 둘 다 첫 자음 입력부터 반응하게 한다.
        private void InnerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 마스크 모드: 사용자 입력은 Preview 단계에서 이미 마스크를 통과했다.
            // 여기로 오는 "우리가 쓰지 않은" 변경(프로그램의 Text 할당 등)만
            // 마스크에 다시 통과시켜 표시 문자열로 정규화한다. 표시 문자열은
            // 리터럴/프롬프트로 항상 차 있으므로 placeholder는 숨긴다.
            if (this.maskProvider != null)
            {
                if (!this.applyingMaskText)
                {
                    this.ResyncMaskFromText(this.InnerTextBox.Text);
                }

                this.PlaceholderText.Visibility = Visibility.Collapsed;
                return;
            }

            // 허용 문자 제한: 텍스트가 정리되면 중첩 TextChanged가 나머지를
            // 처리하므로 이번 호출은 여기서 끝낸다.
            if (this.EnforceAllowedCharacters())
            {
                return;
            }

            this.PlaceholderText.Visibility = string.IsNullOrEmpty(this.InnerTextBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;

            if (this.suppressSuggestions)
            {
                this.suppressSuggestions = false;
                return;
            }

            // 확정/닫기가 아닌 텍스트 변경(타이핑·프로그램 할당)은 음소거를 푼다.
            this.suggestionsMuted = false;
            this.RefreshSuggestions();
        }

        // AllowedCharacters에 없는 문자를 제거하고 캐럿 위치를 보정한다.
        // 타이핑·붙여넣기·IME 확정 텍스트가 모두 TextChanged를 지나므로
        // 이 한 곳에서 전부 걸러진다. 변경이 있었으면 true.
        private bool EnforceAllowedCharacters()
        {
            string allowed = this.AllowedCharacters;
            string text = this.InnerTextBox.Text;

            if (string.IsNullOrEmpty(allowed) || text.Length == 0)
            {
                return false;
            }

            StringBuilder filtered = new StringBuilder(text.Length);
            int caret = this.InnerTextBox.CaretIndex;
            int newCaret = caret;

            for (int index = 0; index < text.Length; index++)
            {
                char current = text[index];

                if (allowed.IndexOf(current) >= 0)
                {
                    filtered.Append(current);
                }
                else if (index < caret)
                {
                    newCaret = newCaret - 1;
                }
            }

            if (filtered.Length == text.Length)
            {
                return false;
            }

            this.InnerTextBox.Text = filtered.ToString();
            this.InnerTextBox.CaretIndex = Math.Max(0, newCaret);
            return true;
        }

        private void RefreshSuggestions()
        {
            string typed = this.InnerTextBox.Text;
            IEnumerable candidates = this.AutoCompleteItemsSource;

            this.suggestionItems.Clear();

            // 여러 줄/마스크 모드에서는 자동완성을 쓰지 않는다
            // (비고 입력에 추천은 소음, 마스크 표시 문자열엔 매칭이 무의미).
            if (candidates == null || string.IsNullOrEmpty(typed) || this.IsReadOnly
                    || this.Multiline || this.maskProvider != null)
            {
                this.CloseSuggestionsCore();
                return;
            }

            // None(서버 신뢰): 디바운스 typeahead의 후보는 서버가 이미 키워드로
            // 걸렀고, 후보 텍스트에 보이지 않는 컬럼(의뢰번호 등)으로 매칭했을 수
            // 있으므로 재대조하지 않는다 — 재대조하면 그런 후보가 전부 걸러져
            // 드롭다운이 열리지 않는다 (현장 보고 2026-08-25).
            bool matchAgainstTyped = this.SuggestionFilterMode == SuggestionFilterMode.Contains;

            foreach (object candidate in candidates)
            {
                if (candidate == null)
                {
                    continue;
                }

                string candidateText = candidate.ToString();

                // 한국어 인식 매칭: 자음 자모는 음절의 초성과 매칭되고(초성 검색)
                // IME 조합 중간 음절도 계속 매칭된다. 입력과 동일한 후보는
                // 모드와 무관하게 제외한다(이미 입력된 값의 추천은 소음).
                if ((!matchAgainstTyped || HangulTextMatcher.Contains(candidateText, typed)) &&
                    !string.Equals(candidateText, typed, StringComparison.Ordinal))
                {
                    this.suggestionItems.Add(candidateText);

                    if (this.suggestionItems.Count >= MaxSuggestionCount)
                    {
                        break;
                    }
                }
            }

            if (this.suggestionItems.Count > 0)
            {
                this.SuggestionList.SelectedIndex = -1;
                this.SuggestionPopup.IsOpen = true;
            }
            else
            {
                this.CloseSuggestionsCore();
            }
        }

        /// <summary>
        /// 추천 드롭다운을 닫고 음소거를 건다. 검색 실행처럼 "지금부터는 추천이
        /// 무의미한" 시점에 폼이 호출한다 — 뒤늦게 도착한 typeahead 응답이
        /// 목록을 다시 열어 화면에 남는 것을 막는다. 사용자가 다시 타이핑하면
        /// 음소거가 풀리고 추천이 재개된다.
        /// </summary>
        public void CloseSuggestions()
        {
            this.suggestionsMuted = true;
            this.CloseSuggestionsCore();
        }

        // 드롭다운만 닫는다(음소거 없음) — 필터 결과 없음·포커스 이탈·모드 전환
        // 같은 내부 상태 변화용. 여기서 음소거를 걸면 타이핑 중 후보가 아직
        // 도착하지 않은 순간의 닫기가 이후 도착한 응답까지 막아 typeahead가
        // 통째로 죽는다.
        private void CloseSuggestionsCore()
        {
            this.SuggestionPopup.IsOpen = false;
            this.SuggestionList.SelectedIndex = -1;
        }

        // 드롭다운을 다시 열지 않고 추천 항목을 에디터에 적용한다.
        // CloseSuggestions(음소거 포함)를 쓰므로, 확정이 유발한 typeahead
        // 재조회가 확정 값의 접두사 후보(자식 랏 등)를 들고 돌아와도
        // 사용자가 다시 타이핑하기 전까지 목록이 도로 열리지 않는다.
        private void CommitSuggestion(string value)
        {
            this.suppressSuggestions = true;
            this.InnerTextBox.Text = value;
            this.InnerTextBox.CaretIndex = value.Length;
            this.CloseSuggestions();
        }

        // 방향키로 열린 드롭다운을 탐색하는 동안 포커스는 에디터에 남는다.
        private void InnerTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 마스크 모드의 편집 키 — 기본 TextBox 편집을 막고 마스크 규칙으로
            // 지운다. 스페이스는 WPF가 TextInput으로 올리지 않으므로 여기서
            // 문자 입력처럼 처리한다 (ResetOnSpace: 그 자리 값 지움).
            if (this.maskProvider != null && !this.IsReadOnly)
            {
                if (e.Key == Key.Back)
                {
                    this.HandleMaskedBackspace();
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.Delete)
                {
                    this.HandleMaskedDelete();
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.Space)
                {
                    this.InsertMaskedText(" ");
                    e.Handled = true;
                    return;
                }
            }

            if (!this.SuggestionPopup.IsOpen)
            {
                return;
            }

            if (e.Key == Key.Down)
            {
                if (this.SuggestionList.SelectedIndex < this.suggestionItems.Count - 1)
                {
                    this.SuggestionList.SelectedIndex = this.SuggestionList.SelectedIndex + 1;
                    this.SuggestionList.ScrollIntoView(this.SuggestionList.SelectedItem);
                }

                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                if (this.SuggestionList.SelectedIndex >= 0)
                {
                    this.SuggestionList.SelectedIndex = this.SuggestionList.SelectedIndex - 1;

                    if (this.SuggestionList.SelectedItem != null)
                    {
                        this.SuggestionList.ScrollIntoView(this.SuggestionList.SelectedItem);
                    }
                }

                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                string selected = this.SuggestionList.SelectedItem as string;

                if (selected != null)
                {
                    // 먼저 확정한 뒤, KeyDown이 완성된 텍스트로 EnterPressed를
                    // 발생시키게 한다(검색창 동작).
                    this.CommitSuggestion(selected);
                }
                else
                {
                    this.CloseSuggestions();
                }
            }
            else if (e.Key == Key.Escape)
            {
                this.CloseSuggestions();
                e.Handled = true;
            }
        }

        private void SuggestionItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item = (ListBoxItem)sender;
            string value = item.DataContext as string;

            e.Handled = true;

            if (value == null)
            {
                return;
            }

            // 먼저 확정해 텍스트를 후보로 바꾼 뒤 알린다 — 핸들러가 Text 를 읽으면 후보다.
            this.CommitSuggestion(value);

            if (this.SuggestionClicked != null)
            {
                this.SuggestionClicked(this, EventArgs.Empty);
            }
        }

        private void InnerTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            this.CloseSuggestionsCore();
        }

        private void InnerTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // 여러 줄 모드에서 Enter는 줄바꿈이다 — "엔터로 검색"을 올리지 않는다.
            if (e.Key == Key.Enter && !this.Multiline && this.EnterPressed != null)
            {
                this.EnterPressed(this, EventArgs.Empty);
            }
        }
    }
}
