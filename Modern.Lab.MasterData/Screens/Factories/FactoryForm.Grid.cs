using System;
using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.MasterData;

namespace Modern.Lab.MasterData
{
    public partial class FactoryForm
    {
        // 선택 설정이다 — 이 파일이 없어도 응답 컬럼 전부가 텍스트 편집기로 구성되고 키는 엔티티가 정한다. 응답에 없는 컬럼의 설정은 무시된다.
        // FAC_TYP 의 값은 Factory 엔티티의 종류 필드(ProductionFactory · StorageFactory · AssembleFactory)에서 가져온다. 조회조건 콤보가 이 표를 함께 쓴다.
        protected override void DefineEditors()
        {
            DataTable types = new DataTable();
            types.Columns.Add("CODE");
            foreach (string type in Schemas.Factory.FacTyp.ValueCodes())
            {
                types.Rows.Add(type);
            }
            this.CrudEditor.Field(Schemas.Factory.FacTyp.Name).Combo(types, "CODE", "CODE");
            this.CrudEditor.Field(Schemas.Factory.CtnDesc.Name).Multiline();
        }
    }
}
