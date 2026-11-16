using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>
    /// 판정 입력 표(Job End · Create Chip · Create Lamella 공통) — 서버가 줄 수와 입력 칸이 아닌 값을 내려주고,
    /// 화면은 SLOT_NO · FINGER_ID · FINGER_POSITION · JUDGE_RSLT · GRID_INDEX 만 채운다. 분석 공정의 Job End 는
    /// 여기에 ANALYZE_RSLT(PASS / FAIL)를 더 채운다. 키는 DURABLE_ID + SLOT_NO + FINGER_ID + TIMEKEY 다.
    /// DURABLE_TYP 은 TRAY, DET_DURABLE_TYP 은 STUB(Chip) 또는 LCC(Lamella · 분석)다.
    /// 판정 창은 표 안에서 바로 입력한다(2026-11-15) — 입력 칸 · 필수 칸 · 콤보 선택지는 창의 <c>.Grid.cs</c> 에 있고,
    /// 이 엔티티는 칸 이름 · 캡션 · 값 목록만 준다.
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
        public readonly CommonFields.AnalyzeRslt AnalyzeRslt = Field<CommonFields.AnalyzeRslt>("ANALYZE_RSLT").Caption("Analyze Result");
        public readonly Field GridIndex = Field("GRID_INDEX");

        public JudgeResultEntity()
        {
            this.KeyFields = Fields(this.DurableId, this.SlotNo, this.FingerId, this.TimeKey);
            this.Seal();
        }
    }
}
