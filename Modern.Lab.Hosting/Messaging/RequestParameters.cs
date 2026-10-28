using System;
using System.Collections.Generic;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Messaging
{
    /// <summary>
    /// 화면이 모아 <c>.Server.cs</c>에 넘기는 값 묶음(2026-10-24, 필드 키는 2026-10-28). 값이 늘어도 조회 메서드의 선언이
    /// 바뀌지 않아 회사가 고친 <c>.Server.cs</c>를 교체하지 않는다. 선택한 행은 묶음에 넣지 않고 <c>DataRow</c> 인자로 따로 넘긴다.
    /// <para>
    /// 화면(<c>.cs</c>)은 검색창·다이얼로그 값을 <see cref="Of"/>에 필드와 값을 번갈아 넣는다 —
    /// <c>Of(Schemas.Lot.LotId, lotId, Schemas.Criteria.Keyword, keyword)</c>. 글자 키는 받지 않는다.
    /// <c>.Server.cs</c>는 <see cref="Value"/>로 그 필드의 값을 글자로 꺼내 전문 키 옆에 적는다 —
    /// <c>"LotId", parameters.Value(Schemas.Lot.LotId)</c>. 화면이 넣지 않은 필드를 꺼내면 넣은 필드 목록과 함께
    /// 예외가 난다(빈 값으로 조용히 나가지 않는다). 키는 필드의 컬럼 이름이라 같은 컬럼을 가진 다른 엔티티의 필드로 꺼내도 된다.
    /// </para>
    /// </summary>
    public sealed class RequestParameters
    {
        private readonly Dictionary<string, object> values;
        private readonly List<string> names;

        private RequestParameters(Dictionary<string, object> values, List<string> names)
        {
            this.values = values;
            this.names = names;
        }

        /// <summary>필드/값이 번갈아 오는 배열로 묶음을 만든다. <c>Of()</c>는 빈 묶음. 필드 자리에 다른 것이 오면 예외.</summary>
        public static RequestParameters Of(params object[] fieldValues)
        {
            object[] pairs = fieldValues ?? new object[0];

            if (pairs.Length % 2 != 0)
            {
                throw new ArgumentException("Of takes field/value pairs.", "fieldValues");
            }

            Dictionary<string, object> values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            List<string> names = new List<string>();

            for (int index = 0; index < pairs.Length; index = index + 2)
            {
                string name = KeyName(pairs[index], index);

                if (values.ContainsKey(name))
                {
                    throw new ArgumentException("Field must be given once: " + name, "fieldValues");
                }

                values.Add(name, pairs[index + 1]);
                names.Add(name);
            }

            return new RequestParameters(values, names);
        }

        /// <summary>
        /// <see cref="Of"/>로 넣은 그 필드의 값을 글자로 돌려준다 — bool 은 Y/N, 숫자·날짜는 불변 문화권 표기, null 은 빈 글자.
        /// 화면이 넣지 않은 필드면 넣은 필드 목록을 담은 예외.
        /// </summary>
        public string Value(Field field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }

            object value;

            if (!this.values.TryGetValue(field.Name, out value))
            {
                throw new ArgumentException(
                        "The screen did not provide " + field.Name + ". Provided: " + this.ProvidedNames() + ".", "field");
            }

            return ServerMessageFormat.ToText(value);
        }

        private string ProvidedNames()
        {
            return this.names.Count == 0 ? "(none)" : string.Join(", ", this.names.ToArray());
        }

        private static string KeyName(object key, int position)
        {
            Field field = key as Field;

            if (field == null || string.IsNullOrEmpty(field.Name))
            {
                throw new ArgumentException("Position " + position + " must be a Field (Schemas.X.Y).", "fieldValues");
            }

            return field.Name;
        }
    }
}
