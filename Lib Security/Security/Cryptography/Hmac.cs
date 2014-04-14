using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Web;

namespace Security.Cryptography
{
    public class Hmac
    {

        public static string sign(string inputhash, string key)
        {
            using (HMACMD5 hmac = new HMACMD5(HttpServerUtility.UrlTokenDecode(key)))
            {

                byte[] hashValue = hmac.ComputeHash(Encoding.Default.GetBytes(inputhash));

                return HttpServerUtility.UrlTokenEncode(hashValue);
            }
        }


        public static Boolean versign(string sign, string inputhash,string key)
        {
            using (HMACMD5 hmac = new HMACMD5(HttpServerUtility.UrlTokenDecode(key)))
            {
                Boolean err = false;
                byte[] hashValue = hmac.ComputeHash(Encoding.Default.GetBytes(inputhash));
                byte[] signValue = HttpServerUtility.UrlTokenDecode(sign);
                for (int i = 0; i < hashValue.Length; i++)
                {
                    if (signValue[i] != hashValue[i])
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
