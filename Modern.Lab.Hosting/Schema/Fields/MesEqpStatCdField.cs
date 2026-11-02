using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class MesEqpStatCdField : Field
    {
        public readonly FieldValue Run = Value("Run");
        public readonly FieldValue Idle = Value("Idle");
        public readonly FieldValue Up = Value("Up");
        public readonly FieldValue Down = Value("Down");
    }
}
