using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>
    /// Transport 가족(<see cref="TransportEntity"/> · <see cref="TransportHistoryEntity"/>)이 함께 쓰는 필드 —
    /// 회사 테이블(TRANS_MAS · TRANS_HIS) 기준으로 둘 이상의 가족 엔티티가 쓰는 컬럼을 둔다(2026-10-26).
    /// 홈의 컬럼은 구조를 보이기 위한 것이라 회사에서 실제 컬럼으로 바꾼다. 한 엔티티만 쓰는 컬럼(TRANS_MAS 의 <c>LAST_…</c>,
    /// TRANS_HIS 의 <c>TIMEKEY</c> 류)은 그 엔티티에 둔다. 파생은 생성자 끝에서 <c>Seal()</c> 을 한 번 부른다.
    /// </summary>
    public abstract class TransportBase : EntityDefinition
    {
        public readonly Field DurableId = Field("DURABLE_ID").Caption("Durable Id");
        public readonly DurableTypField DurableTyp = Field<DurableTypField>("DURABLE_TYP").Caption("Type").Badge();
        public readonly Field LotId = Field("LOT_ID").Caption("Lot Id");
        public readonly Field Location = Field("LOCATION").Caption("Location");

        /// <summary>기본값 — 파생이 바꾸지 않으면 Durable Id 가 키·값이고 콤보는 Durable Id · Type 을 보인다.</summary>
        protected TransportBase()
        {
            this.KeyFields = Fields(this.DurableId);
            this.ComboDisplayFields = Fields(this.DurableId, this.DurableTyp);
            this.ValueField = this.DurableId;
        }
    }
}
