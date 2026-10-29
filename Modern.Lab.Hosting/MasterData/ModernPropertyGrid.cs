using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;

using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Controls.Wpf.Input;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.Hosting.MasterData
{
    /// <summary>
    /// 표 스키마로 편집기를 스스로 구성하는 속성 편집기.
    ///
    /// 서버가 SELECT한 컬럼을 <b>그 순서대로</b> "라벨 + 편집 컨트롤" 행으로 전부 뿌린다.
    /// 컬럼 수가 늘면 패널 폭에 맞춰 열을 나눠 흘려 넣는다(<see cref="MinColumnWidth"/>마다 한 열).
    /// 기본 편집기는 텍스트박스이고, 폼이 <see cref="Field"/>로 컬럼을 골라 <c>.Combo(…)</c>·<c>.Toggle()</c>을 이은
    /// 컬럼만 콤보박스·스위치가 된다. 라벨은 그리드 헤더와 같은 순서로 정해진다 — <see cref="GridCaptionCatalog.Humanize"/> →
    /// 캡션 사전 → 엔티티(<see cref="UseEntityLabels"/>) → 화면의 <c>.Field(…).Label(…).TopLabel(…)</c>(2026-10-04).
    ///
    /// 입력칸 전부는 WPF 섬 하나(<see cref="ModernPropertyGridControl"/>) 안에 그려진다(2026-10-29) — 전에는 컬럼마다
    /// WinForms 래퍼(ElementHost)를 만들어 40컬럼이면 섬 40개가 생겼고 생성 1.4초 · 크기 변경 0.4초 · 열 수 변경 1.4초가 들었다
    /// (design-notes 7절). 이 래퍼는 스키마 · 종류 지정 · 라벨 층 · 키/필수/읽기 전용 · 되돌리기 기준을 들고, 그리기와 값 읽기·쓰기는
    /// 컬럼 이름으로 WPF 쪽에 맡긴다. 공개 API 와 화면의 <c>.Grid.cs</c> 체인은 그대로다.
    ///
    /// 콤보 항목은 전문으로 채운다 — <c>.Field(…).Combo(…)</c>에 전문 이름·필드를 적고, 폼이
    /// <see cref="FetchComboSources"/>(작업 스레드) → <see cref="ApplyComboSources"/>(UI 스레드)를
    /// 자기 <c>LoadAsync</c>로 돌린다. 편집기는 서버를 모르고 폼은 컬럼 종류만 안다.
    /// 콤보를 펼칠 때는 <see cref="ComboRefresh"/>로 같은 전문을 다시 보내 목록만 갱신한다.
    /// </summary>
    [ToolboxItem(true)]
    public class ModernPropertyGrid : WpfElementHostBase<ModernPropertyGridControl>
    {
        private readonly HashSet<string> keyColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> readOnlyColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> requiredColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> createOnlyColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, FieldDefinition> definitions =
                new Dictionary<string, FieldDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DataTable> comboSources =
                new Dictionary<string, DataTable>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> defaultValues =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, LabelDefinition> labels =
                new Dictionary<string, LabelDefinition>(StringComparer.OrdinalIgnoreCase);
        private Modern.Lab.WinForms.Controls.Data.EntityDefinition labelEntity;

        private Dictionary<string, string> baseline =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private string keyColumnText = string.Empty;
        private string[] schemaColumns = new string[0];
        private int labelWidth = 140;
        private int minColumnWidth = 420;
        private int editorHeight = 28;
        private int multilineHeight = 72;
        private int rowGap = 4;
        private bool isNew = true;
        private ParameterNameStyle parameterNameStyle = ParameterNameStyle.PascalCase;

        /// <summary>편집 값이 바뀔 때 발생한다(어느 컬럼이든).</summary>
        public event EventHandler ValueChanged;

        public ModernPropertyGrid()
        {
            this.Size = new Size(480, 320);

            if (this.Wpf != null)
            {
                this.Wpf.ValueChanged += this.OnWpfValueChanged;
                this.Wpf.DropDownRefresh = this.RefreshDropDownItems;
            }
        }

        // ===== 설정 =====

        /// <summary>
        /// 키 컬럼 이름(쉼표 구분). 새 행에서는 편집할 수 있고, 기존 행을 불러오면 읽기 전용이 된다.
        /// 폼이 이름을 알아야 하는 유일한 컬럼이다.
        /// </summary>
        [Category("Modern")]
        [Description("Key column names (comma separated). Editable only for a new row.")]
        [DefaultValue("")]
        public string KeyColumns
        {
            get
            {
                return this.keyColumnText;
            }
            set
            {
                this.keyColumnText = value ?? string.Empty;
                this.keyColumns.Clear();

                foreach (string part in this.keyColumnText.Split(','))
                {
                    string name = part.Trim();

                    if (name.Length > 0)
                    {
                        this.keyColumns.Add(name);
                    }
                }

                this.ApplyKeyState();
            }
        }

        /// <summary>라벨 열 너비(px).</summary>
        [Category("Modern")]
        [Description("Width of each label column in pixels.")]
        [DefaultValue(140)]
        public int LabelWidth
        {
            get
            {
                return this.labelWidth;
            }
            set
            {
                this.labelWidth = Math.Max(60, value);

                if (this.Wpf != null)
                {
                    this.Wpf.LabelWidth = this.labelWidth;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>한 열(라벨+편집기)이 차지할 최소 폭. 패널 폭을 이 값으로 나눈 만큼 열이 생긴다.</summary>
        [Category("Modern")]
        [Description("Minimum width of one label+editor column; the panel width divided by this gives the column count.")]
        [DefaultValue(420)]
        public int MinColumnWidth
        {
            get
            {
                return this.minColumnWidth;
            }
            set
            {
                this.minColumnWidth = Math.Max(200, value);

                if (this.Wpf != null)
                {
                    this.Wpf.MinColumnWidth = this.minColumnWidth;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>한 줄 입력칸 높이(px). 줄이면 같은 화면에 더 많은 행이 들어간다.</summary>
        [Category("Modern")]
        [Description("Height of a single-line editor in pixels.")]
        [DefaultValue(28)]
        public int EditorHeight
        {
            get
            {
                return this.editorHeight;
            }
            set
            {
                this.editorHeight = Math.Max(20, value);

                if (this.Wpf != null)
                {
                    this.Wpf.EditorHeight = this.editorHeight;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>여러 줄 입력칸 높이(px).</summary>
        [Category("Modern")]
        [Description("Height of a multiline editor in pixels.")]
        [DefaultValue(72)]
        public int MultilineHeight
        {
            get
            {
                return this.multilineHeight;
            }
            set
            {
                this.multilineHeight = Math.Max(40, value);

                if (this.Wpf != null)
                {
                    this.Wpf.MultilineHeight = this.multilineHeight;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>행과 행 사이 간격(px).</summary>
        [Category("Modern")]
        [Description("Vertical gap between rows in pixels.")]
        [DefaultValue(4)]
        public int RowGap
        {
            get
            {
                return this.rowGap;
            }
            set
            {
                this.rowGap = Math.Max(0, value);

                if (this.Wpf != null)
                {
                    this.Wpf.RowGap = this.rowGap;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>
        /// <see cref="ToRequestFields"/>가 컬럼 이름을 전문 파라미터 이름으로 바꾸는 방식.
        /// 기본은 PascalCase(<c>PROD_ID</c> → <c>ProdId</c>). 예외 표는 없다 — 규칙과 다른 이름이 필요하면 컬럼 쪽을 맞춘다.
        /// </summary>
        [Category("Modern")]
        [Description("How column names become request parameter names (PROD_ID -> ProdId).")]
        [DefaultValue(ParameterNameStyle.PascalCase)]
        public ParameterNameStyle ParameterNameStyle
        {
            get { return this.parameterNameStyle; }
            set { this.parameterNameStyle = value; }
        }

        /// <summary>
        /// 전문으로 선언된 콤보(<c>.Field(…).Combo(전문 이름, …)</c>)를 <b>펼칠 때</b> 목록을 다시 받아 오는 조회
        /// — (전문 이름, 필드 쌍) → 표. 다른 화면에서 방금 추가한 코드를 화면을 다시 열지 않고 고를 수 있게 한다.
        /// UI 스레드에서 동기로 불린다. null을 돌려주면(조회 실패) 지금 목록을 그대로 둔다. 지정하지 않으면 펼칠 때 조회하지 않는다.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<string, object[], DataTable> ComboRefresh { get; set; }

        /// <summary>새 행 입력 중이면 true, 기존 행을 불러온 상태면 false.</summary>
        [Browsable(false)]
        public bool IsNew
        {
            get { return this.isNew; }
        }

        /// <summary>현재 편집기가 나열한 컬럼 이름(스키마 순서).</summary>
        [Browsable(false)]
        public string[] Columns
        {
            get { return (string[])this.schemaColumns.Clone(); }
        }

        // ===== 컬럼 종류 지정 (폼이 아는 몇 개만) =====

        /// <summary>
        /// 이 컬럼을 콤보박스로 만들고, 항목은 <paramref name="requestName"/> 전문(<paramref name="requestFields"/>는
        /// 이름/값 쌍)으로 채운다. 응답 표의 <paramref name="valueMember"/>가 저장 값, <paramref name="displayMember"/>가 표시 글자다.
        /// </summary>
        internal void DefineCombo(
                string column, string requestName, string valueMember, string displayMember, params object[] requestFields)
        {
            FieldDefinition definition = new FieldDefinition(PropertyEditorKind.Combo);
            definition.RequestName = requestName;
            definition.RequestFields = requestFields ?? new object[0];
            definition.ValueMember = string.IsNullOrEmpty(valueMember) ? "CODE" : valueMember;
            definition.DisplayMember = string.IsNullOrEmpty(displayMember) ? definition.ValueMember : displayMember;

            this.Define(column, definition);
        }

        /// <summary>이 컬럼을 콤보박스로 만들고, 항목은 폼이 직접 넘긴 표로 채운다(서버 조회가 없는 고정 목록용).</summary>
        internal void DefineCombo(string column, DataTable items, string valueMember, string displayMember)
        {
            FieldDefinition definition = new FieldDefinition(PropertyEditorKind.Combo);
            definition.ValueMember = string.IsNullOrEmpty(valueMember) ? "CODE" : valueMember;
            definition.DisplayMember = string.IsNullOrEmpty(displayMember) ? definition.ValueMember : displayMember;

            this.comboSources[column] = items;
            this.Define(column, definition);
        }

        /// <summary>이 컬럼을 Y/N 스위치로 만든다.</summary>
        internal void DefineToggle(string column)
        {
            this.Define(column, new FieldDefinition(PropertyEditorKind.Toggle));
        }

        /// <summary>이 컬럼을 여러 줄 텍스트로 만든다(한 행을 통째로 차지한다).</summary>
        internal void DefineMultiline(string column)
        {
            this.Define(column, new FieldDefinition(PropertyEditorKind.MultilineText));
        }

        /// <summary>이 컬럼을 날짜 선택기로 만든다(값은 yyyy-MM-dd).</summary>
        internal void DefineDate(string column)
        {
            this.Define(column, new FieldDefinition(PropertyEditorKind.Date));
        }

        /// <summary>이 컬럼을 숫자 입력으로 만든다.</summary>
        internal void DefineNumber(string column, int decimalPlaces)
        {
            FieldDefinition definition = new FieldDefinition(PropertyEditorKind.Number);
            definition.DecimalPlaces = Math.Max(0, decimalPlaces);
            this.Define(column, definition);
        }

        /// <summary>
        /// 새 행 입력(<see cref="BeginNew()"/>)을 시작할 때 이 컬럼에 채울 값. 기존 행을 싣는 <see cref="LoadRow"/>에는
        /// 적용하지 않는다 — 조회 값이 비어 있어도 덮지 않는다. 응답에 없는 컬럼이면 무시된다.
        /// </summary>
        internal void DefineDefaultValue(string column, string value)
        {
            if (string.IsNullOrEmpty(column))
            {
                return;
            }

            this.defaultValues[column] = value ?? string.Empty;
        }

        /// <summary>
        /// 컬럼을 고른다 — 편집기 종류·라벨·읽기 전용 같은 설정은 돌려받은 <see cref="PropertyGridField"/>에 점으로 잇는다
        /// (그리드 <c>GridColumns.Column(…)</c>·캡션 사전·엔티티와 같은 문법, 2026-10-04 통일).
        /// <c>this.CrudEditor.Field("USE_YN").Toggle().DefaultValue("Y");</c> 이름을 여러 개 주면 같은 설정이 모두에 걸린다.
        /// </summary>
        public PropertyGridField Field(params string[] columns)
        {
            return new PropertyGridField(this, columns);
        }

        /// <summary>
        /// 엔티티 한 벌을 편집기에 연결한다 — 키(<c>KeyFields</c>) · 필수(<c>RequiredFields</c>) · 신규 전용(<c>CreateOnlyFields</c>) ·
        /// 읽기 전용(<c>ReadOnlyFields</c>) · 라벨을 한 번에 적용한다.
        /// 기준정보 CRUD 베이스와 편집기를 단독으로 놓는 다이얼로그가 같은 메서드를 불러 연결 방식이 갈리지 않게 한다(2026-10-11).
        /// 화면별 꾸밈(콤보·스위치·추가 필수)은 이 뒤에 <see cref="Field"/> 체인으로 이어 적는다.
        /// </summary>
        public void UseEntity(Modern.Lab.WinForms.Controls.Data.EntityDefinition entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            this.KeyColumns = entity.RowKey;
            this.DefineRequired(Modern.Lab.WinForms.Controls.Data.EntityDefinition.Names(entity.RequiredFields));
            this.DefineCreateOnly(Modern.Lab.WinForms.Controls.Data.EntityDefinition.Names(entity.CreateOnlyFields));
            this.DefineReadOnly(Modern.Lab.WinForms.Controls.Data.EntityDefinition.Names(entity.ReadOnlyFields));
            this.UseEntityLabels(entity);
        }

        /// <summary>
        /// 라벨이 읽을 엔티티 — 필드의 <c>Caption</c>·<c>TopLabel</c>·<c>BottomLabel</c>이 캡션 사전보다 앞선다.
        /// 보통 <see cref="UseEntity"/>가 함께 부른다. null 이면 엔티티 층을 건너뛴다.
        /// </summary>
        public void UseEntityLabels(Modern.Lab.WinForms.Controls.Data.EntityDefinition entity)
        {
            this.labelEntity = entity;
            this.RebuildIfBuilt();
        }

        // 화면이 정하는 라벨 본문. 아래 층(엔티티·사전)에서 받은 위·아래 작은 글씨는 함께 지운다 — 그리드의 Caption 과 같은 규칙.
        internal void SetLabel(string column, string text)
        {
            if (string.IsNullOrEmpty(column))
            {
                return;
            }

            LabelDefinition definition = this.ScreenLabel(column);
            definition.Text = text ?? string.Empty;
            definition.TopLabel = null;
            definition.BottomLabel = null;
            this.RebuildIfBuilt();
        }

        internal void SetTopLabel(string column, string text)
        {
            if (string.IsNullOrEmpty(column))
            {
                return;
            }

            this.ScreenLabel(column).TopLabel = text ?? string.Empty;
            this.RebuildIfBuilt();
        }

        internal void SetBottomLabel(string column, string text)
        {
            if (string.IsNullOrEmpty(column))
            {
                return;
            }

            this.ScreenLabel(column).BottomLabel = text ?? string.Empty;
            this.RebuildIfBuilt();
        }

        private LabelDefinition ScreenLabel(string column)
        {
            LabelDefinition definition;

            if (!this.labels.TryGetValue(column, out definition))
            {
                definition = new LabelDefinition();
                this.labels[column] = definition;
            }

            return definition;
        }

        private void RebuildIfBuilt()
        {
            if (this.schemaColumns.Length > 0)
            {
                this.Rebuild();
            }
        }

        /// <summary>
        /// 이 컬럼들을 읽기 전용으로 만든다. 값은 보이지만 고칠 수 없고, 저장 전문에는 그대로 실린다.
        /// 입력칸은 테마의 읽기 전용(비활성) 색으로 그려진다. 키 컬럼을 넣으면 "신규일 때만 열린다"는
        /// 규칙을 덮어 신규에서도 잠기고 필수 검사에서도 빠진다 — 서버가 PK를 만드는 화면용이다.
        /// </summary>
        internal void DefineReadOnly(params string[] columns)
        {
            foreach (string column in columns ?? new string[0])
            {
                if (!string.IsNullOrEmpty(column))
                {
                    this.readOnlyColumns.Add(column);
                }
            }

            this.ApplyKeyState();
        }

        /// <summary>이 컬럼이 항상 읽기 전용인가(<c>.Field(…).ReadOnly()</c>) — 서버가 값을 만드는 컬럼이라 복제할 때 옮기지 않는다.</summary>
        public bool IsReadOnlyColumn(string column)
        {
            return !string.IsNullOrEmpty(column) && this.readOnlyColumns.Contains(column);
        }

        /// <summary>
        /// 이 컬럼들을 신규 전용 편집으로 만든다 — 새 행에서는 편집할 수 있고, 기존 행을 불러오면 키처럼 읽기 전용이 된다.
        /// 필수 여부는 바뀌지 않는다(필수는 <c>.Field(…).Required()</c>). 키 컬럼은 지정 없이 신규 전용이다.
        /// </summary>
        internal void DefineCreateOnly(params string[] columns)
        {
            foreach (string column in columns ?? new string[0])
            {
                if (!string.IsNullOrEmpty(column))
                {
                    this.createOnlyColumns.Add(column);
                }
            }

            this.ApplyKeyState();
        }

        /// <summary>이 컬럼들을 필수 입력으로 표시한다(라벨 별표 + 입력칸 빨간 바). 키 컬럼은 지정 없이 필수다.</summary>
        internal void DefineRequired(params string[] columns)
        {
            foreach (string column in columns ?? new string[0])
            {
                if (!string.IsNullOrEmpty(column))
                {
                    this.requiredColumns.Add(column);
                }
            }

            this.ApplyRequiredMarks();
        }

        /// <summary>
        /// 값이 비어 있는 필수 컬럼 이름들(스키마 순서). 비어 있으면 저장해도 된다.
        /// 읽기 전용 컬럼은 사용자가 채울 수 없으므로 필수라도 세지 않는다(서버가 만드는 키 등).
        /// </summary>
        public string[] MissingRequired()
        {
            List<string> missing = new List<string>();

            foreach (string column in this.schemaColumns)
            {
                if (this.IsRequired(column) && !this.ReadOnlyOf(column) && this.ReadValue(column).Trim().Length == 0)
                {
                    missing.Add(column);
                }
            }

            return missing.ToArray();
        }

        /// <summary>컬럼의 한 줄 라벨 — 위·본·아래를 공백으로 잇는다. 필수 누락 안내가 읽는다. 출처 순서는 라벨과 같다.</summary>
        public string CaptionOf(string column)
        {
            LabelDefinition definition = this.LabelOf(column);
            return GridCaptionCatalog.PlainCaption(definition.TopLabel, definition.Text, definition.BottomLabel);
        }

        // 라벨 글자의 출처 — 그리드 헤더와 같은 순서로 덮어쓴다(2026-10-04): Humanize → 캡션 사전 → 엔티티 → 화면(.Grid.cs).
        // 뒤 층이 본문을 정하면 앞 층의 위·아래 작은 글씨는 지워지고, 뒤 층이 작은 글씨만 주면 앞 층의 본문 위에 얹힌다.
        private LabelDefinition LabelOf(string column)
        {
            // 사전·Humanize 층은 그리드가 캡션 인자 없는 컬럼을 만들 때와 같은 해석이다.
            ModernDataGridColumn dictionary = new ModernDataGridColumn(column);
            LabelDefinition result = new LabelDefinition();
            result.Text = dictionary.HeaderText ?? string.Empty;
            result.TopLabel = dictionary.TopLabel ?? string.Empty;
            result.BottomLabel = dictionary.BottomLabel ?? string.Empty;

            Modern.Lab.WinForms.Controls.Data.Field field = this.EntityField(column);

            if (field != null)
            {
                Overlay(result, field.CaptionSetting, field.TopLabelSetting, field.BottomLabelSetting);
            }

            LabelDefinition screen;

            if (this.labels.TryGetValue(column, out screen))
            {
                Overlay(result, screen.Text, screen.TopLabel, screen.BottomLabel);
            }

            if (result.Text.Length == 0)
            {
                result.Text = GridCaptionCatalog.Humanize(column);
            }

            return result;
        }

        // null 은 "이 층은 정하지 않았다"다.
        private static void Overlay(LabelDefinition target, string text, string topLabel, string bottomLabel)
        {
            if (text != null)
            {
                target.Text = text;
                target.TopLabel = string.Empty;
                target.BottomLabel = string.Empty;
            }

            if (topLabel != null)
            {
                target.TopLabel = topLabel;
            }

            if (bottomLabel != null)
            {
                target.BottomLabel = bottomLabel;
            }
        }

        private Modern.Lab.WinForms.Controls.Data.Field EntityField(string column)
        {
            if (this.labelEntity == null)
            {
                return null;
            }

            foreach (Modern.Lab.WinForms.Controls.Data.Field field in this.labelEntity.AllFields)
            {
                if (string.Equals(field.Name, column, StringComparison.OrdinalIgnoreCase))
                {
                    return field;
                }
            }

            return null;
        }

        private bool IsRequired(string column)
        {
            return this.keyColumns.Contains(column) || this.requiredColumns.Contains(column);
        }

        // 지금 잠겨 있는가 — 항상 읽기 전용이거나, 키·신규 전용 컬럼인데 기존 행을 싣고 있을 때.
        private bool ReadOnlyOf(string column)
        {
            bool createOnly = this.keyColumns.Contains(column) || this.createOnlyColumns.Contains(column);
            return this.readOnlyColumns.Contains(column) || (createOnly && !this.isNew);
        }

        /// <summary>지정을 지워 기본 텍스트박스로 되돌린다.</summary>
        internal void DefineText(string column)
        {
            if (this.definitions.Remove(column))
            {
                this.RebuildIfBuilt();
            }
        }

        private void Define(string column, FieldDefinition definition)
        {
            if (string.IsNullOrEmpty(column))
            {
                return;
            }

            this.definitions[column] = definition;
            this.RebuildIfBuilt();
        }

        // ===== 콤보 항목 채우기 =====

        /// <summary>
        /// 전문으로 선언된 콤보들의 항목을 한 번에 가져온다. <paramref name="send"/>는 (전문 이름, 필드 쌍) → 표이고,
        /// 보통 폼의 <c>RequestFields(action, fields).Table</c>이다. 작업 스레드에서 불러도 된다 — UI를 건드리지 않는다.
        /// 같은 전문·같은 필드는 한 번만 보낸다.
        /// </summary>
        public Dictionary<string, DataTable> FetchComboSources(Func<string, object[], DataTable> send)
        {
            Dictionary<string, DataTable> result = new Dictionary<string, DataTable>(StringComparer.OrdinalIgnoreCase);

            if (send == null)
            {
                return result;
            }

            Dictionary<string, DataTable> byRequest = new Dictionary<string, DataTable>(StringComparer.Ordinal);

            foreach (KeyValuePair<string, FieldDefinition> entry in this.definitions)
            {
                FieldDefinition definition = entry.Value;

                if (definition.Kind != PropertyEditorKind.Combo || string.IsNullOrEmpty(definition.RequestName))
                {
                    continue;
                }

                string signature = definition.RequestSignature();
                DataTable table;

                if (!byRequest.TryGetValue(signature, out table))
                {
                    table = send(definition.RequestName, definition.RequestFields) ?? new DataTable();
                    byRequest[signature] = table;
                }

                result[entry.Key] = table;
            }

            return result;
        }

        /// <summary><see cref="FetchComboSources"/> 결과를 콤보에 싣는다. UI 스레드에서 부른다. 현재 편집 값은 유지된다.</summary>
        public void ApplyComboSources(Dictionary<string, DataTable> sources)
        {
            if (sources == null)
            {
                return;
            }

            foreach (KeyValuePair<string, DataTable> entry in sources)
            {
                this.comboSources[entry.Key] = entry.Value;
                this.BindComboColumn(entry.Key, entry.Value);
            }
        }

        // 콤보 컬럼에 항목을 싣고 원래 값을 다시 쓴다 — 바인딩이 첫 항목을 고르더라도 원래 값(빈 값 포함)이 남는다.
        private void BindComboColumn(string column, DataTable items)
        {
            FieldDefinition definition;

            if (this.Wpf == null || !this.IsSchemaColumn(column)
                    || !this.definitions.TryGetValue(column, out definition) || definition.Kind != PropertyEditorKind.Combo)
            {
                return;
            }

            string kept = this.Wpf.GetValue(column);
            this.Wpf.BindComboItems(column, ItemsOf(items));
            this.Wpf.SetValue(column, kept);
        }

        private static IEnumerable ItemsOf(DataTable items)
        {
            return items == null ? null : items.DefaultView;
        }

        // 펼치는 콤보의 전문을 다시 보내 목록만 바꾼다 — 같은 전문을 쓰는 다른 콤보도 함께 바뀌고 편집 값은 유지된다.
        // 여기서 전부 실었으므로 WPF 쪽에는 null 을 돌려준다(두 번 싣지 않는다).
        private IEnumerable RefreshDropDownItems(string column)
        {
            Func<string, object[], DataTable> refresh = this.ComboRefresh;
            FieldDefinition definition;

            if (refresh == null || string.IsNullOrEmpty(column) || !this.definitions.TryGetValue(column, out definition)
                    || string.IsNullOrEmpty(definition.RequestName))
            {
                return null;
            }

            DataTable items = refresh(definition.RequestName, definition.RequestFields);

            if (items == null)
            {
                return null;
            }

            string signature = definition.RequestSignature();
            Dictionary<string, DataTable> sources = new Dictionary<string, DataTable>(StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, FieldDefinition> entry in this.definitions)
            {
                if (entry.Value.Kind == PropertyEditorKind.Combo && !string.IsNullOrEmpty(entry.Value.RequestName)
                        && string.Equals(entry.Value.RequestSignature(), signature, StringComparison.Ordinal))
                {
                    sources[entry.Key] = items;
                }
            }

            this.ApplyComboSources(sources);
            return null;
        }

        // ===== 스키마 · 값 =====

        /// <summary>
        /// 표의 컬럼 순서대로 편집기를 구성한다. 컬럼 목록이 이전과 같으면 다시 만들지 않는다
        /// (조회 새로고침마다 깜빡이지 않도록).
        /// </summary>
        public void SetSchema(DataTable table)
        {
            string[] names = ColumnNames(table);

            if (SameNames(names, this.schemaColumns))
            {
                return;
            }

            this.schemaColumns = names;
            this.Rebuild();
        }

        /// <summary>기존 행을 편집기에 싣는다. 키 컬럼은 읽기 전용이 된다.</summary>
        public void LoadRow(DataRow row)
        {
            if (row == null)
            {
                this.BeginNew();
                return;
            }

            foreach (string column in this.schemaColumns)
            {
                this.WriteValue(column, row.Table.Columns.Contains(column)
                        ? Convert.ToString(row[column], CultureInfo.InvariantCulture)
                        : string.Empty);
            }

            this.isNew = false;
            this.ApplyKeyState();
            this.CaptureBaseline();
        }

        /// <summary>새 행 입력을 시작한다. 값을 비우고 <c>.Field(…).DefaultValue(…)</c>의 기본값을 채운 뒤 키 컬럼을 편집 가능하게 한다.</summary>
        public void BeginNew()
        {
            this.BeginNew(null);
        }

        /// <summary>
        /// 기본값을 채운 새 행 입력을 시작한다. <c>.Field(…).DefaultValue(…)</c>로 선언한 값을 먼저 채우고
        /// <paramref name="defaults"/>(폼이 조회 상태에서 정하는 값)를 그 위에 채운다 — 같은 컬럼이면 이쪽이 이긴다.
        /// 스키마에 없는 컬럼은 건너뛴다. 기본값이 되돌리기 기준이 되므로 신규 중 <see cref="RevertEdits"/>는 이 값으로 돌아간다.
        /// </summary>
        public void BeginNew(IDictionary<string, string> defaults)
        {
            this.ClearValues();
            this.WriteDefaults(this.defaultValues);
            this.WriteDefaults(defaults);
            this.isNew = true;
            this.ApplyKeyState();
            this.CaptureBaseline();
        }

        private void WriteDefaults(IDictionary<string, string> values)
        {
            if (values == null)
            {
                return;
            }

            foreach (KeyValuePair<string, string> value in values)
            {
                if (this.IsSchemaColumn(value.Key))
                {
                    this.WriteValue(value.Key, value.Value ?? string.Empty);
                }
            }
        }

        /// <summary>
        /// 마지막으로 불러온 상태(<see cref="LoadRow"/> · <see cref="BeginNew"/>) 이후 값이 하나라도 바뀌었나.
        /// 폼은 이것으로 되돌리기 버튼을 열고 닫는다 — 꺼진 버튼이 "잃을 것이 없다"는 표시가 되므로
        /// 확인 다이얼로그를 두지 않는다.
        /// </summary>
        [Browsable(false)]
        public bool IsDirty
        {
            get
            {
                foreach (string column in this.schemaColumns)
                {
                    if (!string.Equals(this.ReadValue(column), this.BaselineOf(column), StringComparison.Ordinal))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// 편집 값을 마지막으로 불러온 상태로 되돌린다. 신규 여부와 키 잠금은 그대로다 —
        /// 신규 입력을 버리고 목록 선택으로 돌아가는 것은 폼이 <see cref="LoadRow"/>로 한다.
        /// </summary>
        public void RevertEdits()
        {
            foreach (string column in this.schemaColumns)
            {
                this.WriteValue(column, this.BaselineOf(column));
            }
        }

        private string BaselineOf(string column)
        {
            string original;
            return this.baseline.TryGetValue(column, out original) ? original : string.Empty;
        }

        private void CaptureBaseline()
        {
            this.baseline = this.ReadValues();
        }

        /// <summary>모든 편집기 값을 비운다(스위치는 N, 콤보는 선택 없음 — 첫 항목 기본값은 두지 않는다).</summary>
        public void ClearValues()
        {
            foreach (string column in this.schemaColumns)
            {
                this.WriteValue(column, string.Empty);
            }
        }

        /// <summary>편집기 값을 컬럼 이름 → 문자열로 읽는다(스키마 순서 유지).</summary>
        public Dictionary<string, string> ReadValues()
        {
            Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string column in this.schemaColumns)
            {
                values[column] = this.ReadValue(column);
            }

            return values;
        }

        /// <summary>
        /// 편집 값 전부를 전문 파라미터 글자로 내놓는다 — <c>ProdId=PROD-0001 ProdNm=[300mm Bare Wafer] UseYn=Y …</c>.
        /// 이름은 <see cref="ParameterNameStyle"/>로 바뀌고(<c>PROD_ID</c> → <c>ProdId</c>), 공백이 든 값은
        /// <c>[…]</c>로 감싸져 있다. 순서는 스키마 순서다. <b>서버로 보낼 때는 이 글자를 쓰지 않는다</b> —
        /// 원문 문자열을 넘기는 <c>Request(string)</c>은 값을 감싸 주지 않으므로 <see cref="ToRequestFields"/>를
        /// 그대로 넘긴다. 이 속성은 사람이 읽는 진단·검사용이다.
        /// </summary>
        [Browsable(false)]
        public string RequestText
        {
            get { return ServerMessageFormat.BuildRequestText(string.Empty, this.ToRequestFields()).TrimStart(); }
        }

        /// <summary>
        /// 서버로 보내는 재료 — 이름/값이 번갈아 오는 배열. 폼은
        /// <c>RequestFields("ProductAction", fields)</c>로 넘기고, 값 감싸기는 그
        /// 조립 경로(<c>ServerMessageFormat.FormatValue</c>)가 맡는다. 원문 문자열을 직접 만들어
        /// <c>Request(string)</c>으로 보내면 감싸기가 적용되지 않는다.
        /// </summary>
        public object[] ToRequestFields()
        {
            List<object> pairs = new List<object>();

            foreach (string column in this.schemaColumns)
            {
                pairs.Add(this.ParameterNameOf(column));
                pairs.Add(this.ReadValue(column));
            }

            return pairs.ToArray();
        }

        /// <summary>컬럼 이름을 현재 <see cref="ParameterNameStyle"/>로 바꾼 전문 파라미터 이름.</summary>
        public string ParameterNameOf(string column)
        {
            return ParameterNameConverter.Convert(column, this.parameterNameStyle);
        }

        /// <summary>컬럼 하나의 편집 값. 없는 컬럼이면 빈 문자열.</summary>
        public string ReadValue(string column)
        {
            if (this.Wpf == null || !this.IsSchemaColumn(column))
            {
                return string.Empty;
            }

            return this.Wpf.GetValue(column);
        }

        /// <summary>키 컬럼 값들을 순서대로 이어 붙인 문자열(단일 키면 그 값 그대로).</summary>
        public string ReadKey()
        {
            List<string> parts = new List<string>();

            foreach (string column in this.schemaColumns)
            {
                if (this.keyColumns.Contains(column))
                {
                    parts.Add(this.ReadValue(column));
                }
            }

            return string.Join(",", parts.ToArray());
        }

        /// <summary>첫 편집 가능한 편집기에 포커스를 준다.</summary>
        public void FocusFirstEditor()
        {
            if (this.Wpf == null)
            {
                return;
            }

            foreach (string column in this.schemaColumns)
            {
                if (!this.ReadOnlyOf(column))
                {
                    this.Focus();
                    this.Wpf.FocusField(column);
                    return;
                }
            }
        }

        // ===== 구성 =====

        // 스키마 · 종류 · 라벨로 WPF 쪽 입력칸을 다시 놓는다. 값은 읽어 두었다가 다시 쓰고, 키 잠금 · 필수 표시 · 콤보 항목을 다시 건다.
        private void Rebuild()
        {
            if (this.Wpf == null)
            {
                return;
            }

            Dictionary<string, string> kept = this.ReadValues();
            bool keptNew = this.isNew;

            List<PropertyGridFieldSpec> specs = new List<PropertyGridFieldSpec>();

            foreach (string column in this.schemaColumns)
            {
                FieldDefinition definition;

                if (!this.definitions.TryGetValue(column, out definition))
                {
                    definition = FieldDefinition.Text;
                }

                LabelDefinition label = this.LabelOf(column);
                PropertyGridFieldSpec spec = new PropertyGridFieldSpec(column);
                spec.Kind = definition.Kind;
                spec.LabelText = label.Text;
                spec.TopLabel = label.TopLabel;
                spec.BottomLabel = label.BottomLabel;
                spec.IsKey = this.keyColumns.Contains(column);
                spec.DecimalPlaces = definition.DecimalPlaces;
                spec.ValueMember = definition.ValueMember;
                spec.DisplayMember = definition.DisplayMember;
                specs.Add(spec);
            }

            this.Wpf.SetFields(specs);

            foreach (string column in this.schemaColumns)
            {
                DataTable items;

                if (this.comboSources.TryGetValue(column, out items))
                {
                    this.Wpf.BindComboItems(column, ItemsOf(items));
                }

                string value;
                this.WriteValue(column, kept.TryGetValue(column, out value) ? value : string.Empty);
            }

            this.isNew = keptNew;
            this.ApplyKeyState();

            // 컬럼이 바뀌면 옛 기준값에 없는 컬럼이 생기고 없어진 컬럼이 남는다. 그대로 두면 값이
            // 그대로인데도 되돌리기가 열려 있는 것처럼 보인다. 그렇다고 통째로 다시 잡으면 **고치던
            // 값이 조용히 기준이 되어** 되돌릴 것이 사라진다(코덱스 리뷰 2026-09-13). 살아남은 컬럼의
            // 기준은 지키고, 새로 생긴 컬럼만 현재 값으로 채운다.
            Dictionary<string, string> rebuilt =
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string column in this.schemaColumns)
            {
                string original;
                rebuilt[column] = this.baseline.TryGetValue(column, out original)
                        ? original
                        : this.ReadValue(column);
            }

            this.baseline = rebuilt;
        }

        // 잠금과 필수 표시를 함께 다시 건다 — 필수 표시(라벨 별표 · 입력칸 표시)는 사용자가 채울 수 있는 칸에만 보인다. 잠긴 칸은
        // 필수라도 사용자가 할 일이 없고 필수 검사(MissingRequired)에서도 빠지므로 표시를 함께 뺀다(2026-10-12).
        private void ApplyKeyState()
        {
            if (this.Wpf == null)
            {
                return;
            }

            foreach (string column in this.schemaColumns)
            {
                this.Wpf.SetReadOnly(column, this.ReadOnlyOf(column));
            }

            this.ApplyRequiredMarks();
        }

        private void ApplyRequiredMarks()
        {
            if (this.Wpf == null)
            {
                return;
            }

            foreach (string column in this.schemaColumns)
            {
                this.Wpf.SetRequired(column, this.IsRequired(column) && !this.ReadOnlyOf(column));
            }
        }

        private void WriteValue(string column, string value)
        {
            if (this.Wpf != null)
            {
                this.Wpf.SetValue(column, value);
            }
        }

        private bool IsSchemaColumn(string column)
        {
            if (string.IsNullOrEmpty(column))
            {
                return false;
            }

            foreach (string name in this.schemaColumns)
            {
                if (string.Equals(name, column, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void OnWpfValueChanged(object sender, EventArgs e)
        {
            EventHandler handler = this.ValueChanged;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private static string[] ColumnNames(DataTable table)
        {
            if (table == null)
            {
                return new string[0];
            }

            string[] names = new string[table.Columns.Count];

            for (int i = 0; i < table.Columns.Count; i = i + 1)
            {
                names[i] = table.Columns[i].ColumnName;
            }

            return names;
        }

        private static bool SameNames(string[] left, string[] right)
        {
            if (left.Length != right.Length)
            {
                return false;
            }

            for (int i = 0; i < left.Length; i = i + 1)
            {
                if (!string.Equals(left[i], right[i], StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        private sealed class LabelDefinition
        {
            internal string Text;
            internal string TopLabel;
            internal string BottomLabel;
        }

        private sealed class FieldDefinition
        {
            internal static readonly FieldDefinition Text = new FieldDefinition(PropertyEditorKind.Text);

            internal FieldDefinition(PropertyEditorKind kind)
            {
                this.Kind = kind;
                this.RequestFields = new object[0];
                this.ValueMember = "CODE";
                this.DisplayMember = "NAME";
            }

            internal PropertyEditorKind Kind { get; private set; }

            internal string RequestName { get; set; }

            internal object[] RequestFields { get; set; }

            internal string ValueMember { get; set; }

            internal string DisplayMember { get; set; }

            internal int DecimalPlaces { get; set; }

            internal string RequestSignature()
            {
                List<string> parts = new List<string>();
                parts.Add(this.RequestName ?? string.Empty);

                foreach (object field in this.RequestFields)
                {
                    parts.Add(Convert.ToString(field, CultureInfo.InvariantCulture) ?? string.Empty);
                }

                return string.Join("|", parts.ToArray());
            }
        }
    }
}
