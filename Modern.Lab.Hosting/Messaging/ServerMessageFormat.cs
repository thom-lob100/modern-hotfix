using System;
using System.Collections.Generic;
using System.Data;

namespace Modern.Lab.Hosting.Messaging
{

    public static class ServerMessageFormat
    {

        public const string FieldReturnCode = "ReturnCode";

        public const string FieldResultCode = "ResultCode";

        public const string FieldResultMessage = "ResultMessage";

        public const string FieldErrorCode = "ErrorCode";

        public const string FieldErrorMessage = "ErrorMessage";

        public const string FieldSendMessage = "SendMessage";

        public const string SuccessReturnCode = "0";

        public const string ReplyNameSuffix = "R";

        /// <summary>
        /// 요청 전문을 조립한다 — <c>Name Key=value Key2=[value with space]</c>.
        /// 값 감싸기 규칙은 <see cref="FormatValue"/> 한 곳에 있다. 폼과 편집기는 값을 감싸지 않는다.
        /// </summary>
        public static string BuildRequestText(string action, IEnumerable<KeyValuePair<string, object>> fields)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder((action ?? string.Empty).Trim());

            if (fields != null)
            {
                foreach (KeyValuePair<string, object> field in fields)
                {
                    builder.Append(' ');
                    builder.Append(field.Key);
                    builder.Append('=');
                    builder.Append(FormatValue(field.Value));
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// 이름/값이 번갈아 오는 배열로 요청 전문을 조립한다(<c>RequestFields</c>와 같은 입력 모양).
        /// 배열 안의 <see cref="RequestParameter"/>는 이름/값 한 쌍으로 펼친다(<see cref="ExpandParameters"/>).
        /// </summary>
        public static string BuildRequestText(string action, params object[] fields)
        {
            object[] pairs = ExpandParameters(fields);
            Dictionary<string, object> map = ToFieldMap(pairs);
            System.Text.StringBuilder builder =
                    new System.Text.StringBuilder((action ?? string.Empty).Trim());

            for (int index = 0; index < pairs.Length; index = index + 2)
            {
                string name = (string)pairs[index];
                builder.Append(' ');
                builder.Append(name);
                builder.Append('=');
                builder.Append(FormatValue(map[name]));
            }

            return builder.ToString();
        }

        /// <summary>
        /// 이름/값 배열 안에 섞인 <see cref="RequestParameter"/>를 이름, 값 두 칸으로 펼친다 —
        /// <c>["LotId", "L1", Take("KEYWORD")]</c> → <c>["LotId", "L1", "Keyword", "k"]</c>. 쌍이 없으면 같은 내용의 새 배열.
        /// </summary>
        public static object[] ExpandParameters(object[] fields)
        {
            List<object> pairs = new List<object>();

            if (fields == null)
            {
                return pairs.ToArray();
            }

            foreach (object field in fields)
            {
                RequestParameter parameter = field as RequestParameter;

                if (parameter == null)
                {
                    pairs.Add(field);
                    continue;
                }

                pairs.Add(parameter.Name);
                pairs.Add(parameter.Value);
            }

            return pairs.ToArray();
        }

        /// <summary>
        /// 이름/값이 번갈아 오는 배열을 검증된 사전으로 바꾼다. null 값은 빈 문자열.
        /// 배열 안의 <see cref="RequestParameter"/>는 먼저 펼친다.
        /// </summary>
        public static Dictionary<string, object> ToFieldMap(object[] fields)
        {
            Dictionary<string, object> map =
                    new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            if (fields == null || fields.Length == 0)
            {
                return map;
            }

            fields = ExpandParameters(fields);

            if (fields.Length % 2 != 0)
            {
                throw new ArgumentException("Fields must be name/value pairs.", "fields");
            }

            for (int i = 0; i < fields.Length; i = i + 2)
            {
                string name = fields[i] as string;

                if (string.IsNullOrEmpty(name))
                {
                    throw new ArgumentException("Field name must be a non-empty string.", "fields");
                }

                if (map.ContainsKey(name))
                {
                    throw new ArgumentException("Field name must be unique: " + name, "fields");
                }

                map.Add(name, fields[i + 1] ?? (object)string.Empty);
            }

            return map;
        }

        /// <summary>
        /// 전문 값 하나를 글자로 만든다. bool은 Y/N, 숫자·날짜는 불변 문화권 표기, 그 밖의 객체는 JSON.
        /// 공백이나 <c>=</c>가 들어 있으면 <c>[…]</c>로 감싼다(<see cref="NeedsBrackets"/>). 이미 <c>[…]</c>로
        /// 감싸 온 값과 JSON은 그대로 둔다. 값 안의 대괄호는 이 형식으로 표현할 수 없어 소괄호로 바꾼다.
        /// </summary>
        public static string FormatValue(object value)
        {
            string text;

            if (value == null)
            {
                text = string.Empty;
            }
            else if (value is string)
            {
                text = (string)value;
            }
            else if (value is bool)
            {
                text = ((bool)value) ? "Y" : "N";
            }
            else if (value is IFormattable)
            {
                text = ((IFormattable)value).ToString(null, System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                text = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(value);
            }

            if (IsWrapped(text) || StartsJson(text))
            {
                return text;
            }

            if (!NeedsBrackets(text))
            {
                return text;
            }

            return "[" + text.Replace('[', '(').Replace(']', ')') + "]";
        }

        /// <summary>
        /// 손으로 조립한 요청 원문의 값들을 <see cref="FormatValue"/> 규칙으로 감싼다 —
        /// <c>UpdateProduct ProdNm=300mm Base Wafer UseYn=Y</c> → <c>UpdateProduct ProdNm=[300mm Base Wafer] UseYn=Y</c>.
        /// 값의 끝은 응답 파서와 같은 규칙(공백 뒤 <c>이름=</c>)으로 정하고, 이미 <c>[…]</c>·<c>"…"</c>로 감싼 값은
        /// 글자 그대로 둔다. 원문을 다시 해석하므로 전송 경로에서는 쓰지 않고 진단과 호환 확인에만 사용한다.
        /// </summary>
        public static string NormalizeRequest(string requestText)
        {
            string text = (requestText ?? string.Empty).Trim();

            if (text.Length == 0)
            {
                return text;
            }

            System.Text.RegularExpressions.MatchCollection fields = RequestFieldPattern.Matches(text);

            if (fields.Count == 0)
            {
                return text;
            }

            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append(text.Substring(0, fields[0].Index).Trim());

            int consumed = 0;

            for (int i = 0; i < fields.Count; i = i + 1)
            {
                System.Text.RegularExpressions.Match field = fields[i];

                if (field.Index < consumed)
                {
                    continue;
                }

                int start = field.Index + field.Length;
                int next = text.Length;

                for (int j = i + 1; j < fields.Count; j = j + 1)
                {
                    if (fields[j].Index >= start)
                    {
                        next = fields[j].Index;
                        break;
                    }
                }

                string rawValue;

                if (start < text.Length && (text[start] == '[' || text[start] == '"'))
                {
                    int end = text[start] == '[' ? MatchingBracket(text, start) : ClosingQuote(text, start);
                    rawValue = text.Substring(start, end - start);
                    consumed = end;
                    builder.Append(' ').Append(field.Groups["key"].Value).Append('=').Append(rawValue);
                    continue;
                }

                rawValue = text.Substring(start, next - start).Trim();
                consumed = next;
                builder.Append(' ').Append(field.Groups["key"].Value).Append('=').Append(FormatValue(rawValue));
            }

            return builder.ToString();
        }

        private static readonly System.Text.RegularExpressions.Regex RequestFieldPattern =
                new System.Text.RegularExpressions.Regex(
                        @"(?:^|\s)(?<key>[A-Za-z_][A-Za-z0-9_]*)=",
                        System.Text.RegularExpressions.RegexOptions.CultureInvariant
                        | System.Text.RegularExpressions.RegexOptions.Compiled);

        private static int MatchingBracket(string text, int openAt)
        {
            int depth = 0;

            for (int i = openAt; i < text.Length; i = i + 1)
            {
                if (text[i] == '[')
                {
                    depth = depth + 1;
                }
                else if (text[i] == ']')
                {
                    depth = depth - 1;

                    if (depth == 0)
                    {
                        return i + 1;
                    }
                }
            }

            return text.Length;
        }

        private static int ClosingQuote(string text, int openAt)
        {
            int closeAt = text.IndexOf('"', openAt + 1);
            return closeAt < 0 ? text.Length : closeAt + 1;
        }

        /// <summary>값을 <c>[…]</c>로 감싸야 하는가 — 공백이 있거나 <c>=</c>가 있으면. 빈 값은 그대로 둔다.</summary>
        public static bool NeedsBrackets(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            if (text.IndexOf('=') >= 0)
            {
                return true;
            }

            foreach (char letter in text)
            {
                if (char.IsWhiteSpace(letter))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsWrapped(string text)
        {
            return text.Length >= 2 && text[0] == '[' && text[text.Length - 1] == ']';
        }

        private static bool StartsJson(string text)
        {
            return text.Length > 0 && (text[0] == '{' || text[0] == '[');
        }

        public static DataTable ParseQueryTable(string queryData)
        {
            return JsonTableConverter.ToDataTable(queryData);
        }

        public static string[] ColumnValues(DataTable table)
        {
            return ColumnValues(table, null);
        }

        public static string[] ColumnValues(DataTable table, string columnName)
        {
            List<string> values = new List<string>();

            if (table == null || table.Columns.Count == 0)
            {
                return values.ToArray();
            }

            if (!string.IsNullOrEmpty(columnName))
            {
                if (!table.Columns.Contains(columnName))
                {
                    return values.ToArray();
                }

                foreach (DataRow row in table.Rows)
                {
                    Add(values, Convert.ToString(row[columnName]));
                }

                return values.ToArray();
            }

            if (table.Rows.Count == 0)
            {
                foreach (DataColumn column in table.Columns)
                {
                    Add(values, column.ColumnName);
                }

                return values.ToArray();
            }

            if (table.Rows.Count == 1 && table.Columns.Count > 1)
            {
                for (int c = 0; c < table.Columns.Count; c = c + 1)
                {
                    Add(values, Convert.ToString(table.Rows[0][c]));
                }

                return values.ToArray();
            }

            foreach (DataRow row in table.Rows)
            {
                Add(values, Convert.ToString(row[0]));
            }

            return values.ToArray();
        }

        private static void Add(List<string> values, string value)
        {
            string trimmed = (value ?? string.Empty).Trim();

            if (trimmed.Length > 0)
            {
                values.Add(trimmed);
            }
        }
    }
}
