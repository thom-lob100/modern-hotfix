using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>
    /// Lot 가족(<see cref="LotEntity"/> · <see cref="LotHistoryEntity"/> · <see cref="LotPriorityEntity"/> · <see cref="LotTreeEntity"/>)이
    /// 함께 쓰는 필드 — 회사 컬럼 이름·캡션이 다르면 여기 한 곳만 고친다(2026-10-22).
    /// 파생 엔티티는 자기 화면에만 있는 필드를 추가하고, 캡션·배지를 바꿀 때는 생성자에서 <c>this.LastEventCd.Caption("Job").Badge()</c> 처럼
    /// 재정의한다 — 같은 컬럼을 다시 선언하면 <c>Seal</c> 이 "두 필드에 선언됐다"로 막는다. 응답에 없는 기본 필드는 표·카드에 나타나지 않는다.
    /// <c>Seal()</c> 은 파생 생성자 끝에서 부른다.
    /// </summary>
    public abstract class LotBase : EntityDefinition
    {
        public readonly Field LotId = Field("LOT_ID").Caption("Lot Id");
        public readonly SubProdTypField SubProdTyp = Field<SubProdTypField>("SUB_PROD_TYP").Caption("Type");
        public readonly Field ProdId = Field("PROD_ID").Caption("Prod");
        public readonly Field FlowId = Field("FLOW_ID").Caption("Flow");
        public readonly Field OperId = Field("OPER_ID").Caption("Operation");
        public readonly Field ReqSerialNo = Field("REQ_SERIAL_NO").Caption("Serial No").TopLabel("Request");
        public readonly LotStatTypField LotStatTyp = Field<LotStatTypField>("LOT_STAT_TYP").Caption("Lot Status");
        public readonly LotHoldStatField LotHoldStatCd = Field<LotHoldStatField>("LOT_HOLD_STAT_CD").Caption("Hold State");
        public readonly MesProcStatField MesProcStatCd = Field<MesProcStatField>("MES_PROC_STAT_CD").Caption("Process").TopLabel("Mes").BottomLabel("Status").TopLabelAlignment(GridTextAlignment.Left).BottomLabelAlignment(GridTextAlignment.Right).Badge();
        public readonly LotProcStatField LotProcStatCd = Field<LotProcStatField>("LOT_PROC_STAT_CD").Caption("Lot Proc Status");
        public readonly LastEventCdField LastEventCd = Field<LastEventCdField>("LAST_EVENT_CD").Caption("Last Event");

        /// <summary>기본값 — 파생이 바꾸지 않으면 Lot Id 가 키·값이고 콤보는 Lot Id · Type 을 보인다.</summary>
        protected LotBase()
        {
            this.KeyFields = Fields(this.LotId);
            this.ComboDisplayFields = Fields(this.LotId, this.SubProdTyp);
            this.ValueField = this.LotId;
        }
    }
}
