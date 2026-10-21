using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.Hosting.ResponseContracts;
using Modern.Lab.Samples.Contracts;
using Modern.Lab.Hosting.NewItems;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples.Services
{
    public static class LotManagementPresenter
    {

        public const string MesEnd = "END";

        public const string ActionCreateWafer = "CreateWafer";
        public const string ActionCreateChip = "CreateChip";
        public const string ActionCreateLamella = "CreateLamella";
        public const string ActionHold = "Hold";
        public const string ActionNotOnHold = "NotOnHold";
        public const string ActionChangeSpec = "ChangeSpec";
        public const string ActionScrap = "Scrap";
        public const string ActionChangeSpecCancel = "ChangeSpecCancel";
        public const string ActionScrapCancel = "ScrapCancel";

        public const string ChangeSpecCancelLabel = "ChangeSpec Cancel";
        public const string ScrapCancelLabel = "Scrap Cancel";

        public const string MenuValueColumn = "VALUE";
        public const string MenuLabelColumn = "LABEL";
        public const string MenuEnabledColumn = "CAN";
        public const string MenuGlyphColumn = "GLYPH";
        public const string MenuAccentColumn = "ACCENT";

        public const string DateFormat = "yyyy-MM-dd";

        public static readonly IList<LotAction> CreateActions =
                new List<LotAction>
                {
                    new LotAction(ActionCreateWafer, "Create Wafer"),
                    new LotAction(ActionCreateChip, "Create Chip"),
                    new LotAction(ActionCreateLamella, "Create Lamella")
                }.AsReadOnly();

        public static readonly IList<LotAction> HoldActions =
                new List<LotAction>
                {
                    new LotAction(ActionHold, "Hold"),
                    new LotAction(ActionNotOnHold, "NotOnHold")
                }.AsReadOnly();

        public static readonly IList<LotAction> ChangeSpecActions =
                new List<LotAction>
                {
                    new LotAction(ActionChangeSpec, "ChangeSpec"),
                    new LotAction(ActionChangeSpecCancel, ChangeSpecCancelLabel)
                }.AsReadOnly();

        public static readonly IList<LotAction> ScrapActions =
                new List<LotAction>
                {
                    new LotAction(ActionScrap, "Scrap"),
                    new LotAction(ActionScrapCancel, ScrapCancelLabel)
                }.AsReadOnly();

        public static string Id(DataRow lot)
        {
            return TableHelper.CellText(lot, Schemas.Lot.LotId.Name).Trim();
        }

        public static string Type(DataRow lot)
        {
            return TableHelper.CellText(lot, Schemas.Lot.SubProdTyp.Name).Trim();
        }

        public static string Status(DataRow lot)
        {
            return TableHelper.CellText(lot, Schemas.Lot.LotStatTyp.Name).Trim();
        }

        public static string MesStatus(DataRow lot)
        {
            return TableHelper.CellText(lot, Schemas.Lot.MesProcStatCd.Name).Trim();
        }

        public static string HoldState(DataRow lot)
        {
            return TableHelper.CellText(lot, Schemas.Lot.LotHoldStatCd.Name).Trim();
        }

        public static bool IsActionKey(string key)
        {
            return key == ActionCreateWafer || key == ActionCreateChip || key == ActionCreateLamella
                    || key == ActionHold || key == ActionNotOnHold
                    || key == ActionChangeSpec || key == ActionScrap
                    || key == ActionChangeSpecCancel || key == ActionScrapCancel;
        }

        public static DataTable BuildGatedMenuTable(IList<LotAction> actions, Func<string, bool> canExecute)
        {
            DataTable table = new DataTable();
            table.Locale = CultureInfo.InvariantCulture;
            table.Columns.Add(MenuValueColumn, typeof(string));
            table.Columns.Add(MenuLabelColumn, typeof(string));
            table.Columns.Add(MenuEnabledColumn, typeof(bool));
            table.Columns.Add(MenuGlyphColumn, typeof(string));
            table.Columns.Add(MenuAccentColumn, typeof(string));

            foreach (LotAction action in actions)
            {
                table.Rows.Add(action.Key, action.Label, canExecute(action.Key),
                        MenuGlyphs.For(action.Label), MenuGlyphs.AccentOf(action.Label));
            }

            return table;
        }

        public static string ChildTypeOf(string key)
        {
            return key == ActionCreateChip ? Schemas.Lot.SubProdTyp.Chip.Value : Schemas.Lot.SubProdTyp.Lamella.Value;
        }

        public static string DateText(DateTime? date)
        {
            return date.HasValue
                    ? date.Value.ToString(DateFormat, CultureInfo.InvariantCulture)
                    : string.Empty;
        }

        public static string WaferListTitle(string subProdTyp)
        {
            return SubProdTypeCaption(subProdTyp) + " List";
        }

        public static string WaferHistoryTitle(string subProdTyp)
        {
            return SubProdTypeCaption(subProdTyp) + " History";
        }

        public static string SubProdTypeCaption(string subProdTyp)
        {
            if (subProdTyp == Schemas.Lot.SubProdTyp.Chip.Value)
            {
                return Schemas.Lot.SubProdTyp.Chip.Value;
            }

            if (subProdTyp == Schemas.Lot.SubProdTyp.Lamella.Value)
            {
                return Schemas.Lot.SubProdTyp.Lamella.Value;
            }

            return Schemas.Lot.SubProdTyp.Wafer.Value;
        }

        public static string SelectionAfterBind(DataTable lots, string keepLotId)
        {
            if (lots == null || lots.Rows.Count == 0)
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(keepLotId))
            {
                foreach (DataRow row in lots.Rows)
                {
                    if (Id(row) == keepLotId)
                    {
                        return keepLotId;
                    }
                }
            }

            foreach (DataRow row in lots.Rows)
            {
                if (Id(row).Length > 0)
                {
                    return Id(row);
                }
            }
            return string.Empty;
        }

        public static string StatusLineText(DataRow lot)
        {
            return TargetText(lot);
        }

        public static string TargetText(DataRow lot)
        {
            if (lot == null)
            {
                return "No lot selected";
            }

            return Id(lot) + " · " + Type(lot) + " · " + Status(lot) + " / " + MesStatus(lot)
                    + " · " + HoldState(lot) + " · " + TableHelper.CellText(lot, Schemas.Lot.Qty.Name).Trim() + " " + SubProdTypeCaption(Type(lot));
        }
    }
}
