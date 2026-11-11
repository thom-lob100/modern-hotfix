using System;
using System.Data;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class DurableChangeDestDialogForm
    {
        // Change Destination 의 조건(사용자 2026-11-11) — 받은 목록에서 조건에 맞는 행만 표에 남긴다.
        // 한 줄에 하나, 회사 조건이 바뀌면 여기만 고친다. 응답에 컬럼이 없으면 그 행은 빠진다(값 규칙은 컬럼이 없으면 막는다).
        private const string TransportEntityName = "Transport";
        private const string EquipmentEntityName = "Equipment";
        private const string PortEntityName = "Port";

        // 반송 요청 — 이동 상태(MOVE_STAT_CD)가 Request · Requested · Scheduled 인 Manual In(EISMHS_LOAD_REQ) · Manual Out(EISMHS_UNLOAD_REQ) 요청만 목적지를 바꾼다.
        private static readonly ActionRule[] TransportRules =
        {
            ActionRule.Value(TransportEntityName, Schemas.Transport.MoveStatCd,
                    Schemas.Transport.MoveStatCd.Request, Schemas.Transport.MoveStatCd.Requested, Schemas.Transport.MoveStatCd.Scheduled),
            ActionRule.Value(TransportEntityName, Schemas.Transport.LastEventDesc,
                    Schemas.Transport.LastEventDesc.EisMhsLoadReq, Schemas.Transport.LastEventDesc.EisMhsUnloadReq)
        };

        // Manual In 요청의 새 목적지 장비 · 포트 — Manual In 창과 같은 조건(2026-11-09). 스토커(Manual Out 요청의 새 목적지)는 조건 없이 받은 목록 그대로다.
        private static readonly ActionRule[] EquipmentRules =
        {
            ActionRule.Value(EquipmentEntityName, Schemas.EquipmentExt.McsItgYn, Schemas.EquipmentExt.McsItgYn.Y),
            ActionRule.Value(EquipmentEntityName, Schemas.EquipmentExt.MesStatTyp, Schemas.EquipmentExt.MesStatTyp.Up),
            ActionRule.Value(EquipmentEntityName, Schemas.EquipmentExt.EqpTyp, Schemas.EquipmentExt.EqpTyp.ProductionMachine),
            ActionRule.Value(EquipmentEntityName, Schemas.EquipmentExt.CommStatTyp, Schemas.EquipmentExt.CommStatTyp.OnLineRemote, Schemas.EquipmentExt.CommStatTyp.OnLineLocal)
        };

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

        // 요청 종류 — LAST_EVENT_DESC 가 EISMHS_UNLOAD_REQ 면 Manual Out(새 목적지는 스토커), 그 밖(EISMHS_LOAD_REQ)은 Manual In(새 목적지는 장비 · 포트).
        private static bool IsManualOut(DataRow transport)
        {
            return Schemas.Transport.LastEventDesc.Is(transport, Schemas.Transport.LastEventDesc.EisMhsUnloadReq);
        }

        // 지금 목적지는 후보에서 뺀다 — Manual In 은 목적 장비(TO_EQP_ID)를 장비 표에서 통째로(그 장비의 포트도 함께 빠진다), Manual Out 은 목적 스토커를 스토커 표에서
        // (사용자 2026-11-11 "동일 장비/동일 포트 / 동일 스토커은 선택 목록에서 빼야해").
        private static DataTable OtherThanCurrent(DataTable destinations, DataRow transport)
        {
            string current = Schemas.Transport.ToEqpId.Text(transport);
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
