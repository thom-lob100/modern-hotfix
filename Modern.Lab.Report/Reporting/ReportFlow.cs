using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Data;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Report.Reporting
{
    public abstract class ReportFlow
    {
        internal const string StepLabelColumn = "LABEL";
        internal const string StepStateColumn = "STATE";

        private const string StepCompleted = "Completed";
        private const string StepCurrent = "Current";
        private const string StepPending = "Pending";
        private const string StepFailed = "Failed";

        private const string SemanticSuffix = "_SEM";
        private const string CellTimeFormat = "MM-dd HH:mm";
        private const string ErrorSemantic = "error";
        private const string SuccessSemantic = "success";
        private const string InfoSemantic = "info";
        private const string NeutralSemantic = "neutral";
        private const string DoneMark = "\u2713 ";

        private readonly string label;

        private ReportFlow(string label)
        {
            this.label = label ?? string.Empty;
        }

        public string Label
        {
            get { return this.label; }
        }

        internal abstract int StepCount { get; }

        internal abstract DataTable Indicator(DataRow row);

        internal abstract void Decorate(GridColumns columns);

        internal virtual void Prepare(DataTable rows)
        {
        }

        internal virtual string FailValue
        {
            get { return string.Empty; }
        }

        internal virtual bool IsFailed(DataRow row)
        {
            return false;
        }

        internal virtual IEnumerable<string> HiddenColumns
        {
            get { return new string[0]; }
        }

        private static DataTable StepTable()
        {
            DataTable steps = new DataTable();
            steps.Columns.Add(StepLabelColumn, typeof(string));
            steps.Columns.Add(StepStateColumn, typeof(string));
            return steps;
        }

        private static string CellOf(DataRow row, string column)
        {
            return row == null || string.IsNullOrEmpty(column) ? string.Empty : TableHelper.CellText(row, column).Trim();
        }

        private static string ShortTime(string text)
        {
            DateTime parsed;

            return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed)
                    ? parsed.ToString(CellTimeFormat, CultureInfo.InvariantCulture)
                    : text;
        }

        public sealed class StateFlow : ReportFlow
        {
            private readonly Field stateField;
            private readonly string[] codes;
            private string cancelledCode = string.Empty;
            private string cancelledText = string.Empty;

            internal StateFlow(string label, Field stateField, string[] codes)
                    : base(label)
            {
                this.stateField = stateField;
                this.codes = codes ?? new string[0];
            }

            public Field StateField
            {
                get { return this.stateField; }
            }

            public string StateColumn
            {
                get { return this.Label.ToUpperInvariant() + "_STATE"; }
            }

            private string ColorColumn
            {
                get { return this.StateColumn + "_COLOR"; }
            }

            internal override IEnumerable<string> HiddenColumns
            {
                get { return new string[] { this.ColorColumn, this.stateField.Name }; }
            }

            internal override void Prepare(DataTable rows)
            {
                if (rows == null)
                {
                    return;
                }

                TableHelper.EnsureColumn(rows, this.StateColumn, typeof(string));
                TableHelper.EnsureColumn(rows, this.ColorColumn, typeof(string));

                foreach (DataRow row in rows.Rows)
                {
                    string code = this.stateField.Text(row);
                    row[this.StateColumn] = this.StateText(code);
                    row[this.ColorColumn] = this.IsCancelled(code)
                            ? System.Drawing.ColorTranslator.ToHtml(Modern.Lab.Theming.ModernTheme.NeutralBackground)
                            : (this.IndexOf(code) < 0 ? string.Empty : Modern.Lab.Theming.Palette.FromValue(code));
                }
            }

            public StateFlow Cancelled(string code, string text)
            {
                this.cancelledCode = (code ?? string.Empty).Trim();
                this.cancelledText = text ?? string.Empty;
                return this;
            }

            public DataTable Options()
            {
                DataTable options = new DataTable();
                options.Columns.Add(ReportCondition.OptionValueColumn, typeof(string));
                options.Columns.Add(ReportCondition.OptionLabelColumn, typeof(string));

                foreach (string code in this.AllCodes())
                {
                    options.Rows.Add(code, this.StateText(code));
                }

                return options;
            }

            internal override int StepCount
            {
                get { return Math.Max(1, this.codes.Length); }
            }

            internal string StateText(string code)
            {
                string value = (code ?? string.Empty).Trim();

                if (this.IsCancelled(value))
                {
                    return this.cancelledText;
                }

                int index = this.IndexOf(value);
                return index < 0 ? value : GridCaptionCatalog.Humanize(this.codes[index]);
            }

            internal bool IsCancelled(string code)
            {
                return this.cancelledCode.Length > 0
                        && string.Equals(this.cancelledCode, (code ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase);
            }

            internal override DataTable Indicator(DataRow row)
            {
                string code = row == null ? string.Empty : this.stateField.Text(row);
                bool cancelled = this.IsCancelled(code);
                int index = cancelled ? -1 : this.IndexOf(code);
                int last = this.codes.Length - 1;
                DataTable steps = StepTable();

                for (int step = 0; step <= last; step++)
                {
                    steps.Rows.Add(GridCaptionCatalog.Humanize(this.codes[step]), StepState(step, index, last, cancelled));
                }

                return steps;
            }

            internal override void Decorate(GridColumns columns)
            {
                columns.Column(this.StateColumn).Caption(this.Label).TopLabel(string.Empty).Badge().BadgeWidth(this.AllTexts());
                ReportDefinition.Each(columns, new[] { this.StateColumn }, column => column.BadgeColorMember = this.ColorColumn);
            }

            private string[] AllTexts()
            {
                string[] codes = this.AllCodes();
                string[] texts = new string[codes.Length];

                for (int index = 0; index < codes.Length; index++)
                {
                    texts[index] = this.StateText(codes[index]);
                }

                return texts;
            }

            private string[] AllCodes()
            {
                List<string> all = new List<string>(this.codes);

                if (this.cancelledCode.Length > 0 && !all.Contains(this.cancelledCode))
                {
                    all.Add(this.cancelledCode);
                }

                return all.ToArray();
            }

            private int IndexOf(string code)
            {
                string value = (code ?? string.Empty).Trim();

                if (value.Length == 0)
                {
                    return -1;
                }

                for (int index = 0; index < this.codes.Length; index++)
                {
                    if (string.Equals(this.codes[index], value, StringComparison.OrdinalIgnoreCase))
                    {
                        return index;
                    }
                }

                return -1;
            }

            private static string StepState(int step, int index, int last, bool cancelled)
            {
                if (cancelled)
                {
                    return StepPending;
                }

                if (index < 0)
                {
                    return StepPending;
                }

                if (index == last || step < index)
                {
                    return StepCompleted;
                }

                return step == index ? StepCurrent : StepPending;
            }
        }

        public sealed class EventFlow : ReportFlow
        {
            private readonly List<FlowStep> steps = new List<FlowStep>();

            internal EventFlow(string label)
                    : base(label)
            {
            }

            public string StageColumn
            {
                get { return this.Prefix + "STAGE"; }
            }

            public string[] Columns
            {
                get
                {
                    string[] names = new string[this.steps.Count];

                    for (int index = 0; index < this.steps.Count; index++)
                    {
                        names[index] = this.steps[index].Column;
                    }

                    return names;
                }
            }

            private string Prefix
            {
                get { return this.Label.ToUpperInvariant() + "_"; }
            }

            private string StageSemanticColumn
            {
                get { return this.StageColumn + SemanticSuffix; }
            }

            public EventFlow Step(string stepLabel, string column)
            {
                this.steps.Add(new FlowStep(stepLabel, this.Prefix).Event(column, string.Empty, string.Empty));
                return this;
            }

            public EventFlow Job(string stepLabel, string prepColumn, string startColumn, string endColumn)
            {
                this.steps.Add(new FlowStep(stepLabel, this.Prefix)
                        .Event(endColumn, "Job End", "End")
                        .Event(startColumn, "Job Start", "Start")
                        .Event(prepColumn, "Job Prep", "Prep"));
                return this;
            }

            public EventFlow Fail(string column, string failValue)
            {
                this.LastStep().Fail(column, failValue);
                return this;
            }

            public EventFlow Runs(string column)
            {
                this.LastStep().Runs(column);
                return this;
            }

            internal override int StepCount
            {
                get { return Math.Max(1, this.steps.Count); }
            }

            internal override string FailValue
            {
                get
                {
                    foreach (FlowStep step in this.steps)
                    {
                        if (step.FailValue.Length > 0)
                        {
                            return step.FailValue;
                        }
                    }

                    return string.Empty;
                }
            }

            internal override IEnumerable<string> HiddenColumns
            {
                get
                {
                    List<string> names = new List<string>();
                    names.Add(this.StageSemanticColumn);

                    foreach (FlowStep step in this.steps)
                    {
                        names.Add(step.SemanticColumn);
                    }

                    return names;
                }
            }

            internal override bool IsFailed(DataRow row)
            {
                foreach (FlowStep step in this.steps)
                {
                    if (step.IsFailed(row))
                    {
                        return true;
                    }
                }

                return false;
            }

            internal override void Prepare(DataTable rows)
            {
                if (rows == null)
                {
                    return;
                }

                TableHelper.EnsureColumn(rows, this.StageColumn, typeof(string));
                TableHelper.EnsureColumn(rows, this.StageSemanticColumn, typeof(string));

                foreach (FlowStep step in this.steps)
                {
                    TableHelper.EnsureColumn(rows, step.Column, typeof(string));
                    TableHelper.EnsureColumn(rows, step.SemanticColumn, typeof(string));
                }

                foreach (DataRow row in rows.Rows)
                {
                    row[this.StageColumn] = this.StageText(row);
                    row[this.StageSemanticColumn] = this.StageSemantic(row);

                    foreach (FlowStep step in this.steps)
                    {
                        row[step.Column] = step.CellText(row);
                        row[step.SemanticColumn] = step.CellSemantic(row);
                    }
                }
            }

            internal override DataTable Indicator(DataRow row)
            {
                DataTable table = StepTable();
                int current = row == null ? -1 : this.LastReached(row);
                int last = this.steps.Count - 1;
                bool failed = current >= 0 && this.steps[current].IsFailed(row);
                bool finished = !failed && current == last && current >= 0 && this.steps[last].IsDone(row);

                for (int index = 0; index < this.steps.Count; index++)
                {
                    FlowStep step = this.steps[index];
                    string text = step.Label;
                    string state;

                    if (current < 0)
                    {
                        state = StepPending;
                    }
                    else if (finished || index < current)
                    {
                        state = StepCompleted;
                    }
                    else if (index == current)
                    {
                        state = failed ? StepFailed : StepCurrent;
                        string detail = (step.RunPrefix(row) + " " + step.Reached(row)).Trim();

                        if (detail.Length > 0)
                        {
                            text = step.Label + "\n" + detail;
                        }
                    }
                    else
                    {
                        state = StepPending;
                    }

                    table.Rows.Add(text, state);
                }

                return table;
            }

            internal override void Decorate(GridColumns columns)
            {
                foreach (FlowStep step in this.steps)
                {
                    columns.Column(step.Column).Caption(step.Label).TopLabel(string.Empty).Badge();
                    ReportDefinition.Each(columns, new[] { step.Column }, column => column.BadgeSemanticMember = step.SemanticColumn);
                }

                columns.Column(this.StageColumn).Caption(this.Label).TopLabel(string.Empty).Badge();
                ReportDefinition.Each(
                        columns, new[] { this.StageColumn }, column => column.BadgeSemanticMember = this.StageSemanticColumn);
            }

            private string StageText(DataRow row)
            {
                int current = this.LastReached(row);

                if (current < 0)
                {
                    return string.Empty;
                }

                FlowStep step = this.steps[current];
                string prefix = step.RunPrefix(row);
                string head = prefix.Length == 0 ? step.Label : step.Label + " " + prefix;
                string phase = step.Reached(row);
                bool finished = current == this.steps.Count - 1 && !step.IsFailed(row) && step.IsDone(row);

                return (finished ? DoneMark : string.Empty) + (string.IsNullOrEmpty(phase) ? head : head + " " + phase);
            }

            private string StageSemantic(DataRow row)
            {
                int current = this.LastReached(row);

                if (current < 0)
                {
                    return string.Empty;
                }

                if (this.steps[current].IsFailed(row))
                {
                    return ErrorSemantic;
                }

                return current == this.steps.Count - 1 && this.steps[current].IsDone(row) ? NeutralSemantic : InfoSemantic;
            }

            private int LastReached(DataRow row)
            {
                for (int index = this.steps.Count - 1; index >= 0; index--)
                {
                    if (this.steps[index].Reached(row) != null)
                    {
                        return index;
                    }
                }

                return -1;
            }

            private FlowStep LastStep()
            {
                if (this.steps.Count == 0)
                {
                    throw new InvalidOperationException("Add a step before Fail or Runs.");
                }

                return this.steps[this.steps.Count - 1];
            }
        }

        private sealed class FlowStep
        {
            private readonly List<string> columns = new List<string>();
            private readonly List<string> phases = new List<string>();
            private readonly List<string> cellPhases = new List<string>();
            private readonly string label;
            private readonly string column;
            private string failColumn = string.Empty;
            private string failValue = string.Empty;
            private string runColumn = string.Empty;

            internal FlowStep(string label, string prefix)
            {
                this.label = label ?? string.Empty;
                this.column = prefix + this.label.ToUpperInvariant();
            }

            internal string Label
            {
                get { return this.label; }
            }

            internal string Column
            {
                get { return this.column; }
            }

            internal string SemanticColumn
            {
                get { return this.column + SemanticSuffix; }
            }

            internal string FailValue
            {
                get { return this.failValue; }
            }

            internal FlowStep Event(string eventColumn, string phase, string cellPhase)
            {
                this.columns.Add(eventColumn ?? string.Empty);
                this.phases.Add(phase ?? string.Empty);
                this.cellPhases.Add(cellPhase ?? string.Empty);
                return this;
            }

            internal void Fail(string judgeColumn, string value)
            {
                this.failColumn = judgeColumn ?? string.Empty;
                this.failValue = value ?? string.Empty;
            }

            internal void Runs(string countColumn)
            {
                this.runColumn = countColumn ?? string.Empty;
            }

            internal bool IsFailed(DataRow row)
            {
                return this.failColumn.Length > 0
                        && this.failValue.Length > 0
                        && string.Equals(CellOf(row, this.failColumn), this.failValue, StringComparison.OrdinalIgnoreCase);
            }

            internal bool IsDone(DataRow row)
            {
                return this.columns.Count > 0 && CellOf(row, this.columns[0]).Length > 0;
            }

            internal string Reached(DataRow row)
            {
                if (this.IsFailed(row))
                {
                    return this.failValue;
                }

                int index = this.ReachedIndex(row);
                return index < 0 ? null : this.phases[index];
            }

            internal string RunPrefix(DataRow row)
            {
                int runs;

                if (this.runColumn.Length == 0
                        || !int.TryParse(CellOf(row, this.runColumn), NumberStyles.Integer, CultureInfo.InvariantCulture, out runs)
                        || runs < 2)
                {
                    return string.Empty;
                }

                return "#" + runs.ToString(CultureInfo.InvariantCulture);
            }

            internal string CellText(DataRow row)
            {
                int index = this.ReachedIndex(row);
                bool failed = this.IsFailed(row);

                if (index < 0 && !failed)
                {
                    return string.Empty;
                }

                List<string> parts = new List<string>();
                string prefix = this.RunPrefix(row);

                if (!failed && this.IsDone(row))
                {
                    parts.Add(DoneMark.Trim());
                }

                if (prefix.Length > 0)
                {
                    parts.Add(prefix);
                }

                if (failed)
                {
                    parts.Add(this.failValue);
                }
                else if (index > 0 && this.cellPhases[index].Length > 0)
                {
                    parts.Add(this.cellPhases[index]);
                }

                if (index >= 0)
                {
                    parts.Add(ShortTime(CellOf(row, this.columns[index])));
                }

                return string.Join(" ", parts.ToArray());
            }

            internal string CellSemantic(DataRow row)
            {
                if (this.IsFailed(row))
                {
                    return ErrorSemantic;
                }

                if (this.IsDone(row))
                {
                    return NeutralSemantic;
                }

                return this.ReachedIndex(row) < 0 ? string.Empty : InfoSemantic;
            }

            private int ReachedIndex(DataRow row)
            {
                for (int index = 0; index < this.columns.Count; index++)
                {
                    if (CellOf(row, this.columns[index]).Length > 0)
                    {
                        return index;
                    }
                }

                return -1;
            }
        }
    }
}
