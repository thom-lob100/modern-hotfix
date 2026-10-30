using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Equipment / Lots의 Lot 목록 — 기본 필드는 <see cref="LotBase"/>. Job 역할 필드는 소스 Port <see cref="PortNm"/>·대상 Port <see cref="GoalPortNm"/>·
    /// 소스 Durable <see cref="DurableId"/>(응답 컬럼 <c>CARRIER_ID</c>)·대상 Durable <see cref="GoalDurableId"/>(<c>GOAL_CARRIER_ID</c>)다.
    /// 마지막 처리(<c>LAST_EVENT_CD</c>)는 이 화면에서 Job 배지다. 소스 Durable 의 MES 상태 <see cref="MesStatTyp"/>(<c>MES_STAT_TYP</c>)·
    /// 위치 <see cref="LocNm"/>(<c>LOC_NM</c>)는 Lot 행에 같이 오며 JobPrep Cancel 조건이 본다(2026-10-28).</summary>
    public class LotPriorityEntity : LotBase
    {
        public readonly Field EqpId = Field("EQP_ID").Caption("Eqp Id");
        public readonly Field ChildEqpId = Field("CHILD_EQP_ID").Caption("Child Eqp");
        public readonly Field PortNm = Field("PORT_NM").Caption("Port");
        public readonly Field GoalPortNm = Field("GOAL_PORT_NM").Caption("Goal Port");
        public readonly Field DurableId = Field("CARRIER_ID").Caption("Durable Id");
        public readonly MesStatTypField MesStatTyp = Field<MesStatTypField>("MES_STAT_TYP").Caption("Durable MES Status");
        public readonly LocNmField LocNm = Field<LocNmField>("LOC_NM").Caption("Durable Location");
        public readonly Field GoalDurableId = Field("GOAL_CARRIER_ID").Caption("Goal Durable Id");
        public readonly Field Priority = Field("PRIORITY").Caption("Prio");
        public readonly Field CreateTm = Field("CREATE_TM").Caption("Create Time");

        public LotPriorityEntity()
        {
            // 작업 시작(JobStart)된 Lot 은 Job 배지가 돈다(2026-10-26).
            this.LastEventCd.Caption("Job").Spin(this.LastEventCd.JobStart);
            this.ComboDisplayFields = Fields(this.LotId, this.ReqSerialNo);
            this.Seal();
        }
    }
}
