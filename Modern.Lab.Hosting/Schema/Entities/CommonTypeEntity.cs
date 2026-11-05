using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class CommonTypeEntity : TableEntity
    {
        public readonly Field CommonTyp = CommonColumns.CommonTyp();
        public readonly Field Description = CommonColumns.Description();
        public readonly Field SortNo = CommonColumns.SortNo();
        public readonly Field UseYn = CommonColumns.UseYn();
        public readonly Field UpdatedBy = CommonColumns.UpdatedBy();
        public readonly Field UpdatedAt = CommonColumns.UpdatedAt();

        public CommonTypeEntity()
        {
            this.KeyFields = Fields(this.CommonTyp);
            this.ComboDisplayFields = Fields(this.CommonTyp);
            this.ValueField = this.CommonTyp;
            this.Seal();
        }
    }
}
