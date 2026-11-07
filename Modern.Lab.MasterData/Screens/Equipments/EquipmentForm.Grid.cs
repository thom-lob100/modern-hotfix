using System;
using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.MasterData;

namespace Modern.Lab.MasterData
{
    public partial class EquipmentForm
    {
        // 선택 설정이다 — 이 파일이 없어도 응답 컬럼 전부가 텍스트 편집기로 구성되고 키(EQP_ID)는 엔티티가 정한다. 응답에 없는 컬럼의 설정은 무시된다.
        protected override void DefineEditors()
        {
            this.CrudEditor.Field(Schemas.Equipment.EqpModelCd.Name).Combo("GetEquipmentModelList", "EQP_MODEL_CD", "EQP_MODEL_CD");
            this.CrudEditor.Field(Schemas.Equipment.VendorNm.Name).Combo("GetVendorList", "VENDOR_NM", "VENDOR_NM");
            this.CrudEditor.Field(Schemas.Equipment.EqpGrpId.Name).Combo("GetEquipmentGroupList", "EQP_GRP_ID", "EQP_GRP_ID");
            this.CrudEditor.Field(Schemas.Equipment.CtnDesc.Name).Multiline();
        }
    }
}
