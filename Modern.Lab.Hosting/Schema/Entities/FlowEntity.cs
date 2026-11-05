using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class FlowEntity : TableEntity
    {
        public readonly Field FlowId = CommonColumns.FlowId();
        public readonly Field FlowName = Field("FLOW_NAME");
        public readonly Field FlowVersion = Field("FLOW_VERSION");
        public readonly Field StepCount = Field("STEP_COUNT");
        public readonly Field Owner = Field("OWNER");
        public readonly Field UseYn = CommonColumns.UseYn();
        public readonly Field Remark = CommonColumns.Remark();

        public FlowEntity()
        {
            this.KeyFields = Fields(this.FlowId);
            this.ComboDisplayFields = Fields(this.FlowId);
            this.ValueField = this.FlowId;
            this.Seal();
        }
    }
}
