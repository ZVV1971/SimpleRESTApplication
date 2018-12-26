using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using System.Web;
using System.IO;
using System.Net.Http.Headers;

namespace SimpleRESTApplication.Controllers
{
    public class TestController : ApiController
    {
        public byte[] Get()//HttpResponseMessage Get()
        {
            //HttpResponseMessage httpResponseMessage = new HttpResponseMessage();

            //// Путь к файлу
            string file_path = @"e:/1.test.pdf"; //Server.MapPath("~/Files/PDFIcon.pdf");
            //// Тип файла - content-type
            //string file_type = "application/pdf";
            //// Имя файла - необязательно
            //string file_name = "PDFIcon.pdf";
            //return File(file_path, file_type, file_name);

            byte[] fileBytes = File.ReadAllBytes(file_path);
            //var dataStream = new MemoryStream(fileBytes);
            //HttpResponseMessage httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
            //httpResponseMessage.Content = new ByteArrayContent(dataStream.ToArray());//StreamContent(dataStream);
            //httpResponseMessage.Content.Headers.ContentDisposition = 
            //    new ContentDispositionHeaderValue("attachment");
            //httpResponseMessage.Content.Headers.ContentDisposition.FileName = file_path;
            //httpResponseMessage.Content.Headers.ContentType = 
            //    new MediaTypeHeaderValue("application/octet-stream");
            //return httpResponseMessage;
            return fileBytes;
        }
    }
}
