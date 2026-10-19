using System.Data;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.WinForms.Controls.Display;

namespace Modern.Lab.Samples
{
    public partial class BackToPrevOperDialogForm
    {
        // Lot 카드에 무엇을 보일지. Lot 행은 Equipment / Lots · Lot / Equipment 의 Lot 목록 행(Schemas.LotPriority)이다 — Cancel Oper 창과 같은 구성.
        // Prod·Flow·Oper 콤보의 표시·값은 엔티티(Schemas.Product · Flow · Oper)의 ComboDisplayFields · ValueField 를 따른다.
        private static FieldDefinitions SourceDefinitions(DataTable lot)
        {
            return Schemas.LotPriority.Fields(lot).Only(Schemas.LotPriority.LotId.Name, Schemas.LotPriority.SubProdTyp.Name,
                    Schemas.LotPriority.LotStatTyp.Name, Schemas.LotPriority.LotHoldStatCd.Name,
                    Schemas.LotPriority.ProdId.Name, Schemas.LotPriority.FlowId.Name, Schemas.LotPriority.OperId.Name,
                    Schemas.LotPriority.LastEventCd.Name);
        }
    }
}
