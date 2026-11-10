using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.ResponseContracts;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.MasterData
{
    public partial class UserForm
    {
        // USER_ID
        protected override DataTable RequestItems()
        {
            DataTable table = this.RequestFields("UserAction", "MethodCommand", "SelectUsers").Table;
            string missing = ResponseColumns.Missing(table, EntityDefinition.Names(Schemas.User.KeyFields));
            if (missing.Length > 0)
            {
                throw new ResponseContractException("User — missing column " + missing);
            }
            return table;
        }

        protected override DataActionResult InsertItem(object[] requestFields)
        {
            return this.RequestFields("UserAction", WithMethodCommand("InsertUser", requestFields));
        }

        protected override DataActionResult UpdateItem(object[] requestFields)
        {
            return this.RequestFields("UserAction", WithMethodCommand("UpdateUser", requestFields));
        }

        protected override DataActionResult DeleteItem(string key)
        {
            return this.RequestFields(
                    "UserAction", "MethodCommand", "DeleteUser", this.CrudEditor.ParameterNameOf("USER_ID"), key);
        }
    }
}
