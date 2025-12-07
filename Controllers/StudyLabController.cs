using Classly.Models;
using Classly.Models.AIGen;
using Classly.Services;
using Classly.Services.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;
using System.Text.Json;

namespace Classly.Controllers
{
    [Authorize]
    public class StudyLabController : Controller
    {
        private readonly ICourseNotesService _courseNotesService;
        public StudyLabController(ICourseNotesService courseNotesService)
        {
            _courseNotesService= courseNotesService;
        }
        public IActionResult Index()
        {

            //_courseNotesService.get
            return View();
        }

       public async Task<IActionResult> ViewLessonNotes(Guid studentId)
        {

            var notes = await _courseNotesService.GetNotesForStudentAsync(studentId);
            return View(notes);
        }

    }
}
