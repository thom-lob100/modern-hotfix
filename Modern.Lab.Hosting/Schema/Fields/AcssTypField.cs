using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>포트 접근 방식(ACSS_TYP).</summary>
    public sealed class AcssTypField : Field
    {
        public readonly FieldValue Auto = Value("Auto");
        public readonly FieldValue Manual = Value("Manual");
    }
}
