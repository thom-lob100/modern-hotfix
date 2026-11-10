using System;
using System.Collections.Generic;
using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.Messaging;

namespace Modern.Lab.MasterData
{
    public partial class PortForm
    {
        // EQP_ID
        private DataTable GetEquipments()
        {
            return this.RequestFields("PortAction", "MethodCommand", "SelectEquipment").Table;
        }

        private DataActionResult WritePort(string method, string eqpId, object[] fields)
        {
            string eqpIdParameter = this.CrudEditor.ParameterNameOf("EQP_ID");
            List<object> pairs = new List<object> { eqpIdParameter, eqpId };
            pairs.AddRange(WithoutFields(fields, eqpIdParameter));
            return this.RequestFields("PortAction", WithMethodCommand(method, pairs.ToArray()));
        }

        private DataActionResult DeletePort(string eqpId, string portNm)
        {
            return this.WritePort("DeletePort", eqpId,
                    new object[] { this.CrudEditor.ParameterNameOf("PORT_NM"), portNm });
        }

        // EQP_ID
        // PORT_NM
        private DataTable GetPorts(string eqpId)
        {
            return this.RequestFields("PortAction", "MethodCommand", "SelectPorts", "EqpId", eqpId).Table;
        }
    }
}
