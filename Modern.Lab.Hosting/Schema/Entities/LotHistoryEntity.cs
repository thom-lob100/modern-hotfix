using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Lot 이력 — 위 열한 칸은 Lot 화면들이 함께 쓰는 칸이고, 아래 다섯은 이력 칸이다. 키는 Lot Id + Timekey.</summary>
    public sealed class LotHistoryEntity : TableEntity
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
        public readonly Field TimeKey = CommonColumns.TimeKey();
        public readonly Field EventTm = CommonColumns.EventTm();
        public readonly Field EventCd = CommonColumns.EventCd();
        public readonly Field UserId = CommonColumns.UserId();
        public readonly Field Remark = CommonColumns.Remark();

        public LotHistoryEntity()
        {
            this.KeyFields = Fields(this.LotId, this.TimeKey);
            this.ComboDisplayFields = Fields(this.LotId, this.SubProdTyp);
            this.ValueField = this.LotId;
            this.Seal();
        }
    }
}
