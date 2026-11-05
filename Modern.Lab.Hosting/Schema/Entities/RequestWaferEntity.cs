using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class RequestWaferEntity : TableEntity
    {
        public readonly Field WfId = WaferColumns.WfId();
        public readonly Field SlotNo = CommonColumns.SlotNo();
        public readonly Field LotId = LotColumns.LotId();
        public readonly CommonFields.SubProdTyp SubProdTyp = CommonColumns.SubProdTyp();

        public RequestWaferEntity()
        {
            this.KeyFields = Fields(this.WfId);
            this.ComboDisplayFields = Fields(this.WfId);
            this.ValueField = this.WfId;
            this.Seal();
        }
    }
}
