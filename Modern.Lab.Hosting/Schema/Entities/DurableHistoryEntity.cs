using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class DurableHistoryEntity : TableEntity
    {
        public readonly Field DurableId = DurableColumns.DurableId();
        public readonly Field TimeKey = CommonColumns.TimeKey();
        public readonly Field EventTm = CommonColumns.EventTm();
        public readonly Field EventCd = CommonColumns.EventCd();
        public readonly DurableFields.DurableStatCd DurableStatCd = DurableColumns.DurableStatCd();
        public readonly WaferFields.WfLoadStatCd WfLoadStatCd = WaferColumns.WfLoadStatCd();
        public readonly Field LotQty = LotColumns.LotQty();
        public readonly Field Location = CommonColumns.Location();
        public readonly Field UserId = CommonColumns.UserId();
        public readonly Field Remark = CommonColumns.Remark();

        // ── 필드 꾸밈 예제 — 다른 엔티티도 같은 형식으로 적는다 ─────────────────────
        // 사전에서 가져오기  CommonColumns.Location()            — 여러 엔티티가 쓰는 칸(TableEntity.*Columns.cs)
        // 사전 + 다른 캡션   CommonColumns.Location().Caption("Place")
        // 캡션            Field("LOT_QTY").Caption("Qty")
        // 윗글씨          Field("LOT_QTY").TopLabel("Lot")
        // 아랫글씨        Field("LOT_QTY").BottomLabel("ea")
        // 형식(숫자)      Field("LOT_QTY").Format("#,##0")
        // 형식(날짜)      Field("EVENT_TM").Format("yyyy-MM-dd HH:mm:ss")
        // 셀 정렬         Field("LOT_QTY").TextAlignment(GridTextAlignment.Right)
        // 헤더 정렬       Field("LOT_QTY").HeaderAlignment(GridTextAlignment.Center)
        // 윗글씨 정렬     Field("LOT_QTY").TopLabelAlignment(GridTextAlignment.Left)
        // 아랫글씨 정렬   Field("LOT_QTY").BottomLabelAlignment(GridTextAlignment.Right)
        // 배지            Field<WaferFields.WfLoadStatCd>("WF_LOAD_STAT_CD").Badge()
        // 한 줄에 전부    Field("LOT_QTY").Caption("Qty").TopLabel("Lot").BottomLabel("ea").Format("#,##0")
        //                         .TextAlignment(GridTextAlignment.Right).HeaderAlignment(GridTextAlignment.Center)
        // 정렬 값을 쓰려면 파일 위에 using Modern.Lab.Controls.Wpf.Data; 를 더한다.

        public DurableHistoryEntity()
        {
            this.KeyFields = Fields(this.DurableId, this.TimeKey);
            this.DisplayFields = Fields(
                    this.DurableId, this.TimeKey, this.EventTm, this.EventCd, this.DurableStatCd,
                    this.WfLoadStatCd, this.LotQty, this.Location, this.UserId, this.Remark);

            // ── 필드 목록 예제 — Seal() 앞에 적는다 ─────────────────────────────────
            // 행 키                     this.KeyFields          = Fields(this.DurableId, this.TimeKey);
            // 보일 컬럼과 순서          this.DisplayFields      = Fields(this.DurableId, this.EventTm);
            // 숨길 컬럼                 this.ExcludeFields      = Fields(this.TimeKey);
            // 키 외의 필수 입력         this.RequiredFields     = Fields(this.EventCd);
            // 신규 때만 입력            this.CreateOnlyFields   = Fields(this.Location);
            // 항상 읽기 전용            this.ReadOnlyFields     = Fields(this.UserId);
            // 콤보에 보일 컬럼과 순서   this.ComboDisplayFields = Fields(this.DurableId, this.Location);
            // 콤보의 선택 값            this.ValueField         = this.DurableId;
            // 트리의 부모 연결          this.ParentField        = this.Location;

            this.Seal();
        }
    }
}
