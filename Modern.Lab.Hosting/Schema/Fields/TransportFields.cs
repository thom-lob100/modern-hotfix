using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>
    /// 반송이 정하는 값 목록 — 타입 이름은 칸 이름 그대로다(<c>TRANS_YN</c> → <see cref="TransYn"/>).
    /// 이 파일에는 값만 둔다. 칸 이름·캡션은 그 칸을 쓰는 엔티티 줄에 있다(2026-11-05).
    /// </summary>
    public static class TransportFields
    {
        /// <summary>반송의 마지막 이벤트 설명(TRANS_LAST_EVENT_DESC) — 화면이 보는 값은 적재 요청(EISMHS_LOAD_REQ)과 반출 요청(EISMHS_UNLOAD_REQ) 둘이다(2026-11-04 사용자). 그 밖의 값은 그대로 보인다.</summary>
        public sealed class TransLastEventDesc : Field
        {
            public readonly FieldValue EisMhsLoadReq = Value("EISMHS_LOAD_REQ");
            public readonly FieldValue EisMhsUnloadReq = Value("EISMHS_UNLOAD_REQ");
        }

        /// <summary>반송 이동 상태(TRANS_MOVE_STAT_CD) — Changed · Moving · NoMove · Request · Requested · Scheduled(2026-11-04 사용자). Manual In · Out Cancel 은 Request · Requested · Scheduled · Moving 에서 연다.</summary>
        public sealed class TransMoveStatCd : Field
        {
            public readonly FieldValue Changed = Value("Changed");
            public readonly FieldValue Moving = Value("Moving");
            public readonly FieldValue NoMove = Value("NoMove");
            public readonly FieldValue Request = Value("Request");
            public readonly FieldValue Requested = Value("Requested");
            public readonly FieldValue Scheduled = Value("Scheduled");
        }

        /// <summary>반송이 걸려 있는지(TRANS_YN) — Y 이면 Manual In · Manual Out 은 닫히고 취소만 된다(2026-11-05 사용자).</summary>
        public sealed class TransYn : Field
        {
            public readonly FieldValue Y = Value("Y");
            public readonly FieldValue N = Value("N");
        }
    }
}
