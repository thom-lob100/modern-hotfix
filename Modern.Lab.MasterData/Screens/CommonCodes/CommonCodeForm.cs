using System;
using System.Collections.Generic;
using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.MasterData;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.ResponseContracts;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Selection;

namespace Modern.Lab.MasterData
{
    public partial class CommonCodeForm : MasterDataCrudFormBase
    {
        private const string TypesChannel = "types";

        private readonly bool manageTypes;
        private bool bindingTypes;
        private bool loadingTypes;
        private bool typeSelectionQueued;
        private System.Windows.Controls.TextChangedEventHandler typeTextChanged;
        private string commonTyp = string.Empty;
        private string queryCommonTyp = string.Empty;
        private string actionCommonTyp = string.Empty;

        public CommonCodeForm() : this(false)
        {
        }

        internal CommonCodeForm(bool manageTypes)
        {
            this.manageTypes = manageTypes;
            this.InitializeComponent();
            this.ApplyScreenMode();
            this.InitializeCrud(this.CreateDefinition());
            if (!this.manageTypes)
            {
                this.CrudEditor.Field(Schemas.CommonTypeDet.CommonTyp.Name).ReadOnly();
                this.cmbCommonType.UseEntity(Schemas.CommonType);
                this.cmbCommonType.SelectedIndexChanged += this.OnCommonTypeChanged;
                this.btnRefreshTypes.Click += (sender, e) => this.LoadTypes(false);
                this.typeTextChanged = this.OnCommonTypeTextChanged;
                this.cmbCommonType.Child.AddHandler(System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent,
                        this.typeTextChanged, true);
            }
            this.SyncQueryInputs();
        }

        private void ApplyScreenMode()
        {
            if (!this.manageTypes)
            {
                return;
            }
            this.Text = "Common Type Master";
            this.lblTitle.Text = "Common Type Master — Session demo - changes reset on restart";
            this.typeBar.Visible = false;
            this.listCard.Text = "Common Type List";
            this.editorCard.Text = "Common Type";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.typeTextChanged != null && this.cmbCommonType.Child != null)
            {
                this.cmbCommonType.Child.RemoveHandler(System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent,
                        this.typeTextChanged);
                this.typeTextChanged = null;
            }
            if (disposing && this.components != null)
            {
                this.components.Dispose();
                this.components = null;
            }
            base.Dispose(disposing);
        }

        private MasterDataCrudDefinition CreateDefinition()
        {
            if (this.manageTypes)
            {
                return new MasterDataCrudDefinition
                {
                    EntityName = "Common Type",
                    Entity = Schemas.CommonType,
                    ListTableId = "CommonCode.SelectCommonTypes.CommonType"
                };
            }
            return new MasterDataCrudDefinition
            {
                EntityName = "Common Code",
                Entity = Schemas.CommonTypeDet,
                ListTableId = "CommonCode.SelectCommonCodes.CommonCode"
            };
        }

        protected override void OnFormLoad(object sender, EventArgs e)
        {
            if (this.manageTypes)
            {
                base.OnFormLoad(sender, e);
            }
            else
            {
                this.LoadTypes(true);
            }
        }

        private bool CanQuery
        {
            get
            {
                return !this.ActionInProgress && !this.loadingTypes
                        && (this.manageTypes || (this.commonTyp.Length > 0 && this.commonTyp == this.SelectedCommonTyp));
            }
        }

        protected override void OnRefreshClick(object sender, EventArgs e)
        {
            if (this.CanQuery) { base.OnRefreshClick(sender, e); }
        }

        protected override void OnWriteLockChanged(bool locked)
        {
            this.SyncQueryInputs();
        }

        private void SyncQueryInputs()
        {
            bool writing = this.ActionInProgress;
            this.btnRefresh.Enabled = !writing && !this.loadingTypes
                    && (this.manageTypes || this.SelectedCommonTyp.Length > 0);
            this.cmbCommonType.Enabled = !writing && !this.loadingTypes;
            this.btnRefreshTypes.Enabled = !writing && !this.loadingTypes;
        }

        protected override void OnListLoading()
        {
            this.queryCommonTyp = this.manageTypes ? string.Empty : this.commonTyp;
            this.ClearEditorPane("Loading…");
        }

        private void ClearEditorPane(string emptyText)
        {
            this.gridCommonCodes.DataSource = null;
            this.gridCommonCodes.EmptyText = emptyText;
            this.CrudEditor.SetSchema(new DataTable());
            this.CrudEditor.BeginNew();
        }

        protected override IDictionary<string, string> NewItemDefaults()
        {
            if (this.manageTypes)
            {
                return null;
            }
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { Schemas.CommonTypeDet.CommonTyp.Name, this.queryCommonTyp }
            };
        }

        protected override bool CanStartAction()
        {
            if (!this.manageTypes && (this.queryCommonTyp.Length == 0 || this.queryCommonTyp != this.commonTyp
                    || this.commonTyp != this.SelectedCommonTyp))
            {
                return false;
            }
            if (!base.CanStartAction())
            {
                return false;
            }
            this.actionCommonTyp = this.queryCommonTyp;
            return true;
        }

        private void OnListSelectionChanged(object sender, EventArgs e)
        {
            if (this.gridCommonCodes.DataSource != null)
            {
                this.gridCommonCodes.EmptyText = "No matching items.";
            }
            this.HandleSelectionChanged();
        }

        private string SelectedCommonTyp
        {
            get
            {
                DataRowView view = this.cmbCommonType.SelectedItem as DataRowView;
                DataRow row = view == null ? this.cmbCommonType.SelectedItem as DataRow : view.Row;
                if (row == null || !string.Equals(this.cmbCommonType.Text,
                        Convert.ToString(row[this.cmbCommonType.DisplayMember]), StringComparison.Ordinal))
                {
                    return string.Empty;
                }
                return Convert.ToString(row[this.cmbCommonType.ValueMember]);
            }
        }

        private void InvalidateCodes()
        {
            this.commonTyp = string.Empty;
            this.CloseItems();
            this.ClearEditorPane("Select a saved common type.");
            this.SyncQueryInputs();
        }

        private void OnCommonTypeTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.OnCommonTypeChanged(sender, EventArgs.Empty);
        }

        private void OnCommonTypeChanged(object sender, EventArgs e)
        {
            if (this.bindingTypes || this.loadingTypes || this.ActionInProgress)
            {
                return;
            }
            this.InvalidateCodes();
            this.QueueTypeSelection();
        }

        private void QueueTypeSelection()
        {
            if (this.typeSelectionQueued)
            {
                return;
            }
            this.typeSelectionQueued = true;
            this.PostToUi(() =>
            {
                this.typeSelectionQueued = false;
                if (this.loadingTypes || this.ActionInProgress)
                {
                    return;
                }
                string selected = this.SelectedCommonTyp;
                if (selected.Length == 0)
                {
                    this.InvalidateCodes();
                    return;
                }
                this.commonTyp = selected;
                this.SyncQueryInputs();
                if (this.CanQuery)
                {
                    this.ReloadItems();
                }
            });
        }

        private void LoadTypes(bool selectFirst)
        {
            if (this.ActionInProgress || this.loadingTypes)
            {
                return;
            }
            string previous = this.commonTyp;
            this.loadingTypes = true;
            this.bindingTypes = true;
            this.cmbCommonType.DataSource = null;
            this.cmbCommonType.Text = string.Empty;
            this.bindingTypes = false;
            this.InvalidateCodes();
            this.gridCommonCodes.EmptyText = "Loading common types…";
            bool applied = false;
            this.LoadAsync(TypesChannel, () => CheckedItems(this.GetCommonTypes(), false, string.Empty), table =>
            {
                string commonTypColumn = Schemas.CommonType.ValueField.Name;
                DataTable choices = new DataTable();
                choices.Columns.Add(commonTypColumn);
                int selected = selectFirst && table.Rows.Count > 0 ? 0 : -1;
                foreach (DataRow row in table.Rows)
                {
                    string key = Convert.ToString(row[commonTypColumn]);
                    choices.Rows.Add(key);
                    if (string.Equals(key, previous, StringComparison.OrdinalIgnoreCase))
                    {
                        selected = choices.Rows.Count - 1;
                    }
                }
                this.bindingTypes = true;
                try
                {
                    this.cmbCommonType.DataSource = choices;
                    this.cmbCommonType.SelectedIndex = selected;
                    if (selected < 0)
                    {
                        this.cmbCommonType.Text = string.Empty;
                    }
                }
                finally
                {
                    this.bindingTypes = false;
                }
                applied = true;
            }, current =>
            {
                if (!current)
                {
                    return;
                }
                this.loadingTypes = false;
                this.gridCommonCodes.EmptyText = applied ? "Select a saved common type."
                        : "Could not load common types. Click Refresh Types to retry.";
                if (applied)
                {
                    this.QueueTypeSelection();
                }
                this.PostToUi(this.SyncQueryInputs);
            });
        }

        protected override void OnLoadFailed(string channel, Exception failure)
        {
            if (channel != TypesChannel)
            {
                this.gridCommonCodes.EmptyText = "Could not load items. Search to retry.";
            }
            if (failure is ResponseContractException && !this.LoadFailureSilent)
            {
                this.ShowErrorMessage(this.QueryFailedCaption, ResponseColumns.MissingMessage, failure.Message);
                return;
            }
            base.OnLoadFailed(channel, failure);
        }

        protected override string ListQueryCriteria()
        {
            return Modern.Lab.Hosting.NewItems.NewItemTracker.Criteria(
                    this.manageTypes ? "TYPES" : "CODES", this.queryCommonTyp);
        }

        protected override DataTable RequestItems()
        {
            return this.manageTypes
                    ? CheckedItems(this.GetCommonTypes(), false, string.Empty)
                    : CheckedItems(this.GetCommonTypeDets(this.queryCommonTyp), true, this.queryCommonTyp);
        }

        private static DataTable CheckedItems(DataTable table, bool isDetail, string commonTyp)
        {
            string missing = ResponseColumns.Missing(table, isDetail ? CommonTypeDetColumns : CommonTypeColumns);
            if (missing.Length > 0)
            {
                throw new ResponseContractException("Common codes — missing columns: " + missing);
            }
            HashSet<string> keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow row in table.Rows)
            {
                string parent = Convert.ToString(row[Schemas.CommonTypeDet.CommonTyp.Name]);
                string key = Convert.ToString(row[isDetail ? Schemas.CommonTypeDet.TypVal.Name : Schemas.CommonType.CommonTyp.Name]);
                if (string.IsNullOrWhiteSpace(parent) || string.IsNullOrWhiteSpace(key) || !keys.Add(key)
                        || (isDetail && !string.Equals(parent, commonTyp, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ResponseContractException("Common codes — empty/duplicate key or mismatched COMMON_TYP.");
                }
            }
            return table;
        }

        protected override string[] ReadKeyValues()
        {
            if (this.manageTypes && !this.CrudEditor.IsNew)
            {
                return new string[] { this.CrudEditor.ReadValue(Schemas.CommonType.CommonTyp.Name) };
            }
            return base.ReadKeyValues();
        }

        protected override string[] SavedItemKey(DataActionResult reply, string[] savedKey, bool isNew)
        {
            if (this.manageTypes && !isNew)
            {
                return savedKey;
            }
            return base.SavedItemKey(reply, savedKey, isNew);
        }

    }
}
