namespace PSOK.Kernel.Tasks
{
    /// <summary>
    /// Represents a parameter value to an <see cref="Agent" />'s entry method.
    /// </summary>
    internal class Param
    {
        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The value of the parameter.
        /// </summary>
        public string Value { get; set; }
    }
}