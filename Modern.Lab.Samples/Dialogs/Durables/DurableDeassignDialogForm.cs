using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class DurableDeassignDialogForm : ModernFormBase
    {
        private readonly DurableDeassignDialogOptions options;

        public DurableDeassignDialogForm()
        {
            this.InitializeComponent();
            this.InitializeModernForm(false);
        }

        public DurableDeassignDialogForm(DurableDeassignDialogOptions options) : this()
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Description { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string[] LotIds { get; private set; }

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
            this.gridContents.RowKeyMember = Schemas.DurableWafer.RowKey;
            ContentColumns(this.options.Contents).Bind(this.gridContents);
            this.txtDescription.Text = this.options.DefaultDescription ?? string.Empty;
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            if (this.options == null || !this.btnOk.Enabled)
            {
                return;
            }

            string[] lotIds = DistinctLotIds(this.options.Contents);

            if (lotIds.Length == 0)
            {
                this.ShowWarningMessage("The durable has no lot to deassign.");
                return;
            }

            this.LotIds = lotIds;
            this.Description = this.txtDescription.Text ?? string.Empty;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void OnCancelClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        internal static string[] DistinctLotIds(DataTable contents)
        {
            List<string> lotIds = new List<string>();

            if (contents == null)
            {
                return lotIds.ToArray();
            }

            foreach (DataRow row in contents.Rows)
            {
                string lotId = Schemas.DurableWafer.LotId.Text(row);

                if (lotId.Length > 0 && !lotIds.Contains(lotId))
                {
                    lotIds.Add(lotId);
                }
            }

            return lotIds.ToArray();
        }
    }
}
