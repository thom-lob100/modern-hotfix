namespace Modern.Lab.Samples
{
    partial class DurableChangeDestDialogForm
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
            this.gridTransports = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.destinationPanel = new System.Windows.Forms.TableLayoutPanel();
            this.lblDestinationKind = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboDestinationKind = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.lblDestination = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboDestination = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.lblPort = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboPort = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.inputPanel = new System.Windows.Forms.TableLayoutPanel();
            this.lblDescription = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.txtDescription = new Modern.Lab.WinForms.Controls.Input.ModernTextBox();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.lblStatus = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.btnOk = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnCancel = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.destinationPanel.SuspendLayout();
            this.inputPanel.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // gridTransports
            //
            this.gridTransports.AllowFindPanel = false;
            this.gridTransports.AutoFitColumns = true;
            this.gridTransports.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridTransports.EmptyText = "No transport can change its destination";
            this.gridTransports.Location = new System.Drawing.Point(16, 16);
            this.gridTransports.Name = "gridTransports";
            this.gridTransports.ShowExportMenu = false;
            this.gridTransports.ShowStatusBar = true;
            this.gridTransports.Size = new System.Drawing.Size(1088, 264);
            this.gridTransports.StatusCountFormat = "{0:N0} transports";
            this.gridTransports.TabIndex = 0;
            this.gridTransports.SelectionChanged += new System.EventHandler(this.OnTransportSelectionChanged);
            this.gridTransports.Child = null;
            //
            // destinationPanel
            //
            this.destinationPanel.AutoSize = true;
            this.destinationPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.destinationPanel.ColumnCount = 7;
            this.destinationPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.destinationPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.destinationPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.destinationPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 240F));
            this.destinationPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 64F));
            this.destinationPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.destinationPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.destinationPanel.Controls.Add(this.lblDestinationKind, 0, 0);
            this.destinationPanel.Controls.Add(this.cboDestinationKind, 1, 0);
            this.destinationPanel.Controls.Add(this.lblDestination, 2, 0);
            this.destinationPanel.Controls.Add(this.cboDestination, 3, 0);
            this.destinationPanel.Controls.Add(this.lblPort, 4, 0);
            this.destinationPanel.Controls.Add(this.cboPort, 5, 0);
            this.destinationPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.destinationPanel.Location = new System.Drawing.Point(16, 280);
            this.destinationPanel.Name = "destinationPanel";
            this.destinationPanel.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.destinationPanel.RowCount = 1;
            this.destinationPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.destinationPanel.Size = new System.Drawing.Size(1088, 48);
            this.destinationPanel.TabIndex = 1;
            //
            // lblDestinationKind
            //
            this.lblDestinationKind.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDestinationKind.BackColor = System.Drawing.Color.Transparent;
            this.lblDestinationKind.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblDestinationKind.Location = new System.Drawing.Point(3, 15);
            this.lblDestinationKind.Name = "lblDestinationKind";
            this.lblDestinationKind.Size = new System.Drawing.Size(114, 24);
            this.lblDestinationKind.TabIndex = 0;
            this.lblDestinationKind.Text = "Destination";
            //
            // cboDestinationKind
            //
            this.cboDestinationKind.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboDestinationKind.Height = 32;
            this.cboDestinationKind.Location = new System.Drawing.Point(123, 11);
            this.cboDestinationKind.Margin = new System.Windows.Forms.Padding(3, 3, 3, 5);
            this.cboDestinationKind.Name = "cboDestinationKind";
            this.cboDestinationKind.Size = new System.Drawing.Size(194, 32);
            this.cboDestinationKind.TabIndex = 1;
            this.cboDestinationKind.SelectedIndexChanged += new System.EventHandler(this.OnDestinationKindChanged);
            //
            // lblDestination
            //
            this.lblDestination.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDestination.BackColor = System.Drawing.Color.Transparent;
            this.lblDestination.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblDestination.Location = new System.Drawing.Point(323, 15);
            this.lblDestination.Name = "lblDestination";
            this.lblDestination.Size = new System.Drawing.Size(66, 24);
            this.lblDestination.TabIndex = 2;
            this.lblDestination.Text = "Eqp Id";
            //
            // cboDestination
            //
            this.cboDestination.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboDestination.Height = 32;
            this.cboDestination.Location = new System.Drawing.Point(403, 11);
            this.cboDestination.Margin = new System.Windows.Forms.Padding(3, 3, 3, 5);
            this.cboDestination.Name = "cboDestination";
            this.cboDestination.Size = new System.Drawing.Size(234, 32);
            this.cboDestination.TabIndex = 3;
            this.cboDestination.SelectedIndexChanged += new System.EventHandler(this.OnDestinationSelectionChanged);
            //
            // lblPort
            //
            this.lblPort.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblPort.BackColor = System.Drawing.Color.Transparent;
            this.lblPort.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblPort.Location = new System.Drawing.Point(643, 15);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(50, 24);
            this.lblPort.TabIndex = 4;
            this.lblPort.Text = "Port";
            //
            // cboPort
            //
            this.cboPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPort.Height = 32;
            this.cboPort.Location = new System.Drawing.Point(707, 11);
            this.cboPort.Margin = new System.Windows.Forms.Padding(3, 3, 3, 5);
            this.cboPort.Name = "cboPort";
            this.cboPort.Size = new System.Drawing.Size(154, 32);
            this.cboPort.TabIndex = 5;
            this.cboPort.SelectedIndexChanged += new System.EventHandler(this.OnPortSelectionChanged);
            //
            // inputPanel
            //
            this.inputPanel.AutoSize = true;
            this.inputPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.inputPanel.ColumnCount = 2;
            this.inputPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.inputPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.inputPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.inputPanel.Location = new System.Drawing.Point(16, 328);
            this.inputPanel.Name = "inputPanel";
            this.inputPanel.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.inputPanel.Controls.Add(this.lblDescription, 0, 0);
            this.inputPanel.Controls.Add(this.txtDescription, 1, 0);
            this.inputPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.inputPanel.RowCount = 1;
            this.inputPanel.Size = new System.Drawing.Size(1088, 48);
            this.inputPanel.TabIndex = 2;
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
            this.txtDescription.Size = new System.Drawing.Size(962, 32);
            this.txtDescription.TabIndex = 1;
            this.txtDescription.Child = null;
            //
            // buttonPanel
            //
            this.buttonPanel.Controls.Add(this.lblStatus);
            this.buttonPanel.Controls.Add(this.btnOk);
            this.buttonPanel.Controls.Add(this.btnCancel);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonPanel.Location = new System.Drawing.Point(16, 376);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(1088, 48);
            this.buttonPanel.TabIndex = 3;
            //
            // lblStatus
            //
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.BackColor = System.Drawing.Color.Transparent;
            this.lblStatus.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblStatus.Location = new System.Drawing.Point(0, 16);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(832, 24);
            this.lblStatus.TabIndex = 2;
            //
            // btnOk
            //
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnOk.Location = new System.Drawing.Point(840, 12);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(152, 32);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "Change Destination";
            this.btnOk.Click += new System.EventHandler(this.OnOkClick);
            this.btnOk.Child = null;
            //
            // btnCancel
            //
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Subtle;
            this.btnCancel.Location = new System.Drawing.Point(1000, 12);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 32);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.OnCancelClick);
            this.btnCancel.Child = null;
            //
            // DurableChangeDestDialogForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AcceptButton = this.btnOk;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(1120, 440);
            this.Controls.Add(this.gridTransports);
            this.Controls.Add(this.destinationPanel);
            this.Controls.Add(this.inputPanel);
            this.Controls.Add(this.buttonPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DurableChangeDestDialogForm";
            this.Padding = new System.Windows.Forms.Padding(16);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Change Destination";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.destinationPanel.ResumeLayout(false);
            this.inputPanel.ResumeLayout(false);
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridTransports;
        private System.Windows.Forms.TableLayoutPanel destinationPanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblDestinationKind;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboDestinationKind;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblDestination;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboDestination;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblPort;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboPort;
        private System.Windows.Forms.TableLayoutPanel inputPanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblDescription;
        private Modern.Lab.WinForms.Controls.Input.ModernTextBox txtDescription;
        private System.Windows.Forms.Panel buttonPanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblStatus;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnOk;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnCancel;
    }
}
