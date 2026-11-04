using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Data;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Display;

namespace Modern.Lab.Samples
{
    public partial class MonitorDetailDialogForm : ModernFormBase
    {
        private const string LookupHint = "Try again, or contact support.";

        public MonitorDetailDialogForm()
        {
            this.InitializeComponent();
            this.InitializeModernForm(false);
            this.gridWafers.RowKeyMember = Schemas.Wafer.RowKey;
            this.gridHistory.RowKeyMember = Schemas.LotHistory.RowKey;
            this.RegisterFindShortcut(this.gridHistory, this.gridWafers);
        }

        public MonitorDetailDialogForm(string reqSerialNo, string lotId) : this()
        {
            this.ReqSerialNo = (reqSerialNo ?? string.Empty).Trim();
            this.LotId = (lotId ?? string.Empty).Trim();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ReqSerialNo { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string LotId { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Loaded { get; private set; }

        internal static void ShowFor(IWin32Window owner, string reqSerialNo, string lotId)
        {
            string request = (reqSerialNo ?? string.Empty).Trim();
            string lot = (lotId ?? string.Empty).Trim();

            if (request.Length == 0 && lot.Length == 0)
            {
                return;
            }

            using (MonitorDetailDialogForm dialog = new MonitorDetailDialogForm(request, lot))
            {
                dialog.ShowDialog(owner);
            }
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            if (IsDesignTime() || this.DesignMode)
            {
                return;
            }

            bool hasLot = !string.IsNullOrEmpty(this.LotId);
            this.Text = hasLot ? "Lot Detail - " + this.LotId : "Request Detail - " + this.ReqSerialNo;
            this.lotZone.Visible = hasLot;

            if (!hasLot)
            {
                this.ClientSize = new System.Drawing.Size(this.ClientSize.Width, this.requestCard.Height + this.buttonPanel.Height + this.Padding.Vertical);
            }

            if (!this.LoadDetail())
            {
                this.CloseLater();
            }
        }

        internal bool LoadDetail()
        {
            this.Loaded = false;

            if (!string.IsNullOrEmpty(this.ReqSerialNo) && !this.LoadRequest())
            {
                return false;
            }

            if (string.IsNullOrEmpty(this.ReqSerialNo))
            {
                this.requestCard.Text = "Request - none";
                this.fieldRequest.DefineFields();
            }

            if (!string.IsNullOrEmpty(this.LotId) && !this.LoadLot())
            {
                return false;
            }

            this.Loaded = true;
            return true;
        }

        private bool LoadRequest()
        {
            DataTable joined;

            try
            {
                joined = this.LookupRequestInfo(this.ReqSerialNo);
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage(
                        "Request lookup failed",
                        "The request information could not be loaded. " + LookupHint,
                        this.ServerFailureText(ex));
                return false;
            }

            DataTable overview = RequestInfoTables.Overview(joined, RequestSampleFirstColumn);
            this.requestCard.Text = "Request - " + this.ReqSerialNo;
            this.fieldRequest.DefineFields(RequestFieldDefinitions(overview));

            if (overview != null && overview.Rows.Count > 0)
            {
                this.fieldRequest.SetRow(overview.Rows[0]);
            }

            return true;
        }

        private bool LoadLot()
        {
            DataTable history;
            DataTable wafers;

            try
            {
                history = this.GetLotHistory(this.LotId);
                wafers = this.GetWafers(this.LotId);
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage(
                        "Lot lookup failed",
                        "The lot history could not be loaded. " + LookupHint,
                        this.ServerFailureText(ex));
                return false;
            }

            LotHistoryPresenter.AddDurationColumn(history);
            this.gridHistory.DataSource = history;
            this.gridHistory.StatusText = this.LotId + LotHistoryPresenter.CycleTimeSuffix(history);
            this.historyCard.Text = "Lot History - " + this.LotId;
            this.gridWafers.DataSource = wafers;
            this.waferCard.Text = "Wafers - " + this.LotId;
            return true;
        }

        private void CloseLater()
        {
            if (!this.IsHandleCreated)
            {
                return;
            }

            this.BeginInvoke(new MethodInvoker(delegate
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }));
        }

        private void OnCloseClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
