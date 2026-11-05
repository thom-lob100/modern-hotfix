using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>
    /// 판정 입력 표(Job End · Create Chip · Create Lamella 공통) — 서버가 줄 수와 입력 칸이 아닌 값을 내려주고,
    /// 화면은 SLOT_NO · FINGER_ID · FINGER_POSITION · JUDGE_RSLT · GRID_INDEX 만 채운다. 키는 DURABLE_ID + SLOT_NO +
    /// FINGER_ID + TIMEKEY 다. DURABLE_TYP 은 TRAY, DET_DURABLE_TYP 은 STUB(Chip) 또는 LCC(Lamella)다.
    /// 판정 창의 편집기는 이 엔티티를 <c>UseEntity</c>로 연결한다 — 키 중 서버가 주는 DURABLE_ID · TIMEKEY 는 읽기 전용이고,
    /// JUDGE_RSLT · GRID_INDEX 는 Chip · Lamella 공통 필수다. Lamella 만의 필수(FINGER_POSITION)는 창이 더한다.
    /// </summary>
    public sealed class JudgeResultEntity : TableEntity
    {
        public readonly Field DurableId = DurableColumns.DurableId();
        public readonly Field SlotNo = CommonColumns.SlotNo();
        public readonly Field FingerId = DurableColumns.FingerId();
        public readonly Field TimeKey = CommonColumns.TimeKey();
        public readonly DurableFields.DurableTyp DurableTyp = DurableColumns.DurableTyp();
        public readonly DurableFields.DetDurableTyp DetDurableTyp = Field<DurableFields.DetDurableTyp>("DET_DURABLE_TYP");
        public readonly DurableFields.FingerPosition FingerPosition = Field<DurableFields.FingerPosition>("FINGER_POSITION").Caption("Position").TopLabel("Finger");
        public readonly CommonFields.JudgeRslt JudgeRslt = CommonColumns.JudgeRslt();
        public readonly Field GridIndex = Field("GRID_INDEX");

        public JudgeResultEntity()
        {
            this.KeyFields = Fields(this.DurableId, this.SlotNo, this.FingerId, this.TimeKey);
            this.ReadOnlyFields = Fields(this.DurableId, this.TimeKey);
            this.RequiredFields = Fields(this.JudgeRslt, this.GridIndex);
            this.Seal();
        }
    }
}
