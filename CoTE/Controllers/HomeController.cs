using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace SimpleMembershipDemo2.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            if (!WebSecurity.IsAuthenticated)
            {
                ViewBag.ErrorMessage = 0;
                Response.Redirect("/dotnet/webpages/webpage.aspx");
            }
            return View();
        }

    }
}
