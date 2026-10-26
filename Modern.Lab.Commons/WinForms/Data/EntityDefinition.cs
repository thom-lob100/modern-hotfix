using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Modern.Lab.WinForms.Controls.Data
{
    /// <summary>
    /// 응답 표 하나의 필드를 선언하는 공유 정의 — 키·표시 목록·필드 꾸밈을 한 곳에 둔다. ORM이 아니다.
    /// 데이터는 지금처럼 전문으로 받은 <see cref="DataTable"/>이고, 이 정의는 그 표를 어떻게 읽고 보일지만 안다.
    ///
    /// 선언 형태:
    /// <code>
    /// public sealed class LotEntity : EntityDefinition
    /// {
    ///     public readonly Field LotId = Field("LOT_ID");
    ///     public readonly Field Qty = Field("QTY").Format("#,##0");
    ///
    ///     public LotEntity()
    ///     {
    ///         this.KeyFields = Fields(this.LotId);
    ///         this.Seal();
    ///     }
    /// }
    /// </code>
    /// 필드 초기화식은 인스턴스 메서드를 부를 수 없으므로 <see cref="Field(string)"/>는 정적 팩터리다. 소유권과
    /// 등록은 <see cref="Seal"/>이 리플렉션으로 <see cref="Field"/> 타입 필드를 모아 처리한다 — 키·표시 목록에 넣지 않은
    /// 필드도 잠긴다. 리플렉션 순서는 선언 순서를 보장하지 않으므로 표시 순서는 <see cref="DisplayFields"/>로만 정한다.
    ///
    /// 한 번 만들어 공유한다(<c>public static LotEntity Lot { get; } = new LotEntity();</c>). 화면별 숨김·꾸밈 차이는
    /// <see cref="Columns"/>가 돌려주는 <see cref="GridColumns"/>에 이어 적고, 공유 정의는 바꾸지 않는다.
    ///
    /// 목록 속성의 뜻:
    /// <list type="bullet">
    /// <item><see cref="KeyFields"/> — 행 키. 편집기에서는 기본 필수이며 신규에만 편집된다.</item>
    /// <item><see cref="DisplayFields"/> · <see cref="ExcludeFields"/> — 그리드 표시 목록과 표시 제외. 표시만의 설정이다.</item>
    /// <item><see cref="RequiredFields"/> · <see cref="CreateOnlyFields"/> — 편집기의 키 외 필수 입력·신규 전용 편집. 서버 검증을 대신하지 않는다.</item>
    /// <item><see cref="ReadOnlyFields"/> — 편집기에서 항상 잠기는 필드(서버 채번 키·서버 기록 컬럼). 필수 검사에서 빠진다.</item>
    /// <item><see cref="ComboDisplayFields"/> · <see cref="ValueField"/> — 콤보의 표시 필드(순서 유지)와 값 필드. 첫 표시 필드가 <c>DisplayMember</c>다.</item>
    /// <item><see cref="ParentField"/> — 계층 엔티티의 부모 연결 컬럼. 트리만 읽는다.</item>
    /// </list>
    /// </summary>
    public abstract class EntityDefinition
    {
        private static readonly Field[] noFields = new Field[0];

        private IReadOnlyList<Field> keyFields = noFields;
        private IReadOnlyList<Field> displayFields = noFields;
        private IReadOnlyList<Field> excludeFields = noFields;
        private IReadOnlyList<Field> requiredFields = noFields;
        private IReadOnlyList<Field> createOnlyFields = noFields;
        private IReadOnlyList<Field> readOnlyFields = noFields;
        private IReadOnlyList<Field> comboDisplayFields = noFields;
        private IReadOnlyList<Field> allFields = noFields;
        private Field valueField;
        private Field displayField;
        private Field parentField;
        private bool isSealed;

        /// <summary>행 키 — 선택 복원·New 표시가 쓰는 유일 키. 복합 키는 여러 필드. 편집기에서는 기본 필수이고 신규에만 편집된다.</summary>
        public IReadOnlyList<Field> KeyFields
        {
            get { return this.keyFields; }
            protected set
            {
                this.EnsureUnsealed();
                this.keyFields = value ?? noFields;
            }
        }

        /// <summary>표시 목록과 순서 — 비워 두면 응답이 준 컬럼을 전부 보인다(현행 원칙). 채우면 이 목록만 이 순서로.</summary>
        public IReadOnlyList<Field> DisplayFields
        {
            get { return this.displayFields; }
            protected set
            {
                this.EnsureUnsealed();
                this.displayFields = value ?? noFields;
            }
        }

        /// <summary>
        /// 표시 제외 목록 — 화면의 <c>.Hide()</c> 대신 엔티티가 공통으로 숨기는 필드. <see cref="DisplayFields"/>를 비우면
        /// 응답 컬럼 전체에서, 채우면 그 목록에서 뺀다(남은 순서 유지). 그리드 표시만의 설정이며 데이터 컬럼·키·필수·콤보 정의는 바꾸지 않는다.
        /// </summary>
        public IReadOnlyList<Field> ExcludeFields
        {
            get { return this.excludeFields; }
            protected set
            {
                this.EnsureUnsealed();
                this.excludeFields = value ?? noFields;
            }
        }

        /// <summary>
        /// 키 외의 추가 필수 입력 — 편집기의 필수 표시·저장 전 누락 안내에 쓴다. 실제 필수 목록은 키와 이 목록의 합집합이라
        /// 키를 여기 다시 적지 않는다. 입력 편의 검사이며 서버 검증과 조회 응답의 필수 컬럼 검사를 대신하지 않는다.
        /// </summary>
        public IReadOnlyList<Field> RequiredFields
        {
            get { return this.requiredFields; }
            protected set
            {
                this.EnsureUnsealed();
                this.requiredFields = value ?? noFields;
            }
        }

        /// <summary>
        /// 키 외의 신규 전용 편집 항목 — 신규 등록에서만 입력할 수 있고 기존 항목 수정에서는 읽기 전용이다. 필수 여부는
        /// 키·<see cref="RequiredFields"/>가 정한다. 기존 항목에서 잠기는 항목은 키와 이 목록의 합집합이다.
        /// </summary>
        public IReadOnlyList<Field> CreateOnlyFields
        {
            get { return this.createOnlyFields; }
            protected set
            {
                this.EnsureUnsealed();
                this.createOnlyFields = value ?? noFields;
            }
        }

        /// <summary>
        /// 편집기에서 항상 읽기 전용인 필드 — 서버가 값을 만드는 키(채번 <c>PROD_ID</c>)나 서버가 기록하는 컬럼.
        /// 신규 입력에서도 잠기고 필수 검사에서 빠지므로, 서버가 키를 만드는 화면은 Grid 설정 없이도 New·Save가 성립한다.
        /// 편집기만의 설정이며 그리드 표시·데이터·키 정의는 바꾸지 않는다. 화면마다 다른 꾸밈은 <c>.Grid.cs</c>에 둔다.
        /// </summary>
        public IReadOnlyList<Field> ReadOnlyFields
        {
            get { return this.readOnlyFields; }
            protected set
            {
                this.EnsureUnsealed();
                this.readOnlyFields = value ?? noFields;
            }
        }

        /// <summary>
        /// 콤보의 표시 필드 — 순서를 유지한다. 첫 필드가 닫힌 콤보의 <c>DisplayMember</c>이고, 둘 이상이면 펼친 목록의 컬럼이
        /// 이 순서로 구성된다. 하나의 <c>DisplayMember</c>에 콤마 문자열을 넣지 않는다. 이력처럼 콤보에 쓰지 않는 엔티티는 비워 둔다.
        /// </summary>
        public IReadOnlyList<Field> ComboDisplayFields
        {
            get { return this.comboDisplayFields; }
            protected set
            {
                this.EnsureUnsealed();
                this.comboDisplayFields = value ?? noFields;
            }
        }

        /// <summary>
        /// 콤보가 닫힌 채 보이는 필드(<c>DisplayMember</c>). 지정하지 않으면 <see cref="ComboDisplayFields"/>의 첫 필드다.
        /// 펼친 목록의 첫 열과 닫힌 글자를 다르게 둘 때만 지정한다(Durable 순위처럼 목록은 Prio 가 먼저, 닫히면 Id). 없으면 null.
        /// </summary>
        public Field DisplayField
        {
            get { return this.displayField ?? (this.comboDisplayFields.Count > 0 ? this.comboDisplayFields[0] : null); }
            protected set
            {
                this.EnsureUnsealed();
                this.displayField = value;
            }
        }

        /// <summary>콤보의 선택 값 필드(<c>ValueMember</c>) — 없으면 null.</summary>
        public Field ValueField
        {
            get { return this.valueField; }
            protected set
            {
                this.EnsureUnsealed();
                this.valueField = value;
            }
        }

        /// <summary>
        /// 계층 관계의 부모 연결 필드 — 트리가 키와 함께 읽어 <c>IdMember</c>·<c>ParentIdMember</c>를 정한다.
        /// 일반 그리드는 읽지 않는다. 부모 관계가 없는 엔티티는 null이다. 부모 필드는 하나다.
        /// </summary>
        public Field ParentField
        {
            get { return this.parentField; }
            protected set
            {
                this.EnsureUnsealed();
                this.parentField = value;
            }
        }

        /// <summary>선언한 모든 필드 — <see cref="Seal"/> 뒤에 채워진다.</summary>
        public IReadOnlyList<Field> AllFields
        {
            get { return this.allFields; }
        }

        /// <summary>잠겼는지 — 잠긴 뒤에는 필드·목록을 바꿀 수 없다.</summary>
        public bool IsSealed
        {
            get { return this.isSealed; }
        }

        /// <summary>그리드 <c>RowKeyMember</c> 형식의 키 — 복합 키는 쉼표로 잇는다.</summary>
        public string RowKey
        {
            get { return string.Join(",", Names(this.keyFields)); }
        }

        /// <summary>편집기의 실제 필수 입력 — 키와 <see cref="RequiredFields"/>의 합집합(키 먼저, 중복 없이).</summary>
        public IReadOnlyList<Field> EditorRequiredFields
        {
            get { return Union(this.keyFields, this.requiredFields); }
        }

        /// <summary>기존 항목 수정에서 잠기는 항목 — 키와 <see cref="CreateOnlyFields"/>의 합집합(키 먼저, 중복 없이).</summary>
        public IReadOnlyList<Field> EditorCreateOnlyFields
        {
            get { return Union(this.keyFields, this.createOnlyFields); }
        }

        /// <summary>
        /// 조회 전에 헤더로 보일 필드 — <see cref="DisplayFields"/>가 있으면 그것, 없으면 선언한 필드 전부(<see cref="ExcludeFields"/>는
        /// <see cref="Columns"/>·<see cref="Fields(DataTable)"/>가 뒤에서 뺀다). 선언 순서는 필드 선언 순서를 따른다.
        /// </summary>
        public IReadOnlyList<Field> HeaderFields
        {
            get { return this.displayFields.Count > 0 ? this.displayFields : this.allFields; }
        }

        /// <summary>
        /// 응답 표에서 그리드 컬럼 정의를 만든다 — 표시 목록·표시 제외·캡션·형식·배지를 이 정의에서 적용한 <see cref="GridColumns"/>.
        /// 화면은 여기에 자기만의 꾸밈(<c>Hide</c>·<c>Caption</c>·화면이 만든 컬럼의 배지)을 이어 적고 <c>Apply</c>한다.
        /// 표시 목록의 필드가 응답에 하나도 없으면 <see cref="GridColumns.Only"/>의 규칙대로 전부 보인다.
        /// 표가 null(조회 전)이면 <see cref="HeaderFields"/>로 헤더만 만든다 — 그리드가 조회 전에도 컬럼을 보이게(2026-10-19).
        /// </summary>
        public GridColumns Columns(DataTable table)
        {
            this.EnsureSealed();

            // null 표는 조회 전이다 — 표시 목록(없으면 선언한 필드 전부)으로 헤더만 만든다(2026-10-19).
            // 정상 0건(행이 없는 표)은 컬럼이 없거나 일부만 보충돼 있어도 DisplayFields 전체·순서로 헤더를 만든다 —
            // 서버가 0건일 때 컬럼을 보내지 않는 제약의 보완(2026-09-29).
            // 데이터가 있는 표에서는 지금처럼 응답에 있는 컬럼만 고른다(빠진 필드를 새로 만들지 않는다).
            bool beforeLoad = table == null;
            bool emptyResult = table != null && table.Rows.Count == 0;
            GridColumns columns;

            if (beforeLoad)
            {
                columns = GridColumns.Declared(null, Names(this.HeaderFields));
            }
            else if (this.displayFields.Count > 0 && emptyResult)
            {
                columns = GridColumns.Declared(table, Names(this.displayFields));
            }
            else
            {
                columns = GridColumns.Of(table);
            }

            if (this.displayFields.Count > 0 && !emptyResult && !beforeLoad)
            {
                columns.Only(Names(this.displayFields));
            }

            if (this.excludeFields.Count > 0)
            {
                columns.Hide(Names(this.excludeFields));
            }

            foreach (Field field in this.allFields)
            {
                if (field.CaptionSetting != null)
                {
                    columns.Column(field.Name).Caption(field.CaptionSetting);
                }

                if (field.TopLabelSetting != null)
                {
                    columns.Column(field.Name).TopLabel(field.TopLabelSetting);
                }

                if (field.BottomLabelSetting != null)
                {
                    columns.Column(field.Name).BottomLabel(field.BottomLabelSetting);
                }

                if (field.FormatSetting != null)
                {
                    columns.Column(field.Name).Format(field.FormatSetting);
                }

                if (field.TextAlignmentSetting.HasValue)
                {
                    columns.Column(field.Name).TextAlignment(field.TextAlignmentSetting.Value);
                }

                if (field.HeaderAlignmentSetting.HasValue)
                {
                    columns.Column(field.Name).HeaderAlignment(field.HeaderAlignmentSetting.Value);
                }

                if (field.TopLabelAlignmentSetting.HasValue)
                {
                    columns.Column(field.Name).TopLabelAlignment(field.TopLabelAlignmentSetting.Value);
                }

                if (field.BottomLabelAlignmentSetting.HasValue)
                {
                    columns.Column(field.Name).BottomLabelAlignment(field.BottomLabelAlignmentSetting.Value);
                }

                if (field.BadgeSetting)
                {
                    columns.Column(field.Name).Badge();
                }

                if (field.SpinValuesSetting.Length > 0)
                {
                    columns.Column(field.Name).Spin(field.SpinValuesSetting);
                }
            }

            return columns;
        }

        /// <summary>
        /// 응답 표에서 정보 카드(<c>ModernFieldList</c>) 필드 정의를 만든다 — <see cref="Columns"/>의 정보 카드 판이다.
        /// 표시 목록·표시 제외·캡션(위·본·아래를 한 줄로 이음)·형식·배지를 이 정의에서 적용한다.
        /// 화면은 여기에 자기만의 꾸밈(<c>Only</c>·<c>Span</c>·<c>Link</c>)을 이어 적고 <c>Apply</c>한다.
        /// 표가 null(조회 전)이면 <see cref="HeaderFields"/>로 캡션만 있는 카드를 만든다(2026-10-19).
        /// </summary>
        public Modern.Lab.WinForms.Controls.Display.FieldDefinitions Fields(DataTable table)
        {
            this.EnsureSealed();

            bool beforeLoad = table == null;
            Modern.Lab.WinForms.Controls.Display.FieldDefinitions fields = beforeLoad
                    ? Modern.Lab.WinForms.Controls.Display.FieldDefinitions.Declared(Names(this.HeaderFields))
                    : Modern.Lab.WinForms.Controls.Display.FieldDefinitions.Of(table);

            if (this.displayFields.Count > 0 && !beforeLoad)
            {
                fields.Only(Names(this.displayFields));
            }

            if (this.excludeFields.Count > 0)
            {
                fields.Hide(Names(this.excludeFields));
            }

            foreach (Field field in this.allFields)
            {
                if (field.CaptionSetting != null)
                {
                    fields.Field(field.Name).Caption(Modern.Lab.Controls.Wpf.Data.GridCaptionCatalog.PlainCaption(
                            field.TopLabelSetting, field.CaptionSetting, field.BottomLabelSetting));
                }

                if (field.FormatSetting != null)
                {
                    fields.Field(field.Name).Format(field.FormatSetting);
                }

                if (field.BadgeSetting)
                {
                    fields.Field(field.Name).Badge();
                }

                if (field.SpinValuesSetting.Length > 0)
                {
                    fields.Field(field.Name).BadgeSpin(field.SpinValuesSetting);
                }
            }

            return fields;
        }

        /// <summary>행 하나만 들고 있을 때 — 그 행이 속한 표로 <see cref="Fields(DataTable)"/>를 만든다.</summary>
        public Modern.Lab.WinForms.Controls.Display.FieldDefinitions Fields(DataRow row)
        {
            return this.Fields(row == null ? null : row.Table);
        }

        /// <summary>조회 전 카드 — <see cref="HeaderFields"/>로 캡션만 있는 정의(<c>Fields(null)</c>은 표·행 오버로드가 겹쳐 이 꼴로 부른다).</summary>
        public Modern.Lab.WinForms.Controls.Display.FieldDefinitions Fields()
        {
            return this.Fields((DataTable)null);
        }

        /// <summary>조회 전 그리드 — <see cref="HeaderFields"/>로 헤더만 있는 정의. <c>Columns(null)</c>과 같다.</summary>
        public GridColumns Columns()
        {
            return this.Columns(null);
        }

        /// <summary>필드들의 컬럼 이름 배열 — 문자열을 받는 기존 API에 넘길 때.</summary>
        public static string[] Names(IReadOnlyList<Field> fields)
        {
            if (fields == null)
            {
                return new string[0];
            }

            string[] names = new string[fields.Count];

            for (int index = 0; index < names.Length; index++)
            {
                names[index] = fields[index] == null ? string.Empty : fields[index].Name;
            }

            return names;
        }

        /// <summary>필드 초기화식에서 필드를 만든다 — 서버 컬럼 이름을 여기 한 번만 적는다.</summary>
        protected static Field Field(string name)
        {
            Field field = new Field();
            field.SetName(name);
            return field;
        }

        /// <summary>상태값 멤버를 가진 파생 필드 타입을 만든다 — <c>Field&lt;WfLoadStatField&gt;("WF_LOAD_STAT_CD")</c>.</summary>
        protected static T Field<T>(string name) where T : Field, new()
        {
            T field = new T();
            field.SetName(name);
            return field;
        }

        /// <summary>키·표시·필수·신규 전용·콤보 표시 목록을 필드 객체로 적는다 — 문자열이 아니다.</summary>
        protected static Field[] Fields(params Field[] fields)
        {
            return fields ?? noFields;
        }

        /// <summary>
        /// 선언과 설정을 마친 뒤 생성자 끝에서 부른다 — 모든 <see cref="Field"/> 필드를 모아 소유권을 주고 잠근다.
        /// 이름이 비거나 겹치는 필드, 목록·단일 속성에 있으나 선언되지 않은 필드는 예외다.
        /// </summary>
        protected void Seal()
        {
            if (this.isSealed)
            {
                throw new InvalidOperationException(this.GetType().Name + ": 이미 잠겼다.");
            }

            List<Field> found = new List<Field>();
            HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            foreach (FieldInfo info in this.GetType().GetFields(flags))
            {
                if (!typeof(Field).IsAssignableFrom(info.FieldType))
                {
                    continue;
                }

                Field field = (Field)info.GetValue(this);

                if (field == null)
                {
                    throw new InvalidOperationException(
                            this.GetType().Name + "." + info.Name + " 이 null이다 — Field(\"컬럼 이름\")으로 만들어야 한다.");
                }

                if (!names.Add(field.Name))
                {
                    throw new InvalidOperationException(
                            this.GetType().Name + ": 컬럼 '" + field.Name + "'이 두 필드에 선언됐다.");
                }

                found.Add(field);
            }

            this.EnsureDeclared(this.keyFields, found, "KeyFields");
            this.EnsureDeclared(this.displayFields, found, "DisplayFields");
            this.EnsureDeclared(this.excludeFields, found, "ExcludeFields");
            this.EnsureDeclared(this.requiredFields, found, "RequiredFields");
            this.EnsureDeclared(this.createOnlyFields, found, "CreateOnlyFields");
            this.EnsureDeclared(this.readOnlyFields, found, "ReadOnlyFields");
            this.EnsureDeclared(this.comboDisplayFields, found, "ComboDisplayFields");
            this.EnsureDeclared(this.valueField, found, "ValueField");
            this.EnsureDeclared(this.parentField, found, "ParentField");

            foreach (Field field in found)
            {
                field.Seal(this);
            }

            this.allFields = found.ToArray();
            this.isSealed = true;
        }

        private static IReadOnlyList<Field> Union(IReadOnlyList<Field> first, IReadOnlyList<Field> second)
        {
            List<Field> union = new List<Field>(first);

            foreach (Field field in second)
            {
                if (field != null && !union.Contains(field))
                {
                    union.Add(field);
                }
            }

            return union.ToArray();
        }

        private void EnsureDeclared(IReadOnlyList<Field> listed, List<Field> declared, string listName)
        {
            foreach (Field field in listed)
            {
                if (field == null || !declared.Contains(field))
                {
                    throw new InvalidOperationException(
                            this.GetType().Name + "." + listName + ": 이 엔티티가 선언하지 않은 필드가 들어 있다"
                            + (field == null ? "." : " — '" + field.Name + "'."));
                }
            }
        }

        private void EnsureDeclared(Field single, List<Field> declared, string memberName)
        {
            if (single != null && !declared.Contains(single))
            {
                throw new InvalidOperationException(
                        this.GetType().Name + "." + memberName + ": 이 엔티티가 선언하지 않은 필드다 — '" + single.Name + "'.");
            }
        }

        private void EnsureSealed()
        {
            if (!this.isSealed)
            {
                throw new InvalidOperationException(this.GetType().Name + ": 생성자 끝에서 Seal()을 불러야 쓸 수 있다.");
            }
        }

        private void EnsureUnsealed()
        {
            if (this.isSealed)
            {
                throw new InvalidOperationException(this.GetType().Name + ": 잠긴 뒤에는 키·표시 목록을 바꿀 수 없다.");
            }
        }
    }
}
