using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Classly.Controllers.API
{
    public class BasketController: Controller
    {
        public IActionResult AddToBasket(Guid courseId)
        {
            var courseIds = JsonConvert.DeserializeObject<List<Guid>>(HttpContext.Session.GetString("BasketCourseIds") ?? "") ?? [];
            courseIds.Add(courseId);
            HttpContext.Session.SetString("BasketCourseIds", JsonConvert.SerializeObject(courseIds));

            return Ok(courseIds.Count);
        }

        public IActionResult RemoveCourseFromBasket(Guid courseId)
        {
            var courseIds = JsonConvert.DeserializeObject<List<Guid>>(HttpContext.Session.GetString("BasketCourseIds") ?? "") ?? [];
            courseIds.Remove(courseId);
            HttpContext.Session.SetString("BasketCourseIds", JsonConvert.SerializeObject(courseIds));

            return Ok(courseIds.Count);
        }

        public IActionResult ClearBasket()
        {
            HttpContext.Session.Remove("BasketCourseIds");

            return Ok();
        }
    }
}
