using Classly.Models;
using Classly.Models.Login;
using Classly.Services.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Claims;

namespace Classly.Controllers
{
    public class LoginController : Controller
    {
        private IUserService _userService { get; set; }

        public LoginController(IUserService userService)
        {
            _userService = userService;
        }

        public IActionResult RegisterTutor()
        {

            ViewBag.IsTutor = true;
            return View();
        }

        public IActionResult Register(Guid? linkingTutorId)
        {
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


            ViewBag.Error = "Unable to register user";
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
        [Authorize]
        public IActionResult ForgotPassword()
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var allowedEmails = new[] { "daniel-einon@live.co.uk", "rickyjones28@gmail.com" };

            if (!allowedEmails.Contains(email))
            {
                return Forbid();
            }
            return View();
        }

        [Authorize]
        public IActionResult UpdatePassword(Guid code)
        {

            if(!TempCodeStore.ContainsValue(code))
            {
                return Forbid();
            }
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdatePassword(User user)
        {

            var emailIn = user.Email;

            var userIn = _userService.GetUser(emailIn);

            if (userIn == null)
            {
                ViewBag.Error = "Unable to process request";
            }

            userIn.Password = user.Password;

            var updated = await _userService.UpdateAsync(userIn, new CancellationToken());

            if (!updated)
            {
                ViewBag.Error = "unable to update";

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(User user)
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var allowedEmails = new[] { "daniel-einon@live.co.uk", "rickyjones28@gmail.com" };

            if (!allowedEmails.Contains(email))
            {
                return Forbid();
            }

            var emailIn = user.Email;

            var userIn = _userService.GetUser(emailIn);

            if (userIn == null)
            {
                ViewBag.Error = "Unable to process request";

                return View();
            }

            var tempCode = TempCodeStore.Generate(userIn.Email);
            ViewBag.TempCode = tempCode;
            ViewBag.Success = true;

            return View();
        }
        public IActionResult ForgotPasswordConfirmation(Guid changeRequestId)
        {

            //
            return View();
        }
    }

public static class TempCodeStore
    {
        private static readonly ConcurrentDictionary<string, (Guid Code, DateTime Expiry)> _codes
            = new();

        public static Guid Generate(string key)
        {
            var code = Guid.NewGuid();
            _codes[key] = (code, DateTime.UtcNow.AddMinutes(10));
            return code;
        }

        public static Guid? Get(string key)
        {
            if (_codes.TryGetValue(key, out var entry))
            {
                if (DateTime.UtcNow <= entry.Expiry)
                    return entry.Code;

                _codes.TryRemove(key, out _); // expired
            }
            return null;
        }

        public static bool ContainsValue(Guid code)
        {
            foreach (var kvp in _codes)
            {
                var (storedCode, expiry) = kvp.Value;
                if (storedCode == code)
                {
                    if (DateTime.UtcNow <= expiry)
                        return true;

                    _codes.TryRemove(kvp.Key, out _); // expired
                }
            }
            return false;
        }
    }

}
