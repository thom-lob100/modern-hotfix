using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Equipment / Lots · Lot / Equipment 의 포트 목록 — 기본 필드는 <see cref="PortBase"/>. 포트가 받는 Durable 종류는 <see cref="DurableTyp"/>, 순위는 <see cref="Priority"/>.</summary>
    public class PortPriorityEntity : PortBase
    {
        public readonly DurableTypField DurableTyp = Field<DurableTypField>("DURABLE_TYP").Caption("Type");
        public readonly Field Priority = Field("PRIORITY").Caption("Prio");

        public PortPriorityEntity()
        {
            this.Seal();
        }
    }
}
