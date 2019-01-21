using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleRESTApplication.Alumni
{
    public class RemoveServerHeaderModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.PreSendRequestHeaders += OnPreSendRequestHeaders;
        }

        public void Dispose() { }

        void OnPreSendRequestHeaders(object sender, EventArgs e)
        {
            //HttpContext.Current.Response.Headers.Remove("X-SourceFiles");
        }
    }
}