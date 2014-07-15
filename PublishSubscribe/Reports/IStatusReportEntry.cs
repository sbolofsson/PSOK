namespace PSOK.PublishSubscribe.Reports
{
    /// <summary>
    /// Defines an entry in an <see cref="IStatusReport" />.
    /// </summary>
    public interface IStatusReportEntry
    {
        /// <summary>
        /// A description of the <see cref="IStatusReportEntry" />.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// The value of the <see cref="IStatusReportEntry" />.
        /// </summary>
        string Value { get; }
    }
}