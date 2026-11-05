using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class CommonTypeDetEntity : TableEntity
    {
        public readonly Field CommonTyp = CommonColumns.CommonTyp();
        public readonly Field TypVal = Field("TYP_VAL");
        public readonly Field Description = CommonColumns.Description();
        public readonly Field SortNo = CommonColumns.SortNo();
        public readonly Field UseYn = CommonColumns.UseYn();
        public readonly Field UpdatedBy = CommonColumns.UpdatedBy();
        public readonly Field UpdatedAt = CommonColumns.UpdatedAt();

        public CommonTypeDetEntity()
        {
            this.KeyFields = Fields(this.CommonTyp, this.TypVal);
            this.ComboDisplayFields = Fields(this.TypVal);
            this.ValueField = this.TypVal;
            this.Seal();
        }
    }
}
