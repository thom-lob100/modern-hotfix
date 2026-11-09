using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>
    /// Equipment / Port 의 포트 목록 — 위 일곱 칸은 포트 화면들이 함께 쓰는 칸이다. 아래 다섯은 Durable Manual In 창의 포트 후보 조건
    /// (<c>PORT_TYP</c> Input · InputOutput, <c>ACSS_TYP</c> Auto, <c>PORT_STAT_NM</c> Up, <c>RESRC_STAT_TYP</c> InService,
    /// <c>TRANSFER_STAT_CD</c> ReadyToLoad, <c>CURR_DURABLE_ID</c> 빈 값, <c>VIRT_PORT_YN</c> N, 2026-11-02 · 2026-11-09)이 읽는다.
    /// </summary>
    public sealed class PortDetEntity : TableEntity
    {
        public readonly Field EqpId = EquipmentColumns.EqpId();
        public readonly Field PortNm = PortColumns.PortNm();
        public readonly PortFields.PortTyp PortTyp = PortColumns.PortTyp().Badge();
        public readonly Field Mode = PortColumns.Mode();
        public readonly PortFields.TransferStatCd TransferStatCd = PortColumns.TransferStatCd();
        public readonly Field LotId = LotColumns.LotId();
        public readonly Field DurableId = DurableColumns.DurableId();
        public readonly CommonFields.AcssTyp AcssTyp = CommonColumns.AcssTyp().Badge();
        public readonly PortFields.PortStatNm PortStatNm = Field<PortFields.PortStatNm>("PORT_STAT_NM").Caption("Port Status").Badge();
        public readonly PortFields.ResrcStatTyp ResrcStatTyp = Field<PortFields.ResrcStatTyp>("RESRC_STAT_TYP").Caption("Resource");
        public readonly Field CurrDurableId = Field("CURR_DURABLE_ID").Caption("Current Durable");
        public readonly PortFields.VirtPortYn VirtPortYn = Field<PortFields.VirtPortYn>("VIRT_PORT_YN").Caption("Virtual");

        public PortDetEntity()
        {
            this.KeyFields = Fields(this.EqpId, this.PortNm);
            this.ComboDisplayFields = Fields(this.PortNm, this.PortTyp);
            this.ValueField = this.PortNm;
            this.Seal();
        }
    }
}
