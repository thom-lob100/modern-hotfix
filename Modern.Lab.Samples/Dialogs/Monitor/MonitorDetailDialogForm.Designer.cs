namespace Modern.Lab.Samples
{
    public partial class MonitorDetailDialogForm
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
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
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
            this.requestCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.fieldRequest = new Modern.Lab.WinForms.Controls.Display.ModernFieldList();
            this.spRequest = new System.Windows.Forms.Panel();
            this.lotZone = new System.Windows.Forms.Panel();
            this.historyCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.gridHistory = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.spWafer = new System.Windows.Forms.Panel();
            this.waferCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.gridWafers = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.btnClose = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.requestCard.SuspendLayout();
            this.lotZone.SuspendLayout();
            this.historyCard.SuspendLayout();
            this.waferCard.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // requestCard
            //
            this.requestCard.Controls.Add(this.fieldRequest);
            this.requestCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.requestCard.Location = new System.Drawing.Point(16, 16);
            this.requestCard.Name = "requestCard";
            this.requestCard.Padding = new System.Windows.Forms.Padding(12, 40, 12, 8);
            this.requestCard.Size = new System.Drawing.Size(948, 150);
            this.requestCard.TabIndex = 0;
            this.requestCard.Text = "Request";
            this.requestCard.TitleBar = true;
            //
            // fieldRequest
            //
            this.fieldRequest.Columns = 4;
            this.fieldRequest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fieldRequest.Location = new System.Drawing.Point(12, 40);
            this.fieldRequest.Name = "fieldRequest";
            this.fieldRequest.Size = new System.Drawing.Size(924, 102);
            this.fieldRequest.TabIndex = 0;
            //
            // spRequest
            //
            this.spRequest.Dock = System.Windows.Forms.DockStyle.Top;
            this.spRequest.Location = new System.Drawing.Point(16, 166);
            this.spRequest.Name = "spRequest";
            this.spRequest.Size = new System.Drawing.Size(948, 8);
            this.spRequest.TabIndex = 1;
            //
            // lotZone
            //
            this.lotZone.Controls.Add(this.historyCard);
            this.lotZone.Controls.Add(this.spWafer);
            this.lotZone.Controls.Add(this.waferCard);
            this.lotZone.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lotZone.Location = new System.Drawing.Point(16, 174);
            this.lotZone.Name = "lotZone";
            this.lotZone.Size = new System.Drawing.Size(948, 482);
            this.lotZone.TabIndex = 2;
            //
            // historyCard
            //
            this.historyCard.Controls.Add(this.gridHistory);
            this.historyCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.historyCard.Location = new System.Drawing.Point(328, 0);
            this.historyCard.Name = "historyCard";
            this.historyCard.Padding = new System.Windows.Forms.Padding(8, 40, 8, 8);
            this.historyCard.Size = new System.Drawing.Size(620, 482);
            this.historyCard.TabIndex = 2;
            this.historyCard.Text = "Lot History";
            this.historyCard.TitleBar = true;
            //
            // gridHistory
            //
            this.gridHistory.AllowColumnFilters = true;
            this.gridHistory.AutoFitColumns = true;
            this.gridHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridHistory.EmptyText = "No history";
            this.gridHistory.ExportFileNamePrefix = "LotHistory";
            this.gridHistory.ExportSheetName = "Lot History";
            this.gridHistory.Location = new System.Drawing.Point(8, 40);
            this.gridHistory.Name = "gridHistory";
            this.gridHistory.ShowStatusBar = true;
            this.gridHistory.Size = new System.Drawing.Size(604, 434);
            this.gridHistory.StatusCountFormat = "{0:N0} events";
            this.gridHistory.TabIndex = 0;
            this.gridHistory.Child = null;
            //
            // spWafer
            //
            this.spWafer.Dock = System.Windows.Forms.DockStyle.Left;
            this.spWafer.Location = new System.Drawing.Point(320, 0);
            this.spWafer.Name = "spWafer";
            this.spWafer.Size = new System.Drawing.Size(8, 482);
            this.spWafer.TabIndex = 1;
            //
            // waferCard
            //
            this.waferCard.Controls.Add(this.gridWafers);
            this.waferCard.Dock = System.Windows.Forms.DockStyle.Left;
            this.waferCard.Location = new System.Drawing.Point(0, 0);
            this.waferCard.Name = "waferCard";
            this.waferCard.Padding = new System.Windows.Forms.Padding(8, 40, 8, 8);
            this.waferCard.Size = new System.Drawing.Size(320, 482);
            this.waferCard.TabIndex = 0;
            this.waferCard.Text = "Wafers";
            this.waferCard.TitleBar = true;
            //
            // gridWafers
            //
            this.gridWafers.AllowColumnFilters = false;
            this.gridWafers.AutoFitColumns = true;
            this.gridWafers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridWafers.EmptyText = "No wafers";
            this.gridWafers.Location = new System.Drawing.Point(8, 40);
            this.gridWafers.Name = "gridWafers";
            this.gridWafers.ShowStatusBar = true;
            this.gridWafers.Size = new System.Drawing.Size(304, 434);
            this.gridWafers.StatusCountFormat = "{0:N0} wafers";
            this.gridWafers.TabIndex = 0;
            this.gridWafers.Child = null;
            //
            // buttonPanel
            //
            this.buttonPanel.Controls.Add(this.btnClose);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonPanel.Location = new System.Drawing.Point(16, 656);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(948, 48);
            this.buttonPanel.TabIndex = 3;
            //
            // btnClose
            //
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Subtle;
            this.btnClose.Location = new System.Drawing.Point(860, 12);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(88, 32);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.OnCloseClick);
            this.btnClose.Child = null;
            //
            // MonitorDetailDialogForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AcceptButton = this.btnClose;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(980, 720);
            this.Controls.Add(this.lotZone);
            this.Controls.Add(this.spRequest);
            this.Controls.Add(this.requestCard);
            this.Controls.Add(this.buttonPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MonitorDetailDialogForm";
            this.Padding = new System.Windows.Forms.Padding(16);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Lot Detail";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.requestCard.ResumeLayout(false);
            this.lotZone.ResumeLayout(false);
            this.historyCard.ResumeLayout(false);
            this.waferCard.ResumeLayout(false);
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox requestCard;
        private Modern.Lab.WinForms.Controls.Display.ModernFieldList fieldRequest;
        private System.Windows.Forms.Panel spRequest;
        private System.Windows.Forms.Panel lotZone;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox historyCard;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridHistory;
        private System.Windows.Forms.Panel spWafer;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox waferCard;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridWafers;
        private System.Windows.Forms.Panel buttonPanel;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnClose;
    }
}
