using System;
using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.MasterData;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.ResponseContracts;

namespace Modern.Lab.MasterData
{
    public partial class EquipmentGroupForm : MasterDataCrudFormBase
    {
        private string searchEqpGrpTyp = string.Empty;

        public EquipmentGroupForm()
        {
            this.InitializeComponent();
            this.InitializeCrud(CreateDefinition());
        }

        private static MasterDataCrudDefinition CreateDefinition()
        {
            return new MasterDataCrudDefinition
            {
                EntityName = "Equipment Group",
                Entity = Schemas.EquipmentGroup,
                ListTableId = "EquipmentGroup.SelectEquipmentGroups.EquipmentGroup"
            };
        }

        protected override void ResetCriteria()
        {
            this.cboEqpGrpTyp.SelectedIndex = -1;
            this.cboEqpGrpTyp.Text = string.Empty;
        }

        protected override void OnRefreshClick(object sender, EventArgs e)
        {
            if (this.ActionInProgress)
            {
                return;
            }

            this.searchEqpGrpTyp = this.SelectedEqpGrpTyp();
            base.OnRefreshClick(sender, e);
        }

        protected override void OnListLoading()
        {
            this.gridEquipmentGroups.DataSource = null;
            this.CrudEditor.SetSchema(new DataTable());
            this.CrudEditor.BeginNew();
        }

        protected override string ListQueryCriteria()
        {
            return Modern.Lab.Hosting.NewItems.NewItemTracker.Criteria(this.searchEqpGrpTyp);
        }

        private string SelectedEqpGrpTyp()
        {
            string value = Convert.ToString(this.cboEqpGrpTyp.SelectedValue);
            return string.IsNullOrEmpty(value) ? this.cboEqpGrpTyp.Text.Trim() : value.Trim();
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

        private void OnEquipmentGroupSelectionChanged(object sender, EventArgs e)
        {
            this.HandleSelectionChanged();
        }
    }
}
