using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class AreaEntity : TableEntity
    {
        public readonly Field AreaId = Field("AREA_ID");
        public readonly Field LocationType = Field("LOCATION_TYPE");
        public readonly Field LocationName = Field("LOCATION_NAME");
        public readonly Field FabId = Field("FAB_ID");
        public readonly Field ParentAreaId = Field("PARENT_AREA_ID");
        public readonly Field UseYn = CommonColumns.UseYn();
        public readonly Field Description = CommonColumns.Description();
        public readonly Field UpdatedBy = CommonColumns.UpdatedBy();
        public readonly Field UpdatedAt = CommonColumns.UpdatedAt();

        public AreaEntity()
        {
            this.KeyFields = Fields(this.AreaId);
            // 키 외 필수 입력은 회사가 정한다 — 홈 예제 컬럼을 필수로 두지 않는다. 값의 유효성은 서버가 판정한다.
            this.ComboDisplayFields = Fields(this.AreaId);
            this.ValueField = this.AreaId;
            this.Seal();
        }
    }
}
