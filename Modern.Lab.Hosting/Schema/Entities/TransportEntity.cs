using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Transport History 의 Transport 목록(TRANS_MAS) — 위 네 칸은 반송 화면들이 함께 쓰는 칸이다. 키는 Durable Id.
    /// 마지막 처리(<c>LAST_EVENT_CD</c> · <c>LAST_TIMEKEY</c>)는 이 표에만 있다. 홈의 컬럼은 구조를 보이기 위한 것이라 회사에서 실제 컬럼으로 바꾼다(2026-10-26).
    /// 아래 여섯 칸은 Durable Change Destination 창의 반송 요청 목록(<c>GetManualTransportList</c>, 홈 이름)이 읽는다 — 이동 상태 <c>MOVE_STAT_CD</c>
    /// (Request · Requested · Scheduled 만 고른다), 요청 종류 <c>LAST_EVENT_DESC</c>(EISMHS_LOAD_REQ = Manual In · EISMHS_UNLOAD_REQ = Manual Out),
    /// 출발 · 목적 장비와 포트(2026-11-11 사용자). <c>MOVE_STAT_CD</c> 외의 이름은 홈 이름이라 회사 응답 컬럼으로 바꾼다.</summary>
    public sealed class TransportEntity : TableEntity
    {
        public readonly Field DurableId = DurableColumns.DurableId();
        public readonly DurableFields.DurableTyp DurableTyp = DurableColumns.DurableTyp().Badge();
        public readonly Field LotId = LotColumns.LotId();
        public readonly Field Location = CommonColumns.Location();
        public readonly CommonFields.LastEventCd LastEventCd = CommonColumns.LastEventCd();
        public readonly Field LastTimeKey = Field("LAST_TIMEKEY").Caption("Last Timekey");
        public readonly TransportFields.TransMoveStatCd MoveStatCd = Field<TransportFields.TransMoveStatCd>("MOVE_STAT_CD").Caption("Move Status");
        public readonly TransportFields.TransLastEventDesc LastEventDesc = Field<TransportFields.TransLastEventDesc>("LAST_EVENT_DESC").Caption("Transport Event");
        public readonly Field FromEqpId = Field("FROM_EQP_ID");
        public readonly Field FromPortNm = Field("FROM_PORT_NM").Caption("From Port");
        public readonly Field ToEqpId = Field("TO_EQP_ID");
        public readonly Field ToPortNm = Field("TO_PORT_NM").Caption("To Port");

        public TransportEntity()
        {
            this.KeyFields = Fields(this.DurableId);
            this.ComboDisplayFields = Fields(this.DurableId, this.DurableTyp);
            this.ValueField = this.DurableId;
            this.Seal();
        }
    }
}
