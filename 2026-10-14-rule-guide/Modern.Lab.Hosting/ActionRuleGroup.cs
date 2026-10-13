namespace Modern.Lab.Hosting
{
    /// <summary>
    /// 액션 하나의 규칙 목록 — 화면의 <c>static ActionRule[] XxxRules</c> 필드 하나다
    /// (<see cref="ActionRuleCatalog.Collect"/>). <see cref="Name"/> 은 안내 창에 보이는 액션 이름이고,
    /// <see cref="Key"/> 는 필드 이름에서 <c>Rules</c> 를 뗀 것(<c>ChangeSpec</c>)이다.
    /// </summary>
    public sealed class ActionRuleGroup
    {
        private readonly string key;
        private readonly ActionRule[] rules;
        private readonly bool common;
        private string name;

        public ActionRuleGroup(string key, string name, ActionRule[] rules, bool common)
        {
            this.key = key ?? string.Empty;
            this.name = name ?? string.Empty;
            this.rules = rules ?? new ActionRule[0];
            this.common = common;
        }

        /// <summary>필드 이름에서 <c>Rules</c> 를 뗀 키 — 버튼 글자와 맞출 때 쓴다.</summary>
        public string Key
        {
            get { return this.key; }
        }

        /// <summary>안내 창에 보이는 액션 이름 — 버튼·메뉴 글자가 있으면 그것, 없으면 키를 띄어 쓴 것.</summary>
        public string Name
        {
            get { return this.name; }
            set { this.name = value ?? string.Empty; }
        }

        /// <summary>규칙 목록 — 선언한 순서 그대로.</summary>
        public ActionRule[] Rules
        {
            get { return this.rules; }
        }

        /// <summary>모든 액션에 먼저 적용되는 공통 조건(<c>CommonRules</c>)인가.</summary>
        public bool IsCommon
        {
            get { return this.common; }
        }
    }
}
