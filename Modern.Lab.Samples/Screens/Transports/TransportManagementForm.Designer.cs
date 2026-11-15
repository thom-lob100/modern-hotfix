namespace Modern.Lab.Samples
{
    public partial class TransportManagementForm
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
            this.lblType = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboType = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.lblDurableId = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.txtDurableId = new Modern.Lab.WinForms.Controls.Input.ModernTextBox();
            this.btnRefresh = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnReset = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.menuTransport = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.gapSearch = new System.Windows.Forms.Panel();
            this.midPanel = new System.Windows.Forms.Panel();
            this.splitMid = new Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer();
            this.splitLeft = new Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer();
            this.listCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.gridTransports = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.tabContent = new Modern.Lab.WinForms.Controls.Layout.ModernTabControl();
            this.contentBusy = new Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay();
            this.historyBusy = new Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay();
            this.pageWaferList = new Modern.Lab.WinForms.Controls.Layout.ModernTabPage();
            this.gridWafers = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.pageChipList = new Modern.Lab.WinForms.Controls.Layout.ModernTabPage();
            this.gridChips = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.pageLamellaList = new Modern.Lab.WinForms.Controls.Layout.ModernTabPage();
            this.gridLamellas = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.splitRight = new Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer();
            this.infoCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.fieldInfo = new Modern.Lab.WinForms.Controls.Display.ModernFieldList();
            this.tabHistory = new Modern.Lab.WinForms.Controls.Layout.ModernTabControl();
            this.pageHistory = new Modern.Lab.WinForms.Controls.Layout.ModernTabPage();
            this.gridHistory = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.gapBottom = new System.Windows.Forms.Panel();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.actionCard = new Modern.Lab.WinForms.Controls.Layout.ModernCardPanel();
            this.lblTarget = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.ddbManualIn = new Modern.Lab.WinForms.Controls.Input.ModernDropDownButton();
            this.ddbManualOut = new Modern.Lab.WinForms.Controls.Input.ModernDropDownButton();
            this.btnChangeDest = new Modern.Lab.WinForms.Controls.Input.ModernButton();
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
            this.listCard.SuspendLayout();
            this.tabContent.SuspendLayout();
            this.pageWaferList.SuspendLayout();
            this.pageChipList.SuspendLayout();
            this.pageLamellaList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitRight)).BeginInit();
            this.splitRight.Panel1.SuspendLayout();
            this.splitRight.Panel2.SuspendLayout();
            this.splitRight.SuspendLayout();
            this.infoCard.SuspendLayout();
            this.tabHistory.SuspendLayout();
            this.pageHistory.SuspendLayout();
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
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Title;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(1776, 28);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Transport Management";
            this.lblTitle.TitleBar = true;
            this.lblTitle.Child = null;
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
            this.searchCard.Size = new System.Drawing.Size(1776, 56);
            this.searchCard.TabIndex = 2;
            //
            // lblType
            //
            this.lblType.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblType.Location = new System.Drawing.Point(12, 12);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(76, 32);
            this.lblType.TabIndex = 0;
            this.lblType.Text = "Durable Type";
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
            // lblDurableId
            //
            this.lblDurableId.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblDurableId.Location = new System.Drawing.Point(292, 12);
            this.lblDurableId.Name = "lblDurableId";
            this.lblDurableId.Size = new System.Drawing.Size(76, 32);
            this.lblDurableId.TabIndex = 2;
            this.lblDurableId.Text = "Durable Id";
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
            this.txtDurableId.Child = null;
            //
            // btnRefresh
            //
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.BackColor = System.Drawing.Color.Transparent;
            this.btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRefresh.IconGlyph = "";
            this.btnRefresh.Location = new System.Drawing.Point(1660, 12);
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
            this.btnReset.Location = new System.Drawing.Point(1548, 12);
            this.btnReset.Size = new System.Drawing.Size(104, 32);
            this.btnReset.Text = "Reset";
            this.btnReset.Name = "btnReset";
            this.btnReset.TabIndex = 5;
            this.btnReset.Click += new System.EventHandler(this.OnResetClick);
            this.btnReset.Child = null;
            //
            // menuTransport
            //
            this.menuTransport.Name = "menuTransport";
            this.menuTransport.Size = new System.Drawing.Size(61, 4);
            this.menuTransport.Opening += new System.ComponentModel.CancelEventHandler(this.OnMenuTransportOpening);
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
            this.splitMid.SplitterDistance = 660;
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
            this.splitLeft.Panel1.Controls.Add(this.listCard);
            //
            // splitLeft
            //
            this.splitLeft.Panel1MinSize = 150;
            //
            // splitLeft.Panel2
            //
            this.splitLeft.Panel2.Controls.Add(this.contentBusy);
            this.splitLeft.Panel2.Controls.Add(this.tabContent);
            //
            // contentBusy
            //
            this.contentBusy.Location = new System.Drawing.Point(100, 120);
            this.contentBusy.Message = "Loading contents...";
            this.contentBusy.Name = "contentBusy";
            this.contentBusy.Size = new System.Drawing.Size(300, 120);
            this.contentBusy.TabIndex = 1;
            this.contentBusy.Visible = false;
            this.contentBusy.Child = null;
            //
            // splitLeft.Panel2
            //
            this.splitLeft.Panel2.Padding = new System.Windows.Forms.Padding(0);
            //
            // splitLeft
            //
            this.splitLeft.Panel2MinSize = 120;
            this.splitLeft.Size = new System.Drawing.Size(660, 612);
            this.splitLeft.SplitterDistance = 330;
            this.splitLeft.SplitterWidth = 8;
            this.splitLeft.TabIndex = 0;
            this.splitLeft.TabStop = false;
            //
            // listCard
            //
            this.listCard.Controls.Add(this.gridTransports);
            this.listCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listCard.Location = new System.Drawing.Point(0, 0);
            this.listCard.Name = "listCard";
            this.listCard.Padding = new System.Windows.Forms.Padding(6, 40, 6, 6);
            this.listCard.Size = new System.Drawing.Size(660, 330);
            this.listCard.TabIndex = 0;
            this.listCard.Text = "Transport List";
            this.listCard.TitleBar = true;
            //
            // gridTransports
            //
            this.gridTransports.AllowColumnFilters = true;
            this.gridTransports.AutoFitColumns = true;
            this.gridTransports.EnableColumnVirtualization = true;
            this.gridTransports.ContextMenuStrip = this.menuTransport;
            this.gridTransports.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridTransports.EmptyText = "Search by Durable Id or type";
            this.gridTransports.ExportFileNamePrefix = "Transports";
            this.gridTransports.ExportSheetName = "Transports";
            this.gridTransports.Location = new System.Drawing.Point(6, 40);
            this.gridTransports.Name = "gridTransports";
            this.gridTransports.ShowStatusBar = true;
            this.gridTransports.Size = new System.Drawing.Size(648, 284);
            this.gridTransports.StatusCountFormat = "{0:N0} transports";
            this.gridTransports.TabIndex = 0;
            this.gridTransports.SelectionChanged += new System.EventHandler(this.OnListSelectionChanged);
            this.gridTransports.Child = null;
            //
            // tabContent
            //
            this.tabContent.Controls.Add(this.pageWaferList);
            this.tabContent.Controls.Add(this.pageChipList);
            this.tabContent.Controls.Add(this.pageLamellaList);
            this.tabContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabContent.Location = new System.Drawing.Point(0, 8);
            this.tabContent.Name = "tabContent";
            this.tabContent.Size = new System.Drawing.Size(660, 262);
            this.tabContent.TabIndex = 0;
            //
            // pageWaferList
            //
            this.pageWaferList.Controls.Add(this.gridWafers);
            this.pageWaferList.Location = new System.Drawing.Point(0, 40);
            this.pageWaferList.Name = "pageWaferList";
            this.pageWaferList.Size = new System.Drawing.Size(660, 222);
            this.pageWaferList.TabIndex = 0;
            this.pageWaferList.Text = "Wafer List";
            //
            // gridWafers
            //
            this.gridWafers.AllowFindPanel = false;
            this.gridWafers.AutoFitColumns = true;
            this.gridWafers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridWafers.EmptyText = "No wafers";
            this.gridWafers.Location = new System.Drawing.Point(0, 0);
            this.gridWafers.Name = "gridWafers";
            this.gridWafers.ShowExportMenu = false;
            this.gridWafers.ShowStatusBar = true;
            this.gridWafers.Size = new System.Drawing.Size(660, 222);
            this.gridWafers.StatusCountFormat = "{0:N0} wafers";
            this.gridWafers.TabIndex = 0;
            this.gridWafers.Child = null;
            //
            // pageChipList
            //
            this.pageChipList.Controls.Add(this.gridChips);
            this.pageChipList.Location = new System.Drawing.Point(0, 40);
            this.pageChipList.Name = "pageChipList";
            this.pageChipList.Size = new System.Drawing.Size(660, 222);
            this.pageChipList.TabIndex = 1;
            this.pageChipList.Text = "Chip List";
            //
            // gridChips
            //
            this.gridChips.AllowFindPanel = false;
            this.gridChips.AutoFitColumns = true;
            this.gridChips.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridChips.EmptyText = "No chips";
            this.gridChips.Location = new System.Drawing.Point(0, 0);
            this.gridChips.Name = "gridChips";
            this.gridChips.ShowExportMenu = false;
            this.gridChips.ShowStatusBar = true;
            this.gridChips.Size = new System.Drawing.Size(660, 222);
            this.gridChips.StatusCountFormat = "{0:N0} chips";
            this.gridChips.TabIndex = 0;
            this.gridChips.Child = null;
            //
            // pageLamellaList
            //
            this.pageLamellaList.Controls.Add(this.gridLamellas);
            this.pageLamellaList.Location = new System.Drawing.Point(0, 40);
            this.pageLamellaList.Name = "pageLamellaList";
            this.pageLamellaList.Size = new System.Drawing.Size(660, 222);
            this.pageLamellaList.TabIndex = 2;
            this.pageLamellaList.Text = "Lamella List";
            //
            // gridLamellas
            //
            this.gridLamellas.AllowFindPanel = false;
            this.gridLamellas.AutoFitColumns = true;
            this.gridLamellas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridLamellas.EmptyText = "No lamellae";
            this.gridLamellas.Location = new System.Drawing.Point(0, 0);
            this.gridLamellas.Name = "gridLamellas";
            this.gridLamellas.ShowExportMenu = false;
            this.gridLamellas.ShowStatusBar = true;
            this.gridLamellas.Size = new System.Drawing.Size(660, 222);
            this.gridLamellas.StatusCountFormat = "{0:N0} lamellae";
            this.gridLamellas.TabIndex = 0;
            this.gridLamellas.Child = null;
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
            this.splitRight.Size = new System.Drawing.Size(1108, 612);
            this.splitRight.SplitterDistance = 200;
            this.splitRight.SplitterWidth = 8;
            this.splitRight.TabIndex = 0;
            this.splitRight.TabStop = false;
            //
            // infoCard
            //
            this.infoCard.Controls.Add(this.fieldInfo);
            this.infoCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.infoCard.Location = new System.Drawing.Point(0, 0);
            this.infoCard.Name = "infoCard";
            this.infoCard.Padding = new System.Windows.Forms.Padding(12, 40, 12, 8);
            this.infoCard.Size = new System.Drawing.Size(1108, 200);
            this.infoCard.TabIndex = 0;
            this.infoCard.Text = "Transport Info";
            this.infoCard.TitleBar = true;
            //
            // fieldInfo
            //
            this.fieldInfo.Columns = 6;
            this.fieldInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fieldInfo.Location = new System.Drawing.Point(12, 40);
            this.fieldInfo.Name = "fieldInfo";
            this.fieldInfo.Size = new System.Drawing.Size(1084, 152);
            this.fieldInfo.TabIndex = 0;
            //
            // tabHistory
            //
            this.tabHistory.Controls.Add(this.pageHistory);
            this.tabHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabHistory.Location = new System.Drawing.Point(0, 8);
            this.tabHistory.Name = "tabHistory";
            this.tabHistory.Size = new System.Drawing.Size(1108, 404);
            this.tabHistory.TabIndex = 0;
            //
            // pageHistory
            //
            this.pageHistory.Controls.Add(this.gridHistory);
            this.pageHistory.Location = new System.Drawing.Point(0, 40);
            this.pageHistory.Name = "pageHistory";
            this.pageHistory.Size = new System.Drawing.Size(1108, 364);
            this.pageHistory.TabIndex = 0;
            this.pageHistory.Text = "Transport History";
            //
            // gridHistory
            //
            this.gridHistory.AllowColumnFilters = true;
            this.gridHistory.AutoFitColumns = true;
            this.gridHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridHistory.EmptyText = "No transport history";
            this.gridHistory.ExportFileNamePrefix = "TransportHistory";
            this.gridHistory.ExportSheetName = "Transport History";
            this.gridHistory.Location = new System.Drawing.Point(0, 0);
            this.gridHistory.Name = "gridHistory";
            this.gridHistory.ShowStatusBar = true;
            this.gridHistory.Size = new System.Drawing.Size(1108, 364);
            this.gridHistory.StatusCountFormat = "{0:N0} events";
            this.gridHistory.TabIndex = 0;
            this.gridHistory.Child = null;
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
            this.actionCard.Controls.Add(this.btnChangeDest);
            this.actionCard.Controls.Add(this.ddbManualOut);
            this.actionCard.Controls.Add(this.ddbManualIn);
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
            this.lblTarget.BackColor = System.Drawing.Color.Transparent;
            this.lblTarget.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblTarget.Location = new System.Drawing.Point(12, 16);
            this.lblTarget.Name = "lblTarget";
            this.lblTarget.Size = new System.Drawing.Size(560, 24);
            this.lblTarget.TabIndex = 0;
            this.lblTarget.Text = "No transport selected";
            //
            // ddbManualIn
            //
            this.ddbManualIn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ddbManualIn.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.ddbManualIn.Location = new System.Drawing.Point(1376, 12);
            this.ddbManualIn.Name = "ddbManualIn";
            this.ddbManualIn.Size = new System.Drawing.Size(124, 32);
            this.ddbManualIn.TabIndex = 1;
            this.ddbManualIn.Text = "Manual In";
            this.ddbManualIn.ItemClicked += new System.EventHandler<Modern.Lab.Controls.Wpf.Input.DropDownItemClickedEventArgs>(this.OnActionClicked);
            this.ddbManualIn.Child = null;
            //
            // ddbManualOut
            //
            this.ddbManualOut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ddbManualOut.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.ddbManualOut.Location = new System.Drawing.Point(1508, 12);
            this.ddbManualOut.Name = "ddbManualOut";
            this.ddbManualOut.Size = new System.Drawing.Size(124, 32);
            this.ddbManualOut.TabIndex = 2;
            this.ddbManualOut.Text = "Manual Out";
            this.ddbManualOut.ItemClicked += new System.EventHandler<Modern.Lab.Controls.Wpf.Input.DropDownItemClickedEventArgs>(this.OnActionClicked);
            this.ddbManualOut.Child = null;
            //
            // btnChangeDest
            //
            this.btnChangeDest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChangeDest.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnChangeDest.Location = new System.Drawing.Point(1640, 12);
            this.btnChangeDest.Name = "btnChangeDest";
            this.btnChangeDest.Size = new System.Drawing.Size(124, 32);
            this.btnChangeDest.TabIndex = 3;
            this.btnChangeDest.Text = "Destination";
            this.btnChangeDest.TopLabel = "Change";
            this.btnChangeDest.Click += new System.EventHandler(this.OnChangeDestClick);
            this.btnChangeDest.Child = null;
            //
            // TransportManagementForm
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
            this.Name = "TransportManagementForm";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Transport Management";
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
            this.listCard.ResumeLayout(false);
            this.tabContent.ResumeLayout(false);
            this.pageWaferList.ResumeLayout(false);
            this.pageChipList.ResumeLayout(false);
            this.pageLamellaList.ResumeLayout(false);
            this.splitRight.Panel1.ResumeLayout(false);
            this.splitRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitRight)).EndInit();
            this.splitRight.ResumeLayout(false);
            this.infoCard.ResumeLayout(false);
            this.tabHistory.ResumeLayout(false);
            this.pageHistory.ResumeLayout(false);
            this.bottomPanel.ResumeLayout(false);
            this.actionCard.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel titlePanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblTitle;
        private System.Windows.Forms.Panel spTitle;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel searchCard;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblType;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboType;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblDurableId;
        private Modern.Lab.WinForms.Controls.Input.ModernTextBox txtDurableId;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnRefresh;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnReset;
        private System.Windows.Forms.ContextMenuStrip menuTransport;
        private System.Windows.Forms.Panel gapSearch;
        private System.Windows.Forms.Panel midPanel;
        private Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer splitMid;
        private Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer splitLeft;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox listCard;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridTransports;
        private Modern.Lab.WinForms.Controls.Layout.ModernTabControl tabContent;
        private Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay contentBusy;
        private Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay historyBusy;
        private Modern.Lab.WinForms.Controls.Layout.ModernTabPage pageWaferList;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridWafers;
        private Modern.Lab.WinForms.Controls.Layout.ModernTabPage pageChipList;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridChips;
        private Modern.Lab.WinForms.Controls.Layout.ModernTabPage pageLamellaList;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridLamellas;
        private Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer splitRight;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox infoCard;
        private Modern.Lab.WinForms.Controls.Display.ModernFieldList fieldInfo;
        private Modern.Lab.WinForms.Controls.Layout.ModernTabControl tabHistory;
        private Modern.Lab.WinForms.Controls.Layout.ModernTabPage pageHistory;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridHistory;
        private System.Windows.Forms.Panel gapBottom;
        private System.Windows.Forms.Panel bottomPanel;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel actionCard;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblTarget;
        private Modern.Lab.WinForms.Controls.Input.ModernDropDownButton ddbManualIn;
        private Modern.Lab.WinForms.Controls.Input.ModernDropDownButton ddbManualOut;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnChangeDest;
    }
}
