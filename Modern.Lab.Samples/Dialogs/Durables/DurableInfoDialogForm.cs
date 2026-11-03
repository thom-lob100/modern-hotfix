using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Data;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class DurableInfoDialogForm : ModernFormBase
    {
        private const string LookupHint = "Try again, or contact support.";

        public DurableInfoDialogForm()
        {
            this.InitializeComponent();
            this.InitializeModernForm(false);
        }

        public DurableInfoDialogForm(string durableId) : this()
        {
            this.DurableId = (durableId ?? string.Empty).Trim();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DurableId { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Loaded { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataRow Durable { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataTable Slots { get; private set; }

        internal Modern.Lab.WinForms.Controls.Display.ModernSlotMapMini SlotMap
        {
            get { return this.slotMap; }
        }

        internal static string LinkedDurableId(Modern.Lab.Controls.Wpf.Data.GridButtonClickEventArgs e, params string[] columns)
        {
            DataRowView row = e == null ? null : e.Item as DataRowView;

            if (row == null || columns == null)
            {
                return string.Empty;
            }

            foreach (string column in columns)
            {
                if (string.Equals(e.DataPropertyName, column, StringComparison.OrdinalIgnoreCase))
                {
                    return TableHelper.CellText(row.Row, e.DataPropertyName).Trim();
                }
            }

            return string.Empty;
        }

        internal static void ShowFor(IWin32Window owner, string durableId)
        {
            string key = (durableId ?? string.Empty).Trim();

            if (key.Length == 0)
            {
                return;
            }

            using (DurableInfoDialogForm dialog = new DurableInfoDialogForm(key))
            {
                dialog.ShowDialog(owner);
            }
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            if (IsDesignTime() || this.DesignMode || string.IsNullOrEmpty(this.DurableId))
            {
                return;
            }

            this.Text = "Durable Info - " + this.DurableId;
            this.fieldDurable.DefineFields(DurableDialogHelpers.SourceFields());

            if (!this.LoadDurable())
            {
                this.CloseLater();
            }
        }

        internal bool LoadDurable()
        {
            this.Loaded = false;

            DataTable durables;

            try
            {
                durables = this.GetDurable(RequestParameters.Of(Schemas.Durable.DurableId, this.DurableId));
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage(
                        "Durable lookup failed",
                        "The durable information could not be loaded. " + LookupHint,
                        this.ServerFailureText(ex));
                return false;
            }

            DataRow durable = FindDurable(durables, this.DurableId);

            if (durable == null)
            {
                this.ShowErrorMessage(
                        "Durable not found",
                        "Durable " + this.DurableId + " was not found. Refresh the list and try again.");
                return false;
            }

            DataTable slots;

            try
            {
                slots = this.GetDurableSlots(RequestParameters.Of(Schemas.Durable.DurableId, this.DurableId));
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage(
                        "Slot lookup failed",
                        "The slot map could not be loaded. " + LookupHint,
                        this.ServerFailureText(ex));
                return false;
            }

            this.Durable = durable;
            this.Slots = slots;
            this.fieldDurable.SetRow(durable);
            this.slotMap.SlotCapacity = Capacity(durable, Schemas.Durable.FoupCapa.Name);
            this.slotMap.StubCapacity = Capacity(durable, Schemas.Durable.StubCapa.Name);
            this.slotMap.LccCapacity = Capacity(durable, Schemas.Durable.LccCapa.Name);
            this.slotMap.DataSource = slots;
            this.Loaded = true;
            return true;
        }

        private static DataRow FindDurable(DataTable durables, string durableId)
        {
            if (durables == null)
            {
                return null;
            }

            foreach (DataRow row in durables.Rows)
            {
                if (string.Equals(
                        TableHelper.CellText(row, Schemas.Durable.DurableId.Name).Trim(),
                        durableId,
                        StringComparison.Ordinal))
                {
                    return row;
                }
            }

            return null;
        }

        private static int Capacity(DataRow durable, string column)
        {
            return TableHelper.ParseInt(TableHelper.CellText(durable, column));
        }

        private void CloseLater()
        {
            if (!this.IsHandleCreated)
            {
                return;
            }

            this.BeginInvoke(new MethodInvoker(delegate
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }));
        }

        private void OnCloseClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
