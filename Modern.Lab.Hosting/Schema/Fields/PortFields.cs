using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>
    /// 포트가 정하는 값 목록 — 타입 이름은 칸 이름 그대로다(<c>PORT_TYP</c> → <see cref="PortTyp"/>).
    /// 이 파일에는 값만 둔다. 칸 이름·캡션은 칸 사전(<c>TableEntity.PortColumns.cs</c>)과 엔티티 줄에 있다(2026-11-05).
    /// </summary>
    public static class PortFields
    {
        /// <summary>포트 상태(PORT_STAT_NM) — 회사 값은 대문자 UP · DOWN 이다(2026-11-04 사용자 확인). 장비의 MES_STAT_TYP(Up · Down)과 다르다.</summary>
        public sealed class PortStatNm : Field
        {
            public readonly FieldValue Up = Value("UP");
            public readonly FieldValue Down = Value("DOWN");
        }

        public sealed class PortTyp : Field
        {
            public readonly FieldValue Input = Value("Input");
            public readonly FieldValue Output = Value("Output");
            public readonly FieldValue InputOutput = Value("InputOutput");
        }

        /// <summary>포트 자원 상태(RESRC_STAT_TYP).</summary>
        public sealed class ResrcStatTyp : Field
        {
            public readonly FieldValue InService = Value("InService");
            public readonly FieldValue OutOfService = Value("OutOfService");
        }

        public sealed class TransferStatCd : Field
        {
            public readonly FieldValue ReadyToLoad = Value("ReadyToLoad");
            public readonly FieldValue ReadyToUnload = Value("ReadyToUnload");
            public readonly FieldValue Processing = Value("Processing");
            public readonly FieldValue ReservedToLoad = Value("ReservedToLoad");
            public readonly FieldValue ReservedToUnload = Value("ReservedToUnload");
        }

        /// <summary>가상 포트 여부(VIRT_PORT_YN).</summary>
        public sealed class VirtPortYn : Field
        {
            public readonly FieldValue Y = Value("Y");
            public readonly FieldValue N = Value("N");
        }
    }
}
