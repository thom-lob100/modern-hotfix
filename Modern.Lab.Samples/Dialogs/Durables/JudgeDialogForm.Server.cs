using System.Data;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class JudgeDialogForm
    {
        // DURABLE_ID
        // TIMEKEY
        private DataTable GetJudgeResults(DataRow source, RequestParameters parameters)
        {
            return this.RequestFields("GetJudgeRowList",
                    "goalDurableId", parameters.Value(Schemas.Durable.DurableId),
                    "reqSerialNo", parameters.Value(Schemas.Lot.ReqSerialNo),
                    "LOT_ID", Schemas.Lot.LotId.Text(source)).Table;
        }
    }
}
