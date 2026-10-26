using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Transport History 의 Transport 목록(TRANS_MAS) — 공통 필드는 <see cref="TransportBase"/>. 키는 Durable Id.
    /// 마지막 처리(<c>LAST_EVENT_CD</c> · <c>LAST_TIMEKEY</c>)는 이 표에만 있다.</summary>
    public class TransportEntity : TransportBase
    {
        public readonly LastEventCdField LastEventCd = Field<LastEventCdField>("LAST_EVENT_CD").Caption("Last Event");
        public readonly Field LastTimeKey = Field("LAST_TIMEKEY").Caption("Last Timekey");

        public TransportEntity()
        {
            this.Seal();
        }
    }
}
