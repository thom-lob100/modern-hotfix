using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class DurablePriorityEntity : EntityDefinition
    {
        public readonly Field DurableId = Field("DURABLE_ID").Caption("Durable Id");
        public readonly DurableTypField DurableTyp = Field<DurableTypField>("DURABLE_TYP").Caption("Type");
        public readonly DurableStatField DurableStatCd = Field<DurableStatField>("DURABLE_STAT_CD").Caption("Mode").Badge();
        public readonly WfLoadStatField WfLoadStatCd = Field<WfLoadStatField>("WF_LOAD_STAT_CD").Caption("Status").Badge();
        public readonly MesStatTypField MesStatTyp = Field<MesStatTypField>("MES_STAT_TYP").Caption("MES Status");
        public readonly CleanTypField CleanTyp = Field<CleanTypField>("CLEAN_TYP").Caption("Clean Type");
        public readonly Field LotQty = Field("LOT_QTY").Caption("Qty");
        public readonly Field Capa = Field("CAPA").Caption("Capa");
        public readonly Field Priority = Field("PRIORITY").Caption("Prio");
        public readonly Field Location = Field("LOCATION").Caption("Location");

        public DurablePriorityEntity()
        {
            this.KeyFields = Fields(this.DurableId);
            this.ComboDisplayFields = Fields(this.DurableId, this.Priority);
            this.ValueField = this.DurableId;
            this.Seal();
        }
    }
}
