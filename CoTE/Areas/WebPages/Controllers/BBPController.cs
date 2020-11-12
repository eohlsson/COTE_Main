using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CoTE.Areas.WebPages.Controllers
{
    public class BBPController : Controller
    {
        //
        // GET: /WebPages/BBP/

        public ActionResult Index(string slide)
        {
            // if slide is null set to 1
            if (slide == null || slide == "")
            {
                slide = "1";
            }

            ViewBag.slide = slide;
            return View();
        }

    }
}
