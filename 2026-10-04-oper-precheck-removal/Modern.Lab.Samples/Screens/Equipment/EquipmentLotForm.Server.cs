using System;
using System.Data;
using System.Text;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Samples.Services;

namespace Modern.Lab.Samples
{
    public partial class EquipmentLotForm
    {
        // 요청 전용 전송 키 — 응답 컬럼이 아니다. 소스 Port 는 IN_PORT, 대상 Port 는 OUT_PORT 로 보낸다.
        private const string PortNmKey = "IN_PORT";
        private const string GoalPortNmKey = "OUT_PORT";
        private const string ModeKey = "MODE";

        // 응답 필수: EQP_GRP_ID
        private DataTable GetEquipmentGroups()
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetEqpGroupList");
            return this.Request(request.ToString()).Table;
        }

        // 응답 필수: EQP_ID
        // 표시·판정: EQP_GRP_ID, COMM_STAT_TYP, MES_EQP_STAT_CD, AUTO_YN, PRIORITY, PORT_NM, GOAL_PORT_NM
        // 응답 필수(JobPrep 실행 조건 — 없으면 JobPrep 이 꺼진다): COMM_STAT_TYP, MES_EQP_STAT_CD, MES_STAT_TYP(장비 Up/Down),
        //   EQP_TYP(ProductionMachine), DET_EQP_TYP(Process·Sorter·Cleaner·Metrology·Test)
        private DataTable GetEquipmentPriorities(string eqpGrpId)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetEqpList");
            AppendField(request, ServerFields.Equipment.EqpGrpKey, eqpGrpId);
            return this.Request(request.ToString()).Table;
        }

        // Lot / Equipment 화면(Lot 이 기준) — 고른 Lot 에 맞는 장비 목록·순위를 받는다. 응답은 위 GetEqpList 와 같다.
        //   요청: EQP_GRP_ID, LOT_ID(고른 Lot). 홈 서버는 LOT_ID 를 쓰지 않고 그룹의 장비 순위를 그대로 준다 — 회사 서버는 Lot 을 보고
        //   순위를 주거나 그 Lot 에 이미 걸린 장비를 내려준다. 포트·Durable 전문도 같은 식으로 LOT_ID 를 싣는다.
        private DataTable GetEquipmentPriorities(string eqpGrpId, string lotId)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetEqpList");
            AppendGroup(request, eqpGrpId);
            AppendLot(request, lotId);
            return this.Request(request.ToString()).Table;
        }

        // Lot / Equipment 화면만 LOT_ID 를 싣는다 — 서버가 그 Lot 을 보고 순위를 주거나, Lot 에 이미 걸린 장비·포트·Durable 을 내려준다.
        // Equipment / Lots 화면은 lotId 가 비어 있어 키를 싣지 않는다(전문이 전과 같다).
        // Lot / Equipment 화면은 장비그룹 조건이 없다 — 그룹이 비어 있으면 키를 싣지 않는다(서버가 Lot 으로 대상을 정한다).
        private static void AppendGroup(StringBuilder request, string eqpGrpId)
        {
            if (!string.IsNullOrEmpty(eqpGrpId))
            {
                AppendField(request, ServerFields.Equipment.EqpGrpKey, eqpGrpId);
            }
        }

        private static void AppendLot(StringBuilder request, string lotId)
        {
            if (!string.IsNullOrEmpty(lotId))
            {
                AppendField(request, Schemas.LotPriority.LotId.Name, lotId);
            }
        }

        // 응답 필수: PORT_NM
        // 표시·판정: EQP_ID, PORT_TYP, TRANSFER_STAT_CD, DURABLE_TYP, PRIORITY
        // 요청: EQP_ID, LOT_ID(Lot / Equipment 화면에서 고른 Lot — 그 화면만 싣는다)
        private DataTable GetPortPriorities(string eqpId, string lotId)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetEqpPortList");
            AppendField(request, Schemas.EquipmentPriority.EqpId.Name, eqpId);
            AppendLot(request, lotId);
            return this.Request(request.ToString()).Table;
        }

        // 응답 필수: LOT_ID
        // 표시·판정: EQP_ID, PORT_NM, GOAL_PORT_NM, CARRIER_ID, GOAL_CARRIER_ID, LAST_EVENT_CD, LOT_HOLD_STAT_CD,
        //            MES_PROC_STAT_CD, LOT_STAT_TYP, PRIORITY — EQP_ID·PORT_NM·GOAL_PORT_NM·GOAL_CARRIER_ID 는 진행 중 Job 의 값,
        //            CARRIER_ID 는 Lot 이 지금 담긴 소스 Durable 이다.
        // 응답 필수(실행 조건): LAST_EVENT_CD(JobStart 는 Reserved 일 때), JobPrep 은 LOT_PROC_STAT_CD(LoggedOut) ·
        //   MES_PROC_STAT_CD(WAIT) · LOT_HOLD_STAT_CD(NotOnHold) · LOT_STAT_TYP(Released). 없으면 해당 버튼이 꺼진다.
        // Lot 목록과 그 우선순위는 장비그룹 단위다 — 장비 Id 는 싣지 않는다.
        private DataTable GetLotPriorities(string eqpGrpId)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetJobLotList");
            AppendGroup(request, eqpGrpId);
            return this.Request(request.ToString()).Table;
        }

        // 응답 필수: DURABLE_ID
        // 표시·판정: DURABLE_TYP, DURABLE_STAT_CD, WF_LOAD_STAT_CD, LOT_QTY, PRIORITY
        // 응답 필수(JobPrep 실행 조건 — 대상 Durable): MES_STAT_TYP(Normal), CLEAN_TYP(N)
        // 요청: EQP_ID, PORT_NM(대상 포트), LOT_ID(Lot / Equipment 화면에서 고른 Lot — 그 화면만 싣는다)
        private DataTable GetDurablePriorities(string eqpId, string goalPortNm, string lotId)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetJobDurableList");
            AppendField(request, Schemas.EquipmentPriority.EqpId.Name, eqpId);
            AppendField(request, Schemas.PortPriority.PortNm.Name, goalPortNm);
            AppendLot(request, lotId);
            return this.Request(request.ToString()).Table;
        }

        // ── Job 전문 — 회사 교체 지점 ──
        // Job 메서드들(Prep·Start·End·PrepCancel·StartCancel·Skip·Undo)은 같은 인자를 받는다. 전문명과 실제로 보낼 필드는 장비그룹(eqpGrpId) × 통신 상태(commStatTyp)로
        // 이 파일에서만 고른다(본체·Rules 는 전문명을 조합하지 않는다). 아래 분기는 홈 데이터(GRP-A~GRP-D)의 예제이며
        // 회사 계약이 아니다 — 회사는 case(그룹)·if(통신 상태)·전문명·전송 필드를 자기 표로 교체한다.
        // 표에 없는 그룹·통신 상태는 전문을 보내지 않고 ArgumentException 으로 알린다(성공으로 가장하지 않는다).
        // 판정은 서버가 한다 — 홈 서버는 OffLine 장비의 Prep 과 OnLineLocal 이 아닌 장비의 Start·End 를 거절한다.
        // 인자 값의 출처: JobPrep 은 새 조합(선택·추천 장비/소스 Port/대상 Port/대상 Durable + Lot 의 CARRIER_ID),
        // JobStart·JobEnd 와 되돌리기·건너뛰기(JobPrepCancel·JobStartCancel·JobUndo·JobSkip)는 선택 Lot 의 기존 Job 값
        // (EQP_ID·PORT_NM·GOAL_PORT_NM·CARRIER_ID·GOAL_CARRIER_ID)이다. 홈 서버의 의미: JobPrepCancel = 준비 취소(포트 해제),
        // JobStartCancel·JobUndo = JobPrep 으로 되돌림(이벤트 이름만 다르다), JobSkip = 이 공정을 건너뛰어 END.

        // 응답 필수: 없음
        private DataActionResult JobPrep(
                string eqpGrpId, string commStatTyp, string eqpId, string portNm, string goalPortNm,
                string lotId, string durableId, string goalDurableId, string description)
        {
            return this.Request(JobPrepRequest(
                    eqpGrpId, commStatTyp, eqpId, portNm, goalPortNm, lotId, durableId, goalDurableId, description,
                    this.CurrentUserId));
        }

        // 응답 필수: 없음
        private DataActionResult JobStart(
                string eqpGrpId, string commStatTyp, string eqpId, string portNm, string goalPortNm,
                string lotId, string durableId, string goalDurableId, string description)
        {
            return this.Request(JobRunRequest(
                    "JobStart", eqpGrpId, commStatTyp, eqpId, portNm, goalPortNm, lotId, durableId, goalDurableId, description,
                    this.CurrentUserId));
        }

        // 응답 필수: 없음.
        // Job End 는 장비그룹이 Chip·Lamella 생성 그룹이면 판정 표를 함께 실어 **한 전문**으로 보낸다(종료와 판정이 한 번에
        // 비즈로직을 탄다). 판정 표는 JUDGE_RESULTS=[{행},…] JSON 배열 하나다 — 행마다 컬럼 이름과 값이 함께 있어 순서가
        // 어긋나지 않고, GRID_INDEX 에 콤마가 있어도 쪼개지지 않는다. 전문 이름은 홈 예제(세 갈래 모두 JobEnd 표)이며
        // 회사는 갈래마다 전문 이름을 교체한다. 판정 그룹인데 판정 표가 없으면 보내지 않는다.
        private DataActionResult JobEnd(
                string eqpGrpId, string commStatTyp, string eqpId, string portNm, string goalPortNm,
                string lotId, string durableId, string goalDurableId, string description, DataTable judgeResults)
        {
            switch (JudgeKindOf(eqpGrpId))
            {
                case JudgeKind.Chip:
                    if (judgeResults == null)
                    {
                        throw new ArgumentException("Chip judge rows are required to end this job.");
                    }

                    return this.Request(JobRunRequest(
                            "JobEnd", eqpGrpId, commStatTyp, eqpId, portNm, goalPortNm, lotId, durableId, goalDurableId, description,
                            this.CurrentUserId) + " " + JudgeResultsKey + "=" + JudgeResults.ToJson(judgeResults));

                case JudgeKind.Lamella:
                    if (judgeResults == null)
                    {
                        throw new ArgumentException("Lamella judge rows are required to end this job.");
                    }

                    return this.Request(JobRunRequest(
                            "JobEnd", eqpGrpId, commStatTyp, eqpId, portNm, goalPortNm, lotId, durableId, goalDurableId, description,
                            this.CurrentUserId) + " " + JudgeResultsKey + "=" + JudgeResults.ToJson(judgeResults));

                default:
                    return this.Request(JobRunRequest(
                            "JobEnd", eqpGrpId, commStatTyp, eqpId, portNm, goalPortNm, lotId, durableId, goalDurableId, description,
                            this.CurrentUserId));
            }
        }

        // 판정 표 전문 키.
        private const string JudgeResultsKey = "JUDGE_RESULTS";

        // Job End 판정 종류는 **장비그룹으로만** 정한다. 홈: GRP-C = Chip 생성, GRP-D = Lamella 생성, 나머지는 판정 없음.
        // 회사는 그룹 이름만 바꾼다.
        internal static JudgeKind JudgeKindOf(string eqpGrpId)
        {
            switch ((eqpGrpId ?? string.Empty).Trim())
            {
                case "GRP-C":
                    return JudgeKind.Chip;

                case "GRP-D":
                    return JudgeKind.Lamella;

                default:
                    return JudgeKind.None;
            }
        }

        // 응답 필수: 없음
        private DataActionResult JobPrepCancel(
                string eqpGrpId, string commStatTyp, string eqpId, string portNm, string goalPortNm,
                string lotId, string durableId, string goalDurableId, string description)
        {
            return this.Request(JobRunRequest(
                    "JobPrepCancel", eqpGrpId, commStatTyp, eqpId, portNm, goalPortNm, lotId, durableId, goalDurableId, description,
                    this.CurrentUserId));
        }

        // 응답 필수: 없음
        private DataActionResult JobStartCancel(
                string eqpGrpId, string commStatTyp, string eqpId, string portNm, string goalPortNm,
                string lotId, string durableId, string goalDurableId, string description)
        {
            return this.Request(JobRunRequest(
                    "JobStartCancel", eqpGrpId, commStatTyp, eqpId, portNm, goalPortNm, lotId, durableId, goalDurableId, description,
                    this.CurrentUserId));
        }

        // 응답 필수: 없음.
        // Skip 은 다른 Job 전문과 같은 표에 두 값을 더 싣는다(2026-10-04):
        //   OPER_ID  — 건너뛸 공정. Skip 대상 Lot 행의 OPER_ID 다(엔티티 컬럼 이름 그대로).
        //   skipCode — Skip 창에서 고른 사유 코드(REASON_CD). 사유 분류 SKIPCODE 로 조회한 목록에서 고른 값이다.
        // skipCode 라는 이름은 홈 예제다(Hold 의 holdCode · Scrap 의 scrapCode 와 같은 꼴) — 회사 전문 키가 다르면 SkipCodeKey 를 바꾼다.
        // Engineer 는 받지 않는다. 실행자(CurrentUserId)는 다른 Job 전문과 같이 실린다.
        private DataActionResult JobSkip(
                string eqpGrpId, string commStatTyp, string eqpId, string portNm, string goalPortNm,
                string lotId, string durableId, string goalDurableId, string operId, string skipCode, string description)
        {
            if (string.IsNullOrWhiteSpace(skipCode))
            {
                throw new ArgumentException("A skip code is required to skip this lot.");
            }

            StringBuilder request = new StringBuilder(JobRunRequest(
                    "JobSkip", eqpGrpId, commStatTyp, eqpId, portNm, goalPortNm, lotId, durableId, goalDurableId, description,
                    this.CurrentUserId));
            AppendField(request, Schemas.LotPriority.OperId.Name, operId);
            AppendField(request, SkipCodeKey, skipCode);
            return this.Request(request.ToString());
        }

        // Skip 사유 코드의 전문 키 — 홈 예제 이름이다.
        private const string SkipCodeKey = "skipCode";

        // 응답 필수: 없음
        private DataActionResult JobUndo(
                string eqpGrpId, string commStatTyp, string eqpId, string portNm, string goalPortNm,
                string lotId, string durableId, string goalDurableId, string description)
        {
            return this.Request(JobRunRequest(
                    "JobUndo", eqpGrpId, commStatTyp, eqpId, portNm, goalPortNm, lotId, durableId, goalDurableId, description,
                    this.CurrentUserId));
        }

        private static string JobPrepRequest(
                string eqpGrpId, string commStatTyp, string eqpId, string portNm, string goalPortNm,
                string lotId, string durableId, string goalDurableId, string description, string userId)
        {
            StringBuilder request = new StringBuilder();

            switch (eqpGrpId)
            {
                // 홈 장비그룹 — 네 그룹이 같은 표를 쓴다. 회사는 그룹마다 case 를 나눠 적는다.
                case "GRP-A":
                case "GRP-B":
                case "GRP-C":
                case "GRP-D":
                    if (commStatTyp == Schemas.EquipmentPriority.CommStatTyp.OnLineRemote.Code)
                    {
                        // 원격: 호스트가 반송까지 하므로 소스 Durable(CARRIER_ID)도 보낸다.
                        request.Append("JobPrepRemote");
                        AppendJobTarget(request, eqpGrpId, eqpId, portNm, goalPortNm, lotId);
                        AppendField(request, Schemas.LotPriority.DurableId.Name, durableId);
                        AppendField(request, Schemas.LotPriority.GoalDurableId.Name, goalDurableId);
                    }
                    else if (commStatTyp == Schemas.EquipmentPriority.CommStatTyp.OnLineLocal.Code)
                    {
                        // 로컬: 작업자가 싣는다 — 대상 Durable 만 보낸다.
                        request.Append("JobPrepLocal");
                        AppendJobTarget(request, eqpGrpId, eqpId, portNm, goalPortNm, lotId);
                        AppendField(request, Schemas.LotPriority.GoalDurableId.Name, goalDurableId);
                    }
                    else if (commStatTyp == Schemas.EquipmentPriority.CommStatTyp.OffLine.Code)
                    {
                        // 오프라인: 로컬과 같은 필드로 보낸다 — 홈 서버가 거절 사유를 돌려준다.
                        request.Append("JobPrepOffline");
                        AppendJobTarget(request, eqpGrpId, eqpId, portNm, goalPortNm, lotId);
                        AppendField(request, Schemas.LotPriority.GoalDurableId.Name, goalDurableId);
                    }
                    else
                    {
                        throw UnknownJobBranch("JobPrep", eqpGrpId, commStatTyp);
                    }

                    break;

                default:
                    throw UnknownJobBranch("JobPrep", eqpGrpId, commStatTyp);
            }

            AppendField(request, ServerFields.Description, description);
            AppendField(request, ServerFields.UserId, userId);
            return request.ToString();
        }

        // JobStart·JobEnd·JobPrepCancel·JobStartCancel·JobSkip·JobUndo 는 같은 표를 쓴다 — job 이 그 이름이고, 통신 상태 접미어를 붙여 전문명이 된다.
        private static string JobRunRequest(
                string job, string eqpGrpId, string commStatTyp, string eqpId, string portNm, string goalPortNm,
                string lotId, string durableId, string goalDurableId, string description, string userId)
        {
            StringBuilder request = new StringBuilder();

            switch (eqpGrpId)
            {
                // 홈 장비그룹 — 네 그룹이 같은 표를 쓴다. 회사는 그룹마다 case 를 나눠 적는다.
                case "GRP-A":
                case "GRP-B":
                case "GRP-C":
                case "GRP-D":
                    if (commStatTyp == Schemas.EquipmentPriority.CommStatTyp.OnLineLocal.Code)
                    {
                        // 로컬: 작업자가 실행한다 — 기존 Job 의 장비·소스/대상 Port·소스/대상 Durable 을 모두 보낸다.
                        request.Append(job).Append("Local");
                        AppendJobTarget(request, eqpGrpId, eqpId, portNm, goalPortNm, lotId);
                        AppendField(request, Schemas.LotPriority.DurableId.Name, durableId);
                        AppendField(request, Schemas.LotPriority.GoalDurableId.Name, goalDurableId);
                    }
                    else if (commStatTyp == Schemas.EquipmentPriority.CommStatTyp.OnLineRemote.Code)
                    {
                        // 원격: 호스트가 실행한다 — 장비와 Lot 만 보낸다. 홈 서버는 OnLineLocal 이 아니라고 거절한다.
                        request.Append(job).Append("Remote");
                        AppendField(request, ServerFields.Equipment.EqpGrpKey, eqpGrpId);
                        AppendField(request, Schemas.EquipmentPriority.EqpId.Name, eqpId);
                        AppendField(request, Schemas.LotPriority.LotId.Name, lotId);
                    }
                    else if (commStatTyp == Schemas.EquipmentPriority.CommStatTyp.OffLine.Code)
                    {
                        // 오프라인: 원격과 같은 필드로 보낸다 — 홈 서버가 거절 사유를 돌려준다.
                        request.Append(job).Append("Offline");
                        AppendField(request, ServerFields.Equipment.EqpGrpKey, eqpGrpId);
                        AppendField(request, Schemas.EquipmentPriority.EqpId.Name, eqpId);
                        AppendField(request, Schemas.LotPriority.LotId.Name, lotId);
                    }
                    else
                    {
                        throw UnknownJobBranch(job, eqpGrpId, commStatTyp);
                    }

                    break;

                default:
                    throw UnknownJobBranch(job, eqpGrpId, commStatTyp);
            }

            AppendField(request, ServerFields.Description, description);
            AppendField(request, ServerFields.UserId, userId);
            return request.ToString();
        }

        private static void AppendJobTarget(
                StringBuilder request, string eqpGrpId, string eqpId, string portNm, string goalPortNm, string lotId)
        {
            AppendField(request, ServerFields.Equipment.EqpGrpKey, eqpGrpId);
            AppendField(request, Schemas.EquipmentPriority.EqpId.Name, eqpId);
            AppendField(request, PortNmKey, portNm);
            AppendField(request, GoalPortNmKey, goalPortNm);
            AppendField(request, Schemas.LotPriority.LotId.Name, lotId);
        }

        private static ArgumentException UnknownJobBranch(string job, string eqpGrpId, string commStatTyp)
        {
            return new ArgumentException(
                    "No " + job + " message is defined for equipment group '" + eqpGrpId + "' in mode '" + commStatTyp + "'.",
                    "eqpGrpId");
        }

        // 응답 필수: 없음
        private DataActionResult SetMode(string eqpId, string mode)
        {
            StringBuilder request = new StringBuilder();
            request.Append("SetEqpMode");
            AppendField(request, Schemas.EquipmentPriority.EqpId.Name, eqpId);
            AppendField(request, ModeKey, mode);
            AppendField(request, ServerFields.UserId, this.CurrentUserId);
            return this.Request(request.ToString());
        }

        // 응답 필수: 없음
        private DataActionResult SetAuto(string eqpId, bool autoYn)
        {
            StringBuilder request = new StringBuilder();
            request.Append("SetEqpAuto");
            AppendField(request, Schemas.EquipmentPriority.EqpId.Name, eqpId);
            AppendField(request, Schemas.EquipmentPriority.AutoYn.Name,
                    autoYn ? Schemas.EquipmentPriority.AutoYn.Y.Code : Schemas.EquipmentPriority.AutoYn.N.Code);
            AppendField(request, ServerFields.UserId, this.CurrentUserId);
            return this.Request(request.ToString());
        }

        // 응답 필수: REQ_SERIAL_NO
        private DataTable GetRequests(string reqSerialNo)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetReqList");
            AppendField(request, Schemas.Request.ReqSerialNo.Name, reqSerialNo);
            return this.Request(request.ToString()).Table;
        }

        // 선택한 의뢰의 Sample 목록 — 회사 전문 이름·키는 여기서 바꾼다(메서드 이름과 전문 이름은 별개다).
        // 응답 필수: 없음. 표시: REQ_SERIAL_NO·REQ_SUBSERIAL_NO(Sample 복합 키), LOT_ID, UNIT_ID, WAFER_ID
        private DataTable GetSamples(string reqSerialNo)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetRequestInfo");
            AppendField(request, Schemas.Request.ReqSerialNo.Name, reqSerialNo);
            return this.Request(request.ToString()).Table;
        }

        private static void AppendField(StringBuilder request, string key, string value)
        {
            request.Append(' ').Append(key).Append('=').Append(value);
        }
    }
}
