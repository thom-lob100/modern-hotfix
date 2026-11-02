namespace Modern.Lab.Samples
{
    partial class DurableManualInDialogForm
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
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.cardPanel = new System.Windows.Forms.TableLayoutPanel();
            this.sourceCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.fieldSource = new Modern.Lab.WinForms.Controls.Display.ModernFieldList();
            this.lotCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.fieldLot = new Modern.Lab.WinForms.Controls.Display.ModernFieldList();
            this.gridEquipments = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.gridPorts = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.inputPanel = new System.Windows.Forms.TableLayoutPanel();
            this.lblDescription = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.txtDescription = new Modern.Lab.WinForms.Controls.Input.ModernTextBox();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.lblStatus = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.btnOk = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnCancel = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.cardPanel.SuspendLayout();
            this.sourceCard.SuspendLayout();
            this.lotCard.SuspendLayout();
            this.inputPanel.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // cardPanel
            //
            this.cardPanel.ColumnCount = 2;
            this.cardPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.cardPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.cardPanel.Controls.Add(this.sourceCard, 0, 0);
            this.cardPanel.Controls.Add(this.lotCard, 1, 0);
            this.cardPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.cardPanel.Location = new System.Drawing.Point(16, 16);
            this.cardPanel.Margin = new System.Windows.Forms.Padding(0);
            this.cardPanel.Name = "cardPanel";
            this.cardPanel.RowCount = 1;
            this.cardPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.cardPanel.Size = new System.Drawing.Size(848, 156);
            this.cardPanel.TabIndex = 0;
            //
            // sourceCard
            //
            this.sourceCard.Controls.Add(this.fieldSource);
            this.sourceCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sourceCard.Location = new System.Drawing.Point(0, 0);
            this.sourceCard.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this.sourceCard.Name = "sourceCard";
            this.sourceCard.Padding = new System.Windows.Forms.Padding(12, 40, 12, 8);
            this.sourceCard.Size = new System.Drawing.Size(418, 156);
            this.sourceCard.TabIndex = 0;
            this.sourceCard.Text = "Durable";
            this.sourceCard.TitleBar = true;
            //
            // fieldSource
            //
            this.fieldSource.Columns = 3;
            this.fieldSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fieldSource.Location = new System.Drawing.Point(12, 40);
            this.fieldSource.Name = "fieldSource";
            this.fieldSource.Size = new System.Drawing.Size(394, 108);
            this.fieldSource.TabIndex = 0;
            //
            // lotCard
            //
            this.lotCard.Controls.Add(this.fieldLot);
            this.lotCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lotCard.Location = new System.Drawing.Point(430, 0);
            this.lotCard.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.lotCard.Name = "lotCard";
            this.lotCard.Padding = new System.Windows.Forms.Padding(12, 40, 12, 8);
            this.lotCard.Size = new System.Drawing.Size(418, 156);
            this.lotCard.TabIndex = 1;
            this.lotCard.Text = "Lot";
            this.lotCard.TitleBar = true;
            //
            // fieldLot
            //
            this.fieldLot.Columns = 3;
            this.fieldLot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fieldLot.Location = new System.Drawing.Point(12, 40);
            this.fieldLot.Name = "fieldLot";
            this.fieldLot.Size = new System.Drawing.Size(394, 108);
            this.fieldLot.TabIndex = 0;
            //
            // gridEquipments
            //
            this.gridEquipments.AllowFindPanel = false;
            this.gridEquipments.AutoFitColumns = true;
            this.gridEquipments.Dock = System.Windows.Forms.DockStyle.Top;
            this.gridEquipments.EmptyText = "No equipment is ready for Manual In";
            this.gridEquipments.Location = new System.Drawing.Point(16, 172);
            this.gridEquipments.Name = "gridEquipments";
            this.gridEquipments.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.gridEquipments.ShowExportMenu = false;
            this.gridEquipments.ShowStatusBar = true;
            this.gridEquipments.Size = new System.Drawing.Size(848, 216);
            this.gridEquipments.StatusCountFormat = "{0:N0} equipments";
            this.gridEquipments.TabIndex = 1;
            this.gridEquipments.SelectionChanged += new System.EventHandler(this.OnEquipmentSelectionChanged);
            this.gridEquipments.Child = null;
            //
            // gridPorts
            //
            this.gridPorts.AllowFindPanel = false;
            this.gridPorts.AutoFitColumns = true;
            this.gridPorts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridPorts.EmptyText = "Select an equipment to list its ports";
            this.gridPorts.Location = new System.Drawing.Point(16, 388);
            this.gridPorts.Name = "gridPorts";
            this.gridPorts.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.gridPorts.ShowExportMenu = false;
            this.gridPorts.ShowStatusBar = true;
            this.gridPorts.Size = new System.Drawing.Size(848, 184);
            this.gridPorts.StatusCountFormat = "{0:N0} ports";
            this.gridPorts.TabIndex = 2;
            this.gridPorts.SelectionChanged += new System.EventHandler(this.OnPortSelectionChanged);
            this.gridPorts.Child = null;
            //
            // inputPanel
            //
            this.inputPanel.AutoSize = true;
            this.inputPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.inputPanel.ColumnCount = 2;
            this.inputPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.inputPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.inputPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.inputPanel.Location = new System.Drawing.Point(16, 572);
            this.inputPanel.Name = "inputPanel";
            this.inputPanel.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.inputPanel.Controls.Add(this.lblDescription, 0, 0);
            this.inputPanel.Controls.Add(this.txtDescription, 1, 0);
            this.inputPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.inputPanel.RowCount = 1;
            this.inputPanel.Size = new System.Drawing.Size(848, 48);
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
            this.txtDescription.Size = new System.Drawing.Size(722, 32);
            this.txtDescription.TabIndex = 1;
            this.txtDescription.Child = null;
            //
            // buttonPanel
            //
            this.buttonPanel.Controls.Add(this.lblStatus);
            this.buttonPanel.Controls.Add(this.btnOk);
            this.buttonPanel.Controls.Add(this.btnCancel);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonPanel.Location = new System.Drawing.Point(16, 620);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(848, 48);
            this.buttonPanel.TabIndex = 4;
            //
            // lblStatus
            //
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.BackColor = System.Drawing.Color.Transparent;
            this.lblStatus.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblStatus.Location = new System.Drawing.Point(0, 16);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(640, 24);
            this.lblStatus.TabIndex = 2;
            //
            // btnOk
            //
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnOk.Location = new System.Drawing.Point(648, 12);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(104, 32);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "Manual In";
            this.btnOk.Click += new System.EventHandler(this.OnOkClick);
            this.btnOk.Child = null;
            //
            // btnCancel
            //
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Subtle;
            this.btnCancel.Location = new System.Drawing.Point(760, 12);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 32);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.OnCancelClick);
            this.btnCancel.Child = null;
            //
            // DurableManualInDialogForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AcceptButton = this.btnOk;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(880, 684);
            this.Controls.Add(this.gridPorts);
            this.Controls.Add(this.gridEquipments);
            this.Controls.Add(this.cardPanel);
            this.Controls.Add(this.inputPanel);
            this.Controls.Add(this.buttonPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DurableManualInDialogForm";
            this.Padding = new System.Windows.Forms.Padding(16);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Manual In";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.cardPanel.ResumeLayout(false);
            this.sourceCard.ResumeLayout(false);
            this.lotCard.ResumeLayout(false);
            this.inputPanel.ResumeLayout(false);
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel cardPanel;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox sourceCard;
        private Modern.Lab.WinForms.Controls.Display.ModernFieldList fieldSource;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox lotCard;
        private Modern.Lab.WinForms.Controls.Display.ModernFieldList fieldLot;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridEquipments;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridPorts;
        private System.Windows.Forms.TableLayoutPanel inputPanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblDescription;
        private Modern.Lab.WinForms.Controls.Input.ModernTextBox txtDescription;
        private System.Windows.Forms.Panel buttonPanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblStatus;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnOk;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnCancel;
    }
}
