using System.Data;

using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Data;

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
        // 목록 로딩 중이면 모든 액션이 닫힌다(listReady). contents 는 선택 지속재의 내용물 표(GetDurableSlotList 전부) —
        // Deassign 의 Lot 조건과 Assign 의 빈 자리 계산이 읽고, 받는 중(null)이면 Assign · Deassign 만 닫힌다.

        private const string DurableEntityName = "Durable";
        private const string LotEntityName = "Lot";

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

        // Hold — 회사 조건(2026-10-17): Normal · Idle · 예약(Reserved)되지 않은 지속재. Release — Hold · Idle 지속재.
        private static readonly ActionRule[] HoldRules =
        {
            ActionRule.Value(DurableEntityName, Schemas.Durable.MesStatTyp, Schemas.Durable.MesStatTyp.Normal),
            ActionRule.Value(DurableEntityName, Schemas.Durable.MesProcStatCd, Schemas.Durable.MesProcStatCd.Idle),
            ActionRule.NotValue(DurableEntityName, Schemas.Durable.DurableStatCd, Schemas.Durable.DurableStatCd.Reserved)
        };

        private static readonly ActionRule[] ReleaseRules =
        {
            ActionRule.Value(DurableEntityName, Schemas.Durable.MesStatTyp, Schemas.Durable.MesStatTyp.Hold),
            ActionRule.Value(DurableEntityName, Schemas.Durable.MesProcStatCd, Schemas.Durable.MesProcStatCd.Idle)
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

        // Deassign — 회사 조건(2026-10-28): FOUP 이고 Normal · Idle · 비어 있지 않은(WF_LOAD_STAT_CD ≠ Free) 지속재.
        // 내용물의 Lot 조건(모든 Lot 이 MES_PROC_STAT_CD WAIT 이고, Released·NotOnHold 또는 Scrapped·Emptied — 2026-10-28)은
        // DeassignContentsReason 이 본다 — 내용물 표의 여러 행(Lot 상태 컬럼 LOT_STAT_TYP · LOT_HOLD_STAT_CD · MES_PROC_STAT_CD)에
        // 걸친 판정이라 값 규칙이 아니고, 안내 창에는 나오지 않으며 처음 막은 Lot 이 툴팁 사유로 나온다.
        private static readonly ActionRule[] DeassignRules =
        {
            ActionRule.Value(DurableEntityName, Schemas.Durable.DurableTyp, Schemas.Durable.DurableTyp.Foup),
            ActionRule.Value(DurableEntityName, Schemas.Durable.MesStatTyp, Schemas.Durable.MesStatTyp.Normal),
            ActionRule.Value(DurableEntityName, Schemas.Durable.MesProcStatCd, Schemas.Durable.MesProcStatCd.Idle),
            ActionRule.NotValue(DurableEntityName, Schemas.Durable.WfLoadStatCd, Schemas.Durable.WfLoadStatCd.Free)
        };

        // Assign — 회사 조건(2026-10-28): FOUP 이고 Normal · Idle · 가득 차지 않은(WF_LOAD_STAT_CD ≠ Full) 지속재.
        // Lot 조건(Released · NotOnHold · 실리지 않은 Lot · 자리)은 서버가 AssignDurable 에서 판정한다 — 창은 거르지 않는다(2026-10-28).
        // 빈 자리는 내용물 표에서 세므로(기본 Slot 값) 받는 중(null)이면 Assign 도 닫힌다.
        private static readonly ActionRule[] AssignRules =
        {
            ActionRule.Value(DurableEntityName, Schemas.Durable.DurableTyp, Schemas.Durable.DurableTyp.Foup),
            ActionRule.Value(DurableEntityName, Schemas.Durable.MesStatTyp, Schemas.Durable.MesStatTyp.Normal),
            ActionRule.Value(DurableEntityName, Schemas.Durable.MesProcStatCd, Schemas.Durable.MesProcStatCd.Idle),
            ActionRule.NotValue(DurableEntityName, Schemas.Durable.WfLoadStatCd, Schemas.Durable.WfLoadStatCd.Full)
        };

        private const string NoDurableSelected = "Select a durable first.";

        private const string ListLoading = "The list is still loading.";

        private const string ContentsLoading = "The contents are still loading.";

        private const string NoLotInContents = "The durable has no lot to deassign.";

        // Scrap Cancel — 마지막 처리(LAST_EVENT_CD)가 Scrap 일 때만 연다(2026-10-22, Lot 과 같은 규칙). 되돌릴 수 있는지는 서버가 판정한다.
        private static readonly ActionRule[] ScrapCancelRules =
        {
            ActionRule.Value(DurableEntityName, Schemas.Durable.LastEventCd, Schemas.Durable.LastEventCd.Scrap)
        };

        // 선택 없이 생성할 때도 평가한다. 공통 행 조건과 분리하고 저장 직전에도 같은 함수를 쓴다.
        private static string CreateReason()
        {
            return null;
        }

        internal static string DurableActionReason(string actionKey, DataRow durable, DataTable contents, bool listReady)
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

            string reason = ActionRule.Check(CommonRules, durable, NoDurableSelected)
                    ?? ActionRule.Check(RulesFor(actionKey), durable, NoDurableSelected);

            if (reason == null && actionKey == DurableManagementPresenter.ActionDeassign)
            {
                reason = DeassignContentsReason(contents);
            }

            if (reason == null && actionKey == DurableManagementPresenter.ActionAssign && contents == null)
            {
                reason = ContentsLoading;
            }

            return reason;
        }

        // 내용물의 모든 Lot 이 WAIT 이고 (Released 이면서 NotOnHold) 또는 Scrapped · Emptied 여야 연다. Lot 이 하나도 없으면 막는다.
        private static string DeassignContentsReason(DataTable contents)
        {
            if (contents == null)
            {
                return ContentsLoading;
            }

            bool anyLot = false;

            foreach (DataRow row in contents.Rows)
            {
                string lotId = Schemas.DurableWafer.LotId.Text(row);

                if (lotId.Length == 0)
                {
                    continue;
                }

                anyLot = true;

                if (!Schemas.DurableWafer.MesProcStatCd.Is(row, Schemas.DurableWafer.MesProcStatCd.Wait))
                {
                    return LotReason(lotId, Schemas.DurableWafer.MesProcStatCd, row,
                            Schemas.DurableWafer.MesProcStatCd.Wait.Value);
                }

                if (Schemas.DurableWafer.LotStatTyp.Is(row, Schemas.DurableWafer.LotStatTyp.Released))
                {
                    if (!Schemas.DurableWafer.LotHoldStatCd.Is(row, Schemas.DurableWafer.LotHoldStatCd.NotOnHold))
                    {
                        return LotReason(lotId, Schemas.DurableWafer.LotHoldStatCd, row,
                                Schemas.DurableWafer.LotHoldStatCd.NotOnHold.Value);
                    }

                    continue;
                }

                if (!Schemas.DurableWafer.LotStatTyp.Is(row, Schemas.DurableWafer.LotStatTyp.Scrapped)
                        && !Schemas.DurableWafer.LotStatTyp.Is(row, Schemas.DurableWafer.LotStatTyp.Emptied))
                {
                    return LotReason(lotId, Schemas.DurableWafer.LotStatTyp, row,
                            Schemas.DurableWafer.LotStatTyp.Released.Value + ", "
                            + Schemas.DurableWafer.LotStatTyp.Scrapped.Value + " or "
                            + Schemas.DurableWafer.LotStatTyp.Emptied.Value);
                }
            }

            return anyLot ? null : NoLotInContents;
        }

        // 값 규칙의 사유와 같은 꼴 — "Lot <Id> · 캡션: 지금 값 → needs 요구 값". 응답에 컬럼이 없으면 지금 값은 missing.
        private static string LotReason(string lotId, Field field, DataRow row, string needs)
        {
            string value = row.Table.Columns.Contains(field.Name) ? field.Text(row) : string.Empty;

            return LotEntityName + " " + lotId + " · " + field.CaptionSetting + ": "
                    + (value.Length == 0 ? "missing" : value) + " → needs " + needs;
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
                case DurableManagementPresenter.ActionScrapCancel:
                    return ScrapCancelRules;
                case DurableManagementPresenter.ActionDeassign:
                    return DeassignRules;
                case DurableManagementPresenter.ActionAssign:
                    return AssignRules;
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
