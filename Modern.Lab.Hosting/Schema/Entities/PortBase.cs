using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>
    /// Port 가족(기준정보 <see cref="PortEntity"/> · <see cref="PortDetEntity"/> · <see cref="PortDetHistoryEntity"/> · <see cref="PortPriorityEntity"/>)이
    /// 함께 쓰는 필드 — 회사 테이블 기준으로 "둘 이상의 가족 엔티티가 쓰는 컬럼"을 둔다(2026-10-25 사용자 결정). 식별 컬럼과 상태 컬럼이 함께 있고,
    /// 상태 테이블(Det)에만 있는 <c>LAST_…</c> 류는 <see cref="PortDetEntity"/> 에, 이력에만 있는 <c>TIMEKEY</c> 류는 <see cref="PortDetHistoryEntity"/> 에 둔다.
    /// 기준정보 화면은 상태 컬럼이 응답에 없어 표에 나타나지 않는다. 회사 컬럼 이름·캡션이 다르면 여기 한 곳만 고친다.
    /// 파생 엔티티는 자기 화면에만 있는 필드를 추가하고, 캡션·배지를 바꿀 때는 생성자에서 <c>Seal()</c> 전에 재정의한다 —
    /// 같은 컬럼을 다시 선언하면 <c>Seal</c> 이 "두 필드에 선언됐다"로 막는다. 응답에 없는 기본 필드는 표·카드에 나타나지 않는다.
    /// </summary>
    public abstract class PortBase : EntityDefinition
    {
        public readonly Field EqpId = Field("EQP_ID").Caption("Eqp Id");
        public readonly Field PortNm = Field("PORT_NM").Caption("Port");
        public readonly PortTypField PortTyp = Field<PortTypField>("PORT_TYP").Caption("Type").Badge();
        public readonly Field Mode = Field("MODE").Caption("Mode").Badge();
        public readonly TransferStatField TransferStatCd = Field<TransferStatField>("TRANSFER_STAT_CD").Caption("Status").Badge();
        public readonly Field LotId = Field("LOT_ID").Caption("Lot Id");
        public readonly Field DurableId = Field("DURABLE_ID").Caption("Durable Id");

        /// <summary>기본값 — 파생이 바꾸지 않으면 Eqp Id + Port 가 키, Port 가 값이고 콤보는 Port · Type 을 보인다.</summary>
        protected PortBase()
        {
            this.KeyFields = Fields(this.EqpId, this.PortNm);
            this.ComboDisplayFields = Fields(this.PortNm, this.PortTyp);
            this.ValueField = this.PortNm;
        }
    }
}
