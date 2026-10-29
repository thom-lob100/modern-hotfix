using System.Data;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples.Services
{
    public sealed class JobDecision
    {
        private DataRow equipment;
        private DataRow port;
        private DataRow goalPort;
        private DataRow childEquipment;
        private DataRow lot;
        private DataRow goalDurable;

        public DataRow Equipment
        {
            get { return this.equipment; }
            set
            {
                this.equipment = value;
                this.EquipmentMissing = false;
            }
        }

        public DataTable EquipmentList { get; set; }

        public DataRow Port
        {
            get { return this.port; }
            set
            {
                this.port = value;
                this.PortMissing = false;
            }
        }

        public DataRow GoalPort
        {
            get { return this.goalPort; }
            set
            {
                this.goalPort = value;
                this.GoalPortMissing = false;
            }
        }

        public DataRow ChildEquipment
        {
            get { return this.childEquipment; }
            set
            {
                this.childEquipment = value;
                this.ChildEquipmentMissing = false;
            }
        }

        public DataTable ChildEquipmentList { get; set; }

        public DataRow Lot
        {
            get { return this.lot; }
            set
            {
                this.lot = value;
                this.LotMissing = false;
            }
        }

        public DataRow GoalDurable
        {
            get { return this.goalDurable; }
            set
            {
                this.goalDurable = value;
                this.GoalDurableMissing = false;
            }
        }

        public bool EquipmentMissing { get; set; }

        public bool PortMissing { get; set; }

        public bool GoalPortMissing { get; set; }

        public bool ChildEquipmentMissing { get; set; }

        public bool LotMissing { get; set; }

        public bool GoalDurableMissing { get; set; }

        public bool MaterialsPending { get; set; }

        public string EqpId
        {
            get { return Cell(this.Equipment, Schemas.EquipmentPriority.EqpId.Name); }
        }

        public string PortNm
        {
            get { return Cell(this.Port, Schemas.PortPriority.PortNm.Name); }
        }

        public string GoalPortNm
        {
            get { return Cell(this.GoalPort, Schemas.PortPriority.PortNm.Name); }
        }

        public string ChildEqpId
        {
            get { return Cell(this.ChildEquipment, Schemas.EquipmentPriority.EqpId.Name); }
        }

        public string ChildEqpIdAlias
        {
            get { return Cell(this.ChildEquipment, Schemas.EquipmentPriority.EqpIdAlias.Name); }
        }

        public bool ChildRequired
        {
            get { return this.ChildEquipmentList != null && this.ChildEquipmentList.Rows.Count > 0; }
        }

        public string LotId
        {
            get { return Cell(this.Lot, Schemas.LotPriority.LotId.Name); }
        }

        public string DurableId
        {
            get { return Cell(this.Lot, Schemas.LotPriority.DurableId.Name); }
        }

        public string GoalDurableId
        {
            get { return Cell(this.GoalDurable, Schemas.DurablePriority.DurableId.Name); }
        }

        public void DropDetached()
        {
            this.Equipment = Live(this.Equipment);
            this.Port = Live(this.Port);
            this.GoalPort = Live(this.GoalPort);
            this.ChildEquipment = Live(this.ChildEquipment);
            this.Lot = Live(this.Lot);
            this.GoalDurable = Live(this.GoalDurable);
        }

        public static bool IsLive(DataRow row)
        {
            return row != null && row.RowState != DataRowState.Detached && row.RowState != DataRowState.Deleted;
        }

        private static DataRow Live(DataRow row)
        {
            return IsLive(row) ? row : null;
        }

        private static string Cell(DataRow row, string column)
        {
            return IsLive(row) ? TableHelper.CellText(row, column).Trim() : string.Empty;
        }
    }
}
