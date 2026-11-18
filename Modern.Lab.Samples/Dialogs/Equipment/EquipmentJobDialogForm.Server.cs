using System.Data;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class EquipmentJobDialogForm
    {
        // WF_ID
        private DataTable GetWafers(DataRow source)
        {
            return this.RequestFields("GetLotUnitList",
                    "LOT_ID", Schemas.LotPriority.LotId.Text(source)).Table;
        }
    }
}
