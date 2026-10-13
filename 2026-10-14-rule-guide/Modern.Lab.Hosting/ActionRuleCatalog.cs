using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Modern.Lab.Hosting
{
    /// <summary>
    /// 화면 클래스에서 규칙 목록을 모은다 — <c>*.Rules.cs</c> 의 <c>static ActionRule[] XxxRules</c> 필드를
    /// 선언 순서대로 읽는다(2026-10-13). 화면은 아무것도 넘기지 않고, 새 화면도 같은 이름 규칙으로
    /// 적기만 하면 안내 창에 나온다.
    ///
    /// - <c>CommonRules</c> 는 공통 조건으로 맨 앞에 둔다(비어 있으면 뺀다).
    /// - 다른 목록에 통째로 들어간 목록(Exchange · Edit 에 합쳐지는 <c>SourceRules</c>)은 도우미라 뺀다 —
    ///   같은 규칙 개체가 들어 있는지로 알아본다.
    /// - 이름이 <c>Rules</c> 로 끝나지 않는 필드는 읽지 않는다.
    /// </summary>
    public static class ActionRuleCatalog
    {
        private const string Suffix = "Rules";
        private const string CommonFieldName = "CommonRules";

        /// <summary>공통 조건 그룹의 표시 이름.</summary>
        public const string CommonName = "All actions";

        /// <summary><paramref name="formType"/> 의 규칙 목록 — 공통 조건이 먼저, 나머지는 선언 순서다.</summary>
        public static ActionRuleGroup[] Collect(Type formType)
        {
            if (formType == null)
            {
                return new ActionRuleGroup[0];
            }

            List<FieldInfo> fields = new List<FieldInfo>();

            foreach (FieldInfo field in formType.GetFields(
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy))
            {
                if (field.FieldType == typeof(ActionRule[]) && field.Name.EndsWith(Suffix, StringComparison.Ordinal)
                        && field.Name.Length > Suffix.Length)
                {
                    fields.Add(field);
                }
            }

            fields.Sort(CompareDeclaration);

            List<ActionRuleGroup> groups = new List<ActionRuleGroup>();
            ActionRuleGroup common = null;

            foreach (FieldInfo field in fields)
            {
                ActionRule[] rules = field.GetValue(null) as ActionRule[] ?? new ActionRule[0];
                string key = field.Name.Substring(0, field.Name.Length - Suffix.Length);

                if (field.Name == CommonFieldName)
                {
                    common = new ActionRuleGroup(key, CommonName, rules, true);
                    continue;
                }

                groups.Add(new ActionRuleGroup(key, Humanize(key), rules, false));
            }

            List<ActionRuleGroup> visible = new List<ActionRuleGroup>();

            if (common != null && common.Rules.Length > 0)
            {
                visible.Add(common);
            }

            foreach (ActionRuleGroup group in groups)
            {
                if (!IsHelper(group, groups))
                {
                    visible.Add(group);
                }
            }

            return visible.ToArray();
        }

        /// <summary>키를 띄어 쓴 이름 — <c>ChangeSpec</c> → <c>Change Spec</c>, <c>NotOnHold</c> → <c>Not On Hold</c>.</summary>
        public static string Humanize(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            StringBuilder text = new StringBuilder(key.Length + 4);

            for (int index = 0; index < key.Length; index++)
            {
                char current = key[index];

                if (index > 0 && char.IsUpper(current) && !char.IsUpper(key[index - 1]))
                {
                    text.Append(' ');
                }

                text.Append(current);
            }

            return text.ToString();
        }

        /// <summary>버튼 글자와 키를 맞출 때 쓰는 정규화 — 글자와 숫자만 남기고 소문자로.</summary>
        public static string NormalizeLabel(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            StringBuilder normalized = new StringBuilder(text.Length);

            foreach (char current in text)
            {
                if (char.IsLetterOrDigit(current))
                {
                    normalized.Append(char.ToLowerInvariant(current));
                }
            }

            return normalized.ToString();
        }

        // 선언 순서 — 같은 클래스(partial 포함) 안에서는 메타데이터 토큰이 적힌 차례를 따른다.
        private static int CompareDeclaration(FieldInfo first, FieldInfo second)
        {
            return first.MetadataToken.CompareTo(second.MetadataToken);
        }

        private static bool IsHelper(ActionRuleGroup candidate, List<ActionRuleGroup> groups)
        {
            if (candidate.Rules.Length == 0)
            {
                return false;
            }

            foreach (ActionRuleGroup other in groups)
            {
                if (!ReferenceEquals(other, candidate) && other.Rules.Length > candidate.Rules.Length
                        && ContainsAll(other.Rules, candidate.Rules))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsAll(ActionRule[] container, ActionRule[] part)
        {
            foreach (ActionRule rule in part)
            {
                if (Array.IndexOf(container, rule) < 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
