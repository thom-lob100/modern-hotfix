using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.Messaging;

namespace Modern.Lab.MasterData
{
    public partial class EquipmentModelForm
    {
        // FAC_ID
        // EQP_MODEL_CD
        protected override DataTable RequestItems()
        {
            return this.RequestFields("EquipmentModelAction", "MethodCommand", "SelectEquipmentModels").Table;
        }

        protected override DataActionResult InsertItem(object[] requestFields)
        {
            return this.RequestFields("EquipmentModelAction", WithMethodCommand("InsertEquipmentModel", requestFields));
        }

        protected override DataActionResult UpdateItem(object[] requestFields)
        {
            return this.RequestFields("EquipmentModelAction", WithMethodCommand("UpdateEquipmentModel", requestFields));
        }

        protected override DataActionResult DeleteItem(string[] keyValues)
        {
            return this.RequestFields("EquipmentModelAction", WithMethodCommand("DeleteEquipmentModel", this.KeyFields(keyValues)));
        }
    }
}
