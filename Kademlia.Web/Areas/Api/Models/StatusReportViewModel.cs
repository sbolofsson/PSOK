using System.Collections.Generic;
using PSOK.PublishSubscribe.Reports;

namespace PSOK.Kademlia.Web.Areas.Api.Models
{
    /// <summary>
    /// A view model representing an <see cref="IStatusReport" />.
    /// </summary>
    public class StatusReportViewModel
    {
        /// <summary>
        /// Constructs a new <see cref="StatusReportViewModel" />.
        /// </summary>
        public StatusReportViewModel()
        {
            Entries = new List<StatusReportEntryViewModel>();
        }

        /// <summary>
        /// A unique id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The header of the <see cref="StatusReportViewModel" />.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// The overall status of the <see cref="StatusReportViewModel" />.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// A collection of <see cref="StatusReportEntryViewModel" />s.
        /// </summary>
        public IEnumerable<StatusReportEntryViewModel> Entries { get; set; }
    }
}