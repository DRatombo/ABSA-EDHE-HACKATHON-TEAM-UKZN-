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


        // Opens the SME opportunities page
        public IActionResult Opportunities()
        {
            return View();
        }


        // Opens the new opportunity page
        public IActionResult NewOpportunity()
        {
            return View();
        }
    }
}