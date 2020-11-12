using System.Web.Mvc;

namespace CoTE.Areas.SCE
{
    public class SCEAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "SCE";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "SCE_default",
                "SCE/{controller}.aspx/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
