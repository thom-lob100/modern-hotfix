using System;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.ResponseContracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.Samples.Contracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Display;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class ChangeSpecDialogForm
    {
        protected virtual DataTable GetProducts(string lotId)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetProdList");
            request.Append(" LOT_ID=").Append(lotId);
            return this.Request(request.ToString()).Table;
        }

        protected virtual DataTable GetFlows(string prodId)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetFlowList");
            request.Append(" PROD_ID=").Append(prodId);
            return this.Request(request.ToString()).Table;
        }

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
