using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Lab Monitor 의 포트 한 줄(<c>GetMonitorPortList</c>, 홈 이름) — 포트 화면들이 함께 쓰는 일곱 칸(장비 · 포트 · 방향 · 상태 · 실린 랏 · 지속재) 그대로다.
    /// FIB 카드가 IN · OUT 포트의 방향과 점유를 그리는 데 쓴다(2026-11-04).</summary>
    public sealed class MonitorPortEntity : TableEntity
    {
        public readonly Field EqpId = EquipmentColumns.EqpId();
        public readonly Field PortNm = PortColumns.PortNm();
        public readonly PortFields.PortTyp PortTyp = PortColumns.PortTyp().Badge();
        public readonly Field Mode = PortColumns.Mode();
        public readonly PortFields.TransferStatCd TransferStatCd = PortColumns.TransferStatCd();
        public readonly Field LotId = LotColumns.LotId();
        public readonly Field DurableId = DurableColumns.DurableId();

        public MonitorPortEntity()
        {
            this.KeyFields = Fields(this.EqpId, this.PortNm);
            this.ComboDisplayFields = Fields(this.PortNm, this.PortTyp);
            this.ValueField = this.PortNm;
            this.Seal();
        }
    }
}
