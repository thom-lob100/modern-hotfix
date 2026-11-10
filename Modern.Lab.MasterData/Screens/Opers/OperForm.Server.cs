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
            return this.RequestFields("OperAction", "MethodCommand", "SelectOpers").Table;
        }

        protected override DataActionResult InsertItem(object[] requestFields)
        {
            return this.RequestFields("OperAction", WithMethodCommand("InsertOper", requestFields));
        }

        protected override DataActionResult UpdateItem(object[] requestFields)
        {
            return this.RequestFields("OperAction", WithMethodCommand("UpdateOper", requestFields));
        }

        protected override DataActionResult DeleteItem(string key)
        {
            return this.RequestFields(
                    "OperAction", "MethodCommand", "DeleteOper", this.CrudEditor.ParameterNameOf("OPER_ID"), key);
        }
    }
}
