using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Lot 계보 트리 — 위 열한 칸은 Lot 화면들이 함께 쓰는 칸이다. 부모는 <see cref="ParentLotId"/>, Type · Lot Status 는 배지다.</summary>
    public sealed class LotTreeEntity : TableEntity
    {
        public readonly Field LotId = LotColumns.LotId();
        public readonly CommonFields.SubProdTyp SubProdTyp = CommonColumns.SubProdTyp().Badge();
        public readonly Field ProdId = CommonColumns.ProdId();
        public readonly Field FlowId = CommonColumns.FlowId();
        public readonly Field OperId = CommonColumns.OperId();
        public readonly Field ReqSerialNo = RequestColumns.ReqSerialNo();
        public readonly LotFields.LotStatTyp LotStatTyp = LotColumns.LotStatTyp().Badge();
        public readonly LotFields.LotHoldStatCd LotHoldStatCd = LotColumns.LotHoldStatCd();
        public readonly LotFields.MesProcStatCd MesProcStatCd = LotColumns.MesProcStatCd();
        public readonly LotFields.LotProcStatCd LotProcStatCd = LotColumns.LotProcStatCd();
        public readonly CommonFields.LastEventCd LastEventCd = CommonColumns.LastEventCd();
        public readonly Field ParentLotId = Field("PARENT_LOT_ID").Caption("Parent Lot");
        public readonly Field Qty = LotColumns.Qty();
        public readonly Field Description = CommonColumns.Description();

        public LotTreeEntity()
        {
            this.KeyFields = Fields(this.LotId);
            this.ComboDisplayFields = Fields(this.LotId, this.SubProdTyp);
            this.ValueField = this.LotId;
            this.ParentField = this.ParentLotId;
            this.Seal();
        }
    }
}
