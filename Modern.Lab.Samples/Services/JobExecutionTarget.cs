using System.Data;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples.Services
{
    public sealed class JobExecutionTarget
    {
        public string EqpGrpId { get; private set; }

        public string SelectedEqpGrpId { get; private set; }

        public string EqpId { get; private set; }

        public string CommStatTyp { get; private set; }

        public string PortNm { get; private set; }

        public string GoalPortNm { get; private set; }

        public string LotId { get; private set; }

        public string DurableId { get; private set; }

        public string GoalDurableId { get; private set; }

        public string GoalDurableTyp { get; private set; }

        public string LastEventCd { get; private set; }

        public string OperId { get; private set; }

        public JobExecutionTarget(JobDecision decision, string selectedEqpGrpId)
        {
            DataRow lot = decision.Lot;
            bool job = EquipmentLotPresenter.HasActiveJob(lot);
            DataRow equipment = EquipmentLotPresenter.JobEquipment(decision);
            this.EqpGrpId = LotText(equipment, Schemas.EquipmentPriority.EqpGrpId.Name);
            this.SelectedEqpGrpId = selectedEqpGrpId ?? string.Empty;
            this.EqpId = job ? LotText(lot, Schemas.LotPriority.EqpId.Name) : decision.EqpId;
            this.CommStatTyp = equipment == null ? string.Empty : EquipmentPortManagementPresenter.Mode(equipment);
            this.PortNm = job ? LotText(lot, Schemas.LotPriority.PortNm.Name) : decision.PortNm;
            this.GoalPortNm = job ? LotText(lot, Schemas.LotPriority.GoalPortNm.Name) : decision.GoalPortNm;
            this.LotId = decision.LotId;
            this.DurableId = decision.DurableId;
            this.GoalDurableId = job ? LotText(lot, Schemas.LotPriority.GoalDurableId.Name) : decision.GoalDurableId;
            this.GoalDurableTyp = JobDecision.IsLive(decision.GoalDurable)
                    && (!job || decision.GoalDurableId == this.GoalDurableId)
                    ? TableHelper.CellText(decision.GoalDurable, Schemas.DurablePriority.DurableTyp.Name).Trim()
                    : string.Empty;
            this.LastEventCd = EquipmentLotPresenter.JobState(lot);
            this.OperId = LotText(lot, Schemas.LotPriority.OperId.Name);
        }

        private static string LotText(DataRow lot, string column)
        {
            return JobDecision.IsLive(lot) ? TableHelper.CellText(lot, column).Trim() : string.Empty;
        }
    }
}
