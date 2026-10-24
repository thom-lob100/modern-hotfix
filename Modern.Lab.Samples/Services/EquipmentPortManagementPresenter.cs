using System;
using System.Collections.Generic;
using System.Data;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting.ResponseContracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.Samples.Contracts;
using Modern.Lab.Hosting.NewItems;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples.Services
{
    public static class EquipmentPortManagementPresenter
    {
        public const string ActionAuto = "Auto";

        public const string CheckGlyph = "✓ ";

        public static readonly IList<EquipmentPortAction> EquipmentActions =
                new List<EquipmentPortAction>
                {
                    new EquipmentPortAction(Schemas.Equipment.CommStatTyp.OnLineRemote.Value, "OnLineRemote", false, false),
                    new EquipmentPortAction(Schemas.Equipment.CommStatTyp.OnLineLocal.Value, "OnLineLocal", false, false),
                    new EquipmentPortAction(Schemas.Equipment.CommStatTyp.OffLine.Value, "OffLine", false, false),
                    new EquipmentPortAction(ActionAuto, "Auto", true, true)
                }.AsReadOnly();

        public static string Mode(DataRow equipment)
        {
            return Schemas.Equipment.CommStatTyp.Text(equipment);
        }

        public static string AutoText(DataRow equipment)
        {
            return EquipmentPortManagementForm.IsAuto(equipment) ? "On" : "Off";
        }

        public static string GroupFilter(object[] checkedValues)
        {
            if (checkedValues == null || checkedValues.Length == 0)
            {
                return string.Empty;
            }

            List<string> ids = new List<string>();

            foreach (object value in checkedValues)
            {
                string id = (Convert.ToString(value) ?? string.Empty).Trim();

                if (id.Length > 0)
                {
                    ids.Add(id);
                }
            }

            return string.Join(",", ids.ToArray());
        }

        public static bool IsChecked(string key, DataRow equipment)
        {
            return key == ActionAuto && equipment != null && EquipmentPortManagementForm.IsAuto(equipment);
        }

        public static string LabelOf(EquipmentPortAction action, DataRow equipment, bool withCheckGlyph)
        {
            if (withCheckGlyph && action.Checkable && IsChecked(action.Key, equipment))
            {
                return CheckGlyph + action.Label;
            }

            return action.Label;
        }

        public static string SelectionAfterBind(DataTable equipments, string keepEquipmentId)
        {
            if (equipments == null || equipments.Rows.Count == 0)
            {
                return string.Empty;
            }

            string keep = (keepEquipmentId ?? string.Empty).Trim();

            if (keep.Length > 0)
            {
                foreach (DataRow row in equipments.Rows)
                {
                    if (Schemas.Equipment.EqpId.Text(row) == keep)
                    {
                        return keep;
                    }
                }
            }

            foreach (DataRow row in equipments.Rows)
            {
                if (Schemas.Equipment.ParentEqpId.Text(row).Length == 0)
                {
                    return Schemas.Equipment.EqpId.Text(row);
                }
            }

            return Schemas.Equipment.EqpId.Text(equipments.Rows[0]);
        }

        public static string TargetText(DataRow equipment, DataRow port)
        {
            if (equipment == null)
            {
                return "No equipment selected";
            }

            string text = Schemas.Equipment.EqpId.Text(equipment);

            if (port != null)
            {
                text = text + " · Port " + Schemas.Port.PortNm.Text(port);
            }

            return text;
        }

        public static readonly IList<string> ScreenColumns =
                new List<string>().AsReadOnly();

        public static string StatusLineText(DataRow equipment, DataRow port)
        {
            return TargetText(equipment, port);
        }
    }
}
