namespace Modern.Lab.Theming
{
    /// <summary>
    /// 의미 상태의 종류 — <see cref="SemanticColors"/>의 5색에 대응하는 enum.
    /// <c>ModernStatusBadge.Semantic</c>처럼 디자이너/코드에서 의미색을 헥사 없이
    /// 지정할 때 쓴다. None은 "의미색을 쓰지 않음"(컨트롤별 기본 동작으로 폴백)이다.
    /// </summary>
    public enum SemanticKind
    {
        /// <summary>의미색 미사용 — 컨트롤의 기본 색 결정을 따른다.</summary>
        None = 0,

        /// <summary>정상·완료·가능 (<see cref="SemanticColors.Success"/>).</summary>
        Success,

        /// <summary>주의·개입 중·대기 (<see cref="SemanticColors.Warning"/>).</summary>
        Warning,

        /// <summary>불가·실패·위험 (<see cref="SemanticColors.Error"/>).</summary>
        Error,

        /// <summary>진행·정보 (<see cref="SemanticColors.Info"/>).</summary>
        Info,

        /// <summary>무상태·해당없음 (<see cref="SemanticColors.Neutral"/>).</summary>
        Neutral
    }
}
