using System;
using System.ComponentModel;
using System.Windows.Forms;
using Modern.Lab.Hosting;

namespace Modern.Lab.Samples
{
    public partial class DurableScrapDialogForm : ModernFormBase
    {
        private readonly DurableScrapDialogOptions options;

        public DurableScrapDialogForm()
        {
            this.InitializeComponent();
            this.InitializeModernForm(false);
        }

        public DurableScrapDialogForm(DurableScrapDialogOptions options) : this()
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Description { get; private set; }

        private void OnFormLoad(object sender, EventArgs e)
        {
            if (IsDesignTime() || this.DesignMode || this.options == null)
            {
                return;
            }

            this.Text = this.options.Title ?? this.Text;
            this.btnOk.Text = this.options.OkText ?? this.btnOk.Text;
            this.fieldSource.DefineFields(DurableDialogHelpers.SourceFields());
            this.fieldSource.SetRow(this.options.Source);
            this.txtDescription.Text = this.options.DefaultDescription ?? string.Empty;
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            if (this.options == null || !this.btnOk.Enabled)
            {
                return;
            }

            this.Description = (this.txtDescription.Text ?? string.Empty).Trim();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void OnCancelClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
