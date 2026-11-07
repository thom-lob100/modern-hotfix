using System;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.MasterData;

namespace Modern.Lab.MasterData
{
    public partial class FactoryForm : MasterDataCrudFormBase
    {
        public FactoryForm()
        {
            this.InitializeComponent();
            this.InitializeCrud(CreateDefinition());
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

        private void OnFactorySelectionChanged(object sender, EventArgs e)
        {
            this.HandleSelectionChanged();
        }
    }
}
