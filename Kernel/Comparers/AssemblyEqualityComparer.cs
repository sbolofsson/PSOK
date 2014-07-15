using System.Collections.Generic;
using System.Reflection;

namespace PSOK.Kernel.Comparers
{
    /// <summary>
    /// Comparer which defines <see cref="Assembly" /> equality based on their full name.
    /// </summary>
    public class AssemblyEqualityComparer : IEqualityComparer<Assembly>
    {
        bool IEqualityComparer<Assembly>.Equals(Assembly x, Assembly y)
        {
            return string.Equals(x.FullName, y.FullName);
        }

        int IEqualityComparer<Assembly>.GetHashCode(Assembly obj)
        {
            return obj.FullName.GetHashCode();
        }
    }
}