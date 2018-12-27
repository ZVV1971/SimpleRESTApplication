using System.IO;
using System.Linq;
using System;
using System.Web.Http;
using System.Web.Mvc;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;

namespace SimpleRESTApplication.Controllers
{
    public class TestController : ApiController
    {
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Get()//byt[] Get()
        {

            //// Путь к файлу
            string file_path = Properties.Settings.Default.BaseFolder +  "1.test.pdf";

            byte[] fileBytes = File.ReadAllBytes(file_path);
            var dataStream = new MemoryStream(fileBytes);
            HttpResponseMessage httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
            httpResponseMessage.Content = new ByteArrayContent(dataStream.ToArray());//StreamContent(dataStream);
            httpResponseMessage.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment");
            httpResponseMessage.Content.Headers.ContentDisposition.FileName = file_path;
            httpResponseMessage.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");
            return httpResponseMessage;
            //return fileBytes;
        }

        /// <summary>
        /// Returns a full absolute path to the file representing requested certificate if it exists
        /// otherwise return null
        /// </summary>
        /// <param name="fileName">a name of the certificate</param>
        /// <returns>absolute path to the certificate file or null if anything goes wrong</returns>
        private string GetFullPath(string fileName)
        {
            //if base folder set up in th properties does not exists return null
            if (!Directory.Exists(Properties.Settings.Default.BaseFolder)) return null;

            //check if certificate is 8 characters long and digits only
            if (fileName.Length == 8 && fileName.All((x)=> x>=0 && x<=9))
            {
                if (!Directory.Exists(Properties.Settings.Default.BaseFolder + "20" + fileName[1] + fileName[2] + @"\ТМЗ\")) return null;
            }
            return null;
        }
    }
}