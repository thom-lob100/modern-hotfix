namespace Modern.Lab.Controls.Wpf.Display
{
    /// <summary>공정 맵 연결선의 모양 — 굵기는 모두 같고 물량을 나타내지 않는다.</summary>
    public enum ProcessMapLinkStyle
    {
        /// <summary>주 흐름 — 실선 + 화살표.</summary>
        Main = 0,

        /// <summary>가지 — 점선 + 화살표. 주 흐름 밖의 갈래.</summary>
        Branch,

        /// <summary>되돌림 — 노드 위를 지나는 점선 곡선 + 화살표와 이름표. 평소 회색이고 톤이 있으면 그 색이 된다.</summary>
        Rework
    }
}
