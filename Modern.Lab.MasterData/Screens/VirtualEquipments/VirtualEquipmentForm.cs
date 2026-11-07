using System;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.MasterData;

namespace Modern.Lab.MasterData
{
    public partial class VirtualEquipmentForm : MasterDataCrudFormBase
    {
        public VirtualEquipmentForm()
        {
            this.InitializeComponent();
            this.InitializeCrud(CreateDefinition());
        }

        private static MasterDataCrudDefinition CreateDefinition()
        {
            return new MasterDataCrudDefinition
            {
                EntityName = "Virtual Equipment",
                Entity = Schemas.VirtualEquipment,
                ListTableId = "VirtualEquipment.SelectVirtualEquipments.VirtualEquipment"
            };
        }

        private void OnVirtualEquipmentSelectionChanged(object sender, EventArgs e)
        {
            this.HandleSelectionChanged();
        }
    }
}
