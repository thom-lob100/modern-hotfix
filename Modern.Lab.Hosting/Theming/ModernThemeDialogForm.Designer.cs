namespace Modern.Lab.Hosting.Theming
{
    public partial class ModernThemeDialogForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null) { this.components.Dispose(); }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblTitle = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.lblNote = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.scrollHost = new System.Windows.Forms.Panel();
            this.grpLight = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.lightTable = new System.Windows.Forms.TableLayoutPanel();
            this.groupSpacer = new System.Windows.Forms.Panel();
            this.grpDark = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.darkTable = new System.Windows.Forms.TableLayoutPanel();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.lblPath = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.btnSave = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnClose = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.scrollHost.SuspendLayout();
            this.grpLight.SuspendLayout();
            this.grpDark.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // lblTitle
            //
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Height = 32;
            this.lblTitle.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Title;
            this.lblTitle.TitleBar = true;
            this.lblTitle.Text = "Choose a theme";
            //
            // lblNote
            //
            this.lblNote.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblNote.Height = 24;
            this.lblNote.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Helper;
            this.lblNote.Text = "Applies the next time the program starts.";
            //
            // scrollHost
            //
            this.scrollHost.AutoScroll = true;
            this.scrollHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scrollHost.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.scrollHost.Controls.Add(this.grpDark);
            this.scrollHost.Controls.Add(this.groupSpacer);
            this.scrollHost.Controls.Add(this.grpLight);
            this.scrollHost.TabIndex = 0;
            //
            // grpLight
            //
            this.grpLight.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpLight.Height = 228;
            this.grpLight.Text = "Light";
            this.grpLight.TitleBar = true;
            this.grpLight.Controls.Add(this.lightTable);
            this.grpLight.TabIndex = 0;
            //
            // lightTable
            //
            this.lightTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lightTable.ColumnCount = 4;
            this.lightTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.lightTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.lightTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.lightTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.lightTable.RowCount = 1;
            this.lightTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 88F));
            this.lightTable.Margin = new System.Windows.Forms.Padding(0);
            //
            // groupSpacer
            //
            this.groupSpacer.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupSpacer.Height = 8;
            //
            // grpDark
            //
            this.grpDark.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpDark.Height = 228;
            this.grpDark.Text = "Dark";
            this.grpDark.TitleBar = true;
            this.grpDark.Controls.Add(this.darkTable);
            this.grpDark.TabIndex = 1;
            //
            // darkTable
            //
            this.darkTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.darkTable.ColumnCount = 4;
            this.darkTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.darkTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.darkTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.darkTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.darkTable.RowCount = 1;
            this.darkTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 88F));
            this.darkTable.Margin = new System.Windows.Forms.Padding(0);
            //
            // buttonPanel
            //
            this.buttonPanel.Controls.Add(this.lblPath);
            this.buttonPanel.Controls.Add(this.btnSave);
            this.buttonPanel.Controls.Add(this.btnClose);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonPanel.Size = new System.Drawing.Size(608, 48);
            this.buttonPanel.TabIndex = 1;
            //
            // lblPath
            //
            this.lblPath.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.lblPath.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Helper;
            this.lblPath.Location = new System.Drawing.Point(0, 18);
            this.lblPath.Size = new System.Drawing.Size(384, 22);
            this.lblPath.Text = "";
            //
            // btnSave
            //
            this.btnSave.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnSave.Location = new System.Drawing.Point(400, 12);
            this.btnSave.Size = new System.Drawing.Size(104, 32);
            this.btnSave.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnSave.Text = "Save";
            this.btnSave.TabIndex = 0;
            this.btnSave.Click += new System.EventHandler(this.OnSaveClick);
            //
            // btnClose
            //
            this.btnClose.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Subtle;
            this.btnClose.Location = new System.Drawing.Point(512, 12);
            this.btnClose.Size = new System.Drawing.Size(96, 32);
            this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnClose.Text = "Close";
            this.btnClose.TabIndex = 1;
            this.btnClose.Click += new System.EventHandler(this.OnCloseClick);
            //
            // ModernThemeDialogForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 620);
            this.Padding = new System.Windows.Forms.Padding(16);
            this.Controls.Add(this.scrollHost);
            this.Controls.Add(this.lblNote);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.buttonPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Theme";
            this.Name = "ModernThemeDialogForm";
            this.AcceptButton = this.btnSave;
            this.CancelButton = this.btnClose;
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.scrollHost.ResumeLayout(false);
            this.grpLight.ResumeLayout(false);
            this.grpDark.ResumeLayout(false);
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblTitle;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblNote;
        private System.Windows.Forms.Panel scrollHost;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox grpLight;
        private System.Windows.Forms.TableLayoutPanel lightTable;
        private System.Windows.Forms.Panel groupSpacer;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox grpDark;
        private System.Windows.Forms.TableLayoutPanel darkTable;
        private System.Windows.Forms.Panel buttonPanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblPath;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnSave;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnClose;
    }
}
