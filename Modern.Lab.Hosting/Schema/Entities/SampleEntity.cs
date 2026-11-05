using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class SampleEntity : TableEntity
    {
        public readonly Field ReqSerialNo = RequestColumns.ReqSerialNo();
        public readonly Field ReqSubSerialNo = Field("REQ_SUBSERIAL_NO").Caption("SubSerial No").TopLabel("Request");
        public readonly Field UnitId = Field("UNIT_ID");
        public readonly Field WaferId = Field("WAFER_ID");
        public readonly Field ChipId = Field("CHIP_ID");
        public readonly Field LotId = LotColumns.LotId();

        public SampleEntity()
        {
            this.KeyFields = Fields(this.ReqSerialNo, this.ReqSubSerialNo);
            this.Seal();
        }
    }
}
