namespace Modern.Lab.Hosting.Schema
{
    /// <summary>업무 화면 엔티티 — 회사 컬럼 이름을 담는다.</summary>
    public static partial class Schemas
    {
        public static DurableEntity Durable { get; } = new DurableEntity();

        public static DurableHistoryEntity DurableHistory { get; } = new DurableHistoryEntity();

        public static DurableWaferEntity DurableWafer { get; } = new DurableWaferEntity();

        public static DurableChipEntity DurableChip { get; } = new DurableChipEntity();

        public static DurableLamellaEntity DurableLamella { get; } = new DurableLamellaEntity();

        public static DurableChipHistoryEntity DurableChipHistory { get; } = new DurableChipHistoryEntity();

        public static DurableLamellaHistoryEntity DurableLamellaHistory { get; } = new DurableLamellaHistoryEntity();

        public static LotEntity Lot { get; } = new LotEntity();

        public static WaferEntity Wafer { get; } = new WaferEntity();

        public static LotHistoryEntity LotHistory { get; } = new LotHistoryEntity();

        public static WaferHistoryEntity WaferHistory { get; } = new WaferHistoryEntity();

        public static LotTreeEntity LotTree { get; } = new LotTreeEntity();

        public static EquipmentExtEntity EquipmentExt { get; } = new EquipmentExtEntity();

        public static PortDetEntity PortDet { get; } = new PortDetEntity();

        public static EquipmentExtHistoryEntity EquipmentExtHistory { get; } = new EquipmentExtHistoryEntity();

        public static PortDetHistoryEntity PortDetHistory { get; } = new PortDetHistoryEntity();

        public static TransportEntity Transport { get; } = new TransportEntity();

        public static TransportHistoryEntity TransportHistory { get; } = new TransportHistoryEntity();

        public static RequestEntity Request { get; } = new RequestEntity();

        public static SampleEntity Sample { get; } = new SampleEntity();

        public static JudgeResultEntity JudgeResult { get; } = new JudgeResultEntity();

        public static LogisticsEntity Logistics { get; } = new LogisticsEntity();

        public static RequestWaferEntity RequestWafer { get; } = new RequestWaferEntity();

        public static EquipmentPriorityEntity EquipmentPriority { get; } = new EquipmentPriorityEntity();

        public static PortPriorityEntity PortPriority { get; } = new PortPriorityEntity();

        public static LotPriorityEntity LotPriority { get; } = new LotPriorityEntity();

        public static DurablePriorityEntity DurablePriority { get; } = new DurablePriorityEntity();
    }
}
