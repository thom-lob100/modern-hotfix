using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public abstract partial class TableEntity
    {
        /// <summary>포트 칸 사전 — 회사 칸 이름 · 기본 캡션이 다르면 여기 한 줄만 고친다. 값 목록은 <see cref="PortFields"/>.</summary>
        protected static class PortColumns
        {
            public static Field Mode()
            {
                return Field("MODE").Caption("Mode").Badge();
            }

            public static Field PortNm()
            {
                return Field("PORT_NM").Caption("Port");
            }

            public static PortFields.PortTyp PortTyp()
            {
                return Field<PortFields.PortTyp>("PORT_TYP").Caption("Type");
            }

            /// <summary>처리 중(Processing) 포트는 상태 배지가 돈다(2026-10-26).</summary>
            public static PortFields.TransferStatCd TransferStatCd()
            {
                PortFields.TransferStatCd transferStatCd = Field<PortFields.TransferStatCd>("TRANSFER_STAT_CD").Caption("Status").Badge();
                transferStatCd.Spin(transferStatCd.Processing);
                return transferStatCd;
            }
        }
    }
}
