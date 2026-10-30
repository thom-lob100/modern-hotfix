using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class DurableMesProcStatCdField : Field
    {
        public readonly FieldValue Idle = Value("Idle");
        public readonly FieldValue Load = Value("LOAD");
        public readonly FieldValue Process = Value("Process");
        public readonly FieldValue Reserved = Value("Reserved");
        public readonly FieldValue Selected = Value("Selected");
    }
}
