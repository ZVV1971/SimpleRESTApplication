﻿using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRESTApplication.Alumni;
using System.Web.Http;

namespace SimpleRESTApplication
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Конфигурация и службы веб-API

            // Маршруты веб-API
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.MessageHandlers.Add(new LoggingHelper());
        }
    }
}
