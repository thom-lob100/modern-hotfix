using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace Modern.Lab.Captions
{
    /// <summary>
    /// 화면 글자 번역 사전 — "소스에 적힌 영어 글자 → 지금 언어의 글자"의 등록형 그릇(2026-11-15).
    /// 키는 코드값도 컬럼명도 아니고 <b>소스에 적힌 영어 글자 그대로</b>다. 소스·디자이너·엔티티에는 영어 원문이 남고,
    /// 컨트롤이 글자를 그리는 자리에서 이 사전을 지나 바뀐다. 표에 없는 글자는 영어 그대로 보인다.
    ///
    /// 진입점이 첫 폼을 만들기 전에 한 번 부른다:
    /// <code>
    /// ModernLanguage.Use("ko");                                   // 환경설정에서 읽은 언어
    /// ModernLanguage.Load(table, "SOURCE_TEXT", "KO_TEXT", "SCREEN_NM");  // 진입점이 조회한 번역 표 — 컬럼 이름은 회사가 정한다
    /// </code>
    /// <see cref="Use"/>를 부르지 않으면 꺼져 있고 모든 화면이 지금처럼 영어다. 이 클래스는 DB 를 모른다 — 조회는 호출측이 한다
    /// (<c>GridCaptionDictionary.Apply</c> 와 같은 방식).
    ///
    /// 번역 표 한 줄 = 원문 · 글자 · 화면(선택). 화면 이름(폼 클래스 이름)이 적힌 줄이 그 화면에서 공통 줄보다 먼저다.
    /// 표에 없어 영어로 보인 글자는 <see cref="Missing"/>에 쌓이고 <see cref="SaveMissing"/>이 글자 파일로 내보낸다 —
    /// 그 파일을 번역해 표에 넣으면 다음 실행부터 번역돼 보인다. 이미 열린 화면은 실시간으로 바뀌지 않는다.
    /// </summary>
    public static class ModernLanguage
    {
        private static readonly object syncRoot = new object();

        // 공통 줄 — 원문 → 글자. 원문은 대소문자를 가린다("Hold" 와 "HOLD" 는 다른 글자다).
        private static readonly Dictionary<string, string> common =
            new Dictionary<string, string>(StringComparer.Ordinal);

        // 화면 줄 — 화면 이름 → (원문 → 글자).
        private static readonly Dictionary<string, Dictionary<string, string>> byScreen =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        // 번역 결과 글자 — 다시 사전을 지나도(표시용 사본 재번역 등) 누락으로 세지 않는다.
        private static readonly HashSet<string> translatedTexts = new HashSet<string>(StringComparer.Ordinal);

        // 한 줄 표기("Request Serial No")를 이룬 조각 — 한 줄 전체가 표에 없으면 조각마다 번역해 잇는다.
        private static readonly Dictionary<string, string[]> compositions =
            new Dictionary<string, string[]>(StringComparer.Ordinal);

        // 표에 없던 글자 — 원문마다 한 줄, 처음 본 순서대로.
        private static readonly List<MissingEntry> missing = new List<MissingEntry>();

        private static readonly HashSet<string> missingSources = new HashSet<string>(StringComparer.Ordinal);

        private static string current = string.Empty;

        /// <summary>지금 언어 코드 — 빈 문자열이면 꺼져 있다(원문 그대로).</summary>
        public static string Current
        {
            get
            {
                lock (syncRoot)
                {
                    return current;
                }
            }
        }

        /// <summary>번역이 켜져 있는지 — <see cref="Use"/>에 언어를 준 뒤다. 표가 비어 있어도 켜져 있으면 누락 목록이 쌓인다.</summary>
        public static bool IsActive
        {
            get
            {
                lock (syncRoot)
                {
                    return current.Length > 0;
                }
            }
        }

        /// <summary>
        /// 언어를 정한다 — 첫 폼을 만들기 전에 한 번. null/빈 값이면 끈다. 언어가 바뀌면 앞 언어의 번역 표와 누락 목록을 비운다
        /// (그 뒤 <see cref="Load(DataTable, string, string)"/>로 새 언어의 표를 넘긴다).
        /// </summary>
        public static void Use(string language)
        {
            string next = language == null ? string.Empty : language.Trim();

            lock (syncRoot)
            {
                if (!string.Equals(current, next, StringComparison.OrdinalIgnoreCase))
                {
                    ClearEntries();
                }

                current = next;
            }
        }

        /// <summary>번역 표를 넘긴다(화면 구분 없음) — <see cref="Load(DataTable, string, string, string)"/>와 같고 모든 줄이 공통 줄이다.</summary>
        public static void Load(DataTable table, string sourceColumn, string textColumn)
        {
            Load(table, sourceColumn, textColumn, null);
        }

        /// <summary>
        /// 진입점이 조회한 번역 표를 넘긴다. 원문이나 글자가 빈 줄은 건너뛰고, 표가 null·빈 값이거나 컬럼이 없으면 아무것도 하지 않는다 — 예외 없음.
        /// 같은 원문을 다시 주면 뒤 줄이 이긴다. 줄바꿈·탭은 <c>\n</c>·<c>\t</c> 두 글자로 적어도 된다(<see cref="SaveMissing"/>이 그렇게 내보낸다).
        /// </summary>
        /// <param name="table">번역 표(행 = 글자 한 건).</param>
        /// <param name="sourceColumn">원문(소스의 영어 글자) 컬럼.</param>
        /// <param name="textColumn">지금 언어의 글자 컬럼 — 언어마다 컬럼이 다르면 진입점이 골라 넘긴다.</param>
        /// <param name="screenColumn">화면 이름(폼 클래스 이름) 컬럼 — null/빈 값이거나 표에 없으면 모든 줄이 공통 줄이다.</param>
        public static void Load(DataTable table, string sourceColumn, string textColumn, string screenColumn)
        {
            if (table == null || table.Rows.Count == 0
                    || string.IsNullOrEmpty(sourceColumn) || string.IsNullOrEmpty(textColumn)
                    || !table.Columns.Contains(sourceColumn) || !table.Columns.Contains(textColumn))
            {
                return;
            }

            bool hasScreen = !string.IsNullOrEmpty(screenColumn) && table.Columns.Contains(screenColumn);

            foreach (DataRow row in table.Rows)
            {
                string source = Convert.ToString(row[sourceColumn]);
                string text = Convert.ToString(row[textColumn]);
                string screen = hasScreen ? Convert.ToString(row[screenColumn]) : null;
                Register(screen, Unescape(source), Unescape(text));
            }
        }

        /// <summary>공통 줄 한 건을 등록한다 — 원문이나 글자가 비면 건너뛴다.</summary>
        public static void Register(string source, string text)
        {
            Register(null, source, text);
        }

        /// <summary>화면 줄 한 건을 등록한다 — 그 화면(폼 클래스 이름)에서만 공통 줄보다 먼저 쓰인다. 화면이 null/빈 값이면 공통 줄이다.</summary>
        public static void Register(string screen, string source, string text)
        {
            string key = KeyOf(source);

            if (key.Length == 0 || string.IsNullOrEmpty(text))
            {
                return;
            }

            string screenName = screen == null ? string.Empty : screen.Trim();

            lock (syncRoot)
            {
                if (screenName.Length == 0)
                {
                    common[key] = text;
                }
                else
                {
                    Dictionary<string, string> entries;

                    if (!byScreen.TryGetValue(screenName, out entries))
                    {
                        entries = new Dictionary<string, string>(StringComparer.Ordinal);
                        byScreen[screenName] = entries;
                    }

                    entries[key] = text;
                }

                translatedTexts.Add(KeyOf(text));
            }
        }

        /// <summary>번역 표와 누락 목록을 비운다(언어는 그대로).</summary>
        public static void Clear()
        {
            lock (syncRoot)
            {
                ClearEntries();
            }
        }

        /// <summary>공통 줄에서 찾아 번역한다 — <see cref="Translate(string, string, string)"/>의 화면 없는 판.</summary>
        public static string Translate(string source)
        {
            return Translate(null, source, null);
        }

        /// <summary>화면 줄 → 공통 줄 순서로 찾아 번역한다.</summary>
        public static string Translate(string screen, string source)
        {
            return Translate(screen, source, null);
        }

        /// <summary>
        /// 화면에 그릴 글자를 번역한다 — 화면 줄 → 공통 줄 → (한 줄 표기면 조각마다) 순서로 찾고, 없으면 원문을 그대로 돌려주며 누락 목록에 적는다.
        /// 꺼져 있거나 원문이 비면 원문 그대로다. <b>실제로 그릴 글자에만 부른다</b> — 부른 글자가 누락 목록에 들어간다.
        /// </summary>
        /// <param name="screen">글자가 놓인 화면(폼 클래스 이름) — 모르면 null.</param>
        /// <param name="source">소스에 적힌 영어 글자.</param>
        /// <param name="hint">누락 목록에 함께 적을 참고 값(그리드 캡션이면 컬럼명) — 번역 조회에는 쓰지 않는다.</param>
        public static string Translate(string screen, string source, string hint)
        {
            string key = KeyOf(source);

            if (key.Length == 0)
            {
                return source;
            }

            lock (syncRoot)
            {
                if (current.Length == 0)
                {
                    return source;
                }

                return TranslateCore(screen == null ? string.Empty : screen.Trim(), key, source, hint);
            }
        }

        /// <summary>
        /// 표에 없어 영어로 보인 글자 목록 — 컬럼은 <c>SOURCE</c>(원문) · <c>SCREEN</c>(처음 본 화면) · <c>HINT</c>(참고 값, 캡션이면 컬럼명)이고
        /// 원문마다 한 줄이다. 화면을 열어 본 만큼만 쌓인다.
        /// </summary>
        public static DataTable Missing()
        {
            DataTable table = new DataTable("Missing");
            table.Columns.Add("SOURCE", typeof(string));
            table.Columns.Add("SCREEN", typeof(string));
            table.Columns.Add("HINT", typeof(string));

            lock (syncRoot)
            {
                foreach (MissingEntry entry in missing)
                {
                    table.Rows.Add(entry.Source, entry.Screen, entry.Hint);
                }
            }

            return table;
        }

        /// <summary>
        /// 누락 목록을 탭 구분 글자 파일(UTF-8, 첫 줄 머리글 <c>SOURCE · SCREEN · HINT</c>)로 저장하고 줄 수를 돌려준다. 목록이 비면 파일을 만들지 않고 0 이다.
        /// 원문 안의 줄바꿈·탭은 <c>\n</c>·<c>\t</c> 두 글자로 적는다 — 번역해 표에 넣을 때 그대로 두면 <see cref="Load(DataTable, string, string, string)"/>가 되돌린다.
        /// 진입점이 프로그램을 닫을 때 부른다. 폴더가 없으면 만든다.
        /// </summary>
        public static int SaveMissing(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return 0;
            }

            StringBuilder builder = new StringBuilder();
            int count = 0;

            lock (syncRoot)
            {
                if (missing.Count == 0)
                {
                    return 0;
                }

                builder.Append("SOURCE\tSCREEN\tHINT").Append("\r\n");

                foreach (MissingEntry entry in missing)
                {
                    builder.Append(Escape(entry.Source)).Append('\t')
                            .Append(Escape(entry.Screen)).Append('\t')
                            .Append(Escape(entry.Hint)).Append("\r\n");
                    count++;
                }
            }

            string folder = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(path, builder.ToString(), new UTF8Encoding(true));
            return count;
        }

        // 한 줄 표기를 만든 자리(GridCaptionCatalog.PlainCaption)가 그 조각을 알려 둔다 — 조각이 둘 이상일 때만.
        // 정보 카드처럼 한 줄 글자만 받는 자리에서도 윗·본·아랫글씨를 각각 번역해 이을 수 있다.
        internal static void RememberParts(string joined, string topLabel, string caption, string bottomLabel)
        {
            string key = KeyOf(joined);
            int partCount = (KeyOf(topLabel).Length > 0 ? 1 : 0) + (KeyOf(caption).Length > 0 ? 1 : 0) + (KeyOf(bottomLabel).Length > 0 ? 1 : 0);

            // 조각이 하나면 한 줄 글자가 곧 그 조각이다 — 기억할 것이 없다(라벨이 그릴 때마다 지나는 길이라 여기서 끝낸다).
            if (key.Length == 0 || partCount < 2)
            {
                return;
            }

            lock (syncRoot)
            {
                if (compositions.ContainsKey(key))
                {
                    return;
                }

                List<string> parts = new List<string>(3);
                AddPart(parts, topLabel);
                AddPart(parts, caption);
                AddPart(parts, bottomLabel);
                compositions[key] = parts.ToArray();
            }
        }

        // 잠근 채로 부른다.
        private static string TranslateCore(string screen, string key, string source, string hint)
        {
            string text;

            if (TryFind(screen, key, out text))
            {
                return text;
            }

            string[] parts;

            if (compositions.TryGetValue(key, out parts))
            {
                StringBuilder joined = new StringBuilder(key.Length);

                foreach (string part in parts)
                {
                    if (joined.Length > 0)
                    {
                        joined.Append(' ');
                    }

                    joined.Append(TranslateCore(screen, part, part, hint));
                }

                string composed = joined.ToString();
                translatedTexts.Add(KeyOf(composed));
                return composed;
            }

            if (!translatedTexts.Contains(key) && HasLetter(key) && missingSources.Add(key))
            {
                missing.Add(new MissingEntry(key, screen, hint == null ? string.Empty : hint.Trim()));
            }

            return source;
        }

        private static bool TryFind(string screen, string key, out string text)
        {
            Dictionary<string, string> entries;

            if (screen.Length > 0 && byScreen.TryGetValue(screen, out entries) && entries.TryGetValue(key, out text))
            {
                return true;
            }

            return common.TryGetValue(key, out text);
        }

        private static void ClearEntries()
        {
            common.Clear();
            byScreen.Clear();
            translatedTexts.Clear();
            missing.Clear();
            missingSources.Clear();
        }

        private static void AddPart(List<string> parts, string part)
        {
            string text = KeyOf(part);

            if (text.Length > 0)
            {
                parts.Add(text);
            }
        }

        // 조회 · 등록 · 누락 목록이 함께 쓰는 키 — 앞뒤 공백을 떼고 줄바꿈을 LF 하나로 맞춘다. 디자이너의 "\r\n" 과 번역 표의 "\n"
        // (누락 파일이 줄바꿈을 \n 두 글자로 내보내고 Load 가 LF 로 되돌린다)이 같은 글자로 만나야 저장 → 번역 → 등록이 이어진다.
        private static string KeyOf(string source)
        {
            if (source == null)
            {
                return string.Empty;
            }

            string key = source.Trim();
            return key.IndexOf('\r') < 0 ? key : key.Replace("\r\n", "\n").Replace('\r', '\n');
        }

        // 숫자·기호뿐인 글자("-", "0", "…")는 번역할 것이 없다 — 누락으로 세지 않는다.
        private static bool HasLetter(string text)
        {
            foreach (char ch in text)
            {
                if (char.IsLetter(ch))
                {
                    return true;
                }
            }

            return false;
        }

        private static string Escape(string text)
        {
            return (text ?? string.Empty).Replace("\r\n", "\\n").Replace("\n", "\\n").Replace("\r", "\\n").Replace("\t", "\\t");
        }

        private static string Unescape(string text)
        {
            return text == null ? null : text.Replace("\\n", "\n").Replace("\\t", "\t");
        }

        private sealed class MissingEntry
        {
            internal MissingEntry(string source, string screen, string hint)
            {
                this.Source = source;
                this.Screen = screen;
                this.Hint = hint;
            }

            internal string Source { get; private set; }

            internal string Screen { get; private set; }

            internal string Hint { get; private set; }
        }
    }
}
