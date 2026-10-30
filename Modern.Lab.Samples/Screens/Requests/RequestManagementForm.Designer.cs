namespace Modern.Lab.Samples
{
    public partial class RequestManagementForm
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
            this.lblRequestDate = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.dtpRequest = new Modern.Lab.WinForms.Controls.Input.ModernDateRangePicker();
            this.lblAcceptDate = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.dtpAccept = new Modern.Lab.WinForms.Controls.Input.ModernDateRangePicker();
            this.lblReqSerialNo = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.txtReqSerialNo = new Modern.Lab.WinForms.Controls.Input.ModernTextBox();
            this.lblState = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboState = new Modern.Lab.WinForms.Controls.Selection.ModernCheckComboBox();
            this.lblRequester = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.txtRequester = new Modern.Lab.WinForms.Controls.Input.ModernTextBox();
            this.btnReset = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnRefresh = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.spSearch = new System.Windows.Forms.Panel();
            this.mainZone = new System.Windows.Forms.Panel();
            this.splitMain = new Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer();
            this.leftZone = new System.Windows.Forms.Panel();
            this.requestCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.gridRequests = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.rightZone = new System.Windows.Forms.Panel();
            this.sampleCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.gridSamples = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.spStatus = new System.Windows.Forms.Panel();
            this.statusCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.badgeState = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.lblRequestKey = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.stepStatus = new Modern.Lab.WinForms.Controls.Display.ModernStepIndicator();
            this.btnReject = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnNextStep = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.titlePanel.SuspendLayout();
            this.searchCard.SuspendLayout();
            this.mainZone.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.leftZone.SuspendLayout();
            this.requestCard.SuspendLayout();
            this.rightZone.SuspendLayout();
            this.sampleCard.SuspendLayout();
            this.statusCard.SuspendLayout();
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
            this.lblTitle.Text = "Request Management";
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
            this.searchCard.Controls.Add(this.lblRequestDate);
            this.searchCard.Controls.Add(this.dtpRequest);
            this.searchCard.Controls.Add(this.lblAcceptDate);
            this.searchCard.Controls.Add(this.dtpAccept);
            this.searchCard.Controls.Add(this.lblReqSerialNo);
            this.searchCard.Controls.Add(this.txtReqSerialNo);
            this.searchCard.Controls.Add(this.lblState);
            this.searchCard.Controls.Add(this.cboState);
            this.searchCard.Controls.Add(this.lblRequester);
            this.searchCard.Controls.Add(this.txtRequester);
            this.searchCard.Controls.Add(this.btnReset);
            this.searchCard.Controls.Add(this.btnRefresh);
            this.searchCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchCard.Location = new System.Drawing.Point(12, 48);
            this.searchCard.Name = "searchCard";
            this.searchCard.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.searchCard.Size = new System.Drawing.Size(1516, 96);
            this.searchCard.TabIndex = 2;
            //
            // lblRequestDate
            //
            this.lblRequestDate.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblRequestDate.Location = new System.Drawing.Point(12, 12);
            this.lblRequestDate.Name = "lblRequestDate";
            this.lblRequestDate.Size = new System.Drawing.Size(88, 32);
            this.lblRequestDate.TabIndex = 0;
            this.lblRequestDate.Text = "Request Date";
            this.lblRequestDate.Child = null;
            //
            // dtpRequest
            //
            this.dtpRequest.Location = new System.Drawing.Point(104, 12);
            this.dtpRequest.Name = "dtpRequest";
            this.dtpRequest.Required = true;
            this.dtpRequest.ShowPresets = true;
            this.dtpRequest.Size = new System.Drawing.Size(300, 32);
            this.dtpRequest.TabIndex = 1;
            this.dtpRequest.RangeChanged += new System.EventHandler(this.OnRequestRangeChanged);
            this.dtpRequest.Child = null;
            //
            // lblAcceptDate
            //
            this.lblAcceptDate.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblAcceptDate.Location = new System.Drawing.Point(420, 12);
            this.lblAcceptDate.Name = "lblAcceptDate";
            this.lblAcceptDate.Size = new System.Drawing.Size(88, 32);
            this.lblAcceptDate.TabIndex = 2;
            this.lblAcceptDate.Text = "Accept Date";
            this.lblAcceptDate.Child = null;
            //
            // dtpAccept
            //
            this.dtpAccept.Location = new System.Drawing.Point(512, 12);
            this.dtpAccept.Name = "dtpAccept";
            this.dtpAccept.ShowPresets = true;
            this.dtpAccept.Size = new System.Drawing.Size(300, 32);
            this.dtpAccept.TabIndex = 3;
            this.dtpAccept.Child = null;
            //
            // lblReqSerialNo
            //
            this.lblReqSerialNo.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblReqSerialNo.Location = new System.Drawing.Point(12, 52);
            this.lblReqSerialNo.Name = "lblReqSerialNo";
            this.lblReqSerialNo.Size = new System.Drawing.Size(88, 32);
            this.lblReqSerialNo.TabIndex = 4;
            this.lblReqSerialNo.Text = "Request Id";
            this.lblReqSerialNo.Child = null;
            //
            // txtReqSerialNo
            //
            this.txtReqSerialNo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtReqSerialNo.Location = new System.Drawing.Point(104, 52);
            this.txtReqSerialNo.Name = "txtReqSerialNo";
            this.txtReqSerialNo.PlaceholderText = "Request Id (contains)";
            this.txtReqSerialNo.Size = new System.Drawing.Size(281, 32);
            this.txtReqSerialNo.TabIndex = 5;
            this.txtReqSerialNo.EnterPressed += new System.EventHandler(this.OnRefreshClick);
            this.txtReqSerialNo.Child = null;
            //
            // lblState
            //
            this.lblState.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblState.Location = new System.Drawing.Point(420, 52);
            this.lblState.Name = "lblState";
            this.lblState.Size = new System.Drawing.Size(88, 32);
            this.lblState.TabIndex = 6;
            this.lblState.Text = "State Code";
            this.lblState.Child = null;
            //
            // cboState
            //
            this.cboState.Location = new System.Drawing.Point(512, 52);
            this.cboState.Name = "cboState";
            this.cboState.PlaceholderText = "All states";
            this.cboState.Size = new System.Drawing.Size(281, 32);
            this.cboState.TabIndex = 7;
            this.cboState.Child = null;
            //
            // lblRequester
            //
            this.lblRequester.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblRequester.Location = new System.Drawing.Point(828, 52);
            this.lblRequester.Name = "lblRequester";
            this.lblRequester.Size = new System.Drawing.Size(88, 32);
            this.lblRequester.TabIndex = 8;
            this.lblRequester.Text = "Requester";
            this.lblRequester.Child = null;
            //
            // txtRequester
            //
            this.txtRequester.Location = new System.Drawing.Point(920, 52);
            this.txtRequester.Name = "txtRequester";
            this.txtRequester.PlaceholderText = "Requester (contains)";
            this.txtRequester.Size = new System.Drawing.Size(300, 32);
            this.txtRequester.TabIndex = 9;
            this.txtRequester.EnterPressed += new System.EventHandler(this.OnRefreshClick);
            this.txtRequester.Child = null;
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
            this.btnReset.TabIndex = 10;
            this.btnReset.Click += new System.EventHandler(this.OnResetClick);
            this.btnReset.Child = null;
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
            this.btnRefresh.TabIndex = 11;
            this.btnRefresh.Click += new System.EventHandler(this.OnRefreshClick);
            this.btnRefresh.Child = null;
            //
            // spSearch
            //
            this.spSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.spSearch.Location = new System.Drawing.Point(12, 144);
            this.spSearch.Name = "spSearch";
            this.spSearch.Size = new System.Drawing.Size(1516, 8);
            this.spSearch.TabIndex = 3;
            //
            // mainZone
            //
            this.mainZone.Controls.Add(this.splitMain);
            this.mainZone.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainZone.Location = new System.Drawing.Point(12, 152);
            this.mainZone.Name = "mainZone";
            this.mainZone.Size = new System.Drawing.Size(1516, 636);
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
            this.splitMain.Panel1MinSize = 400;
            //
            // splitMain.Panel2
            //
            this.splitMain.Panel2.Controls.Add(this.rightZone);
            //
            // splitMain
            //
            this.splitMain.Panel2MinSize = 480;
            this.splitMain.Size = new System.Drawing.Size(1516, 636);
            this.splitMain.SplitterDistance = 1000;
            this.splitMain.TabIndex = 0;
            //
            // leftZone
            //
            this.leftZone.Controls.Add(this.requestCard);
            this.leftZone.Dock = System.Windows.Forms.DockStyle.Fill;
            this.leftZone.Location = new System.Drawing.Point(0, 0);
            this.leftZone.Name = "leftZone";
            this.leftZone.Size = new System.Drawing.Size(1000, 636);
            this.leftZone.TabIndex = 0;
            //
            // requestCard
            //
            this.requestCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.requestCard.Controls.Add(this.gridRequests);
            this.requestCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.requestCard.Location = new System.Drawing.Point(0, 0);
            this.requestCard.Name = "requestCard";
            this.requestCard.Padding = new System.Windows.Forms.Padding(8, 40, 8, 8);
            this.requestCard.Size = new System.Drawing.Size(1000, 636);
            this.requestCard.TabIndex = 0;
            this.requestCard.Text = "Requests";
            this.requestCard.TitleBar = true;
            this.requestCard.TitleFontSize = 10F;
            //
            // gridRequests
            //
            this.gridRequests.AllowColumnFilters = true;
            this.gridRequests.AutoFitColumns = true;
            this.gridRequests.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridRequests.EmptyText = "No requests";
            this.gridRequests.ExportFileNamePrefix = "Requests";
            this.gridRequests.ExportSheetName = "Requests";
            this.gridRequests.Location = new System.Drawing.Point(8, 40);
            this.gridRequests.Name = "gridRequests";
            this.gridRequests.ShowExportMenu = true;
            this.gridRequests.ShowStatusBar = true;
            this.gridRequests.Size = new System.Drawing.Size(984, 588);
            this.gridRequests.StatusCountFormat = "{0:N0} requests";
            this.gridRequests.TabIndex = 0;
            this.gridRequests.SelectionChanged += new System.EventHandler(this.OnRequestSelectionChanged);
            this.gridRequests.Child = null;
            //
            // rightZone
            //
            this.rightZone.Controls.Add(this.sampleCard);
            this.rightZone.Controls.Add(this.spStatus);
            this.rightZone.Controls.Add(this.statusCard);
            this.rightZone.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightZone.Location = new System.Drawing.Point(0, 0);
            this.rightZone.Name = "rightZone";
            this.rightZone.Size = new System.Drawing.Size(508, 636);
            this.rightZone.TabIndex = 1;
            //
            // statusCard
            //
            this.statusCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.statusCard.Controls.Add(this.badgeState);
            this.statusCard.Controls.Add(this.lblRequestKey);
            this.statusCard.Controls.Add(this.btnReject);
            this.statusCard.Controls.Add(this.btnNextStep);
            this.statusCard.Controls.Add(this.stepStatus);
            this.statusCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.statusCard.Location = new System.Drawing.Point(0, 0);
            this.statusCard.Name = "statusCard";
            this.statusCard.Padding = new System.Windows.Forms.Padding(12, 40, 12, 8);
            this.statusCard.Size = new System.Drawing.Size(508, 152);
            this.statusCard.TabIndex = 0;
            this.statusCard.Text = "Progress";
            this.statusCard.TitleAccent = false;
            this.statusCard.TitleBar = true;
            this.statusCard.TitleFontSize = 10F;
            //
            // badgeState
            //
            this.badgeState.Location = new System.Drawing.Point(12, 44);
            this.badgeState.Name = "badgeState";
            this.badgeState.Semantic = Modern.Lab.Theming.SemanticKind.Neutral;
            this.badgeState.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeState.Size = new System.Drawing.Size(120, 24);
            this.badgeState.TabIndex = 0;
            this.badgeState.Visible = false;
            this.badgeState.Child = null;
            //
            // lblRequestKey
            //
            this.lblRequestKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRequestKey.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblRequestKey.Location = new System.Drawing.Point(140, 46);
            this.lblRequestKey.Name = "lblRequestKey";
            this.lblRequestKey.Size = new System.Drawing.Size(120, 20);
            this.lblRequestKey.TabIndex = 1;
            this.lblRequestKey.Text = "-";
            this.lblRequestKey.Child = null;
            //
            // btnReject
            //
            this.btnReject.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReject.Enabled = false;
            this.btnReject.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Danger;
            this.btnReject.Location = new System.Drawing.Point(268, 40);
            this.btnReject.Name = "btnReject";
            this.btnReject.Size = new System.Drawing.Size(110, 40);
            this.btnReject.TabIndex = 2;
            this.btnReject.Text = "Reject";
            this.btnReject.Click += new System.EventHandler(this.OnRejectClick);
            this.btnReject.Child = null;
            //
            // btnNextStep
            //
            this.btnNextStep.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNextStep.Enabled = false;
            this.btnNextStep.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnNextStep.Location = new System.Drawing.Point(386, 40);
            this.btnNextStep.Name = "btnNextStep";
            this.btnNextStep.Size = new System.Drawing.Size(110, 40);
            this.btnNextStep.TabIndex = 3;
            this.btnNextStep.Text = "No Next Step";
            this.btnNextStep.Click += new System.EventHandler(this.OnNextStepClick);
            this.btnNextStep.Child = null;
            //
            // stepStatus
            //
            this.stepStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stepStatus.DisplayMember = "LABEL";
            this.stepStatus.LabelFontSize = 11D;
            this.stepStatus.Location = new System.Drawing.Point(12, 88);
            this.stepStatus.Name = "stepStatus";
            this.stepStatus.Size = new System.Drawing.Size(484, 56);
            this.stepStatus.StateMember = "STATE";
            this.stepStatus.TabIndex = 4;
            this.stepStatus.Child = null;
            //
            // spStatus
            //
            this.spStatus.Dock = System.Windows.Forms.DockStyle.Top;
            this.spStatus.Location = new System.Drawing.Point(0, 152);
            this.spStatus.Name = "spStatus";
            this.spStatus.Size = new System.Drawing.Size(508, 8);
            this.spStatus.TabIndex = 1;
            //
            // sampleCard
            //
            this.sampleCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.sampleCard.Controls.Add(this.gridSamples);
            this.sampleCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sampleCard.Location = new System.Drawing.Point(0, 160);
            this.sampleCard.Name = "sampleCard";
            this.sampleCard.Padding = new System.Windows.Forms.Padding(8, 40, 8, 8);
            this.sampleCard.Size = new System.Drawing.Size(508, 476);
            this.sampleCard.TabIndex = 2;
            this.sampleCard.Text = "Samples";
            this.sampleCard.TitleBar = true;
            this.sampleCard.TitleFontSize = 10F;
            //
            // gridSamples
            //
            this.gridSamples.AllowColumnFilters = true;
            this.gridSamples.AutoFitColumns = true;
            this.gridSamples.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridSamples.EmptyText = "Select a request";
            this.gridSamples.ExportFileNamePrefix = "Samples";
            this.gridSamples.ExportSheetName = "Samples";
            this.gridSamples.Location = new System.Drawing.Point(8, 40);
            this.gridSamples.Name = "gridSamples";
            this.gridSamples.ShowExportMenu = true;
            this.gridSamples.ShowStatusBar = true;
            this.gridSamples.Size = new System.Drawing.Size(692, 428);
            this.gridSamples.StatusCountFormat = "{0:N0} samples";
            this.gridSamples.TabIndex = 0;
            this.gridSamples.Child = null;
            //
            // RequestManagementForm
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
            this.Name = "RequestManagementForm";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Request Management";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.titlePanel.ResumeLayout(false);
            this.searchCard.ResumeLayout(false);
            this.mainZone.ResumeLayout(false);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.leftZone.ResumeLayout(false);
            this.requestCard.ResumeLayout(false);
            this.rightZone.ResumeLayout(false);
            this.statusCard.ResumeLayout(false);
            this.sampleCard.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel titlePanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblTitle;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeEnv;
        private System.Windows.Forms.Panel spTitle;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel searchCard;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblRequestDate;
        private Modern.Lab.WinForms.Controls.Input.ModernDateRangePicker dtpRequest;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblAcceptDate;
        private Modern.Lab.WinForms.Controls.Input.ModernDateRangePicker dtpAccept;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblReqSerialNo;
        private Modern.Lab.WinForms.Controls.Input.ModernTextBox txtReqSerialNo;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblState;
        private Modern.Lab.WinForms.Controls.Selection.ModernCheckComboBox cboState;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblRequester;
        private Modern.Lab.WinForms.Controls.Input.ModernTextBox txtRequester;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnReset;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnRefresh;
        private System.Windows.Forms.Panel spSearch;
        private System.Windows.Forms.Panel mainZone;
        private Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer splitMain;
        private System.Windows.Forms.Panel leftZone;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox requestCard;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridRequests;
        private System.Windows.Forms.Panel rightZone;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox statusCard;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeState;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblRequestKey;
        private Modern.Lab.WinForms.Controls.Display.ModernStepIndicator stepStatus;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnReject;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnNextStep;
        private System.Windows.Forms.Panel spStatus;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox sampleCard;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridSamples;
    }
}
