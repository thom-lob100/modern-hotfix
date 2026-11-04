using System.Data;
using Modern.Lab.WinForms.Controls.Display;

namespace Modern.Lab.Samples
{
    public sealed class DurableScrapDialogOptions
    {
        public string Title { get; set; }

        public ModernFieldDefinition[] Fields { get; set; }

        public string OkText { get; set; }

        public string DefaultDescription { get; set; }

        public DataRow Source { get; set; }
    }
}
