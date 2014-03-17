using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Security.Cryptography
{
    public class Hmac
    {

        public static byte[] sign(byte[] inputhash, byte[] key)
        {
            using (HMACMD5 hmac = new HMACMD5(key))
            {

                byte[] hashValue = hmac.ComputeHash(inputhash);

                return hashValue;
            }
        }


        public static Boolean versign(byte[] sign, byte[] inputhash, byte[] key)
        {
            using (HMACMD5 hmac = new HMACMD5(key))
            {
                Boolean err = false;
                byte[] hashValue = hmac.ComputeHash(inputhash);

                for (int i = 0; i < hashValue.Length; i++)
                {
                    if (sign[i] != hashValue[i])
                    {
                        err = true;
                    }
                }
                if (err != true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
