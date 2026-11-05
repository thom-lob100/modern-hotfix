using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>
    /// 모든 엔티티의 바탕 — 칸(<see cref="Field"/> 필드)은 두지 않고, 여러 엔티티가 함께 쓰는 칸의 사전(<c>TableEntity.*Columns.cs</c>)만 품는다(2026-11-05).
    /// 엔티티 파일 하나가 응답 표 하나이고, 그 표의 칸을 위에서 아래로 한 줄씩 적는다. 두 엔티티 이상이 쓰는 칸은 사전에서
    /// <c>LotColumns.LotId()</c> 로 가져오고, 한 엔티티만 쓰는 칸은 그 엔티티에 <c>Field("MODEL_ID")</c> 로 바로 적는다.
    /// 캡션 · 배지가 다르면 그 줄에 이어 적고(<c>CommonColumns.Priority().Caption("Priority")</c>), 칸을 빼려면 그 줄을 적지 않는다.
    /// 같은 칸 이름이라도 값 목록이나 뜻이 다르면 그 엔티티에 바로 적는다(Durable 의 <c>MES_PROC_STAT_CD</c> · MonitorDurable 의 <c>LOC_NM</c> ·
    /// SubCategory 의 <c>DURABLE_TYP</c> · Criteria 의 조회 키).
    ///
    /// 사전의 주인은 칸 이름 앞머리(LOT_ · WF_ · DURABLE_ · EQP_ · PORT_ · TRANS_ · REQ_ · MON_)가 정한다. 앞머리가 없으면 그 칸을 쓰는
    /// 업무(Monitor 엔티티는 보는 대상)가 하나일 때 그 업무, 여럿일 때 Common 이다. 같은 이름에 값 목록이 다른 칸(<c>MES_STAT_TYP</c>)은
    /// 값의 주인마다 따로 둔다. 값 목록은 같은 주인의 <c>*Fields</c>(<see cref="LotFields"/> 등)에 있고, 캡션은 사전과 엔티티 줄에만 둔다.
    /// 사전은 부를 때마다 새 칸을 만든다 — 칸을 정적 필드에 담아 두면 두 번째 엔티티의 <c>Seal</c> 이 "이미 잠겼다"로 막는다.
    /// </summary>
    public abstract partial class TableEntity : EntityDefinition
    {
    }
}
