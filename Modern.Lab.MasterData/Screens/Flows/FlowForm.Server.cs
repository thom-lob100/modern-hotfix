using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.Messaging;

namespace Modern.Lab.MasterData
{
    public partial class FlowForm
    {
        // FLOW_ID
        protected override DataTable RequestItems()
        {
            return this.RequestFields("FlowAction", "MethodCommand", "SelectFlows").Table;
        }

        protected override DataActionResult InsertItem(object[] requestFields)
        {
            return this.RequestFields("FlowAction", WithMethodCommand("InsertFlow", requestFields));
        }

        protected override DataActionResult UpdateItem(object[] requestFields)
        {
            return this.RequestFields("FlowAction", WithMethodCommand("UpdateFlow", requestFields));
        }

        protected override DataActionResult DeleteItem(string[] keyValues)
        {
            return this.RequestFields("FlowAction", WithMethodCommand("DeleteFlow", this.KeyFields(keyValues)));
        }
    }
}
