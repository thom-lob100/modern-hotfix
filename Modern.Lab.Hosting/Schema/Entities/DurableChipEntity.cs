using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class DurableChipEntity : TableEntity
    {
        public readonly Field SlotNo = CommonColumns.SlotNo();
        public readonly Field WfId = WaferColumns.WfId();
        public readonly CommonFields.SubProdTyp SubProdTyp = CommonColumns.SubProdTyp();
        public readonly Field LotId = LotColumns.LotId();

        public DurableChipEntity()
        {
            this.KeyFields = Fields(this.SlotNo);
            this.Seal();
        }
    }
}
