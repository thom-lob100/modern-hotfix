using Modern.Lab.Hosting.Reporting;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Theming;

namespace Modern.Lab.Report
{
    public partial class RequestReportForm
    {
        // 리포트 정의 — 리포트가 적는 것은 제목 · 행 키 · 조건 · 흐름 이름 · 가지 코드표 이름 · 리포트 엔티티 · 병합 · 숨김 · 행 배경뿐이다.
        // 응답 표의 칸 순서 · 캡션 · 묶음은 리포트 엔티티 한 장(RequestReportEntity, Hosting Schema/Entities)이 정한다 — 칸을 더하거나 빼거나
        // 묶음을 바꾸려면 그 엔티티의 선언 줄을 고친다(묶음은 첫 칸의 .Band("…") 표시). 이 파일은 손댈 것이 없다.
        // 단계(AOS_STAGE) · 가지(<코드>_YN) · Shared 는 서버(쿼리)가 내려주고, 단계 순서 · 이름은 코드표(AOS_STAGE · AOS_BRANCH)가 정한다.
        // 단계에 붙는 칸은 접두 규칙(<코드>_TM · _PREP_TM · _START_TM · _END_TM · _EQP_ID · _RUN_CNT · _FAIL_CNT · _JUDGE_RSLT)으로 틀이 찾는다.
        // 무엇을 채울지(조회)는 .Server.cs 가 갖는다.

        // 가지 코드 HOLD 의 Y 칸 — Hold 행의 연한 노란 배경(2026-11-05 기준).
        private const string HoldValue = "Y";

        protected override void DefineReport(ReportDefinition report)
        {
            RequestReportEntity row = Schemas.RequestReport;

            report.Title = "Request Report";
            report.Environment = "AOS";
            report.EmptyText = "No requests";
            report.ExportSheetName = "Request Report";
            report.ExportFilePrefix = "RequestReport";
            report.Key(row.ReqSerialNo, row.ReqSubSerialNo);

            // 의뢰서 쪽 단계 — Request 화면과 같은 일곱 칸(엔티티의 코드 순서). DRAFT0 은 취소(회색 배지, 인디케이터 전부 대기).
            ReportFlow.StateFlow request = report.StateFlow("Request", row.StateCode, row.StateCode.FlowCodes())
                    .Cancelled(row.StateCode.Draft0.Value, "Cancelled");

            // AOS 쪽 단계 — 서버가 준 AOS_STAGE 값이 현재 단계, 순서 · 이름은 코드표 AOS_STAGE. Judge 는 FAIL 만 본다(사용자 2026-10-30).
            report.CodeFlow("AOS").Fail(Schemas.Wafer.JudgeRslt.Fail.Value);

            // 가지 — 코드표 AOS_BRANCH 의 코드마다 <코드>_YN. Branch 묶음 · 띠 배지 · KPI 가 이 순서로 선다.
            report.Branches("AOS_BRANCH");

            // 조회 조건 — 키는 GetReqList 와 같은 일곱. 필수 · Primary 는 조회 띠에 늘 보이고, 나머지는 Conditions 서랍에 있다(값이 있으면 칩).
            report.DateRange("Request Date", Schemas.Criteria.ReqFromDt, Schemas.Criteria.ReqToDt).Required().LastMonths(1);
            report.Text("Request Id", row.ReqSerialNo).Upper().Placeholder("Request Id (contains)").Primary();
            report.Codes("State Code", row.StateCode, request.Options()).Placeholder("All states").Primary();
            report.DateRange("Accept Date", Schemas.Criteria.AcceptFromDt, Schemas.Criteria.AcceptToDt);
            report.Text("Requester", row.Requester).Placeholder("Requester (contains)");

            // 묶음 = 리포트 엔티티의 Band 표시(Request · Logistics · Durable · Sample · Stage). Id · Stage · AOS Flow · Branch 는 틀이 앞에 세우고,
            // 단계 상세(PREP · FIB · TEM)는 뒤에 선다.
            report.Bands(row);

            // 세로 병합 — 번호 칸만. 이름 · 공장 · 장비 · FOUP 칸은 이웃 의뢰서와 값이 같을 수 있어 병합하지 않는다(단계 시각 칸은 틀이 병합한다).
            report.Merge(row.ReqSerialNo.Name, row.LotId.Name, row.SharedWith.Name, row.WaferId.Name, row.ChipId.Name);

            report.Hide(row.Status.Name);
            report.RowTint(row.HoldYn.Name, HoldValue, SemanticKind.Warning);
        }
    }
}
