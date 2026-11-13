using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class EquipmentGroupEntity : TableEntity
    {
        public readonly Field EqpGrpId = EquipmentColumns.EqpGrpId();
        public readonly Field EqpGrpTyp = Field("EQP_GRP_TYP").Caption("Group Type");
        public readonly Field Description = CommonColumns.Description();

        public EquipmentGroupEntity()
        {
            this.KeyFields = Fields(this.EqpGrpId);
            this.ComboDisplayFields = Fields(this.EqpGrpId);
            this.ValueField = this.EqpGrpId;
            this.Seal();
        }
    }
}
