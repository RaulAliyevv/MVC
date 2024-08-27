using Microsoft.AspNetCore.Mvc;

namespace HW_mvc1.Areas.ProniaAdminPanel.Controllers
{
	[Area("ProniaAdminPanel")]
	public class ProductImageController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
