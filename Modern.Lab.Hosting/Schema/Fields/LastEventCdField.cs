using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class LastEventCdField : Field
    {
        public readonly FieldValue JobPrep = Value("JobPrep");
        public readonly FieldValue Reserved = Value("Reserved");
        public readonly FieldValue JobStart = Value("JobStart");
        public readonly FieldValue JobEnd = Value("JobEnd");
        public readonly FieldValue ChangeSpec = Value("ChangeSpec");
        public readonly FieldValue Scrap = Value("Scrap");
    }
}
