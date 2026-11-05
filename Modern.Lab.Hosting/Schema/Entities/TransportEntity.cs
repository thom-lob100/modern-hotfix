using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Transport History 의 Transport 목록(TRANS_MAS) — 위 네 칸은 반송 화면들이 함께 쓰는 칸이다. 키는 Durable Id.
    /// 마지막 처리(<c>LAST_EVENT_CD</c> · <c>LAST_TIMEKEY</c>)는 이 표에만 있다. 홈의 컬럼은 구조를 보이기 위한 것이라 회사에서 실제 컬럼으로 바꾼다(2026-10-26).</summary>
    public sealed class TransportEntity : TableEntity
    {
        public readonly Field DurableId = DurableColumns.DurableId();
        public readonly DurableFields.DurableTyp DurableTyp = DurableColumns.DurableTyp().Badge();
        public readonly Field LotId = LotColumns.LotId();
        public readonly Field Location = CommonColumns.Location();
        public readonly CommonFields.LastEventCd LastEventCd = CommonColumns.LastEventCd();
        public readonly Field LastTimeKey = Field("LAST_TIMEKEY").Caption("Last Timekey");

        public TransportEntity()
        {
            this.KeyFields = Fields(this.DurableId);
            this.ComboDisplayFields = Fields(this.DurableId, this.DurableTyp);
            this.ValueField = this.DurableId;
            this.Seal();
        }
    }
}
