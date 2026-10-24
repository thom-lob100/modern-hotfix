using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Equipment / Lots · Lot / Equipment 의 장비 목록 — 기본 필드는 <see cref="EquipmentBase"/>.
    /// Job 역할 필드는 소스 Port <see cref="PortNm"/>·대상 Port <see cref="GoalPortNm"/>, 순위는 <see cref="Priority"/>.</summary>
    public class EquipmentPriorityEntity : EquipmentBase
    {
        public readonly Field EqpIdAlias = Field("EQP_ID_ALIAS").Caption("Alias");
        public readonly EqpMesStatTypField MesStatTyp = Field<EqpMesStatTypField>("MES_STAT_TYP").Caption("MES Status");
        public readonly EqpTypField EqpTyp = Field<EqpTypField>("EQP_TYP").Caption("Equipment Type");
        public readonly DetEqpTypField DetEqpTyp = Field<DetEqpTypField>("DET_EQP_TYP").Caption("Equipment Detail Type");
        public readonly Field Priority = Field("PRIORITY").Caption("Prio");
        public readonly Field PortNm = Field("PORT_NM").Caption("Port");
        public readonly Field GoalPortNm = Field("GOAL_PORT_NM").Caption("Goal Port");

        public EquipmentPriorityEntity()
        {
            this.Seal();
        }
    }
}
