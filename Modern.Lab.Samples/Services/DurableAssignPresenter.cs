using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Samples.Services
{
    internal static class DurableAssignPresenter
    {
        internal static List<int> FreeSlots(DataRow durable, DataTable contents)
        {
            int capacity;

            if (durable == null
                    || !int.TryParse(TableHelper.CellText(durable, Schemas.Durable.Capa.Name), NumberStyles.Integer,
                            CultureInfo.InvariantCulture, out capacity)
                    || capacity <= 0)
            {
                return null;
            }

            HashSet<int> occupied = new HashSet<int>();

            if (contents != null)
            {
                foreach (DataRow row in contents.Rows)
                {
                    int slot;

                    if (int.TryParse(Schemas.DurableWafer.SlotNo.Text(row), NumberStyles.Integer, CultureInfo.InvariantCulture, out slot))
                    {
                        occupied.Add(slot);
                    }
                }
            }

            List<int> free = new List<int>();

            for (int slot = 1; slot <= capacity; slot++)
            {
                if (!occupied.Contains(slot))
                {
                    free.Add(slot);
                }
            }

            return free;
        }

        internal static DataTable BuildPlan(DataTable wafers, List<int> freeSlots)
        {
            DataTable plan = new DataTable();
            plan.Locale = CultureInfo.InvariantCulture;
            plan.Columns.Add(Schemas.Wafer.WfId.Name, typeof(string));
            plan.Columns.Add(Schemas.Wafer.SlotNo.Name, typeof(string));

            if (wafers == null)
            {
                return plan;
            }

            int next = 0;

            foreach (DataRow wafer in wafers.Rows)
            {
                string wfId = Schemas.Wafer.WfId.Text(wafer);

                if (wfId.Length == 0)
                {
                    continue;
                }

                string slot;

                if (freeSlots == null)
                {
                    slot = Schemas.Wafer.SlotNo.Text(wafer);
                }
                else if (next < freeSlots.Count)
                {
                    slot = freeSlots[next].ToString(CultureInfo.InvariantCulture);
                    next++;
                }
                else
                {
                    slot = string.Empty;
                }

                plan.Rows.Add(wfId, slot);
            }

            return plan;
        }

        internal static string FirstWithoutSlot(DataTable plan)
        {
            if (plan == null)
            {
                return null;
            }

            foreach (DataRow row in plan.Rows)
            {
                if (Schemas.Wafer.SlotNo.Text(row).Length == 0)
                {
                    return Schemas.Wafer.WfId.Text(row);
                }
            }

            return null;
        }

        internal static string JoinColumn(DataTable plan, Field field)
        {
            List<string> values = new List<string>();

            if (plan != null)
            {
                foreach (DataRow row in plan.Rows)
                {
                    values.Add(field.Text(row));
                }
            }

            return string.Join(",", values);
        }
    }
}
