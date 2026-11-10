using System;
using System.Collections.Generic;
using System.Data;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.ResponseContracts;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.MasterData
{
    public partial class ReasonCodeForm
    {
        private static readonly string[] KeyColumns = EntityDefinition.Names(Schemas.ReasonCode.KeyFields);

        protected override DataTable RequestItems()
        {
            DataTable table = this.GetReasonCodes();
            string missing = ResponseColumns.Missing(table, KeyColumns);
            if (missing.Length > 0)
            {
                throw new ResponseContractException("Reason Code — missing columns: " + missing);
            }
            return table;
        }

        protected override DataActionResult InsertItem(object[] requestFields)
        {
            return this.WriteReasonCode("InsertReasonCode", KeyOf(requestFields, true), requestFields);
        }

        protected override DataActionResult UpdateItem(object[] requestFields)
        {
            return this.WriteReasonCode("UpdateReasonCode", KeyOf(requestFields, false), requestFields);
        }

        protected override DataActionResult DeleteItem(string[] keyValues)
        {
            return this.WriteReasonCode("DeleteReasonCode", keyValues, new object[0]);
        }

        // FAC_ID
        // REASON_CATG_CD
        // REASON_CD
        private DataTable GetReasonCodes()
        {
            return this.RequestFields("ReasonCodeAction", "MethodCommand", "SelectReasonCodes").Table;
        }

        private static string[] KeyOf(object[] fields, bool isNew)
        {
            string[] key = { string.Empty, string.Empty, string.Empty };
            for (int i = 0; i + 1 < fields.Length; i += 2)
            {
                string name = Convert.ToString(fields[i]);
                string value = Convert.ToString(fields[i + 1]);
                if (isNew) { value = value.Trim(); }
                if (name == "FacId") { key[0] = value; }
                else if (name == "ReasonCatgCd") { key[1] = value; }
                else if (name == "ReasonCd") { key[2] = value; }
            }
            return key;
        }

        private DataActionResult WriteReasonCode(string method, string[] key, object[] fields)
        {
            List<object> request = new List<object> { "MethodCommand", method,
                    "FacId", key[0], "ReasonCatgCd", key[1], "ReasonCd", key[2] };
            request.AddRange(WithoutFields(fields, "FacId", "ReasonCatgCd", "ReasonCd"));
            return this.RequestFields("ReasonCodeAction", request.ToArray());
        }
    }
}
