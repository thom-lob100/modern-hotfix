using System.Data;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Samples.Services;

namespace Modern.Lab.Samples
{
    public partial class TransportManagementForm
    {
        // 액션마다 열리는 조건 — 위에서부터 보고, 처음 막은 줄의 문구가 버튼 · 메뉴 툴팁에 뜬다. 한 줄에 하나, 회사 조건이 바뀌면 여기만 고친다.
        // 조건은 반송 목록의 고른 행 하나를 읽는다 — 응답에 컬럼이 없으면 막는다. 꺼진 버튼 · 메뉴 항목을 누르면 규칙 안내 창에
        // 필드 · 요구 값 · 지금 값이 표로 나온다. 판정은 이 파일에서 끝난다 — 본체는 TransportActionReason 결과를 버튼 · 메뉴에 적용하고 실행 직전에 다시 부른다.
        // 목록 조회가 실패한 뒤에는 모든 액션이 닫힌다(listReady — 조회 시작 때 내려가 성공 응답에만 올라간다).
        // 조건은 Durable Management 의 Transport 묶음과 같다(2026-11-15) — 반송 표의 칸은 그 이름(MOVE_STAT_CD · LAST_EVENT_DESC)으로 읽고,
        // 반송 표에 없는 Durable · 장비 · 포트 칸(EQP_ID · PORT_NM · TRANS_YN · 상태 칸)은 목록 응답에 같이 와야 한다.

        private const string TransportEntityName = "Transport";
        private const string DurableEntityName = "Durable";
        private const string EquipmentEntityName = "Equipment";
        private const string PortEntityName = "Port";

        private const string NoTransportSelected = "Select a transport first.";

        private const string ListLoading = "The list is still loading.";

        // Manual In — 스토커에 있고(LOC_NM = InStocker) 위치 장비가 있으며(EQP_ID 있음) 폐기 아니고 반송이 걸려 있지 않은(TRANS_YN = N) 지속재를
        // 장비 포트에 수동 투입한다. 장비 · 포트 조건은 Manual In 창이 조회해 거른다(TransportManualInDialogForm.Rules.cs).
        private static readonly ActionRule[] ManualInRules =
        {
            ActionRule.Value(DurableEntityName, Schemas.Transport.LocNm, Schemas.Transport.LocNm.InStocker),
            ActionRule.NotEmpty(DurableEntityName, Schemas.Durable.EqpId),
            ActionRule.NotValue(DurableEntityName, Schemas.Transport.DurableStatCd, Schemas.Transport.DurableStatCd.Scrapped),
            ActionRule.Value(DurableEntityName, Schemas.Durable.TransYn, Schemas.Durable.TransYn.N)
        };

        // Manual In Cancel — 적재 요청(LAST_EVENT_DESC = EISMHS_LOAD_REQ)이 아직 끝나지 않은(MOVE_STAT_CD Request · Requested · Scheduled · Moving) 반송만 취소한다.
        private static readonly ActionRule[] ManualInCancelRules =
        {
            ActionRule.Value(TransportEntityName, Schemas.Transport.LastEventDesc, Schemas.Transport.LastEventDesc.EisMhsLoadReq),
            ActionRule.Value(TransportEntityName, Schemas.Transport.MoveStatCd,
                    Schemas.Transport.MoveStatCd.Request, Schemas.Transport.MoveStatCd.Requested,
                    Schemas.Transport.MoveStatCd.Scheduled, Schemas.Transport.MoveStatCd.Moving)
        };

        // Manual Out — 베이가 아닌 곳(LOC_NM ≠ InBay)에서 장비 포트에 실려 있고(EQP_ID · PORT_NM 있음) 폐기 아니며 반송이 걸려 있지 않은(TRANS_YN = N)
        // 지속재를 그 포트에서 수동 반출한다. 장비 조건 MCS_ITG_YN(Y) · COMM_STAT_TYP(OnLineRemote) · EQP_MES_STAT_TYP(Up)과
        // 포트 조건 ACSS_TYP(Auto) · PORT_STAT_NM(UP) · VIRT_PORT_YN(N)은 목록 행에 같이 오는 값으로 본다. 장비 · 포트는 고르지 않고 행의 EQP_ID · PORT_NM 을 그대로 보낸다 —
        // 출발 · 목적지(START_GOAL_VAL · END_GOAL_VAL)가 아니라 지금 실려 있는 자리다.
        private static readonly ActionRule[] ManualOutRules =
        {
            ActionRule.NotValue(DurableEntityName, Schemas.Transport.LocNm, Schemas.Transport.LocNm.InBay),
            ActionRule.NotValue(DurableEntityName, Schemas.Transport.DurableStatCd, Schemas.Transport.DurableStatCd.Scrapped),
            ActionRule.NotEmpty(DurableEntityName, Schemas.Durable.EqpId),
            ActionRule.NotEmpty(DurableEntityName, Schemas.Durable.PortNm),
            ActionRule.Value(DurableEntityName, Schemas.Durable.TransYn, Schemas.Durable.TransYn.N),
            ActionRule.Value(EquipmentEntityName, Schemas.Durable.McsItgYn, Schemas.EquipmentExt.McsItgYn.Y),
            ActionRule.Value(EquipmentEntityName, Schemas.Durable.CommStatTyp, Schemas.EquipmentExt.CommStatTyp.OnLineRemote),
            ActionRule.Value(EquipmentEntityName, Schemas.Durable.EqpMesStatTyp, Schemas.EquipmentExt.MesStatTyp.Up),
            ActionRule.Value(PortEntityName, Schemas.Durable.AcssTyp, Schemas.PortDet.AcssTyp.Auto),
            ActionRule.Value(PortEntityName, Schemas.Durable.PortStatNm, Schemas.PortDet.PortStatNm.Up),
            ActionRule.Value(PortEntityName, Schemas.Durable.VirtPortYn, Schemas.PortDet.VirtPortYn.N)
        };

        // Manual Out Cancel — 반출 요청(LAST_EVENT_DESC = EISMHS_UNLOAD_REQ)이 아직 끝나지 않은 반송만 취소한다(이동 상태는 Manual In Cancel 과 같다).
        private static readonly ActionRule[] ManualOutCancelRules =
        {
            ActionRule.Value(TransportEntityName, Schemas.Transport.LastEventDesc, Schemas.Transport.LastEventDesc.EisMhsUnloadReq),
            ActionRule.Value(TransportEntityName, Schemas.Transport.MoveStatCd,
                    Schemas.Transport.MoveStatCd.Request, Schemas.Transport.MoveStatCd.Requested,
                    Schemas.Transport.MoveStatCd.Scheduled, Schemas.Transport.MoveStatCd.Moving)
        };

        // Change Destination — 이동 상태(MOVE_STAT_CD)가 Request · Requested · Scheduled · Moving · Changed 이고 Durable 이 폐기(Scrapped)되지 않은 반송만
        // 목적지를 바꾼다. Durable 의 Change Destination 창이 목록을 거르던 조건이다 — 이 화면은 목록을 거르지 않고 버튼을 닫는다(2026-11-15).
        // 새 목적지의 장비 · 포트 조건은 창이 조회해 거른다(TransportChangeDestDialogForm.Rules.cs).
        private static readonly ActionRule[] ChangeDestinationRules =
        {
            ActionRule.Value(TransportEntityName, Schemas.Transport.MoveStatCd,
                    Schemas.Transport.MoveStatCd.Request, Schemas.Transport.MoveStatCd.Requested, Schemas.Transport.MoveStatCd.Scheduled,
                    Schemas.Transport.MoveStatCd.Moving, Schemas.Transport.MoveStatCd.Changed),
            ActionRule.NotValue(TransportEntityName, Schemas.Transport.DurableStatCd, Schemas.Transport.DurableStatCd.Scrapped)
        };

        internal static string TransportActionReason(string actionKey, DataRow transport, bool listReady)
        {
            if (!listReady)
            {
                return ListLoading;
            }

            if (!TransportManagementPresenter.IsActionKey(actionKey))
            {
                return "This action is not supported.";
            }

            return ActionRule.Check(RulesFor(actionKey), transport, NoTransportSelected);
        }

        private static ActionRule[] RulesFor(string actionKey)
        {
            switch (actionKey)
            {
                case TransportManagementPresenter.ActionManualIn:
                    return ManualInRules;

                case TransportManagementPresenter.ActionManualInCancel:
                    return ManualInCancelRules;

                case TransportManagementPresenter.ActionManualOut:
                    return ManualOutRules;

                case TransportManagementPresenter.ActionManualOutCancel:
                    return ManualOutCancelRules;

                default:
                    return ChangeDestinationRules;
            }
        }
    }
}
