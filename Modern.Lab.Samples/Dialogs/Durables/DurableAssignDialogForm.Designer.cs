namespace Modern.Lab.Samples
{
    public partial class DurableAssignDialogForm
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
            this.sourceCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.fieldSource = new Modern.Lab.WinForms.Controls.Display.ModernFieldList();
            this.lotPanel = new System.Windows.Forms.TableLayoutPanel();
            this.lblLot = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboLot = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.gridUnits = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.inputPanel = new System.Windows.Forms.TableLayoutPanel();
            this.lblDescription = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.txtDescription = new Modern.Lab.WinForms.Controls.Input.ModernTextBox();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.lblStatus = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.btnOk = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnCancel = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.sourceCard.SuspendLayout();
            this.lotPanel.SuspendLayout();
            this.inputPanel.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // sourceCard
            //
            this.sourceCard.Controls.Add(this.fieldSource);
            this.sourceCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.sourceCard.Location = new System.Drawing.Point(16, 16);
            this.sourceCard.Name = "sourceCard";
            this.sourceCard.Padding = new System.Windows.Forms.Padding(12, 40, 12, 8);
            this.sourceCard.Size = new System.Drawing.Size(688, 132);
            this.sourceCard.TabIndex = 0;
            this.sourceCard.Text = "Durable";
            this.sourceCard.TitleBar = true;
            //
            // fieldSource
            //
            this.fieldSource.Columns = 5;
            this.fieldSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fieldSource.Location = new System.Drawing.Point(12, 40);
            this.fieldSource.Name = "fieldSource";
            this.fieldSource.Size = new System.Drawing.Size(664, 84);
            this.fieldSource.TabIndex = 0;
            //
            // lotPanel
            //
            this.lotPanel.AutoSize = true;
            this.lotPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.lotPanel.ColumnCount = 2;
            this.lotPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.lotPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.lotPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.lotPanel.Location = new System.Drawing.Point(16, 148);
            this.lotPanel.Name = "lotPanel";
            this.lotPanel.Padding = new System.Windows.Forms.Padding(0, 8, 0, 4);
            this.lotPanel.Controls.Add(this.lblLot, 0, 0);
            this.lotPanel.Controls.Add(this.cboLot, 1, 0);
            this.lotPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.lotPanel.RowCount = 1;
            this.lotPanel.Size = new System.Drawing.Size(688, 52);
            this.lotPanel.TabIndex = 1;
            //
            // lblLot
            //
            this.lblLot.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblLot.BackColor = System.Drawing.Color.Transparent;
            this.lblLot.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblLot.Location = new System.Drawing.Point(3, 15);
            this.lblLot.Name = "lblLot";
            this.lblLot.Size = new System.Drawing.Size(114, 24);
            this.lblLot.TabIndex = 0;
            this.lblLot.Text = "Lot";
            //
            // cboLot
            //
            this.cboLot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboLot.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cboLot.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cboLot.Location = new System.Drawing.Point(123, 11);
            this.cboLot.Margin = new System.Windows.Forms.Padding(3, 3, 3, 5);
            this.cboLot.Name = "cboLot";
            this.cboLot.PlaceholderText = "Select or type a lot";
            this.cboLot.Size = new System.Drawing.Size(562, 32);
            this.cboLot.TabIndex = 1;
            this.cboLot.SelectedIndexChanged += new System.EventHandler(this.OnLotChanged);
            this.cboLot.Child = null;
            //
            // gridUnits
            //
            this.gridUnits.AllowFindPanel = false;
            this.gridUnits.AutoFitColumns = true;
            this.gridUnits.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridUnits.EmptyText = "Select a lot to list its units";
            this.gridUnits.Location = new System.Drawing.Point(16, 200);
            this.gridUnits.Name = "gridUnits";
            this.gridUnits.ReadOnly = false;
            this.gridUnits.ShowExportMenu = false;
            this.gridUnits.ShowStatusBar = true;
            this.gridUnits.Size = new System.Drawing.Size(688, 208);
            this.gridUnits.StatusCountFormat = "{0:N0} wafers";
            this.gridUnits.TabIndex = 2;
            this.gridUnits.CellValueChanged += new System.EventHandler<Modern.Lab.Controls.Wpf.Data.GridCellValueChangedEventArgs>(this.OnUnitsCellValueChanged);
            this.gridUnits.Child = null;
            //
            // inputPanel
            //
            this.inputPanel.AutoSize = true;
            this.inputPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.inputPanel.ColumnCount = 2;
            this.inputPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.inputPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.inputPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.inputPanel.Location = new System.Drawing.Point(16, 408);
            this.inputPanel.Name = "inputPanel";
            this.inputPanel.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.inputPanel.Controls.Add(this.lblDescription, 0, 0);
            this.inputPanel.Controls.Add(this.txtDescription, 1, 0);
            this.inputPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.inputPanel.RowCount = 1;
            this.inputPanel.Size = new System.Drawing.Size(688, 48);
            this.inputPanel.TabIndex = 3;
            //
            // lblDescription
            //
            this.lblDescription.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDescription.BackColor = System.Drawing.Color.Transparent;
            this.lblDescription.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblDescription.Location = new System.Drawing.Point(3, 15);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(114, 24);
            this.lblDescription.TabIndex = 0;
            this.lblDescription.Text = "Description";
            //
            // txtDescription
            //
            this.txtDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDescription.Location = new System.Drawing.Point(123, 11);
            this.txtDescription.Margin = new System.Windows.Forms.Padding(3, 3, 3, 5);
            this.txtDescription.MaxLength = 200;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(562, 32);
            this.txtDescription.TabIndex = 1;
            this.txtDescription.Child = null;
            //
            // buttonPanel
            //
            this.buttonPanel.Controls.Add(this.lblStatus);
            this.buttonPanel.Controls.Add(this.btnOk);
            this.buttonPanel.Controls.Add(this.btnCancel);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonPanel.Location = new System.Drawing.Point(16, 456);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(688, 48);
            this.buttonPanel.TabIndex = 4;
            //
            // lblStatus
            //
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.BackColor = System.Drawing.Color.Transparent;
            this.lblStatus.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblStatus.Location = new System.Drawing.Point(0, 16);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(496, 24);
            this.lblStatus.TabIndex = 2;
            //
            // btnOk
            //
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnOk.Location = new System.Drawing.Point(504, 12);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(88, 32);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "OK";
            this.btnOk.Click += new System.EventHandler(this.OnOkClick);
            this.btnOk.Child = null;
            //
            // btnCancel
            //
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Subtle;
            this.btnCancel.Location = new System.Drawing.Point(600, 12);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 32);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.OnCancelClick);
            this.btnCancel.Child = null;
            //
            // DurableAssignDialogForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AcceptButton = this.btnOk;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(720, 520);
            this.Controls.Add(this.gridUnits);
            this.Controls.Add(this.lotPanel);
            this.Controls.Add(this.sourceCard);
            this.Controls.Add(this.inputPanel);
            this.Controls.Add(this.buttonPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DurableAssignDialogForm";
            this.Padding = new System.Windows.Forms.Padding(16);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Assign";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.sourceCard.ResumeLayout(false);
            this.lotPanel.ResumeLayout(false);
            this.inputPanel.ResumeLayout(false);
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox sourceCard;
        private Modern.Lab.WinForms.Controls.Display.ModernFieldList fieldSource;
        private System.Windows.Forms.TableLayoutPanel lotPanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblLot;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboLot;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridUnits;
        private System.Windows.Forms.TableLayoutPanel inputPanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblDescription;
        private Modern.Lab.WinForms.Controls.Input.ModernTextBox txtDescription;
        private System.Windows.Forms.Panel buttonPanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblStatus;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnOk;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnCancel;
    }
}
