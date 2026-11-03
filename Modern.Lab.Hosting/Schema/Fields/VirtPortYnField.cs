using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>가상 포트 여부(VIRT_PORT_YN).</summary>
    public sealed class VirtPortYnField : Field
    {
        public readonly FieldValue Y = Value("Y");
        public readonly FieldValue N = Value("N");
    }
}
