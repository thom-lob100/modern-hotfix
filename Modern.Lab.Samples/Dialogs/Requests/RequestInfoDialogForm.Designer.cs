namespace Modern.Lab.Samples
{
    partial class RequestInfoDialogForm
    {

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableRoot = new System.Windows.Forms.TableLayoutPanel();
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblRequestSerialNo = new System.Windows.Forms.Label();
            this.lblRequestSerialNoCaption = new System.Windows.Forms.Label();
            this.panelAccentBar = new System.Windows.Forms.Panel();
            this.splitRequest = new Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer();
            this.tableRequestMaster = new System.Windows.Forms.TableLayoutPanel();
            this.fieldRequest = new Modern.Lab.WinForms.Controls.Display.ModernFieldList();
            this.panelRequestRemark = new System.Windows.Forms.Panel();
            this.lblRequestRemarkCaption = new System.Windows.Forms.Label();
            this.lblRequestRemark = new System.Windows.Forms.Label();
            this.btnRequestRemarkMore = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.lblRequestEmpty = new System.Windows.Forms.Label();
            this.gridSamples = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.btnClose = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.tableRoot.SuspendLayout();
            this.panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitRequest)).BeginInit();
            this.splitRequest.Panel1.SuspendLayout();
            this.splitRequest.Panel2.SuspendLayout();
            this.splitRequest.SuspendLayout();
            this.tableRequestMaster.SuspendLayout();
            this.panelRequestRemark.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.SuspendLayout();
            //
            // tableRoot
            //
            this.tableRoot.ColumnCount = 1;
            this.tableRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableRoot.Controls.Add(this.panelHeader, 0, 0);
            this.tableRoot.Controls.Add(this.splitRequest, 0, 1);
            this.tableRoot.Controls.Add(this.panelButtons, 0, 2);
            this.tableRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableRoot.Location = new System.Drawing.Point(12, 12);
            this.tableRoot.Name = "tableRoot";
            this.tableRoot.RowCount = 3;
            this.tableRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 64F));
            this.tableRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.tableRoot.Size = new System.Drawing.Size(616, 560);
            this.tableRoot.TabIndex = 0;
            //
            // panelHeader
            //
            this.panelHeader.Controls.Add(this.lblRequestSerialNo);
            this.panelHeader.Controls.Add(this.lblRequestSerialNoCaption);
            this.panelHeader.Controls.Add(this.panelAccentBar);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Margin = new System.Windows.Forms.Padding(0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(616, 64);
            this.panelHeader.TabIndex = 0;
            //
            // lblRequestSerialNo
            //
            this.lblRequestSerialNo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right))));
            this.lblRequestSerialNo.AutoEllipsis = true;
            this.lblRequestSerialNo.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblRequestSerialNo.Location = new System.Drawing.Point(12, 26);
            this.lblRequestSerialNo.Name = "lblRequestSerialNo";
            this.lblRequestSerialNo.Size = new System.Drawing.Size(592, 32);
            this.lblRequestSerialNo.TabIndex = 1;
            this.lblRequestSerialNo.Text = "-";
            //
            // lblRequestSerialNoCaption
            //
            this.lblRequestSerialNoCaption.AutoSize = true;
            this.lblRequestSerialNoCaption.Location = new System.Drawing.Point(13, 8);
            this.lblRequestSerialNoCaption.Name = "lblRequestSerialNoCaption";
            this.lblRequestSerialNoCaption.TabIndex = 0;
            this.lblRequestSerialNoCaption.Text = "Req Serial No";
            //
            // panelAccentBar
            //
            this.panelAccentBar.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelAccentBar.Location = new System.Drawing.Point(0, 0);
            this.panelAccentBar.Name = "panelAccentBar";
            this.panelAccentBar.Size = new System.Drawing.Size(3, 64);
            this.panelAccentBar.TabIndex = 2;
            //
            // splitRequest
            //
            this.splitRequest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitRequest.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitRequest.Location = new System.Drawing.Point(0, 72);
            this.splitRequest.Margin = new System.Windows.Forms.Padding(0, 8, 0, 8);
            this.splitRequest.Name = "splitRequest";
            this.splitRequest.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // splitRequest.Panel1
            //
            this.splitRequest.Panel1.AutoScroll = true;
            this.splitRequest.Panel1.Controls.Add(this.tableRequestMaster);
            this.splitRequest.Panel1.Controls.Add(this.lblRequestEmpty);
            //
            // splitRequest
            //
            this.splitRequest.Panel1MinSize = 120;
            //
            // splitRequest.Panel2
            //
            this.splitRequest.Panel2.Controls.Add(this.gridSamples);
            this.splitRequest.Panel2.Padding = new System.Windows.Forms.Padding(0);
            //
            // splitRequest
            //
            this.splitRequest.Panel2MinSize = 80;
            this.splitRequest.Size = new System.Drawing.Size(616, 432);
            this.splitRequest.SplitterDistance = 200;
            this.splitRequest.SplitterWidth = 8;
            this.splitRequest.TabIndex = 1;
            this.splitRequest.TabStop = false;
            //
            // tableRequestMaster
            //
            this.tableRequestMaster.ColumnCount = 1;
            this.tableRequestMaster.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableRequestMaster.Controls.Add(this.fieldRequest, 0, 0);
            this.tableRequestMaster.Controls.Add(this.panelRequestRemark, 0, 1);
            this.tableRequestMaster.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableRequestMaster.Location = new System.Drawing.Point(0, 0);
            this.tableRequestMaster.Margin = new System.Windows.Forms.Padding(0);
            this.tableRequestMaster.Name = "tableRequestMaster";
            this.tableRequestMaster.RowCount = 2;
            this.tableRequestMaster.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableRequestMaster.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 72F));
            this.tableRequestMaster.Size = new System.Drawing.Size(616, 192);
            this.tableRequestMaster.TabIndex = 0;
            this.tableRequestMaster.Visible = false;
            //
            // fieldRequest
            //
            this.fieldRequest.Columns = 3;
            this.fieldRequest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fieldRequest.Location = new System.Drawing.Point(0, 0);
            this.fieldRequest.Margin = new System.Windows.Forms.Padding(0);
            this.fieldRequest.Name = "fieldRequest";
            this.fieldRequest.Size = new System.Drawing.Size(616, 120);
            this.fieldRequest.TabIndex = 0;
            //
            // panelRequestRemark
            //
            this.panelRequestRemark.Controls.Add(this.btnRequestRemarkMore);
            this.panelRequestRemark.Controls.Add(this.lblRequestRemark);
            this.panelRequestRemark.Controls.Add(this.lblRequestRemarkCaption);
            this.panelRequestRemark.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRequestRemark.Location = new System.Drawing.Point(0, 120);
            this.panelRequestRemark.Margin = new System.Windows.Forms.Padding(0);
            this.panelRequestRemark.Name = "panelRequestRemark";
            this.panelRequestRemark.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.panelRequestRemark.Size = new System.Drawing.Size(616, 72);
            this.panelRequestRemark.TabIndex = 1;
            //
            // lblRequestRemarkCaption
            //
            this.lblRequestRemarkCaption.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblRequestRemarkCaption.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.lblRequestRemarkCaption.Location = new System.Drawing.Point(10, 0);
            this.lblRequestRemarkCaption.Name = "lblRequestRemarkCaption";
            this.lblRequestRemarkCaption.Size = new System.Drawing.Size(596, 14);
            this.lblRequestRemarkCaption.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.lblRequestRemarkCaption.TabIndex = 0;
            this.lblRequestRemarkCaption.Text = "Purpose";
            //
            // lblRequestRemark
            //
            this.lblRequestRemark.AutoEllipsis = true;
            this.lblRequestRemark.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblRequestRemark.Location = new System.Drawing.Point(0, 14);
            this.lblRequestRemark.Name = "lblRequestRemark";
            this.lblRequestRemark.Size = new System.Drawing.Size(596, 20);
            this.lblRequestRemark.Padding = new System.Windows.Forms.Padding(0, 0, 28, 0);
            this.lblRequestRemark.TabIndex = 1;
            this.lblRequestRemark.Text = "-";
            this.lblRequestRemark.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblRequestRemark.UseMnemonic = false;
            //
            // btnRequestRemarkMore
            //
            this.btnRequestRemarkMore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRequestRemarkMore.BackColor = System.Drawing.Color.Transparent;
            this.btnRequestRemarkMore.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRequestRemarkMore.IconGlyph = "\uE8A7";
            this.btnRequestRemarkMore.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Subtle;
            this.btnRequestRemarkMore.Location = new System.Drawing.Point(592, 14);
            this.btnRequestRemarkMore.Name = "btnRequestRemarkMore";
            this.btnRequestRemarkMore.Size = new System.Drawing.Size(24, 20);
            this.btnRequestRemarkMore.TabIndex = 2;
            this.btnRequestRemarkMore.Text = "";
            this.btnRequestRemarkMore.Click += new System.EventHandler(this.OnRequestRemarkMoreClick);
            //
            // lblRequestEmpty
            //
            this.lblRequestEmpty.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRequestEmpty.Name = "lblRequestEmpty";
            this.lblRequestEmpty.Text = "No request";
            this.lblRequestEmpty.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // gridSamples
            //
            this.gridSamples.AllowFindPanel = false;
            this.gridSamples.AutoFitColumns = true;
            this.gridSamples.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridSamples.EmptyText = "No samples on this request";
            this.gridSamples.Location = new System.Drawing.Point(0, 0);
            this.gridSamples.Name = "gridSamples";
            this.gridSamples.ShowExportMenu = false;
            this.gridSamples.ShowStatusBar = true;
            this.gridSamples.Size = new System.Drawing.Size(616, 224);
            this.gridSamples.StatusCountFormat = "{0:N0} samples";
            this.gridSamples.TabIndex = 1;
            this.gridSamples.Child = null;
            //
            // panelButtons
            //
            this.panelButtons.Controls.Add(this.btnClose);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelButtons.Location = new System.Drawing.Point(0, 512);
            this.panelButtons.Margin = new System.Windows.Forms.Padding(0);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(616, 48);
            this.panelButtons.TabIndex = 2;
            //
            // btnClose
            //
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(520, 8);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(96, 36);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.OnCloseClick);
            //
            // RequestInfoDialogForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AcceptButton = this.btnClose;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(640, 584);
            this.Controls.Add(this.tableRoot);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(520, 520);
            this.Name = "RequestInfoDialogForm";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Request Info";
            this.tableRoot.ResumeLayout(false);
            this.tableRoot.PerformLayout();
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.panelRequestRemark.ResumeLayout(false);
            this.tableRequestMaster.ResumeLayout(false);
            this.splitRequest.Panel1.ResumeLayout(false);
            this.splitRequest.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitRequest)).EndInit();
            this.splitRequest.ResumeLayout(false);
            this.panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableRoot;
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblRequestSerialNo;
        private System.Windows.Forms.Label lblRequestSerialNoCaption;
        private System.Windows.Forms.Panel panelAccentBar;
        private Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer splitRequest;
        private System.Windows.Forms.TableLayoutPanel tableRequestMaster;
        private Modern.Lab.WinForms.Controls.Display.ModernFieldList fieldRequest;
        private System.Windows.Forms.Panel panelRequestRemark;
        private System.Windows.Forms.Label lblRequestRemarkCaption;
        private System.Windows.Forms.Label lblRequestRemark;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnRequestRemarkMore;
        private System.Windows.Forms.Label lblRequestEmpty;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridSamples;
        private System.Windows.Forms.Panel panelButtons;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnClose;
    }
}
