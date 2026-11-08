using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Request Report 의 한 줄(<c>GetRequestReport</c>, 홈 이름) — 의뢰서 · 물류 · Durable · 시료 칸을 한 조회로 합친 응답 표다(2026-11-08).
    /// 선언 순서가 그리드 칸 순서이고, <c>Band</c> 표시가 머리글 묶음을 끊는다(표시한 칸부터 다음 표시 전까지 같은 묶음).
    /// 단계(<see cref="AosStage"/> · <c>&lt;코드&gt;_TM · _PREP_TM · _START_TM · _END_TM · _EQP_ID · _RUN_CNT · _FAIL_CNT · _JUDGE_RSLT</c>)와
    /// 가지(<c>&lt;코드&gt;_YN</c> · <c>_CNT</c> · <c>_TM</c>) 칸은 리포트 틀이 코드표(<c>AOS_STAGE</c> · <c>AOS_BRANCH</c>)로 가져가 흐름 · 가지 · 단계 상세 묶음에 세운다 —
    /// 여기서는 응답 표의 칸으로 적고 캡션만 준다. 코드표에 없는 코드의 칸은 마지막 Stage 묶음에 남는다.</summary>
    public sealed class RequestReportEntity : TableEntity
    {
        public readonly Field ReqSerialNo = RequestColumns.ReqSerialNo().Band("Request");
        public readonly Field ReqSubSerialNo = Field("REQ_SUBSERIAL_NO").Caption("SubSerial No").TopLabel("Request");
        public readonly Field Status = Field("STATUS").Caption("Status");
        public readonly RequestFields.StateCode StateCode = Field<RequestFields.StateCode>("STATE_CODE").Badge();
        public readonly Field ReqDt = Field("REQ_DT").Caption("Request Date");
        public readonly Field AcceptDt = Field("ACCEPT_DT").Caption("Accept Date");
        public readonly Field Requester = Field("REQUESTER");
        public readonly Field Department = Field("DEPARTMENT");
        public readonly Field DeviceNm = Field("DEVICE_NM");
        public readonly Field ProjectNm = Field("PROJECT_NM");
        public readonly Field LayerNm = Field("LAYER_NM");
        public readonly Field Purpose = Field("PURPOSE");
        public readonly Field LotId = LotColumns.LotId().Band("Logistics");
        public readonly Field SharedWith = Field("SHARED_WITH").Caption("Shared");
        public readonly Field SendFac = CommonColumns.SendFac();
        public readonly Field SendTm = Field("SEND_TM").Caption("Sent At");
        public readonly Field LinkTm = Field("LINK_TM").Caption("Linked At");
        public readonly Field ArrivalTm = Field("ARRIVAL_TM").Caption("Arrived At");
        public readonly Field BoxId = CommonColumns.BoxId();
        public readonly Field DurableId = DurableColumns.DurableId().Band("Durable");
        public readonly Field ExchangeTm = Field("EXCHANGE_TM").Caption("Exchanged At");
        public readonly Field ReturnTm = Field("RETURN_TM").Caption("Returned At");
        public readonly Field WaferId = Field("WAFER_ID").Band("Sample");
        public readonly Field ChipId = Field("CHIP_ID");
        public readonly Field WfId = WaferColumns.WfId();
        public readonly Field FingerId = DurableColumns.FingerId();
        public readonly Field AosStage = Field("AOS_STAGE").Caption("Stage").Band("Stage");
        public readonly CommonFields.Yn HoldYn = Field<CommonFields.Yn>("HOLD_YN").Caption("Hold");
        public readonly CommonFields.Yn ReworkYn = Field<CommonFields.Yn>("REWORK_YN").Caption("Rework");
        public readonly Field ReworkCnt = Field("REWORK_CNT").Caption("Rework Count");
        public readonly CommonFields.Yn ScrapYn = Field<CommonFields.Yn>("SCRAP_YN").Caption("Scrap");
        public readonly CommonFields.Yn ReturnYn = Field<CommonFields.Yn>("RETURN_YN").Caption("Return");
        public readonly CommonFields.Yn SwapYn = Field<CommonFields.Yn>("SWAP_YN").Caption("Swap");
        public readonly CommonFields.Yn ScrapDueYn = Field<CommonFields.Yn>("SCRAP_DUE_YN").Caption("Scrap Due");
        public readonly Field PrepEqpId = Field("PREP_EQP_ID").Caption("Eqp").TopLabel("PREP");
        public readonly Field PrepPrepTm = Field("PREP_PREP_TM").Caption("Job Prep").TopLabel("PREP");
        public readonly Field PrepStartTm = Field("PREP_START_TM").Caption("Job Start").TopLabel("PREP");
        public readonly Field PrepEndTm = Field("PREP_END_TM").Caption("Job End").TopLabel("PREP");
        public readonly CommonFields.JudgeRslt PrepJudgeRslt = Field<CommonFields.JudgeRslt>("PREP_JUDGE_RSLT").Caption("Judge").TopLabel("PREP").Badge();
        public readonly Field PrepRunCnt = Field("PREP_RUN_CNT").Caption("Runs").TopLabel("PREP");
        public readonly Field PrepFailCnt = Field("PREP_FAIL_CNT").Caption("Fails").TopLabel("PREP");
        public readonly Field FibEqpId = Field("FIB_EQP_ID").Caption("Eqp").TopLabel("FIB");
        public readonly Field FibPrepTm = Field("FIB_PREP_TM").Caption("Job Prep").TopLabel("FIB");
        public readonly Field FibStartTm = Field("FIB_START_TM").Caption("Job Start").TopLabel("FIB");
        public readonly Field FibEndTm = Field("FIB_END_TM").Caption("Job End").TopLabel("FIB");
        public readonly CommonFields.JudgeRslt FibJudgeRslt = Field<CommonFields.JudgeRslt>("FIB_JUDGE_RSLT").Caption("Judge").TopLabel("FIB").Badge();
        public readonly Field FibRunCnt = Field("FIB_RUN_CNT").Caption("Runs").TopLabel("FIB");
        public readonly Field FibFailCnt = Field("FIB_FAIL_CNT").Caption("Fails").TopLabel("FIB");
        public readonly Field TemEqpId = Field("TEM_EQP_ID").Caption("Eqp").TopLabel("TEM");
        public readonly Field TemPrepTm = Field("TEM_PREP_TM").Caption("Job Prep").TopLabel("TEM");
        public readonly Field TemStartTm = Field("TEM_START_TM").Caption("Job Start").TopLabel("TEM");
        public readonly Field TemEndTm = Field("TEM_END_TM").Caption("Job End").TopLabel("TEM");
        public readonly Field TemRunCnt = Field("TEM_RUN_CNT").Caption("Runs").TopLabel("TEM");
        public readonly Field DoneTm = Field("DONE_TM").Caption("Done At");
        public readonly Field SwapTm = Field("SWAP_TM").Caption("Swapped At");
        public readonly Field ScrapTm = Field("SCRAP_TM").Caption("Scrapped At");

        public RequestReportEntity()
        {
            this.KeyFields = Fields(this.ReqSerialNo, this.ReqSubSerialNo);
            this.Seal();
        }
    }
}
