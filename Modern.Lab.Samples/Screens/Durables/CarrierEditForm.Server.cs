using System;
using System.Collections.Generic;
using System.Data;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Samples.Contracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class CarrierEditForm
    {
        // DURABLE_ID
        // LOT_QTY
        // CAPA
        // FOUP_LOT_QTY
        // FOUP_CAPA
        // STUB_LOT_QTY
        // STUB_CAPA
        // LCC_LOT_QTY
        // LCC_CAPA
        private DataTable GetDurables(string durableTyp)
        {
            return this.RequestFields(
                    "GetDurableList", "DURABLE_TYP", durableTyp ?? string.Empty).Table;
        }

        // SUB_PROD_TYP
        // SLOT_NO
        // WF_ID
        // FINGER_ID
        private DataTable GetDurableWafers(string durableId)
        {
            return this.RequestFields(
                    "GetDurableSlotList",
                    "DURABLE_ID", durableId ?? string.Empty,
                    "SUB_PROD_TYP", string.Empty).Table;
        }

        private DataActionResult MoveDurableWafers(
                DataRow source, DataRow target, string durableTyp, DataTable plan, string description)
        {
            if (string.Equals(durableTyp, "TRAY", StringComparison.Ordinal))
            {
                string chip = "Chip";
                string lamella = "Lamella";

                return this.RequestFields(
                        "EditDurable",
                        "SOURCE_ID", TableHelper.CellText(source, "DURABLE_ID").Trim(),
                        "TARGET_ID", TableHelper.CellText(target, "DURABLE_ID").Trim(),
                        "SOURCE_STUB_SLOT_NOS", PlanList(plan, chip, "SOURCE_SLOT"),
                        "SOURCE_CHIP_IDS", PlanList(plan, chip, "WF_ID"),
                        "TARGET_STUB_SLOT_NOS", PlanList(plan, chip, "TARGET_SLOT"),
                        "TARGET_CHIP_IDS", PlanList(plan, chip, "WF_ID"),
                        "SOURCE_LCC_SLOT_NOS", PlanList(plan, lamella, "SOURCE_SLOT"),
                        "SOURCE_LAMELLA_IDS", PlanList(plan, lamella, "WF_ID"),
                        "TARGET_LCC_SLOT_NOS", PlanList(plan, lamella, "TARGET_SLOT"),
                        "TARGET_LAMELLA_IDS", PlanList(plan, lamella, "WF_ID"),
                        "DESCRIPTION", description);
            }

            string wafer = "Wafer";

            return this.RequestFields(
                    "EditDurable",
                    "SOURCE_ID", TableHelper.CellText(source, "DURABLE_ID").Trim(),
                    "TARGET_ID", TableHelper.CellText(target, "DURABLE_ID").Trim(),
                    "SOURCE_SLOT_NOS", PlanList(plan, wafer, "SOURCE_SLOT"),
                    "SOURCE_WAFER_IDS", PlanList(plan, wafer, "WF_ID"),
                    "TARGET_SLOT_NOS", PlanList(plan, wafer, "TARGET_SLOT"),
                    "TARGET_WAFER_IDS", PlanList(plan, wafer, "WF_ID"),
                    "DESCRIPTION", description);
        }

        private DataActionResult ExchangeDurableWafers(
                DataRow source, DataRow target, DataTable wafers, DataTable plan, string description)
        {
            string wafer = "Wafer";
            string chip = "Chip";
            string lamella = "Lamella";

            return this.RequestFields(
                    "ExchangeDurable",
                    "SOURCE_ID", TableHelper.CellText(source, "DURABLE_ID").Trim(),
                    "TARGET_ID", TableHelper.CellText(target, "DURABLE_ID").Trim(),
                    "LOT_ID", DistinctLotIds(wafers)[0],
                    "WAFER_IDS", PlanList(plan, wafer, "WF_ID"),
                    "SLOT_NOS", PlanList(plan, wafer, "SOURCE_SLOT"),
                    "TARGET_SLOT_NOS", PlanList(plan, wafer, "TARGET_SLOT"),
                    "CHIP_IDS", PlanList(plan, chip, "WF_ID"),
                    "STUB_SLOT_NOS", PlanList(plan, chip, "SOURCE_SLOT"),
                    "TARGET_STUB_SLOT_NOS", PlanList(plan, chip, "TARGET_SLOT"),
                    "LAMELLA_IDS", PlanList(plan, lamella, "WF_ID"),
                    "LCC_SLOT_NOS", PlanList(plan, lamella, "SOURCE_SLOT"),
                    "TARGET_LCC_SLOT_NOS", PlanList(plan, lamella, "TARGET_SLOT"),
                    "DESCRIPTION", description);
        }

        private DataActionResult AlignDurableWafers(DataRow durable, DataTable plan, string description)
        {
            string wafer = "Wafer";
            string chip = "Chip";
            string lamella = "Lamella";

            return this.RequestFields(
                    "AlignDurable",
                    "DURABLE_ID", Schemas.Durable.DurableId.Text(durable),
                    "WAFER_IDS", PlanList(plan, wafer, "WF_ID"),
                    "SOURCE_SLOT_NOS", PlanList(plan, wafer, "SOURCE_SLOT"),
                    "TARGET_SLOT_NOS", PlanList(plan, wafer, "TARGET_SLOT"),
                    "CHIP_IDS", PlanList(plan, chip, "WF_ID"),
                    "SOURCE_STUB_SLOT_NOS", PlanList(plan, chip, "SOURCE_SLOT"),
                    "TARGET_STUB_SLOT_NOS", PlanList(plan, chip, "TARGET_SLOT"),
                    "LAMELLA_IDS", PlanList(plan, lamella, "WF_ID"),
                    "SOURCE_LCC_SLOT_NOS", PlanList(plan, lamella, "SOURCE_SLOT"),
                    "TARGET_LCC_SLOT_NOS", PlanList(plan, lamella, "TARGET_SLOT"),
                    "DESCRIPTION", description);
        }

        private static string PlanList(DataTable plan, string subProdTyp, string columnName)
        {
            return Join(plan, subProdTyp, columnName);
        }

        private static string Join(DataTable wafers, string subProdTyp, string columnName)
        {
            List<string> values = new List<string>();

            if (wafers != null)
            {
                foreach (DataRow row in wafers.Rows)
                {
                    string rowSubProdTyp = TableHelper.CellText(
                            row, "SUB_PROD_TYP").Trim();

                    if (!string.Equals(rowSubProdTyp, subProdTyp, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (TableHelper.CellText(row, "WF_ID").Trim().Length == 0)
                    {
                        continue;
                    }

                    values.Add(TableHelper.CellText(row, columnName).Trim());
                }
            }

            return string.Join(",", values);
        }
    }
}
