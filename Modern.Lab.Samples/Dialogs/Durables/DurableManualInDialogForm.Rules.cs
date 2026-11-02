using System.Data;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class DurableManualInDialogForm
    {
        // Manual In 의 장비 · 포트 후보 조건(사용자 2026-11-02) — 받은 목록에서 조건에 맞는 행만 표에 남긴다. 한 줄에 하나,
        // 회사 조건이 바뀌면 여기만 고친다. 응답에 컬럼이 없으면 그 행은 빠진다(값 규칙은 컬럼이 없으면 막는다).
        private const string EquipmentEntityName = "Equipment";
        private const string PortEntityName = "Port";

        private static readonly ActionRule[] EquipmentRules =
        {
            ActionRule.Value(EquipmentEntityName, Schemas.EquipmentExt.CommStatTyp, Schemas.EquipmentExt.CommStatTyp.OnLineLocal),
            ActionRule.Value(EquipmentEntityName, Schemas.EquipmentExt.McsItgYn, Schemas.EquipmentExt.McsItgYn.Y),
            ActionRule.Value(EquipmentEntityName, Schemas.EquipmentExt.MesStatTyp, Schemas.EquipmentExt.MesStatTyp.Up),
            ActionRule.Value(EquipmentEntityName, Schemas.EquipmentExt.EqpTyp, Schemas.EquipmentExt.EqpTyp.ProductionMachine)
        };

        private static readonly ActionRule[] PortRules =
        {
            ActionRule.Value(PortEntityName, Schemas.PortDet.PortTyp, Schemas.PortDet.PortTyp.Input, Schemas.PortDet.PortTyp.InputOutput),
            ActionRule.Value(PortEntityName, Schemas.PortDet.AcssTyp, Schemas.PortDet.AcssTyp.Auto),
            ActionRule.Value(PortEntityName, Schemas.PortDet.PortStatNm, Schemas.PortDet.PortStatNm.Up),
            ActionRule.Value(PortEntityName, Schemas.PortDet.ResrcStatTyp, Schemas.PortDet.ResrcStatTyp.InService),
            ActionRule.Value(PortEntityName, Schemas.PortDet.TransferStatCd, Schemas.PortDet.TransferStatCd.ReadyToLoad),
            ActionRule.Empty(PortEntityName, Schemas.PortDet.CurrDurableId),
            ActionRule.Value(PortEntityName, Schemas.PortDet.VirtPortYn, Schemas.PortDet.VirtPortYn.N)
        };

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
