using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>장비 이력 — 위 일곱 칸은 장비 화면들이 함께 쓰는 칸이고, 아래 다섯은 이력 칸이다. 키는 Eqp Id + Timekey.</summary>
    public sealed class EquipmentExtHistoryEntity : TableEntity
    {
        public readonly Field EqpId = EquipmentColumns.EqpId();
        public readonly Field EqpGrpId = EquipmentColumns.EqpGrpId();
        public readonly Field ParentEqpId = EquipmentColumns.ParentEqpId();
        public readonly EquipmentFields.CommStatTyp CommStatTyp = EquipmentColumns.CommStatTyp();
        public readonly EquipmentFields.MesEqpStatCd MesEqpStatCd = EquipmentColumns.MesEqpStatCd();
        public readonly EquipmentFields.AutoYn AutoYn = EquipmentColumns.AutoYn();
        public readonly Field Location = CommonColumns.Location();
        public readonly Field TimeKey = CommonColumns.TimeKey();
        public readonly Field EventTm = CommonColumns.EventTm();
        public readonly Field EventCd = CommonColumns.EventCd();
        public readonly Field UserId = CommonColumns.UserId();
        public readonly Field Remark = CommonColumns.Remark();

        public EquipmentExtHistoryEntity()
        {
            this.KeyFields = Fields(this.EqpId, this.TimeKey);
            this.ParentField = this.ParentEqpId;
            this.ComboDisplayFields = Fields(this.EqpId, this.CommStatTyp);
            this.ValueField = this.EqpId;
            this.Seal();
        }
    }
}
