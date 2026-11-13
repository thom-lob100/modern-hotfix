using System;
using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.MasterData;

namespace Modern.Lab.MasterData
{
    public partial class AreaBayForm : MasterDataCrudFormBase
    {
        private string searchDetAreaTyp = string.Empty;

        public AreaBayForm()
        {
            this.InitializeComponent();
            this.InitializeCrud(CreateDefinition());
            this.RefreshCrudActionState();
        }

        private static MasterDataCrudDefinition CreateDefinition()
        {
            return new MasterDataCrudDefinition
            {
                EntityName = "Area / Bay",
                Entity = Schemas.Area,
                ListTableId = "AreaBay.SelectLocations.Location"
            };
        }

        protected override void ResetCriteria()
        {
            this.cboDetAreaTyp.SelectedIndex = -1;
            this.cboDetAreaTyp.Text = string.Empty;
        }

        protected override void OnListLoading()
        {
            this.searchDetAreaTyp = this.SelectedDetAreaTyp();
            this.gridAreas.DataSource = null;
            this.CrudEditor.SetSchema(new DataTable());
            this.CrudEditor.BeginNew();
            this.LoadAsync("area-lookups",
                    () => this.CrudEditor.FetchComboSources(this.RequestLookupItems),
                    this.CrudEditor.ApplyComboSources);
        }

        protected override string ListQueryCriteria()
        {
            return Modern.Lab.Hosting.NewItems.NewItemTracker.Criteria(this.searchDetAreaTyp);
        }

        private string SelectedDetAreaTyp()
        {
            string value = Convert.ToString(this.cboDetAreaTyp.SelectedValue);
            return string.IsNullOrEmpty(value) ? this.cboDetAreaTyp.Text.Trim() : value.Trim();
        }

        private void OnAreaSelectionChanged(object sender, EventArgs e)
        {
            this.HandleSelectionChanged();
        }
    }
}
