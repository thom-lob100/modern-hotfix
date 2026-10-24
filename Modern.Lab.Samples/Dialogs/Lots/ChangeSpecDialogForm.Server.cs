using System;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.ResponseContracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.Samples.Contracts;
using Modern.Lab.WinForms.Controls.Display;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class ChangeSpecDialogForm
    {
        protected virtual DataTable GetProducts()
        {
            return this.RequestFields("GetProdList").Table;
        }

        protected virtual DataTable GetFlows(RequestParameters parameters)
        {
            return this.RequestFields("GetFlowList",
                    parameters.Take("PROD_ID")).Table;
        }

        protected virtual DataTable GetOpers(RequestParameters parameters)
        {
            return this.RequestFields("GetOperList",
                    parameters.Take("PROD_ID"),
                    parameters.Take("FLOW_ID")).Table;
        }
    }
}
