using System;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Data;
using Modern.Lab.Hosting;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Display;

using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{

    public partial class LotCreateWaferDialogForm : ModernFormBase
    {
        private static readonly string errorColor = Modern.Lab.Theming.SemanticColors.Error;
        private static readonly string warningColor = Modern.Lab.Theming.SemanticColors.Warning;

        private const int badgeSummaryLength = 40;

        private const string channelSuggest = "suggest";

        private string checkedLotId;

        public string CreatedLotId { get; private set; }

        public string CreatedMessage { get; private set; }

        private const string slotColumn = "SLOT";

        public LotCreateWaferDialogForm()
        {
            this.InitializeComponent();
            this.CreatedMessage = string.Empty;

            this.InitializeModernForm(false);

        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            if (IsDesignTime() || this.DesignMode)
            {
                return;
            }

            this.txtLotId.AutoCompleteMode = AutoCompleteMode.Suggest;
            this.txtLotId.AutoCompleteSource = AutoCompleteSource.CustomSource;
            this.txtRemark.Text = Services.DefaultDescriptions.CreateLot;

            try
            {
                this.cboSendFac.DisplayMember = Schemas.Logistics.FacId.Name;
                this.cboSendFac.ValueMember = Schemas.Logistics.FacId.Name;
                this.cboSendFac.DataSource = this.GetSendFacilities();
            }
            catch (Exception)
            {
                this.cboSendFac.DataSource = null;
            }

            this.cboSendFac.Focus();
        }

        private void OnCheckClick(object sender, EventArgs e)
        {
            string lotId = this.txtLotId.Text.Trim();
            string sendFac = this.SelectedSendFac();

            if (lotId.Length == 0)
            {
                this.ShowResult("Enter a Lot Id.", warningColor);
                this.txtLotId.Focus();
                return;
            }

            DataTable lotInfo;
            DataTable wafers;

            try
            {
                lotInfo = this.GetIncomingLotInfo(lotId, sendFac);
                wafers = this.GetIncomingWafers(lotId, sendFac);
            }
            catch (Exception ex)
            {
                this.ShowFailure("Check failed", this.ServerFailureText(ex));
                return;
            }

            if (lotInfo.Rows.Count == 0)
            {
                this.fieldLotInfo.ClearValues();
                this.gridWafers.DataSource = null;
                this.checkedLotId = null;
                this.btnCreate.Enabled = false;
                this.ShowResult("No incoming lot found for " + lotId + ".", warningColor);
                return;
            }

            LotInfoDefinitions(lotInfo).Apply(this.fieldLotInfo);
            this.fieldLotInfo.SetRow(lotInfo.Rows[0]);
            WaferColumns(wafers, slotColumn).Bind(this.gridWafers);
            this.checkedLotId = lotId;
            this.btnCreate.Enabled = true;
            this.HideResult();
        }

        private void OnCreateClick(object sender, EventArgs e)
        {
            string lotId = this.txtLotId.Text.Trim();

            if (this.checkedLotId == null || this.checkedLotId != lotId)
            {
                this.ShowResult("Check the Lot Id first.", warningColor);
                return;
            }

            DataActionResult result;

            try
            {
                result = this.CreateWaferLot(lotId, this.SelectedSendFac(), this.txtRemark.Text.Trim());
            }
            catch (Exception ex)
            {
                this.ShowFailure("Create failed", this.ServerFailureText(ex));
                return;
            }

            if (!result.Success)
            {
                this.ShowFailure("Create failed", result.Message);
                return;
            }

            this.CreatedLotId = lotId.ToUpperInvariant();
            this.CreatedMessage = result.Message ?? string.Empty;
            this.DialogResult = DialogResult.OK;
        }

        private void OnInputChanged(object sender, EventArgs e)
        {
            if (IsDesignTime() || this.DesignMode)
            {
                return;
            }

            this.checkedLotId = null;
            this.btnCreate.Enabled = false;
            this.HideResult();

            this.Debounce(channelSuggest, 300, new MethodInvoker(this.LoadIdCandidatesAsync));
        }

        private async void LoadIdCandidatesAsync()
        {
            if (IsDesignTime() || this.DesignMode)
            {
                return;
            }

            string keyword = this.txtLotId.Text.Trim();

            if (keyword.Length == 0)
            {
                this.txtLotId.AutoCompleteCustomSource = null;
                return;
            }

            string sendFac = this.SelectedSendFac();
            LoadOutcome<DataTable> outcome = await this.FetchAsync(
                    channelSuggest, () => this.GetIncomingLotIdCandidates(keyword, sendFac));

            if (!outcome.Succeeded || outcome.Value == null)
            {
                return;
            }

            AutoCompleteStringCollection collection = new AutoCompleteStringCollection();

            foreach (DataRow row in outcome.Value.Rows)
            {
                collection.Add(TableHelper.CellText(row, Schemas.Lot.LotId.Name).Trim());
            }

            this.txtLotId.AutoCompleteCustomSource = collection;
        }

        private void OnCancelClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Enter && this.txtRemark.ContainsFocus)
            {
                return base.ProcessDialogKey(keyData);
            }

            if (keyData == Keys.Enter)
            {
                if (this.btnCreate.Enabled)
                {
                    this.OnCreateClick(this.btnCreate, EventArgs.Empty);
                }
                else
                {
                    this.OnCheckClick(this.txtLotId, EventArgs.Empty);
                }

                return true;
            }

            return base.ProcessDialogKey(keyData);
        }

        private string SelectedSendFac()
        {
            return (Convert.ToString(this.cboSendFac.SelectedValue) ?? string.Empty).Trim();
        }

        private void ShowResult(string message, string color)
        {
            this.badgeResult.Text = message;
            this.badgeResult.Color = color;
            this.badgeResult.Visible = true;
        }

        private void ShowFailure(string caption, string message)
        {
            string reason = (message ?? string.Empty).Trim();

            if (reason.Length == 0)
            {
                reason = "The server rejected the request.";
            }

            this.ShowErrorMessage(caption, reason);
            this.ShowResult(Summarize(reason), errorColor);
        }

        private static string Summarize(string reason)
        {
            string firstLine = reason.Replace("\r\n", "\n").Split('\n')[0].Trim();

            if (firstLine.Length <= badgeSummaryLength)
            {
                return firstLine;
            }

            return firstLine.Substring(0, badgeSummaryLength) + "…";
        }

        private void HideResult()
        {
            this.badgeResult.Visible = false;
        }

    }
}
