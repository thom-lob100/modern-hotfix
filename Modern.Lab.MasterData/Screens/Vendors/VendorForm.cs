using System;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.MasterData;

namespace Modern.Lab.MasterData
{
    public partial class VendorForm : MasterDataCrudFormBase
    {
        private string searchVendorTyp = string.Empty;

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

        protected override void ResetCriteria()
        {
            this.cboVendorTyp.SelectedIndex = -1;
            this.cboVendorTyp.Text = string.Empty;
        }

        protected override void OnRefreshClick(object sender, EventArgs e)
        {
            if (this.ActionInProgress)
            {
                return;
            }

            this.searchVendorTyp = this.SelectedVendorTyp();
            base.OnRefreshClick(sender, e);
        }

        protected override string ListQueryCriteria()
        {
            return Modern.Lab.Hosting.NewItems.NewItemTracker.Criteria(this.searchVendorTyp);
        }

        private string SelectedVendorTyp()
        {
            string value = Convert.ToString(this.cboVendorTyp.SelectedValue);
            return string.IsNullOrEmpty(value) ? this.cboVendorTyp.Text.Trim() : value.Trim();
        }

        private void OnVendorSelectionChanged(object sender, EventArgs e)
        {
            this.HandleSelectionChanged();
        }
    }
}
