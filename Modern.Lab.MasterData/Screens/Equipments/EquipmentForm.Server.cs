using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.Messaging;

namespace Modern.Lab.MasterData
{
    public partial class EquipmentForm
    {
        // EQP_ID
        protected override DataTable RequestItems()
        {
            return this.RequestFields("EquipmentAction", "MethodCommand", "SelectEquipments").Table;
        }

        protected override DataActionResult InsertItem(object[] requestFields)
        {
            return this.RequestFields("EquipmentAction", WithMethodCommand("InsertEquipment", requestFields));
        }

        protected override DataActionResult UpdateItem(object[] requestFields)
        {
            return this.RequestFields("EquipmentAction", WithMethodCommand("UpdateEquipment", requestFields));
        }

        protected override DataActionResult DeleteItem(string key)
        {
            return this.RequestFields(
                    "EquipmentAction", "MethodCommand", "DeleteEquipment", this.CrudEditor.ParameterNameOf("EQP_ID"), key);
        }

        protected override DataTable RequestLookupItems(string requestName, object[] requestFields)
        {
            switch (requestName)
            {
                case "GetEquipmentModelList":
                    return this.GetEquipmentModelList();

                case "GetVendorList":
                    return this.GetVendorList();

                case "GetEquipmentGroupList":
                    return this.GetEquipmentGroupList();

                default:
                    return base.RequestLookupItems(requestName, requestFields);
            }
        }

        // EQP_MODEL_CD
        private DataTable GetEquipmentModelList()
        {
            return this.RequestFields("EquipmentAction", "MethodCommand", "GetEquipmentModelList").Table;
        }

        // VENDOR_NM
        private DataTable GetVendorList()
        {
            return this.RequestFields("EquipmentAction", "MethodCommand", "GetVendorList").Table;
        }

        // EQP_GRP_ID
        private DataTable GetEquipmentGroupList()
        {
            return this.RequestFields("EquipmentAction", "MethodCommand", "GetEquipmentGroupList").Table;
        }
    }
}
