using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Controls.Wpf.Input;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.ResponseContracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.Samples.Contracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Display;
using Modern.Lab.Hosting.Messaging;

namespace Modern.Lab.Samples
{
    public partial class EquipmentPortManagementForm
    {
        // EQP_GRP_ID
        private DataTable GetEquipmentGroups()
        {
            return this.RequestFields("GetEqpGroupList").Table;
        }

        // EQP_ID
        private DataTable GetEquipmentExts(RequestParameters parameters)
        {
            return this.RequestFields("GetEqpList",
                    parameters.Take("EQP_GRP_ID")).Table;
        }

        // EQP_ID
        // PORT_NM
        private DataTable GetPortExts(RequestParameters parameters)
        {
            DataTable ports = this.RequestFields("GetEqpPortList",
                    parameters.Take(Schemas.EquipmentExt.EqpId)).Table;
            return WithEqpId(ports, "EQP_ID", parameters.Text(Schemas.EquipmentExt.EqpId));
        }

        private DataTable GetEquipmentExtHistory(RequestParameters parameters)
        {
            return this.RequestFields("GetEqpHistory",
                    parameters.Take(Schemas.EquipmentExt.EqpId)).Table;
        }

        private DataTable GetPortExtHistory(RequestParameters parameters)
        {
            DataTable history = this.RequestFields("GetEqpPortHistory",
                    parameters.Take(Schemas.EquipmentExt.EqpId),
                    parameters.Take(Schemas.PortExt.PortNm)).Table;
            return WithEqpId(history, "EQP_ID", parameters.Text(Schemas.EquipmentExt.EqpId));
        }

        private DataActionResult SetMode(RequestParameters parameters)
        {
            return this.RequestFields("SetEqpMode",
                    parameters.Take(Schemas.EquipmentExt.EqpId),
                    parameters.Take("MODE"),
                    "UserId", this.CurrentUserId);
        }

        private DataActionResult SetAuto(RequestParameters parameters)
        {
            return this.RequestFields("SetEqpAuto",
                    parameters.Take(Schemas.EquipmentExt.EqpId),
                    parameters.Take("AUTO_YN"),
                    "UserId", this.CurrentUserId);
        }

        private static DataTable WithEqpId(DataTable table, string eqpIdColumn, string eqpId)
        {
            if (table == null || table.Columns.Contains(eqpIdColumn))
            {
                return table;
            }

            table.Columns.Add(eqpIdColumn, typeof(string));

            foreach (DataRow row in table.Rows)
            {
                row[eqpIdColumn] = eqpId;
            }

            return table;
        }
    }
}
