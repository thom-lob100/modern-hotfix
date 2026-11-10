using System.Data;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Durable 목록 — <c>MES_PROC_STAT_CD</c> 는 Durable 값(Idle 등)이라 Lot 사전을 쓰지 않고 이 줄에 바로 적는다.</summary>
    public sealed class DurableEntity : TableEntity
    {
        public readonly Field DurableId = DurableColumns.DurableId();
        public readonly DurableFields.DurableTyp DurableTyp = DurableColumns.DurableTyp();
        public readonly DurableFields.DurableStatCd DurableStatCd = DurableColumns.DurableStatCd();
        public readonly DurableFields.MesStatTyp MesStatTyp = DurableColumns.MesStatTyp();
        public readonly DurableFields.MesProcStatCd MesProcStatCd = Field<DurableFields.MesProcStatCd>("MES_PROC_STAT_CD").Caption("Process").TopLabel("Mes").BottomLabel("Status").TopLabelAlignment(GridTextAlignment.Left).BottomLabelAlignment(GridTextAlignment.Right).Badge();
        public readonly WaferFields.WfLoadStatCd WfLoadStatCd = WaferColumns.WfLoadStatCd();
        public readonly Field LotQty = LotColumns.LotQty();
        public readonly Field Capa = DurableColumns.Capa();
        public readonly Field FoupLotQty = Field("FOUP_LOT_QTY");
        public readonly Field FoupCapa = Field("FOUP_CAPA");
        public readonly Field StubLotQty = Field("STUB_LOT_QTY");
        public readonly Field StubCapa = Field("STUB_CAPA");
        public readonly Field LccLotQty = Field("LCC_LOT_QTY");
        public readonly Field LccCapa = Field("LCC_CAPA");
        public readonly Field FromFac = DurableColumns.FromFac();
        public readonly Field FrFabId = Field("FR_FAB_ID").Caption("Foreign");
        public readonly CommonFields.LocNm LocNm = CommonColumns.LocNm();
        public readonly Field EqpId = EquipmentColumns.EqpId();
        public readonly Field PortNm = PortColumns.PortNm();
        public readonly CommonFields.McsItgYn McsItgYn = CommonColumns.McsItgYn();
        public readonly EquipmentFields.MesStatTyp EqpMesStatTyp = Field<EquipmentFields.MesStatTyp>("EQP_MES_STAT_TYP").Caption("Eqp Mes Status");
        public readonly EquipmentFields.CommStatTyp CommStatTyp = EquipmentColumns.CommStatTyp().Caption("Eqp Mode");
        public readonly CommonFields.AcssTyp AcssTyp = CommonColumns.AcssTyp();
        public readonly PortFields.PortTyp PortTyp = PortColumns.PortTyp().Caption("Port Type");
        public readonly PortFields.PortStatNm PortStatNm = PortColumns.PortStatNm();
        public readonly PortFields.VirtPortYn VirtPortYn = PortColumns.VirtPortYn().Caption("Virtual Port");
        public readonly TransportFields.TransYn TransYn = Field<TransportFields.TransYn>("TRANS_YN").Caption("Transport");
        public readonly TransportFields.TransMoveStatCd TransMoveStatCd = Field<TransportFields.TransMoveStatCd>("TRANS_MOVE_STAT_CD").Caption("Move Status");
        public readonly TransportFields.TransLastEventDesc TransLastEventDesc = Field<TransportFields.TransLastEventDesc>("TRANS_LAST_EVENT_DESC").Caption("Transport Event");
        public readonly Field RfId = Field("RFID").Caption("RFID");
        public readonly CommonFields.LastEventCd LastEventCd = CommonColumns.LastEventCd();
        public readonly Field Description = CommonColumns.Description();

        public DurableEntity()
        {
            this.KeyFields = Fields(this.DurableId);
            this.ComboDisplayFields = Fields(this.DurableId, this.DurableTyp);
            this.ValueField = this.DurableId;
            this.Seal();
        }

        /// <summary>MES 상태 Normal 이고 진행 상태 Idle — Exchange·Edit 의 Source 와 Target 이 함께 요구하는 바탕 조건(회사 조건 2026-09-28).</summary>
        public bool IsNormalIdle(DataRow durable)
        {
            return this.MesStatTyp.Is(durable, this.MesStatTyp.Normal)
                    && this.MesProcStatCd.Is(durable, this.MesProcStatCd.Idle);
        }

        /// <summary>비어 있고 쓸 수 있다 — 적재 상태 Free 이고 지속재 상태 Available. Exchange·Edit 의 빈 Target.</summary>
        public bool IsFreeAvailable(DataRow durable)
        {
            return this.WfLoadStatCd.Is(durable, this.WfLoadStatCd.Free)
                    && this.DurableStatCd.Is(durable, this.DurableStatCd.Available);
        }

        /// <summary>일부 차 있고 사용 중 — 적재 상태 Partial 이고 지속재 상태 InUse. Edit 의 Target.</summary>
        public bool IsPartialInUse(DataRow durable)
        {
            return this.WfLoadStatCd.Is(durable, this.WfLoadStatCd.Partial)
                    && this.DurableStatCd.Is(durable, this.DurableStatCd.InUse);
        }

        /// <summary>내용물이 있고 사용 중 — 적재 상태 Full 또는 Partial 이고 지속재 상태 InUse. Exchange·Edit 의 Source.</summary>
        public bool IsLoadedInUse(DataRow durable)
        {
            return (this.WfLoadStatCd.Is(durable, this.WfLoadStatCd.Full)
                            || this.WfLoadStatCd.Is(durable, this.WfLoadStatCd.Partial))
                    && this.DurableStatCd.Is(durable, this.DurableStatCd.InUse);
        }
    }
}
