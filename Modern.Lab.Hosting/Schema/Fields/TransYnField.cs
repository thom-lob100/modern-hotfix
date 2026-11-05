using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>반송이 걸려 있는지(TRANS_YN) — Y 이면 Manual In · Manual Out 은 닫히고 취소만 된다(2026-11-05 사용자).</summary>
    public sealed class TransYnField : Field
    {
        public readonly FieldValue Y = Value("Y");
        public readonly FieldValue N = Value("N");
    }
}
