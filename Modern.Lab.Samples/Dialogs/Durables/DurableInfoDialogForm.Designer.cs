namespace Modern.Lab.Samples
{
    public partial class DurableInfoDialogForm
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
            this.durableCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.fieldDurable = new Modern.Lab.WinForms.Controls.Display.ModernFieldList();
            this.slotCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.slotMap = new Modern.Lab.WinForms.Controls.Display.ModernSlotMapMini();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.btnClose = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.durableCard.SuspendLayout();
            this.slotCard.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // durableCard
            //
            this.durableCard.Controls.Add(this.fieldDurable);
            this.durableCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.durableCard.Location = new System.Drawing.Point(16, 16);
            this.durableCard.Name = "durableCard";
            this.durableCard.Padding = new System.Windows.Forms.Padding(12, 40, 12, 8);
            this.durableCard.Size = new System.Drawing.Size(568, 132);
            this.durableCard.TabIndex = 0;
            this.durableCard.Text = "Durable";
            this.durableCard.TitleBar = true;
            //
            // fieldDurable
            //
            this.fieldDurable.Columns = 5;
            this.fieldDurable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fieldDurable.Location = new System.Drawing.Point(12, 40);
            this.fieldDurable.Name = "fieldDurable";
            this.fieldDurable.Size = new System.Drawing.Size(544, 84);
            this.fieldDurable.TabIndex = 0;
            //
            // slotCard
            //
            this.slotCard.Controls.Add(this.slotMap);
            this.slotCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.slotCard.Location = new System.Drawing.Point(16, 148);
            this.slotCard.Name = "slotCard";
            this.slotCard.Padding = new System.Windows.Forms.Padding(12, 40, 12, 12);
            this.slotCard.Size = new System.Drawing.Size(568, 528);
            this.slotCard.TabIndex = 1;
            this.slotCard.Text = "Slot Map";
            this.slotCard.TitleBar = true;
            //
            // slotMap
            //
            this.slotMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.slotMap.Location = new System.Drawing.Point(12, 40);
            this.slotMap.Name = "slotMap";
            this.slotMap.Size = new System.Drawing.Size(544, 476);
            this.slotMap.TabIndex = 0;
            this.slotMap.Child = null;
            //
            // buttonPanel
            //
            this.buttonPanel.Controls.Add(this.btnClose);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonPanel.Location = new System.Drawing.Point(16, 676);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(568, 48);
            this.buttonPanel.TabIndex = 2;
            //
            // btnClose
            //
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Subtle;
            this.btnClose.Location = new System.Drawing.Point(480, 12);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(88, 32);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.OnCloseClick);
            this.btnClose.Child = null;
            //
            // DurableInfoDialogForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AcceptButton = this.btnClose;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(600, 740);
            this.Controls.Add(this.slotCard);
            this.Controls.Add(this.durableCard);
            this.Controls.Add(this.buttonPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DurableInfoDialogForm";
            this.Padding = new System.Windows.Forms.Padding(16);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Durable Info";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.durableCard.ResumeLayout(false);
            this.slotCard.ResumeLayout(false);
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox durableCard;
        private Modern.Lab.WinForms.Controls.Display.ModernFieldList fieldDurable;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox slotCard;
        private Modern.Lab.WinForms.Controls.Display.ModernSlotMapMini slotMap;
        private System.Windows.Forms.Panel buttonPanel;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnClose;
    }
}
