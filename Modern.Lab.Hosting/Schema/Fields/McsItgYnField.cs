using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>MCS 연동 여부(MCS_ITG_YN).</summary>
    public sealed class McsItgYnField : Field
    {
        public readonly FieldValue Y = Value("Y");
        public readonly FieldValue N = Value("N");
    }
}
