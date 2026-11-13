using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.Messaging;

namespace Modern.Lab.MasterData
{
    public partial class VendorForm
    {
        // FAC_ID
        // VENDOR_NM
        protected override DataTable RequestItems()
        {
            return this.RequestFields(
                    "VendorAction",
                    "MethodCommand", "SelectVendors",
                    this.CrudEditor.ParameterNameOf("VENDOR_TYP"), this.searchVendorTyp).Table;
        }

        protected override DataActionResult InsertItem(object[] requestFields)
        {
            return this.RequestFields("VendorAction", WithMethodCommand("InsertVendor", requestFields));
        }

        protected override DataActionResult UpdateItem(object[] requestFields)
        {
            return this.RequestFields("VendorAction", WithMethodCommand("UpdateVendor", requestFields));
        }

        protected override DataActionResult DeleteItem(string[] keyValues)
        {
            return this.RequestFields(
                    "VendorAction", "MethodCommand", "DeleteVendor",
                    this.CrudEditor.ParameterNameOf("FAC_ID"), keyValues[0],
                    this.CrudEditor.ParameterNameOf("VENDOR_NM"), keyValues[1]);
        }

        protected override DataTable RequestLookupItems(string requestName, object[] requestFields)
        {
            switch (requestName)
            {
                case "GetVendorTypeList":
                    return this.GetVendorTypeList();

                default:
                    return base.RequestLookupItems(requestName, requestFields);
            }
        }

        // TYP_VAL
        // TYP_NM
        private DataTable GetVendorTypeList()
        {
            return this.RequestFields(
                    "CommonCodeAction", "MethodCommand", "SelectCommonCodes", "CommonTyp", "VendorType").Table;
        }
    }
}
