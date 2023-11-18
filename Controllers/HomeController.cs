using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ContactManager.Controllers
{

    public class HomeController : Controller
    {
        private ContactsManagerContext dbContext;

        public ActionResult Index ()
        {

            dbContext = new ContactsManagerContext();
            ViewBag.Title = $"Home Page";

            return View();
        }
    }
}
