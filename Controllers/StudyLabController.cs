using Microsoft.AspNetCore.Mvc;

namespace Classly.Controllers
{
    public class StudyLabController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
