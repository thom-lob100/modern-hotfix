using System;
using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.MasterData;

namespace Modern.Lab.MasterData
{
    public partial class AreaBayForm
    {
        // 선택 설정이다 — 이 파일이 없어도 응답 컬럼 전부가 텍스트 편집기로 구성되고 키는 엔티티가 정한다. 응답에 없는 컬럼의 설정은 무시된다.
        protected override void DefineEditors()
        {
            DataTable types = new DataTable();
            types.Columns.Add("CODE");
            types.Columns.Add("NAME");
            types.Rows.Add("AREA", "Area");
            types.Rows.Add("BAY", "Bay");
            this.CrudEditor.Field(Schemas.Area.LocationType.Name).Combo(types, "CODE", "NAME");
            this.CrudEditor.Field(Schemas.Area.ParentAreaId.Name).Combo("SelectParentAreas", "CODE", "NAME");
            this.CrudEditor.Field(Schemas.Area.UseYn.Name).Toggle();
            this.CrudEditor.Field(Schemas.Area.Description.Name).Multiline();
            this.CrudEditor.Field(Schemas.Area.UpdatedBy.Name, Schemas.Area.UpdatedAt.Name).ReadOnly();
        }
    }
}
