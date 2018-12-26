using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleRESTApplication;
using SimpleRESTApplication.Controllers;

namespace SimpleRESTApplication.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void Index()
        {
            // Упорядочение
            HomeController controller = new HomeController();

            // Действие
            ViewResult result = controller.Index() as ViewResult;

            // Утверждение
            Assert.IsNotNull(result);
            Assert.AreEqual("Home Page", result.ViewBag.Title);
        }
    }
}
