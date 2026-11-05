using System.Data;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Report.Reporting;
using Modern.Lab.Report.Services;

namespace Modern.Lab.Report
{

    public partial class RequestReportForm : ReportFormBase
    {
        public RequestReportForm()
        {
            this.InitializeComponent();
            this.InitializeReport();
        }

        protected override void PrepareRows(DataTable rows)
        {
            RequestReportPresenter.AddShared(rows, LotRequestsColumn);
            RequestReportPresenter.AddBranches(
                    rows, LotHoldColumn, LotHoldValue, ExchangeTmColumn, ReturnTmColumn, SwapTmColumn, ScrapTmColumn, ScrapDueColumn,
                    RunCountColumns, JudgeColumns, Schemas.Wafer.JudgeRslt.Fail.Value);
        }
    }
}
