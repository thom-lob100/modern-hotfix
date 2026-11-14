using System;
using System.Collections.Generic;
using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.MasterData;

namespace Modern.Lab.MasterData
{
    public partial class PortForm : MasterDataCrudFormBase
    {
        private string searchEqpId = string.Empty;

        public PortForm()
        {
            this.InitializeComponent();
            this.InitializeCrud(CreateDefinition());
            this.CrudEditor.Field(Schemas.Port.EqpId.Name).ReadOnly();
        }

        private static MasterDataCrudDefinition CreateDefinition()
        {
            return new MasterDataCrudDefinition
            {
                EntityName = "Port",
                Entity = Schemas.Port,
                ListTableId = "Port.SelectPorts.Port"
            };
        }

        protected override void OnFormLoad(object sender, EventArgs e)
        {
            this.ClosePorts();
        }

        protected override void OnRefreshClick(object sender, EventArgs e)
        {
            if (this.ActionInProgress)
            {
                return;
            }

            string eqpId = this.ShownEqpId();

            if (eqpId.Length == 0)
            {
                this.ClosePorts();
                return;
            }

            this.searchEqpId = eqpId;
            this.ReloadItems();
        }

        protected override void ResetCriteria()
        {
            this.cboEqpId.SelectedIndex = -1;
            this.cboEqpId.Text = string.Empty;
        }

        protected override void OnListLoading()
        {
            this.ClearPorts("No ports");
        }

        protected override string ListQueryCriteria()
        {
            return Modern.Lab.Hosting.NewItems.NewItemTracker.Criteria(this.searchEqpId);
        }

        protected override IDictionary<string, string> NewItemDefaults()
        {
            return new Dictionary<string, string> { { Schemas.Port.EqpId.Name, this.searchEqpId } };
        }

        private void ClosePorts()
        {
            this.ClearPorts("Select an Eqp Id");
            this.CloseItems();
            this.LoadLookupSources();
        }

        private void ClearPorts(string emptyText)
        {
            this.gridPorts.DataSource = null;
            this.gridPorts.EmptyText = emptyText;
            this.CrudEditor.SetSchema(new DataTable());
            this.CrudEditor.BeginNew();
        }

        private string ShownEqpId()
        {
            string shown = this.cboEqpId.Text.Trim();
            DataRowView view = this.cboEqpId.SelectedItem as DataRowView;
            DataRow chosen = view == null ? this.cboEqpId.SelectedItem as DataRow : view.Row;

            if (chosen == null
                    || !chosen.Table.Columns.Contains(this.cboEqpId.DisplayMember)
                    || !chosen.Table.Columns.Contains(this.cboEqpId.ValueMember)
                    || !string.Equals(Convert.ToString(chosen[this.cboEqpId.DisplayMember]).Trim(), shown, StringComparison.Ordinal))
            {
                return shown;
            }

            return Convert.ToString(chosen[this.cboEqpId.ValueMember]).Trim();
        }

        private void OnPortSelectionChanged(object sender, EventArgs e)
        {
            this.HandleSelectionChanged();
        }
    }
}
