using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Lab Monitor 의 지속재 한 줄(<c>GetMonitorDurableList</c>, 홈 이름) — FOUP · 트레이 하나가 한 줄이고 화면은 줄을 센다.
    /// 위치 종류 <see cref="LocKind"/>(STB · STOCKER · EQP · OTHER) · 든 랏 수 <see cref="LotCnt"/>(2 이상이면 섞인 FOUP) · 원 공장 <see cref="FromFac"/> ·
    /// FAB FOUP 의 반송 여부 <see cref="ReturnYn"/> 와 교체 교환 여부 <see cref="SwapYn"/> 는 서버 칸이다(2026-11-04).
    /// <see cref="LocNm"/> 은 위치 이름이라 Durable 의 위치 종류(<c>LOC_NM</c>, InStocker 등)와 뜻이 달라 사전을 쓰지 않는다.</summary>
    public sealed class MonitorDurableEntity : TableEntity
    {
        public readonly Field DurableId = DurableColumns.DurableId();
        public readonly DurableFields.DurableTyp DurableTyp = DurableColumns.DurableTyp().Badge();
        public readonly DurableFields.LocKind LocKind = Field<DurableFields.LocKind>("LOC_KIND").Caption("Location").Badge();
        public readonly Field LocNm = Field("LOC_NM").Caption("Location Name");
        public readonly Field LotCnt = Field("LOT_CNT").Caption("Lots");
        public readonly Field WfQty = Field("WF_QTY").Caption("Wafers");
        public readonly Field FromFac = DurableColumns.FromFac().Caption("From Fab");
        public readonly CommonFields.Yn ReturnYn = Field<CommonFields.Yn>("RETURN_YN").Caption("Returned");
        public readonly CommonFields.Yn SwapYn = Field<CommonFields.Yn>("SWAP_YN").Caption("Swapped");
        public readonly Field EventTm = CommonColumns.EventTm();

        public MonitorDurableEntity()
        {
            this.KeyFields = Fields(this.DurableId);
            this.ComboDisplayFields = Fields(this.DurableId, this.DurableTyp);
            this.ValueField = this.DurableId;
            this.Seal();
        }
    }
}
