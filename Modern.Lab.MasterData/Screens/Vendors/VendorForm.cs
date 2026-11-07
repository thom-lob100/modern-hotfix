using System;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.MasterData;

namespace Modern.Lab.MasterData
{
    public partial class VendorForm : MasterDataCrudFormBase
    {
        public VendorForm()
        {
            this.InitializeComponent();
            this.InitializeCrud(CreateDefinition());
        }

        private static MasterDataCrudDefinition CreateDefinition()
        {
            return new MasterDataCrudDefinition
            {
                EntityName = "Vendor",
                Entity = Schemas.Vendor,
                ListTableId = "Vendor.SelectVendors.Vendor"
            };
        }

        private void OnVendorSelectionChanged(object sender, EventArgs e)
        {
            this.HandleSelectionChanged();
        }
    }
}
