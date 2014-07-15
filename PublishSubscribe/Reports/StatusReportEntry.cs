namespace PSOK.PublishSubscribe.Reports
{
    /// <summary>
    /// Defines an entry in an <see cref="IStatusReport" />.
    /// </summary>
    internal class StatusReportEntry : IStatusReportEntry
    {
        /// <summary>
        /// A description of the <see cref="StatusReportEntry" />.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The value of the <see cref="StatusReportEntry" />.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Prints the <see cref="StatusReportEntry" />.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}: {1}", Description, Value);
        }
    }
}