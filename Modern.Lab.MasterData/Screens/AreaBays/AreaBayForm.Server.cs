using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.Messaging;

namespace Modern.Lab.MasterData
{
    public partial class AreaBayForm
    {
        // AREA_ID
        protected override DataTable RequestItems()
        {
            return this.RequestFields(
                    "AreaBayAction",
                    "MethodCommand", "SelectLocations",
                    this.CrudEditor.ParameterNameOf("DET_AREA_TYP"), this.searchDetAreaTyp).Table;
        }

        protected override DataActionResult InsertItem(object[] requestFields)
        {
            return this.RequestFields("AreaBayAction", WithMethodCommand("InsertLocation", requestFields));
        }

        protected override DataActionResult UpdateItem(object[] requestFields)
        {
            return this.RequestFields("AreaBayAction", WithMethodCommand("UpdateLocation", requestFields));
        }

        protected override DataActionResult DeleteItem(string[] keyValues)
        {
            return this.RequestFields("AreaBayAction", WithMethodCommand("DeleteLocation", this.KeyFields(keyValues)));
        }

        protected override DataTable RequestLookupItems(string requestName, object[] requestFields)
        {
            if (requestName == "SelectParentAreas")
            {
                return this.GetParentAreas();
            }

            if (requestName == "SelectDetailAreaTypes")
            {
                return this.GetDetailAreaTypes();
            }

            return base.RequestLookupItems(requestName, requestFields);
        }

        // CODE
        // NAME
        private DataTable GetParentAreas()
        {
            return this.RequestFields("AreaBayAction", "MethodCommand", "SelectParentAreas").Table;
        }

        // TYP_VAL
        // TYP_NM
        private DataTable GetDetailAreaTypes()
        {
            return this.RequestFields(
                    "CommonCodeAction", "MethodCommand", "SelectCommonCodes", "CommonTyp", "DetailAreaType").Table;
        }
    }
}
