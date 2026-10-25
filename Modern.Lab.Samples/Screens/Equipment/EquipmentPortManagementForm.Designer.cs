namespace Modern.Lab.Samples
{
    public partial class EquipmentPortManagementForm
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
            this.components = new System.ComponentModel.Container();
            this.titlePanel = new System.Windows.Forms.Panel();
            this.lblTitle = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.spTitle = new System.Windows.Forms.Panel();
            this.searchCard = new Modern.Lab.WinForms.Controls.Layout.ModernCardPanel();
            this.lblGroup = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboGroup = new Modern.Lab.WinForms.Controls.Selection.ModernCheckComboBox();
            this.btnRefresh = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnReset = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.menuEqp = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.gapSearch = new System.Windows.Forms.Panel();
            this.midPanel = new System.Windows.Forms.Panel();
            this.splitMid = new Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer();
            this.splitLeft = new Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer();
            this.eqpCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.treeEquipmentExts = new Modern.Lab.WinForms.Controls.Data.ModernTreeList();
            this.portCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.portBusy = new Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay();
            this.historyBusy = new Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay();
            this.gridPortDets = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.splitRight = new Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer();
            this.infoCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.fieldEqpInfo = new Modern.Lab.WinForms.Controls.Display.ModernFieldList();
            this.tabHistory = new Modern.Lab.WinForms.Controls.Layout.ModernTabControl();
            this.pageEqpHistory = new Modern.Lab.WinForms.Controls.Layout.ModernTabPage();
            this.gridEquipmentExtHistory = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.pagePortHistory = new Modern.Lab.WinForms.Controls.Layout.ModernTabPage();
            this.gridPortDetHistory = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.gapBottom = new System.Windows.Forms.Panel();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.actionCard = new Modern.Lab.WinForms.Controls.Layout.ModernCardPanel();
            this.lblTarget = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.titlePanel.SuspendLayout();
            this.searchCard.SuspendLayout();
            this.midPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMid)).BeginInit();
            this.splitMid.Panel1.SuspendLayout();
            this.splitMid.Panel2.SuspendLayout();
            this.splitMid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitLeft)).BeginInit();
            this.splitLeft.Panel1.SuspendLayout();
            this.splitLeft.Panel2.SuspendLayout();
            this.splitLeft.SuspendLayout();
            this.eqpCard.SuspendLayout();
            this.portCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitRight)).BeginInit();
            this.splitRight.Panel1.SuspendLayout();
            this.splitRight.Panel2.SuspendLayout();
            this.splitRight.SuspendLayout();
            this.infoCard.SuspendLayout();
            this.tabHistory.SuspendLayout();
            this.pageEqpHistory.SuspendLayout();
            this.pagePortHistory.SuspendLayout();
            this.bottomPanel.SuspendLayout();
            this.actionCard.SuspendLayout();
            this.SuspendLayout();
            //
            // titlePanel
            //
            this.titlePanel.Controls.Add(this.lblTitle);
            this.titlePanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.titlePanel.Location = new System.Drawing.Point(12, 12);
            this.titlePanel.Name = "titlePanel";
            this.titlePanel.Size = new System.Drawing.Size(1776, 28);
            this.titlePanel.TabIndex = 0;
            //
            // lblTitle
            //
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Title;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(1776, 28);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Equipment/Port Management";
            this.lblTitle.TitleBar = true;
            //
            // spTitle
            //
            this.spTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.spTitle.Location = new System.Drawing.Point(12, 40);
            this.spTitle.Name = "spTitle";
            this.spTitle.Size = new System.Drawing.Size(1776, 8);
            this.spTitle.TabIndex = 1;
            //
            // searchCard
            //
            this.searchCard.Controls.Add(this.lblGroup);
            this.searchCard.Controls.Add(this.cboGroup);
            this.searchCard.Controls.Add(this.btnReset);
            this.searchCard.Controls.Add(this.btnRefresh);
            this.searchCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchCard.Location = new System.Drawing.Point(12, 48);
            this.searchCard.Name = "searchCard";
            this.searchCard.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.searchCard.Size = new System.Drawing.Size(1776, 56);
            this.searchCard.TabIndex = 2;
            //
            // lblGroup
            //
            this.lblGroup.BackColor = System.Drawing.Color.Transparent;
            this.lblGroup.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblGroup.Location = new System.Drawing.Point(12, 12);
            this.lblGroup.Name = "lblGroup";
            this.lblGroup.Size = new System.Drawing.Size(76, 32);
            this.lblGroup.TabIndex = 0;
            this.lblGroup.Text = "Group";
            this.lblGroup.TextAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            this.lblGroup.TopLabel = "Equipment";
            this.lblGroup.TopLabelAlignment = System.Windows.Forms.HorizontalAlignment.Right;
            //
            // cboGroup
            //
            this.cboGroup.Location = new System.Drawing.Point(92, 12);
            this.cboGroup.Name = "cboGroup";
            this.cboGroup.PlaceholderText = "All groups";
            this.cboGroup.Size = new System.Drawing.Size(240, 32);
            this.cboGroup.TabIndex = 1;
            this.cboGroup.CheckedChanged += new System.EventHandler(this.OnGroupChanged);
            this.cboGroup.Child = null;
            //
            // btnReset
            //
            this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReset.BackColor = System.Drawing.Color.Transparent;
            this.btnReset.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnReset.IconGlyph = "\uE777";
            this.btnReset.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Subtle;
            this.btnReset.Location = new System.Drawing.Point(1548, 12);
            this.btnReset.Size = new System.Drawing.Size(104, 32);
            this.btnReset.Text = "Reset";
            this.btnReset.Name = "btnReset";
            this.btnReset.TabIndex = 20;
            this.btnReset.Click += new System.EventHandler(this.OnResetClick);
            this.btnReset.Child = null;
            //
            // btnRefresh
            //
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.BackColor = System.Drawing.Color.Transparent;
            this.btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRefresh.IconGlyph = "\uE72C";
            this.btnRefresh.Location = new System.Drawing.Point(1660, 12);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(104, 32);
            this.btnRefresh.TabIndex = 2;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.Click += new System.EventHandler(this.OnRefreshClick);
            //
            // menuEqp
            //
            this.menuEqp.Name = "menuEqp";
            this.menuEqp.Size = new System.Drawing.Size(61, 4);
            this.menuEqp.Opening += new System.ComponentModel.CancelEventHandler(this.OnMenuEqpOpening);
            //
            // gapSearch
            //
            this.gapSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.gapSearch.Location = new System.Drawing.Point(12, 104);
            this.gapSearch.Name = "gapSearch";
            this.gapSearch.Size = new System.Drawing.Size(1776, 8);
            this.gapSearch.TabIndex = 3;
            //
            // midPanel
            //
            this.midPanel.Controls.Add(this.splitMid);
            this.midPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.midPanel.Location = new System.Drawing.Point(12, 112);
            this.midPanel.Name = "midPanel";
            this.midPanel.Size = new System.Drawing.Size(1776, 612);
            this.midPanel.TabIndex = 4;
            //
            // splitMid
            //
            this.splitMid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMid.Location = new System.Drawing.Point(0, 0);
            this.splitMid.Name = "splitMid";
            //
            // splitMid.Panel1
            //
            this.splitMid.Panel1.Controls.Add(this.splitLeft);
            //
            // splitMid
            //
            this.splitMid.Panel1MinSize = 360;
            //
            // splitMid.Panel2
            //
            this.splitMid.Panel2.Controls.Add(this.splitRight);
            this.splitMid.Panel2.Padding = new System.Windows.Forms.Padding(0);
            //
            // splitMid
            //
            this.splitMid.Panel2MinSize = 520;
            this.splitMid.Size = new System.Drawing.Size(1776, 612);
            this.splitMid.SplitterDistance = 444;
            this.splitMid.SplitterWidth = 8;
            this.splitMid.KeepRatio = true;
            this.splitMid.TabIndex = 0;
            this.splitMid.TabStop = false;
            //
            // splitLeft
            //
            this.splitLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitLeft.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitLeft.Location = new System.Drawing.Point(0, 0);
            this.splitLeft.Name = "splitLeft";
            this.splitLeft.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // splitLeft.Panel1
            //
            this.splitLeft.Panel1.Controls.Add(this.eqpCard);
            //
            // splitLeft
            //
            this.splitLeft.Panel1MinSize = 150;
            //
            // splitLeft.Panel2
            //
            this.splitLeft.Panel2.Controls.Add(this.portCard);
            this.splitLeft.Panel2.Padding = new System.Windows.Forms.Padding(0);
            //
            // splitLeft
            //
            this.splitLeft.Panel2MinSize = 120;
            this.splitLeft.Size = new System.Drawing.Size(540, 612);
            this.splitLeft.SplitterDistance = 372;
            this.splitLeft.SplitterWidth = 8;
            this.splitLeft.TabIndex = 0;
            this.splitLeft.TabStop = false;
            //
            // eqpCard
            //
            this.eqpCard.Controls.Add(this.treeEquipmentExts);
            this.eqpCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.eqpCard.Location = new System.Drawing.Point(0, 0);
            this.eqpCard.Name = "eqpCard";
            this.eqpCard.Padding = new System.Windows.Forms.Padding(6, 40, 6, 6);
            this.eqpCard.Size = new System.Drawing.Size(540, 372);
            this.eqpCard.TabIndex = 0;
            this.eqpCard.Text = "Equipment List";
            this.eqpCard.TitleBar = true;
            //
            // treeEquipmentExts
            //
            this.treeEquipmentExts.ContextMenuStrip = this.menuEqp;
            this.treeEquipmentExts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeEquipmentExts.EmptyText = "No equipment in this group";
            this.treeEquipmentExts.Location = new System.Drawing.Point(8, 40);
            this.treeEquipmentExts.Name = "treeEquipmentExts";
            this.treeEquipmentExts.Size = new System.Drawing.Size(524, 324);
            this.treeEquipmentExts.TabIndex = 0;
            this.treeEquipmentExts.TreeHeaderText = "Eqp Id";
            this.treeEquipmentExts.SelectedValueChanged += new System.EventHandler(this.OnEquipmentSelectionChanged);
            this.treeEquipmentExts.Child = null;
            //
            // portCard
            //
            this.portCard.Controls.Add(this.portBusy);
            this.portCard.Controls.Add(this.gridPortDets);
            //
            // portBusy
            //
            this.portBusy.Location = new System.Drawing.Point(100, 120);
            this.portBusy.Message = "Loading ports...";
            this.portBusy.Name = "portBusy";
            this.portBusy.Size = new System.Drawing.Size(300, 120);
            this.portBusy.TabIndex = 1;
            this.portBusy.Visible = false;
            this.portBusy.Child = null;
            //
            // portCard
            //
            this.portCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.portCard.Location = new System.Drawing.Point(0, 8);
            this.portCard.Name = "portCard";
            this.portCard.Padding = new System.Windows.Forms.Padding(6, 40, 6, 6);
            this.portCard.Size = new System.Drawing.Size(540, 220);
            this.portCard.TabIndex = 0;
            this.portCard.Text = "Port List";
            this.portCard.TitleBar = true;
            //
            // gridPortDets
            //
            this.gridPortDets.AllowFindPanel = false;
            this.gridPortDets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridPortDets.AutoFitColumns = true;
            this.gridPortDets.EmptyText = "Select an equipment";
            this.gridPortDets.Location = new System.Drawing.Point(8, 40);
            this.gridPortDets.Name = "gridPortDets";
            this.gridPortDets.ShowExportMenu = false;
            this.gridPortDets.ShowStatusBar = true;
            this.gridPortDets.Size = new System.Drawing.Size(524, 172);
            this.gridPortDets.StatusCountFormat = "{0:N0} ports";
            this.gridPortDets.TabIndex = 0;
            this.gridPortDets.SelectionChanged += new System.EventHandler(this.OnPortSelectionChanged);
            this.gridPortDets.Child = null;
            //
            // splitRight
            //
            this.splitRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitRight.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitRight.Location = new System.Drawing.Point(8, 0);
            this.splitRight.Name = "splitRight";
            this.splitRight.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // splitRight.Panel1
            //
            this.splitRight.Panel1.Controls.Add(this.infoCard);
            //
            // splitRight
            //
            this.splitRight.Panel1MinSize = 160;
            //
            // splitRight.Panel2
            //
            this.splitRight.Panel2.Controls.Add(this.historyBusy);
            this.splitRight.Panel2.Controls.Add(this.tabHistory);
            //
            // historyBusy
            //
            this.historyBusy.Location = new System.Drawing.Point(100, 120);
            this.historyBusy.Message = "Loading history...";
            this.historyBusy.Name = "historyBusy";
            this.historyBusy.Size = new System.Drawing.Size(300, 120);
            this.historyBusy.TabIndex = 1;
            this.historyBusy.Visible = false;
            this.historyBusy.Child = null;
            //
            // splitRight.Panel2
            //
            this.splitRight.Panel2.Padding = new System.Windows.Forms.Padding(0);
            //
            // splitRight
            //
            this.splitRight.Panel2MinSize = 150;
            this.splitRight.Size = new System.Drawing.Size(1216, 612);
            this.splitRight.SplitterDistance = 200;
            this.splitRight.SplitterWidth = 8;
            this.splitRight.TabIndex = 0;
            this.splitRight.TabStop = false;
            //
            // infoCard
            //
            this.infoCard.Controls.Add(this.fieldEqpInfo);
            this.infoCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.infoCard.Location = new System.Drawing.Point(0, 0);
            this.infoCard.Name = "infoCard";
            this.infoCard.Padding = new System.Windows.Forms.Padding(12, 40, 12, 8);
            this.infoCard.Size = new System.Drawing.Size(1216, 200);
            this.infoCard.TabIndex = 0;
            this.infoCard.Text = "Equipment Info";
            this.infoCard.TitleBar = true;
            //
            // fieldEqpInfo
            //
            this.fieldEqpInfo.Columns = 7;
            this.fieldEqpInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fieldEqpInfo.Location = new System.Drawing.Point(12, 40);
            this.fieldEqpInfo.Name = "fieldEqpInfo";
            this.fieldEqpInfo.Size = new System.Drawing.Size(1192, 152);
            this.fieldEqpInfo.TabIndex = 0;
            //
            // tabHistory
            //
            this.tabHistory.Controls.Add(this.pageEqpHistory);
            this.tabHistory.Controls.Add(this.pagePortHistory);
            this.tabHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabHistory.Location = new System.Drawing.Point(0, 8);
            this.tabHistory.Name = "tabHistory";
            this.tabHistory.Size = new System.Drawing.Size(1216, 392);
            this.tabHistory.TabIndex = 0;
            //
            // pageEqpHistory
            //
            this.pageEqpHistory.Controls.Add(this.gridEquipmentExtHistory);
            this.pageEqpHistory.Location = new System.Drawing.Point(0, 40);
            this.pageEqpHistory.Name = "pageEqpHistory";
            this.pageEqpHistory.Size = new System.Drawing.Size(1216, 352);
            this.pageEqpHistory.TabIndex = 0;
            this.pageEqpHistory.Text = "Eqp History";
            //
            // gridEquipmentExtHistory
            //
            this.gridEquipmentExtHistory.AutoFitColumns = true;
            this.gridEquipmentExtHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridEquipmentExtHistory.EmptyText = "No equipment history";
            this.gridEquipmentExtHistory.Location = new System.Drawing.Point(0, 0);
            this.gridEquipmentExtHistory.Name = "gridEquipmentExtHistory";
            this.gridEquipmentExtHistory.ShowStatusBar = true;
            this.gridEquipmentExtHistory.Size = new System.Drawing.Size(1216, 352);
            this.gridEquipmentExtHistory.StatusCountFormat = "{0:N0} events";
            this.gridEquipmentExtHistory.TabIndex = 0;
            this.gridEquipmentExtHistory.Child = null;
            //
            // pagePortHistory
            //
            this.pagePortHistory.Controls.Add(this.gridPortDetHistory);
            this.pagePortHistory.Location = new System.Drawing.Point(0, 40);
            this.pagePortHistory.Name = "pagePortHistory";
            this.pagePortHistory.Size = new System.Drawing.Size(1216, 352);
            this.pagePortHistory.TabIndex = 1;
            this.pagePortHistory.Text = "Port History";
            //
            // gridPortDetHistory
            //
            this.gridPortDetHistory.AutoFitColumns = true;
            this.gridPortDetHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridPortDetHistory.EmptyText = "No port history";
            this.gridPortDetHistory.Location = new System.Drawing.Point(0, 0);
            this.gridPortDetHistory.Name = "gridPortDetHistory";
            this.gridPortDetHistory.ShowStatusBar = true;
            this.gridPortDetHistory.Size = new System.Drawing.Size(1216, 352);
            this.gridPortDetHistory.StatusCountFormat = "{0:N0} events";
            this.gridPortDetHistory.TabIndex = 0;
            this.gridPortDetHistory.Child = null;
            //
            // gapBottom
            //
            this.gapBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gapBottom.Location = new System.Drawing.Point(12, 724);
            this.gapBottom.Name = "gapBottom";
            this.gapBottom.Size = new System.Drawing.Size(1776, 8);
            this.gapBottom.TabIndex = 5;
            //
            // bottomPanel
            //
            this.bottomPanel.Controls.Add(this.actionCard);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.Location = new System.Drawing.Point(12, 732);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(1776, 56);
            this.bottomPanel.TabIndex = 6;
            //
            // actionCard
            //
            this.actionCard.Controls.Add(this.lblTarget);
            this.actionCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actionCard.Location = new System.Drawing.Point(0, 0);
            this.actionCard.Name = "actionCard";
            this.actionCard.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.actionCard.Size = new System.Drawing.Size(1776, 56);
            this.actionCard.TabIndex = 0;
            //
            // lblTarget
            //
            this.lblTarget.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTarget.BackColor = System.Drawing.Color.Transparent;
            this.lblTarget.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblTarget.Location = new System.Drawing.Point(12, 16);
            this.lblTarget.Name = "lblTarget";
            this.lblTarget.Size = new System.Drawing.Size(1752, 24);
            this.lblTarget.TabIndex = 0;
            this.lblTarget.Text = "No equipment selected";
            //
            // EquipmentPortManagementForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(1800, 800);
            this.Controls.Add(this.midPanel);
            this.Controls.Add(this.gapBottom);
            this.Controls.Add(this.bottomPanel);
            this.Controls.Add(this.gapSearch);
            this.Controls.Add(this.searchCard);
            this.Controls.Add(this.spTitle);
            this.Controls.Add(this.titlePanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(1240, 660);
            this.Name = "EquipmentPortManagementForm";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Equipment/Port Management";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.titlePanel.ResumeLayout(false);
            this.searchCard.ResumeLayout(false);
            this.midPanel.ResumeLayout(false);
            this.splitMid.Panel1.ResumeLayout(false);
            this.splitMid.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMid)).EndInit();
            this.splitMid.ResumeLayout(false);
            this.splitLeft.Panel1.ResumeLayout(false);
            this.splitLeft.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitLeft)).EndInit();
            this.splitLeft.ResumeLayout(false);
            this.eqpCard.ResumeLayout(false);
            this.portCard.ResumeLayout(false);
            this.splitRight.Panel1.ResumeLayout(false);
            this.splitRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitRight)).EndInit();
            this.splitRight.ResumeLayout(false);
            this.infoCard.ResumeLayout(false);
            this.tabHistory.ResumeLayout(false);
            this.pageEqpHistory.ResumeLayout(false);
            this.pagePortHistory.ResumeLayout(false);
            this.bottomPanel.ResumeLayout(false);
            this.actionCard.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel titlePanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblTitle;
        private System.Windows.Forms.Panel spTitle;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel searchCard;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblGroup;
        private Modern.Lab.WinForms.Controls.Selection.ModernCheckComboBox cboGroup;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnRefresh;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnReset;
        private System.Windows.Forms.ContextMenuStrip menuEqp;
        private System.Windows.Forms.Panel gapSearch;
        private System.Windows.Forms.Panel midPanel;
        private Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer splitMid;
        private Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer splitLeft;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox eqpCard;
        private Modern.Lab.WinForms.Controls.Data.ModernTreeList treeEquipmentExts;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox portCard;
        private Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay portBusy;
        private Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay historyBusy;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridPortDets;
        private Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer splitRight;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox infoCard;
        private Modern.Lab.WinForms.Controls.Display.ModernFieldList fieldEqpInfo;
        private Modern.Lab.WinForms.Controls.Layout.ModernTabControl tabHistory;
        private Modern.Lab.WinForms.Controls.Layout.ModernTabPage pageEqpHistory;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridEquipmentExtHistory;
        private Modern.Lab.WinForms.Controls.Layout.ModernTabPage pagePortHistory;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridPortDetHistory;
        private System.Windows.Forms.Panel gapBottom;
        private System.Windows.Forms.Panel bottomPanel;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel actionCard;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblTarget;
    }
}
