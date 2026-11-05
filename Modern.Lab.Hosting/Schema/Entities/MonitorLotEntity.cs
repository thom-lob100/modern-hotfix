using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Lab Monitor 의 랏 한 줄(<c>GetMonitorLotList</c>, 홈 이름) — 위 열한 칸은 Lot 화면들이 함께 쓰는 칸이다. 단계 <see cref="MonStage"/> · 상태 배지 <see cref="MonStatCd"/> ·
    /// 단계에 들어온 시각 <see cref="StageTm"/>(화면은 경과시간만 센다) · 큐 순위 <see cref="Priority"/> · 의뢰서 수 <see cref="ReqCnt"/>(둘 이상이면 "+n") ·
    /// 가지 표시 <see cref="ReworkYn"/>(지금 재작업 중) · <see cref="ReworkCnt"/>(배지 숫자) · <see cref="ScrapDueYn"/>(Done 뒤 14일 지난 후보)는 모두 서버 칸이다.
    /// Hold 는 <see cref="LotHoldStatCd"/>(OnHold · NotOnHold)로 받는다. 의뢰서만 있고 랏이 없는 줄은 <c>LOT_ID</c> 가 비어 있다(2026-11-04).</summary>
    public sealed class MonitorLotEntity : TableEntity
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
        public readonly CommonFields.LastEventCd LastEventCd = CommonColumns.LastEventCd();
        public readonly MonitorFields.MonStage MonStage = MonitorColumns.MonStage();
        public readonly MonitorFields.MonStatCd MonStatCd = Field<MonitorFields.MonStatCd>("MON_STAT_CD").Caption("State").Badge();
        public readonly Field Qty = LotColumns.Qty();
        public readonly Field ReqCnt = Field("REQ_CNT").Caption("Requests");
        public readonly Field StageTm = Field("STAGE_TM").Caption("Since");
        public readonly Field EqpId = EquipmentColumns.EqpId();
        public readonly Field ChildEqpId = LotColumns.ChildEqpId();
        public readonly Field DurableId = DurableColumns.DurableId();
        public readonly DurableFields.DurableTyp DurableTyp = DurableColumns.DurableTyp().Caption("Durable Type").Badge();
        public readonly Field SendFac = CommonColumns.SendFac().Caption("From Fab");
        public readonly Field Priority = CommonColumns.Priority();
        public readonly CommonFields.Yn ReworkYn = Field<CommonFields.Yn>("REWORK_YN").Caption("Rework");
        public readonly Field ReworkCnt = Field("REWORK_CNT").Caption("Rework Count");
        public readonly CommonFields.Yn ScrapDueYn = Field<CommonFields.Yn>("SCRAP_DUE_YN").Caption("Scrap Due");

        public MonitorLotEntity()
        {
            this.KeyFields = Fields(this.MonStage, this.ReqSerialNo, this.LotId);
            this.ComboDisplayFields = Fields(this.LotId, this.ReqSerialNo);
            this.ValueField = this.LotId;
            this.Seal();
        }
    }
}
