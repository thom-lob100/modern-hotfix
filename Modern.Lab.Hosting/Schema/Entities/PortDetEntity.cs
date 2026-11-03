using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>
    /// Equipment / Port 의 포트 목록 — 기본 필드는 <see cref="PortBase"/>. 아래 다섯은 Durable Manual In 창의 포트 후보 조건
    /// (<c>PORT_TYP</c> Input · InputOutput, <c>ACSS_TYP</c> Auto, <c>PORT_STAT_NM</c> Up, <c>RESRC_STAT_TYP</c> InService,
    /// <c>TRANSFER_STAT_CD</c> ReadyToLoad, <c>CURR_DURABLE_ID</c> 빈 값, <c>VIRT_PORT_YN</c> N, 2026-11-02)이 읽는다.
    /// </summary>
    public class PortDetEntity : PortBase
    {
        public readonly AcssTypField AcssTyp = Field<AcssTypField>("ACSS_TYP").Caption("Access").Badge();
        public readonly PortStatNmField PortStatNm = Field<PortStatNmField>("PORT_STAT_NM").Caption("Port Status").Badge();
        public readonly ResrcStatTypField ResrcStatTyp = Field<ResrcStatTypField>("RESRC_STAT_TYP").Caption("Resource");
        public readonly Field CurrDurableId = Field("CURR_DURABLE_ID").Caption("Current Durable");
        public readonly VirtPortYnField VirtPortYn = Field<VirtPortYnField>("VIRT_PORT_YN").Caption("Virtual");

        public PortDetEntity()
        {
            this.Seal();
        }
    }
}
