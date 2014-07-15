using System.Data.Entity;

// ReSharper disable UnusedTypeParameter

namespace PSOK.Kernel.Services
{
    /// <summary>
    /// An interface definition for data services.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataServiceBase<out T> where T : DbContext
    {
    }
}

// ReSharper restore UnusedTypeParameter