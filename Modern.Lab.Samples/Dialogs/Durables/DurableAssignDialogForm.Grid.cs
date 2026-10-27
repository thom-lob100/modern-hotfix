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
        // Lot 후보는 GetLotList 응답에서 Released · NotOnHold · DURABLE_ID 빈 것만 남긴다 — FOUP 은 Wafer Lot, TRAY 는 Chip · Lamella Lot.
        // 응답에 QTY 가 있으면 빈 자리보다 많은 Lot 도 뺀다(없으면 Lot 을 고른 뒤 단위 수로 판정).
        // 단위 표(plan)는 Lot 의 단위마다 한 줄 — Id 는 잠그고 Slot(Lamella 는 Finger Id 도)만 표에서 고친다.
        // 기본값은 지속재의 빈 자리를 순서대로(1, 2, 3 … 찬 자리는 건너뜀, Lamella 는 1A, 1B …) 준다. 용량을 모르면 Lot 쪽 SLOT_NO · FINGER_ID.
        // 자리 수는 FOUP CAPA · Chip STUB_CAPA · Lamella LCC_CAPA 이고, 찬 자리는 내용물 표(GetDurableSlotList)의 같은 종류 행이다.
        // 문제 행은 경고색 배경 + 표식 "Fix" 로 보이고, 안내 문구의 Id · 자리 · 건수는 ** 강조(액센트색)로 쓴다.
        private const string DurableIdColumn = "DURABLE_ID";
        private const string FingerIdColumn = "FINGER_ID";
        private const string IssueMarker = "Fix";
        private const string Emphasis = Modern.Lab.Controls.Wpf.Display.ModernMessageDialogControl.EmphasisMarker;
        private static readonly string[] FingerIds = { "A", "B", "C", "D", "E" };

        private static string[] CandidateTypes(DataRow durable)
        {
            return Schemas.Durable.DurableTyp.Is(durable, Schemas.Durable.DurableTyp.Foup)
                    ? new string[] { Schemas.Lot.SubProdTyp.Wafer.Value }
                    : new string[] { Schemas.Lot.SubProdTyp.Chip.Value, Schemas.Lot.SubProdTyp.Lamella.Value };
        }

        private static DataTable Candidates(DataTable lots, string[] types, DataRow durable, DataTable contents)
        {
            DataTable candidates = lots.Clone();
            bool hasQty = lots.Columns.Contains(Schemas.Lot.Qty.Name);

            foreach (DataRow row in lots.Rows)
            {
                string subProdTyp = Schemas.Lot.SubProdTyp.Text(row);

                if (Array.IndexOf(types, subProdTyp) < 0)
                {
                    continue;
                }

                if (!Schemas.Lot.LotStatTyp.Is(row, Schemas.Lot.LotStatTyp.Released)
                        || !Schemas.Lot.LotHoldStatCd.Is(row, Schemas.Lot.LotHoldStatCd.NotOnHold))
                {
                    continue;
                }

                if (!lots.Columns.Contains(DurableIdColumn)
                        || TableHelper.CellText(row, DurableIdColumn).Trim().Length > 0)
                {
                    continue;
                }

                if (hasQty)
                {
                    int capacity = Capacity(durable, subProdTyp);
                    int qty;

                    if (capacity > 0
                            && int.TryParse(Schemas.Lot.Qty.Text(row).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out qty)
                            && qty > FreePositions(capacity, subProdTyp, Occupied(contents, subProdTyp)).Count)
                    {
                        continue;
                    }
                }

                candidates.ImportRow(row);
            }

            return candidates;
        }

        private static Dictionary<string, string> Occupied(DataTable contents, string subProdTyp)
        {
            Dictionary<string, string> occupied = new Dictionary<string, string>(StringComparer.Ordinal);

            if (contents == null)
            {
                return occupied;
            }

            bool lamella = subProdTyp == Schemas.Lot.SubProdTyp.Lamella.Value;

            foreach (DataRow row in contents.Rows)
            {
                if (contents.Columns.Contains(Schemas.DurableWafer.SubProdTyp.Name)
                        && Schemas.DurableWafer.SubProdTyp.Text(row).Trim() != subProdTyp)
                {
                    continue;
                }

                int slot;

                if (!int.TryParse(Schemas.DurableWafer.SlotNo.Text(row).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out slot))
                {
                    continue;
                }

                string finger = lamella && contents.Columns.Contains(FingerIdColumn)
                        ? TableHelper.CellText(row, FingerIdColumn).Trim().ToUpperInvariant()
                        : string.Empty;
                string position = Position(slot, finger, lamella);

                if (!occupied.ContainsKey(position))
                {
                    occupied[position] = Schemas.DurableWafer.WfId.Text(row).Trim();
                }
            }

            return occupied;
        }

        private static List<string[]> FreePositions(int capacity, string subProdTyp, Dictionary<string, string> occupied)
        {
            List<string[]> free = new List<string[]>();
            bool lamella = subProdTyp == Schemas.Lot.SubProdTyp.Lamella.Value;

            for (int slot = 1; slot <= capacity; slot++)
            {
                if (!lamella)
                {
                    if (!occupied.ContainsKey(Position(slot, string.Empty, false)))
                    {
                        free.Add(new string[] { slot.ToString(CultureInfo.InvariantCulture), string.Empty });
                    }

                    continue;
                }

                foreach (string finger in FingerIds)
                {
                    if (!occupied.ContainsKey(Position(slot, finger, true)))
                    {
                        free.Add(new string[] { slot.ToString(CultureInfo.InvariantCulture), finger });
                    }
                }
            }

            return free;
        }

        private static string Position(int slot, string finger, bool lamella)
        {
            return slot.ToString(CultureInfo.InvariantCulture) + (lamella ? "|" + finger : string.Empty);
        }

        private static DataTable BuildPlan(DataTable units, string subProdTyp, List<string[]> freePositions)
        {
            DataTable plan = new DataTable();
            plan.Locale = CultureInfo.InvariantCulture;
            plan.Columns.Add(Schemas.Wafer.WfId.Name, typeof(string));
            plan.Columns.Add(Schemas.Wafer.SlotNo.Name, typeof(string));
            plan.Columns.Add(FingerIdColumn, typeof(string));
            plan.Columns.Add(Schemas.Wafer.SubProdTyp.Name, typeof(string));

            if (units == null)
            {
                return plan;
            }

            int next = 0;

            foreach (DataRow unit in units.Rows)
            {
                string wfId = TableHelper.CellText(unit, Schemas.Wafer.WfId.Name).Trim();

                if (wfId.Length == 0)
                {
                    continue;
                }

                string slot;
                string finger;

                if (freePositions == null)
                {
                    slot = TableHelper.CellText(unit, Schemas.Wafer.SlotNo.Name).Trim();
                    finger = units.Columns.Contains(FingerIdColumn) ? TableHelper.CellText(unit, FingerIdColumn).Trim() : string.Empty;
                }
                else if (next < freePositions.Count)
                {
                    slot = freePositions[next][0];
                    finger = freePositions[next][1];
                    next++;
                }
                else
                {
                    slot = string.Empty;
                    finger = string.Empty;
                }

                plan.Rows.Add(wfId, slot, finger, subProdTyp);
            }

            return plan;
        }

        private static ModernDataGridColumn[] PlanColumns(DataTable plan, string subProdTyp)
        {
            GridColumns columns = Schemas.Wafer.Columns(plan)
                    .Column(Schemas.Wafer.WfId.Name).Caption(IdCaption(subProdTyp))
                    .Column(Schemas.Wafer.SlotNo.Name).Width(GridWidths.Status)
                    .Column(FingerIdColumn).Caption("Finger Id").Width(GridWidths.Status)
                    .Hide(Schemas.Wafer.SubProdTyp.Name);

            if (subProdTyp != Schemas.Lot.SubProdTyp.Lamella.Value)
            {
                columns.Hide(FingerIdColumn);
            }

            ModernDataGridColumn[] built = columns.ToArray();

            foreach (ModernDataGridColumn column in built)
            {
                column.ReadOnly = column.DataPropertyName != Schemas.Wafer.SlotNo.Name
                        && column.DataPropertyName != FingerIdColumn;
            }

            return built;
        }

        private static string IdCaption(string subProdTyp)
        {
            if (subProdTyp == Schemas.Lot.SubProdTyp.Chip.Value)
            {
                return "Chip Id";
            }

            return subProdTyp == Schemas.Lot.SubProdTyp.Lamella.Value ? "Lamella Id" : "Wafer Id";
        }

        private static string UnitsCaption(string subProdTyp)
        {
            if (subProdTyp == Schemas.Lot.SubProdTyp.Chip.Value)
            {
                return "{0:N0} chips";
            }

            return subProdTyp == Schemas.Lot.SubProdTyp.Lamella.Value ? "{0:N0} lamellas" : "{0:N0} wafers";
        }

        private static string UnitWord(string subProdTyp, int count)
        {
            string word = "wafer";

            if (subProdTyp == Schemas.Lot.SubProdTyp.Chip.Value)
            {
                word = "chip";
            }
            else if (subProdTyp == Schemas.Lot.SubProdTyp.Lamella.Value)
            {
                word = "lamella";
            }

            return count == 1 ? word : word + "s";
        }

        private static int Capacity(DataRow durable, string subProdTyp)
        {
            Field field = Schemas.Durable.Capa;

            if (subProdTyp == Schemas.Lot.SubProdTyp.Chip.Value)
            {
                field = Schemas.Durable.StubCapa;
            }
            else if (subProdTyp == Schemas.Lot.SubProdTyp.Lamella.Value)
            {
                field = Schemas.Durable.LccCapa;
            }

            int capacity;

            return int.TryParse(TableHelper.CellText(durable, field.Name).Trim(), NumberStyles.Integer,
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

        private static string PlaceText(int slot, string finger, bool lamella)
        {
            return "slot " + Em(slot) + (lamella ? " finger " + Em(finger) : string.Empty);
        }

        private static List<string> PlanIssues(
                DataTable plan, string subProdTyp, int capacity, Dictionary<string, string> occupied, int freeCount,
                HashSet<string> issueRows)
        {
            List<string> issues = new List<string>();
            issueRows.Clear();

            if (plan == null || plan.Rows.Count == 0)
            {
                issues.Add("The lot has no unit to assign.");
                return issues;
            }

            bool lamella = subProdTyp == Schemas.Lot.SubProdTyp.Lamella.Value;

            if (capacity > 0 && plan.Rows.Count > freeCount)
            {
                issues.Add(Em(plan.Rows.Count) + " " + UnitWord(subProdTyp, plan.Rows.Count) + " need " + Em(plan.Rows.Count)
                        + " free slots, but the durable has only " + Em(freeCount) + ".");
            }

            Dictionary<string, string> taken = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (DataRow row in plan.Rows)
            {
                string wfId = TableHelper.CellText(row, Schemas.Wafer.WfId.Name).Trim();
                string slotText = TableHelper.CellText(row, Schemas.Wafer.SlotNo.Name).Trim();
                int slot;

                if (!int.TryParse(slotText, NumberStyles.Integer, CultureInfo.InvariantCulture, out slot) || slot < 1)
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

                string finger = TableHelper.CellText(row, FingerIdColumn).Trim().ToUpperInvariant();

                if (lamella && Array.IndexOf(FingerIds, finger) < 0)
                {
                    issues.Add(Em(wfId) + ": enter a finger id (" + Em(string.Join(", ", FingerIds)) + ").");
                    issueRows.Add(wfId);
                    continue;
                }

                string position = Position(slot, finger, lamella);
                string holder;

                if (occupied.TryGetValue(position, out holder))
                {
                    issues.Add(Em(wfId) + ": " + PlaceText(slot, finger, lamella) + " is already taken by " + Em(holder) + ".");
                    issueRows.Add(wfId);
                    continue;
                }

                string other;

                if (taken.TryGetValue(position, out other))
                {
                    issues.Add(Em(wfId) + " and " + Em(other) + " use the same " + PlaceText(slot, finger, lamella) + ".");
                    issueRows.Add(wfId);
                    issueRows.Add(other);
                    continue;
                }

                taken[position] = wfId;
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

        private static DataTable NormalizedPlan(DataTable plan, string subProdTyp)
        {
            DataTable result = plan.Copy();
            bool lamella = subProdTyp == Schemas.Lot.SubProdTyp.Lamella.Value;

            foreach (DataRow row in result.Rows)
            {
                int slot = int.Parse(TableHelper.CellText(row, Schemas.Wafer.SlotNo.Name).Trim(), CultureInfo.InvariantCulture);
                row[Schemas.Wafer.SlotNo.Name] = slot.ToString(CultureInfo.InvariantCulture);
                row[FingerIdColumn] = lamella
                        ? TableHelper.CellText(row, FingerIdColumn).Trim().ToUpperInvariant()
                        : string.Empty;
            }

            return result;
        }
    }
}
