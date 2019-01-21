using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SimpleRESTApplication.Alumni
{
    public class X509EncDec
    {
        private static X509Certificate2 certificate;

        public X509EncDec(string certificateName)
        {
            if (!Properties.Settings.Default.UseFile)
                certificate = getCertificate("");
            else certificate = new X509Certificate2(Properties.Settings.Default.CertFileName, Properties.Settings.Default.CertPwd);
        }

        private X509Certificate2 getCertificate(string certificateName)
        {
            X509Store my = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            my.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection collection =
                my.Certificates.Find(X509FindType.FindBySubjectName, certificateName, false);
            if (collection.Count == 1)
            {
                return collection[0];
            }
            else if (collection.Count > 1)
            {
                throw new Exception(string.Format("More than one certificate with name '{0}' found in store LocalMachine/My.", certificateName));
            }
            else
            {
                throw new Exception(string.Format("Certificate '{0}' not found in store LocalMachine/My.", certificateName));
            }
        }

        public string EncryptRsa(string input)
        {
            string output = string.Empty;
            using (RSACryptoServiceProvider csp = (RSACryptoServiceProvider)certificate.PublicKey.Key)
            {
                byte[] bytesData = Encoding.UTF8.GetBytes(input);
                byte[] bytesEncrypted = csp.Encrypt(bytesData, false);
                output = Convert.ToBase64String(bytesEncrypted);
            }
            return output;
        }

        public string DecryptRsa(string encrypted)
        {
            string text = string.Empty;
            using (RSACryptoServiceProvider csp = (RSACryptoServiceProvider)certificate.PrivateKey)
            {
                byte[] bytesEncrypted = Convert.FromBase64String(encrypted);
                byte[] bytesDecrypted = csp.Decrypt(bytesEncrypted, false);
                text = Encoding.UTF8.GetString(bytesDecrypted);
            }
            return text;
        }

        public string SignData(string input)
        {
            string text = string.Empty;
            using (RSACryptoServiceProvider csp = (RSACryptoServiceProvider)certificate.PrivateKey)
            {
                byte[] bytesData = Encoding.UTF8.GetBytes(input);
                byte[] bytesSigned = csp.SignData(bytesData, CryptoConfig.MapNameToOID("SHA1"));
                text = Convert.ToBase64String(bytesSigned);
            }
            return text;
        }

        public bool VerifySignature(string signed, string original)
        {
            bool checkresult = false;
            using (RSACryptoServiceProvider csp = (RSACryptoServiceProvider)certificate.PublicKey.Key)
            {
                byte[] bytesSigned = Convert.FromBase64String(signed);
                byte[] bytesOriginal = Encoding.UTF8.GetBytes(original);
                checkresult = csp.VerifyData(bytesOriginal, CryptoConfig.MapNameToOID("SHA1"), bytesSigned);
            }
            return checkresult;
        }
    }

}