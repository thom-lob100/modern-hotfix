using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Hosting.Reporting
{
    /// <summary>
    /// 코드표 한 벌 — 공통코드 응답(`TYP_VAL` · `DESCRIPTION` · `SORT_NO` · `USE_YN`)을 순서 있는 (코드, 라벨) 목록으로 푼다.
    /// 흐름의 단계 순서 · 이름과 가지의 종류는 리포트가 적지 않고 이 표에서 온다(2026-11-07). `USE_YN` 이 `N` 인 줄은 뺀다.
    /// </summary>
    internal sealed class ReportCodeTable
    {
        internal static readonly ReportCodeTable Empty = new ReportCodeTable();

        private readonly List<string> values = new List<string>();
        private readonly List<string> labels = new List<string>();

        private ReportCodeTable()
        {
        }

        internal int Count
        {
            get { return this.values.Count; }
        }

        internal string ValueAt(int index)
        {
            return this.values[index];
        }

        internal string LabelAt(int index)
        {
            return this.labels[index];
        }

        internal int IndexOf(string value)
        {
            string wanted = (value ?? string.Empty).Trim();

            if (wanted.Length == 0)
            {
                return -1;
            }

            for (int index = 0; index < this.values.Count; index++)
            {
                if (string.Equals(this.values[index], wanted, StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            return -1;
        }

        internal static ReportCodeTable Parse(DataTable table)
        {
            ReportCodeTable codes = new ReportCodeTable();
            string valueColumn = Schemas.CommonTypeDet.TypVal.Name;
            string labelColumn = Schemas.CommonTypeDet.Description.Name;
            string sortColumn = Schemas.CommonTypeDet.SortNo.Name;
            string useColumn = Schemas.CommonTypeDet.UseYn.Name;

            if (table == null || !table.Columns.Contains(valueColumn))
            {
                return codes;
            }

            List<KeyValuePair<double, int>> order = new List<KeyValuePair<double, int>>();
            List<string> rawValues = new List<string>();
            List<string> rawLabels = new List<string>();

            foreach (DataRow row in table.Rows)
            {
                string value = TableHelper.CellText(row, valueColumn).Trim();

                if (value.Length == 0)
                {
                    continue;
                }

                if (table.Columns.Contains(useColumn)
                        && string.Equals(TableHelper.CellText(row, useColumn).Trim(), "N", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string label = table.Columns.Contains(labelColumn) ? TableHelper.CellText(row, labelColumn).Trim() : string.Empty;
                double sort;

                if (!table.Columns.Contains(sortColumn)
                        || !double.TryParse(TableHelper.CellText(row, sortColumn), NumberStyles.Any, CultureInfo.InvariantCulture, out sort))
                {
                    sort = double.MaxValue;
                }

                order.Add(new KeyValuePair<double, int>(sort, rawValues.Count));
                rawValues.Add(value);
                rawLabels.Add(label.Length == 0 ? GridCaptionCatalog.Humanize(value) : label);
            }

            // 정렬 번호 순, 같으면 응답 순 — 안정 정렬을 위해 (번호, 응답 순) 짝으로 비교한다.
            order.Sort(delegate(KeyValuePair<double, int> left, KeyValuePair<double, int> right)
            {
                int bySort = left.Key.CompareTo(right.Key);
                return bySort != 0 ? bySort : left.Value.CompareTo(right.Value);
            });

            foreach (KeyValuePair<double, int> entry in order)
            {
                if (codes.IndexOf(rawValues[entry.Value]) >= 0)
                {
                    continue;
                }

                codes.values.Add(rawValues[entry.Value]);
                codes.labels.Add(rawLabels[entry.Value]);
            }

            return codes;
        }
    }
}
