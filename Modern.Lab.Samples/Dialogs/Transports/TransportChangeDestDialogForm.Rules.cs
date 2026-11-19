using System.Data;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class TransportChangeDestDialogForm
    {
        // Change Destination 버튼이 열리는 조건(사용자 2026-11-15) — Eqp Id · Port 콤보는 서버가 거른 목록 그대로이고,
        // 이 조건은 목록을 거르지 않고 마지막 버튼만 켜고 끈다. 고른 포트 행을 위에서부터 보고(반송 행의 지금 포트 칸이 아니다),
        // 처음 막은 줄의 문구가 버튼 툴팁에 뜬다. 꺼진 버튼을 누르면 규칙 안내 창이 필드 · 요구 값 · 지금 값을 보인다.
        // 한 줄에 하나, 회사 조건이 바뀌면 여기만 고친다. 응답에 컬럼이 없으면 막는다.
        // 어느 반송의 목적지를 바꿀 수 있는지(이동 상태 · 폐기)는 화면의 버튼 조건이 본다(TransportManagementForm.Rules.cs).
        // 장비는 보지 않는다 — 반송의 출발 · 목적 장비(START_GOAL_VAL · END_GOAL_VAL)와 Destination 을 실어 보내 서버가 거른다.
        private const string PortEntityName = "Port";

        // 포트 — 장비 · STB · Stocker 모두 같은 인포트 조건이다(사용자 2026-11-09 에 받은 포트 조건 7개 — 홈 서버도 같은 조건으로 목록을 거른다).
        private static readonly ActionRule[] ChangeDestinationRules =
        {
            ActionRule.Value(PortEntityName, Schemas.PortDet.PortTyp, Schemas.PortDet.PortTyp.Input, Schemas.PortDet.PortTyp.InputOutput),
            ActionRule.Value(PortEntityName, Schemas.PortDet.AcssTyp, Schemas.PortDet.AcssTyp.Auto),
            ActionRule.Value(PortEntityName, Schemas.PortDet.ResrcStatTyp, Schemas.PortDet.ResrcStatTyp.InService),
            ActionRule.Value(PortEntityName, Schemas.PortDet.PortStatNm, Schemas.PortDet.PortStatNm.Up),
            ActionRule.Value(PortEntityName, Schemas.PortDet.VirtPortYn, Schemas.PortDet.VirtPortYn.N),
            ActionRule.Value(PortEntityName, Schemas.PortDet.TransferStatCd, Schemas.PortDet.TransferStatCd.ReadyToLoad),
            ActionRule.Empty(PortEntityName, Schemas.PortDet.CurrDurableId)
        };

        // Destination 콤보의 두 번째 항목 — DURABLE_TYP 이 FOUP 이면 STB, 그 밖(TRAY)은 Stocker 다.
        private static bool UsesStb(DataRow transport)
        {
            return transport != null && Schemas.Transport.DurableTyp.Is(transport, Schemas.Transport.DurableTyp.Foup);
        }
    }
}
