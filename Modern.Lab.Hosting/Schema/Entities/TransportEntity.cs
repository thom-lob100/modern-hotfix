using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Transport History 의 Transport 목록(TRANS_MAS) — 위 네 칸은 반송 화면들이 함께 쓰는 칸이다. 키는 Durable Id.
    /// 마지막 처리(<c>LAST_EVENT_CD</c> · <c>LAST_TIMEKEY</c>)는 이 표에만 있다. 홈의 컬럼은 구조를 보이기 위한 것이라 회사에서 실제 컬럼으로 바꾼다(2026-10-26).
    /// 아래 칸은 Durable Change Destination 창의 반송 목록(<c>GetManualTransportList</c>, 홈 이름)이 읽는 회사 반송 표 칸이다(2026-11-12 사용자) —
    /// 이동 상태 <c>MOVE_STAT_CD</c>(Request · Requested · Scheduled · Moving · Changed 만 고른다), 마지막 반송 이벤트 <c>LAST_EVENT_DESC</c> · 시각 <c>LAST_EVENT_TM</c>,
    /// 출발 장비 · 포트 <c>START_GOAL_VAL</c> · <c>START_LOC_CD</c>, 목적 장비 · 포트 <c>END_GOAL_VAL</c> · <c>END_LOC_CD</c>(반송 표는 VAL 이 장비, LOC 가 포트다),
    /// 그리고 Durable 을 조인해 받는 <c>DURABLE_STAT_CD</c>(Scrapped 는 뺀다) · <c>LOC_NM</c>.</summary>
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
        public readonly Field StartGoalVal = Field("START_GOAL_VAL").Caption("From Eqp Id");
        public readonly Field StartLocCd = Field("START_LOC_CD").Caption("From Port");
        public readonly Field EndGoalVal = Field("END_GOAL_VAL").Caption("To Eqp Id");
        public readonly Field EndLocCd = Field("END_LOC_CD").Caption("To Port");
        public readonly Field LastEventTm = Field("LAST_EVENT_TM").Caption("Last Event Time");
        public readonly DurableFields.DurableStatCd DurableStatCd = DurableColumns.DurableStatCd();
        public readonly CommonFields.LocNm LocNm = CommonColumns.LocNm();

        public TransportEntity()
        {
            this.KeyFields = Fields(this.DurableId);
            this.ComboDisplayFields = Fields(this.DurableId, this.DurableTyp);
            this.ValueField = this.DurableId;
            this.Seal();
        }
    }
}
