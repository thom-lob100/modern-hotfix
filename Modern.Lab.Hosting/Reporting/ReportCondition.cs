using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Display;
using Modern.Lab.WinForms.Controls.Input;
using Modern.Lab.WinForms.Controls.Selection;

namespace Modern.Lab.Hosting.Reporting
{
    public abstract class ReportCondition
    {
        public const string OptionValueColumn = "VALUE";
        public const string OptionLabelColumn = "LABEL";

        private const string DateFormat = "yyyy-MM-dd";
        private const string CheckedValue = "Y";

        private readonly string caption;
        private bool required;
        private bool primary;
        private ModernLabel label;

        private ReportCondition(string caption)
        {
            this.caption = caption ?? string.Empty;
        }

        public string Caption
        {
            get { return this.caption; }
        }

        internal bool IsRequired
        {
            get { return this.required; }
        }

        internal bool IsPrimary
        {
            get { return this.required || this.primary; }
        }

        internal ModernLabel Label
        {
            get { return this.label; }
        }

        internal abstract Control Editor { get; }

        internal abstract int EditorWidth { get; }

        internal abstract bool IsComplete { get; }

        internal abstract bool HasValue { get; }

        internal abstract string ValueText { get; }

        internal abstract void ApplyDefault();

        internal abstract void Clear();

        internal abstract void AppendValues(List<object> pairs);

        internal abstract void BuildEditor(ReportFormBase form);

        internal void FocusEditor()
        {
            if (this.Editor != null && this.Editor.CanFocus)
            {
                this.Editor.Focus();
            }
        }

        internal void Build(ReportFormBase form)
        {
            this.label = new ModernLabel();
            this.label.Kind = LabelKind.Label;
            this.label.Text = this.caption;
            this.BuildEditor(form);
        }

        private static string DateText(DateTime? date)
        {
            return date.HasValue ? date.Value.ToString(DateFormat, CultureInfo.InvariantCulture) : string.Empty;
        }

        public abstract class Fluent<T> : ReportCondition where T : Fluent<T>
        {
            protected Fluent(string caption)
                    : base(caption)
            {
            }

            public T Required()
            {
                this.required = true;
                return (T)this;
            }

            public T Primary()
            {
                this.primary = true;
                return (T)this;
            }
        }

        public sealed class DateRangeCondition : Fluent<DateRangeCondition>
        {
            private readonly Field fromField;
            private readonly Field toField;
            private int defaultMonths;
            private int defaultDays;
            private ModernDateRangePicker picker;

            internal DateRangeCondition(string caption, Field fromField, Field toField)
                    : base(caption)
            {
                this.fromField = fromField;
                this.toField = toField;
            }

            public DateRangeCondition LastMonths(int months)
            {
                this.defaultMonths = months;
                this.defaultDays = 0;
                return this;
            }

            public DateRangeCondition LastDays(int days)
            {
                this.defaultDays = days;
                this.defaultMonths = 0;
                return this;
            }

            internal override Control Editor
            {
                get { return this.picker; }
            }

            internal override int EditorWidth
            {
                get { return 300; }
            }

            internal override bool IsComplete
            {
                get { return !this.IsRequired || (this.picker.FromDate.HasValue && this.picker.ToDate.HasValue); }
            }

            internal override bool HasValue
            {
                get { return this.picker.FromDate.HasValue || this.picker.ToDate.HasValue; }
            }

            internal override string ValueText
            {
                get { return (DateText(this.picker.FromDate) + " ~ " + DateText(this.picker.ToDate)).Trim(); }
            }

            internal override void Clear()
            {
                this.picker.SetRange(null, null);
            }

            internal override void ApplyDefault()
            {
                DateTime today = DateTime.Today;

                if (this.defaultMonths > 0)
                {
                    this.picker.SetRange(today.AddMonths(-this.defaultMonths), today);
                }
                else if (this.defaultDays > 0)
                {
                    this.picker.SetRange(today.AddDays(-this.defaultDays), today);
                }
                else
                {
                    this.picker.SetRange(null, null);
                }
            }

            internal override void AppendValues(List<object> pairs)
            {
                pairs.Add(this.fromField);
                pairs.Add(DateText(this.picker.FromDate));
                pairs.Add(this.toField);
                pairs.Add(DateText(this.picker.ToDate));
            }

            internal override void BuildEditor(ReportFormBase form)
            {
                this.picker = new ModernDateRangePicker();
                this.picker.Required = this.IsRequired;
                this.picker.ShowPresets = true;
                this.picker.RangeChanged += delegate { form.OnConditionChanged(); };
            }
        }

        public sealed class TextCondition : Fluent<TextCondition>
        {
            private readonly Field field;
            private string placeholder = string.Empty;
            private bool upper;
            private int width = 300;
            private ModernTextBox textBox;

            internal TextCondition(string caption, Field field)
                    : base(caption)
            {
                this.field = field;
            }

            public TextCondition Placeholder(string text)
            {
                this.placeholder = text ?? string.Empty;
                return this;
            }

            public TextCondition Upper()
            {
                this.upper = true;
                return this;
            }

            public TextCondition Width(int logicalWidth)
            {
                this.width = logicalWidth;
                return this;
            }

            internal override Control Editor
            {
                get { return this.textBox; }
            }

            internal override int EditorWidth
            {
                get { return this.width; }
            }

            internal override bool IsComplete
            {
                get { return !this.IsRequired || this.textBox.Text.Trim().Length > 0; }
            }

            internal override bool HasValue
            {
                get { return this.textBox.Text.Trim().Length > 0; }
            }

            internal override string ValueText
            {
                get { return this.textBox.Text.Trim(); }
            }

            internal override void Clear()
            {
                this.textBox.Text = string.Empty;
            }

            internal override void ApplyDefault()
            {
                this.textBox.Text = string.Empty;
            }

            internal override void AppendValues(List<object> pairs)
            {
                pairs.Add(this.field);
                pairs.Add(this.textBox.Text.Trim());
            }

            internal override void BuildEditor(ReportFormBase form)
            {
                this.textBox = new ModernTextBox();
                this.textBox.PlaceholderText = this.placeholder;

                if (this.upper)
                {
                    this.textBox.CharacterCasing = CharacterCasing.Upper;
                }

                this.textBox.EnterPressed += delegate { form.OnConditionEnter(); };
                this.textBox.TextChanged += delegate { form.OnConditionChanged(); };
            }
        }

        public sealed class CodesCondition : Fluent<CodesCondition>
        {
            private const int MaxChipLabels = 2;

            private readonly Field field;
            private readonly DataTable options;
            private string placeholder = string.Empty;
            private ModernCheckComboBox combo;

            internal CodesCondition(string caption, Field field, DataTable options)
                    : base(caption)
            {
                this.field = field;
                this.options = options;
            }

            public CodesCondition Placeholder(string text)
            {
                this.placeholder = text ?? string.Empty;
                return this;
            }

            internal override Control Editor
            {
                get { return this.combo; }
            }

            internal override int EditorWidth
            {
                get { return 300; }
            }

            internal override bool IsComplete
            {
                get { return !this.IsRequired || this.HasValue; }
            }

            internal override bool HasValue
            {
                get
                {
                    object[] values = this.combo.CheckedValues;
                    return values != null && values.Length > 0;
                }
            }

            internal override string ValueText
            {
                get
                {
                    object[] values = this.combo.CheckedValues;
                    int count = values == null ? 0 : values.Length;
                    return count > MaxChipLabels ? count.ToString("N0", CultureInfo.InvariantCulture) + " selected" : this.combo.Text;
                }
            }

            internal override void Clear()
            {
                this.combo.UncheckAll();
            }

            internal override void ApplyDefault()
            {
                this.combo.UncheckAll();
            }

            internal override void AppendValues(List<object> pairs)
            {
                pairs.Add(this.field);
                pairs.Add(Joined(this.combo.CheckedValues));
            }

            internal override void BuildEditor(ReportFormBase form)
            {
                this.combo = new ModernCheckComboBox();
                this.combo.PlaceholderText = this.placeholder;
                this.combo.DisplayMember = OptionLabelColumn;
                this.combo.ValueMember = OptionValueColumn;
                this.combo.DataSource = this.options;
                this.combo.CheckedChanged += delegate { form.OnConditionChanged(); };
            }

            private static string Joined(object[] values)
            {
                if (values == null || values.Length == 0)
                {
                    return string.Empty;
                }

                List<string> codes = new List<string>();

                foreach (object value in values)
                {
                    string code = (Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty).Trim();

                    if (code.Length > 0)
                    {
                        codes.Add(code);
                    }
                }

                return string.Join(",", codes.ToArray());
            }
        }

        public sealed class CheckCondition : Fluent<CheckCondition>
        {
            private readonly Field field;
            private ModernCheckBox checkBox;

            internal CheckCondition(string caption, Field field)
                    : base(caption)
            {
                this.field = field;
            }

            internal override Control Editor
            {
                get { return this.checkBox; }
            }

            internal override int EditorWidth
            {
                get { return 32; }
            }

            internal override bool IsComplete
            {
                get { return !this.IsRequired || this.checkBox.Checked; }
            }

            internal override bool HasValue
            {
                get { return this.checkBox.Checked; }
            }

            internal override string ValueText
            {
                get { return string.Empty; }
            }

            internal override void Clear()
            {
                this.checkBox.Checked = false;
            }

            internal override void ApplyDefault()
            {
                this.checkBox.Checked = false;
            }

            internal override void AppendValues(List<object> pairs)
            {
                pairs.Add(this.field);
                pairs.Add(this.checkBox.Checked ? CheckedValue : string.Empty);
            }

            internal override void BuildEditor(ReportFormBase form)
            {
                this.checkBox = new ModernCheckBox();
                this.checkBox.Text = string.Empty;
                this.checkBox.CheckedChanged += delegate { form.OnConditionChanged(); };
            }
        }
    }
}
