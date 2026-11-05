using System.Data;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Report
{
    public partial class RequestReportForm
    {
        // REQ_SERIAL_NO
        // REQ_SUBSERIAL_NO
        protected override DataTable RequestReport(RequestParameters parameters)
        {
            return this.RequestFields("GetRequestReport",
                    "REQ_SERIAL_NO", parameters.Value(Schemas.Request.ReqSerialNo),
                    "REQUESTER", parameters.Value(Schemas.Request.Requester),
                    "STATE_CODE", parameters.Value(Schemas.Request.StateCode),
                    "REQ_FROM_DT", parameters.Value(Schemas.Criteria.ReqFromDt),
                    "REQ_TO_DT", parameters.Value(Schemas.Criteria.ReqToDt),
                    "ACCEPT_FROM_DT", parameters.Value(Schemas.Criteria.AcceptFromDt),
                    "ACCEPT_TO_DT", parameters.Value(Schemas.Criteria.AcceptToDt)).Table;
        }
    }
}
