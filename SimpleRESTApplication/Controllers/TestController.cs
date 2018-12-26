using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SimpleRESTApplication.Controllers
{
    public class TestController : ApiController
    {
        public string[] Get()
        {
            return new string[]
            {
                "Hello",
                "World"
            };
        }
    }
}
