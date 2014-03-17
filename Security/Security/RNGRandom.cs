using System;
using System.Security.Cryptography;
using System.Linq;

namespace Security
{
    public class RNGRandom
    {
        private static RNGCryptoServiceProvider _Random = new RNGCryptoServiceProvider();
        private static byte[] bytes = new byte[4];


        public static int RandomNumber(int max)
        {
            if (max <= 0)
            {
                throw new ArgumentOutOfRangeException("max");
            }
            _Random.GetBytes(bytes);
            int value = BitConverter.ToInt32(bytes, 0) % max;
            if (value < 0)
            {
                value = -value;
            }
            return value;
        }

        public static String RandomString(int max)
        {
            if (max <= 0)
            {
                throw new ArgumentOutOfRangeException("max");
            }
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var result = new string(
                Enumerable.Repeat(chars, max)
                          .Select(s => s[RandomNumber(chars.Length)])
                          .ToArray());
            return result;
        }

    }
}
