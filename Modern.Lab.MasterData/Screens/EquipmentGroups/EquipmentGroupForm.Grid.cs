using System;
using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.MasterData;

namespace Modern.Lab.MasterData
{
    public partial class EquipmentGroupForm
    {
        // 선택 설정이다 — 이 파일이 없어도 응답 컬럼 전부가 텍스트 편집기로 구성되고 키는 엔티티가 정한다. 응답에 없는 컬럼의 설정은 무시된다.
        // EQP_GRP_TYP 의 목록은 공통코드(CommonTyp EquipmentGroupType)에서 받고, 조회조건 콤보가 같은 목록을 쓴다.
        protected override void DefineEditors()
        {
            this.CrudEditor.Field(Schemas.EquipmentGroup.EqpGrpTyp.Name).Combo("GetEquipmentGroupTypeList", "TYP_VAL", "TYP_NM");
            this.CrudEditor.Field(Schemas.EquipmentGroup.Description.Name).Multiline();
            this.ShareLookup(this.cboEqpGrpTyp, Schemas.EquipmentGroup.EqpGrpTyp.Name);
        }
    }
}
