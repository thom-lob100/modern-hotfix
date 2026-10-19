using System.Data;
using System.Text;

namespace Modern.Lab.Samples
{
    public partial class BackToPrevOperDialogForm
    {
        // PROD_ID
        protected virtual DataTable GetProducts()
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetProdList");
            return this.Request(request.ToString()).Table;
        }

        // FLOW_ID
        protected virtual DataTable GetFlows(string prodId)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetFlowList");
            request.Append(" PROD_ID=").Append(prodId);
            return this.Request(request.ToString()).Table;
        }

        // OPER_ID
        protected virtual DataTable GetOpers(string prodId, string flowId)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetOperList");
            request.Append(" PROD_ID=").Append(prodId);
            request.Append(" FLOW_ID=").Append(flowId);
            return this.Request(request.ToString()).Table;
        }
    }
}
