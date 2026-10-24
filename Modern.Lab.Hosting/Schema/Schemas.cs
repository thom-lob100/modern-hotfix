namespace Modern.Lab.Hosting.Schema
{
    /// <summary>
    /// 공유 엔티티 인스턴스 — 한 번 만들어 모든 화면이 같은 정의를 읽는다. 이 파일은 기준정보(MasterData) 엔티티다.
    /// 업무 화면 엔티티는 <c>Schemas.Business.cs</c>에 있다(회사 컬럼 이름을 담는다).
    /// </summary>
    public static partial class Schemas
    {
        public static ProductEntity Product { get; } = new ProductEntity();

        public static FlowEntity Flow { get; } = new FlowEntity();

        public static OperEntity Oper { get; } = new OperEntity();

        public static LotCodeEntity LotCode { get; } = new LotCodeEntity();

        public static EquipmentGroupEntity EquipmentGroup { get; } = new EquipmentGroupEntity();

        public static AreaEntity Area { get; } = new AreaEntity();

        public static PortMasterEntity PortMaster { get; } = new PortMasterEntity();

        public static UserEntity User { get; } = new UserEntity();

        public static ReasonCodeEntity ReasonCode { get; } = new ReasonCodeEntity();

        public static CommonTypeEntity CommonType { get; } = new CommonTypeEntity();

        public static CommonTypeDetEntity CommonTypeDet { get; } = new CommonTypeDetEntity();

        public static SubCategoryEntity SubCategory { get; } = new SubCategoryEntity();
    }
}
