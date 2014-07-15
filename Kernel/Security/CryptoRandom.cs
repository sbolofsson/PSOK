using System;
using System.Security.Cryptography;

namespace PSOK.Kernel.Security
{
    /// <summary>
    /// Cryptographically secure random class.
    /// </summary>
    public class CryptoRandom : Random
    {
        // Implementation of this class is taken from the publicly available Microsoft implementation located at
        // http://msdn.microsoft.com/en-us/magazine/cc163367.aspx

        private readonly RNGCryptoServiceProvider _rngCryptoServiceProvider = new RNGCryptoServiceProvider();
        private readonly byte[] _uintBuffer = new byte[4];

        /// <summary>
        /// Constructs a new <see cref="CryptoRandom"/>.
        /// </summary>
        public CryptoRandom()
        {
            
        }

        /// <summary>
        /// Constructs a new <see cref="CryptoRandom"/>.
        /// The specified seed is not used in this implementation.
        /// </summary>
        /// <param name="seed"></param>
        // ReSharper disable UnusedParameter.Local
        public CryptoRandom(int seed)
        {
            
        }
        // ReSharper restore UnusedParameter.Local

        /// <summary>
        /// Computes a random number between 0 and <see cref="int.MaxValue"/>.
        /// </summary>
        /// <returns></returns>
        public override int Next()
        {
            _rngCryptoServiceProvider.GetBytes(_uintBuffer);
            return BitConverter.ToInt32(_uintBuffer, 0) & 0x7FFFFFFF;
        }

        /// <summary>
        /// Computes a random number between 0 and the specified max value.
        /// </summary>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public override int Next(int maxValue)
        {
            if (maxValue < 0)
                throw new ArgumentOutOfRangeException("maxValue");
            
            return Next(0, maxValue);
        }

        /// <summary>
        /// Computes a random number between the specified min and max value.
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public override int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
                throw new ArgumentOutOfRangeException("minValue");
            
            if (minValue == maxValue)
                return minValue;
            long diff = maxValue - minValue;
            while (true)
            {
                _rngCryptoServiceProvider.GetBytes(_uintBuffer);
                uint rand = BitConverter.ToUInt32(_uintBuffer, 0);
                const long max = (1 + (long)uint.MaxValue);
                long remainder = max % diff;
                if (rand < max - remainder)
                {
                    return (int)(minValue + (rand % diff));
                }
            }
        }

        /// <summary>
        /// Computes a random number between 0 and <see cref="int.MaxValue"/>.
        /// </summary>
        /// <returns></returns>
        public override double NextDouble()
        {
            _rngCryptoServiceProvider.GetBytes(_uintBuffer);
            uint random = BitConverter.ToUInt32(_uintBuffer, 0);
            return random / (1.0 + uint.MaxValue);
        }

        /// <summary>
        /// Fills the specified byt array with random bytes.
        /// </summary>
        /// <param name="buffer"></param>
        public override void NextBytes(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            _rngCryptoServiceProvider.GetBytes(buffer);
        }
    }
}
