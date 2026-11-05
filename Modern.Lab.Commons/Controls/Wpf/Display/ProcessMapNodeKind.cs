namespace Modern.Lab.Controls.Wpf.Display
{
    /// <summary>공정 맵 노드의 모양 — 같은 데이터 칸을 종류마다 다른 크기와 줄 구성으로 그린다.</summary>
    public enum ProcessMapNodeKind
    {
        /// <summary>단계 노드 — 이름 · 건수와 단위 · 짧은 요약 한 줄. 입구 단계나 Done 처럼 장비가 없는 자리에 쓴다.</summary>
        Flow = 0,

        /// <summary>장비 노드 — 램프 · 장비 Id · 상태 · 건수 · 요약 · 내부 칸 · 상세 줄을 담는 넓은 노드.</summary>
        Equipment,

        /// <summary>가지 노드 — 점선 테두리의 작은 노드. 주 흐름 밖의 갈래(반송 · 교체 · 폐기 후보)에 쓴다.</summary>
        Branch
    }
}
