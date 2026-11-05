using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Report.Services
{
    internal static class LabMonitorPresenter
    {
        internal const string RowKeyColumn = "ROW_KEY";
        internal const string RowKindColumn = "ROW_KIND";
        internal const string TitleColumn = "TITLE";
        internal const string SubtitleColumn = "SUBTITLE";
        internal const string QtyTextColumn = "QTY_TEXT";
        internal const string BadgeTextColumn = "BADGE_TEXT";
        internal const string BadgeKindColumn = "BADGE_KIND";
        internal const string EdgeKindColumn = "EDGE_KIND";
        internal const string LockColumn = "LOCK_YN";
        internal const string TrailingColumn = "TRAILING";
        internal const string ToolTipColumn = "TOOLTIP";
        internal const string IndentColumn = "INDENT";
        internal const string WaitSecondsColumn = "WAIT_SECONDS";
        internal const string PrioritySortColumn = "PRIORITY_NO";
        internal const string TagColumn = "STAGE_TAG";

        internal const string KindLot = "lot";
        internal const string KindFoup = "foup";
        internal const string KindEquipment = "eqp";
        internal const string KindDurable = "durable";
        internal const string KindTray = "tray";

        internal const int DefaultIntervalSeconds = 60;
        internal static readonly int[] IntervalChoices = new int[] { 0, 30, 60 };

        internal static readonly string[] DisplayColumns =
        {
            RowKeyColumn, RowKindColumn, TitleColumn, SubtitleColumn, QtyTextColumn, BadgeTextColumn, BadgeKindColumn,
            EdgeKindColumn, LockColumn, TrailingColumn, ToolTipColumn, IndentColumn, WaitSecondsColumn, PrioritySortColumn, TagColumn
        };

        internal static DataTable DecorateLots(DataTable lots, DateTime now)
        {
            DataTable table = lots == null ? NewDisplayTable() : lots.Copy();
            EnsureDisplayColumns(table);

            foreach (DataRow row in table.Rows)
            {
                DecorateLot(row, now);
            }

            return table;
        }

        internal static DataTable NewDisplayTable()
        {
            DataTable table = new DataTable();
            table.Locale = CultureInfo.InvariantCulture;
            EnsureDisplayColumns(table);
            return table;
        }

        internal static void EnsureDisplayColumns(DataTable table)
        {
            foreach (string column in DisplayColumns)
            {
                if (!table.Columns.Contains(column))
                {
                    table.Columns.Add(column, column == WaitSecondsColumn || column == PrioritySortColumn ? typeof(long) : typeof(string));
                }
            }
        }

        internal static string StageFilter(Modern.Lab.WinForms.Controls.Data.FieldValue stage)
        {
            return Schemas.MonitorLot.MonStage.Name + " = '" + stage.Value + "'";
        }

        internal static string RunningFilter(Modern.Lab.WinForms.Controls.Data.FieldValue stage)
        {
            return StageFilter(stage) + " AND " + Schemas.MonitorLot.MonStatCd.Name + " = '" + Schemas.MonitorLot.MonStatCd.Job.Value + "'";
        }

        internal static string QueueFilter(Modern.Lab.WinForms.Controls.Data.FieldValue stage)
        {
            return StageFilter(stage) + " AND " + Schemas.MonitorLot.MonStatCd.Name + " <> '" + Schemas.MonitorLot.MonStatCd.Job.Value + "'";
        }

        internal static string DoneFilter()
        {
            return StageFilter(Schemas.MonitorLot.MonStage.Done) + " AND " + Schemas.MonitorLot.ScrapDueYn.Name + " <> '" + Schemas.MonitorLot.ScrapDueYn.Y.Value + "'";
        }

        internal static string ScrapDueFilter()
        {
            return Schemas.MonitorLot.ScrapDueYn.Name + " = '" + Schemas.MonitorLot.ScrapDueYn.Y.Value + "'";
        }

        internal static string ReworkFilter()
        {
            return Schemas.MonitorLot.ReworkYn.Name + " = '" + Schemas.MonitorLot.ReworkYn.Y.Value + "'";
        }

        internal static string RequestFilter(string stageValue)
        {
            return Schemas.MonitorLot.MonStage.Name + " = '" + stageValue + "'";
        }

        internal static int CountRows(DataTable table, string filter)
        {
            if (table == null)
            {
                return 0;
            }

            return table.Select(filter).Length;
        }

        internal static DataTable ExchangeRows(DataTable lots, ICollection<string> expandedFoups, DateTime now)
        {
            DataTable result = lots == null ? NewDisplayTable() : lots.Clone();
            EnsureDisplayColumns(result);

            if (lots == null)
            {
                return result;
            }

            Dictionary<string, List<DataRow>> byFoup = new Dictionary<string, List<DataRow>>(StringComparer.Ordinal);
            List<string> order = new List<string>();

            foreach (DataRow lot in lots.Select(StageFilter(Schemas.MonitorLot.MonStage.Exchange)))
            {
                string foup = Schemas.MonitorLot.DurableId.Text(lot);
                List<DataRow> group;

                if (!byFoup.TryGetValue(foup, out group))
                {
                    group = new List<DataRow>();
                    byFoup[foup] = group;
                    order.Add(foup);
                }

                group.Add(lot);
            }

            foreach (string foup in order)
            {
                List<DataRow> group = byFoup[foup];
                DataRow head = result.NewRow();
                int qty = 0;
                bool anyHold = false;
                string fab = string.Empty;
                DateTime oldest = DateTime.MaxValue;

                foreach (DataRow lot in group)
                {
                    qty += TableHelper.ParseInt(Schemas.MonitorLot.Qty.Text(lot));
                    anyHold = anyHold || Schemas.MonitorLot.LotHoldStatCd.Is(lot, Schemas.MonitorLot.LotHoldStatCd.OnHold);
                    fab = fab.Length > 0 ? fab : Schemas.MonitorLot.SendFac.Text(lot);
                    DateTime stageTime;

                    if (TryParseTime(Schemas.MonitorLot.StageTm.Text(lot), out stageTime) && stageTime < oldest)
                    {
                        oldest = stageTime;
                    }
                }

                head[Schemas.MonitorLot.MonStage.Name] = Schemas.MonitorLot.MonStage.Exchange.Value;
                head[Schemas.MonitorLot.DurableId.Name] = foup;
                head[RowKeyColumn] = KindFoup + "|" + foup;
                head[RowKindColumn] = KindFoup;
                head[TitleColumn] = foup.Length == 0 ? "(no FOUP)" : foup;
                head[SubtitleColumn] = group.Count == 1 ? "1 lot" : group.Count.ToString(CultureInfo.InvariantCulture) + " lots";
                head[QtyTextColumn] = qty.ToString(CultureInfo.InvariantCulture);
                head[BadgeTextColumn] = fab.Length == 0 ? "FOUP" : fab;
                head[BadgeKindColumn] = ModernQueueListControl.BadgeNeutral;
                head[EdgeKindColumn] = anyHold ? ModernQueueListControl.EdgeHold : string.Empty;
                head[LockColumn] = anyHold ? "Y" : "N";
                head[TrailingColumn] = oldest == DateTime.MaxValue ? string.Empty : Elapsed(now, oldest);
                head[WaitSecondsColumn] = oldest == DateTime.MaxValue ? 0L : (long)(now - oldest).TotalSeconds;
                head[IndentColumn] = "0";
                head[PrioritySortColumn] = (long)result.Rows.Count;
                head[ToolTipColumn] = "FOUP  " + foup + "\nLots  " + group.Count.ToString(CultureInfo.InvariantCulture)
                        + "\nWafers  " + qty.ToString(CultureInfo.InvariantCulture)
                        + (fab.Length == 0 ? string.Empty : "\nFrom Fab  " + fab)
                        + "\nClick to show the lots";
                result.Rows.Add(head);

                if (expandedFoups == null || !expandedFoups.Contains(foup))
                {
                    continue;
                }

                foreach (DataRow lot in group)
                {
                    DataRow child = result.NewRow();
                    child.ItemArray = lot.ItemArray;
                    child[IndentColumn] = "1";
                    result.Rows.Add(child);
                }
            }

            return result;
        }

        internal static DataTable ReturnRows(DataTable durables, DateTime now)
        {
            DataTable result = NewDisplayTable();
            AddDurableColumns(result);

            if (durables == null)
            {
                return result;
            }

            foreach (DataRow durable in durables.Rows)
            {
                string returnYn = Schemas.MonitorDurable.ReturnYn.Text(durable);

                if (returnYn.Length == 0 || !Schemas.MonitorDurable.DurableTyp.Is(durable, Schemas.MonitorDurable.DurableTyp.Foup))
                {
                    continue;
                }

                bool returned = Schemas.MonitorDurable.ReturnYn.Is(durable, Schemas.MonitorDurable.ReturnYn.Y);
                DateTime eventTime;
                bool hasTime = TryParseTime(Schemas.MonitorDurable.EventTm.Text(durable), out eventTime);

                if (returned && (!hasTime || eventTime.Date != now.Date))
                {
                    continue;
                }

                DataRow row = NewDurableRow(result, durable, now, hasTime ? eventTime : DateTime.MinValue);
                row[SubtitleColumn] = "from " + Schemas.MonitorDurable.FromFac.Text(durable);
                row[BadgeTextColumn] = returned ? "✓ Fab" : "To Fab";
                row[BadgeKindColumn] = returned ? ModernQueueListControl.BadgeSuccess : ModernQueueListControl.BadgeWarning;
                row[ToolTipColumn] = "FOUP  " + Schemas.MonitorDurable.DurableId.Text(durable)
                        + "\nFrom Fab  " + Schemas.MonitorDurable.FromFac.Text(durable)
                        + "\nReturned  " + (returned ? "Yes" : "No")
                        + "\nLocation  " + Schemas.MonitorDurable.LocNm.Text(durable);
                result.Rows.Add(row);
            }

            return result;
        }

        internal static DataTable SwapRows(DataTable durables, DateTime now)
        {
            DataTable result = NewDisplayTable();
            AddDurableColumns(result);

            if (durables == null)
            {
                return result;
            }

            foreach (DataRow durable in durables.Select(Schemas.MonitorDurable.SwapYn.Name + " = '" + Schemas.MonitorDurable.SwapYn.Y.Value + "'"))
            {
                DateTime eventTime;
                bool hasTime = TryParseTime(Schemas.MonitorDurable.EventTm.Text(durable), out eventTime);
                DataRow row = NewDurableRow(result, durable, now, hasTime ? eventTime : DateTime.MinValue);
                row[SubtitleColumn] = Schemas.MonitorDurable.LocNm.Text(durable);
                row[BadgeTextColumn] = "Swap";
                row[BadgeKindColumn] = ModernQueueListControl.BadgeNeutral;
                row[ToolTipColumn] = "FOUP  " + Schemas.MonitorDurable.DurableId.Text(durable)
                        + "\nSwapped  Yes"
                        + "\nLocation  " + Schemas.MonitorDurable.LocNm.Text(durable)
                        + "\nLots  " + Schemas.MonitorDurable.LotCnt.Text(durable);
                result.Rows.Add(row);
            }

            return result;
        }

        internal static DataTable AttentionRows(DataTable lots, DataTable equipments, DataTable durables, DateTime now)
        {
            DataTable result = NewDisplayTable();
            AddDurableColumns(result);
            result.Columns.Add(Schemas.MonitorEquipment.EqpId.Name, typeof(string));
            result.Columns.Add(Schemas.MonitorLot.LotId.Name, typeof(string));
            result.Columns.Add(Schemas.MonitorLot.ReqSerialNo.Name, typeof(string));

            if (lots != null)
            {
                foreach (DataRow lot in lots.Rows)
                {
                    bool hold = Schemas.MonitorLot.LotHoldStatCd.Is(lot, Schemas.MonitorLot.LotHoldStatCd.OnHold);
                    bool rework = Schemas.MonitorLot.ReworkYn.Is(lot, Schemas.MonitorLot.ReworkYn.Y);

                    if (!hold && !rework)
                    {
                        continue;
                    }

                    DataRow row = result.NewRow();
                    CopyDisplay(lot, row);
                    row[RowKeyColumn] = "attention|" + TableHelper.CellText(lot, RowKeyColumn);
                    row[Schemas.MonitorLot.LotId.Name] = Schemas.MonitorLot.LotId.Text(lot);
                    row[Schemas.MonitorLot.ReqSerialNo.Name] = Schemas.MonitorLot.ReqSerialNo.Text(lot);
                    row[SubtitleColumn] = Schemas.MonitorLot.LotId.Text(lot) + " · " + StageLabel(Schemas.MonitorLot.MonStage.Text(lot));
                    row[BadgeTextColumn] = hold ? "Hold" : TableHelper.CellText(lot, BadgeTextColumn);
                    row[BadgeKindColumn] = hold ? ModernQueueListControl.BadgeWarning : ModernQueueListControl.BadgeError;
                    result.Rows.Add(row);
                }
            }

            if (equipments != null)
            {
                foreach (DataRow equipment in equipments.Rows)
                {
                    if (!Schemas.MonitorEquipment.MesEqpStatCd.Is(equipment, Schemas.MonitorEquipment.MesEqpStatCd.Down))
                    {
                        continue;
                    }

                    DateTime since;
                    bool hasTime = TryParseTime(Schemas.MonitorEquipment.StatusTm.Text(equipment), out since);
                    DataRow row = result.NewRow();
                    string eqpId = Schemas.MonitorEquipment.EqpId.Text(equipment);
                    row[RowKeyColumn] = "attention|" + KindEquipment + "|" + eqpId;
                    row[RowKindColumn] = KindEquipment;
                    row[Schemas.MonitorEquipment.EqpId.Name] = eqpId;
                    row[TitleColumn] = eqpId;
                    row[SubtitleColumn] = Schemas.MonitorEquipment.EqpNm.Text(equipment) + " · " + StageLabel(Schemas.MonitorEquipment.MonStage.Text(equipment));
                    row[QtyTextColumn] = string.Empty;
                    row[BadgeTextColumn] = "Down";
                    row[BadgeKindColumn] = ModernQueueListControl.BadgeError;
                    row[EdgeKindColumn] = ModernQueueListControl.EdgeRework;
                    row[LockColumn] = "N";
                    row[IndentColumn] = "0";
                    row[TrailingColumn] = hasTime ? Elapsed(now, since) : string.Empty;
                    row[WaitSecondsColumn] = hasTime ? (long)(now - since).TotalSeconds : 0L;
                    row[ToolTipColumn] = "Equipment  " + eqpId + "\nName  " + Schemas.MonitorEquipment.EqpNm.Text(equipment)
                            + "\nStatus  Down" + (hasTime ? "\nSince  " + Schemas.MonitorEquipment.StatusTm.Text(equipment) : string.Empty);
                    result.Rows.Add(row);
                }
            }

            if (durables != null)
            {
                foreach (DataRow durable in durables.Rows)
                {
                    if (!Schemas.MonitorDurable.ReturnYn.Is(durable, Schemas.MonitorDurable.ReturnYn.N)
                            || !Schemas.MonitorDurable.DurableTyp.Is(durable, Schemas.MonitorDurable.DurableTyp.Foup))
                    {
                        continue;
                    }

                    DataRow row = NewDurableRow(result, durable, now, DateTime.MinValue);
                    row[RowKeyColumn] = "attention|" + TableHelper.CellText(row, RowKeyColumn);
                    row[SubtitleColumn] = "FOUP from " + Schemas.MonitorDurable.FromFac.Text(durable) + " not returned";
                    row[BadgeTextColumn] = "To Fab";
                    row[BadgeKindColumn] = ModernQueueListControl.BadgeWarning;
                    row[ToolTipColumn] = "FOUP  " + Schemas.MonitorDurable.DurableId.Text(durable)
                            + "\nFrom Fab  " + Schemas.MonitorDurable.FromFac.Text(durable) + "\nReturned  No";
                    result.Rows.Add(row);
                }

                StorageCounts storage = CountStorage(durables);

                if (storage.FreeTrays == 0)
                {
                    DataRow row = result.NewRow();
                    row[RowKeyColumn] = "attention|" + KindTray;
                    row[RowKindColumn] = KindTray;
                    row[TitleColumn] = "Trays";
                    row[SubtitleColumn] = "No free tray in the stocker";
                    row[QtyTextColumn] = string.Empty;
                    row[BadgeTextColumn] = "Trays";
                    row[BadgeKindColumn] = ModernQueueListControl.BadgeWarning;
                    row[EdgeKindColumn] = string.Empty;
                    row[LockColumn] = "N";
                    row[IndentColumn] = "0";
                    row[TrailingColumn] = string.Empty;
                    row[WaitSecondsColumn] = 0L;
                    row[ToolTipColumn] = "Trays in stocker  " + storage.StockerTrays.ToString(CultureInfo.InvariantCulture) + "\nFree trays  0";
                    result.Rows.Add(row);
                }
            }

            DataView sorted = new DataView(result, string.Empty, WaitSecondsColumn + " DESC, " + TitleColumn, DataViewRowState.CurrentRows);
            return sorted.ToTable();
        }

        internal static StorageCounts CountStorage(DataTable durables)
        {
            StorageCounts counts = new StorageCounts();

            if (durables == null)
            {
                return counts;
            }

            foreach (DataRow durable in durables.Rows)
            {
                bool foup = Schemas.MonitorDurable.DurableTyp.Is(durable, Schemas.MonitorDurable.DurableTyp.Foup);
                bool tray = Schemas.MonitorDurable.DurableTyp.Is(durable, Schemas.MonitorDurable.DurableTyp.Tray);
                bool stb = Schemas.MonitorDurable.LocKind.Is(durable, Schemas.MonitorDurable.LocKind.Stb);
                bool stocker = Schemas.MonitorDurable.LocKind.Is(durable, Schemas.MonitorDurable.LocKind.Stocker);
                int lotCount = TableHelper.ParseInt(Schemas.MonitorDurable.LotCnt.Text(durable));

                if (foup && stb)
                {
                    counts.StbFoups++;

                    if (lotCount >= 1)
                    {
                        counts.StoredFoups++;
                    }

                    if (lotCount >= 2)
                    {
                        counts.MixedFoups++;
                    }
                }

                if (tray && stocker)
                {
                    counts.StockerTrays++;

                    if (lotCount == 0)
                    {
                        counts.FreeTrays++;
                    }
                }
            }

            return counts;
        }

        internal static DataRow FindEquipment(DataTable equipments, Modern.Lab.WinForms.Controls.Data.FieldValue stage)
        {
            if (equipments == null)
            {
                return null;
            }

            foreach (DataRow row in equipments.Rows)
            {
                if (Schemas.MonitorEquipment.MonStage.Is(row, stage) && Schemas.MonitorEquipment.ParentEqpId.Text(row).Length == 0)
                {
                    return row;
                }
            }

            return null;
        }

        internal const string SiteIdColumn = "SITE_ID";
        internal const string SiteStateColumn = "SITE_STATE";
        internal const string PortLabelColumn = "PORT_LABEL";
        internal const string PortDirectionColumn = "PORT_DIRECTION";
        internal const string PortStateColumn = "PORT_STATE";

        internal static DataTable HandlerSiteRows(DataTable equipments, string parentEqpId)
        {
            DataTable sites = new DataTable();
            sites.Locale = CultureInfo.InvariantCulture;
            sites.Columns.Add(SiteIdColumn, typeof(string));
            sites.Columns.Add(SiteStateColumn, typeof(string));
            sites.Columns.Add(Schemas.MonitorEquipment.LotId.Name, typeof(string));
            sites.Columns.Add(Schemas.MonitorEquipment.ReservedLotId.Name, typeof(string));

            if (equipments == null || string.IsNullOrEmpty(parentEqpId))
            {
                return sites;
            }

            foreach (DataRow row in equipments.Rows)
            {
                if (!string.Equals(Schemas.MonitorEquipment.ParentEqpId.Text(row), parentEqpId, StringComparison.Ordinal))
                {
                    continue;
                }

                sites.Rows.Add(
                        ShortSiteId(Schemas.MonitorEquipment.EqpId.Text(row), parentEqpId),
                        Schemas.MonitorEquipment.MesEqpStatCd.Text(row),
                        Schemas.MonitorEquipment.LotId.Text(row),
                        Schemas.MonitorEquipment.ReservedLotId.Text(row));
            }

            return sites;
        }

        internal static DataTable HandlerPortRows(DataTable ports, string eqpId)
        {
            DataTable result = new DataTable();
            result.Locale = CultureInfo.InvariantCulture;
            result.Columns.Add(PortLabelColumn, typeof(string));
            result.Columns.Add(PortDirectionColumn, typeof(string));
            result.Columns.Add(PortStateColumn, typeof(string));
            result.Columns.Add(Schemas.MonitorPort.LotId.Name, typeof(string));

            if (ports == null || string.IsNullOrEmpty(eqpId))
            {
                return result;
            }

            foreach (DataRow row in ports.Rows)
            {
                if (!string.Equals(Schemas.MonitorPort.EqpId.Text(row), eqpId, StringComparison.Ordinal))
                {
                    continue;
                }

                bool isOut = Schemas.MonitorPort.PortTyp.Is(row, Schemas.MonitorPort.PortTyp.Output);
                result.Rows.Add(
                        (isOut ? "Out " : "In ") + Schemas.MonitorPort.PortNm.Text(row),
                        isOut ? "Out" : "In",
                        PortState(row),
                        Schemas.MonitorPort.LotId.Text(row));
            }

            return result;
        }

        internal static string EquipmentHeadline(DataRow equipment, DateTime now)
        {
            if (equipment == null)
            {
                return "No equipment";
            }

            string lotId = Schemas.MonitorEquipment.LotId.Text(equipment);
            string status = Schemas.MonitorEquipment.MesEqpStatCd.Text(equipment);
            DateTime since;
            bool hasTime = TryParseTime(Schemas.MonitorEquipment.StatusTm.Text(equipment), out since);
            string elapsed = hasTime ? Elapsed(now, since) : string.Empty;

            if (lotId.Length > 0)
            {
                return lotId + (elapsed.Length == 0 ? string.Empty : " · " + elapsed);
            }

            return (status.Length == 0 ? "Idle" : status) + (elapsed.Length == 0 ? string.Empty : " · " + elapsed);
        }

        internal static string StageLabel(string stageValue)
        {
            MonitorFields.MonStage monStage = Schemas.MonitorLot.MonStage;

            if (stageValue == monStage.Request.Value)
            {
                return "Request";
            }

            if (stageValue == monStage.Send.Value)
            {
                return "Send";
            }

            if (stageValue == monStage.Arrival.Value)
            {
                return "Arrival";
            }

            if (stageValue == monStage.Exchange.Value)
            {
                return "Exchange";
            }

            if (stageValue == monStage.Prep.Value)
            {
                return "PREP";
            }

            if (stageValue == monStage.Fib.Value)
            {
                return "FIB";
            }

            if (stageValue == monStage.Tem.Value)
            {
                return "TEM";
            }

            if (stageValue == monStage.Done.Value)
            {
                return "Done";
            }

            return stageValue;
        }

        internal static string Elapsed(DateTime now, DateTime since)
        {
            TimeSpan span = now - since;

            if (span.Ticks < 0)
            {
                span = TimeSpan.Zero;
            }

            if (span.TotalDays >= 1d)
            {
                return ((int)span.TotalDays).ToString(CultureInfo.InvariantCulture) + "d " + span.Hours.ToString(CultureInfo.InvariantCulture) + "h";
            }

            if (span.TotalHours >= 1d)
            {
                return span.Hours.ToString(CultureInfo.InvariantCulture) + "h " + span.Minutes.ToString(CultureInfo.InvariantCulture) + "m";
            }

            return span.Minutes.ToString(CultureInfo.InvariantCulture) + "m";
        }

        internal static bool TryParseTime(string text, out DateTime value)
        {
            value = DateTime.MinValue;

            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out value)
                    || DateTime.TryParse(text, out value);
        }

        private static void DecorateLot(DataRow row, DateTime now)
        {
            MonitorLotEntity lot = Schemas.MonitorLot;
            string request = lot.ReqSerialNo.Text(row);
            string lotId = lot.LotId.Text(row);
            int requestCount = TableHelper.ParseInt(lot.ReqCnt.Text(row));
            bool hold = lot.LotHoldStatCd.Is(row, lot.LotHoldStatCd.OnHold);
            bool rework = lot.ReworkYn.Is(row, lot.ReworkYn.Y);
            bool scrapDue = lot.ScrapDueYn.Is(row, lot.ScrapDueYn.Y);
            bool done = lot.MonStage.Is(row, lot.MonStage.Done);
            string state = lot.MonStatCd.Text(row);
            DateTime stageTime;
            bool hasTime = TryParseTime(lot.StageTm.Text(row), out stageTime);

            row[RowKeyColumn] = lot.MonStage.Text(row) + "|" + request + "|" + lotId;
            row[RowKindColumn] = KindLot;
            row[TitleColumn] = request.Length == 0
                    ? (lotId.Length == 0 ? "(no request)" : lotId)
                    : (requestCount > 1 ? request + " +" + (requestCount - 1).ToString(CultureInfo.InvariantCulture) : request);
            row[SubtitleColumn] = request.Length == 0 ? string.Empty : lotId;
            row[QtyTextColumn] = lot.Qty.Text(row);
            row[EdgeKindColumn] = hold ? ModernQueueListControl.EdgeHold
                    : rework ? ModernQueueListControl.EdgeRework
                    : lot.MonStatCd.Is(row, lot.MonStatCd.Job) ? ModernQueueListControl.EdgeJob
                    : string.Empty;
            row[LockColumn] = hold ? "Y" : "N";
            row[IndentColumn] = "0";
            row[TrailingColumn] = hasTime ? Elapsed(now, stageTime) : string.Empty;
            row[WaitSecondsColumn] = hasTime ? (long)(now - stageTime).TotalSeconds : 0L;
            row[PrioritySortColumn] = (long)TableHelper.ParseInt(lot.Priority.Text(row));

            string badgeText;
            string badgeKind;

            if (rework)
            {
                badgeText = "RW " + lot.ReworkCnt.Text(row);
                badgeKind = ModernQueueListControl.BadgeError;
            }
            else if (scrapDue)
            {
                badgeText = "Due";
                badgeKind = ModernQueueListControl.BadgeWarning;
            }
            else if (done)
            {
                badgeText = "Done";
                badgeKind = ModernQueueListControl.BadgeNeutral;
            }
            else if (lot.MonStatCd.Is(row, lot.MonStatCd.Job))
            {
                badgeText = "Job";
                badgeKind = ModernQueueListControl.BadgeSuccess;
            }
            else if (lot.MonStatCd.Is(row, lot.MonStatCd.Next))
            {
                badgeText = "Next";
                badgeKind = ModernQueueListControl.BadgeAccent;
            }
            else
            {
                badgeText = state.Length == 0 ? "Wait" : state;
                badgeKind = ModernQueueListControl.BadgeNeutral;
            }

            row[BadgeTextColumn] = badgeText;
            row[BadgeKindColumn] = badgeKind;
            row[ToolTipColumn] = "Request  " + (request.Length == 0 ? "-" : request)
                    + "\nLot  " + (lotId.Length == 0 ? "-" : lotId)
                    + "\nQty  " + (lot.Qty.Text(row).Length == 0 ? "-" : lot.Qty.Text(row))
                    + "\nHold  " + (hold ? "Yes" : "No")
                    + "\nState  " + badgeText
                    + "\nWaiting  " + (hasTime ? Elapsed(now, stageTime) : "-")
                    + "\nDurable  " + (lot.DurableId.Text(row).Length == 0 ? "-" : lot.DurableId.Text(row));
        }

        private static void CopyDisplay(DataRow source, DataRow target)
        {
            foreach (string column in DisplayColumns)
            {
                if (source.Table.Columns.Contains(column))
                {
                    target[column] = source[column];
                }
            }
        }

        private static void AddDurableColumns(DataTable table)
        {
            if (!table.Columns.Contains(Schemas.MonitorDurable.DurableId.Name))
            {
                table.Columns.Add(Schemas.MonitorDurable.DurableId.Name, typeof(string));
            }
        }

        private static DataRow NewDurableRow(DataTable table, DataRow durable, DateTime now, DateTime eventTime)
        {
            string durableId = Schemas.MonitorDurable.DurableId.Text(durable);
            DataRow row = table.NewRow();
            row[RowKeyColumn] = KindDurable + "|" + durableId;
            row[RowKindColumn] = KindDurable;
            row[Schemas.MonitorDurable.DurableId.Name] = durableId;
            row[TitleColumn] = durableId;
            row[SubtitleColumn] = string.Empty;
            row[QtyTextColumn] = Schemas.MonitorDurable.WfQty.Text(durable);
            row[EdgeKindColumn] = string.Empty;
            row[LockColumn] = "N";
            row[IndentColumn] = "0";
            row[PrioritySortColumn] = (long)table.Rows.Count;
            row[TrailingColumn] = eventTime == DateTime.MinValue ? string.Empty : Elapsed(now, eventTime);
            row[WaitSecondsColumn] = eventTime == DateTime.MinValue ? 0L : (long)(now - eventTime).TotalSeconds;
            return row;
        }

        private static string PortState(DataRow port)
        {
            MonitorPortEntity entity = Schemas.MonitorPort;

            if (entity.TransferStatCd.Is(port, entity.TransferStatCd.Processing))
            {
                return "Running";
            }

            if (entity.TransferStatCd.Is(port, entity.TransferStatCd.ReservedToLoad)
                    || entity.TransferStatCd.Is(port, entity.TransferStatCd.ReservedToUnload))
            {
                return "Reserved";
            }

            if (entity.TransferStatCd.Is(port, entity.TransferStatCd.ReadyToUnload))
            {
                return entity.LotId.Text(port).Length > 0 ? "Done" : "Empty";
            }

            return entity.LotId.Text(port).Length > 0 || entity.DurableId.Text(port).Length > 0 ? "Loaded" : "Empty";
        }

        private static string ShortSiteId(string eqpId, string parentEqpId)
        {
            if (eqpId.StartsWith(parentEqpId + "-", StringComparison.OrdinalIgnoreCase) && eqpId.Length > parentEqpId.Length + 1)
            {
                return eqpId.Substring(parentEqpId.Length + 1);
            }

            return eqpId;
        }

        internal static int FoupCount(DataTable lots)
        {
            if (lots == null)
            {
                return 0;
            }

            HashSet<string> foups = new HashSet<string>(StringComparer.Ordinal);

            foreach (DataRow lot in lots.Select(StageFilter(Schemas.MonitorLot.MonStage.Exchange)))
            {
                foups.Add(Schemas.MonitorLot.DurableId.Text(lot));
            }

            return foups.Count;
        }

        internal static DataTable IncomingRows(DataTable lots, ICollection<string> expandedFoups, DateTime now)
        {
            DataTable result = ExchangeRows(lots, expandedFoups, now);

            foreach (DataRow row in result.Rows)
            {
                row[TagColumn] = "Exchange";
            }

            if (lots == null)
            {
                return result;
            }

            List<DataRow> early = new List<DataRow>();
            early.AddRange(lots.Select(StageFilter(Schemas.MonitorLot.MonStage.Send), PrioritySortColumn));
            early.AddRange(lots.Select(StageFilter(Schemas.MonitorLot.MonStage.Arrival), PrioritySortColumn));

            int position = 0;

            foreach (DataRow lot in early)
            {
                DataRow row = result.NewRow();
                row.ItemArray = lot.ItemArray;
                row[TagColumn] = StageLabel(Schemas.MonitorLot.MonStage.Text(lot));
                result.Rows.InsertAt(row, position);
                position++;
            }

            return result;
        }

        internal static StageTileInfo[] StageTiles(DataTable lots, DataTable equipments, DataTable durables, DateTime now)
        {
            MonitorFields.MonStage monStage = Schemas.MonitorLot.MonStage;
            StorageCounts storage = CountStorage(durables);
            int unreturned = 0;
            int swapped = 0;

            if (durables != null)
            {
                foreach (DataRow durable in durables.Rows)
                {
                    bool foup = Schemas.MonitorDurable.DurableTyp.Is(durable, Schemas.MonitorDurable.DurableTyp.Foup);

                    if (foup && Schemas.MonitorDurable.ReturnYn.Is(durable, Schemas.MonitorDurable.ReturnYn.N))
                    {
                        unreturned++;
                    }

                    if (Schemas.MonitorDurable.SwapYn.Is(durable, Schemas.MonitorDurable.SwapYn.Y))
                    {
                        swapped++;
                    }
                }
            }

            DataRow prep = FindEquipment(equipments, monStage.Prep);
            DataRow fib = FindEquipment(equipments, monStage.Fib);
            DataRow tem = FindEquipment(equipments, monStage.Tem);
            int holdPrep = CountRows(lots, StageFilter(monStage.Prep) + " AND " + Schemas.MonitorLot.LotHoldStatCd.Name + " = '" + Schemas.MonitorLot.LotHoldStatCd.OnHold.Value + "'");
            int rework = CountRows(lots, ReworkFilter());
            int scrapDue = CountRows(lots, ScrapDueFilter());
            string handlerSummary = HandlerSummary(equipments, fib == null ? string.Empty : Schemas.MonitorEquipment.EqpId.Text(fib));

            return new StageTileInfo[]
            {
                Tile(monStage.Request, "Request", CountRows(lots, StageFilter(monStage.Request)), OldestText(lots, monStage.Request), string.Empty, string.Empty),
                Tile(monStage.Send, "Send", CountRows(lots, StageFilter(monStage.Send)), FacsText(lots, monStage.Send), string.Empty, string.Empty),
                Tile(monStage.Arrival, "Arrival", CountRows(lots, StageFilter(monStage.Arrival)), OldestText(lots, monStage.Arrival), string.Empty, string.Empty),
                Tile(monStage.Exchange, "Exchange", FoupCount(lots),
                        "FOUP" + (unreturned > 0 ? " · return " + unreturned.ToString(CultureInfo.InvariantCulture) : string.Empty)
                        + (swapped > 0 ? " · swap " + swapped.ToString(CultureInfo.InvariantCulture) : string.Empty),
                        unreturned > 0 ? "warning" : string.Empty, string.Empty),
                Tile(monStage.Prep, "PREP", CountRows(lots, StageFilter(monStage.Prep)),
                        "job " + CountRows(lots, RunningFilter(monStage.Prep)).ToString(CultureInfo.InvariantCulture)
                        + (holdPrep > 0 ? " · hold " + holdPrep.ToString(CultureInfo.InvariantCulture) : string.Empty),
                        holdPrep > 0 ? "warning" : string.Empty, LampOf(prep)),
                Tile(monStage.Fib, "FIB", CountRows(lots, StageFilter(monStage.Fib)), handlerSummary,
                        handlerSummary.IndexOf("down", StringComparison.OrdinalIgnoreCase) >= 0 ? "error" : string.Empty, LampOf(fib)),
                Tile(monStage.Tem, "TEM", CountRows(lots, StageFilter(monStage.Tem)),
                        rework > 0 ? "rework " + rework.ToString(CultureInfo.InvariantCulture) : "job " + CountRows(lots, RunningFilter(monStage.Tem)).ToString(CultureInfo.InvariantCulture),
                        rework > 0 ? "error" : string.Empty, LampOf(tem)),
                Tile(monStage.Done, "Done", CountRows(lots, DoneFilter()),
                        scrapDue > 0 ? "scrap due " + scrapDue.ToString(CultureInfo.InvariantCulture) : "14 days",
                        scrapDue > 0 ? "warning" : string.Empty, string.Empty)
            };
        }

        internal static string RunningTitle(DataTable lots, Modern.Lab.WinForms.Controls.Data.FieldValue stage)
        {
            if (lots == null)
            {
                return string.Empty;
            }

            DataRow[] running = lots.Select(RunningFilter(stage), PrioritySortColumn);

            if (running.Length == 0)
            {
                return string.Empty;
            }

            string request = Schemas.MonitorLot.ReqSerialNo.Text(running[0]);
            string lotId = Schemas.MonitorLot.LotId.Text(running[0]);
            string title = request.Length == 0 ? lotId : (lotId.Length == 0 ? request : request + " · " + lotId);

            if (running.Length > 1)
            {
                title += " +" + (running.Length - 1).ToString(CultureInfo.InvariantCulture);
            }

            return title;
        }

        internal static string RunningSubtitle(DataTable lots, DataRow equipment, Modern.Lab.WinForms.Controls.Data.FieldValue stage, DateTime now)
        {
            DataRow[] running = lots == null ? new DataRow[0] : lots.Select(RunningFilter(stage), PrioritySortColumn);
            string status = equipment == null ? string.Empty : Schemas.MonitorEquipment.MesEqpStatCd.Text(equipment);
            DateTime since = DateTime.MinValue;
            bool hasStatusTime = equipment != null && TryParseTime(Schemas.MonitorEquipment.StatusTm.Text(equipment), out since);

            if (running.Length == 0)
            {
                string label = status.Length == 0 ? "No equipment" : status;
                return hasStatusTime ? label + " · " + Elapsed(now, since) : label;
            }

            DataRow first = running[0];
            string qty = Schemas.MonitorLot.Qty.Text(first);
            string eqpId = Schemas.MonitorLot.EqpId.Text(first);
            List<string> parts = new List<string>();

            if (qty.Length > 0)
            {
                parts.Add(qty + " wafers");
            }

            if (eqpId.Length > 0)
            {
                parts.Add(eqpId);
            }

            parts.Add(TableHelper.CellText(first, TrailingColumn));
            return string.Join(" · ", parts.ToArray());
        }

        internal static string HandlerSummary(DataTable equipments, string parentEqpId)
        {
            if (equipments == null || string.IsNullOrEmpty(parentEqpId))
            {
                return string.Empty;
            }

            List<string> parts = new List<string>();

            foreach (DataRow row in equipments.Rows)
            {
                if (!string.Equals(Schemas.MonitorEquipment.ParentEqpId.Text(row), parentEqpId, StringComparison.Ordinal))
                {
                    continue;
                }

                string site = ShortSiteId(Schemas.MonitorEquipment.EqpId.Text(row), parentEqpId);
                bool run = Schemas.MonitorEquipment.MesEqpStatCd.Is(row, Schemas.MonitorEquipment.MesEqpStatCd.Run)
                        || Schemas.MonitorEquipment.LotId.Text(row).Length > 0;
                bool down = Schemas.MonitorEquipment.MesEqpStatCd.Is(row, Schemas.MonitorEquipment.MesEqpStatCd.Down);

                if (down)
                {
                    parts.Add(site + " down");
                }
                else if (run)
                {
                    parts.Add(site + " run");
                }
            }

            return parts.Count == 0 ? "idle" : string.Join(" · ", parts.ToArray());
        }

        private static StageTileInfo Tile(Modern.Lab.WinForms.Controls.Data.FieldValue stage, string label, int count, string summary, string summaryKind, string lamp)
        {
            StageTileInfo tile = new StageTileInfo();
            tile.Key = stage.Value;
            tile.Label = label;
            tile.Count = count.ToString(CultureInfo.InvariantCulture);
            tile.Summary = summary;
            tile.SummaryKind = summaryKind;
            tile.LampState = lamp;
            return tile;
        }

        private static string LampOf(DataRow equipment)
        {
            return equipment == null ? string.Empty : Schemas.MonitorEquipment.MesEqpStatCd.Text(equipment);
        }

        private static string OldestText(DataTable lots, Modern.Lab.WinForms.Controls.Data.FieldValue stage)
        {
            if (lots == null)
            {
                return string.Empty;
            }

            long oldest = 0;

            foreach (DataRow row in lots.Select(StageFilter(stage)))
            {
                long seconds = row[WaitSecondsColumn] is long ? (long)row[WaitSecondsColumn] : 0L;

                if (seconds > oldest)
                {
                    oldest = seconds;
                }
            }

            return oldest == 0 ? string.Empty : "oldest " + Elapsed(DateTime.Now, DateTime.Now.AddSeconds(-oldest));
        }

        private static string FacsText(DataTable lots, Modern.Lab.WinForms.Controls.Data.FieldValue stage)
        {
            if (lots == null)
            {
                return string.Empty;
            }

            List<string> facs = new List<string>();

            foreach (DataRow row in lots.Select(StageFilter(stage)))
            {
                string fac = Schemas.MonitorLot.SendFac.Text(row);

                if (fac.Length > 0 && !facs.Contains(fac))
                {
                    facs.Add(fac);
                }
            }

            return facs.Count == 0 ? string.Empty : "from " + string.Join(" · ", facs.ToArray());
        }

        internal sealed class StageTileInfo
        {
            public string Key;
            public string Label;
            public string Count;
            public string Summary;
            public string SummaryKind;
            public string LampState;
        }

        internal sealed class StorageCounts
        {
            public int StbFoups;
            public int StoredFoups;
            public int MixedFoups;
            public int StockerTrays;
            public int FreeTrays;
        }
    }
}
