namespace Modern.Lab.Samples
{
    public partial class DurableTransportDialogForm
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
            this.destinationCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.fieldDestination = new Modern.Lab.WinForms.Controls.Display.ModernFieldList();
            this.fieldSource = new Modern.Lab.WinForms.Controls.Display.ModernFieldList();
            this.inputPanel = new System.Windows.Forms.TableLayoutPanel();
            this.lblLocNm = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboLocNm = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.lblArea = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboArea = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.lblDestination = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboDestination = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.lblDescription = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.txtDescription = new Modern.Lab.WinForms.Controls.Input.ModernTextBox();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.btnOk = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnCancel = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.sourceCard.SuspendLayout();
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
            this.sourceCard.TabIndex = 1;
            this.sourceCard.Text = "Source";
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
            // inputPanel
            //
            this.inputPanel.AutoSize = true;
            this.inputPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.inputPanel.ColumnCount = 2;
            this.inputPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.inputPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.inputPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.inputPanel.Location = new System.Drawing.Point(16, 148);
            this.inputPanel.Name = "inputPanel";
            this.inputPanel.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.inputPanel.Controls.Add(this.lblLocNm, 0, 0);
            this.inputPanel.Controls.Add(this.cboLocNm, 1, 0);
            this.inputPanel.Controls.Add(this.lblArea, 0, 1);
            this.inputPanel.Controls.Add(this.cboArea, 1, 1);
            this.inputPanel.Controls.Add(this.lblDestination, 0, 2);
            this.inputPanel.Controls.Add(this.cboDestination, 1, 2);
            this.inputPanel.Controls.Add(this.lblDescription, 0, 3);
            this.inputPanel.Controls.Add(this.txtDescription, 1, 3);
            this.inputPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.inputPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.inputPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.inputPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.inputPanel.RowCount = 4;
            this.inputPanel.Size = new System.Drawing.Size(688, 168);
            this.inputPanel.TabIndex = 2;
            //
            // lblLocNm
            //
            this.lblLocNm.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblLocNm.BackColor = System.Drawing.Color.Transparent;
            this.lblLocNm.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblLocNm.Location = new System.Drawing.Point(3, 15);
            this.lblLocNm.Name = "lblLocNm";
            this.lblLocNm.Size = new System.Drawing.Size(114, 24);
            this.lblLocNm.TabIndex = 0;
            this.lblLocNm.Text = "Location";
            //
            // cboLocNm
            //
            this.cboLocNm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboLocNm.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLocNm.Location = new System.Drawing.Point(123, 11);
            this.cboLocNm.Margin = new System.Windows.Forms.Padding(3, 3, 3, 5);
            this.cboLocNm.Name = "cboLocNm";
            this.cboLocNm.PlaceholderText = "Select a location";
            this.cboLocNm.Size = new System.Drawing.Size(562, 32);
            this.cboLocNm.TabIndex = 1;
            this.cboLocNm.SelectedIndexChanged += new System.EventHandler(this.OnLocNmChanged);
            this.cboLocNm.Child = null;
            //
            // lblArea
            //
            this.lblArea.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblArea.BackColor = System.Drawing.Color.Transparent;
            this.lblArea.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblArea.Location = new System.Drawing.Point(3, 55);
            this.lblArea.Name = "lblArea";
            this.lblArea.Size = new System.Drawing.Size(114, 24);
            this.lblArea.TabIndex = 2;
            this.lblArea.Text = "Area";
            //
            // cboArea
            //
            this.cboArea.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboArea.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboArea.Enabled = false;
            this.cboArea.Location = new System.Drawing.Point(123, 51);
            this.cboArea.Margin = new System.Windows.Forms.Padding(3, 3, 3, 5);
            this.cboArea.Name = "cboArea";
            this.cboArea.PlaceholderText = "Bay only";
            this.cboArea.Size = new System.Drawing.Size(562, 32);
            this.cboArea.TabIndex = 3;
            this.cboArea.SelectedIndexChanged += new System.EventHandler(this.OnAreaChanged);
            this.cboArea.Child = null;
            //
            // lblDestination
            //
            this.lblDestination.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDestination.BackColor = System.Drawing.Color.Transparent;
            this.lblDestination.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblDestination.Location = new System.Drawing.Point(3, 95);
            this.lblDestination.Name = "lblDestination";
            this.lblDestination.Size = new System.Drawing.Size(114, 24);
            this.lblDestination.TabIndex = 4;
            this.lblDestination.Text = "Destination";
            //
            // cboDestination
            //
            this.cboDestination.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboDestination.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDestination.Location = new System.Drawing.Point(123, 91);
            this.cboDestination.Margin = new System.Windows.Forms.Padding(3, 3, 3, 5);
            this.cboDestination.Name = "cboDestination";
            this.cboDestination.PlaceholderText = "Select a destination";
            this.cboDestination.Size = new System.Drawing.Size(562, 32);
            this.cboDestination.TabIndex = 5;
            this.cboDestination.Child = null;
            this.cboDestination.SelectedIndexChanged += new System.EventHandler(this.OnDestinationChanged);
            //
            // destinationCard
            //
            this.destinationCard.Text = "Destination Info";
            this.destinationCard.TitleBar = true;
            this.destinationCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.destinationCard.Height = 132;
            this.destinationCard.Padding = new System.Windows.Forms.Padding(12, 40, 12, 8);
            this.destinationCard.Controls.Add(this.fieldDestination);
            //
            // fieldDestination
            //
            this.fieldDestination.Columns = 5;
            this.fieldDestination.Dock = System.Windows.Forms.DockStyle.Fill;
            //
            // buttonPanel
            //
            this.buttonPanel.Controls.Add(this.btnOk);
            this.buttonPanel.Controls.Add(this.btnCancel);
            //
            // lblDescription
            //
            this.lblDescription.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDescription.BackColor = System.Drawing.Color.Transparent;
            this.lblDescription.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblDescription.Location = new System.Drawing.Point(3, 135);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(114, 24);
            this.lblDescription.TabIndex = 6;
            this.lblDescription.Text = "Description";
            //
            // txtDescription
            //
            this.txtDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDescription.Location = new System.Drawing.Point(123, 131);
            this.txtDescription.Margin = new System.Windows.Forms.Padding(3, 3, 3, 5);
            this.txtDescription.MaxLength = 200;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(562, 32);
            this.txtDescription.TabIndex = 7;
            this.txtDescription.Child = null;
            //
            // buttonPanel
            //
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonPanel.Location = new System.Drawing.Point(16, 316);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(688, 48);
            this.buttonPanel.TabIndex = 4;
            //
            // btnOk
            //
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnOk.Location = new System.Drawing.Point(504, 12);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(88, 32);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "Transport";
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
            // DurableTransportDialogForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AcceptButton = this.btnOk;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(720, 512);
            this.Controls.Add(this.destinationCard);
            this.Controls.Add(this.inputPanel);
            this.Controls.Add(this.sourceCard);
            this.Controls.Add(this.buttonPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DurableTransportDialogForm";
            this.Padding = new System.Windows.Forms.Padding(16);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Transport";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.sourceCard.ResumeLayout(false);
            this.inputPanel.ResumeLayout(false);
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox sourceCard;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox destinationCard;
        private Modern.Lab.WinForms.Controls.Display.ModernFieldList fieldDestination;
        private Modern.Lab.WinForms.Controls.Display.ModernFieldList fieldSource;
        private System.Windows.Forms.TableLayoutPanel inputPanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblLocNm;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboLocNm;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblArea;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboArea;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblDestination;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboDestination;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblDescription;
        private Modern.Lab.WinForms.Controls.Input.ModernTextBox txtDescription;
        private System.Windows.Forms.Panel buttonPanel;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnOk;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnCancel;
    }
}
