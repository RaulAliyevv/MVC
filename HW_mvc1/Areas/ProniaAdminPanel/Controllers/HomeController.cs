using Microsoft.AspNetCore.Mvc;

namespace HW_mvc1.Areas.ProniaAdminPanel.Controllers
{
    [Area("ProniaAdminPanel")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
