using System;
using System.Collections.Generic;
using System.Data;

namespace Modern.Lab.Controls.Wpf.Data
{
    /// <summary>
    /// 조회 결과(<see cref="DataTable"/>/<see cref="DataView"/>)가 화면의 컬럼을 정한다 —
    /// 컬럼 목록을 손으로 나열하지 않는 그리드·정보 카드의 기본 규칙이다.
    ///
    /// 규칙은 셋뿐이고 전부 컬럼 **이름**으로 정해진다.
    /// <list type="bullet">
    /// <item>캡션은 용어사전(<see cref="GridCaptionCatalog"/>)이 푼다 — "LOT_ID" → "Lot Id".</item>
    /// <item>이름이 <c>_COLOR</c>로 끝나면 표에 내보내지 않는다 — 행 글자색처럼 화면이
    /// 값으로만 읽는 보조 컬럼이다.</item>
    /// <item>이름이 <c>_TM</c>으로 끝나면 시각으로 보고 <c>yyyy-MM-dd HH:mm:ss</c> +
    /// 가운데 정렬을 준다. 시각이 아닌 키(<c>TIMEKEY</c>)는 접미사가 달라 걸리지 않는다.
    /// 캡션 사전에 형식·정렬을 등록한 컬럼은 사전 값이 이 관례보다 우선한다.</item>
    /// <item>(2026-09-30) 셀 정렬 기본은 가운데다. 이름이 <c>QTY</c>·<c>CNT</c>로 끝나면 숫자로 보고 오른쪽 + <c>#,##0</c>(소수점 없음),
    /// <c>NM</c>·<c>DESC</c>로 끝나거나 <c>DESCRIPTION</c>이면 글 컬럼으로 보고 왼쪽이다. 숫자 컬럼은 헤더도 오른쪽이다(2026-10-12,
    /// <see cref="ConventionHeaderAlignment"/>). 이 관례는 이름을 가진 모든 컬럼 정의·정보 카드 필드에
    /// 적용되고(<see cref="ConventionFormat"/>·<see cref="ConventionAlignment"/>), 사전·엔티티·화면 지정이 우선한다.</item>
    /// </list>
    ///
    /// 컬럼을 늘리거나 줄이거나 순서를 바꾸는 일은 **쿼리**에서 한다. 화면이 직접 정해야
    /// 하는 것(배지·링크·정렬·문맥 캡션)만 <c>ConfigureColumns</c>로 선언한다.
    /// </summary>
    public static class AutoColumns
    {
        /// <summary>화면에 내보내지 않는 보조 컬럼의 이름 접미사.</summary>
        private const string colorSuffix = "_COLOR";

        /// <summary>시각 컬럼의 이름 접미사와 표준 표기.</summary>
        private const string timeSuffix = "_TM";

        private const string timeFormat = "yyyy-MM-dd HH:mm:ss";

        // 숫자·글 컬럼의 이름 접미사와 숫자 표준 표기(2026-09-30 사용자 결정).
        private static readonly string[] numberSuffixes = { "QTY", "CNT" };

        private static readonly string[] textSuffixes = { "NM", "DESC", "DESCRIPTION" };

        private const string numberFormat = "#,##0";

        /// <summary>
        /// 소스의 컬럼에서 그리드 컬럼 정의를 만든다. DataTable/DataView가 아니면 빈 배열이다
        /// (그 경우 컬럼은 화면이 선언해야 한다).
        /// </summary>
        public static ModernDataGridColumn[] Of(object source)
        {
            return OfMembers(MembersOf(source));
        }

        /// <summary>
        /// 컬럼 이름만으로 그리드 컬럼 정의를 만든다 — 표에 없는 컬럼도 만든다. 엔티티가 정상 0건 응답의 헤더를
        /// <c>DisplayFields</c>로 구성할 때 쓴다. 시각 컬럼의 형식·정렬 규칙은 <see cref="Of"/>와 같다.
        /// </summary>
        public static ModernDataGridColumn[] OfMembers(string[] members)
        {
            if (members == null)
            {
                return new ModernDataGridColumn[0];
            }

            ModernDataGridColumn[] columns = new ModernDataGridColumn[members.Length];

            for (int index = 0; index < members.Length; index++)
            {
                ModernDataGridColumn column = new ModernDataGridColumn(members[index]);

                if (IsTime(members[index]))
                {
                    if (GridCaptionCatalog.ResolveFormat(members[index]) == null)
                    {
                        column.Format = timeFormat;
                    }

                    if (!GridCaptionCatalog.ResolveTextAlignment(members[index]).HasValue)
                    {
                        column.TextAlignment = GridTextAlignment.Center;
                    }
                }

                columns[index] = column;
            }

            return columns;
        }

        /// <summary>
        /// 소스에서 화면에 내보낼 컬럼 이름을 순서대로 돌려준다 — 정보 카드처럼
        /// 컬럼 정의가 아니라 이름만 필요한 쪽이 쓴다.
        /// </summary>
        public static string[] MembersOf(object source)
        {
            DataTable table = TableOf(source);

            if (table == null)
            {
                return new string[0];
            }

            List<string> members = new List<string>();

            foreach (DataColumn column in table.Columns)
            {
                if (!column.ColumnName.EndsWith(colorSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    members.Add(column.ColumnName);
                }
            }

            return members.ToArray();
        }

        /// <summary>이름이 시각 컬럼 관례에 맞는지.</summary>
        public static bool IsTime(string member)
        {
            return !string.IsNullOrEmpty(member)
                    && member.EndsWith(timeSuffix, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>이름이 숫자 컬럼 관례(끝이 QTY·CNT)에 맞는지(2026-09-30).</summary>
        public static bool IsNumber(string member)
        {
            return EndsWithAny(member, numberSuffixes);
        }

        /// <summary>이름이 글 컬럼 관례(끝이 NM·DESC, 또는 DESCRIPTION)에 맞는지(2026-09-30) — 셀 왼쪽 정렬.</summary>
        public static bool IsText(string member)
        {
            return EndsWithAny(member, textSuffixes);
        }

        /// <summary>이름 관례의 표시 형식 — 시각은 <c>yyyy-MM-dd HH:mm:ss</c>, 숫자는 <c>#,##0</c>, 그 밖은 null.</summary>
        public static string ConventionFormat(string member)
        {
            return IsTime(member) ? timeFormat : IsNumber(member) ? numberFormat : null;
        }

        /// <summary>이름 관례의 셀 정렬 — 숫자는 Right, 글은 Left, 그 밖은 null(기본 Center 그대로).</summary>
        public static GridTextAlignment? ConventionAlignment(string member)
        {
            if (IsNumber(member))
            {
                return GridTextAlignment.Right;
            }

            return IsText(member) ? GridTextAlignment.Left : (GridTextAlignment?)null;
        }

        /// <summary>
        /// 이름 관례의 헤더 정렬 — 숫자는 Right, 그 밖은 null(기본 Center 그대로). 숫자 헤더를 값과 같은 오른쪽 선에 맞추고,
        /// 가운데 정렬 헤더가 자동 맞춤 폭에 더 잡는 글리프 거울 여백(약 20px)을 수량 컬럼에서 없앤다(2026-10-12 사용자).
        /// </summary>
        public static GridTextAlignment? ConventionHeaderAlignment(string member)
        {
            return IsNumber(member) ? GridTextAlignment.Right : (GridTextAlignment?)null;
        }

        private static bool EndsWithAny(string member, string[] suffixes)
        {
            if (string.IsNullOrEmpty(member))
            {
                return false;
            }

            foreach (string suffix in suffixes)
            {
                if (member.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>시각 컬럼의 표준 표기 — 관례를 벗어난 이름에 화면이 직접 줄 때.</summary>
        public static string TimeFormat
        {
            get { return timeFormat; }
        }

        private static DataTable TableOf(object source)
        {
            DataTable table = source as DataTable;

            if (table != null)
            {
                return table;
            }

            DataView view = source as DataView;

            return view == null ? null : view.Table;
        }
    }
}
