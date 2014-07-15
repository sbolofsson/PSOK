using System;

namespace PSOK.Kernel.Encoding
{
    /// <summary>
    /// Class to work with different formats and units.
    /// </summary>
    public static class Formatter
    {
        /// <summary>
        /// Formats the given size as a data size (KB, MB or GB).
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string FormatAsDataSize(long size)
        {
            decimal decSize = size;

            if (decSize > 1073741824)
                return string.Format("{0} GB", (Math.Round(decSize*100/1073741824)/100));

            if (decSize > 1048576)
                return string.Format("{0} MB", (Math.Round(decSize*100/1048576)/100));

            return string.Format("{0} KB", (Math.Round(decSize*100/1024)/100));
        }
    }
}