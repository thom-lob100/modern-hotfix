using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Data;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Reporting
{
    /// <summary>
    /// 리포트의 흐름 — 인디케이터 띠 한 줄과 그리드의 Stage · 흐름 칸을 만든다. 두 종류다.
    /// 상태 흐름(<see cref="StateFlow"/>)은 상태 코드 칸 하나와 엔티티의 코드 순서로, 코드표 흐름(<see cref="CodeFlow"/>)은
    /// 서버가 내려준 단계 칸(<c>&lt;이름&gt;_STAGE</c>)과 코드표(순서 · 라벨)로 선다. 화면이 시각 칸을 보고 단계를 추리지 않는다(2026-11-07).
    /// </summary>
    public abstract class ReportFlow
    {
        internal const string StepLabelColumn = "LABEL";
        internal const string StepStateColumn = "STATE";
        internal const string SemanticSuffix = "_SEM";
        internal const string WarningSemantic = "warning";
        internal const string ErrorSemantic = "error";
        internal const string InfoSemantic = "info";
        internal const string NeutralSemantic = "neutral";
        internal const string DoneMark = "✓";

        private const string StepCompleted = "Completed";
        private const string StepCurrent = "Current";
        private const string StepPending = "Pending";
        private const string StepFailed = "Failed";
        private const string CellTimeFormat = "MM-dd HH:mm";
        private const string CellDateFormat = "MM-dd";

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

        internal abstract void Decorate(GridColumns columns, DataTable table);

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

        internal virtual IEnumerable<string> HiddenColumns(DataTable table)
        {
            return new string[0];
        }

        internal static string CellOf(DataRow row, string column)
        {
            if (row == null || string.IsNullOrEmpty(column) || !row.Table.Columns.Contains(column))
            {
                return string.Empty;
            }

            return TableHelper.CellText(row, column).Trim();
        }

        internal static string ShortTime(string text)
        {
            return Reformat(text, CellTimeFormat);
        }

        internal static string ShortDate(string text)
        {
            return Reformat(text, CellDateFormat);
        }

        internal static string Join(params string[] parts)
        {
            List<string> kept = new List<string>();

            foreach (string part in parts)
            {
                if (!string.IsNullOrEmpty(part))
                {
                    kept.Add(part);
                }
            }

            return string.Join(" ", kept.ToArray());
        }

        private static string Reformat(string text, string format)
        {
            DateTime parsed;

            return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed)
                    ? parsed.ToString(format, CultureInfo.InvariantCulture)
                    : text;
        }

        private static DataTable StepTable()
        {
            DataTable steps = new DataTable();
            steps.Columns.Add(StepLabelColumn, typeof(string));
            steps.Columns.Add(StepStateColumn, typeof(string));
            return steps;
        }

        /// <summary>상태 흐름 — 상태 코드 칸 하나. 코드 순서가 단계이고 표시 칸 <c>&lt;이름&gt;_STATE</c> 를 보탠다.</summary>
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

            internal override IEnumerable<string> HiddenColumns(DataTable table)
            {
                return new string[] { this.ColorColumn, this.stateField.Name };
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

            internal override void Decorate(GridColumns columns, DataTable table)
            {
                columns.Column(this.StateColumn).Caption(this.Label).TopLabel(string.Empty).Badge().BadgeWidth(this.AllTexts());
                ReportDefinition.Each(columns, new[] { this.StateColumn }, column => column.BadgeColorMember = this.ColorColumn);
            }

            private string[] AllTexts()
            {
                string[] all = this.AllCodes();
                string[] texts = new string[all.Length];

                for (int index = 0; index < all.Length; index++)
                {
                    texts[index] = this.StateText(all[index]);
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
                if (cancelled || index < 0)
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

        /// <summary>
        /// 코드표 흐름 — 서버가 행마다 내려준 단계 코드(<c>&lt;이름&gt;_STAGE</c>)가 현재 단계이고, 단계의 순서 · 라벨은 같은 이름의 코드표가 정한다.
        /// 단계에 붙는 칸은 이름 접두 규칙으로 찾는다 — <c>&lt;코드&gt;_TM · _PREP_TM · _START_TM · _END_TM · _EQP_ID · _RUN_CNT · _FAIL_CNT · _JUDGE_RSLT</c>.
        /// 틀이 보태는 칸: 단계마다 흐름 칸 <c>&lt;이름&gt;_&lt;코드&gt;</c>(배지 하나)와 Stage 표시 칸 <c>&lt;이름&gt;_STAGE_NM</c>. 칸이 둘 이상인 단계는
        /// 상세 묶음(라벨이 머리)이 되고, <c>_TM</c> 하나뿐인 단계는 흐름 칸이 그 시각을 보이므로 그 칸은 숨는다.
        /// 실패는 <c>&lt;코드&gt;_JUDGE_RSLT</c> 가 실패 값일 때, 회차 <c>#n</c> 은 <c>&lt;코드&gt;_RUN_CNT</c> 가 2 이상일 때다.
        /// </summary>
        public sealed class CodeFlow : ReportFlow
        {
            private const string StageSuffix = "STAGE";
            private const string StageTextSuffix = "_NM";
            private const string TimeSuffix = "_TM";
            private const string PrepSuffix = "_PREP_TM";
            private const string StartSuffix = "_START_TM";
            private const string EndSuffix = "_END_TM";
            private const string RunSuffix = "_RUN_CNT";
            private const string JudgeSuffix = "_JUDGE_RSLT";
            private const string DetailTimeFormat = "yyyy-MM-dd HH:mm";
            private const string DefaultFailValue = "FAIL";

            private static readonly string[] DetailSuffixes =
            {
                "_EQP_ID", PrepSuffix, StartSuffix, EndSuffix, TimeSuffix, RunSuffix, "_FAIL_CNT", JudgeSuffix
            };

            private static readonly string[] DetailCaptions = { "Eqp", "Job Prep", "Job Start", "Job End", "Time", "Runs", "Fails", "Judge" };

            private ReportCodeTable codes = ReportCodeTable.Empty;
            private string failValue = DefaultFailValue;

            internal CodeFlow(string label)
                    : base(label)
            {
            }

            /// <summary>서버가 내려주는 단계 칸이자 코드표 이름 — <c>AOS_STAGE</c>.</summary>
            public string StageColumn
            {
                get { return this.Prefix + StageSuffix; }
            }

            public string CodeType
            {
                get { return this.StageColumn; }
            }

            /// <summary>그리드 · 엑셀에 보이는 Stage 칸 — 라벨 · 회차 · Job 단계 글자(배지).</summary>
            public string StageTextColumn
            {
                get { return this.StageColumn + StageTextSuffix; }
            }

            internal bool HasCodes
            {
                get { return this.codes.Count > 0; }
            }

            internal string[] CellColumns
            {
                get
                {
                    string[] names = new string[this.codes.Count];

                    for (int index = 0; index < names.Length; index++)
                    {
                        names[index] = this.CellColumn(index);
                    }

                    return names;
                }
            }

            internal override int StepCount
            {
                get { return Math.Max(1, this.codes.Count); }
            }

            internal override string FailValue
            {
                get { return this.failValue; }
            }

            private string Prefix
            {
                get { return this.Label.ToUpperInvariant() + "_"; }
            }

            private string StageSemanticColumn
            {
                get { return this.StageColumn + SemanticSuffix; }
            }

            /// <summary>실패 값(기본 <c>FAIL</c>) — <c>&lt;코드&gt;_JUDGE_RSLT</c> 가 이 값이면 그 단계는 실패다.</summary>
            public CodeFlow Fail(string value)
            {
                this.failValue = string.IsNullOrEmpty(value) ? DefaultFailValue : value;
                return this;
            }

            internal void SetCodes(ReportCodeTable table)
            {
                this.codes = table ?? ReportCodeTable.Empty;
            }

            internal override bool IsFailed(DataRow row)
            {
                for (int index = 0; index < this.codes.Count; index++)
                {
                    if (this.StageFailed(row, index))
                    {
                        return true;
                    }
                }

                return false;
            }

            internal override IEnumerable<string> HiddenColumns(DataTable table)
            {
                List<string> names = new List<string>();
                names.Add(this.StageColumn);
                names.Add(this.StageSemanticColumn);

                for (int index = 0; index < this.codes.Count; index++)
                {
                    names.Add(this.CellSemanticColumn(index));
                    List<string> details = this.DetailColumns(table, index);

                    if (details.Count == 1 && details[0].EndsWith(TimeSuffix, StringComparison.Ordinal))
                    {
                        names.Add(details[0]);
                    }
                }

                return names;
            }

            /// <summary>응답에서 흐름이 가져가는 칸(접두 규칙) — 엔티티 묶음이 다시 싣지 않는다.</summary>
            internal IEnumerable<string> OwnedColumns(DataTable table)
            {
                List<string> names = new List<string>();

                for (int index = 0; index < this.codes.Count; index++)
                {
                    names.AddRange(this.DetailColumns(table, index));
                }

                return names;
            }

            /// <summary>상세 묶음 — 칸이 둘 이상(또는 <c>_TM</c> 이 아닌 하나)인 단계마다 하나, 라벨이 머리다.</summary>
            internal IEnumerable<ReportDefinition.ColumnBand> DetailBands(DataTable table)
            {
                List<ReportDefinition.ColumnBand> bands = new List<ReportDefinition.ColumnBand>();

                for (int index = 0; index < this.codes.Count; index++)
                {
                    List<string> details = this.DetailColumns(table, index);

                    if (details.Count == 0 || (details.Count == 1 && details[0].EndsWith(TimeSuffix, StringComparison.Ordinal)))
                    {
                        continue;
                    }

                    bands.Add(new ReportDefinition.ColumnBand(this.codes.LabelAt(index), details.ToArray()));
                }

                return bands;
            }

            internal override void Prepare(DataTable rows)
            {
                if (rows == null)
                {
                    return;
                }

                TableHelper.EnsureColumn(rows, this.StageTextColumn, typeof(string));
                TableHelper.EnsureColumn(rows, this.StageSemanticColumn, typeof(string));

                for (int index = 0; index < this.codes.Count; index++)
                {
                    TableHelper.EnsureColumn(rows, this.CellColumn(index), typeof(string));
                    TableHelper.EnsureColumn(rows, this.CellSemanticColumn(index), typeof(string));
                }

                foreach (DataRow row in rows.Rows)
                {
                    this.Fill(row);
                }
            }

            internal override DataTable Indicator(DataRow row)
            {
                DataTable steps = StepTable();
                int current = this.CurrentIndex(row);
                int last = this.codes.Count - 1;
                bool failed = current >= 0 && this.StageFailed(row, current);
                bool finished = current >= 0 && current == last && !failed;

                for (int index = 0; index < this.codes.Count; index++)
                {
                    string text = this.codes.LabelAt(index);
                    string state;

                    if (current < 0 || index > current)
                    {
                        state = StepPending;
                    }
                    else if (finished || index < current)
                    {
                        state = StepCompleted;
                    }
                    else
                    {
                        string phase;
                        this.LatestTime(row, index, out phase);
                        state = failed ? StepFailed : StepCurrent;
                        string detail = Join(this.Runs(row, index), failed ? this.failValue : phase);

                        if (detail.Length > 0)
                        {
                            text = text + "\n" + detail;
                        }
                    }

                    steps.Rows.Add(text, state);
                }

                return steps;
            }

            internal override void Decorate(GridColumns columns, DataTable table)
            {
                for (int index = 0; index < this.codes.Count; index++)
                {
                    string semantic = this.CellSemanticColumn(index);
                    columns.Column(this.CellColumn(index)).Caption(this.codes.LabelAt(index)).TopLabel(string.Empty).Badge();
                    ReportDefinition.Each(columns, new[] { this.CellColumn(index) }, column => column.BadgeSemanticMember = semantic);

                    for (int suffixIndex = 0; suffixIndex < DetailSuffixes.Length; suffixIndex++)
                    {
                        string suffix = DetailSuffixes[suffixIndex];
                        string detail = this.codes.ValueAt(index) + suffix;

                        if (table == null || !table.Columns.Contains(detail))
                        {
                            continue;
                        }

                        columns.Column(detail).Caption(DetailCaptions[suffixIndex]).TopLabel(this.codes.LabelAt(index));

                        if (suffix == JudgeSuffix)
                        {
                            string judge = detail;
                            columns.Column(detail).Badge();
                            ReportDefinition.Each(columns, new[] { detail }, column => column.BadgeSemanticMember = judge);
                        }

                        if (suffix.EndsWith(TimeSuffix, StringComparison.Ordinal))
                        {
                            columns.Column(detail).Format(DetailTimeFormat);
                            ReportDefinition.Each(columns, new[] { detail }, column => column.MergeCells = true);
                        }
                    }
                }

                columns.Column(this.StageTextColumn).Caption(this.Label).TopLabel(string.Empty).Badge();
                ReportDefinition.Each(
                        columns, new[] { this.StageTextColumn }, column => column.BadgeSemanticMember = this.StageSemanticColumn);
            }

            private void Fill(DataRow row)
            {
                int current = this.CurrentIndex(row);
                int last = this.codes.Count - 1;
                bool failed = current >= 0 && this.StageFailed(row, current);
                bool finished = current >= 0 && current == last && !failed;

                if (current < 0)
                {
                    row[this.StageTextColumn] = CellOf(row, this.StageColumn);
                    row[this.StageSemanticColumn] = string.Empty;
                }
                else
                {
                    string phase;
                    this.LatestTime(row, current, out phase);
                    string head = Join(this.codes.LabelAt(current), this.Runs(row, current));
                    row[this.StageTextColumn] = failed ? Join(head, this.failValue)
                            : finished ? Join(DoneMark, head)
                            : Join(head, phase);
                    row[this.StageSemanticColumn] = failed ? ErrorSemantic : finished ? NeutralSemantic : InfoSemantic;
                }

                for (int index = 0; index < this.codes.Count; index++)
                {
                    string text = string.Empty;
                    string semantic = string.Empty;

                    if (current >= 0 && index <= current)
                    {
                        string phase;
                        string time = this.LatestTime(row, index, out phase);
                        string runs = this.Runs(row, index);

                        if (finished || index < current)
                        {
                            text = Join(DoneMark, runs, time);
                            semantic = NeutralSemantic;
                        }
                        else if (failed)
                        {
                            text = Join(runs, this.failValue, time);
                            semantic = ErrorSemantic;
                        }
                        else
                        {
                            text = Join(runs, phase, time);
                            semantic = InfoSemantic;

                            if (text.Length == 0)
                            {
                                text = this.codes.LabelAt(index);
                            }
                        }
                    }

                    row[this.CellColumn(index)] = text;
                    row[this.CellSemanticColumn(index)] = semantic;
                }
            }

            private int CurrentIndex(DataRow row)
            {
                return row == null ? -1 : this.codes.IndexOf(CellOf(row, this.StageColumn));
            }

            private bool StageFailed(DataRow row, int index)
            {
                return string.Equals(CellOf(row, this.codes.ValueAt(index) + JudgeSuffix), this.failValue, StringComparison.OrdinalIgnoreCase);
            }

            private string Runs(DataRow row, int index)
            {
                int runs;

                if (!int.TryParse(CellOf(row, this.codes.ValueAt(index) + RunSuffix), NumberStyles.Integer, CultureInfo.InvariantCulture, out runs)
                        || runs < 2)
                {
                    return string.Empty;
                }

                return "#" + runs.ToString(CultureInfo.InvariantCulture);
            }

            /// <summary>단계의 가장 뒤 시각 — End → Start → Prep → 대표 시각 순으로 찾고, Job 단계 이름을 함께 돌려준다.</summary>
            private string LatestTime(DataRow row, int index, out string phase)
            {
                string code = this.codes.ValueAt(index);
                string end = CellOf(row, code + EndSuffix);

                if (end.Length > 0)
                {
                    phase = "End";
                    return ShortTime(end);
                }

                string start = CellOf(row, code + StartSuffix);

                if (start.Length > 0)
                {
                    phase = "Start";
                    return ShortTime(start);
                }

                string prep = CellOf(row, code + PrepSuffix);

                if (prep.Length > 0)
                {
                    phase = "Prep";
                    return ShortTime(prep);
                }

                phase = string.Empty;
                string time = CellOf(row, code + TimeSuffix);
                return time.Length > 0 ? ShortTime(time) : string.Empty;
            }

            private List<string> DetailColumns(DataTable table, int index)
            {
                List<string> names = new List<string>();

                if (table == null)
                {
                    return names;
                }

                string code = this.codes.ValueAt(index);

                foreach (string suffix in DetailSuffixes)
                {
                    if (table.Columns.Contains(code + suffix))
                    {
                        names.Add(table.Columns[code + suffix].ColumnName);
                    }
                }

                return names;
            }

            private string CellColumn(int index)
            {
                return this.Prefix + this.codes.ValueAt(index);
            }

            private string CellSemanticColumn(int index)
            {
                return this.CellColumn(index) + SemanticSuffix;
            }
        }
    }
}
