using System.Web.Mvc;

namespace CoTE.Areas.WebPages
{
    public class WebPagesAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "WebPages";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "WebPages_default",
                "WebPages/{controller}.aspx/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
