using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>포트 자원 상태(RESRC_STAT_TYP).</summary>
    public sealed class ResrcStatTypField : Field
    {
        public readonly FieldValue InService = Value("InService");
        public readonly FieldValue OutOfService = Value("OutOfService");
    }
}
