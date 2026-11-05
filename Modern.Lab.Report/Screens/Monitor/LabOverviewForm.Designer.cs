namespace Modern.Lab.Report
{
    public partial class LabOverviewForm
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
            if (disposing)
            {
                this.refreshTimer.Dispose();

                if (this.components != null)
                {
                    this.components.Dispose();
                }
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
            this.rightBar = new System.Windows.Forms.FlowLayoutPanel();
            this.btnRefresh = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.badgeCountdown = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.cboInterval = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.lblInterval = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.badgeFailed = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.lblUpdated = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.badgeEnv = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.spTitle = new System.Windows.Forms.Panel();
            this.bodyLayout = new System.Windows.Forms.TableLayoutPanel();
            this.mapCard = new System.Windows.Forms.Panel();
            this.mapProcess = new Modern.Lab.WinForms.Controls.Display.ModernProcessMap();
            this.lblMapTitle = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.attentionCard = new System.Windows.Forms.Panel();
            this.listAttention = new Modern.Lab.WinForms.Controls.Display.ModernQueueList();
            this.attentionHead = new System.Windows.Forms.Panel();
            this.lblAttentionTitle = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.lblAttentionCount = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.bottomLayout = new System.Windows.Forms.TableLayoutPanel();
            this.storageCard = new System.Windows.Forms.Panel();
            this.storageLayout = new System.Windows.Forms.TableLayoutPanel();
            this.kpiStb = new Modern.Lab.WinForms.Controls.Display.ModernKpiCard();
            this.kpiStored = new Modern.Lab.WinForms.Controls.Display.ModernKpiCard();
            this.kpiTrays = new Modern.Lab.WinForms.Controls.Display.ModernKpiCard();
            this.lblStorageTitle = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.selectedCard = new System.Windows.Forms.Panel();
            this.fieldSelected = new Modern.Lab.WinForms.Controls.Display.ModernFieldList();
            this.lblSelectedTitle = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.titlePanel.SuspendLayout();
            this.rightBar.SuspendLayout();
            this.bodyLayout.SuspendLayout();
            this.mapCard.SuspendLayout();
            this.attentionCard.SuspendLayout();
            this.attentionHead.SuspendLayout();
            this.bottomLayout.SuspendLayout();
            this.storageCard.SuspendLayout();
            this.storageLayout.SuspendLayout();
            this.selectedCard.SuspendLayout();
            this.SuspendLayout();
            //
            // titlePanel
            //
            this.titlePanel.Controls.Add(this.lblTitle);
            this.titlePanel.Controls.Add(this.rightBar);
            this.titlePanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.titlePanel.Location = new System.Drawing.Point(12, 12);
            this.titlePanel.Name = "titlePanel";
            this.titlePanel.Size = new System.Drawing.Size(1476, 36);
            this.titlePanel.TabIndex = 0;
            //
            // lblTitle
            //
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Title;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(656, 36);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Lab Overview";
            this.lblTitle.TitleBar = true;
            this.lblTitle.Child = null;
            //
            // rightBar
            //
            this.rightBar.Controls.Add(this.btnRefresh);
            this.rightBar.Controls.Add(this.badgeCountdown);
            this.rightBar.Controls.Add(this.cboInterval);
            this.rightBar.Controls.Add(this.lblInterval);
            this.rightBar.Controls.Add(this.badgeFailed);
            this.rightBar.Controls.Add(this.lblUpdated);
            this.rightBar.Controls.Add(this.badgeEnv);
            this.rightBar.Dock = System.Windows.Forms.DockStyle.Right;
            this.rightBar.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.rightBar.Location = new System.Drawing.Point(656, 0);
            this.rightBar.Margin = new System.Windows.Forms.Padding(0);
            this.rightBar.Name = "rightBar";
            this.rightBar.Size = new System.Drawing.Size(820, 36);
            this.rightBar.TabIndex = 1;
            this.rightBar.WrapContents = false;
            //
            // btnRefresh
            //
            this.btnRefresh.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Primary;
            this.btnRefresh.Location = new System.Drawing.Point(720, 2);
            this.btnRefresh.Margin = new System.Windows.Forms.Padding(8, 2, 0, 2);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(100, 32);
            this.btnRefresh.TabIndex = 0;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.Click += new System.EventHandler(this.OnRefreshClick);
            this.btnRefresh.Child = null;
            //
            // badgeCountdown
            //
            this.badgeCountdown.Location = new System.Drawing.Point(584, 4);
            this.badgeCountdown.Margin = new System.Windows.Forms.Padding(8, 4, 0, 4);
            this.badgeCountdown.Name = "badgeCountdown";
            this.badgeCountdown.Semantic = Modern.Lab.Theming.SemanticKind.Warning;
            this.badgeCountdown.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeCountdown.Size = new System.Drawing.Size(120, 28);
            this.badgeCountdown.TabIndex = 1;
            this.badgeCountdown.Text = "-";
            this.badgeCountdown.Visible = false;
            this.badgeCountdown.Child = null;
            //
            // cboInterval
            //
            this.cboInterval.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboInterval.Location = new System.Drawing.Point(486, 2);
            this.cboInterval.Margin = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.cboInterval.Name = "cboInterval";
            this.cboInterval.Size = new System.Drawing.Size(90, 32);
            this.cboInterval.TabIndex = 2;
            this.cboInterval.SelectedIndexChanged += new System.EventHandler(this.OnIntervalChanged);
            this.cboInterval.Child = null;
            //
            // lblInterval
            //
            this.lblInterval.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblInterval.Location = new System.Drawing.Point(430, 2);
            this.lblInterval.Margin = new System.Windows.Forms.Padding(12, 2, 0, 2);
            this.lblInterval.Name = "lblInterval";
            this.lblInterval.Size = new System.Drawing.Size(44, 32);
            this.lblInterval.TabIndex = 3;
            this.lblInterval.Text = "Auto";
            this.lblInterval.Child = null;
            //
            // badgeFailed
            //
            this.badgeFailed.Location = new System.Drawing.Point(248, 4);
            this.badgeFailed.Margin = new System.Windows.Forms.Padding(8, 4, 0, 4);
            this.badgeFailed.Name = "badgeFailed";
            this.badgeFailed.Semantic = Modern.Lab.Theming.SemanticKind.Error;
            this.badgeFailed.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeFailed.Size = new System.Drawing.Size(170, 28);
            this.badgeFailed.TabIndex = 4;
            this.badgeFailed.Text = "Refresh failed";
            this.badgeFailed.Visible = false;
            this.badgeFailed.Child = null;
            //
            // lblUpdated
            //
            this.lblUpdated.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Helper;
            this.lblUpdated.Location = new System.Drawing.Point(76, 2);
            this.lblUpdated.Margin = new System.Windows.Forms.Padding(12, 2, 0, 2);
            this.lblUpdated.Name = "lblUpdated";
            this.lblUpdated.Size = new System.Drawing.Size(160, 32);
            this.lblUpdated.TabIndex = 5;
            this.lblUpdated.Text = "Updated -";
            this.lblUpdated.Child = null;
            //
            // badgeEnv
            //
            this.badgeEnv.Location = new System.Drawing.Point(10, 4);
            this.badgeEnv.Margin = new System.Windows.Forms.Padding(6, 4, 0, 4);
            this.badgeEnv.Name = "badgeEnv";
            this.badgeEnv.Semantic = Modern.Lab.Theming.SemanticKind.Info;
            this.badgeEnv.Shape = Modern.Lab.WinForms.Controls.Display.BadgeShape.Rounded;
            this.badgeEnv.Size = new System.Drawing.Size(60, 28);
            this.badgeEnv.TabIndex = 6;
            this.badgeEnv.Text = "AOS";
            this.badgeEnv.Child = null;
            //
            // spTitle
            //
            this.spTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.spTitle.Location = new System.Drawing.Point(12, 48);
            this.spTitle.Name = "spTitle";
            this.spTitle.Size = new System.Drawing.Size(1476, 8);
            this.spTitle.TabIndex = 1;
            //
            // bodyLayout
            //
            this.bodyLayout.ColumnCount = 2;
            this.bodyLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bodyLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 340F));
            this.bodyLayout.Controls.Add(this.mapCard, 0, 0);
            this.bodyLayout.Controls.Add(this.bottomLayout, 0, 1);
            this.bodyLayout.Controls.Add(this.attentionCard, 1, 0);
            this.bodyLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bodyLayout.Location = new System.Drawing.Point(12, 56);
            this.bodyLayout.Margin = new System.Windows.Forms.Padding(0);
            this.bodyLayout.Name = "bodyLayout";
            this.bodyLayout.RowCount = 2;
            this.bodyLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bodyLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 168F));
            this.bodyLayout.SetRowSpan(this.attentionCard, 2);
            this.bodyLayout.Size = new System.Drawing.Size(1476, 772);
            this.bodyLayout.TabIndex = 2;
            //
            // mapCard
            //
            this.mapCard.Controls.Add(this.mapProcess);
            this.mapCard.Controls.Add(this.lblMapTitle);
            this.mapCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mapCard.Location = new System.Drawing.Point(0, 0);
            this.mapCard.Margin = new System.Windows.Forms.Padding(0, 0, 12, 12);
            this.mapCard.Name = "mapCard";
            this.mapCard.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.mapCard.Size = new System.Drawing.Size(1124, 592);
            this.mapCard.TabIndex = 0;
            //
            // mapProcess
            //
            this.mapProcess.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mapProcess.Location = new System.Drawing.Point(12, 34);
            this.mapProcess.Name = "mapProcess";
            this.mapProcess.Size = new System.Drawing.Size(1100, 550);
            this.mapProcess.TabIndex = 1;
            this.mapProcess.Child = null;
            //
            // lblMapTitle
            //
            this.lblMapTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblMapTitle.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblMapTitle.Location = new System.Drawing.Point(12, 8);
            this.lblMapTitle.Name = "lblMapTitle";
            this.lblMapTitle.Size = new System.Drawing.Size(1100, 26);
            this.lblMapTitle.TabIndex = 0;
            this.lblMapTitle.Text = "Process map";
            this.lblMapTitle.Child = null;
            //
            // attentionCard
            //
            this.attentionCard.Controls.Add(this.listAttention);
            this.attentionCard.Controls.Add(this.attentionHead);
            this.attentionCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.attentionCard.Location = new System.Drawing.Point(1136, 0);
            this.attentionCard.Margin = new System.Windows.Forms.Padding(0);
            this.attentionCard.Name = "attentionCard";
            this.attentionCard.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.attentionCard.Size = new System.Drawing.Size(340, 772);
            this.attentionCard.TabIndex = 2;
            //
            // listAttention
            //
            this.listAttention.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listAttention.EmptyText = "Not loaded";
            this.listAttention.Location = new System.Drawing.Point(10, 34);
            this.listAttention.Name = "listAttention";
            this.listAttention.Size = new System.Drawing.Size(320, 730);
            this.listAttention.TabIndex = 1;
            this.listAttention.TwoLineBelowWidth = 300;
            this.listAttention.Child = null;
            //
            // attentionHead
            //
            this.attentionHead.Controls.Add(this.lblAttentionTitle);
            this.attentionHead.Controls.Add(this.lblAttentionCount);
            this.attentionHead.Dock = System.Windows.Forms.DockStyle.Top;
            this.attentionHead.Location = new System.Drawing.Point(10, 8);
            this.attentionHead.Name = "attentionHead";
            this.attentionHead.Size = new System.Drawing.Size(320, 26);
            this.attentionHead.TabIndex = 0;
            //
            // lblAttentionTitle
            //
            this.lblAttentionTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAttentionTitle.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblAttentionTitle.Location = new System.Drawing.Point(0, 0);
            this.lblAttentionTitle.Name = "lblAttentionTitle";
            this.lblAttentionTitle.Size = new System.Drawing.Size(230, 26);
            this.lblAttentionTitle.TabIndex = 0;
            this.lblAttentionTitle.Text = "Attention";
            this.lblAttentionTitle.Child = null;
            //
            // lblAttentionCount
            //
            this.lblAttentionCount.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblAttentionCount.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Helper;
            this.lblAttentionCount.Location = new System.Drawing.Point(230, 0);
            this.lblAttentionCount.Name = "lblAttentionCount";
            this.lblAttentionCount.Size = new System.Drawing.Size(90, 26);
            this.lblAttentionCount.TabIndex = 1;
            this.lblAttentionCount.Text = "-";
            this.lblAttentionCount.TextAlignment = System.Windows.Forms.HorizontalAlignment.Right;
            this.lblAttentionCount.Child = null;
            //
            // bottomLayout
            //
            this.bottomLayout.ColumnCount = 2;
            this.bottomLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 42F));
            this.bottomLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 58F));
            this.bottomLayout.Controls.Add(this.storageCard, 0, 0);
            this.bottomLayout.Controls.Add(this.selectedCard, 1, 0);
            this.bottomLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bottomLayout.Location = new System.Drawing.Point(0, 604);
            this.bottomLayout.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.bottomLayout.Name = "bottomLayout";
            this.bottomLayout.RowCount = 1;
            this.bottomLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bottomLayout.Size = new System.Drawing.Size(1124, 168);
            this.bottomLayout.TabIndex = 1;
            //
            // storageCard
            //
            this.storageCard.Controls.Add(this.storageLayout);
            this.storageCard.Controls.Add(this.lblStorageTitle);
            this.storageCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.storageCard.Location = new System.Drawing.Point(0, 0);
            this.storageCard.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.storageCard.Name = "storageCard";
            this.storageCard.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.storageCard.Size = new System.Drawing.Size(460, 168);
            this.storageCard.TabIndex = 0;
            //
            // storageLayout
            //
            this.storageLayout.ColumnCount = 3;
            this.storageLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.storageLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.storageLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.storageLayout.Controls.Add(this.kpiStb, 0, 0);
            this.storageLayout.Controls.Add(this.kpiStored, 1, 0);
            this.storageLayout.Controls.Add(this.kpiTrays, 2, 0);
            this.storageLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.storageLayout.Location = new System.Drawing.Point(12, 34);
            this.storageLayout.Margin = new System.Windows.Forms.Padding(0);
            this.storageLayout.Name = "storageLayout";
            this.storageLayout.RowCount = 1;
            this.storageLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.storageLayout.Size = new System.Drawing.Size(436, 126);
            this.storageLayout.TabIndex = 1;
            //
            // kpiStb
            //
            this.kpiStb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiStb.Flat = true;
            this.kpiStb.Location = new System.Drawing.Point(0, 0);
            this.kpiStb.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.kpiStb.Name = "kpiStb";
            this.kpiStb.Size = new System.Drawing.Size(137, 126);
            this.kpiStb.TabIndex = 0;
            this.kpiStb.Title = "STB FOUP";
            this.kpiStb.Value = "-";
            this.kpiStb.Child = null;
            //
            // kpiStored
            //
            this.kpiStored.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiStored.Flat = true;
            this.kpiStored.Location = new System.Drawing.Point(145, 0);
            this.kpiStored.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.kpiStored.Name = "kpiStored";
            this.kpiStored.Size = new System.Drawing.Size(137, 126);
            this.kpiStored.TabIndex = 1;
            this.kpiStored.Title = "Stored FOUP";
            this.kpiStored.Value = "-";
            this.kpiStored.Child = null;
            //
            // kpiTrays
            //
            this.kpiTrays.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiTrays.Flat = true;
            this.kpiTrays.Location = new System.Drawing.Point(290, 0);
            this.kpiTrays.Margin = new System.Windows.Forms.Padding(0);
            this.kpiTrays.Name = "kpiTrays";
            this.kpiTrays.Size = new System.Drawing.Size(146, 126);
            this.kpiTrays.TabIndex = 2;
            this.kpiTrays.Title = "Stocker trays";
            this.kpiTrays.Value = "-";
            this.kpiTrays.Child = null;
            //
            // lblStorageTitle
            //
            this.lblStorageTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblStorageTitle.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblStorageTitle.Location = new System.Drawing.Point(12, 8);
            this.lblStorageTitle.Name = "lblStorageTitle";
            this.lblStorageTitle.Size = new System.Drawing.Size(436, 26);
            this.lblStorageTitle.TabIndex = 0;
            this.lblStorageTitle.Text = "Storage";
            this.lblStorageTitle.Child = null;
            //
            // selectedCard
            //
            this.selectedCard.Controls.Add(this.fieldSelected);
            this.selectedCard.Controls.Add(this.lblSelectedTitle);
            this.selectedCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectedCard.Location = new System.Drawing.Point(472, 0);
            this.selectedCard.Margin = new System.Windows.Forms.Padding(0);
            this.selectedCard.Name = "selectedCard";
            this.selectedCard.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.selectedCard.Size = new System.Drawing.Size(652, 168);
            this.selectedCard.TabIndex = 1;
            //
            // fieldSelected
            //
            this.fieldSelected.Columns = 2;
            this.fieldSelected.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fieldSelected.LabelPlacement = Modern.Lab.WinForms.Controls.Display.FieldLabelPlacement.Left;
            this.fieldSelected.LabelWidth = 86;
            this.fieldSelected.Location = new System.Drawing.Point(12, 34);
            this.fieldSelected.Name = "fieldSelected";
            this.fieldSelected.RowHeight = 26;
            this.fieldSelected.Size = new System.Drawing.Size(628, 126);
            this.fieldSelected.TabIndex = 1;
            //
            // lblSelectedTitle
            //
            this.lblSelectedTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSelectedTitle.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblSelectedTitle.Location = new System.Drawing.Point(12, 8);
            this.lblSelectedTitle.Name = "lblSelectedTitle";
            this.lblSelectedTitle.Size = new System.Drawing.Size(628, 26);
            this.lblSelectedTitle.TabIndex = 0;
            this.lblSelectedTitle.Text = "Selected";
            this.lblSelectedTitle.Child = null;
            //
            // LabOverviewForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(244)))), ((int)(((byte)(246)))));
            this.ClientSize = new System.Drawing.Size(1500, 840);
            this.Controls.Add(this.bodyLayout);
            this.Controls.Add(this.spTitle);
            this.Controls.Add(this.titlePanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(940, 600);
            this.Name = "LabOverviewForm";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Lab Overview";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.titlePanel.ResumeLayout(false);
            this.rightBar.ResumeLayout(false);
            this.bodyLayout.ResumeLayout(false);
            this.mapCard.ResumeLayout(false);
            this.attentionCard.ResumeLayout(false);
            this.attentionHead.ResumeLayout(false);
            this.bottomLayout.ResumeLayout(false);
            this.storageCard.ResumeLayout(false);
            this.storageLayout.ResumeLayout(false);
            this.selectedCard.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel titlePanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblTitle;
        private System.Windows.Forms.FlowLayoutPanel rightBar;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnRefresh;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeCountdown;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboInterval;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblInterval;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeFailed;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblUpdated;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeEnv;
        private System.Windows.Forms.Panel spTitle;
        private System.Windows.Forms.TableLayoutPanel bodyLayout;
        private System.Windows.Forms.Panel mapCard;
        private Modern.Lab.WinForms.Controls.Display.ModernProcessMap mapProcess;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblMapTitle;
        private System.Windows.Forms.Panel attentionCard;
        private Modern.Lab.WinForms.Controls.Display.ModernQueueList listAttention;
        private System.Windows.Forms.Panel attentionHead;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblAttentionTitle;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblAttentionCount;
        private System.Windows.Forms.TableLayoutPanel bottomLayout;
        private System.Windows.Forms.Panel storageCard;
        private System.Windows.Forms.TableLayoutPanel storageLayout;
        private Modern.Lab.WinForms.Controls.Display.ModernKpiCard kpiStb;
        private Modern.Lab.WinForms.Controls.Display.ModernKpiCard kpiStored;
        private Modern.Lab.WinForms.Controls.Display.ModernKpiCard kpiTrays;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblStorageTitle;
        private System.Windows.Forms.Panel selectedCard;
        private Modern.Lab.WinForms.Controls.Display.ModernFieldList fieldSelected;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblSelectedTitle;
    }
}
