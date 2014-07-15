namespace PSOK.Kernel.Encoding
{
    /// <summary>
    /// Helper classes for working with binaries.
    /// </summary>
    public static class BitOperations
    {
        /// <summary>
        /// Indicates the index of the most signifcant bit in a byte.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static int MostSignificantBit(byte word)
        {
            int msb = 7;
            while ((word >>= 1) != 0)
                msb--;
            return msb;
        }

        /// <summary>
        /// Ensures that a binary integer representation is represented as a positive integer.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte[] EnsurePositive(byte[] bytes)
        {
            bool isNegative = (bytes[bytes.Length - 1] & 0x80) > 0;

            // For negative numbers we have to flip the first byte and add 1
            // Also remember to flip first byte
            if (isNegative)
            {
                bytes[0] = (byte)(255 - bytes[0] + 1);
                for (int i = 1; i < bytes.Length - 1; i++)
                {
                    bytes[i] = (byte)(255 - bytes[i]);
                }
            }
            return bytes;
        }
    }
}
