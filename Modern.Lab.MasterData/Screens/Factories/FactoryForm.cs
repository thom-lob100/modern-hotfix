using System;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.MasterData;

namespace Modern.Lab.MasterData
{
    public partial class FactoryForm : MasterDataCrudFormBase
    {
        private string searchFacTyp = string.Empty;

        public FactoryForm()
        {
            this.InitializeComponent();
            this.InitializeCrud(CreateDefinition());
            this.ShareLookup(this.cboFacTyp, Schemas.Factory.FacTyp.Name);
        }

        private static MasterDataCrudDefinition CreateDefinition()
        {
            return new MasterDataCrudDefinition
            {
                EntityName = "Factory",
                Entity = Schemas.Factory,
                ListTableId = "Factory.SelectFactories.Factory"
            };
        }

        protected override void ResetCriteria()
        {
            this.cboFacTyp.SelectedIndex = -1;
            this.cboFacTyp.Text = string.Empty;
        }

        protected override void OnListLoading()
        {
            this.searchFacTyp = this.cboFacTyp.Text.Trim();
        }

        protected override string ListQueryCriteria()
        {
            return Modern.Lab.Hosting.NewItems.NewItemTracker.Criteria(this.searchFacTyp);
        }

        private void OnFactorySelectionChanged(object sender, EventArgs e)
        {
            this.HandleSelectionChanged();
        }
    }
}
