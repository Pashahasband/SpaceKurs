namespace SpaceKurs.Server
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Web.Http;

    [RoutePrefix("/api/previews")]
    public class PreviewsController : ApiController
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

        [Route("{id:guid")]
        [HttpGet]
        public HttpResponseMessage Get(
            Guid id)
        {
            var imageInfo = ImageRegistry.GetImageInfo(id);
            if (imageInfo == null)
            {
                throw new Exception(string.Format("Image with id={0} does not exist", id));
            }

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            var fileStream = new FileStream(imageInfo.PreviewPath, FileMode.Open, FileAccess.Read);
            response.Content = new StreamContent(fileStream);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = string.Format("{0}{1}", id, imageInfo.Extension),
            };
            string mediaType = string.Format("image/{0}", imageInfo.Extension);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

            return response;
        }
    }
}
