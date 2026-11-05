using Modern.Lab.Hosting.Schema;
using Modern.Lab.Report.Reporting;
using Modern.Lab.Report.Services;
using Modern.Lab.Theming;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Report
{
    public partial class RequestReportForm
    {
        // 리포트 정의 — 조건 · 흐름 · 칸만 적는다. 화면 배치 · 조회 · 인디케이터 · 엑셀은 공통 틀(ReportFormBase)이 만든다.
        // 리포트 한 줄 = 서브시리얼 하나. 왼쪽 진행판(Id · Stage · Lot · Sample · AOS 흐름 · 가지), 오른쪽 의뢰서 정보와 상세.
        // 회사 칸 이름이 다르면 이 파일의 글자만 바꾼다. 무엇을 채울지(조회)는 .Server.cs 가 갖는다.

        // 같은 Lot 을 쓰는 의뢰서 번호 목록(쉼표) — 추가분석이면 둘 이상이다. Shared 칸에는 자기 번호를 뺀 나머지가 보인다.
        private const string LotRequestsColumn = "LOT_REQ_NOS";

        // 홈 이름 — Hold · 교체 교환 · 폐기 · 폐기 후보 칸(2026-11-05). 응답에 없으면 그 가지 칸은 비고 판정하지 않는다.
        private const string LotHoldColumn = "LOT_HOLD_STAT_CD";
        private const string LotHoldValue = "OnHold";
        private const string ExchangeTmColumn = "EXCHANGE_TM";
        private const string ReturnTmColumn = "RETURN_TM";
        private const string SwapTmColumn = "SWAP_TM";
        private const string ScrapTmColumn = "SCRAP_TM";
        private const string ScrapDueColumn = "SCRAP_DUE_YN";

        private static readonly string[] SampleBand =
        {
            Schemas.Sample.WaferId.Name, Schemas.Sample.ChipId.Name, Schemas.RequestWafer.WfId.Name
        };

        private static readonly string[] BranchBand =
        {
            RequestReportPresenter.ReturnColumn, RequestReportPresenter.SwapColumn, RequestReportPresenter.ReworkColumn,
            RequestReportPresenter.ScrapColumn
        };

        private static readonly string[] RequestBand =
        {
            "REQ_DT", "ACCEPT_DT", Schemas.Request.Requester.Name, Schemas.Request.Department.Name,
            Schemas.Request.DeviceNm.Name, Schemas.Request.ProjectNm.Name, Schemas.Request.LayerNm.Name, Schemas.Request.Purpose.Name
        };

        private static readonly string[] LogisticsBand =
        {
            Schemas.Logistics.SendFac.Name, Schemas.Logistics.SendTm.Name, "LINK_TM", Schemas.Logistics.ProcTm.Name
        };

        private static readonly string[] FoupBand =
        {
            Schemas.Logistics.BoxId.Name, Schemas.Durable.DurableId.Name, ExchangeTmColumn, ReturnTmColumn, SwapTmColumn
        };

        private static readonly string[] PrepBand =
        {
            "PREP_EQP_ID", "PREP_PREP_TM", "PREP_START_TM", "PREP_END_TM", "PREP_RUN_CNT", "PREP_FAIL_CNT", "PREP_JUDGE_RSLT"
        };

        private static readonly string[] FibBand =
        {
            "FIB_EQP_ID", "FIB_PREP_TM", "FIB_START_TM", "FIB_END_TM", Schemas.DurableLamella.FingerId.Name,
            "FIB_RUN_CNT", "FIB_FAIL_CNT", "FIB_JUDGE_RSLT"
        };

        private static readonly string[] TemBand = { "TEM_EQP_ID", "TEM_PREP_TM", "TEM_START_TM", "TEM_END_TM", "TEM_RUN_CNT" };

        private static readonly string[] ScrapBand = { ScrapTmColumn, ScrapDueColumn };

        // 재작업 이력(RW n)은 공정별 추가 회차의 합이고, 현재 FAIL 은 PREP · FIB Judge 칸으로 따로 센다(2026-11-05).
        private static readonly string[] RunCountColumns = { "PREP_RUN_CNT", "FIB_RUN_CNT", "TEM_RUN_CNT" };
        private static readonly string[] JudgeColumns = { "PREP_JUDGE_RSLT", "FIB_JUDGE_RSLT" };

        // 시각 칸 — 분까지만 보인다(이름 관례의 기본은 초까지라 칸이 넓어진다).
        private static readonly string[] TimeColumns =
        {
            Schemas.Logistics.SendTm.Name, "LINK_TM", Schemas.Logistics.ProcTm.Name, ExchangeTmColumn, ReturnTmColumn, SwapTmColumn, ScrapTmColumn,
            "PREP_PREP_TM", "PREP_START_TM", "PREP_END_TM", "FIB_PREP_TM", "FIB_START_TM", "FIB_END_TM",
            "TEM_PREP_TM", "TEM_START_TM", "TEM_END_TM"
        };

        protected override void DefineReport(ReportDefinition report)
        {
            report.Title = "Request Report";
            report.Environment = "AOS";
            report.EmptyText = "No requests";
            report.ExportSheetName = "Request Report";
            report.ExportFilePrefix = "RequestReport";
            report.Key(Schemas.Request.ReqSerialNo, Schemas.Sample.ReqSubSerialNo);

            // 의뢰서 쪽 단계 — Request 화면과 같은 일곱 칸. DRAFT0 은 취소(회색 배지, 인디케이터 전부 대기).
            // 그리드 · 엑셀에는 표시 칸(REQUEST_STATE, "Accept Wait")이 나가고 STATE_CODE 원본은 숨긴 채 조회 · 판정에 남는다.
            ReportFlow.StateFlow request = report.StateFlow("Request", Schemas.Request.StateCode, Schemas.Request.StateCode.FlowCodes())
                    .Cancelled(Schemas.Request.StateCode.Draft0.Value, "Cancelled");

            // AOS 쪽 단계 일곱 — Send → Arrival(접수와 랏 생성까지 끝남, PROC_TM) → Exchange → PREP → FIB → TEM → Done(TEM Job End).
            // 공정은 Job Prep → Job Start → Job End. Judge 는 FAIL 만 본다 — SUCC 는 Wafer 표에 들어가 Chip Id · Lamella Id 로 보인다(사용자 2026-10-30).
            // 회차 칸(RUN_CNT)이 2 이상이면 그 칸에 #2 처럼 붙는다. 시각은 마지막 회차 것이다. Link 는 흐름에서 빠져 Logistics 묶음의 Linked At 으로 남는다.
            ReportFlow.EventFlow aos = report.EventFlow("AOS")
                    .Step("Send", Schemas.Logistics.SendTm.Name)
                    .Step("Arrival", Schemas.Logistics.ProcTm.Name)
                    .Step("Exchange", ExchangeTmColumn)
                    .Job("PREP", "PREP_PREP_TM", "PREP_START_TM", "PREP_END_TM")
                            .Fail("PREP_JUDGE_RSLT", Schemas.Wafer.JudgeRslt.Fail.Value).Runs("PREP_RUN_CNT")
                    .Job("FIB", "FIB_PREP_TM", "FIB_START_TM", "FIB_END_TM")
                            .Fail("FIB_JUDGE_RSLT", Schemas.Wafer.JudgeRslt.Fail.Value).Runs("FIB_RUN_CNT")
                    .Job("TEM", "TEM_PREP_TM", "TEM_START_TM", "TEM_END_TM").Runs("TEM_RUN_CNT")
                    .Step("Done", "TEM_END_TM");

            // 조회 조건 — 키는 GetReqList 와 같은 일곱. 필수 · Primary 는 조회 띠에 늘 보이고, 나머지는 Conditions 서랍에 있다(값이 있으면 칩).
            report.DateRange("Request Date", Schemas.Criteria.ReqFromDt, Schemas.Criteria.ReqToDt).Required().LastMonths(1);
            report.Text("Request Id", Schemas.Request.ReqSerialNo).Upper().Placeholder("Request Id (contains)").Primary();
            report.Codes("State Code", Schemas.Request.StateCode, request.Options()).Placeholder("All states").Primary();
            report.DateRange("Accept Date", Schemas.Criteria.AcceptFromDt, Schemas.Criteria.AcceptToDt);
            report.Text("Requester", Schemas.Request.Requester).Placeholder("Requester (contains)");

            // 2단 머리의 묶음 — 이 순서가 그리드 칸 순서다. 왼쪽 두 묶음은 가로 스크롤해도 고정한다.
            report.Band("Id", Schemas.Request.ReqSerialNo.Name, Schemas.Sample.ReqSubSerialNo.Name).Frozen();
            report.Band("Stage", request.StateColumn, aos.StageColumn, RequestReportPresenter.HoldColumn).Frozen();
            report.Band("Lot", Schemas.Logistics.LotId.Name, RequestReportPresenter.SharedColumn);
            report.Band("Sample", SampleBand);
            report.Band("AOS Flow", aos.Columns);
            report.Band("Branch", BranchBand);
            report.Band("Request", RequestBand);
            report.Band("Logistics", LogisticsBand);
            report.Band("FOUP", FoupBand);
            report.Band("PREP", PrepBand);
            report.Band("FIB", FibBand);
            report.Band("TEM", TemBand);
            report.Band("Scrap", ScrapBand);

            // 세로 병합 — 번호 · 시각 칸만. 이름 · 공장 · 장비 · FOUP 칸은 이웃 의뢰서와 값이 같을 수 있어 병합하지 않는다.
            report.Merge(Schemas.Request.ReqSerialNo.Name, Schemas.Logistics.LotId.Name, RequestReportPresenter.SharedColumn,
                    Schemas.Sample.WaferId.Name, Schemas.Sample.ChipId.Name);
            report.Merge(TimeColumns);

            report.Hide(Schemas.Request.Status.Name, Schemas.RequestWafer.SubProdTyp.Name, LotRequestsColumn, LotHoldColumn);
            report.Hide(RequestReportPresenter.HiddenColumns);

            // Judge 칸에는 FAIL 만 온다 — 그 값 그대로 뜻 색(빨강)으로 그린다. 가지 · Hold 칸은 프레젠터가 글자와 색을 따로 만든다.
            report.SemanticBadges("PREP_JUDGE_RSLT", "FIB_JUDGE_RSLT");
            report.SemanticBadge(RequestReportPresenter.HoldColumn, RequestReportPresenter.HoldSemanticColumn);
            report.SemanticBadge(RequestReportPresenter.ReturnColumn, RequestReportPresenter.ReturnSemanticColumn);
            report.SemanticBadge(RequestReportPresenter.SwapColumn, RequestReportPresenter.SwapSemanticColumn);
            report.SemanticBadge(RequestReportPresenter.ReworkColumn, RequestReportPresenter.ReworkSemanticColumn);
            report.SemanticBadge(RequestReportPresenter.ScrapColumn, RequestReportPresenter.ScrapSemanticColumn);

            // Hold 행은 연한 노란 배경, 인디케이터 띠의 번호 옆에는 Hold · RW n 배지(2026-11-05).
            report.RowTint(LotHoldColumn, LotHoldValue, SemanticKind.Warning);
            report.BadgeWhen(LotHoldColumn, LotHoldValue, "Hold", SemanticKind.Warning);
            report.BadgeCount(RequestReportPresenter.ReworkCountColumn, "RW ", SemanticKind.Error);

            // KPI 여섯 — 1건의 뜻: Requests · Lots · Lamellae 는 빈 값 아닌 고유 Id, FAIL 은 현재 PREP 또는 FIB Judge 가 FAIL 인 행(한 행 한 번),
            // Hold · Rework 는 그 상태인 고유 Lot 수. 칸 필터 뒤에는 보이는 행으로 다시 센다(2026-11-05).
            // 집계 칸(HOLD_LOT · REWORK_LOT · FAIL_NOW)은 원본 칸(Hold 칸 · 회차 셋 · Judge 둘)이 다 왔을 때만 생기므로, 하나라도 없으면 카드는 "-" 다.
            report.KpiDistinct("Requests", Schemas.Request.ReqSerialNo.Name)
                    .KpiDistinct("Lots", Schemas.Logistics.LotId.Name)
                    .KpiDistinct("Lamellae", Schemas.RequestWafer.WfId.Name)
                    .KpiMatch("FAIL", RequestReportPresenter.FailNowColumn, Schemas.Wafer.JudgeRslt.Fail.Value)
                    .KpiDistinct("Hold", RequestReportPresenter.HoldLotColumn)
                    .KpiDistinct("Rework", RequestReportPresenter.ReworkLotColumn);

            report.Columns(DecorateColumns);
        }

        // 캡션 · 형식 — 상세 공정 칸의 위 작은 글씨(PREP · FIB · TEM)는 엑셀 머리를 "PREP Job End" 처럼 구분하려고 둔다.
        private static GridColumns DecorateColumns(GridColumns columns)
        {
            return columns
                    .Column(Schemas.Request.ReqSerialNo.Name).Caption("Serial No").TopLabel(string.Empty)
                    .Column(Schemas.Sample.ReqSubSerialNo.Name).Caption("SubSerial No").TopLabel(string.Empty)
                    .Column(RequestReportPresenter.HoldColumn).Caption("Hold").TopLabel(string.Empty)
                    .Column(Schemas.Logistics.LotId.Name).Caption("Lot Id")
                    .Column(RequestReportPresenter.SharedColumn).Caption("Shared")
                    .Column(Schemas.Sample.WaferId.Name).Caption("Wafer Id")
                    .Column(Schemas.Sample.ChipId.Name).Caption("Chip Id")
                    .Column(Schemas.RequestWafer.WfId.Name).Caption("Lamella Id")
                    .Column(RequestReportPresenter.ReturnColumn).Caption("Return")
                    .Column(RequestReportPresenter.SwapColumn).Caption("Swap")
                    .Column(RequestReportPresenter.ReworkColumn).Caption("Rework")
                    .Column(RequestReportPresenter.ScrapColumn).Caption("Scrap")
                    .Column(Schemas.Logistics.SendTm.Name).Caption("Sent At")
                    .Column("LINK_TM").Caption("Linked At")
                    .Column(Schemas.Logistics.ProcTm.Name).Caption("Created At")
                    .Column(Schemas.Logistics.BoxId.Name).Caption("From FOUP")
                    .Column(Schemas.Durable.DurableId.Name).Caption("To FOUP")
                    .Column(ExchangeTmColumn).Caption("Exchanged At")
                    .Column(ReturnTmColumn).Caption("Returned At")
                    .Column(SwapTmColumn).Caption("Swapped At")
                    .Column(ScrapTmColumn).Caption("Scrapped At")
                    .Column(ScrapDueColumn).Caption("Due")
                    .Column(Schemas.DurableLamella.FingerId.Name).Caption("Finger")
                    .Column("PREP_EQP_ID", "FIB_EQP_ID", "TEM_EQP_ID").Caption("Eqp")
                    .Column("PREP_PREP_TM", "FIB_PREP_TM", "TEM_PREP_TM").Caption("Job Prep")
                    .Column("PREP_START_TM", "FIB_START_TM", "TEM_START_TM").Caption("Job Start")
                    .Column("PREP_END_TM", "FIB_END_TM", "TEM_END_TM").Caption("Job End")
                    .Column("PREP_RUN_CNT", "FIB_RUN_CNT", "TEM_RUN_CNT").Caption("Runs")
                    .Column("PREP_FAIL_CNT", "FIB_FAIL_CNT").Caption("Fails")
                    .Column("PREP_JUDGE_RSLT", "FIB_JUDGE_RSLT").Caption("Judge")
                    .Column("PREP_EQP_ID", "PREP_PREP_TM", "PREP_START_TM", "PREP_END_TM", "PREP_RUN_CNT", "PREP_FAIL_CNT",
                            "PREP_JUDGE_RSLT").TopLabel("PREP")
                    .Column("FIB_EQP_ID", "FIB_PREP_TM", "FIB_START_TM", "FIB_END_TM", "FIB_RUN_CNT", "FIB_FAIL_CNT",
                            "FIB_JUDGE_RSLT").TopLabel("FIB")
                    .Column("TEM_EQP_ID", "TEM_PREP_TM", "TEM_START_TM", "TEM_END_TM", "TEM_RUN_CNT").TopLabel("TEM")
                    .Column(TimeColumns).Format("yyyy-MM-dd HH:mm");
        }
    }
}
