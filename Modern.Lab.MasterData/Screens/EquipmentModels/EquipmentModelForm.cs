using System;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.MasterData;

namespace Modern.Lab.MasterData
{
    public partial class EquipmentModelForm : MasterDataCrudFormBase
    {
        public EquipmentModelForm()
        {
            this.InitializeComponent();
            this.InitializeCrud(CreateDefinition());
        }

        private static MasterDataCrudDefinition CreateDefinition()
        {
            return new MasterDataCrudDefinition
            {
                EntityName = "Equipment Model",
                Entity = Schemas.EquipmentModel,
                ListTableId = "EquipmentModel.SelectEquipmentModels.EquipmentModel"
            };
        }

        private void OnEquipmentModelSelectionChanged(object sender, EventArgs e)
        {
            this.HandleSelectionChanged();
        }
    }
}
