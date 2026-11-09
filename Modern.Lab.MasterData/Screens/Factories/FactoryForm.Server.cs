using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.Messaging;

namespace Modern.Lab.MasterData
{
    public partial class FactoryForm
    {
        // FAC_ID
        protected override DataTable RequestItems(string keyword)
        {
            return this.RequestFields(
                    "FactoryAction",
                    "MethodCommand", "SelectFactories",
                    this.CrudEditor.ParameterNameOf("FAC_TYP"), this.searchFacTyp).Table;
        }

        protected override DataActionResult InsertItem(object[] requestFields)
        {
            return this.RequestFields("FactoryAction", WithMethodCommand("InsertFactory", requestFields));
        }

        protected override DataActionResult UpdateItem(object[] requestFields)
        {
            return this.RequestFields("FactoryAction", WithMethodCommand("UpdateFactory", requestFields));
        }

        protected override DataActionResult DeleteItem(string key)
        {
            return this.RequestFields(
                    "FactoryAction", "MethodCommand", "DeleteFactory", this.CrudEditor.ParameterNameOf("FAC_ID"), key);
        }
    }
}
