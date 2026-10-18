using System.Data;

namespace Modern.Lab.Samples
{
    public sealed class DurableScrapDialogOptions
    {
        public string Title { get; set; }

        public string OkText { get; set; }

        public string DefaultDescription { get; set; }

        public DataRow Source { get; set; }
    }
}
