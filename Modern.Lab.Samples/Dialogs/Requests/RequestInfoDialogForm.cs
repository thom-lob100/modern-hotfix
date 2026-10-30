using System;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Data;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Display;

namespace Modern.Lab.Samples
{

    public partial class RequestInfoDialogForm : ModernFormBase
    {
        public RequestInfoDialogForm()
        {
            this.InitializeComponent();

            this.InitializeModernForm();
            this.ApplyHeaderColors();

            this.gridSamples.AllowColumnFilters = false;
            this.gridSamples.RowKeyMember = Schemas.Sample.RowKey;
            this.lblRequestSerialNoCaption.Text = FieldCaption(ReqSerialNoColumn);
            this.lblRequestRemarkCaption.ForeColor = Modern.Lab.Theming.ModernTheme.TextSecondary;
            this.lblRequestRemark.Font = Modern.Lab.WinForms.Rendering.ModernFonts.BodyStrong;
        }

        public void SetRequest(string requestSerialNo, DataTable master, object samples, string purpose)
        {
            this.lblRequestSerialNoCaption.Text = FieldCaption(ReqSerialNoColumn);
            this.ApplyRequest(requestSerialNo, master, samples, purpose);
        }

        public void SetRequest(DataRow header, object samples)
        {
            this.SetRequest(header, samples, null);
        }

        public void SetRequest(DataRow header, object samples, string purposeMember)
        {
            DataTable overview = header == null ? null : OverviewOf(header);
            string purpose = ExtractMember(overview, purposeMember);
            this.lblRequestSerialNoCaption.Text = FieldCaption(ReqSerialNoColumn);
            this.ApplyRequest(TableHelper.CellText(header, ReqSerialNoColumn), overview, samples, purpose);
        }

        public void SetRequest(DataTable joined, string firstSampleMember, string purposeMember)
        {
            DataTable overview = RequestInfoTables.Overview(joined, firstSampleMember);
            DataRow header = overview == null || overview.Rows.Count == 0 ? null : overview.Rows[0];

            this.SetRequest(header, RequestInfoTables.Samples(joined, firstSampleMember), purposeMember);
        }

        private void ApplyRequest(string requestSerialNo, DataTable overview, object samples, string purpose)
        {
            this.lblRequestSerialNo.Text = ValueOrDash(requestSerialNo);
            this.BindRequestFields(overview, purpose);
            this.BindSamples(samples);
        }

        private void BindSamples(object samples)
        {
            DataTable table = samples as DataTable;

            if (table == null)
            {
                this.gridSamples.DataSource = samples;
                return;
            }

            SampleColumns(table).Bind(this.gridSamples);
        }

        private void BindRequestFields(DataTable overview, string purpose)
        {
            ModernFieldDefinition[] fields = RequestFieldDefinitions(overview);

            this.fieldRequest.DefineFields(fields);
            DataRow row = overview == null || overview.Rows.Count == 0 ? null : overview.Rows[0];
            this.fieldRequest.SetRow(row);
            int fieldRows = (fields.Length + this.fieldRequest.Columns - 1) / this.fieldRequest.Columns;
            int fieldHeight = Math.Max(1, fieldRows) * 32 * Modern.Lab.WinForms.Controls.Hosting.ScreenDpi.Of(this) / 96;
            int remarkHeight = row == null ? 0 : 38 * Modern.Lab.WinForms.Controls.Hosting.ScreenDpi.Of(this) / 96;
            this.tableRequestMaster.RowStyles[0].Height = fieldHeight;
            this.tableRequestMaster.RowStyles[1].Height = remarkHeight;
            this.tableRequestMaster.Height = fieldHeight + remarkHeight;
            int headerHeight = Math.Max(this.splitRequest.Panel1MinSize, fieldHeight + remarkHeight);

            if (this.splitRequest.Height - this.splitRequest.SplitterWidth - this.splitRequest.Panel2MinSize >= headerHeight)
            {
                this.splitRequest.SplitterDistance = headerHeight;
            }

            this.lblRequestRemarkCaption.Text = "Purpose";
            this.ApplyRemarkLine(row == null ? string.Empty : purpose);
            this.panelRequestRemark.Visible = row != null;
            this.tableRequestMaster.Visible = row != null;
            this.lblRequestEmpty.Visible = row == null;
        }

        private void OnRequestRemarkMoreClick(object sender, EventArgs e)
        {
            string remarks = this.btnRequestRemarkMore.Tag as string;

            if (!string.IsNullOrEmpty(remarks))
            {
                Modern.Lab.WinForms.Controls.Dialogs.ModernMessageDialog.ShowInformation(this, "Purpose", remarks);
            }
        }

        private void ApplyRemarkLine(string remarks)
        {
            string full = (remarks ?? string.Empty).Trim();
            this.lblRequestRemark.Text = full.Length == 0 ? "-" : full.Replace("\r\n", " ").Replace('\n', ' ');
            this.btnRequestRemarkMore.Tag = full;
            this.btnRequestRemarkMore.Enabled = full.Length > 0;
            this.btnRequestRemarkMore.Location = new System.Drawing.Point(
                    this.panelRequestRemark.ClientSize.Width - this.btnRequestRemarkMore.Width, this.lblRequestRemark.Top);
            this.lblRequestRemark.Dock = System.Windows.Forms.DockStyle.None;
            this.lblRequestRemark.Padding = System.Windows.Forms.Padding.Empty;
            this.lblRequestRemark.SetBounds(
                    0, this.lblRequestRemarkCaption.Bottom,
                    System.Math.Max(0, this.btnRequestRemarkMore.Left - 4), this.btnRequestRemarkMore.Height);
            this.lblRequestRemark.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.btnRequestRemarkMore.BringToFront();
        }
        private static string ExtractMember(DataTable overview, string member)
        {
            string resolvedMember = ResolveRemarkMember(overview, member);

            if (resolvedMember == null)
            {
                return string.Empty;
            }

            string value = overview.Rows.Count == 0 ? string.Empty : TableHelper.CellText(overview.Rows[0], resolvedMember);
            overview.Columns.Remove(resolvedMember);
            return value;
        }

        private static string ResolveRemarkMember(DataTable overview, string preferred)
        {
            if (overview == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(preferred) && overview.Columns.Contains(preferred))
            {
                return preferred;
            }

            if (overview.Columns.Contains("REMARK"))
            {
                return "REMARK";
            }

            return overview.Columns.Contains(Schemas.Request.Purpose.Name) ? Schemas.Request.Purpose.Name : null;
        }

        private static DataTable OverviewOf(DataRow header)
        {
            DataTable overview = header.Table.Clone();
            overview.ImportRow(header);
            overview.AcceptChanges();
            return overview;
        }

        private static string FieldCaption(string member)
        {
            ModernDataGridColumn column = new ModernDataGridColumn(member);
            return Modern.Lab.Controls.Wpf.Data.GridCaptionCatalog.PlainCaption(column.TopLabel, column.HeaderText, column.BottomLabel);
        }

        private void ApplyHeaderColors()
        {
            this.panelAccentBar.BackColor = Modern.Lab.Theming.ModernTheme.Accent;
            this.lblRequestSerialNo.ForeColor = Modern.Lab.Theming.ModernTheme.Accent;
            this.lblRequestSerialNoCaption.ForeColor = Modern.Lab.Theming.ModernTheme.TextSecondary;
        }

        private static string ValueOrDash(string value)
        {
            return string.IsNullOrEmpty(value) ? "-" : value;
        }

        private void OnCloseClick(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
