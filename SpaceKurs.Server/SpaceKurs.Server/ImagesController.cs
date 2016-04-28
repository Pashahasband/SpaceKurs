namespace SpaceKurs.Server
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Web.Http;

    [RoutePrefix("api/images")]
    public class ImagesController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Get()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            var fileStream = new FileStream("lena.jpg", FileMode.Open, FileAccess.Read);
            response.Content = new StreamContent(fileStream);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                                                          {
                                                              FileName = "lena.jpg"
                                                          };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpg");

            return response;
        }

        [HttpGet]
        [Route("{id:guid}")]
        public HttpResponseMessage Get(
            Guid id)
        {
            var imageInfo = ImageRegistry.GetImageInfo(id);
            if (imageInfo == null)
            {
                throw new Exception(string.Format("Image with id={0} does not exist", id));
            }

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            var fileStream = new FileStream(imageInfo.ImagePath, FileMode.Open, FileAccess.Read);
            response.Content = new StreamContent(fileStream);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                                                          {
                                                              FileName = string.Format("{0}.jpg", id),
                                                          };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpg");

            return response;
        }
    }
}
