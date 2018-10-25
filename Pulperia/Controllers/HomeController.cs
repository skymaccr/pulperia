using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pulperia.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "PULPERIA ASOGUARUMO";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "jdasoguarumo@cecropiasolutions.com";

            return View();
        }
    }
}