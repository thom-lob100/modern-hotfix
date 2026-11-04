using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Lab Monitor 의 랏 한 줄(<c>GetMonitorLotList</c>, 홈 이름) — 기본 필드는 <see cref="LotBase"/>. 단계 <see cref="MonStage"/> · 상태 배지 <see cref="MonStatCd"/> ·
    /// 단계에 들어온 시각 <see cref="StageTm"/>(화면은 경과시간만 센다) · 큐 순위 <see cref="Priority"/> · 의뢰서 수 <see cref="ReqCnt"/>(둘 이상이면 "+n") ·
    /// 가지 표시 <see cref="ReworkYn"/>(지금 재작업 중) · <see cref="ReworkCnt"/>(배지 숫자) · <see cref="ScrapDueYn"/>(Done 뒤 14일 지난 후보)는 모두 서버 칸이다.
    /// Hold 는 <see cref="LotBase.LotHoldStatCd"/>(OnHold · NotOnHold)로 받는다. 의뢰서만 있고 랏이 없는 줄은 <c>LOT_ID</c> 가 비어 있다(2026-11-04).</summary>
    public class MonitorLotEntity : LotBase
    {
        public readonly MonitorStageField MonStage = Field<MonitorStageField>("MON_STAGE").Caption("Stage").Badge();
        public readonly MonitorStatCdField MonStatCd = Field<MonitorStatCdField>("MON_STAT_CD").Caption("State").Badge();
        public readonly Field Qty = Field("QTY").Caption("Qty");
        public readonly Field ReqCnt = Field("REQ_CNT").Caption("Requests");
        public readonly Field StageTm = Field("STAGE_TM").Caption("Since");
        public readonly Field EqpId = Field("EQP_ID").Caption("Eqp Id");
        public readonly Field ChildEqpId = Field("CHILD_EQP_ID").Caption("Child Eqp");
        public readonly Field DurableId = Field("DURABLE_ID").Caption("Durable Id");
        public readonly DurableTypField DurableTyp = Field<DurableTypField>("DURABLE_TYP").Caption("Durable Type").Badge();
        public readonly Field SendFac = Field("SEND_FAC").Caption("From Fab");
        public readonly Field Priority = Field("PRIORITY").Caption("Prio");
        public readonly YnField ReworkYn = Field<YnField>("REWORK_YN").Caption("Rework");
        public readonly Field ReworkCnt = Field("REWORK_CNT").Caption("Rework Count");
        public readonly YnField ScrapDueYn = Field<YnField>("SCRAP_DUE_YN").Caption("Scrap Due");

        public MonitorLotEntity()
        {
            this.KeyFields = Fields(this.MonStage, this.ReqSerialNo, this.LotId);
            this.ComboDisplayFields = Fields(this.LotId, this.ReqSerialNo);
            this.Seal();
        }
    }
}
