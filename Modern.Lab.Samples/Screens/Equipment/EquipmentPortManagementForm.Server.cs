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
        private DataTable GetPortDets(RequestParameters parameters)
        {
            return this.RequestFields("GetEqpPortList",
                    parameters.Take(Schemas.EquipmentExt.EqpId)).Table;
        }

        private DataTable GetEquipmentExtHistory(RequestParameters parameters)
        {
            return this.RequestFields("GetEqpHistory",
                    parameters.Take(Schemas.EquipmentExt.EqpId)).Table;
        }

        private DataTable GetPortDetHistory(RequestParameters parameters)
        {
            return this.RequestFields("GetEqpPortHistory",
                    parameters.Take(Schemas.EquipmentExt.EqpId),
                    parameters.Take(Schemas.PortDet.PortNm)).Table;
        }

        private DataActionResult SetMode(RequestParameters parameters)
        {
            return this.RequestFields("SetEqpMode",
                    parameters.Take(Schemas.EquipmentExt.EqpId),
                    parameters.Take("COMM_STAT_TYP", "Mode"),
                    "UserId", this.CurrentUserId);
        }

        private DataActionResult SetAuto(RequestParameters parameters)
        {
            return this.RequestFields("SetEqpAuto",
                    parameters.Take(Schemas.EquipmentExt.EqpId),
                    parameters.Take("AUTO_YN"),
                    "UserId", this.CurrentUserId);
        }

    }
}
