using System.Data;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class ChangeSpecDialogForm
    {
        protected virtual DataTable GetProducts(DataRow lot)
        {
            return this.RequestFields("GetProdList",
                    "LOT_ID", Schemas.Lot.LotId.Text(lot)).Table;
        }

        protected virtual DataTable GetFlows(RequestParameters parameters)
        {
            return this.RequestFields("GetFlowList",
                    "PROD_ID", parameters.Value(Schemas.Lot.ProdId)).Table;
        }

        protected virtual DataTable GetOpers(RequestParameters parameters)
        {
            return this.RequestFields("GetOperList",
                    "PROD_ID", parameters.Value(Schemas.Lot.ProdId),
                    "FLOW_ID", parameters.Value(Schemas.Lot.FlowId)).Table;
        }
    }
}
