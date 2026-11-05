using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class WaferEntity : TableEntity
    {
        public readonly Field WfId = WaferColumns.WfId();
        public readonly Field SlotNo = CommonColumns.SlotNo();
        public readonly CommonFields.SubProdTyp SubProdTyp = CommonColumns.SubProdTyp();
        public readonly Field LotId = LotColumns.LotId();
        public readonly CommonFields.JudgeRslt JudgeRslt = CommonColumns.JudgeRslt();
        public readonly Field ReqSerialNo = RequestColumns.ReqSerialNo();

        public WaferEntity()
        {
            this.KeyFields = Fields(this.WfId);
            this.ComboDisplayFields = Fields(this.WfId);
            this.ValueField = this.WfId;
            this.Seal();
        }
    }
}
