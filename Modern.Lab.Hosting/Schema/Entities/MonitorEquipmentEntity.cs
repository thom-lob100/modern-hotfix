using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Lab Monitor 의 장비 한 줄(<c>GetMonitorEqpList</c>, 홈 이름) — 기본 필드는 <see cref="EquipmentBase"/>. 어느 단계의 장비인지 <see cref="MonStage"/>,
    /// 지금 돌고 있는 랏 <see cref="LotId"/> · 예약 랏 <see cref="ReservedLotId"/> · 상태가 바뀐 시각 <see cref="StatusTm"/>. 핸들러의 자식(F1~F4)은 <c>PARENT_EQP_ID</c> 로 묶인다(2026-11-04).</summary>
    public class MonitorEquipmentEntity : EquipmentBase
    {
        public readonly MonitorStageField MonStage = Field<MonitorStageField>("MON_STAGE").Caption("Stage").Badge();
        public readonly Field EqpNm = Field("EQP_NM").Caption("Name");
        public readonly Field StatusTm = Field("STATUS_TM").Caption("Since");
        public readonly Field LotId = Field("LOT_ID").Caption("Lot Id");
        public readonly Field ReservedLotId = Field("RESERVED_LOT_ID").Caption("Reserved Lot");

        public MonitorEquipmentEntity()
        {
            this.Seal();
        }
    }
}
