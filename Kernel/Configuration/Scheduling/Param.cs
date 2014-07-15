using System.Configuration;

namespace PSOK.Kernel.Configuration.Scheduling
{
    /// <summary>
    /// A parameter for an <see cref="Agent" />'s method property.
    /// </summary>
    public class Param : ConfigurationElement
    {
        /// <summary>
        /// The parameter name of the argument that is to be parsed to the <see cref="Agent" />'s method.
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return this["name"] as string; }
        }

        /// <summary>
        /// The value of the argument.
        /// </summary>
        public string Value
        {
            get { return CData; }
        }
    }
}