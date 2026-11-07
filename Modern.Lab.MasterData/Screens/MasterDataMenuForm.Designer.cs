namespace Modern.Lab.MasterData
{
    public partial class MasterDataMenuForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel titlePanel;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblTitle;
        private System.Windows.Forms.Panel spTitle;
        private Modern.Lab.WinForms.Controls.Layout.ModernCardPanel menuCard;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnProduct;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnFlow;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnOper;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnPfo;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnAreaBay;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnLotCode;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnPort;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnEquipmentGroup;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnCommonType;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnCommonCode;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnReasonCode;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnUser;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnSubCategories;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnFactory;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnEquipmentModel;
        private Modern.Lab.WinForms.Controls.Input.ModernButton btnVendor;
        private Modern.Lab.WinForms.Controls.Display.ModernLabel lblHint;

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
            this.titlePanel = new System.Windows.Forms.Panel();
            this.lblTitle = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.spTitle = new System.Windows.Forms.Panel();
            this.menuCard = new Modern.Lab.WinForms.Controls.Layout.ModernCardPanel();
            this.btnProduct = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnFlow = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnOper = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnPfo = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnAreaBay = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnLotCode = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnPort = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnEquipmentGroup = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnCommonType = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnCommonCode = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnReasonCode = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnUser = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnSubCategories = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnFactory = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnEquipmentModel = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.btnVendor = new Modern.Lab.WinForms.Controls.Input.ModernButton();
            this.lblHint = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
            this.titlePanel.SuspendLayout();
            this.menuCard.SuspendLayout();
            this.SuspendLayout();
            //
            // titlePanel
            //
            this.titlePanel.Controls.Add(this.lblTitle);
            this.titlePanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.titlePanel.Location = new System.Drawing.Point(12, 12);
            this.titlePanel.Name = "titlePanel";
            this.titlePanel.Size = new System.Drawing.Size(396, 28);
            this.titlePanel.TabIndex = 0;
            //
            // lblTitle
            //
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Title;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(396, 28);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Master Data";
            this.lblTitle.TitleBar = true;
            //
            // spTitle
            //
            this.spTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.spTitle.Location = new System.Drawing.Point(12, 40);
            this.spTitle.Name = "spTitle";
            this.spTitle.Size = new System.Drawing.Size(396, 8);
            this.spTitle.TabIndex = 1;
            //
            // menuCard
            //
            this.menuCard.Controls.Add(this.btnProduct);
            this.menuCard.Controls.Add(this.btnFlow);
            this.menuCard.Controls.Add(this.btnOper);
            this.menuCard.Controls.Add(this.btnPfo);
            this.menuCard.Controls.Add(this.btnAreaBay);
            this.menuCard.Controls.Add(this.btnLotCode);
            this.menuCard.Controls.Add(this.btnPort);
            this.menuCard.Controls.Add(this.btnEquipmentGroup);
            this.menuCard.Controls.Add(this.btnCommonType);
            this.menuCard.Controls.Add(this.btnCommonCode);
            this.menuCard.Controls.Add(this.btnReasonCode);
            this.menuCard.Controls.Add(this.btnUser);
            this.menuCard.Controls.Add(this.btnSubCategories);
            this.menuCard.Controls.Add(this.btnFactory);
            this.menuCard.Controls.Add(this.btnEquipmentModel);
            this.menuCard.Controls.Add(this.btnVendor);
            this.menuCard.Controls.Add(this.lblHint);
            this.menuCard.AutoScroll = true;
            this.menuCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.menuCard.Location = new System.Drawing.Point(12, 48);
            this.menuCard.Name = "menuCard";
            this.menuCard.Padding = new System.Windows.Forms.Padding(16, 16, 16, 16);
            this.menuCard.Size = new System.Drawing.Size(396, 644);
            this.menuCard.TabIndex = 2;
            //
            // btnProduct
            //
            this.btnProduct.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnProduct.Location = new System.Drawing.Point(20, 20);
            this.btnProduct.Name = "btnProduct";
            this.btnProduct.Size = new System.Drawing.Size(336, 44);
            this.btnProduct.TabIndex = 0;
            this.btnProduct.Text = "Product Master";
            this.btnProduct.Click += new System.EventHandler(this.OnProductClick);
            //
            // btnFlow
            //
            this.btnFlow.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnFlow.Location = new System.Drawing.Point(20, 76);
            this.btnFlow.Name = "btnFlow";
            this.btnFlow.Size = new System.Drawing.Size(336, 44);
            this.btnFlow.TabIndex = 1;
            this.btnFlow.Text = "Flow Master";
            this.btnFlow.Click += new System.EventHandler(this.OnFlowClick);
            //
            // btnOper
            //
            this.btnOper.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnOper.Location = new System.Drawing.Point(20, 132);
            this.btnOper.Name = "btnOper";
            this.btnOper.Size = new System.Drawing.Size(336, 44);
            this.btnOper.TabIndex = 2;
            this.btnOper.Text = "Oper Master";
            this.btnOper.Click += new System.EventHandler(this.OnOperClick);
            //
            // btnPfo
            //
            this.btnPfo.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnPfo.Location = new System.Drawing.Point(20, 188);
            this.btnPfo.Name = "btnPfo";
            this.btnPfo.Size = new System.Drawing.Size(336, 44);
            this.btnPfo.TabIndex = 3;
            this.btnPfo.Text = "PFO Node";
            this.btnPfo.Click += new System.EventHandler(this.OnPfoClick);
            //
            // btnAreaBay
            //
            this.btnAreaBay.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnAreaBay.Location = new System.Drawing.Point(20, 244);
            this.btnAreaBay.Name = "btnAreaBay";
            this.btnAreaBay.Size = new System.Drawing.Size(336, 44);
            this.btnAreaBay.TabIndex = 4;
            this.btnAreaBay.Text = "Area / Bay Master";
            this.btnAreaBay.Click += new System.EventHandler(this.OnAreaBayClick);
            //
            // btnLotCode
            //
            this.btnLotCode.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnLotCode.Location = new System.Drawing.Point(20, 300);
            this.btnLotCode.Name = "btnLotCode";
            this.btnLotCode.Size = new System.Drawing.Size(336, 44);
            this.btnLotCode.TabIndex = 5;
            this.btnLotCode.Text = "Lot Code Master";
            this.btnLotCode.Click += new System.EventHandler(this.OnLotCodeClick);
            //
            // btnPort
            //
            this.btnPort.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnPort.Location = new System.Drawing.Point(20, 356);
            this.btnPort.Name = "btnPort";
            this.btnPort.Size = new System.Drawing.Size(336, 44);
            this.btnPort.TabIndex = 6;
            this.btnPort.Text = "Port Master";
            this.btnPort.Click += new System.EventHandler(this.OnPortClick);
            //
            // btnEquipmentGroup
            //
            this.btnEquipmentGroup.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnEquipmentGroup.Location = new System.Drawing.Point(20, 412);
            this.btnEquipmentGroup.Name = "btnEquipmentGroup";
            this.btnEquipmentGroup.Size = new System.Drawing.Size(336, 44);
            this.btnEquipmentGroup.TabIndex = 7;
            this.btnEquipmentGroup.Text = "Equipment Group Master";
            this.btnEquipmentGroup.Click += new System.EventHandler(this.OnEquipmentGroupClick);
            //
            // btnCommonType
            //
            this.btnCommonType.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnCommonType.Location = new System.Drawing.Point(20, 468);
            this.btnCommonType.Name = "btnCommonType";
            this.btnCommonType.Size = new System.Drawing.Size(336, 44);
            this.btnCommonType.TabIndex = 8;
            this.btnCommonType.Text = "Common Type Master";
            this.btnCommonType.Click += new System.EventHandler(this.OnCommonTypeClick);
            //
            // btnCommonCode
            //
            this.btnCommonCode.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnCommonCode.Location = new System.Drawing.Point(20, 524);
            this.btnCommonCode.Name = "btnCommonCode";
            this.btnCommonCode.Size = new System.Drawing.Size(336, 44);
            this.btnCommonCode.TabIndex = 9;
            this.btnCommonCode.Text = "Common Code Master";
            this.btnCommonCode.Click += new System.EventHandler(this.OnCommonCodeClick);
            //
            // btnReasonCode
            //
            this.btnReasonCode.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnReasonCode.Location = new System.Drawing.Point(20, 580);
            this.btnReasonCode.Name = "btnReasonCode";
            this.btnReasonCode.Size = new System.Drawing.Size(336, 44);
            this.btnReasonCode.TabIndex = 10;
            this.btnReasonCode.Text = "Reason Code Master";
            this.btnReasonCode.Click += new System.EventHandler(this.OnReasonCodeClick);
            //
            // btnUser
            //
            this.btnUser.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnUser.Location = new System.Drawing.Point(20, 636);
            this.btnUser.Name = "btnUser";
            this.btnUser.Size = new System.Drawing.Size(336, 44);
            this.btnUser.TabIndex = 11;
            this.btnUser.Text = "User Master";
            this.btnUser.Click += new System.EventHandler(this.OnUserClick);
            //
            // btnSubCategories
            //
            this.btnSubCategories.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnSubCategories.Location = new System.Drawing.Point(20, 692);
            this.btnSubCategories.Name = "btnSubCategories";
            this.btnSubCategories.Size = new System.Drawing.Size(336, 44);
            this.btnSubCategories.TabIndex = 12;
            this.btnSubCategories.Text = "Sub Category Master";
            this.btnSubCategories.Click += new System.EventHandler(this.OnSubCategoriesClick);
            //
            // btnFactory
            //
            this.btnFactory.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnFactory.Location = new System.Drawing.Point(20, 748);
            this.btnFactory.Name = "btnFactory";
            this.btnFactory.Size = new System.Drawing.Size(336, 44);
            this.btnFactory.TabIndex = 13;
            this.btnFactory.Text = "Factory Master";
            this.btnFactory.Click += new System.EventHandler(this.OnFactoryClick);
            //
            // btnEquipmentModel
            //
            this.btnEquipmentModel.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnEquipmentModel.Location = new System.Drawing.Point(20, 804);
            this.btnEquipmentModel.Name = "btnEquipmentModel";
            this.btnEquipmentModel.Size = new System.Drawing.Size(336, 44);
            this.btnEquipmentModel.TabIndex = 14;
            this.btnEquipmentModel.Text = "Equipment Model Master";
            this.btnEquipmentModel.Click += new System.EventHandler(this.OnEquipmentModelClick);
            //
            // btnVendor
            //
            this.btnVendor.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;
            this.btnVendor.Location = new System.Drawing.Point(20, 860);
            this.btnVendor.Name = "btnVendor";
            this.btnVendor.Size = new System.Drawing.Size(336, 44);
            this.btnVendor.TabIndex = 15;
            this.btnVendor.Text = "Vendor Master";
            this.btnVendor.Click += new System.EventHandler(this.OnVendorClick);
            //
            // lblHint
            //
            this.lblHint.BackColor = System.Drawing.Color.Transparent;
            this.lblHint.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Helper;
            this.lblHint.Location = new System.Drawing.Point(20, 920);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(336, 50);
            this.lblHint.TabIndex = 16;
            this.lblHint.Text = "Each screen loads its own list. Screens that cannot reach their server stay read-only.";
            //
            // MasterDataMenuForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(420, 704);
            this.Controls.Add(this.menuCard);
            this.Controls.Add(this.spTitle);
            this.Controls.Add(this.titlePanel);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(436, 743);
            this.Name = "MasterDataMenuForm";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Master Data";
            this.titlePanel.ResumeLayout(false);
            this.menuCard.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion
    }
}
