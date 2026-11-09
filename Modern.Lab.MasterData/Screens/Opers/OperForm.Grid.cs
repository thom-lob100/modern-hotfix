using System;
using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.MasterData;

namespace Modern.Lab.MasterData
{
    public partial class OperForm
    {
        // 선택 설정이다 — 이 파일이 없어도 응답 컬럼 전부가 텍스트 편집기로 구성되고 키는 엔티티가 정한다. 응답에 없는 컬럼의 설정은 무시된다.
        protected override void DefineEditors()
        {
            this.CrudEditor.Field(Schemas.Oper.StdTimeMin.Name).Number(0);
            this.CrudEditor.Field(Schemas.Oper.UseYn.Name).Toggle();
            this.CrudEditor.Field(Schemas.Oper.Remark.Name).Multiline();
        }
    }
}
