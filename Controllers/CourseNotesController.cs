using Classly.Models;
using Classly.Models.AIGen;
using Classly.Models.Config;
using Classly.Services;
using Classly.Services.AI;
using Classly.Services.Data;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenAI.Chat;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Classly.Controllers
{
    //[Authorize]
    public class CourseNotesController : Controller
    {
        private readonly ICourseNotesService _courseNotesService;
        private readonly IUserService _userService;
        private readonly SiteSettings _settings;
        private readonly IAIService _aiService;
        private readonly IHomeworkSubmissionService _homeworkSubmissionService;
        public CourseNotesController(ICourseNotesService courseNotesService, IUserService userService, 
            IOptions<SiteSettings> options, IAIService aiService, IHomeworkSubmissionService hwServ)
        {
            _courseNotesService = courseNotesService;
            _userService = userService;
            _settings = options.Value;
            _aiService = aiService;
            _homeworkSubmissionService = hwServ;
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
                    var file = model.FileUpload;

                    bool isDocx =
                        file.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document" ||
                        Path.GetExtension(file.FileName).Equals(".docx", StringComparison.OrdinalIgnoreCase);

                    if (isDocx)
                    {
                        notesContent = ExtractTextFromWordDoc(file);
                    } else
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
            }

            var prompts = _courseNotesService.GetAIPrompts();

            var difficulty = model.Difficulty;

            var client = new ChatClient(model: "gpt-3.5-turbo", apiKey: _settings.AIKey);

            var messages = new List<string>
            {
                prompts.AITablePrompt + notesContent + ". Wrap the HTML I need with <content></content>"

            };

            var tablesResponse = await _aiService.AskAIAsync(messages);

            string startTag = "<content>";
            string endTag = "</content>";

            int startIndex = tablesResponse.IndexOf(startTag) + startTag.Length;
            int endIndex = tablesResponse.IndexOf(endTag);

            tablesResponse = tablesResponse.Substring(startIndex, endIndex - startIndex);

            //generate mixed homework

            var specifyHomeworkType = string.Join(",", model.HomeworkTypes);// "fill in the blanks, Multiple choice, Sentence matching / Half-sentence matching, create your own sentences ";
            var askAIForHomework = "";
            bool createNextLessonPlan = model.GenerateLessonPlan;
            string topicOfNextLesson = model.topicOfNextLesson;

            int classDurationMins = model.LessonDuration;


            var homeworkMessages = new List<string>
            {
                prompts.AIHomewrokPrompt + $"Create JSON that can be parsed my the given models structure, wrapping the JSON I need in <content></content>, producing 10 items for each question type at difficulty: " + difficulty + ". Model: " + ModelDefinition
            };

            var homeworkResponse = await _aiService.AskAIAsync(homeworkMessages);
            startIndex = homeworkResponse.IndexOf(startTag) + startTag.Length;
            endIndex = homeworkResponse.IndexOf(endTag);

            homeworkResponse = homeworkResponse.Substring(startIndex, endIndex - startIndex);
            //check it parses
            var hwStrong = JsonConvert.DeserializeObject<GeneratedHomeworkModel>(homeworkResponse);

            string lessonPlanResponse = string.Empty;
            if (createNextLessonPlan)
            {
                var lessonPlan = new List<string>
                {
                     $"Genrate an online 1-1 ESL lesson plan using the exact structure that follows. No preamble or explanation, don't allow more than 2 sequential <br/> if you use them.\r\nThe student level is {difficulty} and the class length will be {classDurationMins} mins.\r\nThe topic will be {topicOfNextLesson}. All vocabulary, idioms, and phrasal verbs must include simle definitions and clear example sentences, adapted to student level." + prompts.AILessonPlanPrompt
                };

                lessonPlanResponse = await _aiService.AskAIAsync(lessonPlan);
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

            string json = System.Text.Json.JsonSerializer.Serialize(structured, new JsonSerializerOptions { WriteIndented = true });

            var createdNote = await _courseNotesService.CreateCourseNoteAsync(new Models.Courses.CourseNote
            {
                CreatedAt = DateTime.UtcNow,
                Difficulty = difficulty,
                Homework = homeworkResponse,
                Notes = tablesResponse,
                TutorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                StudentId = model.StudentId,
                NextLessonPlan = lessonPlanResponse,
                Topic = model.Topic
            });
            //await System.IO.File.WriteAllTextAsync("StudyNotes.json", json);


            return new RedirectToActionResult("ViewNote", "CourseNotes", new { noteId = createdNote.Id });
            // Pass AI output to a view
            //return View("ViewAIGen", new AINotesResponse{ tablesResponse = tablesResponse, tasks = homeworkResponse });
        }

        private string ExtractTextFromWordDoc(IFormFile file)
        {
            using (var ms = new MemoryStream())
            {
                file.OpenReadStream().CopyTo(ms);

                using (var doc = WordprocessingDocument.Open(ms, false))
                {
                    return doc.MainDocumentPart.Document.Body.InnerText;
                }
            }
        }


        public IActionResult ViewHomework(Guid homeworkId)
        {
            var hw = _homeworkSubmissionService.GetSubmissionById(homeworkId);
            return View(hw);
        }

        private string ModelDefinition => @"
public class GeneratedHomeworkModel
    {
        public FillInTheBlanks[] FillInTheBlanksQuestions { get; set; }
        public QuestionWithOptions[] QuestionsWithOptions { get; set; }
        public SentenceMatching SentenceMatching { get; set; }
        public CreateYourOwnSentences[] CreateYourOwnSentences { get; set; }
    }

    public class FillInTheBlanks
    {
        public string SentenceWithBlanks { get; set; }
        public string CorrectSentence { get; set; }
    }

    public class QuestionWithOptions
    {
        public string Question { get; set; }
        public string[] PossibleAnswers { get; set; }
        public string CorrectAnswer { get; set; }
    }

    public class SentenceMatching
    {
        /// <summary>
        /// Each pair contains the first half and second half of a sentence.
        /// </summary>
        public SentencePair[] Pairs { get; set; }
    }

    public class SentencePair
    {
        public string FirstHalf { get; set; }
        public string SecondHalf { get; set; }
    }

    public class CreateYourOwnSentences
    {
        public string WordToInclude { get; set; }
    }";
    }
}
