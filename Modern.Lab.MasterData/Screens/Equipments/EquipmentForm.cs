using System;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.MasterData;

namespace Modern.Lab.MasterData
{
    public partial class EquipmentForm : MasterDataCrudFormBase
    {
        public EquipmentForm()
        {
            this.InitializeComponent();
            this.InitializeCrud(CreateDefinition());
        }

        private static MasterDataCrudDefinition CreateDefinition()
        {
            return new MasterDataCrudDefinition
            {
                EntityName = "Equipment",
                Entity = Schemas.Equipment,
                ListTableId = "Equipment.SelectEquipments.Equipment"
            };
        }

        private void OnEquipmentSelectionChanged(object sender, EventArgs e)
        {
            this.HandleSelectionChanged();
        }
    }
}
