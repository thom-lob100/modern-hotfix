namespace Modern.Lab.MasterData
{
    public partial class VirtualEquipmentForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel titlePanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblTitle;
        private System.Windows.Forms.Panel spTitle;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel searchCard;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblFacId;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboFacId;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblEqpTyp;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboEqpTyp;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblDetEqpTyp;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboDetEqpTyp;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblEqpGrpId;
        private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboEqpGrpId;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnRefresh;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnReset;
        private System.Windows.Forms.Panel gapSearch;
        private Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer splitMain;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox listCard;
        private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridVirtualEquipments;
        private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox editorCard;
        private Modern.Lab.Hosting.MasterData.ModernPropertyGrid propertyGrid;
        private System.Windows.Forms.FlowLayoutPanel actionPanel;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnDelete;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnSave;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnCancel;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnDuplicate;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnNew;
        private System.Windows.Forms.ContextMenuStrip menuActions;
        private System.Windows.Forms.ToolStripMenuItem miNew;
        private System.Windows.Forms.ToolStripMenuItem miCancel;
        private System.Windows.Forms.ToolStripMenuItem miSave;
        private System.Windows.Forms.ToolStripSeparator sepDelete;
        private System.Windows.Forms.ToolStripMenuItem miDelete;

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
            this.components = new System.ComponentModel.Container();
            this.titlePanel = new System.Windows.Forms.Panel();
            this.lblTitle = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.spTitle = new System.Windows.Forms.Panel();
            this.searchCard = new Modern.Lab.WinForms.Controls.Layout.ModernCardPanel();
            this.lblFacId = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboFacId = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.lblEqpTyp = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboEqpTyp = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.lblDetEqpTyp = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboDetEqpTyp = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.lblEqpGrpId = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.cboEqpGrpId = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
            this.btnRefresh = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnReset = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.gapSearch = new System.Windows.Forms.Panel();
            this.splitMain = new Modern.Lab.WinForms.Controls.Layout.ModernSplitContainer();
            this.listCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.gridVirtualEquipments = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
            this.editorCard = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
            this.propertyGrid = new Modern.Lab.Hosting.MasterData.ModernPropertyGrid();
            this.actionPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.btnDelete = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnSave = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnCancel = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnDuplicate = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnNew = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.menuActions = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miNew = new System.Windows.Forms.ToolStripMenuItem();
            this.miCancel = new System.Windows.Forms.ToolStripMenuItem();
            this.miSave = new System.Windows.Forms.ToolStripMenuItem();
            this.sepDelete = new System.Windows.Forms.ToolStripSeparator();
            this.miDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.titlePanel.SuspendLayout();
            this.searchCard.SuspendLayout();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.listCard.SuspendLayout();
            this.editorCard.SuspendLayout();
            this.actionPanel.SuspendLayout();
            this.menuActions.SuspendLayout();
            this.SuspendLayout();
            //
            // titlePanel
            //
            this.titlePanel.Controls.Add(this.lblTitle);
            this.titlePanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.titlePanel.Location = new System.Drawing.Point(12, 12);
            this.titlePanel.Name = "titlePanel";
            this.titlePanel.Size = new System.Drawing.Size(1376, 28);
            this.titlePanel.TabIndex = 0;
            //
            // lblTitle
            //
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Title;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(1376, 28);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Virtual Equipment Master";
            this.lblTitle.TitleBar = true;
            //
            // spTitle
            //
            this.spTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.spTitle.Location = new System.Drawing.Point(12, 40);
            this.spTitle.Name = "spTitle";
            this.spTitle.Size = new System.Drawing.Size(1376, 8);
            this.spTitle.TabIndex = 1;
            //
            // searchCard
            //
            this.searchCard.Controls.Add(this.lblFacId);
            this.searchCard.Controls.Add(this.cboFacId);
            this.searchCard.Controls.Add(this.lblEqpTyp);
            this.searchCard.Controls.Add(this.cboEqpTyp);
            this.searchCard.Controls.Add(this.lblDetEqpTyp);
            this.searchCard.Controls.Add(this.cboDetEqpTyp);
            this.searchCard.Controls.Add(this.lblEqpGrpId);
            this.searchCard.Controls.Add(this.cboEqpGrpId);
            this.searchCard.Controls.Add(this.btnReset);
            this.searchCard.Controls.Add(this.btnRefresh);
            this.searchCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchCard.Location = new System.Drawing.Point(12, 48);
            this.searchCard.Name = "searchCard";
            this.searchCard.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.searchCard.Size = new System.Drawing.Size(1376, 56);
            this.searchCard.TabIndex = 2;
            //
            // lblFacId
            //
            this.lblFacId.BackColor = System.Drawing.Color.Transparent;
            this.lblFacId.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblFacId.Location = new System.Drawing.Point(12, 12);
            this.lblFacId.Name = "lblFacId";
            this.lblFacId.Size = new System.Drawing.Size(72, 32);
            this.lblFacId.TabIndex = 0;
            this.lblFacId.Text = "Fac Id";
            //
            // cboFacId
            //
            this.cboFacId.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cboFacId.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cboFacId.Location = new System.Drawing.Point(88, 12);
            this.cboFacId.Name = "cboFacId";
            this.cboFacId.PlaceholderText = "All";
            this.cboFacId.ShowClearButton = true;
            this.cboFacId.Size = new System.Drawing.Size(110, 32);
            this.cboFacId.TabIndex = 1;
            this.cboFacId.Child = null;
            //
            // lblEqpTyp
            //
            this.lblEqpTyp.BackColor = System.Drawing.Color.Transparent;
            this.lblEqpTyp.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblEqpTyp.Location = new System.Drawing.Point(214, 8);
            this.lblEqpTyp.Name = "lblEqpTyp";
            this.lblEqpTyp.Size = new System.Drawing.Size(72, 40);
            this.lblEqpTyp.TabIndex = 0;
            this.lblEqpTyp.Text = "Type";
            this.lblEqpTyp.TopLabel = "Equipment";
            //
            // cboEqpTyp
            //
            this.cboEqpTyp.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cboEqpTyp.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cboEqpTyp.Location = new System.Drawing.Point(290, 12);
            this.cboEqpTyp.Name = "cboEqpTyp";
            this.cboEqpTyp.PlaceholderText = "All";
            this.cboEqpTyp.ShowClearButton = true;
            this.cboEqpTyp.Size = new System.Drawing.Size(180, 32);
            this.cboEqpTyp.TabIndex = 2;
            this.cboEqpTyp.Child = null;
            //
            // lblDetEqpTyp
            //
            this.lblDetEqpTyp.BackColor = System.Drawing.Color.Transparent;
            this.lblDetEqpTyp.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblDetEqpTyp.Location = new System.Drawing.Point(486, 8);
            this.lblDetEqpTyp.Name = "lblDetEqpTyp";
            this.lblDetEqpTyp.Size = new System.Drawing.Size(72, 40);
            this.lblDetEqpTyp.TabIndex = 0;
            this.lblDetEqpTyp.Text = "Type";
            this.lblDetEqpTyp.TopLabel = "Detail Eqp";
            //
            // cboDetEqpTyp
            //
            this.cboDetEqpTyp.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cboDetEqpTyp.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cboDetEqpTyp.Location = new System.Drawing.Point(562, 12);
            this.cboDetEqpTyp.Name = "cboDetEqpTyp";
            this.cboDetEqpTyp.PlaceholderText = "All";
            this.cboDetEqpTyp.ShowClearButton = true;
            this.cboDetEqpTyp.Size = new System.Drawing.Size(180, 32);
            this.cboDetEqpTyp.TabIndex = 3;
            this.cboDetEqpTyp.Child = null;
            //
            // lblEqpGrpId
            //
            this.lblEqpGrpId.BackColor = System.Drawing.Color.Transparent;
            this.lblEqpGrpId.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
            this.lblEqpGrpId.Location = new System.Drawing.Point(758, 8);
            this.lblEqpGrpId.Name = "lblEqpGrpId";
            this.lblEqpGrpId.Size = new System.Drawing.Size(72, 40);
            this.lblEqpGrpId.TabIndex = 0;
            this.lblEqpGrpId.Text = "Group";
            this.lblEqpGrpId.TopLabel = "Equipment";
            //
            // cboEqpGrpId
            //
            this.cboEqpGrpId.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cboEqpGrpId.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cboEqpGrpId.Location = new System.Drawing.Point(834, 12);
            this.cboEqpGrpId.Name = "cboEqpGrpId";
            this.cboEqpGrpId.PlaceholderText = "All";
            this.cboEqpGrpId.ShowClearButton = true;
            this.cboEqpGrpId.Size = new System.Drawing.Size(180, 32);
            this.cboEqpGrpId.TabIndex = 4;
            this.cboEqpGrpId.Child = null;
            //
            // btnReset
            //
            this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReset.BackColor = System.Drawing.Color.Transparent;
            this.btnReset.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnReset.IconGlyph = "\uE777";
            this.btnReset.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Subtle;
            this.btnReset.Location = new System.Drawing.Point(1148, 12);
            this.btnReset.Size = new System.Drawing.Size(104, 32);
            this.btnReset.Text = "Reset";
            this.btnReset.Name = "btnReset";
            this.btnReset.TabIndex = 20;
            this.btnReset.Child = null;
            //
            // btnRefresh
            //
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.BackColor = System.Drawing.Color.Transparent;
            this.btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRefresh.IconGlyph = "\uE72C";
            this.btnRefresh.Location = new System.Drawing.Point(1260, 12);
            this.btnRefresh.Size = new System.Drawing.Size(104, 32);
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.TabIndex = 10;
            //
            // gapSearch
            //
            this.gapSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.gapSearch.Location = new System.Drawing.Point(12, 104);
            this.gapSearch.Name = "gapSearch";
            this.gapSearch.Size = new System.Drawing.Size(1376, 8);
            this.gapSearch.TabIndex = 3;
            //
            // splitMain
            //
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.KeepRatio = true;
            this.splitMain.Location = new System.Drawing.Point(12, 112);
            this.splitMain.Name = "splitMain";
            //
            // splitMain.Panel1
            //
            this.splitMain.Panel1.Controls.Add(this.listCard);
            //
            // splitMain.Panel2
            //
            this.splitMain.Panel2.Controls.Add(this.editorCard);
            //
            // splitMain
            //
            this.splitMain.Size = new System.Drawing.Size(1376, 676);
            this.splitMain.SplitterDistance = 800;
            this.splitMain.SplitterWidth = 8;
            this.splitMain.TabIndex = 4;
            this.splitMain.TabStop = false;
            //
            // listCard
            //
            this.listCard.Controls.Add(this.gridVirtualEquipments);
            this.listCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listCard.Location = new System.Drawing.Point(0, 0);
            this.listCard.Name = "listCard";
            this.listCard.Padding = new System.Windows.Forms.Padding(6, 40, 6, 6);
            this.listCard.Size = new System.Drawing.Size(800, 676);
            this.listCard.TabIndex = 0;
            this.listCard.Text = "Virtual Equipment List";
            this.listCard.TitleBar = true;
            //
            // gridVirtualEquipments
            //
            this.gridVirtualEquipments.AutoFitColumns = true;
            this.gridVirtualEquipments.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridVirtualEquipments.EmptyText = "No virtual equipment";
            this.gridVirtualEquipments.Location = new System.Drawing.Point(8, 40);
            this.gridVirtualEquipments.Name = "gridVirtualEquipments";
            this.gridVirtualEquipments.ContextMenuStrip = this.menuActions;
            this.gridVirtualEquipments.ReadOnly = true;
            this.gridVirtualEquipments.ShowStatusBar = true;
            this.gridVirtualEquipments.Size = new System.Drawing.Size(804, 628);
            this.gridVirtualEquipments.StatusCountFormat = "{0:N0} virtual equipment";
            this.gridVirtualEquipments.TabIndex = 0;
            this.gridVirtualEquipments.SelectionChanged += new System.EventHandler(this.OnVirtualEquipmentSelectionChanged);
            this.gridVirtualEquipments.Child = null;
            //
            // editorCard
            //
            this.editorCard.Controls.Add(this.propertyGrid);
            this.editorCard.Controls.Add(this.actionPanel);
            this.editorCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.editorCard.Location = new System.Drawing.Point(0, 0);
            this.editorCard.Name = "editorCard";
            this.editorCard.Padding = new System.Windows.Forms.Padding(12, 44, 12, 8);
            this.editorCard.Size = new System.Drawing.Size(568, 676);
            this.editorCard.TabIndex = 0;
            this.editorCard.Text = "Virtual Equipment";
            this.editorCard.TitleBar = true;
            //
            // propertyGrid
            //
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid.LabelWidth = 150;
            this.propertyGrid.Location = new System.Drawing.Point(14, 44);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(540, 572);
            this.propertyGrid.TabIndex = 0;
            //
            // actionPanel
            //
            this.actionPanel.Controls.Add(this.btnDelete);
            this.actionPanel.Controls.Add(this.btnSave);
            this.actionPanel.Controls.Add(this.btnCancel);
            this.actionPanel.Controls.Add(this.btnDuplicate);
            this.actionPanel.Controls.Add(this.btnNew);
            this.actionPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.actionPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.actionPanel.Location = new System.Drawing.Point(14, 616);
            this.actionPanel.Name = "actionPanel";
            this.actionPanel.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.actionPanel.Size = new System.Drawing.Size(540, 50);
            this.actionPanel.TabIndex = 1;
            this.actionPanel.WrapContents = false;
            //
            // btnDelete
            //
            this.btnDelete.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Danger;
            this.btnDelete.Location = new System.Drawing.Point(420, 8);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(100, 34);
            this.btnDelete.TabIndex = 4;
            this.btnDelete.Text = "Delete";
            //
            // btnSave
            //
            this.btnSave.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Execute;
            this.btnSave.Location = new System.Drawing.Point(312, 8);
            this.btnSave.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 34);
            this.btnSave.TabIndex = 3;
            this.btnSave.Text = "Save";
            //
            // btnCancel
            //
            this.btnCancel.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnCancel.Location = new System.Drawing.Point(204, 8);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 34);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            //
            // btnDuplicate
            //
            this.btnDuplicate.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnDuplicate.Location = new System.Drawing.Point(116, 8);
            this.btnDuplicate.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.btnDuplicate.Name = "btnDuplicate";
            this.btnDuplicate.Size = new System.Drawing.Size(100, 34);
            this.btnDuplicate.TabIndex = 1;
            this.btnDuplicate.Text = "Duplicate";
            //
            // btnNew
            //
            this.btnNew.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnNew.Location = new System.Drawing.Point(96, 8);
            this.btnNew.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(100, 34);
            this.btnNew.TabIndex = 0;
            this.btnNew.Text = "New";
            //
            // menuActions
            //
            this.menuActions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miNew,
            this.miCancel,
            this.miSave,
            this.sepDelete,
            this.miDelete});
            this.menuActions.Name = "menuActions";
            this.menuActions.Size = new System.Drawing.Size(153, 98);
            //
            // miNew
            //
            this.miNew.Name = "miNew";
            this.miNew.Size = new System.Drawing.Size(152, 22);
            this.miNew.Text = "New";
            //
            // miCancel
            //
            this.miCancel.Name = "miCancel";
            this.miCancel.Size = new System.Drawing.Size(152, 22);
            this.miCancel.Text = "Cancel";
            //
            // miSave
            //
            this.miSave.Name = "miSave";
            this.miSave.Size = new System.Drawing.Size(152, 22);
            this.miSave.Text = "Save";
            //
            // sepDelete
            //
            this.sepDelete.Name = "sepDelete";
            this.sepDelete.Size = new System.Drawing.Size(149, 6);
            //
            // miDelete
            //
            this.miDelete.Name = "miDelete";
            this.miDelete.Size = new System.Drawing.Size(152, 22);
            this.miDelete.Text = "Delete";
            //
            // VirtualEquipmentForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1400, 800);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.gapSearch);
            this.Controls.Add(this.searchCard);
            this.Controls.Add(this.spTitle);
            this.Controls.Add(this.titlePanel);
            this.MinimumSize = new System.Drawing.Size(1300, 600);
            this.Name = "VirtualEquipmentForm";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Virtual Equipment Master";
            this.titlePanel.ResumeLayout(false);
            this.searchCard.ResumeLayout(false);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            this.splitMain.ResumeLayout(false);
            this.listCard.ResumeLayout(false);
            this.editorCard.ResumeLayout(false);
            this.actionPanel.ResumeLayout(false);
            this.menuActions.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion
    }
}
