namespace Modern.Lab.Samples
{
    public partial class LotManagementForm
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
            this.lblCreate = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.dtpCreate = new Modern.Lab.WinForms.Controls.Input.ModernDateRangePicker();
            this.lblKeyword = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.txtKeyword = new Modern.Lab.WinForms.Controls.Input.ModernTextBox();
            this.btnRefresh = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnReset = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.menuLot = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.gapSearch = new System.Windows.Forms.Panel();
            this.midPanel = new System.Windows.Forms.Panel();
            this.splitMid = new Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer();
            this.splitLeft = new Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer();
            this.lotCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.gridLots = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.waferCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.waferBusy = new Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay();
            this.historyBusy = new Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay();
            this.gridWafers = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.splitRight = new Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer();
            this.infoCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.fieldInfo = new Modern.Lab.WinForms.Controls.Display.ModernFieldList();
            this.tabHistory = new Modern.Lab.WinForms.Controls.Layout.ModernTabControl();
            this.pageLotHistory = new Modern.Lab.WinForms.Controls.Layout.ModernTabPage();
            this.gridLotHistory = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.pageWaferHistory = new Modern.Lab.WinForms.Controls.Layout.ModernTabPage();
            this.gridWaferHistory = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.gapBottom = new System.Windows.Forms.Panel();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.actionCard = new Modern.Lab.WinForms.Controls.Layout.ModernCardPanel();
            this.lblTarget = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.ddbScrap = new Modern.Lab.WinForms.Controls.Input.ModernDropDownButton();
            this.ddbChangeSpec = new Modern.Lab.WinForms.Controls.Input.ModernDropDownButton();
            this.ddbHold = new Modern.Lab.WinForms.Controls.Input.ModernDropDownButton();
            this.ddbCreate = new Modern.Lab.WinForms.Controls.Input.ModernDropDownButton();
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
            this.lotCard.SuspendLayout();
            this.waferCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitRight)).BeginInit();
            this.splitRight.Panel1.SuspendLayout();
            this.splitRight.Panel2.SuspendLayout();
            this.splitRight.SuspendLayout();
            this.infoCard.SuspendLayout();
            this.tabHistory.SuspendLayout();
            this.pageLotHistory.SuspendLayout();
            this.pageWaferHistory.SuspendLayout();
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
            this.lblTitle.Text = "Lot Management";
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
            this.searchCard.Controls.Add(this.lblCreate);
            this.searchCard.Controls.Add(this.dtpCreate);
            this.searchCard.Controls.Add(this.lblKeyword);
            this.searchCard.Controls.Add(this.txtKeyword);
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
            this.lblType.Size = new System.Drawing.Size(64, 32);
            this.lblType.TabIndex = 0;
            this.lblType.Text = "Type";
            this.lblType.TopLabel = "Sub Prod";
            this.lblType.TextAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            this.lblType.TopLabelAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            //
            // cboType
            //
            this.cboType.Location = new System.Drawing.Point(80, 12);
            this.cboType.Name = "cboType";
            this.cboType.PlaceholderText = "All types";
            this.cboType.Size = new System.Drawing.Size(180, 32);
            this.cboType.TabIndex = 1;
            this.cboType.Child = null;
            //
            // lblCreate
            //
            this.lblCreate.BackColor = System.Drawing.Color.Transparent;
            this.lblCreate.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblCreate.Location = new System.Drawing.Point(284, 12);
            this.lblCreate.Name = "lblCreate";
            this.lblCreate.Size = new System.Drawing.Size(84, 32);
            this.lblCreate.TabIndex = 2;
            this.lblCreate.Text = "Create Date";
            //
            // dtpCreate
            //
            this.dtpCreate.Location = new System.Drawing.Point(372, 12);
            this.dtpCreate.Name = "dtpCreate";
            this.dtpCreate.ShowPresets = true;
            this.dtpCreate.Size = new System.Drawing.Size(300, 32);
            this.dtpCreate.TabIndex = 3;
            this.dtpCreate.Child = null;
            //
            // lblKeyword
            //
            this.lblKeyword.BackColor = System.Drawing.Color.Transparent;
            this.lblKeyword.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblKeyword.Location = new System.Drawing.Point(696, 12);
            this.lblKeyword.Name = "lblKeyword";
            this.lblKeyword.Size = new System.Drawing.Size(64, 32);
            this.lblKeyword.TabIndex = 4;
            this.lblKeyword.Text = "Keyword";
            //
            // txtKeyword
            //
            this.txtKeyword.AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-";
            this.txtKeyword.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtKeyword.Location = new System.Drawing.Point(764, 12);
            this.txtKeyword.Name = "txtKeyword";
            this.txtKeyword.PlaceholderText = "Lot Id or Request Serial No (empty = all)";
            this.txtKeyword.Size = new System.Drawing.Size(300, 32);
            this.txtKeyword.TabIndex = 5;
            this.txtKeyword.EnterPressed += new System.EventHandler(this.OnRefreshClick);
            this.txtKeyword.TextChanged += new System.EventHandler(this.OnKeywordTextChanged);
            this.txtKeyword.Child = null;
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
            this.btnRefresh.TabIndex = 6;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.Click += new System.EventHandler(this.OnRefreshClick);
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
            this.splitLeft.Panel1.Controls.Add(this.lotCard);
            //
            // splitLeft
            //
            this.splitLeft.Panel1MinSize = 150;
            //
            // splitLeft.Panel2
            //
            this.splitLeft.Panel2.Controls.Add(this.waferCard);
            this.splitLeft.Panel2.Padding = new System.Windows.Forms.Padding(0);
            //
            // splitLeft
            //
            this.splitLeft.Panel2MinSize = 120;
            this.splitLeft.Size = new System.Drawing.Size(700, 612);
            this.splitLeft.SplitterDistance = 340;
            this.splitLeft.SplitterWidth = 8;
            this.splitLeft.TabIndex = 0;
            this.splitLeft.TabStop = false;
            //
            // lotCard
            //
            this.lotCard.Controls.Add(this.gridLots);
            this.lotCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lotCard.Location = new System.Drawing.Point(0, 0);
            this.lotCard.Name = "lotCard";
            this.lotCard.Padding = new System.Windows.Forms.Padding(6, 40, 6, 6);
            this.lotCard.Size = new System.Drawing.Size(700, 340);
            this.lotCard.TabIndex = 0;
            this.lotCard.Text = "Lot List";
            this.lotCard.TitleBar = true;
            //
            // gridLots
            //
            this.gridLots.AutoFitColumns = true;
            this.gridLots.EnableColumnVirtualization = true;
            this.gridLots.ContextMenuStrip = this.menuLot;
            this.gridLots.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridLots.EmptyText = "No lots";
            this.gridLots.Location = new System.Drawing.Point(8, 40);
            this.gridLots.Name = "gridLots";
            this.gridLots.ShowStatusBar = true;
            this.gridLots.Size = new System.Drawing.Size(684, 292);
            this.gridLots.StatusCountFormat = "{0:N0} lots";
            this.gridLots.TabIndex = 0;
            this.gridLots.SelectionChanged += new System.EventHandler(this.OnLotSelectionChanged);
            this.gridLots.Child = null;
            //
            // waferCard
            //
            this.waferCard.Controls.Add(this.waferBusy);
            this.waferCard.Controls.Add(this.gridWafers);
            //
            // waferBusy
            //
            this.waferBusy.Location = new System.Drawing.Point(100, 120);
            this.waferBusy.Message = "Loading Wafer...";
            this.waferBusy.Name = "waferBusy";
            this.waferBusy.Size = new System.Drawing.Size(300, 120);
            this.waferBusy.TabIndex = 1;
            this.waferBusy.Visible = false;
            this.waferBusy.Child = null;
            //
            // waferCard
            //
            this.waferCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.waferCard.Location = new System.Drawing.Point(0, 8);
            this.waferCard.Name = "waferCard";
            this.waferCard.Padding = new System.Windows.Forms.Padding(6, 40, 6, 6);
            this.waferCard.Size = new System.Drawing.Size(700, 252);
            this.waferCard.TabIndex = 0;
            this.waferCard.Text = "Wafer List";
            this.waferCard.TitleBar = true;
            //
            // gridWafers
            //
            this.gridWafers.AllowFindPanel = false;
            this.gridWafers.AutoFitColumns = true;
            this.gridWafers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridWafers.EmptyText = "No Wafer";
            this.gridWafers.Location = new System.Drawing.Point(8, 40);
            this.gridWafers.Name = "gridWafers";
            this.gridWafers.ShowExportMenu = false;
            this.gridWafers.ShowStatusBar = true;
            this.gridWafers.Size = new System.Drawing.Size(684, 204);
            this.gridWafers.StatusCountFormat = "{0:N0} Wafer";
            this.gridWafers.TabIndex = 0;
            this.gridWafers.SelectionChanged += new System.EventHandler(this.OnWaferSelectionChanged);
            this.gridWafers.Child = null;
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
            this.splitRight.Size = new System.Drawing.Size(1056, 612);
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
            this.infoCard.Size = new System.Drawing.Size(1096, 200);
            this.infoCard.TabIndex = 0;
            this.infoCard.Text = "Lot Info";
            this.infoCard.TitleBar = true;
            //
            // fieldInfo
            //
            this.fieldInfo.Columns = 7;
            this.fieldInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fieldInfo.Location = new System.Drawing.Point(12, 40);
            this.fieldInfo.Name = "fieldInfo";
            this.fieldInfo.Size = new System.Drawing.Size(1072, 152);
            this.fieldInfo.TabIndex = 0;
            //
            // tabHistory
            //
            this.tabHistory.Controls.Add(this.pageLotHistory);
            this.tabHistory.Controls.Add(this.pageWaferHistory);
            this.tabHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabHistory.Location = new System.Drawing.Point(0, 8);
            this.tabHistory.Name = "tabHistory";
            this.tabHistory.Size = new System.Drawing.Size(1096, 392);
            this.tabHistory.TabIndex = 0;
            //
            // pageLotHistory
            //
            this.pageLotHistory.Controls.Add(this.gridLotHistory);
            this.pageLotHistory.Location = new System.Drawing.Point(0, 40);
            this.pageLotHistory.Name = "pageLotHistory";
            this.pageLotHistory.Size = new System.Drawing.Size(1096, 352);
            this.pageLotHistory.TabIndex = 0;
            this.pageLotHistory.Text = "Lot History";
            //
            // gridLotHistory
            //
            this.gridLotHistory.AutoFitColumns = true;
            this.gridLotHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridLotHistory.EmptyText = "No lot history";
            this.gridLotHistory.Location = new System.Drawing.Point(0, 0);
            this.gridLotHistory.Name = "gridLotHistory";
            this.gridLotHistory.ShowStatusBar = true;
            this.gridLotHistory.Size = new System.Drawing.Size(1096, 352);
            this.gridLotHistory.StatusCountFormat = "{0:N0} events";
            this.gridLotHistory.TabIndex = 0;
            this.gridLotHistory.Child = null;
            //
            // pageWaferHistory
            //
            this.pageWaferHistory.Controls.Add(this.gridWaferHistory);
            this.pageWaferHistory.Location = new System.Drawing.Point(0, 40);
            this.pageWaferHistory.Name = "pageWaferHistory";
            this.pageWaferHistory.Size = new System.Drawing.Size(1096, 352);
            this.pageWaferHistory.TabIndex = 1;
            this.pageWaferHistory.Text = "Wafer History";
            //
            // gridWaferHistory
            //
            this.gridWaferHistory.AutoFitColumns = true;
            this.gridWaferHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridWaferHistory.EmptyText = "No Wafer history";
            this.gridWaferHistory.Location = new System.Drawing.Point(0, 0);
            this.gridWaferHistory.Name = "gridWaferHistory";
            this.gridWaferHistory.ShowStatusBar = true;
            this.gridWaferHistory.Size = new System.Drawing.Size(1096, 352);
            this.gridWaferHistory.StatusCountFormat = "{0:N0} events";
            this.gridWaferHistory.TabIndex = 0;
            this.gridWaferHistory.Child = null;
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
            this.actionCard.Controls.Add(this.ddbScrap);
            this.actionCard.Controls.Add(this.ddbChangeSpec);
            this.actionCard.Controls.Add(this.ddbHold);
            this.actionCard.Controls.Add(this.ddbCreate);
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
            this.lblTarget.Text = "No lot selected";
            //
            // ddbScrap
            //
            this.ddbScrap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ddbScrap.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.ddbScrap.Location = new System.Drawing.Point(1632, 12);
            this.ddbScrap.Name = "ddbScrap";
            this.ddbScrap.Size = new System.Drawing.Size(132, 32);
            this.ddbScrap.TabIndex = 4;
            this.ddbScrap.Text = "Scrap";
            this.ddbScrap.ItemClicked += new System.EventHandler<Modern.Lab.Controls.Wpf.Input.DropDownItemClickedEventArgs>(this.OnActionClicked);
            this.ddbScrap.Child = null;
            //
            // ddbChangeSpec
            //
            this.ddbChangeSpec.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ddbChangeSpec.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.ddbChangeSpec.Location = new System.Drawing.Point(1492, 12);
            this.ddbChangeSpec.Name = "ddbChangeSpec";
            this.ddbChangeSpec.Size = new System.Drawing.Size(132, 32);
            this.ddbChangeSpec.TabIndex = 3;
            this.ddbChangeSpec.Text = "ChangeSpec";
            this.ddbChangeSpec.ItemClicked += new System.EventHandler<Modern.Lab.Controls.Wpf.Input.DropDownItemClickedEventArgs>(this.OnActionClicked);
            this.ddbChangeSpec.Child = null;
            //
            // ddbHold
            //
            this.ddbHold.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ddbHold.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.ddbHold.Location = new System.Drawing.Point(1352, 12);
            this.ddbHold.Name = "ddbHold";
            this.ddbHold.Size = new System.Drawing.Size(132, 32);
            this.ddbHold.TabIndex = 2;
            this.ddbHold.Text = "Hold";
            this.ddbHold.ItemClicked += new System.EventHandler<Modern.Lab.Controls.Wpf.Input.DropDownItemClickedEventArgs>(this.OnActionClicked);
            this.ddbHold.Child = null;
            //
            // ddbCreate
            //
            this.ddbCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ddbCreate.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.ddbCreate.Location = new System.Drawing.Point(1212, 12);
            this.ddbCreate.Name = "ddbCreate";
            this.ddbCreate.Size = new System.Drawing.Size(132, 32);
            this.ddbCreate.TabIndex = 1;
            this.ddbCreate.Text = "Create";
            this.ddbCreate.ItemClicked += new System.EventHandler<Modern.Lab.Controls.Wpf.Input.DropDownItemClickedEventArgs>(this.OnActionClicked);
            this.ddbCreate.Child = null;
            //
            // LotManagementForm
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
            this.Name = "LotManagementForm";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Lot Management";
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
            this.lotCard.ResumeLayout(false);
            this.waferCard.ResumeLayout(false);
            this.splitRight.Panel1.ResumeLayout(false);
            this.splitRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitRight)).EndInit();
            this.splitRight.ResumeLayout(false);
            this.infoCard.ResumeLayout(false);
            this.tabHistory.ResumeLayout(false);
            this.pageLotHistory.ResumeLayout(false);
            this.pageWaferHistory.ResumeLayout(false);
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
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblCreate;
        private Modern.Lab.WinForms.Controls.Input.ModernDateRangePicker dtpCreate;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblKeyword;
        private Modern.Lab.WinForms.Controls.Input.ModernTextBox txtKeyword;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnRefresh;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnReset;
        private System.Windows.Forms.ContextMenuStrip menuLot;
        private System.Windows.Forms.Panel gapSearch;
        private System.Windows.Forms.Panel midPanel;
        private Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer splitMid;
        private Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer splitLeft;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox lotCard;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridLots;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox waferCard;
        private Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay waferBusy;
        private Modern.Lab.WinForms.Controls.Display.ModernBusyOverlay historyBusy;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridWafers;
        private Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer splitRight;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox infoCard;
        private Modern.Lab.WinForms.Controls.Display.ModernFieldList fieldInfo;
        private Modern.Lab.WinForms.Controls.Layout.ModernTabControl tabHistory;
        private Modern.Lab.WinForms.Controls.Layout.ModernTabPage pageLotHistory;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridLotHistory;
        private Modern.Lab.WinForms.Controls.Layout.ModernTabPage pageWaferHistory;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridWaferHistory;
        private System.Windows.Forms.Panel gapBottom;
        private System.Windows.Forms.Panel bottomPanel;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel actionCard;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblTarget;
        private Modern.Lab.WinForms.Controls.Input.ModernDropDownButton ddbScrap;
        private Modern.Lab.WinForms.Controls.Input.ModernDropDownButton ddbChangeSpec;
        private Modern.Lab.WinForms.Controls.Input.ModernDropDownButton ddbHold;
        private Modern.Lab.WinForms.Controls.Input.ModernDropDownButton ddbCreate;
    }
}
