using Classly.Models.AIGen;
using Classly.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Classly.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ICourseNotesService _courseNotesService;

        public AdminController(ICourseNotesService courseNotesService)
        {
            _courseNotesService = courseNotesService;
        }
        public IActionResult Index()
        {
            var prompts = _courseNotesService.GetAIPrompts();
            return View(prompts);
        }

        [HttpPost]
        public IActionResult AIPrompts(AIPromptsModel model)
        {
            _courseNotesService.UpdateAIPrompts(model);
            return View();
        }

    }
}
