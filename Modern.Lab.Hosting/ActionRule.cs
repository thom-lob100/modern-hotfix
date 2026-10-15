using System;
using System.Collections.Generic;
using System.Data;

namespace Modern.Lab.Hosting
{
    /// <summary>
    /// 버튼·메뉴를 열지 말지 정하는 규칙 하나 — <b>판정 + 꺼진 이유</b>다.
    ///
    /// 조건을 <c>&amp;&amp;</c> 로 이어 붙이면 셋만 넘어가도 눈으로 파싱해야 하고, 하나를 더 넣을 때
    /// 어디에 끼울지부터 봐야 한다. 더 큰 문제는 <b>버튼이 왜 꺼졌는지 화면이 말해 주지 않는다</b>는
    /// 것이다 — 회사에서 고치는 사람이 조건을 전부 기억하고 있어야 한다(2026-09-20 사용자 지적).
    ///
    /// 그래서 규칙을 **목록**으로 적고 각 규칙이 자기 사유를 들고 있게 한다. 목록은 언제나 AND —
    /// 하나라도 막으면 닫히고, <see cref="Check(ActionRule[], DataRow)"/> 가 <b>처음 막은 규칙의
    /// 사유</b>를 돌려준다. 그 문장을 그대로 <c>ToolTipText</c> 에 세우면 마우스만 올려도 이유가 보인다.
    ///
    /// <c>OR</c> 문법은 일부러 없다 — 목록에 OR 를 허용하는 순간 다시 눈으로 파싱해야 한다.
    /// 같은 필드 안의 OR 는 <see cref="Value"/> 에 허용 값을 여러 개 주고, 다른 필드끼리의 OR 가
    /// 필요한 자리는 판정 메서드 하나로 감싼다.
    ///
    /// 규칙 안내 창(꺼진 버튼·메뉴 항목을 누르면 열린다)은 화면의 <c>static ActionRule[] XxxRules</c> 필드를 읽어
    /// 액션마다 조건 표를 보인다(2026-10-13). <see cref="Value"/> · <see cref="NotValue"/> ·
    /// <see cref="Empty"/> 로 적은 규칙은 필드 · 요구 값 · 지금 값이 표의 열로 나오고,
    /// <see cref="Is"/> · <see cref="Not"/> 로 적은 규칙은 문장과 통과 여부만 나온다.
    /// </summary>
    /// <example><code>
    /// private static readonly ActionRule[] ExchangeRules =
    /// {
    ///     ActionRule.Value("Durable", Schemas.Durable.DurableTyp, Schemas.Durable.DurableTyp.Foup),
    ///     ActionRule.Value("Durable", Schemas.Durable.WfLoadStatCd,
    ///             Schemas.Durable.WfLoadStatCd.Full, Schemas.Durable.WfLoadStatCd.Partial),
    /// };
    ///
    /// string reason = ActionRule.Check(ExchangeRules, row);
    /// this.btnExchange.Enabled = reason == null;
    /// this.btnExchange.ToolTipText = reason;
    /// </code></example>
    public sealed class ActionRule
    {
        private readonly Func<DataRow, DataRow, bool> test;
        private readonly string reason;
        private readonly Func<DataRow, string> describe;
        private readonly string entity;
        private readonly Modern.Lab.WinForms.Controls.Data.Field field;
        private readonly string requirement;

        private ActionRule(Func<DataRow, DataRow, bool> test, string reason)
            : this(test, reason, null, null, null, null)
        {
        }

        private ActionRule(Func<DataRow, DataRow, bool> test, string reason, Func<DataRow, string> describe,
                string entity, Modern.Lab.WinForms.Controls.Data.Field field, string requirement)
        {
            if (test == null)
            {
                throw new ArgumentNullException("test");
            }

            this.test = test;
            this.reason = reason ?? string.Empty;
            this.describe = describe;
            this.entity = entity ?? string.Empty;
            this.field = field;
            this.requirement = requirement ?? string.Empty;
        }

        /// <summary>
        /// 필드 값이 <paramref name="allowed"/> 중 하나여야 연다. 사유는 행에서 만든다 —
        /// <c>엔티티 · 캡션: 지금 값 → needs 허용 값</c>(예: <c>Lot · Lot Proc Status: LoggedIn → needs LoggedOut</c>).
        /// 캡션은 그리드 머리글과 같은 캡션 사전에서 읽고, 응답에 컬럼이 없으면 지금 값이 <c>missing</c> 이다(막는다).
        /// </summary>
        public static ActionRule Value(string entity, Modern.Lab.WinForms.Controls.Data.Field field,
                params Modern.Lab.WinForms.Controls.Data.FieldValue[] allowed)
        {
            string codes = JoinCodes(allowed);
            return new ActionRule(
                    delegate(DataRow row, DataRow second) { return HasValue(row, field) && IsAny(row, field, allowed); },
                    string.Empty,
                    delegate(DataRow row) { return Describe(entity, field, row) + " → needs " + codes; },
                    entity, field, codes);
        }

        /// <summary>필드 값이 <paramref name="blocked"/> 중 어느 것도 아니어야 연다 — 사유는 <c>… → must not be 값</c>
        /// (여러 값이면 <c>A, B or C</c>). 컬럼이 없으면 막는다. 값 여러 개는 "진행 중인 작업이 없어야 한다" 처럼
        /// 막는 상태가 여럿인 조건 하나를 한 줄로 적기 위한 것이다(2026-10-16).</summary>
        public static ActionRule NotValue(string entity, Modern.Lab.WinForms.Controls.Data.Field field,
                params Modern.Lab.WinForms.Controls.Data.FieldValue[] blocked)
        {
            string codes = JoinCodes(blocked);
            return new ActionRule(
                    delegate(DataRow row, DataRow second) { return HasColumn(row, field) && !IsAny(row, field, blocked); },
                    string.Empty,
                    delegate(DataRow row) { return Describe(entity, field, row) + " → must not be " + codes; },
                    entity, field, "not " + codes);
        }

        /// <summary>필드 값이 비어 있어야(NULL 또는 공백뿐) 연다 — 사유는 <c>… → needs empty</c>. 컬럼이 없으면 막는다.</summary>
        public static ActionRule Empty(string entity, Modern.Lab.WinForms.Controls.Data.Field field)
        {
            return new ActionRule(
                    delegate(DataRow row, DataRow second) { return HasColumn(row, field) && IsBlank(row, field); },
                    string.Empty,
                    delegate(DataRow row) { return Describe(entity, field, row) + " → needs empty"; },
                    entity, field, "empty");
        }

        /// <summary>필드에 값이 있어야 연다(<see cref="Empty"/> 의 반대) — 사유는 <c>… → needs a value</c>. 컬럼이 없으면 막는다.
        /// "선택이 있어야 한다" 류의 조건을 문장 규칙 대신 필드 · 요구 값 · 지금 값으로 보이기 위한 것이다(2026-10-16).</summary>
        public static ActionRule NotEmpty(string entity, Modern.Lab.WinForms.Controls.Data.Field field)
        {
            return new ActionRule(
                    delegate(DataRow row, DataRow second) { return HasColumn(row, field) && !IsBlank(row, field); },
                    string.Empty,
                    delegate(DataRow row) { return Describe(entity, field, row) + " → needs a value"; },
                    entity, field, "not empty");
        }

        /// <summary>값 규칙이 보는 응답 컬럼 이름 — 판정 메서드 규칙(<see cref="Is"/> · <see cref="Not"/>)은 <c>null</c>.</summary>
        public string FieldName
        {
            get { return this.field == null ? null : this.field.Name; }
        }

        /// <summary>
        /// 규칙 하나를 행에 대조한 결과 — 규칙 안내 창의 한 줄이다. 값 규칙은 필드 캡션 · 요구 값 ·
        /// 지금 값이 채워지고, 판정 메서드 규칙은 문장만 있다. <paramref name="row"/> 가 <c>null</c> 이면
        /// 통과 여부가 없다(<see cref="ActionRuleCheck.HasRow"/>).
        /// </summary>
        public ActionRuleCheck Explain(DataRow row)
        {
            bool hasRow = row != null;
            bool passed = hasRow && this.test(row, null);
            string text = hasRow && this.describe != null ? this.describe(row) : this.reason;

            if (this.field == null)
            {
                return new ActionRuleCheck(string.Empty, this.reason, string.Empty, hasRow, passed, text);
            }

            string caption = this.entity.Length == 0 ? Caption(this.field) : this.entity + " · " + Caption(this.field);
            string current = hasRow ? CurrentText(row, this.field) : string.Empty;
            return new ActionRuleCheck(caption, this.requirement, current, hasRow, passed, text);
        }

        private static bool HasColumn(DataRow row, Modern.Lab.WinForms.Controls.Data.Field field)
        {
            return row != null && row.Table != null && row.Table.Columns.Contains(field.Name);
        }

        private static bool HasValue(DataRow row, Modern.Lab.WinForms.Controls.Data.Field field)
        {
            return HasColumn(row, field) && field.Text(row).Length > 0;
        }

        private static bool IsNull(DataRow row, Modern.Lab.WinForms.Controls.Data.Field field)
        {
            object value = row[field.Name];
            return value == null || value == DBNull.Value;
        }

        private static bool IsBlank(DataRow row, Modern.Lab.WinForms.Controls.Data.Field field)
        {
            return IsNull(row, field) || field.Text(row).Trim().Length == 0;
        }

        private static bool IsAny(DataRow row, Modern.Lab.WinForms.Controls.Data.Field field,
                Modern.Lab.WinForms.Controls.Data.FieldValue[] allowed)
        {
            foreach (Modern.Lab.WinForms.Controls.Data.FieldValue value in allowed)
            {
                if (field.Is(row, value))
                {
                    return true;
                }
            }

            return false;
        }

        private static string Caption(Modern.Lab.WinForms.Controls.Data.Field field)
        {
            Modern.Lab.Controls.Wpf.Data.ModernDataGridColumn column =
                    new Modern.Lab.Controls.Wpf.Data.ModernDataGridColumn(field.Name);
            return string.IsNullOrEmpty(field.CaptionSetting)
                    ? Modern.Lab.Controls.Wpf.Data.GridCaptionCatalog.PlainCaption(
                            column.TopLabel, column.HeaderText, column.BottomLabel)
                    : Modern.Lab.Controls.Wpf.Data.GridCaptionCatalog.PlainCaption(
                            field.TopLabelSetting, field.CaptionSetting, field.BottomLabelSetting);
        }

        private static string CurrentText(DataRow row, Modern.Lab.WinForms.Controls.Data.Field field)
        {
            return !HasColumn(row, field) ? "missing"
                    : IsNull(row, field) || field.Text(row).Length == 0 ? "(empty)"
                    : field.Text(row);
        }

        private static string Describe(string entity, Modern.Lab.WinForms.Controls.Data.Field field, DataRow row)
        {
            return entity + " · " + Caption(field) + ": " + CurrentText(row, field);
        }

        private static string JoinCodes(Modern.Lab.WinForms.Controls.Data.FieldValue[] values)
        {
            if (values == null || values.Length == 0)
            {
                return "(none)";
            }

            if (values.Length == 1)
            {
                return values[0].Value;
            }

            string[] codes = new string[values.Length - 1];

            for (int index = 0; index < codes.Length; index++)
            {
                codes[index] = values[index].Value;
            }

            return string.Join(", ", codes) + " or " + values[values.Length - 1].Value;
        }

        /// <summary>선택한 행이 없을 때의 기본 사유 — 화면마다 바꾸려면
        /// <see cref="Check(ActionRule[], DataRow, string)"/> 를 쓴다.</summary>
        public const string NoSelectionReason = "Select a row first.";

        /// <summary><paramref name="test"/> 가 참이어야 연다.</summary>
        public static ActionRule Is(Func<DataRow, bool> test, string reason)
        {
            return new ActionRule(delegate(DataRow row, DataRow second) { return test(row); }, reason);
        }

        /// <summary><paramref name="test"/> 가 거짓이어야 연다.</summary>
        public static ActionRule Not(Func<DataRow, bool> test, string reason)
        {
            return new ActionRule(delegate(DataRow row, DataRow second) { return !test(row); }, reason);
        }

        /// <summary>둘째 행까지 보는 규칙 — 장비와 포트처럼 한 액션이 두 행에 걸릴 때 쓴다.</summary>
        public static ActionRule Pair(Func<DataRow, DataRow, bool> test, string reason)
        {
            return new ActionRule(test, reason);
        }

        /// <summary>어떤 행에서도 열지 않는다 — 기능을 잠시 닫아 둘 때.</summary>
        public static ActionRule Never(string reason)
        {
            return new ActionRule(delegate(DataRow row, DataRow second) { return false; }, reason);
        }

        /// <summary>처음 막은 규칙의 사유 — 전부 통과하면 <c>null</c> 이다.
        /// 행이 없으면 <see cref="NoSelectionReason"/>.</summary>
        public static string Check(ActionRule[] rules, DataRow row)
        {
            return Check(rules, row, NoSelectionReason);
        }

        /// <summary>처음 막은 규칙의 사유 — 전부 통과하면 <c>null</c> 이다.
        /// 행이 없으면 <paramref name="noSelectionReason"/>.</summary>
        public static string Check(ActionRule[] rules, DataRow row, string noSelectionReason)
        {
            return Check(rules, row, null, noSelectionReason);
        }

        /// <summary>둘째 행까지 넘기는 판.</summary>
        public static string Check(ActionRule[] rules, DataRow row, DataRow second, string noSelectionReason)
        {
            if (row == null)
            {
                return noSelectionReason;
            }

            if (rules == null)
            {
                return null;
            }

            foreach (ActionRule rule in rules)
            {
                if (rule != null && !rule.test(row, second))
                {
                    return rule.describe != null ? rule.describe(row) : rule.reason;
                }
            }

            return null;
        }

        /// <summary>행 없이 도는 액션(Create 처럼 선택을 보지 않는 것)의 판정 —
        /// 행 규칙은 적용하지 않고 <paramref name="gates"/> 만 본다.</summary>
        public static string Check(IEnumerable<KeyValuePair<bool, string>> gates)
        {
            if (gates == null)
            {
                return null;
            }

            foreach (KeyValuePair<bool, string> gate in gates)
            {
                if (!gate.Key)
                {
                    return gate.Value;
                }
            }

            return null;
        }

        /// <summary>열려 있는가 — <see cref="Check(ActionRule[], DataRow)"/> 의 사유가 없으면 참.</summary>
        public static bool IsOpen(ActionRule[] rules, DataRow row)
        {
            return Check(rules, row) == null;
        }
    }
}
