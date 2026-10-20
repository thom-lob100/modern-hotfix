using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Equipment / Lots · Lot / Equipment 의 자식 장비 목록 — 핸들러처럼 자식 장비(작업 자리)를 거느린 장비를 결정했을 때
    /// Port List 아래에 보이는 표다. 행은 장비 표와 같은 컬럼 이름이고 <see cref="ParentEqpId"/> 로 부모에 묶인다.</summary>
    public sealed class ChildEquipmentEntity : EntityDefinition
    {
        public readonly Field EqpId = Field("EQP_ID").Caption("Child Eqp");
        public readonly Field ParentEqpId = Field("PARENT_EQP_ID").Caption("Parent Eqp");
        public readonly Field EqpNm = Field("EQP_NM").Caption("Eqp Name");
        public readonly CommStatTypField CommStatTyp = Field<CommStatTypField>("COMM_STAT_TYP").Caption("Mode").Badge();
        public readonly MesEqpStatField MesEqpStatCd = Field<MesEqpStatField>("MES_EQP_STAT_CD").Caption("Status").Badge();
        public readonly Field LotId = Field("LOT_ID").Caption("Lot Id");
        public readonly Field Priority = Field("PRIORITY").Caption("Prio");

        public ChildEquipmentEntity()
        {
            this.KeyFields = Fields(this.EqpId);
            this.Seal();
        }
    }
}
