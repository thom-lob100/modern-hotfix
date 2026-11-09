using System;
using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.MasterData;

namespace Modern.Lab.MasterData
{
    public partial class SubCategoriesForm
    {
        // 선택 설정이다 — 이 파일이 없어도 응답 컬럼 전부가 텍스트 편집기로 구성되고 키는 엔티티가 정한다. 응답에 없는 컬럼의 설정은 무시된다.
        // DURABLE_TYP 의 값은 Durable 엔티티의 종류 필드(FOUP · TRAY)에서 가져온다.
        protected override void DefineEditors()
        {
            DataTable types = new DataTable();
            types.Columns.Add("CODE");
            foreach (string type in Schemas.Durable.DurableTyp.ValueCodes())
            {
                types.Rows.Add(type);
            }
            this.CrudEditor.Field(Schemas.SubCategory.DurableTyp.Name).Combo(types, "CODE", "CODE");
        }
    }
}
