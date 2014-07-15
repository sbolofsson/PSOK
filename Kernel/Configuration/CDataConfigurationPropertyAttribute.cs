using System;

namespace PSOK.Kernel.Configuration
{
    /// <summary>
    /// Defines a configuration element with a CData value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CDataConfigurationPropertyAttribute : Attribute
    {
    }
}