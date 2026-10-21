using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting.ResponseContracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples.Contracts
{

    public static class EquipmentLotContracts
    {
        public const string EquipmentTable = "Equipment/Lots.GetEqpList.Equipment";
        public const string PortTable = "Equipment/Lots.GetEqpPortList.Port";
        public const string ChildEquipmentTable = "Equipment/Lots.GetChildEqpList.ChildEquipment";
        public const string LotTable = "Equipment/Lots.GetJobLotList.Lot";
        public const string DurableTable = "Equipment/Lots.GetJobDurableList.Durable";
        public const string RequestTable = "Equipment/Lots.GetReqList.Request";
        public const string SampleTable = "Equipment/Lots.GetRequestInfo.Sample";

        public static ResponseContractSet Build()
        {
            return new ResponseContractSet.Builder(ColumnAliasCatalog.Empty)
                    .Table(Equipment())
                    .Table(Port())
                    .Table(ChildEquipment())
                    .Table(Lot())
                    .Table(Durable())
                    .Table(Request())
                    .Table(Sample())

                    .Build();
        }

        private static TableContract Equipment()
        {
            return new TableContract(EquipmentTable)
                    .Key(Schemas.EquipmentPriority.EqpId)
                    .Status(Schemas.EquipmentPriority.CommStatTyp)
                    .Status(Schemas.EquipmentPriority.MesEqpStatCd)
                    .Status(Schemas.EquipmentPriority.AutoYn)
                    .Numeric(Schemas.EquipmentPriority.Priority);
        }

        private static TableContract Port()
        {
            return new TableContract(PortTable)
                    .Key(Schemas.PortPriority.PortNm)
                    .Status(Schemas.PortPriority.PortTyp)
                    .Status(Schemas.PortPriority.TransferStatCd)
                    .Numeric(Schemas.PortPriority.Priority);
        }

        private static TableContract ChildEquipment()
        {
            return new TableContract(ChildEquipmentTable)
                    .Key(Schemas.EquipmentPriority.EqpId)
                    .Status(Schemas.EquipmentPriority.CommStatTyp)
                    .Status(Schemas.EquipmentPriority.MesEqpStatCd)
                    .Numeric(Schemas.EquipmentPriority.Priority);
        }

        private static TableContract Lot()
        {
            return new TableContract(LotTable)
                    .Key(Schemas.LotPriority.LotId)
                    .Standard(Schemas.LotPriority.EqpId)
                    .Standard(Schemas.LotPriority.ChildEqpId)
                    .StatusUnrestricted(Schemas.LotPriority.LastEventCd.Name)
                    .Status(Schemas.LotPriority.LotHoldStatCd)
                    .Status(Schemas.LotPriority.MesProcStatCd)
                    .Status(Schemas.LotPriority.LotStatTyp)
                    .Numeric(Schemas.LotPriority.Priority);
        }

        private static TableContract Durable()
        {
            return new TableContract(DurableTable)
                    .Key(Schemas.DurablePriority.DurableId)
                    .Status(Schemas.DurablePriority.WfLoadStatCd)
                    .Numeric(Schemas.DurablePriority.LotQty)
                    .Numeric(Schemas.DurablePriority.Priority);
        }

        private static TableContract Request()
        {
            return new TableContract(RequestTable)
                    .Key(Schemas.Request.ReqSerialNo);
        }

        private static TableContract Sample()
        {
            return new TableContract(SampleTable);
        }
    }
}
