using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>
    /// 여러 업무가 함께 쓰거나 주인을 정할 근거가 없는 값 목록 — 타입 이름은 칸 이름 그대로다(<c>SUB_PROD_TYP</c> → <see cref="SubProdTyp"/>).
    /// 예외는 <see cref="Yn"/> 하나로, 뜻이 칸 이름에 있는 Y · N 플래그 여럿이 함께 쓴다(2026-11-04).
    /// 이 파일에는 값만 둔다. 칸 이름·캡션은 칸 사전(<c>TableEntity.CommonColumns.cs</c>)과 엔티티 줄에 있다(2026-11-05).
    /// </summary>
    public static class CommonFields
    {
        /// <summary>포트 접근 방식(ACSS_TYP) — Auto · Manual(2026-11-04 사용자: Manual Out 은 Manual + Output 포트를 막는다. 앞서 적었던 Normal 은 잘못 말한 값이라 지웠다).</summary>
        public sealed class AcssTyp : Field
        {
            public readonly FieldValue Auto = Value("Auto");
            public readonly FieldValue Manual = Value("Manual");
        }

        /// <summary>공장 종류(FAC_TYP) — 2026-11-10 사용자가 준 세 값.</summary>
        public sealed class FacTyp : Field
        {
            public readonly FieldValue ProductionFactory = Value("ProductionFactory");
            public readonly FieldValue StorageFactory = Value("StorageFactory");
            public readonly FieldValue AssembleFactory = Value("AssembleFactory");
        }

        public sealed class JudgeRslt : Field
        {
            public readonly FieldValue Succ = Value("SUCC");
            public readonly FieldValue Fail = Value("FAIL");
        }

        public sealed class LastEventCd : Field
        {
            public readonly FieldValue JobPrep = Value("JobPrep");
            public readonly FieldValue Reserved = Value("Reserved");
            public readonly FieldValue JobStart = Value("JobStart");
            public readonly FieldValue JobEnd = Value("JobEnd");
            public readonly FieldValue ChangeSpec = Value("ChangeSpec");
            public readonly FieldValue Scrap = Value("Scrap");
        }

        public sealed class LocNm : Field
        {
            public readonly FieldValue InStocker = Value("InStocker");
            public readonly FieldValue InMachine = Value("InMachine");
            public readonly FieldValue InBay = Value("InBay");
            // 장비 버퍼 — Lot 이 LOAD 인 준비를 되돌릴 수 있는 캐리어 위치(2026-10-28, JobPrep Cancel 조건).
            public readonly FieldValue InBuffer = Value("InBuffer");
        }

        /// <summary>MCS 연동 여부(MCS_ITG_YN).</summary>
        public sealed class McsItgYn : Field
        {
            public readonly FieldValue Y = Value("Y");
            public readonly FieldValue N = Value("N");
        }

        public sealed class SubProdTyp : Field
        {
            public readonly FieldValue Wafer = Value("Wafer");
            public readonly FieldValue Chip = Value("Chip");
            public readonly FieldValue Lamella = Value("Lamella");
        }

        /// <summary>Y · N 두 값만 갖는 칸 — 뜻이 칸 이름에 있는 플래그(REWORK_YN · RETURN_YN · SWAP_YN · SCRAP_DUE_YN)에 쓴다(2026-11-04).
        /// 뜻이 하나로 정해진 칸(<see cref="EquipmentFields.AutoYn"/> · <see cref="DurableFields.DurableMultiLotYn"/>)은 자기 타입을 그대로 둔다.</summary>
        public sealed class Yn : Field
        {
            public readonly FieldValue Y = Value("Y");
            public readonly FieldValue N = Value("N");
        }
    }
}
