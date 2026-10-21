using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class LotEntity : EntityDefinition
    {
        public readonly Field LotId = Field("LOT_ID").Caption("Lot Id");
        public readonly SubProdTypField SubProdTyp = Field<SubProdTypField>("SUB_PROD_TYP").Caption("Type");
        public readonly Field ProdId = Field("PROD_ID").Caption("Prod");
        public readonly Field FlowId = Field("FLOW_ID").Caption("Flow");
        public readonly Field OperId = Field("OPER_ID").Caption("Operation");
        public readonly Field Qty = Field("QTY").Caption("Qty");
        public readonly Field CreateTm = Field("CREATE_TM").Caption("Create Time");
        public readonly Field SendFac = Field("SEND_FAC").Caption("Send Fac");
        public readonly Field ReqSerialNo = Field("REQ_SERIAL_NO").Caption("Serial No").TopLabel("Request");
        public readonly LotStatTypField LotStatTyp = Field<LotStatTypField>("LOT_STAT_TYP").Caption("Lot Status");
        public readonly LotHoldStatField LotHoldStatCd = Field<LotHoldStatField>("LOT_HOLD_STAT_CD").Caption("Hold State");
        public readonly MesProcStatField MesProcStatCd = Field<MesProcStatField>("MES_PROC_STAT_CD").Caption("Process").TopLabel("Mes").BottomLabel("Status").TopLabelAlignment(GridTextAlignment.Left).BottomLabelAlignment(GridTextAlignment.Right).Badge();
        public readonly LocNmField LocNm = Field<LocNmField>("LOC_NM").Caption("Location");
        public readonly LotProcStatField LotProcStatCd = Field<LotProcStatField>("LOT_PROC_STAT_CD").Caption("Lot Proc Status");
        public readonly Field EqpId = Field("EQP_ID").Caption("Eqp Id");
        public readonly LastEventCdField LastEventCd = Field<LastEventCdField>("LAST_EVENT_CD").Caption("Last Event");
        public readonly Field Description = Field("DESCRIPTION").Caption("Description");
        public readonly Field ModelId = Field("MODEL_ID").Caption("Product");
        public readonly Field StatTyp = Field("STAT_TYP").Caption("Status");
        public readonly Field EventCd = Field("EVENT_CD").Caption("Event");
        public readonly Field BoxId = Field("BOX_ID").Caption("Carrier");
        public readonly Field StoreId = Field("STORE_ID").Caption("Stocker");
        public readonly Field EventTm = Field("EVENT_TM").Caption("Event Time");
        public readonly Field MesStat = Field("MES_STAT").Caption("Mes Status");

        public LotEntity()
        {
            this.KeyFields = Fields(this.LotId);
            this.ComboDisplayFields = Fields(this.LotId, this.SubProdTyp);
            this.ValueField = this.LotId;
            this.Seal();
        }
    }
}
