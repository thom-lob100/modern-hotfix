using System;
using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.Messaging;

namespace Modern.Lab.MasterData
{
    public partial class ProductForm
    {
        // PROD_ID
        protected override DataTable RequestItems()
        {
            return this.RequestFields(
                    "ProductAction",
                    "MethodCommand", "SelectProducts",
                    this.CrudEditor.ParameterNameOf("SUB_PROD_TYP"), this.searchSubProdTyp,
                    this.CrudEditor.ParameterNameOf("LOT_CD"), this.searchLotCd,
                    this.CrudEditor.ParameterNameOf("PROD_ID"), this.searchProdId).Table;
        }

        protected override DataActionResult InsertItem(object[] requestFields)
        {
            return this.RequestFields("ProductAction", WithMethodCommand("InsertProduct", requestFields));
        }

        protected override DataActionResult UpdateItem(object[] requestFields)
        {
            return this.RequestFields("ProductAction", WithMethodCommand("UpdateProduct", requestFields));
        }

        protected override DataActionResult DeleteItem(string[] keyValues)
        {
            return this.RequestFields("ProductAction", WithMethodCommand("DeleteProduct", this.KeyFields(keyValues)));
        }

        protected override DataTable RequestLookupItems(string requestName, object[] requestFields)
        {
            switch (requestName)
            {
                case "GetProductTypeList":
                    return this.GetProductTypeList();

                case "GetCodeList":
                    return this.GetCodeList(FieldValue(requestFields, "GroupCd"));

                case "GetLotCodeList":
                    return this.GetLotCodeList();

                default:
                    return base.RequestLookupItems(requestName, requestFields);
            }
        }

        // CODE
        // NAME
        private DataTable GetProductTypeList()
        {
            return this.RequestFields("ProductAction", "MethodCommand", "GetProductTypeList").Table;
        }

        // CODE
        // NAME
        private DataTable GetCodeList(string groupCd)
        {
            return this.RequestFields("ProductAction", "MethodCommand", "GetCodeList", "GroupCd", groupCd).Table;
        }

        // LOT_CD
        private DataTable GetLotCodeList()
        {
            return this.RequestFields("ProductAction", "MethodCommand", "GetLotCodeList").Table;
        }

        private static string FieldValue(object[] fields, string name)
        {
            if (fields == null)
            {
                return string.Empty;
            }

            for (int index = 0; index + 1 < fields.Length; index += 2)
            {
                if (string.Equals(Convert.ToString(fields[index]), name, StringComparison.Ordinal))
                {
                    return Convert.ToString(fields[index + 1]) ?? string.Empty;
                }
            }

            return string.Empty;
        }
    }
}
