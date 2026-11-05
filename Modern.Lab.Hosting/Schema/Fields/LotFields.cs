using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>
    /// Lot 이 정하는 값 목록 — 타입 이름은 칸 이름 그대로다(<c>LOT_HOLD_STAT_CD</c> → <see cref="LotHoldStatCd"/>).
    /// 이 파일에는 값만 둔다. 칸 이름·캡션은 칸 사전(<c>TableEntity.LotColumns.cs</c>)과 엔티티 줄에 있다(2026-11-05).
    /// </summary>
    public static class LotFields
    {
        public sealed class LotHoldStatCd : Field
        {
            public readonly FieldValue OnHold = Value("OnHold");
            public readonly FieldValue NotOnHold = Value("NotOnHold");
        }

        /// <summary>Lot 의 장비 투입 상태 — LoggedOut 이면 장비에서 빠져 있고, WaitingToLogin 은 준비(JobPrep)만 된 것,
        /// LoggedIn 은 장비에 들어가 실행 중(JobStart)이다(2026-10-28 — JobPrep Cancel · JobStart Cancel 조건).</summary>
        public sealed class LotProcStatCd : Field
        {
            public readonly FieldValue LoggedOut = Value("LoggedOut");
            public readonly FieldValue WaitingToLogin = Value("WaitingToLogin");
            public readonly FieldValue LoggedIn = Value("LoggedIn");
        }

        public sealed class LotStatTyp : Field
        {
            public readonly FieldValue Created = Value("Created");
            public readonly FieldValue Released = Value("Released");
            public readonly FieldValue Scrapped = Value("Scrapped");
            public readonly FieldValue Emptied = Value("Emptied");
        }

        /// <summary>Lot 의 MES 진행 상태(MES_PROC_STAT_CD) — Durable 의 같은 칸은 <see cref="DurableFields.MesProcStatCd"/>(Idle 등)다.</summary>
        public sealed class MesProcStatCd : Field
        {
            public readonly FieldValue Wait = Value("WAIT");
            public readonly FieldValue Proc = Value("PROC");
            public readonly FieldValue Sele = Value("SELE");
            public readonly FieldValue Resv = Value("RESV");
            public readonly FieldValue Load = Value("LOAD");
            public readonly FieldValue Merg = Value("MERG");
            public readonly FieldValue Scrp = Value("SCRP");
            public readonly FieldValue JobEnd = Value("JobEnd");
        }
    }
}
