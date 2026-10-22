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
            this.inputPanel = new System.Windows.Forms.TableLayoutPanel();
            this.lblTheme = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboTheme = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.lblPathCaption = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.lblPath = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.lblNote = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.btnSave = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnClose = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.inputPanel.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // inputPanel
            //
            this.inputPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputPanel.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.inputPanel.ColumnCount = 2;
            this.inputPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 96F));
            this.inputPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.inputPanel.RowCount = 3;
            this.inputPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.inputPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.inputPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.inputPanel.Controls.Add(this.lblTheme, 0, 0);
            this.inputPanel.Controls.Add(this.cboTheme, 1, 0);
            this.inputPanel.Controls.Add(this.lblPathCaption, 0, 1);
            this.inputPanel.Controls.Add(this.lblPath, 1, 1);
            this.inputPanel.Controls.Add(this.lblNote, 1, 2);
            this.inputPanel.TabIndex = 0;
            //
            // lblTheme
            //
            this.lblTheme.Text = "Theme";
            this.lblTheme.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblTheme.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblTheme.Size = new System.Drawing.Size(88, 24);
            //
            // cboTheme
            //
            this.cboTheme.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.cboTheme.Height = 32;
            this.cboTheme.TabIndex = 0;
            this.cboTheme.PlaceholderText = "Select a theme";
            //
            // lblPathCaption
            //
            this.lblPathCaption.Text = "Saved to";
            this.lblPathCaption.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblPathCaption.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblPathCaption.Size = new System.Drawing.Size(88, 24);
            //
            // lblPath
            //
            this.lblPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPath.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Helper;
            this.lblPath.Text = "";
            //
            // lblNote
            //
            this.lblNote.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNote.Text = "Applies the next time the program starts.";
            //
            // buttonPanel
            //
            this.buttonPanel.Controls.Add(this.btnSave);
            this.buttonPanel.Controls.Add(this.btnClose);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonPanel.Size = new System.Drawing.Size(488, 48);
            this.buttonPanel.TabIndex = 1;
            //
            // btnSave
            //
            this.btnSave.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnSave.Location = new System.Drawing.Point(280, 12);
            this.btnSave.Size = new System.Drawing.Size(104, 32);
            this.btnSave.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnSave.Text = "Save";
            this.btnSave.TabIndex = 0;
            this.btnSave.Click += new System.EventHandler(this.OnSaveClick);
            //
            // btnClose
            //
            this.btnClose.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Subtle;
            this.btnClose.Location = new System.Drawing.Point(392, 12);
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
            this.ClientSize = new System.Drawing.Size(520, 220);
            this.Padding = new System.Windows.Forms.Padding(16);
            this.Controls.Add(this.inputPanel);
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
            this.inputPanel.ResumeLayout(false);
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TableLayoutPanel inputPanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblTheme;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboTheme;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblPathCaption;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblPath;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblNote;
        private System.Windows.Forms.Panel buttonPanel;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnSave;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnClose;
    }
}
