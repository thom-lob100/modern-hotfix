using System.Data;
using System.Text;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class DurableInfoDialogForm
    {
        // DURABLE_ID
        protected virtual DataTable GetDurable(RequestParameters parameters)
        {
            return this.RequestFields("GetDurableList",
                    "DURABLE_TYP", string.Empty,
                    "DURABLE_ID", parameters.Value(Schemas.Durable.DurableId)).Table;
        }

        // SUB_PROD_TYP
        // SLOT_NO
        // FINGER_ID
        protected virtual DataTable GetDurableSlots(RequestParameters parameters)
        {
            return this.RequestFields("GetDurableSlotList",
                    "DURABLE_ID", parameters.Value(Schemas.Durable.DurableId),
                    "SUB_PROD_TYP", string.Empty).Table;
        }
    }
}
