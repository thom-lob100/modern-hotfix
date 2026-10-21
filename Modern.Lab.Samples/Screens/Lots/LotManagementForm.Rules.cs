using System;
using System.Data;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting;
using Modern.Lab.Samples.Services;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class LotManagementForm
    {
        // 액션마다 열리는 조건 — 위에서부터 보고, 처음 막은 줄의 문구가 버튼·메뉴 툴팁에 뜬다.
        // 공통 조건은 CommonRules, 액션별 조건은 해당 목록에 추가한다. 생성 창 진입은 CreateWaferReason이다.
        // 조건은 필드 조건(ActionRule.Value · NotValue · Empty)으로 적는다 — 꺼진 이유가 "Lot · 캡션: 지금 값 → needs 필요한 값"으로
        // 자동으로 나오고, 규칙 안내 창(꺼진 버튼·메뉴 항목 클릭)에 필드 · 요구 값 · 지금 값이 표로 나온다. 응답에 컬럼이 없으면 막는다
        // (2026-10-01 실행 조건 · 2026-10-13 전 액션).
        // 다른 필드끼리의 OR 가 필요한 조건만 (DataRow lot) 판정 메서드로 적는다 — 그 줄은 안내 창에 문장으로 나온다.

        private const string LotEntityName = "Lot";

        private static readonly ActionRule[] CommonRules = new ActionRule[0];

        // Create Chip · Create Lamella — 진행 중(PROC)이 아니라는 조건은 WAIT 요구에 들어 있다.
        internal static readonly ActionRule[] CreateChipRules =
        {
            ActionRule.Value(LotEntityName, Schemas.Lot.SubProdTyp, Schemas.Lot.SubProdTyp.Wafer),
            ActionRule.Value(LotEntityName, Schemas.Lot.LotStatTyp, Schemas.Lot.LotStatTyp.Released),
            ActionRule.Value(LotEntityName, Schemas.Lot.MesProcStatCd, Schemas.Lot.MesProcStatCd.Wait),
            ActionRule.NotValue(LotEntityName, Schemas.Lot.LotHoldStatCd, Schemas.Lot.LotHoldStatCd.OnHold)
        };

        internal static readonly ActionRule[] CreateLamellaRules =
        {
            ActionRule.Value(LotEntityName, Schemas.Lot.SubProdTyp, Schemas.Lot.SubProdTyp.Wafer, Schemas.Lot.SubProdTyp.Chip),
            ActionRule.Value(LotEntityName, Schemas.Lot.LotStatTyp, Schemas.Lot.LotStatTyp.Released),
            ActionRule.Value(LotEntityName, Schemas.Lot.MesProcStatCd, Schemas.Lot.MesProcStatCd.Wait),
            ActionRule.NotValue(LotEntityName, Schemas.Lot.LotHoldStatCd, Schemas.Lot.LotHoldStatCd.OnHold)
        };

        internal static readonly ActionRule[] HoldRules =
        {
            ActionRule.NotValue(LotEntityName, Schemas.Lot.MesProcStatCd, Schemas.Lot.MesProcStatCd.Proc),
            ActionRule.NotValue(LotEntityName, Schemas.Lot.LotStatTyp, Schemas.Lot.LotStatTyp.Scrapped),
            ActionRule.NotValue(LotEntityName, Schemas.Lot.LotHoldStatCd, Schemas.Lot.LotHoldStatCd.OnHold)
        };

        internal static readonly ActionRule[] NotOnHoldRules =
        {
            ActionRule.NotValue(LotEntityName, Schemas.Lot.MesProcStatCd, Schemas.Lot.MesProcStatCd.Proc),
            ActionRule.Value(LotEntityName, Schemas.Lot.LotHoldStatCd, Schemas.Lot.LotHoldStatCd.OnHold)
        };

        internal static readonly ActionRule[] ChangeSpecRules =
        {
            ActionRule.Value(LotEntityName, Schemas.Lot.LotProcStatCd, Schemas.Lot.LotProcStatCd.LoggedOut),
            ActionRule.Value(LotEntityName, Schemas.Lot.MesProcStatCd, Schemas.Lot.MesProcStatCd.Wait),
            ActionRule.Value(LotEntityName, Schemas.Lot.LotHoldStatCd, Schemas.Lot.LotHoldStatCd.NotOnHold),
            ActionRule.Value(LotEntityName, Schemas.Lot.LotStatTyp, Schemas.Lot.LotStatTyp.Released),
            ActionRule.Empty(LotEntityName, Schemas.Lot.EqpId)
        };

        internal static readonly ActionRule[] ScrapRules =
        {
            ActionRule.Value(LotEntityName, Schemas.Lot.LotStatTyp, Schemas.Lot.LotStatTyp.Released),
            ActionRule.Value(LotEntityName, Schemas.Lot.MesProcStatCd,
                    Schemas.Lot.MesProcStatCd.Wait, Schemas.Lot.MesProcStatCd.Merg, Schemas.Lot.MesProcStatCd.Scrp),
            ActionRule.Value(LotEntityName, Schemas.Lot.LotHoldStatCd, Schemas.Lot.LotHoldStatCd.NotOnHold),
            ActionRule.Value(LotEntityName, Schemas.Lot.LotProcStatCd, Schemas.Lot.LotProcStatCd.LoggedOut),
            ActionRule.NotValue(LotEntityName, Schemas.Lot.LocNm, Schemas.Lot.LocNm.InStocker)
        };

        // ChangeSpec Cancel · Scrap Cancel — 마지막 처리(LAST_EVENT_CD)가 그 동작일 때만 연다(2026-10-22). 되돌릴 수 있는지는 서버가 판정한다.
        internal static readonly ActionRule[] ChangeSpecCancelRules =
        {
            ActionRule.Value(LotEntityName, Schemas.Lot.LastEventCd, Schemas.Lot.LastEventCd.ChangeSpec)
        };

        internal static readonly ActionRule[] ScrapCancelRules =
        {
            ActionRule.Value(LotEntityName, Schemas.Lot.LastEventCd, Schemas.Lot.LastEventCd.Scrap)
        };

        // Scrap 창에 넘기는 단위 — 화면의 단위 목록은 그대로 두고, 응답 WF_STAT 가 SCRAP 인 행만 뺀다.
        // 응답에 WF_STAT 컬럼이 없으면 전부 넘긴다. 컬럼 이름과 값은 회사 응답에 맞춰 고친다.
        private static DataTable ScrapCandidates(DataTable wafers)
        {
            if (wafers == null || !wafers.Columns.Contains("WF_STAT"))
            {
                return wafers;
            }
            return new DataView(wafers, "WF_STAT <> 'SCRAP'", null, DataViewRowState.CurrentRows).ToTable();
        }

        internal const string NoLotSelected = "Select a lot first.";

        internal static ActionRule[] RulesFor(string actionKey)
        {
            switch (actionKey)
            {
                case LotManagementPresenter.ActionCreateChip:
                    return CreateChipRules;

                case LotManagementPresenter.ActionCreateLamella:
                    return CreateLamellaRules;

                case LotManagementPresenter.ActionHold:
                    return HoldRules;

                case LotManagementPresenter.ActionNotOnHold:
                    return NotOnHoldRules;

                case LotManagementPresenter.ActionChangeSpec:
                    return ChangeSpecRules;

                case LotManagementPresenter.ActionScrap:
                    return ScrapRules;

                case LotManagementPresenter.ActionChangeSpecCancel:
                    return ChangeSpecCancelRules;

                case LotManagementPresenter.ActionScrapCancel:
                    return ScrapCancelRules;
            }

            return null;
        }

        // 생성 창을 여는 조건. 선택 Lot을 보지 않으며, 창 안의 저장 검사는 다이얼로그가 맡는다.
        private static string CreateWaferReason()
        {
            return null;
        }

        private const string ListLoading = "The list is still loading.";

        // 판정은 이 파일에서 끝난다 — 본체는 결과를 버튼·메뉴에 적용하고 실행 직전에 다시 부른다.
        // CreateWafer 만 선택·목록 로딩과 무관하게 열린다 — 나머지는 로딩이 끝나고 고른 Lot 이 있어야 한다.
        internal static string LotActionReason(string actionKey, DataRow lot, bool listReady)
        {
            if (actionKey == LotManagementPresenter.ActionCreateWafer)
            {
                return CreateWaferReason();
            }

            if (!listReady)
            {
                return ListLoading;
            }

            if (lot == null || lot.RowState == DataRowState.Deleted || lot.RowState == DataRowState.Detached
                    || Schemas.Lot.LotId.Text(lot).Length == 0)
            {
                return NoLotSelected;
            }

            if (!LotManagementPresenter.IsActionKey(actionKey))
            {
                return NoLotSelected;
            }

            string reason = ActionRule.Check(CommonRules, lot, NoLotSelected);
            return reason ?? ActionRule.Check(RulesFor(actionKey), lot, NoLotSelected);
        }
    }
}
