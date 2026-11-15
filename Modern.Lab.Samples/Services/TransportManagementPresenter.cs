using System;
using System.Collections.Generic;
using System.Data;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples.Services
{

    public static class TransportManagementPresenter
    {

        public const string ActionManualIn = "ManualIn";
        public const string ActionManualInCancel = "ManualInCancel";
        public const string ActionManualOut = "ManualOut";
        public const string ActionManualOutCancel = "ManualOutCancel";
        public const string ActionChangeDest = "ChangeDest";

        public const string ManualInLabel = "Manual In";
        public const string ManualInCancelLabel = "Manual In Cancel";
        public const string ManualOutLabel = "Manual Out";
        public const string ManualOutCancelLabel = "Manual Out Cancel";
        public const string ChangeDestLabel = "Change Destination";

        public const string MenuValueColumn = "VALUE";
        public const string MenuLabelColumn = "LABEL";
        public const string MenuEnabledColumn = "CAN";
        public const string MenuGlyphColumn = "GLYPH";
        public const string MenuAccentColumn = "ACCENT";

        public static readonly IList<DurableAction> Actions =
                new List<DurableAction>
                {
                    new DurableAction(ActionManualIn, ManualInLabel, true),
                    new DurableAction(ActionManualInCancel, ManualInCancelLabel, true),
                    new DurableAction(ActionManualOut, ManualOutLabel, true),
                    new DurableAction(ActionManualOutCancel, ManualOutCancelLabel, true),
                    new DurableAction(ActionChangeDest, ChangeDestLabel, true)
                }.AsReadOnly();

        public static readonly IList<DurableAction> ManualInActions =
                new List<DurableAction>
                {
                    Of(ActionManualIn),
                    Of(ActionManualInCancel)
                }.AsReadOnly();

        public static readonly IList<DurableAction> ManualOutActions =
                new List<DurableAction>
                {
                    Of(ActionManualOut),
                    Of(ActionManualOutCancel)
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

        public static string Id(DataRow transport)
        {
            return Schemas.Transport.DurableId.Text(transport);
        }

        public static string Place(string eqpId, string portNm)
        {
            if (string.IsNullOrEmpty(eqpId))
            {
                return "-";
            }

            return string.IsNullOrEmpty(portNm) ? eqpId : eqpId + " · " + portNm;
        }

        public static string StatusLineText(DataRow transport)
        {
            if (transport == null)
            {
                return "No transport selected";
            }

            string text = Id(transport) + " · " + Schemas.Transport.MoveStatCd.Text(transport);
            string destination = Schemas.Transport.EndGoalVal.Text(transport);

            if (destination.Length == 0)
            {
                return text;
            }

            return text + " → " + Place(destination, Schemas.Transport.EndLocCd.Text(transport));
        }
    }
}
