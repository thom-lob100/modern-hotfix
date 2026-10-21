namespace Modern.Lab.Samples.Services
{
    public static class DefaultDescriptions
    {
        public const string JobPrepare = "Manual JobPrepare";
        public const string JobStart = "Manual JobStart";
        public const string JobEnd = "Manual JobEnd";
        public const string JobPrepareCancel = "Manual JobPrepare Cancel";
        public const string JobStartCancel = "Manual JobStart Cancel";
        public const string BackToPrevOper = "Manual Back to Prev Oper";
        public const string CancelOper = "Manual Cancel Oper";
        public const string CreateLot = "Manual Create Lot";
        public const string CreateChip = "Manual Create Chip";
        public const string CreateLamella = "Manual Create Lamella";
        public const string HoldLot = "Manual Hold Lot";
        public const string ReleaseLot = "Manual Release Lot";
        public const string ScrapLot = "Manual Scrap Lot";
        public const string ChangeLotSpec = "Manual Change Lot Spec";
        public const string ChangeLotSpecCancel = "Manual ChangeSpec Cancel";
        public const string ScrapLotCancel = "Manual Scrap Cancel";
        public const string ReceiveLot = "Manual Receive Lot";
        public const string CreateDurable = "Manual Create Durable";
        public const string TransportDurable = "Manual Transport Durable";
        public const string AssignRfId = "Manual Assign RFID";
        public const string HoldDurable = "Manual Hold Durable";
        public const string ReleaseDurable = "Manual Release Durable";
        public const string AdjustDurable = "Manual Adjust Durable";
        public const string ScrapDurable = "Manual Scrap Durable";
        public const string MoveWafer = "Manual Move Wafer";
        public const string MoveChipLamella = "Manual Move Chip / Lamella";
        public const string ExchangeDurable = "Manual Exchange Durable";
        public const string CreateLogistics = "Manual Create Logistics";
        public const string ReceiveLogistics = "Manual Receive Logistics";

        public static string ForJob(string actionKey)
        {
            switch (actionKey)
            {
                case EquipmentLotPresenter.ActionJobPrep:
                    return JobPrepare;

                case EquipmentLotPresenter.ActionJobStart:
                    return JobStart;

                case EquipmentLotPresenter.ActionJobEnd:
                    return JobEnd;

                case EquipmentLotPresenter.ActionJobPrepCancel:
                    return JobPrepareCancel;

                case EquipmentLotPresenter.ActionJobStartCancel:
                    return JobStartCancel;

                case EquipmentLotPresenter.ActionCancelOper:
                    return CancelOper;

                case EquipmentLotPresenter.ActionBackToPrevOper:
                    return BackToPrevOper;

                default:
                    return string.Empty;
            }
        }
    }
}
