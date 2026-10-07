using System.Data;

using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Samples.Services;

namespace Modern.Lab.Samples
{
    public partial class LotEquipmentForm
    {
        // 공통 조건은 CommonReason, 액션별 조건은 JobPrepReason·JobStartReason·JobEndReason·JobPrepCancelReason·JobStartCancelReason·
        // CancelOperReason·BackToPrevOperReason에 둔다. Cancel 은 잘못 눌러 되돌리는 것(JobPrep Cancel = 준비 전으로, JobStart Cancel = JobPrep 으로),
        // Back to Prev Oper 는 실제 JobStart 를 했지만 다시 JobStart 하러 JobPrep 으로 되돌아가는 것, Cancel Oper 는 이 공정을 취소해 다음 공정으로
        // 보내는 것이다(2026-09-30 사용자 정의). JobPrep Cancel 은 JobStart 와, JobStart Cancel 은 JobEnd 와 같은 조건으로 열린다(2026-10-07 사용자 결정).
        // Back to Prev Oper 는 되돌릴 상태일 때만, Cancel Oper 는 실행 중(JobStart)이 아닐 때만 열린다 — 둘 다 OnLineLocal 장비에서만(2026-10-07 사용자 결정).
        // 조건과 사유를 if 한 덩이로 추가한다. null이면 허용, 첫 사유가 버튼 툴팁에 나온다.
        // decision은 JobPrep용 새 조합(소스 Port·대상 Port·Lot·대상 Durable)이고, equipment는 실제 Job 대상 장비다.
        // JobStart·JobEnd는 선택 Lot의 기존 Job 값(EQP_ID 등)을 쓰므로 equipment를 decision.Equipment로 대체하지 않는다.
        // 지원 액션·장비 선택 확인은 본체에서 수행한다.
        // 장비 모드·포트 메뉴 조건은 EquipmentPortManagementForm.Rules.cs를 공유한다.
        // 판정은 이 파일에서 끝난다 — 본체는 JobActionReason 결과를 버튼·메뉴에 적용하고 Job 확인 뒤 실행 직전에 다시 부른다.
        // 상태·종류는 엔티티 값 필드로 직접 읽는다. 후보 순위(IsCandidate)와 포트 궁합(PortsCompatible)은 여러 행을 보는 계산이라 Presenter에 둔다.

        internal const string NoDecision = "Pick an equipment, ports, a lot and a goal durable first.";

        // Lot · 장비 · 포트 · Durable 의 자격(상태 · 종류 · 빈 포트 · 빈 Durable)은 서버가 우선순위에서 빼서 내려주고, 화면은 다시 보지 않는다
        // (2026-10-07 사용자 결정 — 기준표 2026-09-13 「우선순위가 곧 대상이다」). 화면이 보는 것은 선택 여부 · 순위 여부 · 작업 단계 · 통신 모드 · 포트 짝이다.
        // JobStart 와 JobPrep Cancel 은 LAST_EVENT_CD = Reserved 일 때 열린다(JobPrep 처리 결과). 작업 단계 조건은 ActionRule 로 두어 꺼진 이유가
        // "엔티티 · 캡션: 지금 값 → needs …"로 보인다.
        private const string LotEntityName = "Lot";

        private static readonly ActionRule[] JobStartLotRules =
        {
            ActionRule.Value(LotEntityName, Schemas.LotPriority.LastEventCd, Schemas.LotPriority.LastEventCd.Reserved)
        };

        private static string CommonReason(string actionKey, JobDecision decision, DataRow equipment, DataRow lot)
        {
            CommStatTypField mode = Schemas.EquipmentPriority.CommStatTyp;
            if (!mode.Is(equipment, mode.OnLineRemote) && !mode.Is(equipment, mode.OnLineLocal))
            {
                return "The equipment is offline.";
            }

            // 우선순위(후보)는 일곱 액션 전부에 건다(2026-10-07 사용자 결정) — 순위 없는 Lot 은 어느 Job 버튼도 열리지 않는다.
            if (lot != null && !EquipmentLotPresenter.IsCandidate(lot))
            {
                return "The lot is not a candidate for this equipment.";
            }

            return null;
        }

        private static string JobStartReason(JobDecision decision, DataRow equipment, DataRow lot)
        {
            if (!IsOnLineLocal(equipment))
            {
                return "JobStart needs OnLineLocal.";
            }

            if (!JobDecision.IsLive(lot))
            {
                return "The lot is not prepared.";
            }

            return ActionRule.Check(JobStartLotRules, lot);
        }

        private static string JobEndReason(JobDecision decision, DataRow equipment, DataRow lot)
        {
            if (!IsOnLineLocal(equipment))
            {
                return "JobEnd needs OnLineLocal.";
            }

            if (!IsJobState(lot, Schemas.LotPriority.LastEventCd.JobStart))
            {
                return "The job has not started.";
            }

            return null;
        }

        private static string JobPrepReason(JobDecision decision, DataRow equipment, DataRow lot)
        {
            if (lot == null)
            {
                return "Select a lot first.";
            }

            if (IsJobState(lot, Schemas.LotPriority.LastEventCd.Reserved)
                    || IsJobState(lot, Schemas.LotPriority.LastEventCd.JobStart))
            {
                return "The lot already has a job.";
            }

            if (equipment == null)
            {
                return NoDecision;
            }

            if (!EquipmentLotPresenter.PortsCompatible(decision.Port, decision.GoalPort))
            {
                return "The port and goal port do not fit together.";
            }

            if (decision.GoalDurable == null)
            {
                return "Select a goal durable first.";
            }

            return null;
        }

        private static string JobPrepCancelReason(JobDecision decision, DataRow equipment, DataRow lot)
        {
            return JobStartReason(decision, equipment, lot);
        }

        private static string JobStartCancelReason(JobDecision decision, DataRow equipment, DataRow lot)
        {
            return JobEndReason(decision, equipment, lot);
        }

        private static string CancelOperReason(JobDecision decision, DataRow equipment, DataRow lot)
        {
            if (!IsOnLineLocal(equipment))
            {
                return "Cancel Oper needs OnLineLocal.";
            }

            if (lot == null)
            {
                return "Select a lot first.";
            }

            if (IsJobState(lot, Schemas.LotPriority.LastEventCd.JobStart))
            {
                return "A running job cannot be cancelled. End it first.";
            }

            return null;
        }

        private static string BackToPrevOperReason(JobDecision decision, DataRow equipment, DataRow lot)
        {
            if (!IsOnLineLocal(equipment))
            {
                return "Back to Prev Oper needs OnLineLocal.";
            }

            if (!IsJobState(lot, Schemas.LotPriority.LastEventCd.JobStart))
            {
                return "There is no started job to return from.";
            }

            return null;
        }

        private static bool IsOnLineLocal(DataRow equipment)
        {
            CommStatTypField mode = Schemas.EquipmentPriority.CommStatTyp;
            return mode.Is(equipment, mode.OnLineLocal);
        }

        // 조회에서 빠진 행은 값을 읽지 않는다 — 상태가 없는 것으로 본다.
        private static bool IsJobState(DataRow lot, Modern.Lab.WinForms.Controls.Data.FieldValue state)
        {
            return JobDecision.IsLive(lot) && Schemas.LotPriority.LastEventCd.Is(lot, state);
        }
    }
}
