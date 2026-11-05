using System;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>
    /// 의뢰서가 정하는 값 목록 — 타입 이름은 칸 이름 그대로다(<c>STATE_CODE</c> → <see cref="StateCode"/>).
    /// 이 파일에는 값만 둔다. 칸 이름·캡션은 칸 사전(<c>TableEntity.RequestColumns.cs</c>)과 엔티티 줄에 있다(2026-11-05).
    /// </summary>
    public static class RequestFields
    {
        /// <summary>의뢰서 진행 상태 — 스텝 인디케이터의 칸 순서와 배지 문구가 <see cref="Flow"/>에서 나온다. 숫자 <c>STATUS</c>는 화면이 해석하지 않는다.</summary>
        public sealed class StateCode : Field
        {
            public readonly FieldValue Request = Value("REQUEST");
            public readonly FieldValue Delivering = Value("DELIVERING");
            public readonly FieldValue AcceptWait = Value("ACCEPT_WAIT");
            public readonly FieldValue SampleWait = Value("SAMPLE_WAIT");
            public readonly FieldValue Analizing = Value("ANALIZING");
            public readonly FieldValue CompleteWait = Value("COMPLETE_WAIT");
            public readonly FieldValue Completed = Value("COMPLETED");

            /// <summary>접수취소 — 흐름 밖의 종료 상태다.</summary>
            public readonly FieldValue Draft0 = Value("DRAFT0");

            /// <summary>접수취소의 숫자 짝.</summary>
            public const string Draft0Status = "0";

            /// <summary><see cref="Flow"/>와 같은 순서의 숫자 짝 — 두 배열의 길이는 같아야 한다.</summary>
            private static readonly string[] flowStatus = { "1", "5", "17", "49", "177", "8241", "24625" };

            /// <summary>진행 순서 — 이 배열이 스텝 칸의 개수와 순서를 정한다.</summary>
            public FieldValue[] Flow
            {
                get
                {
                    return new FieldValue[]
                    {
                        this.Request, this.Delivering, this.AcceptWait, this.SampleWait, this.Analizing, this.CompleteWait, this.Completed
                    };
                }
            }

            /// <summary><see cref="Flow"/>의 코드 배열 — 문자열을 받는 기존 API에 넘길 때.</summary>
            public string[] FlowCodes()
            {
                FieldValue[] flow = this.Flow;
                string[] codes = new string[flow.Length];

                for (int index = 0; index < codes.Length; index++)
                {
                    codes[index] = flow[index].Value;
                }

                return codes;
            }

            /// <summary>회사가 알려 준 <c>STATE_CODE</c> ↔ <c>STATUS</c> 짝. 쓰기 전문은 둘을 같은 상태로 함께 싣는다.
            /// 숫자의 뜻은 화면이 풀지 않는다 — 짝을 옮겨 적을 뿐이고 판정은 서버가 한다. 모르는 상태면 빈 문자열이다.</summary>
            public string StatusOf(string stateCode)
            {
                int index = Array.IndexOf(this.FlowCodes(), stateCode);

                if (index >= 0)
                {
                    return flowStatus[index];
                }

                return string.Equals(stateCode, this.Draft0.Value, StringComparison.Ordinal) ? Draft0Status : string.Empty;
            }
        }
    }
}
