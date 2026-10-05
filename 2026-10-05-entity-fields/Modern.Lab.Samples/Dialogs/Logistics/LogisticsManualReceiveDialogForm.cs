using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Modern.Lab.Hosting;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Data;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Display;

using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{

    public partial class LogisticsManualReceiveDialogForm : ModernFormBase
    {
        private static readonly string errorColor = Modern.Lab.Theming.SemanticColors.Error;
        private static readonly string warningColor = Modern.Lab.Theming.SemanticColors.Warning;

        private const int badgeSummaryLength = 40;

        private const string channelSuggest = "suggest";

        private string checkedLotId;

        public string ReceivedLotId { get; private set; }

        public string ReceivedMessage { get; private set; }

        private const string slotColumn = "SLOT";

        public LogisticsManualReceiveDialogForm()
        {
            this.InitializeComponent();
            this.ReceivedMessage = string.Empty;

            this.InitializeModernForm(false);

        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            this.txtLotId.AutoCompleteMode = AutoCompleteMode.Suggest;
            this.txtLotId.AutoCompleteSource = AutoCompleteSource.CustomSource;
            this.txtRemark.Text = Services.DefaultDescriptions.ReceiveLot;

            try
            {
                this.cboSendFac.DataSource = GetSendFacilities();
            }
            catch (Exception)
            {
                this.cboSendFac.DataSource = new string[0];
            }

            this.cboSendFac.Focus();
        }

        private void OnCheckClick(object sender, EventArgs e)
        {
            string lotId = this.txtLotId.Text.Trim();

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
                lotInfo = GetEhubLots(lotId);
                wafers = GetEhubWafers(lotId);
            }
            catch (Exception ex)
            {
                this.ShowFailure("Check failed", this.ServerFailureText(ex));
                return;
            }

            if (lotInfo.Rows.Count == 0)
            {
                this.ClearLotInfo();
                this.gridWafers.DataSource = null;
                this.checkedLotId = null;
                this.btnReceive.Enabled = false;
                this.ShowResult("No lot found for " + lotId + ".", warningColor);
                return;
            }

            this.FillLotInfo(lotInfo.Rows[0]);
            ApplySlotColumn(wafers);
            WaferColumns(wafers, slotColumn).Bind(this.gridWafers);
            this.checkedLotId = lotId;
            this.btnReceive.Enabled = true;
            this.HideResult();
        }

        private void OnReceiveClick(object sender, EventArgs e)
        {
            string lotId = this.txtLotId.Text.Trim();

            if (this.checkedLotId == null || this.checkedLotId != lotId)
            {
                this.ShowResult("Check the Lot Id first.", warningColor);
                return;
            }

            string sendFac = this.cboSendFac.SelectedValue as string;

            DataActionResult result;

            try
            {
                result = ManualReceive(lotId, sendFac, this.txtRemark.Text.Trim());
            }
            catch (Exception ex)
            {
                this.ShowFailure("Receive failed", this.ServerFailureText(ex));
                return;
            }

            if (!result.Success)
            {
                this.ShowFailure("Receive failed", result.Message);
                return;
            }

            this.ReceivedLotId = lotId.ToUpperInvariant();
            this.ReceivedMessage = result.Message ?? string.Empty;
            this.DialogResult = DialogResult.OK;
        }

        private void OnInputChanged(object sender, EventArgs e)
        {
            this.checkedLotId = null;
            this.btnReceive.Enabled = false;
            this.HideResult();

            this.Debounce(channelSuggest, 300, new MethodInvoker(this.LoadIdCandidatesAsync));
        }

        private async void LoadIdCandidatesAsync()
        {
            string keyword = this.txtLotId.Text.Trim();

            if (keyword.Length == 0)
            {
                this.txtLotId.AutoCompleteCustomSource = null;
                return;
            }

            LoadOutcome<string[]> outcome = await this.FetchAsync(
                    channelSuggest, () => GetEhubLotIdCandidates(keyword));

            if (!outcome.Succeeded)
            {
                return;
            }

            AutoCompleteStringCollection collection = new AutoCompleteStringCollection();
            collection.AddRange(outcome.Value);
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
                if (this.btnReceive.Enabled)
                {
                    this.OnReceiveClick(this.btnReceive, EventArgs.Empty);
                }
                else
                {
                    this.OnCheckClick(this.txtLotId, EventArgs.Empty);
                }

                return true;
            }

            return base.ProcessDialogKey(keyData);
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

        private static void ApplySlotColumn(DataTable wafers)
        {
            if (wafers == null)
            {
                return;
            }

            if (!wafers.Columns.Contains(slotColumn))
            {
                wafers.Columns.Add(slotColumn, typeof(string));
            }

            foreach (DataRow row in wafers.Rows)
            {
                if (TableHelper.CellText(row, slotColumn).Trim().Length > 0)
                {
                    continue;
                }

                string waferId = TableHelper.CellText(row, Schemas.Wafer.WfId.Name);
                int dot = waferId.LastIndexOf('.');
                string suffix = dot >= 0 && dot < waferId.Length - 1
                        ? waferId.Substring(dot + 1)
                        : string.Empty;
                int slot = TableHelper.ParseInt(suffix);

                row[slotColumn] = slot > 0 ? slot.ToString("N0") : string.Empty;
            }
        }

        private void FillLotInfo(DataRow row)
        {
            LotInfoDefinitions(row.Table).Apply(this.fieldLotInfo);
            this.fieldLotInfo.SetRow(row);
        }

        private void ClearLotInfo()
        {
            this.fieldLotInfo.ClearValues();
        }

    }
}
