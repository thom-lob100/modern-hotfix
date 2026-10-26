using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>
    /// Equipment 가족(<see cref="EquipmentExtEntity"/> · <see cref="EquipmentExtHistoryEntity"/> · <see cref="EquipmentPriorityEntity"/>)이
    /// 함께 쓰는 필드 — 회사 테이블 기준으로 "둘 이상의 가족 엔티티가 쓰는 컬럼"을 둔다(2026-10-25 사용자 결정). 식별 컬럼과 상태 컬럼이 함께 있고,
    /// Ext 에만 있는 <c>LAST_…</c> 류는 <see cref="EquipmentExtEntity"/> 에, 이력에만 있는 <c>TIMEKEY</c> 류는 <see cref="EquipmentExtHistoryEntity"/> 에 둔다.
    /// 회사 컬럼 이름·캡션이 다르면 여기 한 곳만 고친다. 장비는 기준정보 화면이 없어 기준정보 엔티티를 두지 않는다.
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
            // 트리의 부모 연결 — 트리 컨트롤이 UseEntity 로 키·부모 키를 읽는다(2026-10-26).
            this.ParentField = this.ParentEqpId;
            this.ComboDisplayFields = Fields(this.EqpId, this.CommStatTyp);
            this.ValueField = this.EqpId;
            // 작업 중(Run) 장비는 상태 배지가 돈다 — 표·카드가 이 선언을 기본으로 쓴다(2026-10-26).
            this.MesEqpStatCd.Spin(this.MesEqpStatCd.Run);
        }
    }
}
