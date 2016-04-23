using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace SpaceKurs.Server
{
    public class ImagesController : ApiController
    {
        public HttpResponseMessage Get()
        {
            //using (var fileStream = new FileStream("D:\\lena.jpg", FileMode.Open))
            //{
            //    return fileStream;
            //}

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(new FileStream("lena.jpg", FileMode.Open, FileAccess.Read));
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = "lena.jpg";
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpg");

            return response;
            //return "ImagesContoller works!";
        }
    }
}
