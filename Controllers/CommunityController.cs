using Microsoft.AspNetCore.Mvc;

namespace Classly.Controllers
{
    public class CommunityController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
