using System;
using System.Globalization;
using System.Text;
using System.Web;
using System.Xml;

namespace PSOK.Kernel.Encoding
{
    /// <summary>
    /// Helper class for encoding and decoding strings.
    /// </summary>
    public static class Encoder
    {
        /// <summary>
        /// Encodes a normal string to a hexidecimal string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string HexEncode(string str)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentNullException("str");

            StringBuilder stringBuilder = new StringBuilder();
            foreach (char c in str)
            {
                stringBuilder.Append(string.Format("{0:x2}",
                    Convert.ToUInt32(((int) c).ToString(CultureInfo.InvariantCulture))));
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Decodes a hexidecimal string to a normal string
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static string HexDecode(string hexString)
        {
            if (string.IsNullOrEmpty(hexString))
                throw new ArgumentNullException("hexString");

            StringBuilder stringBuilder = new StringBuilder();
            while (hexString.Length > 0)
            {
                stringBuilder.Append(
                    Convert.ToChar(Convert.ToUInt32(hexString.Substring(0, 2), 16))
                        .ToString(CultureInfo.InvariantCulture));
                hexString = hexString.Substring(2, hexString.Length - 2);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Html encodes a string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string HtmlEncode(string str)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentNullException("str");

            return HttpUtility.HtmlEncode(str);
        }

        /// <summary>
        /// Html decodes a string
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string HtmlDecode(string html)
        {
            if (string.IsNullOrEmpty(html))
                throw new ArgumentNullException("html");

            return HttpUtility.HtmlDecode(html);
        }

        /// <summary>
        /// Removes the characters that are illegal in an <see cref="XmlDictionary" />.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string SafeEncode(string str)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentNullException("str");

            return str
                .Replace("+", "2B")
                .Replace("`", "0x60")
                .Replace("[", "0x5B")
                .Replace(",", "0x2C")
                .Replace(" ", "0x20")
                .Replace("=", "0x3D")
                .Replace("]", "0x5D");
        }

        /// <summary>
        /// Reintroduces the characters that are illegal in an <see cref="XmlDictionary" />.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string SafeDecode(string str)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentNullException("str");

            return str
                .Replace("2B", "+")
                .Replace("0x60", "`")
                .Replace("0x5B", "[")
                .Replace("0x2C", ",")
                .Replace("0x20", " ")
                .Replace("0x3D", "=")
                .Replace("0x5D", "]");
        }
    }
}