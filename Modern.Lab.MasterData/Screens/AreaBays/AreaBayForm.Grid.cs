using System;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.MasterData;

namespace Modern.Lab.MasterData
{
    public partial class AreaBayForm
    {
        // 선택 설정이다 — 이 파일이 없어도 응답 컬럼 전부가 텍스트 편집기로 구성되고 키는 엔티티가 정한다. 응답에 없는 컬럼의 설정은 무시된다.
        // DET_AREA_TYP 의 목록은 공통코드(CommonTyp DetailAreaType)에서 받는다. 조회조건 콤보가 이 목록을 함께 쓴다.
        protected override void DefineEditors()
        {
            this.CrudEditor.Field(Schemas.Area.DetAreaTyp.Name).Combo("SelectDetailAreaTypes", "TYP_VAL", "TYP_NM");
            this.CrudEditor.Field(Schemas.Area.ParentAreaId.Name).Combo("SelectParentAreas", "CODE", "NAME");
            this.CrudEditor.Field(Schemas.Area.UseYn.Name).Toggle();
            this.CrudEditor.Field(Schemas.Area.Description.Name).Multiline();
            this.CrudEditor.Field(Schemas.Area.UpdatedBy.Name, Schemas.Area.UpdatedAt.Name).ReadOnly();
            this.ShareLookup(this.cboDetAreaTyp, Schemas.Area.DetAreaTyp.Name);
        }
    }
}
