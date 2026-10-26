using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Transport 이력(TRANS_HIS) — 공통 필드는 <see cref="TransportBase"/>. 키는 Durable Id + Timekey.</summary>
    public class TransportHistoryEntity : TransportBase
    {
        public readonly Field TimeKey = Field("TIMEKEY").Caption("Timekey");
        public readonly Field EventTm = Field("EVENT_TM").Caption("Event Time");
        public readonly Field EventCd = Field("EVENT_CD").Caption("Event");
        public readonly Field UserId = Field("USER_ID").Caption("User");
        public readonly Field Remark = Field("REMARK").Caption("Remark");

        public TransportHistoryEntity()
        {
            this.KeyFields = Fields(this.DurableId, this.TimeKey);
            this.Seal();
        }
    }
}
