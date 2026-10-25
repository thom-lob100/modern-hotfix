using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>기준정보 Port 화면(Port Master 메뉴) — 공통 필드는 <see cref="PortBase"/>. 편집 컬럼(우선순위 · 사용 여부 · 설명 · 수정자/수정시각)만 더 가진다.
    /// <c>PRIORITY</c> 는 <see cref="PortPriorityEntity"/> 와 캡션이 달라("Priority" / "Prio") Base 에 올리지 않는다.</summary>
    public class PortEntity : PortBase
    {
        public readonly Field Priority = Field("PRIORITY").Caption("Priority");
        public readonly Field UseYn = Field("USE_YN");
        public readonly Field Description = Field("DESCRIPTION").Caption("Description");
        public readonly Field UpdatedBy = Field("UPDATED_BY");
        public readonly Field UpdatedAt = Field("UPDATED_AT");

        public PortEntity()
        {
            // 키 외 필수 입력은 회사가 정한다 — 홈 예제 컬럼을 필수로 두지 않는다. 값의 유효성은 서버가 판정한다.
            this.Seal();
        }
    }
}
