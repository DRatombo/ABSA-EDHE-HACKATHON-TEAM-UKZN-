using Microsoft.AspNetCore.Mvc;
using VERA.Web.ViewModels;
using VERA.Models.ViewModels;

namespace VERA.Web.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // your Register POST remains here


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            TempData["LoginSuccess"] = "Welcome back to VERA.";

            return RedirectToAction("Index", "Home");
        }
    }
}