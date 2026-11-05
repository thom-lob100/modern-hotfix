using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public abstract partial class TableEntity
    {
        /// <summary>Lab Monitor 칸 사전(홈 이름) — 회사 칸 이름 · 기본 캡션이 다르면 여기 한 줄만 고친다. 값 목록은 <see cref="MonitorFields"/>.</summary>
        protected static class MonitorColumns
        {
            public static MonitorFields.MonStage MonStage()
            {
                return Field<MonitorFields.MonStage>("MON_STAGE").Caption("Stage").Badge();
            }
        }
    }
}
