using System.Web.Mvc;

namespace CoTE.Areas.FormProcessor
{
    public class FormProcessorAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "FormProcessor";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {

            context.MapRoute(
                "FormProcessor_default",
                "FormProcessor/{controller}.aspx/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );

        }
    }
}
