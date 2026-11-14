using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.ResponseContracts;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.MasterData
{
    public partial class EquipmentGroupForm
    {
        // EQP_GRP_ID
        protected override DataTable RequestItems()
        {
            DataTable table = this.RequestFields(
                    "EquipmentGroupAction",
                    "MethodCommand", "SelectEquipmentGroups",
                    this.CrudEditor.ParameterNameOf("EQP_GRP_TYP"), this.searchEqpGrpTyp).Table;
            string missing = ResponseColumns.Missing(table, EntityDefinition.Names(Schemas.EquipmentGroup.KeyFields));
            if (missing.Length > 0)
            {
                throw new ResponseContractException("Equipment Group — missing column " + missing);
            }
            return table;
        }

        protected override DataActionResult InsertItem(object[] requestFields)
        {
            return this.RequestFields("EquipmentGroupAction", WithMethodCommand("InsertEquipmentGroup", requestFields));
        }

        protected override DataActionResult UpdateItem(object[] requestFields)
        {
            return this.RequestFields("EquipmentGroupAction", WithMethodCommand("UpdateEquipmentGroup", requestFields));
        }

        protected override DataActionResult DeleteItem(string[] keyValues)
        {
            return this.RequestFields("EquipmentGroupAction", WithMethodCommand("DeleteEquipmentGroup", this.KeyFields(keyValues)));
        }

        protected override DataTable RequestLookupItems(string requestName, object[] requestFields)
        {
            switch (requestName)
            {
                case "GetEquipmentGroupTypeList":
                    return this.GetEquipmentGroupTypeList();

                default:
                    return base.RequestLookupItems(requestName, requestFields);
            }
        }

        // TYP_VAL
        // TYP_NM
        private DataTable GetEquipmentGroupTypeList()
        {
            return this.RequestFields(
                    "CommonCodeAction", "MethodCommand", "SelectCommonCodes", "CommonTyp", "EquipmentGroupType").Table;
        }
    }
}
