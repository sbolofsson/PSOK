using System.Collections.Generic;
using System.Reflection;

namespace PSOK.Kernel.Comparers
{
    /// <summary>
    /// Comparer which defines <see cref="AssemblyName" /> equality based on their full name.
    /// </summary>
    public class AssemblyNameEqualityComparer : IEqualityComparer<AssemblyName>
    {
        bool IEqualityComparer<AssemblyName>.Equals(AssemblyName x, AssemblyName y)
        {
            return string.Equals(x.FullName, y.FullName);
        }

        int IEqualityComparer<AssemblyName>.GetHashCode(AssemblyName obj)
        {
            return obj.FullName.GetHashCode();
        }
    }
}