namespace Modern.Lab.Hosting
{
    /// <summary>
    /// 규칙 하나를 선택 행에 대조한 결과 — 규칙 안내 창의 한 줄이다(<see cref="ActionRule.Explain"/>).
    /// 값 규칙은 <see cref="Field"/> · <see cref="Required"/> · <see cref="Current"/> 가 채워지고,
    /// 판정 메서드 규칙은 <see cref="Required"/> 에 문장만 있다.
    /// </summary>
    public sealed class ActionRuleCheck
    {
        private readonly string field;
        private readonly string required;
        private readonly string current;
        private readonly bool hasRow;
        private readonly bool passed;
        private readonly string text;

        public ActionRuleCheck(string field, string required, string current, bool hasRow, bool passed, string text)
        {
            this.field = field ?? string.Empty;
            this.required = required ?? string.Empty;
            this.current = current ?? string.Empty;
            this.hasRow = hasRow;
            this.passed = passed;
            this.text = text ?? string.Empty;
        }

        /// <summary>엔티티와 필드 캡션(<c>Lot · Lot Proc Status</c>) — 판정 메서드 규칙은 빈 값.</summary>
        public string Field
        {
            get { return this.field; }
        }

        /// <summary>요구 값(<c>LoggedOut</c> · <c>Full or Partial</c> · <c>not Run</c> · <c>empty</c>) — 판정 메서드 규칙은 사유 문장.</summary>
        public string Required
        {
            get { return this.required; }
        }

        /// <summary>선택 행의 지금 값 — 컬럼이 없으면 <c>missing</c>, 비었으면 <c>(empty)</c>, 행이 없으면 빈 값.</summary>
        public string Current
        {
            get { return this.current; }
        }

        /// <summary>대조할 행이 있었는가 — 없으면 <see cref="Passed"/> 는 뜻이 없다.</summary>
        public bool HasRow
        {
            get { return this.hasRow; }
        }

        /// <summary>행이 이 규칙을 통과했는가.</summary>
        public bool Passed
        {
            get { return this.passed; }
        }

        /// <summary>툴팁에 쓰는 것과 같은 전체 문장.</summary>
        public string Text
        {
            get { return this.text; }
        }
    }
}
