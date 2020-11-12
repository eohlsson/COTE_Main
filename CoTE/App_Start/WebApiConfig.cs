using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace CoTE
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}.aspx/{action}/{form_id}/{index_id}",
                defaults: new { form_id = RouteParameter.Optional, index_id = RouteParameter.Optional }
            );
        }
    }
}
