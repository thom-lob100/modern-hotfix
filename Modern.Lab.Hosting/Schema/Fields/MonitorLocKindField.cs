using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Lab Monitor 의 지속재 위치 종류(LOC_KIND) — STB(FOUP 보관) · STOCKER(트레이 보관) · EQP(장비) · OTHER. 서버가 정한다(2026-11-04).</summary>
    public sealed class MonitorLocKindField : Field
    {
        public readonly FieldValue Stb = Value("STB");
        public readonly FieldValue Stocker = Value("STOCKER");
        public readonly FieldValue Eqp = Value("EQP");
        public readonly FieldValue Other = Value("OTHER");
    }
}
