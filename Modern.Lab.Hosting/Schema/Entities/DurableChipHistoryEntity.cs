using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class DurableChipHistoryEntity : TableEntity
    {
        public readonly Field WfId = WaferColumns.WfId();
        public readonly Field TimeKey = CommonColumns.TimeKey();
        public readonly Field EventTm = CommonColumns.EventTm();
        public readonly Field EventCd = CommonColumns.EventCd();
        public readonly Field LotId = LotColumns.LotId();
        public readonly CommonFields.SubProdTyp SubProdTyp = CommonColumns.SubProdTyp();
        public readonly CommonFields.JudgeRslt JudgeRslt = CommonColumns.JudgeRslt();
        public readonly Field ReqSerialNo = RequestColumns.ReqSerialNo();
        public readonly Field UserId = CommonColumns.UserId();
        public readonly Field Remark = CommonColumns.Remark();

        public DurableChipHistoryEntity()
        {
            this.KeyFields = Fields(this.WfId, this.TimeKey);
            this.Seal();
        }
    }
}
