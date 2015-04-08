using System.Web.Mvc;

namespace GitReleaseNotes.Website.Controllers
{
    public class HomeController : ControllerBase
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}