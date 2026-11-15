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
            this.typePanel = new System.Windows.Forms.Panel();
            this.lblType = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboType = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.gridTransports = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.destinationPanel = new System.Windows.Forms.Panel();
            this.lblDestinationKind = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboDestinationKind = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.lblDestination = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboDestination = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.lblPort = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboPort = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.inputPanel = new System.Windows.Forms.Panel();
            this.lblDescription = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.txtDescription = new Modern.Lab.WinForms.Controls.Input.ModernTextBox();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.lblStatus = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.btnOk = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnCancel = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.typePanel.SuspendLayout();
            this.destinationPanel.SuspendLayout();
            this.inputPanel.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // typePanel
            //
            this.typePanel.Controls.Add(this.lblType);
            this.typePanel.Controls.Add(this.cboType);
            this.typePanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.typePanel.Location = new System.Drawing.Point(16, 16);
            this.typePanel.Name = "typePanel";
            this.typePanel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.typePanel.Size = new System.Drawing.Size(1088, 48);
            this.typePanel.TabIndex = 0;
            //
            // lblType
            //
            this.lblType.BackColor = System.Drawing.Color.Transparent;
            this.lblType.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblType.Location = new System.Drawing.Point(0, 4);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(88, 32);
            this.lblType.TabIndex = 0;
            this.lblType.Text = "Type";
            this.lblType.TopLabel = "Durable";
            //
            // cboType
            //
            this.cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboType.Location = new System.Drawing.Point(96, 4);
            this.cboType.Name = "cboType";
            this.cboType.Size = new System.Drawing.Size(200, 32);
            this.cboType.TabIndex = 1;
            this.cboType.SelectedIndexChanged += new System.EventHandler(this.OnTypeChanged);
            //
            // gridTransports
            //
            this.gridTransports.AllowFindPanel = false;
            this.gridTransports.AutoFitColumns = true;
            this.gridTransports.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridTransports.EmptyText = "No transport can change its destination";
            this.gridTransports.Location = new System.Drawing.Point(16, 64);
            this.gridTransports.Name = "gridTransports";
            this.gridTransports.ShowExportMenu = false;
            this.gridTransports.ShowStatusBar = true;
            this.gridTransports.Size = new System.Drawing.Size(1088, 264);
            this.gridTransports.StatusCountFormat = "{0:N0} transports";
            this.gridTransports.TabIndex = 1;
            this.gridTransports.SelectionChanged += new System.EventHandler(this.OnTransportSelectionChanged);
            this.gridTransports.Child = null;
            //
            // destinationPanel
            //
            this.destinationPanel.Controls.Add(this.lblDestinationKind);
            this.destinationPanel.Controls.Add(this.cboDestinationKind);
            this.destinationPanel.Controls.Add(this.lblDestination);
            this.destinationPanel.Controls.Add(this.cboDestination);
            this.destinationPanel.Controls.Add(this.lblPort);
            this.destinationPanel.Controls.Add(this.cboPort);
            this.destinationPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.destinationPanel.Location = new System.Drawing.Point(16, 328);
            this.destinationPanel.Name = "destinationPanel";
            this.destinationPanel.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.destinationPanel.Size = new System.Drawing.Size(1088, 48);
            this.destinationPanel.TabIndex = 2;
            //
            // lblDestinationKind
            //
            this.lblDestinationKind.BackColor = System.Drawing.Color.Transparent;
            this.lblDestinationKind.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblDestinationKind.Location = new System.Drawing.Point(0, 12);
            this.lblDestinationKind.Name = "lblDestinationKind";
            this.lblDestinationKind.Size = new System.Drawing.Size(88, 32);
            this.lblDestinationKind.TabIndex = 0;
            this.lblDestinationKind.Text = "Destination";
            //
            // cboDestinationKind
            //
            this.cboDestinationKind.Location = new System.Drawing.Point(96, 12);
            this.cboDestinationKind.Name = "cboDestinationKind";
            this.cboDestinationKind.Size = new System.Drawing.Size(200, 32);
            this.cboDestinationKind.TabIndex = 1;
            this.cboDestinationKind.SelectedIndexChanged += new System.EventHandler(this.OnDestinationKindChanged);
            //
            // lblDestination
            //
            this.lblDestination.BackColor = System.Drawing.Color.Transparent;
            this.lblDestination.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblDestination.Location = new System.Drawing.Point(320, 12);
            this.lblDestination.Name = "lblDestination";
            this.lblDestination.Size = new System.Drawing.Size(88, 32);
            this.lblDestination.TabIndex = 2;
            this.lblDestination.Text = "Eqp Id";
            //
            // cboDestination
            //
            this.cboDestination.Location = new System.Drawing.Point(416, 12);
            this.cboDestination.Name = "cboDestination";
            this.cboDestination.Size = new System.Drawing.Size(240, 32);
            this.cboDestination.TabIndex = 3;
            this.cboDestination.SelectedIndexChanged += new System.EventHandler(this.OnDestinationSelectionChanged);
            //
            // lblPort
            //
            this.lblPort.BackColor = System.Drawing.Color.Transparent;
            this.lblPort.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblPort.Location = new System.Drawing.Point(680, 12);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(88, 32);
            this.lblPort.TabIndex = 4;
            this.lblPort.Text = "Port";
            //
            // cboPort
            //
            this.cboPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPort.Location = new System.Drawing.Point(776, 12);
            this.cboPort.Name = "cboPort";
            this.cboPort.Size = new System.Drawing.Size(160, 32);
            this.cboPort.TabIndex = 5;
            this.cboPort.SelectedIndexChanged += new System.EventHandler(this.OnPortSelectionChanged);
            //
            // inputPanel
            //
            this.inputPanel.Controls.Add(this.lblDescription);
            this.inputPanel.Controls.Add(this.txtDescription);
            this.inputPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.inputPanel.Location = new System.Drawing.Point(16, 376);
            this.inputPanel.Name = "inputPanel";
            this.inputPanel.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.inputPanel.Size = new System.Drawing.Size(1088, 48);
            this.inputPanel.TabIndex = 3;
            //
            // lblDescription
            //
            this.lblDescription.BackColor = System.Drawing.Color.Transparent;
            this.lblDescription.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblDescription.Location = new System.Drawing.Point(0, 12);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(88, 32);
            this.lblDescription.TabIndex = 0;
            this.lblDescription.Text = "Description";
            //
            // txtDescription
            //
            this.txtDescription.Location = new System.Drawing.Point(96, 12);
            this.txtDescription.MaxLength = 200;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(992, 32);
            this.txtDescription.TabIndex = 1;
            this.txtDescription.Child = null;
            //
            // buttonPanel
            //
            this.buttonPanel.Controls.Add(this.lblStatus);
            this.buttonPanel.Controls.Add(this.btnOk);
            this.buttonPanel.Controls.Add(this.btnCancel);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonPanel.Location = new System.Drawing.Point(16, 424);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(1088, 48);
            this.buttonPanel.TabIndex = 4;
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
            this.ClientSize = new System.Drawing.Size(1120, 488);
            this.Controls.Add(this.gridTransports);
            this.Controls.Add(this.destinationPanel);
            this.Controls.Add(this.inputPanel);
            this.Controls.Add(this.buttonPanel);
            this.Controls.Add(this.typePanel);
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
            this.typePanel.ResumeLayout(false);
            this.destinationPanel.ResumeLayout(false);
            this.inputPanel.ResumeLayout(false);
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel typePanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblType;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboType;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridTransports;
        private System.Windows.Forms.Panel destinationPanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblDestinationKind;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboDestinationKind;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblDestination;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboDestination;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblPort;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboPort;
        private System.Windows.Forms.Panel inputPanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblDescription;
        private Modern.Lab.WinForms.Controls.Input.ModernTextBox txtDescription;
        private System.Windows.Forms.Panel buttonPanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblStatus;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnOk;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnCancel;
    }
}
