using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>기준정보 Equipment Model 화면(Equipment Model Master 메뉴) — 키 <c>FAC_ID</c> + <c>EQP_MODEL_CD</c> 와 설명 <c>CTN_DESC</c> 만 폼이 이름으로 안다.</summary>
    public sealed class EquipmentModelEntity : TableEntity
    {
        public readonly Field FacId = CommonColumns.FacId();
        public readonly Field EqpModelCd = Field("EQP_MODEL_CD").Caption("Model Code");
        public readonly Field CtnDesc = CommonColumns.CtnDesc();

        public EquipmentModelEntity()
        {
            this.KeyFields = Fields(this.FacId, this.EqpModelCd);
            this.ComboDisplayFields = Fields(this.EqpModelCd, this.CtnDesc);
            this.ValueField = this.EqpModelCd;
            this.Seal();
        }
    }
}
