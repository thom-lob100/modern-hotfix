namespace Modern.Lab.Samples
{
    public partial class LogisticsRequestForm
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
            this.boardMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miReceive = new Modern.Lab.WinForms.Rendering.ModernMenuItem();
            this.miCreate = new System.Windows.Forms.ToolStripMenuItem();
            this.titlePanel = new System.Windows.Forms.Panel();
            this.lblTitle = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.badgeEnv = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.spTitle = new System.Windows.Forms.Panel();
            this.searchCard = new Modern.Lab.WinForms.Controls.Layout.ModernCardPanel();
            this.lblStatus = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboStatus = new Modern.Lab.WinForms.Controls.Selection.ModernCheckComboBox();
            this.lblLotId = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.txtLotId = new Modern.Lab.WinForms.Controls.Input.ModernTextBox();
            this.lblReqSerialNo = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.txtReqSerialNo = new Modern.Lab.WinForms.Controls.Input.ModernTextBox();
            this.btnRefresh = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnReset = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.badgeCountdown = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.lblInterval = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboInterval = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.gapSearch = new System.Windows.Forms.Panel();
            this.midPanel = new System.Windows.Forms.Panel();
            this.splitMid = new Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer();
            this.boardCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.gridLogistics = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.waferCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.gridRequestWafers = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.gapPipe = new System.Windows.Forms.Panel();
            this.pipeCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.stepPipeline = new Modern.Lab.WinForms.Controls.Display.ModernStepIndicator();
            this.gapKpi = new System.Windows.Forms.Panel();
            this.kpiStrip = new Modern.Lab.WinForms.Controls.Layout.ModernCardPanel();
            this.badgeSent = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.badgeReceived = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.badgeUnlinked = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.badgeLinked = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.badgeCompleted = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.badgeUnmatched = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.badgeAvg = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.badgeOldest = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.gapAction = new System.Windows.Forms.Panel();
            this.actionCard = new Modern.Lab.WinForms.Controls.Layout.ModernCardPanel();
            this.btnReceive = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnManualReceive = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnCreate = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.titlePanel.SuspendLayout();
            this.searchCard.SuspendLayout();
            this.midPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMid)).BeginInit();
            this.splitMid.Panel1.SuspendLayout();
            this.splitMid.Panel2.SuspendLayout();
            this.splitMid.SuspendLayout();
            this.boardCard.SuspendLayout();
            this.waferCard.SuspendLayout();
            this.pipeCard.SuspendLayout();
            this.kpiStrip.SuspendLayout();
            this.actionCard.SuspendLayout();
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
            this.lblTitle.Text = "Logistics & Request";
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
            this.searchCard.Controls.Add(this.lblStatus);
            this.searchCard.Controls.Add(this.cboStatus);
            this.searchCard.Controls.Add(this.lblLotId);
            this.searchCard.Controls.Add(this.txtLotId);
            this.searchCard.Controls.Add(this.lblReqSerialNo);
            this.searchCard.Controls.Add(this.txtReqSerialNo);
            this.searchCard.Controls.Add(this.btnRefresh);
            this.searchCard.Controls.Add(this.badgeCountdown);
            this.searchCard.Controls.Add(this.lblInterval);
            this.searchCard.Controls.Add(this.cboInterval);
            this.searchCard.Controls.Add(this.btnReset);
            this.searchCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchCard.Location = new System.Drawing.Point(12, 48);
            this.searchCard.Name = "searchCard";
            this.searchCard.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.searchCard.Size = new System.Drawing.Size(1516, 56);
            this.searchCard.TabIndex = 2;
            //
            // lblStatus
            //
            this.lblStatus.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblStatus.Location = new System.Drawing.Point(12, 12);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(48, 32);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "Status";
            this.lblStatus.Child = null;
            //
            // cboStatus
            //
            this.cboStatus.Location = new System.Drawing.Point(64, 12);
            this.cboStatus.Name = "cboStatus";
            this.cboStatus.Size = new System.Drawing.Size(320, 32);
            this.cboStatus.TabIndex = 1;
            this.cboStatus.Child = null;
            //
            // lblLotId
            //
            this.lblLotId.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblLotId.Location = new System.Drawing.Point(408, 12);
            this.lblLotId.Name = "lblLotId";
            this.lblLotId.Size = new System.Drawing.Size(56, 32);
            this.lblLotId.TabIndex = 2;
            this.lblLotId.Text = "Lot Id";
            this.lblLotId.Child = null;
            //
            // txtLotId
            //
            this.txtLotId.Location = new System.Drawing.Point(468, 12);
            this.txtLotId.Name = "txtLotId";
            this.txtLotId.PlaceholderText = "Part of a Lot Id";
            this.txtLotId.ShowClearButton = true;
            this.txtLotId.Size = new System.Drawing.Size(140, 32);
            this.txtLotId.TabIndex = 3;
            this.txtLotId.EnterPressed += new System.EventHandler(this.OnSearchEnterPressed);
            this.txtLotId.Child = null;
            //
            // lblReqSerialNo
            //
            this.lblReqSerialNo.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblReqSerialNo.Location = new System.Drawing.Point(632, 12);
            this.lblReqSerialNo.Name = "lblReqSerialNo";
            this.lblReqSerialNo.Size = new System.Drawing.Size(76, 32);
            this.lblReqSerialNo.TabIndex = 4;
            this.lblReqSerialNo.Text = "Request Serial No";
            this.lblReqSerialNo.Child = null;
            //
            // txtReqSerialNo
            //
            this.txtReqSerialNo.Location = new System.Drawing.Point(712, 12);
            this.txtReqSerialNo.Name = "txtReqSerialNo";
            this.txtReqSerialNo.PlaceholderText = "Part of a Serial No";
            this.txtReqSerialNo.ShowClearButton = true;
            this.txtReqSerialNo.Size = new System.Drawing.Size(140, 32);
            this.txtReqSerialNo.TabIndex = 5;
            this.txtReqSerialNo.EnterPressed += new System.EventHandler(this.OnSearchEnterPressed);
            this.txtReqSerialNo.Child = null;
            //
            // btnRefresh
            //
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.BackColor = System.Drawing.Color.Transparent;
            this.btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRefresh.IconGlyph = "\uE72C";
            this.btnRefresh.Location = new System.Drawing.Point(1400, 12);
            this.btnRefresh.Size = new System.Drawing.Size(104, 32);
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.TabIndex = 8;
            this.btnRefresh.Click += new System.EventHandler(this.OnRefreshClick);
            this.btnRefresh.Child = null;
            //
            // badgeCountdown
            //
            this.badgeCountdown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.badgeCountdown.Location = new System.Drawing.Point(960, 14);
            this.badgeCountdown.Name = "badgeCountdown";
            this.badgeCountdown.Semantic = Modern.Lab.Theming.SemanticKind.Warning;
            this.badgeCountdown.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeCountdown.Size = new System.Drawing.Size(112, 28);
            this.badgeCountdown.TabIndex = 21;
            this.badgeCountdown.Text = "-";
            this.badgeCountdown.Visible = false;
            this.badgeCountdown.Child = null;
            //
            // lblInterval
            //
            this.lblInterval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInterval.BackColor = System.Drawing.Color.Transparent;
            this.lblInterval.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblInterval.Location = new System.Drawing.Point(1080, 12);
            this.lblInterval.Name = "lblInterval";
            this.lblInterval.Size = new System.Drawing.Size(88, 32);
            this.lblInterval.TabIndex = 22;
            this.lblInterval.Text = "Auto refresh";
            //
            // cboInterval
            //
            this.cboInterval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cboInterval.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboInterval.Location = new System.Drawing.Point(1172, 12);
            this.cboInterval.Name = "cboInterval";
            this.cboInterval.Size = new System.Drawing.Size(100, 32);
            this.cboInterval.TabIndex = 23;
            this.cboInterval.SelectedIndexChanged += new System.EventHandler(this.OnIntervalChanged);
            this.cboInterval.Child = null;
            //
            // btnReset
            //
            this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReset.BackColor = System.Drawing.Color.Transparent;
            this.btnReset.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnReset.IconGlyph = "\uE777";
            this.btnReset.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Subtle;
            this.btnReset.Location = new System.Drawing.Point(1288, 12);
            this.btnReset.Size = new System.Drawing.Size(104, 32);
            this.btnReset.Text = "Reset";
            this.btnReset.Name = "btnReset";
            this.btnReset.TabIndex = 9;
            this.btnReset.Click += new System.EventHandler(this.OnResetClick);
            this.btnReset.Child = null;
            //
            // gapSearch
            //
            this.gapSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.gapSearch.Location = new System.Drawing.Point(12, 104);
            this.gapSearch.Name = "gapSearch";
            this.gapSearch.Size = new System.Drawing.Size(1516, 8);
            this.gapSearch.TabIndex = 3;
            //
            // midPanel
            //
            this.midPanel.Controls.Add(this.splitMid);
            this.midPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.midPanel.Location = new System.Drawing.Point(12, 112);
            this.midPanel.Name = "midPanel";
            this.midPanel.Size = new System.Drawing.Size(1516, 612);
            this.midPanel.TabIndex = 4;
            //
            // splitMid
            //
            this.splitMid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMid.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitMid.Location = new System.Drawing.Point(0, 0);
            this.splitMid.Name = "splitMid";
            this.splitMid.Orientation = System.Windows.Forms.Orientation.Vertical;
            //
            // splitMid.Panel1
            //
            this.splitMid.Panel1.Controls.Add(this.boardCard);
            this.splitMid.Panel1.Controls.Add(this.gapKpi);
            this.splitMid.Panel1.Controls.Add(this.kpiStrip);
            //
            // splitMid
            //
            this.splitMid.Panel1MinSize = 600;
            //
            // splitMid.Panel2
            //
            this.splitMid.Panel2.Controls.Add(this.waferCard);
            this.splitMid.Panel2.Controls.Add(this.gapPipe);
            this.splitMid.Panel2.Controls.Add(this.pipeCard);
            this.splitMid.Panel2.Controls.Add(this.gapAction);
            this.splitMid.Panel2.Controls.Add(this.actionCard);
            this.splitMid.Panel2.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            //
            // splitMid
            //
            this.splitMid.Panel2MinSize = 260;
            this.splitMid.Size = new System.Drawing.Size(1516, 612);
            this.splitMid.SplitterDistance = 1130;
            this.splitMid.TabIndex = 0;
            //
            // boardCard
            //
            this.boardCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.boardCard.Controls.Add(this.gridLogistics);
            this.boardCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.boardCard.Location = new System.Drawing.Point(0, 0);
            this.boardCard.Name = "boardCard";
            this.boardCard.Padding = new System.Windows.Forms.Padding(8, 40, 8, 8);
            this.boardCard.Size = new System.Drawing.Size(1130, 612);
            this.boardCard.TabIndex = 0;
            this.boardCard.Text = "Send / Receive Status";
            this.boardCard.TitleBar = true;
            //
            // boardMenu
            //
            this.boardMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miReceive,
            this.miCreate});
            this.boardMenu.Name = "boardMenu";
            this.boardMenu.Size = new System.Drawing.Size(140, 48);
            this.boardMenu.Opening += new System.ComponentModel.CancelEventHandler(this.OnLogisticsMenuOpening);
            //
            // miReceive
            //
            this.miReceive.Name = "miReceive";
            this.miReceive.Size = new System.Drawing.Size(139, 22);
            this.miReceive.Glyph = Modern.Lab.WinForms.Rendering.ModernMenuGlyphs.Download;
            this.miReceive.Text = "Receive";
            this.miReceive.Click += new System.EventHandler(this.OnMenuReceiveClick);
            //
            // miCreate
            //
            this.miCreate.Name = "miCreate";
            this.miCreate.Size = new System.Drawing.Size(139, 22);
            this.miCreate.Text = "Create";
            this.miCreate.Click += new System.EventHandler(this.OnMenuCreateClick);
            //
            // gridLogistics
            //
            this.gridLogistics.AutoFitColumns = true;
            this.gridLogistics.ContextMenuStrip = this.boardMenu;
            this.gridLogistics.ExportFileNamePrefix = "LogisticsBoard";
            this.gridLogistics.ExportSheetName = "Logistics";
            this.gridLogistics.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridLogistics.Location = new System.Drawing.Point(8, 40);
            this.gridLogistics.Name = "gridLogistics";
            this.gridLogistics.Size = new System.Drawing.Size(1114, 528);
            this.gridLogistics.ShowStatusBar = true;
            this.gridLogistics.StatusCountFormat = "{0:N0} lots";
            this.gridLogistics.TabIndex = 0;
            this.gridLogistics.ColumnFiltersChanged += new System.EventHandler(this.OnLogisticsColumnFiltersChanged);
            this.gridLogistics.SelectionChanged += new System.EventHandler(this.OnLogisticsSelectionChanged);
            this.gridLogistics.CellButtonClick += new System.EventHandler<Modern.Lab.Controls.Wpf.Data.GridButtonClickEventArgs>(this.OnGridCellButtonClick);
            this.gridLogistics.CellLinkClick += new System.EventHandler<Modern.Lab.Controls.Wpf.Data.GridButtonClickEventArgs>(this.OnReqSerialNoLinkClick);
            this.gridLogistics.Child = null;
            //
            // waferCard
            //
            this.waferCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.waferCard.Controls.Add(this.gridRequestWafers);
            this.waferCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.waferCard.Location = new System.Drawing.Point(8, 0);
            this.waferCard.Name = "waferCard";
            this.waferCard.Padding = new System.Windows.Forms.Padding(8, 40, 8, 8);
            this.waferCard.Size = new System.Drawing.Size(370, 612);
            this.waferCard.TabIndex = 0;
            this.waferCard.Text = "Wafers";
            this.waferCard.TitleBar = true;
            //
            // gridRequestWafers
            //
            this.gridRequestWafers.AutoFitColumns = true;
            this.gridRequestWafers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridRequestWafers.ExportFileNamePrefix = "Wafers";
            this.gridRequestWafers.ExportSheetName = "Wafers";
            this.gridRequestWafers.Location = new System.Drawing.Point(8, 40);
            this.gridRequestWafers.Name = "gridRequestWafers";
            this.gridRequestWafers.ShowStatusBar = true;
            this.gridRequestWafers.Size = new System.Drawing.Size(354, 452);
            this.gridRequestWafers.StatusCountFormat = "{0:N0} wafers";
            this.gridRequestWafers.TabIndex = 0;
            this.gridRequestWafers.Child = null;
            //
            // gapPipe
            //
            this.gapPipe.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gapPipe.Location = new System.Drawing.Point(8, 500);
            this.gapPipe.Name = "gapPipe";
            this.gapPipe.Size = new System.Drawing.Size(370, 8);
            this.gapPipe.TabIndex = 1;
            //
            // pipeCard
            //
            this.pipeCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.pipeCard.Controls.Add(this.stepPipeline);
            this.pipeCard.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pipeCard.Location = new System.Drawing.Point(8, 492);
            this.pipeCard.Name = "pipeCard";
            this.pipeCard.Padding = new System.Windows.Forms.Padding(8, 40, 8, 8);
            this.pipeCard.Size = new System.Drawing.Size(370, 120);
            this.pipeCard.TabIndex = 2;
            this.pipeCard.Text = "Pipeline";
            //
            // stepPipeline
            //
            this.stepPipeline.DisplayMember = "LABEL";
            this.stepPipeline.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stepPipeline.LabelFontSize = 11D;
            this.stepPipeline.Location = new System.Drawing.Point(8, 40);
            this.stepPipeline.Name = "stepPipeline";
            this.stepPipeline.Size = new System.Drawing.Size(354, 72);
            this.stepPipeline.StateMember = "STATE";
            this.stepPipeline.TabIndex = 0;
            this.stepPipeline.Child = null;
            //
            // gapKpi
            //
            this.gapKpi.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gapKpi.Location = new System.Drawing.Point(0, 548);
            this.gapKpi.Name = "gapKpi";
            this.gapKpi.Size = new System.Drawing.Size(1130, 8);
            this.gapKpi.TabIndex = 1;
            //
            // kpiStrip
            //
            this.kpiStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.kpiStrip.Controls.Add(this.badgeSent);
            this.kpiStrip.Controls.Add(this.badgeReceived);
            this.kpiStrip.Controls.Add(this.badgeUnlinked);
            this.kpiStrip.Controls.Add(this.badgeLinked);
            this.kpiStrip.Controls.Add(this.badgeCompleted);
            this.kpiStrip.Controls.Add(this.badgeUnmatched);
            this.kpiStrip.Controls.Add(this.badgeAvg);
            this.kpiStrip.Controls.Add(this.badgeOldest);
            this.kpiStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.kpiStrip.Location = new System.Drawing.Point(0, 556);
            this.kpiStrip.Name = "kpiStrip";
            this.kpiStrip.Size = new System.Drawing.Size(1130, 56);
            this.kpiStrip.TabIndex = 2;
            //
            // badgeSent
            //
            this.badgeSent.ColorValue = "Sent";
            this.badgeSent.Location = new System.Drawing.Point(12, 11);
            this.badgeSent.Name = "badgeSent";
            this.badgeSent.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeSent.FillHeight = true;
            this.badgeSent.FillWidth = true;
            this.badgeSent.Size = new System.Drawing.Size(100, 34);
            this.badgeSent.TabIndex = 0;
            this.badgeSent.Text = "-";
            this.badgeSent.Child = null;
            //
            // badgeReceived
            //
            this.badgeReceived.ColorValue = "Received";
            this.badgeReceived.Location = new System.Drawing.Point(120, 11);
            this.badgeReceived.Name = "badgeReceived";
            this.badgeReceived.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeReceived.FillHeight = true;
            this.badgeReceived.FillWidth = true;
            this.badgeReceived.Size = new System.Drawing.Size(100, 34);
            this.badgeReceived.TabIndex = 1;
            this.badgeReceived.Text = "-";
            this.badgeReceived.Child = null;
            //
            // badgeUnlinked
            //
            this.badgeUnlinked.ColorValue = "Request Unlinked";
            this.badgeUnlinked.Location = new System.Drawing.Point(228, 11);
            this.badgeUnlinked.Name = "badgeUnlinked";
            this.badgeUnlinked.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeUnlinked.FillHeight = true;
            this.badgeUnlinked.FillWidth = true;
            this.badgeUnlinked.Size = new System.Drawing.Size(120, 34);
            this.badgeUnlinked.TabIndex = 2;
            this.badgeUnlinked.Text = "-";
            this.badgeUnlinked.TopLabel = "Request";
            this.badgeUnlinked.Child = null;
            //
            // badgeLinked
            //
            this.badgeLinked.ColorValue = "Request Linked";
            this.badgeLinked.Location = new System.Drawing.Point(356, 11);
            this.badgeLinked.Name = "badgeLinked";
            this.badgeLinked.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeLinked.FillHeight = true;
            this.badgeLinked.FillWidth = true;
            this.badgeLinked.Size = new System.Drawing.Size(100, 34);
            this.badgeLinked.TabIndex = 3;
            this.badgeLinked.Text = "-";
            this.badgeLinked.TopLabel = "Request";
            this.badgeLinked.Child = null;
            //
            // badgeCompleted
            //
            this.badgeCompleted.ColorValue = "Completed";
            this.badgeCompleted.Location = new System.Drawing.Point(464, 11);
            this.badgeCompleted.Name = "badgeCompleted";
            this.badgeCompleted.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeCompleted.FillHeight = true;
            this.badgeCompleted.FillWidth = true;
            this.badgeCompleted.Size = new System.Drawing.Size(120, 34);
            this.badgeCompleted.TabIndex = 4;
            this.badgeCompleted.Text = "-";
            this.badgeCompleted.Child = null;
            //
            // badgeUnmatched
            //
            this.badgeUnmatched.Semantic = Modern.Lab.Theming.SemanticKind.Error;
            this.badgeUnmatched.Location = new System.Drawing.Point(592, 11);
            this.badgeUnmatched.Name = "badgeUnmatched";
            this.badgeUnmatched.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeUnmatched.FillHeight = true;
            this.badgeUnmatched.FillWidth = true;
            this.badgeUnmatched.Size = new System.Drawing.Size(120, 34);
            this.badgeUnmatched.TabIndex = 5;
            this.badgeUnmatched.Text = "-";
            this.badgeUnmatched.Child = null;
            //
            // badgeAvg
            //
            this.badgeAvg.Location = new System.Drawing.Point(720, 11);
            this.badgeAvg.Name = "badgeAvg";
            this.badgeAvg.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeAvg.FillHeight = true;
            this.badgeAvg.FillWidth = true;
            this.badgeAvg.Size = new System.Drawing.Size(100, 34);
            this.badgeAvg.TabIndex = 6;
            this.badgeAvg.Text = "-";
            this.badgeAvg.Child = null;
            //
            // badgeOldest
            //
            this.badgeOldest.Location = new System.Drawing.Point(828, 11);
            this.badgeOldest.Name = "badgeOldest";
            this.badgeOldest.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeOldest.FillHeight = true;
            this.badgeOldest.FillWidth = true;
            this.badgeOldest.Size = new System.Drawing.Size(110, 34);
            this.badgeOldest.TabIndex = 7;
            this.badgeOldest.Text = "-";
            this.badgeOldest.Child = null;
            //
            // gapAction
            //
            this.gapAction.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gapAction.Location = new System.Drawing.Point(8, 548);
            this.gapAction.Name = "gapAction";
            this.gapAction.Size = new System.Drawing.Size(378, 8);
            this.gapAction.TabIndex = 3;
            //
            // actionCard
            //
            this.actionCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.actionCard.Controls.Add(this.btnManualReceive);
            this.actionCard.Controls.Add(this.btnReceive);
            this.actionCard.Controls.Add(this.btnCreate);
            this.actionCard.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.actionCard.Location = new System.Drawing.Point(8, 556);
            this.actionCard.Name = "actionCard";
            this.actionCard.Size = new System.Drawing.Size(378, 56);
            this.actionCard.TabIndex = 4;
            //
            // btnManualReceive
            //
            this.btnManualReceive.Location = new System.Drawing.Point(12, 12);
            this.btnManualReceive.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnManualReceive.Name = "btnManualReceive";
            this.btnManualReceive.Size = new System.Drawing.Size(90, 32);
            this.btnManualReceive.TabIndex = 0;
            this.btnManualReceive.Text = "Receive";
            this.btnManualReceive.TopLabel = "Manual";
            this.btnManualReceive.TopLabelAlignment = System.Windows.Forms.HorizontalAlignment.Right;
            this.btnManualReceive.Click += new System.EventHandler(this.OnManualReceiveClick);
            this.btnManualReceive.Child = null;
            //
            // btnReceive
            //
            this.btnReceive.Location = new System.Drawing.Point(110, 12);
            this.btnReceive.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnReceive.Name = "btnReceive";
            this.btnReceive.Size = new System.Drawing.Size(90, 32);
            this.btnReceive.TabIndex = 2;
            this.btnReceive.Text = "Receive";
            this.btnReceive.Click += new System.EventHandler(this.OnReceiveClick);
            this.btnReceive.Child = null;
            //
            // btnCreate
            //
            this.btnCreate.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnCreate.Location = new System.Drawing.Point(208, 12);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(90, 32);
            this.btnCreate.TabIndex = 3;
            this.btnCreate.Text = "Create";
            this.btnCreate.Click += new System.EventHandler(this.OnCreateClick);
            this.btnCreate.Child = null;
            //
            // LogisticsRequestForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(1540, 800);
            this.Controls.Add(this.midPanel);
            this.Controls.Add(this.gapSearch);
            this.Controls.Add(this.searchCard);
            this.Controls.Add(this.spTitle);
            this.Controls.Add(this.titlePanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(1240, 660);
            this.Name = "LogisticsRequestForm";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Logistics & Request";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.titlePanel.ResumeLayout(false);
            this.searchCard.ResumeLayout(false);
            this.midPanel.ResumeLayout(false);
            this.splitMid.Panel1.ResumeLayout(false);
            this.splitMid.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMid)).EndInit();
            this.splitMid.ResumeLayout(false);
            this.boardCard.ResumeLayout(false);
            this.waferCard.ResumeLayout(false);
            this.pipeCard.ResumeLayout(false);
            this.kpiStrip.ResumeLayout(false);
            this.actionCard.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel titlePanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblTitle;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeEnv;
        private System.Windows.Forms.Panel spTitle;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel searchCard;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblStatus;
        private Modern.Lab.WinForms.Controls.Selection.ModernCheckComboBox cboStatus;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblLotId;
        private Modern.Lab.WinForms.Controls.Input.ModernTextBox txtLotId;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblReqSerialNo;
        private Modern.Lab.WinForms.Controls.Input.ModernTextBox txtReqSerialNo;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnRefresh;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnReset;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeCountdown;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblInterval;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboInterval;
        private System.Windows.Forms.Panel gapSearch;
        private System.Windows.Forms.Panel midPanel;
        private Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer splitMid;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox boardCard;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridLogistics;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox waferCard;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridRequestWafers;
        private System.Windows.Forms.Panel gapPipe;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox pipeCard;
        private Modern.Lab.WinForms.Controls.Display.ModernStepIndicator stepPipeline;
        private System.Windows.Forms.Panel gapKpi;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel kpiStrip;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeSent;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeReceived;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeUnlinked;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeLinked;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeCompleted;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeUnmatched;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeAvg;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeOldest;
        private System.Windows.Forms.Panel gapAction;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel actionCard;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnReceive;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnManualReceive;
        private System.Windows.Forms.ContextMenuStrip boardMenu;
        private Modern.Lab.WinForms.Rendering.ModernMenuItem miReceive;
        private System.Windows.Forms.ToolStripMenuItem miCreate;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnCreate;
    }
}
