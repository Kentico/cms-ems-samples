using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace CustomWebApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.Routes.MapHttpRoute("customapi", "customapi/{controller}/{id}", new { id = RouteParameter.Optional });
            config.MapHttpAttributeRoutes();
        }
    }
}
