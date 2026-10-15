using System.Data;

using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Samples.Services;

namespace Modern.Lab.Samples
{
    public partial class DurableManagementForm
    {
        // 액션마다 열리는 조건 — 위에서부터 보고, 처음 막은 줄의 문구가 버튼·메뉴 툴팁에 뜬다.
        // 공통 조건은 CommonRules, 액션별 조건은 해당 목록에 추가한다. 생성 조건은 CreateReason이다.
        // 조건은 필드 조건(ActionRule.Value · NotValue · Empty)으로 적는다 — 꺼진 이유가 "Durable · 캡션: 지금 값 → needs 필요한 값"으로
        // 자동으로 나오고, 규칙 안내 창(꺼진 버튼·메뉴 항목 클릭)에 필드 · 요구 값 · 지금 값이 표로 나온다. 응답에 컬럼이 없으면 막는다(2026-10-13).
        // 다른 필드끼리의 OR 가 필요한 조건만 (DataRow durable) 판정 메서드로 적는다 — 그 줄은 안내 창에 문장으로 나온다.
        // 판정은 이 파일에서 끝난다 — 본체는 DurableActionReason 결과를 버튼·메뉴에 적용하고, 실행 직전에 다시 부른다.
        // 목록 로딩 중이면 모든 액션이 닫힌다(listReady).

        private const string DurableEntityName = "Durable";

        private static readonly ActionRule[] CommonRules = new ActionRule[0];

        // RfId — 회사 조건(2026-09-28): 새로 만든(Created) Idle 지속재에만 태그를 붙인다.
        private static readonly ActionRule[] RfIdRules =
        {
            ActionRule.Value(DurableEntityName, Schemas.Durable.MesStatTyp, Schemas.Durable.MesStatTyp.Created),
            ActionRule.Value(DurableEntityName, Schemas.Durable.MesProcStatCd, Schemas.Durable.MesProcStatCd.Idle)
        };

        // Exchange·Edit 의 Source 공통 — 회사 조건(2026-09-28): Normal · Idle · Full 또는 Partial · InUse.
        // Full 또는 Partial 이 "비어 있지 않다"(WF_LOAD_STAT_CD ≠ Free)를 겸한다.
        // Target 조건(Exchange: Free·Available, Edit: Free·Available 또는 Partial·InUse)은 후보 목록이 거른다 —
        // DurableManagementPresenter.Targets 와 CarrierEditPresenter.BuildTargetCandidates.
        // 안내 창은 이 목록을 Exchange · Edit 안에 합쳐 보이고 따로 보이지 않는다.
        private static readonly ActionRule[] SourceRules =
        {
            ActionRule.Value(DurableEntityName, Schemas.Durable.MesStatTyp, Schemas.Durable.MesStatTyp.Normal),
            ActionRule.Value(DurableEntityName, Schemas.Durable.MesProcStatCd, Schemas.Durable.MesProcStatCd.Idle),
            ActionRule.Value(DurableEntityName, Schemas.Durable.WfLoadStatCd,
                    Schemas.Durable.WfLoadStatCd.Full, Schemas.Durable.WfLoadStatCd.Partial),
            ActionRule.Value(DurableEntityName, Schemas.Durable.DurableStatCd, Schemas.Durable.DurableStatCd.InUse)
        };

        // Exchange 는 FOUP 전체를 옮긴다.
        private static readonly ActionRule[] ExchangeRules = Join(
            new ActionRule[]
            {
                ActionRule.Value(DurableEntityName, Schemas.Durable.DurableTyp, Schemas.Durable.DurableTyp.Foup)
            },
            SourceRules);

        // Edit 은 우리 공장 지속재에만 연다 — FR_FAB_ID 는 존재 규칙이라 공백을 뺀 내용이 없어야 한다(2026-09-06).
        private static readonly ActionRule[] EditRules = Join(
            new ActionRule[]
            {
                ActionRule.Empty(DurableEntityName, Schemas.Durable.FrFabId)
            },
            SourceRules);

        private static readonly ActionRule[] TransportRules = new ActionRule[0];

        // Hold 는 Normal(NotOnHold) 지속재에, Release 는 Hold 지속재에 연다(사용자 2026-10-15).
        private static readonly ActionRule[] HoldRules =
        {
            ActionRule.Value(DurableEntityName, Schemas.Durable.MesStatTyp, Schemas.Durable.MesStatTyp.Normal)
        };

        private static readonly ActionRule[] ReleaseRules =
        {
            ActionRule.Value(DurableEntityName, Schemas.Durable.MesStatTyp, Schemas.Durable.MesStatTyp.Hold)
        };

        // Adjust 는 Normal(NotOnHold) 지속재에만 연다(사용자 2026-10-15).
        private static readonly ActionRule[] AdjustRules =
        {
            ActionRule.Value(DurableEntityName, Schemas.Durable.MesStatTyp, Schemas.Durable.MesStatTyp.Normal)
        };

        // Scrap — 회사 조건(2026-10-15): Idle · Free(빈 지속재) · Hold 또는 Normal.
        private static readonly ActionRule[] ScrapRules =
        {
            ActionRule.Value(DurableEntityName, Schemas.Durable.MesProcStatCd, Schemas.Durable.MesProcStatCd.Idle),
            ActionRule.Value(DurableEntityName, Schemas.Durable.WfLoadStatCd, Schemas.Durable.WfLoadStatCd.Free),
            ActionRule.Value(DurableEntityName, Schemas.Durable.MesStatTyp,
                    Schemas.Durable.MesStatTyp.Hold, Schemas.Durable.MesStatTyp.Normal)
        };

        private const string NoDurableSelected = "Select a durable first.";

        private const string ListLoading = "The list is still loading.";

        // 선택 없이 생성할 때도 평가한다. 공통 행 조건과 분리하고 저장 직전에도 같은 함수를 쓴다.
        private static string CreateReason()
        {
            return null;
        }

        internal static string DurableActionReason(string actionKey, DataRow durable, bool listReady)
        {
            if (!listReady)
            {
                return ListLoading;
            }

            if (!DurableManagementPresenter.IsActionKey(actionKey))
            {
                return "This action is not supported.";
            }

            if (actionKey == DurableManagementPresenter.ActionCreate)
            {
                return CreateReason();
            }

            string reason = ActionRule.Check(CommonRules, durable, NoDurableSelected);
            return reason ?? ActionRule.Check(RulesFor(actionKey), durable, NoDurableSelected);
        }

        private static ActionRule[] RulesFor(string actionKey)
        {
            switch (actionKey)
            {
                case DurableManagementPresenter.ActionRfId:
                    return RfIdRules;

                case DurableManagementPresenter.ActionExchange:
                    return ExchangeRules;

                case DurableManagementPresenter.ActionEdit:
                    return EditRules;

                case DurableManagementPresenter.ActionTransport:
                    return TransportRules;
                case DurableManagementPresenter.ActionHold:
                    return HoldRules;
                case DurableManagementPresenter.ActionRelease:
                    return ReleaseRules;
                case DurableManagementPresenter.ActionAdjust:
                    return AdjustRules;
                case DurableManagementPresenter.ActionScrap:
                    return ScrapRules;
            }

            return null;
        }

        private static ActionRule[] Join(ActionRule[] first, ActionRule[] second)
        {
            ActionRule[] joined = new ActionRule[first.Length + second.Length];
            first.CopyTo(joined, 0);
            second.CopyTo(joined, first.Length);
            return joined;
        }
    }
}
