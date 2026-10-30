using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>
    /// 조회조건 키 — 행에서 읽는 컬럼이 아니라 화면이 적어 넣는 값(검색 키워드 · 쉼표로 이은 Id 목록 · 작업 비고)의 키를 모아 둔다.
    /// 화면은 <c>RequestParameters.Of(Schemas.Criteria.Keyword, 값)</c> 으로 넣고, <c>.Server.cs</c> 는
    /// <c>parameters.Value(Schemas.Criteria.Keyword)</c> 로 꺼내 전문 키 옆에 적는다. 선택한 행의 컬럼이면 여기가 아니라 그 표의
    /// 엔티티로 넣는다. 같은 이름의 컬럼이 다른 표에 있어도 된다 — <c>Description</c> 은 다이얼로그가 적는 비고라 여기 있고,
    /// 회사 표의 비고 컬럼 이름과 무관하다(2026-10-28). 회사 적용 중 필요한 키를 한 줄씩 더한다.
    /// </summary>
    public sealed class CriteriaEntity : EntityDefinition
    {
        public readonly Field Keyword = Field("KEYWORD").Caption("Keyword");
        public readonly Field LotIds = Field("LOT_IDS").Caption("Lot Ids");
        public readonly Field SlotNos = Field("SLOT_NOS").Caption("Slots");
        public readonly Field WfIds = Field("WF_IDS").Caption("Wafer Ids");
        public readonly Field Description = Field("DESCRIPTION").Caption("Description");
        public readonly Field ReqFromDt = Field("REQ_FROM_DT").Caption("Request From");
        public readonly Field ReqToDt = Field("REQ_TO_DT").Caption("Request To");
        public readonly Field AcceptFromDt = Field("ACCEPT_FROM_DT").Caption("Accept From");
        public readonly Field AcceptToDt = Field("ACCEPT_TO_DT").Caption("Accept To");

        public CriteriaEntity()
        {
            this.Seal();
        }
    }
}
