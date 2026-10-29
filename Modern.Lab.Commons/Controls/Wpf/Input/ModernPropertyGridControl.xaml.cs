using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

using Modern.Lab.Controls.Wpf.Selection;

namespace Modern.Lab.Controls.Wpf.Input
{
    /// <summary>
    /// 기준정보 편집기의 WPF 본체 — 입력칸(라벨 + 입력 컨트롤) 전부를 섬 하나 안의 Grid 에 놓는다(2026-10-29).
    ///
    /// 전에는 상위 Common 의 <c>ModernPropertyGrid</c> 가 컬럼마다 WinForms 래퍼(ElementHost)를 하나씩 만들어 40컬럼이면 섬 40개가
    /// 생겼고, 생성 · 크기 변경 · Enabled 토글 비용이 컬럼 수에 비례했다(design-notes 7절 "ElementHost 는 개수가 성능이다").
    /// 이제 호스트는 컬럼 명세(<see cref="PropertyGridFieldSpec"/>) 목록을 <see cref="SetFields"/> 로 넘기고, 값·잠금·필수·콤보 항목은
    /// 컬럼 이름으로 묻고 바꾼다. 키·필수·신규·되돌리기 판정은 호스트 몫이다 — 이 컨트롤은 서버도 엔티티도 모른다.
    ///
    /// 배치 규칙은 전과 같다 — 패널 폭을 <see cref="MinColumnWidth"/> 로 나눈 만큼 열, 한 줄 입력 행은 전부 같은 높이
    /// (<see cref="EditorHeight"/> 와 가장 큰 라벨 높이 중 큰 쪽, 2026-10-18), 여러 줄 텍스트는 한 행을 통째로 차지한다.
    /// 열 수가 바뀌면 같은 요소의 Grid 행·열만 다시 정한다(떼었다 붙이지 않는다).
    /// </summary>
    public partial class ModernPropertyGridControl : UserControl
    {
        private const double ColumnGap = 16d;
        private const double LabelGap = 8d;

        /// <summary>라벨 열 너비(px).</summary>
        public static readonly DependencyProperty LabelWidthProperty =
            DependencyProperty.Register(
                "LabelWidth",
                typeof(double),
                typeof(ModernPropertyGridControl),
                new PropertyMetadata(140d, OnLayoutSettingChanged));

        /// <summary>한 열(라벨+입력칸)이 차지할 최소 폭 — 패널 폭을 이 값으로 나눈 만큼 열이 생긴다.</summary>
        public static readonly DependencyProperty MinColumnWidthProperty =
            DependencyProperty.Register(
                "MinColumnWidth",
                typeof(double),
                typeof(ModernPropertyGridControl),
                new PropertyMetadata(420d, OnLayoutSettingChanged));

        /// <summary>한 줄 입력칸 높이(px).</summary>
        public static readonly DependencyProperty EditorHeightProperty =
            DependencyProperty.Register(
                "EditorHeight",
                typeof(double),
                typeof(ModernPropertyGridControl),
                new PropertyMetadata(28d, OnLayoutSettingChanged));

        /// <summary>여러 줄 입력칸 높이(px).</summary>
        public static readonly DependencyProperty MultilineHeightProperty =
            DependencyProperty.Register(
                "MultilineHeight",
                typeof(double),
                typeof(ModernPropertyGridControl),
                new PropertyMetadata(72d, OnLayoutSettingChanged));

        /// <summary>행과 행 사이 간격(px).</summary>
        public static readonly DependencyProperty RowGapProperty =
            DependencyProperty.Register(
                "RowGap",
                typeof(double),
                typeof(ModernPropertyGridControl),
                new PropertyMetadata(4d, OnLayoutSettingChanged));

        private readonly List<FieldRow> rows = new List<FieldRow>();
        private int layoutColumns = 1;

        /// <summary>어느 입력칸이든 값이 바뀌면 발생한다.</summary>
        public event EventHandler ValueChanged;

        /// <summary>
        /// 콤보를 펼칠 때 항목을 다시 받아 오는 조회 — 컬럼 이름 → 항목(<see cref="IEnumerable"/>, 보통 <c>DataView</c>).
        /// null 을 돌려주면 지금 목록을 그대로 둔다. 호스트가 같은 전문을 쓰는 다른 콤보까지 <see cref="BindComboItems"/> 로
        /// 함께 바꿨으면 null 을 돌려줘도 된다. 지정하지 않으면 펼칠 때 조회하지 않는다.
        /// </summary>
        public Func<string, IEnumerable> DropDownRefresh { get; set; }

        public ModernPropertyGridControl()
        {
            this.InitializeComponent();
            this.SizeChanged += this.OnSizeChanged;
        }

        public double LabelWidth
        {
            get { return (double)this.GetValue(LabelWidthProperty); }
            set { this.SetValue(LabelWidthProperty, value); }
        }

        public double MinColumnWidth
        {
            get { return (double)this.GetValue(MinColumnWidthProperty); }
            set { this.SetValue(MinColumnWidthProperty, value); }
        }

        public double EditorHeight
        {
            get { return (double)this.GetValue(EditorHeightProperty); }
            set { this.SetValue(EditorHeightProperty, value); }
        }

        public double MultilineHeight
        {
            get { return (double)this.GetValue(MultilineHeightProperty); }
            set { this.SetValue(MultilineHeightProperty, value); }
        }

        public double RowGap
        {
            get { return (double)this.GetValue(RowGapProperty); }
            set { this.SetValue(RowGapProperty, value); }
        }

        /// <summary>지금 놓인 입력칸 수.</summary>
        public int FieldCount
        {
            get { return this.rows.Count; }
        }

        /// <summary>지금 열 수 — 자체 검사가 읽는다.</summary>
        public int ColumnCount
        {
            get { return this.layoutColumns; }
        }

        /// <summary>
        /// 입력칸을 명세 순서대로 새로 놓는다 — 전에 있던 입력칸과 값은 버린다(값 보존은 호스트가 읽어 두었다가 다시 쓴다).
        /// null 이나 빈 목록이면 비운다.
        /// </summary>
        public void SetFields(IList<PropertyGridFieldSpec> specs)
        {
            foreach (FieldRow old in this.rows)
            {
                old.Detach(this.OnEditorChanged, this.OnComboDropDownOpened);
            }

            this.rows.Clear();
            this.Rows.Children.Clear();

            if (specs != null)
            {
                foreach (PropertyGridFieldSpec spec in specs)
                {
                    if (spec == null || spec.Column.Length == 0)
                    {
                        continue;
                    }

                    FieldRow row = new FieldRow(spec);
                    row.Attach(this.OnEditorChanged, this.OnComboDropDownOpened);
                    this.rows.Add(row);
                    this.Rows.Children.Add(row.Label);
                    this.Rows.Children.Add(row.Editor);
                }
            }

            this.Arrange(this.ComputeColumns());
        }

        /// <summary>컬럼의 명세 — 없으면 null.</summary>
        public PropertyGridFieldSpec FieldOf(string column)
        {
            FieldRow row = this.Find(column);
            return row == null ? null : row.Spec;
        }

        /// <summary>컬럼의 라벨 요소 — 자체 검사와 배치 확인용. 없으면 null.</summary>
        public FrameworkElement LabelElementOf(string column)
        {
            FieldRow row = this.Find(column);
            return row == null ? null : row.Label;
        }

        /// <summary>컬럼의 입력 요소 — 자체 검사와 배치 확인용. 없으면 null.</summary>
        public FrameworkElement EditorElementOf(string column)
        {
            FieldRow row = this.Find(column);
            return row == null ? null : row.Editor;
        }

        /// <summary>컬럼이 놓인 Grid 행의 실제 높이 — 없으면 0.</summary>
        public double RowHeightOf(string column)
        {
            FieldRow row = this.Find(column);

            if (row == null)
            {
                return 0d;
            }

            int index = Grid.GetRow(row.Label);
            return index < this.Rows.RowDefinitions.Count ? this.Rows.RowDefinitions[index].ActualHeight : 0d;
        }

        /// <summary>컬럼의 필수 표시가 켜져 있는가(별표 · 입력칸 표시).</summary>
        public bool RequiredMarkOf(string column)
        {
            FieldRow row = this.Find(column);
            return row != null && row.RequiredMark;
        }

        /// <summary>컬럼의 편집 값 — 없는 컬럼이면 빈 문자열.</summary>
        public string GetValue(string column)
        {
            FieldRow row = this.Find(column);
            return row == null ? string.Empty : row.Read();
        }

        /// <summary>컬럼에 값을 쓴다 — 없는 컬럼이면 무시한다.</summary>
        public void SetValue(string column, string value)
        {
            FieldRow row = this.Find(column);

            if (row != null)
            {
                row.Write(value);
            }
        }

        /// <summary>컬럼을 잠그거나 푼다 — 텍스트는 읽기 전용, 그 밖의 입력칸은 비활성이 된다.</summary>
        public void SetReadOnly(string column, bool readOnly)
        {
            FieldRow row = this.Find(column);

            if (row != null)
            {
                row.SetReadOnly(readOnly);
            }
        }

        /// <summary>컬럼의 필수 표시(라벨 별표 · 입력칸 표시)를 켜거나 끈다. 잠긴 칸에는 표시하지 않는 판정은 호스트가 한다.</summary>
        public void SetRequired(string column, bool required)
        {
            FieldRow row = this.Find(column);

            if (row != null)
            {
                row.SetRequired(required);
            }
        }

        /// <summary>콤보 컬럼에 항목을 싣는다(명세의 ValueMember · DisplayMember 로 읽는다). 콤보가 아니면 무시한다. 선택 값은 호스트가 다시 쓴다.</summary>
        public void BindComboItems(string column, IEnumerable items)
        {
            FieldRow row = this.Find(column);

            if (row != null)
            {
                row.BindCombo(items);
            }
        }

        /// <summary>컬럼의 입력칸에 포커스를 준다 — 없거나 잠겨 있으면 false.</summary>
        public bool FocusField(string column)
        {
            FieldRow row = this.Find(column);
            return row != null && row.Focus();
        }

        private FieldRow Find(string column)
        {
            if (string.IsNullOrEmpty(column))
            {
                return null;
            }

            foreach (FieldRow row in this.rows)
            {
                if (string.Equals(row.Spec.Column, column, StringComparison.OrdinalIgnoreCase))
                {
                    return row;
                }
            }

            return null;
        }

        private static void OnLayoutSettingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernPropertyGridControl control = (ModernPropertyGridControl)d;

            if (control.Rows != null)
            {
                control.Arrange(control.ComputeColumns());
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            int columns = this.ComputeColumns();

            if (columns != this.layoutColumns && this.rows.Count > 0)
            {
                this.Arrange(columns);
            }
        }

        private int ComputeColumns()
        {
            double usable = this.ActualWidth - this.Scroller.Padding.Left - this.Scroller.Padding.Right;
            double minimum = Math.Max(1d, this.MinColumnWidth);
            return Math.Max(1, (int)Math.Floor(usable / minimum));
        }

        // 행·열 배치 — 같은 요소의 Grid 행·열만 다시 정한다. 열마다 [라벨 | 입력칸 | 간격] 세 칸이고 마지막 열에는 간격이 없다.
        // 한 줄 입력 행은 전부 같은 높이(입력칸 높이와 가장 큰 라벨 높이 중 큰 쪽 + 행 간격), 여러 줄 칸은 제 높이로 한 행을 차지한다.
        private void Arrange(int columns)
        {
            this.layoutColumns = Math.Max(1, columns);

            this.Rows.ColumnDefinitions.Clear();
            this.Rows.RowDefinitions.Clear();

            for (int i = 0; i < this.layoutColumns; i = i + 1)
            {
                this.Rows.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Max(1d, this.LabelWidth)) });
                this.Rows.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1d, GridUnitType.Star) });

                if (i < this.layoutColumns - 1)
                {
                    this.Rows.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(ColumnGap) });
                }
            }

            double uniformHeight = this.EditorHeight;

            foreach (FieldRow row in this.rows)
            {
                if (row.Spec.Kind != PropertyEditorKind.MultilineText)
                {
                    row.Label.Measure(new Size(Math.Max(1d, this.LabelWidth - LabelGap), double.PositiveInfinity));
                    uniformHeight = Math.Max(uniformHeight, row.Label.DesiredSize.Height);
                }
            }

            int gridRow = 0;
            int slot = 0;
            double gap = this.RowGap;

            foreach (FieldRow row in this.rows)
            {
                bool fullRow = row.Spec.Kind == PropertyEditorKind.MultilineText;
                double height = fullRow ? this.MultilineHeight : uniformHeight;

                if (fullRow && slot > 0)
                {
                    gridRow = gridRow + 1;
                    slot = 0;
                }

                if (slot == 0)
                {
                    this.Rows.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto, MinHeight = height + gap });
                }
                else if (this.Rows.RowDefinitions[gridRow].MinHeight < height + gap)
                {
                    this.Rows.RowDefinitions[gridRow].MinHeight = height + gap;
                }

                int labelColumn = slot * 3;

                row.Label.Margin = new Thickness(0d, gap / 2d, LabelGap, gap / 2d);
                row.Label.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetRow(row.Label, gridRow);
                Grid.SetColumn(row.Label, labelColumn);
                Grid.SetColumnSpan(row.Label, 1);

                row.Editor.Margin = new Thickness(0d, gap / 2d, 0d, gap / 2d);
                row.Editor.Height = fullRow ? this.MultilineHeight : this.EditorHeight;
                row.Editor.VerticalAlignment = fullRow ? VerticalAlignment.Top : VerticalAlignment.Center;
                row.Editor.HorizontalAlignment = HorizontalAlignment.Stretch;
                Grid.SetRow(row.Editor, gridRow);
                Grid.SetColumn(row.Editor, labelColumn + 1);
                Grid.SetColumnSpan(row.Editor, fullRow ? Math.Max(1, this.layoutColumns * 3 - 2) : 1);

                if (fullRow)
                {
                    gridRow = gridRow + 1;
                    slot = 0;
                }
                else
                {
                    slot = slot + 1;

                    if (slot == this.layoutColumns)
                    {
                        slot = 0;
                        gridRow = gridRow + 1;
                    }
                }
            }
        }

        private void OnEditorChanged(object sender, EventArgs e)
        {
            EventHandler handler = this.ValueChanged;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void OnComboDropDownOpened(object sender, EventArgs e)
        {
            Func<string, IEnumerable> refresh = this.DropDownRefresh;

            if (refresh == null)
            {
                return;
            }

            foreach (FieldRow row in this.rows)
            {
                if (object.ReferenceEquals(row.Editor, sender))
                {
                    IEnumerable items = refresh(row.Spec.Column);

                    if (items != null)
                    {
                        string kept = row.Read();
                        row.BindCombo(items);
                        row.Write(kept);
                    }

                    return;
                }
            }
        }

        // 입력칸 하나 — 라벨(위·본·아래 글씨 · 필수 별표)과 입력 컨트롤, 그리고 종류별 값 읽기·쓰기.
        private sealed class FieldRow
        {
            private readonly TextBlock topText;
            private readonly TextBlock mainText;
            private readonly Run mainRun;
            private readonly Run requiredRun;
            private readonly TextBlock bottomText;

            // 콤보에 실은 원래 값. 항목이 아직 없거나 값이 항목 코드에 없어 선택이 비어 있을 때 Read 가 이 값을 그대로 내놓는다 —
            // 서버가 준 값은 항목 도착 순서와 무관하게 보존된다(상태 계약 product-master.md 표 ②).
            private string comboValue = string.Empty;

            internal FieldRow(PropertyGridFieldSpec spec)
            {
                this.Spec = spec;

                StackPanel label = new StackPanel();
                label.Orientation = Orientation.Vertical;

                this.topText = SmallLine();
                this.topText.Text = spec.TopLabel ?? string.Empty;
                this.topText.Visibility = this.topText.Text.Length == 0 ? Visibility.Collapsed : Visibility.Visible;

                this.mainText = new TextBlock();
                this.mainText.SetResourceReference(FrameworkElement.StyleProperty, "Text.Label");
                this.mainText.TextTrimming = TextTrimming.CharacterEllipsis;
                this.mainRun = new Run(spec.LabelText ?? string.Empty);
                this.requiredRun = new Run(" *");
                this.requiredRun.SetResourceReference(TextElement.ForegroundProperty, "Brush.ErrorText");
                this.mainText.Inlines.Add(this.mainRun);

                this.bottomText = SmallLine();
                this.bottomText.Text = spec.BottomLabel ?? string.Empty;
                this.bottomText.Visibility = this.bottomText.Text.Length == 0 ? Visibility.Collapsed : Visibility.Visible;

                label.Children.Add(this.topText);
                label.Children.Add(this.mainText);
                label.Children.Add(this.bottomText);
                this.Label = label;

                this.Editor = CreateEditor(spec);
            }

            internal PropertyGridFieldSpec Spec { get; private set; }

            internal FrameworkElement Label { get; private set; }

            internal FrameworkElement Editor { get; private set; }

            internal bool ReadOnly { get; private set; }

            internal bool RequiredMark { get; private set; }

            private static TextBlock SmallLine()
            {
                TextBlock line = new TextBlock();
                line.SetResourceReference(FrameworkElement.StyleProperty, "Text.Label");
                line.SetResourceReference(TextBlock.FontSizeProperty, "Font.Size.SubLabel");
                line.FontWeight = FontWeights.Normal;
                line.TextTrimming = TextTrimming.CharacterEllipsis;
                return line;
            }

            private static FrameworkElement CreateEditor(PropertyGridFieldSpec spec)
            {
                switch (spec.Kind)
                {
                    case PropertyEditorKind.Combo:
                        ModernComboBoxControl combo = new ModernComboBoxControl();
                        combo.IsEditable = false;
                        combo.SelectedValuePath = spec.ValueMember ?? string.Empty;
                        combo.DisplayMemberPath = spec.DisplayMember ?? string.Empty;
                        return combo;

                    case PropertyEditorKind.Toggle:
                        ModernToggleSwitchControl toggle = new ModernToggleSwitchControl();
                        toggle.Text = "N";
                        return toggle;

                    case PropertyEditorKind.Date:
                        ModernDatePickerControl date = new ModernDatePickerControl();
                        date.Placeholder = "yyyy-MM-dd";
                        return date;

                    case PropertyEditorKind.Number:
                        ModernNumericTextBoxControl number = new ModernNumericTextBoxControl();
                        number.DecimalPlaces = Math.Max(0, spec.DecimalPlaces);
                        return number;

                    case PropertyEditorKind.MultilineText:
                        ModernTextBoxControl multiline = new ModernTextBoxControl();
                        multiline.Multiline = true;
                        return multiline;

                    default:
                        return new ModernTextBoxControl();
                }
            }

            internal void Attach(EventHandler changed, EventHandler dropDownOpened)
            {
                ModernTextBoxControl text = this.Editor as ModernTextBoxControl;
                ModernComboBoxControl combo = this.Editor as ModernComboBoxControl;
                ModernToggleSwitchControl toggle = this.Editor as ModernToggleSwitchControl;
                ModernDatePickerControl date = this.Editor as ModernDatePickerControl;
                ModernNumericTextBoxControl number = this.Editor as ModernNumericTextBoxControl;

                if (text != null)
                {
                    text.TextChanged += changed;
                }
                else if (combo != null)
                {
                    combo.SelectionChanged += changed;
                    combo.DropDownOpened += dropDownOpened;
                }
                else if (toggle != null)
                {
                    toggle.CheckedChanged += this.OnToggleChanged;
                    toggle.CheckedChanged += changed;
                }
                else if (date != null)
                {
                    date.ValueChanged += changed;
                }
                else if (number != null)
                {
                    number.ValueChanged += changed;
                }
            }

            internal void Detach(EventHandler changed, EventHandler dropDownOpened)
            {
                ModernTextBoxControl text = this.Editor as ModernTextBoxControl;
                ModernComboBoxControl combo = this.Editor as ModernComboBoxControl;
                ModernToggleSwitchControl toggle = this.Editor as ModernToggleSwitchControl;
                ModernDatePickerControl date = this.Editor as ModernDatePickerControl;
                ModernNumericTextBoxControl number = this.Editor as ModernNumericTextBoxControl;

                if (text != null)
                {
                    text.TextChanged -= changed;
                }
                else if (combo != null)
                {
                    combo.SelectionChanged -= changed;
                    combo.DropDownOpened -= dropDownOpened;
                }
                else if (toggle != null)
                {
                    toggle.CheckedChanged -= this.OnToggleChanged;
                    toggle.CheckedChanged -= changed;
                }
                else if (date != null)
                {
                    date.ValueChanged -= changed;
                }
                else if (number != null)
                {
                    number.ValueChanged -= changed;
                }
            }

            private void OnToggleChanged(object sender, EventArgs e)
            {
                ModernToggleSwitchControl toggle = (ModernToggleSwitchControl)this.Editor;
                toggle.Text = toggle.IsChecked ? "Y" : "N";
            }

            internal void SetRequired(bool required)
            {
                this.RequiredMark = required;

                if (required && !this.mainText.Inlines.Contains(this.requiredRun))
                {
                    this.mainText.Inlines.Add(this.requiredRun);
                }
                else if (!required && this.mainText.Inlines.Contains(this.requiredRun))
                {
                    this.mainText.Inlines.Remove(this.requiredRun);
                }

                ModernTextBoxControl text = this.Editor as ModernTextBoxControl;
                ModernComboBoxControl combo = this.Editor as ModernComboBoxControl;
                ModernDatePickerControl date = this.Editor as ModernDatePickerControl;
                ModernNumericTextBoxControl number = this.Editor as ModernNumericTextBoxControl;

                if (text != null)
                {
                    text.Required = required;
                }
                else if (combo != null)
                {
                    combo.Required = required;
                }
                else if (date != null)
                {
                    date.Required = required;
                }
                else if (number != null)
                {
                    number.Required = required;
                }
            }

            internal void SetReadOnly(bool readOnly)
            {
                this.ReadOnly = readOnly;
                ModernTextBoxControl text = this.Editor as ModernTextBoxControl;

                if (text != null)
                {
                    text.IsReadOnly = readOnly;
                }
                else
                {
                    this.Editor.IsEnabled = !readOnly;
                }
            }

            internal void BindCombo(IEnumerable items)
            {
                ModernComboBoxControl combo = this.Editor as ModernComboBoxControl;

                if (combo != null)
                {
                    combo.ItemsSource = items;
                }
            }

            internal bool Focus()
            {
                if (this.ReadOnly || !this.Editor.IsEnabled)
                {
                    return false;
                }

                ModernTextBoxControl text = this.Editor as ModernTextBoxControl;

                if (text != null)
                {
                    text.FocusEditor();
                    return true;
                }

                return this.Editor.Focus();
            }

            internal string Read()
            {
                switch (this.Spec.Kind)
                {
                    case PropertyEditorKind.Combo:
                        ModernComboBoxControl combo = (ModernComboBoxControl)this.Editor;

                        if (combo.SelectedIndex < 0)
                        {
                            return this.comboValue;
                        }

                        object selected = combo.SelectedValue;
                        return selected == null ? this.comboValue : Convert.ToString(selected, CultureInfo.InvariantCulture);

                    case PropertyEditorKind.Toggle:
                        return ((ModernToggleSwitchControl)this.Editor).IsChecked ? "Y" : "N";

                    case PropertyEditorKind.Date:
                        DateTime? date = ((ModernDatePickerControl)this.Editor).SelectedDate;
                        return date.HasValue ? date.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : string.Empty;

                    case PropertyEditorKind.Number:
                        decimal? number = ((ModernNumericTextBoxControl)this.Editor).Value;
                        return number.HasValue ? number.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;

                    default:
                        return ((ModernTextBoxControl)this.Editor).Text ?? string.Empty;
                }
            }

            internal void Write(string value)
            {
                string text = value ?? string.Empty;

                switch (this.Spec.Kind)
                {
                    case PropertyEditorKind.Combo:
                        // 빈 값은 선택 없음이다 — 첫 항목으로 바꾸지 않는다. 코드에 없는 값도 선택 없음으로 보이되
                        // comboValue 로 남아 저장 전문에 원래 값 그대로 실린다.
                        ModernComboBoxControl combo = (ModernComboBoxControl)this.Editor;
                        this.comboValue = text;
                        combo.SelectedIndex = -1;

                        if (text.Length > 0)
                        {
                            combo.SelectedValue = text;
                        }

                        break;

                    case PropertyEditorKind.Toggle:
                        ((ModernToggleSwitchControl)this.Editor).IsChecked =
                                string.Equals(text.Trim(), "Y", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(text.Trim(), "TRUE", StringComparison.OrdinalIgnoreCase);
                        break;

                    case PropertyEditorKind.Date:
                        ((ModernDatePickerControl)this.Editor).SelectedDate = ParseDate(text);
                        break;

                    case PropertyEditorKind.Number:
                        decimal parsed;
                        ((ModernNumericTextBoxControl)this.Editor).Value =
                                decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out parsed)
                                        ? parsed : (decimal?)null;
                        break;

                    default:
                        ((ModernTextBoxControl)this.Editor).Text = text;
                        break;
                }
            }

            private static DateTime? ParseDate(string text)
            {
                string trimmed = (text ?? string.Empty).Trim();

                if (trimmed.Length == 0)
                {
                    return null;
                }

                DateTime value;
                string[] formats = new string[] { "yyyy-MM-dd", "yyyyMMdd", "yyyy-MM-dd HH:mm:ss", "yyyy/MM/dd" };

                if (DateTime.TryParseExact(trimmed, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out value))
                {
                    return value;
                }

                if (DateTime.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.None, out value))
                {
                    return value;
                }

                return null;
            }
        }
    }
}
