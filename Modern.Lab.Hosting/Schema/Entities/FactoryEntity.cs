using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>기준정보 Factory 화면(Factory Master 메뉴) — 키 <c>FAC_ID</c> 와 설명 <c>CTN_DESC</c> 만 폼이 이름으로 안다. 회사 SELECT 가 더 주는 컬럼은 그대로 목록·편집기에 붙는다.</summary>
    public sealed class FactoryEntity : TableEntity
    {
        public readonly Field FacId = CommonColumns.FacId();
        public readonly Field CtnDesc = CommonColumns.CtnDesc();

        public FactoryEntity()
        {
            this.KeyFields = Fields(this.FacId);
            this.ComboDisplayFields = Fields(this.FacId, this.CtnDesc);
            this.ValueField = this.FacId;
            this.Seal();
        }
    }
}
