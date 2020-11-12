using System.Web.Mvc;

namespace CoTE.Areas.LACES
{
    public class LACESAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "LACES";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "LACES_default",
                "LACES/{controller}.aspx/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
