using Microsoft.AspNetCore.Mvc;

namespace Classly.Controllers.Tutor
{
    public class StudentManagementController : Controller
    {
        public IActionResult Index()
        {
            return View("Index");
        }
    }
}
