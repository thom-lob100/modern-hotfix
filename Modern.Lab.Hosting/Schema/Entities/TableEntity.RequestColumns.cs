using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public abstract partial class TableEntity
    {
        /// <summary>의뢰서 칸 사전 — 회사 칸 이름 · 기본 캡션이 다르면 여기 한 줄만 고친다. 값 목록은 <see cref="RequestFields"/>.</summary>
        protected static class RequestColumns
        {
            public static Field ReqSerialNo()
            {
                return Field("REQ_SERIAL_NO").Caption("Serial No").TopLabel("Request");
            }
        }
    }
}
