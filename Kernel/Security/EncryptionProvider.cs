using System;
using System.Linq;
using System.Security.Cryptography;

namespace PSOK.Kernel.Security
{
    /// <summary>
    /// A class providing encryption methods.
    /// </summary>
    public static class EncryptionProvider
    {
        /// <summary>
        /// Computes the SHA1 hash of the specified string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Hash(string str)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentNullException("str");

            SHA1 sha1CryptoServiceProvider = new SHA1CryptoServiceProvider();
            byte[] hash = sha1CryptoServiceProvider.ComputeHash(System.Text.Encoding.UTF8.GetBytes(str));
            return GetString(hash);
        }

        /// <summary>
        /// Converts the specified string to a byte array.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] GetBytes(string str)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentNullException("str");

            return str.Split('-').Select(b => Convert.ToByte(b, 16)).ToArray();
        }

        /// <summary>
        /// Converts the specified byte array to a string.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string GetString(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");

            return BitConverter.ToString(bytes);
        }
    }
}