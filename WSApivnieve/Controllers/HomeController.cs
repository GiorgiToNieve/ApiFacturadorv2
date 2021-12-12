using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace WSApivnieve.Controllers
{

    [System.Web.Http.RoutePrefix("servicio/Home")]
    public class HomeController : Controller
    {
        // GET: Home
        [System.Web.Http.HttpGet]
        public ActionResult Index()
        {
            return View();
        }
    }
}