using System;
using System.Collections.Generic;
using System.Data;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting.ResponseContracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.Samples.Contracts;
using Modern.Lab.Hosting.NewItems;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples.Services
{

    public static class DurableManagementPresenter
    {

        public const string ActionCreate = "Create";
        public const string ActionHold = "Hold";
        public const string ActionRelease = "Release";
        public const string ActionAdjust = "Adjust";
        public const string ActionRfId = "RfId";
        public const string ActionExchange = "Exchange";
        public const string ActionEdit = "Edit";
        public const string ActionDeassign = "Deassign";
        public const string ActionScrap = "Scrap";
        public const string ActionScrapCancel = "ScrapCancel";
        public const string ActionTransport = "Transport";

        public const string ScrapCancelLabel = "Scrap Cancel";
        public const string RegisterGroupLabel = "Register";
        public const string MoveWaferGroupLabel = "Move Wafer";

        public const string MenuValueColumn = "VALUE";
        public const string MenuLabelColumn = "LABEL";
        public const string MenuEnabledColumn = "CAN";
        public const string MenuGlyphColumn = "GLYPH";
        public const string MenuAccentColumn = "ACCENT";

        public static readonly IList<DurableAction> Actions =
                new List<DurableAction>
                {
                    new DurableAction(ActionCreate, "Create", false),
                    new DurableAction(ActionHold, "Hold", true),
                    new DurableAction(ActionRelease, "Release", true),
                    new DurableAction(ActionAdjust, "Adjust", true),
                    new DurableAction(ActionRfId, "RFID", true),
                    new DurableAction(ActionExchange, "Exchange", true),
                    new DurableAction(ActionEdit, "Edit", true),
                    new DurableAction(ActionDeassign, "Deassign", true),
                    new DurableAction(ActionScrap, "Scrap", true),
                    new DurableAction(ActionScrapCancel, ScrapCancelLabel, true),
                    new DurableAction(ActionTransport, "Transport", true)
                }.AsReadOnly();

        public static readonly IList<DurableAction> RegisterActions =
                new List<DurableAction>
                {
                    Of(ActionCreate),
                    Of(ActionRfId),
                    Of(ActionAdjust)
                }.AsReadOnly();

        public static readonly IList<DurableAction> HoldActions =
                new List<DurableAction>
                {
                    Of(ActionHold),
                    Of(ActionRelease)
                }.AsReadOnly();

        public static readonly IList<DurableAction> MoveWaferActions =
                new List<DurableAction>
                {
                    Of(ActionExchange),
                    Of(ActionEdit),
                    Of(ActionDeassign)
                }.AsReadOnly();

        public static readonly IList<DurableAction> ScrapActions =
                new List<DurableAction>
                {
                    Of(ActionScrap),
                    Of(ActionScrapCancel)
                }.AsReadOnly();

        public static DataTable BuildGatedMenuTable(IList<DurableAction> actions, Func<string, bool> canExecute)
        {
            DataTable table = new DataTable();
            table.Locale = System.Globalization.CultureInfo.InvariantCulture;
            table.Columns.Add(MenuValueColumn, typeof(string));
            table.Columns.Add(MenuLabelColumn, typeof(string));
            table.Columns.Add(MenuEnabledColumn, typeof(bool));
            table.Columns.Add(MenuGlyphColumn, typeof(string));
            table.Columns.Add(MenuAccentColumn, typeof(string));

            foreach (DurableAction action in actions)
            {
                table.Rows.Add(action.Key, action.Label, canExecute(action.Key),
                        MenuGlyphs.For(action.Label), MenuGlyphs.AccentOf(action.Label));
            }

            return table;
        }

        public static DurableAction Of(string key)
        {
            foreach (DurableAction action in Actions)
            {
                if (action.Key == key)
                {
                    return action;
                }
            }

            return null;
        }

        public static bool IsActionKey(string key)
        {
            return Of(key) != null;
        }

        public static string Type(DataRow durable)
        {
            return TableHelper.CellText(durable, Schemas.Durable.DurableTyp.Name).Trim();
        }

        public static string Status(DataRow durable)
        {
            return Schemas.Durable.WfLoadStatCd.Text(durable);
        }

        public static string Id(DataRow durable)
        {
            return TableHelper.CellText(durable, Schemas.Durable.DurableId.Name).Trim();
        }

        public static string Location(DataRow durable)
        {
            return TableHelper.CellText(durable, Schemas.Durable.LocNm.Name).Trim();
        }

        public static DataTable TransportTargets(DataTable locations, string idColumn, string currentLocationId)
        {
            DataTable targets = locations == null ? new DataTable() : locations.Clone();

            if (locations == null || string.IsNullOrEmpty(idColumn))
            {
                return targets;
            }

            string current = (currentLocationId ?? string.Empty).Trim();

            foreach (DataRow row in locations.Rows)
            {
                if (current.Length > 0 && TableHelper.CellText(row, idColumn).Trim() == current)
                {
                    continue;
                }

                targets.ImportRow(row);
            }

            return targets;
        }

        public static DataTable ExchangeTargets(DataTable durables, DataRow source)
        {
            return Targets(durables, source, true);
        }

        public static DataTable EditTargets(DataTable durables, DataRow source)
        {
            return Targets(durables, source, false);
        }

        private static DataTable Targets(DataTable durables, DataRow source, bool emptyOnly)
        {
            DataTable targets = durables == null ? new DataTable() : durables.Clone();

            if (durables == null || source == null)
            {
                return targets;
            }

            string durableTyp = Type(source);
            string sourceId = Id(source);

            foreach (DataRow row in durables.Rows)
            {
                if (Id(row).Length == 0 || Type(row).Length == 0 || Type(row) != durableTyp || Id(row) == sourceId)
                {
                    continue;
                }

                if (!emptyOnly
                        && (!ResponseColumns.IsInteger(TableHelper.CellText(row, Schemas.Durable.LotQty.Name))
                                || !ResponseColumns.IsInteger(TableHelper.CellText(row, Schemas.Durable.Capa.Name))))
                {
                    continue;
                }

                if (!Schemas.Durable.IsNormalIdle(row))
                {
                    continue;
                }

                if (emptyOnly
                        ? Schemas.Durable.IsFreeAvailable(row)
                        : Schemas.Durable.IsFreeAvailable(row) || Schemas.Durable.IsPartialInUse(row))
                {
                    targets.ImportRow(row);
                }
            }

            return targets;
        }

        public static DataTable MarkJudgeColors(DataTable history)
        {
            return JudgeHistoryColors.Mark(history);
        }

        public static string TargetText(DataRow durable)
        {
            if (durable == null)
            {
                return "No durable selected";
            }

            return Id(durable) + " · " + Status(durable) + " · "
                    + TableHelper.CellText(durable, Schemas.Durable.LotQty.Name).Trim() + "/" + TableHelper.CellText(durable, Schemas.Durable.Capa.Name).Trim();
        }

        public static string SelectionAfterBind(DataTable durables, string keepDurableId, string fallbackDurableId)
        {
            string selected = SelectionAfterBind(durables, keepDurableId);
            string keep = (keepDurableId ?? string.Empty).Trim();

            if (keep.Length == 0 || selected == keep)
            {
                return selected;
            }

            return SelectionAfterBind(durables, fallbackDurableId);
        }

        public static string SelectionAfterBind(DataTable durables, string keepDurableId)
        {
            if (durables == null || durables.Rows.Count == 0)
            {
                return string.Empty;
            }

            string keep = (keepDurableId ?? string.Empty).Trim();

            if (keep.Length > 0)
            {
                foreach (DataRow row in durables.Rows)
                {
                    if (Id(row) == keep)
                    {
                        return keep;
                    }
                }
            }

            return Id(durables.Rows[0]);
        }

        public static readonly IList<string> ScreenColumns =
                new List<string>().AsReadOnly();

        public static string StatusLineText(DataRow durable)
        {
            return TargetText(durable);
        }
    }
}
