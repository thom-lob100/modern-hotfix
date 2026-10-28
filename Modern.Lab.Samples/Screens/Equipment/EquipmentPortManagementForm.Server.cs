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
                    "EqpGrpId", parameters.Value(Schemas.EquipmentExt.EqpGrpId)).Table;
        }

        // EQP_ID
        // PORT_NM
        private DataTable GetPortDets(DataRow equipment)
        {
            return this.RequestFields("GetEqpPortList",
                    "EqpId", Schemas.EquipmentExt.EqpId.Text(equipment)).Table;
        }

        private DataTable GetEquipmentExtHistory(DataRow equipment)
        {
            return this.RequestFields("GetEqpHistory",
                    "EqpId", Schemas.EquipmentExt.EqpId.Text(equipment)).Table;
        }

        private DataTable GetPortDetHistory(DataRow equipment, DataRow port)
        {
            return this.RequestFields("GetEqpPortHistory",
                    "EqpId", Schemas.EquipmentExt.EqpId.Text(equipment),
                    "PortNm", Schemas.PortDet.PortNm.Text(port)).Table;
        }

        private DataActionResult SetMode(DataRow equipment, RequestParameters parameters)
        {
            return this.RequestFields("SetEqpMode",
                    "EqpId", Schemas.EquipmentExt.EqpId.Text(equipment),
                    "Mode", parameters.Value(Schemas.EquipmentExt.CommStatTyp),
                    "UserId", this.CurrentUserId);
        }

        private DataActionResult SetAuto(DataRow equipment, RequestParameters parameters)
        {
            return this.RequestFields("SetEqpAuto",
                    "EqpId", Schemas.EquipmentExt.EqpId.Text(equipment),
                    "AutoYn", parameters.Value(Schemas.EquipmentExt.AutoYn),
                    "UserId", this.CurrentUserId);
        }

    }
}
