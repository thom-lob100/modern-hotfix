using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Sub Category 기준정보 — <c>DURABLE_TYP</c> 는 값 목록 없이 글자로 받는 칸이라 Durable 사전(값 목록 FOUP · TRAY)을 쓰지 않는다.</summary>
    public sealed class SubCategoryEntity : TableEntity
    {
        public readonly Field SubCatgCd = Field("SUB_CATG_CD");
        public readonly Field DurableTyp = Field("DURABLE_TYP").Caption("Type");

        public SubCategoryEntity()
        {
            this.KeyFields = Fields(this.SubCatgCd);
            this.ComboDisplayFields = Fields(this.SubCatgCd);
            this.ValueField = this.SubCatgCd;
            this.Seal();
        }
    }
}
