using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Modern.Lab.Hosting;

namespace Modern.Lab.MasterData
{
    public partial class MasterDataMenuForm : ModernFormBase
    {
        private readonly Dictionary<string, Form> openScreens = new Dictionary<string, Form>(StringComparer.Ordinal);

        public MasterDataMenuForm()
        {
            this.InitializeComponent();
            this.InitializeModernForm();
        }

        public static Form Create(string screen)
        {
            if (string.Equals(screen, "reasoncode", StringComparison.OrdinalIgnoreCase)) { return new ReasonCodeForm(); }
            if (string.Equals(screen, "user", StringComparison.OrdinalIgnoreCase)) { return new UserForm(); }

            if (string.Equals(screen, "commontype", StringComparison.OrdinalIgnoreCase))
            {
                return new CommonCodeForm(true);
            }

            if (string.Equals(screen, "commoncode", StringComparison.OrdinalIgnoreCase))
            {
                return new CommonCodeForm();
            }

            if (string.Equals(screen, "equipmentgroup", StringComparison.OrdinalIgnoreCase))
            {
                return new EquipmentGroupForm();
            }

            if (string.Equals(screen, "port", StringComparison.OrdinalIgnoreCase))
            {
                return new PortForm();
            }

            if (string.Equals(screen, "areabay", StringComparison.OrdinalIgnoreCase))
            {
                return new AreaBayForm();
            }

            if (string.Equals(screen, "flow", StringComparison.OrdinalIgnoreCase))
            {
                return new FlowForm();
            }

            if (string.Equals(screen, "oper", StringComparison.OrdinalIgnoreCase))
            {
                return new OperForm();
            }

            if (string.Equals(screen, "product", StringComparison.OrdinalIgnoreCase))
            {
                return new ProductForm();
            }

            if (string.Equals(screen, "pfo", StringComparison.OrdinalIgnoreCase))
            {
                return new PfoNodeForm();
            }

            if (string.Equals(screen, "lotcode", StringComparison.OrdinalIgnoreCase))
            {
                return new LotCodeForm();
            }

            if (string.Equals(screen, "subcategories", StringComparison.OrdinalIgnoreCase))
            {
                return new SubCategoriesForm();
            }

            if (string.Equals(screen, "factory", StringComparison.OrdinalIgnoreCase))
            {
                return new FactoryForm();
            }

            if (string.Equals(screen, "equipmentmodel", StringComparison.OrdinalIgnoreCase))
            {
                return new EquipmentModelForm();
            }

            if (string.Equals(screen, "vendor", StringComparison.OrdinalIgnoreCase))
            {
                return new VendorForm();
            }

            return null;
        }

        private void OnReasonCodeClick(object sender, EventArgs e) { this.OpenScreen("reasoncode"); }
        private void OnUserClick(object sender, EventArgs e) { this.OpenScreen("user"); }

        private void OnLotCodeClick(object sender, EventArgs e)
        {
            this.OpenScreen("lotcode");
        }

        private void OnSubCategoriesClick(object sender, EventArgs e)
        {
            this.OpenScreen("subcategories");
        }

        private void OnFactoryClick(object sender, EventArgs e)
        {
            this.OpenScreen("factory");
        }

        private void OnEquipmentModelClick(object sender, EventArgs e)
        {
            this.OpenScreen("equipmentmodel");
        }

        private void OnVendorClick(object sender, EventArgs e)
        {
            this.OpenScreen("vendor");
        }

        private void OnCommonTypeClick(object sender, EventArgs e)
        {
            this.OpenScreen("commontype");
        }

        private void OnCommonCodeClick(object sender, EventArgs e)
        {
            this.OpenScreen("commoncode");
        }

        private void OnEquipmentGroupClick(object sender, EventArgs e)
        {
            this.OpenScreen("equipmentgroup");
        }

        private void OnPortClick(object sender, EventArgs e)
        {
            this.OpenScreen("port");
        }

        private void OnProductClick(object sender, EventArgs e)
        {
            this.OpenScreen("product");
        }

        private void OnAreaBayClick(object sender, EventArgs e)
        {
            this.OpenScreen("areabay");
        }

        private void OnFlowClick(object sender, EventArgs e)
        {
            this.OpenScreen("flow");
        }

        private void OnOperClick(object sender, EventArgs e)
        {
            this.OpenScreen("oper");
        }

        private void OnPfoClick(object sender, EventArgs e)
        {
            this.OpenScreen("pfo");
        }

        private void OpenScreen(string screen)
        {
            Form existing;

            if (this.openScreens.TryGetValue(screen, out existing) && !existing.IsDisposed)
            {
                if (existing.WindowState == FormWindowState.Minimized)
                {
                    existing.WindowState = FormWindowState.Normal;
                }

                existing.Activate();
                return;
            }

            Form created = Create(screen);

            if (created == null)
            {
                return;
            }

            this.openScreens[screen] = created;
            created.FormClosed += (s, e) => this.openScreens.Remove(screen);
            created.Show(this);
        }
    }
}
