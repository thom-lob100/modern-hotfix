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
            this.gridDestinations = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.gridPorts = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.inputPanel = new System.Windows.Forms.TableLayoutPanel();
            this.lblDescription = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.txtDescription = new Modern.Lab.WinForms.Controls.Input.ModernTextBox();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.lblStatus = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.btnOk = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnCancel = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.inputPanel.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // gridTransports
            //
            this.gridTransports.AllowFindPanel = false;
            this.gridTransports.AutoFitColumns = true;
            this.gridTransports.Dock = System.Windows.Forms.DockStyle.Top;
            this.gridTransports.EmptyText = "No Manual In or Manual Out request is waiting";
            this.gridTransports.Location = new System.Drawing.Point(16, 16);
            this.gridTransports.Name = "gridTransports";
            this.gridTransports.ShowExportMenu = false;
            this.gridTransports.ShowStatusBar = true;
            this.gridTransports.Size = new System.Drawing.Size(1088, 216);
            this.gridTransports.StatusCountFormat = "{0:N0} requests";
            this.gridTransports.TabIndex = 0;
            this.gridTransports.SelectionChanged += new System.EventHandler(this.OnTransportSelectionChanged);
            this.gridTransports.Child = null;
            //
            // gridDestinations
            //
            this.gridDestinations.AllowFindPanel = false;
            this.gridDestinations.AutoFitColumns = true;
            this.gridDestinations.Dock = System.Windows.Forms.DockStyle.Top;
            this.gridDestinations.EmptyText = "Select a request to list its destinations";
            this.gridDestinations.Location = new System.Drawing.Point(16, 232);
            this.gridDestinations.Name = "gridDestinations";
            this.gridDestinations.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.gridDestinations.ShowExportMenu = false;
            this.gridDestinations.ShowStatusBar = true;
            this.gridDestinations.Size = new System.Drawing.Size(1088, 200);
            this.gridDestinations.StatusCountFormat = "{0:N0} destinations";
            this.gridDestinations.TabIndex = 1;
            this.gridDestinations.SelectionChanged += new System.EventHandler(this.OnDestinationSelectionChanged);
            this.gridDestinations.Child = null;
            //
            // gridPorts
            //
            this.gridPorts.AllowFindPanel = false;
            this.gridPorts.AutoFitColumns = true;
            this.gridPorts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridPorts.EmptyText = "Select an equipment to list its ports";
            this.gridPorts.Location = new System.Drawing.Point(16, 432);
            this.gridPorts.Name = "gridPorts";
            this.gridPorts.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.gridPorts.ShowExportMenu = false;
            this.gridPorts.ShowStatusBar = true;
            this.gridPorts.Size = new System.Drawing.Size(1088, 200);
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
            this.inputPanel.Location = new System.Drawing.Point(16, 632);
            this.inputPanel.Name = "inputPanel";
            this.inputPanel.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.inputPanel.Controls.Add(this.lblDescription, 0, 0);
            this.inputPanel.Controls.Add(this.txtDescription, 1, 0);
            this.inputPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.inputPanel.RowCount = 1;
            this.inputPanel.Size = new System.Drawing.Size(1088, 48);
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
            this.buttonPanel.Location = new System.Drawing.Point(16, 680);
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
            this.ClientSize = new System.Drawing.Size(1120, 744);
            this.Controls.Add(this.gridPorts);
            this.Controls.Add(this.gridDestinations);
            this.Controls.Add(this.gridTransports);
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
            this.inputPanel.ResumeLayout(false);
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridTransports;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridDestinations;
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
