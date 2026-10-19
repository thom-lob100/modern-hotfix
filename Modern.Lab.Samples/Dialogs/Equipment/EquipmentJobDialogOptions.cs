using System.Data;
using Modern.Lab.WinForms.Controls.Display;

namespace Modern.Lab.Samples
{
    public sealed class EquipmentJobDialogOptions
    {
        public string Title { get; set; }

        public string OkText { get; set; }

        public string OkTopLabel { get; set; }

        public DataRow Source { get; set; }

        public Services.JobDecision Target { get; set; }

        public Services.JobExecutionTarget ExecutionTarget { get; set; }

        public ModernFieldDefinition[] SourceFields { get; set; }

        public string SourceCaption { get; set; }

        public string DefaultDescription { get; set; }

        public string ProdId { get; set; }

        public string FlowId { get; set; }

        public string OperId { get; set; }
    }
}
