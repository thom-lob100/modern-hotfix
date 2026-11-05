using System.Data;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Samples.Services;

namespace Modern.Lab.Samples.Services
{

    public static class JudgeHistoryColors
    {

        public const string ColorColumn = "JUDGE_COLOR";

        public static DataTable Mark(DataTable history)
        {
            if (history == null)
            {
                return null;
            }

            if (!history.Columns.Contains(ColorColumn))
            {
                history.Columns.Add(ColorColumn, typeof(string));
            }

            string failColor = System.Drawing.ColorTranslator.ToHtml(
                    Modern.Lab.Theming.ModernTokenColors.Get("Brush.ErrorBorder", System.Drawing.Color.FromArgb(220, 38, 38)));

            foreach (DataRow row in history.Rows)
            {
                row[ColorColumn] = TableHelper.CellText(row, Schemas.Wafer.JudgeRslt.Name).Trim()
                        == Schemas.Wafer.JudgeRslt.Fail.Value ? failColor : string.Empty;
            }

            return history;
        }
    }
}
