using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Lot 이력 — 기본 필드는 <see cref="LotBase"/>. 키는 Lot Id + Timekey.</summary>
    public class LotHistoryEntity : LotBase
    {
        public readonly Field TimeKey = Field("TIMEKEY").Caption("Timekey");
        public readonly Field EventTm = Field("EVENT_TM").Caption("Event Time");
        public readonly Field EventCd = Field("EVENT_CD").Caption("Event");
        public readonly Field UserId = Field("USER_ID").Caption("User");
        public readonly Field Remark = Field("REMARK").Caption("Remark");

        public LotHistoryEntity()
        {
            this.KeyFields = Fields(this.LotId, this.TimeKey);
            this.Seal();
        }
    }
}
