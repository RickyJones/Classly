using Classly.Models;
using Classly.Models.AIGen;
using Classly.Services;
using Classly.Services.AI;
using Classly.Services.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenAI.Chat;
using System.Text.Json;

namespace Classly.Controllers
{
    [Authorize]
    public class StudyLabController : Controller
    {
        private readonly ICourseNotesService _courseNotesService;
        private readonly IHomeworkSubmissionService _homeworkService;
        public StudyLabController(ICourseNotesService courseNotesService, IHomeworkSubmissionService homeworkSubmission)
        {
            _courseNotesService= courseNotesService;
            _homeworkService= homeworkSubmission;
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

        //public async Task<IActionResult> CreateHomeworkSubmission(Guid courseNoteId)
        //{

        //    var notes = await _courseNotesService.GetCourseNoteAsync(courseNoteId);
        //    return View(notes);
        //}

        public async Task<IActionResult> HomeworkSubmission(Guid homeworkSubmissionId, Guid courseNoteId)
        {
            HomeworkSubmission hw = null;
            if (homeworkSubmissionId != Guid.Empty)
            {
                hw = _homeworkService.GetSubmissionById(homeworkSubmissionId);
            }

            if(hw == null)
            {
                hw = new HomeworkSubmission
                {
                    LinkedCourseNoteId = courseNoteId.ToString(),
                };
            }
            var notes = await _courseNotesService.GetCourseNoteAsync(courseNoteId);
            ViewBag.Notes = JsonConvert.SerializeObject(notes);
            return View(hw);
        }

        [HttpPost]
        public IActionResult SaveHomework(HomeworkSubmission homeworkSubmission)
        {
            if(homeworkSubmission.Id == Guid.Empty)
            {
                _homeworkService.AddSubmission(homeworkSubmission);
            } else
            {
                _homeworkService.UpdateSubmission(homeworkSubmission);
            }
            if (homeworkSubmission.MarkAsComplete)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("HomeworkSubmission", new { homeworkSubmissionId = homeworkSubmission.Id, courseNoteId = homeworkSubmission.LinkedCourseNoteId });
            }
        }

    }
}
