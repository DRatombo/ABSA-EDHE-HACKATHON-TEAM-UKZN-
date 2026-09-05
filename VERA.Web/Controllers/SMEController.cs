using Microsoft.AspNetCore.Mvc;

namespace VERA.Web.Controllers
{
	public class SMEController : Controller
	{
		public IActionResult Dashboard()
		{
			return View();
		}

		public IActionResult Opportunities()
		{
			return View();
		}

		public IActionResult NewOpportunity()
		{
			return View();
		}
	}
}
