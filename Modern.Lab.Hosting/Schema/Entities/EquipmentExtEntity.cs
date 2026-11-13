using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>
    /// Equipment / Port 의 장비 목록 — 위 일곱 칸은 장비 화면들이 함께 쓰는 칸이다. 아래 셋은 Durable Manual In 창의 장비 후보 조건
    /// (<c>COMM_STAT_TYP</c> OnLineLocal · <c>MCS_ITG_YN</c> Y · <c>MES_STAT_TYP</c> Up · <c>EQP_TYP</c> ProductionMachine, 2026-11-02)이 읽는다.
    /// 대분류 <c>EQP_MAIN_GRP_ID</c> 는 Durable Change Destination 창이 장비 표에서 STB · Stocker 를 가를 때 읽는다(2026-11-12).
    /// </summary>
    public sealed class EquipmentExtEntity : TableEntity
    {
        public readonly Field EqpId = EquipmentColumns.EqpId();
        public readonly Field EqpGrpId = EquipmentColumns.EqpGrpId();
        public readonly Field ParentEqpId = EquipmentColumns.ParentEqpId();
        public readonly EquipmentFields.CommStatTyp CommStatTyp = EquipmentColumns.CommStatTyp();
        public readonly EquipmentFields.MesEqpStatCd MesEqpStatCd = EquipmentColumns.MesEqpStatCd();
        public readonly EquipmentFields.AutoYn AutoYn = EquipmentColumns.AutoYn();
        public readonly Field Location = CommonColumns.Location();
        public readonly CommonFields.McsItgYn McsItgYn = CommonColumns.McsItgYn();
        public readonly EquipmentFields.MesStatTyp MesStatTyp = EquipmentColumns.MesStatTyp().Caption("Mes Status").Badge();
        public readonly EquipmentFields.EqpTyp EqpTyp = EquipmentColumns.EqpTyp();
        public readonly EquipmentFields.EqpMainGrpId EqpMainGrpId = Field<EquipmentFields.EqpMainGrpId>("EQP_MAIN_GRP_ID").Caption("Main Group");

        public EquipmentExtEntity()
        {
            this.KeyFields = Fields(this.EqpId);
            this.ParentField = this.ParentEqpId;
            this.ComboDisplayFields = Fields(this.EqpId, this.CommStatTyp);
            this.ValueField = this.EqpId;
            this.Seal();
        }
    }
}
