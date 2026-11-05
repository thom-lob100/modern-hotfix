using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Lot 목록 · Lot Info — 위 열한 칸은 Lot 화면들이 함께 쓰는 칸이고, 아래쪽 일곱 컬럼(<c>MODEL_ID</c> …)은 들어올 Lot 카드(Create Wafer · Logistics 수신)가 쓴다.</summary>
    public sealed class LotEntity : TableEntity
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
        public readonly Field Qty = LotColumns.Qty();
        public readonly Field CreateTm = LotColumns.CreateTm();
        public readonly Field SendFac = CommonColumns.SendFac();
        public readonly CommonFields.LocNm LocNm = CommonColumns.LocNm();
        public readonly Field EqpId = EquipmentColumns.EqpId();
        public readonly Field Description = CommonColumns.Description();
        public readonly Field ModelId = Field("MODEL_ID").Caption("Product");
        public readonly Field StatTyp = Field("STAT_TYP").Caption("Status");
        public readonly Field EventCd = CommonColumns.EventCd();
        public readonly Field BoxId = CommonColumns.BoxId();
        public readonly Field StoreId = Field("STORE_ID").Caption("Stocker");
        public readonly Field EventTm = CommonColumns.EventTm();
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
