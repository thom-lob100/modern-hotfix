using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>기준정보 Vendor 화면(Vendor Master 메뉴) — 키 <c>FAC_ID</c> + <c>VENDOR_NM</c> 와 종류 <c>VENDOR_TYP</c>(조회조건 · 2026-11-12) · 설명 <c>CTN_DESC</c> 만 폼이 이름으로 안다.</summary>
    public sealed class VendorEntity : TableEntity
    {
        public readonly Field FacId = CommonColumns.FacId();
        public readonly Field VendorNm = Field("VENDOR_NM").Caption("Vendor Name");
        public readonly Field VendorTyp = Field("VENDOR_TYP").Caption("Vendor Type");
        public readonly Field CtnDesc = CommonColumns.CtnDesc();

        public VendorEntity()
        {
            this.KeyFields = Fields(this.FacId, this.VendorNm);
            this.ComboDisplayFields = Fields(this.VendorNm, this.CtnDesc);
            this.ValueField = this.VendorNm;
            this.Seal();
        }
    }
}
