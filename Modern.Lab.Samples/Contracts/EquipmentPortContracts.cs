using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting.ResponseContracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples.Contracts
{

    public static class EquipmentPortContracts
    {
        public const string EquipmentTable = "Equipment/Port.GetEqpList.Equipment";
        public const string PortTable = "Equipment/Port.GetEqpPortList.Port";

        public static ResponseContractSet Build()
        {
            return new ResponseContractSet.Builder(ColumnAliasCatalog.Empty)
                    .Table(Equipment())
                    .Table(Port())

                    .Build();
        }

        private static TableContract Equipment()
        {
            return new TableContract(EquipmentTable)
                    .Key(Schemas.Equipment.EqpId)

                    .Status(Schemas.Equipment.CommStatTyp)

                    .Status(Schemas.Equipment.MesEqpStatCd)

                    .Status(Schemas.Equipment.AutoYn);
        }

        private static TableContract Port()
        {
            return new TableContract(PortTable)
                    .Key(Schemas.Port.EqpId, Schemas.Port.PortNm)

                    .Status(Schemas.Port.PortTyp)

                    .Status(Schemas.Port.TransferStatCd)

                    .Standard(Schemas.Port.Mode);
        }
    }
}
