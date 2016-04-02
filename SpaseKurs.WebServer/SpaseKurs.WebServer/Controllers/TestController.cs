using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SpaseKurs.WebServer.Controllers
{
    [RoutePrefix("test")]
    public class TestController : ApiController
    {
        [Route("hello")]
        public string GetHello()
        {
            return "Hello!";
        }
    }
}
