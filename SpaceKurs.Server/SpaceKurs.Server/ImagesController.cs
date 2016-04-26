namespace SpaceKurs.Server
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Web.Http;

    public class ImagesController : ApiController
    {
        public HttpResponseMessage Get()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            using (var fileStream = new FileStream("lena.jpg", FileMode.Open, FileAccess.Read))
            {
                response.Content = new StreamContent(fileStream);
            }
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                                                          {
                                                              FileName = "lena.jpg"
                                                          };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpg");

            return response;
        }

        public HttpResponseMessage Get(
            int id)
        {
            var imagePath = Program.Images.ElementAtOrDefault(id);
            if (imagePath == null)
            {
                throw new Exception(string.Format("Image with id={0} does not exist", id));
            }

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            using (var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                response.Content = new StreamContent(fileStream);
            }
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                                                          {
                                                              FileName = string.Format("{0}.jpg", id),
                                                          };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpg");

            return response;
        }
    }
}
