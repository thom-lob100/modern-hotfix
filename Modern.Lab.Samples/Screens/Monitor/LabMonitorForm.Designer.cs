namespace Modern.Lab.Samples
{
    public partial class LabMonitorForm
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
            this.lblUpdated = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.toggleBoard = new Modern.Lab.WinForms.Controls.Input.ModernToggleGroup();
            this.badgeEnv = new Modern.Lab.WinForms.Controls.Display.ModernStatusBadge();
            this.spTitle = new System.Windows.Forms.Panel();
            this.flowPanel = new System.Windows.Forms.TableLayoutPanel();
            this.linePanel = new System.Windows.Forms.Panel();
            this.focusPanel = new System.Windows.Forms.Panel();
            this.gridFocus = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.focusHead = new System.Windows.Forms.Panel();
            this.lblFocusTitle = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.btnBack = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.railPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.titlePanel.SuspendLayout();
            this.rightBar.SuspendLayout();
            this.focusPanel.SuspendLayout();
            this.focusHead.SuspendLayout();
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
            this.lblTitle.Size = new System.Drawing.Size(716, 36);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Lab Monitor";
            this.lblTitle.TitleBar = true;
            this.lblTitle.Child = null;
            //
            // rightBar
            //
            this.rightBar.Controls.Add(this.btnRefresh);
            this.rightBar.Controls.Add(this.badgeCountdown);
            this.rightBar.Controls.Add(this.cboInterval);
            this.rightBar.Controls.Add(this.lblInterval);
            this.rightBar.Controls.Add(this.lblUpdated);
            this.rightBar.Controls.Add(this.toggleBoard);
            this.rightBar.Controls.Add(this.badgeEnv);
            this.rightBar.Dock = System.Windows.Forms.DockStyle.Right;
            this.rightBar.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.rightBar.Location = new System.Drawing.Point(716, 0);
            this.rightBar.Margin = new System.Windows.Forms.Padding(0);
            this.rightBar.Name = "rightBar";
            this.rightBar.Size = new System.Drawing.Size(760, 36);
            this.rightBar.TabIndex = 1;
            this.rightBar.WrapContents = false;
            //
            // btnRefresh
            //
            this.btnRefresh.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Primary;
            this.btnRefresh.Location = new System.Drawing.Point(660, 2);
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
            this.badgeCountdown.Location = new System.Drawing.Point(524, 4);
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
            this.cboInterval.Location = new System.Drawing.Point(426, 2);
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
            this.lblInterval.Location = new System.Drawing.Point(370, 2);
            this.lblInterval.Margin = new System.Windows.Forms.Padding(12, 2, 0, 2);
            this.lblInterval.Name = "lblInterval";
            this.lblInterval.Size = new System.Drawing.Size(44, 32);
            this.lblInterval.TabIndex = 3;
            this.lblInterval.Text = "Auto";
            this.lblInterval.Child = null;
            //
            // lblUpdated
            //
            this.lblUpdated.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Helper;
            this.lblUpdated.Location = new System.Drawing.Point(198, 2);
            this.lblUpdated.Margin = new System.Windows.Forms.Padding(12, 2, 0, 2);
            this.lblUpdated.Name = "lblUpdated";
            this.lblUpdated.Size = new System.Drawing.Size(160, 32);
            this.lblUpdated.TabIndex = 4;
            this.lblUpdated.Text = "Updated -";
            this.lblUpdated.Child = null;
            //
            // toggleBoard
            //
            this.toggleBoard.Location = new System.Drawing.Point(78, 2);
            this.toggleBoard.Margin = new System.Windows.Forms.Padding(12, 2, 0, 2);
            this.toggleBoard.Name = "toggleBoard";
            this.toggleBoard.Size = new System.Drawing.Size(120, 32);
            this.toggleBoard.TabIndex = 5;
            this.toggleBoard.SelectedValueChanged += new System.EventHandler(this.OnBoardToggleChanged);
            //
            // badgeEnv
            //
            this.badgeEnv.Location = new System.Drawing.Point(6, 4);
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
            // flowPanel
            //
            this.flowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowPanel.Location = new System.Drawing.Point(12, 56);
            this.flowPanel.Margin = new System.Windows.Forms.Padding(0);
            this.flowPanel.Name = "flowPanel";
            this.flowPanel.Size = new System.Drawing.Size(1476, 748);
            this.flowPanel.TabIndex = 2;
            //
            // linePanel
            //
            this.linePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.linePanel.Location = new System.Drawing.Point(12, 56);
            this.linePanel.Name = "linePanel";
            this.linePanel.Size = new System.Drawing.Size(1476, 748);
            this.linePanel.TabIndex = 3;
            this.linePanel.Visible = false;
            //
            // focusPanel
            //
            this.focusPanel.Controls.Add(this.gridFocus);
            this.focusPanel.Controls.Add(this.focusHead);
            this.focusPanel.Controls.Add(this.railPanel);
            this.focusPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.focusPanel.Location = new System.Drawing.Point(12, 56);
            this.focusPanel.Name = "focusPanel";
            this.focusPanel.Size = new System.Drawing.Size(1476, 748);
            this.focusPanel.TabIndex = 4;
            this.focusPanel.Visible = false;
            //
            // gridFocus
            //
            this.gridFocus.AllowColumnFilters = true;
            this.gridFocus.AutoFitColumns = true;
            this.gridFocus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridFocus.EmptyText = "No rows";
            this.gridFocus.ExportFileNamePrefix = "LabMonitor";
            this.gridFocus.ExportSheetName = "Lab Monitor";
            this.gridFocus.Location = new System.Drawing.Point(160, 40);
            this.gridFocus.Name = "gridFocus";
            this.gridFocus.ShowExportMenu = true;
            this.gridFocus.ShowStatusBar = true;
            this.gridFocus.Size = new System.Drawing.Size(1316, 708);
            this.gridFocus.StatusCountFormat = "{0:N0} rows";
            this.gridFocus.TabIndex = 2;
            this.gridFocus.Child = null;
            //
            // focusHead
            //
            this.focusHead.Controls.Add(this.lblFocusTitle);
            this.focusHead.Controls.Add(this.btnBack);
            this.focusHead.Dock = System.Windows.Forms.DockStyle.Top;
            this.focusHead.Location = new System.Drawing.Point(160, 0);
            this.focusHead.Name = "focusHead";
            this.focusHead.Padding = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.focusHead.Size = new System.Drawing.Size(1316, 40);
            this.focusHead.TabIndex = 1;
            //
            // lblFocusTitle
            //
            this.lblFocusTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFocusTitle.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Title;
            this.lblFocusTitle.Location = new System.Drawing.Point(96, 0);
            this.lblFocusTitle.Name = "lblFocusTitle";
            this.lblFocusTitle.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.lblFocusTitle.Size = new System.Drawing.Size(1220, 32);
            this.lblFocusTitle.TabIndex = 1;
            this.lblFocusTitle.Text = "-";
            this.lblFocusTitle.Child = null;
            //
            // btnBack
            //
            this.btnBack.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnBack.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Subtle;
            this.btnBack.Location = new System.Drawing.Point(0, 0);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(96, 32);
            this.btnBack.TabIndex = 0;
            this.btnBack.Text = "Back";
            this.btnBack.Click += new System.EventHandler(this.OnBackClick);
            this.btnBack.Child = null;
            //
            // railPanel
            //
            this.railPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.railPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.railPanel.Location = new System.Drawing.Point(0, 0);
            this.railPanel.Name = "railPanel";
            this.railPanel.Padding = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.railPanel.Size = new System.Drawing.Size(160, 748);
            this.railPanel.TabIndex = 0;
            this.railPanel.WrapContents = false;
            //
            // LabMonitorForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(244)))), ((int)(((byte)(246)))));
            this.ClientSize = new System.Drawing.Size(1500, 816);
            this.Controls.Add(this.focusPanel);
            this.Controls.Add(this.linePanel);
            this.Controls.Add(this.flowPanel);
            this.Controls.Add(this.spTitle);
            this.Controls.Add(this.titlePanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(1240, 700);
            this.Name = "LabMonitorForm";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Lab Monitor";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.titlePanel.ResumeLayout(false);
            this.rightBar.ResumeLayout(false);
            this.focusPanel.ResumeLayout(false);
            this.focusHead.ResumeLayout(false);
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
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblUpdated;
        private Modern.Lab.WinForms.Controls.Input.ModernToggleGroup toggleBoard;
        private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeEnv;
        private System.Windows.Forms.Panel spTitle;
        private System.Windows.Forms.TableLayoutPanel flowPanel;
        private System.Windows.Forms.Panel linePanel;
        private System.Windows.Forms.Panel focusPanel;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridFocus;
        private System.Windows.Forms.Panel focusHead;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblFocusTitle;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnBack;
        private System.Windows.Forms.FlowLayoutPanel railPanel;
    }
}
