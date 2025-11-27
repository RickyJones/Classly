using Classly.Models;
using Classly.Models.Login;
using Classly.Services.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Security.Claims;

namespace Classly.Controllers
{
    public class LoginController : Controller
    {
        private IUserService _userService {  get; set; }

        public LoginController(IUserService userService)
        {
            _userService = userService;
        }

        public IActionResult RegisterTutor() {

            ViewBag.IsTutor = true;
            return View();
        }

        public IActionResult Register(Guid? linkingTutorId) {
            ViewBag.TutorId = linkingTutorId;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            CancellationToken cancellationToken = HttpContext.RequestAborted;
            //user.IsTutor = HttpContext.Request.Form.
            var registered = await _userService.CreateAsync(user, cancellationToken);
            if (registered != null) return RedirectToAction("Login");

            //error
            return View();
        }
        public async Task<IActionResult> Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(Login login)
        {
            var user = _userService.GetUser(login.Email);

            if (user == null || !_userService.ValidatePassword(login.Password, user.Password))
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                ViewBag.Error = "Invalid login attempt";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email)
            };

            if (user.IsTutor)
            {
                claims.Add(new Claim("IsTutor", user.IsTutor.ToString()));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true
            };

            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Login");
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
