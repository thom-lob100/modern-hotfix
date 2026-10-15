using System.Data;

namespace Modern.Lab.Samples
{
    public partial class DurableHoldDialogForm
    {
        // REASON_CD
        // CTN_DESC
        private DataTable GetReasonCodes(bool releaseMode)
        {
            return this.RequestFields("GetLotHoldReasonList", "FacId", this.CurrentFacId,
                    "ReasonCatgCd", releaseMode ? "RELEASE" : "HOLD").Table;
        }

        // USER_ID
        // USER_NM
        private DataTable GetEngineers()
        {
            return this.RequestFields("UserAction", "MethodCommand", "SelectUsers").Table;
        }
    }
}
