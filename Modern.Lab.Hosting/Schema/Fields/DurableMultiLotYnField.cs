using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Lot 이 든 캐리어에 Lot 이 둘 이상인지(DURABLE_MULTI_LOT_YN) — Lot 행에 실리는 캐리어 값. Exchange · JobPrep 은 N 일 때만 연다(2026-11-04).</summary>
    public sealed class DurableMultiLotYnField : Field
    {
        public readonly FieldValue Y = Value("Y");
        public readonly FieldValue N = Value("N");
    }
}
