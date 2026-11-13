using System;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.MasterData;
using Modern.Lab.WinForms.Controls.Selection;

namespace Modern.Lab.MasterData
{
    public partial class EquipmentForm : MasterDataCrudFormBase
    {
        private string searchFacId = string.Empty;
        private string searchEqpTyp = string.Empty;
        private string searchDetEqpTyp = string.Empty;
        private string searchEqpGrpId = string.Empty;

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

        protected override void ResetCriteria()
        {
            foreach (ModernComboBox combo in this.CriteriaCombos())
            {
                combo.SelectedIndex = -1;
                combo.Text = string.Empty;
            }
        }

        protected override void OnListLoading()
        {
            this.searchFacId = SelectedValue(this.cboFacId);
            this.searchEqpTyp = SelectedValue(this.cboEqpTyp);
            this.searchDetEqpTyp = SelectedValue(this.cboDetEqpTyp);
            this.searchEqpGrpId = SelectedValue(this.cboEqpGrpId);
        }

        protected override string ListQueryCriteria()
        {
            return Modern.Lab.Hosting.NewItems.NewItemTracker.Criteria(
                    this.searchFacId, this.searchEqpTyp, this.searchDetEqpTyp, this.searchEqpGrpId);
        }

        private ModernComboBox[] CriteriaCombos()
        {
            return new ModernComboBox[] { this.cboFacId, this.cboEqpTyp, this.cboDetEqpTyp, this.cboEqpGrpId };
        }

        private static string SelectedValue(ModernComboBox combo)
        {
            string value = Convert.ToString(combo.SelectedValue);
            return string.IsNullOrEmpty(value) ? combo.Text.Trim() : value.Trim();
        }

        private void OnEquipmentSelectionChanged(object sender, EventArgs e)
        {
            this.HandleSelectionChanged();
        }
    }
}
