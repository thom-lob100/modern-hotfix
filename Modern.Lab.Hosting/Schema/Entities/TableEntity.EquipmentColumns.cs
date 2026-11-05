using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public abstract partial class TableEntity
    {
        /// <summary>장비 칸 사전 — 회사 칸 이름 · 기본 캡션이 다르면 여기 한 줄만 고친다. 값 목록은 <see cref="EquipmentFields"/>.</summary>
        protected static class EquipmentColumns
        {
            public static EquipmentFields.AutoYn AutoYn()
            {
                return Field<EquipmentFields.AutoYn>("AUTO_YN").Caption("Auto");
            }

            public static EquipmentFields.CommStatTyp CommStatTyp()
            {
                return Field<EquipmentFields.CommStatTyp>("COMM_STAT_TYP").Caption("Mode").Badge();
            }

            public static Field EqpGrpId()
            {
                return Field("EQP_GRP_ID").Caption("Group").TopLabel("Equipment")
                        .HeaderAlignment(GridTextAlignment.Center).TopLabelAlignment(GridTextAlignment.Center);
            }

            public static Field EqpId()
            {
                return Field("EQP_ID").Caption("Eqp Id");
            }

            public static EquipmentFields.EqpTyp EqpTyp()
            {
                return Field<EquipmentFields.EqpTyp>("EQP_TYP").Caption("Eqp Type");
            }

            /// <summary>작업 중(Run) 장비는 상태 배지가 돈다 — 표 · 카드가 이 선언을 기본으로 쓴다(2026-10-26).</summary>
            public static EquipmentFields.MesEqpStatCd MesEqpStatCd()
            {
                EquipmentFields.MesEqpStatCd mesEqpStatCd = Field<EquipmentFields.MesEqpStatCd>("MES_EQP_STAT_CD").Caption("Status").Badge();
                mesEqpStatCd.Spin(mesEqpStatCd.Run);
                return mesEqpStatCd;
            }

            /// <summary>장비의 MES 상태 — Durable 의 같은 칸은 <see cref="DurableColumns.MesStatTyp"/> 다.</summary>
            public static EquipmentFields.MesStatTyp MesStatTyp()
            {
                return Field<EquipmentFields.MesStatTyp>("MES_STAT_TYP").Caption("MES Status");
            }

            /// <summary>트리의 부모 연결 칸 — 트리 컨트롤이 UseEntity 로 키 · 부모 키를 읽는다(2026-10-26).</summary>
            public static Field ParentEqpId()
            {
                return Field("PARENT_EQP_ID").Caption("Parent Eqp");
            }
        }
    }
}
