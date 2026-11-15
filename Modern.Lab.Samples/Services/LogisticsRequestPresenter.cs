using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples.Services
{
    internal static class LogisticsRequestPresenter
    {
        internal const string StatusColumn = "STATUS";
        internal const string ElapsedDaysColumn = "ELAPSED_DAYS";
        internal const string DaysColorColumn = "DAYS_COLOR";
        internal const string CheckColumn = "CHK";
        internal const string ReceiveCanColumn = "RECV_CAN";
        internal const string CreateCanColumn = "CRT_CAN";
        internal const string AddRequestColumn = "ADD_REQ";
        internal const string ActionTextColumn = "ACTION_TEXT";
        internal const string ActionColumn = "ACTION";

        internal const string AddRequestText = "Add Request";

        internal const string DateFormat = "yyyy-MM-dd";

        internal static readonly string[] WorkflowFlagColumns =
        {
            ReceiveCanColumn, CreateCanColumn, AddRequestColumn, ActionTextColumn
        };

        internal const int StatusSent = 0;

        internal const int StatusReceived = 1;

        internal const int StatusUnlinked = 2;

        internal const int StatusLinked = 3;

        internal const int StatusCompleted = 4;

        private const int unlinkedAfterDays = 1;

        internal static readonly string[] StatusNames =
        {
            "Sent", "Received", "Request Unlinked", "Request Linked", "Completed"
        };

        internal const string StatusColorColumn = "STATUS_COLOR";

        internal static readonly string[] StatusColors =
        {
            "#93C5FD",
            "#86EFAC",
            "#FDBA74",
            "#C4B5FD",
            "#FDE047"
        };

        private static readonly string[] agingBadgeColors =
                Modern.Lab.Theming.SemanticColors.GetAgingRamp();

        internal sealed class LogisticsSummary
        {
            internal readonly int[] StatusCounts = new int[5];

            internal int UnmatchedCount;

            internal int UnlinkedCount;

            internal double DaysAverage;

            internal int DaysMax;
        }

        internal static void ApplyWorkflowColumns(DataTable logistics, Action<DataRow> applyRules)
        {
            if (logistics == null)
            {
                return;
            }

            NormalizeTimeColumns(logistics, Schemas.Logistics.SendTm.Name, Schemas.Logistics.RecvTm.Name, Schemas.Logistics.ProcTm.Name);

            TableHelper.EnsureColumn(logistics, StatusColumn, typeof(string));
            TableHelper.EnsureColumn(logistics, StatusColorColumn, typeof(string));
            TableHelper.EnsureColumn(logistics, Schemas.Logistics.SendYn.Name, typeof(string));
            TableHelper.EnsureColumn(logistics, ElapsedDaysColumn, typeof(int));
            TableHelper.EnsureColumn(logistics, DaysColorColumn, typeof(string));
            TableHelper.EnsureColumn(logistics, CheckColumn, typeof(bool));
            TableHelper.EnsureColumn(logistics, ReceiveCanColumn, typeof(bool));
            TableHelper.EnsureColumn(logistics, CreateCanColumn, typeof(bool));
            TableHelper.EnsureColumn(logistics, AddRequestColumn, typeof(bool));
            TableHelper.EnsureColumn(logistics, ActionTextColumn, typeof(string));

            foreach (DataRow row in logistics.Rows)
            {
                ApplyStatus(row, applyRules);

                if (row.IsNull(CheckColumn))
                {
                    row[CheckColumn] = false;
                }
            }
        }

        internal static bool IsNotified(DataRow row)
        {
            return TableHelper.CellText(row, Schemas.Logistics.SendYn.Name).Trim() == "Y";
        }

        internal static void ApplyStatus(DataRow row, Action<DataRow> applyRules)
        {
            bool received = TableHelper.CellText(row, Schemas.Logistics.RecvYn.Name).Trim() == "Y"
                    || TableHelper.CellText(row, Schemas.Logistics.RecvTm.Name).Trim().Length > 0;
            bool linked = TableHelper.CellText(row, Schemas.Logistics.ReqSerialNo.Name).Trim().Length > 0;
            bool created = TableHelper.CellText(row, Schemas.Logistics.ProcYn.Name).Trim() == "Y";

            bool notified = IsNotified(row);
            row[Schemas.Logistics.SendYn.Name] = notified ? "Y" : "N";

            int status;

            if (!received)
            {
                status = StatusSent;
            }
            else if (!linked)
            {
                int waitedDays = ElapsedDays(TableHelper.CellText(row, Schemas.Logistics.RecvTm.Name).Trim());
                status = waitedDays >= unlinkedAfterDays ? StatusUnlinked : StatusReceived;
            }
            else if (!created)
            {
                status = StatusLinked;
            }
            else
            {
                status = StatusCompleted;
            }

            row[StatusColumn] = StatusNames[status];
            row[StatusColorColumn] = StatusColors[status];

            applyRules(row);

            row[ActionTextColumn] = ActionText(row);

            string agingBasis = string.Empty;

            if (!received)
            {
                agingBasis = TableHelper.CellText(row, Schemas.Logistics.SendTm.Name).Trim();
            }
            else if (!linked)
            {
                agingBasis = TableHelper.CellText(row, Schemas.Logistics.RecvTm.Name).Trim();
            }

            int elapsedDays = agingBasis.Length > 0 ? ElapsedDays(agingBasis) : 0;

            if (elapsedDays > 0)
            {
                row[ElapsedDaysColumn] = elapsedDays;
                row[DaysColorColumn] = agingBadgeColors[AgingBand(elapsedDays)];
            }
            else
            {
                row[ElapsedDaysColumn] = DBNull.Value;
                row[DaysColorColumn] = string.Empty;
            }
        }

        internal static LogisticsSummary Aggregate(DataTable result)
        {
            LogisticsSummary summary = new LogisticsSummary();

            if (result == null)
            {
                return summary;
            }

            int daysSum = 0;

            foreach (DataRow row in result.Rows)
            {
                int status = Array.IndexOf(StatusNames, TableHelper.CellText(row, StatusColumn));

                if (status >= 0)
                {
                    summary.StatusCounts[status] += 1;
                }

                if (TableHelper.CellText(row, Schemas.Logistics.SendYn.Name) == "N")
                {
                    summary.UnmatchedCount += 1;
                }

                if (status == StatusReceived || status == StatusUnlinked)
                {
                    summary.UnlinkedCount += 1;

                    int days = row.Table.Columns.Contains(ElapsedDaysColumn) && !row.IsNull(ElapsedDaysColumn)
                            ? TableHelper.ParseInt(TableHelper.CellText(row, ElapsedDaysColumn))
                            : 0;
                    daysSum += days;

                    if (days > summary.DaysMax)
                    {
                        summary.DaysMax = days;
                    }
                }
            }

            if (summary.UnlinkedCount > 0)
            {
                summary.DaysAverage = (double)daysSum / summary.UnlinkedCount;
            }

            return summary;
        }

        private static void NormalizeTimeColumns(DataTable logistics, params string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                if (!logistics.Columns.Contains(columnName))
                {
                    continue;
                }

                foreach (DataRow row in logistics.Rows)
                {
                    string text = TableHelper.CellText(row, columnName).Trim();
                    DateTime parsed;

                    if (text.Length > 0 && DateTime.TryParse(
                            text, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                    {
                        row[columnName] = parsed.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                }
            }
        }

        internal static string NextStep(DataRow row)
        {
            if (TableHelper.FlagSet(row, ReceiveCanColumn))
            {
                return "Receive";
            }

            return TableHelper.FlagSet(row, CreateCanColumn) ? "Create" : string.Empty;
        }

        internal static string ActionText(DataRow row)
        {
            string step = NextStep(row);

            return step == "Create" && TableHelper.FlagSet(row, AddRequestColumn)
                    ? AddRequestText
                    : step;
        }

        internal static string DateText(DateTime? date)
        {
            return date.HasValue
                    ? date.Value.ToString(DateFormat, CultureInfo.InvariantCulture)
                    : string.Empty;
        }

        private static int ElapsedDays(string arriveText)
        {
            DateTime arrived;

            if (!DateTime.TryParse(arriveText, out arrived))
            {
                return 0;
            }

            int days = (int)(DateTime.Today - arrived.Date).TotalDays;
            return days > 0 ? days : 0;
        }

        private static int AgingBand(int days)
        {
            if (days >= 14)
            {
                return 3;
            }

            if (days >= 7)
            {
                return 2;
            }

            if (days >= 3)
            {
                return 1;
            }

            return 0;
        }
    }
}
