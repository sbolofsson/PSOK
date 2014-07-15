using PSOK.PublishSubscribe.Reports;

namespace PSOK.Kademlia.Web.Areas.Api.Models
{
    /// <summary>
    /// A view model representing an <see cref="IStatusReportEntry" />.
    /// </summary>
    public class StatusReportEntryViewModel
    {
        /// <summary>
        /// A description of the <see cref="StatusReportEntryViewModel" />.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The value of the <see cref="StatusReportEntryViewModel" />.
        /// </summary>
        public string Value { get; set; }
    }
}