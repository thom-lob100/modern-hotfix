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
            return this.RequestFields("AreaBayAction", "MethodCommand", "SelectLocations").Table;
        }

        protected override DataActionResult InsertItem(object[] requestFields)
        {
            return this.RequestFields("AreaBayAction", WithMethodCommand("InsertLocation", requestFields));
        }

        protected override DataActionResult UpdateItem(object[] requestFields)
        {
            return this.RequestFields("AreaBayAction", WithMethodCommand("UpdateLocation", requestFields));
        }

        protected override DataActionResult DeleteItem(string key)
        {
            return this.RequestFields(
                    "AreaBayAction", "MethodCommand", "DeleteLocation", this.CrudEditor.ParameterNameOf("AREA_ID"), key);
        }

        protected override DataTable RequestLookupItems(string requestName, object[] requestFields)
        {
            if (requestName == "SelectParentAreas")
            {
                return this.GetParentAreas();
            }

            return base.RequestLookupItems(requestName, requestFields);
        }

        // CODE
        // NAME
        private DataTable GetParentAreas()
        {
            return this.RequestFields("AreaBayAction", "MethodCommand", "SelectParentAreas").Table;
        }
    }
}
