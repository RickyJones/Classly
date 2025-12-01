namespace Classly.Models.Courses
{
    public class CourseNote
    {
        public Guid Id { get; set; }
        public Guid TutorId { get; set; }
        public Guid StudentId { get; set; }

        public string Difficulty { get; set; }

        // Store JSON as string (EF will map to LONGTEXT/JSON in MySQL)
        public string Notes { get; set; }
        public string Homework { get; set; }
        public string NextLessonPlan { get; set; }

        public DateTime CreatedAt { get; set; }
    }

}
