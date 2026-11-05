using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public abstract partial class TableEntity
    {
        /// <summary>Wafer 칸 사전 — 회사 칸 이름 · 기본 캡션이 다르면 여기 한 줄만 고친다. 값 목록은 <see cref="WaferFields"/>.</summary>
        protected static class WaferColumns
        {
            public static Field WfId()
            {
                return Field("WF_ID").Caption("Wafer Id");
            }

            public static WaferFields.WfLoadStatCd WfLoadStatCd()
            {
                return Field<WaferFields.WfLoadStatCd>("WF_LOAD_STAT_CD").Caption("Status").Badge();
            }
        }
    }
}
