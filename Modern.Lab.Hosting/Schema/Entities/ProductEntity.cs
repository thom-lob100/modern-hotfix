using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class ProductEntity : TableEntity
    {
        public readonly Field ProdId = CommonColumns.ProdId();
        public readonly Field CtnDesc = CommonColumns.CtnDesc();
        public readonly Field ProductType = Field("PRODUCT_TYPE");
        public readonly CommonFields.SubProdTyp SubProdTyp = CommonColumns.SubProdTyp();
        public readonly Field LotCd = LotColumns.LotCd();
        public readonly Field Unit = Field("UNIT");
        public readonly Field UseYn = CommonColumns.UseYn();
        public readonly Field Description = CommonColumns.Description();
        public readonly Field UpdatedBy = CommonColumns.UpdatedBy();
        public readonly Field UpdatedAt = CommonColumns.UpdatedAt();

        public ProductEntity()
        {
            this.KeyFields = Fields(this.ProdId);
            // 키는 서버가 채번한다 — 신규에서도 잠기고 필수 검사에서 빠진다. 키 외 필수 입력은 회사가 정한다(홈 예제 컬럼을 필수로 두지 않는다).
            this.ReadOnlyFields = Fields(this.ProdId);
            this.ComboDisplayFields = Fields(this.ProdId, this.CtnDesc);
            this.ValueField = this.ProdId;
            this.Seal();
        }
    }
}
