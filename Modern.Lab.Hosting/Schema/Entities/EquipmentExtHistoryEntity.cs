using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>장비 이력 — 기본 필드는 <see cref="EquipmentBase"/>. 키는 Eqp Id + Timekey.</summary>
    public class EquipmentExtHistoryEntity : EquipmentBase
    {
        public readonly Field TimeKey = Field("TIMEKEY").Caption("Timekey");
        public readonly Field EventTm = Field("EVENT_TM").Caption("Event Time");
        public readonly Field EventCd = Field("EVENT_CD").Caption("Event");
        public readonly Field UserId = Field("USER_ID").Caption("User");
        public readonly Field Remark = Field("REMARK").Caption("Remark");

        public EquipmentExtHistoryEntity()
        {
            this.KeyFields = Fields(this.EqpId, this.TimeKey);
            this.Seal();
        }
    }
}
