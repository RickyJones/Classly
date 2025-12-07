using AspNetCoreGeneratedDocument;
using Classly.Models;
using Classly.Services.AI;
using Classly.Services.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;
using System.Diagnostics;
using System.Security.Claims;

namespace Classly.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserService _userService;

        public HomeController(ILogger<HomeController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }
        [AllowAnonymous] //tutors only atm. students added with bespoke link from tutor
        public IActionResult Landing()
        {
            ViewBag.IsTutor = true;
            return View();
        }

        public async Task<IActionResult> Index()
        {
            if (!HttpContext.User.Identity.IsAuthenticated) {
             
                return RedirectToAction("Landing");
            }
            if (User.Claims.Any(x => x.Type == "IsTutor"))
            {
                return RedirectToAction("TutorIndex");
            } else
            {
                return RedirectToAction("TempStudentDashboard");
            }
            return View();
        }

        public async Task <IActionResult> TempStudentDashboard()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userService.GetUser(Guid.Parse(userId));
            
            return View(user);
        }

        public IActionResult TutorIndex()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
