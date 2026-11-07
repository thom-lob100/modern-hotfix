using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Theming;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Reporting
{
    /// <summary>
    /// 리포트 정의 — 리포트가 적는 것은 제목 · 행 키 · 조건 · 흐름 이름 · 가지 코드표 이름 · 묶음 엔티티 순서 · 병합 · 숨김 · 행 배경뿐이다.
    /// 칸의 주인은 흐름(접두 규칙) → 가지 → 엔티티(선언 순서) → Other 순으로 정하고, 캡션 · 배지 · 형식은 엔티티의 칸 선언에서 온다.
    /// KPI 는 적지 않는다 — Key 첫 칸의 고유 수 · 실패 행 수 · 가지 코드표 순서대로 Y 행 수, 합쳐 여섯 장까지다(2026-11-07).
    /// </summary>
    public sealed class ReportDefinition
    {
        private const string NoValueText = "-";
        private const string Separator = " · ";
        private const string IdBand = "Id";
        private const string StageBand = "Stage";
        private const string BranchBand = "Branch";
        private const string OtherBand = "Other";
        private const string FlowBandSuffix = " Flow";
        private const string EntitySuffix = "Entity";
        private const string ColorSuffix = "_COLOR";
        private const int MaxKpis = 6;

        private readonly List<ReportCondition> conditions = new List<ReportCondition>();
        private readonly List<ReportFlow> flows = new List<ReportFlow>();
        private readonly List<EntityDefinition> bandEntities = new List<EntityDefinition>();
        private readonly List<string> merged = new List<string>();
        private readonly List<string> hidden = new List<string>();
        private readonly List<string> semanticBadges = new List<string>();
        private ReportBranches branches;
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

        internal int BranchCount
        {
            get { return this.branches == null ? 0 : this.branches.Count; }
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

        /// <summary>코드표 흐름 — 서버가 준 <c>&lt;이름&gt;_STAGE</c> 칸과 같은 이름의 코드표. 리포트는 이름 하나만 적는다.</summary>
        public ReportFlow.CodeFlow CodeFlow(string label)
        {
            ReportFlow.CodeFlow flow = new ReportFlow.CodeFlow(label);
            this.flows.Add(flow);
            return flow;
        }

        /// <summary>가지 코드표 — 코드마다 <c>&lt;코드&gt;_YN</c> 을 읽어 Branch 묶음 · 띠 배지 · KPI 를 만든다.</summary>
        public ReportBranches Branches(string codeType)
        {
            this.branches = new ReportBranches(codeType);
            return this.branches;
        }

        /// <summary>묶음 = 엔티티. 적은 순서대로 2단 머리가 서고, 각 엔티티에 선언된 칸 중 응답에 있는 것이 선언 순서대로 들어간다.</summary>
        public ReportDefinition Bands(params EntityDefinition[] entities)
        {
            foreach (EntityDefinition entity in entities ?? new EntityDefinition[0])
            {
                if (entity != null && !this.bandEntities.Contains(entity))
                {
                    this.bandEntities.Add(entity);
                }
            }

            return this;
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

        public ReportDefinition RowTint(string column, string value, SemanticKind kind)
        {
            this.tintColumn = column ?? string.Empty;
            this.tintValue = value ?? string.Empty;
            this.tintColor = TintColorOf(kind);
            return this;
        }

        public ReportDefinition Columns(Func<GridColumns, GridColumns> columnDecorator)
        {
            this.decorate = columnDecorator;
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

        /// <summary>아직 코드표를 받지 못한 흐름 · 가지의 코드표 이름(중복 없이).</summary>
        internal List<string> PendingCodeTypes()
        {
            List<string> types = new List<string>();

            foreach (ReportFlow.CodeFlow flow in this.CodeFlows())
            {
                if (!flow.HasCodes && !types.Contains(flow.CodeType))
                {
                    types.Add(flow.CodeType);
                }
            }

            if (this.branches != null && !this.branches.HasCodes && this.branches.CodeType.Length > 0
                    && !types.Contains(this.branches.CodeType))
            {
                types.Add(this.branches.CodeType);
            }

            return types;
        }

        internal void ApplyCodes(string codeType, DataTable table)
        {
            ReportCodeTable codes = ReportCodeTable.Parse(table);

            foreach (ReportFlow.CodeFlow flow in this.CodeFlows())
            {
                if (string.Equals(flow.CodeType, codeType, StringComparison.OrdinalIgnoreCase))
                {
                    flow.SetCodes(codes);
                }
            }

            if (this.branches != null && string.Equals(this.branches.CodeType, codeType, StringComparison.OrdinalIgnoreCase))
            {
                this.branches.SetCodes(codes);
            }
        }

        internal string BranchBadgeText(DataRow row, int index)
        {
            return this.branches == null ? string.Empty : this.branches.BadgeText(row, index);
        }

        /// <summary>KPI 카드 — Key 첫 칸의 고유 수, 실패 행 수(실패 값을 가진 흐름이 있을 때), 가지 코드표 순서대로 Y 행 수. 여섯 장까지.</summary>
        internal List<KpiCard> KpiCards()
        {
            List<KpiCard> cards = new List<KpiCard>();

            if (this.keyFields.Length > 0)
            {
                string column = this.keyFields[0].Name;
                cards.Add(new KpiCard(PluralOf(this.keyFields[0]), table => table.Columns.Contains(column), rows => DistinctCount(rows, column)));
            }

            string failValue = this.FailValue();

            if (failValue.Length > 0)
            {
                cards.Add(new KpiCard(failValue, table => true, this.FailedCount));
            }

            if (this.branches != null)
            {
                for (int index = 0; index < this.branches.Count && cards.Count < MaxKpis; index++)
                {
                    int branch = index;
                    cards.Add(new KpiCard(this.branches.Label(branch), table => this.branches.IsAvailable(table, branch),
                            rows => this.branches.CountOn(rows, branch)));
                }
            }

            return cards;
        }

        internal GridColumns BuildColumns(DataTable table)
        {
            List<ColumnBand> bands = this.Bands(table);
            string[] order = Order(bands);
            string[] hiddenNames = this.HiddenColumns(table);
            GridColumns columns = GridColumns.Of(table ?? EmptyTable(order))
                    .Hide(hiddenNames)
                    .First(order);

            foreach (EntityDefinition entity in this.bandEntities)
            {
                ApplyFieldSettings(columns, entity);
            }

            foreach (ReportFlow flow in this.flows)
            {
                flow.Decorate(columns, table);
            }

            if (this.branches != null)
            {
                this.branches.Decorate(columns);
            }

            if (this.semanticBadges.Count > 0)
            {
                columns.Column(this.semanticBadges.ToArray()).Badge();
                Each(columns, this.semanticBadges, column => column.BadgeSemanticMember = column.DataPropertyName);
            }

            foreach (Field field in this.keyFields)
            {
                columns.Column(field.Name).TopLabel(string.Empty);
            }

            if (this.decorate != null)
            {
                columns = this.decorate(columns);
            }

            foreach (ColumnBand band in bands)
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

            if (this.branches != null)
            {
                this.branches.Prepare(rows);
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

        /// <summary>묶음 순서 — Id(키, 고정) · Stage(흐름 표시 칸, 고정) · 흐름 칸 · Branch · 엔티티 순서 · 흐름 상세 · Other.</summary>
        private List<ColumnBand> Bands(DataTable table)
        {
            List<ColumnBand> bands = new List<ColumnBand>();
            HashSet<string> owned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> hiddenNames = new HashSet<string>(this.HiddenColumns(table), StringComparer.OrdinalIgnoreCase);

            List<string> idColumns = new List<string>();

            foreach (Field field in this.keyFields)
            {
                idColumns.Add(field.Name);
            }

            AddBand(bands, owned, IdBand, idColumns, true);

            List<string> stageColumns = new List<string>();

            foreach (ReportFlow flow in this.flows)
            {
                ReportFlow.StateFlow state = flow as ReportFlow.StateFlow;
                ReportFlow.CodeFlow code = flow as ReportFlow.CodeFlow;
                stageColumns.Add(state != null ? state.StateColumn : code.StageTextColumn);
            }

            AddBand(bands, owned, StageBand, stageColumns, true);

            foreach (ReportFlow.CodeFlow flow in this.CodeFlows())
            {
                AddBand(bands, owned, flow.Label + FlowBandSuffix, new List<string>(flow.CellColumns), false);
                owned.UnionWith(flow.OwnedColumns(table));
            }

            if (this.branches != null)
            {
                AddBand(bands, owned, BranchBand, new List<string>(this.branches.Columns), false);
                owned.UnionWith(this.branches.OwnedColumns(table));
            }

            foreach (EntityDefinition entity in this.bandEntities)
            {
                List<string> names = new List<string>();

                foreach (Field field in entity.AllFields)
                {
                    if (table != null && table.Columns.Contains(field.Name) && !owned.Contains(field.Name) && !hiddenNames.Contains(field.Name))
                    {
                        names.Add(field.Name);
                        owned.Add(field.Name);
                    }
                }

                AddBand(bands, owned, EntityCaption(entity), names, false);
            }

            foreach (ReportFlow.CodeFlow flow in this.CodeFlows())
            {
                foreach (ColumnBand band in flow.DetailBands(table))
                {
                    bands.Add(band);
                }
            }

            if (table != null)
            {
                List<string> others = new List<string>();

                foreach (DataColumn column in table.Columns)
                {
                    string name = column.ColumnName;

                    if (owned.Contains(name) || hiddenNames.Contains(name)
                            || name.EndsWith(ReportFlow.SemanticSuffix, StringComparison.OrdinalIgnoreCase)
                            || name.EndsWith(ColorSuffix, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    others.Add(name);
                }

                AddBand(bands, owned, OtherBand, others, false);
            }

            return bands;
        }

        private List<ReportFlow.CodeFlow> CodeFlows()
        {
            List<ReportFlow.CodeFlow> found = new List<ReportFlow.CodeFlow>();

            foreach (ReportFlow flow in this.flows)
            {
                ReportFlow.CodeFlow code = flow as ReportFlow.CodeFlow;

                if (code != null)
                {
                    found.Add(code);
                }
            }

            return found;
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

        private string[] HiddenColumns(DataTable table)
        {
            List<string> names = new List<string>(this.hidden);

            foreach (ReportFlow flow in this.flows)
            {
                names.AddRange(flow.HiddenColumns(table));
            }

            if (this.branches != null)
            {
                names.AddRange(this.branches.HiddenColumns(table));
            }

            return names.ToArray();
        }

        private string FailValue()
        {
            foreach (ReportFlow flow in this.flows)
            {
                if (flow.FailValue.Length > 0)
                {
                    return flow.FailValue;
                }
            }

            return string.Empty;
        }

        private int FailedCount(IList<DataRow> rows)
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

            return failed;
        }

        private static void AddBand(List<ColumnBand> bands, HashSet<string> owned, string caption, List<string> columns, bool frozen)
        {
            if (columns.Count == 0)
            {
                return;
            }

            ColumnBand band = new ColumnBand(caption, columns.ToArray());

            if (frozen)
            {
                band.Frozen();
            }

            bands.Add(band);
            owned.UnionWith(columns);
        }

        private static string[] Order(List<ColumnBand> bands)
        {
            List<string> order = new List<string>();

            foreach (ColumnBand band in bands)
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

        /// <summary>엔티티 칸 선언(캡션 · 위아래 글씨 · 형식 · 정렬 · 배지)을 그리드 칸에 옮긴다 — <c>EntityDefinition.Columns</c> 와 같은 규칙.</summary>
        private static void ApplyFieldSettings(GridColumns columns, EntityDefinition entity)
        {
            foreach (Field field in entity.AllFields)
            {
                if (field.CaptionSetting != null)
                {
                    columns.Column(field.Name).Caption(field.CaptionSetting);
                }

                if (field.TopLabelSetting != null)
                {
                    columns.Column(field.Name).TopLabel(field.TopLabelSetting);
                }

                if (field.BottomLabelSetting != null)
                {
                    columns.Column(field.Name).BottomLabel(field.BottomLabelSetting);
                }

                if (field.FormatSetting != null)
                {
                    columns.Column(field.Name).Format(field.FormatSetting);
                }

                if (field.TextAlignmentSetting.HasValue)
                {
                    columns.Column(field.Name).TextAlignment(field.TextAlignmentSetting.Value);
                }

                if (field.HeaderAlignmentSetting.HasValue)
                {
                    columns.Column(field.Name).HeaderAlignment(field.HeaderAlignmentSetting.Value);
                }

                if (field.TopLabelAlignmentSetting.HasValue)
                {
                    columns.Column(field.Name).TopLabelAlignment(field.TopLabelAlignmentSetting.Value);
                }

                if (field.BottomLabelAlignmentSetting.HasValue)
                {
                    columns.Column(field.Name).BottomLabelAlignment(field.BottomLabelAlignmentSetting.Value);
                }

                if (field.BadgeSetting)
                {
                    columns.Column(field.Name).Badge();
                }
            }
        }

        /// <summary>엔티티 묶음의 머리 — 타입 이름에서 <c>Entity</c> 를 떼고 대문자 앞에 띄어쓰기(<c>RequestWaferEntity</c> → <c>Request Wafer</c>).</summary>
        private static string EntityCaption(EntityDefinition entity)
        {
            return Spaced(StripEntity(entity.GetType().Name));
        }

        /// <summary>Key 첫 칸의 KPI 제목 — 그 칸을 선언한 엔티티 이름의 복수형(<c>RequestEntity</c> → <c>Requests</c>).</summary>
        private static string PluralOf(Field field)
        {
            string name = field.Owner == null ? "Key" : StripEntity(field.Owner.GetType().Name);
            string spaced = Spaced(name);
            return spaced.EndsWith("s", StringComparison.OrdinalIgnoreCase) ? spaced + "es" : spaced + "s";
        }

        private static string StripEntity(string typeName)
        {
            return typeName.EndsWith(EntitySuffix, StringComparison.Ordinal)
                    ? typeName.Substring(0, typeName.Length - EntitySuffix.Length)
                    : typeName;
        }

        private static string Spaced(string pascal)
        {
            StringBuilder text = new StringBuilder();

            for (int index = 0; index < pascal.Length; index++)
            {
                if (index > 0 && char.IsUpper(pascal[index]) && !char.IsUpper(pascal[index - 1]))
                {
                    text.Append(' ');
                }

                text.Append(pascal[index]);
            }

            return text.ToString();
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
            private readonly Func<DataTable, bool> available;
            private readonly Func<IList<DataRow>, int> count;

            internal KpiCard(string title, Func<DataTable, bool> available, Func<IList<DataRow>, int> count)
            {
                this.title = title ?? string.Empty;
                this.available = available;
                this.count = count;
            }

            internal string Title
            {
                get { return this.title; }
            }

            internal string ValueText(DataTable table, IList<DataRow> rows)
            {
                if (table == null || !this.available(table))
                {
                    return NoValueText;
                }

                return this.count(rows).ToString("N0", CultureInfo.InvariantCulture);
            }
        }

        internal sealed class ColumnBand
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

            internal void Frozen()
            {
                this.frozen = true;
            }
        }
    }
}
