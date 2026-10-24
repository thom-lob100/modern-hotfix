using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>
    /// Equipment 가족(<see cref="EquipmentEntity"/> · <see cref="EquipmentHistoryEntity"/> · <see cref="EquipmentPriorityEntity"/>)이
    /// 함께 쓰는 필드 — 회사 컬럼 이름·캡션이 다르면 여기 한 곳만 고친다(2026-10-25).
    /// 파생 엔티티는 자기 화면에만 있는 필드를 추가하고, 캡션·배지를 바꿀 때는 생성자에서 <c>Seal()</c> 전에 재정의한다 —
    /// 같은 컬럼을 다시 선언하면 <c>Seal</c> 이 "두 필드에 선언됐다"로 막는다. 응답에 없는 기본 필드는 표·카드에 나타나지 않는다.
    /// </summary>
    public abstract class EquipmentBase : EntityDefinition
    {
        public readonly Field EqpId = Field("EQP_ID").Caption("Eqp Id");
        public readonly Field EqpGrpId = Field("EQP_GRP_ID").Caption("Group").TopLabel("Equipment").HeaderAlignment(GridTextAlignment.Center).TopLabelAlignment(GridTextAlignment.Center);
        public readonly Field EqpNm = Field("EQP_NM").Caption("Eqp Name");
        public readonly Field ParentEqpId = Field("PARENT_EQP_ID").Caption("Parent Eqp");
        public readonly CommStatTypField CommStatTyp = Field<CommStatTypField>("COMM_STAT_TYP").Caption("Mode").Badge();
        public readonly MesEqpStatField MesEqpStatCd = Field<MesEqpStatField>("MES_EQP_STAT_CD").Caption("Status").Badge();
        public readonly AutoYnField AutoYn = Field<AutoYnField>("AUTO_YN").Caption("Auto");
        public readonly Field Location = Field("LOCATION").Caption("Location");

        /// <summary>기본값 — 파생이 바꾸지 않으면 Eqp Id 가 키·값이고 콤보는 Eqp Id · Mode 를 보인다.</summary>
        protected EquipmentBase()
        {
            this.KeyFields = Fields(this.EqpId);
            this.ComboDisplayFields = Fields(this.EqpId, this.CommStatTyp);
            this.ValueField = this.EqpId;
        }
    }
}
