using Newtonsoft.Json.Linq;
using SimpleRESTApplication.Alumni;
using SimpleRESTApplication.Models;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;

namespace SimpleRESTApplication.Controllers
{
    /// <summary>
    /// Provides method to get certificates PDF with/without inscription
    /// </summary>
    public class CertificatesController : ApiController
    {
        [HttpGet]
        [ContentTypeRoute("api/certificates", "application/x-www-form-urlencoded")]  // need to add it to allow routing even if content type is not present since form-urlencoded is by default
        public HttpResponseMessage Get(string id, string pos = "", string name = "")
        {
            HttpResponseMessage httpResponseMessage = null;

            Logger.WriteToLog("got id " + id + " - " + DateTime.Now.ToString());
            bool needToRotate;
            string file_path = GetFullPath(ref id, out needToRotate);

            if (file_path != null)
            {
                try
                {
                    byte[] fileBytes = File.ReadAllBytes(file_path);
                    using (MemoryStream dataStream = new MemoryStream(fileBytes),
                                        outputStream = new MemoryStream())
                    {
                        httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
                        if (pos != null && !pos.Equals(""))
                        {
                            if (name == null || name.Equals("")) throw new ArgumentNullException("name",
                                    "Name cannot be empty when position provided");
                            PdfInscriptor pdf = new PdfInscriptor(dataStream);
                            pdf.MakeInscription("Копия верна" + Environment.NewLine + pos
                                    + Environment.NewLine + Environment.NewLine + name, needToRotate);
                            pdf.document.Save(outputStream, false);
                            httpResponseMessage.Content = new ByteArrayContent(outputStream.ToArray());
                        }
                        else
                        {
                            httpResponseMessage.Content = new ByteArrayContent(dataStream.ToArray());
                        }
                    }
                    httpResponseMessage.Content.Headers.ContentDisposition =
                        new ContentDispositionHeaderValue("attachment");
                    httpResponseMessage.Content.Headers.ContentDisposition.FileName = id + ".pdf";
                    httpResponseMessage.Content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/pdf");
                }
                catch (Exception ex)
                {
                    httpResponseMessage = Request.CreateResponse(HttpStatusCode.NotFound);
#if DEBUG
                    httpResponseMessage.Content = new StringContent(ex.Message);
#endif
                    return httpResponseMessage;
                }
            }
            else
            {
                httpResponseMessage = Request.CreateResponse(HttpStatusCode.NotFound);
#if DEBUG
                httpResponseMessage.Content = new StringContent(id);
#endif
            }
            return httpResponseMessage;
        }

        /// <summary>
        /// Returns welcome message. For test purproses only.
        /// </summary>
        /// <returns><see cref="HttpResponseMessage"/></returns>
        [HttpGet]
        [ContentTypeRoute("api/certificates", "application/x-www-form-urlencoded")]
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
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ContentTypeRoute("api/certificates", "application/x-www-form-urlencoded")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] InscriptionData id)
        {
            return Get(id: id.id, pos: id.pos, name: id.name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns><see cref="HttpResponseMessage"/></returns>
        [ContentTypeRoute("api/certificates", "application/json")]
        public HttpResponseMessage Post(HttpRequestMessage id)
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            if (id == null)
            {
                httpResponseMessage.Content = new StringContent("Empty id");
                httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
            }
            else
            {
                Task<string> id_json = id.Content.ReadAsStringAsync();
                try
                {
                    InscriptionData inscr = JObject.Parse(id_json.Result).ToObject<InscriptionData>();
                    return Post(inscr);
                }
                catch
                {
                    httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
                }
            }
            return httpResponseMessage;
        }

        /// <summary>
        /// Returns a full absolute path to the file representing requested certificate
        /// </summary>
        /// <param name="fileName">a name of the certificate: <see cref="String"/></param>
        /// <param name="needToRotate">necessity to rotate defined on the base of the ceritificate type: <see cref="Boolean"/></param>
        /// <returns>absolute path to the certificate file</returns>
        private string GetFullPath(ref string fileName, out bool needToRotate)
        {
            string pattern = @"\d{4}|(?<=\.)\d{2}(?=\D+$)";
            string realPath = null;
            needToRotate = true;
           
            //create aray of separators
            char[] separators = new char[] { (char)0x20, (char)0x09, 'o', 'O', 'о', 'О' };

            //split string to separate certificate number from the date
            string[] partsOfID = fileName.Split(separator: separators,
                                          options: StringSplitOptions.RemoveEmptyEntries,
                                          count: 2);//do not need more than 2 parts
            //in case no delimiters met in the string
            if (partsOfID.Count() != 2) { fileName = "error at splitting";  return null; }

            //check if certificate is 8 characters long and digits only
            //Taganrog ceritificates detection
            if (partsOfID[0].Length == 8 && partsOfID[0].All((x) => x >= '0' && x <= '9'))
                realPath = HostingEnvironment.MapPath("/Certificates") + @"\20" + partsOfID[0][1] + partsOfID[0][2] 
                        + @"\ТМЗ\" + partsOfID[0] + ".pdf";
            //non-TMZ certificate 
            else if (partsOfID[0].Contains('/')) //for the present time the only attribute of the KTZ certificate
            {
                //KTZ -- have to look for the date
                Match mtch = Regex.Match(partsOfID[1], pattern, RegexOptions.IgnoreCase);
                try // in case wrong year is provided (neither 2 nor 4 digits)
                {
                    realPath = HostingEnvironment.MapPath("/Certificates") + ((mtch.Captures[0].Value.Length == 2) ?
                        @"\20" + mtch.Captures[0].Value + @"\КТЗ\" : @"\" + mtch.Captures[0].Value + @"\КТЗ\")
                        //replacing is obligatory to comply with OS file naming rules
                        + partsOfID[0].Replace('/', '_') + ".pdf";
                    fileName = fileName.Replace('/', '_');
                    needToRotate = false;
                }
                catch { }
            }
            return realPath;
        }
    }
}