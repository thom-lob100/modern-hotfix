using System;
using System.Data;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class DurableChangeDestDialogForm
    {
        // Change Destination 의 조건(사용자 2026-11-11 · 2026-11-12) — 받은 목록에서 조건에 맞는 행만 표 · 콤보에 남긴다.
        // 한 줄에 하나, 회사 조건이 바뀌면 여기만 고친다. 응답에 컬럼이 없으면 그 행은 빠진다(값 규칙은 컬럼이 없으면 막는다).
        // 장비는 여기서 거르지 않는다 — 고른 반송의 출발 · 목적 장비(START_GOAL_VAL · END_GOAL_VAL)와 Destination 을 실어 보내 서버가 거른다(사용자 2026-11-15).
        private const string TransportEntityName = "Transport";
        private const string PortEntityName = "Port";

        // 반송 — 이동 상태(MOVE_STAT_CD)가 Request · Requested · Scheduled · Moving · Changed 이고 Durable 이 폐기(Scrapped)되지 않은 반송만 목적지를 바꾼다.
        // 요청 종류(LAST_EVENT_DESC)로는 거르지 않는다(2026-11-12 사용자). FOUP · TRAY 는 Type 콤보 값(DURABLE_TYP)을 실어 서버가 거른다(2026-11-15).
        private static readonly ActionRule[] TransportRules =
        {
            ActionRule.Value(TransportEntityName, Schemas.Transport.MoveStatCd,
                    Schemas.Transport.MoveStatCd.Request, Schemas.Transport.MoveStatCd.Requested, Schemas.Transport.MoveStatCd.Scheduled,
                    Schemas.Transport.MoveStatCd.Moving, Schemas.Transport.MoveStatCd.Changed),
            ActionRule.NotValue(TransportEntityName, Schemas.Transport.DurableStatCd, Schemas.Transport.DurableStatCd.Scrapped)
        };

        // 포트 — 장비 · STB · Stocker 모두 Manual In 창과 같은 포트 조건이다.
        private static readonly ActionRule[] PortRules =
        {
            ActionRule.Value(PortEntityName, Schemas.PortDet.PortTyp, Schemas.PortDet.PortTyp.Input, Schemas.PortDet.PortTyp.InputOutput),
            ActionRule.Value(PortEntityName, Schemas.PortDet.AcssTyp, Schemas.PortDet.AcssTyp.Auto),
            ActionRule.Value(PortEntityName, Schemas.PortDet.ResrcStatTyp, Schemas.PortDet.ResrcStatTyp.InService),
            ActionRule.Value(PortEntityName, Schemas.PortDet.PortStatNm, Schemas.PortDet.PortStatNm.Up),
            ActionRule.Value(PortEntityName, Schemas.PortDet.VirtPortYn, Schemas.PortDet.VirtPortYn.N),
            ActionRule.Value(PortEntityName, Schemas.PortDet.TransferStatCd, Schemas.PortDet.TransferStatCd.ReadyToLoad),
            ActionRule.Empty(PortEntityName, Schemas.PortDet.CurrDurableId)
        };

        // Destination 콤보의 두 번째 항목 — Type 이 FOUP 이면 STB, 그 밖(TRAY)은 Stocker 다.
        private static bool UsesStb(string durableTyp)
        {
            return string.Equals(durableTyp, Schemas.Transport.DurableTyp.Foup.Value, StringComparison.OrdinalIgnoreCase);
        }

        private static DataTable Candidates(DataTable rows, ActionRule[] rules)
        {
            DataTable result = rows.Clone();

            foreach (DataRow row in rows.Rows)
            {
                if (ActionRule.IsOpen(rules, row))
                {
                    result.ImportRow(row);
                }
            }

            return result;
        }
    }
}
