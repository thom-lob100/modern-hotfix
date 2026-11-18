using System;
using System.Collections.Generic;
using System.Data;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Samples.Contracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class CarrierEditForm
    {
        // 회사 조건은 CommonReason(공통), MoveReason(이동), ExchangeReason(전량 교환), AlignReason(FOUP 안 자리 정렬)에 추가한다.
        // 조건과 차단 문구를 if 한 덩이로 붙인다. 빈 문자열이면 허용, 첫 사유가 버튼 툴팁에 나온다.
        // sourceCarrier/targetCarrier는 선택한 Carrier 원본 행이다. 회사 컬럼은 이 행에서 읽는다.
        // 키·맵·미리보기 준비 확인은 본체에서 먼저 수행한다.

        // 공통 업무 조건. 지금은 추가 제한이 없다. 회사 조건을 넣으면 Move·Exchange에 함께 적용된다.
        private static string CommonReason(
                CarrierEditEntryMode mode, DataRow sourceCarrier, DataRow targetCarrier,
                string durableTyp, DataTable source, DataTable target, DataTable staged)
        {
            return string.Empty;
        }

        // 일반 Move 전용 업무 조건. Exchange에는 적용되지 않는다.
        private static string MoveReason(
                CarrierEditEntryMode mode, DataRow sourceCarrier, DataRow targetCarrier,
                string durableTyp, DataTable source, DataTable target, DataTable staged)
        {
            return MoveLotReason(source, staged);
        }

        // Lot 하나는 여러 Durable 에 나뉠 수 없다(2026-11-19 사용자) — 스테이징한 Lot 은 Source 에 있는 그 Lot 의 내용물 전부여야 한다.
        // 깨진 단위가 든 Lot 은 깨진 단위를 옮길 수 없으므로 이 Durable 밖으로 나가지 못한다(같은 날 사용자 — 폐기 · Deassign 만 된다).
        // FOUP · TRAY 모두 본다. Lot Id 가 빈 행은 세지 않는다. 같은 Durable 안에서 자리만 바꾸는 Align 에는 적용하지 않는다.
        private static string MoveLotReason(DataTable source, DataTable staged)
        {
            if (source == null || staged == null)
            {
                return string.Empty;
            }

            foreach (string lotId in DistinctLotIds(staged))
            {
                int held = 0;
                bool broken = false;

                foreach (DataRow row in source.Rows)
                {
                    if (!string.Equals(Schemas.DurableWafer.LotId.Text(row), lotId, StringComparison.Ordinal)
                            || Schemas.DurableWafer.WfId.Text(row).Length == 0)
                    {
                        continue;
                    }

                    held++;
                    broken = broken || IsBrokenSubProdTyp(
                            TableHelper.CellText(row, CarrierEditPresenter.OriginalSubProdTypColumn));
                }

                if (broken)
                {
                    return "Lot " + lotId + " holds broken wafers. It cannot leave this carrier.";
                }

                int moving = 0;

                foreach (DataRow row in staged.Rows)
                {
                    if (string.Equals(Schemas.DurableWafer.LotId.Text(row), lotId, StringComparison.Ordinal)
                            && Schemas.DurableWafer.WfId.Text(row).Length > 0)
                    {
                        moving++;
                    }
                }

                if (moving < held)
                {
                    return "Lot " + lotId + ": " + moving.ToString("N0") + " of " + held.ToString("N0")
                            + " staged. A lot moves as a whole.";
                }
            }

            return string.Empty;
        }

        // Exchange 전용 업무 조건. 일반 Move에는 적용되지 않는다.
        private static string ExchangeReason(
                CarrierEditEntryMode mode, DataRow sourceCarrier, DataRow targetCarrier,
                string durableTyp, DataTable source, DataTable target, DataTable staged)
        {
            string reason = ExchangeBrokenReason(source);

            if (reason.Length > 0)
            {
                return reason;
            }

            return ExchangeLotReason(staged);
        }

        // Align 전용 업무 조건. 같은 Durable 안에서 자리만 바꾼다(FOUP · TRAY). 지금은 추가 제한이 없다.
        private static string AlignReason(
                CarrierEditEntryMode mode, DataRow sourceCarrier, DataRow targetCarrier,
                string durableTyp, DataTable source, DataTable target, DataTable staged)
        {
            return string.Empty;
        }

        // 깨진 단위(2026-11-18 사용자) — FOUP 안에 정상 Wafer 와 섞인 BrokenWafer · BrokenChip 은 장비로 옮길 수 없어 이 화면에서 옮기지 않는다.
        // 손으로는 옮길 수 있다고 들었으나 우선 막는다(같은 날 사용자 "3번으로 우선" — 손으로 옮긴 기록 방법은 docs/OPEN.md).
        // 맵에서 회색 칸으로 제자리에 남고 고르거나 끌 수 없으며, Move All · Align 은 이 단위를 빼고 옮긴다.
        // 판정은 맵 응답의 원래 SUB_PROD_TYP 으로 한다. 회사 값이 다르면 CommonFields.SubProdTyp 의 두 값을 고친다.
        internal static bool IsBrokenSubProdTyp(string subProdTyp)
        {
            string value = (subProdTyp ?? string.Empty).Trim();

            return string.Equals(value, Schemas.DurableWafer.SubProdTyp.BrokenWafer.Value, StringComparison.Ordinal)
                    || string.Equals(value, Schemas.DurableWafer.SubProdTyp.BrokenChip.Value, StringComparison.Ordinal);
        }

        // Exchange 는 Durable 의 내용물 전부를 옮기므로 Broken 이 하나라도 있으면 닫는다 — 정상 Wafer 는 Edit(Move)으로 옮긴다.
        private static string ExchangeBrokenReason(DataTable source)
        {
            if (source == null)
            {
                return string.Empty;
            }

            foreach (DataRow row in source.Rows)
            {
                if (IsBrokenSubProdTyp(TableHelper.CellText(row, CarrierEditPresenter.OriginalSubProdTypColumn)))
                {
                    return "Broken wafers cannot move. Use Edit to move the other wafers.";
                }
            }

            return string.Empty;
        }

        // Exchange 전문은 LOT_ID 하나를 싣는다 — 스테이징한 Wafer 의 Lot 이 없거나 둘 이상이면 버튼을 닫는다.
        // Exchange 는 Lot 하나일 때만 한다(사용자 확정 2026-10-03). 여러 Lot 이 섞인 이동은 Edit(Move)로 하고, Move 전문은 LOT_ID 를 싣지 않는다.
        private static string ExchangeLotReason(DataTable staged)
        {
            List<string> lots = DistinctLotIds(staged);

            if (lots.Count == 1)
            {
                return string.Empty;
            }

            return lots.Count == 0
                    ? "The wafers to exchange have no lot."
                    : "The wafers to exchange belong to " + lots.Count.ToString("N0") + " lots. Exchange sends one lot.";
        }

        // Target 콤보 후보 조건(2026-09-29 사용자 확정) — FR_FAB_ID 가 비어 있거나 현재 공장(CurrentFacId)과 같은 Durable 만 후보로 남긴다.
        // 기존 후보 조건(Normal·Idle·빈 자리)은 Presenter 가 먼저 본다.
        internal static bool IsTargetOfCurrentFab(DataRow target, string currentFacId)
        {
            string fab = Schemas.Durable.FrFabId.Text(target);

            return fab.Length == 0
                    || string.Equals(fab, (currentFacId ?? string.Empty).Trim(), StringComparison.Ordinal);
        }

    }
}
