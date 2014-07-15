using System;
using System.IO;
using System.Security.Cryptography;
using PSOK.Kernel;
using PSOK.Kernel.Configuration;
using PSOK.Kernel.Environment;
using PSOK.Kernel.Reflection;
using PSOK.Kernel.Security;
using Transport = PSOK.Kernel.Services.Transport;

namespace PSOK.Kademlia
{
    /// <summary>
    /// Provider of Kademlia keys.
    /// </summary>
    internal static class KeyProvider
    {
        private static IContact _contact;

        /// <summary>
        /// Generates a <see cref="IContact"/> for a node.
        /// </summary>
        /// <returns></returns>
        public static IContact GetContact()
        {
            if (_contact != null)
                return _contact;

            Config config = Config.ReadConfig();
            string nodeId = config.Kademlia.NodeId;

            if (!string.IsNullOrEmpty(nodeId))
            {
                return new Contact(Transport.GetServiceBaseUrl(), nodeId);
            }
            
            string publicIp = EnvironmentHelper.GetPublicIp();
            string localIp = EnvironmentHelper.GetLocalIp();
            string machineName = EnvironmentHelper.GetFqdn();
            FileInfo assemblyFileInfo = AssemblyHelper.GetEntryAssemblyFileInfo();
            string directory = assemblyFileInfo.DirectoryName;
            string applicationId = AppSettings.InstanceMode == InstanceMode.Single ?
                AssemblyHelper.GetEntryAssemblyId() :
                EncryptionProvider.GetString(GetRandomKey());

            string uniqueAttributes = string.Format("{0}#{1}#{2}#{3}#{4}", publicIp, localIp, machineName, directory, applicationId);
            nodeId = EncryptionProvider.Hash(uniqueAttributes);

            return (_contact = new Contact(Transport.GetServiceBaseUrl(), nodeId));
        }

        /// <summary>
        /// Computes a cryptographically secure random Kademlia key.
        /// </summary>
        /// <returns></returns>
        public static byte[] GetRandomKey()
        {
            byte[] bytes = new byte[Constants.B / 8];
            RNGCryptoServiceProvider rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            rngCryptoServiceProvider.GetNonZeroBytes(bytes);
            return bytes;
        }

        /// <summary>
        /// Computes a cryptographically secure random Kademlia key that matches the boundaries of this <see cref="IBucket"/>.
        /// </summary>
        /// <returns></returns>
        public static string GetRandomKey(string key, int index)
        {
            byte[] keyBytes = EncryptionProvider.GetBytes(key);
            byte[] bytes = GetRandomKey();

            int sharedPrefixLengthBytes = (int)Math.Floor((index / (decimal)8));
            int sharedPrefixLengthBits = index % 8;

            CryptoRandom cryptoRandom = new CryptoRandom();

            for (int i = 0; i < keyBytes.Length; i++)
            {
                if (i > sharedPrefixLengthBytes)
                {
                    // Keep randomness
                    bytes[i] = (byte)(bytes[i] ^ keyBytes[i]);
                }
                else if (i < sharedPrefixLengthBytes)
                {
                    // Use the bytes from the given key
                    bytes[i] = keyBytes[i];
                }
                else if (i == sharedPrefixLengthBytes)
                {
                    // Preserve some random bits without modifying the most significant bit index
                    int min = (255 >> (sharedPrefixLengthBits + 1)) + 1;
                    int max = (255 >> sharedPrefixLengthBits) + 1;
                    int shift = cryptoRandom.Next(min, max);
                    bytes[i] = (byte)(shift ^ keyBytes[i]);
                }
            }
            return EncryptionProvider.GetString(bytes);
        }
    }
}
