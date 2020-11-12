using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CoTE.Areas.FormProcessor.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult HttpError()
        {
            Exception ex = null;

            try
            {
                ex = (Exception)HttpContext.Application[Request.UserHostAddress.ToString()];
            }
            catch
            {
            }

            if (ex != null)
            {
                ViewData["Description"] = ex.Message;
            }
            else
            {
                ViewData["Description"] = "An error occurred.";
            }

            ViewData["Title"] = "An error has occurred.  <br/><br/> An email has been sent to Technical Support and we will resolve this issues as soon as possible.  <br/><br/> We apologize for the inconvenience.";

            return View("Error");
        }

        public ActionResult Http404()
        {
            ViewData["Title"] = "The page you requested was not found";

            return View("Error");
        }

        // (optional) Redirect to home when /Error is navigated to directly  
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }
    }  
}
