using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.ResponseContracts;

namespace Modern.Lab.MasterData
{
    public partial class PortForm
    {
        // EQP_ID
        // PORT_NM
        protected override DataTable RequestItems()
        {
            return this.RequestFields(
                    "PortAction",
                    "MethodCommand", "SelectPorts",
                    this.CrudEditor.ParameterNameOf("EQP_ID"), this.searchEqpId).Table;
        }

        protected override DataActionResult InsertItem(object[] requestFields)
        {
            return this.RequestFields("PortAction", WithMethodCommand("InsertPort", requestFields));
        }

        protected override DataActionResult UpdateItem(object[] requestFields)
        {
            return this.RequestFields("PortAction", WithMethodCommand("UpdatePort", requestFields));
        }

        protected override DataActionResult DeleteItem(string[] keyValues)
        {
            return this.RequestFields("PortAction", WithMethodCommand("DeletePort", this.KeyFields(keyValues)));
        }

        protected override DataTable RequestLookupItems(string requestName, object[] requestFields)
        {
            switch (requestName)
            {
                case "GetEquipmentList":
                    return this.GetEquipmentList();

                case "GetPortTypeList":
                    return this.GetPortTypeList();

                default:
                    return base.RequestLookupItems(requestName, requestFields);
            }
        }

        // EQP_ID
        private DataTable GetEquipmentList()
        {
            DataTable table = this.RequestFields("PortAction", "MethodCommand", "SelectEquipment").Table;
            string missing = ResponseColumns.Missing(table, "EQP_ID");
            if (missing.Length > 0)
            {
                throw new ResponseContractException("Equipment — missing column " + missing);
            }
            return table;
        }

        // TYP_VAL
        // TYP_NM
        private DataTable GetPortTypeList()
        {
            return this.RequestFields(
                    "CommonCodeAction", "MethodCommand", "SelectCommonCodes", "CommonTyp", "PortType").Table;
        }
    }
}
