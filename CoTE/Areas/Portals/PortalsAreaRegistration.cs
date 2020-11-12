using System.Web.Mvc;

namespace CoTE.Areas.Portals
{
    public class PortalsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Portals";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Portals_default",
                "Portals/{controller}.aspx/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
