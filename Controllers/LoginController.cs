using Classly.Models;
using Classly.Models.Login;
using Classly.Services.Data;
using Microsoft.AspNetCore.Mvc;

namespace Classly.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Register() { return View(); }
        [HttpPost]
        public IActionResult Register(User user)
        {
            var registered = UserService.RegisterUser(user.Name, user.Email, user.Password);
            if (registered) return RedirectToAction("Login");

            //error
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(Login login)
        {
            var verified = UserService.VerifyLogin(login.Email, login.Password);
            if (verified) return RedirectToAction("index", "home");

            ViewBag.Error = "Incorrect email / password";
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
