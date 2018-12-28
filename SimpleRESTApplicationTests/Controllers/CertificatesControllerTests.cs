using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleRESTApplication.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleRESTApplication.Controllers.Tests
{
    [TestClass()]
    public class CertificatesControllerTests
    {
        [TestMethod()]
        public void GetTest()
        {
            string str = "123/235345 от 12.12.2012г";
            CertificatesController CC = new CertificatesController();
            string s = CC.GetFullPath(str);
            Assert.Fail();
        }
    }
}