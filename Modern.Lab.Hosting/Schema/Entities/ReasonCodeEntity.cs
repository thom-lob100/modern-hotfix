using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class ReasonCodeEntity : TableEntity
    {
        public readonly Field FacId = CommonColumns.FacId();
        public readonly Field ReasonCatgCd = Field("REASON_CATG_CD");
        public readonly Field ReasonCd = Field("REASON_CD");
        public readonly Field CtnDesc = CommonColumns.CtnDesc();

        public ReasonCodeEntity()
        {
            this.KeyFields = Fields(this.FacId, this.ReasonCatgCd, this.ReasonCd);
            this.ComboDisplayFields = Fields(this.ReasonCd, this.CtnDesc);
            this.ValueField = this.ReasonCd;
            this.Seal();
        }
    }
}
