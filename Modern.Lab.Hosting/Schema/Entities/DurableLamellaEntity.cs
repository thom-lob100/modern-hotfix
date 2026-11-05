using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class DurableLamellaEntity : TableEntity
    {
        public readonly Field SlotNo = CommonColumns.SlotNo();
        public readonly Field FingerId = DurableColumns.FingerId();
        public readonly Field FingerIndex = Field("FINGER_INDEX").Caption("Finger Index");
        public readonly Field WfId = WaferColumns.WfId();
        public readonly CommonFields.SubProdTyp SubProdTyp = CommonColumns.SubProdTyp();
        public readonly Field LotId = LotColumns.LotId();

        public DurableLamellaEntity()
        {
            this.KeyFields = Fields(this.SlotNo, this.FingerId);
            this.Seal();
        }
    }
}
