using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>
    /// Lab Monitor 가 정하는 값 목록 — 타입 이름은 칸 이름 그대로다(<c>MON_STAGE</c> → <see cref="MonStage"/>). 홈 이름이다.
    /// 이 파일에는 값만 둔다. 칸 이름·캡션은 칸 사전(<c>TableEntity.MonitorColumns.cs</c>)과 엔티티 줄에 있다(2026-11-05).
    /// </summary>
    public static class MonitorFields
    {
        /// <summary>Lab Monitor 의 단계(MON_STAGE) — 보드 윗줄의 메인 여덟 칸. 서버가 정해 내려주고 화면은 그대로 묶는다(2026-11-04).
        /// 홈 이름이다 — 회사 칸 이름과 값은 이 선언에서 바꾼다.</summary>
        public sealed class MonStage : Field
        {
            public readonly FieldValue Request = Value("REQUEST");
            public readonly FieldValue Send = Value("SEND");
            public readonly FieldValue Arrival = Value("ARRIVAL");
            public readonly FieldValue Exchange = Value("EXCHANGE");
            public readonly FieldValue Prep = Value("PREP");
            public readonly FieldValue Fib = Value("FIB");
            public readonly FieldValue Tem = Value("TEM");
            public readonly FieldValue Done = Value("DONE");

            /// <summary>보드 순서 — 왼쪽에서 오른쪽.</summary>
            public FieldValue[] Flow
            {
                get
                {
                    return new FieldValue[]
                    {
                        this.Request, this.Send, this.Arrival, this.Exchange, this.Prep, this.Fib, this.Tem, this.Done
                    };
                }
            }
        }

        /// <summary>Lab Monitor 의 랏 상태 배지(MON_STAT_CD) — 작업 중(Job) · 다음 차례(Next) · 대기(Wait). 서버가 정한다(2026-11-04).</summary>
        public sealed class MonStatCd : Field
        {
            public readonly FieldValue Job = Value("Job");
            public readonly FieldValue Next = Value("Next");
            public readonly FieldValue Wait = Value("Wait");
        }
    }
}
