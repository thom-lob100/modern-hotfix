using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Controls.Wpf.Input;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.ResponseContracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.Samples.Contracts;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Display;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{

    public partial class LotManagementForm
    {
        // 전문 키(요청 전용) — 응답 컬럼이 아니라 엔티티에 두지 않는다.
        private const string TrayId = "TRAY_ID";
        private const string ParentLotIdKey = "PARENT_LOT_ID";

        // 응답 필수: TYP_VAL
        private DataTable GetLotTypes()
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetLotTypeList");
            return this.Request(request.ToString()).Table;
        }

        // 응답 필수: LOT_ID. Hold/NotOnHold 입력에는 현재 OPER_ID도 필요하다.
        // 표시·판정: SUB_PROD_TYP, LOT_STAT_TYP, MES_PROC_STAT_CD, LOT_HOLD_STAT_CD
        // 응답 필수(실행 조건 — 없으면 해당 버튼이 꺼지고 툴팁에 "Lot · 캡션: missing"으로 보인다):
        //   Scrap      LOT_STAT_TYP, MES_PROC_STAT_CD, LOT_HOLD_STAT_CD, LOT_PROC_STAT_CD,
        //              LOC_NM(Lot 이 담긴 캐리어의 위치 — Durable 의 LOC_NM, InStocker 면 막는다)
        //   ChangeSpec LOT_PROC_STAT_CD, MES_PROC_STAT_CD, LOT_HOLD_STAT_CD, LOT_STAT_TYP, EQP_ID(NULL 이어야 열린다)
        private DataTable GetLots(string subProdTyp, string crtFromDt, string crtToDt, string lotId)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetLotList");
            request.Append(" " + Schemas.Lot.SubProdTyp.Name + "=").Append(subProdTyp);
            request.Append(" " + ServerFields.Lot.CrtFromDt + "=").Append(crtFromDt);
            request.Append(" " + ServerFields.Lot.CrtToDt + "=").Append(crtToDt);
            request.Append(" " + Schemas.Lot.LotId.Name + "=").Append(lotId);
            return this.Request(request.ToString()).Table;
        }

        // 응답 필수: LOT_ID
        private DataTable GetLotIdCandidates(string keyword)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetLotIdCandidates");
            request.Append(" " + ServerFields.Keyword + "=").Append(keyword);
            return this.Request(request.ToString()).Table;
        }

        // 응답 필수: WF_ID, SLOT_NO
        private DataTable GetWafers(string lotId)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetLotUnitList");
            request.Append(" " + Schemas.Lot.LotId.Name + "=").Append(lotId);
            return this.Request(request.ToString()).Table;
        }

        // 응답 필수: 없음
        private DataTable GetLotHistory(string lotId)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetLotHistory");
            request.Append(" " + Schemas.Lot.LotId.Name + "=").Append(lotId);
            return this.Request(request.ToString()).Table;
        }

        // Create Chip · Create Lamella 의 Tray 후보 — Carrier Edit 화면이 Tray 를 받는 전문과 같다(GetDurableList DURABLE_TYP=TRAY).
        // 응답 필수: DURABLE_ID, STUB_LOT_QTY(Create Chip) / LCC_LOT_QTY(Create Lamella)
        //   Create Chip 은 STUB_LOT_QTY 가 0, Create Lamella 는 LCC_LOT_QTY 가 0 인 Tray 만 후보로 보인다(Services/TrayRoom.cs).
        private DataTable GetTrays()
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetDurableList");
            request.Append(" " + Schemas.Durable.DurableTyp.Name + "=").Append(Schemas.Durable.DurableTyp.Tray.Code);
            return this.Request(request.ToString()).Table;
        }

        // 응답 필수: 없음
        private DataTable GetWaferHistory(string lotId, string wfId)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetLotUnitHistory");
            request.Append(" " + Schemas.Lot.LotId.Name + "=").Append(lotId);
            request.Append(" " + Schemas.Wafer.WfId.Name + "=").Append(wfId);
            return this.Request(request.ToString()).Table;
        }

        // 응답 필수: 없음
        // Create Chip / Create Lamella 는 판정 표를 함께 실어 **한 전문**으로 보낸다(생성과 판정이 한 번에 비즈로직을 탄다).
        // 판정 표는 JUDGE_RESULTS=[{행},…] JSON 배열 하나다. 전문 이름은 홈 예제이며 회사는 갈래마다 교체한다.
        private DataActionResult CreateChildLot(string subProdTyp, string parentLotId, string trayId, DataTable judgeResults)
        {
            StringBuilder request = new StringBuilder();

            if (subProdTyp == Schemas.Lot.SubProdTyp.Chip.Code)
            {
                request.Append("CreateChipLot");
            }
            else
            {
                request.Append("CreateLamellaLot");
            }

            request.Append(" " + ParentLotIdKey + "=").Append(parentLotId);
            request.Append(" " + TrayId + "=").Append(trayId);
            request.Append(" " + ServerFields.UserId + "=").Append(this.CurrentUserId);
            request.Append(" JUDGE_RESULTS=").Append(JudgeResults.ToJson(judgeResults));
            return this.Request(request.ToString());
        }

        // 응답 필수: 없음. PROD_ID · FLOW_ID · OPER_ID 는 바꿀 새 스펙이고, currentOperId 는 Lot 정보에 있는 지금 공정이다.
        // currentOperId · description 은 컬럼이 없는 값이라 사용자가 정한 이름 그대로 싣는다(Hold 전문과 같은 이름).
        private DataActionResult ChangeLotSpec(
                string lotId, string prodId, string flowId, string operId, string currentOperId, string description)
        {
            return this.RequestFields("ChangeLotSpec", Schemas.Lot.LotId.Name, lotId,
                    Schemas.Lot.ProdId.Name, prodId, Schemas.Lot.FlowId.Name, flowId, Schemas.Lot.OperId.Name, operId,
                    "currentOperId", currentOperId, "description", description, ServerFields.UserId, this.CurrentUserId);
        }

        // 응답 필수: 없음
        private DataActionResult HoldLot(string lotId, string currentOperId, string holdCode, string engrUserId, string description)
        {
            return this.RequestFields("HoldLot", Schemas.Lot.LotId.Name, lotId,
                    "currentOperId", currentOperId, "holdCode", holdCode, "engrUserId", engrUserId,
                    "description", description, ServerFields.UserId, this.CurrentUserId);
        }

        // 응답 필수: 없음
        private DataActionResult NotOnHoldLot(string lotId, string currentOperId, string releaseCode, string engrUserId, string description)
        {
            return this.RequestFields("NotOnHoldLot", Schemas.Lot.LotId.Name, lotId,
                    "currentOperId", currentOperId, "releaseCode", releaseCode, "engrUserId", engrUserId,
                    "description", description, ServerFields.UserId, this.CurrentUserId);
        }

        // 응답 필수: 없음. LOT_ID 는 필수다. 전체 Scrap(모든 단위 선택)이면 WF_ID 를 싣지 않고,
        // 부분 Scrap 이면 고른 단위 Id 를 콤마로 이어 WF_ID 에 싣는다. 사유 코드 키(scrapCode)는 회사 전문 키로 교체한다.
        private DataActionResult ScrapLot(string lotId, string scrapCode, string[] waferIds, string description)
        {
            if (waferIds == null || waferIds.Length == 0)
            {
                return this.RequestFields("ScrapLot", Schemas.Lot.LotId.Name, lotId,
                        "scrapCode", scrapCode, "description", description, ServerFields.UserId, this.CurrentUserId);
            }

            return this.RequestFields("ScrapLot", Schemas.Lot.LotId.Name, lotId,
                    Schemas.Wafer.WfId.Name, string.Join(",", waferIds),
                    "scrapCode", scrapCode, "description", description, ServerFields.UserId, this.CurrentUserId);
        }
    }
}
