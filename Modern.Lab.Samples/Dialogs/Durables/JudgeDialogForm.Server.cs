using System.Data;
using Modern.Lab.Data;

namespace Modern.Lab.Samples
{
    public partial class JudgeDialogForm
    {
        // DURABLE_ID
        // TIMEKEY
        private DataTable GetJudgeResults(string goalDurableId, string reqSerialNo)
        {
            return this.RequestFields("GetJudgeRowList", "goalDurableId", goalDurableId, "reqSerialNo", reqSerialNo,
                    "LOT_ID", TableHelper.CellText(this.options.Source, "LOT_ID").Trim()).Table;
        }
    }
}
