using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>
    /// Equipment / Port 의 장비 목록 — 기본 필드는 <see cref="EquipmentBase"/>. 아래 셋은 Durable Manual In 창의 장비 후보 조건
    /// (<c>COMM_STAT_TYP</c> OnLineLocal · <c>MCS_ITG_YN</c> Y · <c>MES_STAT_TYP</c> Up · <c>EQP_TYP</c> ProductionMachine, 2026-11-02)이 읽는다.
    /// </summary>
    public class EquipmentExtEntity : EquipmentBase
    {
        public readonly McsItgYnField McsItgYn = Field<McsItgYnField>("MCS_ITG_YN").Caption("MCS");
        public readonly EqpMesStatTypField MesStatTyp = Field<EqpMesStatTypField>("MES_STAT_TYP").Caption("Mes Status").Badge();
        public readonly EqpTypField EqpTyp = Field<EqpTypField>("EQP_TYP").Caption("Eqp Type");

        public EquipmentExtEntity()
        {
            this.Seal();
        }
    }
}
