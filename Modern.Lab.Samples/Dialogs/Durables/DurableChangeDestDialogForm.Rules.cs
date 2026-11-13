using System;
using System.Data;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class DurableChangeDestDialogForm
    {
        // Change Destination 의 조건(사용자 2026-11-11 · 2026-11-12) — 받은 목록에서 조건에 맞는 행만 표에 남긴다.
        // 한 줄에 하나, 회사 조건이 바뀌면 여기만 고친다. 응답에 컬럼이 없으면 그 행은 빠진다(값 규칙은 컬럼이 없으면 막는다).
        private const string TransportEntityName = "Transport";
        private const string EquipmentEntityName = "Equipment";
        private const string PortEntityName = "Port";

        // 반송 — 이동 상태(MOVE_STAT_CD)가 Request · Requested · Scheduled · Moving · Changed 이고 Durable 이 폐기(Scrapped)되지 않은 반송만 목적지를 바꾼다.
        // 요청 종류(LAST_EVENT_DESC)로는 거르지 않는다(2026-11-12 사용자).
        private static readonly ActionRule[] TransportRules =
        {
            ActionRule.Value(TransportEntityName, Schemas.Transport.MoveStatCd,
                    Schemas.Transport.MoveStatCd.Request, Schemas.Transport.MoveStatCd.Requested, Schemas.Transport.MoveStatCd.Scheduled,
                    Schemas.Transport.MoveStatCd.Moving, Schemas.Transport.MoveStatCd.Changed),
            ActionRule.NotValue(TransportEntityName, Schemas.Transport.DurableStatCd, Schemas.Transport.DurableStatCd.Scrapped)
        };

        // 새 목적지가 장비일 때 — Manual In 창과 같은 장비 조건(2026-11-09).
        private static readonly ActionRule[] EquipmentRules =
        {
            ActionRule.Value(EquipmentEntityName, Schemas.EquipmentExt.McsItgYn, Schemas.EquipmentExt.McsItgYn.Y),
            ActionRule.Value(EquipmentEntityName, Schemas.EquipmentExt.MesStatTyp, Schemas.EquipmentExt.MesStatTyp.Up),
            ActionRule.Value(EquipmentEntityName, Schemas.EquipmentExt.EqpTyp, Schemas.EquipmentExt.EqpTyp.ProductionMachine),
            ActionRule.Value(EquipmentEntityName, Schemas.EquipmentExt.CommStatTyp, Schemas.EquipmentExt.CommStatTyp.OnLineRemote, Schemas.EquipmentExt.CommStatTyp.OnLineLocal)
        };

        // 새 목적지가 STB · Stocker 일 때 — 같은 장비 표에서 대분류(EQP_MAIN_GRP_ID)로만 가른다(2026-11-12 사용자). FOUP 은 STB, TRAY 는 Stocker 다.
        private static readonly ActionRule[] StbRules =
        {
            ActionRule.Value(EquipmentEntityName, Schemas.EquipmentExt.EqpMainGrpId, Schemas.EquipmentExt.EqpMainGrpId.Stb)
        };

        private static readonly ActionRule[] StockerRules =
        {
            ActionRule.Value(EquipmentEntityName, Schemas.EquipmentExt.EqpMainGrpId, Schemas.EquipmentExt.EqpMainGrpId.Stocker)
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

        // Destination 콤보의 두 번째 항목 — DURABLE_TYP 이 FOUP 이면 STB, 그 밖(TRAY)은 Stocker 다.
        private static bool UsesStb(DataRow transport)
        {
            return transport != null && Schemas.Transport.DurableTyp.Is(transport, Schemas.Transport.DurableTyp.Foup);
        }

        private static ActionRule[] StorageRules(DataRow transport)
        {
            return UsesStb(transport) ? StbRules : StockerRules;
        }

        // 지금 목적지(END_GOAL_VAL)는 후보에서 뺀다 — 장비째 빠지므로 그 장비의 다른 포트로도 바꾸지 않는다
        // (사용자 2026-11-11 "동일 장비/동일 포트 / 동일 스토커은 선택 목록에서 빼야해").
        private static DataTable OtherThanCurrent(DataTable destinations, DataRow transport)
        {
            string current = Schemas.Transport.EndGoalVal.Text(transport);
            DataTable result = destinations.Clone();

            foreach (DataRow row in destinations.Rows)
            {
                if (!string.Equals(Schemas.EquipmentExt.EqpId.Text(row), current, StringComparison.OrdinalIgnoreCase))
                {
                    result.ImportRow(row);
                }
            }

            return result;
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
