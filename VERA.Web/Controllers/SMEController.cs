using Microsoft.AspNetCore.Mvc;

namespace VERA.Web.Controllers
{
	public class SMEController : Controller
	{
		public IActionResult Dashboard()
		{
			return View();
		}
	}
}
