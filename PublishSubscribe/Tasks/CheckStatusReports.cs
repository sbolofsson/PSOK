using System.Collections.Generic;
using PSOK.PublishSubscribe.Reports;

namespace PSOK.PublishSubscribe.Tasks
{
    /// <summary>
    /// Checks the status for all <see cref="IStatusReport" />s stored locally.
    /// </summary>
    internal class CheckStatusReports
    {
        public void Run()
        {
            IEnumerable<IStatusReport> reports = Reports.StatusReport.GetLocalReports();
            foreach (IStatusReport report in reports)
            {
                report.ResolveStatusRemotely();
            }
        }
    }
}