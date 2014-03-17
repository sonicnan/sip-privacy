using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Security;
using System.Collections;

namespace Security.Cryptography
{
    public class RSAcrypto : IDisposable
    {

        // Members:
        // RSA Key components (just the three I'm using, there is more...)
        static private BigInteger D = null;
        static private BigInteger Exponent = null;
        static private BigInteger Modulus = null;

        static RSACryptoServiceProvider _RSA;
        string _privatekey;
        string _publickey;

        public  RSAcrypto()
        {
            _RSA =  new RSACryptoServiceProvider() ;
            _privatekey = _RSA.ToXmlString(true);
            _publickey = _RSA.ToXmlString(false);

        }

        public RSAcrypto(int BitStrength)
        {
            _RSA = new RSACryptoServiceProvider(BitStrength);
            _privatekey = _RSA.ToXmlString(true);
            _publickey = _RSA.ToXmlString(false);

        }

        public string privatekey
        {
            get { return _privatekey; }
        }
        public string publickey
        {
            get { return _publickey; }
        }

        // Encrypt data using private key
        public static byte[] PrivateEncryption(byte[] plantext, string xmlString)
        {
            _RSA = new RSACryptoServiceProvider();
            _RSA.FromXmlString(xmlString);
            RSAParameters rsaParams = _RSA.ExportParameters(true);
            D = new BigInteger(rsaParams.D);
            Modulus = new BigInteger(rsaParams.Modulus);
            Exponent = new BigInteger(rsaParams.Exponent);


            // Converting the byte array data into a BigInteger instance
            BigInteger bnData = new BigInteger(plantext);

            // (bnData ^ D) % Modulus - This Encrypt the data using the private Exponent: D
            BigInteger encData = bnData.modPow(D, Modulus);


            return encData.getBytes();
        }

        // Encrypt data using public key
        public static byte[] PublicEncryption(byte[] plantext, string xmlString)
        {
            _RSA = new RSACryptoServiceProvider();
            _RSA.FromXmlString(xmlString);
            RSAParameters rsaParams = _RSA.ExportParameters(false);
            Modulus = new BigInteger(rsaParams.Modulus);
            Exponent = new BigInteger(rsaParams.Exponent);


            // Converting the byte array data into a BigInteger instance
            BigInteger bnData = new BigInteger(plantext);

            // (bnData ^ D) % Modulus - This Encrypt the data using the private Exponent: D
            BigInteger encData = bnData.modPow(Exponent, Modulus);


            return encData.getBytes();
        }
        
        // Decrypt data using private key (for data encrypted with public key)
        public static byte[] PrivateDecryption(byte[] cyphertext, string xmlString)
        {
            _RSA.FromXmlString(xmlString);
            RSAParameters rsaParams = _RSA.ExportParameters(true);
            D = new BigInteger(rsaParams.D);
            Modulus = new BigInteger(rsaParams.Modulus);
            Exponent = new BigInteger(rsaParams.Exponent);

            // Converting the byte array data into a BigInteger instance
            BigInteger bnData = new BigInteger(cyphertext);

            // (bnData ^ Exponent) % Modulus - This Encrypt the data using the public Exponent
            BigInteger encData = bnData.modPow(D, Modulus);

            return encData.getBytes();
        }

        // Decrypt data using public key (for data encrypted with private key)
        public static byte[] PublicDecryption(byte[] cyphertext, string xmlString)
        {
            _RSA.FromXmlString(xmlString);
            RSAParameters rsaParams = _RSA.ExportParameters(false);
            Modulus = new BigInteger(rsaParams.Modulus);
            Exponent = new BigInteger(rsaParams.Exponent);

            // Converting the byte array data into a BigInteger instance
            BigInteger bnData = new BigInteger(cyphertext);

            // (bnData ^ Exponent) % Modulus - This Encrypt the data using the public Exponent
            BigInteger encData = bnData.modPow(Exponent, Modulus);

            return encData.getBytes();
        }

        public void Dispose()
        {
            _RSA.Clear();
        }
    }
}
