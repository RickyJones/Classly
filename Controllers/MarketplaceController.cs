using Classly.Dummy.Datasets;
using Classly.Services.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Classly.Controllers
{
    [Authorize]
    public class MarketplaceController : Controller
    {
        private readonly ICourseService _courseService;
        public MarketplaceController(ICourseService courseService)
        {
            _courseService = courseService;       
        }
        public async Task<IActionResult> Index()
        {
            var courses = await _courseService.GetAllCoursesAsync();//CoursesDummyDataset.GetCourses();
            return View(courses);
        }

        public async Task<IActionResult> Basket()
        {
            var courseIds = JsonConvert.DeserializeObject<List<Guid>>(HttpContext.Session.GetString("BasketCourseIds") ?? "") ?? [];
            var courses = await _courseService.GetAllCoursesAsync();
            courses = courses.Where(c => courseIds.Contains(c.Id))?.ToList() ?? [];
            return View(courses);
        }
    }
}
