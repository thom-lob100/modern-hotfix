using System;
using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.WinForms.Controls.Selection;

using Modern.Lab.Hosting.MasterData;

namespace Modern.Lab.MasterData
{
    public partial class ProductForm
    {
        // 선택 설정이다 — 이 파일이 없어도 응답 컬럼 전부가 텍스트 편집기로 구성되고 키(PROD_ID, 서버 채번 → Schemas.Product.ReadOnlyFields)는
        // 엔티티가 잠근다. 응답에 없는 컬럼의 설정은 무시된다.
        protected override void DefineEditors()
        {
            this.CrudEditor.Field(Schemas.Product.ProductType.Name).Combo("GetProductTypeList", "CODE", "NAME");
            this.CrudEditor.Field(Schemas.Product.Unit.Name).Combo("GetCodeList", "CODE", "NAME", "GroupCd", "UNIT");
            this.CrudEditor.Field(Schemas.Product.SubProdTyp.Name).Combo(SubProdTypChoices(), ChoiceValueColumn, ChoiceLabelColumn);
            this.CrudEditor.Field(Schemas.Product.LotCd.Name).Combo(
                    LotCodeRequest, Schemas.LotCode.ValueField.Name, Schemas.LotCode.DisplayField.Name);
            this.CrudEditor.Field(Schemas.Product.UseYn.Name).Toggle();
            this.CrudEditor.Field(Schemas.Product.Description.Name).Multiline();
            // 감사 컬럼은 서버가 기록한다 — 보이되 고치지 않는다.
            this.CrudEditor.Field(Schemas.Product.UpdatedBy.Name, Schemas.Product.UpdatedAt.Name).ReadOnly();
            // LOT_CD 의 목록은 조회조건 콤보가 함께 쓴다.
            this.ShareLookup(this.cboLotCd, Schemas.Product.LotCd.Name);
        }
    }
}
