// main controller for site

using System.Web.Mvc;
using WebRole1.Models;

namespace WebRole1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        // GET: home/search
        [HttpGet]
        public ActionResult Search()
        {
            return View();
        }

        // POST: home/search
        [HttpPost]
        public ActionResult Search(Search searchFor)
        {
            return View(searchFor);
        }
    }
}