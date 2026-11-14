using System;
using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.MasterData;

namespace Modern.Lab.MasterData
{
    public partial class OperForm
    {
        // 선택 설정이다 — 이 파일이 없어도 응답 컬럼 전부가 텍스트 편집기로 구성되고 키는 엔티티가 정한다. 응답에 없는 컬럼의 설정은 무시된다.
        // MID_OPER_TYP · DET_OPER_TYP 의 목록은 공통코드(CommonTyp MidOperType · DetailOperType)에서 받고, 조회조건 콤보가 같은 목록을 쓴다.
        protected override void DefineEditors()
        {
            this.CrudEditor.Field(Schemas.Oper.MidOperTyp.Name).Combo("GetMidOperTypeList", "TYP_VAL", "TYP_NM");
            this.CrudEditor.Field(Schemas.Oper.DetOperTyp.Name).Combo("GetDetailOperTypeList", "TYP_VAL", "TYP_NM");
            this.CrudEditor.Field(Schemas.Oper.StdTimeMin.Name).Number(0);
            this.CrudEditor.Field(Schemas.Oper.UseYn.Name).Toggle();
            this.CrudEditor.Field(Schemas.Oper.Remark.Name).Multiline();
            this.ShareLookup(this.cboMidOperTyp, Schemas.Oper.MidOperTyp.Name);
            this.ShareLookup(this.cboDetOperTyp, Schemas.Oper.DetOperTyp.Name);
        }
    }
}
