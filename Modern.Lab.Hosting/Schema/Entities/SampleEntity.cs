using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class SampleEntity : EntityDefinition
    {
        public readonly Field ReqSerialNo = Field("REQ_SERIAL_NO").Caption("Serial No").TopLabel("Request");
        public readonly Field ReqSubSerialNo = Field("REQ_SUBSERIAL_NO").Caption("SubSerial No").TopLabel("Request");
        public readonly Field UnitId = Field("UNIT_ID");
        public readonly Field WaferId = Field("WAFER_ID");
        public readonly Field ChipId = Field("CHIP_ID");
        public readonly Field LotId = Field("LOT_ID").Caption("Lot Id");

        public SampleEntity()
        {
            this.KeyFields = Fields(this.ReqSerialNo, this.ReqSubSerialNo);
            this.Seal();
        }
    }
}
