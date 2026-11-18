using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Modern.Lab.Data;
using Modern.Lab.Samples.Contracts;

using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting.Schema;
namespace Modern.Lab.Samples.Services
{
    internal static class CarrierEditPresenter
    {
        internal const string ActionMove = "Move";
        internal const string MapKeyColumn = "MapKey";

        internal static readonly string[] ActionKeys = { ActionMove };
        internal const string LabelColumn = "LABEL";
        internal const string StatColorColumn = "STAT_COLOR";
        internal const string OriginalSubProdTypColumn = "ORG_SUB_PROD_TYP";

        internal static readonly string[] ScreenColumns = { LabelColumn, StatColorColumn, MapKeyColumn, OriginalSubProdTypColumn };

        internal const string PlanSourceSlotColumn = "SOURCE_SLOT";
        internal const string PlanSourceFingerColumn = "SOURCE_FINGER";
        internal const string PlanTargetSlotColumn = "TARGET_SLOT";
        internal const string PlanTargetFingerColumn = "TARGET_FINGER";

        internal static DataTable BuildMovePlan(DataTable staged, IDictionary<string, string> sourceKeyByTargetKey)
        {
            DataTable plan = new DataTable();
            plan.Locale = CultureInfo.InvariantCulture;
            plan.Columns.Add(Schemas.DurableWafer.SubProdTyp.Name, typeof(string));
            plan.Columns.Add(PlanSourceSlotColumn, typeof(string));
            plan.Columns.Add(PlanSourceFingerColumn, typeof(string));
            plan.Columns.Add(Schemas.DurableWafer.WfId.Name, typeof(string));
            plan.Columns.Add(Schemas.DurableWafer.LotId.Name, typeof(string));
            plan.Columns.Add(PlanTargetSlotColumn, typeof(string));
            plan.Columns.Add(PlanTargetFingerColumn, typeof(string));

            if (staged == null)
            {
                return plan;
            }

            Dictionary<string, string> targetBySource = new Dictionary<string, string>(StringComparer.Ordinal);

            if (sourceKeyByTargetKey != null)
            {
                foreach (KeyValuePair<string, string> entry in sourceKeyByTargetKey)
                {
                    string[] parts = (entry.Key ?? string.Empty).Split('|');

                    if (parts.Length > 1 && !targetBySource.ContainsKey(entry.Value ?? string.Empty))
                    {
                        targetBySource[entry.Value ?? string.Empty] = parts[1];
                    }
                }
            }

            foreach (DataRow row in staged.Rows)
            {
                string waferId = TableHelper.CellText(row, Schemas.DurableWafer.WfId.Name).Trim();

                if (waferId.Length == 0)
                {
                    continue;
                }

                string subProdTyp = TableHelper.CellText(row, Schemas.DurableWafer.SubProdTyp.Name).Trim();
                string sourceSlot = TableHelper.CellText(row, Schemas.DurableWafer.SlotNo.Name).Trim();
                bool lamella = subProdTyp == Schemas.DurableLamella.SubProdTyp.Lamella.Value;
                string finger = lamella ? TableHelper.CellText(row, Schemas.DurableLamella.FingerId.Name).Trim() : string.Empty;
                string targetSlot;

                if (!targetBySource.TryGetValue(subProdTyp + "|" + sourceSlot, out targetSlot))
                {
                    targetSlot = string.Empty;
                }

                plan.Rows.Add(
                        subProdTyp, sourceSlot, finger, waferId,
                        TableHelper.CellText(row, Schemas.DurableWafer.LotId.Name).Trim(),
                        targetSlot, lamella && targetSlot.Length > 0 ? finger : string.Empty);
            }

            return plan;
        }

        internal static bool PlanIsComplete(DataTable plan)
        {
            if (plan == null || plan.Rows.Count == 0)
            {
                return false;
            }

            foreach (DataRow row in plan.Rows)
            {
                if (TableHelper.CellText(row, PlanTargetSlotColumn).Trim().Length == 0)
                {
                    return false;
                }
            }

            return true;
        }

        internal static DataTable ChangedRows(DataTable plan)
        {
            DataTable changed = plan == null ? new DataTable() : plan.Clone();

            if (plan == null)
            {
                return changed;
            }

            foreach (DataRow row in plan.Rows)
            {
                if (!string.Equals(
                        TableHelper.CellText(row, PlanSourceSlotColumn).Trim(),
                        TableHelper.CellText(row, PlanTargetSlotColumn).Trim(),
                        StringComparison.Ordinal))
                {
                    changed.ImportRow(row);
                }
            }

            return changed;
        }

        internal static void MarkOriginalKinds(DataTable map, DataTable raw)
        {
            if (map == null)
            {
                return;
            }

            if (!map.Columns.Contains(OriginalSubProdTypColumn))
            {
                map.Columns.Add(OriginalSubProdTypColumn, typeof(string));
            }

            Dictionary<string, string> kindBySlot = new Dictionary<string, string>(StringComparer.Ordinal);

            if (raw != null)
            {
                foreach (DataRow row in raw.Rows)
                {
                    string kind = TableHelper.CellText(row, Schemas.DurableWafer.SubProdTyp.Name).Trim();
                    int position = TableHelper.ParseInt(TableHelper.CellText(row, Schemas.DurableWafer.SlotNo.Name));

                    if (position <= 0
                            || TableHelper.CellText(row, Schemas.DurableWafer.WfId.Name).Trim().Length == 0
                            || string.Equals(kind, Schemas.DurableLamella.SubProdTyp.Chip.Value, StringComparison.OrdinalIgnoreCase)
                            || string.Equals(kind, Schemas.DurableLamella.SubProdTyp.Lamella.Value, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    kindBySlot[Schemas.DurableWafer.SubProdTyp.Wafer.Value + "|"
                            + position.ToString(CultureInfo.InvariantCulture)] = kind;
                }
            }

            foreach (DataRow row in map.Rows)
            {
                string subProdTyp = TableHelper.CellText(row, Schemas.DurableWafer.SubProdTyp.Name).Trim();
                int position = TableHelper.ParseInt(TableHelper.CellText(row, Schemas.DurableWafer.SlotNo.Name));
                string original;

                if (TableHelper.CellText(row, Schemas.DurableWafer.WfId.Name).Trim().Length == 0
                        || !kindBySlot.TryGetValue(
                                subProdTyp + "|" + position.ToString(CultureInfo.InvariantCulture), out original))
                {
                    original = subProdTyp;
                }

                row[OriginalSubProdTypColumn] = original;
            }
        }

        internal static DataTable PrepareMap(DataTable incoming)
        {
            DataTable prepared = incoming == null ? new DataTable() : incoming.Copy();

            if (!prepared.Columns.Contains(MapKeyColumn))
            {
                prepared.Columns.Add(MapKeyColumn, typeof(string));
            }

            foreach (DataRow row in prepared.Rows)
            {
                row[MapKeyColumn] = MapStableKey(row);
            }

            return prepared;
        }

        internal static int CountFilled(DataTable wafers)
        {
            int count = 0;

            if (wafers == null)
            {
                return count;
            }

            foreach (DataRow row in wafers.Rows)
            {
                if (TableHelper.CellText(row, Schemas.DurableWafer.WfId.Name).Trim().Length > 0)
                {
                    count = count + 1;
                }
            }

            return count;
        }

        internal static bool IsSourceCandidate(DataRow row)
        {
            return IsValidCarrier(row)
                    && TableHelper.ParseInt(
                            TableHelper.CellText(row, Schemas.Durable.LotQty.Name)) > 0;
        }

        internal static bool IsValidCarrier(DataRow row)
        {
            if (row == null)
            {
                return false;
            }

            int capacity;
            int filled;
            bool capacityParsed = int.TryParse(
                    TableHelper.CellText(row, Schemas.Durable.Capa.Name),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out capacity);
            bool filledParsed = int.TryParse(
                    TableHelper.CellText(row, Schemas.Durable.LotQty.Name),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out filled);
            if (!capacityParsed
                    || !filledParsed
                    || capacity <= 0
                    || filled < 0
                    || filled > capacity)
            {
                return false;
            }

            string durableTyp = TableHelper.CellText(row, Schemas.Durable.DurableTyp.Name).Trim();

            if (durableTyp == Schemas.Durable.DurableTyp.Foup.Value)
            {
                return CountIsValid(row, Schemas.Durable.FoupLotQty.Name, Schemas.Durable.FoupCapa.Name);
            }

            return durableTyp == Schemas.Durable.DurableTyp.Tray.Value
                    && CountIsValid(row, Schemas.Durable.StubLotQty.Name, Schemas.Durable.StubCapa.Name)
                    && CountIsValid(row, Schemas.Durable.LccLotQty.Name, Schemas.Durable.LccCapa.Name);
        }

        private static bool CountIsValid(DataRow row, string countColumn, string capacityColumn)
        {
            int capacity;
            int filled;
            bool capacityParsed = int.TryParse(
                    TableHelper.CellText(row, capacityColumn),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out capacity);
            bool filledParsed = int.TryParse(
                    TableHelper.CellText(row, countColumn),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out filled);
            return capacityParsed
                    && filledParsed
                    && capacity > 0
                    && filled >= 0
                    && filled <= capacity;
        }

        internal static DataTable BuildTargetCandidates(DataTable carriers, string sourceId)
        {
            return BuildTargetCandidates(carriers, sourceId, null);
        }

        internal static DataTable BuildTargetCandidates(
                DataTable carriers, string sourceId, DataRow exchangeSource)
        {
            DataTable targets = carriers == null ? new DataTable() : carriers.Clone();

            if (carriers == null)
            {
                return targets;
            }

            foreach (DataRow row in carriers.Select(
                    string.Empty, Schemas.Durable.LotQty.Name + " ASC"))
            {
                string candidateId = TableHelper.CellText(row, Schemas.Durable.DurableId.Name);

                if (!IsValidCarrier(row)
                        || string.Equals(candidateId, sourceId, StringComparison.Ordinal))
                {
                    continue;
                }

                bool accepted = Schemas.Durable.IsNormalIdle(row)
                        && (exchangeSource == null
                                ? (Schemas.Durable.IsFreeAvailable(row) || Schemas.Durable.IsPartialInUse(row))
                                        && HasAvailableSlot(row)
                                : Schemas.Durable.IsFreeAvailable(row)
                                        && AcceptsWholeCarrier(row, exchangeSource));

                if (accepted)
                {
                    targets.ImportRow(row);
                }
            }

            return targets;
        }

        internal static bool AcceptsWholeCarrier(DataRow candidate, DataRow source)
        {
            if (!IsCompletelyEmpty(candidate))
            {
                return false;
            }

            return Fits(candidate, source, Schemas.Durable.FoupCapa.Name, Schemas.Durable.FoupLotQty.Name)
                    && Fits(candidate, source, Schemas.Durable.StubCapa.Name, Schemas.Durable.StubLotQty.Name)
                    && Fits(candidate, source, Schemas.Durable.LccCapa.Name, Schemas.Durable.LccLotQty.Name);
        }

        internal static bool IsCompletelyEmpty(DataRow row)
        {
            string status = TableHelper.CellText(row, Schemas.Durable.WfLoadStatCd.Name).Trim();

            return string.Equals(status, Schemas.Durable.WfLoadStatCd.Free.Value, StringComparison.Ordinal)
                    && Count(row, Schemas.Durable.LotQty.Name) == 0;
        }

        private static bool Fits(DataRow candidate, DataRow source, string capacityColumn, string usedColumn)
        {
            return Count(candidate, capacityColumn) >= Count(source, usedColumn);
        }

        private static int Count(DataRow row, string column)
        {
            int value;

            return int.TryParse(TableHelper.CellText(row, column).Trim(), out value) ? value : 0;
        }

        internal static bool HasAvailableSlot(DataRow row)
        {
            string durableTyp = TableHelper.CellText(row, Schemas.Durable.DurableTyp.Name).Trim();

            if (durableTyp == Schemas.Durable.DurableTyp.Foup.Value)
            {
                return TableHelper.ParseInt(TableHelper.CellText(row, Schemas.Durable.FoupLotQty.Name))
                        < TableHelper.ParseInt(TableHelper.CellText(row, Schemas.Durable.FoupCapa.Name));
            }

            return durableTyp == Schemas.Durable.DurableTyp.Tray.Value
                    && (TableHelper.ParseInt(TableHelper.CellText(row, Schemas.Durable.StubLotQty.Name))
                            < TableHelper.ParseInt(TableHelper.CellText(row, Schemas.Durable.StubCapa.Name))
                        || TableHelper.ParseInt(TableHelper.CellText(row, Schemas.Durable.LccLotQty.Name))
                            < TableHelper.ParseInt(TableHelper.CellText(row, Schemas.Durable.LccCapa.Name)));
        }

        internal static string MapStableKey(DataRow row)
        {
            if (row == null)
            {
                return string.Empty;
            }

            string subProdTyp = TableHelper.CellText(row, Schemas.DurableWafer.SubProdTyp.Name).Trim();
            int position = TableHelper.ParseInt(TableHelper.CellText(row, Schemas.DurableWafer.SlotNo.Name));

            if (position <= 0
                    || (subProdTyp != Schemas.DurableWafer.SubProdTyp.Wafer.Value
                            && subProdTyp != Schemas.DurableLamella.SubProdTyp.Chip.Value
                            && subProdTyp != Schemas.DurableLamella.SubProdTyp.Lamella.Value))
            {
                return string.Empty;
            }

            string key = subProdTyp + "|" + position.ToString(CultureInfo.InvariantCulture);

            if (subProdTyp != Schemas.DurableLamella.SubProdTyp.Lamella.Value)
            {
                return key;
            }

            string finger = NormalizeFinger(TableHelper.CellText(row, Schemas.DurableLamella.FingerId.Name));
            return finger.Length == 0 ? string.Empty : key + "|" + finger;
        }

        internal static bool MapIsStable(string durableTyp, DataTable map)
        {
            if (map == null
                    || (durableTyp != Schemas.Durable.DurableTyp.Foup.Value && durableTyp != Schemas.Durable.DurableTyp.Tray.Value))
            {
                return false;
            }

            HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);

            foreach (DataRow row in map.Rows)
            {
                string subProdTyp = TableHelper.CellText(row, Schemas.DurableWafer.SubProdTyp.Name).Trim();

                if ((durableTyp == Schemas.Durable.DurableTyp.Foup.Value && subProdTyp != Schemas.DurableWafer.SubProdTyp.Wafer.Value)
                        || (durableTyp == Schemas.Durable.DurableTyp.Tray.Value
                                && subProdTyp != Schemas.DurableLamella.SubProdTyp.Chip.Value
                                && subProdTyp != Schemas.DurableLamella.SubProdTyp.Lamella.Value))
                {
                    return false;
                }

                string key = MapStableKey(row);

                if (key.Length == 0 || !keys.Add(key))
                {
                    return false;
                }
            }

            return map.Rows.Count > 0;
        }

        internal static bool StagedRowsAreCurrent(DataTable source, DataTable staged)
        {
            Dictionary<string, string> current = FilledRows(source);
            Dictionary<string, string> selected = FilledRows(staged);

            if (selected.Count == 0)
            {
                return false;
            }

            foreach (KeyValuePair<string, string> row in selected)
            {
                string waferId;

                if (!current.TryGetValue(row.Key, out waferId)
                        || !string.Equals(waferId, row.Value, StringComparison.Ordinal))
                {
                    return false;
                }
            }

            HashSet<string> selectedLccPositions = LccPositions(staged);

            foreach (DataRow row in source.Rows)
            {
                if (TableHelper.CellText(row, Schemas.DurableWafer.WfId.Name).Trim().Length == 0)
                {
                    continue;
                }

                string positionKey = LccPositionKey(row);

                if (positionKey.Length > 0
                        && selectedLccPositions.Contains(positionKey)
                        && !selected.ContainsKey(MapStableKey(row)))
                {
                    return false;
                }
            }

            return true;
        }

        private static Dictionary<string, string> FilledRows(DataTable table)
        {
            Dictionary<string, string> rows = new Dictionary<string, string>(StringComparer.Ordinal);

            if (table == null)
            {
                return rows;
            }

            foreach (DataRow row in table.Rows)
            {
                string waferId = TableHelper.CellText(row, Schemas.DurableWafer.WfId.Name).Trim();

                if (waferId.Length == 0)
                {
                    continue;
                }

                string key = MapStableKey(row);

                if (key.Length == 0 || rows.ContainsKey(key))
                {
                    rows.Clear();
                    return rows;
                }

                rows.Add(key, waferId);
            }

            return rows;
        }

        internal static bool CanPlaceAll(string durableTyp, DataTable target, DataTable staged)
        {
            if (durableTyp == Schemas.Durable.DurableTyp.Foup.Value)
            {
                return CountEmptyRows(target, Schemas.DurableWafer.SubProdTyp.Wafer.Value) >= CountFilled(staged);
            }

            if (durableTyp != Schemas.Durable.DurableTyp.Tray.Value)
            {
                return false;
            }

            int stagedStubs = CountFilledRows(staged, Schemas.DurableLamella.SubProdTyp.Chip.Value);
            int emptyStubs = CountEmptyRows(target, Schemas.DurableLamella.SubProdTyp.Chip.Value);
            int stagedLccPositions = LccPositions(staged).Count;
            int emptyLccPositions = EmptyLccPositions(target);

            return stagedStubs <= emptyStubs && stagedLccPositions <= emptyLccPositions;
        }

        internal static string StagingCapacityReason(string durableTyp, DataTable target, DataTable staged)
        {
            if (target == null || staged == null || CountFilled(staged) == 0
                    || !MapIsStable(durableTyp, target))
            {
                return string.Empty;
            }

            if (durableTyp == Schemas.Durable.DurableTyp.Foup.Value)
            {
                int emptySlots = CountEmptyRows(target, Schemas.DurableWafer.SubProdTyp.Wafer.Value);
                int plannedSlots = CountFilledRows(staged, Schemas.DurableWafer.SubProdTyp.Wafer.Value);

                return plannedSlots <= emptySlots
                        ? string.Empty
                        : "Target has only " + emptySlots.ToString("N0")
                                + " compatible empty slots; " + plannedSlots.ToString("N0")
                                + " would be staged.";
            }

            if (durableTyp != Schemas.Durable.DurableTyp.Tray.Value)
            {
                return string.Empty;
            }

            int emptyStubs = CountEmptyRows(target, Schemas.DurableLamella.SubProdTyp.Chip.Value);
            int plannedStubs = CountFilledRows(staged, Schemas.DurableLamella.SubProdTyp.Chip.Value);

            if (plannedStubs > emptyStubs)
            {
                return "Target has only " + emptyStubs.ToString("N0")
                        + " empty STUB positions; " + plannedStubs.ToString("N0")
                        + " would be staged.";
            }

            int emptyLccs = EmptyLccPositions(target);
            int plannedLccs = LccPositions(staged).Count;

            return plannedLccs <= emptyLccs
                    ? string.Empty
                    : "Target has only " + emptyLccs.ToString("N0")
                            + " empty LCC positions; " + plannedLccs.ToString("N0")
                            + " would be staged.";
        }

        private static int CountFilledRows(DataTable table, string subProdTyp)
        {
            int count = 0;

            foreach (DataRow row in table.Rows)
            {
                if (string.Equals(
                        TableHelper.CellText(row, Schemas.DurableWafer.SubProdTyp.Name).Trim(),
                        subProdTyp,
                        StringComparison.OrdinalIgnoreCase)
                        && TableHelper.CellText(row, Schemas.DurableWafer.WfId.Name).Trim().Length > 0)
                {
                    count++;
                }
            }

            return count;
        }

        internal static int CountEmptyRows(DataTable table, string subProdTyp)
        {
            int count = 0;

            foreach (DataRow row in table.Rows)
            {
                if (string.Equals(
                        TableHelper.CellText(row, Schemas.DurableWafer.SubProdTyp.Name).Trim(),
                        subProdTyp,
                        StringComparison.OrdinalIgnoreCase)
                        && TableHelper.CellText(row, Schemas.DurableWafer.WfId.Name).Trim().Length == 0)
                {
                    count++;
                }
            }

            return count;
        }

        private static HashSet<string> LccPositions(DataTable table)
        {
            HashSet<string> positions = new HashSet<string>(StringComparer.Ordinal);

            if (table == null)
            {
                return positions;
            }

            foreach (DataRow row in table.Rows)
            {
                if (TableHelper.CellText(row, Schemas.DurableWafer.WfId.Name).Trim().Length == 0)
                {
                    continue;
                }

                string key = LccPositionKey(row);

                if (key.Length > 0)
                {
                    positions.Add(key);
                }
            }

            return positions;
        }

        internal static int EmptyLccPositions(DataTable target)
        {
            Dictionary<string, bool> empty = new Dictionary<string, bool>(StringComparer.Ordinal);

            foreach (DataRow row in target.Rows)
            {
                string key = LccPositionKey(row);

                if (key.Length == 0)
                {
                    continue;
                }

                if (!empty.ContainsKey(key))
                {
                    empty[key] = true;
                }

                if (TableHelper.CellText(row, Schemas.DurableWafer.WfId.Name).Trim().Length > 0)
                {
                    empty[key] = false;
                }
            }

            int count = 0;

            foreach (bool available in empty.Values)
            {
                if (available)
                {
                    count++;
                }
            }

            return count;
        }

        private static string LccPositionKey(DataRow row)
        {
            if (!string.Equals(
                    TableHelper.CellText(row, Schemas.DurableWafer.SubProdTyp.Name).Trim(),
                    Schemas.DurableLamella.SubProdTyp.Lamella.Value,
                    StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            int position = TableHelper.ParseInt(TableHelper.CellText(row, Schemas.DurableWafer.SlotNo.Name));
            return position <= 0
                    ? string.Empty
                    : Schemas.DurableLamella.SubProdTyp.Lamella.Value + "|" + position.ToString(CultureInfo.InvariantCulture);
        }

        private static string NormalizeFinger(string finger)
        {
            string value = (finger ?? string.Empty).Trim().ToUpperInvariant();

            if (value.Length == 0 || value == "-" || value == "--"
                    || value == "N/A" || value == "NA" || value == "NULL")
            {
                return string.Empty;
            }

            return value;
        }
    }
}
