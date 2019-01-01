using System.IO;
using System.Linq;
using System;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Hosting;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using SimpleRESTApplication.Alumni;

namespace SimpleRESTApplication.Controllers
{
    public class CertificatesController : ApiController
    {
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Get(string id)
        {
            HttpResponseMessage httpResponseMessage = null;

            Logger.WriteToLog("got id " + id + " - " + DateTime.Now.ToString());
            //// Путь к файлу
            string file_path = GetFullPath(ref id);

            if (file_path != null)
            {
                try
                {
                    byte[] fileBytes = File.ReadAllBytes(file_path);
                    var dataStream = new MemoryStream(fileBytes);
                    httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
                    httpResponseMessage.Content = new ByteArrayContent(dataStream.ToArray());
                    httpResponseMessage.Content.Headers.ContentDisposition =
                        new ContentDispositionHeaderValue("attachment");
                    httpResponseMessage.Content.Headers.ContentDisposition.FileName = id + ".pdf";
                    httpResponseMessage.Content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/pdf");
                }
                catch (Exception ex)
                {
                    httpResponseMessage = Request.CreateResponse(HttpStatusCode.NotFound);
                    httpResponseMessage.Content = new StringContent(ex.Message);
                    return httpResponseMessage;
                }
            }
            else
            {
                httpResponseMessage = Request.CreateResponse(HttpStatusCode.NotFound);
                httpResponseMessage.Content = new StringContent(id);
            }
            return httpResponseMessage;
            //return fileBytes;
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage Get()
        {
            HttpResponseMessage res = null;
            try
            {
                string resp = Properties.Settings.Default.CertificatesGetTestResponse;
                res = new HttpResponseMessage(HttpStatusCode.OK);
                res.Content = new StringContent(resp);
                return res;
            }
            catch
            {
                res = new HttpResponseMessage(HttpStatusCode.NotFound);
                return res;
            }
        }

        /// <summary>
        /// Returns a full absolute path to the file representing requested certificate
        /// </summary>
        /// <param name="fileName">a name of the certificate</param>
        /// <returns>absolute path to the certificate file</returns>
        private string GetFullPath(ref string fileName)
        {
            string pattern = @"\d{4}|(?<=\.)\d{2}(?=\D+$)";
            string realPath = null;
           
            //create aray of separators
            char[] sep = new char[] { (char)0x20, 'o', 'O', 'о', 'О' };
           
            //split string to separate certificate number from the date
            string[] str = fileName.Split(separator: sep,
                options: StringSplitOptions.RemoveEmptyEntries,
                count:2);//do not need more than 2 parts

            if (str.Count() != 2) { fileName = "error at splitting";  return null; }


            //check if certificate is 8 characters long and digits only
            //Taganrog ceritificates detection
            if (str[0].Length == 8 && str[0].All((x) => x >= '0' && x <= '9'))
                realPath = HostingEnvironment.MapPath("/Certificates") + @"\20" + str[0][1] + str[0][2] + @"\ТМЗ\" + str[0] + ".pdf";
            else //non-TMZ certificate have to look for the date
            {
                Match mtch = Regex.Match(str[1], pattern, RegexOptions.IgnoreCase);
                realPath = HostingEnvironment.MapPath("/Certificates") + ((mtch.Captures[0].Value.Length == 2) ?
                        @"\20" + mtch.Captures[0].Value + @"\КТЗ\" : @"\" + mtch.Captures[0].Value + @"\КТЗ\") 
                        + str[0].Replace('/', '_') + ".pdf";
            }
            return realPath;
        }
    }
}