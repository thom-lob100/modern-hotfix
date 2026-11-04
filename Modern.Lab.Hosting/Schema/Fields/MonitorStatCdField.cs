using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Lab Monitor 의 랏 상태 배지(MON_STAT_CD) — 작업 중(Job) · 다음 차례(Next) · 대기(Wait). 서버가 정한다(2026-11-04).</summary>
    public sealed class MonitorStatCdField : Field
    {
        public readonly FieldValue Job = Value("Job");
        public readonly FieldValue Next = Value("Next");
        public readonly FieldValue Wait = Value("Wait");
    }
}
