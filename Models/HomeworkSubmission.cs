namespace Classly.Models
{
    public class HomeworkSubmission
    {
        public Guid Id { get; set; }           // CHAR(36) → Guid string
        public string Content { get; set; }         // up to 10k chars
        public DateTime? CreatedAt { get; set; }    // nullable timestamp
        public string LinkedCourseNoteId { get; set; } // CHAR(36)
        public bool MarkAsComplete { get; set; }
    }
}
