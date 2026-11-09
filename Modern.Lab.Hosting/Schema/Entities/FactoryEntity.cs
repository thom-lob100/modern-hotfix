using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>기준정보 Factory 화면(Factory Master 메뉴) — 키 <c>FAC_ID</c>, 설명 <c>CTN_DESC</c>, 조회조건·편집기 콤보 <c>FAC_TYP</c>(2026-11-10)만 폼이 이름으로 안다.
    /// 회사 SELECT 가 더 주는 컬럼은 그대로 목록·편집기에 붙는다.</summary>
    public sealed class FactoryEntity : TableEntity
    {
        public readonly Field FacId = CommonColumns.FacId();
        public readonly Field CtnDesc = CommonColumns.CtnDesc();
        public readonly CommonFields.FacTyp FacTyp = CommonColumns.FacTyp();

        public FactoryEntity()
        {
            this.KeyFields = Fields(this.FacId);
            this.ComboDisplayFields = Fields(this.FacId, this.CtnDesc);
            this.ValueField = this.FacId;
            this.Seal();
        }
    }
}
