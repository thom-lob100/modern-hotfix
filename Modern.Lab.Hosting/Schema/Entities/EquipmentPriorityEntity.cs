using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Equipment / Lots · Lot / Equipment 의 장비 목록 — 위 일곱 칸은 장비 화면들이 함께 쓰는 칸이다.
    /// Job 역할 필드는 소스 Port <see cref="PortNm"/>·대상 Port <see cref="GoalPortNm"/>, 순위는 <see cref="Priority"/>.</summary>
    public sealed class EquipmentPriorityEntity : TableEntity
    {
        public readonly Field EqpId = EquipmentColumns.EqpId();
        public readonly Field EqpGrpId = EquipmentColumns.EqpGrpId();
        public readonly Field ParentEqpId = EquipmentColumns.ParentEqpId();
        public readonly EquipmentFields.CommStatTyp CommStatTyp = EquipmentColumns.CommStatTyp();
        public readonly EquipmentFields.MesEqpStatCd MesEqpStatCd = EquipmentColumns.MesEqpStatCd();
        public readonly EquipmentFields.AutoYn AutoYn = EquipmentColumns.AutoYn();
        public readonly Field Location = CommonColumns.Location();
        public readonly Field EqpIdAlias = Field("EQP_ID_ALIAS").Caption("Alias");
        public readonly EquipmentFields.MesStatTyp MesStatTyp = EquipmentColumns.MesStatTyp();
        public readonly EquipmentFields.EqpTyp EqpTyp = EquipmentColumns.EqpTyp().Caption("Equipment Type");
        public readonly EquipmentFields.DetEqpTyp DetEqpTyp = Field<EquipmentFields.DetEqpTyp>("DET_EQP_TYP").Caption("Equipment Detail Type");
        public readonly Field Priority = CommonColumns.Priority();
        public readonly Field PortNm = PortColumns.PortNm();
        public readonly Field GoalPortNm = CommonColumns.GoalPortNm();

        public EquipmentPriorityEntity()
        {
            this.KeyFields = Fields(this.EqpId);
            this.ParentField = this.ParentEqpId;
            this.ComboDisplayFields = Fields(this.EqpId, this.CommStatTyp);
            this.ValueField = this.EqpId;
            this.Seal();
        }
    }
}
