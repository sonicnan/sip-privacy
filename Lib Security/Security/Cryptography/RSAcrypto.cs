using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Security;
using System.Collections;
using System.Web;
using System.IO;
using System.Windows.Forms;

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
            _RSA =  new RSACryptoServiceProvider();
            _privatekey = _RSA.ToXmlString(true);
            _publickey = _RSA.ToXmlString(false);

        }

        public RSAcrypto(int BitStrength,string username)
        {
            _RSA = new RSACryptoServiceProvider(BitStrength);
            _privatekey = _RSA.ToXmlString(true);
            _publickey = _RSA.ToXmlString(false);
            string subPath = Application.StartupPath + "\\Settings\\Key\\";
            if (!Directory.Exists(subPath))
                Directory.CreateDirectory(subPath);

            File.WriteAllText(subPath + username + ".pik", _privatekey);
            File.WriteAllText(subPath + username + ".puk", _publickey);
        }

        // Methods:
        public void LoadPublicFromXml(string username)
        {
            string publicPath = Application.StartupPath + "\\Settings\\Key\\" + username + ".puk";

            if (!File.Exists(publicPath))
                throw new FileNotFoundException("File not exists: " + publicPath);
            // Using the .NET RSA class to load a key from an Xml file, and populating the relevant members
            // of my class with it's RSAParameters
            try
            {
                _RSA.FromXmlString(File.ReadAllText(publicPath));
                _publickey = _RSA.ToXmlString(false);
            }
            // Examle for the proper use of try - catch blocks: Informing the main app where and why the Exception occurred
            catch (XmlSyntaxException ex)  // Not an xml file
            {
                string excReason = "Exception occurred at LoadPublicFromXml(), Selected file is not a valid xml file.";
                System.Diagnostics.Debug.WriteLine(excReason + " Exception Message: " + ex.Message);
                throw new Exception(excReason, ex);
            }
            catch (CryptographicException ex)  // Not a Key file
            {
                string excReason = "Exception occurred at LoadPublicFromXml(), Selected xml file is not a public key file.";
                System.Diagnostics.Debug.WriteLine(excReason + " Exception Message: " + ex.Message);
                throw new Exception(excReason, ex);
            }
            catch (Exception ex)  // other exception, hope the ex.message will help
            {
                string excReason = "General Exception occurred at LoadPublicFromXml().";
                System.Diagnostics.Debug.WriteLine(excReason + " Exception Message: " + ex.Message);
                throw new Exception(excReason, ex);
            }
            // You might want to replace the Diagnostics.Debug with your Log statement
        }

        // Same as the previous one, but this time loading the private Key
        public void LoadPrivateFromXml(string username)
        {
            string privatePath = Application.StartupPath + "\\Settings\\Key\\" + username + ".pik";

            if (!File.Exists(privatePath))
                throw new FileNotFoundException("File not exists: " + privatePath);
            try
            {
                _RSA.FromXmlString(File.ReadAllText(privatePath));
                _privatekey = _RSA.ToXmlString(true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception occurred at LoadPrivateFromXml()\nMessage: " + ex.Message);
                throw ex;
            }
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
        public static string PrivateEncryption(string plantext, string xmlString)
        {
            _RSA = new RSACryptoServiceProvider();
            _RSA.FromXmlString(xmlString);
            RSAParameters rsaParams = _RSA.ExportParameters(true);
            D = new BigInteger(rsaParams.D);
            Modulus = new BigInteger(rsaParams.Modulus);
            Exponent = new BigInteger(rsaParams.Exponent);


            // Converting the byte array data into a BigInteger instance
            BigInteger bnData = new BigInteger(Encoding.Default.GetBytes(plantext));

            // (bnData ^ D) % Modulus - This Encrypt the data using the private Exponent: D
            BigInteger encData = bnData.modPow(D, Modulus);


            return HttpServerUtility.UrlTokenEncode(encData.getBytes());
        }

        // Encrypt data using public key
        public static string PublicEncryption(string plantext, string xmlString)
        {
            _RSA = new RSACryptoServiceProvider();
            _RSA.FromXmlString(xmlString);
            RSAParameters rsaParams = _RSA.ExportParameters(false);
            Modulus = new BigInteger(rsaParams.Modulus);
            Exponent = new BigInteger(rsaParams.Exponent);


            // Converting the byte array data into a BigInteger instance
            BigInteger bnData = new BigInteger(Encoding.Default.GetBytes(plantext));

            // (bnData ^ D) % Modulus - This Encrypt the data using the private Exponent: D
            BigInteger encData = bnData.modPow(Exponent, Modulus);


            return HttpServerUtility.UrlTokenEncode(encData.getBytes());
        }
        
        // Decrypt data using private key (for data encrypted with public key)
        public static string PrivateDecryption(string cyphertext, string xmlString)
        {
            _RSA.FromXmlString(xmlString);
            RSAParameters rsaParams = _RSA.ExportParameters(true);
            D = new BigInteger(rsaParams.D);
            Modulus = new BigInteger(rsaParams.Modulus);
            Exponent = new BigInteger(rsaParams.Exponent);

            // Converting the byte array data into a BigInteger instance
            BigInteger bnData = new BigInteger(HttpServerUtility.UrlTokenDecode(cyphertext));

            // (bnData ^ Exponent) % Modulus - This Encrypt the data using the public Exponent
            BigInteger encData = bnData.modPow(D, Modulus);

            return Encoding.Default.GetString(encData.getBytes());
        }

        // Decrypt data using public key (for data encrypted with private key)
        public static string PublicDecryption(string cyphertext, string xmlString)
        {
            _RSA.FromXmlString(xmlString);
            RSAParameters rsaParams = _RSA.ExportParameters(false);
            Modulus = new BigInteger(rsaParams.Modulus);
            Exponent = new BigInteger(rsaParams.Exponent);

            // Converting the byte array data into a BigInteger instance
            BigInteger bnData = new BigInteger(HttpServerUtility.UrlTokenDecode(cyphertext));

            // (bnData ^ Exponent) % Modulus - This Encrypt the data using the public Exponent
            BigInteger encData = bnData.modPow(Exponent, Modulus);

            return Encoding.Default.GetString(encData.getBytes());
        }

        public void Dispose()
        {
            _RSA.Clear();
        }
    }
}
