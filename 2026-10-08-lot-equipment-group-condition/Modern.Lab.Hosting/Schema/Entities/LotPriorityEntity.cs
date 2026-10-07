using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Equipment / Lots의 Lot 목록 — Job 역할 필드는 소스 Port <see cref="PortNm"/>·대상 Port <see cref="GoalPortNm"/>·
    /// 소스 Durable <see cref="DurableId"/>(응답 컬럼 <c>CARRIER_ID</c>)·대상 Durable <see cref="GoalDurableId"/>(<c>GOAL_CARRIER_ID</c>)다.</summary>
    public sealed class LotPriorityEntity : EntityDefinition
    {
        public readonly Field LotId = Field("LOT_ID").Caption("Lot Id");
        public readonly Field EqpId = Field("EQP_ID").Caption("Eqp Id");
        public readonly Field PortNm = Field("PORT_NM").Caption("Port");
        public readonly Field GoalPortNm = Field("GOAL_PORT_NM").Caption("Goal Port");
        public readonly Field DurableId = Field("CARRIER_ID").Caption("Durable Id");
        public readonly Field GoalDurableId = Field("GOAL_CARRIER_ID").Caption("Goal Durable Id");
        public readonly LastEventCdField LastEventCd = Field<LastEventCdField>("LAST_EVENT_CD").Caption("Job").Badge();
        public readonly LotHoldStatField LotHoldStatCd = Field<LotHoldStatField>("LOT_HOLD_STAT_CD").Caption("Hold State");
        public readonly MesProcStatField MesProcStatCd = Field<MesProcStatField>("MES_PROC_STAT_CD").Caption("Process").TopLabel("Mes").BottomLabel("Status").TopLabelAlignment(GridTextAlignment.Left).BottomLabelAlignment(GridTextAlignment.Right).Badge();
        public readonly LotStatTypField LotStatTyp = Field<LotStatTypField>("LOT_STAT_TYP").Caption("Lot Status");
        public readonly LotProcStatField LotProcStatCd = Field<LotProcStatField>("LOT_PROC_STAT_CD").Caption("Lot Proc Status");
        public readonly SubProdTypField SubProdTyp = Field<SubProdTypField>("SUB_PROD_TYP").Caption("Type");
        public readonly Field Priority = Field("PRIORITY").Caption("Prio");
        public readonly Field ReqSerialNo = Field("REQ_SERIAL_NO").Caption("Serial No").TopLabel("Request");
        public readonly Field ProdId = Field("PROD_ID").Caption("Prod");
        public readonly Field FlowId = Field("FLOW_ID").Caption("Flow");
        public readonly Field OperId = Field("OPER_ID").Caption("Operation");
        public readonly Field CreateTm = Field("CREATE_TM").Caption("Create Time");

        public LotPriorityEntity()
        {
            this.KeyFields = Fields(this.LotId);
            this.ComboDisplayFields = Fields(this.LotId, this.ReqSerialNo);
            this.ValueField = this.LotId;
            this.Seal();
        }
    }
}
