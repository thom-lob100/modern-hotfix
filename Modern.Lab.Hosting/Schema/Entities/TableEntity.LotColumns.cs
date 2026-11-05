using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public abstract partial class TableEntity
    {
        /// <summary>Lot 칸 사전 — 회사 칸 이름 · 기본 캡션이 다르면 여기 한 줄만 고친다. 값 목록은 <see cref="LotFields"/>.</summary>
        protected static class LotColumns
        {
            public static Field ChildEqpId()
            {
                return Field("CHILD_EQP_ID").Caption("Child Eqp");
            }

            public static Field CreateTm()
            {
                return Field("CREATE_TM").Caption("Create Time");
            }

            public static Field LotCd()
            {
                return Field("LOT_CD");
            }

            public static LotFields.LotHoldStatCd LotHoldStatCd()
            {
                return Field<LotFields.LotHoldStatCd>("LOT_HOLD_STAT_CD").Caption("Hold State");
            }

            public static Field LotId()
            {
                return Field("LOT_ID").Caption("Lot Id");
            }

            public static LotFields.LotProcStatCd LotProcStatCd()
            {
                return Field<LotFields.LotProcStatCd>("LOT_PROC_STAT_CD").Caption("Lot Proc Status");
            }

            public static Field LotQty()
            {
                return Field("LOT_QTY").Caption("Qty");
            }

            public static LotFields.LotStatTyp LotStatTyp()
            {
                return Field<LotFields.LotStatTyp>("LOT_STAT_TYP").Caption("Lot Status");
            }

            public static LotFields.MesProcStatCd MesProcStatCd()
            {
                return Field<LotFields.MesProcStatCd>("MES_PROC_STAT_CD").Caption("Process").TopLabel("Mes").BottomLabel("Status")
                        .TopLabelAlignment(GridTextAlignment.Left).BottomLabelAlignment(GridTextAlignment.Right).Badge();
            }

            public static Field Qty()
            {
                return Field("QTY").Caption("Qty");
            }
        }
    }
}
