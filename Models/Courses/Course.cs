using Classly.Models.Tutors;

namespace Classly.Models.Courses
{
    public class Course
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Tutor Tutor { get; set; } = new Tutor();
        public DateOnly DateCommencing { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int DuarationMins => (EndTime - StartTime).Minutes;
        public decimal Price { get; set; }
    }
}
