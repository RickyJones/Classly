using System.Configuration;
using System.Diagnostics;
using Classly.Models;
using Classly.Services.AI;
using Classly.Services.Data;
using Microsoft.AspNetCore.Mvc;

namespace Classly.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            //var airesp = ChatGPTService.AskAI("Are you working?");
            var gotIt = UserService.GetUser("test@mail.com");
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
