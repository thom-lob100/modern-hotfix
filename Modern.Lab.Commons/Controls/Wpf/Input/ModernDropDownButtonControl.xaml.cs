using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Modern.Lab.Controls.Wpf.Common;

namespace Modern.Lab.Controls.Wpf.Input
{
    /// <summary>
    /// 드롭다운 버튼 (버튼 + 메뉴): 클릭하면 항목 메뉴가 열리고,
    /// 항목을 클릭하면 ItemClicked가 발생한다.
    /// - Text: 버튼 캡션 (셰브런은 자동으로 붙는다)
    /// - ItemsSource / DisplayMemberPath / ValueMemberPath: 메뉴 항목 (코드/명칭 계약)
    /// </summary>
    public partial class ModernDropDownButtonControl : UserControl
    {
        /// <summary>버튼 캡션.</summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(ModernDropDownButtonControl),
                new PropertyMetadata(string.Empty));

        /// <summary>메뉴 항목 목록.</summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
                typeof(IEnumerable),
                typeof(ModernDropDownButtonControl),
                new PropertyMetadata(null, OnDataShapeChanged));

        /// <summary>메뉴 표시 텍스트로 쓸 컬럼/속성 이름.</summary>
        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register(
                "DisplayMemberPath",
                typeof(string),
                typeof(ModernDropDownButtonControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>항목 값으로 쓸 컬럼/속성 이름.</summary>
        public static readonly DependencyProperty ValueMemberPathProperty =
            DependencyProperty.Register(
                "ValueMemberPath",
                typeof(string),
                typeof(ModernDropDownButtonControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>항목 실행 가능 여부로 쓸 컬럼/속성 이름 (bool 또는
        /// "Y"/"true"/"1"). 비우면 모든 항목이 활성이다. 비활성 항목은
        /// 회색으로 표시되고 클릭되지 않는다.</summary>
        public static readonly DependencyProperty EnabledMemberPathProperty =
            DependencyProperty.Register(
                "EnabledMemberPath",
                typeof(string),
                typeof(ModernDropDownButtonControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>항목 왼쪽 글리프로 쓸 컬럼/속성 이름(Segoe MDL2 Assets 문자). 비우면 글리프 칸이 없고,
        /// 지정하면 값이 빈 항목도 칸을 비워 두어 글자 시작이 맞는다 — 컨텍스트 메뉴의 글리프 여백과 같은 뜻.</summary>
        public static readonly DependencyProperty GlyphMemberPathProperty =
            DependencyProperty.Register(
                "GlyphMemberPath",
                typeof(string),
                typeof(ModernDropDownButtonControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>강조색(토큰 Brush.ErrorText)으로 그릴 끝 단어의 컬럼/속성 이름 — 값이 표시 텍스트의 끝 단어와 같을 때만
        /// 그 단어를 빨갛게 그린다(컨텍스트 메뉴 ModernMenuItem.AccentText 와 같은 규칙). 비우면 전부 본문색이다.</summary>
        public static readonly DependencyProperty AccentTextMemberPathProperty =
            DependencyProperty.Register(
                "AccentTextMemberPath",
                typeof(string),
                typeof(ModernDropDownButtonControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>캡션 앞에 표시되는 아이콘 글리프(Segoe MDL2 Assets) — ModernButton 의 IconGlyph 와 같은 규칙. 비어 있으면 숨긴다.</summary>
        public static readonly DependencyProperty IconGlyphProperty =
            DependencyProperty.Register(
                "IconGlyph",
                typeof(string),
                typeof(ModernDropDownButtonControl),
                new PropertyMetadata(string.Empty));

        /// <summary>버튼 시각 종류 — ModernButton과 같은 문법(Secondary 기본,
        /// Execute = Success 초록 채움 등).</summary>
        public static readonly DependencyProperty KindProperty =
            DependencyProperty.Register(
                "Kind",
                typeof(ButtonKind),
                typeof(ModernDropDownButtonControl),
                new PropertyMetadata(ButtonKind.Secondary));

        /// <summary>캡션 위에 붙는 아주 작은 상단 라벨(비어 있으면 숨김) — ModernButton 의 TopLabel 과 같은 규칙.
        /// 값이 있으면 버튼 높이를 내용에 맞춰 늘린다(고정 높이 해제).</summary>
        public static readonly DependencyProperty TopLabelProperty =
            DependencyProperty.Register(
                "TopLabel",
                typeof(string),
                typeof(ModernDropDownButtonControl),
                new PropertyMetadata(string.Empty, OnSubLabelChanged));

        /// <summary>캡션 아래에 붙는 아주 작은 하단 라벨(비어 있으면 숨김). 상단 라벨과 같은 규칙이고 함께 쓸 수 있다.</summary>
        public static readonly DependencyProperty BottomLabelProperty =
            DependencyProperty.Register(
                "BottomLabel",
                typeof(string),
                typeof(ModernDropDownButtonControl),
                new PropertyMetadata(string.Empty, OnSubLabelChanged));

        /// <summary>상단 라벨의 가로 정렬(기본 Center). Left·Right 는 캡션 덩어리의 왼쪽·오른쪽 끝에 맞춘다.</summary>
        public static readonly DependencyProperty TopLabelAlignmentProperty =
            DependencyProperty.Register(
                "TopLabelAlignment",
                typeof(HorizontalAlignment),
                typeof(ModernDropDownButtonControl),
                new PropertyMetadata(HorizontalAlignment.Center));

        /// <summary>하단 라벨의 가로 정렬(기본 Center).</summary>
        public static readonly DependencyProperty BottomLabelAlignmentProperty =
            DependencyProperty.Register(
                "BottomLabelAlignment",
                typeof(HorizontalAlignment),
                typeof(ModernDropDownButtonControl),
                new PropertyMetadata(HorizontalAlignment.Center));

        /// <summary>메뉴 팝업이 현재 열려 있는지 여부.</summary>
        public static readonly DependencyProperty IsDropDownOpenProperty =
            DependencyProperty.Register(
                "IsDropDownOpen",
                typeof(bool),
                typeof(ModernDropDownButtonControl),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        private readonly ObservableCollection<DropDownButtonItem> menuItems;

        /// <summary>메뉴 항목이 클릭될 때 발생한다.</summary>
        public event EventHandler<DropDownItemClickedEventArgs> ItemClicked;

        /// <summary>비활성 항목을 눌렀을 때 발생한다 — 항목은 실행되지 않고 메뉴만 닫힌다.
        /// 왜 닫혀 있는지 안내하는 용도다(규칙 안내 창, 2026-10-14). 구분선은 올리지 않는다.</summary>
        public event EventHandler<DropDownItemClickedEventArgs> DisabledItemClicked;

        public ModernDropDownButtonControl()
        {
            this.menuItems = new ObservableCollection<DropDownButtonItem>();
            this.InitializeComponent();
            this.MenuItemsControl.ItemsSource = this.menuItems;
            this.PopupBorder.PreviewMouseLeftButtonUp += this.OnPopupPreviewMouseLeftButtonUp;
        }

        // 요구 높이 계약 — 자동 배치(무한 높이)에서는 설계 높이(32), 유한
        // 슬롯에서는 min(설계, 가용). 위·아래 라벨이 있으면 내용 높이 그대로(ModernButton 과 같다).
        // 근거: Common/FieldSlotHeight.cs.
        protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
        {
            if (this.HasSubLabels)
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

        /// <summary>버튼 캡션.</summary>
        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        /// <summary>메뉴 항목 목록.</summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)this.GetValue(ItemsSourceProperty); }
            set { this.SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>메뉴 표시 텍스트로 쓸 컬럼/속성 이름.</summary>
        public string DisplayMemberPath
        {
            get { return (string)this.GetValue(DisplayMemberPathProperty); }
            set { this.SetValue(DisplayMemberPathProperty, value); }
        }

        /// <summary>항목 값으로 쓸 컬럼/속성 이름.</summary>
        public string ValueMemberPath
        {
            get { return (string)this.GetValue(ValueMemberPathProperty); }
            set { this.SetValue(ValueMemberPathProperty, value); }
        }

        /// <summary>항목 실행 가능 여부로 쓸 컬럼/속성 이름 (비우면 전부 활성).</summary>
        public string EnabledMemberPath
        {
            get { return (string)this.GetValue(EnabledMemberPathProperty); }
            set { this.SetValue(EnabledMemberPathProperty, value); }
        }

        /// <summary>항목 왼쪽 글리프로 쓸 컬럼/속성 이름 (비우면 글리프 칸 없음).</summary>
        public string GlyphMemberPath
        {
            get { return (string)this.GetValue(GlyphMemberPathProperty); }
            set { this.SetValue(GlyphMemberPathProperty, value); }
        }

        /// <summary>강조색으로 그릴 끝 단어의 컬럼/속성 이름 (비우면 전부 본문색).</summary>
        public string AccentTextMemberPath
        {
            get { return (string)this.GetValue(AccentTextMemberPathProperty); }
            set { this.SetValue(AccentTextMemberPathProperty, value); }
        }

        /// <summary>캡션 앞의 아이콘 글리프 (비어 있으면 숨김).</summary>
        public string IconGlyph
        {
            get { return (string)this.GetValue(IconGlyphProperty); }
            set { this.SetValue(IconGlyphProperty, value); }
        }

        /// <summary>버튼 시각 종류 (Secondary 기본, Execute = 초록 채움).</summary>
        public ButtonKind Kind
        {
            get { return (ButtonKind)this.GetValue(KindProperty); }
            set { this.SetValue(KindProperty, value); }
        }

        /// <summary>캡션 위에 붙는 아주 작은 상단 라벨(비어 있으면 숨김).</summary>
        public string TopLabel
        {
            get { return (string)this.GetValue(TopLabelProperty); }
            set { this.SetValue(TopLabelProperty, value); }
        }

        /// <summary>캡션 아래에 붙는 아주 작은 하단 라벨(비어 있으면 숨김).</summary>
        public string BottomLabel
        {
            get { return (string)this.GetValue(BottomLabelProperty); }
            set { this.SetValue(BottomLabelProperty, value); }
        }

        /// <summary>상단 라벨의 가로 정렬(기본 Center).</summary>
        public HorizontalAlignment TopLabelAlignment
        {
            get { return (HorizontalAlignment)this.GetValue(TopLabelAlignmentProperty); }
            set { this.SetValue(TopLabelAlignmentProperty, value); }
        }

        /// <summary>하단 라벨의 가로 정렬(기본 Center).</summary>
        public HorizontalAlignment BottomLabelAlignment
        {
            get { return (HorizontalAlignment)this.GetValue(BottomLabelAlignmentProperty); }
            set { this.SetValue(BottomLabelAlignmentProperty, value); }
        }

        /// <summary>메뉴 팝업이 현재 열려 있는지 여부.</summary>
        public bool IsDropDownOpen
        {
            get { return (bool)this.GetValue(IsDropDownOpenProperty); }
            private set { this.SetValue(IsDropDownOpenProperty, value); }
        }

        // 위·아래 작은 라벨 중 하나라도 있으면 두 줄 이상이라 고정 높이를 풀고 내용 높이로 잰다.
        private bool HasSubLabels
        {
            get { return !string.IsNullOrEmpty(this.TopLabel) || !string.IsNullOrEmpty(this.BottomLabel); }
        }

        private static void OnSubLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernDropDownButtonControl)d).ApplyHeight();
        }

        // 토글 버튼 높이의 단일 결정 지점 — 라벨이 있으면 내용 높이로 늘리고, 없으면 토큰 기본(Size.ControlHeight).
        // 고정 Height 가 아니라 MaxHeight 를 쓰는 이유는 ModernButtonControl.ApplyHeight 와 같다.
        private void ApplyHeight()
        {
            this.DropToggle.MaxHeight = this.HasSubLabels
                ? double.PositiveInfinity
                : (double)this.FindResource("Size.ControlHeight");
        }

        private static void OnDataShapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernDropDownButtonControl)d).RebuildItems();
        }

        // 소스 행을 메뉴 항목으로 재구성한다. null/빈 소스와 존재하지 않는
        // 컬럼은 빈 메뉴/빈 텍스트로 처리하며 예외를 던지지 않는다(계약 규칙 3).
        private void RebuildItems()
        {
            this.menuItems.Clear();

            IEnumerable source = this.ItemsSource;

            if (source == null)
            {
                return;
            }

            bool showGlyph = !string.IsNullOrEmpty(this.GlyphMemberPath);
            bool accented = !string.IsNullOrEmpty(this.AccentTextMemberPath);

            foreach (object row in source)
            {
                object value = string.IsNullOrEmpty(this.ValueMemberPath)
                    ? row
                    : MemberPathReader.Read(row, this.ValueMemberPath);

                string displayText = MemberPathReader.ReadDisplayText(row, this.DisplayMemberPath);
                string glyph = showGlyph ? MemberPathReader.ReadDisplayText(row, this.GlyphMemberPath) : string.Empty;
                string accentText = accented ? MemberPathReader.ReadDisplayText(row, this.AccentTextMemberPath) : string.Empty;
                this.menuItems.Add(new DropDownButtonItem(
                        value, displayText, this.ReadEnabled(row), glyph, accentText, showGlyph));
            }
        }

        // 항목 실행 가능 여부를 관용적으로 읽는다 — bool 또는 "Y"/"true"/"1"
        // 문자열을 허용하고, 경로가 비어 있으면 항상 활성이다.
        private bool ReadEnabled(object row)
        {
            if (string.IsNullOrEmpty(this.EnabledMemberPath))
            {
                return true;
            }

            object value = MemberPathReader.Read(row, this.EnabledMemberPath);

            if (value is bool)
            {
                return (bool)value;
            }

            string text = value == null ? string.Empty : value.ToString().Trim();
            return string.Equals(text, "Y", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(text, "true", StringComparison.OrdinalIgnoreCase)
                    || text == "1";
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DropDownButtonItem item = ((Button)sender).DataContext as DropDownButtonItem;

            this.DropToggle.IsChecked = false;

            if (item != null && this.ItemClicked != null)
            {
                this.ItemClicked(this, new DropDownItemClickedEventArgs(item.Value, item.DisplayText));
            }
        }

        // 비활성 항목은 WPF 입력 판정에서 빠져 클릭이 팝업 테두리에 떨어진다 — 시각 트리 판정으로
        // 그 자리의 항목을 찾아 비활성이면 DisabledItemClicked 를 올리고 메뉴를 닫는다.
        private void OnPopupPreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DropDownButtonItem item = this.ItemAt(e.GetPosition(this.PopupBorder));

            if (item == null || item.IsEnabled || item.IsSeparator)
            {
                return;
            }

            this.DropToggle.IsChecked = false;

            if (this.DisabledItemClicked != null)
            {
                this.DisabledItemClicked(this, new DropDownItemClickedEventArgs(item.Value, item.DisplayText));
            }
        }

        private DropDownButtonItem ItemAt(Point position)
        {
            System.Windows.Media.HitTestResult hit = System.Windows.Media.VisualTreeHelper.HitTest(this.PopupBorder, position);
            DependencyObject current = hit == null ? null : hit.VisualHit;

            while (current != null && !ReferenceEquals(current, this.PopupBorder))
            {
                FrameworkElement element = current as FrameworkElement;
                DropDownButtonItem item = element == null ? null : element.DataContext as DropDownButtonItem;

                if (item != null)
                {
                    return item;
                }

                current = System.Windows.Media.VisualTreeHelper.GetParent(current);
            }

            return null;
        }
    }
}
