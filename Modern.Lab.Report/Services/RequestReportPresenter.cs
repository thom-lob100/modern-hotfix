using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Report.Services
{
    internal static class RequestReportPresenter
    {
        internal const string SharedColumn = "SHARED_WITH";
        internal const string HoldColumn = "HOLD_BADGE";
        internal const string HoldSemanticColumn = "HOLD_BADGE_SEM";
        internal const string HoldLotColumn = "HOLD_LOT";
        internal const string ReturnColumn = "RETURN_STATE";
        internal const string ReturnSemanticColumn = "RETURN_STATE_SEM";
        internal const string SwapColumn = "SWAP_STATE";
        internal const string SwapSemanticColumn = "SWAP_STATE_SEM";
        internal const string ReworkColumn = "REWORK_STATE";
        internal const string ReworkSemanticColumn = "REWORK_STATE_SEM";
        internal const string ReworkCountColumn = "REWORK_EXTRA";
        internal const string ReworkLotColumn = "REWORK_LOT";
        internal const string ScrapColumn = "SCRAP_STATE";
        internal const string ScrapSemanticColumn = "SCRAP_STATE_SEM";
        internal const string FailNowColumn = "FAIL_NOW";

        internal const string WarningSemantic = "warning";
        internal const string ErrorSemantic = "error";
        internal const string NeutralSemantic = "neutral";

        private const string ShortTimeFormat = "MM-dd";

        internal static readonly string[] HiddenColumns =
        {
            HoldSemanticColumn, HoldLotColumn, ReturnSemanticColumn, SwapSemanticColumn, ReworkSemanticColumn,
            ReworkCountColumn, ReworkLotColumn, ScrapSemanticColumn, FailNowColumn
        };

        internal static void AddShared(DataTable rows, string lotRequestsColumn)
        {
            if (rows == null)
            {
                return;
            }

            TableHelper.EnsureColumn(rows, SharedColumn, typeof(string));

            foreach (DataRow row in rows.Rows)
            {
                row[SharedColumn] = SharedWith(row, lotRequestsColumn);
            }
        }

        internal static void AddBranches(
                DataTable rows, string holdColumn, string holdValue, string exchangeTmColumn, string returnTmColumn,
                string swapTmColumn, string scrapTmColumn, string scrapDueColumn, string[] runCountColumns,
                string[] judgeColumns, string failValue)
        {
            if (rows == null)
            {
                return;
            }

            bool hasHoldColumn = Has(rows, holdColumn);
            bool hasRunCountColumns = HasAll(rows, runCountColumns);
            bool hasJudgeColumns = HasAll(rows, judgeColumns);
            bool hasReturnColumn = Has(rows, returnTmColumn);

            TableHelper.EnsureColumn(rows, HoldColumn, typeof(string));
            TableHelper.EnsureColumn(rows, HoldSemanticColumn, typeof(string));
            TableHelper.EnsureColumn(rows, ReturnColumn, typeof(string));
            TableHelper.EnsureColumn(rows, ReturnSemanticColumn, typeof(string));
            TableHelper.EnsureColumn(rows, SwapColumn, typeof(string));
            TableHelper.EnsureColumn(rows, SwapSemanticColumn, typeof(string));
            TableHelper.EnsureColumn(rows, ReworkColumn, typeof(string));
            TableHelper.EnsureColumn(rows, ReworkSemanticColumn, typeof(string));
            TableHelper.EnsureColumn(rows, ReworkCountColumn, typeof(string));
            TableHelper.EnsureColumn(rows, ScrapColumn, typeof(string));
            TableHelper.EnsureColumn(rows, ScrapSemanticColumn, typeof(string));

            if (hasHoldColumn)
            {
                TableHelper.EnsureColumn(rows, HoldLotColumn, typeof(string));
            }

            if (hasRunCountColumns)
            {
                TableHelper.EnsureColumn(rows, ReworkLotColumn, typeof(string));
            }

            if (hasJudgeColumns)
            {
                TableHelper.EnsureColumn(rows, FailNowColumn, typeof(string));
            }

            foreach (DataRow row in rows.Rows)
            {
                string lotId = Schemas.Logistics.LotId.Text(row);
                bool hold = hasHoldColumn
                        && string.Equals(Cell(row, holdColumn), holdValue, StringComparison.OrdinalIgnoreCase);
                row[HoldColumn] = hold ? "Hold" : string.Empty;
                row[HoldSemanticColumn] = hold ? WarningSemantic : string.Empty;

                if (hasHoldColumn)
                {
                    row[HoldLotColumn] = hold ? lotId : string.Empty;
                }

                string returnTm = hasReturnColumn ? Cell(row, returnTmColumn) : string.Empty;
                bool fabFoup = Schemas.Logistics.BoxId.Text(row).Length > 0;
                bool exchanged = Cell(row, exchangeTmColumn).Length > 0;

                if (returnTm.Length > 0)
                {
                    row[ReturnColumn] = "Returned " + ShortTime(returnTm);
                    row[ReturnSemanticColumn] = NeutralSemantic;
                }
                else if (hasReturnColumn && fabFoup && exchanged)
                {
                    row[ReturnColumn] = "To Fab";
                    row[ReturnSemanticColumn] = WarningSemantic;
                }
                else
                {
                    row[ReturnColumn] = string.Empty;
                    row[ReturnSemanticColumn] = string.Empty;
                }

                string swapTm = Cell(row, swapTmColumn);
                row[SwapColumn] = swapTm.Length == 0 ? string.Empty : "Swapped " + ShortTime(swapTm);
                row[SwapSemanticColumn] = swapTm.Length == 0 ? string.Empty : NeutralSemantic;

                int rework = ExtraRuns(row, runCountColumns);
                row[ReworkCountColumn] = rework.ToString(CultureInfo.InvariantCulture);
                row[ReworkColumn] = rework == 0 ? string.Empty : "RW " + rework.ToString(CultureInfo.InvariantCulture);
                row[ReworkSemanticColumn] = rework == 0 ? string.Empty : ErrorSemantic;

                if (hasRunCountColumns)
                {
                    row[ReworkLotColumn] = rework == 0 ? string.Empty : lotId;
                }

                string scrapTm = Cell(row, scrapTmColumn);
                bool scrapDue = string.Equals(Cell(row, scrapDueColumn), "Y", StringComparison.OrdinalIgnoreCase);

                if (scrapTm.Length > 0)
                {
                    row[ScrapColumn] = "Scrapped " + ShortTime(scrapTm);
                    row[ScrapSemanticColumn] = NeutralSemantic;
                }
                else if (scrapDue)
                {
                    row[ScrapColumn] = "Due";
                    row[ScrapSemanticColumn] = WarningSemantic;
                }
                else
                {
                    row[ScrapColumn] = string.Empty;
                    row[ScrapSemanticColumn] = string.Empty;
                }

                if (hasJudgeColumns)
                {
                    row[FailNowColumn] = IsFailNow(row, judgeColumns, failValue) ? failValue : string.Empty;
                }
            }
        }

        private static bool HasAll(DataTable rows, string[] columns)
        {
            if (columns == null || columns.Length == 0)
            {
                return false;
            }

            foreach (string column in columns)
            {
                if (!Has(rows, column))
                {
                    return false;
                }
            }

            return true;
        }

        private static int ExtraRuns(DataRow row, string[] runCountColumns)
        {
            int extra = 0;

            foreach (string column in runCountColumns ?? new string[0])
            {
                int runs;

                if (int.TryParse(Cell(row, column), NumberStyles.Integer, CultureInfo.InvariantCulture, out runs) && runs > 1)
                {
                    extra += runs - 1;
                }
            }

            return extra;
        }

        private static bool IsFailNow(DataRow row, string[] judgeColumns, string failValue)
        {
            foreach (string column in judgeColumns ?? new string[0])
            {
                if (string.Equals(Cell(row, column), failValue, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Has(DataTable rows, string column)
        {
            return !string.IsNullOrEmpty(column) && rows.Columns.Contains(column);
        }

        private static string Cell(DataRow row, string column)
        {
            if (string.IsNullOrEmpty(column) || !row.Table.Columns.Contains(column))
            {
                return string.Empty;
            }

            return TableHelper.CellText(row, column).Trim();
        }

        private static string ShortTime(string text)
        {
            DateTime parsed;

            return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed)
                    ? parsed.ToString(ShortTimeFormat, CultureInfo.InvariantCulture)
                    : text;
        }

        private static string SharedWith(DataRow row, string lotRequestsColumn)
        {
            string own = Schemas.Request.ReqSerialNo.Text(row);
            List<string> others = new List<string>();
            string list = string.IsNullOrEmpty(lotRequestsColumn) ? string.Empty : TableHelper.CellText(row, lotRequestsColumn);

            foreach (string part in list.Split(','))
            {
                string serial = part.Trim();

                if (serial.Length > 0
                        && !string.Equals(serial, own, StringComparison.OrdinalIgnoreCase)
                        && !others.Contains(serial))
                {
                    others.Add(serial);
                }
            }

            return string.Join(", ", others.ToArray());
        }
    }
}
