using System;
using System.Collections.Generic;
using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.WinForms.Controls.Selection;

using Modern.Lab.Hosting.MasterData;

namespace Modern.Lab.MasterData
{
    public partial class ProductForm : MasterDataCrudFormBase
    {
        private const string ChannelCombos = "combos";
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

        protected override void OnActionMenuOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            base.OnActionMenuOpening(sender, e);
            this.miDuplicate.Enabled = this.DuplicateSourceKey().Length > 0;
        }

        private string DuplicateSourceKey()
        {
            return this.CrudEditor.IsNew ? string.Empty : this.CrudEditor.ReadKey().Trim();
        }

        private void OnDuplicateClick(object sender, EventArgs e)
        {
            string key = this.DuplicateSourceKey();

            if (key.Length == 0)
            {
                return;
            }

            if (!this.Confirm("Duplicate product '" + key + "'?", "Duplicate Product"))
            {
                return;
            }

            this.RunAction(
                    () => this.DuplicateProduct(key),
                    reply => this.AfterDuplicated(reply),
                    "Duplicating product…");
        }

        private void AfterDuplicated(DataActionResult reply)
        {
            this.ShowToast(reply.Message.Length > 0 ? reply.Message : "Duplicated.");
            this.ReloadItems();
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

        protected override void LoadLookupSources()
        {
            this.LoadAsync(
                    ChannelCombos,
                    () => this.CrudEditor.FetchComboSources(this.RequestLookupItems),
                    this.ApplyComboSources);
        }

        private void ApplyComboSources(Dictionary<string, DataTable> sources)
        {
            this.CrudEditor.ApplyComboSources(sources);
            DataTable lotCodes;

            if (sources != null && sources.TryGetValue(Schemas.Product.LotCd.Name, out lotCodes))
            {
                this.ApplyLotCodeChoices(lotCodes);
            }
        }

        private void InitializeSearchChoices()
        {
            this.cboSubProdTyp.DisplayMember = ChoiceLabelColumn;
            this.cboSubProdTyp.ValueMember = ChoiceValueColumn;
            this.cboSubProdTyp.DataSource = SubProdTypChoices();
            ClearChoice(this.cboSubProdTyp);

            this.cboLotCd.UseEntity(Schemas.LotCode);
        }

        private void ApplyLotCodeChoices(DataTable lotCodes)
        {
            string keep = this.cboLotCd.Text.Trim();
            this.cboLotCd.DataSource = lotCodes == null ? null : lotCodes.Copy();
            ClearChoice(this.cboLotCd);
            this.cboLotCd.Text = keep;
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
