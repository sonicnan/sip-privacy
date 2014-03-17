using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Security.Cryptography
{
    public class THashAlgorithm
    {
        public enum SHATYPE
        {
            MD5, SHA1, SHA256, SHA384, SHA512
        }

        public static string ComputeHash(string plainText, SHATYPE hashAlgorithm)
        {
            HashAlgorithm hash;
            if (hashAlgorithm == SHATYPE.MD5)
            {
                hashAlgorithm = SHATYPE.MD5;
            }
            switch (hashAlgorithm)
            {
                case SHATYPE.SHA1:
                    hash = new SHA1Managed();
                    break;

                case SHATYPE.SHA256:
                    hash = new SHA256Managed();
                    break;

                case SHATYPE.SHA384:
                    hash = new SHA384Managed();
                    break;

                case SHATYPE.SHA512:
                    hash = new SHA512Managed();
                    break;

                default:
                    hash = new MD5CryptoServiceProvider();
                    break;
            }
            byte[] hashBytes = hash.ComputeHash(Encoding.Default.GetBytes(plainText));
            string ret = "";
            foreach (byte a in hashBytes)
            {
                if (a < 16)
                    ret += "0" + a.ToString("x");
                else
                    ret += a.ToString("x");
            }
            return ret;
        }
    }
}
