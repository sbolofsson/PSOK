using PSOK.PublishSubscribe.Reports;

namespace PSOK.PublishSubscribe.Tasks
{
    /// <summary>
    /// Creates <see cref="IStatusReport" />s for the current application.
    /// </summary>
    internal class StatusReport
    {
        public void Run()
        {
            IStatusReport report = Reports.StatusReport.Create();
            report.StoreRemotely();
        }
    }
}