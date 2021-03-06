﻿using Newtonsoft.Json.Linq;
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
using System.Collections.Generic;

namespace SimpleRESTApplication.Controllers
{
    /// <summary>
    /// Provides method to get certificates PDF with/without inscription
    /// </summary>
    public class CertificatesController : ApiController
    {
        /// <summary>
        /// Standard method to GET file using url-encoded parameters
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pos"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet]
        [ContentTypeRoute("api/certificates", "application/x-www-form-urlencoded")]  // need to add it to allow routing even if content type is not present since form-urlencoded is by default
        public HttpResponseMessage Get(string id, string pos = "", string name = "")
        {
            HttpResponseMessage httpResponseMessage = CheckSignature(Request.RequestUri.Query);
            if (httpResponseMessage.StatusCode != HttpStatusCode.OK) return httpResponseMessage;
            Logger.WriteToLog("got id " + id + " - " + DateTime.Now.ToString());
            return CreateResponse(id, pos, name);
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
        /// Gets data from the POST body as ww-form-urlencoded
        /// </summary>
        /// <param name="id">x-ww-form-urlencoded request</param>
        /// <returns></returns>
        [ContentTypeRoute("api/certificates", "application/x-www-form-urlencoded")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] InscriptionData id)
        {
            HttpResponseMessage httpResponseMessage = CheckSignature(Request.Content.ToString());
            if (httpResponseMessage.StatusCode != HttpStatusCode.OK) return httpResponseMessage;
            Logger.WriteToLog("got " + Request.Content.ToString());
            return CreateResponse(id: id.id, pos: id.pos, name: id.name);
        }

        /// <summary>
        /// Gets data POSTed as JSON
        /// </summary>
        /// <param name="id">JSON-formatted request</param>
        /// <returns><see cref="HttpResponseMessage"/></returns>
        [ContentTypeRoute("api/certificates", "multipart/form-data")]
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
                switch (id.Content.Headers.ContentType.MediaType)
                {
                    case "application/json":
                        try
                        {
                            httpResponseMessage = CheckSignature(id_json.Result);
                            if (httpResponseMessage.StatusCode != HttpStatusCode.OK) return httpResponseMessage;
                            Logger.WriteToLog("got " + id_json);

                            InscriptionData inscr = JObject.Parse(id_json.Result).ToObject<InscriptionData>();
                            return CreateResponse(id: inscr.id, pos: inscr.pos, name: inscr.name);
                        }
                        catch
                        {
                            httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
                        }
                        break;
                    case "multipart/form-data":
                        if (id.Content.Headers.ContentType.Parameters.Count == 0 ||
                            id.Content.Headers.ContentType.Parameters.Count((x) => x.Name == "boundary") != 1)
                        {
                            httpResponseMessage.StatusCode = HttpStatusCode.NotAcceptable;
                        }
                        string boundary=String.Empty;
                        try
                        {
                            boundary = id.Content.Headers.ContentType.Parameters
                                    .Select((x) => new { x.Name, x.Value })
                                    .FirstOrDefault(x => x.Name == "boundary").Value;
                        }
                        catch
                        {
                            httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
                        }
                        string[] parts = id_json.Result.Split(new string[] { boundary }, StringSplitOptions.RemoveEmptyEntries);
                        
                        break;
                    default:
                        break;
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

        private HttpResponseMessage CreateResponse(string id, string pos, string name)
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
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
                        if (pos != null && !pos.Equals(""))
                        {
                            if (name == null || name.Equals("")) throw new ArgumentNullException("name",
                                    "Name cannot be empty when position provided");
                            PdfInscriptor pdf = new PdfInscriptor(dataStream);
                            pdf.MakeInscription("Копия верна" + Environment.NewLine + pos
                                    + Environment.NewLine + Environment.NewLine + name, needToRotate);
                            pdf.document.Save(outputStream, false);
                            httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
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

        private HttpResponseMessage CheckSignature(string strToCheck)
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            IEnumerable<string> values;

            if (!(Request.Headers.Contains("X-Signature") && Request.Headers.TryGetValues("X-Signature", out values)))
            {
                return httpResponseMessage = Request.CreateResponse(HttpStatusCode.BadRequest,
                    "Dully signed responces only accepted");
            }
            else
            {
                try
                {
                    X509EncDec encDec = new X509EncDec(Properties.Settings.Default.X509Name);
                    if (!encDec.VerifySignature(values.ToList()[0], strToCheck))
                    {
                        return httpResponseMessage = Request.CreateResponse(HttpStatusCode.BadRequest,
                        "Bogus signature");
                    }
                }
                catch (Exception ex)
                {
                    return httpResponseMessage = Request.CreateResponse(HttpStatusCode.BadRequest,
                    "Error verifying the signature"
#if DEBUG
                    + Environment.NewLine + ex.Message
#endif
                    );
                }
            }
            httpResponseMessage.StatusCode = HttpStatusCode.OK;
            return httpResponseMessage;
        }
    }
}