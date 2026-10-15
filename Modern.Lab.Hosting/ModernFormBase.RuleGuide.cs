using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Modern.Lab.Hosting
{
    /// <summary>
    /// 규칙 안내 창 연결 — 화면 쪽 코드 없이 베이스가 연다(2026-10-13 · 입구 2026-10-14).
    /// <c>ModernFormBase.cs</c> 의 partial 메서드 네 개(키 · 로드 · 버튼 기억 · 해제)를 여기서 채운다 —
    /// 이 파일을 함께 컴파일하지 않는 프로젝트(갤러리)에서는 그 호출이 빈 호출이 되어 안내 창이 없다.
    ///
    /// - 입구: <b>꺼진 액션을 누르면 열린다.</b> ① <see cref="ApplyActionReason(Control, string)"/> 을 지난
    ///   버튼이 꺼져 있을 때 그 자리를 누르면(꺼진 컨트롤의 클릭은 부모가 받는다), ② 폼이 컨트롤에 단 우클릭
    ///   메뉴의 꺼진 항목을 누르면, ③ 드롭다운 버튼(<c>ModernDropDownButton</c>)의 꺼진 항목을 누르면.
    ///   창은 누른 액션만 펼치고 그 첫 조건을 선택한 채 열린다. 단축키는 기본 없음(<see cref="RuleGuideShortcut"/>).
    /// - 규칙: <see cref="ActionRuleCatalog.Collect"/> 가 이 화면 클래스의 <c>static ActionRule[] XxxRules</c> 필드를 읽는다.
    /// - 액션 이름: 버튼 글자와 메뉴 항목 글자 중 필드 이름과 맞는 것을 쓰고, 없으면 필드 이름을 띄어 쓴다.
    /// - 선택 행: 화면의 그리드·트리 중 규칙 필드를 가장 많이 가진 표의 선택 행이다. 선택이 바뀌면 창이 따라간다.
    /// </summary>
    public partial class ModernFormBase
    {
        private const string ruleGuideRefreshKey = "modernformbase.rule-guide";

        private ActionRuleGuideForm ruleGuide;
        private ActionRuleGroup[] ruleGuideGroups;
        private string ruleGuideFocusKey;
        private bool? hasRuleLists;
        private readonly Dictionary<string, string> actionLabels =
                new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly HashSet<Control> actionControls = new HashSet<Control>();
        private readonly HashSet<Control> actionParents = new HashSet<Control>();
        private readonly HashSet<ToolStripDropDown> hookedMenus = new HashSet<ToolStripDropDown>();
        private readonly List<Control> ruleGuideSelectionSources = new List<Control>();

        /// <summary>꺼진 액션 클릭으로 규칙 안내 창을 열 수 있는가 — 안내 창 자신처럼 닫아 둘 화면만 재정의한다.</summary>
        protected virtual bool ShowActionRuleGuide
        {
            get { return true; }
        }

        /// <summary>규칙 안내 창 단축키 — 기본 없음(<see cref="Keys.None"/>). 회사 상위 Common 의 폼 베이스가
        /// 도움말 키와 겹치지 않는 키로 한 번만 정한다.</summary>
        protected virtual Keys RuleGuideShortcut
        {
            get { return Keys.None; }
        }

        /// <summary>규칙 안내 창을 열거나 앞으로 가져오고, 지금 선택 행으로 채운다. 모든 액션을 펼친다.</summary>
        protected void ShowRuleGuide()
        {
            this.ShowRuleGuide(null);
        }

        /// <summary><paramref name="actionLabel"/> 에 맞는 액션만 펼친 채 연다 — 버튼·메뉴 글자나 규칙 필드 키.
        /// 맞는 액션이 없으면 모든 액션을 펼친다.</summary>
        protected void ShowRuleGuide(string actionLabel)
        {
            if (!this.ShowActionRuleGuide || this.IsDisposed || this.Disposing || IsDesignTime() || this.DesignMode)
            {
                return;
            }

            this.ruleGuideGroups = this.CollectRuleGuideGroups();
            this.ruleGuideFocusKey = FindGroupKey(this.ruleGuideGroups, actionLabel);

            if (this.ruleGuide == null || this.ruleGuide.IsDisposed)
            {
                this.ruleGuide = new ActionRuleGuideForm();
                this.ruleGuide.FormClosed += this.OnRuleGuideClosed;
                this.SubscribeRuleGuideSelectionSources();
                this.RefreshRuleGuide();
                this.PlaceRuleGuideOverScreen();
                this.ruleGuide.Show(this);
                return;
            }

            this.RefreshRuleGuide();
            this.ruleGuide.Activate();
        }

        // 안내 창을 이 화면의 가운데에 놓는다 — 화면 폼 자신의 화면 좌표로 계산하므로 화면이 다른 창 안에 들어 있어도
        // (TopLevel = false, WPF 셸, SetParent 호스팅) 같은 결과다. Show(owner) 의 Owner 에 기대면 최상위가
        // WinForms Form 이 아닐 때 Owner 가 없어 좌측 상단에 떴다(2026-10-15 사용자 보고). 화면 밖으로는 나가지 않는다.
        private void PlaceRuleGuideOverScreen()
        {
            if (!this.IsHandleCreated)
            {
                return;
            }

            Rectangle screenBounds = this.RectangleToScreen(this.ClientRectangle);
            Rectangle area = Screen.FromControl(this).WorkingArea;
            int left = screenBounds.Left + (screenBounds.Width - this.ruleGuide.Width) / 2;
            int top = screenBounds.Top + (screenBounds.Height - this.ruleGuide.Height) / 2;
            left = Math.Max(area.Left, Math.Min(left, area.Right - this.ruleGuide.Width));
            top = Math.Max(area.Top, Math.Min(top, area.Bottom - this.ruleGuide.Height));
            this.ruleGuide.StartPosition = FormStartPosition.Manual;
            this.ruleGuide.Location = new Point(left, top);
        }

        partial void RuleGuideProcessCmdKey(Keys keyData, ref bool handled)
        {
            Keys shortcut = this.RuleGuideShortcut;

            if (shortcut != Keys.None && keyData == shortcut && this.ShowActionRuleGuide)
            {
                this.ShowRuleGuide();
                handled = true;
            }
        }

        partial void RuleGuideOnLoad()
        {
            this.HookRuleGuideEntries(this);
        }

        partial void RuleGuideRememberControl(Control control)
        {
            this.RememberActionLabel(control.Text);
            this.RememberActionControl(control);
            this.ScheduleRuleGuideRefresh();
        }

        partial void RuleGuideDispose()
        {
            this.UnsubscribeRuleGuideSelectionSources();

            if (this.ruleGuide != null && !this.ruleGuide.IsDisposed)
            {
                this.ruleGuide.FormClosed -= this.OnRuleGuideClosed;
                this.ruleGuide.Dispose();
            }

            this.ruleGuide = null;
            this.ruleGuideGroups = null;
        }

        private bool HasRuleLists()
        {
            if (!this.hasRuleLists.HasValue)
            {
                this.hasRuleLists = ActionRuleCatalog.Collect(this.GetType()).Length > 0;
            }

            return this.hasRuleLists.Value;
        }

        private ActionRuleGroup[] CollectRuleGuideGroups()
        {
            foreach (ToolStripDropDown menu in this.hookedMenus)
            {
                this.RememberMenuLabels(menu.Items);
            }

            ActionRuleGroup[] groups = ActionRuleCatalog.Collect(this.GetType());

            foreach (ActionRuleGroup group in groups)
            {
                string label;

                if (!group.IsCommon
                        && this.actionLabels.TryGetValue(ActionRuleCatalog.NormalizeLabel(group.Key), out label))
                {
                    group.Name = label;
                }
            }

            return groups;
        }

        // 버튼·메뉴 글자를 액션 그룹 키로 — 이름이나 키와 글자가 같으면 그 그룹. 없으면 null(전부 펼친다).
        private static string FindGroupKey(ActionRuleGroup[] groups, string actionLabel)
        {
            string wanted = ActionRuleCatalog.NormalizeLabel(actionLabel);

            if (wanted.Length == 0)
            {
                return null;
            }

            foreach (ActionRuleGroup group in groups)
            {
                if (group.IsCommon)
                {
                    continue;
                }

                if (ActionRuleCatalog.NormalizeLabel(group.Name) == wanted
                        || ActionRuleCatalog.NormalizeLabel(group.Key) == wanted)
                {
                    return group.Key;
                }
            }

            return null;
        }

        private void RememberMenuLabels(ToolStripItemCollection items)
        {
            foreach (ToolStripItem entry in items)
            {
                ToolStripMenuItem item = entry as ToolStripMenuItem;

                if (item == null)
                {
                    continue;
                }

                this.RememberActionLabel(item.Text);
                this.RememberMenuLabels(item.DropDownItems);
            }
        }

        private void RememberActionLabel(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            string label = text.Replace("&", string.Empty).Trim();
            string key = ActionRuleCatalog.NormalizeLabel(label);

            if (key.Length > 0 && !this.actionLabels.ContainsKey(key))
            {
                this.actionLabels[key] = label;
            }
        }

        // 꺼진 컨트롤의 클릭은 Windows 가 부모에게 보낸다 — 액션 버튼의 부모에 한 번만 클릭을 건다.
        private void RememberActionControl(Control control)
        {
            this.actionControls.Add(control);
            Control parent = control.Parent;

            if (parent != null && this.actionParents.Add(parent))
            {
                parent.MouseClick += this.OnActionParentMouseClick;
            }
        }

        private void OnActionParentMouseClick(object sender, MouseEventArgs e)
        {
            Control parent = sender as Control;

            if (parent == null || e.Button != MouseButtons.Left || !this.HasRuleLists())
            {
                return;
            }

            Control child = parent.GetChildAtPoint(e.Location, GetChildAtPointSkip.Invisible);

            if (child != null && !child.Enabled && this.actionControls.Contains(child))
            {
                this.ShowRuleGuide(child.Text);
            }
        }

        // 폼이 컨트롤에 단 우클릭 메뉴와 드롭다운 버튼 전부에 꺼진 항목 클릭을 건다.
        private void HookRuleGuideEntries(Control root)
        {
            if (root == null)
            {
                return;
            }

            if (root.ContextMenuStrip != null)
            {
                this.HookMenu(root.ContextMenuStrip);
            }

            Modern.Lab.WinForms.Controls.Input.ModernDropDownButton dropDown =
                    root as Modern.Lab.WinForms.Controls.Input.ModernDropDownButton;

            if (dropDown != null)
            {
                dropDown.DisabledItemClicked += this.OnDropDownDisabledItemClicked;
                return;
            }

            foreach (Control child in root.Controls)
            {
                this.HookRuleGuideEntries(child);
            }
        }

        private void HookMenu(ToolStripDropDown menu)
        {
            if (menu != null && this.hookedMenus.Add(menu))
            {
                menu.MouseUp += this.OnMenuMouseUp;
                menu.Opening += this.OnMenuOpening;
            }
        }

        // 하위 메뉴는 열릴 때 걸어 둔다 — 디자이너가 만든 항목도, 열 때 폼이 채우는 항목도 같이 잡힌다.
        private void OnMenuOpening(object sender, CancelEventArgs e)
        {
            ToolStripDropDown menu = sender as ToolStripDropDown;

            if (menu == null)
            {
                return;
            }

            foreach (ToolStripItem entry in menu.Items)
            {
                ToolStripMenuItem item = entry as ToolStripMenuItem;

                if (item != null && item.HasDropDownItems)
                {
                    this.HookMenu(item.DropDown);
                }
            }
        }

        private void OnMenuMouseUp(object sender, MouseEventArgs e)
        {
            ToolStripDropDown menu = sender as ToolStripDropDown;

            if (menu == null || e.Button != MouseButtons.Left || !this.HasRuleLists())
            {
                return;
            }

            ToolStripMenuItem item = menu.GetItemAt(e.Location) as ToolStripMenuItem;

            if (item == null || item.Enabled || item.HasDropDownItems)
            {
                return;
            }

            menu.Close(ToolStripDropDownCloseReason.ItemClicked);
            this.ShowRuleGuide(item.Text);
        }

        private void OnDropDownDisabledItemClicked(object sender,
                Modern.Lab.Controls.Wpf.Input.DropDownItemClickedEventArgs e)
        {
            if (this.HasRuleLists())
            {
                this.ShowRuleGuide(e.DisplayText);
            }
        }

        private void ScheduleRuleGuideRefresh()
        {
            if (this.ruleGuide == null || this.ruleGuide.IsDisposed)
            {
                return;
            }

            this.Debounce(ruleGuideRefreshKey, 100, new MethodInvoker(this.RefreshRuleGuide));
        }

        private void RefreshRuleGuide()
        {
            if (this.ruleGuide == null || this.ruleGuide.IsDisposed || this.IsDisposed || this.Disposing)
            {
                return;
            }

            if (this.ruleGuideGroups == null)
            {
                this.ruleGuideGroups = this.CollectRuleGuideGroups();
            }

            this.ruleGuide.ShowRules(this.Text, this.ruleGuideGroups,
                    this.ResolveRuleGuideRow(this.ruleGuideGroups), this.ruleGuideFocusKey);
        }

        // 규칙 필드를 가장 많이 가진 표의 선택 행 — 같으면 포커스가 있는 표, 그다음 먼저 놓인 표.
        private DataRow ResolveRuleGuideRow(ActionRuleGroup[] groups)
        {
            HashSet<string> fields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (ActionRuleGroup group in groups)
            {
                foreach (ActionRule rule in group.Rules)
                {
                    if (rule != null && rule.FieldName != null)
                    {
                        fields.Add(rule.FieldName);
                    }
                }
            }

            List<Control> sources = new List<Control>();
            CollectSelectionSources(this, sources);

            DataRow best = null;
            int bestScore = -1;
            bool bestFocused = false;

            foreach (Control source in sources)
            {
                DataRow row = SelectedRowOf(source);

                if (row == null || row.Table == null)
                {
                    continue;
                }

                int score = 0;

                foreach (string field in fields)
                {
                    if (row.Table.Columns.Contains(field))
                    {
                        score++;
                    }
                }

                bool focused = source.ContainsFocus;

                if (score > bestScore || (score == bestScore && focused && !bestFocused))
                {
                    best = row;
                    bestScore = score;
                    bestFocused = focused;
                }
            }

            return best;
        }

        private static DataRow SelectedRowOf(Control source)
        {
            Modern.Lab.WinForms.Controls.Data.ModernDataGrid grid = source as Modern.Lab.WinForms.Controls.Data.ModernDataGrid;
            Modern.Lab.WinForms.Controls.Data.ModernTreeList tree = source as Modern.Lab.WinForms.Controls.Data.ModernTreeList;
            object item = grid != null ? grid.SelectedItem : tree != null ? tree.SelectedItem : null;
            DataRowView view = item as DataRowView;
            DataRow row = view != null ? view.Row : item as DataRow;

            if (row == null || row.RowState == DataRowState.Deleted || row.RowState == DataRowState.Detached)
            {
                return null;
            }

            return row;
        }

        private static void CollectSelectionSources(Control root, List<Control> sources)
        {
            if (root == null)
            {
                return;
            }

            if (root is Modern.Lab.WinForms.Controls.Data.ModernDataGrid
                    || root is Modern.Lab.WinForms.Controls.Data.ModernTreeList)
            {
                sources.Add(root);
                return;
            }

            foreach (Control child in root.Controls)
            {
                CollectSelectionSources(child, sources);
            }
        }

        private void SubscribeRuleGuideSelectionSources()
        {
            this.UnsubscribeRuleGuideSelectionSources();
            CollectSelectionSources(this, this.ruleGuideSelectionSources);

            foreach (Control source in this.ruleGuideSelectionSources)
            {
                Modern.Lab.WinForms.Controls.Data.ModernDataGrid grid = source as Modern.Lab.WinForms.Controls.Data.ModernDataGrid;
                Modern.Lab.WinForms.Controls.Data.ModernTreeList tree = source as Modern.Lab.WinForms.Controls.Data.ModernTreeList;

                if (grid != null)
                {
                    grid.SelectionChanged += this.OnRuleGuideSelectionChanged;
                }
                else if (tree != null)
                {
                    tree.SelectedValueChanged += this.OnRuleGuideSelectionChanged;
                }
            }
        }

        private void UnsubscribeRuleGuideSelectionSources()
        {
            foreach (Control source in this.ruleGuideSelectionSources)
            {
                Modern.Lab.WinForms.Controls.Data.ModernDataGrid grid = source as Modern.Lab.WinForms.Controls.Data.ModernDataGrid;
                Modern.Lab.WinForms.Controls.Data.ModernTreeList tree = source as Modern.Lab.WinForms.Controls.Data.ModernTreeList;

                if (grid != null)
                {
                    grid.SelectionChanged -= this.OnRuleGuideSelectionChanged;
                }
                else if (tree != null)
                {
                    tree.SelectedValueChanged -= this.OnRuleGuideSelectionChanged;
                }
            }

            this.ruleGuideSelectionSources.Clear();
        }

        private void OnRuleGuideSelectionChanged(object sender, EventArgs e)
        {
            this.ScheduleRuleGuideRefresh();
        }

        private void OnRuleGuideClosed(object sender, FormClosedEventArgs e)
        {
            this.UnsubscribeRuleGuideSelectionSources();
            this.CancelDebounce(ruleGuideRefreshKey);
            this.ruleGuide = null;
            this.ruleGuideGroups = null;
            this.ruleGuideFocusKey = null;
        }
    }
}
