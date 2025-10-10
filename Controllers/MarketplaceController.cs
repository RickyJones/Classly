using Classly.Dummy.Datasets;
using Microsoft.AspNetCore.Mvc;

namespace Classly.Controllers
{
    public class MarketplaceController : Controller
    {
        public IActionResult Index()
        {
            var courses = CoursesDummyDataset.GetCourses();
            return View(courses);
        }

        public IActionResult Basket(List<Guid> item)
        {
            var courses = CoursesDummyDataset.GetCourses().Where(x => item.Contains(x.Id)).ToList();
            return View(courses);
        }
    }
}
