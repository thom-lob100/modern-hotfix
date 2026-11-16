using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using Modern.Lab.Hosting;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Data;

using Modern.Lab.Hosting.Messaging;

namespace Modern.Lab.Samples
{
    public partial class LogisticsRequestForm
    {
        // 회사의 Receive / Create 조건 — 위에서부터 보고 처음 막은 사유가 메뉴 툴팁·실행 직전 경고에 나온다(null이면 허용).
        // 판정은 이 파일에서 끝난다 — 행 표식(ApplyActionRules)·메뉴·셀 버튼·일괄 처리 직전 재판정이 모두 이 함수를 부른다.
        internal static string LogisticsActionReason(DataRow row, string step)
        {
            if (row == null)
            {
                return "Select a lot first.";
            }

            bool received = TableHelper.CellText(row, Schemas.Logistics.RecvYn.Name).Trim() == "Y"
                    || TableHelper.CellText(row, Schemas.Logistics.RecvTm.Name).Trim().Length > 0;
            bool linked = TableHelper.CellText(row, Schemas.Logistics.ReqSerialNo.Name).Trim().Length > 0;
            bool created = TableHelper.CellText(row, Schemas.Logistics.ProcYn.Name).Trim() == "Y";
            bool notified = LogisticsRequestPresenter.IsNotified(row);

            switch (step)
            {
                case "Receive":
                    if (received)
                    {
                        return "The lot is already received.";
                    }

                    return notified ? null : "The lot has not been sent yet.";

                case "Create":
                    if (!received)
                    {
                        return "The lot is not received yet.";
                    }

                    if (!linked)
                    {
                        return "The lot has no linked request.";
                    }

                    if (created)
                    {
                        return "The lot is already created.";
                    }

                    return HasOtherLotRequest(row) && !IsPartialWafer(row)
                            ? "Only a PartialWafer lot can add a request."
                            : null;

                default:
                    return "This action is not supported.";
            }
        }

        internal static bool CanProcessRow(DataRow row, string step)
        {
            return LogisticsActionReason(row, step) == null;
        }

        // 추가의뢰 조건 — Lot 의 의뢰 번호(LOT_REQ_SERIAL_NO)가 있고 의뢰서의 REQ_SERIAL_NO 와 다르며, Lot 의 SUB_PROD_TYP 가 PartialWafer 다.
        // 맞으면 Create 가 열린 행의 버튼·메뉴가 Add Request 로 나오고 Create 전문 대신 AddRequest 전문을 보낸다.
        // 번호는 다른데 PartialWafer 가 아니면 Create 도 Add Request 도 열리지 않는다(위 LogisticsActionReason 의 Create 마지막 사유).
        internal static bool IsAddRequest(DataRow row)
        {
            return HasOtherLotRequest(row) && IsPartialWafer(row);
        }

        private static bool HasOtherLotRequest(DataRow row)
        {
            string lotReqSerialNo = Schemas.Logistics.LotReqSerialNo.Text(row);

            return lotReqSerialNo.Length > 0 && lotReqSerialNo != Schemas.Logistics.ReqSerialNo.Text(row);
        }

        private static bool IsPartialWafer(DataRow row)
        {
            return Schemas.Logistics.SubProdTyp.Is(row, Schemas.Logistics.SubProdTyp.PartialWafer);
        }

        internal static void ApplyActionRules(DataRow row)
        {
            row[LogisticsRequestPresenter.ReceiveCanColumn] = CanProcessRow(row, "Receive");
            row[LogisticsRequestPresenter.CreateCanColumn] = CanProcessRow(row, "Create");
            row[LogisticsRequestPresenter.AddRequestColumn] = IsAddRequest(row);
        }
    }
}
