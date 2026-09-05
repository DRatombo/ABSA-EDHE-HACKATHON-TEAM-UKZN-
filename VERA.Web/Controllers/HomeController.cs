using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VERA.Web.Models;

namespace VERA.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Opens the home page
        public IActionResult Index()
        {
            return View();
        }

        // Opens the privacy page
        public IActionResult Privacy()
        {
            return View();
        }

        // Opens the How It Works page
        public IActionResult HowItWorks()
        {
            return View("~/Views/HowItWorks/HowItWorks.cshtml");
        }

        // Opens the For SMEs page
        [HttpGet]
        public IActionResult ForSMEs()
        {
            return View();
        }

        // Opens the For Funders page
        [HttpGet]
        public IActionResult ForFunders()
        {
            return View();
        }

        // Opens the About page
        [HttpGet]
        public IActionResult About()
        {
            return View();
        }

        // Opens the error page
        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId =
                    Activity.Current?.Id ??
                    HttpContext.TraceIdentifier
            });
        }
    }
}