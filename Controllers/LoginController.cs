using Classly.Models.Login;
using Microsoft.AspNetCore.Mvc;

namespace Classly.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(Login login)
        {
            return View();
        }
        public IActionResult Logout() { 

            return new RedirectToActionResult(nameof(Login), "Login", null);
        }
        public IActionResult ForgotPassword() { 
            return View();
        }
        [HttpPost]
        public IActionResult ForgotPassword(string emailAddress)
        {
            return View();
        }
        public IActionResult ForgotPasswordConfirmation(Guid changeRequestId) {

            //
            return View();
        }
    }
}
