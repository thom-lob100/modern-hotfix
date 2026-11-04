using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>반송의 마지막 이벤트 설명(TRANS_LAST_EVENT_DESC) — 화면이 보는 값은 적재 요청(EISMHS_LOAD_REQ)과 반출 요청(EISMHS_UNLOAD_REQ) 둘이다(2026-11-04 사용자). 그 밖의 값은 그대로 보인다.</summary>
    public sealed class TransLastEventDescField : Field
    {
        public readonly FieldValue EisMhsLoadReq = Value("EISMHS_LOAD_REQ");
        public readonly FieldValue EisMhsUnloadReq = Value("EISMHS_UNLOAD_REQ");
    }
}
