using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>기준정보 Port 화면(Port Master 메뉴) — 위 일곱 칸은 포트 화면들이 함께 쓰는 칸이고, 편집 컬럼(우선순위 · 사용 여부 · 설명 · 수정자/수정시각)만 더 가진다.
    /// 기준정보 화면은 상태 칸이 응답에 없어 표에 나타나지 않는다. <c>PRIORITY</c> 캡션은 이 화면만 "Priority" 다(다른 화면은 "Prio").</summary>
    public sealed class PortEntity : TableEntity
    {
        public readonly Field EqpId = EquipmentColumns.EqpId();
        public readonly Field PortNm = PortColumns.PortNm();
        public readonly PortFields.PortTyp PortTyp = PortColumns.PortTyp().Badge();
        public readonly Field Mode = PortColumns.Mode();
        public readonly PortFields.TransferStatCd TransferStatCd = PortColumns.TransferStatCd();
        public readonly Field LotId = LotColumns.LotId();
        public readonly Field DurableId = DurableColumns.DurableId();
        public readonly Field Priority = CommonColumns.Priority().Caption("Priority");
        public readonly Field UseYn = CommonColumns.UseYn();
        public readonly Field Description = CommonColumns.Description();
        public readonly Field UpdatedBy = CommonColumns.UpdatedBy();
        public readonly Field UpdatedAt = CommonColumns.UpdatedAt();

        public PortEntity()
        {
            this.KeyFields = Fields(this.EqpId, this.PortNm);
            // 키 외 필수 입력은 회사가 정한다 — 홈 예제 컬럼을 필수로 두지 않는다. 값의 유효성은 서버가 판정한다.
            this.ComboDisplayFields = Fields(this.PortNm, this.PortTyp);
            this.ValueField = this.PortNm;
            this.Seal();
        }
    }
}
