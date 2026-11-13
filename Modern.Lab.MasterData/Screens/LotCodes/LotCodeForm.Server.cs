using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.Messaging;

namespace Modern.Lab.MasterData
{
    public partial class LotCodeForm
    {
        // LOT_CD
        protected override DataTable RequestItems()
        {
            return this.RequestFields("LotCodeAction", "MethodCommand", "SelectLotCodes").Table;
        }

        protected override DataActionResult InsertItem(object[] requestFields)
        {
            return this.RequestFields("LotCodeAction", WithMethodCommand("InsertLotCode", requestFields));
        }

        protected override DataActionResult UpdateItem(object[] requestFields)
        {
            return this.RequestFields("LotCodeAction", WithMethodCommand("UpdateLotCode", requestFields));
        }

        protected override DataActionResult DeleteItem(string key)
        {
            return this.RequestFields(
                    "LotCodeAction", "MethodCommand", "DeleteLotCode", this.CrudEditor.ParameterNameOf("LOT_CD"), key);
        }
    }
}
