using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Lab Monitor 의 지속재 한 줄(<c>GetMonitorDurableList</c>, 홈 이름) — FOUP · 트레이 하나가 한 줄이고 화면은 줄을 센다.
    /// 위치 종류 <see cref="LocKind"/>(STB · STOCKER · EQP · OTHER) · 든 랏 수 <see cref="LotCnt"/>(2 이상이면 섞인 FOUP) · 원 공장 <see cref="FromFac"/> ·
    /// FAB FOUP 의 반송 여부 <see cref="ReturnYn"/> 와 교체 교환 여부 <see cref="SwapYn"/> 는 서버 칸이다(2026-11-04).</summary>
    public class MonitorDurableEntity : EntityDefinition
    {
        public readonly Field DurableId = Field("DURABLE_ID").Caption("Durable Id");
        public readonly DurableTypField DurableTyp = Field<DurableTypField>("DURABLE_TYP").Caption("Type").Badge();
        public readonly MonitorLocKindField LocKind = Field<MonitorLocKindField>("LOC_KIND").Caption("Location").Badge();
        public readonly Field LocNm = Field("LOC_NM").Caption("Location Name");
        public readonly Field LotCnt = Field("LOT_CNT").Caption("Lots");
        public readonly Field WfQty = Field("WF_QTY").Caption("Wafers");
        public readonly Field FromFac = Field("FROM_FAC").Caption("From Fab");
        public readonly YnField ReturnYn = Field<YnField>("RETURN_YN").Caption("Returned");
        public readonly YnField SwapYn = Field<YnField>("SWAP_YN").Caption("Swapped");
        public readonly Field EventTm = Field("EVENT_TM").Caption("Event Time");

        public MonitorDurableEntity()
        {
            this.KeyFields = Fields(this.DurableId);
            this.ComboDisplayFields = Fields(this.DurableId, this.DurableTyp);
            this.ValueField = this.DurableId;
            this.Seal();
        }
    }
}
