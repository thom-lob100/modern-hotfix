using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class LogisticsEntity : TableEntity
    {
        public readonly Field LotId = LotColumns.LotId();
        public readonly Field SharedWith = Field("SHARED_WITH").Caption("Shared");
        public readonly Field ItemId = Field("ITEM_ID").Caption("Lot Id");
        public readonly Field ReqSerialNo = RequestColumns.ReqSerialNo();
        public readonly Field SendYn = Field("SEND_YN").Caption("Sent");
        public readonly Field SendTm = Field("SEND_TM").Caption("Sent At");
        public readonly Field SendFac = CommonColumns.SendFac();
        public readonly Field LinkTm = Field("LINK_TM").Caption("Linked At");
        public readonly Field RecvYn = Field("RECV_YN").Caption("Received");
        public readonly Field RecvTm = Field("RECV_TM").Caption("Received At");
        public readonly Field RecvDesc = Field("RECV_DESC").Caption("Receive Note");
        public readonly Field ProcYn = Field("PROC_YN").Caption("Processed");
        public readonly Field ProcTm = Field("PROC_TM").Caption("Processed At");
        public readonly Field ItemStat = Field("ITEM_STAT").Caption("Lot Status");
        public readonly Field SampleNm = Field("SAMPLE_NM").Caption("Sample");
        public readonly Field FacId = CommonColumns.FacId();
        public readonly Field BoxId = CommonColumns.BoxId();

        public LogisticsEntity()
        {
            this.KeyFields = Fields(this.LotId);
            this.Seal();
        }
    }
}
