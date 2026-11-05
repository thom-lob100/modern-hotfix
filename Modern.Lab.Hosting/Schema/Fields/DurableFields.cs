using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>
    /// Durable 이 정하는 값 목록 — 타입 이름은 칸 이름 그대로다(<c>DURABLE_STAT_CD</c> → <see cref="DurableStatCd"/>).
    /// 이 파일에는 값만 둔다. 칸 이름·캡션은 칸 사전(<c>TableEntity.DurableColumns.cs</c>)과 엔티티 줄에 있다(2026-11-05).
    /// </summary>
    public static class DurableFields
    {
        /// <summary>Durable 세정 필요 여부(CLEAN_TYP) — N 은 세정이 필요 없다.</summary>
        public sealed class CleanTyp : Field
        {
            public readonly FieldValue N = Value("N");
        }

        /// <summary>TRAY 안의 세부 수납 종류 — STUB 은 Chip, LCC 는 Lamella 를 담는다.</summary>
        public sealed class DetDurableTyp : Field
        {
            public readonly FieldValue Stub = Value("STUB");
            public readonly FieldValue Lcc = Value("LCC");
        }

        /// <summary>Lot 이 든 캐리어에 Lot 이 둘 이상인지(DURABLE_MULTI_LOT_YN) — Lot 행에 실리는 캐리어 값. Exchange · JobPrep 은 N 일 때만 연다(2026-11-04).</summary>
        public sealed class DurableMultiLotYn : Field
        {
            public readonly FieldValue Y = Value("Y");
            public readonly FieldValue N = Value("N");
        }

        public sealed class DurableStatCd : Field
        {
            public readonly FieldValue Available = Value("Available");
            public readonly FieldValue InUse = Value("InUse");
            public readonly FieldValue NotAvailable = Value("NotAvailable");
            public readonly FieldValue Reserved = Value("Reserved");
            public readonly FieldValue Scrapped = Value("Scrapped");
        }

        public sealed class DurableTyp : Field
        {
            public readonly FieldValue Foup = Value("FOUP");
            public readonly FieldValue Tray = Value("TRAY");
        }

        /// <summary>Lamella 를 핑거에 꽂은 위치.</summary>
        public sealed class FingerPosition : Field
        {
            public readonly FieldValue Top = Value("Top");
            public readonly FieldValue Left = Value("Left");
            public readonly FieldValue Right = Value("Right");
        }

        /// <summary>Lab Monitor 의 지속재 위치 종류(LOC_KIND) — STB(FOUP 보관) · STOCKER(트레이 보관) · EQP(장비) · OTHER. 서버가 정한다(2026-11-04).</summary>
        public sealed class LocKind : Field
        {
            public readonly FieldValue Stb = Value("STB");
            public readonly FieldValue Stocker = Value("STOCKER");
            public readonly FieldValue Eqp = Value("EQP");
            public readonly FieldValue Other = Value("OTHER");
        }

        /// <summary>Durable 의 MES 진행 상태(MES_PROC_STAT_CD) — Lot 의 같은 칸은 <see cref="LotFields.MesProcStatCd"/>(WAIT 등)다.</summary>
        public sealed class MesProcStatCd : Field
        {
            public readonly FieldValue Idle = Value("Idle");
            public readonly FieldValue Load = Value("LOAD");
            public readonly FieldValue Process = Value("Process");
            public readonly FieldValue Reserved = Value("Reserved");
            public readonly FieldValue Selected = Value("Selected");
        }

        /// <summary>Durable 의 MES 상태(MES_STAT_TYP) — 장비의 같은 칸은 <see cref="EquipmentFields.MesStatTyp"/>(Up · Down)다.</summary>
        public sealed class MesStatTyp : Field
        {
            public readonly FieldValue Created = Value("Created");
            public readonly FieldValue Hold = Value("Hold");
            public readonly FieldValue Normal = Value("Normal");
            public readonly FieldValue Scrapped = Value("Scrapped");
        }
    }
}
