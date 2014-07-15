using System.Configuration;

namespace PSOK.Kernel.Configuration.Pipelines
{
    /// <summary>
    /// Container for several <see cref="Pipeline" />s.
    /// </summary>
    [ConfigurationCollection(typeof (Pipeline), CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class Pipelines : ConfigurationElementCollection<Pipeline>
    {
        /// <summary>
        /// Contruscts a new pipeline element dynamically with the given name.
        /// </summary>
        /// <param name="elementName"></param>
        /// <returns></returns>
        protected override System.Configuration.ConfigurationElement CreateNewElement(string elementName)
        {
            return new Pipeline(elementName);
        }
    }
}