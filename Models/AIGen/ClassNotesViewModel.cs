namespace Classly.Models.AIGen
{
    public class ClassNotesViewModel
    {
        public string RichText { get; set; }
        public IFormFile FileUpload { get; set; }
        public string Difficulty { get; set; }
        public Guid StudentId { get; set; }
        public bool GenerateLessonPlan { get; set; }
        public string[] HomeworkTypes { get; set; } = [];

    }

}
