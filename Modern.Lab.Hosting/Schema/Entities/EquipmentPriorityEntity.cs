using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class EquipmentPriorityEntity : EntityDefinition
    {
        public readonly Field EqpId = Field("EQP_ID").Caption("Eqp Id");
        public readonly Field EqpIdAlias = Field("EQP_ID_ALIAS").Caption("Alias");
        public readonly Field EqpGrpId = Field("EQP_GRP_ID").Caption("Group").TopLabel("Equipment").HeaderAlignment(GridTextAlignment.Center).TopLabelAlignment(GridTextAlignment.Center);
        public readonly Field EqpNm = Field("EQP_NM").Caption("Eqp Name");
        public readonly Field ParentEqpId = Field("PARENT_EQP_ID").Caption("Parent Eqp");
        public readonly CommStatTypField CommStatTyp = Field<CommStatTypField>("COMM_STAT_TYP").Caption("Mode").Badge();
        public readonly MesEqpStatField MesEqpStatCd = Field<MesEqpStatField>("MES_EQP_STAT_CD").Caption("Status").Badge();
        public readonly EqpMesStatTypField MesStatTyp = Field<EqpMesStatTypField>("MES_STAT_TYP").Caption("MES Status");
        public readonly EqpTypField EqpTyp = Field<EqpTypField>("EQP_TYP").Caption("Equipment Type");
        public readonly DetEqpTypField DetEqpTyp = Field<DetEqpTypField>("DET_EQP_TYP").Caption("Equipment Detail Type");
        public readonly AutoYnField AutoYn = Field<AutoYnField>("AUTO_YN").Caption("Auto");
        public readonly Field Priority = Field("PRIORITY").Caption("Prio");
        public readonly Field PortNm = Field("PORT_NM").Caption("Port");
        public readonly Field GoalPortNm = Field("GOAL_PORT_NM").Caption("Goal Port");
        public readonly Field Location = Field("LOCATION").Caption("Location");

        public EquipmentPriorityEntity()
        {
            this.KeyFields = Fields(this.EqpId);
            this.Seal();
        }
    }
}
