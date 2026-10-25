namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Equipment / Port 의 포트 목록 — 필드는 모두 <see cref="PortBase"/>.</summary>
    public class PortExtEntity : PortBase
    {
        public PortExtEntity()
        {
            this.Seal();
        }
    }
}
