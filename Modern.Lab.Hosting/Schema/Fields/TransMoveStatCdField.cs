using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>반송 이동 상태(TRANS_MOVE_STAT_CD) — Changed · Moving · NoMove · Request · Requested · Scheduled(2026-11-04 사용자). Manual In · Out Cancel 은 Request · Requested · Scheduled · Moving 에서 연다.</summary>
    public sealed class TransMoveStatCdField : Field
    {
        public readonly FieldValue Changed = Value("Changed");
        public readonly FieldValue Moving = Value("Moving");
        public readonly FieldValue NoMove = Value("NoMove");
        public readonly FieldValue Request = Value("Request");
        public readonly FieldValue Requested = Value("Requested");
        public readonly FieldValue Scheduled = Value("Scheduled");
    }
}
