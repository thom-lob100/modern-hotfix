using System;
using System.Data;
using System.Windows.Forms;

using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Theming;

namespace Modern.Lab.Hosting
{
    /// <summary>
    /// 규칙 안내 창 — 화면의 액션마다 열리는 조건을 표로 보이고, 선택한 행의 값과 대조해 막힌 줄을
    /// 배지와 행 색으로 표시한다(2026-10-13). 베이스(<see cref="ModernFormBase"/>)가 꺼진 버튼·메뉴 항목을
    /// 눌렀을 때 그 액션을 펼친 채 열고, 선택이 바뀔 때마다 <see cref="ShowRules"/> 로 다시 채운다. 화면 쪽 코드는 없다.
    ///
    /// 표는 액션이 부모, 조건이 자식인 트리다. 값 규칙은 필드 · 요구 값 · 지금 값이 열로 나오고, 판정
    /// 메서드 규칙은 Required 열에 문장만 나온다. 이 창은 화면의 사전 검사를 보여 줄 뿐이고 최종 판정은
    /// 서버가 한다.
    /// </summary>
    public partial class ActionRuleGuideForm : ModernFormBase
    {
        private const string IdColumn = "Id";
        private const string ParentIdColumn = "ParentId";
        private const string NameColumn = "Name";
        private const string FieldColumn = "Field";
        private const string RequiredColumn = "Required";
        private const string CurrentColumn = "Current";
        private const string ResultColumn = "Result";
        private const string ResultSemanticColumn = "ResultSemantic";
        private const string RowColorColumn = "RowColor";

        private const int TreeColumnWidth = 180;
        private const int FieldColumnWidth = 190;
        private const int RequiredColumnWidth = 150;
        private const int CurrentColumnWidth = 120;
        private const int ResultColumnWidth = 90;
        private const int ScrollBarAllowance = 20;
        private const int ChromeAllowance = 4;

        private const string ResultOk = "OK";
        private const string ResultBlocked = "Blocked";
        private const string SemanticOk = "success";
        private const string SemanticBlocked = "error";
        private const string NoCondition = "No condition";
        private const string NoRuleLists = "This screen declares no rule lists.";
        private const string NoRowSelected = "No row selected — pick a row in the screen to compare.";
        private const string CheckedAgainst = "Checked against the selected row: ";

        public ActionRuleGuideForm()
        {
            this.InitializeComponent();
            this.InitializeModernForm(false);
            this.ConfigureTree();
            this.FitWidthToColumns();
        }

        // 창 폭은 열 합계에 맞춘다 — 트리 180 + Field 190 + Required 150 + Current 120 + Result 90 = 730.
        // WPF 표는 화면 배율로 그려지고 이 폼은 배율을 적용하지 않으므로(픽셀 그대로) 배율을 곱한 뒤
        // 세로 스크롤바와 테두리 여백을 더한다. 디자이너의 1240 은 배율을 모른 채 넉넉히 잡은 값이라 오른쪽이 비었다
        // (2026-10-15 사용자 보고). 사용자가 창을 늘리는 것은 그대로 되고, 최소 폭도 이 값이다.
        private void FitWidthToColumns()
        {
            double scale = 1d;

            using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
            {
                scale = graphics.DpiX > 0f ? graphics.DpiX / 96d : 1d;
            }

            int columns = TreeColumnWidth + FieldColumnWidth + RequiredColumnWidth + CurrentColumnWidth + ResultColumnWidth;
            int width = (int)Math.Round((columns + ScrollBarAllowance) * scale) + ChromeAllowance;
            this.ClientSize = new System.Drawing.Size(width, this.ClientSize.Height);
            this.MinimumSize = new System.Drawing.Size(this.Width, this.MinimumSize.Height);
        }

        /// <summary>안내 창 안에서는 안내 창을 다시 열지 않는다.</summary>
        protected override bool ShowActionRuleGuide
        {
            get { return false; }
        }

        /// <summary>표를 채운다 — <paramref name="row"/> 가 없으면 조건만 나열하고 결과 열을 비운다.
        /// <paramref name="focusKey"/> 가 있는 액션은 그것만 펼치고 첫 조건을 선택한다. 없으면 모두 펼친다.</summary>
        public void ShowRules(string screenTitle, ActionRuleGroup[] groups, DataRow row, string focusKey)
        {
            string title = string.IsNullOrEmpty(screenTitle) ? "Rules" : screenTitle + " · Rules";

            if (this.Text != title)
            {
                this.Text = title;
            }

            this.lblSelection.Text = row == null ? NoRowSelected : CheckedAgainst + RowSummary(row);
            DataTable table = BuildTable(groups ?? new ActionRuleGroup[0], row);
            this.treeRules.DataSource = table;

            string focusId = FocusRowId(table, focusKey);

            if (focusId == null)
            {
                this.treeRules.ExpandAll();
                return;
            }

            this.treeRules.CollapseAll();
            this.treeRules.SelectedValue = focusId;
        }

        // 펼칠 액션의 첫 조건 행 — 조건이 없으면 액션 행 자체. 자식을 고르면 트리가 부모를 펼친다.
        private static string FocusRowId(DataTable table, string focusKey)
        {
            if (string.IsNullOrEmpty(focusKey))
            {
                return null;
            }

            string groupId = null;
            string firstChildId = null;

            foreach (DataRow entry in table.Rows)
            {
                string id = entry[IdColumn] as string;
                string parentId = entry[ParentIdColumn] as string;

                if (id == focusKey)
                {
                    groupId = id;
                }
                else if (parentId == focusKey && firstChildId == null)
                {
                    firstChildId = id;
                }
            }

            return firstChildId ?? groupId;
        }

        // 모달이 아닌 창은 CenterParent 가 먹지 않는다 — 부모 화면 가운데에 놓되 화면 밖으로 나가지 않게 한다.
        // 폼 베이스가 열기 전에 화면 폼 좌표로 위치를 정해 두면(StartPosition = Manual) 그대로 둔다 — 회사 셸처럼
        // 최상위 창이 WinForms Form 이 아니면 Owner 가 없어 여기서는 못 놓는다(2026-10-15 사용자 보고).
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (this.StartPosition == FormStartPosition.Manual || this.Owner == null)
            {
                return;
            }

            // 샘플 셸처럼 화면이 다른 창 안에 들어가 있어도(TopLevel = false) 화면 좌표로 계산한다.
            System.Drawing.Rectangle parent = this.Owner.RectangleToScreen(this.Owner.ClientRectangle);
            System.Drawing.Rectangle area = Screen.FromControl(this.Owner).WorkingArea;
            int left = parent.Left + (parent.Width - this.Width) / 2;
            int top = parent.Top + (parent.Height - this.Height) / 2;
            left = Math.Max(area.Left, Math.Min(left, area.Right - this.Width));
            top = Math.Max(area.Top, Math.Min(top, area.Bottom - this.Height));
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new System.Drawing.Point(left, top);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ConfigureTree()
        {
            this.treeRules.IdMember = IdColumn;
            this.treeRules.ParentIdMember = ParentIdColumn;
            this.treeRules.DisplayMember = NameColumn;
            this.treeRules.TreeHeaderText = "Action / Condition";
            // 가로 스크롤이 있는 표에서는 별표 폭이 머리글 폭까지 줄어 액션 이름이 잘린다 — 고정 폭으로 둔다.
            this.treeRules.TreeColumnWidth = TreeColumnWidth;
            this.treeRules.RowColorMember = RowColorColumn;
            this.treeRules.EmptyText = NoRuleLists;

            ModernDataGridColumn result = new ModernDataGridColumn(ResultColumn, "Result", ResultColumnWidth);
            result.Kind = GridColumnKind.Badge;
            result.BadgeSemanticMember = ResultSemanticColumn;
            result.BadgeWidthValues = ResultOk + ";" + ResultBlocked;

            // 열 폭 합계가 창 폭을 정한다(FitWidthToColumns).
            this.treeRules.ConfigureColumns(
                    new ModernDataGridColumn(FieldColumn, "Field", FieldColumnWidth),
                    new ModernDataGridColumn(RequiredColumn, "Required", RequiredColumnWidth),
                    new ModernDataGridColumn(CurrentColumn, "Current", CurrentColumnWidth),
                    result);
        }

        private static DataTable BuildTable(ActionRuleGroup[] groups, DataRow row)
        {
            DataTable table = new DataTable();
            table.Columns.Add(IdColumn, typeof(string));
            table.Columns.Add(ParentIdColumn, typeof(string));
            table.Columns.Add(NameColumn, typeof(string));
            table.Columns.Add(FieldColumn, typeof(string));
            table.Columns.Add(RequiredColumn, typeof(string));
            table.Columns.Add(CurrentColumn, typeof(string));
            table.Columns.Add(ResultColumn, typeof(string));
            table.Columns.Add(ResultSemanticColumn, typeof(string));
            table.Columns.Add(RowColorColumn, typeof(string));

            bool hasRow = row != null;
            bool commonBlocked = false;

            foreach (ActionRuleGroup group in groups)
            {
                if (group.IsCommon)
                {
                    commonBlocked = !AddGroup(table, group, row, false);
                    break;
                }
            }

            foreach (ActionRuleGroup group in groups)
            {
                if (!group.IsCommon)
                {
                    AddGroup(table, group, row, hasRow && commonBlocked);
                }
            }

            return table;
        }

        // 액션 하나를 부모 행으로, 조건을 자식 행으로 넣고 전부 통과했는지 돌려준다.
        private static bool AddGroup(DataTable table, ActionRuleGroup group, DataRow row, bool blockedByCommon)
        {
            string groupId = group.Key;
            bool hasRow = row != null;
            bool allPassed = !blockedByCommon;
            DataRow parent = table.NewRow();
            parent[IdColumn] = groupId;
            parent[ParentIdColumn] = DBNull.Value;
            parent[NameColumn] = group.Name;
            parent[FieldColumn] = group.Rules.Length == 0 ? NoCondition : string.Empty;
            parent[RequiredColumn] = string.Empty;
            parent[CurrentColumn] = string.Empty;
            table.Rows.Add(parent);

            for (int index = 0; index < group.Rules.Length; index++)
            {
                ActionRule rule = group.Rules[index];

                if (rule == null)
                {
                    continue;
                }

                ActionRuleCheck check = rule.Explain(row);
                bool blocked = hasRow && !check.Passed;
                allPassed = allPassed && !blocked;

                DataRow child = table.NewRow();
                child[IdColumn] = groupId + "." + (index + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
                child[ParentIdColumn] = groupId;
                child[NameColumn] = (index + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
                child[FieldColumn] = check.Field;
                child[RequiredColumn] = check.Required;
                child[CurrentColumn] = check.Current;
                FillResult(child, hasRow, blocked);
                table.Rows.Add(child);
            }

            FillResult(parent, hasRow, !allPassed);
            return allPassed;
        }

        private static void FillResult(DataRow target, bool hasRow, bool blocked)
        {
            target[ResultColumn] = !hasRow ? string.Empty : blocked ? ResultBlocked : ResultOk;
            target[ResultSemanticColumn] = !hasRow ? string.Empty : blocked ? SemanticBlocked : SemanticOk;
            target[RowColorColumn] = hasRow && blocked ? SemanticColors.Error : string.Empty;
        }

        private static string RowSummary(DataRow row)
        {
            if (row.Table == null || row.Table.Columns.Count == 0 || row.RowState == DataRowState.Detached
                    || row.RowState == DataRowState.Deleted)
            {
                return "(row)";
            }

            DataColumn first = row.Table.Columns[0];
            object value = row[first];
            string text = value == null || value == DBNull.Value ? "(empty)" : value.ToString();
            return first.ColumnName + " = " + text;
        }
    }
}
