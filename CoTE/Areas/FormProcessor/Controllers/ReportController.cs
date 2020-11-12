using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcReportViewer;

namespace CoTE.Areas.FormProcessor.Controllers
{
    public class ReportController : Controller
    {
        //
        // GET: /FormProcessor/Report/

        [Authorize]
        public ActionResult Index()
        {
            var cookie = HttpContext.Request.Cookies["authenticated_user"];
            string username = cookie.Value; 
            string password = "";

            
            // Get username
            username = "";

            // Get password
            password = "";

            // Set viewbag
            ViewBag.username = username;
            ViewBag.param1 = password;

            return View();
        }

    }
}
