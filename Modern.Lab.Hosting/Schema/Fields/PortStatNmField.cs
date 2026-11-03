using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>포트 상태(PORT_STAT_NM).</summary>
    public sealed class PortStatNmField : Field
    {
        public readonly FieldValue Up = Value("Up");
        public readonly FieldValue Down = Value("Down");
    }
}
