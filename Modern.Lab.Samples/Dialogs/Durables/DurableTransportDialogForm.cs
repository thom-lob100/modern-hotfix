using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Samples.Contracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Selection;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class DurableTransportDialogForm : ModernFormBase
    {
        private const string LookupHint = "Select again, or contact support.";

        private readonly DurableTransportDialogOptions options;
        private bool cascading;

        public DurableTransportDialogForm()
        {
            this.InitializeComponent();
            this.InitializeModernForm(false);
        }

        public DurableTransportDialogForm(DurableTransportDialogOptions options) : this()
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string LocNm { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Destination { get; private set; }

        public string Description { get; private set; }

        private void OnFormLoad(object sender, EventArgs e)
        {
            if (IsDesignTime() || this.DesignMode || this.options == null)
            {
                return;
            }

            this.Text = this.options.Title ?? this.Text;
            this.btnOk.Text = this.options.OkText ?? this.btnOk.Text;
            this.fieldSource.DefineFields(SourceFields());
            this.fieldSource.SetRow(this.options.Source);
            this.txtDescription.Text = Services.DefaultDescriptions.TransportDurable;

            this.cboLocNm.UseEntity(Schemas.CommonTypeDet);
            this.cboArea.UseEntity(Schemas.Area);
            this.cboArea.Enabled = false;

            this.cascading = true;

            try
            {
                if (!this.LoadLocNmCodes())
                {
                    return;
                }

                string current = (this.options.CurrentLocNm ?? string.Empty).Trim();

                if (current.Length > 0)
                {
                    this.cboLocNm.SelectedValue = current;
                }

                if (current.Length == 0 || this.SelectedValue(this.cboLocNm) != current)
                {
                    this.cboLocNm.SelectedIndex = -1;
                }

                this.ApplyLocNm();
            }
            finally
            {
                this.cascading = false;
            }
        }

        private void OnLocNmChanged(object sender, EventArgs e)
        {
            if (IsDesignTime() || this.DesignMode || this.options == null || this.cascading)
            {
                return;
            }

            this.cascading = true;

            try
            {
                this.ApplyLocNm();
            }
            finally
            {
                this.cascading = false;
            }
        }

        private void OnAreaChanged(object sender, EventArgs e)
        {
            if (IsDesignTime() || this.DesignMode || this.options == null || this.cascading)
            {
                return;
            }

            this.cascading = true;

            try
            {
                this.LoadBays();
            }
            finally
            {
                this.cascading = false;
            }
        }

        private bool LoadLocNmCodes()
        {
            DataTable codes;

            try
            {
                codes = this.GetLocNmCodes();
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage(
                        "Location lookup failed",
                        "Locations could not be loaded. " + LookupHint,
                        this.ServerFailureText(ex));
                return false;
            }

            this.cboLocNm.DataSource = codes;
            return true;
        }

        private void ApplyLocNm()
        {
            this.cboDestination.DataSource = null;
            this.cboArea.DataSource = null;

            string locNm = this.SelectedValue(this.cboLocNm);
            bool bay = locNm == Schemas.Durable.LocNm.InBay.Value;

            this.cboArea.Enabled = bay;

            if (locNm.Length == 0)
            {
                return;
            }

            if (bay)
            {
                this.LoadAreas();
            }
            else
            {
                this.LoadEquipments(locNm);
            }
        }

        private void LoadEquipments(string locNm)
        {
            DataTable equipments;

            try
            {
                equipments = this.GetEquipments(locNm);
            }
            catch (Exception ex)
            {
                this.ClearLater(this.cboLocNm);
                this.ShowErrorMessage(
                        "Destination lookup failed",
                        "Destinations could not be loaded. " + LookupHint,
                        this.ServerFailureText(ex));
                return;
            }

            this.FillDestinations(equipments, Schemas.Equipment.ValueField.Name);
        }

        private void LoadAreas()
        {
            DataTable areas;

            try
            {
                areas = this.GetAreas();
            }
            catch (Exception ex)
            {
                this.cboArea.Enabled = false;
                this.ClearLater(this.cboLocNm);
                this.ShowErrorMessage(
                        "Area lookup failed",
                        "Areas could not be loaded. " + LookupHint,
                        this.ServerFailureText(ex));
                return;
            }

            this.cboArea.DataSource = areas;
            this.cboArea.SelectedIndex = -1;
        }

        private void LoadBays()
        {
            this.cboDestination.DataSource = null;

            string areaId = this.SelectedValue(this.cboArea);

            if (areaId.Length == 0)
            {
                return;
            }

            DataTable bays;

            try
            {
                bays = this.GetBays(areaId);
            }
            catch (Exception ex)
            {
                this.ClearLater(this.cboArea);
                this.ShowErrorMessage(
                        "Destination lookup failed",
                        "Destinations could not be loaded. " + LookupHint,
                        this.ServerFailureText(ex));
                return;
            }

            this.FillDestinations(bays, Schemas.Area.ValueField.Name);
        }

        private void OnDestinationChanged(object sender, EventArgs e)
        {
            DataRowView selected = this.cboDestination.SelectedItem as DataRowView;
            this.fieldDestination.ClearValues();
            if (selected == null)
            {
                return;
            }

            DestinationDefinitions(selected.Row.Table).Apply(this.fieldDestination);
            this.fieldDestination.SetRow(selected.Row);
        }

        private void FillDestinations(DataTable rows, string idColumn)
        {
            this.cboDestination.DisplayMember = idColumn;
            this.cboDestination.ValueMember = idColumn;
            this.cboDestination.DataSource = DurableManagementPresenter.TransportTargets(
                    rows, idColumn, this.options.CurrentLocationId);
            this.cboDestination.SelectedIndex = -1;
        }

        private void ClearLater(ModernComboBox combo)
        {
            this.BeginInvoke(new MethodInvoker(delegate
            {
                bool previous = this.cascading;
                this.cascading = true;

                try
                {
                    combo.SelectedIndex = -1;
                }
                finally
                {
                    this.cascading = previous;
                }
            }));
        }

        private string SelectedValue(ModernComboBox combo)
        {
            return (Convert.ToString(combo.SelectedValue) ?? string.Empty).Trim();
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            if (this.options == null || !this.btnOk.Enabled)
            {
                return;
            }

            if (this.cboDestination.SelectedValue == null)
            {
                this.ShowWarningMessage("Select a destination first.");
                return;
            }

            this.LocNm = this.SelectedValue(this.cboLocNm);
            this.Destination = this.SelectedValue(this.cboDestination);
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
