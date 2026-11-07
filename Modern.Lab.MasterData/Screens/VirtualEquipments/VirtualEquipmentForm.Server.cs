using System.Collections.Generic;
using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.Messaging;

namespace Modern.Lab.MasterData
{
    public partial class VirtualEquipmentForm
    {
        // EQP_ID
        protected override DataTable RequestItems(string keyword)
        {
            return this.RequestFields("EquipmentAction", "MethodCommand", "SelectVirtualEquipments", "Keyword", keyword).Table;
        }

        protected override DataActionResult InsertItem(object[] requestFields)
        {
            return this.WriteVirtualEquipment("InsertVirtualEquipment", requestFields);
        }

        protected override DataActionResult UpdateItem(object[] requestFields)
        {
            return this.WriteVirtualEquipment("UpdateVirtualEquipment", requestFields);
        }

        protected override DataActionResult DeleteItem(string key)
        {
            return this.RequestFields(
                    "EquipmentAction", "MethodCommand", "DeleteVirtualEquipment", this.CrudEditor.ParameterNameOf("EQP_ID"), key);
        }

        private DataActionResult WriteVirtualEquipment(string method, object[] requestFields)
        {
            List<object> pairs = new List<object> { "EqpModelCd", "VIRTUAL" };
            pairs.AddRange(WithoutFields(requestFields, "EqpModelCd"));
            return this.RequestFields("EquipmentAction", WithMethodCommand(method, pairs.ToArray()));
        }
    }
}
