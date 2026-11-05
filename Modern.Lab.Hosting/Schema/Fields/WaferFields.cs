using System.Data;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>
    /// Wafer 가 정하는 값 목록 — 타입 이름은 칸 이름 그대로다(<c>WF_LOAD_STAT_CD</c> → <see cref="WfLoadStatCd"/>).
    /// 이 파일에는 값만 둔다. 칸 이름·캡션은 칸 사전(<c>TableEntity.WaferColumns.cs</c>)과 엔티티 줄에 있다(2026-11-05).
    /// </summary>
    public static class WaferFields
    {
        public sealed class WfLoadStatCd : Field
        {
            public readonly FieldValue Free = Value("Free", "Free");
            public readonly FieldValue Partial = Value("Partial", "Partial");
            public readonly FieldValue Full = Value("Full", "Full");

            // ── 값 예제 — 다른 상태값 필드도 같은 형식으로 적는다 ───────────────────────
            // 서버 값과 표시가 같다     Value("Free")
            // 서버 값과 표시가 다르다   Value("F", "Free")
            // 읽을 때                   Free.Value → "F" · Free.Caption → "Free"

            /// <summary>빈 지속재 판정 — 적재 상태가 <see cref="Free"/>다. 화면 규칙·후보 필터가 같은 판정을 공유한다.</summary>
            public bool IsFree(DataRow durable)
            {
                return this.Is(durable, this.Free);
            }
        }
    }
}
