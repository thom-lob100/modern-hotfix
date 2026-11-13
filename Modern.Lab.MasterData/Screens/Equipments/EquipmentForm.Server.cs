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
            return this.RequestFields(
                    "EquipmentAction",
                    "MethodCommand", "SelectEquipments",
                    this.CrudEditor.ParameterNameOf("FAC_ID"), this.searchFacId,
                    this.CrudEditor.ParameterNameOf("EQP_TYP"), this.searchEqpTyp,
                    this.CrudEditor.ParameterNameOf("DET_EQP_TYP"), this.searchDetEqpTyp,
                    this.CrudEditor.ParameterNameOf("EQP_GRP_ID"), this.searchEqpGrpId).Table;
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
                case "GetFactoryList":
                    return this.GetFactoryList();

                case "GetEquipmentTypeList":
                    return this.GetEquipmentTypeList();

                case "GetDetailEquipmentTypeList":
                    return this.GetDetailEquipmentTypeList();

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

        // FAC_ID
        private DataTable GetFactoryList()
        {
            return this.RequestFields("FactoryAction", "MethodCommand", "SelectFactories").Table;
        }

        // TYP_VAL
        // TYP_NM
        private DataTable GetEquipmentTypeList()
        {
            return this.RequestFields(
                    "CommonCodeAction", "MethodCommand", "SelectCommonCodes", "CommonTyp", "EquipmentType").Table;
        }

        // TYP_VAL
        // TYP_NM
        private DataTable GetDetailEquipmentTypeList()
        {
            return this.RequestFields(
                    "CommonCodeAction", "MethodCommand", "SelectCommonCodes", "CommonTyp", "DetailEquipmentType").Table;
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
