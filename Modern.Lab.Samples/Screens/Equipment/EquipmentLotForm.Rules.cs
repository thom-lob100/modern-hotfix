using System.Data;

using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Samples.Services;

namespace Modern.Lab.Samples
{
    public partial class EquipmentLotForm
    {
        // Job 일곱 액션의 조건 — 액션마다 ActionRule 목록이고 위에서부터 보며, 처음 막은 줄의 문구가 버튼·메뉴 툴팁에 뜬다.
        // 공통 조건은 CommonRules, 액션별 조건은 해당 목록에 추가한다(2026-10-15 사용자 지시 — 전에는 if 문 메서드라 규칙 안내 창에 안 나왔다).
        // 대조하는 행은 EquipmentLotPresenter.RuleRow(decision) 가 만드는 「결정 행」 하나다 — 결정 Lot 행의 LOT_ID · LAST_EVENT_CD · PRIORITY,
        // 작업 장비 행의 EQP_ID · COMM_STAT_TYP(작업이 걸린 Lot 은 그 작업의 장비, 아니면 결정 장비), 결정 In/Out 포트 이름, 목표 Durable(GOAL_CARRIER_ID),
        // 포트 짝(PORTS_FIT = Y, Decision 엔티티)을 한 행에 모은다. 그래서 장비 · Lot · 준비 재료 조건이 모두 값 규칙으로 들어가 안내 창에
        // 필드 · 요구 값 · 지금 값으로 보이고(문장 규칙 없음 — 2026-10-16 사용자 지시), 안내 창 머리글은 LOT_ID 다.
        // Cancel 은 잘못 눌러 되돌리는 것(JobPrep Cancel = 준비 전으로, JobStart Cancel = JobPrep 으로), Back to Prev Oper 는 실제 JobStart 를 했지만
        // 다시 JobStart 하러 JobPrep 으로 되돌아가는 것, Cancel Oper 는 이 공정을 취소해 다음 공정으로 보내는 것이다(2026-09-30 사용자 정의).
        // JobPrep Cancel 은 JobStart 와, JobStart Cancel 은 JobEnd 와 같은 조건으로 열린다(2026-10-07 사용자 결정).
        // Back to Prev Oper 는 되돌릴 상태일 때만, Cancel Oper 는 실행 중(JobStart)이 아닐 때만 열린다 — 둘 다 OnLineLocal 장비에서만(2026-10-07 사용자 결정).
        // Lot · 장비 · 포트 · Durable 의 자격(상태 · 종류 · 빈 포트 · 빈 Durable)은 서버가 우선순위에서 빼서 내려주고, 화면은 다시 보지 않는다
        // (2026-10-07 사용자 결정 — 기준표 2026-09-13 「우선순위가 곧 대상이다」). 화면이 보는 것은 선택 여부 · 순위 여부 · 작업 단계 · 통신 모드 · 포트 짝이다.
        // 우선순위(후보)는 일곱 액션 전부에 건다(2026-10-07 사용자 결정) — 순위 없는 Lot 은 어느 Job 버튼도 열리지 않는다.
        // 판정은 이 파일에서 끝난다 — 본체는 JobActionReason 결과를 버튼·메뉴에 적용하고 Job 확인 뒤 실행 직전에 다시 부른다.
        // 장비 모드·포트 메뉴 조건은 EquipmentPortManagementForm.Rules.cs를 공유한다.

        internal const string NoDecision = "Pick an equipment, ports, a lot and a goal durable first.";

        private const string LotEntityName = "Lot";
        private const string EquipmentEntityName = "Equipment";
        private const string DecisionEntityName = "Decision";

        private static readonly ActionRule[] CommonRules =
        {
            ActionRule.Value(EquipmentEntityName, Schemas.EquipmentPriority.CommStatTyp,
                    Schemas.EquipmentPriority.CommStatTyp.OnLineRemote, Schemas.EquipmentPriority.CommStatTyp.OnLineLocal),
            ActionRule.NotEmpty(LotEntityName, Schemas.LotPriority.LotId),
            ActionRule.NotEmpty(LotEntityName, Schemas.LotPriority.Priority)
        };

        // JobPrep — Lot 자격 네 줄(LoggedOut · WAIT · NotOnHold · Released) + 포트 짝 + 목표 Durable.
        // 자격 네 줄은 순위 없는 Lot 을 더블클릭해 결정했을 때를 위한 것이다 — 서버가 순위를 준 Lot 은 이미 통과한다
        // (2026-10-16 사용자 지시, 2026-10-07 에 뺐던 것을 되살림). 진행 중인 작업(JobPrep · Reserved · JobStart)이 걸린 Lot 은
        // WAIT · LoggedOut 이 아니므로 작업 단계 줄을 따로 두지 않는다(2026-10-16 사용자 확인).
        private static readonly ActionRule[] JobPrepRules =
        {
            ActionRule.Value(LotEntityName, Schemas.LotPriority.LotProcStatCd, Schemas.LotPriority.LotProcStatCd.LoggedOut),
            ActionRule.Value(LotEntityName, Schemas.LotPriority.MesProcStatCd, Schemas.LotPriority.MesProcStatCd.Wait),
            ActionRule.Value(LotEntityName, Schemas.LotPriority.LotHoldStatCd, Schemas.LotPriority.LotHoldStatCd.NotOnHold),
            ActionRule.Value(LotEntityName, Schemas.LotPriority.LotStatTyp, Schemas.LotPriority.LotStatTyp.Released),
            ActionRule.Value(DecisionEntityName, EquipmentLotPresenter.DecisionRule.PortsFit, EquipmentLotPresenter.DecisionRule.PortsFit.Fit),
            ActionRule.NotEmpty(DecisionEntityName, Schemas.LotPriority.GoalDurableId)
        };

        // JobStart — OnLineLocal 장비에서 준비가 끝난(Reserved) Lot.
        private static readonly ActionRule[] JobStartRules =
        {
            ActionRule.Value(EquipmentEntityName, Schemas.EquipmentPriority.CommStatTyp, Schemas.EquipmentPriority.CommStatTyp.OnLineLocal),
            ActionRule.Value(LotEntityName, Schemas.LotPriority.LastEventCd, Schemas.LotPriority.LastEventCd.Reserved)
        };

        // JobEnd — OnLineLocal 장비에서 실행 중(JobStart)인 Lot.
        private static readonly ActionRule[] JobEndRules =
        {
            ActionRule.Value(EquipmentEntityName, Schemas.EquipmentPriority.CommStatTyp, Schemas.EquipmentPriority.CommStatTyp.OnLineLocal),
            ActionRule.Value(LotEntityName, Schemas.LotPriority.LastEventCd, Schemas.LotPriority.LastEventCd.JobStart)
        };

        private static readonly ActionRule[] JobPrepCancelRules = JobStartRules;

        private static readonly ActionRule[] JobStartCancelRules = JobEndRules;

        // Cancel Oper — OnLineLocal 장비, 실행 중이 아닌 Lot.
        private static readonly ActionRule[] CancelOperRules =
        {
            ActionRule.Value(EquipmentEntityName, Schemas.EquipmentPriority.CommStatTyp, Schemas.EquipmentPriority.CommStatTyp.OnLineLocal),
            ActionRule.NotValue(LotEntityName, Schemas.LotPriority.LastEventCd, Schemas.LotPriority.LastEventCd.JobStart)
        };

        // Back to Prev Oper — OnLineLocal 장비, 실행 중(JobStart)인 Lot.
        private static readonly ActionRule[] BackToPrevOperRules =
        {
            ActionRule.Value(EquipmentEntityName, Schemas.EquipmentPriority.CommStatTyp, Schemas.EquipmentPriority.CommStatTyp.OnLineLocal),
            ActionRule.Value(LotEntityName, Schemas.LotPriority.LastEventCd, Schemas.LotPriority.LastEventCd.JobStart)
        };

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

            DataRow row = EquipmentLotPresenter.RuleRow(decision);
            string reason = ActionRule.Check(CommonRules, row);
            return reason ?? ActionRule.Check(RulesFor(actionKey), row);
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
                case EquipmentLotPresenter.ActionCancelOper:
                    return CancelOperRules;
                case EquipmentLotPresenter.ActionBackToPrevOper:
                    return BackToPrevOperRules;
                default:
                    return null;
            }
        }
    }
}
