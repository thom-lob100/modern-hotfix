using System;
using System.Collections.Generic;
using System.Data;
using Modern.Lab.Data;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Messaging
{
    /// <summary>
    /// 화면이 모아 <c>.Server.cs</c>에 넘기는 조건 묶음(2026-10-24). 조건이 늘어도 조회 메서드의 선언이
    /// 바뀌지 않아 회사가 고친 <c>.Server.cs</c>를 교체하지 않는다.
    /// <para>
    /// 화면(<c>.cs</c>)은 검색창 값을 <see cref="Of"/>로, 선택한 행을 <see cref="From"/>으로 넣는다.
    /// <c>.Server.cs</c>는 <see cref="Take(string)"/>(검색창 값) · <see cref="Take(Field)"/>(그 엔티티 행의 컬럼)로
    /// 꺼내 <c>RequestFields</c>에 그대로 적는다. 전문 키는 적지 않으면 컬럼 이름의 PascalCase
    /// (<see cref="ParameterNameConverter"/>), 적으면 그 글자다. 없는 값·없는 행·없는 컬럼은 빈 값이다.
    /// </para>
    /// </summary>
    public sealed class RequestParameters
    {
        private readonly Dictionary<string, object> values;
        private readonly Dictionary<EntityDefinition, DataRow> rows;

        private RequestParameters(Dictionary<string, object> values)
        {
            this.values = values;
            this.rows = new Dictionary<EntityDefinition, DataRow>();
        }

        /// <summary>검색창 값으로 묶음을 만든다 — 컬럼 이름/값이 번갈아 온다. <c>Of()</c>는 빈 묶음.</summary>
        public static RequestParameters Of(params object[] columnValues)
        {
            return new RequestParameters(ServerMessageFormat.ToFieldMap(columnValues));
        }

        /// <summary>선택한 행을 엔티티 이름으로 넣는다. 같은 엔티티를 다시 넣으면 행이 바뀐다. <paramref name="row"/>는 null 이어도 된다.</summary>
        public RequestParameters From(EntityDefinition entity, DataRow row)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            this.rows[entity] = row;
            return this;
        }

        /// <summary>
        /// <see cref="Of"/>로 넣은 값을 꺼낸다. 키는 컬럼 이름(<c>SUB_PROD_TYP</c>)이면 PascalCase(<c>SubProdTyp</c>),
        /// 소문자가 든 이름(<c>holdCode</c> · <c>description</c>)은 회사가 정한 전문 키이므로 글자 그대로다.
        /// </summary>
        public RequestParameter Take(string column)
        {
            return this.Take(column, WireName(column));
        }

        /// <summary><see cref="Of"/>로 넣은 값을 <paramref name="name"/> 키로 꺼낸다.</summary>
        public RequestParameter Take(string column, string name)
        {
            if (string.IsNullOrEmpty(column))
            {
                throw new ArgumentException("Column name must be a non-empty string.", "column");
            }

            object value;
            return new RequestParameter(name, this.values.TryGetValue(column, out value) ? value : string.Empty);
        }

        /// <summary><see cref="From"/>으로 넣은 그 엔티티 행에서 필드 컬럼을 PascalCase 키로 꺼낸다.</summary>
        public RequestParameter Take(Field field)
        {
            return this.Take(field, ParameterNameConverter.Convert(FieldName(field), ParameterNameStyle.PascalCase));
        }

        /// <summary><see cref="From"/>으로 넣은 그 엔티티 행에서 필드 컬럼을 <paramref name="name"/> 키로 꺼낸다.</summary>
        public RequestParameter Take(Field field, string name)
        {
            string column = FieldName(field);
            DataRow row;

            if (!this.rows.TryGetValue(field.Owner, out row))
            {
                return new RequestParameter(name, string.Empty);
            }

            return new RequestParameter(name, TableHelper.CellText(row, column));
        }

        /// <summary><see cref="Of"/>로 넣은 값을 글자로 돌려준다 — 전문 이름을 가를 때 쓴다. 없으면 빈 문자열.</summary>
        public string Text(string column)
        {
            object value;

            if (string.IsNullOrEmpty(column) || !this.values.TryGetValue(column, out value) || value == null)
            {
                return string.Empty;
            }

            return value is string ? (string)value : ServerMessageFormat.FormatValue(value);
        }

        /// <summary><see cref="Of"/>로 넣은 컬럼인가.</summary>
        public bool Contains(string column)
        {
            return !string.IsNullOrEmpty(column) && this.values.ContainsKey(column);
        }

        /// <summary><see cref="From"/>으로 넣은 엔티티 행 — 없으면 null.</summary>
        public DataRow Row(EntityDefinition entity)
        {
            DataRow row;
            return entity != null && this.rows.TryGetValue(entity, out row) ? row : null;
        }

        private static string WireName(string column)
        {
            string name = (column ?? string.Empty).Trim();

            foreach (char letter in name)
            {
                if (char.IsLower(letter))
                {
                    return name;
                }
            }

            return ParameterNameConverter.Convert(name, ParameterNameStyle.PascalCase);
        }

        private static string FieldName(Field field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }

            if (field.Owner == null)
            {
                throw new ArgumentException("Field must belong to a sealed entity: " + field.Name, "field");
            }

            return field.Name;
        }
    }
}
