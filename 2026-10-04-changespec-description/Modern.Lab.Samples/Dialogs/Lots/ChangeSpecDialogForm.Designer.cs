namespace Modern.Lab.Samples
{
    public partial class ChangeSpecDialogForm
    {

        private System.ComponentModel.IContainer components = null;



        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code





        private void InitializeComponent()
        {
            this.sourceCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.fieldSource = new Modern.Lab.WinForms.Controls.Display.ModernFieldList();
            this.inputPanel = new System.Windows.Forms.TableLayoutPanel();
            this.lblProd = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboProd = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.lblFlow = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboFlow = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.lblOper = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboOper = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.lblDescription = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.txtDescription = new Modern.Lab.WinForms.Controls.Input.ModernTextBox();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.btnOk = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnCancel = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.sourceCard.SuspendLayout();
            this.inputPanel.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            this.sourceCard.Controls.Add(this.fieldSource);
            this.sourceCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.sourceCard.Location = new System.Drawing.Point(16, 16);
            this.sourceCard.Name = "sourceCard";
            this.sourceCard.Padding = new System.Windows.Forms.Padding(12, 40, 12, 8);
            this.sourceCard.Size = new System.Drawing.Size(688, 132);
            this.sourceCard.TabIndex = 0;
            this.sourceCard.Text = "Lot";
            this.sourceCard.TitleBar = true;
            this.fieldSource.Columns = 5;
            this.fieldSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fieldSource.Location = new System.Drawing.Point(12, 40);
            this.fieldSource.Name = "fieldSource";
            this.fieldSource.Size = new System.Drawing.Size(664, 84);
            this.fieldSource.TabIndex = 0;
            this.inputPanel.ColumnCount = 4;
            this.inputPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 104F));
            this.inputPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.inputPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 88F));
            this.inputPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.inputPanel.Controls.Add(this.lblProd, 0, 0);
            this.inputPanel.Controls.Add(this.cboProd, 1, 0);
            this.inputPanel.SetColumnSpan(this.cboProd, 3);
            this.inputPanel.Controls.Add(this.lblFlow, 0, 1);
            this.inputPanel.Controls.Add(this.cboFlow, 1, 1);
            this.inputPanel.Controls.Add(this.lblOper, 2, 1);
            this.inputPanel.Controls.Add(this.cboOper, 3, 1);
            this.inputPanel.Controls.Add(this.lblDescription, 0, 2);
            this.inputPanel.Controls.Add(this.txtDescription, 1, 2);
            this.inputPanel.SetColumnSpan(this.txtDescription, 3);
            this.inputPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.inputPanel.Location = new System.Drawing.Point(16, 148);
            this.inputPanel.Name = "inputPanel";
            this.inputPanel.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.inputPanel.RowCount = 3;
            this.inputPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.inputPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.inputPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.inputPanel.Size = new System.Drawing.Size(688, 168);
            this.inputPanel.TabIndex = 1;
            this.lblProd.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblProd.BackColor = System.Drawing.Color.Transparent;
            this.lblProd.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblProd.Location = new System.Drawing.Point(3, 16);
            this.lblProd.Name = "lblProd";
            this.lblProd.Required = true;
            this.lblProd.Size = new System.Drawing.Size(98, 24);
            this.lblProd.TabIndex = 0;
            this.lblProd.Text = "Prod Id";
            this.cboProd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboProd.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboProd.Location = new System.Drawing.Point(107, 11);
            this.cboProd.Margin = new System.Windows.Forms.Padding(3, 3, 3, 5);
            this.cboProd.Name = "cboProd";
            this.cboProd.PlaceholderText = "Select a product";
            this.cboProd.Size = new System.Drawing.Size(578, 32);
            this.cboProd.TabIndex = 1;
            this.cboProd.SelectedIndexChanged += new System.EventHandler(this.OnProdChanged);
            this.cboProd.Child = null;
            this.lblFlow.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblFlow.BackColor = System.Drawing.Color.Transparent;
            this.lblFlow.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblFlow.Location = new System.Drawing.Point(3, 56);
            this.lblFlow.Name = "lblFlow";
            this.lblFlow.Required = true;
            this.lblFlow.Size = new System.Drawing.Size(98, 24);
            this.lblFlow.TabIndex = 2;
            this.lblFlow.Text = "Flow Id";
            this.cboFlow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboFlow.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFlow.Location = new System.Drawing.Point(107, 51);
            this.cboFlow.Margin = new System.Windows.Forms.Padding(3, 3, 3, 5);
            this.cboFlow.Name = "cboFlow";
            this.cboFlow.PlaceholderText = "Select a flow";
            this.cboFlow.Size = new System.Drawing.Size(242, 32);
            this.cboFlow.TabIndex = 3;
            this.cboFlow.SelectedIndexChanged += new System.EventHandler(this.OnFlowChanged);
            this.cboFlow.Child = null;
            this.lblOper.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblOper.BackColor = System.Drawing.Color.Transparent;
            this.lblOper.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblOper.Location = new System.Drawing.Point(363, 56);
            this.lblOper.Margin = new System.Windows.Forms.Padding(11, 3, 3, 3);
            this.lblOper.Name = "lblOper";
            this.lblOper.Required = true;
            this.lblOper.Size = new System.Drawing.Size(74, 24);
            this.lblOper.TabIndex = 4;
            this.lblOper.Text = "Oper Id";
            this.cboOper.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboOper.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboOper.Location = new System.Drawing.Point(443, 51);
            this.cboOper.Margin = new System.Windows.Forms.Padding(3, 3, 3, 5);
            this.cboOper.Name = "cboOper";
            this.cboOper.PlaceholderText = "Select an operation";
            this.cboOper.Size = new System.Drawing.Size(242, 32);
            this.cboOper.TabIndex = 5;
            this.cboOper.Child = null;
            this.lblDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.lblDescription.BackColor = System.Drawing.Color.Transparent;
            this.lblDescription.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblDescription.Location = new System.Drawing.Point(3, 96);
            this.lblDescription.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(98, 24);
            this.lblDescription.TabIndex = 6;
            this.lblDescription.Text = "Description";
            this.txtDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDescription.Location = new System.Drawing.Point(107, 91);
            this.txtDescription.Margin = new System.Windows.Forms.Padding(3, 3, 3, 5);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.PlaceholderText = "Description (optional)";
            this.txtDescription.Size = new System.Drawing.Size(578, 72);
            this.txtDescription.TabIndex = 7;
            this.buttonPanel.Controls.Add(this.btnOk);
            this.buttonPanel.Controls.Add(this.btnCancel);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonPanel.Location = new System.Drawing.Point(16, 324);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(688, 48);
            this.buttonPanel.TabIndex = 2;
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnOk.Location = new System.Drawing.Point(504, 12);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(88, 32);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "Change";
            this.btnOk.Click += new System.EventHandler(this.OnOkClick);
            this.btnOk.Child = null;
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Subtle;
            this.btnCancel.Location = new System.Drawing.Point(600, 12);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 32);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.OnCancelClick);
            this.btnCancel.Child = null;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(720, 388);
            this.Controls.Add(this.inputPanel);
            this.Controls.Add(this.sourceCard);
            this.Controls.Add(this.buttonPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChangeSpecDialogForm";
            this.Padding = new System.Windows.Forms.Padding(16);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ChangeSpec";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.sourceCard.ResumeLayout(false);
            this.inputPanel.ResumeLayout(false);
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox sourceCard;
        private Modern.Lab.WinForms.Controls.Display.ModernFieldList fieldSource;
        private System.Windows.Forms.TableLayoutPanel inputPanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblProd;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboProd;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblFlow;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboFlow;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblOper;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboOper;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblDescription;
        private Modern.Lab.WinForms.Controls.Input.ModernTextBox txtDescription;
        private System.Windows.Forms.Panel buttonPanel;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnOk;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnCancel;
    }
}
