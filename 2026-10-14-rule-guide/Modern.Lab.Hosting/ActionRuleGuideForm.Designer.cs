namespace Modern.Lab.Hosting
{
    public partial class ActionRuleGuideForm
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

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.treeRules = new Modern.Lab.WinForms.Controls.Data.ModernTreeList();
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblSelection = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.lblNote = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.panelTop.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            //
            // treeRules
            //
            this.treeRules.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeRules.Name = "treeRules";
            this.treeRules.ShowStatusBar = false;
            this.treeRules.ShowExportMenu = false;
            this.treeRules.AllowColumnFilters = false;
            this.treeRules.TabIndex = 0;
            //
            // panelTop
            //
            this.panelTop.Controls.Add(this.lblSelection);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Height = 36;
            this.panelTop.Name = "panelTop";
            this.panelTop.Padding = new System.Windows.Forms.Padding(12, 8, 12, 4);
            //
            // lblSelection
            //
            this.lblSelection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSelection.Name = "lblSelection";
            this.lblSelection.Text = "No row selected — pick a row in the screen to compare.";
            //
            // panelBottom
            //
            this.panelBottom.Controls.Add(this.lblNote);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Height = 32;
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Padding = new System.Windows.Forms.Padding(12, 6, 12, 6);
            //
            // lblNote
            //
            this.lblNote.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNote.Name = "lblNote";
            this.lblNote.Text = "The screen checks these before sending. The server makes the final decision.";
            //
            // ActionRuleGuideForm
            //
            this.ClientSize = new System.Drawing.Size(1240, 600);
            this.Controls.Add(this.treeRules);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.panelBottom);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(900, 360);
            this.Name = "ActionRuleGuideForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Rules";
            this.panelTop.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private Modern.Lab.WinForms.Controls.Data.ModernTreeList treeRules;
        private System.Windows.Forms.Panel panelTop;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblSelection;
        private System.Windows.Forms.Panel panelBottom;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblNote;
    }
}
