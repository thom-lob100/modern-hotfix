using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Lot 계보 트리 — 기본 필드는 <see cref="LotBase"/>. 부모는 <see cref="ParentLotId"/>, Type · Lot Status 는 배지다.</summary>
    public class LotTreeEntity : LotBase
    {
        public readonly Field ParentLotId = Field("PARENT_LOT_ID").Caption("Parent Lot");
        public readonly Field Qty = Field("QTY").Caption("Qty");
        public readonly Field Description = Field("DESCRIPTION").Caption("Description");

        public LotTreeEntity()
        {
            this.SubProdTyp.Badge();
            this.LotStatTyp.Badge();
            this.ParentField = this.ParentLotId;
            this.Seal();
        }
    }
}
