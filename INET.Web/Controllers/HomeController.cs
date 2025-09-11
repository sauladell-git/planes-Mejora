using INET.Utils.Helpers;
using System.Web.Mvc;

namespace INET.Web.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            if (!SessionHelper.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            return View();
        }
    }
}
