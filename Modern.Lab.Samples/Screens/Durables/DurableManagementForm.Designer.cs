namespace Modern.Lab.Samples
{
    public partial class DurableManagementForm
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
            this.lblLoadStatus = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboLoadStatus = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.chkFromFab = new Modern.Lab.WinForms.Controls.Input.ModernCheckBox();
            this.lblDurableId = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.txtDurableId = new Modern.Lab.WinForms.Controls.Input.ModernTextBox();
            this.btnRefresh = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnReset = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.menuDurable = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.gapSearch = new System.Windows.Forms.Panel();
            this.midPanel = new System.Windows.Forms.Panel();
            this.splitMid = new Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer();
            this.splitLeft = new Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer();
            this.durableCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.gridDurables = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.tabContent = new Modern.Lab.WinForms.Controls.Layout.ModernTabControl();
            this.contentBusy = new Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay();
            this.historyBusy = new Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay();
            this.pageWaferList = new Modern.Lab.WinForms.Controls.Layout.ModernTabPage();
            this.gridDurableWafers = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.pageChipList = new Modern.Lab.WinForms.Controls.Layout.ModernTabPage();
            this.gridDurableChips = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.pageLamellaList = new Modern.Lab.WinForms.Controls.Layout.ModernTabPage();
            this.gridDurableLamellas = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.splitRight = new Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer();
            this.infoCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.fieldInfo = new Modern.Lab.WinForms.Controls.Display.ModernFieldList();
            this.tabHistory = new Modern.Lab.WinForms.Controls.Layout.ModernTabControl();
            this.pageHistory = new Modern.Lab.WinForms.Controls.Layout.ModernTabPage();
            this.gridDurableHistory = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.pageStubHistory = new Modern.Lab.WinForms.Controls.Layout.ModernTabPage();
            this.gridDurableChipHistory = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.pageLccHistory = new Modern.Lab.WinForms.Controls.Layout.ModernTabPage();
            this.gridDurableLamellaHistory = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.gapBottom = new System.Windows.Forms.Panel();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.actionCard = new Modern.Lab.WinForms.Controls.Layout.ModernCardPanel();
            this.lblTarget = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.btnTransport = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnEdit = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnExchange = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnRfId = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnCreate = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.ddbHold = new Modern.Lab.WinForms.Controls.Input.ModernDropDownButton();
            this.btnAdjust = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnScrap = new Modern.Lab.WinForms.Controls.Input.ModernButton();
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
            this.durableCard.SuspendLayout();
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
            this.pageStubHistory.SuspendLayout();
            this.pageLccHistory.SuspendLayout();
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
            this.lblTitle.Text = "Durable Management";
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
            this.searchCard.Controls.Add(this.lblType);
            this.searchCard.Controls.Add(this.cboType);
            this.searchCard.Controls.Add(this.lblLoadStatus);
            this.searchCard.Controls.Add(this.cboLoadStatus);
            this.searchCard.Controls.Add(this.chkFromFab);
            this.searchCard.Controls.Add(this.lblDurableId);
            this.searchCard.Controls.Add(this.txtDurableId);
            this.searchCard.Controls.Add(this.btnReset);
            this.searchCard.Controls.Add(this.btnRefresh);
            this.searchCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchCard.Location = new System.Drawing.Point(12, 48);
            this.searchCard.Name = "searchCard";
            this.searchCard.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.searchCard.Size = new System.Drawing.Size(1776, 56);
            this.searchCard.TabIndex = 2;
            //
            // lblType
            //
            this.lblType.BackColor = System.Drawing.Color.Transparent;
            this.lblType.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblType.Location = new System.Drawing.Point(12, 12);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(88, 32);
            this.lblType.TabIndex = 0;
            this.lblType.Text = "Durable Type";
            //
            // cboType
            //
            this.cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboType.Location = new System.Drawing.Point(104, 12);
            this.cboType.Name = "cboType";
            this.cboType.Size = new System.Drawing.Size(120, 32);
            this.cboType.TabIndex = 1;
            this.cboType.SelectedIndexChanged += new System.EventHandler(this.OnTypeChanged);
            this.cboType.Child = null;
            //
            // lblLoadStatus
            //
            this.lblLoadStatus.BackColor = System.Drawing.Color.Transparent;
            this.lblLoadStatus.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblLoadStatus.Location = new System.Drawing.Point(240, 12);
            this.lblLoadStatus.Name = "lblLoadStatus";
            this.lblLoadStatus.Size = new System.Drawing.Size(88, 32);
            this.lblLoadStatus.TabIndex = 2;
            this.lblLoadStatus.Text = "Load Status";
            //
            // cboLoadStatus
            //
            this.cboLoadStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLoadStatus.Location = new System.Drawing.Point(332, 12);
            this.cboLoadStatus.Name = "cboLoadStatus";
            this.cboLoadStatus.Size = new System.Drawing.Size(120, 32);
            this.cboLoadStatus.TabIndex = 2;
            this.cboLoadStatus.SelectedIndexChanged += new System.EventHandler(this.OnLoadStatusChanged);
            this.cboLoadStatus.Child = null;
            //
            // chkFromFab
            //
            this.chkFromFab.Checked = false;
            this.chkFromFab.Location = new System.Drawing.Point(468, 16);
            this.chkFromFab.Name = "chkFromFab";
            this.chkFromFab.Size = new System.Drawing.Size(100, 24);
            this.chkFromFab.TabIndex = 3;
            this.chkFromFab.Text = "From Fab";
            this.chkFromFab.CheckedChanged += new System.EventHandler(this.OnFromFabChanged);
            this.chkFromFab.Child = null;
            //
            // lblDurableId
            //
            this.lblDurableId.BackColor = System.Drawing.Color.Transparent;
            this.lblDurableId.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblDurableId.Location = new System.Drawing.Point(584, 12);
            this.lblDurableId.Name = "lblDurableId";
            this.lblDurableId.Size = new System.Drawing.Size(72, 32);
            this.lblDurableId.TabIndex = 4;
            this.lblDurableId.Text = "Durable Id";
            //
            // txtDurableId
            //
            this.txtDurableId.Location = new System.Drawing.Point(660, 12);
            this.txtDurableId.Name = "txtDurableId";
            this.txtDurableId.PlaceholderText = "Part of a Durable Id";
            this.txtDurableId.ShowClearButton = true;
            this.txtDurableId.Size = new System.Drawing.Size(200, 32);
            this.txtDurableId.TabIndex = 5;
            this.txtDurableId.EnterPressed += new System.EventHandler(this.OnDurableIdEnterPressed);
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
            // menuDurable
            //
            this.menuDurable.Name = "menuDurable";
            this.menuDurable.Size = new System.Drawing.Size(61, 4);
            this.menuDurable.Opening += new System.ComponentModel.CancelEventHandler(this.OnMenuDurableOpening);
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
            this.splitLeft.Panel1.Controls.Add(this.durableCard);
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
            // durableCard
            //
            this.durableCard.Controls.Add(this.gridDurables);
            this.durableCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.durableCard.Location = new System.Drawing.Point(0, 0);
            this.durableCard.Name = "durableCard";
            this.durableCard.Padding = new System.Windows.Forms.Padding(6, 40, 6, 6);
            this.durableCard.Size = new System.Drawing.Size(660, 330);
            this.durableCard.TabIndex = 0;
            this.durableCard.Text = "Durable List";
            this.durableCard.TitleBar = true;
            //
            // gridDurables
            //
            this.gridDurables.AutoFitColumns = true;
            this.gridDurables.EnableColumnVirtualization = true;
            this.gridDurables.ContextMenuStrip = this.menuDurable;
            this.gridDurables.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridDurables.EmptyText = "No durables of this type";
            this.gridDurables.Location = new System.Drawing.Point(8, 40);
            this.gridDurables.Name = "gridDurables";
            this.gridDurables.ShowStatusBar = true;
            this.gridDurables.Size = new System.Drawing.Size(644, 282);
            this.gridDurables.StatusCountFormat = "{0:N0} durables";
            this.gridDurables.TabIndex = 0;
            this.gridDurables.SelectionChanged += new System.EventHandler(this.OnDurableSelectionChanged);
            this.gridDurables.Child = null;
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
            this.pageWaferList.Controls.Add(this.gridDurableWafers);
            this.pageWaferList.Location = new System.Drawing.Point(0, 40);
            this.pageWaferList.Name = "pageWaferList";
            this.pageWaferList.Size = new System.Drawing.Size(660, 222);
            this.pageWaferList.TabIndex = 0;
            this.pageWaferList.Text = "Wafer List";
            //
            // gridDurableWafers
            //
            this.gridDurableWafers.AllowFindPanel = false;
            this.gridDurableWafers.AutoFitColumns = true;
            this.gridDurableWafers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridDurableWafers.EmptyText = "No wafers";
            this.gridDurableWafers.Location = new System.Drawing.Point(0, 0);
            this.gridDurableWafers.Name = "gridDurableWafers";
            this.gridDurableWafers.ShowExportMenu = false;
            this.gridDurableWafers.ShowStatusBar = true;
            this.gridDurableWafers.Size = new System.Drawing.Size(660, 222);
            this.gridDurableWafers.StatusCountFormat = "{0:N0} wafers";
            this.gridDurableWafers.TabIndex = 0;
            this.gridDurableWafers.Child = null;
            //
            // pageChipList
            //
            this.pageChipList.Controls.Add(this.gridDurableChips);
            this.pageChipList.Location = new System.Drawing.Point(0, 40);
            this.pageChipList.Name = "pageChipList";
            this.pageChipList.Size = new System.Drawing.Size(660, 222);
            this.pageChipList.TabIndex = 1;
            this.pageChipList.Text = "Chip List";
            //
            // gridDurableChips
            //
            this.gridDurableChips.AllowFindPanel = false;
            this.gridDurableChips.AutoFitColumns = true;
            this.gridDurableChips.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridDurableChips.EmptyText = "No chips";
            this.gridDurableChips.Location = new System.Drawing.Point(0, 0);
            this.gridDurableChips.Name = "gridDurableChips";
            this.gridDurableChips.ShowExportMenu = false;
            this.gridDurableChips.ShowStatusBar = true;
            this.gridDurableChips.Size = new System.Drawing.Size(660, 222);
            this.gridDurableChips.StatusCountFormat = "{0:N0} chips";
            this.gridDurableChips.TabIndex = 0;
            this.gridDurableChips.Child = null;
            //
            // pageLamellaList
            //
            this.pageLamellaList.Controls.Add(this.gridDurableLamellas);
            this.pageLamellaList.Location = new System.Drawing.Point(0, 40);
            this.pageLamellaList.Name = "pageLamellaList";
            this.pageLamellaList.Size = new System.Drawing.Size(660, 222);
            this.pageLamellaList.TabIndex = 2;
            this.pageLamellaList.Text = "Lamella List";
            //
            // gridDurableLamellas
            //
            this.gridDurableLamellas.AllowFindPanel = false;
            this.gridDurableLamellas.AutoFitColumns = true;
            this.gridDurableLamellas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridDurableLamellas.EmptyText = "No lamellae";
            this.gridDurableLamellas.Location = new System.Drawing.Point(0, 0);
            this.gridDurableLamellas.Name = "gridDurableLamellas";
            this.gridDurableLamellas.ShowExportMenu = false;
            this.gridDurableLamellas.ShowStatusBar = true;
            this.gridDurableLamellas.Size = new System.Drawing.Size(660, 222);
            this.gridDurableLamellas.StatusCountFormat = "{0:N0} lamellae";
            this.gridDurableLamellas.TabIndex = 0;
            this.gridDurableLamellas.Child = null;
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
            this.splitRight.Size = new System.Drawing.Size(1096, 612);
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
            this.infoCard.Size = new System.Drawing.Size(1136, 200);
            this.infoCard.TabIndex = 0;
            this.infoCard.Text = "Durable Info";
            this.infoCard.TitleBar = true;
            //
            // fieldInfo
            //
            this.fieldInfo.Columns = 7;
            this.fieldInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fieldInfo.Location = new System.Drawing.Point(12, 40);
            this.fieldInfo.Name = "fieldInfo";
            this.fieldInfo.Size = new System.Drawing.Size(1112, 152);
            this.fieldInfo.TabIndex = 0;
            //
            // tabHistory
            //
            this.tabHistory.Controls.Add(this.pageHistory);
            this.tabHistory.Controls.Add(this.pageStubHistory);
            this.tabHistory.Controls.Add(this.pageLccHistory);
            this.tabHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabHistory.Location = new System.Drawing.Point(0, 8);
            this.tabHistory.Name = "tabHistory";
            this.tabHistory.Size = new System.Drawing.Size(1136, 392);
            this.tabHistory.TabIndex = 0;
            //
            // pageHistory
            //
            this.pageHistory.Controls.Add(this.gridDurableHistory);
            this.pageHistory.Location = new System.Drawing.Point(0, 40);
            this.pageHistory.Name = "pageHistory";
            this.pageHistory.Size = new System.Drawing.Size(1136, 352);
            this.pageHistory.TabIndex = 0;
            this.pageHistory.Text = "Durable History";
            //
            // gridDurableHistory
            //
            this.gridDurableHistory.AutoFitColumns = true;
            this.gridDurableHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridDurableHistory.EmptyText = "No durable history";
            this.gridDurableHistory.Location = new System.Drawing.Point(0, 0);
            this.gridDurableHistory.Name = "gridDurableHistory";
            this.gridDurableHistory.ShowStatusBar = true;
            this.gridDurableHistory.Size = new System.Drawing.Size(1136, 352);
            this.gridDurableHistory.StatusCountFormat = "{0:N0} events";
            this.gridDurableHistory.TabIndex = 0;
            this.gridDurableHistory.Child = null;
            //
            // pageStubHistory
            //
            this.pageStubHistory.Controls.Add(this.gridDurableChipHistory);
            this.pageStubHistory.Location = new System.Drawing.Point(0, 40);
            this.pageStubHistory.Name = "pageStubHistory";
            this.pageStubHistory.Size = new System.Drawing.Size(1136, 352);
            this.pageStubHistory.TabIndex = 2;
            this.pageStubHistory.Text = "Stub History";
            //
            // gridDurableChipHistory
            //
            this.gridDurableChipHistory.AutoFitColumns = true;
            this.gridDurableChipHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridDurableChipHistory.EmptyText = "No Stub history";
            this.gridDurableChipHistory.Location = new System.Drawing.Point(0, 0);
            this.gridDurableChipHistory.Name = "gridDurableChipHistory";
            this.gridDurableChipHistory.ShowStatusBar = true;
            this.gridDurableChipHistory.Size = new System.Drawing.Size(1136, 352);
            this.gridDurableChipHistory.StatusCountFormat = "{0:N0} events";
            this.gridDurableChipHistory.TabIndex = 0;
            this.gridDurableChipHistory.Child = null;
            //
            // pageLccHistory
            //
            this.pageLccHistory.Controls.Add(this.gridDurableLamellaHistory);
            this.pageLccHistory.Location = new System.Drawing.Point(0, 40);
            this.pageLccHistory.Name = "pageLccHistory";
            this.pageLccHistory.Size = new System.Drawing.Size(1136, 352);
            this.pageLccHistory.TabIndex = 3;
            this.pageLccHistory.Text = "Lcc History";
            //
            // gridDurableLamellaHistory
            //
            this.gridDurableLamellaHistory.AutoFitColumns = true;
            this.gridDurableLamellaHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridDurableLamellaHistory.EmptyText = "No Lcc history";
            this.gridDurableLamellaHistory.Location = new System.Drawing.Point(0, 0);
            this.gridDurableLamellaHistory.Name = "gridDurableLamellaHistory";
            this.gridDurableLamellaHistory.ShowStatusBar = true;
            this.gridDurableLamellaHistory.Size = new System.Drawing.Size(1136, 352);
            this.gridDurableLamellaHistory.StatusCountFormat = "{0:N0} events";
            this.gridDurableLamellaHistory.TabIndex = 0;
            this.gridDurableLamellaHistory.Child = null;
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
            this.actionCard.Controls.Add(this.btnTransport);
            this.actionCard.Controls.Add(this.btnEdit);
            this.actionCard.Controls.Add(this.btnExchange);
            this.actionCard.Controls.Add(this.btnRfId);
            this.actionCard.Controls.Add(this.btnCreate);
            this.actionCard.Controls.Add(this.ddbHold);
            this.actionCard.Controls.Add(this.btnAdjust);
            this.actionCard.Controls.Add(this.btnScrap);
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
            this.lblTarget.Text = "No durable selected";
            //
            // btnTransport
            //
            this.btnTransport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTransport.Location = new System.Drawing.Point(1656, 12);
            this.btnTransport.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnTransport.Name = "btnTransport";
            this.btnTransport.Size = new System.Drawing.Size(108, 32);
            this.btnTransport.TabIndex = 8;
            this.btnTransport.Text = "Transport";
            this.btnTransport.Click += new System.EventHandler(this.OnTransportClick);
            this.btnTransport.Child = null;
            //
            // btnEdit
            //
            this.btnEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEdit.Location = new System.Drawing.Point(1424, 12);
            this.btnEdit.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(108, 32);
            this.btnEdit.TabIndex = 6;
            this.btnEdit.Text = "Edit";
            this.btnEdit.Click += new System.EventHandler(this.OnEditClick);
            this.btnEdit.Child = null;
            //
            // btnExchange
            //
            this.btnExchange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExchange.Location = new System.Drawing.Point(1308, 12);
            this.btnExchange.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnExchange.Name = "btnExchange";
            this.btnExchange.Size = new System.Drawing.Size(108, 32);
            this.btnExchange.TabIndex = 5;
            this.btnExchange.Text = "Exchange";
            this.btnExchange.Click += new System.EventHandler(this.OnExchangeClick);
            this.btnExchange.Child = null;
            //
            // btnRfId
            //
            this.btnRfId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRfId.Location = new System.Drawing.Point(1192, 12);
            this.btnRfId.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnRfId.Name = "btnRfId";
            this.btnRfId.Size = new System.Drawing.Size(108, 32);
            this.btnRfId.TabIndex = 4;
            this.btnRfId.Text = "RfId";
            this.btnRfId.Click += new System.EventHandler(this.OnRfIdClick);
            this.btnRfId.Child = null;
            //
            // btnCreate
            //
            this.btnCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreate.Location = new System.Drawing.Point(844, 12);
            this.btnCreate.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(108, 32);
            this.btnCreate.TabIndex = 1;
            this.btnCreate.Text = "Create";
            this.btnCreate.Click += new System.EventHandler(this.OnCreateClick);
            this.btnCreate.Child = null;
            //
            // ddbHold
            //
            this.ddbHold.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ddbHold.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.ddbHold.Location = new System.Drawing.Point(960, 12);
            this.ddbHold.Name = "ddbHold";
            this.ddbHold.Size = new System.Drawing.Size(108, 32);
            this.ddbHold.TabIndex = 2;
            this.ddbHold.Text = "Release";
            this.ddbHold.TopLabel = "Hold";
            this.ddbHold.ItemClicked += new System.EventHandler<Modern.Lab.Controls.Wpf.Input.DropDownItemClickedEventArgs>(this.OnHoldActionClicked);
            this.ddbHold.Child = null;
            //
            // btnAdjust
            //
            this.btnAdjust.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdjust.Location = new System.Drawing.Point(1076, 12);
            this.btnAdjust.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnAdjust.Name = "btnAdjust";
            this.btnAdjust.Size = new System.Drawing.Size(108, 32);
            this.btnAdjust.TabIndex = 3;
            this.btnAdjust.Text = "Adjust";
            this.btnAdjust.Click += new System.EventHandler(this.OnAdjustClick);
            this.btnAdjust.Child = null;
            //
            // btnScrap
            //
            this.btnScrap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnScrap.Location = new System.Drawing.Point(1540, 12);
            this.btnScrap.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnScrap.Name = "btnScrap";
            this.btnScrap.Size = new System.Drawing.Size(108, 32);
            this.btnScrap.TabIndex = 7;
            this.btnScrap.Text = "Scrap";
            this.btnScrap.Click += new System.EventHandler(this.OnScrapClick);
            this.btnScrap.Child = null;
            //
            // DurableManagementForm
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
            this.Name = "DurableManagementForm";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Durable Management";
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
            this.durableCard.ResumeLayout(false);
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
            this.pageStubHistory.ResumeLayout(false);
            this.pageLccHistory.ResumeLayout(false);
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
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblLoadStatus;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboLoadStatus;
        private Modern.Lab.WinForms.Controls.Input.ModernCheckBox chkFromFab;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblDurableId;
        private Modern.Lab.WinForms.Controls.Input.ModernTextBox txtDurableId;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnRefresh;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnReset;
        private System.Windows.Forms.ContextMenuStrip menuDurable;
        private System.Windows.Forms.Panel gapSearch;
        private System.Windows.Forms.Panel midPanel;
        private Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer splitMid;
        private Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer splitLeft;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox durableCard;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridDurables;
        private Modern.Lab.WinForms.Controls.Layout.ModernTabControl tabContent;
        private Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay contentBusy;
        private Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay historyBusy;
        private Modern.Lab.WinForms.Controls.Layout.ModernTabPage pageWaferList;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridDurableWafers;
        private Modern.Lab.WinForms.Controls.Layout.ModernTabPage pageChipList;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridDurableChips;
        private Modern.Lab.WinForms.Controls.Layout.ModernTabPage pageLamellaList;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridDurableLamellas;
        private Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer splitRight;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox infoCard;
        private Modern.Lab.WinForms.Controls.Display.ModernFieldList fieldInfo;
        private Modern.Lab.WinForms.Controls.Layout.ModernTabControl tabHistory;
        private Modern.Lab.WinForms.Controls.Layout.ModernTabPage pageHistory;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridDurableHistory;
        private Modern.Lab.WinForms.Controls.Layout.ModernTabPage pageStubHistory;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridDurableChipHistory;
        private Modern.Lab.WinForms.Controls.Layout.ModernTabPage pageLccHistory;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridDurableLamellaHistory;
        private System.Windows.Forms.Panel gapBottom;
        private System.Windows.Forms.Panel bottomPanel;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel actionCard;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblTarget;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnTransport;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnEdit;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnExchange;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnRfId;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnCreate;
        private Modern.Lab.WinForms.Controls.Input.ModernDropDownButton ddbHold;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnAdjust;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnScrap;
    }
}
