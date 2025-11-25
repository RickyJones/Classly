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
                ChatMessage.CreateUserMessage("As a raw HTML table, organise these notes into sections (vocab, phrases, idioms, grammer, etc) each along with definition and example. Output the HTML table inside <html_output>...</html_output> tags.:\n" + notesContent)

            };

            var tablesResponse = await ChatGPTService.AskAIAsync(messages);

            //string response = /* AI response */;

            // Extract between <html_output> and </html_output>
            int start = tablesResponse.IndexOf("<html_output>") + "<html_output>".Length;
            int end = tablesResponse.IndexOf("</html_output>");
            string htmlTable = tablesResponse.Substring(start, end - start);


            //generate mixed homework

            var homeworkMessages = new List<ChatMessage>
            {
                ChatMessage.CreateSystemMessage("You are a helpful teaching assistant."),
                ChatMessage.CreateUserMessage("Generate homework for each section with a mixed set of excercises (fill in the gaps & create your own sentences). Format so that excerises are grouped together by type with about 10 items in each. So, 10 fill in the blanks, followed by 10 create your own sentences etc.  For difficulty:\n" + difficulty)
            };

            var homeworkResponse = await ChatGPTService.AskAIAsync(homeworkMessages);

            // Example: store AI output as JSON
            var structured = new
            {
                Notes = tablesResponse,
                Homework = homeworkResponse,
                model.Difficulty,
                Timestamp = DateTime.UtcNow
            };

            string json = JsonSerializer.Serialize(structured, new JsonSerializerOptions { WriteIndented = true });

            await _courseNotesService.CreateCourseNoteAsync(new Models.Courses.CourseNote
            {
                CreatedAt = DateTime.UtcNow,
                Difficulty = difficulty,
                Homework = homeworkResponse,
                Notes = tablesResponse,
                TutorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                StudentId = model.StudentId,
            });
            //await System.IO.File.WriteAllTextAsync("StudyNotes.json", json);

            // Pass AI output to a view
            return View("ViewAIGen", new AINotesResponse{ tablesResponse = tablesResponse, tasks = homeworkResponse });
        }
    }
}
