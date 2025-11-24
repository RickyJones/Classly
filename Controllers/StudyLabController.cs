using Classly.Models;
using Classly.Models.AIGen;
using Classly.Services;
using Classly.Services.AI;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;
using System.Text.Json;

namespace Classly.Controllers
{
    public class StudyLabController : Controller
    {
        private readonly ICourseNotesService _courseNotesService;
        public StudyLabController(ICourseNotesService courseNotesService)
        {
            _courseNotesService= courseNotesService;
        }
        public IActionResult Index()
        {
            return View();
        }

       

    }
}
