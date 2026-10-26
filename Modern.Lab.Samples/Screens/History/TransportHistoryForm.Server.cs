using System.Data;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class TransportHistoryForm
    {
        // DURABLE_ID
        private DataTable GetTransportList(RequestParameters parameters)
        {
            return this.RequestFields("GetTransportList",
                    parameters.Take("DURABLE_TYP"),
                    parameters.Take(Schemas.Transport.DurableId)).Table;
        }

        // DURABLE_ID
        private DataTable GetTransportCandidates(RequestParameters parameters)
        {
            return this.RequestFields("GetTransportCandidates",
                    parameters.Take("DURABLE_TYP"),
                    parameters.Take("KEYWORD")).Table;
        }

        private DataTable GetTransportHistory(RequestParameters parameters)
        {
            return this.RequestFields("GetTransportHistory",
                    parameters.Take(Schemas.Transport.DurableId)).Table;
        }
    }
}
