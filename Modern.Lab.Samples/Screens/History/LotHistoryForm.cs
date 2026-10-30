using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Controls.Wpf.Input;
using Modern.Lab.Data;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Display;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{

    public partial class LotHistoryForm : ModernFormBase
    {

        private const string channelSuggest = "suggest";
        private const string channelTree = "tree";
        private const string channelLotDetail = "lotDetail";
        private const string channelWaferHistory = "waferHistory";

        private DataTable treeData;

        private bool searchReady;
        private bool suppressTreeEvent;

        private string lotHistoryOwner;
        private string waferHistoryOwner;

        private static string DurationColor
        {
            get { return Modern.Lab.Theming.ModernTheme.IsDarkBased ? "#4DD0C2" : "#0F7B6C"; }
        }

        public LotHistoryForm()
        {
            this.InitializeComponent();
            SearchBandLayout.Attach(this.searchCard,
                    new SearchBandLayout.Field[]
                    {
                        SearchBandLayout.Pair(this.lblLotId, this.txtLotId),
                    },
                    new Control[] { this.btnReset, this.btnRefresh });

            this.InitializeModernForm();
            this.RegisterRefreshShortcut(this.btnRefresh);

            this.DeferredResize = true;

            this.btnExcel.BringToFront();

            this.RegisterFindShortcut(this.gridLotHistory, this.gridWaferHistory, this.gridWafers);
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            this.txtLotId.AutoCompleteMode = AutoCompleteMode.Suggest;
            this.txtLotId.AutoCompleteSource = AutoCompleteSource.CustomSource;
            this.txtLotId.SuggestionFilterMode = SuggestionFilterMode.None;

            this.treeLotTree.UseEntity(Schemas.LotTree);
            this.treeLotTree.DisplayMember = Schemas.LotTree.DisplayField.Name;
            this.treeLotTree.RowColorMember = NodeColorColumn;

            this.gridWafers.RowKeyMember = Schemas.Wafer.RowKey;
            this.gridLotHistory.RowKeyMember = Schemas.LotHistory.RowKey;
            this.fieldDetail.FieldLinkClick += this.OnInfoLinkClick;
            this.gridWaferHistory.RowKeyMember = Schemas.WaferHistory.RowKey;

            this.ClearSelection();
            this.searchReady = true;
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            this.ExecuteSearchAsync();
        }

        private void OnResetClick(object sender, EventArgs e)
        {
            this.InvalidateChannel(channelTree);
            this.InvalidateChannel(channelSuggest);
            this.CancelDebounce(channelSuggest);
            this.txtLotId.CloseSuggestions();
            this.ResetBusy();
            this.txtLotId.Text = string.Empty;
            this.treeData = null;
            this.treeLotTree.DataSource = null;
            this.ClearSelection();
        }

        private async void ExecuteSearchAsync()
        {
            if (!this.searchReady)
            {
                return;
            }

            string keyword = this.txtLotId.Text.Trim();

            if (keyword.Length == 0)
            {
                this.ShowWarningMessage("Lot Id is required.");
                return;
            }

            this.CancelDebounce(channelSuggest);
            this.InvalidateChannel(channelSuggest);
            this.txtLotId.CloseSuggestions();

            this.InvalidateChannel(channelLotDetail);
            this.InvalidateChannel(channelWaferHistory);

            LoadOutcome<DataTable> outcome;

            using (this.Busy("Loading history...", keyword))
            {
                outcome = await this.FetchAsync(channelTree, () => this.GetLotTree(keyword));
            }

            if (!outcome.IsCurrent)
            {
                return;
            }

            DataTable tree = CopyOf(outcome.Failure != null ? null : outcome.Value);

            if (tree == null)
            {
                this.treeData = null;
                this.treeLotTree.DataSource = null;
                this.ClearSelection();
                return;
            }

            ApplyScrapColor(tree);
            this.treeData = tree;

            this.suppressTreeEvent = true;

            try
            {
                this.treeLotTree.DataSource = tree;
                this.treeLotTree.SelectedValue = FindAutoSelectId(tree, keyword);
            }
            finally
            {
                this.suppressTreeEvent = false;
            }

            this.ApplyTreeSelection();
        }

        private void OnLotIdTextChanged(object sender, EventArgs e)
        {
            if (!this.searchReady)
            {
                return;
            }

            if (this.IsExistingCandidate(this.txtLotId.Text.Trim()))
            {
                this.CancelDebounce(channelSuggest);
                return;
            }

            this.Debounce(channelSuggest, 300, new MethodInvoker(this.LoadIdCandidatesAsync));
        }

        private bool IsExistingCandidate(string text)
        {
            AutoCompleteStringCollection source = this.txtLotId.AutoCompleteCustomSource;

            if (source == null || text.Length == 0)
            {
                return false;
            }

            foreach (string candidate in source)
            {
                if (string.Equals(candidate, text, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private async void LoadIdCandidatesAsync()
        {
            string keyword = this.txtLotId.Text.Trim();

            if (keyword.Length == 0)
            {
                this.txtLotId.AutoCompleteCustomSource = null;
                return;
            }

            LoadOutcome<DataTable> outcome = await this.FetchAsync(
                    channelSuggest, () => this.GetLotIdCandidates(keyword));

            if (!outcome.IsCurrent || outcome.Failure != null || outcome.Value == null)
            {
                return;
            }

            AutoCompleteStringCollection collection = new AutoCompleteStringCollection();

            foreach (DataRow row in outcome.Value.Rows)
            {
                string candidate = TableHelper.CellText(row, Schemas.Lot.LotId.Name);

                if (candidate.Length > 0)
                {
                    collection.Add(candidate);
                }
            }

            this.txtLotId.AutoCompleteCustomSource = collection;
        }

        private static string FindAutoSelectId(DataTable tree, string keyword)
        {
            foreach (DataRow row in tree.Rows)
            {
                string lotId = TableHelper.CellText(row, Schemas.LotTree.LotId.Name);

                if (string.Equals(lotId, keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return lotId;
                }
            }

            return tree.Rows.Count > 0 ? TableHelper.CellText(tree.Rows[0], Schemas.LotTree.LotId.Name) : null;
        }

        private void OnTreeSelectionChanged(object sender, EventArgs e)
        {
            if (!this.searchReady || this.suppressTreeEvent)
            {
                return;
            }

            this.ApplyTreeSelection();
        }

        private void ApplyTreeSelection()
        {
            DataRowView selected = this.treeLotTree.SelectedItem as DataRowView;

            if (selected == null || string.IsNullOrWhiteSpace(TableHelper.CellText(selected, Schemas.LotTree.LotId.Name)))
            {
                this.ClearSelection();
                return;
            }

            this.FillDetail(selected);
            this.ClearWaferHistory();
            this.LoadLotDetailsAsync(TableHelper.CellText(selected, Schemas.LotTree.LotId.Name));
        }

        private void FillDetail(DataRowView row)
        {
            this.fieldDetail.SetRow(row.Row);
        }

        private void OnInfoLinkClick(object sender, ModernFieldLinkClickEventArgs e)
        {
            if (e.Member == ReqSerialNoColumn)
            {
                this.ShowRequestInfo(e.Value);
            }
        }

        protected override void ShowRequestInfoDialog(DataTable requestInfo)
        {
            using (RequestInfoDialogForm dialog = new RequestInfoDialogForm())
            {
                dialog.SetRequest(requestInfo, RequestSampleFirstColumn, RequestPurposeColumn);
                dialog.ShowDialog(this);
            }
        }

        private async void LoadLotDetailsAsync(string lotId)
        {
            IDisposable cover = this.Busy("Loading history...", lotId, this.rightZone);
            LoadOutcome<DataTable[]> outcome;

            try
            {
                outcome = await this.FetchAsync(
                        channelLotDetail,
                        () => new DataTable[]
                        {
                            this.GetLotHistory(lotId),
                            this.GetWafers(lotId)
                        });
            }
            finally
            {
                cover.Dispose();
            }

            if (!outcome.IsCurrent)
            {
                return;
            }

            if (outcome.Failure != null)
            {
                this.ClearLotDetails();
                return;
            }

            DataTable history = outcome.Value[0];
            DataTable wafers = outcome.Value[1];

            LotHistoryPresenter.AddDurationColumn(history);
            this.gridLotHistory.DataSource = history;
            this.gridLotHistory.StatusText = lotId + LotHistoryPresenter.CycleTimeSuffix(history);
            this.tabHistory.SetTabTitle(0, "Lot History — " + lotId);

            this.lotHistoryOwner = lotId;

            this.BindWafers(wafers, lotId);
        }

        private void BindWafers(DataTable wafers, string lotId)
        {
            DataTable table = CopyOf(wafers);
            if (table == null)
            {
                this.gridWafers.DataSource = null;
                this.waferCard.Text = "Wafers";
                this.ClearWaferHistory();
                return;
            }
            this.gridWafers.DataSource = table;
            this.waferCard.Text = "Wafers — " + lotId;
        }

        private void OnWaferSelectionChanged(object sender, EventArgs e)
        {
            DataRowView row = this.gridWafers.SelectedItem as DataRowView;

            if (row == null || string.IsNullOrWhiteSpace(this.lotHistoryOwner))
            {
                this.ClearWaferHistory();
                return;
            }

            string waferId = TableHelper.CellText(row, Schemas.Wafer.WfId.Name);

            if (string.IsNullOrWhiteSpace(waferId))
            {
                this.ClearWaferHistory();
                return;
            }

            this.LoadWaferHistoryAsync(this.lotHistoryOwner, waferId);
        }

        private async void LoadWaferHistoryAsync(string lotId, string waferId)
        {
            LoadOutcome<DataTable> outcome = await this.FetchAsync(
                    channelWaferHistory, () => this.GetWaferHistory(lotId, waferId));

            if (!outcome.Succeeded)
            {
                return;
            }

            DataTable history = outcome.Value;
            LotHistoryPresenter.AddDurationColumn(history);
            this.gridWaferHistory.DataSource = history;
            this.gridWaferHistory.StatusText = waferId + LotHistoryPresenter.CycleTimeSuffix(history);
            this.tabHistory.SetTabTitle(1, "Wafer History — " + waferId);

            this.waferHistoryOwner = waferId;
        }

        private void OnExportClick(object sender, EventArgs e)
        {
            bool waferTab = this.tabHistory.SelectedIndex == 1;
            ModernDataGrid grid = waferTab ? this.gridWaferHistory : this.gridLotHistory;

            string sheetName = waferTab ? "Wafer History" : "Lot History";
            string owner = waferTab ? this.waferHistoryOwner : this.lotHistoryOwner;

            this.ExportToExcel(
                    grid,
                    sheetName,
                    grid.DataSource as DataTable,
                    sheetName.Replace(" ", string.Empty)
                            + (string.IsNullOrEmpty(owner) ? string.Empty : "_" + owner));
        }

        private void ClearSelection()
        {
            this.InvalidateChannel(channelLotDetail);

            this.fieldDetail.ClearValues();

            this.ClearLotDetails();
            this.ClearWaferHistory();
        }

        private void ClearLotDetails()
        {
            this.gridWafers.DataSource = null;
            this.waferCard.Text = "Wafers";
            this.gridLotHistory.DataSource = null;
            this.gridLotHistory.StatusText = string.Empty;
            this.tabHistory.SetTabTitle(0, "Lot History");

            this.lotHistoryOwner = null;
        }

        private void ClearWaferHistory()
        {
            this.InvalidateChannel(channelWaferHistory);

            this.gridWaferHistory.DataSource = null;
            this.gridWaferHistory.StatusText = string.Empty;
            this.tabHistory.SetTabTitle(1, "Wafer History");

            this.waferHistoryOwner = null;
        }

        private static DataTable CopyOf(DataTable incoming)
        {
            return incoming == null ? null : incoming.Copy();
        }
    }
}
