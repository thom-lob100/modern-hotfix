using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class LotCodeEntity : TableEntity
    {
        public readonly Field LotCd = LotColumns.LotCd();
        public readonly Field LotNm = Field("LOT_NM");
        public readonly CommonFields.SubProdTyp SubProdTyp = CommonColumns.SubProdTyp();
        public readonly Field UseYn = CommonColumns.UseYn();
        public readonly Field Remark = CommonColumns.Remark();

        public LotCodeEntity()
        {
            this.KeyFields = Fields(this.LotCd);
            this.ComboDisplayFields = Fields(this.LotCd);
            this.ValueField = this.LotCd;
            this.Seal();
        }
    }
}
