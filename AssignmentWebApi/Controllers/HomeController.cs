using System.Web.Mvc;

namespace AssignmentWebApi.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Help Page";

            return View();
        }
    }
}
