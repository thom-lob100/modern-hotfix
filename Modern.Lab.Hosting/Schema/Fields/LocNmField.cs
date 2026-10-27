using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class LocNmField : Field
    {
        public readonly FieldValue InStocker = Value("InStocker");
        public readonly FieldValue InMachine = Value("InMachine");
        public readonly FieldValue InBay = Value("InBay");
        // 장비 버퍼 — Lot 이 LOAD 인 준비를 되돌릴 수 있는 캐리어 위치(2026-10-28, JobPrep Cancel 조건).
        public readonly FieldValue InBuffer = Value("InBuffer");
    }
}
