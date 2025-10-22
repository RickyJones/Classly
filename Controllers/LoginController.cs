using Classly.Models;
using Classly.Models.Login;
using Classly.Services.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Classly.Controllers
{
    public class LoginController : Controller
    {
        private  SignInManager<User> _signInManager {  get; set; }
        private IUserService _userService {  get; set; }

        public LoginController(SignInManager<User> signIn, IUserService userService)
        {
            _signInManager = signIn;
            _userService = userService;
        }

        public IActionResult Register() { return View(); }
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            CancellationToken cancellationToken = HttpContext.RequestAborted;
            var registered = await _userService.CreateAsync(user, cancellationToken);
            if (registered.Succeeded) return RedirectToAction("Login");

            //error
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(Login login)
        {
            var user = _userService.GetUser(login.Email);

            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user.Email, login.Password, true, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    // Login successful
                    return RedirectToAction("index", "home");
                }
            }


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
