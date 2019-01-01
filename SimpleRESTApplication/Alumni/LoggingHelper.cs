using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SimpleRESTApplication.Alumni
{
    public class LoggingHelper : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            // log request body
            string requestBody = await request.Content.ReadAsStringAsync();
            Logger.WriteToLog(requestBody);
            Logger.WriteToLog(ipAddress);

            // let other handlers process the request
            var result = await base.SendAsync(request, cancellationToken);

            if (result.Content != null)
            {
                // once response body is ready, log it
                var responseBody = await result.Content.ReadAsStringAsync();
                //Logger.WriteToLog(responseBody);
            }

            return result;
        }
    }
}