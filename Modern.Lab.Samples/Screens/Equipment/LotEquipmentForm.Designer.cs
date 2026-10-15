namespace Modern.Lab.Samples
{
    public partial class LotEquipmentForm
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
            this.cboGroup = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.lblLot = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboLot = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.badgeCountdown = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.lblInterval = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboInterval = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.btnRefresh = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.menuEqp = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuLot = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.gapSearch = new System.Windows.Forms.Panel();
            this.midPanel = new System.Windows.Forms.Panel();
            this.splitMain = new Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer();
            this.splitLeft = new Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer();
            this.splitLotRequest = new Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer();
            this.eqpCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.gridEquipmentPriorities = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.portCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.portBusy = new Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay();
            this.durableBusy = new Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay();
            this.gridPortPriorities = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.splitRight = new Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer();
            this.splitDurableDecision = new Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer();
            this.lotCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.gridLotPriorities = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.requestCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.splitRequest = new Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer();
            this.tableRequestMaster = new System.Windows.Forms.TableLayoutPanel();
            this.fieldRequest = new Modern.Lab.WinForms.Controls.Display.ModernFieldList();
            this.panelRequestRemark = new System.Windows.Forms.Panel();
            this.lblRequestRemarkCaption = new System.Windows.Forms.Label();
            this.lblRequestRemark = new System.Windows.Forms.Label();
            this.btnRequestRemarkMore = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.lblRequestEmpty = new System.Windows.Forms.Label();
            this.gridSamples = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.durablePanel = new System.Windows.Forms.Panel();
            this.lblDurable = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboDurable = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.decisionCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.decisionBusy = new Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay();
            this.requestBusy = new Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay();
            this.decisionGrid = new System.Windows.Forms.TableLayoutPanel();
            this.kpiEquipment = new Modern.Lab.WinForms.Controls.Display.ModernKpiCard();
            this.kpiPorts = new Modern.Lab.WinForms.Controls.Display.ModernKpiCard();
            this.kpiLot = new Modern.Lab.WinForms.Controls.Display.ModernKpiCard();
            this.kpiDurable = new Modern.Lab.WinForms.Controls.Display.ModernKpiCard();
            this.decisionBar = new System.Windows.Forms.Panel();
            this.badgeMode = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.badgeJob = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.jobActions = new System.Windows.Forms.TableLayoutPanel();
            this.gapActions = new System.Windows.Forms.Panel();
            this.gapJob = new System.Windows.Forms.Panel();
            this.actionCard = new Modern.Lab.WinForms.Controls.Layout.ModernCardPanel();
            this.lblTarget = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.jobCard = new Modern.Lab.WinForms.Controls.Layout.ModernCardPanel();
            this.btnJobPrep = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnJobStart = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnJobEnd = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.titlePanel.SuspendLayout();
            this.searchCard.SuspendLayout();
            this.midPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitLeft)).BeginInit();
            this.splitLeft.Panel1.SuspendLayout();
            this.splitLeft.Panel2.SuspendLayout();
            this.splitLeft.SuspendLayout();
            this.eqpCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitLotRequest)).BeginInit();
            this.splitLotRequest.Panel1.SuspendLayout();
            this.splitLotRequest.Panel2.SuspendLayout();
            this.splitLotRequest.SuspendLayout();
            this.lotCard.SuspendLayout();
            this.requestCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitRequest)).BeginInit();
            this.splitRequest.Panel1.SuspendLayout();
            this.splitRequest.Panel2.SuspendLayout();
            this.splitRequest.SuspendLayout();
            this.tableRequestMaster.SuspendLayout();
            this.panelRequestRemark.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitRight)).BeginInit();
            this.splitRight.Panel1.SuspendLayout();
            this.splitRight.Panel2.SuspendLayout();
            this.splitRight.SuspendLayout();
            this.portCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitDurableDecision)).BeginInit();
            this.splitDurableDecision.Panel1.SuspendLayout();
            this.splitDurableDecision.Panel2.SuspendLayout();
            this.splitDurableDecision.SuspendLayout();
            this.jobCard.SuspendLayout();
            this.durablePanel.SuspendLayout();
            this.decisionCard.SuspendLayout();
            this.decisionGrid.SuspendLayout();
            this.decisionBar.SuspendLayout();
            this.jobActions.SuspendLayout();
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
            this.lblTitle.Text = "Lot / Equipment";
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
            this.searchCard.Controls.Add(this.lblLot);
            this.searchCard.Controls.Add(this.cboLot);
            this.searchCard.Controls.Add(this.badgeCountdown);
            this.searchCard.Controls.Add(this.lblInterval);
            this.searchCard.Controls.Add(this.cboInterval);
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
            this.cboGroup.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cboGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cboGroup.Location = new System.Drawing.Point(92, 12);
            this.cboGroup.Name = "cboGroup";
            this.cboGroup.PlaceholderText = "All groups";
            this.cboGroup.Size = new System.Drawing.Size(160, 32);
            this.cboGroup.TabIndex = 1;
            this.cboGroup.SelectedIndexChanged += new System.EventHandler(this.OnGroupChanged);
            this.cboGroup.Child = null;
            //
            // lblLot
            //
            this.lblLot.BackColor = System.Drawing.Color.Transparent;
            this.lblLot.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblLot.Location = new System.Drawing.Point(264, 12);
            this.lblLot.Name = "lblLot";
            this.lblLot.Size = new System.Drawing.Size(76, 32);
            this.lblLot.TabIndex = 2;
            this.lblLot.Text = "Lot Id";
            this.lblLot.TextAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            this.lblLot.TopLabel = "Find";
            this.lblLot.TopLabelAlignment = System.Windows.Forms.HorizontalAlignment.Right;
            this.lblLot.Visible = true;
            //
            // cboLot
            //
            this.cboLot.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cboLot.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.cboLot.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cboLot.Location = new System.Drawing.Point(344, 12);
            this.cboLot.Name = "cboLot";
            this.cboLot.PlaceholderText = "Type a lot id";
            this.cboLot.Size = new System.Drawing.Size(200, 32);
            this.cboLot.TabIndex = 3;
            this.cboLot.Visible = true;
            this.cboLot.SelectedIndexChanged += new System.EventHandler(this.OnLotFinderChanged);
            this.cboLot.Child = null;
            //
            // badgeCountdown
            //
            this.badgeCountdown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.badgeCountdown.Location = new System.Drawing.Point(1312, 14);
            this.badgeCountdown.Name = "badgeCountdown";
            this.badgeCountdown.Semantic = Modern.Lab.Theming.SemanticKind.Warning;
            this.badgeCountdown.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeCountdown.Size = new System.Drawing.Size(136, 28);
            this.badgeCountdown.TabIndex = 4;
            this.badgeCountdown.Text = "-";
            this.badgeCountdown.Visible = false;
            this.badgeCountdown.Child = null;
            //
            // lblInterval
            //
            this.lblInterval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInterval.BackColor = System.Drawing.Color.Transparent;
            this.lblInterval.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblInterval.Location = new System.Drawing.Point(1456, 12);
            this.lblInterval.Name = "lblInterval";
            this.lblInterval.Size = new System.Drawing.Size(88, 32);
            this.lblInterval.TabIndex = 5;
            this.lblInterval.Text = "Auto refresh";
            //
            // cboInterval
            //
            this.cboInterval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cboInterval.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboInterval.Location = new System.Drawing.Point(1548, 12);
            this.cboInterval.Name = "cboInterval";
            this.cboInterval.Size = new System.Drawing.Size(100, 32);
            this.cboInterval.TabIndex = 6;
            this.cboInterval.SelectedIndexChanged += new System.EventHandler(this.OnIntervalChanged);
            this.cboInterval.Child = null;
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
            this.btnRefresh.TabIndex = 7;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.Click += new System.EventHandler(this.OnRefreshClick);
            //
            // menuEqp
            //
            this.menuEqp.Name = "menuEqp";
            this.menuEqp.Size = new System.Drawing.Size(61, 4);
            this.menuEqp.Opening += new System.ComponentModel.CancelEventHandler(this.OnMenuEqpOpening);
            //
            // menuLot
            //
            this.menuLot.Name = "menuLot";
            this.menuLot.Size = new System.Drawing.Size(61, 4);
            this.menuLot.Opening += new System.ComponentModel.CancelEventHandler(this.OnMenuLotOpening);
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
            this.midPanel.Controls.Add(this.splitRight);
            this.midPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.midPanel.Location = new System.Drawing.Point(12, 112);
            this.midPanel.Name = "midPanel";
            this.midPanel.Size = new System.Drawing.Size(1776, 876);
            this.midPanel.TabIndex = 4;
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
            this.splitMain.Panel1.Controls.Add(this.eqpCard);
            //
            // splitMain
            //
            this.splitMain.Panel1MinSize = 280;
            //
            // splitMain.Panel2
            //
            this.splitMain.Panel2.Controls.Add(this.splitLotRequest);
            this.splitMain.Panel2.Padding = new System.Windows.Forms.Padding(0);
            //
            // splitMain
            //
            this.splitMain.Panel2MinSize = 300;
            this.splitMain.Size = new System.Drawing.Size(1356, 876);
            this.splitMain.SplitterDistance = 1008;
            this.splitMain.SplitterWidth = 8;
            this.splitMain.KeepRatio = true;
            this.splitMain.TabIndex = 0;
            this.splitMain.TabStop = false;
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
            this.splitLeft.Panel1.Controls.Add(this.lotCard);
            //
            // splitLeft
            //
            this.splitLeft.Panel1MinSize = 160;
            //
            // splitLeft.Panel2
            //
            this.splitLeft.Panel2.Controls.Add(this.splitMain);
            this.splitLeft.Panel2.Padding = new System.Windows.Forms.Padding(0);
            //
            // splitLeft
            //
            this.splitLeft.Panel2MinSize = 160;
            this.splitLeft.Size = new System.Drawing.Size(852, 876);
            this.splitLeft.SplitterDistance = 282;
            this.splitLeft.SplitterWidth = 8;
            this.splitLeft.TabIndex = 0;
            this.splitLeft.TabStop = false;
            //
            // eqpCard
            //
            this.eqpCard.Controls.Add(this.gridEquipmentPriorities);
            this.eqpCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.eqpCard.Location = new System.Drawing.Point(0, 0);
            this.eqpCard.Name = "eqpCard";
            this.eqpCard.Padding = new System.Windows.Forms.Padding(6, 40, 6, 6);
            this.eqpCard.Size = new System.Drawing.Size(852, 282);
            this.eqpCard.TabIndex = 0;
            this.eqpCard.Text = "Equipment List";
            this.eqpCard.TitleBar = true;
            //
            // gridEquipmentPriorities
            //
            this.gridEquipmentPriorities.AllowColumnFilters = false;
            this.gridEquipmentPriorities.AutoFitColumns = true;
            this.gridEquipmentPriorities.ContextMenuStrip = this.menuEqp;
            this.gridEquipmentPriorities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridEquipmentPriorities.EmptyText = "No equipment in this group";
            this.gridEquipmentPriorities.Location = new System.Drawing.Point(6, 40);
            this.gridEquipmentPriorities.Name = "gridEquipmentPriorities";
            this.gridEquipmentPriorities.ShowExportMenu = false;
            this.gridEquipmentPriorities.ShowStatusBar = true;
            this.gridEquipmentPriorities.Size = new System.Drawing.Size(840, 236);
            this.gridEquipmentPriorities.StatusCountFormat = "{0:N0} equipment";
            this.gridEquipmentPriorities.TabIndex = 0;
            this.gridEquipmentPriorities.RowDoubleClick += new System.EventHandler(this.OnEqpRowDoubleClick);
            this.gridEquipmentPriorities.SelectionChanged += new System.EventHandler(this.OnEqpSelectionChanged);
            this.gridEquipmentPriorities.CellCheckChanged += new System.EventHandler<Modern.Lab.Controls.Wpf.Data.GridCheckChangedEventArgs>(this.OnEqpCellCheckChanged);
            this.gridEquipmentPriorities.Child = null;
            //
            // splitLotRequest
            //
            this.splitLotRequest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitLotRequest.Location = new System.Drawing.Point(0, 0);
            this.splitLotRequest.Name = "splitLotRequest";
            //
            // splitLotRequest.Panel1
            //
            this.splitLotRequest.Panel1.Controls.Add(this.portCard);
            //
            // splitLotRequest
            //
            this.splitLotRequest.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitLotRequest.Panel1MinSize = 150;
            //
            // splitLotRequest.Panel2
            //
            this.splitLotRequest.Panel2.Controls.Add(this.durablePanel);
            this.splitLotRequest.Panel2.Padding = new System.Windows.Forms.Padding(0);
            //
            // splitLotRequest
            //
            this.splitLotRequest.Panel2MinSize = 50;
            this.splitLotRequest.Size = new System.Drawing.Size(496, 876);
            this.splitLotRequest.SplitterDistance = 236;
            this.splitLotRequest.SplitterWidth = 8;
            this.splitLotRequest.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitLotRequest.TabIndex = 0;
            this.splitLotRequest.TabStop = false;
            //
            // lotCard
            //
            this.lotCard.Controls.Add(this.gridLotPriorities);
            this.lotCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lotCard.Location = new System.Drawing.Point(0, 0);
            this.lotCard.Name = "lotCard";
            this.lotCard.Padding = new System.Windows.Forms.Padding(6, 40, 6, 6);
            this.lotCard.Size = new System.Drawing.Size(852, 586);
            this.lotCard.TabIndex = 0;
            this.lotCard.Text = "Lot List";
            this.lotCard.TitleBar = true;
            //
            // gridLotPriorities
            //
            this.gridLotPriorities.AllowColumnFilters = false;
            this.gridLotPriorities.AutoFitColumns = true;
            this.gridLotPriorities.ContextMenuStrip = this.menuLot;
            this.gridLotPriorities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridLotPriorities.EmptyText = "No lot to work in this group";
            this.gridLotPriorities.Location = new System.Drawing.Point(6, 40);
            this.gridLotPriorities.Name = "gridLotPriorities";
            this.gridLotPriorities.ShowStatusBar = true;
            this.gridLotPriorities.Size = new System.Drawing.Size(840, 540);
            this.gridLotPriorities.StatusCountFormat = "{0:N0} lots";
            this.gridLotPriorities.TabIndex = 0;
            this.gridLotPriorities.RowDoubleClick += new System.EventHandler(this.OnLotRowDoubleClick);
            this.gridLotPriorities.SelectionChanged += new System.EventHandler(this.OnLotSelectionChanged);
            this.gridLotPriorities.Child = null;
            //
            // splitDurableDecision
            //
            this.splitDurableDecision.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitDurableDecision.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitDurableDecision.Location = new System.Drawing.Point(0, 0);
            this.splitDurableDecision.Name = "splitDurableDecision";
            this.splitDurableDecision.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // splitDurableDecision.Panel1
            //
            this.splitDurableDecision.Panel1.Controls.Add(this.requestCard);
            //
            // splitDurableDecision
            //
            this.splitDurableDecision.Panel1MinSize = 80;
            //
            // splitDurableDecision.Panel2
            //
            this.splitDurableDecision.Panel2.Controls.Add(this.decisionCard);
            this.splitDurableDecision.Panel2.Padding = new System.Windows.Forms.Padding(0);
            //
            // splitDurableDecision
            //
            this.splitDurableDecision.Panel2MinSize = 270;
            this.splitDurableDecision.Size = new System.Drawing.Size(412, 876);
            this.splitDurableDecision.SplitterDistance = 580;
            this.splitDurableDecision.SplitterWidth = 8;
            this.splitDurableDecision.TabIndex = 0;
            this.splitDurableDecision.TabStop = false;
            this.splitDurableDecision.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.OnDecisionSplitterMoved);
            //
            // durablePanel
            //
            this.durablePanel.Controls.Add(this.durableBusy);
            this.durablePanel.Controls.Add(this.cboDurable);
            this.durablePanel.Controls.Add(this.lblDurable);
            this.durablePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.durablePanel.Location = new System.Drawing.Point(0, 0);
            this.durablePanel.Name = "durablePanel";
            this.durablePanel.Size = new System.Drawing.Size(412, 32);
            this.durablePanel.TabIndex = 0;
            //
            // durableBusy
            //
            this.durableBusy.Location = new System.Drawing.Point(6, 0);
            this.durableBusy.Message = "Loading durables...";
            this.durableBusy.Name = "durableBusy";
            this.durableBusy.Size = new System.Drawing.Size(400, 32);
            this.durableBusy.TabIndex = 2;
            this.durableBusy.Visible = false;
            this.durableBusy.Child = null;
            //
            // lblDurable
            //
            this.lblDurable.BackColor = System.Drawing.Color.Transparent;
            this.lblDurable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.lblDurable.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblDurable.Location = new System.Drawing.Point(6, 2);
            this.lblDurable.Name = "lblDurable";
            this.lblDurable.Size = new System.Drawing.Size(52, 32);
            this.lblDurable.TabIndex = 0;
            this.lblDurable.Text = "-";
            this.lblDurable.TextAlignment = System.Windows.Forms.HorizontalAlignment.Left;
            this.lblDurable.TopLabel = "Target";
            this.lblDurable.TopLabelAlignment = System.Windows.Forms.HorizontalAlignment.Left;
            this.lblDurable.Visible = true;
            //
            // cboDurable
            //
            this.cboDurable.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cboDurable.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.cboDurable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboDurable.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cboDurable.Location = new System.Drawing.Point(66, 2);
            this.cboDurable.Name = "cboDurable";
            this.cboDurable.PlaceholderText = "No durable for this port";
            this.cboDurable.Size = new System.Drawing.Size(324, 32);
            this.cboDurable.TabIndex = 1;
            this.cboDurable.Visible = true;
            this.cboDurable.Child = null;
            //
            // requestCard
            //
            this.requestCard.Controls.Add(this.requestBusy);
            this.requestCard.Controls.Add(this.splitRequest);
            //
            // requestBusy
            //
            this.requestBusy.Location = new System.Drawing.Point(100, 250);
            this.requestBusy.Message = "Loading request...";
            this.requestBusy.Name = "requestBusy";
            this.requestBusy.Size = new System.Drawing.Size(300, 120);
            this.requestBusy.TabIndex = 2;
            this.requestBusy.Visible = false;
            this.requestBusy.Child = null;
            //
            // requestCard
            //
            this.requestCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.requestCard.Location = new System.Drawing.Point(0, 0);
            this.requestCard.Name = "requestCard";
            this.requestCard.Padding = new System.Windows.Forms.Padding(6, 40, 6, 6);
            this.requestCard.Size = new System.Drawing.Size(496, 586);
            this.requestCard.TabIndex = 0;
            this.requestCard.Text = "Request List";
            this.requestCard.TitleBar = true;
            //
            // splitRequest
            //
            this.splitRequest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitRequest.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitRequest.Location = new System.Drawing.Point(6, 40);
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
            this.splitRequest.Panel1MinSize = 40;
            //
            // splitRequest.Panel2
            //
            this.splitRequest.Panel2.Controls.Add(this.gridSamples);
            this.splitRequest.Panel2.Padding = new System.Windows.Forms.Padding(0);
            //
            // splitRequest
            //
            this.splitRequest.Panel2MinSize = 40;
            this.splitRequest.Size = new System.Drawing.Size(484, 540);
            this.splitRequest.SplitterDistance = 192;
            this.splitRequest.SplitterWidth = 8;
            this.splitRequest.TabIndex = 0;
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
            this.tableRequestMaster.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableRequestMaster.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 72F));
            this.tableRequestMaster.Size = new System.Drawing.Size(484, 152);
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
            this.fieldRequest.Size = new System.Drawing.Size(484, 80);
            this.fieldRequest.TabIndex = 0;
            //
            // panelRequestRemark
            //
            this.panelRequestRemark.Controls.Add(this.btnRequestRemarkMore);
            this.panelRequestRemark.Controls.Add(this.lblRequestRemark);
            this.panelRequestRemark.Controls.Add(this.lblRequestRemarkCaption);
            this.panelRequestRemark.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRequestRemark.Location = new System.Drawing.Point(0, 80);
            this.panelRequestRemark.Margin = new System.Windows.Forms.Padding(0);
            this.panelRequestRemark.Name = "panelRequestRemark";
            this.panelRequestRemark.Padding = new System.Windows.Forms.Padding(0);
            this.panelRequestRemark.Size = new System.Drawing.Size(484, 72);
            this.panelRequestRemark.TabIndex = 1;
            //
            // lblRequestRemarkCaption
            //
            this.lblRequestRemarkCaption.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblRequestRemarkCaption.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.lblRequestRemarkCaption.Location = new System.Drawing.Point(10, 0);
            this.lblRequestRemarkCaption.Name = "lblRequestRemarkCaption";
            this.lblRequestRemarkCaption.Size = new System.Drawing.Size(464, 14);
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
            this.lblRequestRemark.Size = new System.Drawing.Size(464, 20);
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
            this.btnRequestRemarkMore.Location = new System.Drawing.Point(460, 14);
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
            this.lblRequestEmpty.Text = "Select a lot";
            this.lblRequestEmpty.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // gridSamples
            //
            this.gridSamples.AllowColumnFilters = false;
            this.gridSamples.AllowFindPanel = false;
            this.gridSamples.AutoFitColumns = true;
            this.gridSamples.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridSamples.EmptyText = "No sample on this request";
            this.gridSamples.Location = new System.Drawing.Point(0, 0);
            this.gridSamples.Name = "gridSamples";
            this.gridSamples.ShowExportMenu = false;
            this.gridSamples.ShowStatusBar = true;
            this.gridSamples.Size = new System.Drawing.Size(484, 40);
            this.gridSamples.StatusCountFormat = "{0:N0} samples";
            this.gridSamples.TabIndex = 1;
            this.gridSamples.Child = null;
            //
            // splitRight
            //
            this.splitRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitRight.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitRight.Location = new System.Drawing.Point(0, 0);
            this.splitRight.Name = "splitRight";
            this.splitRight.Orientation = System.Windows.Forms.Orientation.Vertical;
            //
            // splitRight.Panel1
            //
            this.splitRight.Panel1.Controls.Add(this.splitLeft);
            this.splitRight.Panel1.Controls.Add(this.gapActions);
            this.splitRight.Panel1.Controls.Add(this.actionCard);
            //
            // splitRight
            //
            this.splitRight.Panel1MinSize = 892;
            //
            // splitRight.Panel2
            //
            this.splitRight.Panel2.Controls.Add(this.splitDurableDecision);
            this.splitRight.Panel2.Controls.Add(this.gapJob);
            this.splitRight.Panel2.Controls.Add(this.jobCard);
            this.splitRight.Panel2.Padding = new System.Windows.Forms.Padding(0);
            //
            // splitRight
            //
            this.splitRight.Panel2MinSize = 380;
            this.splitRight.Size = new System.Drawing.Size(1776, 876);
            this.splitRight.SplitterDistance = 1356;
            this.splitRight.SplitterWidth = 8;
            this.splitRight.TabIndex = 0;
            this.splitRight.TabStop = false;
            //
            // portCard
            //
            this.portCard.Controls.Add(this.portBusy);
            this.portCard.Controls.Add(this.gridPortPriorities);
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
            this.portCard.Location = new System.Drawing.Point(0, 0);
            this.portCard.Name = "portCard";
            this.portCard.Padding = new System.Windows.Forms.Padding(6, 40, 6, 6);
            this.portCard.Size = new System.Drawing.Size(496, 282);
            this.portCard.TabIndex = 0;
            this.portCard.Text = "Port List";
            this.portCard.TitleBar = true;
            //
            // gridPortPriorities
            //
            this.gridPortPriorities.AllowColumnFilters = false;
            this.gridPortPriorities.AllowFindPanel = false;
            this.gridPortPriorities.AutoFitColumns = true;
            this.gridPortPriorities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridPortPriorities.EmptyText = "Decide an equipment";
            this.gridPortPriorities.Location = new System.Drawing.Point(6, 40);
            this.gridPortPriorities.Name = "gridPortPriorities";
            this.gridPortPriorities.ShowExportMenu = false;
            this.gridPortPriorities.ShowStatusBar = true;
            this.gridPortPriorities.StatusCountFormat = "{0:N0} ports";
            this.gridPortPriorities.Size = new System.Drawing.Size(484, 236);
            this.gridPortPriorities.TabIndex = 0;
            this.gridPortPriorities.RowDoubleClick += new System.EventHandler(this.OnPortRowDoubleClick);
            this.gridPortPriorities.SelectionChanged += new System.EventHandler(this.OnPortSelectionChanged);
            this.gridPortPriorities.Child = null;
            //
            // decisionCard
            //
            this.decisionCard.Controls.Add(this.decisionBusy);
            this.decisionCard.Controls.Add(this.decisionGrid);
            this.decisionCard.Controls.Add(this.decisionBar);
            this.decisionCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.decisionCard.Location = new System.Drawing.Point(0, 0);
            this.decisionCard.Name = "decisionCard";
            this.decisionCard.Padding = new System.Windows.Forms.Padding(6, 40, 6, 6);
            this.decisionCard.Size = new System.Drawing.Size(412, 270);
            this.decisionCard.TabIndex = 0;
            this.decisionCard.Text = "Decision";
            this.decisionCard.TitleBar = true;
            //
            // decisionBusy
            //
            this.decisionBusy.Location = new System.Drawing.Point(110, 70);
            this.decisionBusy.Message = "Loading decision...";
            this.decisionBusy.Name = "decisionBusy";
            this.decisionBusy.Size = new System.Drawing.Size(300, 120);
            this.decisionBusy.TabIndex = 2;
            this.decisionBusy.Visible = false;
            this.decisionBusy.Child = null;
            //
            // decisionGrid
            //
            this.decisionGrid.ColumnCount = 2;
            this.decisionGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.decisionGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.decisionGrid.Controls.Add(this.kpiEquipment, 0, 1);
            this.decisionGrid.Controls.Add(this.kpiPorts, 1, 1);
            this.decisionGrid.Controls.Add(this.kpiLot, 0, 0);
            this.decisionGrid.Controls.Add(this.kpiDurable, 1, 0);
            this.decisionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.decisionGrid.Location = new System.Drawing.Point(6, 40);
            this.decisionGrid.Name = "decisionGrid";
            this.decisionGrid.RowCount = 2;
            this.decisionGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.decisionGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.decisionGrid.Size = new System.Drawing.Size(400, 184);
            this.decisionGrid.TabIndex = 0;
            //
            // kpiEquipment
            //
            this.kpiEquipment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiEquipment.Location = new System.Drawing.Point(4, 4);
            this.kpiEquipment.Margin = new System.Windows.Forms.Padding(4);
            this.kpiEquipment.HighlightValueChange = true;
            this.kpiEquipment.Name = "kpiEquipment";
            this.kpiEquipment.Size = new System.Drawing.Size(246, 96);
            this.kpiEquipment.TabIndex = 0;
            this.kpiEquipment.Title = "Equipment";
            this.kpiEquipment.Value = "-";
            //
            // kpiPorts
            //
            this.kpiPorts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiPorts.Location = new System.Drawing.Point(258, 4);
            this.kpiPorts.Margin = new System.Windows.Forms.Padding(4);
            this.kpiPorts.HighlightValueChange = true;
            this.kpiPorts.Name = "kpiPorts";
            this.kpiPorts.Size = new System.Drawing.Size(246, 96);
            this.kpiPorts.TabIndex = 1;
            this.kpiPorts.Title = "In Port → Out Port";
            this.kpiPorts.Value = "-";
            //
            // kpiLot
            //
            this.kpiLot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiLot.Location = new System.Drawing.Point(4, 108);
            this.kpiLot.Margin = new System.Windows.Forms.Padding(4);
            this.kpiLot.HighlightValueChange = true;
            this.kpiLot.Name = "kpiLot";
            this.kpiLot.Size = new System.Drawing.Size(246, 96);
            this.kpiLot.TabIndex = 2;
            this.kpiLot.Title = "Lot";
            this.kpiLot.Value = "-";
            //
            // kpiDurable
            //
            this.kpiDurable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiDurable.Location = new System.Drawing.Point(258, 108);
            this.kpiDurable.Margin = new System.Windows.Forms.Padding(4);
            this.kpiDurable.HighlightValueChange = true;
            this.kpiDurable.Name = "kpiDurable";
            this.kpiDurable.Size = new System.Drawing.Size(246, 96);
            this.kpiDurable.TabIndex = 3;
            this.kpiDurable.Title = "Durable";
            this.kpiDurable.Value = "-";
            //
            // decisionBar
            //
            this.decisionBar.Controls.Add(this.badgeMode);
            this.decisionBar.Controls.Add(this.badgeJob);
            this.decisionBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.decisionBar.Location = new System.Drawing.Point(6, 224);
            this.decisionBar.Name = "decisionBar";
            this.decisionBar.Size = new System.Drawing.Size(400, 40);
            this.decisionBar.TabIndex = 1;
            //
            // badgeMode
            //
            this.badgeMode.Location = new System.Drawing.Point(4, 6);
            this.badgeMode.Name = "badgeMode";
            this.badgeMode.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeMode.Size = new System.Drawing.Size(104, 28);
            this.badgeMode.TabIndex = 0;
            this.badgeMode.Text = "-";
            this.badgeMode.Child = null;
            //
            // badgeJob
            //
            this.badgeJob.Location = new System.Drawing.Point(112, 6);
            this.badgeJob.Name = "badgeJob";
            this.badgeJob.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeJob.Size = new System.Drawing.Size(80, 28);
            this.badgeJob.TabIndex = 1;
            this.badgeJob.Text = "No job";
            this.badgeJob.Child = null;
            //
            // jobActions
            //
            this.jobActions.ColumnCount = 3;
            this.jobActions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 132F));
            this.jobActions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 132F));
            this.jobActions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.jobActions.Controls.Add(this.btnJobPrep, 0, 0);
            this.jobActions.Controls.Add(this.btnJobStart, 1, 0);
            this.jobActions.Controls.Add(this.btnJobEnd, 2, 0);
            this.jobActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.jobActions.Location = new System.Drawing.Point(6, 8);
            this.jobActions.Margin = new System.Windows.Forms.Padding(0);
            this.jobActions.Name = "jobActions";
            this.jobActions.Padding = new System.Windows.Forms.Padding(0);
            this.jobActions.RowCount = 1;
            this.jobActions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.jobActions.Size = new System.Drawing.Size(396, 40);
            this.jobActions.TabIndex = 2;
            //
            // gapActions
            //
            this.gapActions.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gapActions.Name = "gapActions";
            this.gapActions.Size = new System.Drawing.Size(1356, 8);
            //
            // gapJob
            //
            this.gapJob.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gapJob.Name = "gapJob";
            this.gapJob.Size = new System.Drawing.Size(412, 8);
            //
            // actionCard
            //
            this.actionCard.Controls.Add(this.lblTarget);
            this.actionCard.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.actionCard.Location = new System.Drawing.Point(0, 812);
            this.actionCard.Name = "actionCard";
            this.actionCard.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.actionCard.Size = new System.Drawing.Size(1356, 56);
            this.actionCard.TabIndex = 0;
            //
            // lblTarget
            //
            this.lblTarget.BackColor = System.Drawing.Color.Transparent;
            this.lblTarget.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblTarget.Location = new System.Drawing.Point(12, 16);
            this.lblTarget.Name = "lblTarget";
            this.lblTarget.Size = new System.Drawing.Size(560, 24);
            this.lblTarget.TabIndex = 0;
            this.lblTarget.Text = "No equipment decided";
            //
            // jobCard
            //
            this.jobCard.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.jobCard.Controls.Add(this.jobActions);
            this.jobCard.Location = new System.Drawing.Point(0, 812);
            this.jobCard.Name = "jobCard";
            this.jobCard.Padding = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.jobCard.Size = new System.Drawing.Size(412, 56);
            this.jobCard.TabIndex = 1;
            //
            // btnJobPrep
            //
            this.btnJobPrep.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnJobPrep.BackColor = System.Drawing.Color.Transparent;
            this.btnJobPrep.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnJobPrep.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnJobPrep.IconGlyph = Modern.Lab.WinForms.Rendering.ModernMenuGlyphs.Flag;
            this.btnJobPrep.Location = new System.Drawing.Point(4, 4);
            this.btnJobPrep.Margin = new System.Windows.Forms.Padding(4);
            this.btnJobPrep.Name = "btnJobPrep";
            this.btnJobPrep.Size = new System.Drawing.Size(124, 32);
            this.btnJobPrep.TabIndex = 2;
            this.btnJobPrep.Text = "JobPrep";
            this.btnJobPrep.Click += new System.EventHandler(this.OnJobPrepClick);
            //
            // btnJobStart
            //
            this.btnJobStart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnJobStart.BackColor = System.Drawing.Color.Transparent;
            this.btnJobStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnJobStart.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnJobStart.IconGlyph = Modern.Lab.WinForms.Rendering.ModernMenuGlyphs.Play;
            this.btnJobStart.Location = new System.Drawing.Point(136, 4);
            this.btnJobStart.Margin = new System.Windows.Forms.Padding(4);
            this.btnJobStart.Name = "btnJobStart";
            this.btnJobStart.Size = new System.Drawing.Size(124, 32);
            this.btnJobStart.TabIndex = 3;
            this.btnJobStart.Text = "JobStart";
            this.btnJobStart.Click += new System.EventHandler(this.OnJobStartClick);
            //
            // btnJobEnd
            //
            this.btnJobEnd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnJobEnd.BackColor = System.Drawing.Color.Transparent;
            this.btnJobEnd.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnJobEnd.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnJobEnd.IconGlyph = Modern.Lab.WinForms.Rendering.ModernMenuGlyphs.Stop;
            this.btnJobEnd.Location = new System.Drawing.Point(268, 4);
            this.btnJobEnd.Margin = new System.Windows.Forms.Padding(4);
            this.btnJobEnd.Name = "btnJobEnd";
            this.btnJobEnd.Size = new System.Drawing.Size(124, 32);
            this.btnJobEnd.TabIndex = 4;
            this.btnJobEnd.Text = "JobEnd";
            this.btnJobEnd.Click += new System.EventHandler(this.OnJobEndClick);
            //
            // LotEquipmentForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(1800, 1000);
            this.Controls.Add(this.midPanel);
            this.Controls.Add(this.gapSearch);
            this.Controls.Add(this.searchCard);
            this.Controls.Add(this.spTitle);
            this.Controls.Add(this.titlePanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(1360, 840);
            this.Name = "LotEquipmentForm";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Lot / Equipment";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.titlePanel.ResumeLayout(false);
            this.searchCard.ResumeLayout(false);
            this.midPanel.ResumeLayout(false);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.splitLeft.Panel1.ResumeLayout(false);
            this.splitLeft.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitLeft)).EndInit();
            this.splitLeft.ResumeLayout(false);
            this.eqpCard.ResumeLayout(false);
            this.lotCard.ResumeLayout(false);
            this.panelRequestRemark.ResumeLayout(false);
            this.tableRequestMaster.ResumeLayout(false);
            this.splitRequest.Panel1.ResumeLayout(false);
            this.splitRequest.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitRequest)).EndInit();
            this.splitRequest.ResumeLayout(false);
            this.requestCard.ResumeLayout(false);
            this.splitLotRequest.Panel1.ResumeLayout(false);
            this.splitLotRequest.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitLotRequest)).EndInit();
            this.splitLotRequest.ResumeLayout(false);
            this.durablePanel.ResumeLayout(false);
            this.splitDurableDecision.Panel1.ResumeLayout(false);
            this.splitDurableDecision.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitDurableDecision)).EndInit();
            this.splitDurableDecision.ResumeLayout(false);
            this.portCard.ResumeLayout(false);
            this.decisionCard.ResumeLayout(false);
            this.jobCard.ResumeLayout(false);
            this.splitRight.Panel1.ResumeLayout(false);
            this.splitRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitRight)).EndInit();
            this.splitRight.ResumeLayout(false);
            this.decisionGrid.ResumeLayout(false);
            this.decisionBar.ResumeLayout(false);
            this.jobActions.ResumeLayout(false);
            this.actionCard.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel titlePanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblTitle;
        private System.Windows.Forms.Panel spTitle;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel searchCard;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblLot;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblGroup;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboGroup;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboLot;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeCountdown;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblInterval;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboInterval;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnRefresh;
        private System.Windows.Forms.ContextMenuStrip menuEqp;
        private System.Windows.Forms.ContextMenuStrip menuLot;
        private System.Windows.Forms.Panel gapSearch;
        private System.Windows.Forms.Panel midPanel;
        private Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer splitMain;
        private Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer splitLeft;
        private Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer splitLotRequest;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox eqpCard;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridEquipmentPriorities;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox portCard;
        private Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay portBusy;
        private Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay durableBusy;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridPortPriorities;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox lotCard;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridLotPriorities;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox requestCard;
        private Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer splitRequest;
        private System.Windows.Forms.TableLayoutPanel tableRequestMaster;
        private Modern.Lab.WinForms.Controls.Display.ModernFieldList fieldRequest;
        private System.Windows.Forms.Panel panelRequestRemark;
        private System.Windows.Forms.Label lblRequestRemarkCaption;
        private System.Windows.Forms.Label lblRequestRemark;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnRequestRemarkMore;
        private System.Windows.Forms.Label lblRequestEmpty;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridSamples;
        private Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer splitRight;
        private Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer splitDurableDecision;
        private System.Windows.Forms.Panel durablePanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblDurable;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboDurable;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox decisionCard;
        private Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay decisionBusy;
        private Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay requestBusy;
        private System.Windows.Forms.TableLayoutPanel decisionGrid;
        private Modern.Lab.WinForms.Controls.Display.ModernKpiCard kpiEquipment;
        private Modern.Lab.WinForms.Controls.Display.ModernKpiCard kpiPorts;
        private Modern.Lab.WinForms.Controls.Display.ModernKpiCard kpiLot;
        private Modern.Lab.WinForms.Controls.Display.ModernKpiCard kpiDurable;
        private System.Windows.Forms.Panel decisionBar;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeMode;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeJob;
        private System.Windows.Forms.TableLayoutPanel jobActions;
        private System.Windows.Forms.Panel gapActions;
        private System.Windows.Forms.Panel gapJob;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel actionCard;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblTarget;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel jobCard;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnJobPrep;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnJobStart;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnJobEnd;
    }
}
