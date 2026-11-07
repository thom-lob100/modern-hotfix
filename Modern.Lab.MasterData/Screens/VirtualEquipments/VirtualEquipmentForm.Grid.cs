using System;
using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.MasterData;

namespace Modern.Lab.MasterData
{
    public partial class VirtualEquipmentForm
    {
        // 선택 설정이다 — 가상 장비는 모델이 VIRTUAL 로 고정이라 편집기에 기본값으로 채우고 잠근다(값은 저장 전문에 그대로 실린다). 응답에 없는 컬럼의 설정은 무시된다.
        protected override void DefineEditors()
        {
            this.CrudEditor.Field(Schemas.VirtualEquipment.EqpModelCd.Name).DefaultValue("VIRTUAL").ReadOnly();
            this.CrudEditor.Field(Schemas.VirtualEquipment.CtnDesc.Name).Multiline();
        }
    }
}
