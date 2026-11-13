using System;
using System.Collections.Generic;
using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.ResponseContracts;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.MasterData
{
    public partial class CommonCodeForm
    {
        private static readonly string[] CommonTypeColumns = EntityDefinition.Names(Schemas.CommonType.KeyFields);
        private static readonly string[] CommonTypeDetColumns = EntityDefinition.Names(Schemas.CommonTypeDet.KeyFields);

        // COMMON_TYP
        private DataTable GetCommonTypes()
        {
            return this.RequestFields("CommonCodeAction", "MethodCommand", "SelectCommonTypes", "CommonTyp", string.Empty).Table;
        }

        // COMMON_TYP
        // TYP_VAL
        private DataTable GetCommonTypeDets(string commonTyp)
        {
            return this.RequestFields("CommonCodeAction", "MethodCommand", "SelectCommonCodes", "CommonTyp", commonTyp).Table;
        }

        protected override DataActionResult InsertItem(object[] requestFields)
        {
            return this.WriteEditedItem("Insert", requestFields, true);
        }

        protected override DataActionResult UpdateItem(object[] requestFields)
        {
            return this.WriteEditedItem("Update", requestFields, false);
        }

        protected override DataActionResult DeleteItem(string[] keyValues)
        {
            if (this.manageTypes)
            {
                return this.WriteItem("DeleteCommonType", keyValues[0], null, new object[0]);
            }
            return this.WriteItem("DeleteCommonCode", this.actionCommonTyp, keyValues[1], new object[0]);
        }

        private DataActionResult WriteEditedItem(string verb, object[] fields, bool isNew)
        {
            if (this.manageTypes)
            {
                string commonTyp = KeyText(fields, this.EditorName(Schemas.CommonType.CommonTyp), isNew);
                return this.WriteItem(verb + "CommonType", commonTyp, null, fields);
            }
            string typVal = KeyText(fields, this.EditorName(Schemas.CommonTypeDet.TypVal), isNew);
            return this.WriteItem(verb + "CommonCode", this.actionCommonTyp, typVal, fields);
        }

        private DataActionResult WriteItem(string method, string commonTyp, string typVal, object[] fields)
        {
            List<object> request = new List<object> { "MethodCommand", method, "CommonTyp", commonTyp };
            if (typVal != null)
            {
                request.Add("TypVal");
                request.Add(typVal);
            }
            request.AddRange(WithoutFields(fields, this.ExcludedEditorNames()));
            return this.RequestFields("CommonCodeAction", request.ToArray());
        }

        private string[] ExcludedEditorNames()
        {
            if (this.manageTypes)
            {
                return new[]
                {
                    this.EditorName(Schemas.CommonType.CommonTyp),
                    this.EditorName(Schemas.CommonType.UpdatedBy),
                    this.EditorName(Schemas.CommonType.UpdatedAt)
                };
            }
            return new[]
            {
                this.EditorName(Schemas.CommonTypeDet.CommonTyp),
                this.EditorName(Schemas.CommonTypeDet.TypVal),
                this.EditorName(Schemas.CommonTypeDet.UpdatedBy),
                this.EditorName(Schemas.CommonTypeDet.UpdatedAt)
            };
        }

        private string EditorName(Field field)
        {
            return this.CrudEditor.ParameterNameOf(field.Name);
        }

        private static string KeyText(object[] fields, string name, bool isNew)
        {
            for (int i = 0; i + 1 < fields.Length; i += 2)
            {
                if (Convert.ToString(fields[i]) == name)
                {
                    string value = Convert.ToString(fields[i + 1]);
                    return isNew ? value.Trim() : value;
                }
            }
            return string.Empty;
        }
    }
}
