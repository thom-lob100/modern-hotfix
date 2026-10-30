using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class MesProcStatCdField : Field
    {
        public readonly FieldValue Wait = Value("WAIT");
        public readonly FieldValue Proc = Value("PROC");
        public readonly FieldValue Sele = Value("SELE");
        public readonly FieldValue Resv = Value("RESV");
        public readonly FieldValue Load = Value("LOAD");
        public readonly FieldValue Merg = Value("MERG");
        public readonly FieldValue Scrp = Value("SCRP");
        public readonly FieldValue JobEnd = Value("JobEnd");
    }
}
