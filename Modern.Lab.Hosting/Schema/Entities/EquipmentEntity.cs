namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Equipment / Port 의 장비 목록 — 필드는 모두 <see cref="EquipmentBase"/>.</summary>
    public class EquipmentEntity : EquipmentBase
    {
        public EquipmentEntity()
        {
            this.Seal();
        }
    }
}
