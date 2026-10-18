using System;
using System.Collections.Generic;
using System.Data;
using Modern.Lab.Controls.Wpf.Data;

namespace Modern.Lab.WinForms.Controls.Display
{
    /// <summary>
    /// 조회 결과의 컬럼을 그대로 정보 카드(<see cref="ModernFieldList"/>)에 싣는다 —
    /// 필드 목록을 손으로 적지 않는다. 캡션은 그리드 헤더와 **같은 용어사전**에서 나온다
    /// ("DURABLE_ID" → "Durable Id").
    ///
    /// ★ 필드를 늘리거나 줄이거나 순서를 바꾸는 일은 **쿼리**에서 한다.
    ///
    /// <code>
    /// FieldDefinitions.Of(durables)
    ///         .Hide("FOREIGN_YN")
    ///         .Field("DESCRIPTION").Span(5)
    ///         .Apply(this.fieldInfo);
    /// </code>
    ///
    /// 없는 컬럼 이름은 조용히 무시한다 — 쿼리가 바뀌어도 화면이 깨지지 않는다.
    /// <see cref="GridColumns"/>와 같은 어휘를 쓴다 — 목록 단위는 Of · Only · Hide · First, 필드 하나의 설정은 <see cref="Field"/>로
    /// 그 필드를 고른 뒤 점으로 잇는다(Caption · Span · StartAt · Link · Badge · Format, 2026-10-04). 캡션은 한 줄이고
    /// 설정은 서로 독립이다 — Link 는 링크 여부만, Span 은 열 수만 바꾸며 호출 순서가 캡션을 바꾸지 않는다.
    /// </summary>
    public sealed class FieldDefinitions
    {
        private readonly List<ModernFieldDefinition> fields;

        private FieldDefinitions(DataTable table)
        {
            // 표시 대상 컬럼을 고르는 규칙은 Commons의 AutoColumns 한 곳에 있다 —
            // 카드가 스스로 정의할 때(ModernFieldList.SetRow)와 같은 결과여야 한다.
            string[] members = AutoColumns.MembersOf(table);
            this.fields = new List<ModernFieldDefinition>();

            foreach (string member in members)
            {
                this.fields.Add(new ModernFieldDefinition(member));
            }
        }

        public static FieldDefinitions Of(DataTable table)
        {
            return new FieldDefinitions(table);
        }

        /// <summary>
        /// 표 없이 이름만으로 필드를 선언한다 — 엔티티가 조회 전 카드의 캡션을 만들 때(<c>entity.Fields(null)</c>, 2026-10-19).
        /// 값은 비어 있고 <see cref="ModernFieldList.SetRow"/>가 채운다.
        /// </summary>
        internal static FieldDefinitions Declared(string[] members)
        {
            FieldDefinitions declared = new FieldDefinitions((DataTable)null);

            foreach (string member in Names(members))
            {
                if (!string.IsNullOrEmpty(member))
                {
                    declared.fields.Add(new ModernFieldDefinition(member));
                }
            }

            return declared;
        }

        /// <summary>행 하나만 들고 있을 때 — 그 행이 속한 표의 컬럼을 쓴다.</summary>
        public static FieldDefinitions Of(DataRow row)
        {
            return new FieldDefinitions(row == null ? null : row.Table);
        }

        /// <summary>적은 필드만 적은 순서대로 남긴다.</summary>
        public FieldDefinitions Only(params string[] members)
        {
            List<ModernFieldDefinition> kept = new List<ModernFieldDefinition>();

            foreach (string member in Names(members))
            {
                int index = this.IndexOf(member);

                if (index >= 0)
                {
                    kept.Add(this.fields[index]);
                }
            }

            if (kept.Count > 0)
            {
                this.fields.Clear();
                this.fields.AddRange(kept);
            }

            return this;
        }

        public FieldDefinitions Hide(params string[] members)
        {
            foreach (string member in Names(members))
            {
                int index = this.IndexOf(member);

                if (index >= 0)
                {
                    this.fields.RemoveAt(index);
                }
            }

            return this;
        }

        /// <summary>
        /// 필드를 고른다 — 설정은 돌려받은 <see cref="FieldSelection"/>에 점으로 잇는다(그리드 <c>GridColumns.Column(…)</c>·캡션 사전·엔티티와
        /// 같은 문법, 2026-10-04 통일). <c>.Field("DESCRIPTION").Span(3).StartAt(3)</c>. 이름을 여러 개 주면 같은 설정이 모두에 걸리고
        /// (<c>.Field(a, b).Badge()</c>), 표에 없는 이름은 조용히 무시한다.
        /// </summary>
        public FieldSelection Field(params string[] members)
        {
            return new FieldSelection(this, Names(members));
        }

        internal void SetCaption(string member, string caption)
        {
            ModernFieldDefinition field = this.Find(member);

            if (field != null)
            {
                field.Caption(caption);
            }
        }

        internal void SetSpan(string member, int columnSpan)
        {
            ModernFieldDefinition field = this.Find(member);

            if (field != null)
            {
                if (columnSpan >= 1)
                {
                    field.ColumnSpan = columnSpan;
                }
            }
        }

        internal void SetStartAt(string member, int column)
        {
            ModernFieldDefinition field = this.Find(member);

            if (field != null)
            {
                field.StartColumn = column;
            }
        }

        internal void SetLink(string member)
        {
            ModernFieldDefinition field = this.Find(member);

            if (field != null)
            {
                field.IsLink = true;
            }
        }

        internal void SetBadge(string member)
        {
            ModernFieldDefinition field = this.Find(member);

            if (field != null)
            {
                field.IsBadge = true;
            }
        }

        internal void SetBadgeAccent(string member, string accentValues)
        {
            ModernFieldDefinition field = this.Find(member);

            if (field != null)
            {
                field.IsBadge = true;
                field.BadgeAccentValues = accentValues;
            }
        }

        internal void SetBadgeSpin(string member, string spinValues)
        {
            ModernFieldDefinition field = this.Find(member);

            if (field != null)
            {
                field.IsBadge = true;
                field.BadgeSpinValues = spinValues;
            }
        }

        internal void SetFormat(string member, string format)
        {
            ModernFieldDefinition field = this.Find(member);

            if (field != null)
            {
                field.Format = format;
            }
        }

        /// <summary>적은 순서대로 맨 앞으로 — 쿼리 순서가 카드 순서와 다를 때만 쓴다.</summary>
        public FieldDefinitions First(params string[] members)
        {
            int position = 0;

            foreach (string member in Names(members))
            {
                int index = this.IndexOf(member);

                if (index < 0)
                {
                    continue;
                }

                ModernFieldDefinition field = this.fields[index];
                this.fields.RemoveAt(index);
                this.fields.Insert(position, field);
                position++;
            }

            return this;
        }

        public ModernFieldDefinition[] ToArray()
        {
            return this.fields.ToArray();
        }

        /// <summary>
        /// 카드에 싣는다. <see cref="ModernFieldList.DefineFields"/>는 값을 비우므로,
        /// 값을 채우는 <see cref="ModernFieldList.SetRow"/>보다 **먼저** 부른다.
        /// 결과가 비어 있으면(조회 실패·null) 지금 필드를 그대로 둔다.
        /// </summary>
        public void Apply(ModernFieldList list)
        {
            if (list == null || this.fields.Count == 0)
            {
                return;
            }

            list.DefineFields(this.fields.ToArray());
        }

        private ModernFieldDefinition Find(string member)
        {
            int index = this.IndexOf(member);

            return index < 0 ? null : this.fields[index];
        }

        private int IndexOf(string member)
        {
            for (int index = 0; index < this.fields.Count; index++)
            {
                if (string.Equals(this.fields[index].Member, member, StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            return -1;
        }

        private static string[] Names(string[] names)
        {
            return names == null ? new string[0] : names;
        }
    }
}
