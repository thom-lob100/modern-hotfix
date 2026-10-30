using System.Data;

using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Samples.Services;

namespace Modern.Lab.Samples
{
    public partial class LotEquipmentForm
    {
        // Job 일곱 액션의 조건 — 액션마다 ActionRule 목록이고 위에서부터 보며, 처음 막은 줄의 문구가 버튼·메뉴 툴팁에 뜬다.
        // 액션별 조건은 해당 목록에 추가한다(2026-10-15 사용자 지시 — 전에는 if 문 메서드라 규칙 안내 창에 안 나왔다). 공통 목록은 없다(2026-10-16).
        // 대조하는 행은 EquipmentLotPresenter.RuleRow(decision) 가 만드는 「결정 행」 하나다 — 결정 Lot 행의 LOT_ID · LAST_EVENT_CD · PRIORITY,
        // 작업 장비 행의 EQP_ID · COMM_STAT_TYP(작업이 걸린 Lot 은 그 작업의 장비, 아니면 결정 장비), 결정 In/Out 포트 이름, 목표 Durable(GOAL_CARRIER_ID),
        // 포트 짝(PORTS_FIT = Y, Decision 엔티티)을 한 행에 모은다. 그래서 장비 · Lot · 준비 재료 조건이 모두 값 규칙으로 들어가 안내 창에
        // 필드 · 요구 값 · 지금 값으로 보이고(문장 규칙 없음 — 2026-10-16 사용자 지시), 안내 창 머리글은 LOT_ID 다.
        // Cancel 은 잘못 눌러 되돌리는 것(JobPrep Cancel = 준비 전으로, JobStart Cancel = JobPrep 으로), Back to Prev Oper 는 실제 JobStart 를 했지만
        // 다시 JobStart 하러 JobPrep 으로 되돌아가는 것, Skip Oper 는 이 공정을 취소해 다음 공정으로 보내는 것이다(2026-09-30 사용자 정의).
        // JobPrep Cancel · JobStart Cancel 은 Lot 행의 상태 값(MES · Lot · Hold · 투입 상태)과 Lot 을 담은 캐리어로 연다(2026-10-28 사용자 지시 —
        // 2026-10-07 의 "JobStart · JobEnd 와 같은 조건"을 대체).
        // Back to Prev Oper 는 되돌릴 상태일 때만, Skip Oper 는 실행 중(JobStart)이 아닐 때만 열린다 — 둘 다 OnLineLocal 장비에서만(2026-10-07 사용자 결정).
        // Lot · 장비 · 포트 · Durable 의 자격(상태 · 종류 · 빈 포트 · 빈 Durable)은 서버가 우선순위에서 빼서 내려주고, 화면은 다시 보지 않는다
        // (2026-10-07 사용자 결정 — 기준표 2026-09-13 「우선순위가 곧 대상이다」). 화면이 보는 것은 선택 여부 · 순위 여부 · 작업 단계 · 통신 모드 · 포트 짝이다.
        // 순위(PRIORITY)는 규칙에 두지 않는다(2026-10-16 사용자 결정) — 순위가 비는 사정은 다른 줄(상태 · 장비 모드)에 드러나고, 최종 판정은 서버다.
        // 판정은 이 파일에서 끝난다 — 본체는 JobActionReason 결과를 버튼·메뉴에 적용하고 Job 확인 뒤 실행 직전에 다시 부른다.
        // Decision · In list 줄(EquipmentInList · PortInList · GoalPortInList · ChildInList · LotInList · DurableInList): Job 버튼·메뉴를 누르면 본체가
        // 확인 창 전에 결정 재료를 서버에서 조용히 다시 받는데(Prep 은 장비 · 포트 · 자식 · Durable · Lot 한 건을 동시에, 나머지는 Lot 한 건 + 장비 목록),
        // 결정한 행이 새 목록에 없으면 그 자리가 Missing 이 돼 이 줄이 막는다 — 서버가 중간 서비스에서 거절 문구를 못 돌려주는 경우의 보완이다
        // (2026-10-29 사용자 지시). 지금 값은 결정 Id 또는 Missing, 결정이 없으면 빈 값이라 막지 않는다(선택 여부는 NotEmpty 줄이 본다).
        // 장비 모드·포트 메뉴 조건은 EquipmentPortManagementForm.Rules.cs를 공유한다.

        internal const string NoDecision = "Pick an equipment, ports, a lot and a goal durable first.";

        private const string LotEntityName = "Lot";
        private const string DecidedEquipmentEntityName = "Decided Equipment";
        private const string JobEquipmentEntityName = "Job Equipment";
        private const string ChildEquipmentEntityName = "Child Equipment";
        private const string DecisionEntityName = "Decision";

        // 공통 목록은 두지 않는다 — 액션마다 열리기 위한 조건 전부를 자기 목록에 적는다(2026-10-16 사용자 지시). JobPrep 은 사용자가
        // 결정한 장비(Decided Equipment)를, 작업이 걸린 뒤의 액션은 그 작업의 장비(Job Equipment)를 본다. 결정 행이 두 장비의 모드를 따로 담는다.

        // JobPrep — 결정 장비가 통신 중(Remote 또는 Local) · Lot 선택 · Lot 자격 네 줄(LoggedOut · WAIT · NotOnHold · Released) · 포트 짝 · 목표 Durable
        // · 자식 장비 짝(Child Eqp Fit — 자식 장비가 있는 장비(핸들러)면 자식 하나가 결정돼 있어야 한다. 자식 없는 장비는 늘 Y. 2026-10-20 사용자 지시)
        // · 자식 장비 모드(Child Equipment · Mode — 부모 규칙과 같은 꼴. 결정 행의 CHILD_COMM_STAT_TYP · CHILD_MES_EQP_STAT_CD 는 결정 자식의 값이고,
        //   자식 없는 장비면 부모 값이 그대로 들어간다. 회사에서 자식 조건을 더할 때는 이 줄처럼 ChildEquipmentEntityName 으로 한 줄씩 — 2026-10-20 사용자 지시).
        // 자격 네 줄은 순위 없는 Lot 을 클릭해 결정했을 때를 위한 것이다 — 서버가 순위를 준 Lot 은 이미 통과한다
        // (2026-10-16 사용자 지시, 2026-10-07 에 뺐던 것을 되살림). 진행 중인 작업(JobPrep · Reserved · JobStart)이 걸린 Lot 은
        // WAIT · LoggedOut 이 아니므로 작업 단계 줄을 따로 두지 않는다(2026-10-16 사용자 확인).
        private static readonly ActionRule[] JobPrepRules =
        {
            ActionRule.NotValue(DecisionEntityName, EquipmentLotPresenter.DecisionRule.EquipmentInList, EquipmentLotPresenter.DecisionRule.EquipmentInList.Missing),
            ActionRule.NotValue(DecisionEntityName, EquipmentLotPresenter.DecisionRule.PortInList, EquipmentLotPresenter.DecisionRule.PortInList.Missing),
            ActionRule.NotValue(DecisionEntityName, EquipmentLotPresenter.DecisionRule.GoalPortInList, EquipmentLotPresenter.DecisionRule.GoalPortInList.Missing),
            ActionRule.NotValue(DecisionEntityName, EquipmentLotPresenter.DecisionRule.ChildInList, EquipmentLotPresenter.DecisionRule.ChildInList.Missing),
            ActionRule.NotValue(DecisionEntityName, EquipmentLotPresenter.DecisionRule.LotInList, EquipmentLotPresenter.DecisionRule.LotInList.Missing),
            ActionRule.NotValue(DecisionEntityName, EquipmentLotPresenter.DecisionRule.DurableInList, EquipmentLotPresenter.DecisionRule.DurableInList.Missing),
            ActionRule.Value(DecidedEquipmentEntityName, EquipmentLotPresenter.DecisionRule.DecidedEqpMode,
                    EquipmentLotPresenter.DecisionRule.DecidedEqpMode.OnLineRemote, EquipmentLotPresenter.DecisionRule.DecidedEqpMode.OnLineLocal),
            ActionRule.Value(ChildEquipmentEntityName, EquipmentLotPresenter.DecisionRule.ChildEqpMode,
                    EquipmentLotPresenter.DecisionRule.ChildEqpMode.OnLineRemote, EquipmentLotPresenter.DecisionRule.ChildEqpMode.OnLineLocal),
            ActionRule.NotEmpty(LotEntityName, Schemas.LotPriority.LotId),
            ActionRule.Value(LotEntityName, Schemas.LotPriority.LotProcStatCd, Schemas.LotPriority.LotProcStatCd.LoggedOut),
            ActionRule.Value(LotEntityName, Schemas.LotPriority.MesProcStatCd, Schemas.LotPriority.MesProcStatCd.Wait),
            ActionRule.Value(LotEntityName, Schemas.LotPriority.LotHoldStatCd, Schemas.LotPriority.LotHoldStatCd.NotOnHold),
            ActionRule.Value(LotEntityName, Schemas.LotPriority.LotStatTyp, Schemas.LotPriority.LotStatTyp.Released),
            ActionRule.Value(DecisionEntityName, EquipmentLotPresenter.DecisionRule.PortsFit, EquipmentLotPresenter.DecisionRule.PortsFit.Fit),
            ActionRule.Value(DecisionEntityName, EquipmentLotPresenter.DecisionRule.ChildFit, EquipmentLotPresenter.DecisionRule.ChildFit.Fit),
            ActionRule.NotEmpty(DecisionEntityName, Schemas.LotPriority.GoalDurableId)
        };

        // JobStart — 작업 장비가 OnLineLocal · Lot 선택 · 준비가 끝난(Reserved) Lot.
        private static readonly ActionRule[] JobStartRules =
        {
            ActionRule.NotValue(DecisionEntityName, EquipmentLotPresenter.DecisionRule.EquipmentInList, EquipmentLotPresenter.DecisionRule.EquipmentInList.Missing),
            ActionRule.NotValue(DecisionEntityName, EquipmentLotPresenter.DecisionRule.LotInList, EquipmentLotPresenter.DecisionRule.LotInList.Missing),
            ActionRule.Value(JobEquipmentEntityName, EquipmentLotPresenter.DecisionRule.JobEqpMode, EquipmentLotPresenter.DecisionRule.JobEqpMode.OnLineLocal),
            ActionRule.NotEmpty(LotEntityName, Schemas.LotPriority.LotId),
            ActionRule.Value(LotEntityName, Schemas.LotPriority.LastEventCd, Schemas.LotPriority.LastEventCd.Reserved)
        };

        // JobEnd — 작업 장비가 OnLineLocal · Lot 선택 · 실행 중(JobStart)인 Lot. Back to Prev Oper 도 같은 조건.
        private static readonly ActionRule[] JobEndRules =
        {
            ActionRule.NotValue(DecisionEntityName, EquipmentLotPresenter.DecisionRule.EquipmentInList, EquipmentLotPresenter.DecisionRule.EquipmentInList.Missing),
            ActionRule.NotValue(DecisionEntityName, EquipmentLotPresenter.DecisionRule.LotInList, EquipmentLotPresenter.DecisionRule.LotInList.Missing),
            ActionRule.Value(JobEquipmentEntityName, EquipmentLotPresenter.DecisionRule.JobEqpMode, EquipmentLotPresenter.DecisionRule.JobEqpMode.OnLineLocal),
            ActionRule.NotEmpty(LotEntityName, Schemas.LotPriority.LotId),
            ActionRule.Value(LotEntityName, Schemas.LotPriority.LastEventCd, Schemas.LotPriority.LastEventCd.JobStart)
        };

        // JobPrep Cancel — 작업 장비 OnLineLocal · Lot 선택 · Lot 이 준비 단계(SELE · RESV · LOAD) · Released · NotOnHold · WaitingToLogin
        // · 작업 장비(EQP_ID)가 있음 · Lot 을 담은 캐리어가 Normal · Lot 이 LOAD 면 그 캐리어가 InBuffer 에 있어야 한다(Load Location Fit —
        // LOAD 가 아니면 늘 Y). 캐리어의 MES_STAT_TYP · LOC_NM 은 Lot 행에 같이 온다(2026-10-28 사용자 지시).
        private static readonly ActionRule[] JobPrepCancelRules =
        {
            ActionRule.NotValue(DecisionEntityName, EquipmentLotPresenter.DecisionRule.EquipmentInList, EquipmentLotPresenter.DecisionRule.EquipmentInList.Missing),
            ActionRule.NotValue(DecisionEntityName, EquipmentLotPresenter.DecisionRule.LotInList, EquipmentLotPresenter.DecisionRule.LotInList.Missing),
            ActionRule.Value(JobEquipmentEntityName, EquipmentLotPresenter.DecisionRule.JobEqpMode, EquipmentLotPresenter.DecisionRule.JobEqpMode.OnLineLocal),
            ActionRule.NotEmpty(LotEntityName, Schemas.LotPriority.LotId),
            ActionRule.Value(LotEntityName, Schemas.LotPriority.MesProcStatCd,
                    Schemas.LotPriority.MesProcStatCd.Sele, Schemas.LotPriority.MesProcStatCd.Resv, Schemas.LotPriority.MesProcStatCd.Load),
            ActionRule.Value(LotEntityName, Schemas.LotPriority.LotStatTyp, Schemas.LotPriority.LotStatTyp.Released),
            ActionRule.Value(LotEntityName, Schemas.LotPriority.LotHoldStatCd, Schemas.LotPriority.LotHoldStatCd.NotOnHold),
            ActionRule.Value(LotEntityName, Schemas.LotPriority.LotProcStatCd, Schemas.LotPriority.LotProcStatCd.WaitingToLogin),
            ActionRule.NotEmpty(LotEntityName, Schemas.LotPriority.EqpId),
            ActionRule.Value(LotEntityName, Schemas.LotPriority.MesStatTyp, Schemas.LotPriority.MesStatTyp.Normal),
            ActionRule.Value(DecisionEntityName, EquipmentLotPresenter.DecisionRule.LoadLocationFit, EquipmentLotPresenter.DecisionRule.LoadLocationFit.Fit)
        };

        // JobStart Cancel — 작업 장비 OnLineLocal · Lot 선택 · Lot 이 PROC · Released · NotOnHold · LoggedIn · 실행 중(JobStart)(2026-10-28 사용자 지시).
        private static readonly ActionRule[] JobStartCancelRules =
        {
            ActionRule.NotValue(DecisionEntityName, EquipmentLotPresenter.DecisionRule.EquipmentInList, EquipmentLotPresenter.DecisionRule.EquipmentInList.Missing),
            ActionRule.NotValue(DecisionEntityName, EquipmentLotPresenter.DecisionRule.LotInList, EquipmentLotPresenter.DecisionRule.LotInList.Missing),
            ActionRule.Value(JobEquipmentEntityName, EquipmentLotPresenter.DecisionRule.JobEqpMode, EquipmentLotPresenter.DecisionRule.JobEqpMode.OnLineLocal),
            ActionRule.NotEmpty(LotEntityName, Schemas.LotPriority.LotId),
            ActionRule.Value(LotEntityName, Schemas.LotPriority.MesProcStatCd, Schemas.LotPriority.MesProcStatCd.Proc),
            ActionRule.Value(LotEntityName, Schemas.LotPriority.LotStatTyp, Schemas.LotPriority.LotStatTyp.Released),
            ActionRule.Value(LotEntityName, Schemas.LotPriority.LotHoldStatCd, Schemas.LotPriority.LotHoldStatCd.NotOnHold),
            ActionRule.Value(LotEntityName, Schemas.LotPriority.LotProcStatCd, Schemas.LotPriority.LotProcStatCd.LoggedIn),
            ActionRule.Value(LotEntityName, Schemas.LotPriority.LastEventCd, Schemas.LotPriority.LastEventCd.JobStart)
        };

        // Skip Oper — 작업 장비가 OnLineLocal · Lot 선택 · 실행 중이 아닌 Lot.
        private static readonly ActionRule[] SkipOperRules =
        {
            ActionRule.NotValue(DecisionEntityName, EquipmentLotPresenter.DecisionRule.EquipmentInList, EquipmentLotPresenter.DecisionRule.EquipmentInList.Missing),
            ActionRule.NotValue(DecisionEntityName, EquipmentLotPresenter.DecisionRule.LotInList, EquipmentLotPresenter.DecisionRule.LotInList.Missing),
            ActionRule.Value(JobEquipmentEntityName, EquipmentLotPresenter.DecisionRule.JobEqpMode, EquipmentLotPresenter.DecisionRule.JobEqpMode.OnLineLocal),
            ActionRule.NotEmpty(LotEntityName, Schemas.LotPriority.LotId),
            ActionRule.NotValue(LotEntityName, Schemas.LotPriority.LastEventCd, Schemas.LotPriority.LastEventCd.JobStart)
        };

        private static readonly ActionRule[] BackToPrevOperRules = JobEndRules;

        internal static string JobActionReason(string actionKey, JobDecision decision)
        {
            if (decision == null || !EquipmentLotPresenter.IsJobKey(actionKey))
            {
                return NoDecision;
            }

            if (actionKey == EquipmentLotPresenter.ActionJobPrep && decision.MaterialsPending)
            {
                return "The decision is still loading.";
            }

            return ActionRule.Check(RulesFor(actionKey), EquipmentLotPresenter.RuleRow(decision));
        }

        private static ActionRule[] RulesFor(string actionKey)
        {
            switch (actionKey)
            {
                case EquipmentLotPresenter.ActionJobPrep:
                    return JobPrepRules;
                case EquipmentLotPresenter.ActionJobStart:
                    return JobStartRules;
                case EquipmentLotPresenter.ActionJobEnd:
                    return JobEndRules;
                case EquipmentLotPresenter.ActionJobPrepCancel:
                    return JobPrepCancelRules;
                case EquipmentLotPresenter.ActionJobStartCancel:
                    return JobStartCancelRules;
                case EquipmentLotPresenter.ActionSkipOper:
                    return SkipOperRules;
                case EquipmentLotPresenter.ActionBackToPrevOper:
                    return BackToPrevOperRules;
                default:
                    return null;
            }
        }
    }
}
