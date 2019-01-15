//Taken from Web API Routing by Content-Type
//Implementing a custom Route Attribute for Web API that considers Content-Type
//https://massivescale.com/web-api-routing-by-content-type/

using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using System.Net.Http;
using System.Web.Http.Routing;

namespace SimpleRESTApplication.Alumni
{
    /// <summary>
    /// Adding a Constraint to the standard Route attribute. 
    /// This evaluates the request’s Content-Type and compares it to the method’s Content-Type constraint. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class ContentTypeRouteAttribute : RouteFactoryAttribute
    {
        public string ContentType { get; private set; }

        public ContentTypeRouteAttribute(string template, string contentType)
            : base(template)
        {
            ContentType = contentType;
        }

        public override IDictionary<string, object> Constraints
        {
            get
            {
                var constraints = new HttpRouteValueDictionary();
                constraints.Add("Content-Type", new ContentTypeConstraint(ContentType));
                return constraints;
            }
        }

        internal class ContentTypeConstraint : IHttpRouteConstraint
        {
            public ContentTypeConstraint(string allowedMediaType)
            {
                AllowedMediaType = allowedMediaType;
            }

            public string AllowedMediaType { get; private set; }

            public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, 
                IDictionary<string, object> values, HttpRouteDirection routeDirection)
            {
                if (routeDirection == HttpRouteDirection.UriResolution)
                    return (GetMediaHeader(request) == AllowedMediaType);
                else
                    return true;
            }

            private string GetMediaHeader(HttpRequestMessage request)
            {
                IEnumerable<string> headerValues;
                if (request.Content.Headers.TryGetValues("Content-Type", out headerValues) && headerValues.Count() == 1)
                    return request.Content.Headers.ContentType.MediaType;
                else
                    return "application/x-www-form-urlencoded";
            }
        }
    }
}