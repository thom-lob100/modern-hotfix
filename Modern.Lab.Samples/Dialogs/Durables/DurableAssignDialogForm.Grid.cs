using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Samples
{
    public partial class DurableAssignDialogForm
    {
        // Durable 정보 카드의 항목은 DurableDialogHelpers.SourceFields 가 정한다(Deassign 창과 같은 항목).
        // Assign 은 FOUP 에만 열리므로 이 창은 Wafer Lot 만 다룬다.
        // Lot 후보는 GetLotList 응답에서 Released · NotOnHold · DURABLE_ID 빈 것만 남긴다.
        // 응답에 QTY 가 있으면 빈 자리보다 많은 Lot 도 뺀다(없으면 Lot 을 고른 뒤 단위 수로 판정).
        // 단위 표(plan)는 Lot 의 웨이퍼마다 한 줄 — Id 는 잠그고 Slot 만 표에서 고친다.
        // 기본값은 FOUP 의 빈 자리를 순서대로(1, 2, 3 … 찬 자리는 건너뜀) 준다. 용량(CAPA)을 모르면 Lot 쪽 SLOT_NO.
        // 찬 자리는 내용물 표(GetDurableSlotList)의 행이다.
        // 문제 행은 경고색 배경 + 표식 "Fix" 로 보이고, 안내 문구의 Id · 자리 · 건수는 ** 강조(액센트색)로 쓴다.
        private const string DurableIdColumn = "DURABLE_ID";
        private const string IssueMarker = "Fix";
        private const string Emphasis = Modern.Lab.Controls.Wpf.Display.ModernMessageDialogControl.EmphasisMarker;

        private static DataTable Candidates(DataTable lots, DataRow durable, DataTable contents)
        {
            DataTable candidates = lots.Clone();
            bool hasQty = lots.Columns.Contains(Schemas.Lot.Qty.Name);
            int capacity = Capacity(durable);
            int free = capacity > 0 ? FreeSlots(capacity, Occupied(contents)).Count : -1;

            foreach (DataRow row in lots.Rows)
            {
                if (!Schemas.Lot.SubProdTyp.Is(row, Schemas.Lot.SubProdTyp.Wafer))
                {
                    continue;
                }

                if (!Schemas.Lot.LotStatTyp.Is(row, Schemas.Lot.LotStatTyp.Released)
                        || !Schemas.Lot.LotHoldStatCd.Is(row, Schemas.Lot.LotHoldStatCd.NotOnHold))
                {
                    continue;
                }

                if (!lots.Columns.Contains(DurableIdColumn)
                        || TableHelper.CellText(row, DurableIdColumn).Length > 0)
                {
                    continue;
                }

                int qty;

                if (hasQty && free >= 0
                        && int.TryParse(Schemas.Lot.Qty.Text(row), NumberStyles.Integer, CultureInfo.InvariantCulture, out qty)
                        && qty > free)
                {
                    continue;
                }

                candidates.ImportRow(row);
            }

            return candidates;
        }

        private static Dictionary<int, string> Occupied(DataTable contents)
        {
            Dictionary<int, string> occupied = new Dictionary<int, string>();

            if (contents == null)
            {
                return occupied;
            }

            foreach (DataRow row in contents.Rows)
            {
                int slot;

                if (!int.TryParse(Schemas.DurableWafer.SlotNo.Text(row), NumberStyles.Integer, CultureInfo.InvariantCulture, out slot))
                {
                    continue;
                }

                if (!occupied.ContainsKey(slot))
                {
                    occupied[slot] = Schemas.DurableWafer.WfId.Text(row);
                }
            }

            return occupied;
        }

        private static List<int> FreeSlots(int capacity, Dictionary<int, string> occupied)
        {
            List<int> free = new List<int>();

            for (int slot = 1; slot <= capacity; slot++)
            {
                if (!occupied.ContainsKey(slot))
                {
                    free.Add(slot);
                }
            }

            return free;
        }

        private static DataTable BuildPlan(DataTable units, List<int> freeSlots)
        {
            DataTable plan = new DataTable();
            plan.Locale = CultureInfo.InvariantCulture;
            plan.Columns.Add(Schemas.Wafer.WfId.Name, typeof(string));
            plan.Columns.Add(Schemas.Wafer.SlotNo.Name, typeof(string));

            if (units == null)
            {
                return plan;
            }

            int next = 0;

            foreach (DataRow unit in units.Rows)
            {
                string wfId = TableHelper.CellText(unit, Schemas.Wafer.WfId.Name);

                if (wfId.Length == 0)
                {
                    continue;
                }

                string slot;

                if (freeSlots == null)
                {
                    slot = TableHelper.CellText(unit, Schemas.Wafer.SlotNo.Name);
                }
                else if (next < freeSlots.Count)
                {
                    slot = freeSlots[next].ToString(CultureInfo.InvariantCulture);
                    next++;
                }
                else
                {
                    slot = string.Empty;
                }

                plan.Rows.Add(wfId, slot);
            }

            return plan;
        }

        private static ModernDataGridColumn[] PlanColumns(DataTable plan)
        {
            ModernDataGridColumn[] built = Schemas.Wafer.Columns(plan)
                    .Column(Schemas.Wafer.SlotNo.Name).Width(GridWidths.Status)
                    .ToArray();

            foreach (ModernDataGridColumn column in built)
            {
                column.ReadOnly = column.DataPropertyName != Schemas.Wafer.SlotNo.Name;
            }

            return built;
        }

        private static string WaferWord(int count)
        {
            return count == 1 ? "wafer" : "wafers";
        }

        private static int Capacity(DataRow durable)
        {
            int capacity;

            return int.TryParse(TableHelper.CellText(durable, Schemas.Durable.Capa.Name), NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out capacity) ? capacity : 0;
        }

        private static string Em(string text)
        {
            return Emphasis + text + Emphasis;
        }

        private static string Em(int number)
        {
            return Em(number.ToString(CultureInfo.InvariantCulture));
        }

        private static List<string> PlanIssues(
                DataTable plan, int capacity, Dictionary<int, string> occupied, int freeCount, HashSet<string> issueRows)
        {
            List<string> issues = new List<string>();
            issueRows.Clear();

            if (plan == null || plan.Rows.Count == 0)
            {
                issues.Add("The lot has no wafer to assign.");
                return issues;
            }

            if (capacity > 0 && plan.Rows.Count > freeCount)
            {
                issues.Add(Em(plan.Rows.Count) + " " + WaferWord(plan.Rows.Count) + " need " + Em(plan.Rows.Count)
                        + " free slots, but the durable has only " + Em(freeCount) + ".");
            }

            Dictionary<int, string> taken = new Dictionary<int, string>();

            foreach (DataRow row in plan.Rows)
            {
                string wfId = TableHelper.CellText(row, Schemas.Wafer.WfId.Name);
                int slot;

                if (!int.TryParse(TableHelper.CellText(row, Schemas.Wafer.SlotNo.Name), NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out slot) || slot < 1)
                {
                    issues.Add(Em(wfId) + ": enter a slot number.");
                    issueRows.Add(wfId);
                    continue;
                }

                if (capacity > 0 && slot > capacity)
                {
                    issues.Add(Em(wfId) + ": slot " + Em(slot) + " is over the capacity " + Em(capacity) + ".");
                    issueRows.Add(wfId);
                    continue;
                }

                string holder;

                if (occupied.TryGetValue(slot, out holder))
                {
                    issues.Add(Em(wfId) + ": slot " + Em(slot) + " is already taken by " + Em(holder) + ".");
                    issueRows.Add(wfId);
                    continue;
                }

                string other;

                if (taken.TryGetValue(slot, out other))
                {
                    issues.Add(Em(wfId) + " and " + Em(other) + " use the same slot " + Em(slot) + ".");
                    issueRows.Add(wfId);
                    issueRows.Add(other);
                    continue;
                }

                taken[slot] = wfId;
            }

            return issues;
        }

        private static string IssueSummary(List<string> issues, int limit)
        {
            List<string> lines = new List<string>();

            for (int index = 0; index < issues.Count && index < limit; index++)
            {
                lines.Add(issues[index]);
            }

            if (issues.Count > limit)
            {
                lines.Add("… and " + Em(issues.Count - limit) + " more.");
            }

            return string.Join(Environment.NewLine, lines);
        }

        private static string PlainText(string emphasized)
        {
            return Modern.Lab.Controls.Wpf.Display.ModernMessageDialogControl.StripEmphasis(emphasized);
        }

        private static string IssueRowColor()
        {
            return System.Drawing.ColorTranslator.ToHtml(
                    Modern.Lab.Theming.ModernTokenColors.Get("Brush.WarningBackground", System.Drawing.Color.FromArgb(255, 251, 235)));
        }

        private static string JoinColumn(DataTable plan, string columnName)
        {
            List<string> values = new List<string>();

            foreach (DataRow row in plan.Rows)
            {
                string text = TableHelper.CellText(row, columnName);
                int slot;

                values.Add(columnName == Schemas.Wafer.SlotNo.Name
                        && int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out slot)
                        ? slot.ToString(CultureInfo.InvariantCulture)
                        : text);
            }

            return string.Join(",", values);
        }
    }
}
