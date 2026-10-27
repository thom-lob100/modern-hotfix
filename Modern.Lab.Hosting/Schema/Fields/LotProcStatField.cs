using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Lot 의 장비 투입 상태 — LoggedOut 이면 장비에서 빠져 있고, WaitingToLogin 은 준비(JobPrep)만 된 것,
    /// LoggedIn 은 장비에 들어가 실행 중(JobStart)이다(2026-10-28 — JobPrep Cancel · JobStart Cancel 조건).</summary>
    public sealed class LotProcStatField : Field
    {
        public readonly FieldValue LoggedOut = Value("LoggedOut");
        public readonly FieldValue WaitingToLogin = Value("WaitingToLogin");
        public readonly FieldValue LoggedIn = Value("LoggedIn");
    }
}
