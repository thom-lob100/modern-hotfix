using System;
using System.Data;
using Modern.Lab.Data;

namespace Modern.Lab.Samples.Services
{
    public static class RefreshInterval
    {
        public const string SecondsColumn = "SECONDS";
        public const string LabelColumn = "LABEL";
        public const int CountdownSeconds = 5;

        public static readonly int[] Choices = new int[] { 0, 5, 10, 15, 30, 60 };

        public static DataTable Options()
        {
            return Options(Choices);
        }

        public static DataTable Options(int[] choices)
        {
            DataTable table = new DataTable();
            table.Locale = System.Globalization.CultureInfo.InvariantCulture;
            table.Columns.Add(SecondsColumn, typeof(int));
            table.Columns.Add(LabelColumn, typeof(string));

            foreach (int seconds in choices)
            {
                table.Rows.Add(seconds, seconds == 0 ? "Off" : seconds.ToString() + " s");
            }

            return table;
        }

        public static string CountdownText(int secondsLeft, int intervalSeconds)
        {
            if (intervalSeconds <= 0 || secondsLeft <= 0 || secondsLeft > CountdownSeconds)
            {
                return string.Empty;
            }

            return "Refresh in " + secondsLeft + " s";
        }

        public static int Seconds(object selectedValue)
        {
            return TableHelper.ParseInt(Convert.ToString(selectedValue) ?? string.Empty);
        }
    }
}
