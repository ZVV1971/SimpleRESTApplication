using System.IO;
using System.Linq;
using System;
using System.Web.Http;
using System.Web.Mvc;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace SimpleRESTApplication.Controllers
{
    public class CertificatesController : ApiController
    {
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Get(string id)
        {
            HttpResponseMessage httpResponseMessage = null;
            //// Путь к файлу
            string file_path = GetFullPath(id);

            if (file_path != null)
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
            else
            {
                httpResponseMessage = Request.CreateResponse(HttpStatusCode.NotFound);
            }
            return httpResponseMessage;
            //return fileBytes;
        }

        /// <summary>
        /// Returns a full absolute path to the file representing requested certificate if it exists
        /// otherwise returns null
        /// </summary>
        /// <param name="fileName">a name of the certificate</param>
        /// <returns>absolute path to the certificate file or null if anything goes wrong</returns>
        public string GetFullPath(string fileName)
        {
            string baseF;
            string pattern = @"\d{4}|\d{2}$";
            try
            {
                baseF = Properties.Settings.Default.BaseFolder;
            }
            catch
            {
                return null;
            }

            //create aray of separators
            char[] sep = new char[] { (char)0x20 };
           
            //split string to separate certificate number from the date
            string[] str = fileName.Split(separator: sep,
                options: StringSplitOptions.RemoveEmptyEntries);
            
            //if base folder set up in the properties does not exists return null
            if (Directory.Exists(baseF))
            {
                //check if certificate is 8 characters long and digits only
                //Taganrog ceritificates detection
                if (str[0].Length == 8 && str[0].All((x)=> x>='0' && x<='9'))
                {
                    if (Directory.Exists(baseF += "20" + str[0][1] + str[0][2] + @"\ТМЗ\")) 
                    {
                        if (File.Exists(baseF += str[0] + ".pdf")) return baseF;
                    }
                }
                else //non-TMZ certificate have to look for the date
                {
                    if (str.Count() >= 3)
                    {
                        Match mtch = Regex.Match(str[2], pattern, RegexOptions.IgnoreCase);
                        if(Directory.Exists(baseF += (mtch.Captures[0].Value.Length==2)?"20" + mtch.Captures[0].Value: mtch.Captures[0].Value + @"\КТЗ\"))
                        {
                            if (File.Exists(baseF += str[0].Replace('/', '_') + ".pdf")) return baseF; 
                        }
                    }
                }
            }
            return null;
        }
    }
}