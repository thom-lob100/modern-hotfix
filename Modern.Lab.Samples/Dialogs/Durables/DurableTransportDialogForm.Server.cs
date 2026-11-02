using System.Data;
using System.Text;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Samples.Contracts;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class DurableTransportDialogForm
    {
        // TYP_VAL
        protected virtual DataTable GetLocNmCodes()
        {
            return this.RequestFields("GetCommonCodeList",
                    "GRP_CD", "LOC_NM").Table;
        }

        // EQP_ID
        protected virtual DataTable GetEquipments(RequestParameters parameters)
        {
            return this.RequestFields("GetEqpList",
                    "LOC_NM", parameters.Value(Schemas.Durable.LocNm)).Table;
        }

        // AREA_ID
        protected virtual DataTable GetAreas()
        {
            return this.RequestFields("GetAreaList").Table;
        }

        // AREA_ID
        protected virtual DataTable GetBays(RequestParameters parameters)
        {
            return this.RequestFields("GetBayList",
                    "AREA_ID", parameters.Value(Schemas.Area.AreaId)).Table;
        }
    }
}
