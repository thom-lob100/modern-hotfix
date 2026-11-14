using System;
using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.MasterData;
using Modern.Lab.WinForms.Controls.Selection;

namespace Modern.Lab.MasterData
{
    public partial class OperForm : MasterDataCrudFormBase
    {
        private string searchMidOperTyp = string.Empty;
        private string searchDetOperTyp = string.Empty;

        public OperForm()
        {
            this.InitializeComponent();
            this.InitializeCrud(CreateDefinition());
        }

        private static MasterDataCrudDefinition CreateDefinition()
        {
            return new MasterDataCrudDefinition
            {
                EntityName = "Oper",
                Entity = Schemas.Oper,
                ListTableId = "Oper.SelectOpers.Oper"
            };
        }

        protected override void ResetCriteria()
        {
            foreach (ModernComboBox combo in new ModernComboBox[] { this.cboMidOperTyp, this.cboDetOperTyp })
            {
                combo.SelectedIndex = -1;
                combo.Text = string.Empty;
            }
        }

        protected override void OnRefreshClick(object sender, EventArgs e)
        {
            if (this.ActionInProgress)
            {
                return;
            }

            this.searchMidOperTyp = ShownValue(this.cboMidOperTyp);
            this.searchDetOperTyp = ShownValue(this.cboDetOperTyp);
            base.OnRefreshClick(sender, e);
        }

        protected override string ListQueryCriteria()
        {
            return Modern.Lab.Hosting.NewItems.NewItemTracker.Criteria(this.searchMidOperTyp, this.searchDetOperTyp);
        }

        private static string ShownValue(ModernComboBox combo)
        {
            string shown = combo.Text.Trim();
            DataRowView view = combo.SelectedItem as DataRowView;
            DataRow chosen = view == null ? combo.SelectedItem as DataRow : view.Row;

            if (chosen == null
                    || !chosen.Table.Columns.Contains(combo.DisplayMember)
                    || !chosen.Table.Columns.Contains(combo.ValueMember)
                    || !string.Equals(Convert.ToString(chosen[combo.DisplayMember]).Trim(), shown, StringComparison.Ordinal))
            {
                return shown;
            }

            return Convert.ToString(chosen[combo.ValueMember]).Trim();
        }

        private void OnOperSelectionChanged(object sender, EventArgs e)
        {
            this.HandleSelectionChanged();
        }
    }
}
