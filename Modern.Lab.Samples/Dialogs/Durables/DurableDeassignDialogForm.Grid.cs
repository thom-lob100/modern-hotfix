using System.Data;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Samples
{
    public partial class DurableDeassignDialogForm
    {
        private static GridColumns ContentColumns(DataTable contents)
        {
            return Schemas.DurableWafer.Columns(contents)
                    .Column(Schemas.DurableWafer.WfId.Name).Caption("Wafer Id");
        }
    }
}
