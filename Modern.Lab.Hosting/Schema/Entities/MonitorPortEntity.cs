using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Lab Monitor 의 포트 한 줄(<c>GetMonitorPortList</c>, 홈 이름) — 필드는 <see cref="PortBase"/>(장비 · 포트 · 방향 · 상태 · 실린 랏 · 지속재) 그대로다.
    /// FIB 카드가 IN · OUT 포트의 방향과 점유를 그리는 데 쓴다(2026-11-04).</summary>
    public class MonitorPortEntity : PortBase
    {
        public MonitorPortEntity()
        {
            this.Seal();
        }
    }
}
