using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class DurableWaferEntity : EntityDefinition
    {
        public readonly Field SlotNo = Field("SLOT_NO").Caption("Slot");
        public readonly Field WfId = Field("WF_ID").Caption("Wafer Id");
        public readonly SubProdTypField SubProdTyp = Field<SubProdTypField>("SUB_PROD_TYP").Caption("Type");
        public readonly Field LotId = Field("LOT_ID").Caption("Lot Id");
        public readonly LotStatTypField LotStatTyp = Field<LotStatTypField>("LOT_STAT_TYP").Caption("Lot Status");
        public readonly LotHoldStatField LotHoldStatCd = Field<LotHoldStatField>("LOT_HOLD_STAT_CD").Caption("Hold State");
        public readonly MesProcStatField MesProcStatCd = Field<MesProcStatField>("MES_PROC_STAT_CD").Caption("Process").TopLabel("Mes").BottomLabel("Status").TopLabelAlignment(GridTextAlignment.Left).BottomLabelAlignment(GridTextAlignment.Right).Badge();
        public readonly JudgeResultField JudgeResult = Field<JudgeResultField>("JUDGE_RSLT").Caption("Judge Result");

        public DurableWaferEntity()
        {
            this.KeyFields = Fields(this.SlotNo);
            this.Seal();
        }
    }
}
