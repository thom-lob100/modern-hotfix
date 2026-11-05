using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Theming;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Report.Reporting
{
    public sealed class ReportDefinition
    {
        private const string NoValueText = "-";
        private const string Separator = " · ";
        private const int MaxKpis = 6;

        private readonly List<ReportCondition> conditions = new List<ReportCondition>();
        private readonly List<ReportFlow> flows = new List<ReportFlow>();
        private readonly List<ColumnBand> bands = new List<ColumnBand>();
        private readonly List<string> merged = new List<string>();
        private readonly List<string> hidden = new List<string>();
        private readonly List<string> semanticBadges = new List<string>();
        private readonly List<Field> countFields = new List<Field>();
        private readonly List<string> countNouns = new List<string>();
        private readonly List<KpiCard> kpis = new List<KpiCard>();
        private readonly List<KeyValuePair<string, string>> semanticBadgePairs = new List<KeyValuePair<string, string>>();
        private readonly List<KeyBadge> keyBadges = new List<KeyBadge>();
        private string tintColumn = string.Empty;
        private string tintValue = string.Empty;
        private string tintColor = string.Empty;
        private Field[] keyFields = new Field[0];
        private Func<GridColumns, GridColumns> decorate;

        public ReportDefinition()
        {
            this.Title = string.Empty;
            this.Environment = string.Empty;
            this.EmptyText = "No rows";
            this.ExportSheetName = "Report";
            this.ExportFilePrefix = "Report";
            this.SearchOnOpen = true;
            this.Collapsible = true;
        }

        public string Title { get; set; }

        public string Environment { get; set; }

        public string EmptyText { get; set; }

        public string ExportSheetName { get; set; }

        public string ExportFilePrefix { get; set; }

        public bool SearchOnOpen { get; set; }

        public bool Collapsible { get; set; }

        internal IList<ReportCondition> Conditions
        {
            get { return this.conditions; }
        }

        internal IList<ReportFlow> Flows
        {
            get { return this.flows; }
        }

        internal IList<KpiCard> Kpis
        {
            get { return this.kpis; }
        }

        internal IList<KeyBadge> KeyBadges
        {
            get { return this.keyBadges; }
        }

        internal string KeyMember
        {
            get
            {
                List<string> names = new List<string>();

                foreach (Field field in this.keyFields)
                {
                    names.Add(field.Name);
                }

                return string.Join(",", names.ToArray());
            }
        }

        public ReportDefinition Key(params Field[] fields)
        {
            this.keyFields = fields ?? new Field[0];
            return this;
        }

        public ReportCondition.DateRangeCondition DateRange(string caption, Field fromField, Field toField)
        {
            ReportCondition.DateRangeCondition condition = new ReportCondition.DateRangeCondition(caption, fromField, toField);
            this.conditions.Add(condition);
            return condition;
        }

        public ReportCondition.TextCondition Text(string caption, Field field)
        {
            ReportCondition.TextCondition condition = new ReportCondition.TextCondition(caption, field);
            this.conditions.Add(condition);
            return condition;
        }

        public ReportCondition.CodesCondition Codes(string caption, Field field, DataTable options)
        {
            ReportCondition.CodesCondition condition = new ReportCondition.CodesCondition(caption, field, options);
            this.conditions.Add(condition);
            return condition;
        }

        public ReportCondition.CheckCondition Check(string caption, Field field)
        {
            ReportCondition.CheckCondition condition = new ReportCondition.CheckCondition(caption, field);
            this.conditions.Add(condition);
            return condition;
        }

        public ReportFlow.StateFlow StateFlow(string label, Field stateField, string[] codes)
        {
            ReportFlow.StateFlow flow = new ReportFlow.StateFlow(label, stateField, codes);
            this.flows.Add(flow);
            return flow;
        }

        public ReportFlow.EventFlow EventFlow(string label)
        {
            ReportFlow.EventFlow flow = new ReportFlow.EventFlow(label);
            this.flows.Add(flow);
            return flow;
        }

        public ColumnBand Band(string caption, params string[] columns)
        {
            ColumnBand band = new ColumnBand(caption, columns);
            this.bands.Add(band);
            return band;
        }

        public ReportDefinition Merge(params string[] columns)
        {
            this.merged.AddRange(columns ?? new string[0]);
            return this;
        }

        public ReportDefinition Hide(params string[] columns)
        {
            this.hidden.AddRange(columns ?? new string[0]);
            return this;
        }

        public ReportDefinition SemanticBadges(params string[] columns)
        {
            this.semanticBadges.AddRange(columns ?? new string[0]);
            return this;
        }

        public ReportDefinition SemanticBadge(string column, string semanticColumn)
        {
            this.semanticBadgePairs.Add(new KeyValuePair<string, string>(column ?? string.Empty, semanticColumn ?? string.Empty));
            return this;
        }

        public ReportDefinition RowTint(string column, string value, SemanticKind kind)
        {
            this.tintColumn = column ?? string.Empty;
            this.tintValue = value ?? string.Empty;
            this.tintColor = TintColorOf(kind);
            return this;
        }

        public ReportDefinition BadgeWhen(string column, string value, string text, SemanticKind kind)
        {
            this.keyBadges.Add(new KeyBadge(column, value, text, kind));
            return this;
        }

        public ReportDefinition BadgeCount(string column, string prefix, SemanticKind kind)
        {
            this.keyBadges.Add(new KeyBadge(column, null, prefix, kind));
            return this;
        }

        internal string RowColor(object item)
        {
            DataRowView view = item as DataRowView;
            DataRow row = view == null ? item as DataRow : view.Row;

            if (row == null || this.tintColumn.Length == 0 || !row.Table.Columns.Contains(this.tintColumn))
            {
                return null;
            }

            return string.Equals(TableHelper.CellText(row, this.tintColumn).Trim(), this.tintValue, StringComparison.OrdinalIgnoreCase)
                    ? this.tintColor
                    : null;
        }

        private static string TintColorOf(SemanticKind kind)
        {
            string token = kind == SemanticKind.Warning ? "Brush.WarningBackground"
                    : kind == SemanticKind.Error ? "Brush.ErrorBackground"
                    : kind == SemanticKind.Success ? "Brush.SuccessBackground"
                    : kind == SemanticKind.Info ? "Brush.InfoBackground"
                    : "Brush.NeutralBackground";

            return System.Drawing.ColorTranslator.ToHtml(ModernTokenColors.Get(token, System.Drawing.Color.LightGoldenrodYellow));
        }

        public ReportDefinition Count(Field field, string noun)
        {
            this.countFields.Add(field);
            this.countNouns.Add(noun ?? string.Empty);
            return this;
        }

        public ReportDefinition KpiDistinct(string title, string column)
        {
            return this.AddKpi(new KpiCard(title, column, null));
        }

        public ReportDefinition KpiMatch(string title, string column, string value)
        {
            return this.AddKpi(new KpiCard(title, column, value ?? string.Empty));
        }

        public ReportDefinition Columns(Func<GridColumns, GridColumns> columnDecorator)
        {
            this.decorate = columnDecorator;
            return this;
        }

        internal static void Each(GridColumns columns, IEnumerable<string> names, Action<ModernDataGridColumn> apply)
        {
            List<string> wanted = new List<string>(names);

            foreach (ModernDataGridColumn column in columns.ToArray())
            {
                foreach (string name in wanted)
                {
                    if (string.Equals(column.DataPropertyName, name, StringComparison.OrdinalIgnoreCase))
                    {
                        apply(column);
                        break;
                    }
                }
            }
        }

        internal string[] Order()
        {
            List<string> order = new List<string>();

            foreach (ColumnBand band in this.bands)
            {
                foreach (string column in band.Columns)
                {
                    if (!order.Contains(column))
                    {
                        order.Add(column);
                    }
                }
            }

            return order.ToArray();
        }

        internal GridColumns BuildColumns(DataTable table)
        {
            string[] order = this.Order();
            GridColumns columns = GridColumns.Of(table ?? EmptyTable(order))
                    .Hide(this.HiddenColumns())
                    .First(order);

            foreach (ReportFlow flow in this.flows)
            {
                flow.Decorate(columns);
            }

            if (this.semanticBadges.Count > 0)
            {
                columns.Column(this.semanticBadges.ToArray()).Badge();
                Each(columns, this.semanticBadges, column => column.BadgeSemanticMember = column.DataPropertyName);
            }

            foreach (KeyValuePair<string, string> pair in this.semanticBadgePairs)
            {
                string semantic = pair.Value;
                columns.Column(pair.Key).Badge();
                Each(columns, new[] { pair.Key }, column => column.BadgeSemanticMember = semantic);
            }

            if (this.decorate != null)
            {
                columns = this.decorate(columns);
            }

            foreach (ColumnBand band in this.bands)
            {
                ColumnBand current = band;
                Each(columns, current.Columns, column =>
                {
                    column.HeaderGroup = current.Caption;

                    if (current.IsFrozen)
                    {
                        column.Frozen = true;
                    }
                });
            }

            Each(columns, this.merged, column => column.MergeCells = true);
            return columns;
        }

        internal List<ReportCondition> PrimaryConditions()
        {
            return this.ConditionsWhere(true);
        }

        internal List<ReportCondition> DrawerConditions()
        {
            return this.ConditionsWhere(false);
        }

        private List<ReportCondition> ConditionsWhere(bool primary)
        {
            List<ReportCondition> found = new List<ReportCondition>();

            foreach (ReportCondition condition in this.conditions)
            {
                if (condition.IsPrimary == primary)
                {
                    found.Add(condition);
                }
            }

            return found;
        }

        internal bool RequiredComplete()
        {
            foreach (ReportCondition condition in this.conditions)
            {
                if (condition.IsRequired && !condition.IsComplete)
                {
                    return false;
                }
            }

            return true;
        }

        internal RequestParameters Parameters()
        {
            List<object> pairs = new List<object>();

            foreach (ReportCondition condition in this.conditions)
            {
                condition.AppendValues(pairs);
            }

            return RequestParameters.Of(pairs.ToArray());
        }

        internal void Prepare(DataTable rows)
        {
            foreach (ReportFlow flow in this.flows)
            {
                flow.Prepare(rows);
            }
        }

        internal string KeyText(DataRow row)
        {
            if (row == null)
            {
                return NoValueText;
            }

            List<string> parts = new List<string>();

            foreach (Field field in this.keyFields)
            {
                string value = field.Text(row);

                if (value.Length > 0)
                {
                    parts.Add(value);
                }
            }

            return parts.Count == 0 ? NoValueText : string.Join(Separator, parts.ToArray());
        }

        internal ReportFlow.StateFlow FirstStateFlow()
        {
            foreach (ReportFlow flow in this.flows)
            {
                ReportFlow.StateFlow state = flow as ReportFlow.StateFlow;

                if (state != null)
                {
                    return state;
                }
            }

            return null;
        }

        internal string CountText(DataTable table, IList<DataRow> rows)
        {
            if (table == null)
            {
                return string.Empty;
            }

            List<string> parts = new List<string>();

            for (int index = 0; index < this.countFields.Count; index++)
            {
                parts.Add(Count(table, rows, this.countFields[index]) + " " + this.countNouns[index]);
            }

            string failValue = string.Empty;

            foreach (ReportFlow flow in this.flows)
            {
                if (flow.FailValue.Length > 0)
                {
                    failValue = flow.FailValue;
                    break;
                }
            }

            if (failValue.Length > 0)
            {
                int failed = 0;

                foreach (DataRow row in rows)
                {
                    foreach (ReportFlow flow in this.flows)
                    {
                        if (flow.IsFailed(row))
                        {
                            failed++;
                            break;
                        }
                    }
                }

                if (failed > 0)
                {
                    parts.Add(failed.ToString("N0", CultureInfo.InvariantCulture) + " " + failValue);
                }
            }

            return string.Join(Separator, parts.ToArray());
        }

        private string[] HiddenColumns()
        {
            List<string> names = new List<string>(this.hidden);

            foreach (ReportFlow flow in this.flows)
            {
                names.AddRange(flow.HiddenColumns);
            }

            return names.ToArray();
        }

        private ReportDefinition AddKpi(KpiCard kpi)
        {
            if (this.kpis.Count >= MaxKpis)
            {
                throw new InvalidOperationException("A report shows at most " + MaxKpis.ToString(CultureInfo.InvariantCulture) + " KPI cards.");
            }

            this.kpis.Add(kpi);
            return this;
        }

        private static string Count(DataTable table, IList<DataRow> rows, Field field)
        {
            if (!table.Columns.Contains(field.Name))
            {
                return NoValueText;
            }

            return DistinctCount(rows, field.Name).ToString("N0", CultureInfo.InvariantCulture);
        }

        private static int DistinctCount(IList<DataRow> rows, string column)
        {
            HashSet<string> ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (DataRow row in rows)
            {
                string id = TableHelper.CellText(row, column).Trim();

                if (id.Length > 0)
                {
                    ids.Add(id);
                }
            }

            return ids.Count;
        }

        private static DataTable EmptyTable(IList<string> columns)
        {
            DataTable table = new DataTable("REPORT");

            foreach (string column in columns)
            {
                if (!table.Columns.Contains(column))
                {
                    table.Columns.Add(column, typeof(string));
                }
            }

            return table;
        }

        internal sealed class KpiCard
        {
            private readonly string title;
            private readonly string column;
            private readonly string matchValue;

            internal KpiCard(string title, string column, string matchValue)
            {
                this.title = title ?? string.Empty;
                this.column = column ?? string.Empty;
                this.matchValue = matchValue;
            }

            internal string Title
            {
                get { return this.title; }
            }

            internal string ValueText(DataTable table, IList<DataRow> rows)
            {
                if (table == null || !table.Columns.Contains(this.column))
                {
                    return NoValueText;
                }

                int count = this.matchValue == null ? DistinctCount(rows, this.column) : this.MatchCount(rows);
                return count.ToString("N0", CultureInfo.InvariantCulture);
            }

            private int MatchCount(IList<DataRow> rows)
            {
                int count = 0;

                foreach (DataRow row in rows)
                {
                    if (string.Equals(TableHelper.CellText(row, this.column).Trim(), this.matchValue, StringComparison.OrdinalIgnoreCase))
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        internal sealed class KeyBadge
        {
            private readonly string column;
            private readonly string value;
            private readonly string text;
            private readonly SemanticKind kind;

            internal KeyBadge(string column, string value, string text, SemanticKind kind)
            {
                this.column = column ?? string.Empty;
                this.value = value;
                this.text = text ?? string.Empty;
                this.kind = kind;
            }

            internal SemanticKind Kind
            {
                get { return this.kind; }
            }

            internal string TextFor(DataRow row)
            {
                if (row == null || !row.Table.Columns.Contains(this.column))
                {
                    return string.Empty;
                }

                string cell = TableHelper.CellText(row, this.column).Trim();

                if (this.value != null)
                {
                    return string.Equals(cell, this.value, StringComparison.OrdinalIgnoreCase) ? this.text : string.Empty;
                }

                int count;

                if (!int.TryParse(cell, NumberStyles.Integer, CultureInfo.InvariantCulture, out count) || count < 1)
                {
                    return string.Empty;
                }

                return this.text + count.ToString(CultureInfo.InvariantCulture);
            }
        }

        public sealed class ColumnBand
        {
            private readonly string caption;
            private readonly string[] columns;
            private bool frozen;

            internal ColumnBand(string caption, string[] columns)
            {
                this.caption = caption ?? string.Empty;
                this.columns = columns ?? new string[0];
            }

            internal string Caption
            {
                get { return this.caption; }
            }

            internal string[] Columns
            {
                get { return this.columns; }
            }

            internal bool IsFrozen
            {
                get { return this.frozen; }
            }

            public ColumnBand Frozen()
            {
                this.frozen = true;
                return this;
            }
        }
    }
}
