using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>기준정보 Equipment 화면(Equipment Master 메뉴) — 장비 마스터 행이다. 업무 화면의 장비 목록(상태 · 모드 · 포트)은 <see cref="EquipmentExtEntity"/> 가 맡고,
    /// 이 엔티티는 키 <c>EQP_ID</c> 와 소속(공장 · 모델 · 벤더 · 그룹) · 종류(<c>EQP_TYP</c> · <c>DET_EQP_TYP</c>) · 설명만 폼이 이름으로 안다. 가상 장비(<c>EQP_MODEL_CD</c> = VIRTUAL)는 <see cref="VirtualEquipmentEntity"/>.</summary>
    public sealed class EquipmentEntity : TableEntity
    {
        public readonly Field EqpId = EquipmentColumns.EqpId();
        public readonly Field FacId = CommonColumns.FacId();
        public readonly EquipmentFields.EqpTyp EqpTyp = EquipmentColumns.EqpTyp();
        public readonly EquipmentFields.DetEqpTyp DetEqpTyp = Field<EquipmentFields.DetEqpTyp>("DET_EQP_TYP").Caption("Detail Eqp Type");
        public readonly Field EqpModelCd = Field("EQP_MODEL_CD").Caption("Model Code");
        public readonly Field VendorNm = Field("VENDOR_NM").Caption("Vendor Name");
        public readonly Field EqpGrpId = EquipmentColumns.EqpGrpId();
        public readonly Field CtnDesc = CommonColumns.CtnDesc();

        public EquipmentEntity()
        {
            this.KeyFields = Fields(this.EqpId);
            this.ComboDisplayFields = Fields(this.EqpId, this.CtnDesc);
            this.ValueField = this.EqpId;
            this.Seal();
        }
    }
}
