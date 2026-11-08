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
    /// 가지(Hold · Rework · Scrap · Return · Swap …) — 종류와 순서는 코드표(<see cref="CodeType"/>)가 정하고, 행이 그 가지에 있는지는
    /// 서버가 내려준 <c>&lt;코드&gt;_YN</c>(`Y`)이 말한다. <c>&lt;코드&gt;_TM</c>(시각) · <c>&lt;코드&gt;_CNT</c>(수)가 있으면 배지 글자에 붙는다.
    /// 틀이 가지마다 배지 칸 <c>&lt;코드&gt;_BRANCH</c>(Branch 묶음)와 인디케이터 띠의 배지를 만들고, KPI 카드도 이 순서로 선다(2026-11-07).
    /// 색은 모두 노랑(Warning)이다 — 빨강은 FAIL 전용이다.
    /// </summary>
    public sealed class ReportBranches
    {
        private const string BadgeSuffix = "_BRANCH";
        private const string YesSuffix = "_YN";
        private const string TimeSuffix = "_TM";
        private const string CountSuffix = "_CNT";
        private const string Yes = "Y";

        private readonly string codeType;
        private ReportCodeTable codes = ReportCodeTable.Empty;

        internal ReportBranches(string codeType)
        {
            this.codeType = (codeType ?? string.Empty).Trim();
        }

        public string CodeType
        {
            get { return this.codeType; }
        }

        internal bool HasCodes
        {
            get { return this.codes.Count > 0; }
        }

        internal int Count
        {
            get { return this.codes.Count; }
        }

        internal string[] Columns
        {
            get
            {
                string[] names = new string[this.codes.Count];

                for (int index = 0; index < names.Length; index++)
                {
                    names[index] = this.BadgeColumn(index);
                }

                return names;
            }
        }

        internal void SetCodes(ReportCodeTable table)
        {
            this.codes = table ?? ReportCodeTable.Empty;
        }

        internal string Label(int index)
        {
            return this.codes.LabelAt(index);
        }

        internal bool IsOn(DataRow row, int index)
        {
            return string.Equals(ReportFlow.CellOf(row, this.codes.ValueAt(index) + YesSuffix), Yes, StringComparison.OrdinalIgnoreCase);
        }

        internal int CountOn(IList<DataRow> rows, int index)
        {
            int count = 0;

            foreach (DataRow row in rows)
            {
                if (this.IsOn(row, index))
                {
                    count++;
                }
            }

            return count;
        }

        internal bool IsAvailable(DataTable table, int index)
        {
            return table != null && table.Columns.Contains(this.codes.ValueAt(index) + YesSuffix);
        }

        /// <summary>인디케이터 띠의 배지 글자 — 가지에 있으면 라벨(수가 있으면 뒤에 붙음), 아니면 빈 글자(숨김).</summary>
        internal string BadgeText(DataRow row, int index)
        {
            if (row == null || !this.IsOn(row, index))
            {
                return string.Empty;
            }

            string count = this.CountText(row, index);
            return count.Length == 0 ? this.Label(index) : this.Label(index) + " " + count;
        }

        /// <summary>응답에서 가지가 가져가는 칸 — 엔티티 묶음이 다시 싣지 않는다.</summary>
        internal IEnumerable<string> OwnedColumns(DataTable table)
        {
            List<string> names = new List<string>();

            for (int index = 0; index < this.codes.Count; index++)
            {
                string code = this.codes.ValueAt(index);
                AddIfPresent(names, table, code + YesSuffix);
                AddIfPresent(names, table, code + TimeSuffix);
                AddIfPresent(names, table, code + CountSuffix);
            }

            return names;
        }

        internal IEnumerable<string> HiddenColumns(DataTable table)
        {
            List<string> names = new List<string>(this.OwnedColumns(table));

            for (int index = 0; index < this.codes.Count; index++)
            {
                names.Add(this.SemanticColumn(index));
            }

            return names;
        }

        internal void Prepare(DataTable rows)
        {
            if (rows == null)
            {
                return;
            }

            for (int index = 0; index < this.codes.Count; index++)
            {
                TableHelper.EnsureColumn(rows, this.BadgeColumn(index), typeof(string));
                TableHelper.EnsureColumn(rows, this.SemanticColumn(index), typeof(string));
            }

            foreach (DataRow row in rows.Rows)
            {
                for (int index = 0; index < this.codes.Count; index++)
                {
                    bool on = this.IsOn(row, index);
                    row[this.BadgeColumn(index)] = on ? this.CellText(row, index) : string.Empty;
                    row[this.SemanticColumn(index)] = on ? ReportFlow.WarningSemantic : string.Empty;
                }
            }
        }

        internal void Decorate(GridColumns columns)
        {
            for (int index = 0; index < this.codes.Count; index++)
            {
                string semantic = this.SemanticColumn(index);
                columns.Column(this.BadgeColumn(index)).Caption(this.Label(index)).TopLabel(string.Empty).Badge();
                ReportDefinition.Each(columns, new[] { this.BadgeColumn(index) }, column => column.BadgeSemanticMember = semantic);
            }
        }

        private string CellText(DataRow row, int index)
        {
            List<string> parts = new List<string>();
            parts.Add(this.Label(index));

            string count = this.CountText(row, index);

            if (count.Length > 0)
            {
                parts.Add(count);
            }

            string time = ReportFlow.CellOf(row, this.codes.ValueAt(index) + TimeSuffix);

            if (time.Length > 0)
            {
                parts.Add(ReportFlow.ShortDate(time));
            }

            return string.Join(" ", parts.ToArray());
        }

        private string CountText(DataRow row, int index)
        {
            int count;

            if (!int.TryParse(ReportFlow.CellOf(row, this.codes.ValueAt(index) + CountSuffix), NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out count) || count < 1)
            {
                return string.Empty;
            }

            return count.ToString(CultureInfo.InvariantCulture);
        }

        private string BadgeColumn(int index)
        {
            return this.codes.ValueAt(index) + BadgeSuffix;
        }

        private string SemanticColumn(int index)
        {
            return this.BadgeColumn(index) + ReportFlow.SemanticSuffix;
        }

        private static void AddIfPresent(List<string> names, DataTable table, string column)
        {
            if (table != null && table.Columns.Contains(column))
            {
                names.Add(column);
            }
        }
    }
}
