using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>포트 접근 방식(ACSS_TYP) — Auto · Manual · Normal(2026-11-04 사용자: Manual Out 은 Normal + Output 포트를 막는다).</summary>
    public sealed class AcssTypField : Field
    {
        public readonly FieldValue Auto = Value("Auto");
        public readonly FieldValue Manual = Value("Manual");
        public readonly FieldValue Normal = Value("Normal");
    }
}
