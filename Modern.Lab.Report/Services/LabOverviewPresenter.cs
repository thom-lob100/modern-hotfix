using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Theming;

namespace Modern.Lab.Report.Services
{
    internal static class LabOverviewPresenter
    {
        internal const string KeyColumn = "NODE_KEY";
        internal const string KindColumn = "NODE_KIND";
        internal const string ColumnColumn = "GRID_COL";
        internal const string RowColumn = "GRID_ROW";
        internal const string ColumnSpanColumn = "GRID_COL_SPAN";
        internal const string RowSpanColumn = "GRID_ROW_SPAN";
        internal const string TitleColumn = "NODE_TITLE";
        internal const string EquipmentColumn = "NODE_EQP";
        internal const string StateColumn = "NODE_STATE";
        internal const string ToneColumn = "NODE_TONE";
        internal const string CountColumn = "NODE_COUNT";
        internal const string CountWordColumn = "NODE_COUNT_WORD";
        internal const string SummaryColumn = "NODE_SUMMARY";
        internal const string HoldColumn = "NODE_HOLD";
        internal const string DetailColumn = "NODE_DETAIL";
        internal const string ToolTipColumn = "NODE_TOOLTIP";

        internal const string LinkFromColumn = "LINK_FROM";
        internal const string LinkToColumn = "LINK_TO";
        internal const string LinkStyleColumn = "LINK_STYLE";
        internal const string LinkLabelColumn = "LINK_LABEL";
        internal const string LinkToneColumn = "LINK_TONE";

        internal const string CellNodeColumn = "CELL_NODE";
        internal const string CellIdColumn = "CELL_ID";
        internal const string CellStateColumn = "CELL_STATE";
        internal const string CellLotColumn = "CELL_LOT";
        internal const string CellToneColumn = "CELL_TONE";

        internal const string KindTagColumn = "KIND_TAG";
        internal const string TimeOrderColumn = "TIME_ORDER";

        internal const string KeyReturn = "RETURN";
        internal const string KeySwap = "SWAP";
        internal const string KeyScrap = "SCRAP";

        internal const string TargetNode = "node";
        internal const string TargetAttention = "attention";

        internal const int AttentionLimit = 6;

        private const string attentionPrefix = "attention|";

        internal static readonly string[] LotColumns =
        {
            Schemas.MonitorLot.MonStage.Name, Schemas.MonitorLot.MonStatCd.Name, Schemas.MonitorLot.LotHoldStatCd.Name,
            Schemas.MonitorLot.ReworkYn.Name, Schemas.MonitorLot.ScrapDueYn.Name
        };

        internal static readonly string[] EquipmentColumns =
        {
            Schemas.MonitorEquipment.MonStage.Name, Schemas.MonitorEquipment.EqpId.Name,
            Schemas.MonitorEquipment.ParentEqpId.Name, Schemas.MonitorEquipment.MesEqpStatCd.Name
        };

        internal static readonly string[] DurableColumns =
        {
            Schemas.MonitorDurable.DurableTyp.Name, Schemas.MonitorDurable.LocKind.Name, Schemas.MonitorDurable.LotCnt.Name,
            Schemas.MonitorDurable.ReturnYn.Name, Schemas.MonitorDurable.SwapYn.Name
        };

        internal static DataTable NewNodeTable()
        {
            DataTable table = NewTable(KeyColumn, KindColumn, TitleColumn, EquipmentColumn, StateColumn, ToneColumn, CountColumn, CountWordColumn,
                    SummaryColumn, HoldColumn, DetailColumn, ToolTipColumn);
            table.Columns.Add(ColumnColumn, typeof(int));
            table.Columns.Add(RowColumn, typeof(int));
            table.Columns.Add(ColumnSpanColumn, typeof(int));
            table.Columns.Add(RowSpanColumn, typeof(int));
            return table;
        }

        internal static DataTable NewLinkTable()
        {
            return NewTable(LinkFromColumn, LinkToColumn, LinkStyleColumn, LinkLabelColumn, LinkToneColumn);
        }

        internal static DataTable NewCellTable()
        {
            return NewTable(CellNodeColumn, CellIdColumn, CellStateColumn, CellLotColumn, CellToneColumn);
        }

        internal static DataTable PendingNodes(DataTable layout)
        {
            DataTable nodes = layout.Copy();

            foreach (DataRow node in nodes.Rows)
            {
                node[CountColumn] = "-";
                node[CountWordColumn] = string.Empty;
                node[StateColumn] = string.Empty;
                node[ToneColumn] = string.Empty;
                node[SummaryColumn] = string.Empty;
                node[HoldColumn] = string.Empty;
                node[DetailColumn] = string.Empty;
                node[EquipmentColumn] = string.Empty;
                node[ToolTipColumn] = TableHelper.CellText(node, TitleColumn) + "\nNot loaded";
            }

            return nodes;
        }

        internal static DataTable Prepare(DataTable table, string[] required, string messageName)
        {
            DataTable source = table ?? new DataTable();

            if (source.Rows.Count == 0)
            {
                DataTable empty = source.Clone();
                empty.Locale = CultureInfo.InvariantCulture;

                foreach (string column in required)
                {
                    if (!empty.Columns.Contains(column))
                    {
                        empty.Columns.Add(column, typeof(string));
                    }
                }

                return empty;
            }

            List<string> missing = new List<string>();

            foreach (string column in required)
            {
                if (!source.Columns.Contains(column))
                {
                    missing.Add(column);
                }
            }

            if (missing.Count > 0)
            {
                throw new InvalidOperationException(messageName + " returned rows without the columns " + string.Join(", ", missing.ToArray()) + ".");
            }

            return source;
        }

        internal static Snapshot Build(DataTable lotRows, DataTable equipmentRows, DataTable durableRows, DataTable nodeLayout, DataTable linkLayout, DateTime now)
        {
            Snapshot snapshot = new Snapshot();
            snapshot.At = now;
            DataTable lots = Prepare(lotRows, LotColumns, "GetMonitorLotList");
            RequireExchangeFoups(lots);
            snapshot.Lots = LabMonitorPresenter.DecorateLots(lots, now);
            snapshot.Equipments = Prepare(equipmentRows, EquipmentColumns, "GetMonitorEqpList");
            snapshot.Durables = Prepare(durableRows, DurableColumns, "GetMonitorDurableList");
            snapshot.Cells = NewCellTable();
            snapshot.Nodes = BuildNodes(snapshot, nodeLayout, now);
            snapshot.Links = BuildLinks(snapshot.Lots, linkLayout);

            int total;
            snapshot.Attention = BuildAttention(snapshot.Lots, snapshot.Equipments, snapshot.Durables, now, out total);
            snapshot.AttentionTotal = total;
            snapshot.Storage = LabMonitorPresenter.CountStorage(snapshot.Durables);
            return snapshot;
        }

        internal static void RequireExchangeFoups(DataTable lots)
        {
            DataRow[] exchange = lots.Select(LabMonitorPresenter.StageFilter(Schemas.MonitorLot.MonStage.Exchange));

            if (exchange.Length == 0)
            {
                return;
            }

            bool hasColumn = lots.Columns.Contains(Schemas.MonitorLot.DurableId.Name);
            int empty = 0;

            foreach (DataRow lot in exchange)
            {
                if (!hasColumn || Schemas.MonitorLot.DurableId.Text(lot).Length == 0)
                {
                    empty++;
                }
            }

            if (empty > 0)
            {
                throw new InvalidOperationException("GetMonitorLotList returned " + Number(empty) + " Exchange rows without " + Schemas.MonitorLot.DurableId.Name
                        + (hasColumn ? " values." : " (the column is missing)."));
            }
        }

        private static DataTable BuildNodes(Snapshot snapshot, DataTable layout, DateTime now)
        {
            DataTable nodes = layout.Copy();
            MonitorFields.MonStage monStage = Schemas.MonitorLot.MonStage;
            Dictionary<string, LabMonitorPresenter.StageTileInfo> tiles = new Dictionary<string, LabMonitorPresenter.StageTileInfo>(StringComparer.Ordinal);

            foreach (LabMonitorPresenter.StageTileInfo tile in LabMonitorPresenter.StageTiles(snapshot.Lots, snapshot.Equipments, snapshot.Durables, now))
            {
                tiles[tile.Key] = tile;
            }

            foreach (DataRow node in nodes.Rows)
            {
                string key = TableHelper.CellText(node, KeyColumn);
                Clear(node);

                if (key == monStage.Request.Value || key == monStage.Send.Value || key == monStage.Arrival.Value)
                {
                    FillIncoming(node, snapshot.Lots, tiles, key);
                }
                else if (key == monStage.Exchange.Value)
                {
                    FillExchange(node, snapshot.Lots);
                }
                else if (key == monStage.Prep.Value)
                {
                    FillEquipment(node, snapshot, monStage.Prep, now, false);
                }
                else if (key == monStage.Fib.Value)
                {
                    FillEquipment(node, snapshot, monStage.Fib, now, true);
                }
                else if (key == monStage.Tem.Value)
                {
                    FillEquipment(node, snapshot, monStage.Tem, now, false);
                }
                else if (key == monStage.Done.Value)
                {
                    FillDone(node, snapshot.Lots);
                }
                else if (key == KeyReturn)
                {
                    FillReturn(node, snapshot.Durables);
                }
                else if (key == KeySwap)
                {
                    FillSwap(node, snapshot.Durables);
                }
                else if (key == KeyScrap)
                {
                    FillScrap(node, snapshot.Lots);
                }
            }

            return nodes;
        }

        private static void Clear(DataRow node)
        {
            foreach (string column in new string[] { EquipmentColumn, StateColumn, ToneColumn, CountColumn, CountWordColumn, SummaryColumn, HoldColumn, DetailColumn, ToolTipColumn })
            {
                node[column] = string.Empty;
            }
        }

        private static void FillIncoming(DataRow node, DataTable lots, Dictionary<string, LabMonitorPresenter.StageTileInfo> tiles, string key)
        {
            Modern.Lab.WinForms.Controls.Data.FieldValue stage = StageOf(key);
            int count = LabMonitorPresenter.CountRows(lots, LabMonitorPresenter.StageFilter(stage));
            bool request = key == Schemas.MonitorLot.MonStage.Request.Value;
            string word = request ? Plural(count, "request", "requests") : Plural(count, "record", "records");
            LabMonitorPresenter.StageTileInfo tile;
            string summary = tiles.TryGetValue(key, out tile) ? tile.Summary : string.Empty;

            node[CountColumn] = Number(count);
            node[CountWordColumn] = word;
            node[SummaryColumn] = summary;
            node[HoldColumn] = HoldText(lots, LabMonitorPresenter.StageFilter(stage));
            node[ToolTipColumn] = TableHelper.CellText(node, TitleColumn) + "  " + Number(count) + " " + word
                    + (summary.Length == 0 ? string.Empty : "\n" + summary)
                    + (request ? string.Empty : "\nRows received, so one request can show more than once");
        }

        private static void FillExchange(DataRow node, DataTable lots)
        {
            Modern.Lab.WinForms.Controls.Data.FieldValue stage = Schemas.MonitorLot.MonStage.Exchange;
            int foups = LabMonitorPresenter.FoupCount(lots);
            int lotCount = LabMonitorPresenter.CountRows(lots, LabMonitorPresenter.StageFilter(stage));

            node[CountColumn] = Number(foups);
            node[CountWordColumn] = "FOUP";
            node[SummaryColumn] = Lots(lotCount);
            node[HoldColumn] = HoldText(lots, LabMonitorPresenter.StageFilter(stage));
            node[ToolTipColumn] = "Exchange  " + Number(foups) + " FOUP\nLots  " + Number(lotCount);
        }

        private static void FillEquipment(DataRow node, Snapshot snapshot, Modern.Lab.WinForms.Controls.Data.FieldValue stage, DateTime now, bool handler)
        {
            DataTable lots = snapshot.Lots;
            DataRow equipment = LabMonitorPresenter.FindEquipment(snapshot.Equipments, stage);
            string eqpId = equipment == null ? string.Empty : Schemas.MonitorEquipment.EqpId.Text(equipment);
            string status = equipment == null ? string.Empty : Schemas.MonitorEquipment.MesEqpStatCd.Text(equipment);
            bool run = equipment != null && Schemas.MonitorEquipment.MesEqpStatCd.Is(equipment, Schemas.MonitorEquipment.MesEqpStatCd.Run);
            bool down = equipment != null && Schemas.MonitorEquipment.MesEqpStatCd.Is(equipment, Schemas.MonitorEquipment.MesEqpStatCd.Down);
            DateTime since = DateTime.MinValue;
            bool hasSince = equipment != null && LabMonitorPresenter.TryParseTime(Schemas.MonitorEquipment.StatusTm.Text(equipment), out since);
            int count = LabMonitorPresenter.CountRows(lots, LabMonitorPresenter.StageFilter(stage));
            int job = LabMonitorPresenter.CountRows(lots, LabMonitorPresenter.RunningFilter(stage));
            int queue = LabMonitorPresenter.CountRows(lots, LabMonitorPresenter.QueueFilter(stage));
            List<string> downSites = new List<string>();

            if (handler && eqpId.Length > 0)
            {
                foreach (DataRow site in LabMonitorPresenter.HandlerSiteRows(snapshot.Equipments, eqpId).Rows)
                {
                    string siteId = TableHelper.CellText(site, LabMonitorPresenter.SiteIdColumn);
                    string siteState = TableHelper.CellText(site, LabMonitorPresenter.SiteStateColumn);
                    string lotId = Schemas.MonitorEquipment.LotId.Text(site);
                    string reserved = Schemas.MonitorEquipment.ReservedLotId.Text(site);
                    string shown = siteState.Length > 0 ? siteState : (lotId.Length > 0 ? Schemas.MonitorEquipment.MesEqpStatCd.Run.Value : Schemas.MonitorEquipment.MesEqpStatCd.Idle.Value);
                    bool siteDown = shown == Schemas.MonitorEquipment.MesEqpStatCd.Down.Value;
                    bool siteRun = shown == Schemas.MonitorEquipment.MesEqpStatCd.Run.Value;

                    DataRow cell = snapshot.Cells.NewRow();
                    cell[CellNodeColumn] = stage.Value;
                    cell[CellIdColumn] = siteId;
                    cell[CellStateColumn] = shown;
                    cell[CellLotColumn] = lotId + (reserved.Length == 0 ? string.Empty : (lotId.Length == 0 ? string.Empty : " ") + "(" + reserved + ")");
                    cell[CellToneColumn] = siteDown ? SemanticKind.Error.ToString() : siteRun ? SemanticKind.Success.ToString() : SemanticKind.Neutral.ToString();
                    snapshot.Cells.Rows.Add(cell);

                    if (siteDown)
                    {
                        downSites.Add(siteId + " down");
                    }
                }
            }

            string state = status.Length == 0 ? "-" : status;

            if (downSites.Count > 0)
            {
                state += " · " + string.Join(" · ", downSites.ToArray());
            }
            else if (hasSince)
            {
                state += " · " + LabMonitorPresenter.Elapsed(now, since);
            }

            node[EquipmentColumn] = eqpId.Length == 0 ? "No equipment" : eqpId;
            node[StateColumn] = state;
            node[ToneColumn] = run ? SemanticKind.Success.ToString() : down ? SemanticKind.Error.ToString() : SemanticKind.Neutral.ToString();
            node[CountColumn] = Number(count);
            node[CountWordColumn] = count == 1 ? "lot" : "lots";
            node[SummaryColumn] = "Job " + Lots(job) + " · Queue " + Lots(queue);
            node[HoldColumn] = HoldText(lots, LabMonitorPresenter.StageFilter(stage));
            node[DetailColumn] = handler ? QueueDetail(lots, stage) : RunningDetail(lots, stage) + "\n\n" + QueueDetail(lots, stage);
            node[ToolTipColumn] = TableHelper.CellText(node, TitleColumn) + "  " + (eqpId.Length == 0 ? "No equipment" : eqpId)
                    + "\nState  " + state
                    + "\nLots  " + Number(count) + " (Job " + Number(job) + " · Queue " + Number(queue) + ")"
                    + "\nHold  " + Number(HoldCount(lots, LabMonitorPresenter.StageFilter(stage)))
                    + (handler ? "\nSites  " + SitesText(snapshot, stage.Value) : "\nRunning  " + RunningText(lots, stage))
                    + "\nNext  " + NextText(lots, stage)
                    + "\nQueue  " + QueueNames(lots, stage);
        }

        private static void FillDone(DataRow node, DataTable lots)
        {
            int count = LabMonitorPresenter.CountRows(lots, LabMonitorPresenter.DoneFilter());
            node[CountColumn] = Number(count);
            node[CountWordColumn] = count == 1 ? "lot" : "lots";
            node[SummaryColumn] = "current";
            node[HoldColumn] = HoldText(lots, LabMonitorPresenter.DoneFilter());
            node[ToolTipColumn] = "Done  " + Lots(count) + "\nLots in Done now, not today's output";
        }

        private static void FillReturn(DataRow node, DataTable durables)
        {
            int count = CountDurables(durables, true);
            node[CountColumn] = Number(count);
            node[CountWordColumn] = "FOUP";
            node[SummaryColumn] = "to return";
            node[ToneColumn] = count > 0 ? SemanticKind.Warning.ToString() : string.Empty;
            node[ToolTipColumn] = "Return  " + Number(count) + " FOUP\nFOUP from other fabs not returned yet";
        }

        private static void FillSwap(DataRow node, DataTable durables)
        {
            int count = CountDurables(durables, false);
            node[CountColumn] = Number(count);
            node[CountWordColumn] = "FOUP";
            node[SummaryColumn] = "swapped";
            node[ToolTipColumn] = "Swap  " + Number(count) + " FOUP\nFOUP swapped at exchange";
        }

        private static void FillScrap(DataRow node, DataTable lots)
        {
            int count = LabMonitorPresenter.CountRows(lots, LabMonitorPresenter.ScrapDueFilter());
            node[CountColumn] = Number(count);
            node[CountWordColumn] = count == 1 ? "lot" : "lots";
            node[SummaryColumn] = "candidates";
            node[ToneColumn] = count > 0 ? SemanticKind.Warning.ToString() : string.Empty;
            node[ToolTipColumn] = "Scrap due  " + Lots(count) + "\nScrap candidates, not scrapped yet";
        }

        private static DataTable BuildLinks(DataTable lots, DataTable layout)
        {
            DataTable links = layout.Copy();
            int rework = LabMonitorPresenter.CountRows(lots, LabMonitorPresenter.ReworkFilter());

            foreach (DataRow link in links.Rows)
            {
                if (TableHelper.CellText(link, LinkStyleColumn) != Modern.Lab.Controls.Wpf.Display.ProcessMapLinkStyle.Rework.ToString())
                {
                    continue;
                }

                link[LinkLabelColumn] = rework > 0 ? "Rework · " + Lots(rework) : string.Empty;
                link[LinkToneColumn] = rework > 0 ? SemanticKind.Error.ToString() : string.Empty;
            }

            return links;
        }

        private static DataTable BuildAttention(DataTable lots, DataTable equipments, DataTable durables, DateTime now, out int total)
        {
            DataTable source = LabMonitorPresenter.AttentionRows(lots, equipments, durables, now);
            DataTable result = source.Clone();
            result.Columns.Add(KindTagColumn, typeof(string));
            result.Columns.Add(TimeOrderColumn, typeof(int));
            Dictionary<string, DataRow> lotByKey = new Dictionary<string, DataRow>(StringComparer.Ordinal);

            foreach (DataRow lot in lots.Rows)
            {
                lotByKey[TableHelper.CellText(lot, LabMonitorPresenter.RowKeyColumn)] = lot;
            }

            foreach (DataRow row in source.Rows)
            {
                DataRow copy = result.NewRow();

                foreach (DataColumn column in source.Columns)
                {
                    copy[column.ColumnName] = row[column];
                }

                string kind = TableHelper.CellText(row, LabMonitorPresenter.RowKindColumn);
                string trailing = TableHelper.CellText(row, LabMonitorPresenter.TrailingColumn);
                bool hasTime = trailing.Length > 0;

                if (kind == LabMonitorPresenter.KindLot)
                {
                    string rowKey = TableHelper.CellText(row, LabMonitorPresenter.RowKeyColumn);
                    DataRow lot;
                    lotByKey.TryGetValue(rowKey.StartsWith(attentionPrefix, StringComparison.Ordinal) ? rowKey.Substring(attentionPrefix.Length) : rowKey, out lot);
                    bool hold = lot != null && Schemas.MonitorLot.LotHoldStatCd.Is(lot, Schemas.MonitorLot.LotHoldStatCd.OnHold);
                    copy[KindTagColumn] = hold ? "Hold" : "Rework";

                    if (lot != null)
                    {
                        copy[LabMonitorPresenter.BadgeTextColumn] = TableHelper.CellText(lot, LabMonitorPresenter.BadgeTextColumn);
                        copy[LabMonitorPresenter.BadgeKindColumn] = TableHelper.CellText(lot, LabMonitorPresenter.BadgeKindColumn);
                    }

                    copy[LabMonitorPresenter.TrailingColumn] = hasTime ? "in stage " + trailing : string.Empty;
                }
                else if (kind == LabMonitorPresenter.KindEquipment)
                {
                    copy[KindTagColumn] = "Down";
                    copy[LabMonitorPresenter.BadgeTextColumn] = string.Empty;
                    copy[LabMonitorPresenter.TrailingColumn] = hasTime ? "down " + trailing : string.Empty;
                }
                else if (kind == LabMonitorPresenter.KindTray)
                {
                    hasTime = false;
                    copy[KindTagColumn] = "Tray";
                    copy[LabMonitorPresenter.TitleColumn] = "Stocker";
                    copy[LabMonitorPresenter.BadgeTextColumn] = string.Empty;
                    copy[LabMonitorPresenter.TrailingColumn] = string.Empty;
                }
                else
                {
                    hasTime = false;
                    copy[KindTagColumn] = "To Fab";
                    copy[LabMonitorPresenter.BadgeTextColumn] = string.Empty;
                    copy[LabMonitorPresenter.TrailingColumn] = string.Empty;
                }

                long wait = row[LabMonitorPresenter.WaitSecondsColumn] is long ? (long)row[LabMonitorPresenter.WaitSecondsColumn] : 0L;
                copy[LabMonitorPresenter.WaitSecondsColumn] = hasTime ? Math.Max(0L, wait) : 0L;
                copy[TimeOrderColumn] = hasTime ? 0 : 1;
                result.Rows.Add(copy);
            }

            DataView sorted = new DataView(result, string.Empty,
                    TimeOrderColumn + " ASC, " + LabMonitorPresenter.WaitSecondsColumn + " DESC, " + LabMonitorPresenter.TitleColumn + " ASC",
                    DataViewRowState.CurrentRows);
            DataTable top = result.Clone();
            total = sorted.Count;

            for (int index = 0; index < sorted.Count && index < AttentionLimit; index++)
            {
                top.ImportRow(sorted[index].Row);
            }

            return top;
        }

        internal static SelectedView Selected(Snapshot snapshot, string target, string key)
        {
            if (snapshot == null || string.IsNullOrEmpty(key))
            {
                return null;
            }

            if (target == TargetAttention)
            {
                return SelectedAttention(snapshot, key);
            }

            DataRow node = FindRow(snapshot.Nodes, KeyColumn, key);
            return node == null ? null : SelectedNode(snapshot, node, key);
        }

        private static SelectedView SelectedNode(Snapshot snapshot, DataRow node, string key)
        {
            MonitorFields.MonStage monStage = Schemas.MonitorLot.MonStage;
            DataTable lots = snapshot.Lots;
            SelectedView view = new SelectedView();
            string title = TableHelper.CellText(node, TitleColumn);
            string count = TableHelper.CellText(node, CountColumn) + " " + TableHelper.CellText(node, CountWordColumn);

            if (key == monStage.Prep.Value || key == monStage.Fib.Value || key == monStage.Tem.Value)
            {
                Modern.Lab.WinForms.Controls.Data.FieldValue value = StageOf(key);
                view.Title = title + " · " + TableHelper.CellText(node, EquipmentColumn);

                if (key == monStage.Fib.Value)
                {
                    view.Add("Sites", SitesText(snapshot, key), 2);
                }
                else
                {
                    view.Add("Running", RunningText(lots, value), 2);
                }

                view.Add("Next", NextText(lots, value), 2);
                view.Add("State", TableHelper.CellText(node, StateColumn), 1);
                view.Add("Hold", Lots(HoldCount(lots, LabMonitorPresenter.StageFilter(value))), 1);
                view.Add("Queue", Lots(LabMonitorPresenter.CountRows(lots, LabMonitorPresenter.QueueFilter(value))), 1);
                view.Add("Oldest wait", OldestWait(lots, LabMonitorPresenter.QueueFilter(value)), 1);
                return view;
            }

            if (key == KeyReturn || key == KeySwap)
            {
                bool returns = key == KeyReturn;
                view.Title = title;
                view.Add(returns ? "To return" : "Swapped", DurableList(snapshot.Durables, returns), 2);
                view.Add("Count", count, 1);
                view.Add("Meaning", returns ? "Not returned to the fab yet" : "Swapped at exchange", 2);
                return view;
            }

            if (key == KeyScrap)
            {
                view.Title = title;
                view.Add("Lots", LotList(lots, LabMonitorPresenter.ScrapDueFilter()), 2);
                view.Add("Count", count, 1);
                view.Add("Meaning", "Scrap candidates, not scrapped yet", 2);
                return view;
            }

            Modern.Lab.WinForms.Controls.Data.FieldValue flow = StageOf(key);
            string filter = key == monStage.Done.Value ? LabMonitorPresenter.DoneFilter() : LabMonitorPresenter.StageFilter(flow);
            view.Title = title;
            view.Add("Oldest", OldestRow(lots, filter), 2);
            view.Add("Count", count, 1);
            view.Add("Hold", Lots(HoldCount(lots, filter)), 1);
            view.Add("Summary", TableHelper.CellText(node, SummaryColumn).Length == 0 ? "-" : TableHelper.CellText(node, SummaryColumn), 2);
            return view;
        }

        private static SelectedView SelectedAttention(Snapshot snapshot, string key)
        {
            DataRow row = FindRow(snapshot.Attention, LabMonitorPresenter.RowKeyColumn, key);

            if (row == null)
            {
                return null;
            }

            SelectedView view = new SelectedView();
            string kind = TableHelper.CellText(row, LabMonitorPresenter.RowKindColumn);
            string tag = TableHelper.CellText(row, KindTagColumn);

            if (kind == LabMonitorPresenter.KindLot)
            {
                DataRow lot = FindRow(snapshot.Lots, LabMonitorPresenter.RowKeyColumn, key.StartsWith(attentionPrefix, StringComparison.Ordinal) ? key.Substring(attentionPrefix.Length) : key);
                string lotId = Schemas.MonitorLot.LotId.Text(row);
                view.Title = tag + " · " + (lotId.Length == 0 ? TableHelper.CellText(row, LabMonitorPresenter.TitleColumn) : lotId);
                view.Add("Request", Dash(Schemas.MonitorLot.ReqSerialNo.Text(row)), 1);
                view.Add("Lot", Dash(lotId), 1);
                view.Add("Stage", lot == null ? "-" : LabMonitorPresenter.StageLabel(Schemas.MonitorLot.MonStage.Text(lot)), 1);
                view.Add("State", Dash(TableHelper.CellText(row, LabMonitorPresenter.BadgeTextColumn)), 1);
                view.Add("Waiting", Dash(TableHelper.CellText(row, LabMonitorPresenter.TrailingColumn)), 1);
                view.Add("Rework", lot != null && Schemas.MonitorLot.ReworkYn.Is(lot, Schemas.MonitorLot.ReworkYn.Y) ? "RW " + Schemas.MonitorLot.ReworkCnt.Text(lot) : "No", 1);
                return view;
            }

            if (kind == LabMonitorPresenter.KindEquipment)
            {
                string eqpId = Schemas.MonitorEquipment.EqpId.Text(row);
                DataRow equipment = FindRow(snapshot.Equipments, Schemas.MonitorEquipment.EqpId.Name, eqpId);
                view.Title = tag + " · " + eqpId;
                view.Add("Equipment", equipment == null ? eqpId : eqpId + " · " + Dash(Schemas.MonitorEquipment.EqpNm.Text(equipment)), 2);
                view.Add("Stage", equipment == null ? "-" : LabMonitorPresenter.StageLabel(Schemas.MonitorEquipment.MonStage.Text(equipment)), 1);
                view.Add("Down for", Dash(TableHelper.CellText(row, LabMonitorPresenter.TrailingColumn)), 1);
                view.Add("Since", equipment == null ? "-" : Dash(Schemas.MonitorEquipment.StatusTm.Text(equipment)), 1);
                view.Add("Lot", equipment == null || Schemas.MonitorEquipment.LotId.Text(equipment).Length == 0 ? "No lot" : Schemas.MonitorEquipment.LotId.Text(equipment), 1);
                return view;
            }

            if (kind == LabMonitorPresenter.KindTray)
            {
                view.Title = tag + " · Stocker";
                view.Add("Trays", Number(snapshot.Storage.StockerTrays), 1);
                view.Add("Free", Number(snapshot.Storage.FreeTrays), 1);
                view.Add("Meaning", "No free tray in the stocker", 2);
                return view;
            }

            string durableId = Schemas.MonitorDurable.DurableId.Text(row);
            DataRow durable = FindRow(snapshot.Durables, Schemas.MonitorDurable.DurableId.Name, durableId);
            view.Title = tag + " · " + durableId;
            view.Add("FOUP", Dash(durableId), 1);
            view.Add("From Fab", durable == null ? "-" : Dash(Schemas.MonitorDurable.FromFac.Text(durable)), 1);
            view.Add("Location", durable == null ? "-" : Dash(Schemas.MonitorDurable.LocNm.Text(durable)), 2);
            view.Add("Lots", durable == null ? "-" : Dash(Schemas.MonitorDurable.LotCnt.Text(durable)), 1);
            return view;
        }

        internal static bool ContainsAttention(Snapshot snapshot, string key)
        {
            return snapshot != null && FindRow(snapshot.Attention, LabMonitorPresenter.RowKeyColumn, key) != null;
        }

        private static string RunningDetail(DataTable lots, Modern.Lab.WinForms.Controls.Data.FieldValue stage)
        {
            DataRow[] running = lots.Select(LabMonitorPresenter.RunningFilter(stage), LabMonitorPresenter.PrioritySortColumn);

            if (running.Length == 0)
            {
                return "Running\nNo job";
            }

            string qty = Schemas.MonitorLot.Qty.Text(running[0]);
            string trailing = TableHelper.CellText(running[0], LabMonitorPresenter.TrailingColumn);
            List<string> parts = new List<string>();

            if (qty.Length > 0)
            {
                parts.Add("Qty " + qty);
            }

            if (trailing.Length > 0)
            {
                parts.Add(trailing);
            }

            return "Running\n" + LotName(running[0]) + (running.Length > 1 ? " +" + Number(running.Length - 1) : string.Empty)
                    + (parts.Count == 0 ? string.Empty : "\n" + string.Join(" · ", parts.ToArray()));
        }

        private static string QueueDetail(DataTable lots, Modern.Lab.WinForms.Controls.Data.FieldValue stage)
        {
            DataRow[] queue = QueueRows(lots, stage);

            if (queue.Length == 0)
            {
                return "Queue\nEmpty";
            }

            List<string> lines = new List<string>();
            lines.Add("Queue");

            for (int index = 0; index < queue.Length && index < 2; index++)
            {
                lines.Add(LotName(queue[index]) + " · " + TableHelper.CellText(queue[index], LabMonitorPresenter.BadgeTextColumn));
            }

            if (queue.Length > 2)
            {
                lines.Add("+" + Number(queue.Length - 2) + " more");
            }

            return string.Join("\n", lines.ToArray());
        }

        private static DataRow[] QueueRows(DataTable lots, Modern.Lab.WinForms.Controls.Data.FieldValue stage)
        {
            return lots.Select(LabMonitorPresenter.QueueFilter(stage), LabMonitorPresenter.PrioritySortColumn + ", " + LabMonitorPresenter.WaitSecondsColumn + " DESC");
        }

        private static string RunningText(DataTable lots, Modern.Lab.WinForms.Controls.Data.FieldValue stage)
        {
            DataRow[] running = lots.Select(LabMonitorPresenter.RunningFilter(stage), LabMonitorPresenter.PrioritySortColumn);

            if (running.Length == 0)
            {
                return "No job";
            }

            string qty = Schemas.MonitorLot.Qty.Text(running[0]);
            return FullName(running[0]) + (qty.Length == 0 ? string.Empty : " · Qty " + qty)
                    + (running.Length > 1 ? " · +" + Number(running.Length - 1) : string.Empty);
        }

        private static string NextText(DataTable lots, Modern.Lab.WinForms.Controls.Data.FieldValue stage)
        {
            List<DataRow> next = new List<DataRow>();

            foreach (DataRow row in QueueRows(lots, stage))
            {
                if (Schemas.MonitorLot.MonStatCd.Is(row, Schemas.MonitorLot.MonStatCd.Next))
                {
                    next.Add(row);
                }
            }

            return next.Count == 0 ? "-" : FullName(next[0]) + (next.Count > 1 ? " +" + Number(next.Count - 1) : string.Empty);
        }

        private static string QueueNames(DataTable lots, Modern.Lab.WinForms.Controls.Data.FieldValue stage)
        {
            DataRow[] queue = QueueRows(lots, stage);
            List<string> names = new List<string>();

            for (int index = 0; index < queue.Length && index < 3; index++)
            {
                names.Add(FullName(queue[index]) + " · " + TableHelper.CellText(queue[index], LabMonitorPresenter.BadgeTextColumn));
            }

            if (queue.Length > 3)
            {
                names.Add("+" + Number(queue.Length - 3) + " more");
            }

            return names.Count == 0 ? "Empty" : string.Join("\n         ", names.ToArray());
        }

        private static string SitesText(Snapshot snapshot, string key)
        {
            List<string> parts = new List<string>();

            foreach (DataRow cell in snapshot.Cells.Rows)
            {
                if (TableHelper.CellText(cell, CellNodeColumn) != key)
                {
                    continue;
                }

                string lot = TableHelper.CellText(cell, CellLotColumn);
                parts.Add(TableHelper.CellText(cell, CellIdColumn) + " " + TableHelper.CellText(cell, CellStateColumn) + (lot.Length == 0 ? string.Empty : " " + lot));
            }

            return parts.Count == 0 ? "-" : string.Join(" · ", parts.ToArray());
        }

        private static string OldestWait(DataTable lots, string filter)
        {
            DataRow oldest = OldestOf(lots, filter);
            return oldest == null ? "-" : Dash(TableHelper.CellText(oldest, LabMonitorPresenter.TrailingColumn));
        }

        private static string OldestRow(DataTable lots, string filter)
        {
            DataRow oldest = OldestOf(lots, filter);

            if (oldest == null)
            {
                return "-";
            }

            string trailing = TableHelper.CellText(oldest, LabMonitorPresenter.TrailingColumn);
            return FullName(oldest) + (trailing.Length == 0 ? string.Empty : " · " + trailing);
        }

        private static DataRow OldestOf(DataTable lots, string filter)
        {
            DataRow[] rows = lots.Select(filter, LabMonitorPresenter.WaitSecondsColumn + " DESC");
            return rows.Length == 0 ? null : rows[0];
        }

        private static string LotList(DataTable lots, string filter)
        {
            DataRow[] rows = lots.Select(filter, LabMonitorPresenter.PrioritySortColumn);
            List<string> names = new List<string>();

            for (int index = 0; index < rows.Length && index < 4; index++)
            {
                names.Add(LotName(rows[index]));
            }

            if (rows.Length > 4)
            {
                names.Add("+" + Number(rows.Length - 4));
            }

            return names.Count == 0 ? "-" : string.Join(", ", names.ToArray());
        }

        private static string DurableList(DataTable durables, bool returns)
        {
            List<string> names = new List<string>();
            int count = 0;

            foreach (DataRow durable in durables.Rows)
            {
                if (!MatchesDurable(durable, returns))
                {
                    continue;
                }

                count++;

                if (names.Count < 4)
                {
                    names.Add(Schemas.MonitorDurable.DurableId.Text(durable));
                }
            }

            if (count > names.Count)
            {
                names.Add("+" + Number(count - names.Count));
            }

            return names.Count == 0 ? "-" : string.Join(", ", names.ToArray());
        }

        private static int CountDurables(DataTable durables, bool returns)
        {
            int count = 0;

            foreach (DataRow durable in durables.Rows)
            {
                if (MatchesDurable(durable, returns))
                {
                    count++;
                }
            }

            return count;
        }

        private static bool MatchesDurable(DataRow durable, bool returns)
        {
            if (returns)
            {
                return Schemas.MonitorDurable.DurableTyp.Is(durable, Schemas.MonitorDurable.DurableTyp.Foup)
                        && Schemas.MonitorDurable.ReturnYn.Is(durable, Schemas.MonitorDurable.ReturnYn.N);
            }

            return Schemas.MonitorDurable.SwapYn.Is(durable, Schemas.MonitorDurable.SwapYn.Y);
        }

        private static string HoldText(DataTable lots, string filter)
        {
            int count = HoldCount(lots, filter);
            return count == 0 ? string.Empty : Lots(count);
        }

        private static int HoldCount(DataTable lots, string filter)
        {
            return LabMonitorPresenter.CountRows(lots, "(" + filter + ") AND " + Schemas.MonitorLot.LotHoldStatCd.Name
                    + " = '" + Schemas.MonitorLot.LotHoldStatCd.OnHold.Value + "'");
        }

        private static string LotName(DataRow lot)
        {
            string lotId = Schemas.MonitorLot.LotId.Text(lot);
            return lotId.Length > 0 ? lotId : TableHelper.CellText(lot, LabMonitorPresenter.TitleColumn);
        }

        private static string FullName(DataRow lot)
        {
            string request = Schemas.MonitorLot.ReqSerialNo.Text(lot);
            string lotId = Schemas.MonitorLot.LotId.Text(lot);

            if (request.Length == 0)
            {
                return Dash(lotId);
            }

            return lotId.Length == 0 ? request : request + " · " + lotId;
        }

        private static Modern.Lab.WinForms.Controls.Data.FieldValue StageOf(string key)
        {
            foreach (Modern.Lab.WinForms.Controls.Data.FieldValue value in Schemas.MonitorLot.MonStage.Flow)
            {
                if (value.Value == key)
                {
                    return value;
                }
            }

            return Schemas.MonitorLot.MonStage.Request;
        }

        private static DataRow FindRow(DataTable table, string column, string value)
        {
            if (table == null || !table.Columns.Contains(column) || string.IsNullOrEmpty(value))
            {
                return null;
            }

            foreach (DataRow row in table.Rows)
            {
                if (string.Equals(TableHelper.CellText(row, column), value, StringComparison.Ordinal))
                {
                    return row;
                }
            }

            return null;
        }

        private static DataTable NewTable(params string[] columns)
        {
            DataTable table = new DataTable();
            table.Locale = CultureInfo.InvariantCulture;

            foreach (string column in columns)
            {
                table.Columns.Add(column, typeof(string));
            }

            return table;
        }

        private static string Plural(int count, string one, string many)
        {
            return count == 1 ? one : many;
        }

        private static string Lots(int count)
        {
            return Number(count) + (count == 1 ? " lot" : " lots");
        }

        private static string Number(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string Dash(string value)
        {
            return string.IsNullOrEmpty(value) ? "-" : value;
        }

        internal sealed class Snapshot
        {
            public DateTime At;
            public DataTable Lots;
            public DataTable Equipments;
            public DataTable Durables;
            public DataTable Nodes;
            public DataTable Links;
            public DataTable Cells;
            public DataTable Attention;
            public int AttentionTotal;
            public LabMonitorPresenter.StorageCounts Storage;
        }

        internal sealed class SelectedLine
        {
            public string Caption;
            public string Value;
            public int Span;
        }

        internal sealed class SelectedView
        {
            public string Title = string.Empty;
            public readonly List<SelectedLine> Lines = new List<SelectedLine>();

            public void Add(string caption, string value, int span)
            {
                SelectedLine line = new SelectedLine();
                line.Caption = caption;
                line.Value = string.IsNullOrEmpty(value) ? "-" : value;
                line.Span = span;
                this.Lines.Add(line);
            }
        }
    }
}
