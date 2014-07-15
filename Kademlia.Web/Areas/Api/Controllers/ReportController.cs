using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using PSOK.Kademlia.Web.Areas.Api.Models;
using PSOK.PublishSubscribe.Reports;

namespace PSOK.Kademlia.Web.Areas.Api.Controllers
{
    /// <summary>
    /// A controller for retrieving reports.
    /// </summary>
    public class ReportController : ApiController
    {
        /// <summary>
        /// Retrieves all <see cref="IStatusReport" />s as <see cref="StatusReportViewModel" />s.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<StatusReportViewModel> Get()
        {
            IEnumerable<IStatusReport> reports = StatusReport.GetRemoteReports();
            return reports.Select(x => new StatusReportViewModel
            {
                Id = x.AppId,
                Header = x.GetHeader(),
                Status = x.AppStatus.ToString().ToLower(),
                Entries = x.GetEntries().Select(y => new StatusReportEntryViewModel
                {
                    Description = y.Description,
                    Value = y.Value
                }).ToList()
            }).ToList();
        }
    }
}