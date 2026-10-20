using Modern.Lab.Data;
using System;
using System.Data;
using System.Text;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Samples.Services;

namespace Modern.Lab.Samples
{
    public partial class LotEquipmentForm
    {
        // EQP_GRP_ID
        private DataTable GetEquipmentGroups()
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetEqpGroupList");
            return this.Request(request.ToString()).Table;
        }

        // EQP_ID
        // COMM_STAT_TYP
        // MES_EQP_STAT_CD
        // MES_STAT_TYP
        // EQP_TYP
        // DET_EQP_TYP
        private DataTable GetEquipmentPriorities(DataRow lot)
        {
            string eqpGrpId = TableHelper.CellText(lot, "EQP_GRP_ID").Trim();
            string eqpId = TableHelper.CellText(lot, "EQP_ID").Trim();
            string lotId = TableHelper.CellText(lot, "LOT_ID").Trim();
            StringBuilder request = new StringBuilder();
            request.Append("GetEqpList");
            if (!string.IsNullOrEmpty(eqpGrpId))
            {
                AppendField(request, "EQP_GRP_ID", eqpGrpId);
            }
            if (!string.IsNullOrEmpty(eqpId))
            {
                AppendField(request, "EQP_ID", eqpId);
            }
            if (!string.IsNullOrEmpty(lotId))
            {
                AppendField(request, "LOT_ID", lotId);
            }
            return this.Request(request.ToString()).Table;
        }

        // PORT_NM
        private DataTable GetPortPriorities(string eqpId, DataRow lot)
        {
            string lotId = TableHelper.CellText(lot, "LOT_ID").Trim();
            string inPort = TableHelper.CellText(lot, "PORT_NM").Trim();
            string outPort = TableHelper.CellText(lot, "GOAL_PORT_NM").Trim();
            StringBuilder request = new StringBuilder();
            request.Append("GetEqpPortList");
            AppendField(request, "EQP_ID", eqpId);
            if (!string.IsNullOrEmpty(lotId))
            {
                AppendField(request, "LOT_ID", lotId);
            }
            if (!string.IsNullOrEmpty(inPort))
            {
                AppendField(request, "IN_PORT", inPort);
            }
            if (!string.IsNullOrEmpty(outPort))
            {
                AppendField(request, "OUT_PORT", outPort);
            }
            return this.Request(request.ToString()).Table;
        }

        // EQP_ID
        // MES_EQP_STAT_CD
        private DataTable GetChildEquipmentPriorities(string eqpId, DataRow lot)
        {
            string lotId = TableHelper.CellText(lot, "LOT_ID").Trim();
            StringBuilder request = new StringBuilder();
            request.Append("GetChildEqpList");
            AppendField(request, "EQP_ID", eqpId);
            if (!string.IsNullOrEmpty(lotId))
            {
                AppendField(request, "LOT_ID", lotId);
            }
            return this.Request(request.ToString()).Table;
        }

        // LOT_ID
        // LAST_EVENT_CD
        // LOT_PROC_STAT_CD
        // MES_PROC_STAT_CD
        // LOT_HOLD_STAT_CD
        // LOT_STAT_TYP
        private DataTable GetLotPriorities(string eqpGrpId, string eqpId, string lotId)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetJobLotList");
            if (!string.IsNullOrEmpty(eqpGrpId))
            {
                AppendField(request, "EQP_GRP_ID", eqpGrpId);
            }
            if (!string.IsNullOrEmpty(eqpId))
            {
                AppendField(request, "EQP_ID", eqpId);
            }
            if (!string.IsNullOrEmpty(lotId))
            {
                AppendField(request, "LOT_ID", lotId);
            }
            return this.Request(request.ToString()).Table;
        }

        // DURABLE_ID
        // MES_STAT_TYP
        // CLEAN_TYP
        private DataTable GetDurablePriorities(string eqpId, string goalPortNm, DataRow lot)
        {
            string lotId = TableHelper.CellText(lot, "LOT_ID").Trim();
            string durableId = TableHelper.CellText(lot, "GOAL_CARRIER_ID").Trim();
            StringBuilder request = new StringBuilder();
            request.Append("GetJobDurableList");
            AppendField(request, "EQP_ID", eqpId);
            AppendField(request, "PORT_NM", goalPortNm);
            if (!string.IsNullOrEmpty(lotId))
            {
                AppendField(request, "LOT_ID", lotId);
            }
            if (!string.IsNullOrEmpty(durableId))
            {
                AppendField(request, "DURABLE_ID", durableId);
            }
            return this.Request(request.ToString()).Table;
        }

        private DataActionResult JobPrep(JobExecutionTarget target, string description)
        {
            return this.Request(JobPrepRequest(target, description, this.CurrentUserId));
        }

        private static string JobPrepRequest(JobExecutionTarget target, string description, string userId)
        {
            string eqpGrpId = target.EqpGrpId.Length == 0 ? target.SelectedEqpGrpId : target.EqpGrpId;
            StringBuilder request = new StringBuilder();
            switch (eqpGrpId)
            {
                case "GRP-A":
                case "GRP-B":
                case "GRP-C":
                case "GRP-D":
                    switch (target.CommStatTyp)
                    {
                        case "OnLineLocal":
                            request.Append("JobPrepLocal");
                            AppendField(request, "EQP_GRP_ID", eqpGrpId);
                            AppendField(request, "EQP_ID", target.EqpId);
                            AppendField(request, "IN_PORT", target.PortNm);
                            AppendField(request, "OUT_PORT", target.GoalPortNm);
                            AppendField(request, "LOT_ID", target.LotId);
                            AppendField(request, "GOAL_CARRIER_ID", target.GoalDurableId);
                            if (target.ChildEqpId.Length > 0)
                            {
                                AppendField(request, "CHILD_EQP_ID", target.ChildEqpId);
                            }
                            break;

                        case "OnLineRemote":
                            request.Append("JobPrepRemote");
                            AppendField(request, "EQP_GRP_ID", eqpGrpId);
                            AppendField(request, "EQP_ID", target.EqpId);
                            AppendField(request, "IN_PORT", target.PortNm);
                            AppendField(request, "OUT_PORT", target.GoalPortNm);
                            AppendField(request, "LOT_ID", target.LotId);
                            AppendField(request, "CARRIER_ID", target.DurableId);
                            AppendField(request, "GOAL_CARRIER_ID", target.GoalDurableId);
                            if (target.ChildEqpId.Length > 0)
                            {
                                AppendField(request, "CHILD_EQP_ID", target.ChildEqpId);
                            }
                            break;

                        case "OffLine":
                            request.Append("JobPrepOffline");
                            AppendField(request, "EQP_GRP_ID", eqpGrpId);
                            AppendField(request, "EQP_ID", target.EqpId);
                            AppendField(request, "IN_PORT", target.PortNm);
                            AppendField(request, "OUT_PORT", target.GoalPortNm);
                            AppendField(request, "LOT_ID", target.LotId);
                            AppendField(request, "GOAL_CARRIER_ID", target.GoalDurableId);
                            if (target.ChildEqpId.Length > 0)
                            {
                                AppendField(request, "CHILD_EQP_ID", target.ChildEqpId);
                            }
                            break;

                        default:
                            throw UnknownJobBranch("JobPrep", eqpGrpId, target.CommStatTyp);
                    }

                    break;

                default:
                    throw UnknownJobBranch("JobPrep", eqpGrpId, target.CommStatTyp);
            }

            AppendField(request, "DESCRIPTION", description);
            AppendField(request, "USER_ID", userId);
            return request.ToString();
        }

        private DataActionResult JobStart(JobExecutionTarget target, string description)
        {
            return this.Request(JobStartRequest(target, description, this.CurrentUserId));
        }

        private static string JobStartRequest(JobExecutionTarget target, string description, string userId)
        {
            string eqpGrpId = target.EqpGrpId.Length == 0 ? target.SelectedEqpGrpId : target.EqpGrpId;
            StringBuilder request = new StringBuilder();
            switch (eqpGrpId)
            {
                case "GRP-A":
                case "GRP-B":
                case "GRP-C":
                case "GRP-D":
                    switch (target.CommStatTyp)
                    {
                        case "OnLineLocal":
                            request.Append("JobStartLocal");
                            AppendField(request, "EQP_GRP_ID", eqpGrpId);
                            AppendField(request, "EQP_ID", target.EqpId);
                            AppendField(request, "IN_PORT", target.PortNm);
                            AppendField(request, "OUT_PORT", target.GoalPortNm);
                            AppendField(request, "LOT_ID", target.LotId);
                            AppendField(request, "CARRIER_ID", target.DurableId);
                            AppendField(request, "GOAL_CARRIER_ID", target.GoalDurableId);
                            break;

                        case "OnLineRemote":
                            request.Append("JobStartRemote");
                            AppendField(request, "EQP_GRP_ID", eqpGrpId);
                            AppendField(request, "EQP_ID", target.EqpId);
                            AppendField(request, "LOT_ID", target.LotId);
                            break;

                        case "OffLine":
                            request.Append("JobStartOffline");
                            AppendField(request, "EQP_GRP_ID", eqpGrpId);
                            AppendField(request, "EQP_ID", target.EqpId);
                            AppendField(request, "LOT_ID", target.LotId);
                            break;

                        default:
                            throw UnknownJobBranch("JobStart", eqpGrpId, target.CommStatTyp);
                    }

                    break;

                default:
                    throw UnknownJobBranch("JobStart", eqpGrpId, target.CommStatTyp);
            }

            AppendField(request, "DESCRIPTION", description);
            AppendField(request, "USER_ID", userId);
            return request.ToString();
        }

        private DataActionResult JobEnd(JobExecutionTarget target, string description, DataTable judgeResults)
        {
            string group = target.EqpGrpId.Length == 0 ? target.SelectedEqpGrpId : target.EqpGrpId;
            JudgeKind kind = JudgeKindOf(group);
            if (kind != JudgeKind.None && judgeResults == null)
            {
                throw new ArgumentException(kind == JudgeKind.Chip
                        ? "Chip judge rows are required to end this job."
                        : "Lamella judge rows are required to end this job.");
            }

            string request = JobEndRequest(target, description, this.CurrentUserId);
            if (kind != JudgeKind.None)
            {
                request += " JUDGE_RESULTS=" + JudgeResultJson.ToJson(judgeResults);
            }

            return this.Request(request);
        }

        private static string JobEndRequest(JobExecutionTarget target, string description, string userId)
        {
            string eqpGrpId = target.EqpGrpId.Length == 0 ? target.SelectedEqpGrpId : target.EqpGrpId;
            StringBuilder request = new StringBuilder();
            switch (eqpGrpId)
            {
                case "GRP-A":
                case "GRP-B":
                case "GRP-C":
                case "GRP-D":
                    switch (target.CommStatTyp)
                    {
                        case "OnLineLocal":
                            request.Append("JobEndLocal");
                            AppendField(request, "EQP_GRP_ID", eqpGrpId);
                            AppendField(request, "EQP_ID", target.EqpId);
                            AppendField(request, "IN_PORT", target.PortNm);
                            AppendField(request, "OUT_PORT", target.GoalPortNm);
                            AppendField(request, "LOT_ID", target.LotId);
                            AppendField(request, "CARRIER_ID", target.DurableId);
                            AppendField(request, "GOAL_CARRIER_ID", target.GoalDurableId);
                            break;

                        case "OnLineRemote":
                            request.Append("JobEndRemote");
                            AppendField(request, "EQP_GRP_ID", eqpGrpId);
                            AppendField(request, "EQP_ID", target.EqpId);
                            AppendField(request, "LOT_ID", target.LotId);
                            break;

                        case "OffLine":
                            request.Append("JobEndOffline");
                            AppendField(request, "EQP_GRP_ID", eqpGrpId);
                            AppendField(request, "EQP_ID", target.EqpId);
                            AppendField(request, "LOT_ID", target.LotId);
                            break;

                        default:
                            throw UnknownJobBranch("JobEnd", eqpGrpId, target.CommStatTyp);
                    }

                    break;

                default:
                    throw UnknownJobBranch("JobEnd", eqpGrpId, target.CommStatTyp);
            }

            AppendField(request, "DESCRIPTION", description);
            AppendField(request, "USER_ID", userId);
            return request.ToString();
        }

        private DataActionResult JobPrepCancel(JobExecutionTarget target, string description)
        {
            return this.Request(JobPrepCancelRequest(target, description, this.CurrentUserId));
        }

        private static string JobPrepCancelRequest(JobExecutionTarget target, string description, string userId)
        {
            string eqpGrpId = target.EqpGrpId.Length == 0 ? target.SelectedEqpGrpId : target.EqpGrpId;
            StringBuilder request = new StringBuilder();
            switch (eqpGrpId)
            {
                case "GRP-A":
                case "GRP-B":
                case "GRP-C":
                case "GRP-D":
                    switch (target.CommStatTyp)
                    {
                        case "OnLineLocal":
                            request.Append("JobPrepCancelLocal");
                            AppendField(request, "EQP_GRP_ID", eqpGrpId);
                            AppendField(request, "EQP_ID", target.EqpId);
                            AppendField(request, "IN_PORT", target.PortNm);
                            AppendField(request, "OUT_PORT", target.GoalPortNm);
                            AppendField(request, "LOT_ID", target.LotId);
                            AppendField(request, "CARRIER_ID", target.DurableId);
                            AppendField(request, "GOAL_CARRIER_ID", target.GoalDurableId);
                            break;

                        case "OnLineRemote":
                            request.Append("JobPrepCancelRemote");
                            AppendField(request, "EQP_GRP_ID", eqpGrpId);
                            AppendField(request, "EQP_ID", target.EqpId);
                            AppendField(request, "LOT_ID", target.LotId);
                            break;

                        case "OffLine":
                            request.Append("JobPrepCancelOffline");
                            AppendField(request, "EQP_GRP_ID", eqpGrpId);
                            AppendField(request, "EQP_ID", target.EqpId);
                            AppendField(request, "LOT_ID", target.LotId);
                            break;

                        default:
                            throw UnknownJobBranch("JobPrepCancel", eqpGrpId, target.CommStatTyp);
                    }

                    break;

                default:
                    throw UnknownJobBranch("JobPrepCancel", eqpGrpId, target.CommStatTyp);
            }

            AppendField(request, "DESCRIPTION", description);
            AppendField(request, "USER_ID", userId);
            return request.ToString();
        }

        private DataActionResult JobStartCancel(JobExecutionTarget target, string description)
        {
            return this.Request(JobStartCancelRequest(target, description, this.CurrentUserId));
        }

        private static string JobStartCancelRequest(JobExecutionTarget target, string description, string userId)
        {
            string eqpGrpId = target.EqpGrpId.Length == 0 ? target.SelectedEqpGrpId : target.EqpGrpId;
            StringBuilder request = new StringBuilder();
            switch (eqpGrpId)
            {
                case "GRP-A":
                case "GRP-B":
                case "GRP-C":
                case "GRP-D":
                    switch (target.CommStatTyp)
                    {
                        case "OnLineLocal":
                            request.Append("JobStartCancelLocal");
                            AppendField(request, "EQP_GRP_ID", eqpGrpId);
                            AppendField(request, "EQP_ID", target.EqpId);
                            AppendField(request, "IN_PORT", target.PortNm);
                            AppendField(request, "OUT_PORT", target.GoalPortNm);
                            AppendField(request, "LOT_ID", target.LotId);
                            AppendField(request, "CARRIER_ID", target.DurableId);
                            AppendField(request, "GOAL_CARRIER_ID", target.GoalDurableId);
                            break;

                        case "OnLineRemote":
                            request.Append("JobStartCancelRemote");
                            AppendField(request, "EQP_GRP_ID", eqpGrpId);
                            AppendField(request, "EQP_ID", target.EqpId);
                            AppendField(request, "LOT_ID", target.LotId);
                            break;

                        case "OffLine":
                            request.Append("JobStartCancelOffline");
                            AppendField(request, "EQP_GRP_ID", eqpGrpId);
                            AppendField(request, "EQP_ID", target.EqpId);
                            AppendField(request, "LOT_ID", target.LotId);
                            break;

                        default:
                            throw UnknownJobBranch("JobStartCancel", eqpGrpId, target.CommStatTyp);
                    }

                    break;

                default:
                    throw UnknownJobBranch("JobStartCancel", eqpGrpId, target.CommStatTyp);
            }

            AppendField(request, "DESCRIPTION", description);
            AppendField(request, "USER_ID", userId);
            return request.ToString();
        }

        private DataActionResult JobSkip(JobExecutionTarget target, string skipCode, string description)
        {
            if (string.IsNullOrWhiteSpace(skipCode))
            {
                throw new ArgumentException("A skip code is required to skip this lot.");
            }

            StringBuilder request = new StringBuilder(JobSkipRequest(target, description, this.CurrentUserId));
            AppendField(request, "OPER_ID", target.OperId);
            AppendField(request, "skipCode", skipCode);
            return this.Request(request.ToString());
        }

        private static string JobSkipRequest(JobExecutionTarget target, string description, string userId)
        {
            string eqpGrpId = target.EqpGrpId.Length == 0 ? target.SelectedEqpGrpId : target.EqpGrpId;
            StringBuilder request = new StringBuilder();
            switch (eqpGrpId)
            {
                case "GRP-A":
                case "GRP-B":
                case "GRP-C":
                case "GRP-D":
                    switch (target.CommStatTyp)
                    {
                        case "OnLineLocal":
                            request.Append("JobSkipLocal");
                            AppendField(request, "EQP_GRP_ID", eqpGrpId);
                            AppendField(request, "EQP_ID", target.EqpId);
                            AppendField(request, "IN_PORT", target.PortNm);
                            AppendField(request, "OUT_PORT", target.GoalPortNm);
                            AppendField(request, "LOT_ID", target.LotId);
                            AppendField(request, "CARRIER_ID", target.DurableId);
                            AppendField(request, "GOAL_CARRIER_ID", target.GoalDurableId);
                            break;

                        case "OnLineRemote":
                            request.Append("JobSkipRemote");
                            AppendField(request, "EQP_GRP_ID", eqpGrpId);
                            AppendField(request, "EQP_ID", target.EqpId);
                            AppendField(request, "LOT_ID", target.LotId);
                            break;

                        case "OffLine":
                            request.Append("JobSkipOffline");
                            AppendField(request, "EQP_GRP_ID", eqpGrpId);
                            AppendField(request, "EQP_ID", target.EqpId);
                            AppendField(request, "LOT_ID", target.LotId);
                            break;

                        default:
                            throw UnknownJobBranch("JobSkip", eqpGrpId, target.CommStatTyp);
                    }

                    break;

                default:
                    throw UnknownJobBranch("JobSkip", eqpGrpId, target.CommStatTyp);
            }

            AppendField(request, "DESCRIPTION", description);
            AppendField(request, "USER_ID", userId);
            return request.ToString();
        }

        private DataActionResult JobUndo(JobExecutionTarget target, string prodId, string flowId, string operId, string description)
        {
            return this.Request(JobUndoRequest(target, prodId, flowId, operId, description, this.CurrentUserId));
        }

        private static string JobUndoRequest(
                JobExecutionTarget target, string prodId, string flowId, string operId, string description, string userId)
        {
            string eqpGrpId = target.EqpGrpId.Length == 0 ? target.SelectedEqpGrpId : target.EqpGrpId;
            StringBuilder request = new StringBuilder();
            switch (eqpGrpId)
            {
                case "GRP-A":
                case "GRP-B":
                case "GRP-C":
                case "GRP-D":
                    switch (target.CommStatTyp)
                    {
                        case "OnLineLocal":
                            request.Append("JobUndoLocal");
                            AppendField(request, "EQP_GRP_ID", eqpGrpId);
                            AppendField(request, "EQP_ID", target.EqpId);
                            AppendField(request, "IN_PORT", target.PortNm);
                            AppendField(request, "OUT_PORT", target.GoalPortNm);
                            AppendField(request, "LOT_ID", target.LotId);
                            AppendField(request, "CARRIER_ID", target.DurableId);
                            AppendField(request, "GOAL_CARRIER_ID", target.GoalDurableId);
                            break;

                        case "OnLineRemote":
                            request.Append("JobUndoRemote");
                            AppendField(request, "EQP_GRP_ID", eqpGrpId);
                            AppendField(request, "EQP_ID", target.EqpId);
                            AppendField(request, "LOT_ID", target.LotId);
                            break;

                        case "OffLine":
                            request.Append("JobUndoOffline");
                            AppendField(request, "EQP_GRP_ID", eqpGrpId);
                            AppendField(request, "EQP_ID", target.EqpId);
                            AppendField(request, "LOT_ID", target.LotId);
                            break;

                        default:
                            throw UnknownJobBranch("JobUndo", eqpGrpId, target.CommStatTyp);
                    }

                    break;

                default:
                    throw UnknownJobBranch("JobUndo", eqpGrpId, target.CommStatTyp);
            }

            AppendField(request, "PROD_ID", prodId);
            AppendField(request, "FLOW_ID", flowId);
            AppendField(request, "OPER_ID", operId);
            AppendField(request, "DESCRIPTION", description);
            AppendField(request, "USER_ID", userId);
            return request.ToString();
        }

        internal static JudgeKind JudgeKindOf(string eqpGrpId)
        {
            switch ((eqpGrpId ?? string.Empty).Trim())
            {
                case "GRP-C":
                    return JudgeKind.Chip;

                case "GRP-D":
                    return JudgeKind.Lamella;

                default:
                    return JudgeKind.None;
            }
        }

        private static ArgumentException UnknownJobBranch(string job, string eqpGrpId, string commStatTyp)
        {
            return new ArgumentException(
                    "No " + job + " message is defined for equipment group '" + eqpGrpId + "' in mode '" + commStatTyp + "'.",
                    "eqpGrpId");
        }

        private DataActionResult SetMode(DataRow equipment, string mode)
        {
            string eqpId = TableHelper.CellText(equipment, "EQP_ID").Trim();
            StringBuilder request = new StringBuilder();
            request.Append("SetEqpMode");
            AppendField(request, "EQP_ID", eqpId);
            AppendField(request, "MODE", mode);
            AppendField(request, "USER_ID", this.CurrentUserId);
            return this.Request(request.ToString());
        }

        private DataActionResult SetAuto(DataRow equipment, bool autoYn)
        {
            string eqpId = TableHelper.CellText(equipment, "EQP_ID").Trim();
            StringBuilder request = new StringBuilder();
            request.Append("SetEqpAuto");
            AppendField(request, "EQP_ID", eqpId);
            AppendField(request, "AUTO_YN", autoYn ? "Y" : "N");
            AppendField(request, "USER_ID", this.CurrentUserId);
            return this.Request(request.ToString());
        }

        // REQ_SERIAL_NO
        private DataTable GetRequests(string reqSerialNo)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetReqList");
            AppendField(request, "REQ_SERIAL_NO", reqSerialNo);
            return this.Request(request.ToString()).Table;
        }

        private DataTable GetSamples(string reqSerialNo)
        {
            StringBuilder request = new StringBuilder();
            request.Append("GetRequestInfo");
            AppendField(request, "REQ_SERIAL_NO", reqSerialNo);
            return this.Request(request.ToString()).Table;
        }

        private static void AppendField(StringBuilder request, string key, string value)
        {
            request.Append(' ').Append(key).Append('=').Append(value);
        }
    }
}
