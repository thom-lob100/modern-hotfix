using System;
using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.MasterData;

namespace Modern.Lab.MasterData
{
    public partial class LotCodeForm
    {
        // 선택 설정이다 — 이 파일이 없어도 응답 컬럼 전부가 텍스트 편집기로 구성되고 키는 엔티티가 정한다. 응답에 없는 컬럼의 설정은 무시된다.
        protected override void DefineEditors()
        {
            DataTable types = new DataTable();
            types.Columns.Add("CODE");
            foreach (string type in Schemas.LotCode.SubProdTyp.ValueCodes())
            {
                types.Rows.Add(type);
            }
            this.CrudEditor.Field(Schemas.LotCode.SubProdTyp.Name).Combo(types, "CODE", "CODE");
            this.CrudEditor.Field(Schemas.LotCode.UseYn.Name).Toggle();
            this.CrudEditor.Field(Schemas.LotCode.Remark.Name).Multiline();
        }
    }
}
