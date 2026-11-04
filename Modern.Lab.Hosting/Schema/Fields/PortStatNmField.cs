using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>포트 상태(PORT_STAT_NM) — 회사 값은 대문자 UP · DOWN 이다(2026-11-04 사용자 확인). 장비의 MES_STAT_TYP(Up · Down)과 다르다.</summary>
    public sealed class PortStatNmField : Field
    {
        public readonly FieldValue Up = Value("UP");
        public readonly FieldValue Down = Value("DOWN");
    }
}
