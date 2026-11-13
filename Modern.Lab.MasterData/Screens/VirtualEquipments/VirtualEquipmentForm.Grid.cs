using System;
using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.MasterData;

namespace Modern.Lab.MasterData
{
    public partial class VirtualEquipmentForm
    {
        // 선택 설정이다 — 가상 장비는 모델이 VIRTUAL 로 고정이라 편집기에 기본값으로 채우고 잠근다(값은 저장 전문에 그대로 실린다). 응답에 없는 컬럼의 설정은 무시된다.
        // FAC_ID 의 목록은 Factory 목록, EQP_TYP · DET_EQP_TYP 의 목록은 공통코드(CommonTyp EquipmentType · DetailEquipmentType)에서 받는다. 조회조건 콤보 네 개가 편집기 목록을 함께 쓴다.
        protected override void DefineEditors()
        {
            this.CrudEditor.Field(Schemas.VirtualEquipment.FacId.Name).Combo("GetFactoryList", "FAC_ID", "FAC_ID");
            this.CrudEditor.Field(Schemas.VirtualEquipment.EqpTyp.Name).Combo("GetEquipmentTypeList", "TYP_VAL", "TYP_NM");
            this.CrudEditor.Field(Schemas.VirtualEquipment.DetEqpTyp.Name).Combo("GetDetailEquipmentTypeList", "TYP_VAL", "TYP_NM");
            this.CrudEditor.Field(Schemas.VirtualEquipment.EqpModelCd.Name).DefaultValue("VIRTUAL").ReadOnly();
            this.CrudEditor.Field(Schemas.VirtualEquipment.EqpGrpId.Name).Combo("GetEquipmentGroupList", "EQP_GRP_ID", "EQP_GRP_ID");
            this.CrudEditor.Field(Schemas.VirtualEquipment.CtnDesc.Name).Multiline();
            this.ShareLookup(this.cboFacId, Schemas.VirtualEquipment.FacId.Name);
            this.ShareLookup(this.cboEqpTyp, Schemas.VirtualEquipment.EqpTyp.Name);
            this.ShareLookup(this.cboDetEqpTyp, Schemas.VirtualEquipment.DetEqpTyp.Name);
            this.ShareLookup(this.cboEqpGrpId, Schemas.VirtualEquipment.EqpGrpId.Name);
        }
    }
}
