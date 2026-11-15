namespace Modern.Lab.Samples
{
    public partial class JudgeDialogForm
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
            this.sourceCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.fieldSource = new Modern.Lab.WinForms.Controls.Display.ModernFieldList();
            this.headerPanel = new System.Windows.Forms.Panel();
            this.inputPanel = new System.Windows.Forms.TableLayoutPanel();
            this.lblDescription = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.txtDescription = new Modern.Lab.WinForms.Controls.Input.ModernTextBox();
            this.targetPanel = new System.Windows.Forms.TableLayoutPanel();
            this.lblTarget = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboTarget = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.judgeCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.gridJudge = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.lblStatus = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.btnOk = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnCancel = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.sourceCard.SuspendLayout();
            this.headerPanel.SuspendLayout();
            this.inputPanel.SuspendLayout();
            this.targetPanel.SuspendLayout();
            this.judgeCard.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // sourceCard
            //
            this.sourceCard.Controls.Add(this.fieldSource);
            this.sourceCard.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.sourceCard.Padding = new System.Windows.Forms.Padding(12, 40, 12, 8);
            this.sourceCard.Height = 168;
            this.sourceCard.Name = "sourceCard";
            this.sourceCard.Text = "Job";
            this.sourceCard.TitleBar = true;
            this.sourceCard.TabIndex = 2;
            //
            // fieldSource
            //
            this.fieldSource.Columns = 4;
            this.fieldSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fieldSource.Name = "fieldSource";
            this.fieldSource.TabIndex = 0;
            //
            // headerPanel
            //
            this.headerPanel.Controls.Add(this.inputPanel);
            this.headerPanel.Controls.Add(this.targetPanel);
            this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerPanel.Height = 52;
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.headerPanel.TabIndex = 0;
            //
            // inputPanel
            //
            this.inputPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputPanel.Name = "inputPanel";
            this.inputPanel.ColumnCount = 2;
            this.inputPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 112F));
            this.inputPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.inputPanel.RowCount = 1;
            this.inputPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.inputPanel.Controls.Add(this.lblDescription, 0, 0);
            this.inputPanel.Controls.Add(this.txtDescription, 1, 0);
            this.inputPanel.TabIndex = 1;
            //
            // lblDescription
            //
            this.lblDescription.Text = "Description";
            this.lblDescription.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblDescription.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(104, 24);
            //
            // txtDescription
            //
            this.txtDescription.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.txtDescription.Height = 32;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.PlaceholderText = "Description (optional)";
            this.txtDescription.TabIndex = 0;
            //
            // targetPanel
            //
            this.targetPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.targetPanel.Width = 408;
            this.targetPanel.Name = "targetPanel";
            this.targetPanel.Padding = new System.Windows.Forms.Padding(0, 0, 16, 0);
            this.targetPanel.ColumnCount = 2;
            this.targetPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 112F));
            this.targetPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.targetPanel.RowCount = 1;
            this.targetPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.targetPanel.Controls.Add(this.lblTarget, 0, 0);
            this.targetPanel.Controls.Add(this.cboTarget, 1, 0);
            this.targetPanel.TabIndex = 0;
            this.targetPanel.Visible = false;
            //
            // lblTarget
            //
            this.lblTarget.Text = "Durable(Tray)";
            this.lblTarget.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblTarget.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblTarget.Name = "lblTarget";
            this.lblTarget.Size = new System.Drawing.Size(104, 24);
            //
            // cboTarget
            //
            this.cboTarget.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cboTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTarget.Name = "cboTarget";
            this.cboTarget.Size = new System.Drawing.Size(280, 32);
            this.cboTarget.TabIndex = 0;
            this.cboTarget.SelectedIndexChanged += new System.EventHandler(this.OnTargetChanged);
            this.cboTarget.Child = null;
            //
            // judgeCard
            //
            this.judgeCard.Controls.Add(this.gridJudge);
            this.judgeCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.judgeCard.Padding = new System.Windows.Forms.Padding(12, 40, 12, 12);
            this.judgeCard.Name = "judgeCard";
            this.judgeCard.Text = "Judge";
            this.judgeCard.TitleBar = true;
            this.judgeCard.TabIndex = 1;
            //
            // gridJudge
            //
            this.gridJudge.AllowColumnFilters = false;
            this.gridJudge.AutoFitColumns = true;
            this.gridJudge.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridJudge.EmptyText = "Loading judge rows…";
            this.gridJudge.Name = "gridJudge";
            this.gridJudge.ReadOnly = false;
            this.gridJudge.TabIndex = 0;
            this.gridJudge.Leave += new System.EventHandler(this.OnCommitPendingEdit);
            this.gridJudge.Child = null;
            //
            // buttonPanel
            //
            this.buttonPanel.Controls.Add(this.lblStatus);
            this.buttonPanel.Controls.Add(this.btnOk);
            this.buttonPanel.Controls.Add(this.btnCancel);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(1168, 48);
            this.buttonPanel.TabIndex = 3;
            this.buttonPanel.MouseEnter += new System.EventHandler(this.OnCommitPendingEdit);
            //
            // lblStatus
            //
            this.lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.lblStatus.Location = new System.Drawing.Point(0, 16);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(940, 24);
            this.lblStatus.Text = "Loading judge rows…";
            //
            // btnOk
            //
            this.btnOk.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnOk.Location = new System.Drawing.Point(960, 12);
            this.btnOk.Size = new System.Drawing.Size(104, 32);
            this.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnOk.Name = "btnOk";
            this.btnOk.Text = "OK";
            this.btnOk.TabIndex = 0;
            this.btnOk.Enabled = false;
            this.btnOk.Click += new System.EventHandler(this.OnOkClick);
            this.btnOk.Child = null;
            //
            // btnCancel
            //
            this.btnCancel.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Subtle;
            this.btnCancel.Location = new System.Drawing.Point(1072, 12);
            this.btnCancel.Size = new System.Drawing.Size(96, 32);
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Text = "Cancel";
            this.btnCancel.TabIndex = 1;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Child = null;
            //
            // JudgeDialogForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 680);
            this.Padding = new System.Windows.Forms.Padding(16);
            this.Controls.Add(this.judgeCard);
            this.Controls.Add(this.sourceCard);
            this.Controls.Add(this.headerPanel);
            this.Controls.Add(this.buttonPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Judge";
            this.Name = "JudgeDialogForm";
            this.AcceptButton = this.btnOk;
            this.CancelButton = this.btnCancel;
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.sourceCard.ResumeLayout(false);
            this.inputPanel.ResumeLayout(false);
            this.targetPanel.ResumeLayout(false);
            this.headerPanel.ResumeLayout(false);
            this.judgeCard.ResumeLayout(false);
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox sourceCard;
        private Modern.Lab.WinForms.Controls.Display.ModernFieldList fieldSource;
        private System.Windows.Forms.Panel headerPanel;
        private System.Windows.Forms.TableLayoutPanel inputPanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblDescription;
        private Modern.Lab.WinForms.Controls.Input.ModernTextBox txtDescription;
        private System.Windows.Forms.TableLayoutPanel targetPanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblTarget;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboTarget;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox judgeCard;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridJudge;
        private System.Windows.Forms.Panel buttonPanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblStatus;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnOk;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnCancel;
    }
}
