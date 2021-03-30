using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SleepyTimeSoaps.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult AccessDenied()
        {
            return View();
        }

        public ActionResult Error(string aspxerrorpath)
        {
            ViewBag.Message = aspxerrorpath;

            return View();
        }
    }
}