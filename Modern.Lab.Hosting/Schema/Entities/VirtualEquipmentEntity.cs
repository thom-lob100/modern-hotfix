using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>기준정보 Virtual Equipment 화면(Virtual Equipment Master 메뉴) — 장비 표의 같은 행이지만 모델이 VIRTUAL 로 고정된 장비라 항목이 다르다(2026-11-07).
    /// 폼이 이름으로 아는 컬럼은 키 <c>EQP_ID</c> · 공장 · 종류(<c>EQP_TYP</c> · <c>DET_EQP_TYP</c>) · <c>EQP_MODEL_CD</c>(편집기에서 VIRTUAL 고정) · 그룹 · 설명이다 —
    /// 공장 · 종류 · 그룹은 Equipment 와 같은 조회조건이다(2026-11-12). 어느 행이 가상인지는 서버가 가른다.</summary>
    public sealed class VirtualEquipmentEntity : TableEntity
    {
        public readonly Field EqpId = EquipmentColumns.EqpId();
        public readonly Field FacId = CommonColumns.FacId();
        public readonly EquipmentFields.EqpTyp EqpTyp = EquipmentColumns.EqpTyp();
        public readonly EquipmentFields.DetEqpTyp DetEqpTyp = Field<EquipmentFields.DetEqpTyp>("DET_EQP_TYP").Caption("Detail Eqp Type");
        public readonly Field EqpModelCd = Field("EQP_MODEL_CD").Caption("Model Code");
        public readonly Field EqpGrpId = EquipmentColumns.EqpGrpId();
        public readonly Field CtnDesc = CommonColumns.CtnDesc();

        public VirtualEquipmentEntity()
        {
            this.KeyFields = Fields(this.EqpId);
            this.ComboDisplayFields = Fields(this.EqpId, this.CtnDesc);
            this.ValueField = this.EqpId;
            this.Seal();
        }
    }
}
