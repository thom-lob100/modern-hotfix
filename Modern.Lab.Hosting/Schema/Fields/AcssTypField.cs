using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>포트 접근 방식(ACSS_TYP) — Auto · Manual(2026-11-04 사용자: Manual Out 은 Manual + Output 포트를 막는다. 앞서 적었던 Normal 은 잘못 말한 값이라 지웠다).</summary>
    public sealed class AcssTypField : Field
    {
        public readonly FieldValue Auto = Value("Auto");
        public readonly FieldValue Manual = Value("Manual");
    }
}
