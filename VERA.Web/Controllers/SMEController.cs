using Microsoft.AspNetCore.Mvc;

namespace VERA.Web.Controllers
{
    // Handles SME pages
    public class SMEController : Controller
    {
        // Opens the SME dashboard
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}