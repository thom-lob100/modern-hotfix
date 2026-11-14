using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class OperEntity : TableEntity
    {
        public readonly Field OperId = CommonColumns.OperId();
        public readonly Field OperName = Field("OPER_NAME");
        public readonly Field OperGroup = Field("OPER_GROUP");
        public readonly Field MidOperTyp = Field("MID_OPER_TYP").Caption("Mid Oper Type");
        public readonly Field DetOperTyp = Field("DET_OPER_TYP").Caption("Detail Oper Type");
        public readonly Field StdTimeMin = Field("STD_TIME_MIN");
        public readonly Field EqpType = Field("EQP_TYPE");
        public readonly Field UseYn = CommonColumns.UseYn();
        public readonly Field Remark = CommonColumns.Remark();

        public OperEntity()
        {
            this.KeyFields = Fields(this.OperId);
            this.ComboDisplayFields = Fields(this.OperId);
            this.ValueField = this.OperId;
            this.Seal();
        }
    }
}
