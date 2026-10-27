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
        // 단위 표(plan)는 Lot 의 단위마다 한 줄 — Id 는 잠그고 Slot(Lamella 는 Finger Id 도)만 표에서 고친다.
        // 기본값은 Lot 쪽 SLOT_NO · FINGER_ID 이고, 자리 수는 FOUP CAPA · Chip STUB_CAPA · Lamella LCC_CAPA 다.
        private const string DurableIdColumn = "DURABLE_ID";
        private const string FingerIdColumn = "FINGER_ID";
        private static readonly string[] FingerIds = { "A", "B", "C", "D", "E" };

        private static string[] CandidateTypes(DataRow durable)
        {
            return Schemas.Durable.DurableTyp.Is(durable, Schemas.Durable.DurableTyp.Foup)
                    ? new string[] { Schemas.Lot.SubProdTyp.Wafer.Value }
                    : new string[] { Schemas.Lot.SubProdTyp.Chip.Value, Schemas.Lot.SubProdTyp.Lamella.Value };
        }

        private static DataTable Candidates(DataTable lots, string[] types)
        {
            DataTable candidates = lots.Clone();

            foreach (DataRow row in lots.Rows)
            {
                if (Array.IndexOf(types, Schemas.Lot.SubProdTyp.Text(row)) < 0)
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

                candidates.ImportRow(row);
            }

            return candidates;
        }

        private static DataTable BuildPlan(DataTable units, string subProdTyp)
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

            foreach (DataRow unit in units.Rows)
            {
                string wfId = TableHelper.CellText(unit, Schemas.Wafer.WfId.Name).Trim();

                if (wfId.Length == 0)
                {
                    continue;
                }

                plan.Rows.Add(
                        wfId,
                        TableHelper.CellText(unit, Schemas.Wafer.SlotNo.Name).Trim(),
                        units.Columns.Contains(FingerIdColumn) ? TableHelper.CellText(unit, FingerIdColumn).Trim() : string.Empty,
                        subProdTyp);
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

        private static string PlanProblem(DataTable plan, string subProdTyp, int capacity)
        {
            if (plan == null || plan.Rows.Count == 0)
            {
                return "The lot has no unit to assign.";
            }

            bool lamella = subProdTyp == Schemas.Lot.SubProdTyp.Lamella.Value;
            Dictionary<string, string> taken = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (DataRow row in plan.Rows)
            {
                string wfId = TableHelper.CellText(row, Schemas.Wafer.WfId.Name).Trim();
                string slotText = TableHelper.CellText(row, Schemas.Wafer.SlotNo.Name).Trim();
                int slot;

                if (!int.TryParse(slotText, NumberStyles.Integer, CultureInfo.InvariantCulture, out slot) || slot < 1)
                {
                    return wfId + ": enter a slot number.";
                }

                if (capacity > 0 && slot > capacity)
                {
                    return wfId + ": slot " + slot + " is over the capacity " + capacity + ".";
                }

                string finger = TableHelper.CellText(row, FingerIdColumn).Trim().ToUpperInvariant();

                if (lamella && Array.IndexOf(FingerIds, finger) < 0)
                {
                    return wfId + ": enter a finger id (" + string.Join(", ", FingerIds) + ").";
                }

                string position = slot.ToString(CultureInfo.InvariantCulture) + (lamella ? "|" + finger : string.Empty);
                string other;

                if (taken.TryGetValue(position, out other))
                {
                    return wfId + " and " + other + " use the same " + (lamella ? "slot and finger." : "slot.");
                }

                taken[position] = wfId;
            }

            return null;
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
