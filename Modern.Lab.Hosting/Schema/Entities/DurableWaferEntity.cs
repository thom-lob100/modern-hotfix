using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class DurableWaferEntity : TableEntity
    {
        public readonly Field SlotNo = CommonColumns.SlotNo();
        public readonly Field WfId = WaferColumns.WfId();
        public readonly CommonFields.SubProdTyp SubProdTyp = CommonColumns.SubProdTyp();
        public readonly Field LotId = LotColumns.LotId();
        public readonly LotFields.LotStatTyp LotStatTyp = LotColumns.LotStatTyp();
        public readonly LotFields.LotHoldStatCd LotHoldStatCd = LotColumns.LotHoldStatCd();
        public readonly LotFields.MesProcStatCd MesProcStatCd = LotColumns.MesProcStatCd();
        public readonly CommonFields.JudgeRslt JudgeRslt = CommonColumns.JudgeRslt();

        public DurableWaferEntity()
        {
            this.KeyFields = Fields(this.SlotNo);
            this.Seal();
        }
    }
}
