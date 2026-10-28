using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace Modern.Lab.Hosting.ResponseContracts
{
    public static class ResponseColumns
    {
        /// <summary>응답 컬럼 누락 창의 Why 줄 — 무엇이 잘못됐는가.</summary>
        public const string MissingWhy =
                "The server response is missing columns this screen needs, so the list was not filled.";

        /// <summary>응답 컬럼 누락 창의 Next 줄 — 현업이 다음에 할 일.</summary>
        public const string MissingNext =
                "Refresh. If it keeps happening, copy the details and send them to support.";

        /// <summary>두 줄을 합친 본문(영어 라벨 · 다이얼로그 강조 표기 <c>**</c> 고정) — 폼 베이스 밖에서 그대로 띄울 때.
        /// 폼 베이스를 쓰는 화면은 <c>ShowErrorMessage(caption, MissingWhy, MissingNext, details)</c> 로 라벨을 화면 언어에 맞춘다.
        /// const 가 아닌 이유 — 줄바꿈이 들어 있어 API 표면 스냅숏(한 줄 한 멤버)이 값을 적을 수 없다.</summary>
        public static readonly string MissingMessage =
                "**Why:** " + MissingWhy + Environment.NewLine + "**Next:** " + MissingNext;

        public static string MissingDetails(TableResponse response)
        {
            if (response == null)
            {
                return string.Empty;
            }

            string table = response.TableId ?? string.Empty;
            int dot = table.LastIndexOf('.');
            string subject = dot < 0 ? table : table.Substring(dot + 1);
            string received = " (received: " + Join(response.ReceivedColumns) + ")";

            if (response.Conflicts.Count > 0)
            {
                List<string> pairs = new List<string>();

                foreach (AliasConflict conflict in response.Conflicts)
                {
                    pairs.Add(conflict.ReceivedColumns.Count > 1
                            ? Join(conflict.ReceivedColumns, " / ")
                            : Join(conflict.ReceivedColumns, " / ") + " / " + conflict.StandardColumn);
                }

                return subject + " — alias conflict " + string.Join(", ", pairs.ToArray()) + received;
            }

            return subject + " — missing column " + Join(response.MissingColumns) + received;
        }

        private static string Join(IList<string> values)
        {
            return Join(values, ", ");
        }

        private static string Join(IList<string> values, string separator)
        {
            string[] array = new string[values.Count];
            values.CopyTo(array, 0);
            return string.Join(separator, array);
        }

        public static string Missing(DataTable table, params string[] required)
        {
            if (required == null || required.Length == 0)
            {
                return string.Empty;
            }

            if (table == null)
            {
                return string.Join(", ", required);
            }

            List<string> missing = new List<string>();

            foreach (string column in required)
            {
                if (!string.IsNullOrEmpty(column) && !table.Columns.Contains(column))
                {
                    missing.Add(column);
                }
            }

            return string.Join(", ", missing.ToArray());
        }

        public static bool IsBlank(string value)
        {
            return value == null || value.Trim().Length == 0;
        }

        public static bool IsInteger(string text)
        {
            int parsed;
            return text != null
                    && int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed);
        }
    }
}
