using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Classly.Controllers
{
    [Authorize]
    public class CommunityController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
