using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public abstract partial class TableEntity
    {
        /// <summary>Durable 칸 사전 — 회사 칸 이름 · 기본 캡션이 다르면 여기 한 줄만 고친다. 값 목록은 <see cref="DurableFields"/>.</summary>
        protected static class DurableColumns
        {
            public static Field Capa()
            {
                return Field("CAPA").Caption("Capa");
            }

            public static Field DurableId()
            {
                return Field("DURABLE_ID").Caption("Durable Id");
            }

            public static DurableFields.DurableStatCd DurableStatCd()
            {
                return Field<DurableFields.DurableStatCd>("DURABLE_STAT_CD").Caption("Mode").Badge();
            }

            public static DurableFields.DurableTyp DurableTyp()
            {
                return Field<DurableFields.DurableTyp>("DURABLE_TYP").Caption("Type");
            }

            public static Field FingerId()
            {
                return Field("FINGER_ID").Caption("Finger Id");
            }

            public static Field FromFac()
            {
                return Field("FROM_FAC").Caption("From");
            }

            /// <summary>Durable 의 MES 상태 — 장비의 같은 칸은 <see cref="EquipmentColumns.MesStatTyp"/> 다.</summary>
            public static DurableFields.MesStatTyp MesStatTyp()
            {
                return Field<DurableFields.MesStatTyp>("MES_STAT_TYP").Caption("MES Status");
            }
        }
    }
}
