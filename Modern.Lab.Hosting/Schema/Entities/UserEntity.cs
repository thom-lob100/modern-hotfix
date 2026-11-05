using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class UserEntity : TableEntity
    {
        public readonly Field UserId = CommonColumns.UserId();
        public readonly Field UserNm = Field("USER_NM");
        public readonly Field Description = CommonColumns.Description();
        public readonly Field UseYn = CommonColumns.UseYn();

        public UserEntity()
        {
            this.KeyFields = Fields(this.UserId);
            this.ComboDisplayFields = Fields(this.UserId, this.UserNm);
            this.ValueField = this.UserId;
            this.Seal();
        }
    }
}
