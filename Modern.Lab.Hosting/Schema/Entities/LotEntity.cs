using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Lot 목록 · Lot Info — 기본 필드는 <see cref="LotBase"/>. 아래쪽 일곱 컬럼(<c>MODEL_ID</c> …)은 들어올 Lot 카드(Create Wafer · Logistics 수신)가 쓴다.</summary>
    public class LotEntity : LotBase
    {
        public readonly Field Qty = Field("QTY").Caption("Qty");
        public readonly Field CreateTm = Field("CREATE_TM").Caption("Create Time");
        public readonly Field SendFac = Field("SEND_FAC").Caption("Send Fac");
        public readonly LocNmField LocNm = Field<LocNmField>("LOC_NM").Caption("Location");
        public readonly Field EqpId = Field("EQP_ID").Caption("Eqp Id");
        public readonly Field Description = Field("DESCRIPTION").Caption("Description");
        public readonly Field ModelId = Field("MODEL_ID").Caption("Product");
        public readonly Field StatTyp = Field("STAT_TYP").Caption("Status");
        public readonly Field EventCd = Field("EVENT_CD").Caption("Event");
        public readonly Field BoxId = Field("BOX_ID").Caption("Carrier");
        public readonly Field StoreId = Field("STORE_ID").Caption("Stocker");
        public readonly Field EventTm = Field("EVENT_TM").Caption("Event Time");
        public readonly Field MesStat = Field("MES_STAT").Caption("Mes Status");

        public LotEntity()
        {
            this.Seal();
        }
    }
}
