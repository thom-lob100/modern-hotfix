using System;
using System.Collections.Generic;
using System.Data;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Display;
using Modern.Lab.Hosting.ResponseContracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.Samples.Contracts;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples.Services
{

    public static class EquipmentLotPresenter
    {
        public const string ActionJobPrep = "JobPrep";
        public const string ActionJobStart = "JobStart";
        public const string ActionJobEnd = "JobEnd";
        public const string ActionJobPrepCancel = "JobPrepCancel";
        public const string ActionJobStartCancel = "JobStartCancel";
        public const string ActionSkipOper = "JobSkipOper";
        public const string ActionBackToPrevOper = "JobBackToPrevOper";
        public const string CancelWord = "Cancel";
        public const string JobMenuSeparatorBefore = ActionSkipOper;
        public const string ChildNoPriorityReason = "This child equipment has no priority (Run or Down).";
        public const string ChildRunningReason = "The running job uses the highlighted child equipment.";
        public const string ChildNoPriorityBadge = "\u2013";

        public static readonly string PriorityColumn = Schemas.LotPriority.Priority.Name;

        public static readonly string JobStatePrep = Schemas.LotPriority.LastEventCd.JobPrep.Value;

        public static readonly string JobStateReserved = Schemas.LotPriority.LastEventCd.Reserved.Value;
        public static readonly string JobStateStart = Schemas.LotPriority.LastEventCd.JobStart.Value;

        public const string JobColorColumn = "JOB_COLOR";
        public const string AutoCanColumn = "AUTO_CAN";
        public const string PriorityColorColumn = "PRIORITY_COLOR";
        public const string ChildEqpIdAliasColumn = "CHILD_EQP_ID_ALIAS";

        public const string ServerOrderColumn = "SERVER_ORDER";

        public const string PrioritySpinColumn = "PRIORITY_SPIN";

        public static readonly IList<string> ScreenColumns = new List<string>
        {
            AutoCanColumn, JobColorColumn, PriorityColorColumn, PrioritySpinColumn, ServerOrderColumn
        }.AsReadOnly();

        public const int DefaultIntervalSeconds = 60;

        public static readonly int[] IntervalChoices = new int[] { 0, 30, 60 };

        public static readonly IList<LotAction> JobActions =
                new List<LotAction>
                {
                    new LotAction(ActionJobPrep, "JobPrep"),
                    new LotAction(ActionJobPrepCancel, "JobPrep Cancel"),
                    new LotAction(ActionJobStart, "JobStart"),
                    new LotAction(ActionJobStartCancel, "JobStart Cancel"),
                    new LotAction(ActionJobEnd, "JobEnd"),
                    new LotAction(ActionSkipOper, "Skip Oper"),
                    new LotAction(ActionBackToPrevOper, "Back to Prev Oper")
                }.AsReadOnly();

        public static string JobColorSignature(DataTable lots)
        {
            if (lots == null)
            {
                return string.Empty;
            }

            System.Text.StringBuilder signature = new System.Text.StringBuilder();

            foreach (DataRow row in lots.Rows)
            {
                signature.Append(TableHelper.CellText(row, Schemas.LotPriority.LotId.Name)).Append('=').Append(JobState(row)).Append(';');
            }

            return signature.ToString();
        }

        public static bool IsJobKey(string key)
        {
            return key != null && JobKeys.Contains(key);
        }

        public static bool IsCandidate(DataRow row)
        {
            return Priority(row) > 0;
        }

        internal sealed class PortsFitField : Field
        {
            public readonly FieldValue Fit = Value("Y");
        }

        internal sealed class ChildFitField : Field
        {
            public readonly FieldValue Fit = Value("Y");
        }

        internal sealed class DecisionRuleEntity : EntityDefinition
        {
            public readonly CommStatTypField DecidedEqpMode = Field<CommStatTypField>("DECIDED_COMM_STAT_TYP").Caption("Mode");
            public readonly CommStatTypField JobEqpMode = Field<CommStatTypField>("JOB_COMM_STAT_TYP").Caption("Mode");
            public readonly PortsFitField PortsFit = Field<PortsFitField>("PORTS_FIT").Caption("Ports Fit");
            public readonly ChildFitField ChildFit = Field<ChildFitField>("CHILD_FIT").Caption("Child Eqp Fit");
            public readonly CommStatTypField ChildEqpMode = Field<CommStatTypField>("CHILD_COMM_STAT_TYP").Caption("Mode");
            public readonly MesEqpStatField ChildEqpStatus = Field<MesEqpStatField>("CHILD_MES_EQP_STAT_CD").Caption("Status");
        }

        internal static readonly DecisionRuleEntity DecisionRule = new DecisionRuleEntity();

        public static DataRow RuleRow(JobDecision decision)
        {
            DataTable table = new DataTable("DecisionRule");
            table.Locale = System.Globalization.CultureInfo.InvariantCulture;
            DataColumn lotId = table.Columns.Add(Schemas.LotPriority.LotId.Name, typeof(string));
            table.Columns.Add(Schemas.LotPriority.LastEventCd.Name, typeof(string));
            table.Columns.Add(PriorityColumn, typeof(string));
            table.Columns.Add(DecisionRule.DecidedEqpMode.Name, typeof(string));
            table.Columns.Add(DecisionRule.JobEqpMode.Name, typeof(string));
            table.Columns.Add(Schemas.LotPriority.PortNm.Name, typeof(string));
            table.Columns.Add(Schemas.LotPriority.GoalPortNm.Name, typeof(string));
            table.Columns.Add(Schemas.LotPriority.GoalDurableId.Name, typeof(string));
            table.Columns.Add(Schemas.LotPriority.LotProcStatCd.Name, typeof(string));
            table.Columns.Add(Schemas.LotPriority.MesProcStatCd.Name, typeof(string));
            table.Columns.Add(Schemas.LotPriority.LotHoldStatCd.Name, typeof(string));
            table.Columns.Add(Schemas.LotPriority.LotStatTyp.Name, typeof(string));
            table.Columns.Add(DecisionRule.PortsFit.Name, typeof(string));
            table.Columns.Add(DecisionRule.ChildFit.Name, typeof(string));
            table.Columns.Add(DecisionRule.ChildEqpMode.Name, typeof(string));
            table.Columns.Add(DecisionRule.ChildEqpStatus.Name, typeof(string));
            table.PrimaryKey = new DataColumn[] { lotId };

            DataRow lot = decision == null ? null : decision.Lot;
            DataRow decidedEquipment = decision == null ? null : decision.Equipment;
            DataRow jobEquipment = HasActiveJob(lot) ? JobEquipment(decision) : null;
            DataRow summary = JobSummary(decision);
            DataRow row = table.NewRow();
            row[Schemas.LotPriority.LotId.Name] = decision == null ? string.Empty : decision.LotId;
            row[Schemas.LotPriority.LastEventCd.Name] = JobState(lot);
            row[PriorityColumn] = JobDecision.IsLive(lot) ? TableHelper.CellText(lot, PriorityColumn).Trim() : string.Empty;
            row[DecisionRule.DecidedEqpMode.Name] = LiveText(decidedEquipment, Schemas.EquipmentPriority.CommStatTyp.Name);
            row[DecisionRule.JobEqpMode.Name] = LiveText(jobEquipment, Schemas.EquipmentPriority.CommStatTyp.Name);
            row[Schemas.LotPriority.PortNm.Name] = TableHelper.CellText(summary, Schemas.LotPriority.PortNm.Name).Trim();
            row[Schemas.LotPriority.GoalPortNm.Name] = TableHelper.CellText(summary, Schemas.LotPriority.GoalPortNm.Name).Trim();
            row[Schemas.LotPriority.GoalDurableId.Name] = DecidedGoalDurableId(decision);
            row[Schemas.LotPriority.LotProcStatCd.Name] = LiveText(lot, Schemas.LotPriority.LotProcStatCd.Name);
            row[Schemas.LotPriority.MesProcStatCd.Name] = LiveText(lot, Schemas.LotPriority.MesProcStatCd.Name);
            row[Schemas.LotPriority.LotHoldStatCd.Name] = LiveText(lot, Schemas.LotPriority.LotHoldStatCd.Name);
            row[Schemas.LotPriority.LotStatTyp.Name] = LiveText(lot, Schemas.LotPriority.LotStatTyp.Name);
            row[DecisionRule.PortsFit.Name] = decision != null && PortsCompatible(decision.Port, decision.GoalPort) ? DecisionRule.PortsFit.Fit.Value : string.Empty;
            row[DecisionRule.ChildFit.Name] = ChildFits(decision) ? DecisionRule.ChildFit.Fit.Value : string.Empty;
            DataRow workSite = WorkSite(decision, decidedEquipment);
            row[DecisionRule.ChildEqpMode.Name] = LiveText(workSite, Schemas.EquipmentPriority.CommStatTyp.Name);
            row[DecisionRule.ChildEqpStatus.Name] = LiveText(workSite, Schemas.EquipmentPriority.MesEqpStatCd.Name);
            table.Rows.Add(row);
            return row;
        }

        private static DataRow WorkSite(JobDecision decision, DataRow decidedEquipment)
        {
            if (decision == null)
            {
                return null;
            }

            if (decision.ChildRequired)
            {
                return decision.ChildEquipment;
            }

            return decidedEquipment;
        }

        public static string ChildLabel(string childEqpId, string alias)
        {
            string id = (childEqpId ?? string.Empty).Trim();
            string shortName = (alias ?? string.Empty).Trim();
            return shortName.Length == 0 ? id : id + " · " + shortName;
        }

        public static bool ChildFits(JobDecision decision)
        {
            if (decision == null)
            {
                return false;
            }

            if (HasActiveJob(decision.Lot))
            {
                return true;
            }

            return !decision.ChildRequired || decision.ChildEqpId.Length > 0;
        }


        public static int Priority(DataRow row)
        {
            return TableHelper.ParseInt(TableHelper.CellText(row, PriorityColumn));
        }

        public static string JobState(DataRow lot)
        {
            return JobDecision.IsLive(lot) ? TableHelper.CellText(lot, Schemas.LotPriority.LastEventCd.Name).Trim() : string.Empty;
        }

        public static string PortTyp(DataRow port)
        {
            return TableHelper.CellText(port, Schemas.PortPriority.PortTyp.Name).Trim();
        }

        public static string PortDurableTyp(DataRow port)
        {
            return TableHelper.CellText(port, Schemas.PortPriority.DurableTyp.Name).Trim();
        }

        public static string JobGoalPortNm(DataRow lot)
        {
            return HasActiveJob(lot) ? TableHelper.CellText(lot, Schemas.LotPriority.GoalPortNm.Name).Trim() : string.Empty;
        }

        public static string DurableListTitle(string durableType)
        {
            return durableType.Length == 0 ? "Durable List (Target)" : "Durable List (Target) — " + durableType;
        }

        public static DataRow TopPriority(DataTable table)
        {
            if (table == null || table.Rows.Count == 0)
            {
                return null;
            }

            DataRow first = null;
            DataRow best = null;

            foreach (DataRow row in table.Rows)
            {
                if (!IsCandidate(row))
                {
                    continue;
                }

                if (Priority(row) == 1)
                {
                    if (first == null || ServerOrder(row) < ServerOrder(first))
                    {
                        first = row;
                    }

                    continue;
                }

                if (best == null || ComesBefore(row, best))
                {
                    best = row;
                }
            }

            return first ?? best;
        }

        private static bool ComesBefore(DataRow row, DataRow other)
        {
            int priority = Priority(row);
            int otherPriority = Priority(other);
            return priority < otherPriority || (priority == otherPriority && ServerOrder(row) < ServerOrder(other));
        }

        private static int ServerOrder(DataRow row)
        {
            if (!row.Table.Columns.Contains(ServerOrderColumn) || row.IsNull(ServerOrderColumn))
            {
                return row.Table.Rows.IndexOf(row);
            }

            return Convert.ToInt32(row[ServerOrderColumn]);
        }

        public static DataRow TopPort(DataTable ports, string portTyp)
        {
            if (ports == null)
            {
                return null;
            }

            DataRow best = null;

            foreach (DataRow row in ports.Rows)
            {
                string rowPortTyp = PortTyp(row);

                if (rowPortTyp != portTyp && rowPortTyp != Schemas.PortPriority.PortTyp.InputOutput.Value)
                {
                    continue;
                }

                if (!IsCandidate(row))
                {
                    continue;
                }

                if (best == null || ComesBefore(row, best))
                {
                    best = row;
                }
            }

            return best;
        }

        public static DataRow FindById(DataTable table, string column, string id)
        {
            if (table == null || string.IsNullOrEmpty(id))
            {
                return null;
            }

            foreach (DataRow row in table.Rows)
            {
                if (TableHelper.CellText(row, column).Trim() == id)
                {
                    return row;
                }
            }

            return null;
        }

        public static string JobGoalDurableId(DataRow lot)
        {
            return HasActiveJob(lot) ? TableHelper.CellText(lot, Schemas.LotPriority.GoalDurableId.Name).Trim() : string.Empty;
        }

        public static string DecidedGoalDurableId(JobDecision decision)
        {
            if (decision == null)
            {
                return string.Empty;
            }

            return HasActiveJob(decision.Lot) ? JobGoalDurableId(decision.Lot) : decision.GoalDurableId;
        }

        public static void OverwriteJobLot(DataRow lot, DataRow fresh)
        {
            lot.BeginEdit();

            foreach (DataColumn column in fresh.Table.Columns)
            {
                string name = column.ColumnName;

                if (name == Schemas.LotPriority.Priority.Name || !lot.Table.Columns.Contains(name))
                {
                    continue;
                }

                lot[name] = fresh[name];
            }

            lot.EndEdit();
        }

        private static string LiveText(DataRow row, string column)
        {
            return JobDecision.IsLive(row) ? TableHelper.CellText(row, column).Trim() : string.Empty;
        }

        public static bool HasActiveJob(DataRow lot)
        {
            string state = JobState(lot);
            return state == JobStatePrep || state == JobStateReserved || state == JobStateStart;
        }

        public static DataRow JobEquipment(JobDecision decision)
        {
            if (decision == null)
            {
                return null;
            }

            if (!HasActiveJob(decision.Lot))
            {
                return decision.Equipment;
            }

            string eqpId = TableHelper.CellText(decision.Lot, Schemas.LotPriority.EqpId.Name).Trim();

            if (eqpId.Length == 0)
            {
                return null;
            }

            if (decision.Equipment != null
                    && TableHelper.CellText(decision.Equipment, Schemas.EquipmentPriority.EqpId.Name).Trim() == eqpId)
            {
                return decision.Equipment;
            }

            return FindById(decision.EquipmentList, Schemas.EquipmentPriority.EqpId.Name, eqpId);
        }

        public static bool PortsCompatible(DataRow port, DataRow goalPort)
        {
            if (port == null || goalPort == null)
            {
                return false;
            }

            string portTyp = PortTyp(port);
            string goalPortTyp = PortTyp(goalPort);

            if (TableHelper.CellText(port, Schemas.PortPriority.PortNm.Name).Trim() == TableHelper.CellText(goalPort, Schemas.PortPriority.PortNm.Name).Trim())
            {
                return portTyp == Schemas.PortPriority.PortTyp.InputOutput.Value;
            }

            return (portTyp == Schemas.PortPriority.PortTyp.Input.Value || portTyp == Schemas.PortPriority.PortTyp.InputOutput.Value)
                    && (goalPortTyp == Schemas.PortPriority.PortTyp.Output.Value || goalPortTyp == Schemas.PortPriority.PortTyp.InputOutput.Value);
        }

        public static DataRow JobSummary(JobDecision decision)
        {
            return JobTargetSummary(decision == null ? null : new JobExecutionTarget(decision, string.Empty));
        }

        public static DataRow JobTargetSummary(JobExecutionTarget target)
        {
            DataTable table = new DataTable();
            table.Columns.Add(Schemas.EquipmentPriority.EqpGrpId.Name);
            table.Columns.Add(Schemas.EquipmentPriority.EqpId.Name);
            table.Columns.Add(Schemas.EquipmentPriority.CommStatTyp.Name);
            table.Columns.Add(Schemas.LotPriority.PortNm.Name);
            table.Columns.Add(Schemas.LotPriority.GoalPortNm.Name);
            table.Columns.Add(Schemas.LotPriority.ChildEqpId.Name);
            table.Columns.Add(ChildEqpIdAliasColumn);
            table.Columns.Add(Schemas.LotPriority.LotId.Name);
            table.Columns.Add(Schemas.LotPriority.DurableId.Name);
            table.Columns.Add(Schemas.LotPriority.GoalDurableId.Name);
            table.Columns.Add(Schemas.DurablePriority.DurableTyp.Name);
            table.Columns.Add(Schemas.LotPriority.LastEventCd.Name);

            DataRow row = table.NewRow();

            if (target != null)
            {
                row[Schemas.EquipmentPriority.EqpGrpId.Name] = target.EqpGrpId;
                row[Schemas.EquipmentPriority.EqpId.Name] = target.EqpId;
                row[Schemas.EquipmentPriority.CommStatTyp.Name] = target.CommStatTyp;
                row[Schemas.LotPriority.PortNm.Name] = target.PortNm;
                row[Schemas.LotPriority.GoalPortNm.Name] = target.GoalPortNm;
                row[Schemas.LotPriority.ChildEqpId.Name] = target.ChildEqpId;
                row[ChildEqpIdAliasColumn] = target.ChildEqpIdAlias;
                row[Schemas.LotPriority.LotId.Name] = target.LotId;
                row[Schemas.LotPriority.DurableId.Name] = target.DurableId;
                row[Schemas.LotPriority.GoalDurableId.Name] = target.GoalDurableId;
                row[Schemas.DurablePriority.DurableTyp.Name] = target.GoalDurableTyp;
                row[Schemas.LotPriority.LastEventCd.Name] = target.LastEventCd;
            }

            table.Rows.Add(row);
            return row;
        }

        public static string JobLabel(string key)
        {
            switch (key)
            {
                default:
                    foreach (LotAction action in JobActions)
                    {
                        if (action.Key == key)
                        {
                            return action.Label;
                        }
                    }

                    return string.Empty;
            }
        }

        public const string PriorityFirstColor = "#F87171";
        public const string PrioritySecondColor = "#FB923C";
        public const string PriorityThirdColor = "#FACC15";
        public const string PriorityNextColor = "#F2E3A6";
        public const string PriorityRestColor = "#F3F4F6";

        public static string PriorityColor(int priority)
        {
            switch (priority)
            {
                case 1:
                    return PriorityFirstColor;

                case 2:
                    return PrioritySecondColor;

                case 3:
                    return PriorityThirdColor;

                case 4:
                case 5:
                    return PriorityNextColor;

                default:
                    return PriorityRestColor;
            }
        }

        public static DataTable MarkPriorityColors(DataTable table)
        {
            if (table == null || !table.Columns.Contains(PriorityColumn))
            {
                return table;
            }

            if (!table.Columns.Contains(PriorityColorColumn))
            {
                table.Columns.Add(PriorityColorColumn, typeof(string));
            }

            foreach (DataRow row in table.Rows)
            {
                row[PriorityColorColumn] = IsCandidate(row) ? PriorityColor(Priority(row)) : string.Empty;
            }

            return table;
        }

        public static DataTable MarkServerOrder(DataTable table, DataTable source, string keyColumn)
        {
            if (table == null)
            {
                return null;
            }

            if (!table.Columns.Contains(ServerOrderColumn))
            {
                table.Columns.Add(ServerOrderColumn, typeof(int));
            }

            if (source == null || string.IsNullOrEmpty(keyColumn))
            {
                for (int index = 0; index < table.Rows.Count; index++)
                {
                    SetServerOrder(table.Rows[index], index);
                }

                return table;
            }

            Dictionary<string, int> order = new Dictionary<string, int>(StringComparer.Ordinal);

            for (int index = 0; index < source.Rows.Count; index++)
            {
                string key = TableHelper.CellText(source.Rows[index], keyColumn).Trim();

                if (key.Length > 0 && !order.ContainsKey(key))
                {
                    order.Add(key, index);
                }
            }

            foreach (DataRow row in table.Rows)
            {
                int position;

                if (order.TryGetValue(TableHelper.CellText(row, keyColumn).Trim(), out position))
                {
                    SetServerOrder(row, position);
                }
            }

            return table;
        }

        private static void SetServerOrder(DataRow row, int position)
        {
            if (row.IsNull(ServerOrderColumn) || Convert.ToInt32(row[ServerOrderColumn]) != position)
            {
                row[ServerOrderColumn] = position;
            }
        }


        public static void LotIdCardLines(string lotId, out string value, out string bottomLabel)
        {
            string id = (lotId ?? string.Empty).Trim();
            value = id;
            bottomLabel = string.Empty;

        }

        public static string LotFlowOper(DataRow lot)
        {
            if (!JobDecision.IsLive(lot))
            {
                return string.Empty;
            }

            string flow = TableHelper.CellText(lot, Schemas.LotPriority.FlowId.Name).Trim();
            string oper = TableHelper.CellText(lot, Schemas.LotPriority.OperId.Name).Trim();

            if (flow.Length > 0 && oper.Length > 0)
            {
                return flow + " · " + oper;
            }

            return flow.Length > 0 ? flow : oper;
        }

        public static DataTable MarkAutoCan(DataTable equipments)
        {
            if (equipments == null)
            {
                return null;
            }

            if (!equipments.Columns.Contains(AutoCanColumn))
            {
                equipments.Columns.Add(AutoCanColumn, typeof(bool));
            }

            foreach (DataRow row in equipments.Rows)
            {
                row[AutoCanColumn] = EquipmentPortManagementForm.EquipmentActionReason(EquipmentPortManagementPresenter.ActionAuto, row) == null;
            }

            return equipments;
        }

        public static DataTable MarkJobColors(DataTable lots)
        {
            if (lots == null)
            {
                return null;
            }

            if (!lots.Columns.Contains(JobColorColumn))
            {
                lots.Columns.Add(JobColorColumn, typeof(string));
            }

            if (!lots.Columns.Contains(PrioritySpinColumn))
            {
                lots.Columns.Add(PrioritySpinColumn, typeof(string));
            }

            System.Drawing.Color surface = Modern.Lab.Theming.ModernTokenColors.Get(
                    "Brush.Surface", System.Drawing.Color.White);
            string runColor = System.Drawing.ColorTranslator.ToHtml(Modern.Lab.Theming.ModernTokenColors.Blend(
                    Modern.Lab.Theming.ModernTokenColors.Get("Brush.Accent", Modern.Lab.Theming.ModernTheme.Accent), surface, 0.16));
            string prepColor = System.Drawing.ColorTranslator.ToHtml(Modern.Lab.Theming.ModernTokenColors.Blend(
                    Modern.Lab.Theming.ModernTokenColors.Get("Brush.WarningBorder", System.Drawing.Color.FromArgb(217, 119, 6)), surface, 0.18));

            foreach (DataRow row in lots.Rows)
            {
                string state = JobState(row);
                row[JobColorColumn] = state == JobStateStart ? runColor : (state == JobStateReserved || state == JobStatePrep ? prepColor : string.Empty);
                row[PrioritySpinColumn] = state == JobStateStart && IsCandidate(row) ? "Y" : string.Empty;
            }

            return lots;
        }

        public static string TargetText(JobDecision decision)
        {
            DataRow equipment = JobEquipment(decision);

            if (equipment == null)
            {
                return "No equipment decided";
            }

            string text = TableHelper.CellText(equipment, Schemas.EquipmentPriority.EqpId.Name).Trim()
                    + " · " + EquipmentPortManagementPresenter.Mode(equipment);

            DataRow summary = JobSummary(decision);
            bool job = HasActiveJob(decision.Lot);
            string portNm = TableHelper.CellText(summary, Schemas.LotPriority.PortNm.Name);
            string goalPortNm = TableHelper.CellText(summary, Schemas.LotPriority.GoalPortNm.Name);

            if (job ? (portNm.Length > 0 || goalPortNm.Length > 0) : (decision.Port != null || decision.GoalPort != null))
            {
                text = text + " · Port " + (portNm.Length > 0 ? portNm : "?")
                        + " → " + (goalPortNm.Length > 0 ? goalPortNm : "?");
            }

            if (decision.Lot != null)
            {
                text = text + " · " + decision.LotId;
                string state = JobState(decision.Lot);

                if (state.Length > 0)
                {
                    text = text + " (" + state + ")";
                }
            }

            string goalDurableId = TableHelper.CellText(summary, Schemas.LotPriority.GoalDurableId.Name);

            if (goalDurableId.Length > 0)
            {
                text = text + " · " + goalDurableId;
            }

            return text;
        }

        public static readonly IList<string> JobKeys =
                new List<string>
                {
                    ActionJobPrep, ActionJobPrepCancel, ActionJobStart, ActionJobStartCancel, ActionJobEnd, ActionSkipOper, ActionBackToPrevOper
                }.AsReadOnly();

        public static string StatusLineText(JobDecision decision)
        {
            return TargetText(decision);
        }

    }
}
