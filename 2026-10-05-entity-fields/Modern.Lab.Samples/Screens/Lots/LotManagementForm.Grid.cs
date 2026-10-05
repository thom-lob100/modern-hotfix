using System.Data;

using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Display;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class LotManagementForm
    {
        private static Modern.Lab.WinForms.Controls.Display.ModernFieldDefinition[] ChildLotSourceFields(DataRow lot)
        {
            return Schemas.Lot.Fields(lot)
                    .Only(Schemas.Lot.LotId.Name, Schemas.Lot.SubProdTyp.Name, Schemas.Lot.LotStatTyp.Name,
                            Schemas.Lot.LotHoldStatCd.Name, Schemas.Lot.ProdId.Name, Schemas.Lot.FlowId.Name,
                            Schemas.Lot.OperId.Name, Schemas.Lot.Qty.Name).ToArray();
        }

        // 표에 무엇을 어떻게 보일지 — 컬럼 캡션·배지·링크·숨김. 쿼리 컬럼이 바뀌면 여기만 고친다.
        // 무엇을 채울지(조회·판정)는 .Server.cs 와 .Rules.cs 가 갖는다.
        // 각 표는 엔티티(Schemas.Lot · Wafer · LotHistory · WaferHistory)가 키·배지를 갖는다 — 이 화면만의 꾸밈은 그 뒤에 이어 적는다.

        private static GridColumns LotColumns(DataTable lots)
        {
            return Schemas.Lot.Columns(lots)
                    .Column(Schemas.Lot.MesProcStatCd.Name).BadgeWidth(Schemas.Lot.MesProcStatCd.ValueCodes())
                    .Column(Schemas.Lot.ReqSerialNo.Name).Link();
        }

        // idCaption 은 탭에 맞는 단위 이름("Wafer Id" · "Chip Id" · "Lamella Id").
        private static GridColumns WaferColumns(DataTable wafers, string idCaption)
        {
            return Schemas.Wafer.Columns(wafers).Column(Schemas.Wafer.WfId.Name).Caption(idCaption);
        }

        private static FieldDefinitions LotFields(DataTable lot)
        {
            return Schemas.Lot.Fields(lot)
                    .Field(Schemas.Lot.ReqSerialNo.Name).Link()
                    .Field(Schemas.Lot.Description.Name).Span(3).StartAt(3);
        }

        private static GridColumns LotHistoryColumns(DataTable history)
        {
            return Schemas.LotHistory.Columns(history)
                    .Column(Schemas.LotHistory.MesProcStatCd.Name).BadgeWidth(Schemas.LotHistory.MesProcStatCd.ValueCodes());
        }

        private static GridColumns WaferHistoryColumns(DataTable history, string idCaption)
        {
            return Schemas.WaferHistory.Columns(history)
                    .Column(Schemas.WaferHistory.WfId.Name).Caption(idCaption)
                    .Column(Schemas.WaferHistory.ReqSerialNo.Name).Link();
        }
    }
}
