using System;
using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.MasterData;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.ResponseContracts;

namespace Modern.Lab.MasterData
{
    public partial class ReasonCodeForm : MasterDataCrudFormBase
    {
        public ReasonCodeForm()
        {
            this.InitializeComponent();
            this.InitializeCrud(CreateDefinition());
        }

        private static MasterDataCrudDefinition CreateDefinition()
        {
            return new MasterDataCrudDefinition
            {
                EntityName = "Reason Code",
                Entity = Schemas.ReasonCode,
                ListTableId = "ReasonCode.SelectReasonCodes.ReasonCode"
            };
        }

        protected override void OnListLoading()
        {
            this.gridReasonCodes.DataSource = null;
            this.CrudEditor.SetSchema(new DataTable());
            this.CrudEditor.BeginNew();
        }

        protected override void OnLoadFailed(string channel, Exception failure)
        {
            if (failure is ResponseContractException && !this.LoadFailureSilent)
            {
                this.ShowErrorMessage(this.QueryFailedCaption, ResponseColumns.MissingMessage, failure.Message);
                return;
            }
            base.OnLoadFailed(channel, failure);
        }

        private void OnReasonCodeSelectionChanged(object sender, EventArgs e)
        {
            this.HandleSelectionChanged();
        }
    }
}
