using System.Data;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public sealed class DurableEntity : EntityDefinition
    {
        public readonly Field DurableId = Field("DURABLE_ID").Caption("Durable Id");
        public readonly DurableTypField DurableTyp = Field<DurableTypField>("DURABLE_TYP").Caption("Type");
        public readonly DurableStatField DurableStatCd = Field<DurableStatField>("DURABLE_STAT_CD").Caption("Mode").Badge();
        public readonly MesStatTypField MesStatTyp = Field<MesStatTypField>("MES_STAT_TYP").Caption("MES Status");
        public readonly DurableMesProcStatField MesProcStatCd = Field<DurableMesProcStatField>("MES_PROC_STAT_CD").Caption("Process").TopLabel("Mes").BottomLabel("Status").TopLabelAlignment(GridTextAlignment.Left).BottomLabelAlignment(GridTextAlignment.Right).Badge();
        public readonly WfLoadStatField WfLoadStatCd = Field<WfLoadStatField>("WF_LOAD_STAT_CD").Caption("Status").Badge();
        public readonly Field LotQty = Field("LOT_QTY").Caption("Qty");
        public readonly Field Capa = Field("CAPA").Caption("Capa");
        public readonly Field FoupLotQty = Field("FOUP_LOT_QTY");
        public readonly Field FoupCapa = Field("FOUP_CAPA");
        public readonly Field StubLotQty = Field("STUB_LOT_QTY");
        public readonly Field StubCapa = Field("STUB_CAPA");
        public readonly Field LccLotQty = Field("LCC_LOT_QTY");
        public readonly Field LccCapa = Field("LCC_CAPA");
        public readonly Field FromFac = Field("FROM_FAC").Caption("From");
        public readonly Field FrFabId = Field("FR_FAB_ID").Caption("Foreign");
        public readonly LocNmField LocNm = Field<LocNmField>("LOC_NM").Caption("Location");
        public readonly Field Location = Field("LOCATION").Caption("Location");
        public readonly Field Rfid = Field("RFID").Caption("RFID");
        public readonly LastEventCdField LastEventCd = Field<LastEventCdField>("LAST_EVENT_CD").Caption("Last Event");
        public readonly Field Description = Field("DESCRIPTION").Caption("Description");

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
