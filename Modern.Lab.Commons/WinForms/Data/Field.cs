using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Data;

namespace Modern.Lab.WinForms.Controls.Data
{
    /// <summary>
    /// 엔티티가 선언하는 필드 하나 — 서버 컬럼 이름과, 그 컬럼을 어떻게 읽고 보일지(캡션·형식·배지)를 한 곳에 둔다.
    /// 데이터 행이 아니라 정의다. <see cref="EntityDefinition"/>의 필드 초기화식에서 <c>Field("LOT_ID")</c>로 만들고
    /// <see cref="FieldExtensions"/>의 체인(<c>.Caption().Format().Badge()</c>)으로 꾸민 뒤 엔티티가 <c>Seal()</c>로 잠근다.
    /// 잠긴 뒤의 설정 호출은 예외다 — 공유 정의를 화면이 바꾸지 못하게 하는 장치다.
    ///
    /// 상태값이 있는 필드는 이 클래스를 상속해 <see cref="FieldValue"/> 멤버를 <see cref="Value"/>로 선언한다.
    /// 잠글 때 그 멤버들이 <see cref="Values"/>로 모인다 — 비교용 멤버와 값 목록이 한 선언에서 나온다.
    ///
    /// 읽기 속성 이름이 체인 메서드와 다른 이유 — 같은 이름의 인스턴스 멤버가 있으면 C#은 확장 메서드보다 그것을 먼저
    /// 골라 <c>field.Caption("…")</c>가 컴파일되지 않는다.
    /// </summary>
    public class Field
    {
        private static readonly FieldValue[] noValues = new FieldValue[0];

        private string name = string.Empty;
        private string caption;
        private string topLabel;
        private string bottomLabel;
        private string format;
        private GridTextAlignment? textAlignment;
        private GridTextAlignment? headerAlignment;
        private GridTextAlignment? topLabelAlignment;
        private GridTextAlignment? bottomLabelAlignment;
        private bool badge;
        private string spinValues = string.Empty;
        private bool isSealed;
        private EntityDefinition owner;
        private IReadOnlyList<FieldValue> values = noValues;

        /// <summary>파생 타입의 <c>new()</c> 제약을 위해 공개한다 — 이름은 <see cref="EntityDefinition"/>의 팩터리가 준다.</summary>
        public Field()
        {
        }

        /// <summary>서버 컬럼 이름 — 전문·<see cref="DataTable"/> 컬럼 그대로.</summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>표시 캡션 — null이면 캡션 사전과 자동 변환("LOT_ID" → "Lot Id")을 쓴다.</summary>
        public string CaptionSetting
        {
            get { return this.caption; }
        }

        /// <summary>그리드 헤더 본 캡션 위의 작은 글씨 — null이면 캡션 사전을 쓴다.</summary>
        public string TopLabelSetting
        {
            get { return this.topLabel; }
        }

        /// <summary>그리드 헤더 본 캡션 아래의 작은 글씨 — null이면 캡션 사전을 쓴다.</summary>
        public string BottomLabelSetting
        {
            get { return this.bottomLabel; }
        }

        /// <summary>표시 형식 — null이면 형식 없음. 값 검증이 아니라 표시 규칙이다.</summary>
        public string FormatSetting
        {
            get { return this.format; }
        }

        /// <summary>그리드 셀 정렬 — null이면 캡션 사전과 이름 관례를 쓴다.</summary>
        public GridTextAlignment? TextAlignmentSetting
        {
            get { return this.textAlignment; }
        }

        /// <summary>그리드 헤더 정렬 — null이면 캡션 사전을 쓴다.</summary>
        public GridTextAlignment? HeaderAlignmentSetting
        {
            get { return this.headerAlignment; }
        }

        /// <summary>그리드 헤더 위 작은 글씨 줄의 정렬 — null이면 헤더 정렬을 따른다.</summary>
        public GridTextAlignment? TopLabelAlignmentSetting
        {
            get { return this.topLabelAlignment; }
        }

        /// <summary>그리드 헤더 아래 작은 글씨 줄의 정렬 — null이면 헤더 정렬을 따른다.</summary>
        public GridTextAlignment? BottomLabelAlignmentSetting
        {
            get { return this.bottomLabelAlignment; }
        }

        /// <summary>배지로 보일지.</summary>
        public bool BadgeSetting
        {
            get { return this.badge; }
        }

        /// <summary>배지 테두리가 도는 값 목록(콤마 구분). 비면 안 돈다 — 표·카드가 기본으로 깔고 화면이 덮어쓴다(2026-10-26).</summary>
        public string SpinValuesSetting
        {
            get { return this.spinValues; }
        }

        /// <summary>엔티티가 잠근 뒤 true — 설정 메서드가 예외를 던진다.</summary>
        public bool IsSealed
        {
            get { return this.isSealed; }
        }

        /// <summary>이 필드를 선언한 엔티티 — 잠그기 전에는 null.</summary>
        public EntityDefinition Owner
        {
            get { return this.owner; }
        }

        /// <summary>선언된 상태값 목록 — 파생 타입의 public <see cref="FieldValue"/> 필드. 잠그기 전에는 비어 있다.</summary>
        public IReadOnlyList<FieldValue> Values
        {
            get { return this.values; }
        }

        /// <summary>행에서 이 필드의 값을 문자열로 읽는다 — 컬럼이 없거나 null이면 빈 문자열, 앞뒤 공백 제거.</summary>
        public string Text(DataRow row)
        {
            return TableHelper.CellText(row, this.name).Trim();
        }

        /// <summary>행의 값이 주어진 상태값인지 — 대소문자를 구분한다. 표시·판정 어느 쪽도 값을 바꾸지 않는다.</summary>
        public bool Is(DataRow row, FieldValue value)
        {
            return value != null && string.Equals(this.Text(row), value.Value, StringComparison.Ordinal);
        }

        /// <summary>상태값 코드만 뽑은 배열 — 계약의 상태 어휘로 넘긴다.</summary>
        public string[] ValueCodes()
        {
            string[] codes = new string[this.values.Count];

            for (int index = 0; index < codes.Length; index++)
            {
                codes[index] = this.values[index].Value;
            }

            return codes;
        }

        public override string ToString()
        {
            return this.name;
        }

        /// <summary>파생 타입의 필드 초기화식에서 상태값을 만든다 — 예: <c>public readonly FieldValue Free = Value("Free");</c></summary>
        protected static FieldValue Value(string value)
        {
            return new FieldValue(value, null);
        }

        /// <summary>서버 값과 표시 문구가 다른 상태값 — 예: <c>public readonly FieldValue Free = Value("F", "Free");</c></summary>
        protected static FieldValue Value(string value, string caption)
        {
            return new FieldValue(value, caption);
        }

        internal void SetName(string value)
        {
            this.EnsureUnsealed();
            this.name = value == null ? string.Empty : value.Trim();
        }

        internal void SetCaption(string value)
        {
            this.EnsureUnsealed();
            this.caption = value;
        }

        internal void SetTopLabel(string value)
        {
            this.EnsureUnsealed();
            this.topLabel = value;
        }

        internal void SetBottomLabel(string value)
        {
            this.EnsureUnsealed();
            this.bottomLabel = value;
        }

        internal void SetTextAlignment(GridTextAlignment value)
        {
            this.EnsureUnsealed();
            this.textAlignment = value;
        }

        internal void SetHeaderAlignment(GridTextAlignment value)
        {
            this.EnsureUnsealed();
            this.headerAlignment = value;
        }

        internal void SetTopLabelAlignment(GridTextAlignment value)
        {
            this.EnsureUnsealed();
            this.topLabelAlignment = value;
        }

        internal void SetBottomLabelAlignment(GridTextAlignment value)
        {
            this.EnsureUnsealed();
            this.bottomLabelAlignment = value;
        }

        internal void SetFormat(string value)
        {
            this.EnsureUnsealed();
            this.format = value;
        }

        internal void SetBadge(bool value)
        {
            this.EnsureUnsealed();
            this.badge = value;
        }

        internal void SetSpin(string value)
        {
            this.EnsureUnsealed();
            this.spinValues = value == null ? string.Empty : value.Trim();
        }

        internal void Seal(EntityDefinition entity)
        {
            if (this.isSealed)
            {
                throw new InvalidOperationException(
                        "필드 '" + this.name + "'은 이미 잠겼다 — 한 필드를 두 엔티티가 공유할 수 없다.");
            }

            if (string.IsNullOrEmpty(this.name))
            {
                throw new InvalidOperationException(
                        entity.GetType().Name + ": 이름이 없는 필드가 있다 — Field(\"컬럼 이름\")으로 만들어야 한다.");
            }

            this.owner = entity;
            this.values = this.CollectValues();

            foreach (FieldValue value in this.values)
            {
                value.Seal();
            }

            this.isSealed = true;
        }

        private IReadOnlyList<FieldValue> CollectValues()
        {
            List<FieldValue> found = new List<FieldValue>();
            HashSet<string> codes = new HashSet<string>(StringComparer.Ordinal);

            foreach (FieldInfo info in this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (info.FieldType != typeof(FieldValue))
                {
                    continue;
                }

                FieldValue value = (FieldValue)info.GetValue(this);

                if (value == null)
                {
                    throw new InvalidOperationException(
                            this.GetType().Name + "." + info.Name + " 이 null이다 — Value(\"코드\")로 만들어야 한다.");
                }

                if (!codes.Add(value.Value))
                {
                    throw new InvalidOperationException(
                            this.GetType().Name + ": 상태값 '" + value.Value + "'이 두 번 선언됐다.");
                }

                found.Add(value);
            }

            return found.Count == 0 ? noValues : found.ToArray();
        }

        private void EnsureUnsealed()
        {
            if (this.isSealed)
            {
                throw new InvalidOperationException(
                        "필드 '" + this.name + "'은 잠겼다 — 공유 엔티티 정의는 화면에서 바꾸지 않는다.");
            }
        }
    }
}
