using System.Data;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Samples
{
    public partial class DurableAssignDialogForm
    {
        private static ModernDataGridColumn[] PlanColumns(DataTable plan)
        {
            ModernDataGridColumn[] built = Schemas.Wafer.Columns(plan)
                    .Column(Schemas.Wafer.SlotNo.Name).Width(GridWidths.Status)
                    .ToArray();

            foreach (ModernDataGridColumn column in built)
            {
                column.ReadOnly = column.DataPropertyName != Schemas.Wafer.SlotNo.Name;
            }

            return built;
        }
    }
}
