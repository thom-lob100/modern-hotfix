using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Equipment / Lots의 Lot 목록 — 위 열한 칸은 Lot 화면들이 함께 쓰는 칸이다. Job 역할 필드는 소스 Port <see cref="PortNm"/>·대상 Port <see cref="GoalPortNm"/>·
    /// 소스 Durable <see cref="DurableId"/>(응답 컬럼 <c>CARRIER_ID</c>)·대상 Durable <see cref="GoalDurableId"/>(<c>GOAL_CARRIER_ID</c>)다.
    /// 마지막 처리(<c>LAST_EVENT_CD</c>)는 이 화면에서 Job 배지다. 소스 Durable 의 MES 상태 <see cref="MesStatTyp"/>(<c>MES_STAT_TYP</c>)·
    /// 위치 <see cref="LocNm"/>(<c>LOC_NM</c>)는 Lot 행에 같이 오며 JobPrep Cancel 조건이 본다(2026-10-28). 그 Durable 에 Lot 이 둘 이상인지
    /// <see cref="DurableMultiLotYn"/>(<c>DURABLE_MULTI_LOT_YN</c>)도 Lot 행에 오며 JobPrep 은 N 일 때만 연다(2026-11-04).</summary>
    public sealed class LotPriorityEntity : TableEntity
    {
        public readonly Field LotId = LotColumns.LotId();
        public readonly CommonFields.SubProdTyp SubProdTyp = CommonColumns.SubProdTyp();
        public readonly Field ProdId = CommonColumns.ProdId();
        public readonly Field FlowId = CommonColumns.FlowId();
        public readonly Field OperId = CommonColumns.OperId();
        public readonly Field ReqSerialNo = RequestColumns.ReqSerialNo();
        public readonly LotFields.LotStatTyp LotStatTyp = LotColumns.LotStatTyp();
        public readonly LotFields.LotHoldStatCd LotHoldStatCd = LotColumns.LotHoldStatCd();
        public readonly LotFields.MesProcStatCd MesProcStatCd = LotColumns.MesProcStatCd();
        public readonly LotFields.LotProcStatCd LotProcStatCd = LotColumns.LotProcStatCd();
        public readonly CommonFields.LastEventCd LastEventCd = CommonColumns.LastEventCd().Caption("Job");
        public readonly Field EqpId = EquipmentColumns.EqpId();
        public readonly Field ChildEqpId = LotColumns.ChildEqpId();
        public readonly Field PortNm = PortColumns.PortNm();
        public readonly Field GoalPortNm = CommonColumns.GoalPortNm();
        public readonly Field DurableId = Field("CARRIER_ID").Caption("Durable Id");
        public readonly DurableFields.MesStatTyp MesStatTyp = DurableColumns.MesStatTyp().Caption("Durable MES Status");
        public readonly CommonFields.LocNm LocNm = CommonColumns.LocNm().Caption("Durable Location");
        public readonly DurableFields.DurableMultiLotYn DurableMultiLotYn = Field<DurableFields.DurableMultiLotYn>("DURABLE_MULTI_LOT_YN").Caption("Durable Multi Lot");
        public readonly Field GoalDurableId = Field("GOAL_CARRIER_ID").Caption("Goal Durable Id");
        public readonly Field Priority = CommonColumns.Priority();
        public readonly Field CreateTm = LotColumns.CreateTm();

        public LotPriorityEntity()
        {
            // 작업 시작(JobStart)된 Lot 은 Job 배지가 돈다(2026-10-26).
            this.LastEventCd.Spin(this.LastEventCd.JobStart);
            this.KeyFields = Fields(this.LotId);
            this.ComboDisplayFields = Fields(this.LotId, this.ReqSerialNo);
            this.ValueField = this.LotId;
            this.Seal();
        }
    }
}
