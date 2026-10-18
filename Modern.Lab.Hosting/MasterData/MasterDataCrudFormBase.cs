using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;

using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.NewItems;
using Modern.Lab.Hosting.ResponseContracts;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.MasterData
{
    /// <summary>기준정보 CRUD 폼의 공통 베이스. VS 디자이너가 파생 폼을 열 때 이 타입의 인스턴스를 만들므로
    /// <c>abstract</c>로 두지 않는다 — 화면이 채울 전문은 가상 메서드이고 기본 구현은 예외를 던진다.</summary>
    public class MasterDataCrudFormBase : ModernFormBase
    {
        private const string ChannelList = "list";

        private enum ListState
        {
            Loading,
            Ready,
            ReadyEmpty,
            Blocked,
            Failed,
            Closed
        }

        private enum WriteKind
        {
            New,
            Save,
            Delete
        }

        private MasterDataCrudDefinition definition;
        private const string DuplicateMenuName = "miDuplicate";

        private MasterDataCrudView view;
        private ToolStripMenuItem duplicateMenuItem;
        private string[] keyColumns;
        private TableContract listContract;
        private ListState listState = ListState.Loading;
        private TableResponse itemList;
        private readonly NewItemTracker newItems = new NewItemTracker();
        private bool blockedNoticeShowing;
        private string[] pendingSelectKey;
        private int selectedIndex = -1;
        private bool restoringSelection;
        private bool writeLocked;
        private bool keywordWasEnabled;
        private bool refreshWasEnabled;
        private bool resetWasEnabled;
        private bool gridWasEnabled;

        protected virtual MasterDataCrudView CreateView()
        {
            return new MasterDataCrudView
            {
                Keyword = this.FindByName<Modern.Lab.WinForms.Controls.Input.ModernTextBox>("txtKeyword")
                        ?? this.FindOnly<Modern.Lab.WinForms.Controls.Input.ModernTextBox>(),
                RefreshButton = this.FindByName<Modern.Lab.WinForms.Controls.Input.ModernButton>("btnRefresh"),
                ResetButton = this.FindByName<Modern.Lab.WinForms.Controls.Input.ModernButton>("btnReset"),
                Grid = this.FindOnly<Modern.Lab.WinForms.Controls.Data.ModernDataGrid>(),
                EditorCard = this.FindByName<Modern.Lab.WinForms.Controls.Layout.ModernGroupBox>("editorCard"),
                Editor = this.FindOnly<ModernPropertyGrid>(),
                NewButton = this.FindByName<Modern.Lab.WinForms.Controls.Input.ModernButton>("btnNew"),
                CancelButton = this.FindByName<Modern.Lab.WinForms.Controls.Input.ModernButton>("btnCancel"),
                SaveButton = this.FindByName<Modern.Lab.WinForms.Controls.Input.ModernButton>("btnSave"),
                DeleteButton = this.FindByName<Modern.Lab.WinForms.Controls.Input.ModernButton>("btnDelete"),
                ActionMenu = this.FindMenu("menuActions"),
                NewMenuItem = this.FindMenuItem("miNew"),
                CancelMenuItem = this.FindMenuItem("miCancel"),
                SaveMenuItem = this.FindMenuItem("miSave"),
                DeleteMenuItem = this.FindMenuItem("miDelete")
            };
        }

        protected void InitializeCrud(MasterDataCrudDefinition definition)
        {
            this.InitializeCrud(definition, this.CreateView());
        }

        private T FindByName<T>(string name) where T : Control
        {
            foreach (Control candidate in Descendants(this))
            {
                T typed = candidate as T;

                if (typed != null && candidate.Name == name)
                {
                    return typed;
                }
            }

            return null;
        }

        private T FindOnly<T>() where T : Control
        {
            T found = null;

            foreach (Control candidate in Descendants(this))
            {
                T typed = candidate as T;

                if (typed == null)
                {
                    continue;
                }

                if (found != null)
                {
                    return null;
                }

                found = typed;
            }

            return found;
        }

        private ContextMenuStrip FindMenu(string name)
        {
            foreach (Component component in this.MenuComponents())
            {
                ContextMenuStrip menu = component as ContextMenuStrip;

                if (menu != null && menu.Name == name)
                {
                    return menu;
                }
            }

            return null;
        }

        private ToolStripMenuItem FindMenuItem(string name)
        {
            foreach (Component component in this.MenuComponents())
            {
                ContextMenuStrip menu = component as ContextMenuStrip;

                if (menu == null)
                {
                    continue;
                }

                foreach (ToolStripItem item in menu.Items)
                {
                    ToolStripMenuItem typed = item as ToolStripMenuItem;

                    if (typed != null && typed.Name == name)
                    {
                        return typed;
                    }
                }
            }

            return null;
        }

        private IEnumerable<Component> MenuComponents()
        {
            System.Reflection.FieldInfo field = null;
            Type type = this.GetType();

            while (type != null && field == null)
            {
                field = type.GetField(
                        "components",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                type = type.BaseType;
            }

            IContainer container = field == null ? null : field.GetValue(this) as IContainer;

            if (container == null)
            {
                return new Component[0];
            }

            List<Component> components = new List<Component>();

            foreach (Component component in container.Components)
            {
                components.Add(component);
            }

            return components;
        }

        private static IEnumerable<Control> Descendants(Control root)
        {
            List<Control> found = new List<Control>();
            Collect(root, found);
            return found;
        }

        private static void Collect(Control parent, List<Control> found)
        {
            foreach (Control child in parent.Controls)
            {
                found.Add(child);
                Collect(child, found);
            }
        }

        private static void EnsureComplete(MasterDataCrudView view)
        {
            List<string> missing = new List<string>();

            if (view.Grid == null) { missing.Add("그리드 (ModernDataGrid 하나)"); }
            if (view.EditorCard == null) { missing.Add("editorCard (ModernGroupBox)"); }
            if (view.Editor == null) { missing.Add("속성 편집기 (ModernPropertyGrid 하나)"); }
            if (view.NewButton == null) { missing.Add("btnNew"); }
            if (view.CancelButton == null) { missing.Add("btnCancel"); }
            if (view.SaveButton == null) { missing.Add("btnSave"); }
            if (view.DeleteButton == null) { missing.Add("btnDelete"); }
            if (view.ActionMenu == null) { missing.Add("menuActions (ContextMenuStrip)"); }
            if (view.NewMenuItem == null) { missing.Add("miNew"); }
            if (view.CancelMenuItem == null) { missing.Add("miCancel"); }
            if (view.SaveMenuItem == null) { missing.Add("miSave"); }
            if (view.DeleteMenuItem == null) { missing.Add("miDelete"); }

            if (missing.Count == 0)
            {
                return;
            }

            throw new InvalidOperationException(
                    "CRUD 화면 배선이 비었다: " + string.Join(" · ", missing.ToArray())
                            + " — 디자이너 이름을 규칙대로 붙이거나 CreateView()를 재정의할 것.");
        }

        protected void InitializeCrud(MasterDataCrudDefinition definition, MasterDataCrudView view)
        {
            if (definition == null)
            {
                throw new ArgumentNullException("definition");
            }

            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            EnsureComplete(view);

            this.definition = definition;
            this.view = view;
            this.keyColumns = ResolveKeyColumns(definition);
            this.listContract = new TableContract(definition.ListTableId).Key(this.keyColumns);

            this.InitializeModernForm();
            this.view.ActionMenu.Renderer = new Modern.Lab.WinForms.Rendering.ModernMenuRenderer();
            this.DeferredResize = true;
            this.RegisterFindShortcut(this.view.Grid);

            string keyText = this.CompositeKey ? string.Join(",", this.keyColumns) : this.keyColumns[0];
            this.view.Grid.RowKeyMember = keyText;
            this.view.Editor.KeyColumns = keyText;
            this.view.Editor.ParameterNameStyle = ParameterNameStyle.PascalCase;

            if (definition.Entity != null)
            {
                this.view.Editor.UseEntity(definition.Entity);
            }

            this.view.Editor.ValueChanged += this.OnEditorValueChanged;
            this.view.Editor.ComboRefresh = this.RefreshLookupItems;
            this.WireViewEvents();
            this.AddDuplicateMenuItem();
            this.DefineEditors();

            this.UpdateActionState();
        }

        /// <summary>
        /// 선택 설정 — 화면의 <c>.Grid.cs</c>가 재정의해 콤보·스위치·여러 줄·숫자·기본값·읽기 전용 꾸밈을 정한다(<see cref="CrudEditor"/>의
        /// <c>Field(…)</c> 체인). 재정의하지 않으면(Grid 파일이 없어도) 응답 컬럼 전부가 텍스트 편집기로 구성되고 키·필수·읽기 전용은
        /// 엔티티가 정하므로 기본 CRUD가 그대로 성립한다. <see cref="InitializeCrud(MasterDataCrudDefinition, MasterDataCrudView)"/> 끝에서 한 번 불린다.
        /// </summary>
        protected virtual void DefineEditors()
        {
        }

        /// <summary>
        /// 목록 그리드 꾸밈 — 화면의 <c>.Grid.cs</c>가 재정의해 엔티티가 만든 컬럼 위에 캡션·위아래 글씨·형식·배지·숨김을 이어 적는다
        /// (업무 화면의 <c>Schemas.X.Columns(table)</c> 체인과 같은 문법, <see cref="DefineEditors"/>의 그리드 짝). 목록이 바인딩될 때마다
        /// 응답 표로 새로 만든 <see cref="GridColumns"/>를 받으므로 응답 컬럼이 바뀌어도 따라간다. 엔티티가 없는 정의에서는 불리지 않는다.
        /// </summary>
        protected virtual void DefineListColumns(GridColumns columns)
        {
        }

        // 엔티티가 있으면 업무 화면과 같은 출처로 목록 컬럼을 구성한다(2026-10-12 사용자 지시) — 캡션·위아래 글씨·형식·정렬·배지·
        // 표시 목록·제외가 엔티티에서 오고 화면 꾸밈이 그 위에 얹힌다. 전에는 그리드가 응답 컬럼으로 스스로 만들어 엔티티를 보지 않았다.
        // 엔티티가 없는 정의(KeyColumn 만 준 화면)는 전처럼 자동 생성에 맡긴다.
        private void ConfigureListColumns(DataTable bound)
        {
            if (this.definition.Entity == null)
            {
                return;
            }

            GridColumns columns = this.definition.Entity.Columns(bound);
            this.DefineListColumns(columns);
            columns.Apply(this.view.Grid);
        }

        // 버튼·메뉴·조회·첫 로드는 베이스가 연결한다. VS 디자이너는 Designer.cs 가 베이스 클래스의 메서드를
        // 이벤트 핸들러로 쓰면 폼을 열지 못한다("… a class this class derives from already defines the method").
        // 예전 Designer.cs 에 같은 연결이 남아 있어도 한 번만 불리도록 먼저 뺀 뒤 붙인다.
        private void WireViewEvents()
        {
            this.Load -= this.OnFormLoad;
            this.Load += this.OnFormLoad;

            if (this.view.Keyword != null)
            {
                this.view.Keyword.EnterPressed -= this.OnKeywordEnterPressed;
                this.view.Keyword.EnterPressed += this.OnKeywordEnterPressed;
            }

            if (this.view.RefreshButton != null)
            {
                Rewire(this.view.RefreshButton, this.OnRefreshClick);
            }

            if (this.view.ResetButton != null)
            {
                Rewire(this.view.ResetButton, this.OnResetClick);
            }

            Rewire(this.view.NewButton, this.OnNewClick);
            Rewire(this.view.CancelButton, this.OnCancelClick);
            Rewire(this.view.SaveButton, this.OnSaveClick);
            Rewire(this.view.DeleteButton, this.OnDeleteClick);
            Rewire(this.view.NewMenuItem, this.OnNewClick);
            Rewire(this.view.CancelMenuItem, this.OnCancelClick);
            Rewire(this.view.SaveMenuItem, this.OnSaveClick);
            Rewire(this.view.DeleteMenuItem, this.OnDeleteClick);

            this.view.ActionMenu.Opening -= this.OnActionMenuOpening;
            this.view.ActionMenu.Opening += this.OnActionMenuOpening;
        }

        private static void Rewire(Control control, EventHandler handler)
        {
            control.Click -= handler;
            control.Click += handler;
        }

        private static void Rewire(ToolStripItem item, EventHandler handler)
        {
            item.Click -= handler;
            item.Click += handler;
        }

        // 엔티티가 있으면 키는 엔티티의 KeyFields 순서다. 없으면 복합 키(KeyColumns), 그것도 없으면 단일 키(KeyColumn)를
        // 그대로 쓴다 — 기존 화면의 값·동작이 바뀌지 않는다.
        private static string[] ResolveKeyColumns(MasterDataCrudDefinition definition)
        {
            List<string> columns = new List<string>();

            if (definition.Entity != null)
            {
                if (!string.IsNullOrWhiteSpace(definition.KeyColumn) || (definition.KeyColumns != null && definition.KeyColumns.Length > 0))
                {
                    throw new InvalidOperationException(
                            "CRUD 정의에 Entity 와 KeyColumn/KeyColumns 가 함께 있다 — 엔티티가 있으면 키는 엔티티의 KeyFields 다.");
                }

                if (definition.Entity.KeyFields.Count == 0)
                {
                    throw new InvalidOperationException(
                            "CRUD 정의의 엔티티 " + definition.Entity.GetType().Name + " 에 KeyFields 가 없다.");
                }

                return EntityDefinition.Names(definition.Entity.KeyFields);
            }

            foreach (string column in definition.KeyColumns ?? new string[0])
            {
                if (!string.IsNullOrWhiteSpace(column))
                {
                    columns.Add(column.Trim());
                }
            }

            if (columns.Count == 0)
            {
                return new string[] { definition.KeyColumn };
            }

            if (!string.IsNullOrWhiteSpace(definition.KeyColumn))
            {
                throw new InvalidOperationException(
                        "CRUD 정의에 KeyColumn 과 KeyColumns 가 함께 있다 — 단일 키는 KeyColumn, 복합 키는 KeyColumns 하나만 채울 것.");
            }

            return columns.ToArray();
        }

        private bool CompositeKey
        {
            get { return this.keyColumns.Length > 1; }
        }

        protected ModernPropertyGrid CrudEditor
        {
            get { return this.view.Editor; }
        }

        protected virtual void LoadLookupSources()
        {
        }

        /// <summary>
        /// 편집기 콤보의 항목 조회 — 기본은 <c>.Grid.cs</c>의 <c>Combo(전문 이름, 값, 표시, 필드…)</c>에 적힌 이름과 필드를
        /// 그대로 전문으로 보낸다(2026-10-12 사용자 결정). <c>Combo("CommonCodeAction", "CODE", "NAME", "MethodCommand", "getlist")</c>
        /// 가 <c>CommonCodeAction MethodCommand=getlist</c> 로 나간다. 응답을 가공하거나 이름표로 전문을 가르는 화면만
        /// <c>.Server.cs</c>에서 재정의한다. 콤보를 펼칠 때의 재조회도 같은 메서드를 탄다.
        /// </summary>
        protected virtual DataTable RequestLookupItems(string requestName, object[] requestFields)
        {
            return this.RequestFields(requestName, requestFields).Table;
        }

        // 편집기 콤보를 펼칠 때의 목록 갱신 — 첫 조회와 같은 RequestLookupItems 를 쓴다. 실패하면 오류를 알리고
        // null 을 돌려 편집기가 지금 목록을 그대로 두게 한다(사용자가 직접 펼친 동작이라 조용히 넘기지 않는다).
        private DataTable RefreshLookupItems(string requestName, object[] requestFields)
        {
            try
            {
                return this.RequestLookupItems(requestName, requestFields);
            }
            catch (Exception failure)
            {
                this.ShowServerFailure(failure);
                return null;
            }
        }

        /// <summary>목록을 다시 조회한다. 저장·삭제가 진행 중이면 아무것도 하지 않는다 — 조회 시작이 편집기를 비워
        /// 쓰기 실패 뒤 입력을 잃지 않게 한다(검색·Enter·화면 코드의 직접 호출 모두).</summary>
        protected void ReloadItems()
        {
            if (this.ActionInProgress)
            {
                return;
            }

            string keyword = this.view.Keyword == null ? string.Empty : this.view.Keyword.Text.Trim();

            this.listState = ListState.Loading;
            this.UpdateActionState();

            this.OnListLoading();

            string criteria = this.ListQueryCriteria(keyword);

            this.LoadAsync(ChannelList,
                    () => this.RequestItems(keyword),
                    table => this.BindItems(table, criteria),
                    this.AfterListSettled);
        }

        protected virtual void OnListLoading()
        {
        }

        /// <summary>
        /// 목록 신규 표시(New)의 비교 조건 — 같은 조건으로 다시 조회했을 때 직전 성공에 없던 행에 New가 붙는다.
        /// 기본은 조회어다. 조회어 밖의 값(상위 선택·검색 콤보)으로 조회하는 화면은 그 값을 함께 돌려준다.
        /// 조회를 시작할 때 <see cref="OnListLoading"/> 다음에 UI 스레드에서 불린다 — 거기서 받아 둔 검색 값을 쓸 수 있다.
        /// </summary>
        protected virtual string ListQueryCriteria(string keyword)
        {
            return NewItemTracker.Criteria(keyword);
        }

        /// <summary>
        /// 목록을 닫고 쓰기를 막는다 — 상위 선택이 풀린 화면이 쓴다. 진행 중이던 목록 응답은 늦게 와도 버린다.
        /// 표와 편집기를 비우는 것은 화면이 한다(<see cref="OnListLoading"/>과 같은 방식).
        /// </summary>
        protected void CloseItems()
        {
            this.InvalidateChannel(ChannelList);
            this.pendingSelectKey = null;
            this.itemList = null;
            this.listState = ListState.Closed;
            this.HideBlockedNotice();
            this.UpdateActionState();
        }

        /// <summary>폼이 조회 상태에서 정하는 신규 입력 값(컬럼 → 값, 예: 선택한 상위 키). 기본은 없음.
        /// 고정 기본값은 <c>.Grid.cs</c>에서 <c>CrudEditor.Field(…).DefaultValue(…)</c>로 선언하고, 같은 컬럼이면 이 값이 이긴다.
        /// New와 선택 없는 목록 반영에서 쓰이고, 신규 중 Cancel은 이 값으로 되돌린다.</summary>
        protected virtual IDictionary<string, string> NewItemDefaults()
        {
            return null;
        }

        private void BeginNewItem()
        {
            this.view.Editor.BeginNew(this.NewItemDefaults());
        }

        /// <summary>목록 선택이 바뀌면 화면이 부른다. 저장·삭제 중에는 선택을 전송 당시 행으로 되돌리고 편집기를 바꾸지 않는다.</summary>
        protected void HandleSelectionChanged()
        {
            if (this.restoringSelection)
            {
                return;
            }

            if (this.ActionInProgress)
            {
                this.restoringSelection = true;

                try
                {
                    this.view.Grid.SelectedIndex = this.selectedIndex;
                }
                finally
                {
                    this.restoringSelection = false;
                }

                return;
            }

            this.selectedIndex = this.view.Grid.SelectedIndex;
            DataRow row = SelectedRow(this.view.Grid.SelectedItem);

            if (row != null && this.ListOpen)
            {
                this.view.Editor.LoadRow(row);
            }

            this.UpdateActionState();
        }

        protected void RefreshCrudActionState()
        {
            this.UpdateActionState();
        }

        /// <summary>첫 로드 — 베이스가 폼 <c>Load</c>에 연결한다. 첫 조회 순서가 다른 화면은 재정의한다.</summary>
        protected virtual void OnFormLoad(object sender, EventArgs e)
        {
            this.LoadLookupSources();
            this.ReloadItems();
        }

        /// <summary>조회 버튼 — 베이스가 <c>btnRefresh</c>에 연결한다. 조회 조건이 다른 화면은 재정의한다.</summary>
        protected virtual void OnRefreshClick(object sender, EventArgs e)
        {
            this.ReloadItems();
        }

        /// <summary>
        /// 조회 조건 되돌리기 — 베이스가 <c>btnReset</c>에 연결한다. 조회어를 비우고 <see cref="ResetCriteria"/>로 화면만의 조건을
        /// 처음 값으로 돌린 뒤 <see cref="OnRefreshClick"/>로 다시 조회한다. 쓰기 중에는 아무것도 하지 않는다.
        /// </summary>
        protected virtual void OnResetClick(object sender, EventArgs e)
        {
            if (this.ActionInProgress)
            {
                return;
            }

            if (this.view.Keyword != null)
            {
                this.view.Keyword.Text = string.Empty;
            }

            this.ResetCriteria();
            this.OnRefreshClick(sender, e);
        }

        /// <summary>조회어 외의 조회 조건을 화면을 처음 열었을 때 값으로 돌린다. 기본은 아무것도 하지 않는다.</summary>
        protected virtual void ResetCriteria()
        {
        }

        /// <summary>조회어 Enter — 베이스가 <c>txtKeyword</c>에 연결한다. 조회 조건이 다른 화면은 재정의한다.</summary>
        protected virtual void OnKeywordEnterPressed(object sender, EventArgs e)
        {
            this.ReloadItems();
        }

        protected void OnNewClick(object sender, EventArgs e)
        {
            if (!this.CanWrite(WriteKind.New))
            {
                return;
            }

            this.BeginNewItem();
            this.view.Editor.FocusFirstEditor();
            this.UpdateActionState();
        }

        // 복제는 새 항목 입력의 한 갈래다 — 고른 행의 값을 새 항목 편집기에 옮겨 놓고, 저장은 기존 Insert 경로를 그대로 탄다.
        // 그래서 화면마다 전문을 더하지 않고 베이스의 신규 흐름에 둔다. 화면이 같은 이름(miDuplicate)의 항목을 이미 갖고 있으면
        // (서버가 키를 만들어 복제하는 Product) 그 화면의 것을 쓰고 여기서는 더하지 않는다.
        private void AddDuplicateMenuItem()
        {
            if (this.view.ActionMenu.Items.ContainsKey(DuplicateMenuName))
            {
                return;
            }

            ToolStripSeparator separator = new ToolStripSeparator();
            separator.Name = "sepDuplicate";
            this.duplicateMenuItem = new ToolStripMenuItem("Duplicate");
            this.duplicateMenuItem.Name = DuplicateMenuName;
            this.duplicateMenuItem.Click += this.OnDuplicateClick;
            this.view.ActionMenu.Items.Add(separator);
            this.view.ActionMenu.Items.Add(this.duplicateMenuItem);
        }

        private bool CanDuplicate
        {
            get { return this.CanWrite(WriteKind.New) && !this.view.Editor.IsNew && this.SelectedRowValid; }
        }

        // 키도 함께 옮긴다 — 새 항목에서는 키가 열리므로 사용자가 고쳐 쓰고, 그대로 저장하면 서버가 중복으로 거절한다.
        // 서버가 값을 만드는 읽기 전용 컬럼(채번 키 · 수정자 · 수정 시각)은 옮기지 않는다.
        private void OnDuplicateClick(object sender, EventArgs e)
        {
            if (!this.CanDuplicate)
            {
                return;
            }

            Dictionary<string, string> copied = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, string> value in this.view.Editor.ReadValues())
            {
                if (!this.view.Editor.IsReadOnlyColumn(value.Key))
                {
                    copied[value.Key] = value.Value;
                }
            }

            IDictionary<string, string> defaults = this.NewItemDefaults();

            if (defaults != null)
            {
                foreach (KeyValuePair<string, string> value in defaults)
                {
                    copied[value.Key] = value.Value;
                }
            }

            this.view.Editor.BeginNew(copied);
            this.view.Editor.FocusFirstEditor();
            this.UpdateActionState();
        }

        protected void OnCancelClick(object sender, EventArgs e)
        {
            if (!this.CanCancel)
            {
                return;
            }

            DataRow row = SelectedRow(this.view.Grid.SelectedItem);

            if (this.view.Editor.IsNew && row != null)
            {
                this.view.Editor.LoadRow(row);
            }
            else
            {
                this.view.Editor.RevertEdits();
            }

            this.UpdateActionState();
        }

        protected void OnSaveClick(object sender, EventArgs e)
        {
            if (!this.CanWrite(WriteKind.Save) || !this.ValidateRequired())
            {
                return;
            }

            object[] requestFields = this.view.Editor.ToRequestFields();
            string[] key = this.ReadKeyValues();
            bool isNew = this.view.Editor.IsNew;

            this.RunAction(
                    () => isNew ? this.InsertItem(requestFields) : this.UpdateItem(requestFields),
                    reply => this.AfterSaved(reply, key, isNew),
                    isNew ? "Inserting " + this.EntityNameLower + "…" : "Updating " + this.EntityNameLower + "…");
        }

        protected void OnDeleteClick(object sender, EventArgs e)
        {
            if (!this.CanWrite(WriteKind.Delete))
            {
                return;
            }

            string[] key = this.ReadKeyValues();

            if (!this.Confirm(
                    "Delete " + this.EntityNameLower + " '" + DisplayKey(key) + "'?",
                    "Delete " + this.definition.EntityName))
            {
                return;
            }

            this.RunAction(
                    () => this.DeleteItem(key),
                    this.AfterDeleted,
                    "Deleting " + this.EntityNameLower + "…");
        }

        protected virtual void OnActionMenuOpening(object sender, CancelEventArgs e)
        {
            this.UpdateActionState();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape && this.CanCancel && !this.view.Grid.IsFindPanelOpen)
            {
                this.OnCancelClick(this, EventArgs.Empty);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private string EntityNameLower
        {
            get { return this.definition.EntityName.ToLowerInvariant(); }
        }

        private bool ListOpen
        {
            get { return this.listState == ListState.Ready || this.listState == ListState.ReadyEmpty; }
        }

        private bool CanCancel
        {
            get
            {
                if (!this.ListOpen || this.ActionInProgress)
                {
                    return false;
                }

                return this.view.Editor.IsDirty
                        || (this.view.Editor.IsNew && this.view.Grid.SelectedItem != null);
            }
        }

        private bool SelectedRowValid
        {
            get
            {
                DataRow row = SelectedRow(this.view.Grid.SelectedItem);

                if (row == null || this.itemList == null || this.itemList.Table == null)
                {
                    return false;
                }

                int index = this.itemList.Table.Rows.IndexOf(row);
                return index >= 0 && this.itemList.IsRowValid(index);
            }
        }

        /// <summary>목록 조회 전문 — 화면의 <c>.Server.cs</c>가 정의한다(★ 교체 지점). <paramref name="keyword"/>는
        /// 조회 카드에 <c>txtKeyword</c>가 있을 때 그 값이다. <b>백그라운드 스레드에서 불린다</b> — 조건이 다른 화면은
        /// 컨트롤을 여기서 읽지 말고 UI 스레드에서 도는 <see cref="OnListLoading"/>에서 필드에 담아 둔 뒤 그 값을 쓴다.</summary>
        protected virtual DataTable RequestItems(string keyword)
        {
            throw new InvalidOperationException(
                    "조회 전문이 없다 — 화면의 .Server.cs 에서 RequestItems 를 재정의할 것.");
        }

        /// <summary>등록 전문 — <paramref name="requestFields"/>는 편집기 컬럼 전부의 이름/값 쌍(자동 생성).</summary>
        protected virtual DataActionResult InsertItem(object[] requestFields)
        {
            throw new InvalidOperationException(
                    "등록 전문이 없다 — 화면의 .Server.cs 에서 InsertItem 을 재정의할 것.");
        }

        /// <summary>수정 전문 — <paramref name="requestFields"/>는 편집기 컬럼 전부의 이름/값 쌍(자동 생성).</summary>
        protected virtual DataActionResult UpdateItem(object[] requestFields)
        {
            throw new InvalidOperationException(
                    "수정 전문이 없다 — 화면의 .Server.cs 에서 UpdateItem 을 재정의할 것.");
        }

        /// <summary>단일 키 삭제 전문 — <paramref name="key"/>는 편집기의 키 값. 단일 키 화면이 재정의한다.</summary>
        protected virtual DataActionResult DeleteItem(string key)
        {
            throw new InvalidOperationException(
                    "삭제 전문이 없다 — 화면의 .Server.cs 에서 DeleteItem 을 재정의할 것.");
        }

        /// <summary>삭제 전문 — <paramref name="keyValues"/>는 정의의 키 컬럼 순서대로 편집기의 키 값이다.
        /// 기본은 단일 키 <see cref="DeleteItem(string)"/>으로 넘긴다. 복합 키 화면은 이것을 재정의한다.</summary>
        protected virtual DataActionResult DeleteItem(string[] keyValues)
        {
            return this.DeleteItem(keyValues.Length == 0 ? string.Empty : keyValues[0]);
        }

        /// <summary>
        /// 이름/값 배열에서 지정한 파라미터 이름의 쌍을 뺀 배열 — <c>.Server.cs</c>가 편집기 값 전부(<c>ToRequestFields</c>) 가운데
        /// 서버가 만드는 컬럼·조인 결과·부모가 공급하는 키를 전문에서 제외할 때 쓴다. 이름은 <see cref="ModernPropertyGrid.ParameterNameOf"/>
        /// 결과(<c>UpdatedBy</c>)와 대소문자 구분 없이 비교한다. 응답 스키마(편집기)와 저장 필드(전문)를 여기서 가른다.
        /// </summary>
        protected static object[] WithoutFields(object[] fields, params string[] parameterNames)
        {
            List<object> kept = new List<object>();
            string[] excluded = parameterNames ?? new string[0];

            for (int index = 0; fields != null && index + 1 < fields.Length; index += 2)
            {
                string name = Convert.ToString(fields[index]);
                bool skip = false;

                foreach (string parameterName in excluded)
                {
                    if (string.Equals(name, parameterName, StringComparison.OrdinalIgnoreCase))
                    {
                        skip = true;
                        break;
                    }
                }

                if (!skip)
                {
                    kept.Add(fields[index]);
                    kept.Add(fields[index + 1]);
                }
            }

            return kept.ToArray();
        }

        /// <summary><c>MethodCommand</c> 쌍을 앞에 붙인 이름/값 배열 — <c>.Server.cs</c>가 전문을 조립할 때 쓴다.</summary>
        protected static object[] WithMethodCommand(string methodCommand, object[] fields)
        {
            return WithMethodCommand(methodCommand, fields, null);
        }

        /// <summary>
        /// <c>MethodCommand</c> 쌍을 앞에, 더 보낼 이름/값 쌍을 뒤에 붙인 배열 —
        /// <c>WithMethodCommand("InsertProduct", requestFields, "USER_ID", this.CurrentUserId)</c>처럼 편집기 값 뒤에
        /// 로그인 사번 같은 값을 한 줄로 덧붙인다(2026-10-18). 쌍은 이름·값 순서로 몇 개든 이어 적는다.
        /// </summary>
        protected static object[] WithMethodCommand(string methodCommand, object[] fields, params object[] moreFields)
        {
            int fieldCount = fields == null ? 0 : fields.Length;
            int moreCount = moreFields == null ? 0 : moreFields.Length;
            object[] result = new object[fieldCount + moreCount + 2];
            result[0] = "MethodCommand";
            result[1] = methodCommand;

            if (fieldCount > 0)
            {
                Array.Copy(fields, 0, result, 2, fieldCount);
            }

            if (moreCount > 0)
            {
                Array.Copy(moreFields, 0, result, 2 + fieldCount, moreCount);
            }

            return result;
        }

        // 목록 조회·바인딩·재선택은 이미 베이스의 흐름이고, 신규 표시는 그 조회 결과를 직전 결과와 비교하는 같은 변경 이유다.
        // 바인딩 표는 행 유효성 판정(itemList)과 같은 표여야 하므로 판정기 사본은 쓰지 않고 판정 결과만 표식에 쓴다.
        // 선택은 옮기지 않는다 — 저장 뒤 재선택·첫 행 정책을 그대로 둔다.
        private void BindItems(DataTable table, string criteria)
        {
            DataTable received = table ?? new DataTable();
            this.DeclareEmptyListColumns(received);

            this.itemList = TableResponse.Read(
                    ResponseKind.Data, received, ColumnAliasCatalog.Empty, this.listContract);
            this.listState = this.ListStateOf(this.itemList);

            if (this.listState == ListState.Blocked)
            {
                this.pendingSelectKey = null;
                this.view.Editor.SetSchema(new DataTable());
                this.view.Editor.BeginNew();
                this.view.Grid.RowMarkerSelector = null;
                this.view.Grid.DataSource = received;
                this.ShowBlockedNotice();
                this.UpdateActionState();
                return;
            }

            this.HideBlockedNotice();

            DataTable bound = this.itemList.Table;
            this.view.Editor.SetSchema(bound);
            this.newItems.Track(bound, this.view.Grid.RowKeyMember, criteria);
            this.ConfigureListColumns(bound);
            this.view.Grid.DataSource = bound;
            NewItemTint.Bind(this.view.Grid, this.newItems, this.view.Grid.RowKeyMember, null, false);

            if (HasKey(this.pendingSelectKey))
            {
                this.SelectByKey(bound, this.pendingSelectKey);
            }

            this.pendingSelectKey = null;

            if (this.view.Grid.SelectedItem == null)
            {
                this.BeginNewItem();
            }
            else
            {
                this.view.Editor.LoadRow(SelectedRow(this.view.Grid.SelectedItem));
            }

            this.UpdateActionState();
        }

        // 서버는 0건일 때 컬럼을 보내지 않는다. 엔티티에 표시 목록(DisplayFields)이 있으면 그 목록과 키로 빈 목록의 컬럼을
        // 세워 그리드 헤더와 편집기 칸을 만든다 — 업무 화면 그리드의 0건 헤더와 같은 출처다. 표시 목록이 없으면 손대지 않아
        // 지금처럼 Blocked 로 남는다. 행이 있거나 컬럼이 하나라도 온 응답은 받은 그대로 판정한다.
        private void DeclareEmptyListColumns(DataTable received)
        {
            EntityDefinition entity = this.definition.Entity;

            if (entity == null || entity.DisplayFields.Count == 0
                    || received.Columns.Count > 0 || received.Rows.Count > 0)
            {
                return;
            }

            foreach (string name in EntityDefinition.Names(entity.DisplayFields))
            {
                if (!received.Columns.Contains(name))
                {
                    received.Columns.Add(name, typeof(string));
                }
            }

            foreach (string key in this.keyColumns)
            {
                if (!received.Columns.Contains(key))
                {
                    received.Columns.Add(key, typeof(string));
                }
            }
        }

        private void AfterListSettled(bool current)
        {
            if (!current || this.listState != ListState.Loading)
            {
                return;
            }

            this.pendingSelectKey = null;
            this.itemList = TableResponse.Read(
                    ResponseKind.Failed,
                    null,
                    ColumnAliasCatalog.Empty,
                    this.listContract,
                    string.Empty,
                    this.definition.EntityName + " list could not be loaded.");
            this.listState = ListState.Failed;
            this.UpdateActionState();
        }

        private ListState ListStateOf(TableResponse response)
        {
            if (response.State == TableResponseState.Failed)
            {
                return ListState.Failed;
            }

            if (response.ReceivedColumns.Count == 0
                    || response.State == TableResponseState.MissingRequired
                    || this.MissingEditorColumns(response).Length > 0
                    || this.ReservedEditorColumns(response).Length > 0)
            {
                return ListState.Blocked;
            }

            return response.State == TableResponseState.Empty ? ListState.ReadyEmpty : ListState.Ready;
        }

        private string[] MissingEditorColumns(TableResponse response)
        {
            List<string> missing = new List<string>();
            string[] required = this.definition.RequiredEditorColumns ?? new string[0];

            foreach (string column in required)
            {
                bool found = false;

                foreach (string received in response.ReceivedColumns)
                {
                    if (string.Equals(received, column, StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    missing.Add(column);
                }
            }

            return missing.ToArray();
        }

        private string[] ReservedEditorColumns(TableResponse response)
        {
            List<string> reserved = new List<string>();

            foreach (string received in response.ReceivedColumns)
            {
                string parameterName = ParameterNameConverter.Convert(
                        received, this.view.Editor.ParameterNameStyle);

                if (string.Equals(parameterName, "MethodCommand", StringComparison.OrdinalIgnoreCase))
                {
                    reserved.Add(received);
                }
            }

            return reserved.ToArray();
        }

        private bool CanWrite(WriteKind kind)
        {
            if (!this.ListOpen || this.ActionInProgress)
            {
                return false;
            }

            if (kind == WriteKind.New)
            {
                return true;
            }

            if (this.view.Editor.IsNew)
            {
                return kind == WriteKind.Save;
            }

            // 기존 행의 Save 는 값이 바뀐 동안만 연다 — 안 바꾸고 눌러도 Update 전문이 나가 서버가 같은 값으로 다시 쓰던 것을
            // 막는다(2026-10-18 사용자 결정). 판정은 Cancel 과 같은 IsDirty 하나다.
            if (kind == WriteKind.Save)
            {
                return this.SelectedRowValid && this.view.Editor.IsDirty;
            }

            return this.SelectedRowValid;
        }

        private void ShowBlockedNotice()
        {
            List<string> details = new List<string>();
            List<string> missing = new List<string>(this.itemList.MissingColumns);
            missing.AddRange(this.MissingEditorColumns(this.itemList));

            if (missing.Count > 0)
            {
                details.Add("missing " + string.Join(", ", missing.ToArray()));
            }

            string[] reserved = this.ReservedEditorColumns(this.itemList);

            if (reserved.Length > 0)
            {
                details.Add("reserved " + string.Join(", ", reserved));
            }

            string text = this.itemList.ReceivedColumns.Count == 0
                    ? this.definition.EntityName + " list did not match the contract (no columns) — writes are disabled."
                    : this.definition.EntityName + " list did not match the contract ("
                            + string.Join("; ", details.ToArray()) + ") — writes are disabled.";

            this.blockedNoticeShowing = true;
            this.ShowStickyNotice(text, Modern.Lab.Controls.Wpf.Display.ToastKind.Warning);
        }

        private void HideBlockedNotice()
        {
            if (!this.blockedNoticeShowing)
            {
                return;
            }

            this.blockedNoticeShowing = false;
            this.HideNotice();
        }

        private void SelectByKey(DataTable table, string[] key)
        {
            if (table == null || key.Length != this.keyColumns.Length)
            {
                return;
            }

            foreach (string column in this.keyColumns)
            {
                if (!table.Columns.Contains(column))
                {
                    return;
                }
            }

            for (int i = 0; i < table.Rows.Count; i = i + 1)
            {
                if (this.RowHasKey(table.Rows[i], key))
                {
                    this.view.Grid.SelectedIndex = i;
                    return;
                }
            }
        }

        private bool RowHasKey(DataRow row, string[] key)
        {
            for (int i = 0; i < this.keyColumns.Length; i = i + 1)
            {
                if (!string.Equals(
                        Convert.ToString(row[this.keyColumns[i]]),
                        key[i],
                        StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 저장·삭제·재선택에 쓰는 키 값(정의의 키 컬럼 순서). 단일 키는 편집기 키의 앞뒤 공백을 지운다(기존 동작).
        /// 복합 키는 신규만 공백을 지우고, 기존 행은 조회한 행의 원값을 그대로 쓴다 — 서버가 공백을 구별해도
        /// 같은 행을 가리킨다. 단일 키 화면이 원값을 써야 하면 재정의한다. UI 스레드에서 불린다.
        /// </summary>
        protected virtual string[] ReadKeyValues()
        {
            if (!this.CompositeKey)
            {
                return new string[] { this.view.Editor.ReadKey().Trim() };
            }

            DataRow stored = this.view.Editor.IsNew ? null : SelectedRow(this.view.Grid.SelectedItem);
            string[] values = new string[this.keyColumns.Length];

            for (int i = 0; i < this.keyColumns.Length; i = i + 1)
            {
                string column = this.keyColumns[i];
                values[i] = stored != null && stored.Table.Columns.Contains(column)
                        ? Convert.ToString(stored[column])
                        : this.view.Editor.ReadValue(column).Trim();
            }

            return values;
        }

        private static string DisplayKey(string[] key)
        {
            return string.Join(" / ", key);
        }

        private static bool HasKey(string[] key)
        {
            if (key == null || key.Length == 0)
            {
                return false;
            }

            foreach (string value in key)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return false;
                }
            }

            return true;
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

        private void OnEditorValueChanged(object sender, EventArgs e)
        {
            this.UpdateActionState();
        }

        private bool ValidateRequired()
        {
            string[] missing = this.view.Editor.MissingRequired();

            if (missing.Length == 0)
            {
                return true;
            }

            List<string> captions = new List<string>();

            foreach (string column in missing)
            {
                captions.Add(this.view.Editor.CaptionOf(column));
            }

            this.ShowToast(
                    "Required: " + string.Join(", ", captions.ToArray()),
                    Modern.Lab.Controls.Wpf.Display.ToastKind.Warning);
            this.view.Editor.FocusFirstEditor();

            return false;
        }

        private void AfterSaved(DataActionResult reply, string[] selectKey, bool isNew)
        {
            this.ShowToast(reply.Message.Length > 0 ? reply.Message : "Done.");
            this.pendingSelectKey = this.SavedItemKey(reply, selectKey, isNew);
            this.ReloadItems();
        }

        /// <summary>
        /// 저장 성공 뒤 재조회에서 다시 선택할 키. 기본: 복합 키는 저장한 키, 단일 키는 등록·수정 모두 서버가 돌려준 키를
        /// 먼저 쓰고 없으면 저장한 키를 쓴다. 서버 반환 키 대신 조회 원값으로 찾아야 하는 화면만 재정의한다.
        /// </summary>
        protected virtual string[] SavedItemKey(DataActionResult reply, string[] savedKey, bool isNew)
        {
            return this.CompositeKey
                    ? savedKey
                    : new string[] { ReturnedKey(reply) ?? savedKey[0] };
        }

        private void AfterDeleted(DataActionResult reply)
        {
            this.ShowToast(reply.Message.Length > 0 ? reply.Message : "Done.");
            this.pendingSelectKey = null;
            this.ReloadItems();
        }

        private static string ReturnedKey(DataActionResult reply)
        {
            if (reply == null || reply.Data.Length == 0)
            {
                return null;
            }

            string[] values = reply.Values;

            return values.Length > 0 && values[0].Trim().Length > 0 ? values[0].Trim() : null;
        }

        /// <summary>
        /// 저장·삭제가 시작되면 조회어·조회 버튼·목록·편집기를 잠그고, 끝나면(성공·실패·예외) 잠그기 전 상태로 되돌린다.
        /// 화면에만 있는 조회 입력(상위 선택 콤보 등)은 <see cref="OnWriteLockChanged"/>에서 같이 잠근다.
        /// </summary>
        protected override void OnActionStateChanged(bool running)
        {
            base.OnActionStateChanged(running);

            if (this.view == null || running == this.writeLocked)
            {
                return;
            }

            this.writeLocked = running;

            if (running)
            {
                this.keywordWasEnabled = this.view.Keyword != null && this.view.Keyword.Enabled;
                this.refreshWasEnabled = this.view.RefreshButton != null && this.view.RefreshButton.Enabled;
                this.resetWasEnabled = this.view.ResetButton != null && this.view.ResetButton.Enabled;
                this.gridWasEnabled = this.view.Grid.Enabled;
            }

            if (this.view.Keyword != null)
            {
                this.view.Keyword.Enabled = !running && this.keywordWasEnabled;
            }

            if (this.view.RefreshButton != null)
            {
                this.view.RefreshButton.Enabled = !running && this.refreshWasEnabled;
            }

            if (this.view.ResetButton != null)
            {
                this.view.ResetButton.Enabled = !running && this.resetWasEnabled;
            }

            this.view.Grid.Enabled = !running && this.gridWasEnabled;
            this.UpdateActionState();
            this.OnWriteLockChanged(running);
        }

        /// <summary>쓰기 중 잠금이 걸리거나(<paramref name="locked"/> true) 풀린 직후 불린다. 기본은 아무것도 하지 않는다.</summary>
        protected virtual void OnWriteLockChanged(bool locked)
        {
        }

        private void UpdateActionState()
        {
            this.view.Editor.Enabled = this.ListOpen && !this.ActionInProgress;
            this.view.NewButton.Enabled = this.CanWrite(WriteKind.New);
            this.view.CancelButton.Enabled = this.CanCancel;
            this.view.SaveButton.Enabled = this.CanWrite(WriteKind.Save);
            this.view.DeleteButton.Enabled = this.CanWrite(WriteKind.Delete);
            this.view.NewMenuItem.Enabled = this.view.NewButton.Enabled;
            this.view.CancelMenuItem.Enabled = this.view.CancelButton.Enabled;
            this.view.SaveMenuItem.Enabled = this.view.SaveButton.Enabled;
            this.view.DeleteMenuItem.Enabled = this.view.DeleteButton.Enabled;

            if (this.duplicateMenuItem != null)
            {
                this.duplicateMenuItem.Enabled = this.CanDuplicate;
            }

            this.view.EditorCard.Text = this.view.Editor.IsNew
                    ? this.definition.EntityName + " — New"
                    : this.definition.EntityName + " — " + this.EditorKeyText();
        }

        private string EditorKeyText()
        {
            if (!this.CompositeKey)
            {
                return this.view.Editor.ReadKey();
            }

            string[] values = new string[this.keyColumns.Length];

            for (int i = 0; i < this.keyColumns.Length; i = i + 1)
            {
                values[i] = this.view.Editor.ReadValue(this.keyColumns[i]);
            }

            return DisplayKey(values);
        }
    }
}
