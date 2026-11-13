using System;
using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.MasterData;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.ResponseContracts;

namespace Modern.Lab.MasterData
{
    public partial class UserForm : MasterDataCrudFormBase
    {
        public UserForm()
        {
            this.InitializeComponent();
            this.InitializeCrud(CreateDefinition());
        }

        private static MasterDataCrudDefinition CreateDefinition()
        {
            return new MasterDataCrudDefinition
            {
                EntityName = "User",
                Entity = Schemas.User,
                ListTableId = "User.SelectUsers.User"
            };
        }

        protected override void OnListLoading()
        {
            this.gridUsers.DataSource = null;
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

        private void OnUserSelectionChanged(object sender, EventArgs e)
        {
            this.HandleSelectionChanged();
        }
    }
}
