namespace Modern.Lab.Samples
{
    public partial class TransportHistoryForm
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
            this.titlePanel = new System.Windows.Forms.Panel();
            this.lblTitle = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.badgeEnv = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.spTitle = new System.Windows.Forms.Panel();
            this.searchCard = new Modern.Lab.WinForms.Controls.Layout.ModernCardPanel();
            this.lblDurableId = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.txtDurableId = new Modern.Lab.WinForms.Controls.Input.ModernTextBox();
            this.lblType = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboType = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.btnRefresh = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnReset = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.spSearch = new System.Windows.Forms.Panel();
            this.mainZone = new System.Windows.Forms.Panel();
            this.splitMain = new Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer();
            this.leftZone = new System.Windows.Forms.Panel();
            this.listCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.gridTransports = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.rightZone = new System.Windows.Forms.Panel();
            this.historyCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.gridHistory = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.btnExcel = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.spDetail = new System.Windows.Forms.Panel();
            this.detailCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.fieldDetail = new Modern.Lab.WinForms.Controls.Display.ModernFieldList();
            this.titlePanel.SuspendLayout();
            this.searchCard.SuspendLayout();
            this.mainZone.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.leftZone.SuspendLayout();
            this.listCard.SuspendLayout();
            this.rightZone.SuspendLayout();
            this.historyCard.SuspendLayout();
            this.detailCard.SuspendLayout();
            this.SuspendLayout();
            //
            // titlePanel
            //
            this.titlePanel.Controls.Add(this.lblTitle);
            this.titlePanel.Controls.Add(this.badgeEnv);
            this.titlePanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.titlePanel.Location = new System.Drawing.Point(12, 12);
            this.titlePanel.Name = "titlePanel";
            this.titlePanel.Size = new System.Drawing.Size(1516, 28);
            this.titlePanel.TabIndex = 0;
            //
            // lblTitle
            //
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Title;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(1456, 28);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Transport History";
            this.lblTitle.TitleBar = true;
            this.lblTitle.Child = null;
            //
            // badgeEnv
            //
            this.badgeEnv.Semantic = Modern.Lab.Theming.SemanticKind.Info;
            this.badgeEnv.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeEnv.Dock = System.Windows.Forms.DockStyle.Right;
            this.badgeEnv.Location = new System.Drawing.Point(1456, 0);
            this.badgeEnv.Name = "badgeEnv";
            this.badgeEnv.Size = new System.Drawing.Size(60, 28);
            this.badgeEnv.TabIndex = 1;
            this.badgeEnv.Text = "AOS";
            this.badgeEnv.Child = null;
            //
            // spTitle
            //
            this.spTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.spTitle.Location = new System.Drawing.Point(12, 40);
            this.spTitle.Name = "spTitle";
            this.spTitle.Size = new System.Drawing.Size(1516, 8);
            this.spTitle.TabIndex = 1;
            //
            // searchCard
            //
            this.searchCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.searchCard.Controls.Add(this.lblType);
            this.searchCard.Controls.Add(this.cboType);
            this.searchCard.Controls.Add(this.lblDurableId);
            this.searchCard.Controls.Add(this.txtDurableId);
            this.searchCard.Controls.Add(this.btnRefresh);
            this.searchCard.Controls.Add(this.btnReset);
            this.searchCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchCard.Location = new System.Drawing.Point(12, 48);
            this.searchCard.Name = "searchCard";
            this.searchCard.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.searchCard.Size = new System.Drawing.Size(1516, 56);
            this.searchCard.TabIndex = 2;
            //
            // lblDurableId
            //
            this.lblDurableId.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblDurableId.Location = new System.Drawing.Point(292, 12);
            this.lblDurableId.Name = "lblDurableId";
            this.lblDurableId.Size = new System.Drawing.Size(76, 32);
            this.lblDurableId.TabIndex = 2;
            this.lblDurableId.Text = "Durable Id";
            this.lblDurableId.TextAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            this.lblDurableId.Child = null;
            //
            // txtDurableId
            //
            this.txtDurableId.AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-";
            this.txtDurableId.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtDurableId.Location = new System.Drawing.Point(372, 12);
            this.txtDurableId.Name = "txtDurableId";
            this.txtDurableId.PlaceholderText = "Durable or Lot Id (empty = all)";
            this.txtDurableId.Size = new System.Drawing.Size(240, 32);
            this.txtDurableId.TabIndex = 3;
            this.txtDurableId.EnterPressed += new System.EventHandler(this.OnRefreshClick);
            this.txtDurableId.SuggestionClicked += new System.EventHandler(this.OnRefreshClick);
            this.txtDurableId.TextChanged += new System.EventHandler(this.OnDurableIdTextChanged);
            this.txtDurableId.Child = null;
            //
            // lblType
            //
            this.lblType.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblType.Location = new System.Drawing.Point(12, 12);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(76, 32);
            this.lblType.TabIndex = 0;
            this.lblType.Text = "Type";
            this.lblType.TextAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            this.lblType.TopLabel = "Durable";
            this.lblType.TopLabelAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            this.lblType.Child = null;
            //
            // cboType
            //
            this.cboType.Location = new System.Drawing.Point(92, 12);
            this.cboType.Name = "cboType";
            this.cboType.PlaceholderText = "All types";
            this.cboType.Size = new System.Drawing.Size(180, 32);
            this.cboType.TabIndex = 1;
            this.cboType.Child = null;
            //
            // btnRefresh
            //
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.BackColor = System.Drawing.Color.Transparent;
            this.btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRefresh.IconGlyph = "";
            this.btnRefresh.Location = new System.Drawing.Point(1400, 12);
            this.btnRefresh.Size = new System.Drawing.Size(104, 32);
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.TabIndex = 4;
            this.btnRefresh.Click += new System.EventHandler(this.OnRefreshClick);
            this.btnRefresh.Child = null;
            //
            // btnReset
            //
            this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReset.BackColor = System.Drawing.Color.Transparent;
            this.btnReset.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnReset.IconGlyph = "";
            this.btnReset.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Subtle;
            this.btnReset.Location = new System.Drawing.Point(1288, 12);
            this.btnReset.Size = new System.Drawing.Size(104, 32);
            this.btnReset.Text = "Reset";
            this.btnReset.Name = "btnReset";
            this.btnReset.TabIndex = 5;
            this.btnReset.Click += new System.EventHandler(this.OnResetClick);
            this.btnReset.Child = null;
            //
            // spSearch
            //
            this.spSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.spSearch.Location = new System.Drawing.Point(12, 104);
            this.spSearch.Name = "spSearch";
            this.spSearch.Size = new System.Drawing.Size(1516, 8);
            this.spSearch.TabIndex = 3;
            //
            // mainZone
            //
            this.mainZone.Controls.Add(this.splitMain);
            this.mainZone.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainZone.Location = new System.Drawing.Point(12, 112);
            this.mainZone.Name = "mainZone";
            this.mainZone.Size = new System.Drawing.Size(1516, 676);
            this.mainZone.TabIndex = 4;
            //
            // splitMain
            //
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(0, 0);
            this.splitMain.Name = "splitMain";
            this.splitMain.Orientation = System.Windows.Forms.Orientation.Vertical;
            //
            // splitMain.Panel1
            //
            this.splitMain.Panel1.Controls.Add(this.leftZone);
            //
            // splitMain
            //
            this.splitMain.Panel1MinSize = 320;
            //
            // splitMain.Panel2
            //
            this.splitMain.Panel2.Controls.Add(this.rightZone);
            //
            // splitMain
            //
            this.splitMain.Panel2MinSize = 480;
            this.splitMain.Size = new System.Drawing.Size(1516, 676);
            this.splitMain.SplitterDistance = 560;
            this.splitMain.TabIndex = 0;
            //
            // leftZone
            //
            this.leftZone.Controls.Add(this.listCard);
            this.leftZone.Dock = System.Windows.Forms.DockStyle.Fill;
            this.leftZone.Location = new System.Drawing.Point(0, 0);
            this.leftZone.Name = "leftZone";
            this.leftZone.Size = new System.Drawing.Size(560, 676);
            this.leftZone.TabIndex = 0;
            //
            // listCard
            //
            this.listCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.listCard.Controls.Add(this.gridTransports);
            this.listCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listCard.Location = new System.Drawing.Point(0, 0);
            this.listCard.Name = "listCard";
            this.listCard.Padding = new System.Windows.Forms.Padding(8, 40, 8, 8);
            this.listCard.Size = new System.Drawing.Size(560, 676);
            this.listCard.TabIndex = 0;
            this.listCard.Text = "Transports";
            this.listCard.TitleBar = true;
            //
            // gridTransports
            //
            this.gridTransports.AllowColumnFilters = true;
            this.gridTransports.AutoFitColumns = true;
            this.gridTransports.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridTransports.EmptyText = "Search by Durable Id or type";
            this.gridTransports.ExportFileNamePrefix = "Transports";
            this.gridTransports.ExportSheetName = "Transports";
            this.gridTransports.Location = new System.Drawing.Point(8, 40);
            this.gridTransports.Name = "gridTransports";
            this.gridTransports.ShowStatusBar = true;
            this.gridTransports.Size = new System.Drawing.Size(544, 628);
            this.gridTransports.StatusCountFormat = "{0:N0} transports";
            this.gridTransports.TabIndex = 0;
            this.gridTransports.SelectionChanged += new System.EventHandler(this.OnListSelectionChanged);
            this.gridTransports.Child = null;
            //
            // rightZone
            //
            this.rightZone.Controls.Add(this.historyCard);
            this.rightZone.Controls.Add(this.spDetail);
            this.rightZone.Controls.Add(this.detailCard);
            this.rightZone.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightZone.Location = new System.Drawing.Point(0, 0);
            this.rightZone.Name = "rightZone";
            this.rightZone.Size = new System.Drawing.Size(952, 676);
            this.rightZone.TabIndex = 1;
            //
            // historyCard
            //
            this.historyCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.historyCard.Controls.Add(this.btnExcel);
            this.historyCard.Controls.Add(this.gridHistory);
            this.historyCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.historyCard.Location = new System.Drawing.Point(0, 168);
            this.historyCard.Name = "historyCard";
            this.historyCard.Padding = new System.Windows.Forms.Padding(8, 40, 8, 8);
            this.historyCard.Size = new System.Drawing.Size(952, 508);
            this.historyCard.TabIndex = 2;
            this.historyCard.Text = "Transport History";
            this.historyCard.TitleBar = true;
            //
            // gridHistory
            //
            this.gridHistory.AllowColumnFilters = true;
            this.gridHistory.AutoFitColumns = true;
            this.gridHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridHistory.EmptyText = "Select a transport";
            this.gridHistory.ExportFileNamePrefix = "TransportHistory";
            this.gridHistory.ExportSheetName = "Transport History";
            this.gridHistory.Location = new System.Drawing.Point(8, 40);
            this.gridHistory.Name = "gridHistory";
            this.gridHistory.ShowStatusBar = true;
            this.gridHistory.Size = new System.Drawing.Size(936, 460);
            this.gridHistory.StatusCountFormat = "{0:N0} events";
            this.gridHistory.TabIndex = 0;
            this.gridHistory.Child = null;
            //
            // btnExcel
            //
            this.btnExcel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExcel.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Excel;
            this.btnExcel.Location = new System.Drawing.Point(864, 4);
            this.btnExcel.Name = "btnExcel";
            this.btnExcel.Size = new System.Drawing.Size(80, 32);
            this.btnExcel.TabIndex = 1;
            this.btnExcel.Text = "Excel";
            this.btnExcel.Click += new System.EventHandler(this.OnExportClick);
            this.btnExcel.Child = null;
            //
            // spDetail
            //
            this.spDetail.Dock = System.Windows.Forms.DockStyle.Top;
            this.spDetail.Location = new System.Drawing.Point(0, 160);
            this.spDetail.Name = "spDetail";
            this.spDetail.Size = new System.Drawing.Size(952, 8);
            this.spDetail.TabIndex = 1;
            //
            // detailCard
            //
            this.detailCard.Controls.Add(this.fieldDetail);
            this.detailCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.detailCard.Location = new System.Drawing.Point(0, 0);
            this.detailCard.Name = "detailCard";
            this.detailCard.Padding = new System.Windows.Forms.Padding(12, 40, 12, 8);
            this.detailCard.Size = new System.Drawing.Size(952, 160);
            this.detailCard.TabIndex = 0;
            this.detailCard.Text = "Transport Info";
            this.detailCard.TitleBar = true;
            //
            // fieldDetail
            //
            this.fieldDetail.Columns = 6;
            this.fieldDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fieldDetail.Location = new System.Drawing.Point(12, 40);
            this.fieldDetail.Name = "fieldDetail";
            this.fieldDetail.Size = new System.Drawing.Size(928, 112);
            this.fieldDetail.TabIndex = 0;
            this.fieldDetail.Child = null;
            //
            // TransportHistoryForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(1540, 800);
            this.Controls.Add(this.mainZone);
            this.Controls.Add(this.spSearch);
            this.Controls.Add(this.searchCard);
            this.Controls.Add(this.spTitle);
            this.Controls.Add(this.titlePanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(1240, 660);
            this.Name = "TransportHistoryForm";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Transport History";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.titlePanel.ResumeLayout(false);
            this.searchCard.ResumeLayout(false);
            this.mainZone.ResumeLayout(false);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.leftZone.ResumeLayout(false);
            this.listCard.ResumeLayout(false);
            this.rightZone.ResumeLayout(false);
            this.historyCard.ResumeLayout(false);
            this.detailCard.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel titlePanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblTitle;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeEnv;
        private System.Windows.Forms.Panel spTitle;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel searchCard;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblDurableId;
        private Modern.Lab.WinForms.Controls.Input.ModernTextBox txtDurableId;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblType;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboType;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnRefresh;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnReset;
        private System.Windows.Forms.Panel spSearch;
        private System.Windows.Forms.Panel mainZone;
        private Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer splitMain;
        private System.Windows.Forms.Panel leftZone;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox listCard;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridTransports;
        private System.Windows.Forms.Panel rightZone;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox historyCard;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridHistory;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnExcel;
        private System.Windows.Forms.Panel spDetail;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox detailCard;
        private Modern.Lab.WinForms.Controls.Display.ModernFieldList fieldDetail;
    }
}
