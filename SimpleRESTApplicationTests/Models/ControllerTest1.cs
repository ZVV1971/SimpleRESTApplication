using System;
using SimpleRESTApplication.Alumni;
using SimpleRESTApplication.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleRESTApplicationTests.Models
{
    [TestClass]
    public class ControllerTest1
    {
        [TestMethod]
        public void TestSignature()
        {
            string query = "?id=41800110ot12.12.2018&pos=Director&name=Nemo";
            X509EncDec x509 = new X509EncDec("KML-SERVER");
            string sig = x509.SignData(query);

            //CertificatesController certificates = new CertificatesController();
            //certificates.Get()
        }
    }
}
