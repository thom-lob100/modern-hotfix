using System;
using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.MasterData;
using Modern.Lab.Hosting.ResponseContracts;

namespace Modern.Lab.MasterData
{
    public partial class PortForm
    {
        // 선택 설정이다 — 이 파일이 없어도 응답 컬럼 전부가 텍스트 편집기로 구성된다. 선택 장비가 공급하는 EQP_ID 잠금은 본체가 한다.
        protected override void DefineEditors()
        {
            DataTable types = new DataTable();
            types.Columns.Add("CODE");
            foreach (string type in Schemas.Port.PortTyp.ValueCodes())
            {
                types.Rows.Add(type);
            }
            this.CrudEditor.Field(Schemas.Port.PortTyp.Name).Combo(types, "CODE", "CODE");
            this.CrudEditor.Field(Schemas.Port.Priority.Name).Number(0);
            this.CrudEditor.Field(Schemas.Port.UseYn.Name).Toggle();
            this.CrudEditor.Field(Schemas.Port.Description.Name).Multiline();
            // 감사 컬럼은 서버가 기록한다 — 보이되 고치지 않는다.
            this.CrudEditor.Field(Schemas.Port.UpdatedBy.Name, Schemas.Port.UpdatedAt.Name).ReadOnly();
        }
    }
}
