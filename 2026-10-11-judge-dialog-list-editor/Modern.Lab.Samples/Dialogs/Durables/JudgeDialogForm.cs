using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Data;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Samples.Services;

namespace Modern.Lab.Samples
{
    public partial class JudgeDialogForm : ModernFormBase
    {
        private const string rowsChannel = "judgeResults";
        private readonly JudgeDialogOptions options;
        private DataTable judgeResults;
        private DataRow editingRow;
        private bool ready;
        private bool closed;
        private bool refreshPending;
        private bool targetSetup;
        private string goalDurableId = string.Empty;

        public JudgeDialogForm()
        {
            this.InitializeComponent();
            this.InitializeModernForm(false);
        }

        public JudgeDialogForm(JudgeDialogOptions options) : this()
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public JudgeDialogResult Result { get; private set; }

        private JudgeKind Kind
        {
            get { return this.options == null ? JudgeKind.None : this.options.Kind; }
        }

        private async void OnFormLoad(object sender, EventArgs e)
        {
            if (IsDesignTime() || this.DesignMode || this.options == null)
            {
                return;
            }

            this.Text = this.options.Title ?? this.Text;
            this.btnOk.Text = this.options.OkText ?? this.btnOk.Text;
            this.ApplySource();
            this.inputPanel.Visible = this.options.ShowDescription;
            this.txtDescription.Text = this.options.DefaultDescription ?? string.Empty;
            this.judgeCard.Text = this.Kind == JudgeKind.Chip ? "Chip Judge" : "Lamella Judge";
            this.DefineJudgeEditors();
            this.SetReady(false);
            this.ApplyTargets();
            this.headerPanel.Visible = this.targetPanel.Visible || this.inputPanel.Visible;
            await this.LoadJudgeRowsAsync();
        }

        private void ApplyTargets()
        {
            this.goalDurableId = (this.options.GoalDurableId ?? string.Empty).Trim();
            this.targetPanel.Visible = this.options.Targets != null;

            if (this.options.Targets == null)
            {
                return;
            }

            string member = string.IsNullOrEmpty(this.options.TargetMember)
                    ? Schemas.Durable.ValueField.Name
                    : this.options.TargetMember;
            this.targetSetup = true;

            try
            {
                this.cboTarget.DisplayMember = member;
                this.cboTarget.ValueMember = member;
                this.cboTarget.DataSource = this.options.Targets;

                if (this.goalDurableId.Length > 0)
                {
                    this.cboTarget.SelectedValue = this.goalDurableId;
                }
            }
            finally
            {
                this.targetSetup = false;
            }

            this.goalDurableId = (Convert.ToString(this.cboTarget.SelectedValue) ?? string.Empty).Trim();
        }

        private async void OnTargetChanged(object sender, EventArgs e)
        {
            if (this.targetSetup || this.closed || this.options == null)
            {
                return;
            }

            string selected = (Convert.ToString(this.cboTarget.SelectedValue) ?? string.Empty).Trim();

            if (selected == this.goalDurableId)
            {
                return;
            }

            this.goalDurableId = selected;
            await this.LoadJudgeRowsAsync();
        }

        private async System.Threading.Tasks.Task LoadJudgeRowsAsync()
        {
            this.SetReady(false);

            if (this.judgeResults != null)
            {
                this.judgeResults.ColumnChanged -= this.OnJudgeColumnChanged;
                this.judgeResults = null;
                this.gridJudge.DataSource = null;
            }

            this.editingRow = null;
            this.editorJudge.ClearValues();
            this.UpdateEditorButtons();

            string goalDurableId = this.goalDurableId;

            if (this.Kind == JudgeKind.None || goalDurableId.Length == 0)
            {
                this.lblStatus.Text = this.options.Targets == null
                        ? "The target tray is not known. Close and try again."
                        : (this.options.Targets.Rows.Count == 0 ? "No tray is available." : "Select a tray.");
                return;
            }

            this.lblStatus.Text = "Loading judge rows…";

            LoadOutcome<DataTable> outcome = await this.FetchAsync(rowsChannel,
                    () => this.GetJudgeResults(goalDurableId, (this.options.ReqSerialNo ?? string.Empty).Trim()));

            if (!outcome.IsCurrent || this.closed || this.IsDisposed || goalDurableId != this.goalDurableId)
            {
                return;
            }

            if (outcome.Failure != null)
            {
                this.lblStatus.Text = "The judge rows could not be loaded. Close and try again.";
                return;
            }

            DataTable rows = outcome.Value;
            if (rows == null)
            {
                this.lblStatus.Text = "The judge rows could not be used.";
                this.ShowErrorMessage("Judge rows", this.lblStatus.Text, "Judge rows — the response has no table");
                return;
            }

            this.BindRows(rows);
            this.SetReady(true);
        }

        private void ApplySource()
        {
            if (this.options.Source == null || this.options.SourceFields == null)
            {
                this.sourceCard.Visible = false;
                return;
            }

            this.sourceCard.Text = this.options.SourceCaption ?? this.Text;
            this.fieldSource.DefineFields(this.options.SourceFields);
            this.fieldSource.SetRow(this.options.Source);
        }

        private void BindRows(DataTable rows)
        {
            DataTable table = rows.Copy();
            EnsureColumn(table, Schemas.JudgeResult.SlotNo.Name);
            EnsureColumn(table, Schemas.JudgeResult.FingerId.Name);
            EnsureColumn(table, Schemas.JudgeResult.FingerPosition.Name);
            EnsureColumn(table, Schemas.JudgeResult.JudgeRslt.Name);
            EnsureColumn(table, Schemas.JudgeResult.GridIndex.Name);
            EnsureColumn(table, Schemas.JudgeResult.DurableTyp.Name);
            EnsureColumn(table, Schemas.JudgeResult.DetDurableTyp.Name);

            string detType = this.Kind == JudgeKind.Chip
                    ? Schemas.JudgeResult.DetDurableTyp.Stub.Value
                    : Schemas.JudgeResult.DetDurableTyp.Lcc.Value;

            foreach (DataRow row in table.Rows)
            {
                if (this.Kind == JudgeKind.Chip)
                {
                    row[Schemas.JudgeResult.FingerId.Name] = ChipFingerId;
                    row[Schemas.JudgeResult.FingerPosition.Name] = string.Empty;
                }

                if (Schemas.JudgeResult.DurableTyp.Text(row).Length == 0)
                {
                    row[Schemas.JudgeResult.DurableTyp.Name] = Schemas.JudgeResult.DurableTyp.Tray.Value;
                }

                if (Schemas.JudgeResult.DetDurableTyp.Text(row).Length == 0)
                {
                    row[Schemas.JudgeResult.DetDurableTyp.Name] = detType;
                }
            }

            this.judgeResults = table;
            this.gridJudge.ConfigureColumns(JudgeColumns(table, this.Kind));
            this.gridJudge.DataSource = table;
            this.editorJudge.SetSchema(EditorSchema(table, this.Kind));
            this.LockServerColumns();
            table.ColumnChanged += this.OnJudgeColumnChanged;
            this.LoadSelectedRow();
        }

        private static void EnsureColumn(DataTable table, string name)
        {
            if (!table.Columns.Contains(name))
            {
                table.Columns.Add(name, typeof(string));
            }
        }

        private void OnJudgeColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            this.QueueRefresh();
        }

        private void QueueRefresh()
        {
            if (this.closed || this.IsDisposed || !this.IsHandleCreated || this.refreshPending)
            {
                return;
            }

            this.refreshPending = true;
            this.BeginInvoke(new MethodInvoker(delegate
            {
                this.refreshPending = false;

                if (!this.closed && !this.IsDisposed)
                {
                    this.RefreshState();
                }
            }));
        }

        private void SetReady(bool value)
        {
            this.ready = value;
            this.RefreshState();
        }

        private void OnJudgeRowSelected(object sender, EventArgs e)
        {
            if (this.closed || this.IsDisposed)
            {
                return;
            }

            this.LoadSelectedRow();
        }

        private void LoadSelectedRow()
        {
            DataRow row = this.judgeResults == null ? null : SelectedRow(this.gridJudge.SelectedItem);
            this.editingRow = row;

            if (row == null)
            {
                this.editorJudge.ClearValues();
            }
            else
            {
                this.editorJudge.BeginNew(RowValues(row));
            }

            this.UpdateEditorButtons();
        }

        private static DataRow SelectedRow(object item)
        {
            DataRowView view = item as DataRowView;

            if (view != null)
            {
                return view.Row;
            }

            return item as DataRow;
        }

        private static Dictionary<string, string> RowValues(DataRow row)
        {
            Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (DataColumn column in row.Table.Columns)
            {
                values[column.ColumnName] = TableHelper.CellText(row, column.ColumnName);
            }

            return values;
        }

        private void OnEditorValueChanged(object sender, EventArgs e)
        {
            this.UpdateEditorButtons();
        }

        private void UpdateEditorButtons()
        {
            bool editable = this.ready && !this.closed && this.editingRow != null && this.editorJudge.IsDirty;
            this.btnApply.Enabled = editable;
            this.btnRevert.Enabled = editable;
        }

        private void OnApplyClick(object sender, EventArgs e)
        {
            DataRow row = this.editingRow;

            if (row == null || !this.ready || this.closed)
            {
                return;
            }

            string[] missing = this.editorJudge.MissingRequired();

            if (missing.Length > 0)
            {
                List<string> captions = new List<string>();

                foreach (string column in missing)
                {
                    captions.Add(this.editorJudge.CaptionOf(column));
                }

                this.ShowToast("Required: " + string.Join(", ", captions.ToArray()), Modern.Lab.Controls.Wpf.Display.ToastKind.Warning);
                this.editorJudge.FocusFirstEditor();
                return;
            }

            foreach (KeyValuePair<string, string> value in this.editorJudge.ReadValues())
            {
                if (row.Table.Columns.Contains(value.Key))
                {
                    row[value.Key] = (value.Value ?? string.Empty).Trim();
                }
            }

            if (this.Kind == JudgeKind.Chip)
            {
                row[Schemas.JudgeResult.FingerId.Name] = ChipFingerId;
            }

            this.editorJudge.BeginNew(RowValues(row));
            this.UpdateEditorButtons();
            this.RefreshState();
        }

        private void OnRevertClick(object sender, EventArgs e)
        {
            this.editorJudge.RevertEdits();
            this.UpdateEditorButtons();
        }

        private string[] RequiredInputs()
        {
            return this.Kind == JudgeKind.Lamella
                    ? new string[]
                    {
                        Schemas.JudgeResult.SlotNo.Name, Schemas.JudgeResult.FingerId.Name, Schemas.JudgeResult.FingerPosition.Name,
                        Schemas.JudgeResult.JudgeRslt.Name, Schemas.JudgeResult.GridIndex.Name
                    }
                    : new string[]
                    {
                        Schemas.JudgeResult.SlotNo.Name, Schemas.JudgeResult.JudgeRslt.Name, Schemas.JudgeResult.GridIndex.Name
                    };
        }

        private string Problem()
        {
            if (this.judgeResults == null)
            {
                return null;
            }

            if (this.judgeResults.Rows.Count == 0)
            {
                return "There are no rows to judge.";
            }

            string[] required = this.RequiredInputs();
            int incomplete = 0;
            Dictionary<string, int> positions = new Dictionary<string, int>(StringComparer.Ordinal);
            string firstDuplicate = null;
            int index = 0;

            foreach (DataRow row in this.judgeResults.Rows)
            {
                index++;
                bool filled = true;

                foreach (string name in required)
                {
                    filled = filled && TableHelper.CellText(row, name).Trim().Length > 0;
                }

                if (!filled)
                {
                    incomplete++;
                    continue;
                }

                string position = Schemas.JudgeResult.DurableId.Text(row) + "|" + Schemas.JudgeResult.SlotNo.Text(row)
                        + "|" + Schemas.JudgeResult.FingerId.Text(row);
                int first;

                if (positions.TryGetValue(position, out first))
                {
                    firstDuplicate = firstDuplicate ?? first + " and " + index;
                }
                else
                {
                    positions[position] = index;
                }
            }

            if (incomplete > 0)
            {
                return incomplete == 1 ? "1 row still needs input." : incomplete + " rows still need input.";
            }

            if (firstDuplicate != null)
            {
                return "Rows " + firstDuplicate + " use the same "
                        + (this.Kind == JudgeKind.Lamella ? "slot and finger." : "slot.");
            }

            return null;
        }

        private void RefreshState()
        {
            string problem = this.ready ? this.Problem() : null;

            if (this.ready)
            {
                this.lblStatus.Text = problem ?? string.Empty;
            }

            this.btnOk.Enabled = this.ready && !this.closed && this.judgeResults != null && problem == null;
            this.UpdateEditorButtons();
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            this.RefreshState();

            if (!this.btnOk.Enabled)
            {
                return;
            }

            DataTable result = this.judgeResults.Copy();

            foreach (DataRow row in result.Rows)
            {
                foreach (DataColumn column in result.Columns)
                {
                    if (column.DataType == typeof(string))
                    {
                        row[column] = TableHelper.CellText(row, column.ColumnName).Trim();
                    }
                }

                if (this.Kind == JudgeKind.Chip)
                {
                    row[Schemas.JudgeResult.FingerId.Name] = ChipFingerId;
                }
            }

            this.Result = new JudgeDialogResult(result, (this.txtDescription.Text ?? string.Empty).Trim(), this.goalDurableId);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            this.closed = true;
            this.ready = false;

            if (this.judgeResults != null)
            {
                this.judgeResults.ColumnChanged -= this.OnJudgeColumnChanged;
            }

            this.InvalidateChannel(rowsChannel);
        }
    }
}
