using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>
    /// 조회조건 키 — 어느 표의 컬럼도 아닌, 화면이 전문에 싣는 키(검색 키워드 · 쉼표로 이은 Id 목록)를 모아 둔다.
    /// 화면은 글자 대신 <c>Schemas.Criteria.Keyword.Name</c> 처럼 이 필드의 이름을 <c>RequestParameters.Of</c> 에 넘기고,
    /// <c>.Server.cs</c> 는 그 이름을 <c>Take("KEYWORD")</c> 로 글자 그대로 꺼낸다. 응답 컬럼과 겹치는 이름은 두지 않는다 —
    /// 컬럼이면 그 표의 엔티티에 선언한다. 회사 적용 중 필요한 키를 한 줄씩 더한다(2026-10-28).
    /// </summary>
    public sealed class CriteriaEntity : EntityDefinition
    {
        public readonly Field Keyword = Field("KEYWORD").Caption("Keyword");
        public readonly Field LotIds = Field("LOT_IDS").Caption("Lot Ids");
        public readonly Field SlotNos = Field("SLOT_NOS").Caption("Slots");
        public readonly Field WfIds = Field("WF_IDS").Caption("Wafer Ids");

        public CriteriaEntity()
        {
            this.Seal();
        }
    }
}
