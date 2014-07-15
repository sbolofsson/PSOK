using System;
using System.Collections.Generic;
using PSOK.Kernel.Reflection;

namespace PSOK.Kernel.Comparers
{
    /// <summary>
    /// Comparer which defines <see cref="Type" /> equality based on their full name.
    /// </summary>
    public class TypeEqualityComparer : IEqualityComparer<Type>
    {
        bool IEqualityComparer<Type>.Equals(Type x, Type y)
        {
            return string.Equals(x.AssemblyQualifiedName(), y.AssemblyQualifiedName());
        }

        int IEqualityComparer<Type>.GetHashCode(Type obj)
        {
            return obj.AssemblyQualifiedName().GetHashCode();
        }
    }
}