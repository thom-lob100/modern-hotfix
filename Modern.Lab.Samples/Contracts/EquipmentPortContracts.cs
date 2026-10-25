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
                    .Key(Schemas.EquipmentExt.EqpId)

                    .Status(Schemas.EquipmentExt.CommStatTyp)

                    .Status(Schemas.EquipmentExt.MesEqpStatCd)

                    .Status(Schemas.EquipmentExt.AutoYn);
        }

        private static TableContract Port()
        {
            return new TableContract(PortTable)
                    .Key(Schemas.PortDet.EqpId, Schemas.PortDet.PortNm)

                    .Status(Schemas.PortDet.PortTyp)

                    .Status(Schemas.PortDet.TransferStatCd)

                    .Standard(Schemas.PortDet.Mode);
        }
    }
}
