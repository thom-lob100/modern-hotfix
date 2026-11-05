using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>
    /// 장비가 정하는 값 목록 — 타입 이름은 칸 이름 그대로다(<c>COMM_STAT_TYP</c> → <see cref="CommStatTyp"/>).
    /// 이 파일에는 값만 둔다. 칸 이름·캡션은 칸 사전(<c>TableEntity.EquipmentColumns.cs</c>)과 엔티티 줄에 있다(2026-11-05).
    /// </summary>
    public static class EquipmentFields
    {
        public sealed class AutoYn : Field
        {
            public readonly FieldValue Y = Value("Y");
            public readonly FieldValue N = Value("N");
        }

        public sealed class CommStatTyp : Field
        {
            public readonly FieldValue OnLineRemote = Value("OnLineRemote");
            public readonly FieldValue OnLineLocal = Value("OnLineLocal");
            public readonly FieldValue OffLine = Value("OffLine");
        }

        /// <summary>장비 세부 종류(DET_EQP_TYP).</summary>
        public sealed class DetEqpTyp : Field
        {
            public readonly FieldValue Process = Value("Process");
            public readonly FieldValue Sorter = Value("Sorter");
            public readonly FieldValue Cleaner = Value("Cleaner");
            public readonly FieldValue Metrology = Value("Metrology");
            public readonly FieldValue Test = Value("Test");
        }

        /// <summary>장비 종류(EQP_TYP).</summary>
        public sealed class EqpTyp : Field
        {
            public readonly FieldValue ProductionMachine = Value("ProductionMachine");
        }

        public sealed class MesEqpStatCd : Field
        {
            public readonly FieldValue Run = Value("Run");
            public readonly FieldValue Idle = Value("Idle");
            public readonly FieldValue Up = Value("Up");
            public readonly FieldValue Down = Value("Down");
        }

        /// <summary>장비의 MES 상태(MES_STAT_TYP) — Durable 의 같은 칸은 <see cref="DurableFields.MesStatTyp"/>(Normal 등)다.</summary>
        public sealed class MesStatTyp : Field
        {
            public readonly FieldValue Up = Value("Up");
            public readonly FieldValue Down = Value("Down");
        }
    }
}
