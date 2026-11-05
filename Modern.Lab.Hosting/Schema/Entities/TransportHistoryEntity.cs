using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Transport 이력(TRANS_HIS) — 위 네 칸은 반송 화면들이 함께 쓰는 칸이고, 아래 다섯은 이력 칸이다. 키는 Durable Id + Timekey.</summary>
    public sealed class TransportHistoryEntity : TableEntity
    {
        public readonly Field DurableId = DurableColumns.DurableId();
        public readonly DurableFields.DurableTyp DurableTyp = DurableColumns.DurableTyp().Badge();
        public readonly Field LotId = LotColumns.LotId();
        public readonly Field Location = CommonColumns.Location();
        public readonly Field TimeKey = CommonColumns.TimeKey();
        public readonly Field EventTm = CommonColumns.EventTm();
        public readonly Field EventCd = CommonColumns.EventCd();
        public readonly Field UserId = CommonColumns.UserId();
        public readonly Field Remark = CommonColumns.Remark();

        public TransportHistoryEntity()
        {
            this.KeyFields = Fields(this.DurableId, this.TimeKey);
            this.ComboDisplayFields = Fields(this.DurableId, this.DurableTyp);
            this.ValueField = this.DurableId;
            this.Seal();
        }
    }
}
