using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Lab Monitor 의 장비 한 줄(<c>GetMonitorEqpList</c>, 홈 이름) — 위 일곱 칸은 장비 화면들이 함께 쓰는 칸이다. 어느 단계의 장비인지 <see cref="MonStage"/>,
    /// 지금 돌고 있는 랏 <see cref="LotId"/> · 예약 랏 <see cref="ReservedLotId"/> · 상태가 바뀐 시각 <see cref="StatusTm"/>. 핸들러의 자식(F1~F4)은 <c>PARENT_EQP_ID</c> 로 묶인다(2026-11-04).</summary>
    public sealed class MonitorEquipmentEntity : TableEntity
    {
        public readonly Field EqpId = EquipmentColumns.EqpId();
        public readonly Field EqpGrpId = EquipmentColumns.EqpGrpId();
        public readonly Field ParentEqpId = EquipmentColumns.ParentEqpId();
        public readonly EquipmentFields.CommStatTyp CommStatTyp = EquipmentColumns.CommStatTyp();
        public readonly EquipmentFields.MesEqpStatCd MesEqpStatCd = EquipmentColumns.MesEqpStatCd();
        public readonly EquipmentFields.AutoYn AutoYn = EquipmentColumns.AutoYn();
        public readonly Field Location = CommonColumns.Location();
        public readonly MonitorFields.MonStage MonStage = MonitorColumns.MonStage();
        public readonly Field EqpNm = Field("EQP_NM").Caption("Name");
        public readonly Field StatusTm = Field("STATUS_TM").Caption("Since");
        public readonly Field LotId = LotColumns.LotId();
        public readonly Field ReservedLotId = Field("RESERVED_LOT_ID").Caption("Reserved Lot");

        public MonitorEquipmentEntity()
        {
            this.KeyFields = Fields(this.EqpId);
            this.ParentField = this.ParentEqpId;
            this.ComboDisplayFields = Fields(this.EqpId, this.CommStatTyp);
            this.ValueField = this.EqpId;
            this.Seal();
        }
    }
}
