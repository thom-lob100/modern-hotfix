using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>포트 이력 — 기본 필드는 <see cref="PortBase"/>. 키는 Eqp Id + Port + Timekey.</summary>
    public class PortHistoryEntity : PortBase
    {
        public readonly Field TimeKey = Field("TIMEKEY").Caption("Timekey");
        public readonly Field EventTm = Field("EVENT_TM").Caption("Event Time");
        public readonly Field EventCd = Field("EVENT_CD").Caption("Event");
        public readonly Field UserId = Field("USER_ID").Caption("User");
        public readonly Field Remark = Field("REMARK").Caption("Remark");

        public PortHistoryEntity()
        {
            this.KeyFields = Fields(this.EqpId, this.PortNm, this.TimeKey);
            this.Seal();
        }
    }
}
