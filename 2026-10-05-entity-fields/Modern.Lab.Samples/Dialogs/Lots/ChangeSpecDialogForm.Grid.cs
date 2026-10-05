using System.Data;
using Modern.Lab.Hosting.Schema;

using Modern.Lab.WinForms.Controls.Display;

namespace Modern.Lab.Samples
{
    public partial class ChangeSpecDialogForm
    {
        // 정보 카드에 무엇을 보일지. 쿼리 컬럼이 바뀌면 여기만 고친다.
        // Flow·Oper 콤보의 표시·값은 엔티티(Schemas.Flow · Oper)의 ComboDisplayFields · ValueField 를 따른다.
        private static FieldDefinitions SourceDefinitions(DataTable lot)
        {
            return Schemas.Lot.Fields(lot).Only(
                    Schemas.Lot.LotId.Name, Schemas.Lot.SubProdTyp.Name, Schemas.Lot.LotStatTyp.Name,
                    Schemas.Lot.MesProcStatCd.Name, Schemas.Lot.ProdId.Name, Schemas.Lot.FlowId.Name,
                    Schemas.Lot.OperId.Name, Schemas.Lot.Qty.Name);
        }
    }
}
