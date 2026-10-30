using System;
using System.Data;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting.Schema;

using Modern.Lab.Hosting;
using Modern.Lab.Samples.Services;

namespace Modern.Lab.Samples
{
    public partial class EquipmentPortManagementForm
    {
        // 액션마다 열리는 조건 — 위에서부터 보고, 처음 막은 줄의 문구가 버튼·메뉴 툴팁에 뜬다.
        // 공통 조건은 CommonRules, 액션별 조건은 해당 목록에 추가한다.
        // 넘어오는 행은 장비다. 조건은 필드 조건(ActionRule.Value · NotValue · Empty)으로 적는다 — 꺼진 이유가
        // "Equipment · 캡션: 지금 값 → must not be 값"으로 자동으로 나오고, 규칙 안내 창(꺼진 버튼·메뉴 항목 클릭)에
        // 필드 · 요구 값 · 지금 값이 표로 나온다. 응답에 컬럼이 없으면 막는다(2026-10-13).
        // 다른 필드끼리의 OR 가 필요한 조건만 (DataRow equipment) 판정 메서드로 적는다 — 그 줄은 안내 창에 문장으로 나온다.
        // 장비 선택은 EquipmentActionReason이 보장한다.

        private const string EquipmentEntityName = "Equipment";

        private static readonly ActionRule[] CommonRules = new ActionRule[0];

        internal const string NoEquipmentSelected = "Select an equipment first.";

        // Run 장비는 모드·Auto 액션이 전부 닫힌다(2026-08-29). 모드 3종은 이미 그 모드면 닫히고, Auto 는 OnLineRemote 에서만 연다.
        internal static readonly ActionRule[] OnLineRemoteRules =
        {
            ActionRule.NotValue(EquipmentEntityName, Schemas.EquipmentExt.MesEqpStatCd, Schemas.EquipmentExt.MesEqpStatCd.Run),
            ActionRule.NotValue(EquipmentEntityName, Schemas.EquipmentExt.CommStatTyp, Schemas.EquipmentExt.CommStatTyp.OnLineRemote)
        };

        internal static readonly ActionRule[] OnLineLocalRules =
        {
            ActionRule.NotValue(EquipmentEntityName, Schemas.EquipmentExt.MesEqpStatCd, Schemas.EquipmentExt.MesEqpStatCd.Run),
            ActionRule.NotValue(EquipmentEntityName, Schemas.EquipmentExt.CommStatTyp, Schemas.EquipmentExt.CommStatTyp.OnLineLocal)
        };

        internal static readonly ActionRule[] OffLineRules =
        {
            ActionRule.NotValue(EquipmentEntityName, Schemas.EquipmentExt.MesEqpStatCd, Schemas.EquipmentExt.MesEqpStatCd.Run),
            ActionRule.NotValue(EquipmentEntityName, Schemas.EquipmentExt.CommStatTyp, Schemas.EquipmentExt.CommStatTyp.OffLine)
        };

        internal static readonly ActionRule[] AutoRules =
        {
            ActionRule.NotValue(EquipmentEntityName, Schemas.EquipmentExt.MesEqpStatCd, Schemas.EquipmentExt.MesEqpStatCd.Run),
            ActionRule.Value(EquipmentEntityName, Schemas.EquipmentExt.CommStatTyp, Schemas.EquipmentExt.CommStatTyp.OnLineRemote)
        };

        internal static ActionRule[] RulesFor(string actionKey)
        {
            // 모드 키는 엔티티 상태값 코드다.
            CommStatTypField mode = Schemas.EquipmentExt.CommStatTyp;

            if (actionKey == mode.OnLineRemote.Value)
            {
                return OnLineRemoteRules;
            }

            if (actionKey == mode.OnLineLocal.Value)
            {
                return OnLineLocalRules;
            }

            if (actionKey == mode.OffLine.Value)
            {
                return OffLineRules;
            }

            if (actionKey == EquipmentPortManagementPresenter.ActionAuto)
            {
                return AutoRules;
            }

            return null;
        }

        private const string ListLoading = "The list is still loading.";

        // 행만 보는 판정 — 목록 로딩 개념이 없는 화면(Equipment / Lots)이 쓴다.
        internal static string EquipmentActionReason(string actionKey, DataRow equipment)
        {
            return EquipmentActionReason(actionKey, equipment, true);
        }

        // 판정은 이 파일에서 끝난다 — 본체는 결과를 버튼·메뉴에 적용하고 확인창 뒤 실행 직전에 다시 부른다.
        // 목록 조회가 실패한 뒤에는 모든 액션이 같은 사유로 닫힌다(listReady — 조회 시작 때 내려가 성공 응답에만 올라간다).
        // 조회 중에는 버튼을 다시 그리지 않고(깜빡임, 2026-10-30) 실행 직전 검사와 로딩 덮개만 이 값을 본다.
        internal static string EquipmentActionReason(string actionKey, DataRow equipment, bool listReady)
        {
            if (!listReady)
            {
                return ListLoading;
            }

            ActionRule[] rules = RulesFor(actionKey);

            if (rules == null)
            {
                return NoEquipmentSelected;
            }

            string reason = ActionRule.Check(CommonRules, equipment, NoEquipmentSelected);
            return reason ?? ActionRule.Check(rules, equipment, NoEquipmentSelected);
        }

        internal static bool IsAuto(DataRow equipment)
        {
            return Schemas.EquipmentExt.AutoYn.Is(equipment, Schemas.EquipmentExt.AutoYn.Y);
        }
    }
}
