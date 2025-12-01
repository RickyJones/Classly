using Classly.Models;
using Classly.Models.AIGen;
using Classly.Services;
using Classly.Services.AI;
using Classly.Services.Data;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Classly.Controllers
{
    public class CourseNotesController : Controller
    {
        private readonly ICourseNotesService _courseNotesService;
        private readonly IUserService _userService;
        public CourseNotesController(ICourseNotesService courseNotesService, IUserService userService)
        {
            _courseNotesService = courseNotesService;
            _userService = userService;
        }

        public IActionResult Index()
        {

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(Guid studentId)
        {
            var notes = await _courseNotesService.GetNotesForStudentAsync(studentId);
            ViewBag.StudentId = studentId;
            return View(notes);
        }

        public async Task<IActionResult> UploadStudyNotes(Guid studentId)
        {
            //var notes = await _courseNotesService.GetNotesForStudentAsync(studentId);
            ViewBag.StudentId = studentId;
            return View();
        }

        public async Task<IActionResult> ViewNote(Guid noteId)
        {
            var note = await _courseNotesService.GetCourseNoteAsync(noteId);
            var student = await _userService.GetUser(note.StudentId);
            var tudor = await _userService.GetUser(note.TutorId);
            ViewBag.Student = student.Name;
            ViewBag.Teacher = tudor.Name;
            return View(note);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> UploadStudyNotes(ClassNotesViewModel model)
        {
            string notesContent = model.RichText;

            if (model.FileUpload != null && model.FileUpload.Length > 0)
            {
                var contentType = model.FileUpload.ContentType;

                if (contentType.StartsWith("text/") || contentType == "application/json" || contentType == "application/xml")
                {
                    // Treat as text
                    using var reader = new StreamReader(model.FileUpload.OpenReadStream(), Encoding.UTF8);
                    notesContent = await reader.ReadToEndAsync();
                }
                else
                {
                    // Treat as binary
                    using var ms = new MemoryStream();
                    await model.FileUpload.CopyToAsync(ms);
                    var fileBytes = ms.ToArray();

                    // You can store the bytes, or hand them off to a parser depending on type
                    // e.g. if PDF: use a PDF library to extract text
                    notesContent = $"[Binary file uploaded: {contentType}, {fileBytes.Length} bytes]";
                }
            }


            var difficulty = model.Difficulty;

            var client = new ChatClient(model: "gpt-3.5-turbo", apiKey: TestKeys.AIKey);

            var messages = new List<ChatMessage>
            {
                ChatMessage.CreateSystemMessage("You are a helpful teaching assistant."),
                ChatMessage.CreateUserMessage("from the vocabulary covered in the following notes, create a raw HTML table with no styling with the fields vocabulary, definition and example. Ensure you populate very cell as appropriate :\n" + notesContent +". Wrap the HTML I need with <content></content>")

            };

            var tablesResponse = await ChatGPTService.AskAIAsync(messages);

            string startTag = "<content>";
            string endTag = "</content>";

            int startIndex = tablesResponse.IndexOf(startTag) + startTag.Length;
            int endIndex = tablesResponse.IndexOf(endTag);

            tablesResponse = tablesResponse.Substring(startIndex, endIndex - startIndex);

            //generate mixed homework

            var specifyHomeworkType = string.Join(",", model.HomeworkTypes);// "fill in the blanks, Multiple choice, Sentence matching / Half-sentence matching, create your own sentences ";
            var askAIForHomework = "";
            bool createNextLessonPlan = model.GenerateLessonPlan;


            var homeworkMessages = new List<ChatMessage>
            {
                ChatMessage.CreateSystemMessage("You are a helpful teaching assistant."),
                ChatMessage.CreateUserMessage($"With no preamble or other explanations, Generate homework to focus on the vocabulary covered. Group the homework into question type(s) ({specifyHomeworkType}). Produce 10 questions for each question type at difficulty:" + difficulty)
            };

            var homeworkResponse = await ChatGPTService.AskAIAsync(homeworkMessages);


            string lessonPlanResponse = string.Empty;
            if (createNextLessonPlan)
            {
                var lessonPlan = new List<ChatMessage>
                {
                     ChatMessage.CreateSystemMessage("You are a helpful teaching assistant."),
                     ChatMessage.CreateUserMessage($"Genrate a lesson plan to further develop the vocabulary covered. No preamble or explanation.")
                };

                lessonPlanResponse = await ChatGPTService.AskAIAsync(lessonPlan);
            }
            


            // Example: store AI output as JSON
            var structured = new
            {
                Notes = tablesResponse,
                Homework = homeworkResponse,
                LessonPlan = createNextLessonPlan,
                model.Difficulty,
                Timestamp = DateTime.UtcNow
            };

            string json = JsonSerializer.Serialize(structured, new JsonSerializerOptions { WriteIndented = true });

            var createdNote = await _courseNotesService.CreateCourseNoteAsync(new Models.Courses.CourseNote
            {
                CreatedAt = DateTime.UtcNow,
                Difficulty = difficulty,
                Homework = homeworkResponse,
                Notes = tablesResponse,
                TutorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                StudentId = model.StudentId,
                NextLessonPlan = lessonPlanResponse
            });
            //await System.IO.File.WriteAllTextAsync("StudyNotes.json", json);


            return new RedirectToActionResult("ViewNote", "CourseNotes", new { noteId = createdNote.Id });
            // Pass AI output to a view
            //return View("ViewAIGen", new AINotesResponse{ tablesResponse = tablesResponse, tasks = homeworkResponse });
        }
    }
}
