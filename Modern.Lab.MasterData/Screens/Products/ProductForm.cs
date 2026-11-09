using System;
using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.WinForms.Controls.Selection;

using Modern.Lab.Hosting.MasterData;

namespace Modern.Lab.MasterData
{
    public partial class ProductForm : MasterDataCrudFormBase
    {
        private const string ChoiceValueColumn = "VALUE";
        private const string ChoiceLabelColumn = "LABEL";
        private const string LotCodeRequest = "GetLotCodeList";

        private string searchSubProdTyp = string.Empty;
        private string searchLotCd = string.Empty;
        private string searchProdId = string.Empty;

        public ProductForm()
        {
            this.InitializeComponent();
            this.InitializeCrud(CreateDefinition());
            this.InitializeSearchChoices();
            this.RefreshCrudActionState();
        }

        private static MasterDataCrudDefinition CreateDefinition()
        {
            return new MasterDataCrudDefinition
            {
                EntityName = "Product",
                Entity = Schemas.Product,
                ListTableId = "Product.SelectProducts.Product"
            };
        }

        protected override void ResetCriteria()
        {
            ClearChoice(this.cboSubProdTyp);
            ClearChoice(this.cboLotCd);
            this.txtProdId.Text = string.Empty;
        }

        protected override void OnListLoading()
        {
            this.searchSubProdTyp = this.cboSubProdTyp.Text.Trim();
            this.searchLotCd = this.cboLotCd.Text.Trim();
            this.searchProdId = this.txtProdId.Text.Trim();
        }

        protected override string ListQueryCriteria(string keyword)
        {
            return Modern.Lab.Hosting.NewItems.NewItemTracker.Criteria(this.searchSubProdTyp, this.searchLotCd, this.searchProdId);
        }

        private void InitializeSearchChoices()
        {
            this.cboSubProdTyp.DisplayMember = ChoiceLabelColumn;
            this.cboSubProdTyp.ValueMember = ChoiceValueColumn;
            this.cboSubProdTyp.DataSource = SubProdTypChoices();
            ClearChoice(this.cboSubProdTyp);

            this.cboLotCd.UseEntity(Schemas.LotCode);
            this.ShareLookup(this.cboLotCd, Schemas.Product.LotCd.Name);
        }

        private static void ClearChoice(ModernComboBox combo)
        {
            combo.SelectedIndex = -1;
            combo.Text = string.Empty;
        }

        private static DataTable SubProdTypChoices()
        {
            DataTable choices = new DataTable();
            choices.Columns.Add(ChoiceValueColumn, typeof(string));
            choices.Columns.Add(ChoiceLabelColumn, typeof(string));
            foreach (string code in Schemas.Product.SubProdTyp.ValueCodes())
            {
                choices.Rows.Add(code, code);
            }

            return choices;
        }

        private void OnProductSelectionChanged(object sender, EventArgs e)
        {
            this.HandleSelectionChanged();
        }
    }
}
