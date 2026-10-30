using System;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Input;
using Modern.Lab.Data;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Input;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{

    public partial class TransportHistoryForm : ModernFormBase
    {

        private const string channelSuggest = "suggest";
        private const string channelList = "list";
        private const string channelHistory = "history";

        private DataTable transportData;

        private bool searchReady;
        private bool suppressListEvent;

        private string historyOwner;

        private static string DurationColor
        {
            get { return Modern.Lab.Theming.ModernTheme.IsDarkBased ? "#4DD0C2" : "#0F7B6C"; }
        }

        public TransportHistoryForm()
        {
            this.InitializeComponent();
            SearchBandLayout.Attach(this.searchCard,
                    new SearchBandLayout.Field[]
                    {
                        SearchBandLayout.Pair(this.lblType, this.cboType),
                        SearchBandLayout.Pair(this.lblDurableId, this.txtDurableId),
                    },
                    new Control[] { this.btnReset, this.btnRefresh });

            this.InitializeModernForm();
            this.RegisterRefreshShortcut(this.btnRefresh);

            this.DeferredResize = true;

            this.btnExcel.BringToFront();

            this.RegisterFindShortcut(this.gridTransports, this.gridHistory);
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            this.txtDurableId.AutoCompleteMode = AutoCompleteMode.Suggest;
            this.txtDurableId.AutoCompleteSource = AutoCompleteSource.CustomSource;
            this.txtDurableId.SuggestionFilterMode = SuggestionFilterMode.None;
            this.txtDurableId.UseEntity(Schemas.Transport);

            this.cboType.DisplayMember = Schemas.Transport.DurableTyp.Name;
            this.cboType.ValueMember = Schemas.Transport.DurableTyp.Name;
            this.cboType.DataSource = TypeTable();
            this.cboType.SelectedIndex = -1;

            this.gridTransports.RowKeyMember = Schemas.Transport.RowKey;
            this.gridHistory.RowKeyMember = Schemas.TransportHistory.RowKey;

            this.ClearSelection();
            this.searchReady = true;
        }

        private static DataTable TypeTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add(Schemas.Transport.DurableTyp.Name, typeof(string));

            foreach (string code in Schemas.Transport.DurableTyp.ValueCodes())
            {
                table.Rows.Add(code);
            }

            return table;
        }

        private string SelectedType()
        {
            DataRowView selected = this.cboType.SelectedItem as DataRowView;

            return selected == null ? string.Empty : TableHelper.CellText(selected.Row, this.cboType.ValueMember);
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            this.ExecuteSearchAsync();
        }

        private void OnResetClick(object sender, EventArgs e)
        {
            this.InvalidateChannel(channelList);
            this.InvalidateChannel(channelSuggest);
            this.CancelDebounce(channelSuggest);
            this.txtDurableId.CloseSuggestions();
            this.ResetBusy();
            this.txtDurableId.Text = string.Empty;
            this.cboType.SelectedIndex = -1;
            this.transportData = null;
            this.gridTransports.DataSource = null;
            this.listCard.Text = "Transports";
            this.ClearSelection();
        }

        private async void ExecuteSearchAsync()
        {
            if (!this.searchReady)
            {
                return;
            }

            string keyword = this.txtDurableId.Text.Trim();
            string type = this.SelectedType();

            this.CancelDebounce(channelSuggest);
            this.InvalidateChannel(channelSuggest);
            this.txtDurableId.CloseSuggestions();
            this.InvalidateChannel(channelHistory);

            LoadOutcome<DataTable> outcome;

            using (this.Busy("Loading transports...", keyword.Length == 0 ? type : keyword))
            {
                outcome = await this.FetchAsync(
                        channelList,
                        () => this.GetTransportList(RequestParameters.Of(Schemas.Transport.DurableTyp, type, Schemas.Transport.DurableId, keyword)));
            }

            if (!outcome.IsCurrent)
            {
                return;
            }

            DataTable transports = CopyOf(outcome.Failure != null ? null : outcome.Value);

            if (transports == null)
            {
                this.transportData = null;
                this.gridTransports.DataSource = null;
                this.listCard.Text = "Transports";
                this.ClearSelection();
                return;
            }

            this.transportData = transports;
            this.listCard.Text = "Transports — " + transports.Rows.Count.ToString("N0");

            this.suppressListEvent = true;

            try
            {
                this.gridTransports.DataSource = transports;
                this.gridTransports.SelectRow(item => keyword.Length > 0 && string.Equals(
                        TableHelper.CellText(item as DataRowView, Schemas.Transport.DurableId.Name), keyword, StringComparison.OrdinalIgnoreCase));

                if (this.gridTransports.SelectedItem == null && transports.Rows.Count > 0)
                {
                    this.gridTransports.SelectedIndex = 0;
                }
            }
            finally
            {
                this.suppressListEvent = false;
            }

            this.ApplyListSelection();
        }

        private void OnDurableIdTextChanged(object sender, EventArgs e)
        {
            if (!this.searchReady)
            {
                return;
            }

            this.Debounce(channelSuggest, 300, new MethodInvoker(this.LoadIdCandidatesAsync));
        }

        private async void LoadIdCandidatesAsync()
        {
            string keyword = this.txtDurableId.Text.Trim();
            string type = this.SelectedType();

            if (keyword.Length == 0)
            {
                this.txtDurableId.SuggestionDataSource = null;
                return;
            }

            LoadOutcome<DataTable> outcome = await this.FetchAsync(
                    channelSuggest, () => this.GetTransportCandidates(RequestParameters.Of(Schemas.Transport.DurableTyp, type, Schemas.Criteria.Keyword, keyword)));

            if (!outcome.IsCurrent || outcome.Failure != null || outcome.Value == null)
            {
                return;
            }

            this.txtDurableId.SuggestionDataSource = outcome.Value;
        }

        private void OnListSelectionChanged(object sender, EventArgs e)
        {
            if (!this.searchReady || this.suppressListEvent)
            {
                return;
            }

            this.ApplyListSelection();
        }

        private void ApplyListSelection()
        {
            DataRowView selected = this.gridTransports.SelectedItem as DataRowView;

            if (selected == null || string.IsNullOrWhiteSpace(TableHelper.CellText(selected, Schemas.Transport.DurableId.Name)))
            {
                this.ClearSelection();
                return;
            }

            this.fieldDetail.SetRow(selected.Row);
            this.LoadHistoryAsync(selected.Row);
        }

        private async void LoadHistoryAsync(DataRow transport)
        {
            string durableId = TableHelper.CellText(transport, Schemas.Transport.DurableId.Name);
            IDisposable cover = this.Busy("Loading history...", durableId, this.rightZone);
            LoadOutcome<DataTable> outcome;

            try
            {
                outcome = await this.FetchAsync(
                        channelHistory,
                        () => this.GetTransportHistory(transport));
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
                this.ClearHistory();
                return;
            }

            DataTable history = outcome.Value;
            LotHistoryPresenter.AddDurationColumn(history);
            this.gridHistory.DataSource = history;
            this.gridHistory.StatusText = durableId + LotHistoryPresenter.CycleTimeSuffix(history);
            this.historyCard.Text = "Transport History — " + durableId;

            this.historyOwner = durableId;
        }

        private void OnExportClick(object sender, EventArgs e)
        {
            this.ExportToExcel(
                    this.gridHistory,
                    "Transport History",
                    this.gridHistory.DataSource as DataTable,
                    "TransportHistory" + (string.IsNullOrEmpty(this.historyOwner) ? string.Empty : "_" + this.historyOwner));
        }

        private void ClearSelection()
        {
            this.fieldDetail.ClearValues();
            this.ClearHistory();
        }

        private void ClearHistory()
        {
            this.InvalidateChannel(channelHistory);

            this.gridHistory.DataSource = null;
            this.gridHistory.StatusText = string.Empty;
            this.historyCard.Text = "Transport History";

            this.historyOwner = null;
        }

        private static DataTable CopyOf(DataTable incoming)
        {
            return incoming == null ? null : incoming.Copy();
        }
    }
}
