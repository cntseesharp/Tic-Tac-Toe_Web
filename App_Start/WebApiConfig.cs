using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace WebApplication1
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //Работаем без WebAPI. Переключаемся на SignalR

            // Web API routes
            //config.MapHttpAttributeRoutes();

            //config.Routes.MapHttpRoute("ReplayApi", "api/{controller}/{action}");
            //config.Routes.MapHttpRoute(name: "DefaultApi", routeTemplate: "api/{controller}");
        }
    }
}
