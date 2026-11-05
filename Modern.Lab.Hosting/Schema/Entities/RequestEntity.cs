using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class RequestEntity : TableEntity
    {
        public readonly Field ReqSerialNo = RequestColumns.ReqSerialNo();
        public readonly RequestFields.StateCode StateCode = Field<RequestFields.StateCode>("STATE_CODE").Badge();
        public readonly Field Status = Field("STATUS").Caption("Status");
        public readonly Field Requester = Field("REQUESTER");
        public readonly Field Department = Field("DEPARTMENT");
        public readonly Field DeviceNm = Field("DEVICE_NM");
        public readonly Field ProjectNm = Field("PROJECT_NM");
        public readonly Field LayerNm = Field("LAYER_NM");
        public readonly Field Purpose = Field("PURPOSE");

        public RequestEntity()
        {
            this.KeyFields = Fields(this.ReqSerialNo);
            this.ComboDisplayFields = Fields(this.ReqSerialNo);
            this.ValueField = this.ReqSerialNo;
            this.Seal();
        }
    }
}
