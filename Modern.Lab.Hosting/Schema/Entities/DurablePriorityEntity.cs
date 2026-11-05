using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class DurablePriorityEntity : TableEntity
    {
        public readonly Field DurableId = DurableColumns.DurableId();
        public readonly DurableFields.DurableTyp DurableTyp = DurableColumns.DurableTyp();
        public readonly DurableFields.DurableStatCd DurableStatCd = DurableColumns.DurableStatCd();
        public readonly WaferFields.WfLoadStatCd WfLoadStatCd = WaferColumns.WfLoadStatCd();
        public readonly DurableFields.MesStatTyp MesStatTyp = DurableColumns.MesStatTyp();
        public readonly DurableFields.CleanTyp CleanTyp = Field<DurableFields.CleanTyp>("CLEAN_TYP").Caption("Clean Type");
        public readonly Field LotQty = LotColumns.LotQty();
        public readonly Field Capa = DurableColumns.Capa();
        public readonly Field Priority = CommonColumns.Priority();
        public readonly Field Location = CommonColumns.Location();

        public DurablePriorityEntity()
        {
            this.KeyFields = Fields(this.DurableId);
            this.ComboDisplayFields = Fields(this.Priority, this.DurableId);
            this.DisplayField = this.DurableId;
            this.ValueField = this.DurableId;
            this.Seal();
        }
    }
}
