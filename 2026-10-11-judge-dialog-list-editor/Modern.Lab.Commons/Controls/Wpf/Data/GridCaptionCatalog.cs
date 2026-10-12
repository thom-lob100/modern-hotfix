using System;
using System.Collections.Generic;
using System.Text;

namespace Modern.Lab.Controls.Wpf.Data
{
    /// <summary>
    /// 그리드 컬럼 캡션 용어사전 — "필드 이름(DB 컬럼) → 표준 캡션"의 등록형 그릇.
    ///
    /// Common의 GridCaptionDictionary가 제공하는 회사 표준 용어집 또는 앱의
    /// 추가 용어집을 앱 시작 시(첫 폼 생성 전) Register/RegisterRange로
    /// 등록하면, 이후 캡션 인자가 없는 컬럼 정의(new ModernDataGridColumn("ITEM_ID"))가
    /// 여기 표준 캡션을 자동으로 받는다. 화면 문맥상 다른 표현이 필요하면
    /// headerText를 받는 생성자로 명시해 재정의한다 — 명시가 항상 사전을 이긴다.
    ///
    /// 별도 해석 로직이 필요하면 ModernDataGridColumn.CaptionResolver에 직접
    /// 제공자를 등록한다 — 그 경우 이 사전 대신 그 제공자가 쓰인다.
    /// </summary>
    public static class GridCaptionCatalog
    {
        // 필드 이름은 대소문자 무시로 찾는다 (DB 컬럼 표기 편차 허용).
        // 등록은 앱 시작 시 1회가 원칙이지만, 안전을 위해 조회/등록을 잠근다.
        private static readonly Dictionary<string, string> captions =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // 그리드 헤더 본 캡션 위·아래의 작은 글씨 — 함께 등록한 항목만 갖는다. 본 캡션 문자열에는 섞지 않는다.
        private static readonly Dictionary<string, string> topLabels =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, string> bottomLabels =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // 컬럼 표시 설정 — 형식·셀 정렬·헤더 정렬. 캡션과 따로 등록하고 캡션을 다시 등록해도(DB 용어 덮어쓰기) 지워지지 않는다.
        private static readonly Dictionary<string, string> formats =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, GridTextAlignment> textAlignments =
            new Dictionary<string, GridTextAlignment>(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, GridTextAlignment> headerAlignments =
            new Dictionary<string, GridTextAlignment>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, GridTextAlignment> topLabelAlignments =
                new Dictionary<string, GridTextAlignment>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, GridTextAlignment> bottomLabelAlignments =
                new Dictionary<string, GridTextAlignment>(StringComparer.OrdinalIgnoreCase);

        private static readonly object syncRoot = new object();

        /// <summary>
        /// 표준 캡션 한 건을 등록한다. 같은 필드 이름을 다시 등록하면 덮어쓴다(위·아래 작은 글씨도 지운다).
        /// 캡션이 null/빈 값이면 항목을 제거한다(필드 이름 그대로 표시로 복귀).
        /// </summary>
        public static void Register(string dataPropertyName, string caption)
        {
            if (string.IsNullOrEmpty(dataPropertyName))
            {
                return;
            }

            lock (syncRoot)
            {
                topLabels.Remove(dataPropertyName);
                bottomLabels.Remove(dataPropertyName);

                if (string.IsNullOrEmpty(caption))
                {
                    captions.Remove(dataPropertyName);
                }
                else
                {
                    captions[dataPropertyName] = caption;
                }
            }
        }

        /// <summary>
        /// 본 캡션과 그리드 헤더의 위·아래 작은 글씨를 함께 등록한다 — <c>Register("SOURCE_SLOT", "Slot", "Source", null)</c>.
        /// 위·아래는 각각 비워도 되고 둘 다 써도 된다. 캡션 인자 없는 컬럼 정의(<c>new ModernDataGridColumn("SOURCE_SLOT")</c>)가
        /// 세 값을 함께 받는다. 캡션이 비면 항목 전체를 지운다.
        /// </summary>
        public static void Register(string dataPropertyName, string caption, string topLabel, string bottomLabel)
        {
            if (string.IsNullOrEmpty(dataPropertyName))
            {
                return;
            }

            string top = GridHeaderCaption.Trimmed(topLabel);
            string bottom = GridHeaderCaption.Trimmed(bottomLabel);

            lock (syncRoot)
            {
                Register(dataPropertyName, caption);

                if (string.IsNullOrEmpty(caption))
                {
                    return;
                }

                if (top.Length > 0)
                {
                    topLabels[dataPropertyName] = top;
                }

                if (bottom.Length > 0)
                {
                    bottomLabels[dataPropertyName] = bottom;
                }
            }
        }

        /// <summary>표준 용어집을 일괄 등록한다. 기존 항목과 겹치면 덮어쓴다.</summary>
        public static void RegisterRange(IEnumerable<KeyValuePair<string, string>> entries)
        {
            if (entries == null)
            {
                return;
            }

            foreach (KeyValuePair<string, string> entry in entries)
            {
                Register(entry.Key, entry.Value);
            }
        }

        /// <summary>등록된 항목을 모두 지운다 (테마 전환 테스트 등 재구성용).</summary>
        public static void Clear()
        {
            lock (syncRoot)
            {
                captions.Clear();
                topLabels.Clear();
                bottomLabels.Clear();
                formats.Clear();
                textAlignments.Clear();
                headerAlignments.Clear();
                topLabelAlignments.Clear();
                bottomLabelAlignments.Clear();
            }
        }

        /// <summary>
        /// 한 줄 표기 — 위·본·아래를 공백 하나로 잇는다("Source Slot"). 그리드 헤더가 아닌 라벨·편집 폼처럼 사전을 직접 읽는 곳이 쓴다
        /// (여러 줄 UI는 그리드 헤더에만 있다). 빈 값은 건너뛰고 모두 비면 빈 문자열이다.
        /// </summary>
        public static string PlainCaption(string topLabel, string caption, string bottomLabel)
        {
            return GridHeaderCaption.PlainText(topLabel, caption, bottomLabel);
        }

        /// <summary>등록된 위 작은 글씨. 없으면 null.</summary>
        public static string ResolveTopLabel(string dataPropertyName)
        {
            return Lookup(topLabels, dataPropertyName);
        }

        /// <summary>등록된 아래 작은 글씨. 없으면 null.</summary>
        public static string ResolveBottomLabel(string dataPropertyName)
        {
            return Lookup(bottomLabels, dataPropertyName);
        }

        /// <summary>
        /// 컬럼의 표준 표시 형식을 등록한다 — <c>RegisterFormat("LOT_QTY", "#,##0")</c>. 문법은 <see cref="ModernDataGridColumn.Format"/>과 같다.
        /// 캡션 인자 유무와 무관하게 컬럼 정의·정보 카드 필드가 받고, 이름 관례(<c>_TM</c> 시각 형식)보다 우선한다. null/빈 값이면 지운다.
        /// </summary>
        public static void RegisterFormat(string dataPropertyName, string format)
        {
            if (string.IsNullOrEmpty(dataPropertyName))
            {
                return;
            }

            lock (syncRoot)
            {
                if (string.IsNullOrEmpty(format))
                {
                    formats.Remove(dataPropertyName);
                }
                else
                {
                    formats[dataPropertyName] = format;
                }
            }
        }

        /// <summary>
        /// 컬럼의 표준 셀 정렬·헤더 정렬을 등록한다. null인 쪽은 등록하지 않고 지운다 — <c>RegisterAlignment("LOT_QTY", GridTextAlignment.Right, GridTextAlignment.Right)</c>.
        /// 컬럼 정의가 받고, 이름 관례(<c>_TM</c> 가운데 정렬)보다 우선한다.
        /// </summary>
        public static void RegisterAlignment(string dataPropertyName, GridTextAlignment? textAlignment, GridTextAlignment? headerAlignment)
        {
            RegisterAlignment(dataPropertyName, textAlignment, headerAlignment, null, null);
        }

        /// <summary>
        /// 셀·헤더 정렬에 더해 헤더 위·아래 작은 글씨 줄의 정렬까지 등록한다 — null 인 쪽은 지운다(작은 줄은 헤더 정렬을 따른다).
        /// </summary>
        public static void RegisterAlignment(
                string dataPropertyName, GridTextAlignment? textAlignment, GridTextAlignment? headerAlignment,
                GridTextAlignment? topLabelAlignment, GridTextAlignment? bottomLabelAlignment)
        {
            if (string.IsNullOrEmpty(dataPropertyName))
            {
                return;
            }

            lock (syncRoot)
            {
                SetOrRemove(textAlignments, dataPropertyName, textAlignment);
                SetOrRemove(headerAlignments, dataPropertyName, headerAlignment);
                SetOrRemove(topLabelAlignments, dataPropertyName, topLabelAlignment);
                SetOrRemove(bottomLabelAlignments, dataPropertyName, bottomLabelAlignment);
            }
        }

        /// <summary>등록된 표시 형식. 없으면 null.</summary>
        public static string ResolveFormat(string dataPropertyName)
        {
            return Lookup(formats, dataPropertyName);
        }

        /// <summary>등록된 셀 정렬. 없으면 null.</summary>
        public static GridTextAlignment? ResolveTextAlignment(string dataPropertyName)
        {
            return LookupAlignment(textAlignments, dataPropertyName);
        }

        /// <summary>등록된 헤더 정렬. 없으면 null.</summary>
        public static GridTextAlignment? ResolveHeaderAlignment(string dataPropertyName)
        {
            return LookupAlignment(headerAlignments, dataPropertyName);
        }

        /// <summary>등록된 헤더 위 작은 글씨 줄 정렬. 없으면 null(헤더 정렬을 따른다).</summary>
        public static GridTextAlignment? ResolveTopLabelAlignment(string dataPropertyName)
        {
            return LookupAlignment(topLabelAlignments, dataPropertyName);
        }

        /// <summary>등록된 헤더 아래 작은 글씨 줄 정렬. 없으면 null(헤더 정렬을 따른다).</summary>
        public static GridTextAlignment? ResolveBottomLabelAlignment(string dataPropertyName)
        {
            return LookupAlignment(bottomLabelAlignments, dataPropertyName);
        }

        // 컬럼 정의가 만들어질 때 사전의 표시 설정을 얹는다 — 등록된 것만 바꾸고 나머지는 컬럼 기본값 그대로다.
        internal static void ApplyDisplay(ModernDataGridColumn column)
        {
            if (column == null || string.IsNullOrEmpty(column.DataPropertyName))
            {
                return;
            }

            // 사전에 없으면 이름 관례(시각·숫자 형식, 숫자 오른쪽·글 왼쪽 — AutoColumns)가 채운다.
            string format = ResolveFormat(column.DataPropertyName) ?? AutoColumns.ConventionFormat(column.DataPropertyName);
            GridTextAlignment? textAlignment = ResolveTextAlignment(column.DataPropertyName)
                    ?? AutoColumns.ConventionAlignment(column.DataPropertyName);
            GridTextAlignment? headerAlignment = ResolveHeaderAlignment(column.DataPropertyName)
                    ?? AutoColumns.ConventionHeaderAlignment(column.DataPropertyName);

            if (format != null)
            {
                column.Format = format;
            }

            if (textAlignment.HasValue)
            {
                column.TextAlignment = textAlignment.Value;
            }

            if (headerAlignment.HasValue)
            {
                column.HeaderAlignment = headerAlignment.Value;
            }

            GridTextAlignment? topLabelAlignment = ResolveTopLabelAlignment(column.DataPropertyName);
            GridTextAlignment? bottomLabelAlignment = ResolveBottomLabelAlignment(column.DataPropertyName);

            if (topLabelAlignment.HasValue)
            {
                column.TopLabelAlignment = topLabelAlignment.Value;
            }

            if (bottomLabelAlignment.HasValue)
            {
                column.BottomLabelAlignment = bottomLabelAlignment.Value;
            }
        }

        private static void SetOrRemove(Dictionary<string, GridTextAlignment> map, string dataPropertyName, GridTextAlignment? value)
        {
            if (value.HasValue)
            {
                map[dataPropertyName] = value.Value;
            }
            else
            {
                map.Remove(dataPropertyName);
            }
        }

        private static GridTextAlignment? LookupAlignment(Dictionary<string, GridTextAlignment> map, string dataPropertyName)
        {
            if (string.IsNullOrEmpty(dataPropertyName))
            {
                return null;
            }

            lock (syncRoot)
            {
                GridTextAlignment alignment;
                return map.TryGetValue(dataPropertyName, out alignment) ? alignment : (GridTextAlignment?)null;
            }
        }

        private static string Lookup(Dictionary<string, string> map, string dataPropertyName)
        {
            if (string.IsNullOrEmpty(dataPropertyName))
            {
                return null;
            }

            lock (syncRoot)
            {
                string text;
                return map.TryGetValue(dataPropertyName, out text) ? text : null;
            }
        }

        /// <summary>
        /// DB 컬럼 표기(대문자·숫자·언더스코어 — 예: "EQP_ID")를 사람이 읽는
        /// 표기("Eqp Id")로 바꾼다 — <b>용어사전에 없는 컬럼의 폴백</b>이다.
        /// Oracle INITCAP과 같은 규칙: '_'를 공백으로 나누고 토큰마다 첫 글자만
        /// 대문자, 나머지는 소문자. 약어 예외는 두지 않는다 — 규칙이 단순해야
        /// "정의 안 된 컬럼은 무조건 이 모양"으로 설명이 끝나고, 기계 변환 티가
        /// 나는 결과("Eqp Id")가 "아직 용어 미정의" 신호를 겸한다.
        ///
        /// 대문자·숫자·언더스코어로만 된 이름에만 적용한다 — camelCase 등 다른
        /// 표기(예: "IsLcc")는 그대로 돌려준다 ("Islcc"로 망가지지 않게).
        /// </summary>
        public static string Humanize(string dataPropertyName)
        {
            if (string.IsNullOrEmpty(dataPropertyName) || !IsDbStyleName(dataPropertyName))
            {
                return dataPropertyName;
            }

            string[] tokens = dataPropertyName.Split(
                    new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder builder = new StringBuilder(dataPropertyName.Length);

            foreach (string token in tokens)
            {
                if (builder.Length > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(char.ToUpperInvariant(token[0]));

                if (token.Length > 1)
                {
                    builder.Append(token.Substring(1).ToLowerInvariant());
                }
            }

            // 전부 언더스코어("___") 같은 극단값은 원본 그대로.
            return builder.Length > 0 ? builder.ToString() : dataPropertyName;
        }

        // DB 컬럼 표기인지 — 대문자/숫자/언더스코어뿐이고 대문자가 하나는 있다.
        private static bool IsDbStyleName(string name)
        {
            bool hasUpperLetter = false;

            foreach (char ch in name)
            {
                if (ch >= 'A' && ch <= 'Z')
                {
                    hasUpperLetter = true;
                }
                else if ((ch < '0' || ch > '9') && ch != '_')
                {
                    return false;
                }
            }

            return hasUpperLetter;
        }

        /// <summary>표준 캡션을 돌려준다. 사전에 없으면 null (호출측이 필드 이름 사용).</summary>
        public static string Resolve(string dataPropertyName)
        {
            if (string.IsNullOrEmpty(dataPropertyName))
            {
                return null;
            }

            lock (syncRoot)
            {
                string caption;
                return captions.TryGetValue(dataPropertyName, out caption) ? caption : null;
            }
        }
    }
}
