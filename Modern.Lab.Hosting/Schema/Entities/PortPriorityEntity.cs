using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Equipment / Lots · Lot / Equipment 의 포트 목록 — 위 일곱 칸은 포트 화면들이 함께 쓰는 칸이다. 포트가 받는 Durable 종류는 <see cref="DurableTyp"/>, 순위는 <see cref="Priority"/>.</summary>
    public sealed class PortPriorityEntity : TableEntity
    {
        public readonly Field EqpId = EquipmentColumns.EqpId();
        public readonly Field PortNm = PortColumns.PortNm();
        public readonly PortFields.PortTyp PortTyp = PortColumns.PortTyp().Badge();
        public readonly Field Mode = PortColumns.Mode();
        public readonly PortFields.TransferStatCd TransferStatCd = PortColumns.TransferStatCd();
        public readonly Field LotId = LotColumns.LotId();
        public readonly Field DurableId = DurableColumns.DurableId();
        public readonly DurableFields.DurableTyp DurableTyp = DurableColumns.DurableTyp();
        public readonly Field Priority = CommonColumns.Priority();

        public PortPriorityEntity()
        {
            this.KeyFields = Fields(this.EqpId, this.PortNm);
            this.ComboDisplayFields = Fields(this.PortNm, this.PortTyp);
            this.ValueField = this.PortNm;
            this.Seal();
        }
    }
}
