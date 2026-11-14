using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.Messaging;

namespace Modern.Lab.MasterData
{
    public partial class OperForm
    {
        // OPER_ID
        protected override DataTable RequestItems()
        {
            return this.RequestFields(
                    "OperAction",
                    "MethodCommand", "SelectOpers",
                    this.CrudEditor.ParameterNameOf("MID_OPER_TYP"), this.searchMidOperTyp,
                    this.CrudEditor.ParameterNameOf("DET_OPER_TYP"), this.searchDetOperTyp).Table;
        }

        protected override DataActionResult InsertItem(object[] requestFields)
        {
            return this.RequestFields("OperAction", WithMethodCommand("InsertOper", requestFields));
        }

        protected override DataActionResult UpdateItem(object[] requestFields)
        {
            return this.RequestFields("OperAction", WithMethodCommand("UpdateOper", requestFields));
        }

        protected override DataActionResult DeleteItem(string[] keyValues)
        {
            return this.RequestFields("OperAction", WithMethodCommand("DeleteOper", this.KeyFields(keyValues)));
        }

        protected override DataTable RequestLookupItems(string requestName, object[] requestFields)
        {
            switch (requestName)
            {
                case "GetMidOperTypeList":
                    return this.GetMidOperTypeList();

                case "GetDetailOperTypeList":
                    return this.GetDetailOperTypeList();

                default:
                    return base.RequestLookupItems(requestName, requestFields);
            }
        }

        // TYP_VAL
        // TYP_NM
        private DataTable GetMidOperTypeList()
        {
            return this.RequestFields(
                    "CommonCodeAction", "MethodCommand", "SelectCommonCodes", "CommonTyp", "MidOperType").Table;
        }

        // TYP_VAL
        // TYP_NM
        private DataTable GetDetailOperTypeList()
        {
            return this.RequestFields(
                    "CommonCodeAction", "MethodCommand", "SelectCommonCodes", "CommonTyp", "DetailOperType").Table;
        }
    }
}
